using InfraSizingCalculator.Models;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Service for managing tier CPU and RAM configuration across different technologies.
/// Encapsulates the mapping between technology/tier combinations and their resource values.
/// </summary>
public interface ITierConfigurationService
{
    /// <summary>
    /// Gets the CPU allocation for a specific technology and tier.
    /// </summary>
    /// <param name="settings">The calculator settings containing tier values</param>
    /// <param name="technology">Technology identifier (dotnet, java, nodejs, python, go, mendix)</param>
    /// <param name="tier">Tier name (Small, Medium, Large, XLarge)</param>
    /// <returns>CPU cores allocated for this combination</returns>
    double GetTierCpu(UICalculatorSettings settings, string technology, string tier);

    /// <summary>
    /// Gets the RAM allocation for a specific technology and tier.
    /// </summary>
    /// <param name="settings">The calculator settings containing tier values</param>
    /// <param name="technology">Technology identifier (dotnet, java, nodejs, python, go, mendix)</param>
    /// <param name="tier">Tier name (Small, Medium, Large, XLarge)</param>
    /// <returns>RAM in GB allocated for this combination</returns>
    double GetTierRam(UICalculatorSettings settings, string technology, string tier);

    /// <summary>
    /// Sets the CPU allocation for a specific technology and tier.
    /// </summary>
    /// <param name="settings">The calculator settings to modify</param>
    /// <param name="technology">Technology identifier (dotnet, java, nodejs, python, go, mendix)</param>
    /// <param name="tier">Tier name (Small, Medium, Large, XLarge)</param>
    /// <param name="value">CPU cores to allocate</param>
    void SetTierCpu(UICalculatorSettings settings, string technology, string tier, double value);

    /// <summary>
    /// Sets the RAM allocation for a specific technology and tier.
    /// </summary>
    /// <param name="settings">The calculator settings to modify</param>
    /// <param name="technology">Technology identifier (dotnet, java, nodejs, python, go, mendix)</param>
    /// <param name="tier">Tier name (Small, Medium, Large, XLarge)</param>
    /// <param name="value">RAM in GB to allocate</param>
    void SetTierRam(UICalculatorSettings settings, string technology, string tier, double value);

    /// <summary>
    /// Gets all supported technology identifiers.
    /// </summary>
    IReadOnlyList<string> GetSupportedTechnologies();

    /// <summary>
    /// Gets all supported tier names.
    /// </summary>
    IReadOnlyList<string> GetSupportedTiers();
}
