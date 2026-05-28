using Ecom.Application.Infrastructure.Data;
using Ecom.Application.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Products;
using Shared.Messaging;
using Shared.Messaging.Events;
using System.Text.Json;

namespace Ecom.Application.Services;

public class EventRouter
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EventRouter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Route(EventEnvelope envelope)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var processed = await db.ProcessedEvents
        .AnyAsync(x => x.EventId == envelope.EventId);

        if (processed)
            return;

        switch (envelope.EventType)
        {
            case nameof(ProductCreated):
                await HandleProductCreated(db, envelope);
                break;

            case nameof(ProductNameUpdated):
                await HandleProductNameUpdated(db, envelope);
                break;

            case nameof(ProductImageUpdated):
                await HandleProductImageUpdated(db, envelope);
                break;

            case nameof(ProductUpdated):
                await HandleProductUpdated(db, envelope);
                break;

            case nameof(ProductStatusUpdated):
                await HandleProductStatusUpdated(db, envelope);
                break;
        }

        db.ProcessedEvents.Add(new ProcessedEvent
        {
            EventId = envelope.EventId,
            ProcessedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    private async Task HandleProductCreated(
        AppDbContext db,
        EventEnvelope envelope)
    {
        var evt = JsonSerializer.Deserialize<ProductCreated>(
            envelope.Payload);

        if (evt == null)
            return;

        var exists = await db.Products
            .AnyAsync(x => x.Id == evt.ProductId);

        if (exists)
            return;

        db.Products.Add(new ProductReadModel
        {
            Id = evt.ProductId,
            SKU = evt.SKU,
            Name = evt.Name,
            Description = evt.Description,
            ImageUrl = evt.ImageUrl,
            Color = evt.Color,
            Size = evt.Size,
            Material = evt.Material,
            Status = ProductStatuses.Normalize(evt.Status),
            Version = evt.Version
        });

        await db.SaveChangesAsync();
    }

    private async Task HandleProductNameUpdated(
      AppDbContext db,
      EventEnvelope envelope)
    {
        var evt = JsonSerializer.Deserialize<ProductNameUpdated>(
            envelope.Payload);

        if (evt == null)
            return;

        var product = await db.Products
            .FirstOrDefaultAsync(x => x.Id == evt.ProductId);

        if (product == null)
            return;

        // Version check
        if (evt.Version <= product.Version)
            return;

        product.Name = evt.Name;
        product.Version = evt.Version;

        await db.SaveChangesAsync();
    }

    private async Task HandleProductImageUpdated(
        AppDbContext db,
        EventEnvelope envelope)
    {
        var evt = JsonSerializer.Deserialize<ProductImageUpdated>(
            envelope.Payload);

        if (evt == null)
            return;

        var product = await db.Products
            .FirstOrDefaultAsync(x => x.Id == evt.ProductId);

        if (product == null)
            return;

        if (evt.Version <= product.Version)
            return;

        product.ImageUrl = evt.ImageUrl;
        product.Version = evt.Version;

        await db.SaveChangesAsync();
    }

    private async Task HandleProductStatusUpdated(
        AppDbContext db,
        EventEnvelope envelope)
    {
        var evt = JsonSerializer.Deserialize<ProductStatusUpdated>(
            envelope.Payload);

        if (evt == null)
            return;

        var product = await db.Products
            .FirstOrDefaultAsync(x => x.Id == evt.ProductId);

        if (product == null)
            return;

        if (evt.Version <= product.Version)
            return;

        product.Status = ProductStatuses.Normalize(evt.Status);
        product.Version = evt.Version;

        await db.SaveChangesAsync();
    }

    private async Task HandleProductUpdated(
        AppDbContext db,
        EventEnvelope envelope)
    {
        var evt = JsonSerializer.Deserialize<ProductUpdated>(
            envelope.Payload);

        if (evt == null)
            return;

        var product = await db.Products
            .FirstOrDefaultAsync(x => x.Id == evt.ProductId);

        if (product == null)
        {
            db.Products.Add(new ProductReadModel
            {
                Id = evt.ProductId,
                SKU = evt.SKU,
                Name = evt.Name,
                Description = evt.Description,
                ImageUrl = evt.ImageUrl,
                Color = evt.Color,
                Size = evt.Size,
                Material = evt.Material,
                Status = ProductStatuses.Normalize(evt.Status),
                Version = evt.Version
            });

            await db.SaveChangesAsync();
            return;
        }

        if (evt.Version <= product.Version)
            return;

        product.SKU = evt.SKU;
        product.Name = evt.Name;
        product.Description = evt.Description;
        product.ImageUrl = evt.ImageUrl;
        product.Color = evt.Color;
        product.Size = evt.Size;
        product.Material = evt.Material;
        product.Status = ProductStatuses.Normalize(evt.Status);
        product.Version = evt.Version;

        await db.SaveChangesAsync();
    }
}
