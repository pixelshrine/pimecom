using Ecom.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Application.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductReadModel> Products => Set<ProductReadModel>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductReadModel>(entity =>
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

        modelBuilder.Entity<ProcessedEvent>(entity =>
        {
            entity.HasKey(processedEvent => processedEvent.EventId);
        });
    }
}
