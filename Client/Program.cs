using Microsoft.AspNetCore.SignalR.Client;

namespace Client
{
    internal class Program
    {
        private static HubConnection Connection { get; set; }
        private static readonly HttpClient Client = new HttpClient() { BaseAddress = new("https://localhost:7250") };

        static async Task Main(string[] args)
        {
            Connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7250/hubs/link")
                .WithAutomaticReconnect()
                .Build();

            // when the server requests a file, open a stream to that file and POST it to /file/{filename}
            // as multipart form data (typical file upload content; there may be a more straightforward way to do this)
            Connection.On<string>("REQUEST", async (filename) =>
            {
                try 
                { 
                    Console.WriteLine($"Server has requested {filename}. Opening...");

                    using var stream = new FileStream(Path.Combine(@"C:\slsk-downloads", filename), FileMode.Open, FileAccess.Read);
                    using var request = new HttpRequestMessage(HttpMethod.Post, $"file/{filename}");
                    using var content = new MultipartFormDataContent
                    {
                        { new StreamContent(stream), "file", filename }
                    };

                    request.Content = content;

                    Console.WriteLine($"Sending {filename}...");
                    var response = await Client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Failed");
                        Console.WriteLine(response.StatusCode);
                    }
                    else
                    {
                        Console.WriteLine("Sent!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });

            await Connection.StartAsync();

            Console.WriteLine("Connected to server.  Press any key to exit.");
            Console.ReadKey();
        }
    }
}