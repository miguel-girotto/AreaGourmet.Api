using AreaGourmet.Api.Data;
using AreaGourmet.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Api.Controllers;
[Authorize(Policy = "Admin")]
public class RelatoriosController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(DateTime? inicio, DateTime? fim, int? produtoId, int? categoriaId, string? modalidade, string? pagamento)
    {
        inicio ??= DateTime.UtcNow.Date.AddDays(-30); fim ??= DateTime.UtcNow.Date;
        ViewBag.Inicio = inicio; ViewBag.Fim = fim; ViewBag.ProdutoId = produtoId; ViewBag.CategoriaId = categoriaId; ViewBag.Modalidade = modalidade; ViewBag.Pagamento = pagamento;
        ViewBag.Produtos = await db.Produtos.OrderBy(p => p.Nome).ToListAsync();
        ViewBag.Categorias = await db.Categorias.OrderBy(c => c.Nome).ToListAsync();
        var end = fim.Value.Date.AddDays(1);
        var query = db.Pedidos.AsNoTracking().Where(p => p.Status == "FINALIZADO" && p.DataHora >= inicio.Value.Date && p.DataHora < end);
        if (!string.IsNullOrEmpty(modalidade)) query = query.Where(p => p.TipoAtendimento == modalidade);
        if (!string.IsNullOrEmpty(pagamento)) query = query.Where(p => p.FormaPagamento == pagamento);
        if (produtoId.HasValue) query = query.Where(p => p.Itens.Any(i => i.ProdutoId == produtoId));
        if (categoriaId.HasValue) query = query.Where(p => p.Itens.Any(i => i.Produto.CategoriaId == categoriaId));
        // LINQ condicional: pivot executada no MySQL sobre pedidos efetivamente finalizados.
        var pivotDados = await query.GroupBy(p => p.DataHora.Date).Select(g => new {
            Dia = g.Key,
            Mesa = g.Sum(p => p.TipoAtendimento == "Mesa" ? p.ValorTotal : 0),
            Retirada = g.Sum(p => p.TipoAtendimento == "Retirada" ? p.ValorTotal : 0),
            Delivery = g.Sum(p => p.TipoAtendimento == "Delivery" ? p.ValorTotal : 0)
        }).OrderBy(p => p.Dia).ToListAsync();
        var pivot = pivotDados.Select(p => new PivotLinha(p.Dia, p.Mesa, p.Retirada, p.Delivery)).ToList();
        var itens = query.SelectMany(p => p.Itens);
        if (produtoId.HasValue) itens = itens.Where(i => i.ProdutoId == produtoId);
        if (categoriaId.HasValue) itens = itens.Where(i => i.Produto.CategoriaId == categoriaId);
        var linhas = await itens.Select(i => new LinhaRelatorio(i.Pedido.DataHora.Date, i.Pedido.TipoAtendimento, i.Pedido.FormaPagamento,
            i.NomeProdutoHistorico ?? i.Produto.Nome, i.Produto.Categoria.Nome, i.Quantidade, i.SubtotalItem)).ToListAsync();
        var agrupamento = await query.GroupBy(p => p.FormaPagamento).Select(g => new { Nome = g.Key, Quantidade = g.Count(), Total = g.Sum(p => p.ValorTotal) }).ToListAsync();
        ViewBag.Pagamentos = agrupamento.Select(g => new GrupoRelatorio(g.Nome, g.Quantidade, g.Total)).ToList();
        return View(new RelatorioVm { Pedidos = await query.Include(p => p.Usuario).Include(p => p.Cliente).OrderByDescending(p => p.Id).ToListAsync(), Itens = linhas, Pivot = pivot });
    }
    public async Task<IActionResult> Auditoria() => View(await db.Auditorias.Include(a => a.Usuario).AsNoTracking().OrderByDescending(a => a.Id).Take(500).ToListAsync());
}
public record GrupoRelatorio(string Nome, int Quantidade, decimal Total);
