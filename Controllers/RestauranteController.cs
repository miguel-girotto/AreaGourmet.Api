using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AreaGourmet.Controllers;

[Authorize]
public class RestauranteController : Controller
{
    public IActionResult Index() => View();
}
