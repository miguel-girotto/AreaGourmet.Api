using AreaGourmet.Api.Data;
using AreaGourmet.Api.Services;
using AreaGourmet.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Api.Controllers;
public class VendasController(AppDbContext db, Operacoes op) : BaseController
{
    [HttpGet] public async Task<IActionResult> Index()
    {
        ViewBag.Produtos = await db.Produtos.AsNoTracking().Include(p => p.Categoria).Include(p => p.EstoqueBebida).Where(p => p.Ativo && p.DisponivelVenda && p.Categoria.Ativo).OrderBy(p => p.CategoriaId).ThenBy(p => p.Nome).ToListAsync();
        return View(new PedidoForm());
    }
    [HttpGet("api/clientes")] public async Task<IActionResult> Clientes(string? telefone)
    {
        var digits = new string((telefone ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length < 3) return Json(Array.Empty<object>());
        return Json(await db.Clientes.AsNoTracking().Where(c => c.Ativo && c.Telefone.Contains(digits)).OrderBy(c => c.Nome).Take(10)
            .Select(c => new { c.Id, c.Nome, c.Telefone, Enderecos = c.Enderecos.Select(e => new { e.Id, Texto = e.Logradouro + ", " + e.Numero + " — " + e.Bairro }) }).ToListAsync());
    }
    [HttpPost("api/pedidos")] public async Task<IActionResult> Criar([FromBody] PedidoForm form)
    {
        if (!ModelState.IsValid) return BadRequest(new { erro = "Confira modalidade, pagamento, taxa e itens." });
        try { var id = await op.CriarPedido(form, UsuarioId); return Ok(new { id, url = Url.Action("Detalhes", "Pedidos", new { id }) }); }
        catch (RegraException ex) { return BadRequest(new { erro = ex.Message }); }
        catch (DbUpdateException) { return Conflict(new { erro = "Não foi possível registrar o pedido. Atualize a página e confira o histórico antes de reenviar." }); }
    }
    [HttpGet] public async Task<IActionResult> Cardapio(string? q, int? categoriaId)
    {
        ViewBag.Q = q; ViewBag.CategoriaId = categoriaId;
        ViewBag.Categorias = await db.Categorias.Where(c => c.Ativo).OrderBy(c => c.Nome).ToListAsync();
        var query = db.Produtos.Include(p => p.Categoria).Include(p => p.EstoqueBebida).Where(p => p.Ativo && p.DisponivelVenda && p.Categoria.Ativo);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Nome.Contains(q));
        if (categoriaId.HasValue) query = query.Where(p => p.CategoriaId == categoriaId);
        return View(await query.OrderBy(p => p.Nome).ToListAsync());
    }
}
