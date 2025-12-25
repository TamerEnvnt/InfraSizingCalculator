namespace InfraSizingCalculator.Models;

/// <summary>
/// Node specifications for CPU, RAM, and Disk
/// </summary>
public record NodeSpecs(int Cpu, int Ram, int Disk = 100)
{
    public static NodeSpecs Zero => new(0, 0, 0);
}
