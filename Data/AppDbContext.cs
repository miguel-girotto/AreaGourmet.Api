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

    public DbSet<EstoqueBebida> EstoqueBebidas { get; set; }

    public DbSet<MovimentacaoEstoque> Movimentacoes { get; set; }
    public DbSet<ConsumoEstoque> Consumos { get; set; }
    public DbSet<Auditoria> Auditorias { get; set; }
    public DbSet<ControleOperacao> ControleOperacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ControleOperacao>().HasData(new ControleOperacao { Id = 1, Versao = 0 });
        modelBuilder.Entity<Pedido>().Property(p => p.ChaveOperacao).HasMaxLength(36);
        modelBuilder.Entity<Pedido>().HasIndex(p => p.ChaveOperacao).IsUnique();
        modelBuilder.Entity<Usuario>().Property(p => p.VersaoSessao).HasMaxLength(36);
        modelBuilder.Entity<ConsumoEstoque>().HasIndex(c => new { c.ProdutoId, c.EstoqueBebidaId }).IsUnique();
        modelBuilder.Entity<MovimentacaoEstoque>().Property(m => m.Tipo).HasMaxLength(24);
        modelBuilder.Entity<MovimentacaoEstoque>().Property(m => m.Observacao).HasMaxLength(300);
        modelBuilder.Entity<Auditoria>().Property(m => m.Acao).HasMaxLength(200);
        modelBuilder.Entity<MovimentacaoEstoque>().HasIndex(m => m.DataHora);
        modelBuilder.Entity<Pedido>().HasIndex(m => m.DataHora);
        modelBuilder.Entity<ConsumoEstoque>().Property(m => m.Quantidade).HasPrecision(18, 3);
        modelBuilder.Entity<MovimentacaoEstoque>().Property(m => m.Quantidade).HasPrecision(18, 3);
        modelBuilder.Entity<MovimentacaoEstoque>().Property(m => m.SaldoApos).HasPrecision(65, 30);


        modelBuilder.Entity<EstoqueBebida>()
            .HasOne(e => e.Produto)
            .WithOne(p => p.EstoqueBebida)
            .HasForeignKey<EstoqueBebida>(e => e.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Nenhuma exclusão de cadastro deve apagar o histórico de vendas/movimentos.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
    }
}