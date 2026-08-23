using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FedestrapClient.WebServer.Controllers.IDE;

[ApiController]
[Route("IDE/Landing.aspx")]
[Route("My/Places.aspx")]
public class LandingController : ControllerBase
{
	private readonly ILogger<LandingController> _logger;

	public LandingController(ILogger<LandingController> logger)
	{
		_logger = logger;
	}

	[HttpGet]
	public IActionResult Get()
	{
		return Content("<head><title>Fedestrap</title></head><html><body><marquee>Fedestrap</marquee></body></html>", "text/html");
	}
}
