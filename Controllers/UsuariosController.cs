using System.Security.Claims;
using AreaGourmet.Data;
using AreaGourmet.Models;
using AreaGourmet.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Controllers;
[Authorize(Roles="Administrador")]
public class UsuariosController(AppDbContext db,IPasswordHasher<Usuario> hasher):Controller {
    public async Task<IActionResult> Index(string? busca){ViewBag.Busca=busca;return View(await db.Usuarios.AsNoTracking().Where(x=>busca==null||x.Nome.Contains(busca)||x.Email.Contains(busca)).OrderBy(x=>x.Nome).Take(200).ToListAsync());}
    public async Task<IActionResult> Editar(int? id){
        if(id is null)return View(new UsuarioForm());var u=await db.Usuarios.FindAsync(id);return u is null?NotFound():View(new UsuarioForm{Id=u.Id,Nome=u.Nome,Email=u.Email,Perfil=u.Perfil,Ativo=u.Ativo});
    }
    [HttpPost] public async Task<IActionResult> Editar(int id,UsuarioForm form){
        if(id!=form.Id)return BadRequest();
        form.Email=form.Email.Trim().ToLowerInvariant();
        if(id==0&&string.IsNullOrWhiteSpace(form.Senha))ModelState.AddModelError("Senha","Informe uma senha de pelo menos 10 caracteres.");
        if(await db.Usuarios.AnyAsync(x=>x.Email==form.Email&&x.Id!=id))ModelState.AddModelError("Email","E-mail já cadastrado.");
        var self=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if(id==self&&(!form.Ativo||form.Perfil!="Administrador"))ModelState.AddModelError("","Você não pode remover seu próprio acesso de administrador.");
        if(!ModelState.IsValid)return View(form);
        var u=id==0?new Usuario():await db.Usuarios.FindAsync(id);if(u is null)return NotFound();
        u.Nome=form.Nome;u.Email=form.Email;u.Perfil=form.Perfil;u.Ativo=form.Ativo;
        if(!string.IsNullOrWhiteSpace(form.Senha)){u.SenhaHash=hasher.HashPassword(u,form.Senha);u.SecurityStamp=Guid.NewGuid().ToString();}
        if(id==0)db.Usuarios.Add(u);
        try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","E-mail duplicado ou alteração concorrente.");return View(form);}
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Excluir(int id){
        if(id==int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)){TempData["Mensagem"]="Você não pode inativar seu próprio usuário.";return RedirectToAction(nameof(Index));}
        var u=await db.Usuarios.FindAsync(id);if(u is null)return NotFound();u.Ativo=false;u.SecurityStamp=Guid.NewGuid().ToString();await db.SaveChangesAsync();return RedirectToAction(nameof(Index));
    }
}
