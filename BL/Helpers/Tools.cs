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
using System.IO;
using System.Net.Http;
/// <summary>
/// Represents geographical coordinates with latitude and longitude.
/// </summary>
public class GeoCoordinates
{
    public double Latitude { get; set; }  
    public double Longitude { get; set; } 

    // Override ToString method to present coordinates in a readable format
    public override string ToString() => $"Latitude: {Latitude}, Longitude: {Longitude}";
}
public static class Tools
{
    // Creating an HttpClient object for all API calls
    private static readonly HttpClient httpClient = new HttpClient();

    /// <summary>
    /// Converts the properties of an object to a formatted string.
    /// </summary>
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
    /// <param name="address">The address to get coordinates for.</param>
    /// <returns>GeoCoordinates object with latitude and longitude.</returns>
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
    /// <param name="address">The address to verify.</param>
    /// <returns>True if address exists, otherwise throws an exception.</returns>
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

    public static bool AreCoordinatesEqual(double lat1, double lon1, double lat2, double lon2, double epsilon = 0.01)
    {
        // חישוב ההבדל המוחלט בין הקואורדינטות
        double latDifference = Math.Abs(lat1 - lat2);
        double lonDifference = Math.Abs(lon1 - lon2);

        // אם ההבדל קטן מהסובלנות, נחזיר true
        return latDifference < epsilon && lonDifference < epsilon;
    }

    #region Route Calculator OpenStreetMap
    /// <summary>
    /// Gets the walking route distance between two geographical coordinates.
    /// This method uses the OpenStreetMap API to fetch the walking route distance between two points.
    /// </summary>
    /// <param name="lat1">The latitude of the first point.</param>
    /// <param name="lon1">The longitude of the first point.</param>
    /// <param name="lat2">The latitude of the second point.</param>
    /// <param name="lon2">The longitude of the second point.</param>
    /// <returns>The walking route distance in kilometers.</returns>
    public static double GetWalkingRouteDistance(double lat1, double lon1, double lat2, double lon2)
    {
        if (AreCoordinatesEqual(lat1, lon1, lat2, lon2))
            return 0.0;


        // Construct the URL for the OpenStreetMap routing API, using the provided coordinates
        string url = $"https://routing.openstreetmap.de/routed-foot/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false";

        // Create an instance of HttpClient to make the web request
        using (HttpClient client = new HttpClient())
        {
            // Send a GET request to the API
            HttpResponseMessage response = client.GetAsync(url).Result;

            // Check if the response was successful
            if (!response.IsSuccessStatusCode)
            {
                // Throw an exception if the request was unsuccessful
                throw new Exception($"Error fetching route distance. Status code: {response.StatusCode}");
            }

            // Read the response content as a string
            string json = response.Content.ReadAsStringAsync().Result;

            // Parse the JSON response to extract the distance in meters
            using (JsonDocument jsonDoc = JsonDocument.Parse(json))
            {
                var distanceMeters = jsonDoc.RootElement.GetProperty("routes")[0].GetProperty("distance").GetDouble();
                // Convert distance from meters to kilometers and return it
                return distanceMeters / 1000.0;
            }
        }
    }

    /// <summary>
    /// Gets the driving route distance between two geographical coordinates.
    /// This method uses the OpenStreetMap API to fetch the driving route distance between two points.
    /// </summary>
    /// <param name="lat1">The latitude of the first point.</param>
    /// <param name="lon1">The longitude of the first point.</param>
    /// <param name="lat2">The latitude of the second point.</param>
    /// <param name="lon2">The longitude of the second point.</param>
    /// <returns>The driving route distance in kilometers.</returns>
    public static double GetDrivingRouteDistance(double lat1, double lon1, double lat2, double lon2)
    {
        if (AreCoordinatesEqual(lat1, lon1, lat2, lon2))
            return 0.0;


        // Construct the URL for the OpenStreetMap routing API, using the provided coordinates
        string url = $"https://routing.openstreetmap.de/routed-car/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false";

        // Create an instance of HttpClient to make the web request
        using (HttpClient client = new HttpClient())
        {
            // Send a GET request to the API
            HttpResponseMessage response = client.GetAsync(url).Result;

            // Check if the response was successful
            if (!response.IsSuccessStatusCode)
            {
                // Throw an exception if the request was unsuccessful
                throw new Exception($"Error fetching route distance. Status code: {response.StatusCode}");
            }

            // Read the response content as a string
            string json = response.Content.ReadAsStringAsync().Result;

            // Parse the JSON response to extract the distance in meters
            using (JsonDocument jsonDoc = JsonDocument.Parse(json))
            {
                var distanceMeters = jsonDoc.RootElement.GetProperty("routes")[0].GetProperty("distance").GetDouble();
                // Convert distance from meters to kilometers and return it
                return distanceMeters / 1000.0;
            }
        }
    }
    #endregion


    #region Password Hashing and Verification Management Using PBKDF2
    /// <summary>
    /// This class provides methods for securely hashing and verifying passwords using PBKDF2.
    /// We have chosen to implement a strong hashing algorithm with salt, 
    /// which prevents the original password from being recovered or exposed.
    /// This additional feature is a bonus that we added ourselves for added security.
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Hashes the given password using PBKDF2 with a randomly generated salt.
        /// The PBKDF2 algorithm is designed to slow down brute-force attacks and make password cracking much harder.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password as a base64 encoded string, with salt and hash separated by a colon.</returns>
        public static string HashPassword(string password)
        {
            // Generate a random salt of 16 bytes
            byte[] salt = GenerateSalt(16);

            // Set the number of iterations for PBKDF2 to increase hashing difficulty
            int iterations = 10000;

            // Hash the password using PBKDF2 with the given salt and number of iterations
            using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                // Get the hash and return it as a base64 encoded string
                byte[] hash = rfc2898.GetBytes(32); // Generate 32-byte hash
                return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash); // Combine salt and hash
            }
        }

        /// <summary>
        /// Verifies if the given password matches the stored hash.
        /// This method splits the stored hash into salt and hash, then compares the computed hash for the given password.
        /// It uses a constant-time comparison to prevent timing attacks.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="storedHash">The stored hash (salt + hash) to compare against.</param>
        /// <returns>True if the password matches the stored hash, otherwise false.</returns>
        public static bool VerifyPassword(string password, string storedHash)
        {
            // Split the stored hash into salt and actual hash
            string[] parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[1]);

            // Set the number of iterations for PBKDF2 to match the original hashing process
            int iterations = 10000;

            // Hash the password using the same salt and iterations
            using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                // Generate the computed hash
                byte[] computedHash = rfc2898.GetBytes(32);

                // Use constant-time comparison to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
            }
        }

        /// <summary>
        /// Checks if the provided password is already hashed.
        /// This method verifies that the password string is in the correct format (salt:hash).
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <returns>True if the password is already hashed, otherwise false.</returns>
        public static bool IsHashed(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // Split the password string into salt and hash components
            var parts = password.Split(':');
            if (parts.Length != 2)
                return false;

            try
            {
                // Try to convert both parts (salt and hash) from base64
                Convert.FromBase64String(parts[0]); // salt
                Convert.FromBase64String(parts[1]); // hash
                return true;
            }
            catch (FormatException)
            {
                // If conversion fails, the format is invalid
                return false;
            }
        }

        /// <summary>
        /// Generates a random salt of the specified size to be used for hashing.
        /// This is a private helper function to ensure randomness and security.
        /// </summary>
        /// <param name="size">The size of the salt in bytes.</param>
        /// <returns>A byte array containing the generated salt.</returns>
        private static byte[] GenerateSalt(int size)
        {
            byte[] salt = new byte[size];

            // Use a cryptographically secure random number generator to generate the salt
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }
    }
    #endregion





    public class MapImageGenerator
    {
        private readonly string _volunteerLat;
        private readonly string _volunteerLng;
        private readonly string _callLat;
        private readonly string _callLng;

        private const string ApiKey = "pk.539fa4b064b6aef33fb2f2970f042808";

        public MapImageGenerator(string volunteerLat, string volunteerLng, string callLat, string callLng)
        {
            _volunteerLat = volunteerLat;
            _volunteerLng = volunteerLng;
            _callLat = callLat;
            _callLng = callLng;
        }

        public string GenerateMapImage()
        {
            string mapUrl = GenerateMapUrl();
            string imagePath = DownloadMapImage(mapUrl);

            return imagePath;
        }

        private string GenerateMapUrl()
        {
            // בניית ה-URL עם מיקומים ומסלול
            string markers = $"{_volunteerLat},{_volunteerLng}|{_callLat},{_callLng}";
            string path = $"{_volunteerLat},{_volunteerLng}|{_callLat},{_callLng}";

            return $"https://maps.locationiq.com/v3/staticmap?key={ApiKey}" +
                   $"&size=600x400&path=weight:3|color:blue|{path}" +
                   $"&markers={markers}";
        }

        private string DownloadMapImage(string mapUrl)
        {
            using HttpClient client = new HttpClient();
            var imageBytes = client.GetByteArrayAsync(mapUrl).Result;

            // שימוש בתיקיית בסיס יחסית לפרויקט או לסביבת הריצה
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Helpers", "Images");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string imageFileName = $"{Guid.NewGuid()}.png";
            string imagePath = Path.Combine(directoryPath, imageFileName);

            File.WriteAllBytes(imagePath, imageBytes);

            return imagePath;
        }

        public string GetIconPath()
        {
            // שימוש בנתיב יחסי ביחס לתיקיית הבסיס של הפרויקט
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Helpers", "Images");
            string iconFileName = "Google_Maps_icon_(2020).png";
            string iconPath = Path.Combine(directoryPath, iconFileName);

            if (File.Exists(iconPath))
            {
                return iconPath;
            }
            else
            {
                Console.WriteLine("Icon file not found.");
                return null;
            }
        }


    }

}    

