namespace DalTest;
using DalApi;
using DO;
using Dal;
using Microsoft.VisualBasic;
using System.Data;

public static class Initialization
{
    
    private static IDal? s_dal;
    private static readonly Random s_rand = new();

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
    "Esther Ben-Shimon",
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
    "david.levi@gmail.com",
    "rachel.cohen@gmail.com",
    "moshe.mizrahi@gmail.com",
    "tamar.kaplan@gmail.com",
    "yosef.goldstein@gmail.com",
    "esther.benshimon@gmail.com",
    "avi.rosenberg@gmail.com",
    "hila.sharabi@gmail.com",
    "noam.azulai@gmail.com",
    "yael.peretz@gmail.com",
    "eli.barak@gmail.com",
    "michal.nave@gmail.com",
    "shai.elbaz@gmail.com",
    "dana.regev@gmail.com",
    "nadav.shamir@gmail.com",
    "orly.yitzhak@gmail.com",
    "daniel.vaknin@gmail.com",
    "adi.kadosh@gmail.com"
};
        string[] addresses = new string[]
{
    "123 Herzl Street, Tel Aviv",
    "45 Dizengoff Street, Haifa",
    "89 Ben Yehuda Street, Jerusalem",
    "12 Kaplan Avenue, Rishon Lezion",
    "77 Goldstein Boulevard, Ashdod",
    "34 Shimon Street, Petah Tikva",
    "56 Rosenberg Lane, Netanya",
    "22 Sharabi Road, Be'er Sheva",
    "98 Azulai Street, Holon",
    "15 Peretz Square, Kiryat Gat",
    "11 Barak Alley, Eilat",
    "67 Nave Promenade, Rehovot",
    "39 Elbaz Avenue, Modi'in",
    "28 Regev Street, Herzliya",
    "74 Shamir Circle, Ramat Gan",
    "18 Yitzhak Boulevard, Hadera",
    "52 Vaknin Road, Ashkelon",
    "26 Kadosh Lane, Nahariya"
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
        int firstId = s_rand.Next(200000000, 400000000);
        DistanceType firstDistanceType = (DistanceType)s_rand.Next(Enum.GetValues(typeof(DistanceType)).Length);
        var firstVolunteer = new Volunteer
            (
                Id: firstId,
                FullName: VolunteerNames[0],
                PhoneNumber: phoneNumbers[0],
                Email: VolunteerEmails[0],
                Password: passwords[0],
                CurrentAddress: addresses[0],
                Latitude: latitudes[0],
                Longitude: longitudes[0],
                Role: VolunteerRole.Manager,
                IsActive: true,
                MaxDistance: maxDistances[0],
                DistanceType: firstDistanceType
           );
        s_dal!.Volunteer.Create(firstVolunteer); //stage 2

        // Iterate over the rest of the volunteers and assign them as regular volunteers.
        for (int i = 1; i < VolunteerNames.Length; i++)
        {
            int id = s_rand.Next(200000000, 400000000);
            DistanceType d = (DistanceType)s_rand.Next(Enum.GetValues(typeof(DistanceType)).Length);

            var volunteer = new Volunteer
            (
                Id: id,
                FullName: VolunteerNames[i],
                PhoneNumber: phoneNumbers[i],
                Email: VolunteerEmails[i],
                Password: passwords[i],
                CurrentAddress: addresses[i],
                Latitude: latitudes[i],
                Longitude: longitudes[i],
                Role: VolunteerRole.Volunteer,
                IsActive: true,
                MaxDistance: maxDistances[i],
                DistanceType: d
           );

            s_dal!.Volunteer.Create(volunteer); //stage 2

        }
    }

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
        string[] Addresses = new[]{
    "1 Herzl Street, Tel Aviv",
    "12 Ben Gurion Boulevard, Haifa",
    "5 Jaffa Street, Jerusalem",
    "10 Weizmann Street, Kfar Saba",
    "8 Rabin Avenue, Be'er Sheva",
    "20 Rothschild Boulevard, Tel Aviv",
    "15 Gaza Street, Jerusalem",
    "9 Independence Street, Ashdod",
    "3 Ga'aton Boulevard, Nahariya",
    "17 Jabotinsky Street, Ramat Gan",
    "4 Jerusalem Boulevard, Holon",
    "11 HaNasi Street, Ra'anana",
    "2 Yehudit Boulevard, Petah Tikva",
    "8 Bialik Street, Ramat HaSharon",
    "6 HaShalom Avenue, Giv'atayim",
    "18 Allenby Street, Tel Aviv",
    "13 Eilat Boulevard, Ashkelon",
    "7 Zionism Street, Afula",
    "22 HaHativot Avenue, Kiryat Shmona",
    "25 Herzl Boulevard, Netanya",
    "50 Dizengoff Street, Tel Aviv",
    "30 HaNevi'im Street, Jerusalem",
    "14 Ben Zvi Boulevard, Be'er Sheva",
    "9 HaGefen Street, Karmiel",
    "19 Rabin Boulevard, Modi'in",
    "3 HaYam Road, Haifa",
    "10 Freedom Boulevard, Eilat",
    "5 Galilee Street, Safed",
    "12 Rothschild Road, Rishon LeZion",
    "6 Wolfson Street, Herzliya",
    "28 Jabotinsky Boulevard, Petah Tikva",
    "1 HaNamal Street, Acre",
    "7 Freedom Boulevard, Tiberias",
    "18 Menachem Begin Road, Tel Aviv",
    "4 Sokolov Street, Holon",
    "8 HaNesi'im Street, Nes Ziona",
    "3 Independence Boulevard, Ashdod",
    "12 HaRimon Street, Beit Shemesh",
    "10 Bialik Boulevard, Kfar Saba",
    "9 Baruch Spinoza Street, Kiryat Motzkin",
    "6 Hillel Street, Jerusalem",
    "20 HaShalom Avenue, Giv'atayim",
    "15 HaNarkis Street, Yavne",
    "8 HaPerahim Boulevard, Or Akiva",
    "7 HaBanim Street, Ramat HaSharon",
    "13 Golani Brigade Road, Beit She'an",
    "5 HaTamar Boulevard, Ashkelon",
    "10 HaShalom Street, Ofakim",
    "11 Golda Meir Boulevard, Tel Aviv",
    "3 Palmach Street, Kiryat Shmona",
    "17 Shalom Aleichem Street, Be'er Sheva",
    "11 Menachem Begin Street, Petah Tikva",
    "2 HaLachish Street, Holon",
    "22 HaHalev Street, Tel Aviv",
    "3 HaLev HaShani Street, Herzliya",
    "8 HaMizrach Street, Eilat"
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
        /*Enum.GetValues(typeof(DO.CallType)).Length*/
        DO.CallType[] callTypes = new DO.CallType[Descriptions.Length];
        for (int i = 0; i < Descriptions.Length; i++)
        {
            callTypes[i] = (DO.CallType)s_rand.Next(11);
        }

        DateTime start = new(s_dal.Config.Clock.Year - 2, 1, 1);

        for (int i = 0; i < Descriptions.Length; i++)
        {
            DateTime startTime = start.AddDays(-s_rand.Next(1, 30));
            DateTime? deadline = s_rand.NextDouble() < 0.5 ? (DateTime?)startTime.AddHours(s_rand.Next(1, 72)) : null;

            int callId = 1;
            var call = new Call
               (
                   Id: callId,
                   Type: callTypes[i],
                   FullAddress: Addresses[i],
                   Latitude: latitudes[i],
                   Longitude: longitudes[i],
                   OpeningTime: startTime,
                   Description: Descriptions[i],
                   MaxCompletionTime: deadline
               );
            s_dal!.Call.Create(call); //stage 2

        }
    }
    /// <summary>
    /// Creates and initializes a predefined set of Assignment objects with varying completion types.
    /// Assignments are created for specific volunteer IDs, and the completion type is determined based on their index.
    /// </summary>
    private static void createAssignment()
    {
       
        int[] volunteerId = new int[]
        {
        123456789, 234567890, 345678901, 456789012,
        567890123, 678901234, 789012345, 890123456,
        901234567, 123098456, 234109567, 345210678,
        456321789, 567432890, 678543901, 789654012,
        890765123, 123890456, 234567890, 345678912,
        456789123, 567890234, 678901345, 789012456,
        890123567, 901234678, 123098789, 234109890,
        345210901, 456321012, 567432123, 678543234,
        789654345, 890765456, 123890567, 234567678,
        345678789, 456789890, 567890901, 678901012,
        789012123, 890123234, 901234345, 123098456,
        234109567, 345210678, 456321789, 567432890,
        678543901, 789654012
        };

        // Get the next available assignment ID.
        int assignmentId = s_dal.Config.NextAssignmentId;

        // Create assignments with CompletionType.Treated for the first 15 volunteers.
        for (int i = 0; i < 15; i++)
        {
            Assignment a = new Assignment(
                Id: assignmentId,
                CallId: 1,
                VolunteerId: volunteerId[i],
                EntryTime: s_dal.Config.Clock,
                CompletionTime: null,
                CompletionType: AssignmentCompletionType.Treated
            );
            s_dal!.Assignment.Create(a); //stage 2

        }

        // Create assignments with CompletionType.VolunteerCancelled for the next set of volunteers.
        for (int i = 15; i < 29; i++)
        {
            Assignment a = new Assignment(
                Id: assignmentId,
                CallId: 1,
                VolunteerId: volunteerId[i],
                EntryTime: s_dal.Config.Clock,
                CompletionTime: null,
                CompletionType: AssignmentCompletionType.VolunteerCancelled
            );
            s_dal!.Assignment.Create(a); //stage 2
        }

        // Create assignments with CompletionType.AdminCancelled for the next set of volunteers.
        for (int i = 29; i < 40; i++)
        {
            Assignment a = new Assignment(
                Id: assignmentId,
                CallId: 1,
                VolunteerId: volunteerId[i],
                EntryTime: s_dal.Config.Clock,
                CompletionTime: null,
                CompletionType: AssignmentCompletionType.AdminCancelled
            );
            s_dal!.Assignment.Create(a); //stage 2
        }

        // Create assignments with CompletionType.Expired for the remaining volunteers.
        for (int i = 40; i < 50; i++)
        {
            Assignment a = new Assignment(
                Id: assignmentId,
                CallId: 1,
                VolunteerId: volunteerId[i],
                EntryTime: s_dal.Config.Clock,
                CompletionTime: null,
                CompletionType: AssignmentCompletionType.Expired
            );
            s_dal!.Assignment.Create(a); //stage 2
        }
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
    public static void Do() //stage 4
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



