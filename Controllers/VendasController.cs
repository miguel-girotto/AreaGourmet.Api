using System.Security.Claims;
using AreaGourmet.Data;
using AreaGourmet.Services;
using AreaGourmet.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Controllers;
[Authorize]
public class VendasController(AppDbContext db, PedidoService service) : Controller {
    public async Task<IActionResult> Index() => View(await db.Produtos.AsNoTracking().Where(x=>x.Ativo).OrderBy(x=>x.Id).ToListAsync());
    [HttpGet] public async Task<IActionResult> Clientes(string? telefone) {
        var digits = new string((telefone??"").Where(char.IsDigit).ToArray());
        if (digits.Length<3) return Json(Array.Empty<object>());
        return Json(await db.Clientes.AsNoTracking().Where(x=>x.Ativo && x.Telefone.Contains(digits)).Take(10)
            .Select(x=>new {x.Id,x.Nome,x.Telefone,Enderecos=x.Enderecos.Where(e=>e.Ativo).Select(e=>new {e.Id,e.Logradouro,e.Numero,e.Bairro,e.Cidade})}).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Finalizar([FromBody] PedidoForm form) {
        if (!ModelState.IsValid) return BadRequest(new {erro="Confira os campos e as quantidades do pedido."});
        try {
            var pedido = await service.Finalizar(form, int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
            return Json(new {pedido.Id,pedido.ValorTotal,url=Url.Action(nameof(Recibo),new {id=pedido.Id})});
        } catch (InvalidOperationException ex) { return BadRequest(new {erro=ex.Message}); }
        catch (DbUpdateException) { return Conflict(new {erro="Os dados foram alterados por outra operação. Confira o estoque e tente novamente."}); }
    }
    public async Task<IActionResult> Recibo(int id) {
        var pedido = await db.Pedidos.AsNoTracking().Include(x=>x.Itens).Include(x=>x.Cliente).Include(x=>x.Usuario).FirstOrDefaultAsync(x=>x.Id==id);
        if (pedido is null) return NotFound();
        if (!User.IsInRole("Administrador") && pedido.UsuarioId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)) return Forbid();
        return View(pedido);
    }
}
