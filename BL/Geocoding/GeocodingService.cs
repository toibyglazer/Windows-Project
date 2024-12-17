using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;


using static System.Runtime.InteropServices.JavaScript.JSType;

public class GeoCoordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // תצוגת המידע בצורה נוחה לקריאה
    public override string ToString() => $"Latitude: {Latitude}, Longitude: {Longitude}";
}

public static class GeocodingService
{
    private static readonly HttpClient httpClient = new HttpClient();

    // קבלת קואורדינטות לפי כתובת
    public static GeoCoordinates CallOpenStreetMapApi(string address)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";

            // הוספת User-Agent לבקשה (נדרש במערכות OpenStreetMap)
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Application");

            // ביצוע קריאה סינכרונית
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = httpClient.Send(request); // קריאה סינכרונית
            var responseBody = response.Content.ReadAsStringAsync().Result;

            var jsonArray = JArray.Parse(responseBody);

            if (jsonArray.Count > 0)
            {
                var location = jsonArray[0];

                return new GeoCoordinates
                {
                    Latitude = double.Parse(location["lat"].ToString()),
                    Longitude = double.Parse(location["lon"].ToString())
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get geolocation: {ex.Message}");
        }

        return null;
    }

    // בדיקת קיום של כתובת במערכות OpenStreetMap
    public static bool VerifyAddressExists(string address)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Application");

            // ביצוע קריאה סינכרונית
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = httpClient.Send(request); // קריאה סינכרונית
            var responseBody = response.Content.ReadAsStringAsync().Result;

            var jsonArray = JArray.Parse(responseBody);

            return jsonArray.Count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to verify address existence: {ex.Message}");
            return false;
        }
    }
}
