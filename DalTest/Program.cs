using Dal;
using DalApi;


namespace DalTest
{
    internal class Program
    {
        private static IVolunteer? s_dalVolunteer = new VolunteerImplementation();
        private static ICall? s_dalCall = new CallImplementation();
        private static IAssignment? s_dalAssignment = new AssignmentImplementation();
        private static IConfig? s_dalConfig = new ConfigImplementation();
       
       
         static void Main(string[] args)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
            }

        }

    }
   

}


