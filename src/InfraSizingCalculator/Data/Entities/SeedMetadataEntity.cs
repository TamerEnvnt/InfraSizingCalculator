namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// Tracks seed data version using a hash-based approach.
/// Single row table - only one record with Id = 1.
/// </summary>
public class SeedMetadataEntity
{
    public int Id { get; set; } = 1;

    /// <summary>
    /// SHA256 hash of all seed data content.
    /// Used to detect when seed data has changed.
    /// </summary>
    public string SeedHash { get; set; } = string.Empty;

    /// <summary>
    /// Semantic version of the seed data (e.g., "1.0.0").
    /// Incremented when breaking changes are made.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// When the seed data was last applied.
    /// </summary>
    public DateTime LastSeededAt { get; set; }

    /// <summary>
    /// Number of times seeding has been performed.
    /// </summary>
    public int SeedCount { get; set; }
}
