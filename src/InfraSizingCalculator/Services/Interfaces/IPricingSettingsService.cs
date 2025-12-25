using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Service for managing pricing settings, including on-prem defaults and cloud API configuration.
/// </summary>
public interface IPricingSettingsService
{
    /// <summary>
    /// Whether to include pricing calculations in results for on-premises deployments.
    /// When false, costs show as "N/A" for on-prem distributions.
    /// </summary>
    bool IncludePricingInResults { get; set; }

    /// <summary>
    /// Event fired when any pricing settings change.
    /// </summary>
    event Action? OnSettingsChanged;

    /// <summary>
    /// Get all pricing settings.
    /// </summary>
    Task<PricingSettings> GetSettingsAsync();

    /// <summary>
    /// Save pricing settings.
    /// </summary>
    Task SaveSettingsAsync(PricingSettings settings);

    /// <summary>
    /// Reset all settings to their default values.
    /// </summary>
    Task ResetToDefaultsAsync();

    /// <summary>
    /// Reset the pricing cache to default values (clears any cached API data).
    /// </summary>
    Task ResetPricingCacheAsync();

    /// <summary>
    /// Get the current status of the pricing cache.
    /// </summary>
    Task<PricingCacheStatus> GetCacheStatusAsync();

    /// <summary>
    /// Get cloud alternatives for a specific on-premises distribution.
    /// Returns distribution-specific alternatives (e.g., ROSA for OpenShift)
    /// plus generic cloud options (EKS, AKS, GKE).
    /// </summary>
    /// <param name="distribution">The on-premises distribution</param>
    /// <returns>List of cloud alternatives, ordered by relevance</returns>
    List<CloudAlternative> GetCloudAlternatives(Distribution distribution);

    /// <summary>
    /// Get only the distribution-specific cloud alternatives.
    /// </summary>
    List<CloudAlternative> GetDistributionSpecificAlternatives(Distribution distribution);

    /// <summary>
    /// Get generic cloud alternatives that apply to any distribution.
    /// </summary>
    List<CloudAlternative> GetGenericCloudAlternatives();

    /// <summary>
    /// Check if a distribution is considered on-premises.
    /// </summary>
    bool IsOnPremDistribution(Distribution distribution);

    /// <summary>
    /// Get the on-premises pricing defaults.
    /// </summary>
    OnPremPricing GetOnPremDefaults();

    /// <summary>
    /// Update the on-premises pricing defaults.
    /// </summary>
    Task UpdateOnPremDefaultsAsync(OnPremPricing defaults);

    /// <summary>
    /// Configure API credentials for a cloud provider.
    /// </summary>
    Task ConfigureCloudApiAsync(CloudProvider provider, CloudApiConfig config);

    /// <summary>
    /// Validate API credentials for a cloud provider.
    /// </summary>
    /// <returns>True if credentials are valid</returns>
    Task<bool> ValidateCloudApiAsync(CloudProvider provider);

    /// <summary>
    /// Get API configuration for a cloud provider.
    /// </summary>
    CloudApiConfig? GetCloudApiConfig(CloudProvider provider);

    /// <summary>
    /// Calculate the total on-premises cost estimate for a deployment.
    /// </summary>
    /// <param name="distribution">The distribution name</param>
    /// <param name="nodeCount">Total number of nodes</param>
    /// <param name="totalCores">Total CPU cores</param>
    /// <param name="totalRamGB">Total RAM in GB</param>
    /// <param name="totalStorageTB">Total storage in TB</param>
    /// <param name="loadBalancers">Number of load balancers</param>
    /// <param name="hasProduction">Whether deployment includes production environment</param>
    /// <returns>Monthly cost breakdown</returns>
    OnPremCostBreakdown CalculateOnPremCost(
        string distribution,
        int nodeCount,
        int totalCores,
        int totalRamGB,
        int totalStorageTB,
        int loadBalancers = 0,
        bool hasProduction = true);

    // ==================== MENDIX PRICING ====================

    /// <summary>
    /// Get Mendix pricing settings.
    /// </summary>
    MendixPricingSettings GetMendixPricingSettings();

    /// <summary>
    /// Update Mendix pricing settings.
    /// </summary>
    Task UpdateMendixPricingSettingsAsync(MendixPricingSettings settings);

    /// <summary>
    /// Calculate Mendix deployment cost based on configuration.
    /// </summary>
    MendixPricingResult CalculateMendixCost(MendixDeploymentConfig config);

    /// <summary>
    /// Get recommended resource pack size based on application requirements.
    /// </summary>
    MendixResourcePackSpec? RecommendResourcePack(
        MendixResourcePackTier tier,
        decimal requiredMemoryGB,
        decimal requiredCpu,
        decimal requiredDbStorageGB);

    /// <summary>
    /// Check if a private cloud provider is officially supported by Mendix.
    /// </summary>
    bool IsMendixSupportedProvider(MendixPrivateCloudProvider provider);

    /// <summary>
    /// Get list of officially supported Mendix private cloud providers.
    /// </summary>
    List<MendixPrivateCloudProvider> GetMendixSupportedProviders();
}

/// <summary>
/// Breakdown of on-premises costs by category.
/// </summary>
public class OnPremCostBreakdown
{
    /// <summary>
    /// Monthly hardware cost (amortized over refresh cycle + maintenance)
    /// </summary>
    public decimal MonthlyHardware { get; set; }

    /// <summary>
    /// Monthly data center cost (power, cooling, rack space)
    /// </summary>
    public decimal MonthlyDataCenter { get; set; }

    /// <summary>
    /// Monthly labor cost (DevOps, SysAdmin, DBA)
    /// </summary>
    public decimal MonthlyLabor { get; set; }

    /// <summary>
    /// Monthly license cost for the distribution
    /// </summary>
    public decimal MonthlyLicense { get; set; }

    /// <summary>
    /// Total monthly cost
    /// </summary>
    public decimal MonthlyTotal => MonthlyHardware + MonthlyDataCenter + MonthlyLabor + MonthlyLicense;

    /// <summary>
    /// Total yearly cost
    /// </summary>
    public decimal YearlyTotal => MonthlyTotal * 12;

    /// <summary>
    /// 3-year TCO
    /// </summary>
    public decimal ThreeYearTCO => MonthlyTotal * 36;

    /// <summary>
    /// Whether pricing was calculated (false if toggle is off)
    /// </summary>
    public bool IsCalculated { get; set; } = true;

    /// <summary>
    /// Create a "N/A" cost breakdown when pricing is disabled
    /// </summary>
    public static OnPremCostBreakdown NotAvailable => new()
    {
        MonthlyHardware = 0,
        MonthlyDataCenter = 0,
        MonthlyLabor = 0,
        MonthlyLicense = 0,
        IsCalculated = false
    };
}
