using Microsoft.EntityFrameworkCore;
using AreaGourmet.Api.Models;

namespace AreaGourmet.Api.Data
{

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(e=>
            {
                e.HasIndex(u => u.Login).IsUnique();
            });

            modelBuilder.Entity<Endereco>()
                .HasOne(en => en.Cliente)
                .WithMany(c => c.Enderecos)
                .HasForeignKey(en => en.ClienteId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Produto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Produtos)
                .HasForeignKey(p => p.CategoriaId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Produto>()
                .Property(p => p.PrecoAtual)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EstoqueBebida>()
                .HasOne(e => e.Produto)
                .WithOne(p => p.EstoqueBebida)
                .HasForeignKey<EstoqueBebida>(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EstoqueBebida>(e =>
            {
                e.HasIndex(x => x.ProdutoId).IsUnique();
                e.Property(x => x.QuantidadeAtual).HasPrecision(18, 3);
                e.Property(x => x.QuantidadeMinima).HasPrecision(18, 3);
            });

            modelBuilder.Entity<Pedido>(e =>
            {
                e.HasOne(p => p.Usuario)
                    .WithMany(u => u.Pedidos)
                    .HasForeignKey(p => p.UsuarioId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                //Pedido Mesa
                e.HasOne(p => p.Cliente)
                    .WithMany(c => c.Pedidos)
                    .HasForeignKey(p => p.ClienteId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                //Pedido Entrega
                e.HasOne(p => p.Endereco)
                    .WithMany(en => en.Pedidos)
                    .HasForeignKey(p => p.EnderecoId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                e.Property(p => p.Subtotal).HasPrecision(18, 2);
                e.Property(p => p.TaxaEntrega).HasPrecision(18, 2);
                e.Property(p => p.ValorTotal).HasPrecision(18, 2);

                e.HasIndex(p => p.DataHora);
                e.HasIndex(p => p.NumeroComanda);
            });

            modelBuilder.Entity<ItemPedido>(e =>
            {
                // Pedido 1:N ItemPedido
                e.HasOne(i => i.Pedido)
                    .WithMany(p => p.Itens)
                    .HasForeignKey(i => i.PedidoId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                // Produto 1:N ItemPedido
                e.HasOne(i => i.Produto)
                    .WithMany(p => p.ItensPedido)
                    .HasForeignKey(i => i.ProdutoId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(i => i.PrecoUnitario).HasPrecision(18, 2);
                e.Property(i => i.SubtotalItem).HasPrecision(18, 2);
            });
            
        }
    }
}