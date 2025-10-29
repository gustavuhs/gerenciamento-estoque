using Domain;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CodigoSku).IsRequired();
                entity.Property(e => e.Nome).IsRequired();
                entity.Property(e => e.PrecoUnitario).HasPrecision(18, 2);
                entity.HasIndex(e => e.CodigoSku).IsUnique();
            });

            modelBuilder.Entity<MovimentacaoEstoque>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantidade).IsRequired();
                entity.HasOne(e => e.Produto)
                      .WithMany(p => p.Movimentacoes)
                      .HasForeignKey(e => e.ProdutoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}