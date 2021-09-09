using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WindowsHost.Payloads;

namespace WindowsHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        private readonly ILogger<HelloController> _logger;

        public HelloController(ILogger<HelloController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post(SimpleMessage message)
        {
            _logger.LogInformation($"Received message '{message.Message}'");
            return Ok();
        }
    }
}
