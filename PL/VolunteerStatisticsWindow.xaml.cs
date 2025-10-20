using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.Json;
using System.IO;
using BO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Threading;

namespace PL
{
    /// <summary>
    /// Interaction logic for VolunteerStatisticsWindow.xaml
    /// </summary>
    public partial class VolunteerStatisticsWindow : Window, INotifyPropertyChanged
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private readonly int _volunteerId;
        private IEnumerable<ClosedCallInList> _closedCalls;
        private static readonly HttpClient httpClient = new HttpClient();

        // Constants
        public const string ImageFolder = "ProfileImage"; // Image folder

        // Volunteer Info Properties
        public string VolunteerName { get; private set; }
        public string VolunteerInitials { get; private set; }
        public int VolunteerId => _volunteerId;
        public string VolunteerRole { get; private set; }

        // Summary Stats
        public int TotalCalls => _closedCalls?.Count() ?? 0;
        public int TreatedCalls => _closedCalls?.Count(c => c.CompletionType == AssignmentCompletionType.Treated) ?? 0;
        public int CanceledCalls => _closedCalls?.Count(c =>
            c.CompletionType == AssignmentCompletionType.VolunteerCancelled ||
            c.CompletionType == AssignmentCompletionType.AdminCancelled) ?? 0;
        public int ExpiredCalls => _closedCalls?.Count(c => c.CompletionType == AssignmentCompletionType.Expired) ?? 0;

        // Chart Images
        private ImageSource _callStatusChartImage;
        public ImageSource CallStatusChartImage
        {
            get => _callStatusChartImage;
            private set
            {
                _callStatusChartImage = value;
                OnPropertyChanged(nameof(CallStatusChartImage));
            }
        }

        private ImageSource _callTypesChartImage;
        public ImageSource CallTypesChartImage
        {
            get => _callTypesChartImage;
            private set
            {
                _callTypesChartImage = value;
                OnPropertyChanged(nameof(CallTypesChartImage));
            }
        }

        private ImageSource _activityTimelineChartImage;
        public ImageSource ActivityTimelineChartImage
        {
            get => _activityTimelineChartImage;
            private set
            {
                _activityTimelineChartImage = value;
                OnPropertyChanged(nameof(ActivityTimelineChartImage));
            }
        }

        public VolunteerStatisticsWindow(int volunteerId)
        {
            _volunteerId = volunteerId;

            // Initialize the component using the standard method
            InitializeComponent();

            // Set DataContext to this instance
            DataContext = this;

            // Initialize with empty data
            _closedCalls = new List<ClosedCallInList>();

            // Add Loaded event handler to load data asynchronously
            Loaded += VolunteerStatisticsWindow_Loaded;

            Debug.WriteLine($"VolunteerStatisticsWindow constructor completed for volunteer ID: {_volunteerId}");
        }

        private async void VolunteerStatisticsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("VolunteerStatisticsWindow_Loaded event started");

                // Load all data asynchronously
                await Task.WhenAll(
                    LoadVolunteerDataAsync(),
                    LoadCallDataAsync(),
                    LoadProfileImageAsync(_volunteerId)
                );

                // Load charts after data is loaded
                await LoadChartsAsync();

                // Debug output to check if data is loaded correctly
                Debug.WriteLine($"Volunteer ID: {_volunteerId}");
                Debug.WriteLine($"Total Calls: {TotalCalls}");
                Debug.WriteLine($"Treated Calls: {TreatedCalls}");
                Debug.WriteLine($"Canceled Calls: {CanceledCalls}");
                Debug.WriteLine($"Expired Calls: {ExpiredCalls}");

                if (_closedCalls != null)
                {
                    Debug.WriteLine($"Closed calls count: {_closedCalls.Count()}");
                    foreach (var call in _closedCalls.Take(5)) // Only log first 5 to avoid flooding
                    {
                        Debug.WriteLine($"Call ID: {call.Id}, Type: {call.Type}, Status: {call.CompletionType}");
                    }
                }
                else
                {
                    Debug.WriteLine("_closedCalls is null");
                }

                Debug.WriteLine("VolunteerStatisticsWindow_Loaded event completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in VolunteerStatisticsWindow_Loaded: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadVolunteerDataAsync()
        {
            try
            {
                Debug.WriteLine("LoadVolunteerDataAsync started");

                // Use the correct method name for your API - now asynchronous
                var volunteer = await s_bl.Volunteer.GetVolunteerDetailsAsync(_volunteerId);
                VolunteerName = volunteer.FullName;

                // Create initials from name
                string[] nameParts = volunteer.FullName.Split(' ');
                if (nameParts.Length >= 2)
                {
                    VolunteerInitials = $"{nameParts[0][0]}{nameParts[1][0]}";
                }
                else if (nameParts.Length == 1 && nameParts[0].Length > 0)
                {
                    VolunteerInitials = nameParts[0][0].ToString();
                }
                else
                {
                    VolunteerInitials = "?";
                }

                VolunteerRole = volunteer.Role.ToString();

                Debug.WriteLine($"Volunteer loaded: {VolunteerName}, Role: {VolunteerRole}");

                // Notify UI
                OnPropertyChanged(nameof(VolunteerName));
                OnPropertyChanged(nameof(VolunteerInitials));
                OnPropertyChanged(nameof(VolunteerRole));

                Debug.WriteLine("LoadVolunteerDataAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading volunteer data: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading volunteer data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCallDataAsync()
        {
            try
            {
                Debug.WriteLine("LoadCallDataAsync started");

                // Get all closed calls for this volunteer - updated to async
                // Make sure this method is actually async in your API
                _closedCalls = s_bl.Call.GetClosedCallsByVolunteer(_volunteerId, null, null);

                // Debug output
                Debug.WriteLine($"Loaded {_closedCalls.Count()} closed calls for volunteer {_volunteerId}");

                // Force property changed notification for statistics
                OnPropertyChanged(nameof(TotalCalls));
                OnPropertyChanged(nameof(TreatedCalls));
                OnPropertyChanged(nameof(CanceledCalls));
                OnPropertyChanged(nameof(ExpiredCalls));

                Debug.WriteLine("LoadCallDataAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading call data: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading call data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _closedCalls = new List<ClosedCallInList>();
            }
        }

        public async Task LoadProfileImageAsync(int volunteerId)
        {
            try
            {
                Debug.WriteLine("LoadProfileImageAsync started");

                // Get image path from folder
                string imagePath = System.IO.Path.Combine(AppContext.BaseDirectory, ImageFolder, $"{volunteerId}.jpg");
                Debug.WriteLine($"Looking for profile image at: {imagePath}");

                if (File.Exists(imagePath))
                {
                    // Display image in WPF - using async file operations
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // Set the image source directly
                    await Dispatcher.InvokeAsync(() => {
                        if (ProfileImageBrush != null)
                        {
                            ProfileImageBrush.ImageSource = bitmap;
                            Debug.WriteLine("Profile image loaded successfully");
                        }
                        else
                        {
                            Debug.WriteLine("ProfileImageBrush is null");
                        }
                    });
                }
                else
                {
                    // Display default image if volunteer image doesn't exist
                    string defaultPath = System.IO.Path.Combine(AppContext.BaseDirectory, ImageFolder, "default.jpg");
                    Debug.WriteLine($"Profile image not found, trying default at: {defaultPath}");

                    if (File.Exists(defaultPath))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(defaultPath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        // Set the image source directly
                        await Dispatcher.InvokeAsync(() => {
                            if (ProfileImageBrush != null)
                            {
                                ProfileImageBrush.ImageSource = bitmap;
                                Debug.WriteLine("Default profile image loaded successfully");
                            }
                        });
                    }
                    else
                    {
                        // If default image doesn't exist, use a solid color
                        await Dispatcher.InvokeAsync(() => {
                            if (ProfileBorder != null)
                            {
                                ProfileBorder.Background = new SolidColorBrush(Color.FromRgb(229, 231, 235));
                                Debug.WriteLine("Using solid color as profile background");
                            }
                        });
                    }
                }

                Debug.WriteLine("LoadProfileImageAsync completed");
            }
            catch (Exception ex)
            {
                // If there's an error loading the image, just log it and continue
                Debug.WriteLine($"Error loading profile image: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Use a solid color as fallback
                await Dispatcher.InvokeAsync(() => {
                    if (ProfileBorder != null)
                    {
                        ProfileBorder.Background = new SolidColorBrush(Color.FromRgb(229, 231, 235));
                    }
                });
            }
        }

        private async Task LoadChartsAsync()
        {
            try
            {
                Debug.WriteLine("LoadChartsAsync started");

                // Generate chart URLs
                string statusChartUrl = GenerateCallStatusChartUrl();
                string typesChartUrl = GenerateCallTypesChartUrl();
                string timelineChartUrl = GenerateActivityTimelineChartUrl();

                Debug.WriteLine($"Status Chart URL: {statusChartUrl?.Substring(0, Math.Min(50, statusChartUrl?.Length ?? 0))}...");
                Debug.WriteLine($"Types Chart URL: {typesChartUrl?.Substring(0, Math.Min(50, typesChartUrl?.Length ?? 0))}...");
                Debug.WriteLine($"Timeline Chart URL: {timelineChartUrl?.Substring(0, Math.Min(50, timelineChartUrl?.Length ?? 0))}...");

                // Load all charts in parallel
                var statusChartTask = LoadChartImageAsync(statusChartUrl);
                var typesChartTask = LoadChartImageAsync(typesChartUrl);
                var timelineChartTask = LoadChartImageAsync(timelineChartUrl);

                // Wait for all tasks to complete
                await Task.WhenAll(statusChartTask, typesChartTask, timelineChartTask);

                // Update UI properties on the UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    CallStatusChartImage = statusChartTask.Result;
                    CallTypesChartImage = typesChartTask.Result;
                    ActivityTimelineChartImage = timelineChartTask.Result;

                    Debug.WriteLine($"Charts loaded: Status={CallStatusChartImage != null}, Types={CallTypesChartImage != null}, Timeline={ActivityTimelineChartImage != null}");
                });

                Debug.WriteLine("LoadChartsAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading charts: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading charts: {ex.Message}. Please check your internet connection.",
                    "Chart Loading Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task<ImageSource> LoadChartImageAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.WriteLine("Chart URL is empty, skipping load");
                return null;
            }

            try
            {
                Debug.WriteLine($"Loading chart from URL: {url?.Substring(0, Math.Min(50, url?.Length ?? 0))}...");

                // Download image data
                byte[] imageData = await httpClient.GetByteArrayAsync(url);
                Debug.WriteLine($"Downloaded {imageData.Length} bytes for chart");

                // Create bitmap image from data
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(imageData);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Important for cross-thread access

                Debug.WriteLine("Chart image created successfully");
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading chart from URL: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        // The chart URL generation methods remain unchanged
        private string GenerateCallStatusChartUrl()
        {
            // Method implementation unchanged
            // If no data, return empty string
            if (TotalCalls == 0)
            {
                Debug.WriteLine("No call data available for status chart");
                return "https://quickchart.io/chart?c=%7B%22type%22%3A%22pie%22%2C%22data%22%3A%7B%22labels%22%3A%5B%22No%20Data%20Available%22%5D%2C%22datasets%22%3A%5B%7B%22data%22%3A%5B1%5D%2C%22backgroundColor%22%3A%5B%22%23cccccc%22%5D%7D%5D%7D%2C%22options%22%3A%7B%22plugins%22%3A%7B%22legend%22%3A%7B%22position%22%3A%22right%22%7D%7D%7D%7D";
            }

            // Create chart configuration with improved colors
            var chartConfig = new
            {
                type = "pie",
                data = new
                {
                    labels = new string[] { "Treated", "Canceled", "Expired" },
                    datasets = new[]
                    {
                        new
                        {
                            data = new int[] { TreatedCalls, CanceledCalls, ExpiredCalls },
                            backgroundColor = new string[] { "#10B981", "#F59E0B", "#EF4444" },
                            borderColor = "#FFFFFF",
                            borderWidth = 2
                        }
                    }
                },
                options = new
                {
                    plugins = new
                    {
                        legend = new
                        {
                            position = "right",
                            labels = new
                            {
                                font = new
                                {
                                    size = 12
                                }
                            }
                        },
                        title = new
                        {
                            display = false
                        },
                        datalabels = new
                        {
                            color = "#FFFFFF",
                            font = new
                            {
                                weight = "bold"
                            },
                            formatter = "function(value) { return value > 0 ? value : ''; }"
                        }
                    }
                }
            };

            // Convert to JSON and encode for URL
            string jsonConfig = JsonSerializer.Serialize(chartConfig);
            string encodedConfig = Uri.EscapeDataString(jsonConfig);

            // Generate URL
            return $"https://quickchart.io/chart?c={encodedConfig}&w=400&h=250&bkg=transparent";
        }

        private string GenerateCallTypesChartUrl()
        {
            // Method implementation unchanged
            try
            {
                // If no data, return empty string
                if (TotalCalls == 0)
                {
                    Debug.WriteLine("No call data available for types chart");
                    return "https://quickchart.io/chart?c=%7B%22type%22%3A%22pie%22%2C%22data%22%3A%7B%22labels%22%3A%5B%22No%20Data%20Available%22%5D%2C%22datasets%22%3A%5B%7B%22data%22%3A%5B1%5D%2C%22backgroundColor%22%3A%5B%22%23cccccc%22%5D%7D%5D%7D%2C%22options%22%3A%7B%22plugins%22%3A%7B%22legend%22%3A%7B%22position%22%3A%22right%22%7D%7D%7D%7D";
                }

                // Group calls by type
                var callTypeGroups = _closedCalls
                    .GroupBy(c => c.Type)
                    .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                Debug.WriteLine($"Generated {callTypeGroups.Count} call type groups");
                foreach (var group in callTypeGroups)
                {
                    Debug.WriteLine($"Type: {group.Type}, Count: {group.Count}");
                }

                // Create chart configuration with vibrant colors
                var chartConfig = new
                {
                    type = "pie",
                    data = new
                    {
                        labels = callTypeGroups.Select(g => g.Type).ToArray(),
                        datasets = new[]
                        {
                            new
                            {
                                data = callTypeGroups.Select(g => g.Count).ToArray(),
                                backgroundColor = new string[]
                                {
                                    "#4F46E5", "#0EA5E9", "#8B5CF6", "#EC4899",
                                    "#10B981", "#F59E0B", "#EF4444", "#6366F1"
                                },
                                borderColor = "#FFFFFF",
                                borderWidth = 2
                            }
                        }
                    },
                    options = new
                    {
                        plugins = new
                        {
                            legend = new
                            {
                                position = "right",
                                labels = new
                                {
                                    font = new
                                    {
                                        size = 12
                                    }
                                }
                            },
                            title = new
                            {
                                display = false
                            },
                            datalabels = new
                            {
                                color = "#FFFFFF",
                                font = new
                                {
                                    weight = "bold"
                                },
                                formatter = "function(value) { return value > 0 ? value : ''; }"
                            }
                        }
                    }
                };

                // Convert to JSON and encode for URL
                string jsonConfig = JsonSerializer.Serialize(chartConfig);
                string encodedConfig = Uri.EscapeDataString(jsonConfig);

                // Generate URL
                return $"https://quickchart.io/chart?c={encodedConfig}&w=400&h=250&bkg=transparent";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating call types chart: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return ""; // Return empty string on error
            }
        }

        private string GenerateActivityTimelineChartUrl()
        {
            // Method implementation unchanged
            try
            {
                // If no data, return empty string
                if (TotalCalls == 0)
                {
                    Debug.WriteLine("No call data available for timeline chart");
                    return "https://quickchart.io/chart?c=%7B%22type%22%3A%22line%22%2C%22data%22%3A%7B%22labels%22%3A%5B%22No%20Data%20Available%22%5D%2C%22datasets%22%3A%5B%7B%22label%22%3A%22Calls%22%2C%22data%22%3A%5B0%5D%2C%22backgroundColor%22%3A%22%23cccccc%22%7D%5D%7D%2C%22options%22%3A%7B%22scales%22%3A%7B%22y%22%3A%7B%22beginAtZero%22%3Atrue%7D%7D%7D%7D";
                }

                // Group calls by month
                var activityByMonth = _closedCalls
                    .Where(c => c.CompletionTime != null)
                    .GroupBy(c => new DateTime(((DateTime)c.CompletionTime).Year, ((DateTime)c.CompletionTime).Month, 1))
                    .OrderBy(g => g.Key)
                    .Select(g => new { Month = g.Key.ToString("MMM yyyy"), Count = g.Count() })
                    .ToList();

                Debug.WriteLine($"Generated {activityByMonth.Count} activity months");
                foreach (var month in activityByMonth)
                {
                    Debug.WriteLine($"Month: {month.Month}, Count: {month.Count}");
                }

                // Create chart configuration with line chart instead of bar chart
                var chartConfig = new
                {
                    type = "line",
                    data = new
                    {
                        labels = activityByMonth.Select(m => m.Month).ToArray(),
                        datasets = new[]
                        {
                            new
                            {
                                label = "Calls per Month",
                                data = activityByMonth.Select(m => m.Count).ToArray(),
                                backgroundColor = "rgba(79, 70, 229, 0.2)",
                                borderColor = "#4F46E5",
                                borderWidth = 2,
                                pointBackgroundColor = "#4F46E5",
                                pointBorderColor = "#FFFFFF",
                                pointBorderWidth = 2,
                                pointRadius = 4,
                                pointHoverRadius = 6,
                                tension = 0.3,  // Makes the line curved for a smoother appearance
                                fill = true     // Fill area under the line
                            }
                        }
                    },
                    options = new
                    {
                        scales = new
                        {
                            y = new
                            {
                                beginAtZero = true,
                                title = new
                                {
                                    display = true,
                                    text = "Number of Calls"
                                },
                                grid = new
                                {
                                    display = true,
                                    color = "rgba(0,0,0,0.05)"
                                }
                            },
                            x = new
                            {
                                title = new
                                {
                                    display = true,
                                    text = "Month"
                                },
                                grid = new
                                {
                                    display = false
                                }
                            }
                        },
                        plugins = new
                        {
                            legend = new
                            {
                                display = false
                            },
                            title = new
                            {
                                display = false
                            }
                        }
                    }
                };

                // Convert to JSON and encode for URL
                string jsonConfig = JsonSerializer.Serialize(chartConfig);
                string encodedConfig = Uri.EscapeDataString(jsonConfig);

                // Generate URL
                return $"https://quickchart.io/chart?c={encodedConfig}&w=800&h=250&bkg=transparent";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating activity timeline chart: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return ""; // Return empty string on error
            }
        }

        // Property changed notification
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            // Ensure we're on the UI thread
            if (Dispatcher.CheckAccess())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Dispatcher.Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
            }
        }
    }
}