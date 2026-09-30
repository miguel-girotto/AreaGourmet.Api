using Microsoft.EntityFrameworkCore;
using AreaGourmet.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/api/health/database", async (AppDbContext db) =>
{
    var conectado = await db.Database.CanConnectAsync();

    if (conectado)
    {
        return Results.Ok(new
        {
            database = "area_gourmet",
            status = "conectado"
        });
    }

    return Results.Problem("Não foi possível conectar ao banco de dados.");
});

app.Run();