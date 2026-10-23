using AreaGourmet.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Api.Controllers;
public class HomeController(AppDbContext db) : Controller
{
    [Authorize(Policy = "Admin")] public async Task<IActionResult> Index()
    {
        ViewBag.Vendas = await db.Pedidos.Where(p => p.Status == "FINALIZADO").SumAsync(p => (decimal?)p.ValorTotal) ?? 0;
        ViewBag.Pedidos = await db.Pedidos.CountAsync();
        ViewBag.Clientes = await db.Clientes.CountAsync(c => c.Ativo);
        ViewBag.EstoqueBaixo = await db.EstoqueBebidas.CountAsync(e => e.QuantidadeAtual <= e.QuantidadeMinima);
        return View();
    }
    [AllowAnonymous, IgnoreAntiforgeryToken] public IActionResult Erro() { Response.StatusCode = 500; return View(); }
    [AllowAnonymous, Route("Home/Status/{code}"), IgnoreAntiforgeryToken] public IActionResult StatusCodePage(int code) { Response.StatusCode = code; ViewBag.Code = code; return View("Status"); }
}
