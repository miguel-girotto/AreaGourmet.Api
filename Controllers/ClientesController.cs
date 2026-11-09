using AreaGourmet.Data;
using AreaGourmet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Controllers;
[Authorize]
public class ClientesController(AppDbContext db) : Controller {
    public async Task<IActionResult> Index(string? busca) {
        ViewBag.Busca=busca;
        var q=db.Clientes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca)) q=q.Where(x=>x.Nome.Contains(busca)||x.Telefone.Contains(busca));
        return View(await q.OrderBy(x=>x.Nome).Take(200).ToListAsync());
    }
    public async Task<IActionResult> Editar(int? id) {
        if (id is null) return View(new Cliente());
        var item=await db.Clientes.AsNoTracking().Include(x=>x.Enderecos).FirstOrDefaultAsync(x=>x.Id==id);
        return item is null ? NotFound() : View(item);
    }
    [HttpPost] public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Telefone,Ativo")] Cliente form) {
        if (id!=form.Id) return BadRequest();
        if (!User.IsInRole("Administrador")) form.Ativo=true;
        if (await db.Clientes.AnyAsync(x=>x.Telefone==form.Telefone && x.Id!=id)) ModelState.AddModelError("Telefone","Telefone já cadastrado.");
        if (!ModelState.IsValid) { form.Enderecos=await db.Enderecos.Where(x=>x.ClienteId==id).ToListAsync(); return View(form); }
        if (id==0) db.Clientes.Add(form);
        else {
            var atual=await db.Clientes.FindAsync(id); if(atual is null)return NotFound();
            if (!User.IsInRole("Administrador") && !atual.Ativo) return Forbid();
            atual.Nome=form.Nome; atual.Telefone=form.Telefone; atual.Ativo=form.Ativo;
        }
        try { await db.SaveChangesAsync(); } catch(DbUpdateException) { ModelState.AddModelError("","Telefone já cadastrado ou alteração concorrente."); return View(form); }
        return RedirectToAction(nameof(Editar),new{id=form.Id});
    }
    public async Task<IActionResult> Endereco(int clienteId, int? id) {
        var cliente=await db.Clientes.FindAsync(clienteId); if(cliente is null || !cliente.Ativo)return NotFound();
        if(id is null) return View(new Endereco {ClienteId=clienteId});
        var e=await db.Enderecos.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id&&x.ClienteId==clienteId);
        return e is null?NotFound():View(e);
    }
    [HttpPost] public async Task<IActionResult> Endereco([Bind("Id,ClienteId,Logradouro,Numero,Bairro,Cidade,CEP,Ativo")] Endereco form) {
        ModelState.Remove("Cliente");
        if (!await db.Clientes.AnyAsync(x=>x.Id==form.ClienteId&&x.Ativo)) return NotFound();
        if(!User.IsInRole("Administrador"))form.Ativo=true;
        if(!ModelState.IsValid)return View(form);
        if(form.Id==0)db.Enderecos.Add(form);
        else {
            var e=await db.Enderecos.FirstOrDefaultAsync(x=>x.Id==form.Id&&x.ClienteId==form.ClienteId);if(e is null)return NotFound();
            e.Logradouro=form.Logradouro;e.Numero=form.Numero;e.Bairro=form.Bairro;e.Cidade=form.Cidade;e.CEP=form.CEP;e.Ativo=form.Ativo;
        }
        await db.SaveChangesAsync();return RedirectToAction(nameof(Editar),new{id=form.ClienteId});
    }
    [HttpPost, Authorize(Roles="Administrador")] public async Task<IActionResult> Excluir(int id) {
        var e=await db.Clientes.FindAsync(id);if(e is null)return NotFound();
        e.Ativo=false;await db.SaveChangesAsync();TempData["Mensagem"]="Cliente inativado; histórico preservado.";return RedirectToAction(nameof(Index));
    }
}
