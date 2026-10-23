using AreaGourmet.Api.Data;
using AreaGourmet.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AreaGourmet.Api.Controllers;
[Authorize(Policy = "Admin")]
public class DemoController(AppDbContext db, Operacoes op) : BaseController
{
    [HttpPost] public async Task<IActionResult> Gerar()
    {
        if (!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()) return Forbid();
        await new DadosDemo(db, op).Gerar(UsuarioId);
        TempData["Sucesso"] = "Dados de demonstração disponíveis. Nenhum registro anterior foi apagado.";
        return RedirectToAction("Index", "Home");
    }
}
