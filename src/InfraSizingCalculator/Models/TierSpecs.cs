namespace InfraSizingCalculator.Models;

/// <summary>
/// Application tier specifications for CPU and RAM per pod
/// </summary>
public record TierSpecs(double Cpu, double Ram);
