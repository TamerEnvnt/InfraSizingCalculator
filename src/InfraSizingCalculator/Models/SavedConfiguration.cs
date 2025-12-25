using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// A saved configuration snapshot for quick recall.
/// </summary>
public class SavedConfiguration
{
    /// <summary>
    /// Unique identifier for this configuration.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User-provided name for this configuration.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Auto-generated description based on settings.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// When this configuration was saved.
    /// </summary>
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this is a K8s or VM configuration.
    /// </summary>
    public DeploymentModel DeploymentModel { get; set; }

    /// <summary>
    /// Technology used in this configuration.
    /// </summary>
    public Technology Technology { get; set; }

    /// <summary>
    /// Distribution used (for K8s configurations).
    /// </summary>
    public Distribution? Distribution { get; set; }

    /// <summary>
    /// K8s sizing input (if K8s deployment).
    /// </summary>
    public K8sSizingInput? K8sInput { get; set; }

    /// <summary>
    /// VM sizing input (if VM deployment).
    /// </summary>
    public VMSizingInput? VMInput { get; set; }

    /// <summary>
    /// Cached result summary for display.
    /// </summary>
    public ConfigurationSummary? Summary { get; set; }

    /// <summary>
    /// Generate a description from the configuration.
    /// </summary>
    public static string GenerateDescription(SavedConfiguration config)
    {
        var parts = new List<string>();

        parts.Add(config.Technology.ToString());

        if (config.DeploymentModel == DeploymentModel.Kubernetes && config.Distribution.HasValue)
        {
            parts.Add(config.Distribution.Value.ToString());
        }
        else
        {
            parts.Add("VMs");
        }

        if (config.Summary != null)
        {
            parts.Add($"{config.Summary.TotalApps} apps");
            if (config.DeploymentModel == DeploymentModel.Kubernetes)
            {
                parts.Add($"{config.Summary.TotalNodes} nodes");
            }
            else
            {
                parts.Add($"{config.Summary.TotalVMs} VMs");
            }
        }

        return string.Join(" | ", parts);
    }
}

/// <summary>
/// Summary of a configuration for quick display.
/// </summary>
public class ConfigurationSummary
{
    public int TotalApps { get; set; }
    public int TotalNodes { get; set; }
    public int TotalVMs { get; set; }
    public int TotalCpu { get; set; }
    public int TotalRam { get; set; }
    public int TotalDisk { get; set; }
    public int EnvironmentCount { get; set; }
}
