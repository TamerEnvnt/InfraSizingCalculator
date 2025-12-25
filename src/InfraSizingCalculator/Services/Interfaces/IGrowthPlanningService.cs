using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Growth;
using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Service for calculating growth projections and capacity planning
/// </summary>
public interface IGrowthPlanningService
{
    /// <summary>
    /// Calculate growth projection for K8s sizing
    /// </summary>
    /// <param name="currentResult">Current sizing result</param>
    /// <param name="currentCost">Current cost estimate (optional)</param>
    /// <param name="settings">Growth settings</param>
    /// <param name="distribution">K8s distribution for limit checking</param>
    /// <returns>Growth projection with timeline, warnings, and recommendations</returns>
    GrowthProjection CalculateK8sGrowthProjection(
        K8sSizingResult currentResult,
        CostEstimate? currentCost,
        GrowthSettings settings,
        Distribution distribution);

    /// <summary>
    /// Calculate growth projection for VM sizing
    /// </summary>
    /// <param name="currentResult">Current VM sizing result</param>
    /// <param name="currentCost">Current cost estimate (optional)</param>
    /// <param name="settings">Growth settings</param>
    /// <returns>Growth projection with timeline, warnings, and recommendations</returns>
    GrowthProjection CalculateVMGrowthProjection(
        VMSizingResult currentResult,
        CostEstimate? currentCost,
        GrowthSettings settings);

    /// <summary>
    /// Get cluster limits for a specific distribution
    /// </summary>
    /// <param name="distribution">K8s distribution</param>
    /// <returns>Dictionary of limit names to values</returns>
    Dictionary<string, int> GetClusterLimits(Distribution distribution);

    /// <summary>
    /// Calculate when a specific limit will be reached
    /// </summary>
    /// <param name="currentValue">Current value</param>
    /// <param name="limit">Limit value</param>
    /// <param name="annualGrowthRate">Annual growth rate as percentage</param>
    /// <param name="pattern">Growth pattern</param>
    /// <returns>Year when limit will be reached, or null if not within 10 years</returns>
    int? CalculateYearToLimit(double currentValue, double limit, double annualGrowthRate, GrowthPattern pattern);

    /// <summary>
    /// Generate scaling recommendations based on projection
    /// </summary>
    /// <param name="projection">Growth projection</param>
    /// <param name="distribution">K8s distribution</param>
    /// <returns>List of scaling recommendations</returns>
    List<ScalingRecommendation> GenerateRecommendations(GrowthProjection projection, Distribution? distribution);
}
