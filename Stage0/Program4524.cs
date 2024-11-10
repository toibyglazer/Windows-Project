using System;

namespace Stage0;    
partial class Program
{
    static void Main(string[] args)
    {
        Welcome4524();
        Wellcome2763();
        Console.ReadKey();
    }
    static partial void Wellcome2763();
    private static void Welcome4524()
    {
        Console.WriteLine("Enter your name: ");
        string userName = Console.ReadLine();
        Console.WriteLine("{0}, welcome to my first console application", userName);
    }
}