using AreaGourmet.Models;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<Usuario>().HasIndex(x => x.Email).IsUnique();
        m.Entity<Cliente>().HasIndex(x => x.Telefone).IsUnique();
        m.Entity<Pedido>().HasIndex(x => x.ChaveOperacao).IsUnique();
        m.Entity<Pedido>().HasIndex(x => x.DataHora);
        m.Entity<Produto>().Property(x => x.Versao).IsConcurrencyToken();
        m.Entity<Produto>().Property(x => x.PrecoUnitario).HasPrecision(12, 2);
        m.Entity<Pedido>().Property(x => x.TaxaEntrega).HasPrecision(12, 2);
        m.Entity<Pedido>().Property(x => x.ValorTotal).HasPrecision(12, 2);
        m.Entity<ItemPedido>().Property(x => x.PrecoPraticado).HasPrecision(12, 2);
        m.Entity<Endereco>().Ignore(x => x.Resumo);
        foreach (var relation in m.Model.GetEntityTypes().SelectMany(x => x.GetForeignKeys())) relation.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
