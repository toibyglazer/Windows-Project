namespace BlTest;
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Geocoding Service Test ===");

        // הזן כתובת לבדיקה
        Console.Write("Enter an address: ");
        string address = Console.ReadLine();

        // בדיקת קיום הכתובת
        bool addressExists = GeocodingService.VerifyAddressExists(address);
        if (addressExists)
        {
            Console.WriteLine($"The address '{address}' exists in OpenStreetMap.");

            // קבלת קואורדינטות של הכתובת
            GeoCoordinates coordinates = GeocodingService.CallOpenStreetMapApi(address);
            if (coordinates != null)
            {
                Console.WriteLine("Coordinates:");
                Console.WriteLine(coordinates.ToString());
            }
            else
            {
                Console.WriteLine("Failed to retrieve coordinates for the address.");
            }
        }
        else
        {
            Console.WriteLine($"The address '{address}' does not exist in OpenStreetMap.");
        }
    }
}
