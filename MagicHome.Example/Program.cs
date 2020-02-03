using System;
using System.Net;
using System.Threading.Tasks;

namespace MagicHome.Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Enter IP address of light:");
            var address = Console.ReadLine();

            var light = new Light();
            await light.ConnectAsync(IPAddress.Parse(address));

            Console.WriteLine(light.Color.ToString());
            Console.ReadLine();
        }
    }
}
