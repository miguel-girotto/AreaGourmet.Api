using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AreaGourmet.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Controllers;
public abstract class BaseController : Controller
{
    protected int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected bool Admin => User.IsInRole("Administrador");
    protected async Task<IActionResult> Salvar(Func<Task> action, Func<IActionResult> success, Func<Task<IActionResult>> invalid)
    {
        if (ModelState.IsValid)
        {
            try { await action(); TempData["Sucesso"] = "Operação realizada com sucesso."; return success(); }
            catch (RegraException ex) { ModelState.AddModelError("", ex.Message); }
            catch (DbUpdateException) { ModelState.AddModelError("", "Não foi possível salvar. Confira os dados e relacionamentos e tente novamente."); }
        }
        return await invalid();
    }
}
