namespace InfraSizingCalculator.Models.Growth;

/// <summary>
/// Settings for growth projection calculations
/// </summary>
public class GrowthSettings
{
    /// <summary>
    /// Annual growth rate as a percentage (e.g., 25 for 25%)
    /// </summary>
    public double AnnualGrowthRate { get; set; } = 20;

    /// <summary>
    /// Number of years to project (1, 3, or 5)
    /// </summary>
    public int ProjectionYears { get; set; } = 3;

    /// <summary>
    /// Growth pattern to use for projections
    /// </summary>
    public GrowthPattern Pattern { get; set; } = GrowthPattern.Linear;

    /// <summary>
    /// Whether to include cost projections
    /// </summary>
    public bool IncludeCostProjections { get; set; } = true;

    /// <summary>
    /// Annual cost inflation rate as percentage (e.g., 3 for 3%)
    /// </summary>
    public double AnnualCostInflation { get; set; } = 3;

    /// <summary>
    /// Whether to show cluster limit warnings
    /// </summary>
    public bool ShowClusterLimitWarnings { get; set; } = true;
}

/// <summary>
/// Pattern for growth projection calculation
/// </summary>
public enum GrowthPattern
{
    /// <summary>
    /// Linear growth: same absolute increase each year
    /// </summary>
    Linear,

    /// <summary>
    /// Exponential/compound growth: percentage-based increase each year
    /// </summary>
    Exponential,

    /// <summary>
    /// S-curve growth: slow start, rapid middle, plateau at end
    /// </summary>
    SCurve,

    /// <summary>
    /// Custom growth rates per year
    /// </summary>
    Custom
}

/// <summary>
/// Custom growth rates for each year when using Custom pattern
/// </summary>
public class CustomGrowthRates
{
    public Dictionary<int, double> YearlyRates { get; set; } = new()
    {
        { 1, 30 },  // Year 1: 30% growth
        { 2, 25 },  // Year 2: 25% growth
        { 3, 20 },  // Year 3: 20% growth
        { 4, 15 },  // Year 4: 15% growth
        { 5, 10 }   // Year 5: 10% growth
    };
}
