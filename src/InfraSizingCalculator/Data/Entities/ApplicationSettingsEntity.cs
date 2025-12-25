namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// Main application settings entity stored in database
/// </summary>
public class ApplicationSettingsEntity
{
    public int Id { get; set; }

    // General Settings
    public bool IncludePricingInResults { get; set; } = false;
    public string DefaultCurrency { get; set; } = "USD";
    public int PricingCacheDurationHours { get; set; } = 24;
    public DateTime? LastCacheReset { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
