using System.Security.Claims;
using AreaGourmet.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Services;
public class SessaoEvents(AppDbContext db) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var versao = context.Principal?.FindFirstValue("versao");
        var u = int.TryParse(id, out var n) ? await db.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.Id == n) : null;
        if (u is null || !u.Ativo || u.VersaoSessao != versao)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    }
    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> c)
    {
        if (c.Request.Path.StartsWithSegments("/api")) { c.Response.StatusCode = 401; return Task.CompletedTask; }
        return base.RedirectToLogin(c);
    }
    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> c)
    {
        if (c.Request.Path.StartsWithSegments("/api")) { c.Response.StatusCode = 403; return Task.CompletedTask; }
        return base.RedirectToAccessDenied(c);
    }
}
