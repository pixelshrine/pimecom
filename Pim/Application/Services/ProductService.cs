using Pim.Application.Repositories;
using Pim.Domain.Entities;
using Pim.Application.Messaging;
using Shared.Contracts.Products;
using Shared.Messaging;
using Shared.Messaging.Events;
using System.Text.Json;

namespace Pim.Application.Services;

public class ProductService
{
    private readonly IProductRepository _repo;
    private readonly IEventBus _bus;

    public ProductService(
        IProductRepository repo,
        IEventBus bus)
    {
        _repo = repo;
        _bus = bus;
    }

    public async Task<Product> Create(CreateProductRequest request)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            SKU = request.Sku.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            ImageUrl = request.ImageUrl.Trim(),
            Color = request.Color.Trim(),
            Size = request.Size.Trim(),
            Material = request.Material.Trim(),
            Status = ProductStatuses.Normalize(request.Status),
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.Create(product);

        await PublishProductCreated(product);

        return product;
    }

    public Task<List<Product>> GetAll()
    {
        return _repo.GetAll();
    }

    public Task<Product?> Get(Guid id)
    {
        return _repo.Get(id);
    }

    public async Task<bool> UpdateName(Guid id, string name)
    {
        var product = await _repo.Get(id);

        if (product == null)
            return false;

        product.Name = name.Trim();
        product.Version++;

        await _repo.Update(product);

        var evt = new ProductNameUpdated(
            product.Id,
            product.Name,
            product.Version);

        await PublishEvent(evt, product.Version);

        return true;
    }

    public async Task<bool> UpdateStatus(Guid id, string status)
    {
        var product = await _repo.Get(id);

        if (product == null)
            return false;

        product.Status = ProductStatuses.Normalize(status);
        product.Version++;

        await _repo.Update(product);

        var evt = new ProductStatusUpdated(
            product.Id,
            product.Status,
            product.Version);

        await PublishEvent(evt, product.Version);

        return true;
    }

    public async Task<bool> UpdateImage(Guid id, string imageUrl)
    {
        var product = await _repo.Get(id);

        if (product == null)
            return false;

        product.ImageUrl = imageUrl.Trim();
        product.Version++;

        await _repo.Update(product);

        var evt = new ProductImageUpdated(
            product.Id,
            product.ImageUrl,
            product.Version);

        await PublishEvent(evt, product.Version);

        return true;
    }

    public async Task<bool> Update(Guid id, UpdateProductRequest request)
    {
        var product = await _repo.Get(id);

        if (product == null)
            return false;

        product.SKU = request.Sku.Trim();
        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.ImageUrl = request.ImageUrl.Trim();
        product.Color = request.Color.Trim();
        product.Size = request.Size.Trim();
        product.Material = request.Material.Trim();
        product.Status = ProductStatuses.Normalize(request.Status);
        product.Version++;

        await _repo.Update(product);

        var evt = new ProductUpdated(
            product.Id,
            product.SKU,
            product.Name,
            product.Description,
            product.ImageUrl,
            product.Color,
            product.Size,
            product.Material,
            product.Status,
            product.Version);

        await PublishEvent(evt, product.Version);

        return true;
    }

    private async Task PublishProductCreated(Product product)
    {
        var evt = new ProductCreated(
            product.Id,
            product.SKU,
            product.Name,
            product.Description,
            product.ImageUrl,
            product.Color,
            product.Size,
            product.Material,
            product.Status,
            product.Version);

        await PublishEvent(evt, product.Version);
    }

    private async Task PublishEvent(object evt, int version)
    {
        var envelope = new EventEnvelope
        {
            EventType = evt.GetType().Name,
            Version = version,
            Payload = JsonSerializer.Serialize(evt)
        };

        await _bus.Publish(envelope);
    }
}
