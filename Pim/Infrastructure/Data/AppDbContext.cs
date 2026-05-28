using Microsoft.EntityFrameworkCore;
using Pim.Domain.Entities;

namespace Pim.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Id);
            entity.Property(product => product.SKU).HasMaxLength(80);
            entity.Property(product => product.Name).HasMaxLength(200);
            entity.Property(product => product.ImageUrl).HasMaxLength(1000);
            entity.Property(product => product.Color).HasMaxLength(120);
            entity.Property(product => product.Size).HasMaxLength(120);
            entity.Property(product => product.Material).HasMaxLength(200);
            entity.Property(product => product.Status).HasMaxLength(80);
        });
    }
}
