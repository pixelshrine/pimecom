using Shared.Contracts.Products;

namespace Ecom.Application.Models;

public class ProductReadModel
{
    public Guid Id { get; set; }

    public string SKU { get; set; } = "";

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string ImageUrl { get; set; } = "";

    public string Color { get; set; } = "";

    public string Size { get; set; } = "";

    public string Material { get; set; } = "";

    public string Status { get; set; } = ProductStatuses.Draft;

    public int Version { get; set; }
}
