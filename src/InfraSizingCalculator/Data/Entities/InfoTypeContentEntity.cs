namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// Database entity for configurable info modal content.
/// Stores general informational content that appears in help modals.
/// Examples: Platform types, Deployment models, Cluster modes, etc.
/// </summary>
public class InfoTypeContentEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Unique key for the info type (e.g., "Platform", "Deployment", "ClusterMode")
    /// </summary>
    public string InfoTypeKey { get; set; } = string.Empty;

    /// <summary>
    /// Display title for the info modal
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// HTML content for the info modal body
    /// </summary>
    public string ContentHtml { get; set; } = string.Empty;

    /// <summary>
    /// Category for grouping related info types
    /// </summary>
    public string Category { get; set; } = "general";

    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this info type is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
