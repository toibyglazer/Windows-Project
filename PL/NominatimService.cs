using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PL
{
    public class NominatimService
    {
        private readonly HttpClient _httpClient;

        public NominatimService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "VolunteerManagementSystem/1.0");
        }

        /// <summary>
        /// Gets address suggestions from Nominatim API based on user input
        /// </summary>
        /// <param name="query">The search query (partial address)</param>
        /// <param name="requestId">Request ID for tracking in logs</param>
        /// <returns>A list of formatted address suggestions</returns>
        public async Task<List<string>> GetAddressSuggestionsAsync(string query, int requestId = 0)
        {
            // Return empty list if query is too short
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return new List<string>();
            }

            // Build URL with parameters to improve results
            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(query)}&addressdetails=1&limit=5&countrycodes=il&accept-language=he,en";

            try
            {
                // Send request to API
                var response = await _httpClient.GetStringAsync(url);

                // Deserialize response to objects using System.Text.Json
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var suggestions = JsonSerializer.Deserialize<List<NominatimResult>>(response, options);

                // Return formatted list of addresses
                if (suggestions == null || suggestions.Count == 0)
                {
                    return new List<string>();
                }

                var formattedAddresses = suggestions.Select(s => FormatAddress(s)).ToList();
                return formattedAddresses;
            }
            catch (Exception)
            {
                // Return empty list on error
                return new List<string>();
            }
        }

        /// <summary>
        /// Formats the address in a user-friendly way
        /// </summary>
        private string FormatAddress(NominatimResult result)
        {
            if (result.Address == null)
                return result.DisplayName;

            var parts = new List<string>();

            // Add street and house number if available
            if (!string.IsNullOrEmpty(result.Address.Road))
            {
                string roadPart = result.Address.Road;
                if (!string.IsNullOrEmpty(result.Address.HouseNumber))
                {
                    roadPart += " " + result.Address.HouseNumber;
                }
                parts.Add(roadPart);
            }

            // Add city/town/village
            string locality = result.Address.City ?? result.Address.Town ?? result.Address.Village ?? result.Address.Suburb;
            if (!string.IsNullOrEmpty(locality))
            {
                parts.Add(locality);
            }

            // Add country
            if (!string.IsNullOrEmpty(result.Address.Country))
            {
                parts.Add(result.Address.Country);
            }

            return string.Join(", ", parts);
        }
    }

    // מחלקות מודל עם System.Text.Json
    public class NominatimResult
    {
        public string DisplayName { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string Road { get; set; }
        public string HouseNumber { get; set; }
        public string City { get; set; }
        public string Town { get; set; }
        public string Village { get; set; }
        public string Suburb { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
    }
}