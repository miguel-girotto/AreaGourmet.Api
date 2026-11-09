using System.Security.Claims;
using AreaGourmet.Data;
using AreaGourmet.Models;
using AreaGourmet.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Controllers;
public class ContaController(AppDbContext db, IPasswordHasher<Usuario> hasher) : Controller {
    [HttpGet, AllowAnonymous] public IActionResult Login() => View(new LoginForm());
    [HttpPost, AllowAnonymous] public async Task<IActionResult> Login(LoginForm form) {
        if (!ModelState.IsValid) return View(form);
        var email = form.Email.Trim().ToLowerInvariant();
        var user = await db.Usuarios.FirstOrDefaultAsync(x=>x.Email==email && x.Ativo);
        if (user != null && hasher.VerifyHashedPassword(user, user.SenhaHash, form.Senha) != PasswordVerificationResult.Failed) {
            var claims = new[] {new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()), new Claim(ClaimTypes.Name,user.Nome),new Claim(ClaimTypes.Role,user.Perfil),new Claim("stamp",user.SecurityStamp)};
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme)));
            return RedirectToAction("Index","Vendas");
        }
        await Task.Delay(350);
        ModelState.AddModelError("", "E-mail ou senha inválidos."); return View(form);
    }
    [HttpPost, Authorize] public async Task<IActionResult> Sair() { await HttpContext.SignOutAsync(); return RedirectToAction(nameof(Login)); }
    public IActionResult Negado() { Response.StatusCode=403; return View(); }
    public IActionResult Erro() { Response.StatusCode=500; return View(); }
}
