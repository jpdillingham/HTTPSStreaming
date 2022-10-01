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
        private static ConcurrentDictionary<string, TaskCompletionSource<string>> WaitingFiles { get; } = new();

        [HttpGet("{filename}")]
        public async Task<IActionResult> Get(string filename)
        {
            Console.WriteLine($"Requesting {filename} from link");
            await Hub.SendRequest(filename);

            var tcs = new TaskCompletionSource<string>();

            if (WaitingFiles.TryAdd(filename, tcs))
            {
                try
                {
                    Console.WriteLine($"Added {filename} record. Waiting on file from link...");
                    var task = await Task.WhenAny(tcs.Task, Task.Delay(5000));

                    if (task == tcs.Task)
                    {
                        Console.WriteLine("Got the stream! Trying to write it back out...");
                        var tempFilename = await tcs.Task;
                        var stream = new FileStream(tempFilename, FileMode.Open, FileAccess.Read);
                        return File(stream, "application/octet-stream", fileDownloadName: filename);
                    }
                    else
                    {
                        throw new TimeoutException("link didn't respond in a timely manner");
                    }
                }
                finally
                {
                    WaitingFiles.TryRemove(filename, out _);
                }
            }
            else
            {
                throw new Exception("already waiting for this file!");
            }
        }

        [HttpPost("{filename}")]
        public async Task<IActionResult> Post(string filename)
        {
            Console.WriteLine($"File POSTed: {filename}");

            if (WaitingFiles.TryGetValue(filename, out var tcs))
            {
                var stream = Request.Form.Files.First().OpenReadStream();

                var temp = Path.GetTempFileName();
                var destinationStream = new FileStream(temp, FileMode.Create);
                await stream.CopyToAsync(destinationStream);
                await destinationStream.FlushAsync();
                stream.Dispose();
                destinationStream.Dispose();

                Console.WriteLine("Setting result with stream...");
                tcs.TrySetResult(temp);
            }
            else
            {
                return NotFound();
            }
            
            return Ok();
        }
    }
}