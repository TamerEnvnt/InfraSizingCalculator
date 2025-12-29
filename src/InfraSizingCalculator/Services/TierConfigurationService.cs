using InfraSizingCalculator.Models;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service for managing tier CPU and RAM configuration across different technologies.
/// Uses a dictionary-based approach to avoid repetitive switch statements.
/// </summary>
public class TierConfigurationService : ITierConfigurationService
{
    private static readonly IReadOnlyList<string> Technologies = new[]
    {
        "dotnet", "java", "nodejs", "python", "go", "mendix"
    };

    private static readonly IReadOnlyList<string> Tiers = new[]
    {
        "Small", "Medium", "Large", "XLarge"
    };

    /// <summary>
    /// Map of (technology, tier) -> (getCpu, setCpu, getRam, setRam) accessors
    /// </summary>
    private static readonly Dictionary<(string tech, string tier), TierAccessors> AccessorMap = new()
    {
        // .NET
        [("dotnet", "Small")] = new(s => s.DotNetSmallCpu, (s, v) => s.DotNetSmallCpu = v, s => s.DotNetSmallRam, (s, v) => s.DotNetSmallRam = v),
        [("dotnet", "Medium")] = new(s => s.DotNetMediumCpu, (s, v) => s.DotNetMediumCpu = v, s => s.DotNetMediumRam, (s, v) => s.DotNetMediumRam = v),
        [("dotnet", "Large")] = new(s => s.DotNetLargeCpu, (s, v) => s.DotNetLargeCpu = v, s => s.DotNetLargeRam, (s, v) => s.DotNetLargeRam = v),
        [("dotnet", "XLarge")] = new(s => s.DotNetXLargeCpu, (s, v) => s.DotNetXLargeCpu = v, s => s.DotNetXLargeRam, (s, v) => s.DotNetXLargeRam = v),

        // Java
        [("java", "Small")] = new(s => s.JavaSmallCpu, (s, v) => s.JavaSmallCpu = v, s => s.JavaSmallRam, (s, v) => s.JavaSmallRam = v),
        [("java", "Medium")] = new(s => s.JavaMediumCpu, (s, v) => s.JavaMediumCpu = v, s => s.JavaMediumRam, (s, v) => s.JavaMediumRam = v),
        [("java", "Large")] = new(s => s.JavaLargeCpu, (s, v) => s.JavaLargeCpu = v, s => s.JavaLargeRam, (s, v) => s.JavaLargeRam = v),
        [("java", "XLarge")] = new(s => s.JavaXLargeCpu, (s, v) => s.JavaXLargeCpu = v, s => s.JavaXLargeRam, (s, v) => s.JavaXLargeRam = v),

        // Node.js
        [("nodejs", "Small")] = new(s => s.NodeJsSmallCpu, (s, v) => s.NodeJsSmallCpu = v, s => s.NodeJsSmallRam, (s, v) => s.NodeJsSmallRam = v),
        [("nodejs", "Medium")] = new(s => s.NodeJsMediumCpu, (s, v) => s.NodeJsMediumCpu = v, s => s.NodeJsMediumRam, (s, v) => s.NodeJsMediumRam = v),
        [("nodejs", "Large")] = new(s => s.NodeJsLargeCpu, (s, v) => s.NodeJsLargeCpu = v, s => s.NodeJsLargeRam, (s, v) => s.NodeJsLargeRam = v),
        [("nodejs", "XLarge")] = new(s => s.NodeJsXLargeCpu, (s, v) => s.NodeJsXLargeCpu = v, s => s.NodeJsXLargeRam, (s, v) => s.NodeJsXLargeRam = v),

        // Python
        [("python", "Small")] = new(s => s.PythonSmallCpu, (s, v) => s.PythonSmallCpu = v, s => s.PythonSmallRam, (s, v) => s.PythonSmallRam = v),
        [("python", "Medium")] = new(s => s.PythonMediumCpu, (s, v) => s.PythonMediumCpu = v, s => s.PythonMediumRam, (s, v) => s.PythonMediumRam = v),
        [("python", "Large")] = new(s => s.PythonLargeCpu, (s, v) => s.PythonLargeCpu = v, s => s.PythonLargeRam, (s, v) => s.PythonLargeRam = v),
        [("python", "XLarge")] = new(s => s.PythonXLargeCpu, (s, v) => s.PythonXLargeCpu = v, s => s.PythonXLargeRam, (s, v) => s.PythonXLargeRam = v),

        // Go
        [("go", "Small")] = new(s => s.GoSmallCpu, (s, v) => s.GoSmallCpu = v, s => s.GoSmallRam, (s, v) => s.GoSmallRam = v),
        [("go", "Medium")] = new(s => s.GoMediumCpu, (s, v) => s.GoMediumCpu = v, s => s.GoMediumRam, (s, v) => s.GoMediumRam = v),
        [("go", "Large")] = new(s => s.GoLargeCpu, (s, v) => s.GoLargeCpu = v, s => s.GoLargeRam, (s, v) => s.GoLargeRam = v),
        [("go", "XLarge")] = new(s => s.GoXLargeCpu, (s, v) => s.GoXLargeCpu = v, s => s.GoXLargeRam, (s, v) => s.GoXLargeRam = v),

        // Mendix
        [("mendix", "Small")] = new(s => s.MendixSmallCpu, (s, v) => s.MendixSmallCpu = v, s => s.MendixSmallRam, (s, v) => s.MendixSmallRam = v),
        [("mendix", "Medium")] = new(s => s.MendixMediumCpu, (s, v) => s.MendixMediumCpu = v, s => s.MendixMediumRam, (s, v) => s.MendixMediumRam = v),
        [("mendix", "Large")] = new(s => s.MendixLargeCpu, (s, v) => s.MendixLargeCpu = v, s => s.MendixLargeRam, (s, v) => s.MendixLargeRam = v),
        [("mendix", "XLarge")] = new(s => s.MendixXLargeCpu, (s, v) => s.MendixXLargeCpu = v, s => s.MendixXLargeRam, (s, v) => s.MendixXLargeRam = v),
    };

    public double GetTierCpu(UICalculatorSettings settings, string technology, string tier)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (AccessorMap.TryGetValue((technology.ToLowerInvariant(), tier), out var accessors))
        {
            return accessors.GetCpu(settings);
        }

        return 0.5; // Default CPU
    }

    public double GetTierRam(UICalculatorSettings settings, string technology, string tier)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (AccessorMap.TryGetValue((technology.ToLowerInvariant(), tier), out var accessors))
        {
            return accessors.GetRam(settings);
        }

        return 1.0; // Default RAM
    }

    public void SetTierCpu(UICalculatorSettings settings, string technology, string tier, double value)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (AccessorMap.TryGetValue((technology.ToLowerInvariant(), tier), out var accessors))
        {
            accessors.SetCpu(settings, value);
        }
    }

    public void SetTierRam(UICalculatorSettings settings, string technology, string tier, double value)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (AccessorMap.TryGetValue((technology.ToLowerInvariant(), tier), out var accessors))
        {
            accessors.SetRam(settings, value);
        }
    }

    public IReadOnlyList<string> GetSupportedTechnologies() => Technologies;

    public IReadOnlyList<string> GetSupportedTiers() => Tiers;

    /// <summary>
    /// Holds the getter and setter accessors for a technology/tier combination.
    /// </summary>
    private readonly record struct TierAccessors(
        Func<UICalculatorSettings, double> GetCpu,
        Action<UICalculatorSettings, double> SetCpu,
        Func<UICalculatorSettings, double> GetRam,
        Action<UICalculatorSettings, double> SetRam
    );
}
