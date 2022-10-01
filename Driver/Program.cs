using System.Net;

namespace Driver
{
    internal class Program
    {
        private static readonly HttpClient Client = new HttpClient() { BaseAddress = new("https://localhost:7250"),  };

        static async Task Main(string[] args)
        {
            await Task.WhenAll(Enumerable.Range(1, 20).Select(n => Get(n)));
            Console.WriteLine("Done");
        }

        static async Task Get(int num)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"file/{num}.mp3");
            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"{num} failed! {response.StatusCode}");
            }
            else
            {
                Console.WriteLine($"{num} succeeded!");
            }
        }
    }
}