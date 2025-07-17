using ApiCatalogo.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCatalogo.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Produto>? Produtos { get; set; }
    public DbSet<Categoria>? Categorias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>().HasKey(c => c.CategoriaId);
        modelBuilder.Entity<Categoria>().Property(c => c.Nome).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Categoria>().Property(c => c.Descricao).HasMaxLength(150).IsRequired();

        modelBuilder.Entity<Produto>().HasKey(p => p.ProdutoId);
        modelBuilder.Entity<Produto>().Property(p => p.Nome).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Produto>().Property(p => p.Descricao).HasMaxLength(150);
        modelBuilder.Entity<Produto>().Property(p => p.Imagem).HasMaxLength(100);
        modelBuilder.Entity<Produto>().Property(p => p.Preco).HasPrecision(14,2);

        modelBuilder.Entity<Produto>()
                    .HasOne<Categoria>(c => c.Categoria)
                    .WithMany(p => p.Produtos)
                    .HasForeignKey(c => c.CategoriaId);
    }
}
