using System;
using System.Net.Http;
using System.Xml.Linq;

namespace Bl;

public static class GeocodingService
{
    private static readonly HttpClient httpClient = new HttpClient();

    // Get coordinates by address
    public static GeoCoordinates CallOpenStreetMapApi(string address)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=xml&q={Uri.EscapeDataString(address)}";

            // Add User-Agent to the request (required by OpenStreetMap systems)
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Application");

            // Synchronous request execution
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = httpClient.Send(request); // Synchronous request
            var responseBody = response.Content.ReadAsStringAsync().Result;

            var doc = XDocument.Parse(responseBody);

            // Check if there are results
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

    // Check if the address exists in OpenStreetMap systems
    public static bool VerifyAddressExists(string address)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=xml&q={Uri.EscapeDataString(address)}";

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Application");

            // Synchronous request execution
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = httpClient.Send(request); // Synchronous request
            var responseBody = response.Content.ReadAsStringAsync().Result;

            var doc = XDocument.Parse(responseBody);

            // Check if there is at least one place
            return doc.Root?.Element("place") != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to verify address existence: {ex.Message}");
            return false;
        }
    }
}
