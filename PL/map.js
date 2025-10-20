// Initialize map function that accepts user coordinates
function initializeMap(userLatitude, userLongitude) {
    // Initialize map
    var map = L.map('map').setView([userLatitude, userLongitude], 13);
    var loadingOverlay = document.getElementById('loading-overlay');
    var markers = {};
    var currentRoute = null;
    var selectedCallId = null;

    // Add OpenStreetMap tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        maxZoom: 19
    }).addTo(map);

    // Add user marker
    var userIcon = L.divIcon({
        className: 'user-marker',
        html: '<div style="background-color: #2563EB; border: 2px solid white; border-radius: 50%; width: 16px; height: 16px;"></div>',
        iconSize: [16, 16],
        iconAnchor: [8, 8]
    });

    var userMarker = L.marker([userLatitude, userLongitude], { icon: userIcon }).addTo(map);
    userMarker.bindPopup('<strong>Your Location</strong>');

    // Function to center map on user location
    window.centerOnUser = function () {
        map.setView([userMarker.getLatLng().lat, userMarker.getLatLng().lng], 13);
    };

    // Function to update user location
    window.updateUserLocation = function (lat, lng) {
        userMarker.setLatLng([lat, lng]);
    };

    // Function to center map on a call
    window.centerOnCall = function (lat, lng) {
        map.setView([lat, lng], 15);
    };

    // Function to show call details
    window.showCallDetails = function (id, lat, lng, details) {
        // Check if marker exists
        if (markers[id]) {
            // Update selected marker
            if (selectedCallId && markers[selectedCallId]) {
                markers[selectedCallId].getElement().classList.remove("call-marker-selected");
            }

            selectedCallId = id;
            markers[id].getElement().classList.add("call-marker-selected");

            // Open popup
            markers[id].bindPopup(details).openPopup();
        }
    };

    // Function to calculate route
    window.calculateRoute = function (fromLat, fromLng, toLat, toLng) {
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
                opacity: 0.8
            }
        ).addTo(map);

        // Adjust view to contain the entire route
        var bounds = L.latLngBounds([
            [fromLat, fromLng],
            [toLat, toLng]
        ]);
        map.fitBounds(bounds, { padding: [50, 50] });
    };

    // Function to add a call marker
    window.addCallMarker = function (id, lat, lng, title) {
        // Remove previous marker if exists
        if (markers[id]) {
            map.removeLayer(markers[id]);
        }

        var callIcon = L.divIcon({
            className: 'call-marker',
            html: '<div class="call-marker-icon">' + id + '</div>',
            iconSize: [24, 24],
            iconAnchor: [12, 12]
        });

        var marker = L.marker([lat, lng], { icon: callIcon }).addTo(map);

        if (title) {
            marker.bindPopup(title);
        }

        // Save marker in dictionary
        markers[id] = marker;

        // Add click event
        marker.on('click', function () {
            // Call C# function when marker is clicked
            window.external.selectCall(id);
        });

        return marker;
    };

    // Function to clear all call markers
    window.clearCallMarkers = function () {
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
    window.zoomIn = function () {
        map.setZoom(map.getZoom() + 1);
    };

    // Function to zoom out
    window.zoomOut = function () {
        map.setZoom(map.getZoom() - 1);
    };

    // Function to set loading state
    window.setMapLoading = function (isLoading) {
        if (isLoading) {
            loadingOverlay.style.display = 'flex';
        } else {
            loadingOverlay.style.display = 'none';
        }
    };

    // Hide loading overlay when map is ready
    map.on('load', function () {
        setMapLoading(false);
    });

    // Initially hide loading overlay
    setMapLoading(false);

    // Return the map object for further use if needed
    return map;
}

// Initialize map when page loads
document.addEventListener('DOMContentLoaded', function () {
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
});