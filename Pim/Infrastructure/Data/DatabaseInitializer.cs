using Microsoft.EntityFrameworkCore;
using Pim.Domain.Entities;
using Shared.Contracts.Products;

namespace Pim.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureDatabaseCreatedAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 12;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                //await db.Database.EnsureCreatedAsync(cancellationToken);
                //await SeedProductsAsync(db, cancellationToken);
                await db.Database.MigrateAsync(cancellationToken);
                //await SeedProductsAsync(db, cancellationToken);
                await db.Database.MigrateAsync(cancellationToken);
                await SeedProductsAsync(db, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex, "Database is not ready. Retrying attempt {Attempt}/{MaxAttempts}.", attempt, maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        using var finalScope = services.CreateScope();
        var finalDb = finalScope.ServiceProvider.GetRequiredService<AppDbContext>();
        //await finalDb.Database.EnsureCreatedAsync(cancellationToken);
        //await SeedProductsAsync(finalDb, cancellationToken);
        await finalDb.Database.MigrateAsync(cancellationToken);
        await SeedProductsAsync(finalDb, cancellationToken);
    }

    private static async Task SeedProductsAsync(
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        if (await db.Products.AnyAsync(cancellationToken))
            return;

        db.Products.Add(new Product
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            SKU = "DEMO-001",
            Name = "Everyday Runner",
            Description = "A demo product seeded for local development when SQL Server and RabbitMQ are not running.",
            ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1200&q=80",
            Color = "Graphite",
            Size = "M",
            Material = "Lightweight knit mesh",
            Status = ProductStatuses.Published,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
