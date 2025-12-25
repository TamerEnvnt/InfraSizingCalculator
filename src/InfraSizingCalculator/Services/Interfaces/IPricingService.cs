using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Interface for pricing services
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Get pricing for a specific provider and region
    /// </summary>
    Task<PricingModel> GetPricingAsync(CloudProvider provider, string region, PricingType pricingType = PricingType.OnDemand);

    /// <summary>
    /// Get available regions for a provider
    /// </summary>
    List<RegionInfo> GetRegions(CloudProvider provider);

    /// <summary>
    /// Get default pricing for a provider (cached or fallback)
    /// </summary>
    PricingModel GetDefaultPricing(CloudProvider provider);

    /// <summary>
    /// Get on-premises pricing configuration
    /// </summary>
    OnPremPricing GetOnPremPricing();

    /// <summary>
    /// Update on-premises pricing configuration
    /// </summary>
    void UpdateOnPremPricing(OnPremPricing pricing);

    /// <summary>
    /// Check if pricing data is stale (needs refresh)
    /// </summary>
    bool IsPricingStale(CloudProvider provider, string region);

    /// <summary>
    /// Get last update time for pricing
    /// </summary>
    DateTime? GetLastPricingUpdate(CloudProvider provider, string region);

    /// <summary>
    /// Force refresh pricing from live API
    /// </summary>
    Task<PricingModel?> RefreshPricingAsync(CloudProvider provider, string region);
}

/// <summary>
/// Interface for cloud pricing providers
/// </summary>
public interface IPricingProvider
{
    /// <summary>
    /// Provider name
    /// </summary>
    CloudProvider Provider { get; }

    /// <summary>
    /// Provider display name
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Whether this provider supports live API pricing
    /// </summary>
    bool SupportsLiveApi { get; }

    /// <summary>
    /// Fetch pricing from the provider's API
    /// </summary>
    Task<PricingModel?> FetchPricingAsync(string region, PricingType pricingType);

    /// <summary>
    /// Get default/fallback pricing (no API call)
    /// </summary>
    PricingModel GetDefaultPricing(string region);

    /// <summary>
    /// Get available regions
    /// </summary>
    List<RegionInfo> GetRegions();
}
