using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using AreaGourmet.Api.Services;
using AreaGourmet.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Api.Controllers;
[Authorize(Policy = "Admin")]
public class EstoqueController(AppDbContext db, Operacoes op) : BaseController
{
    async Task Preparar()
    {
        ViewBag.Produtos = await db.Produtos.OrderBy(p => p.Nome).ToListAsync();
        ViewBag.Estoques = await db.EstoqueBebidas.Include(e => e.Produto).OrderBy(e => e.Produto.Nome).ToListAsync();
    }
    public async Task<IActionResult> Index(string? q, bool baixo = false)
    {
        ViewBag.Q = q; ViewBag.Baixo = baixo;
        var query = db.EstoqueBebidas.Include(e => e.Produto).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(e => e.Produto.Nome.Contains(q));
        if (baixo) query = query.Where(e => e.QuantidadeAtual <= e.QuantidadeMinima);
        return View(await query.OrderBy(e => e.Produto.Nome).ToListAsync());
    }
    [HttpGet] public async Task<IActionResult> Movimento(int produtoId = 0) { await Preparar(); return View(new MovimentoForm { ProdutoId = produtoId }); }
    [HttpPost] public async Task<IActionResult> Movimento(MovimentoForm form) => await Salvar(async () => await op.Movimentar(form, UsuarioId), () => RedirectToAction(nameof(Index)), async () => { await Preparar(); return View(form); });
    public async Task<IActionResult> Historico(string? tipo, int? produtoId, DateTime? inicio, DateTime? fim)
    {
        await Preparar(); ViewBag.Tipo = tipo; ViewBag.ProdutoId = produtoId; ViewBag.Inicio = inicio; ViewBag.Fim = fim;
        var query = db.Movimentacoes.Include(m => m.EstoqueBebida).ThenInclude(e => e.Produto).Include(m => m.Usuario).AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(tipo)) query = query.Where(m => m.Tipo == tipo);
        if (produtoId.HasValue) query = query.Where(m => m.EstoqueBebida.ProdutoId == produtoId);
        if (inicio.HasValue) query = query.Where(m => m.DataHora >= inicio.Value.Date);
        if (fim.HasValue) { var end = fim.Value.Date.AddDays(1); query = query.Where(m => m.DataHora < end); }
        return View(await query.OrderByDescending(m => m.Id).ToListAsync());
    }
    [HttpGet] public async Task<IActionResult> Consumos()
    {
        await Preparar();
        ViewBag.Consumos = await db.Consumos.Include(c => c.Produto).Include(c => c.EstoqueBebida).ThenInclude(e => e.Produto).ToListAsync();
        return View(new ConsumoForm());
    }
    [HttpPost] public async Task<IActionResult> Consumos(ConsumoForm form) => await Salvar(async () => await op.Transacao(async () =>
    {
        Operacoes.Exigir(await db.Produtos.AnyAsync(p => p.Id == form.ProdutoId), "Produto não encontrado.");
        var e = await db.EstoqueBebidas.FindAsync(form.EstoqueBebidaId);
        Operacoes.Exigir(e is not null && e.ProdutoId != form.ProdutoId, "Consumo direto já baixa uma unidade. Escolha outro insumo para a associação adicional.");
        var c = await db.Consumos.SingleOrDefaultAsync(c => c.ProdutoId == form.ProdutoId && c.EstoqueBebidaId == form.EstoqueBebidaId);
        if (c is null) db.Consumos.Add(new ConsumoEstoque { ProdutoId = form.ProdutoId, EstoqueBebidaId = form.EstoqueBebidaId, Quantidade = form.Quantidade }); else c.Quantidade = form.Quantidade;
        op.Auditar(UsuarioId, $"Consumo do produto {form.ProdutoId} configurado"); return 0;
    }), () => RedirectToAction(nameof(Consumos)), async () => { await Preparar(); ViewBag.Consumos = await db.Consumos.Include(c => c.Produto).Include(c => c.EstoqueBebida).ThenInclude(e => e.Produto).ToListAsync(); return View(form); });
    [HttpPost] public async Task<IActionResult> RemoverConsumo(int id)
    {
        await op.Transacao(async () => { var c = await db.Consumos.FindAsync(id); if (c is not null) db.Consumos.Remove(c); op.Auditar(UsuarioId, $"Associação de consumo {id} removida; histórico preservado"); return 0; });
        return RedirectToAction(nameof(Consumos));
    }
}
