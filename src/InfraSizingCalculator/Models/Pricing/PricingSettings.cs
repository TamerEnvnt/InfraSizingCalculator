namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// Global pricing settings for the application, including on-prem defaults and cloud API configuration.
/// </summary>
public class PricingSettings
{
    /// <summary>
    /// Whether to include pricing calculations in results for on-premises deployments.
    /// When false, costs show as "N/A" for on-prem distributions.
    /// </summary>
    public bool IncludePricingInResults { get; set; } = false;

    /// <summary>
    /// Default pricing values for on-premises deployments.
    /// </summary>
    public OnPremPricing OnPremDefaults { get; set; } = new();

    /// <summary>
    /// API configuration for each cloud provider (for live pricing fetches).
    /// </summary>
    public Dictionary<CloudProvider, CloudApiConfig> CloudApiConfigs { get; set; } = new();

    /// <summary>
    /// Mendix platform pricing configuration (from official pricebook).
    /// </summary>
    public MendixPricingSettings MendixPricing { get; set; } = new();

    /// <summary>
    /// Cloud provider pricing defaults (AWS, Azure, GCP).
    /// </summary>
    public CloudPricingSettings CloudPricing { get; set; } = new();

    /// <summary>
    /// When the pricing cache was last reset to defaults.
    /// </summary>
    public DateTime? LastCacheReset { get; set; }

    /// <summary>
    /// When the settings were last modified.
    /// </summary>
    public DateTime? LastModified { get; set; }
}

/// <summary>
/// Configuration for a cloud provider's pricing API.
/// </summary>
public class CloudApiConfig
{
    /// <summary>
    /// API key or access key ID for the cloud provider.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Secret key for providers that require it (e.g., AWS).
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// Service account JSON (for GCP).
    /// </summary>
    public string? ServiceAccountJson { get; set; }

    /// <summary>
    /// Default region to use for pricing queries.
    /// </summary>
    public string? DefaultRegion { get; set; }

    /// <summary>
    /// Whether this provider has valid API credentials configured.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrEmpty(ApiKey) || !string.IsNullOrEmpty(ServiceAccountJson);

    /// <summary>
    /// When the credentials were last validated.
    /// </summary>
    public DateTime? LastValidated { get; set; }

    /// <summary>
    /// Whether the last validation was successful.
    /// </summary>
    public bool? IsValid { get; set; }
}

/// <summary>
/// Status of the pricing cache.
/// </summary>
public class PricingCacheStatus
{
    /// <summary>
    /// When the cache was last reset.
    /// </summary>
    public DateTime? LastReset { get; set; }

    /// <summary>
    /// Number of providers with cached pricing data.
    /// </summary>
    public int CachedProviderCount { get; set; }

    /// <summary>
    /// Number of providers with configured API keys.
    /// </summary>
    public int ConfiguredApiCount { get; set; }

    /// <summary>
    /// Whether the cache is considered stale (older than 24 hours).
    /// </summary>
    public bool IsStale { get; set; }

    /// <summary>
    /// Breakdown of cache status by provider.
    /// </summary>
    public Dictionary<CloudProvider, ProviderCacheStatus> ProviderStatus { get; set; } = new();
}

/// <summary>
/// Cache status for a specific cloud provider.
/// </summary>
public class ProviderCacheStatus
{
    /// <summary>
    /// Whether this provider has cached pricing data.
    /// </summary>
    public bool HasCachedData { get; set; }

    /// <summary>
    /// When the pricing data was last fetched.
    /// </summary>
    public DateTime? LastFetched { get; set; }

    /// <summary>
    /// Source of the pricing data (API, Default, Manual).
    /// </summary>
    public string Source { get; set; } = "Default";

    /// <summary>
    /// Number of regions with pricing data.
    /// </summary>
    public int RegionCount { get; set; }
}
