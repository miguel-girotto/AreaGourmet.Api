using AreaGourmet.Data;
using AreaGourmet.Models;
using AreaGourmet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Controllers;
[Authorize]
public class ProdutosController(AppDbContext db) : Controller {
    public async Task<IActionResult> Index(string? busca,string? categoria) {
        ViewBag.Busca=busca;ViewBag.Categoria=categoria;
        var q=db.Produtos.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(busca))q=q.Where(x=>x.Nome.Contains(busca));
        if(!string.IsNullOrWhiteSpace(categoria))q=q.Where(x=>x.Categoria==categoria);
        return View(await q.OrderBy(x=>x.Nome).Take(200).ToListAsync());
    }
    [Authorize(Roles="Administrador")] public async Task<IActionResult> Editar(int? id) {
        if(id is null)return View(new Produto());var p=await db.Produtos.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id);if(p is not null)p.Imagem=ProductImages.Normalizar(p.Imagem);return p is null?NotFound():View(p);
    }
    [HttpPost, Authorize(Roles="Administrador")] public async Task<IActionResult> Editar(int id,[Bind("Id,Nome,Categoria,PrecoUnitario,EstoqueInsumo,ControlaEstoque,Ativo,Versao,Imagem")] Produto form) {
        if(id!=form.Id)return BadRequest();
        if(!ProductImages.Valida(form.Imagem))ModelState.AddModelError("Imagem","Imagem inválida.");
        if(!ModelState.IsValid)return View(form);
        form.Imagem=ProductImages.Normalizar(form.Imagem);
        if(id==0) {form.Versao=1;db.Produtos.Add(form);}
        else {
            var p=await db.Produtos.FindAsync(id);if(p is null)return NotFound();
            if(p.Versao!=form.Versao){ModelState.AddModelError("","Produto alterado por outra operação. Reabra a edição.");return View(form);}
            p.Nome=form.Nome;p.Categoria=form.Categoria;p.PrecoUnitario=form.PrecoUnitario;p.EstoqueInsumo=form.EstoqueInsumo;
            p.ControlaEstoque=form.ControlaEstoque;p.Ativo=form.Ativo;p.Imagem=form.Imagem;p.Versao++;
        }
        try{await db.SaveChangesAsync();}catch(DbUpdateConcurrencyException){ModelState.AddModelError("","Alteração concorrente. Reabra a edição.");return View(form);}
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, Authorize(Roles="Administrador")]
    public async Task<IActionResult> ImportarDemonstracao() {
        try {
            var count=await DemoCatalog.AdicionarAusentes(db);
            TempData["Mensagem"]=count==0 ? "Os exemplos já estão cadastrados. Nenhum produto foi alterado." : $"{count} produtos demonstrativos adicionados. Preços, estoque e pedidos existentes foram preservados.";
        } catch (DbUpdateException) { TempData["Mensagem"]="Não foi possível importar agora. Tente novamente."; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, Authorize(Roles="Administrador")] public async Task<IActionResult> Excluir(int id) {
        var p=await db.Produtos.FindAsync(id);if(p is null)return NotFound();p.Ativo=false;p.Versao++;
        try{await db.SaveChangesAsync();}catch(DbUpdateConcurrencyException){TempData["Mensagem"]="Produto alterado. Tente novamente.";return RedirectToAction(nameof(Index));}
        TempData["Mensagem"]="Produto inativado; vendas anteriores preservadas.";return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles="Administrador")] public async Task<IActionResult> Estoque(string? busca) {
        ViewBag.Busca=busca;return View(await db.Produtos.AsNoTracking().Where(x=>x.Ativo&&x.ControlaEstoque&&(busca==null||x.Nome.Contains(busca))).OrderBy(x=>x.EstoqueInsumo).Take(200).ToListAsync());
    }
}
