using System.Security.Claims;
using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using AreaGourmet.Api.Services;
using AreaGourmet.Api.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Controllers;
public class AccountController(AppDbContext db) : Controller
{
    [AllowAnonymous, HttpGet] public IActionResult Login(string? returnUrl) => View(new LoginForm { ReturnUrl = returnUrl });
    [AllowAnonymous, HttpPost, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var u = await db.Usuarios.SingleOrDefaultAsync(u => u.Login == form.Login && u.Ativo);
        var ok = false;
        if (u is not null)
        {
            try
            {
                var hasher = new PasswordHasher<Usuario>();
                var result = hasher.VerifyHashedPassword(u, u.Senha, form.Senha);
                ok = result != PasswordVerificationResult.Failed;
                if (result == PasswordVerificationResult.SuccessRehashNeeded) u.Senha = hasher.HashPassword(u, form.Senha);
            }
            catch (FormatException) { /* Senhas legadas requerem redefinição administrativa. */ }
        }
        if (!ok || u is null || u.Perfil is not ("Administrador" or "Caixa"))
        {
            ModelState.AddModelError("", "Login ou senha inválidos.");
            return View(form);
        }
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, u.Id.ToString()), new(ClaimTypes.Name, u.Nome), new(ClaimTypes.Role, u.Perfil), new("versao", u.VersaoSessao), new("clientes", u.PodeCadastrarClientes ? "sim" : "nao") };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        db.Auditorias.Add(new Auditoria { UsuarioId = u.Id, Acao = "Login realizado" });
        await db.SaveChangesAsync();
        return LocalRedirect(Url.IsLocalUrl(form.ReturnUrl) ? form.ReturnUrl! : "/Vendas");
    }
    [HttpPost] public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(); return RedirectToAction(nameof(Login)); }
    [HttpGet] public IActionResult Negado() { Response.StatusCode = 403; return View(); }
}
