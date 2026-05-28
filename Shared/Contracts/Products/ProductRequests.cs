namespace Shared.Contracts.Products;

public record CreateProductRequest(
    string Sku,
    string Name,
    string Description,
    string ImageUrl,
    string Color,
    string Size,
    string Material,
    string? Status);

public record UpdateProductRequest(
    string Sku,
    string Name,
    string Description,
    string ImageUrl,
    string Color,
    string Size,
    string Material,
    string Status);

public record UpdateProductNameRequest(string Name);

public record UpdateProductImageRequest(string ImageUrl);

public record UpdateProductStatusRequest(string Status);
