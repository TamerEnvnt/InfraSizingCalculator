using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Service for retrieving info modal content from the database.
/// Provides a database-first approach with fallback to default content.
/// </summary>
public interface IInfoContentService
{
    /// <summary>
    /// Gets info content for general info types (Platform, Deployment, ClusterMode, etc.)
    /// </summary>
    Task<InfoContent> GetInfoTypeContentAsync(string infoType);

    /// <summary>
    /// Gets info content for a specific Kubernetes distribution.
    /// </summary>
    Task<InfoContent> GetDistributionInfoAsync(Distribution distribution);

    /// <summary>
    /// Gets info content for a specific technology.
    /// </summary>
    Task<InfoContent> GetTechnologyInfoAsync(Technology technology);

    /// <summary>
    /// Checks if custom content exists in the database for an info type.
    /// </summary>
    Task<bool> HasCustomContentAsync(string contentKey);
}

/// <summary>
/// Represents content for an info modal.
/// </summary>
public record InfoContent(string Title, string ContentHtml)
{
    /// <summary>
    /// Whether this content is from the database (true) or default fallback (false).
    /// </summary>
    public bool IsFromDatabase { get; init; }

    /// <summary>
    /// Creates empty content (used when no content is available).
    /// </summary>
    public static InfoContent Empty => new("Information", "") { IsFromDatabase = false };
}
