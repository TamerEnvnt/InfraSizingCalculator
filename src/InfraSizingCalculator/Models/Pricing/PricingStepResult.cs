using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// Result from the pricing step, containing the user's pricing configuration.
/// </summary>
public class PricingStepResult
{
    /// <summary>
    /// Whether the user chose to include pricing in results (for on-prem).
    /// </summary>
    public bool IncludePricing { get; set; }

    /// <summary>
    /// Whether this is an on-premises deployment.
    /// </summary>
    public bool IsOnPrem { get; set; }

    /// <summary>
    /// On-premises cost breakdown (if on-prem and pricing included).
    /// </summary>
    public OnPremCostBreakdown? OnPremCost { get; set; }

    /// <summary>
    /// Cloud cost estimate (if cloud or comparing alternatives).
    /// </summary>
    public CostEstimate? CloudCost { get; set; }

    /// <summary>
    /// Selected cloud alternative (if user chose to compare).
    /// </summary>
    public CloudAlternative? SelectedAlternative { get; set; }

    /// <summary>
    /// Mendix platform cost breakdown (if LowCode platform selected).
    /// </summary>
    public MendixPricingResult? MendixCost { get; set; }

    /// <summary>
    /// Whether costs are available (true if cloud, or on-prem with toggle on, or has Mendix cost).
    /// </summary>
    public bool HasCosts => !IsOnPrem || IncludePricing || MendixCost != null;

    /// <summary>
    /// Get the monthly cost to display (N/A if not available).
    /// </summary>
    public decimal? MonthlyCost
    {
        get
        {
            if (!HasCosts) return null;
            if (IsOnPrem && OnPremCost != null) return OnPremCost.MonthlyTotal;
            if (!IsOnPrem && CloudCost != null) return CloudCost.MonthlyTotal;
            return null;
        }
    }

    /// <summary>
    /// Get the yearly cost to display (N/A if not available).
    /// </summary>
    public decimal? YearlyCost
    {
        get
        {
            if (!HasCosts) return null;
            if (IsOnPrem && OnPremCost != null) return OnPremCost.YearlyTotal;
            if (!IsOnPrem && CloudCost != null) return CloudCost.YearlyTotal;
            return null;
        }
    }

    /// <summary>
    /// Get the 3-year TCO to display (N/A if not available).
    /// </summary>
    public decimal? ThreeYearTCO
    {
        get
        {
            if (!HasCosts) return null;
            if (IsOnPrem && OnPremCost != null) return OnPremCost.ThreeYearTCO;
            if (!IsOnPrem && CloudCost != null) return CloudCost.ThreeYearTCO;
            return null;
        }
    }

    /// <summary>
    /// Format a cost value, returning "N/A" if not available.
    /// </summary>
    public static string FormatCost(decimal? cost, string currency = "USD")
    {
        if (cost == null) return "N/A";
        return cost.Value.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
    }
}
