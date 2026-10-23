using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using AreaGourmet.Api.Services;
using AreaGourmet.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Api.Controllers;
[Authorize(Policy = "Admin")]
public class UsuariosController(AppDbContext db, Operacoes op) : BaseController
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        return View(await db.Usuarios.AsNoTracking().Where(u => q == null || u.Nome.Contains(q) || u.Login.Contains(q)).OrderBy(u => u.Nome)
            .Select(u => new UsuarioForm { Id = u.Id, Nome = u.Nome, Login = u.Login, Perfil = u.Perfil, Ativo = u.Ativo, PodeCadastrarClientes = u.PodeCadastrarClientes }).ToListAsync());
    }
    [HttpGet] public async Task<IActionResult> Editar(int id = 0)
    {
        if (id == 0) return View(new UsuarioForm());
        var u = await db.Usuarios.FindAsync(id);
        return u is null ? NotFound() : View(new UsuarioForm { Id = u.Id, Nome = u.Nome, Login = u.Login, Perfil = u.Perfil, Ativo = u.Ativo, PodeCadastrarClientes = u.PodeCadastrarClientes });
    }
    [HttpPost] public async Task<IActionResult> Editar(UsuarioForm form)
    {
        if (form.Id == 0 && string.IsNullOrEmpty(form.Senha)) ModelState.AddModelError("Senha", "Informe uma senha inicial de pelo menos 12 caracteres.");
        return await Salvar(async () => await op.Transacao(async () =>
        {
            Operacoes.Exigir(!await db.Usuarios.AnyAsync(u => u.Login == form.Login && u.Id != form.Id), "Login já utilizado.");
            var u = form.Id == 0 ? new Usuario() : await db.Usuarios.FindAsync(form.Id);
            Operacoes.Exigir(u is not null, "Usuário não encontrado.");
            if (u!.Ativo && u.Perfil == "Administrador" && (!form.Ativo || form.Perfil != "Administrador"))
                Operacoes.Exigir(await db.Usuarios.AnyAsync(x => x.Id != u.Id && x.Ativo && x.Perfil == "Administrador"), "Não é permitido desativar ou rebaixar o último administrador ativo.");
            u.Nome = form.Nome.Trim(); u.Login = form.Login; u.Perfil = form.Perfil; u.Ativo = form.Ativo; u.PodeCadastrarClientes = form.PodeCadastrarClientes;
            u.VersaoSessao = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(form.Senha)) u.Senha = new PasswordHasher<Usuario>().HashPassword(u, form.Senha);
            if (form.Id == 0) db.Usuarios.Add(u);
            op.Auditar(UsuarioId, $"Usuário {form.Id}: acesso atualizado"); return 0;
        }), () => RedirectToAction(nameof(Index)), () => Task.FromResult<IActionResult>(View(form)));
    }
}
