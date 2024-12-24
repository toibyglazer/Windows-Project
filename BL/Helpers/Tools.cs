using System.Linq;
using System.Reflection;
using System.Text.Json;
using System;
using System.Net.Http;
using System.Xml.Linq;
using System.Text.Json;

namespace Helpers;
public class GeoCoordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public override string ToString() => $"Latitude: {Latitude}, Longitude: {Longitude}";
}

internal static class Tools
{
    private static readonly HttpClient httpClient = new HttpClient();
    public static string ToStringProperty<T>(this T t)
    {
        if (t == null)
            return "The provided object is null and has no properties.";

        Type type = t.GetType();
        PropertyInfo[] properties = type.GetProperties();

        List<string> propertyStrings = new List<string>();
        foreach (PropertyInfo property in properties)
        {
            object value = property.GetValue(t);

            if (value == null)
            {
                propertyStrings.Add($"{property.Name}: null");
            }
            else if (value is IEnumerable<object> enumerable)
            {
                propertyStrings.Add($"{property.Name}: [{string.Join(", ", enumerable)}]");
            }
            else
            {
                propertyStrings.Add($"{property.Name}: {value}");
            }

        }
        return $"{type.Name} Properties:\n {string.Join(", ", propertyStrings)}";
    }

    #region Geocoding
    /// <summary>
    /// Gets the coordinates of a location by its address using OpenStreetMap API.
    /// </summary>
    public static GeoCoordinates CallOpenStreetMapApi(string address)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=xml&q={Uri.EscapeDataString(address)}";

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Application");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = httpClient.Send(request);
            var responseBody = response.Content.ReadAsStringAsync().Result;

            var doc = XDocument.Parse(responseBody);

            var place = doc.Root?.Element("place");
            if (place != null)
            {
                return new GeoCoordinates
                {
                    Latitude = double.Parse(place.Attribute("lat")?.Value ?? "0"),
                    Longitude = double.Parse(place.Attribute("lon")?.Value ?? "0")
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get geolocation: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Verifies if an address exists in OpenStreetMap systems.
    /// </summary>
    public static bool VerifyAddressExists(string address)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=xml&q={Uri.EscapeDataString(address)}";

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Application");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = httpClient.Send(request);
            var responseBody = response.Content.ReadAsStringAsync().Result;

            var doc = XDocument.Parse(responseBody);

            return doc.Root?.Element("place") != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to verify address existence: {ex.Message}");
            return false;
        }
    }
    #endregion

    #region Route Calculator OpenStreetMap
    public static double GetWalkingRouteDistance(double lat1, double lon1, double lat2, double lon2)
    {
        string url = $"https://routing.openstreetmap.de/routed-foot/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false";


        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error fetching route distance. Status code: {response.StatusCode}");
            }

            string json = response.Content.ReadAsStringAsync().Result;

            // Parse JSON response
            using (JsonDocument jsonDoc = JsonDocument.Parse(json))
            {
                var distanceMeters = jsonDoc.RootElement.GetProperty("routes")[0].GetProperty("distance").GetDouble();
                return distanceMeters / 1000.0; // Convert to kilometers
            }
        }
    }

    /// <summary>
    /// Gets the driving route distance between two points.
    /// </summary>
    /// <param name="lat1">The latitude of the first point.</param>
    /// <param name="lon1">The longitude of the first point.</param>
    /// <param name="lat2">The latitude of the second point.</param>
    /// <param name="lon2">The longitude of the second point.</param>
    /// <returns>The driving route distance in kilometers.</returns>
    public static double GetDrivingRouteDistance(double lat1, double lon1, double lat2, double lon2)
    {
        string url = $"https://routing.openstreetmap.de/routed-car/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error fetching route distance. Status code: {response.StatusCode}");
            }

            string json = response.Content.ReadAsStringAsync().Result;

            // Parse JSON response
            using (JsonDocument jsonDoc = JsonDocument.Parse(json))
            {
                var distanceMeters = jsonDoc.RootElement.GetProperty("routes")[0].GetProperty("distance").GetDouble();
                return distanceMeters / 1000.0; // Convert to kilometers
            }
        }
    }
    #endregion

}
