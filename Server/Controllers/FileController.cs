using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {   
        public FileController(IHubContext<LinkHub> hub)
        {
            Hub = hub;
        }

        private IHubContext<LinkHub> Hub { get; }
        private static ConcurrentDictionary<string, TaskCompletionSource<Stream>> Streams { get; } = new();

        [HttpGet("{filename}")]
        public async Task<IActionResult> Get(string filename)
        {
            Console.WriteLine($"Requesting {filename} from link");
            await Hub.SendRequest(filename);

            var tcs = new TaskCompletionSource<Stream>();

            if (Streams.TryAdd(filename, tcs))
            {
                try
                {
                    Console.WriteLine($"Added {filename} record. Waiting on file from link...");
                    var task = await Task.WhenAny(tcs.Task, Task.Delay(5000));

                    if (task == tcs.Task)
                    {
                        Console.WriteLine("Got the stream! Trying to write it back out...");
                        var stream = await tcs.Task;
                        return File(stream, "application/octet-stream");
                    }
                    else
                    {
                        throw new TimeoutException("link didn't respond in a timely manner");
                    }
                }
                finally
                {
                    Streams.TryRemove(filename, out _);
                }
            }
            else
            {
                throw new Exception("already waiting for this file!");
            }
        }

        [HttpPost("{filename}")]
        public IActionResult Post(string filename)
        {
            Console.WriteLine($"File POSTed: {filename}");

            var stream = Request.Form.Files.First().OpenReadStream();

            if (Streams.TryGetValue(filename, out var tcs))
            {
                Console.WriteLine("Setting result with stream...");
                tcs.TrySetResult(stream);
            }
            else
            {
                return NotFound();
            }
            
            return Ok();
        }
    }
}