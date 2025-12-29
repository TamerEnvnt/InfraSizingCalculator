namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// Database entity for technology configurations.
/// Stores technology-specific settings, resource requirements, and info content.
/// </summary>
public class TechnologyConfigEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Technology enum value stored as string for readability
    /// </summary>
    public string TechnologyKey { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the technology
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Vendor/provider name
    /// </summary>
    public string Vendor { get; set; } = string.Empty;

    /// <summary>
    /// Icon identifier for UI
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Brand color in hex format
    /// </summary>
    public string BrandColor { get; set; } = "#326CE5";

    /// <summary>
    /// Category: "native" or "lowcode"
    /// </summary>
    public string Category { get; set; } = "native";

    // Resource Multipliers (relative to baseline)
    /// <summary>
    /// CPU multiplier relative to baseline (1.0 = baseline)
    /// </summary>
    public decimal CpuMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Memory multiplier relative to baseline (1.0 = baseline)
    /// </summary>
    public decimal MemoryMultiplier { get; set; } = 1.0m;

    // Default App Tier Sizing (CPU cores)
    public int SmallAppCpu { get; set; } = 2;
    public int MediumAppCpu { get; set; } = 4;
    public int LargeAppCpu { get; set; } = 8;
    public int XLargeAppCpu { get; set; } = 16;

    // Default App Tier Sizing (RAM in GB)
    public int SmallAppRam { get; set; } = 4;
    public int MediumAppRam { get; set; } = 8;
    public int LargeAppRam { get; set; } = 16;
    public int XLargeAppRam { get; set; } = 32;

    // Info Modal Content
    /// <summary>
    /// Short description for quick reference (1-2 sentences)
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Detailed HTML content for the info modal popup
    /// </summary>
    public string DetailedInfoHtml { get; set; } = string.Empty;

    /// <summary>
    /// Key features as JSON array
    /// </summary>
    public string KeyFeaturesJson { get; set; } = "[]";

    /// <summary>
    /// Performance characteristics and notes
    /// </summary>
    public string PerformanceNotes { get; set; } = string.Empty;

    /// <summary>
    /// Typical use cases for this technology
    /// </summary>
    public string UseCases { get; set; } = string.Empty;

    /// <summary>
    /// Sort order for display in UI
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this technology is currently active/visible
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
