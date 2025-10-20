namespace DalTest;
using DalApi;
using DO;
using Dal;
using Microsoft.VisualBasic;
using System.Data;
using System.Security.Cryptography;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

public static class Initialization
{

    private static IDal? s_dal;
    private static readonly Random s_rand = new();
    private static readonly object _lock = new object();
    /// <summary>
    /// Initializes  a list of volunteers with predefined data and saves them using the DAL (Data Access Layer) object.
    /// The first volunteer is assigned as a Manager, and the rest are regular volunteers.
    /// </summary>
    static private void createVolunteer()
    {
       
            string[] VolunteerNames = new[]
{
    "David Levi",
    "Rachel Cohen",
    "Moshe Mizrahi",
    "Tamar Kaplan",
    "Yosef Goldstein",
    "Dani Brening",
    "Avi Rosenberg",
    "Hila Sharabi",
    "Noam Azulai",
    "Yael Peretz",
    "Eli Barak",
    "Michal Nave",
    "Shai Elbaz",
    "Dana Regev",
    "Nadav Shamir",
    "Orly Yitzhak",
    "Daniel Vaknin",
    "Adi Kadosh"
};
        string[] VolunteerEmails = new string[]
{
    "toibyglazer@gmail.com",
    "danihadarr@gmail.com",
    "shanysh3434@gmail.com",
    "rosensh3434@gmail.com",
    "S0584404949@gmail.com",
    "esther.glazer2017@gmail.com",
    "100origlazer@gmail.com",
    "arie7209@gmail.com",
    "hodayahararii@gmail.com",
    "rothfamily2019@gmail.com",
    "yaeltamir21@gmail.com",
    "ittichaimson@gmail.com",
    "LIEMRAMADI11@gmail.com",
    "zermichal500@gmail.com",
    "nadavenav@gmail.com",
    "davidkarga98@gmail.com",
    "100bat7@gmail.com",
    "zermichal555@gmail.com"
};
        string[] addresses = new[]
{
    "10 Bograshov, Tel Aviv, Israel",
    "1 Allenby, Tel Aviv-Yafo, Israel",
    "34 Herzl, Haifa, Israel",
   "12 Kaplan, Rishon Lezion, Israel",
    "56 Ben Yehuda, Jerusalem, Israel",
    "45 Ben Gurion Avenue, Ashdod, Israel",
    "9 King David, Tel Aviv, Israel",
    "77 Rothschild Boulevard, Tel Aviv-Yafo, Israel",
    "58 Jaffa, Jerusalem, Israel",
    "21 Yehuda Halevi, Tel Aviv, Israel",
    "68 David Ben Gurion, Herzliya, Israel",
    "33 Jabotinsky, Bat Yam, Israel",
    "12 Dubnov, Haifa, Israel",
    "88 Shazar, Ramat Gan, Israel",
    "17 Sokolov, Petah Tikva, Israel",
    "44 Yigal Alon, Tel Aviv, Israel",
    "76 Nahalat Binyamin, Tel Aviv, Israel",
    "22 Rambam, Be'er Sheva, Israel",
    "15 HaNassi, Netanya, Israel",
    "50 Ibn Gvirol, Tel Aviv, Israel"
};
        string[] passwords = new string[]
{
    "D@v1dL3v!2024",
    "R@ch3lC0hen#99",
    "M0sh3M1zrah1*!",
    "T@m4rK@plan!88",
    "Y0s3fG0ld#te!n",
    "E5th3rB3n$h1mon",
    "Av!R0senb3rg^77",
    "H1la$Shar@b!2024",
    "N0@mAzuL@i#555",
    "Ya3lPer3tz$2024",
    "El!Bar@k88!888",
    "M1chalN@v3!2#44",
    "Sh@iElb@z!5599",
    "D@n4Regev!$2024",
    "N@d@vSham1r^#44",
    "Orl!Y1tz#h@k77",
    "D@n!elV@kn!n$$",
    "Ad1K@d0sh!2024"
};
        double[] latitudes = new double[]
{
    32.0853, 32.8217, 31.7683,  31.9686,
     31.8056, 32.1093, 32.3218, 31.2518,
       32.0126, 31.2488,  29.5581, 31.8935,
          31.9061, 32.0809, 32.0730,   32.1585,
            31.6360, 33.0093
};
        double[] longitudes = new double[]
        {
    34.7818,
    34.9896,
    35.2137,
    34.8013,
    34.6439,
    34.8885,
    34.8531,
    34.7913,
    34.7701,
    34.7687,
    34.9519,
    34.8131,
    34.8699,
    34.7670,
    34.8197,
    34.9215,
    34.5663,
    35.0873
        };
        string[] phoneNumbers = new string[]
{
    "052-1234567", "054-2345678", "050-3456789",
    "052-4567890", "054-5678901", "050-6789012",
    "052-7890123", "054-8901234", "050-9012345",
    "052-0123456", "054-1234567", "050-2345678",
    "052-3456789", "054-4567890", "050-5678901",
    "052-6789012", "054-7890123", "050-8901234"
};
        double[] maxDistances = new[]
{
    1.5, 2.0, 3.0,
    4.5, 5.0, 6.0,
    7.5, 8.0, 10.0,
    12.5, 15.0, 18.0,
    20.0, 22.5, 25.0,
    30.0, 35.0, 40.0
};

        // Generate an ID and assign the first volunteer as the Manager.
        int[] volunteerIds = new int[]
  {
    735927006, 985810993, 553026436, 614562379,
    912011889, 680412384, 434477790, 705204873,
    901234567, 294703103, 255335531, 345210678,
    374306371, 523220655, 678543901, 281041475,
    489627356, 154553671
  };

      
        DistanceType firstDistanceType = (DistanceType)s_rand.Next(Enum.GetValues(typeof(DistanceType)).Length);

        
        //var imageProfileFirstPath = GetImagePathForVolunteer(volunteerIds[0]);
        var firstVolunteer = new Volunteer
            (
                Id: volunteerIds[0],
                FullName: VolunteerNames[0],
                PhoneNumber: phoneNumbers[0],
                Email: VolunteerEmails[0],
                Password: PasswordHasher.HashPassword(passwords[0]),
                CurrentAddress: addresses[0],
                Latitude: latitudes[0],
                Longitude: longitudes[0],
                Role: VolunteerRole.Manager,
                IsActive: true,
                MaxDistance: maxDistances[0],
                DistanceType: firstDistanceType
                //ImageProfilePath: imageProfileFirstPath
           );
        s_dal!.Volunteer.Create(firstVolunteer); //stage 2

            // Iterate over the rest of the volunteers and assign them as regular volunteers.
            for (int i = 1; i < VolunteerNames.Length; i++)
            {

                DistanceType d = (DistanceType)s_rand.Next(Enum.GetValues(typeof(DistanceType)).Length);
           
                //var imageProfilePath = GetImagePathForVolunteer(volunteerIds[i]);
            //if (!File.Exists(imageProfilePath))
            //{
            //    throw new FileNotFoundException($"Image not found for volunteer {volunteerIds[i]}");
            //}

            var volunteer = new Volunteer
                (
                    Id: volunteerIds[i],
                    FullName: VolunteerNames[i],
                    PhoneNumber: phoneNumbers[i],
                    Email: VolunteerEmails[i],
                    Password: PasswordHasher.HashPassword(passwords[i]),
                    CurrentAddress: addresses[i],
                    Latitude: latitudes[i],
                    Longitude: longitudes[i],
                    Role: VolunteerRole.Volunteer,
                    IsActive: true,
                    MaxDistance: maxDistances[i],
                    DistanceType: d
                    //ImageProfilePath: imageProfilePath
               );

                s_dal!.Volunteer.Create(volunteer); //stage 2
              
            }
        
    }


    //private static string GetImagePathForVolunteer(int volunteerId)
    //{
    //    string imageDirectory = "ProfileImage";
    //    return Path.Combine(imageDirectory, $"{volunteerId}.jpg");
    //}
    public static DateTime GetRandomDateTimeBetween(DateTime fromDate, DateTime toDate)
    {
        if (fromDate >= toDate.AddSeconds(-2))
            throw new ArgumentException("טווח זמנים קצר מדי – חייבים לפחות שתי שניות הפרש.");

        Random rand = new Random();
        double totalSeconds = (toDate - fromDate).TotalSeconds;
        int secondsToAdd = rand.Next(1, (int)totalSeconds - 1);
        return fromDate.AddSeconds(secondsToAdd);
    }
    public static DateTime[] openingTimesTreated = new DateTime[15];
    public static DateTime[] completionTimesTreated = new DateTime[15];

    public static DateTime[] openingTimes_Cancelled = new DateTime[5];
    public static DateTime?[] completionTimes_Cancelled = new DateTime?[5];

    public static DateTime[] openingTimes_Admin = new DateTime[6];
    public static DateTime[] completionTimes_Admin = new DateTime[6];

    public static DateTime[] openingTimes_Expired = new DateTime[10];
    public static DateTime[] completionTimes_Expired = new DateTime[10];

    public static DateTime[] openingTimes = new DateTime[14];
    public static DateTime[] completionTimes = new DateTime[14];

    public static DateTime[] AllOpeningTimes = new DateTime[56];
    public static DateTime?[] AllMaxCompletionTimes = new DateTime?[56];

    static private void createCall()
    {
       

        
            string[] Descriptions = new[]
        {
             "Food delivery for a family",
    "Transporting medical supplies",
    "Delivering fresh groceries",
    "Fixing plumbing issues",
    "Electrical repair assistance",
    "Household general delivery",
    "Pharmacy medicines delivery",
    "Helping with furniture assembly",
    "Transporting construction materials",
    "Catering setup for an event",
    "Setting up a training kit",
    "Transporting soldiers to the base",
    "Emergency response support",
    "Complex logistics transport",
    "Medical backup team deployment",
    "Base maintenance routine",
    "Special event coordination",
    "Moving heavy equipment",
    "Preparation for a military event",
    "Support for local celebrations",
    "Delivery of kitchen supplies",
    "Urgent food preparation",
    "Assistance with inventory setup",
    "Transporting food packages",
    "Base inspection preparation",
    "Coordinate emergency evacuations",
    "Tech equipment delivery",
    "Setting up event decorations",
    "Medical supply restocking",
    "Carrying out logistic adjustments",
    "Cleaning supplies transport",
    "Delivering backup kits",
    "Overseeing emergency drills",
    "Inspection aid for vehicles",
    "Event tents setup",
    "Base structural repair",
    "Training camp assistance",
    "Monitoring emergency setups",
    "Tools delivery for the unit",
    "Transport for external logistics",
    "Base cleanup operations",
    "Lighting arrangements for events",
    "On-site medical first aid",
    "Providing logistical support",
    "Immediate repair solutions",
    "Packing kits for shipment",
    "Urgent communication delivery",
    "Reinforcing base logistics",
    "Backup power supply delivery",
    "Finalizing event logistics",
    "Transporting donated goods",
    "Organizing event volunteers",
    "Emergency vehicle maintenance",
    "Distributing emergency rations",
    "Delivering urgent medical kits",
    "Setting up temporary shelters"

        };
        string[] Addresses = new[] {
    "1 Herzl, Tel Aviv, Israel",
    "15 HaNassi, Haifa, Israel",
    "5 Jaffa, Jerusalem, Israel",
    "10 Weizmann, Kfar Saba, Israel",
    "9 Begin, Be'er Sheva, Israel",
    "20 Rothschild Boulevard, Tel Aviv, Israel",
    "9 HaYehudim, Jerusalem, Israel",
    "15 Rothschild Boulevard, Tel Aviv, Israel",
    "5 HaGefen, Nahariya, Israel",
    "17 Jabotinsky, Ramat Gan, Israel",
    "4 Jerusalem, Holon, Israel",
    "11 HaNasi, Ra'anana, Israel",
    "1 HaAtzmaut, Petah Tikva, Israel",
    "8 Bialik, Ramat HaSharon, Israel",
    "5 HaNasi, Ra'anana, Israel",
    "18 Allenby, Tel Aviv, Israel",
    "13 Eilat Boulevard, Ashkelon, Israel",
    "9 HaYarden, Afula, Israel",
    "12 Ben Gurion, Haifa, Israel",
    "25 Herzl, Netanya, Israel",
    "50 Dizengoff, Tel Aviv, Israel",
    "30 HaNeviim, Jerusalem, Israel",
    "14 Ben Zvi, Be'er Sheva, Israel",
    "9 HaGefen, Karmiel, Israel",
    "19 Rabin, Modi'in, Israel",
    "3 HaYam, Haifa, Israel",
    "24 , Tel Aviv-Yafo,  Israel",
    "5 Galilee, Safed, Israel",
    "12 Rothschild Road, Rishon LeZion, Israel",
    "6 Wolfson, Herzliya, Israel",
    "28 Jabotinsky, Petah Tikva, Israel",
    "17 Rothschild Boulevard, Tel Aviv, Israel",
    "5 Ben Gurion, Haifa, Israel",
    "18 Menachem Begin, Tel Aviv, Israel",
    "4 Sokolov, Holon, Israel",
    "12 Haatzmaut, Ramat Gan, Israel",
    "20 Herzl, Be'er Sheva, Israel",
    "12 HaRimon, Beit Shemesh, Israel",
    "10 Bialik, Kfar Saba, Israel",
    "15 David Ben Gurion, Petah Tikva, Israel",
    "6 Hillel, Jerusalem, Israel",
    "20 HaShalom, Givatayim, Israel",
    "15 HaNarkis, Yavne, Israel",
    "14 Jabotinsky, Rishon Lezion, Israel",
    "7 HaBanim, Ramat HaSharon, Israel",
    "22 King David, Jerusalem, Israel",
    "5 HaTamar, Ashkelon, Israel",
    "18 HaAtzmaut, Kiryat Gat, Israel",
    "11 Golda Meir, Tel Aviv, Israel",
    "7 HaGalil, Safed, Israel",
    "17 Shalom Aleichem, Be'er Sheva, Israel",
    "11 Menachem Begin, Petah Tikva, Israel",
    "5 HaPlada, Ashkelon, Israel",
    "18 Ibn Gabirol, Tel Aviv, Israel",
    "12 Arlozorov, Herzliya, Israel",
    "5 HaTmarim, Eilat, Israel"
};

        double[] latitudes = new[]
{
    32.0853, 32.7940, 31.7683, 32.1097, 31.2518,
    32.0853, 31.7683, 31.8015, 33.2077, 32.0736,
    32.0320, 32.1833, 32.0790, 32.1635, 32.0378,
    32.0853, 31.6683, 32.6300, 32.5887, 32.3216,
    32.3216, 31.7683, 31.2518, 32.7045, 31.9466,
    32.0853, 29.5581, 33.0471, 31.9685, 32.0779,
    32.1701, 32.0730, 32.2662, 31.7800, 32.0853,
    32.1097, 31.8717, 31.6513, 32.2090, 32.1585,
    31.2518, 31.7719, 32.0924, 32.1099, 29.5581,
    32.0735, 29.5581, 33.0471, 31.9685, 32.0779,
    32.3216, 31.7683, 31.2518, 32.7045, 31.9466,
    32.1701
};
        double[] longitudes = new[]
{
    34.7818, 34.9897, 35.2137, 34.9023, 34.7913,
    34.7818, 35.2137, 34.6525, 35.0907, 34.8144,
    34.7793, 34.8753, 34.8722, 34.8559, 34.8046,
    34.7818, 34.5717, 35.2778, 35.5894, 34.6937,
    34.6937, 35.2137, 34.7913, 34.9318, 35.1815,
    34.7818, 34.7818, 35.3955, 34.8015, 34.8291,
    34.8510, 34.8322, 34.8574, 35.2825, 34.8270,
    34.8555, 35.2190, 34.8259, 34.8560, 34.7902,
    34.8046, 34.8193, 35.2227, 35.2683, 34.7740,
    34.7722, 34.5717, 35.2778, 35.5894, 34.6937,
    34.7818, 34.7818, 35.3955, 34.8015, 34.8291,
    34.7740
};



        int[] callIds = Enumerable.Range(56, 112).ToArray();
        DO.CallType[] callTypes = new DO.CallType[Descriptions.Length];
        for (int i = 0; i < Descriptions.Length; i++)
        {
            callTypes[i] = (DO.CallType)s_rand.Next(Enum.GetValues(typeof(DO.CallType)).Length);
        }

        DateTime fixedOpeningTime = new DateTime(2025, 4, 1, 8, 0, 0); // זמן פתיחה אחיד לכל ה־15
        DateTime now = s_dal.Config.Clock;

        DateTime[] openingTimesTreated = new DateTime[15]; // הקריאות שטופלו
        DateTime[] maxCompletionTimesTreated = new DateTime[15];


        for (int i = 0; i < 15; i++)
        {
            DateTime maxCompletionTime = GetRandomDateTimeBetween(fixedOpeningTime, now);
            AllOpeningTimes[i] = fixedOpeningTime;
            AllMaxCompletionTimes[i] = maxCompletionTime;

            var call = new Call(
                Id: callIds[i],
                Type: callTypes[i],
                FullAddress: Addresses[i],
                Latitude: latitudes[i],
                Longitude: longitudes[i],
                OpeningTime: fixedOpeningTime,
                Description: Descriptions[i],
                MaxCompletionTime: maxCompletionTime
            );

            s_dal!.Call.Create(call);
        }


        DateTime baseTime = now.AddDays(-30);
        for (int i = 15; i < 20; i++)
        {
            DateTime openingTime = GetRandomDateTimeBetween(baseTime, now.AddDays(-1));
            DateTime maxCompletionTime = GetRandomDateTimeBetween(openingTime.AddMinutes(2), now);

            AllOpeningTimes[i] = openingTime;
            AllMaxCompletionTimes[i] = maxCompletionTime;

            var call = new Call(
                Id: callIds[i],
                Type: callTypes[i],
                FullAddress: Addresses[i],
                Latitude: latitudes[i],
                Longitude: longitudes[i],
                OpeningTime: openingTime,
                Description: Descriptions[i],
                MaxCompletionTime: maxCompletionTime
            );

            s_dal!.Call.Create(call);
        }

        for (int i = 20; i < 26; i++)
        {
            DateTime openingTime = now.AddDays(-s_rand.Next(1, 30));
            DateTime maxCompletionTime = GetRandomDateTimeBetween(openingTime, now);

            AllOpeningTimes[i] = openingTime;
            AllMaxCompletionTimes[i] = maxCompletionTime;

            var call = new Call(
                Id: callIds[i],
                Type: callTypes[i],
                FullAddress: Addresses[i],
                Latitude: latitudes[i],
                Longitude: longitudes[i],
                OpeningTime: openingTime,
                Description: Descriptions[i],
                MaxCompletionTime: maxCompletionTime
            );

            s_dal!.Call.Create(call);
        }
        for (int i = 26; i < 36; i++)
        {
            DateTime openingTime = now.AddDays(-s_rand.Next(1, 100));
            DateTime maxCompletionTime = GetRandomDateTimeBetween(openingTime, now);

            AllOpeningTimes[i] = openingTime;
            AllMaxCompletionTimes[i] = maxCompletionTime;

            var call = new Call(
                Id: callIds[i],
                Type: callTypes[i],
                FullAddress: Addresses[i],
                Latitude: latitudes[i],
                Longitude: longitudes[i],
                OpeningTime: openingTime,
                Description: Descriptions[i],
                MaxCompletionTime: maxCompletionTime
            );

            s_dal!.Call.Create(call);
        }


        for (int i = 36; i < 56; i++)
        {
            DateTime openingTime = now.AddDays(-s_rand.Next(1, 30));
            DateTime maxCompletionTime = GetRandomDateTimeBetween(now, now.AddDays(30));

            AllOpeningTimes[i] = openingTime;
            AllMaxCompletionTimes[i] = maxCompletionTime;

            var call = new Call(
                Id: callIds[i],
                Type: callTypes[i],
                FullAddress: Addresses[i],
                Latitude: latitudes[i],
                Longitude: longitudes[i],
                OpeningTime: openingTime,
                Description: Descriptions[i],
                MaxCompletionTime: maxCompletionTime
            );

            s_dal!.Call.Create(call);
        }


    }
    /// <summary>
    /// Creates and initializes a predefined set of Assignment objects with varying completion types.
    /// Assignments are created for specific volunteer IDs, and the completion type is determined based on their index.
    /// </summary>

    private static void createAssignment()
    {



        int[] callIds = Enumerable.Range(1, 56).ToArray();
        int[] volunteerIds = new int[]
{
    735927006, 985810993, 553026436, 614562379,
    912011889, 680412384, 434477790, 705204873,
    901234567, 294703103, 255335531, 345210678,
    374306371, 523220655, 678543901, 281041475,
    489627356, 154553671, 735927006, 985810993,
    912011889, 680412384, 434477790, 705204873,
    901234567, 294703103, 255335531, 345210678,
    374306371, 523220655, 678543901, 281041475,
    489627356, 154553671, 553026436, 614562379,
    912011889, 680412384, 434477790, 705204873,
    901234567, 294703103, 255335531, 345210678,
    374306371, 523220655, 678543901, 281041475,
    489627356, 154553671, 553026436, 614562379,
    374306371, 523220655, 678543901, 281041475,
    489627356, 154553671, 735927006, 985810993,
    912011889, 680412384, 434477790, 705204873,
    901234567, 294703103, 255335531, 345210678,
    374306371, 523220655, 678543901, 281041475,
    489627356, 154553671, 553026436, 614562379,
    374306371, 523220655, 678543901, 281041475,
    489627356, 154553671, 735927006, 985810993,
     735927006, 985810993, 553026436, 614562379,
     374306371, 523220655, 678543901, 281041475,
    489627356, 154553671, 553026436, 614562379,
    374306371, 523220655, 678543901, 281041475,
    912011889, 680412384, 434477790, 705204873,
    901234567, 294703103, 255335531, 345210678
};

        int assignmentId = 0;
        DateTime now = s_dal.Config.Clock;

        for (int i = 0; i < 15; i++)
        {
            DateTime openingTime = AllOpeningTimes[i];
            DateTime? maxCompletionTime = AllMaxCompletionTimes[i] ?? now;

            // כניסה לאחר זמן פתיחה
            TimeSpan timeSinceOpening = maxCompletionTime.Value - openingTime;
            DateTime entryTime;

            if (timeSinceOpening.TotalMinutes >= 1)
            {
                int maxOffset = (int)Math.Min(30, timeSinceOpening.TotalMinutes);
                int offset = s_rand.Next(1, maxOffset + 1);
                entryTime = openingTime.AddMinutes(offset);
            }
            else
            {
                entryTime = openingTime.AddSeconds(1);
            }

            // סיום הטיפול לאחר הכניסה אבל לפני זמן המקסימום
            DateTime completionTime = GetRandomDateTimeBetween(entryTime.AddSeconds(1), maxCompletionTime.Value);

            Assignment a = new Assignment(
                Id: assignmentId++,
                CallId: callIds[i],
                VolunteerId: volunteerIds[i],
                EntryTime: entryTime,
                CompletionTime: completionTime,
                CompletionType: AssignmentCompletionType.Treated
            );

            s_dal!.Assignment.Create(a);
        }
       
        for (int i = 15; i < 20; i++)
        {
            DateTime openingTime = AllOpeningTimes[i];
            DateTime maxTime = AllMaxCompletionTimes[i] ?? now;
            DateTime entryTime = GetRandomDateTimeBetween(openingTime.AddMinutes(1), maxTime.AddMinutes(-1));

            Assignment a = new Assignment(
                Id: assignmentId++,
                CallId: callIds[i],
                VolunteerId: volunteerIds[i],
                EntryTime: entryTime,
                CompletionTime: null, // לא הושלם
                CompletionType: AssignmentCompletionType.VolunteerCancelled
            );

            s_dal!.Assignment.Create(a);
        }

        for (int i = 20; i < 26; i++)
        {
            DateTime openingTime = AllOpeningTimes[i];
            DateTime maxTime = AllMaxCompletionTimes[i] ?? now;
            DateTime entryTime = GetRandomDateTimeBetween(openingTime.AddMinutes(1), maxTime.AddMinutes(-1));

            Assignment a = new Assignment(
                Id: assignmentId++,
                CallId: callIds[i],
                VolunteerId: volunteerIds[i],
                EntryTime: entryTime,
                CompletionTime: null, // עדיין ממתין לאישור
                CompletionType: AssignmentCompletionType.AdminCancelled
            );

            s_dal!.Assignment.Create(a);
        }
        for (int i = 26; i < 36; i++)
        {
            DateTime openingTime = AllOpeningTimes[i];
            DateTime maxTime = AllMaxCompletionTimes[i] ?? now;
            DateTime entryTime = GetRandomDateTimeBetween(openingTime.AddMinutes(1), maxTime.AddMinutes(-1));

            Assignment a = new Assignment(
                Id: assignmentId++,
                CallId: callIds[i],
                VolunteerId: volunteerIds[i],
                EntryTime: entryTime,
                CompletionTime: null, // פג תוקף - לא טופל
                CompletionType: AssignmentCompletionType.Expired
            );

            s_dal!.Assignment.Create(a);
        }
        // הקצאות לקריאות פתוחות
        for (int i = 36; i < 45; i++)
        {
            DateTime openingTime = AllOpeningTimes[i];

            // נוודא שזמן ההקצאה יהיה לפחות דקה אחרי הפתיחה
            DateTime entryTime = openingTime.AddMinutes(s_rand.Next(1, 30));

            Assignment a = new Assignment(
                Id: assignmentId++,
                CallId: callIds[i],
                VolunteerId: volunteerIds[i],
                EntryTime: entryTime,
                CompletionTime: null,             // עדיין לא הסתיים
                CompletionType: null              // עדיין לא ידוע
            );

            s_dal!.Assignment.Create(a);
        }



        //DateTime entryTimeFromPast = new DateTime(2025, 04, 03, 3, 28, 0);
        //DateTime currentTime = s_dal.Config.Clock;

        //for (int i = 0; i < 15; i++)
        //{
        //    Assignment a = new Assignment(
        //        Id: assignmentId,
        //        CallId: callIds[i],
        //        VolunteerId: volunteerIds[i],
        //        EntryTime: entryTimeFromPast,
        //        CompletionTime: GetRandomDateTimeBetween(entryTimeFromPast, currentTime),
        //        CompletionType: AssignmentCompletionType.Treated
        //    );
        //    s_dal!.Assignment.Create(a);
        //}

        //// Create assignments with CompletionType.VolunteerCancelled for the next set of volunteers.
        //for (int i = 15; i < 29; i++)
        //    {
        //        Assignment a = new Assignment(
        //            Id: assignmentId,
        //            CallId: callIds[i],
        //            VolunteerId: volunteerIds[i],
        //            EntryTime: entryTimeFromPast,
        //            CompletionTime: GetRandomDateTimeBetween(entryTimeFromPast, currentTime),
        //            CompletionType: AssignmentCompletionType.VolunteerCancelled
        //        );
        //        s_dal!.Assignment.Create(a); //stage 2
        //    }

        //    // Create assignments with CompletionType.AdminCancelled for the next set of volunteers.
        //    for (int i = 29; i < 40; i++)
        //    {
        //        Assignment a = new Assignment(
        //            Id: assignmentId,
        //            CallId: callIds[i],
        //            VolunteerId: volunteerIds[i],
        //            EntryTime: entryTimeFromPast,
        //            CompletionTime: GetRandomDateTimeBetween(entryTimeFromPast, currentTime),
        //            CompletionType: AssignmentCompletionType.AdminCancelled
        //        );
        //        s_dal!.Assignment.Create(a); //stage 2
        //    }

        //    // Create assignments with CompletionType.Expired for the remaining volunteers.
        //    for (int i = 40; i < 45; i++)
        //    {
        //        Assignment a = new Assignment(
        //            Id: assignmentId,
        //            CallId: callIds[i],
        //            VolunteerId: volunteerIds[i],
        //            EntryTime: entryTimeFromPast,
        //            CompletionTime: GetRandomDateTimeBetween(entryTimeFromPast, currentTime),
        //            CompletionType: AssignmentCompletionType.Expired
        //        );
        //        s_dal!.Assignment.Create(a); //stage 2

        //    }
        ////int j = 0;
        //for (int i = 45; i < 50; i++)
        //    {

        //    Assignment a = new Assignment(
        //      Id: assignmentId,
        //      CallId: callIds[i],
        //      VolunteerId: volunteerIds[i],
        //      EntryTime: s_dal.Config.Clock.AddHours(-9),
        //      CompletionTime: null,
        //      CompletionType: null
        //  );

        //    s_dal!.Assignment.Create(a); //stage 2
        //}

    }


    /// <summary>
    /// Initializes and configures the DAL (Data Access Layer) objects, 
    /// resets configuration and clears existing data, then populates lists for Volunteers, Calls, and Assignments.
    /// Throws exceptions if any DAL object is null.
    /// </summary>
    /// <param name="dalVolunteer">The data access object for Volunteers.</param>
    /// <param name="dalCall">The data access object for Calls.</param>
    /// <param name="dalAssignment">The data access object for Assignments.</param>
    /// <param name="dalConfig">The data access object for Configuration.</param>
    //public static void Do(IDal dal) //stage 2
    public  static void Do() //stage 4
    {
        // Ensure all DAL objects are initialized; throw exceptions for null references.
        // s_dal = dal ?? throw new NullReferenceException("DAL object can not be null!"); stage 2

        s_dal = DalApi.Factory.Get; //stage 4


        // Reset configuration values and clear all data in Volunteers, Calls, and Assignments lists.
        Console.WriteLine("Reset Configuration values and List values...");
        s_dal.ResetDB();//stage 2


         createVolunteer();
         createCall();
         createAssignment();

    }

  
}
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








