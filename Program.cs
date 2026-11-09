using AreaGourmet.Data;
using AreaGourmet.Models;
using AreaGourmet.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDbContext<AppDbContext>(o => {
    if (builder.Configuration["Database:Provider"] == "Sqlite")
        o.UseSqlite(builder.Configuration.GetConnectionString("Default"));
    else o.UseMySql(builder.Configuration.GetConnectionString("Default"), new MySqlServerVersion(new Version(8, 0, 36)));
});
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => {
    o.LoginPath = "/Conta/Login"; o.AccessDeniedPath = "/Conta/Negado";
    o.Cookie.HttpOnly = true; o.Cookie.SameSite = SameSiteMode.Lax;
    o.ExpireTimeSpan = TimeSpan.FromHours(8); o.SlidingExpiration = false;
    o.Events.OnValidatePrincipal = async ctx => {
        var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var id = int.TryParse(ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var n) ? n : 0;
        var user = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (user is null || !user.Ativo || user.Perfil != ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            || user.SecurityStamp != ctx.Principal?.FindFirst("stamp")?.Value) ctx.RejectPrincipal();
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();
var culture = new CultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture("pt-BR").AddSupportedCultures("pt-BR").AddSupportedUICultures("pt-BR"));
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Conta/Erro"); app.UseHsts(); app.UseHttpsRedirection(); }
app.Use(async (ctx, next) => {
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["Referrer-Policy"] = "same-origin";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});
app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Vendas}/{action=Index}/{id?}");
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Configuration["Database:Provider"] == "Sqlite") await db.Database.EnsureCreatedAsync();
    else await db.Database.MigrateAsync();
    await Seed.Run(scope.ServiceProvider, app.Configuration);
}
await app.RunAsync();
