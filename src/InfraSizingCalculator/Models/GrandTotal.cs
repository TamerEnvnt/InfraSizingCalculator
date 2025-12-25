namespace InfraSizingCalculator.Models;

/// <summary>
/// Grand totals across all environments
/// </summary>
public class GrandTotal
{
    public int TotalNodes { get; init; }
    public int TotalMasters { get; init; }
    public int TotalInfra { get; init; }
    public int TotalWorkers { get; init; }
    public int TotalCpu { get; init; }
    public int TotalRam { get; init; }
    public int TotalDisk { get; init; }
}
