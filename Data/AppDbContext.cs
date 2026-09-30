using Microsoft.EntityFrameworkCore;
using AreaGourmet.Api.Models;

namespace AreaGourmet.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Cliente> Clientes { get; set; }

    public DbSet<Endereco> Enderecos { get; set; }

    public DbSet<Categoria> Categorias { get; set; }

    public DbSet<Produto> Produtos { get; set; }

    public DbSet<Pedido> Pedidos { get; set; }

    public DbSet<ItemPedido> ItensPedido { get; set; }

    public DbSet<EstoqueInsumo> EstoqueInsumos { get; set; }
}