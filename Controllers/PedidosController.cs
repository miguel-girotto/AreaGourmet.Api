using AreaGourmet.Api.Data;
using AreaGourmet.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Api.Controllers;
public class PedidosController(AppDbContext db, Operacoes op) : BaseController
{
    public async Task<IActionResult> Index(DateTime? inicio, DateTime? fim, string? status, string? modalidade, string? q)
    {
        ViewBag.Inicio = inicio; ViewBag.Fim = fim; ViewBag.Status = status; ViewBag.Modalidade = modalidade; ViewBag.Q = q;
        var query = db.Pedidos.AsNoTracking().Include(p => p.Cliente).Include(p => p.Usuario).AsQueryable();
        if (inicio.HasValue) query = query.Where(p => p.DataHora >= inicio.Value.Date);
        if (fim.HasValue) { var end = fim.Value.Date.AddDays(1); query = query.Where(p => p.DataHora < end); }
        if (!string.IsNullOrEmpty(status)) query = query.Where(p => p.Status == status);
        if (!string.IsNullOrEmpty(modalidade)) query = query.Where(p => p.TipoAtendimento == modalidade);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => (p.ClienteHistorico != null && p.ClienteHistorico.Contains(q)) || (p.Cliente != null && p.Cliente.Nome.Contains(q)));
        return View(await query.OrderByDescending(p => p.Id).ToListAsync());
    }
    public async Task<IActionResult> Detalhes(int id)
    {
        var p = await db.Pedidos.AsNoTracking().Include(p => p.Usuario).Include(p => p.Cliente).Include(p => p.Endereco).Include(p => p.Itens).ThenInclude(i => i.Produto).SingleOrDefaultAsync(p => p.Id == id);
        return p is null ? NotFound() : View(p);
    }
    [HttpPost] public async Task<IActionResult> Status(int id, string status)
    {
        try { await op.AlterarStatus(id, status, UsuarioId, Admin); TempData["Sucesso"] = "Status atualizado."; }
        catch (RegraException ex) { TempData["Erro"] = ex.Message; }
        return RedirectToAction(nameof(Detalhes), new { id });
    }
}
