using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using AreaGourmet.Api.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Services;
public static class Bootstrap
{
    public static async Task CriarAdmin(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var op = scope.ServiceProvider.GetRequiredService<Operacoes>();
        var login = config["Bootstrap:Login"];
        var senha = config["Bootstrap:Senha"];
        if (string.IsNullOrWhiteSpace(login) || !System.Text.RegularExpressions.Regex.IsMatch(login, "^[a-zA-Z0-9._-]{3,80}$") || senha is null || senha.Length < 12 || senha.Length > 128)
            throw new RegraException("Defina Bootstrap__Login (3–80 caracteres) e Bootstrap__Senha (12–128 caracteres) somente para este comando.");
        await op.Transacao(async () =>
        {
            Operacoes.Exigir(!await db.Usuarios.AnyAsync(u => u.Perfil == "Administrador" && u.Ativo), "Já existe administrador ativo. Use a gestão de usuários.");
            Operacoes.Exigir(!await db.Usuarios.AnyAsync(u => u.Login == login), "Login já cadastrado. Não sobrescrevemos contas existentes.");
            var u = new Usuario { Login = login, Nome = "Administrador", Perfil = "Administrador" };
            u.Senha = new PasswordHasher<Usuario>().HashPassword(u, senha);
            db.Usuarios.Add(u);
            op.Auditar(null, "Primeiro administrador criado por comando local");
            return 0;
        });
        Console.WriteLine("Administrador criado. Remova as variáveis Bootstrap do processo.");
    }
}
