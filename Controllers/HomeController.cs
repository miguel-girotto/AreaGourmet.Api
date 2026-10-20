using Microsoft.AspNetCore.Mvc;

namespace AreaGourmet.Api.Controllers;

[Route("")]
[Route("home")]
public class HomeController : Controller
{
    [HttpGet("")]
    [HttpGet("index")]
    public IActionResult Index()
    {
        return View("~/Views/Home/Index.cshtml");
    }
}
