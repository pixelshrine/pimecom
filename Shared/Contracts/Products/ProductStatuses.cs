namespace Shared.Contracts.Products;

public static class ProductStatuses
{
    public const string Draft = "Draft";
    public const string InProgress = "In Progress";
    public const string PendingReview = "Pending Review";
    public const string Approved = "Approved";
    public const string ReadyForPublish = "Ready for Publish";
    public const string Published = "Published";
    public const string Discontinued = "Discontinued";
    public const string Archived = "Archived";
    public const string Recalled = "Recalled";

    public static readonly string[] All =
    [
        Draft,
        InProgress,
        PendingReview,
        Approved,
        ReadyForPublish,
        Published,
        Discontinued,
        Archived,
        Recalled
    ];

    public static string Normalize(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return Draft;

        return All.FirstOrDefault(
            value => string.Equals(value, status.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? Draft;
    }
}
