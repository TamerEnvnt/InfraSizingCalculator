namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// Cloud provider API credentials stored securely in database
/// </summary>
public class CloudApiCredentialsEntity
{
    public int Id { get; set; }

    public string ProviderName { get; set; } = string.Empty; // AWS, Azure, GCP, etc.

    // Encrypted API credentials
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
    public string? Region { get; set; }
    public string? TenantId { get; set; } // For Azure
    public string? SubscriptionId { get; set; } // For Azure
    public string? ProjectId { get; set; } // For GCP

    // Metadata
    public bool IsConfigured { get; set; } = false;
    public DateTime? LastValidated { get; set; }
    public string? ValidationStatus { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
