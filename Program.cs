using AreaGourmet.Api.Data;
using AreaGourmet.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection ou User Secrets antes de iniciar.");
builder.Services.AddDbContext<AppDbContext>(o => o.UseMySql(connection, ServerVersion.AutoDetect(connection)));
builder.Services.AddScoped<Operacoes>();
builder.Services.AddScoped<SessaoEvents>();
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddAntiforgery(o => o.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Negado";
    o.Cookie.Name = "AreaGourmet.Sessao"; o.Cookie.HttpOnly = true; o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    o.ExpireTimeSpan = TimeSpan.FromHours(8); o.SlidingExpiration = true; o.EventsType = typeof(SessaoEvents);
});
builder.Services.AddAuthorization(o =>
{
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    o.AddPolicy("Admin", p => p.RequireRole("Administrador"));
    o.AddPolicy("CadastrarCliente", p => p.RequireAssertion(c => c.User.IsInRole("Administrador") || c.User.HasClaim("clientes", "sim")));
});
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = 429;
    o.AddPolicy("login", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
        { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddOpenApi();
var app = builder.Build();
if (args.Contains("--criar-admin"))
{
    try { await Bootstrap.CriarAdmin(app.Services, app.Configuration); }
    catch (RegraException ex) { Console.Error.WriteLine(ex.Message); Environment.ExitCode = 1; }
    return;
}
app.UseExceptionHandler("/Home/Erro");
app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");
if (!app.Environment.IsDevelopment()) { app.UseHsts(); app.UseHttpsRedirection(); }
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' https://images.unsplash.com; style-src 'self'; script-src 'self'; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await next();
});
app.UseStaticFiles(); app.UseRouting(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Vendas}/{action=Index}/{id?}");
if (app.Environment.IsDevelopment()) app.MapOpenApi().RequireAuthorization();
app.MapGet("/api/health/database", async (AppDbContext db) =>
    await db.Database.CanConnectAsync() ? Results.Ok(new { database = "area_gourmet", status = "conectado" }) : Results.Problem("Banco indisponível.")).RequireAuthorization();
app.Run();
public partial class Program { }
