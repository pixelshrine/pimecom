namespace Shared.Messaging.Events;

public record ProductCreated(
    Guid ProductId,
    string SKU,
    string Name,
    string Description,
    string ImageUrl,
    string Color,
    string Size,
    string Material,
    string Status,
    int Version);

public record ProductNameUpdated(
    Guid ProductId,
    string Name,
    int Version);

public record ProductImageUpdated(
    Guid ProductId,
    string ImageUrl,
    int Version);

public record ProductUpdated(
    Guid ProductId,
    string SKU,
    string Name,
    string Description,
    string ImageUrl,
    string Color,
    string Size,
    string Material,
    string Status,
    int Version);

public record ProductStatusUpdated(
    Guid ProductId,
    string Status,
    int Version);
