using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using BO;
using System.Diagnostics;
using Helpers;

namespace PL
{
    /// <summary>
    /// Helper class for geocoding and coordinate operations
    /// </summary>
    public static class GeocodingHelper
    {
        // Cache for coordinates by address
        private static readonly ConcurrentDictionary<string, GeoCoordinates> _addressCache =
            new ConcurrentDictionary<string, GeoCoordinates>();

        // Cache for coordinates by volunteer ID
        private static readonly ConcurrentDictionary<int, GeoCoordinates> _volunteerCache =
            new ConcurrentDictionary<int, GeoCoordinates>();

        // HttpClient for API calls
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Gets the coordinates for a volunteer by ID
        /// </summary>
        /// <param name="volunteerId">The volunteer ID</param>
        /// <param name="address">The volunteer's address</param>
        /// <returns>An object containing latitude and longitude</returns>
        public static async Task<GeoCoordinates> GetVolunteerCoordinatesAsync(int volunteerId, string address)
        {
            // Check if coordinates are in volunteer cache
            if (_volunteerCache.TryGetValue(volunteerId, out var cachedCoordinates))
            {
                Debug.WriteLine($"Volunteer coordinates found in cache for volunteer {volunteerId}");
                return cachedCoordinates;
            }

            // If not in volunteer cache, calculate from address
            var coordinates = await GetCoordinatesForAddressAsync(address);

            // Save to volunteer cache
            if (coordinates != null)
            {
                _volunteerCache[volunteerId] = coordinates;
            }

            return coordinates;
        }

        /// <summary>
        /// Gets coordinates for an address
        /// </summary>
        /// <param name="address">The address to geocode</param>
        /// <returns>An object containing latitude and longitude</returns>
        public static async Task<GeoCoordinates> GetCoordinatesForAddressAsync(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            // Check if address is in cache
            if (_addressCache.TryGetValue(address, out var cachedCoordinates))
            {
                Debug.WriteLine($"Address coordinates found in cache: {address}");
                return cachedCoordinates;
            }

            try
            {
                // Use existing service to calculate coordinates
                var coordinates = await CallOpenStreetMapApiAsync(address);

                // Save to cache
                if (coordinates != null)
                {
                    _addressCache[address] = coordinates;
                    Debug.WriteLine($"Address coordinates calculated and cached: {address}");
                }

                return coordinates;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting coordinates for address: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets coordinates for multiple calls
        /// </summary>
        /// <param name="calls">List of calls</param>
        /// <returns>Dictionary mapping call ID to coordinates</returns>
        public static async Task<Dictionary<int, GeoCoordinates>> GetCoordinatesForCallsAsync(IEnumerable<OpenCallInList> calls)
        {
            var result = new Dictionary<int, GeoCoordinates>();

            foreach (var call in calls)
            {
                if (!string.IsNullOrEmpty(call.FullAddress))
                {
                    var coordinates = await GetCoordinatesForAddressAsync(call.FullAddress);
                    if (coordinates != null)
                    {
                        result[call.Id] = coordinates;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates distance between two geographic points
        /// </summary>
        /// <param name="lat1">Latitude of point 1</param>
        /// <param name="lon1">Longitude of point 1</param>
        /// <param name="lat2">Latitude of point 2</param>
        /// <param name="lon2">Longitude of point 2</param>
        /// <returns>Distance in kilometers</returns>
        public static async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2)
        {
            try
            {
                return await RouteCalculator.GetDrivingRouteDistanceAsync(lat1, lon1, lat2, lon2);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating distance: {ex.Message}");

                // If route calculation fails, fall back to straight-line distance
                return CalculateHaversineDistance(lat1, lon1, lat2, lon2);
            }
        }

        /// <summary>
        /// Calculates straight-line distance between two geographic points using Haversine formula
        /// </summary>
        private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0;

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            lat1 = DegreesToRadians(lat1);
            lat2 = DegreesToRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// Clears the cache for a specific volunteer
        /// </summary>
        public static void ClearVolunteerCache(int volunteerId)
        {
            _volunteerCache.TryRemove(volunteerId, out _);
        }

        /// <summary>
        /// Clears all caches
        /// </summary>
        public static void ClearAllCache()
        {
            _addressCache.Clear();
            _volunteerCache.Clear();
        }

        /// <summary>
        /// Checks if two sets of coordinates are approximately equal
        /// </summary>
        public static bool AreCoordinatesEqual(double lat1, double lon1, double lat2, double lon2, double epsilon = 0.01)
        {
            double latDifference = Math.Abs(lat1 - lat2);
            double lonDifference = Math.Abs(lon1 - lon2);
            return latDifference < epsilon && lonDifference < epsilon;
        }

        #region Geocoding
        public static async Task<GeoCoordinates> CallOpenStreetMapApiAsync(string address)
        {
            try
            {
                var url = $"https://nominatim.openstreetmap.org/search?format=xml&q={Uri.EscapeDataString(address)}";

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Application");

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                var doc = XDocument.Parse(responseBody);

                var place = doc.Root?.Element("place");
                if (place != null)
                {
                    return new GeoCoordinates
                    {
                        Latitude = double.Parse(place.Attribute("lat")?.Value ?? "0", CultureInfo.InvariantCulture),
                        Longitude = double.Parse(place.Attribute("lon")?.Value ?? "0", CultureInfo.InvariantCulture)
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get geolocation: {ex.Message}");
            }

            return null;
        }

        public static async Task<bool> VerifyAddressExistsAsync(string address)
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=xml&q={Uri.EscapeDataString(address)}";

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Application");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            var doc = XDocument.Parse(responseBody);
            var places = doc.Root?.Elements("place");

            if (places != null && places.Any())
            {
                return true;
            }
            throw new Exception("Address not found");
        }
        #endregion

        #region Route Calculator OpenStreetMap
        public class RouteCalculator
        {
            private static readonly HttpClient _httpClient = new HttpClient();

            public static async Task<double> GetRouteDistanceAsync(double lat1, double lon1, double lat2, double lon2, string mode)
            {
                if (AreCoordinatesEqual(lat1, lon1, lat2, lon2))
                    return 0.0;

                string url = $"https://routing.openstreetmap.de/routed-{mode}/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error fetching route distance. Status code: {response.StatusCode}");

                string json = await response.Content.ReadAsStringAsync();
                using (JsonDocument jsonDoc = JsonDocument.Parse(json))
                {
                    var distanceMeters = jsonDoc.RootElement.GetProperty("routes")[0].GetProperty("distance").GetDouble();
                    return distanceMeters / 1000.0;
                }
            }

            public static Task<double> GetWalkingRouteDistanceAsync(double lat1, double lon1, double lat2, double lon2)
                => GetRouteDistanceAsync(lat1, lon1, lat2, lon2, "foot");

            public static Task<double> GetDrivingRouteDistanceAsync(double lat1, double lon1, double lat2, double lon2)
                => GetRouteDistanceAsync(lat1, lon1, lat2, lon2, "car");
        }
        #endregion
    }
}