using System.Linq;
using System.Reflection;
using System.Text.Json;
using System;
using System.Net.Http;
using System.Xml.Linq;
using DalApi;
using System.Security.Cryptography;
using System.Text;
using System.Collections;
namespace Helpers;
public class GeoCoordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public override string ToString() => $"Latitude: {Latitude}, Longitude: {Longitude}";
}

public static class Tools
{
    private static readonly HttpClient httpClient = new HttpClient();
    public static string ToStringProperty<T>(this T t, int indentLevel = 0)
    {
        if (t == null)
            return $"{new string(' ', indentLevel)}The provided object is null and has no properties.";

        Type type = t.GetType();
        PropertyInfo[] properties = type.GetProperties();

        StringBuilder result = new StringBuilder();
        string indent = new string(' ', indentLevel);
        result.AppendLine($"{indent}{type.Name} Properties:");

        foreach (PropertyInfo property in properties)
        {
            object value = property.GetValue(t);
            string propertyName = property.Name;
            string valueStr = value?.ToString() ?? "null";

            if (value == null)
            {
                result.AppendLine($"{indent}  {propertyName}: {valueStr}");
            }
            else if (value is IEnumerable enumerable && !(value is string))
            {
                result.AppendLine($"{indent}  {propertyName}:");
                foreach (var item in enumerable)
                {
                    result.AppendLine(item.ToStringProperty(indentLevel + 4));
                }
            }
            else
            {
                result.AppendLine($"{indent}  {propertyName}: {valueStr}");
            }
        }

        return result.ToString();
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
    /// 
    public static bool VerifyAddressExists(string address)
    {
        var url = $"https://nominatim.openstreetmap.org/search?format=xml&q={Uri.EscapeDataString(address)}";

        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Application");

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = httpClient.Send(request);
        var responseBody = response.Content.ReadAsStringAsync().Result;

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

    #region Password Hashing and Verification Management Using PBKDF2
    public static class PasswordHasher
    {

        public static string HashPassword(string password)
        {

            byte[] salt = GenerateSalt(16);
            int iterations = 10000;

            using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                byte[] hash = rfc2898.GetBytes(32);
                return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
            }
        }

        // Function to verify a password against a stored hash
        public static bool VerifyPassword(string password, string storedHash)
        {

            string[] parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[1]);
            int iterations = 10000;
            using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                byte[] computedHash = rfc2898.GetBytes(32);
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
            }
        }
        public static bool IsHashed(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            var parts = password.Split(':');
            if (parts.Length != 2)
                return false;

            try
            {
                
                Convert.FromBase64String(parts[0]); // salt
                Convert.FromBase64String(parts[1]); // hash
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        // Private function to generate a random salt
        private static byte[] GenerateSalt(int size)
        {
            byte[] salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
    #endregion


}
