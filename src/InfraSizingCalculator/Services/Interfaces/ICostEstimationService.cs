using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Interface for cost estimation services
/// </summary>
public interface ICostEstimationService
{
    /// <summary>
    /// Estimate costs for K8s sizing result
    /// </summary>
    Task<CostEstimate> EstimateK8sCostAsync(
        K8sSizingResult sizing,
        CloudProvider provider,
        string region,
        CostEstimationOptions? options = null);

    /// <summary>
    /// Estimate costs for VM sizing result
    /// </summary>
    Task<CostEstimate> EstimateVMCostAsync(
        VMSizingResult sizing,
        CloudProvider provider,
        string region,
        CostEstimationOptions? options = null);

    /// <summary>
    /// Estimate on-premises costs
    /// </summary>
    CostEstimate EstimateOnPremCost(
        K8sSizingResult sizing,
        OnPremPricing pricing,
        CostEstimationOptions? options = null);

    /// <summary>
    /// Estimate on-premises costs for VM
    /// </summary>
    CostEstimate EstimateOnPremVMCost(
        VMSizingResult sizing,
        OnPremPricing pricing,
        CostEstimationOptions? options = null);

    /// <summary>
    /// Compare multiple cost estimates
    /// </summary>
    CostComparison Compare(params CostEstimate[] estimates);

    /// <summary>
    /// Calculate TCO for a given period
    /// </summary>
    decimal CalculateTCO(CostEstimate estimate, int years);
}

/// <summary>
/// Options for cost estimation
/// </summary>
public class CostEstimationOptions
{
    /// <summary>
    /// Pricing type (on-demand, reserved, spot)
    /// </summary>
    public PricingType PricingType { get; set; } = PricingType.OnDemand;

    /// <summary>
    /// Include license costs (OpenShift, Tanzu, etc.)
    /// </summary>
    public bool IncludeLicenses { get; set; } = true;

    /// <summary>
    /// Include support costs
    /// </summary>
    public bool IncludeSupport { get; set; } = true;

    /// <summary>
    /// Support tier
    /// </summary>
    public SupportTier SupportTier { get; set; } = SupportTier.Business;

    /// <summary>
    /// Include storage costs
    /// </summary>
    public bool IncludeStorage { get; set; } = true;

    /// <summary>
    /// Include network costs
    /// </summary>
    public bool IncludeNetwork { get; set; } = true;

    /// <summary>
    /// Storage GB per node (average)
    /// </summary>
    public int StorageGBPerNode { get; set; } = 100;

    /// <summary>
    /// Monthly egress GB estimate
    /// </summary>
    public int MonthlyEgressGB { get; set; } = 100;

    /// <summary>
    /// K8s distribution (for license calculation)
    /// </summary>
    public string? Distribution { get; set; }

    /// <summary>
    /// Include managed control plane costs
    /// </summary>
    public bool IncludeManagedControlPlane { get; set; } = true;

    /// <summary>
    /// Number of load balancers
    /// </summary>
    public int LoadBalancers { get; set; } = 1;

    /// <summary>
    /// Headroom percentage (0-100)
    /// </summary>
    public int HeadroomPercent { get; set; } = 0;

    /// <summary>
    /// Currency for output
    /// </summary>
    public Currency Currency { get; set; } = Currency.USD;
}

/// <summary>
/// Support tier levels
/// </summary>
public enum SupportTier
{
    None,
    Basic,
    Developer,
    Business,
    Enterprise
}
