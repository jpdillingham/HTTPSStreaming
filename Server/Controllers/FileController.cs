using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{filename}")]
        public IActionResult Get(string filename)
        {
            Console.WriteLine($"Fetching {filename}");
            var stream = new FileStream(Path.Combine(@"C:\slsk-downloads", filename), FileMode.Open, FileAccess.Read);
            return File(stream, "application/octet-stream");
        }
    }
}