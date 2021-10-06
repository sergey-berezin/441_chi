using System;
using Library;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string imageFolder)
        {
           
                Console.WriteLine("Please type path to the image folder");
                imageFolder = Console.ReadLine();
                await Program.Main(imageFolder);
            

        }
    }
}
