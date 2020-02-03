using System;
using System.Drawing;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MagicHome.Example
{
    public class Program
    {
        public static JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true, Converters = { new JsonStringEnumConverter() }
        };

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Enter IP address of light:");
            var address = Console.ReadLine();

            var light = new Light()
            {
                AutoRefreshEnabled = true
            };

            await light.ConnectAsync(IPAddress.Parse(address));
            await light.TurnOnAsync();

            Console.WriteLine(JsonSerializer.Serialize(light, DefaultJsonOptions));

            Console.WriteLine("Calibrating...");
            
            await light.SetColorAsync(Color.Red);
            await Task.Delay(1000);
            await light.SetColorAsync(Color.Green);
            await Task.Delay(1000);
            await light.SetColorAsync(Color.Blue);

            Console.WriteLine("Restoring state...");

            await Task.Delay(1000);
            await light.RestoreAsync();

            Console.ReadLine();
        }
    }
}
