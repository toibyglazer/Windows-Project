using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BO;
using BlApi;
using System.Runtime.InteropServices;
using Helpers;
using System.Diagnostics;

namespace PL
{
    /// <summary>
    /// Interaction logic for OpenCallsWindow.xaml
    /// </summary>
    public class PascalCaseToWordsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            string text = value.ToString();
            string result = Regex.Replace(text, @"(?<=[a-z])(?=[A-Z])", " ");
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ComVisible(true)]
    public class MapBrowserScriptingObject
    {
        private readonly OpenCallsWindow _window;

        public MapBrowserScriptingObject(OpenCallsWindow window)
        {
            _window = window;
        }

        public void selectCall(int id)
        {
            _window.Dispatcher.Invoke(() => {
                _window.SelectCallById(id);
            });
        }

        public void mapInitialized()
        {
            _window.Dispatcher.Invoke(() => {
                _window.OnMapInitialized();
            });
        }
    }

    public partial class OpenCallsWindow : Window, INotifyPropertyChanged
    {
        private Dictionary<int, GeoCoordinates> _callCoordinates = new Dictionary<int, GeoCoordinates>();
        private bool _isMapLoading = false;
        public bool IsMapLoading
        {
            get => _isMapLoading;
            set
            {
                if (_isMapLoading != value)
                {
                    _isMapLoading = value;
                    OnPropertyChanged();

                    if (_mapInitialized)
                    {
                        try
                        {
                            _mapBrowser.InvokeScript("setMapLoading", new object[] { value });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error setting map loading state: {ex.Message}");
                        }
                    }
                }
            }
        }

        // Constants and static fields
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public const string ImageFolder = "ProfileImage";

        // Fields
        private readonly int _volunteerId;
        private readonly MainVolunteerScreen _parentWindow;
        private DispatcherTimer _animationTimer;
        private double _spinnerAngle = 0;
        private bool _isLoading;
        private double _userLatitude = 31.895049;
        private double _userLongitude = 34.810181;
        private bool _mapInitialized = false;
        private Dictionary<int, object> _callMarkers = new Dictionary<int, object>();
        private BO.CallType _selectedCallType = BO.CallType.All;
        private string _selectedSortKey = "default";
        private OpenCallInList _selectedCall;
        private WebBrowser _mapBrowser;

        // Properties
        public string VolunteerName { get; private set; }
        public CallTypesCollection CallTypes { get; } = new CallTypesCollection();
        public bool AllowSelection { get; set; } = true;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    UpdateLoadingIndicator();

                    if (_mapInitialized)
                    {
                        try
                        {
                            _mapBrowser.InvokeScript("setMapLoading", new object[] { value });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error setting map loading state: {ex.Message}");
                        }
                    }
                }
            }
        }

        public BO.CallType SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                _selectedCallType = value;
                OnPropertyChanged();
            }
        }

        public string SelectedSortKey
        {
            get => _selectedSortKey;
            set
            {
                _selectedSortKey = value;
                OnPropertyChanged();
            }
        }

        public OpenCallInList SelectedCall
        {
            get => _selectedCall;
            set
            {
                _selectedCall = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCallSelected));

                if (value != null && !string.IsNullOrEmpty(value.FullAddress))
                {
                    // Use actual coordinates if available
                    if (_callCoordinates.TryGetValue(value.Id, out var coords))
                    {
                        CenterMapOnCall(coords.Latitude, coords.Longitude);
                        ShowCallDetails(value.Id, coords.Latitude, coords.Longitude, FormatCallDetails(value));
                        CalculateRoute(_userLatitude, _userLongitude, coords.Latitude, coords.Longitude);
                    }
                    else
                    {
                        // If coordinates not available, try to load them
                        Task.Run(async () =>
                        {
                            try
                            {
                                var coordinates = await GeocodingHelper.GetCoordinatesForAddressAsync(value.FullAddress);
                                if (coordinates != null)
                                {
                                    _callCoordinates[value.Id] = coordinates;

                                    await Dispatcher.InvokeAsync(() =>
                                    {
                                        CenterMapOnCall(coordinates.Latitude, coordinates.Longitude);
                                        ShowCallDetails(value.Id, coordinates.Latitude, coordinates.Longitude, FormatCallDetails(value));
                                        CalculateRoute(_userLatitude, _userLongitude, coordinates.Latitude, coordinates.Longitude);
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error getting coordinates for selected call: {ex.Message}");
                            }
                        });
                    }
                }
            }
        }

        public bool IsCallSelected => SelectedCall != null && AllowSelection;

        private IEnumerable<OpenCallInList> _openCalls;
        public IEnumerable<OpenCallInList> OpenCalls
        {
            get => _openCalls;
            set
            {
                _openCalls = value;
                OnPropertyChanged();
                Debug.WriteLine($"OpenCalls updated with {value?.Count() ?? 0} items");
                UpdateMapMarkersWithCoordinates();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor
        public OpenCallsWindow(int volunteerId, MainVolunteerScreen parentWindow,
            double userLatitude = 31.895049, double userLongitude = 34.810181)
        {
            SetBrowserFeatureControl();
            InitializeComponent();

            _volunteerId = volunteerId;
            _parentWindow = parentWindow;
            _userLatitude = userLatitude;
            _userLongitude = userLongitude;

            DataContext = this;
            OpenCalls = new List<OpenCallInList>();
            Loaded += Window_Loaded;

            Debug.WriteLine($"OpenCallsWindow constructor completed with user location: {_userLatitude}, {_userLongitude}");
        }

        // Browser initialization methods
        private static void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                string.Format(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\{0}", feature),
                Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, value, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        public static void SetBrowserFeatureControl()
        {
            string appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", appName, 11001);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", appName, 1);
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", appName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", appName, 1);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", appName, 0);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", appName, 1);
            SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", appName, 0);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING", appName, 1);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_SCRIPT_PASTE_URLACTION_IF_PROMPT", appName, 1);

            string rootKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\0";
            Microsoft.Win32.Registry.SetValue(rootKey, "1400", 0, Microsoft.Win32.RegistryValueKind.DWord);
            Microsoft.Win32.Registry.SetValue(rootKey, "1200", 0, Microsoft.Win32.RegistryValueKind.DWord);
            Microsoft.Win32.Registry.SetValue(rootKey, "1201", 0, Microsoft.Win32.RegistryValueKind.DWord);
            Microsoft.Win32.Registry.SetValue(rootKey, "1001", 0, Microsoft.Win32.RegistryValueKind.DWord);
            Microsoft.Win32.Registry.SetValue(rootKey, "1004", 0, Microsoft.Win32.RegistryValueKind.DWord);
            Microsoft.Win32.Registry.SetValue(rootKey, "1209", 0, Microsoft.Win32.RegistryValueKind.DWord);
            Microsoft.Win32.Registry.SetValue(rootKey, "1806", 0, Microsoft.Win32.RegistryValueKind.DWord);
            Microsoft.Win32.Registry.SetValue(rootKey, "2201", 0, Microsoft.Win32.RegistryValueKind.DWord);
            Microsoft.Win32.Registry.SetValue(rootKey, "2000", 0, Microsoft.Win32.RegistryValueKind.DWord);
        }

        // Event handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Window_Loaded event started");

                _animationTimer = new DispatcherTimer();
                _animationTimer.Interval = TimeSpan.FromMilliseconds(16);
                _animationTimer.Tick += AnimationTimer_Tick;

                // Initialize map
                InitializeMap();

                // Load data
                LoadDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Window_Loaded: {ex.Message}");
                MessageBox.Show($"Error initializing window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Animation handled by ProgressBar
        }

        // Map related methods
        private void InitializeMap()
        {
            try
            {
                if (_mapBrowser == null)
                {
                    _mapBrowser = new WebBrowser();
                    var mapContainer = this.FindName("MapContainer") as Grid;
                    if (mapContainer != null)
                    {
                        mapContainer.Children.Add(_mapBrowser);
                    }
                }

                _mapBrowser.ObjectForScripting = new MapBrowserScriptingObject(this);

                string htmlContent = GenerateMapHtml();
                string tempFile = Path.Combine(Path.GetTempPath(), "openstreetmap.html");
                File.WriteAllText(tempFile, htmlContent);

                string jsFile = Path.Combine(Path.GetTempPath(), "map.js");
                File.WriteAllText(jsFile, GetMapJavaScript());

                _mapBrowser.Navigate(new Uri(tempFile));

                _mapBrowser.LoadCompleted += (s, e) => {
                    _mapInitialized = true;
                    UpdateMapMarkersWithCoordinates();
                };

                _mapBrowser.Navigating += (s, e) => {
                    // Handle unwanted navigation if needed
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing map: {ex.Message}");
                MessageBox.Show($"Error initializing map: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateMapHtml()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>OpenStreetMap</title>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.7.1/dist/leaflet.css' />
    <script src='https://unpkg.com/leaflet@1.7.1/dist/leaflet.js'></script>
    <script src='map.js'></script>
    <style>
        html, body, #map {
            width: 100%;
            height: 100%;
            margin: 0;
            padding: 0;
        }
        
        /* Loading overlay */
        .map-loading-overlay {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(255, 255, 255, 0.7);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }
        .map-loading-spinner {
            position: relative;
            width: 30px;
            height: 30px;
            animation: spin 1s linear infinite;
        }
        .map-loading-spinner .dot {
            position: absolute;
            width: 5px;
            height: 5px;
            background-color: #2563EB;
            border-radius: 50%;
        }
        .map-loading-spinner .dot:nth-child(1) { top: 0; left: 12.5px; opacity: 1.0; }
        .map-loading-spinner .dot:nth-child(2) { top: 3px; left: 21px; opacity: 0.9; }
        .map-loading-spinner .dot:nth-child(3) { top: 12.5px; left: 25px; opacity: 0.8; }
        .map-loading-spinner .dot:nth-child(4) { top: 22px; left: 21px; opacity: 0.7; }
        .map-loading-spinner .dot:nth-child(5) { top: 25px; left: 12.5px; opacity: 0.6; }
        .map-loading-spinner .dot:nth-child(6) { top: 22px; left: 4px; opacity: 0.5; }
        .map-loading-spinner .dot:nth-child(7) { top: 12.5px; left: 0; opacity: 0.4; }
        .map-loading-spinner .dot:nth-child(8) { top: 3px; left: 4px; opacity: 0.3; }
        
        /* Custom zoom controls */
        .leaflet-control-zoom a {
            width: 30px !important;
            height: 30px !important;
            line-height: 30px !important;
            font-size: 18px;
            font-weight: bold;
            border-radius: 4px !important;
            margin-bottom: 5px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
            background-color: white;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        
        /* Locate control */
        .leaflet-control-locate {
            margin-bottom: 10px !important;
        }
        
        .leaflet-control-locate a {
            width: 36px !important;
            height: 36px !important;
            border-radius: 4px !important;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
            background-color: white;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        
        .locate-icon {
            display: inline-block;
            width: 18px;
            height: 18px;
            background-color: #2563EB;
            border: 2px solid white;
            border-radius: 50%;
            position: relative;
            box-shadow: 0 0 3px rgba(0,0,0,0.3);
        }
        
        /* User marker */
        .user-marker-icon {
            width: 20px;
            height: 20px;
            background-color: #2563EB;
            border: 3px solid white;
            border-radius: 50%;
            box-shadow: 0 0 5px rgba(0,0,0,0.5);
        }
        
        /* Call markers */
        .call-marker-icon {
            width: 30px;
            height: 30px;
            background-color: #ef4444;
            border: 2px solid white;
            border-radius: 50%;
            color: white;
            font-weight: bold;
            font-size: 14px;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 2px 5px rgba(0,0,0,0.3);
        }
        
        .call-marker-selected {
            background-color: #3b82f6;
            transform: scale(1.2);
            box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.3);
        }
        
        /* Route line */
        .route-line {
            stroke-dasharray: 10, 10;
            animation: dash 20s linear infinite;
        }
        
        /* Popup styles */
        .leaflet-popup-content {
            margin: 10px 12px;
            line-height: 1.5;
        }
        
        .call-popup-header {
            font-weight: bold;
            font-size: 16px;
            margin-bottom: 8px;
            color: #2563EB;
        }
        
        .call-popup-address {
            margin-bottom: 5px;
        }
        
        .call-popup-distance {
            color: #4b5563;
        }
        
        /* Animations */
        @keyframes spin {
            to { transform: rotate(360deg); }
        }
        
        @keyframes dash {
            to { stroke-dashoffset: 1000; }
        }
    </style>
</head>
<body>
    <div id='map'></div>
    <div id='loading-overlay' class='map-loading-overlay'>
        <div class='map-loading-spinner'>
            <div class='dot'></div>
            <div class='dot'></div>
            <div class='dot'></div>
            <div class='dot'></div>
            <div class='dot'></div>
            <div class='dot'></div>
            <div class='dot'></div>
            <div class='dot'></div>
        </div>
    </div>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            try {
                var map = initializeMap(" + _userLatitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", " + _userLongitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + @");
                window.external.mapInitialized();
            } catch (e) {
                console.error('Error initializing map:', e);
            }
        });
    </script>
</body>
</html>";
        }
        private string GetMapJavaScript()
        {
            return @"// Initialize map function that accepts user coordinates
function initializeMap(userLatitude, userLongitude) {
    // Initialize map
    var map = L.map('map', {
        zoomControl: false  // Disable default zoom control to add custom one
    }).setView([userLatitude, userLongitude], 13);
    
    var loadingOverlay = document.getElementById('loading-overlay');
    var markers = {};
    var currentRoute = null;
    var selectedCallId = null;

    // Add OpenStreetMap tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href=""https://www.openstreetmap.org/copyright"">OpenStreetMap</a> contributors',
        maxZoom: 19
    }).addTo(map);

    // Add custom zoom control
    var zoomControl = L.control.zoom({
        position: 'topleft',
        zoomInText: '+',
        zoomOutText: '-',
        zoomInTitle: 'Zoom in',
        zoomOutTitle: 'Zoom out'
    }).addTo(map);

    // Add custom locate control
    var locateControl = L.control({
        position: 'bottomright'
    });

    locateControl.onAdd = function(map) {
        var div = L.DomUtil.create('div', 'leaflet-control-locate leaflet-bar leaflet-control');
        div.innerHTML = '<a class=""leaflet-control-locate-button"" href=""#"" title=""Center on my location""><i class=""locate-icon""></i></a>';
        
        div.onclick = function(e) {
            e.preventDefault();
            e.stopPropagation();
            centerOnUserWithAnimation();
            return false;
        };
        
        return div;
    };

    locateControl.addTo(map);

    // Add user marker with custom icon
    var userIcon = L.divIcon({
        className: 'user-marker',
        html: '<div class=""user-marker-icon""></div>',
        iconSize: [20, 20],
        iconAnchor: [10, 10]
    });

    var userMarker = L.marker([userLatitude, userLongitude], { icon: userIcon }).addTo(map);
    userMarker.bindPopup('<strong>Your Location</strong>');

    // Function to center map on user location with animation
    function centerOnUserWithAnimation() {
        map.flyTo([userMarker.getLatLng().lat, userMarker.getLatLng().lng], 15, {
            duration: 1.5,
            easeLinearity: 0.25
        });
    }

    // Function to center map on user location
    window.centerOnUser = function() {
        centerOnUserWithAnimation();
    };

    // Function to update user location
    window.updateUserLocation = function(lat, lng) {
        userMarker.setLatLng([lat, lng]);
    };

    // Function to center map on a call
    window.centerOnCall = function(lat, lng) {
        map.flyTo([lat, lng], 16, {
            duration: 1,
            easeLinearity: 0.25
        });
    };

    // Function to show call details
    window.showCallDetails = function(id, lat, lng, details) {
        // Check if marker exists
        if (markers[id]) {
            // Update selected marker
            if (selectedCallId && markers[selectedCallId]) {
                var prevElement = markers[selectedCallId].getElement();
                if (prevElement) {
                    prevElement.classList.remove(""call-marker-selected"");
                }
            }

            selectedCallId = id;
            var element = markers[id].getElement();
            if (element) {
                element.classList.add(""call-marker-selected"");
            }

            // Open popup
            markers[id].bindPopup(details).openPopup();
            
            // Zoom in on the call
            centerOnCall(lat, lng);
        }
    };

    // Function to calculate route
    window.calculateRoute = function(fromLat, fromLng, toLat, toLng) {
        // Remove previous route if exists
        if (currentRoute) {
            map.removeLayer(currentRoute);
        }

        // Create simple line between two points
        currentRoute = L.polyline(
            [
                [fromLat, fromLng],
                [toLat, toLng]
            ],
            {
                className: 'route-line',
                color: '#2563EB',
                weight: 5,
                opacity: 0.8,
                dashArray: '10, 10',
                lineCap: 'round'
            }
        ).addTo(map);

        // Adjust view to contain the entire route
        var bounds = L.latLngBounds([
            [fromLat, fromLng],
            [toLat, toLng]
        ]);
        map.fitBounds(bounds, { 
            padding: [50, 50],
            animate: true,
            duration: 1
        });
    };

    // Function to add a call marker
    window.addCallMarker = function(id, lat, lng, title) {
        // Remove previous marker if exists
        if (markers[id]) {
            map.removeLayer(markers[id]);
        }

        // Create custom call marker icon
        var callIcon = L.divIcon({
            className: 'call-marker',
            html: '<div class=""call-marker-icon"">' + id + '</div>',
            iconSize: [30, 30],
            iconAnchor: [15, 15]
        });

        var marker = L.marker([lat, lng], { icon: callIcon }).addTo(map);

        if (title) {
            marker.bindPopup(title);
        }

        // Save marker in dictionary
        markers[id] = marker;

        // Add click event
        marker.on('click', function() {
            // Call C# function when marker is clicked
            window.external.selectCall(id);
        });

        return marker;
    };

    // Function to clear all call markers
    window.clearCallMarkers = function() {
        for (var id in markers) {
            if (markers.hasOwnProperty(id)) {
                map.removeLayer(markers[id]);
            }
        }
        markers = {};
        selectedCallId = null;

        // Remove route if exists
        if (currentRoute) {
            map.removeLayer(currentRoute);
            currentRoute = null;
        }
    };

    // Function to zoom in
    window.zoomIn = function() {
        map.setZoom(map.getZoom() + 1);
    };

    // Function to zoom out
    window.zoomOut = function() {
        map.setZoom(map.getZoom() - 1);
    };

    // Function to set loading state
    window.setMapLoading = function(isLoading) {
        if (isLoading) {
            loadingOverlay.style.display = 'flex';
        } else {
            loadingOverlay.style.display = 'none';
        }
    };

    // Hide loading overlay when map is ready
    map.on('load', function() {
        setMapLoading(false);
        
        // Animate to user location after map loads
        setTimeout(function() {
            centerOnUserWithAnimation();
        }, 500);
    });

    // Initially hide loading overlay
    setMapLoading(false);

    // Return the map object for further use if needed
    return map;
}

// Initialize map when page loads
document.addEventListener('DOMContentLoaded', function() {
    try {
        // Note: coordinates will be passed from C# when creating the page
        // var map = initializeMap(userLatitude, userLongitude);

        // Notify C# that map is ready
        if (window.external && typeof window.external.mapInitialized === 'function') {
            window.external.mapInitialized();
        }
    } catch (e) {
        console.error('Error initializing map:', e);
    }
});";
        }
        public void OnMapInitialized()
        {
            _mapInitialized = true;

            if (_callCoordinates.Count > 0)
            {
                UpdateMapMarkersWithCoordinates();
            }
            else
            {
                LoadCoordinatesAsync();
            }

            // Load volunteer coordinates
            LoadVolunteerCoordinatesAsync();
        }

        /// <summary>
        /// Loads volunteer coordinates and updates the map
        /// </summary>
        private async Task LoadVolunteerCoordinatesAsync()
        {
            try
            {
                Debug.WriteLine("LoadVolunteerCoordinatesAsync started");
                IsMapLoading = true;

                // Get volunteer details including address
                var volunteer = await s_bl.Volunteer.GetVolunteerDetailsAsync(_volunteerId);

                // Make sure we have an address to geocode
                if (string.IsNullOrEmpty(volunteer.CurrentAddress))
                {
                    Debug.WriteLine("Volunteer has no address information");
                    IsMapLoading = false;
                    return;
                }

                // Get coordinates using our helper class
                var coordinates = await GeocodingHelper.GetVolunteerCoordinatesAsync(_volunteerId, volunteer.CurrentAddress);

                if (coordinates != null)
                {
                    // Update volunteer location
                    _userLatitude = coordinates.Latitude;
                    _userLongitude = coordinates.Longitude;

                    Debug.WriteLine($"Volunteer coordinates loaded: {_userLatitude}, {_userLongitude}");

                    // Update map with new location if map is initialized
                    if (_mapInitialized)
                    {
                        try
                        {
                            // Update user marker on map
                            _mapBrowser.InvokeScript("updateUserLocation", new object[] { _userLatitude, _userLongitude });

                            // Center map on user
                            CenterMapOnUser();

                            // If we have call coordinates, update distances
                            if (_callCoordinates.Count > 0)
                            {
                                await UpdateDistancesToCallsAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error updating user location on map: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Could not determine volunteer coordinates");
                }

                IsMapLoading = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading volunteer coordinates: {ex.Message}");
                IsMapLoading = false;
            }
        }

        /// <summary>
        /// Updates distances from volunteer to all calls
        /// </summary>
        private async Task UpdateDistancesToCallsAsync()
        {
            try
            {
                Debug.WriteLine("UpdateDistancesToCallsAsync started");

                if (OpenCalls == null || !OpenCalls.Any() || _callCoordinates.Count == 0)
                    return;

                foreach (var call in OpenCalls)
                {
                    if (_callCoordinates.TryGetValue(call.Id, out var coords))
                    {
                        // Calculate distance between volunteer and call
                        double distance = await GeocodingHelper.CalculateDistanceAsync(
                            _userLatitude, _userLongitude, coords.Latitude, coords.Longitude);

                        // Update distance in call object
                        call.DistanceToActiveCall = distance;
                    }
                }

                // Refresh the data grid
                await Dispatcher.InvokeAsync(() => {
                    OpenCallsDataGrid.Items.Refresh();
                });

                Debug.WriteLine("UpdateDistancesToCallsAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating distances to calls: {ex.Message}");
            }
        }

        public async void SelectCallById(int callId)
        {
            var call = OpenCalls.FirstOrDefault(c => c.Id == callId);
            if (call != null)
            {
                SelectedCall = call;
                OpenCallsDataGrid.SelectedItem = call;
                OpenCallsDataGrid.ScrollIntoView(call);

                // Use actual coordinates if available
                if (_callCoordinates.TryGetValue(call.Id, out var coords))
                {
                    CenterMapOnCall(coords.Latitude, coords.Longitude);
                    ShowCallDetails(call.Id, coords.Latitude, coords.Longitude, FormatCallDetails(call));
                    CalculateRoute(_userLatitude, _userLongitude, coords.Latitude, coords.Longitude);
                }
            }
        }

        private string FormatCallDetails(OpenCallInList call)
        {
            // הוספת רווחים לסוג הקריאה לפי אותיות גדולות
            string formattedType = Regex.Replace(call.Type.ToString(), @"(?<=[a-z])(?=[A-Z])", " ");

            return $@"
        <div class='call-popup-header'>{formattedType}</div>
        <div class='call-popup-address'>{call.FullAddress}</div>
        <div class='call-popup-distance'>{call.DistanceToActiveCall:N2} km</div>";
        }

        private void CenterMapOnCall(double latitude, double longitude)
        {
            if (!_mapInitialized) return;
            try { _mapBrowser.InvokeScript("centerOnCall", new object[] { latitude, longitude }); }
            catch (Exception ex) { Debug.WriteLine($"Error centering map on call: {ex.Message}"); }
        }

        private void ShowCallDetails(int id, double latitude, double longitude, string details)
        {
            if (!_mapInitialized) return;
            try { _mapBrowser.InvokeScript("showCallDetails", new object[] { id, latitude, longitude, details }); }
            catch (Exception ex) { Debug.WriteLine($"Error showing call details: {ex.Message}"); }
        }

        private void CalculateRoute(double fromLat, double fromLng, double toLat, double toLng)
        {
            if (!_mapInitialized) return;
            try { _mapBrowser.InvokeScript("calculateRoute", new object[] { fromLat, fromLng, toLat, toLng }); }
            catch (Exception ex) { Debug.WriteLine($"Error calculating route: {ex.Message}"); }
        }

        private void CenterMapOnUser()
        {
            if (!_mapInitialized) return;
            try { _mapBrowser.InvokeScript("centerOnUser"); }
            catch (Exception ex) { Debug.WriteLine($"Error centering map on user: {ex.Message}"); }
        }

        private void AddCallMarker(int id, double latitude, double longitude, string title = null)
        {
            if (!_mapInitialized) return;
            try { _mapBrowser.InvokeScript("addCallMarker", new object[] { id, latitude, longitude, title }); }
            catch (Exception ex) { Debug.WriteLine($"Error adding call marker: {ex.Message}"); }
        }

        private void ZoomIn()
        {
            if (!_mapInitialized) return;
            try { _mapBrowser.InvokeScript("zoomIn"); }
            catch (Exception ex) { Debug.WriteLine($"Error zooming in: {ex.Message}"); }
        }

        private void ZoomOut()
        {
            if (!_mapInitialized) return;
            try { _mapBrowser.InvokeScript("zoomOut"); }
            catch (Exception ex) { Debug.WriteLine($"Error zooming out: {ex.Message}"); }
        }

        // Data loading methods
        private async void LoadDataAsync()
        {
            try
            {
                Debug.WriteLine("LoadDataAsync started");
                IsLoading = true;

                // Load volunteer data
                await LoadVolunteerDataAsync();
                await LoadProfileImageAsync(_volunteerId);
                InitializeComboBoxes();

                // Load open calls
                await UpdateOpenCallsAsync();

                // Load call coordinates
                LoadCoordinatesAsync();

                IsLoading = false;
                Debug.WriteLine("LoadDataAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadDataAsync: {ex.Message}");
                MessageBox.Show($"Error initializing window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsLoading = false;
            }
        }

        private async void LoadCoordinatesAsync()
        {
            try
            {
                if (OpenCalls == null || !OpenCalls.Any())
                    return;

                IsMapLoading = true;
                Debug.WriteLine("LoadCoordinatesAsync started");

                // Get coordinates for all calls
                _callCoordinates = await GeocodingHelper.GetCoordinatesForCallsAsync(OpenCalls);

                // Update map with new coordinates
                UpdateMapMarkersWithCoordinates();

                // Update distances if we have volunteer coordinates
                if (_userLatitude != 0 && _userLongitude != 0)
                {
                    await UpdateDistancesToCallsAsync();
                }

                IsMapLoading = false;
                Debug.WriteLine("LoadCoordinatesAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading coordinates: {ex.Message}");
                IsMapLoading = false;
            }
        }

        private void UpdateMapMarkersWithCoordinates()
        {
            if (!_mapInitialized || OpenCalls == null)
                return;

            try
            {
                _mapBrowser.InvokeScript("clearCallMarkers");

                foreach (var call in OpenCalls)
                {
                    if (_callCoordinates.TryGetValue(call.Id, out var coords))
                    {
                        string popupContent = FormatCallDetails(call);
                        AddCallMarker(call.Id, coords.Latitude, coords.Longitude, popupContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating map markers: {ex.Message}");
            }
        }

        private void UpdateLoadingIndicator()
        {
            if (LoadingIndicator != null)
            {
                LoadingIndicator.Visibility = IsLoading ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async Task LoadVolunteerDataAsync()
        {
            try
            {
                Debug.WriteLine("LoadVolunteerDataAsync started");
                var volunteer = await s_bl.Volunteer.GetVolunteerDetailsAsync(_volunteerId);
                VolunteerName = volunteer.FullName;
                OnPropertyChanged(nameof(VolunteerName));
                Debug.WriteLine($"Volunteer data loaded: {VolunteerName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading volunteer data: {ex.Message}");
                MessageBox.Show($"Error loading volunteer data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadProfileImageAsync(int volunteerId)
        {
            try
            {
                Debug.WriteLine("LoadProfileImageAsync started");
                string imagePath = System.IO.Path.Combine(AppContext.BaseDirectory, ImageFolder, $"{volunteerId}.jpg");
                Debug.WriteLine($"Looking for profile image at: {imagePath}");

                if (File.Exists(imagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    await Dispatcher.InvokeAsync(() => {
                        VolunteerProfileImageBrush.ImageSource = bitmap;
                    });

                    Debug.WriteLine("Profile image loaded successfully");
                }
                else
                {
                    string defaultPath = System.IO.Path.Combine(AppContext.BaseDirectory, ImageFolder, "default.jpg");
                    Debug.WriteLine($"Profile image not found, trying default at: {defaultPath}");

                    if (File.Exists(defaultPath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(defaultPath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        await Dispatcher.InvokeAsync(() => {
                            VolunteerProfileImageBrush.ImageSource = bitmap;
                        });

                        Debug.WriteLine("Default profile image loaded successfully");
                    }
                    else
                    {
                        await Dispatcher.InvokeAsync(() => {
                            VolunteerProfileBorder.Background = new SolidColorBrush(Color.FromRgb(229, 231, 235));
                        });

                        Debug.WriteLine("Using solid color as profile background");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading profile image: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                await Dispatcher.InvokeAsync(() => {
                    VolunteerProfileBorder.Background = new SolidColorBrush(Color.FromRgb(229, 231, 235));
                });
            }
        }

        private void InitializeComboBoxes()
        {
            try
            {
                Debug.WriteLine("InitializeComboBoxes started");

                CallTypeFilter.ItemsSource = CallTypes;
                CallTypeFilter.SelectedValue = SelectedCallType;

                var sortFields = new Dictionary<string, string>() {
                    { "default", "Default (ID)" },
                    { "id", "ID" },
                    { "type", "Call Type" },
                    { "address", "Address" },
                    { "openingTime", "Opening Time" },
                    { "maxCompletionTime", "Max Completion Time" },
                    { "distance", "Distance" }
                };

                SortByField.ItemsSource = sortFields;
                SortByField.DisplayMemberPath = "Value";
                SortByField.SelectedValuePath = "Key";
                SortByField.SelectedValue = "default";

                Debug.WriteLine("ComboBoxes initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing combo boxes: {ex.Message}");
                throw new Exception($"Error initializing combo boxes: {ex.Message}", ex);
            }
        }

        public async Task UpdateOpenCallsAsync()
        {
            try
            {
                Debug.WriteLine("UpdateOpenCallsAsync started");
                Debug.WriteLine($"Current filters - Type: {SelectedCallType}, Sort: {SelectedSortKey}");

                IsLoading = true;
                OpenCallSortField? sortField = GetSortFieldFromKey(SelectedSortKey);

                var openCalls = await s_bl.Call.GetOpenCallsByVolunteerAsync(
                    _volunteerId,
                    SelectedCallType == BO.CallType.All ? null : SelectedCallType,
                    sortField);

                OpenCalls = openCalls.ToList();
                Debug.WriteLine($"Retrieved {OpenCalls.Count()} open calls");

                await Dispatcher.InvokeAsync(() => {
                    OpenCallsDataGrid.Items.Refresh();

                    foreach (var column in OpenCallsDataGrid.Columns)
                    {
                        column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                    }

                    Debug.WriteLine("DataGrid refreshed and columns auto-sized");
                });

                // Load coordinates after updating the list
                LoadCoordinatesAsync();

                IsLoading = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading open calls: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading open calls: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsLoading = false;
            }
        }

        private OpenCallSortField? GetSortFieldFromKey(string key)
        {
            switch (key)
            {
                case "id": return OpenCallSortField.Id;
                case "type": return OpenCallSortField.Type;
                case "description": return OpenCallSortField.Description;
                case "address": return OpenCallSortField.FullAddress;
                case "openingTime": return OpenCallSortField.OpeningTime;
                case "maxCompletionTime": return OpenCallSortField.MaxCompletionTime;
                case "distance": return OpenCallSortField.DistanceToActiveCall;
                default: return null;
            }
        }

        // UI event handlers
        private async void CallTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CallTypeFilter.SelectedItem is BO.CallType callType)
                {
                    SelectedCallType = callType;
                    await UpdateOpenCallsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing call type filter: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SortByField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (SortByField.SelectedValue is string key)
                {
                    SelectedSortKey = key;
                    await UpdateOpenCallsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing sort field: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await UpdateOpenCallsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing open calls: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCallsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // No action - call selection is done via "Select Call" button
        }

        private void OpenCallsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OpenCallsDataGrid.SelectedItem is OpenCallInList selectedCall)
            {
                SelectedCall = selectedCall;
            }
        }

        private void SelectCallButton_Click(object sender, RoutedEventArgs e)
        {
            SelectCallAsync().ConfigureAwait(false);
        }

        private async Task SelectCallAsync()
        {
            if (SelectedCall == null)
            {
                MessageBox.Show("Please select a call first.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!AllowSelection)
            {
                MessageBox.Show(
                    "You cannot select a new call while you have an active call. Please complete or cancel your current call first.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to select Call #{SelectedCall.Id}?",
                    "Confirm Selection",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result != MessageBoxResult.Yes) return;

                IsLoading = true;
                await Task.Run(() => s_bl.Call.AssignCallToVolunteer(_volunteerId, SelectedCall.Id));

                MessageBox.Show($"Call #{SelectedCall.Id} has been assigned to you. You can now view it in your active call.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                await _parentWindow.RefreshVolunteerDataAsync();
                IsLoading = false;

                await Dispatcher.InvokeAsync(() => {
                    DialogResult = true;
                    Close();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning call: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsLoading = false;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
        }

        private void CenterMapButton_Click(object sender, RoutedEventArgs e)
        {
            CenterMapOnUser();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CallTypesCollection : IEnumerable
    {
        static readonly IEnumerable<BO.CallType> s_enums =
            Enum.GetValues(typeof(BO.CallType)) as IEnumerable<BO.CallType> ?? throw new InvalidOperationException();

        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }
}
