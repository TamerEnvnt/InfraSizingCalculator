using System.ComponentModel.DataAnnotations;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Complete input model for K8s sizing calculation
/// </summary>
public class K8sSizingInput : IValidatableObject
{
    [Required(ErrorMessage = "Distribution is required")]
    [EnumDataType(typeof(Distribution), ErrorMessage = "Invalid distribution")]
    public Distribution Distribution { get; set; } = Distribution.OpenShift;

    [Required(ErrorMessage = "Technology is required")]
    [EnumDataType(typeof(Technology), ErrorMessage = "Invalid technology")]
    public Technology Technology { get; set; } = Technology.DotNet;

    [Required(ErrorMessage = "Cluster mode is required")]
    [EnumDataType(typeof(ClusterMode), ErrorMessage = "Invalid cluster mode")]
    public ClusterMode ClusterMode { get; set; } = ClusterMode.MultiCluster;

    /// <summary>
    /// Application counts for Production environments (used as fallback)
    /// </summary>
    [Required(ErrorMessage = "Production apps configuration is required")]
    [ValidateComplexType]
    public AppConfig ProdApps { get; set; } = new();

    /// <summary>
    /// Application counts for Non-Production environments (used as fallback)
    /// In Multi-Cluster mode, non-prod environments use these counts
    /// </summary>
    [Required(ErrorMessage = "Non-production apps configuration is required")]
    [ValidateComplexType]
    public AppConfig NonProdApps { get; set; } = new();

    /// <summary>
    /// Per-environment application counts (takes precedence over ProdApps/NonProdApps)
    /// Allows different app counts for Dev, Test, Stage, Prod, DR
    /// </summary>
    public Dictionary<EnvironmentType, AppConfig>? EnvironmentApps { get; set; }

    /// <summary>
    /// Enabled environments (BR-E002: Production is always enabled)
    /// </summary>
    [Required(ErrorMessage = "Enabled environments is required")]
    public HashSet<EnvironmentType> EnabledEnvironments { get; set; } = new()
    {
        EnvironmentType.Dev,
        EnvironmentType.Test,
        EnvironmentType.Stage,
        EnvironmentType.Prod,
        EnvironmentType.DR
    };

    /// <summary>
    /// Selected environment for PerEnvironment cluster mode
    /// </summary>
    [EnumDataType(typeof(EnvironmentType), ErrorMessage = "Invalid environment type")]
    public EnvironmentType SelectedEnvironment { get; set; } = EnvironmentType.Prod;

    [Required(ErrorMessage = "Replica settings are required")]
    [ValidateComplexType]
    public ReplicaSettings Replicas { get; set; } = new();

    [Required(ErrorMessage = "Headroom settings are required")]
    [ValidateComplexType]
    public HeadroomSettings Headroom { get; set; } = new();

    /// <summary>
    /// BR-H009: When disabled, headroom percentage = 0 for all environments
    /// </summary>
    public bool EnableHeadroom { get; set; } = true;

    [Required(ErrorMessage = "Production overcommit settings are required")]
    [ValidateComplexType]
    public OvercommitSettings ProdOvercommit { get; set; } = new();

    [Required(ErrorMessage = "Non-production overcommit settings are required")]
    [ValidateComplexType]
    public OvercommitSettings NonProdOvercommit { get; set; } = new();

    /// <summary>
    /// Custom node specifications (overrides distribution defaults)
    /// </summary>
    public DistributionConfig? CustomNodeSpecs { get; set; }

    /// <summary>
    /// HA/DR configuration for the cluster(s).
    /// In Multi-Cluster mode, this applies to each cluster.
    /// </summary>
    public K8sHADRConfig HADRConfig { get; set; } = new();

    /// <summary>
    /// Per-environment HA/DR overrides (optional).
    /// Allows different HA/DR settings for Production vs Non-Production.
    /// </summary>
    public Dictionary<EnvironmentType, K8sHADRConfig>? EnvironmentHADRConfigs { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // BR-E002: Production is always enabled
        if (!EnabledEnvironments.Contains(EnvironmentType.Prod))
        {
            yield return new ValidationResult(
                "Production environment must always be enabled (BR-E002)",
                new[] { nameof(EnabledEnvironments) });
        }

        // Validate that at least one environment is enabled
        if (EnabledEnvironments.Count == 0)
        {
            yield return new ValidationResult(
                "At least one environment must be enabled",
                new[] { nameof(EnabledEnvironments) });
        }

        // For PerEnvironment mode, validate the selected environment is enabled
        if (ClusterMode == ClusterMode.PerEnvironment &&
            !EnabledEnvironments.Contains(SelectedEnvironment))
        {
            yield return new ValidationResult(
                "Selected environment must be in the enabled environments list",
                new[] { nameof(SelectedEnvironment) });
        }

        // Validate nested objects
        var nestedValidations = new (object? obj, string name)[]
        {
            (ProdApps, nameof(ProdApps)),
            (NonProdApps, nameof(NonProdApps)),
            (Replicas, nameof(Replicas)),
            (Headroom, nameof(Headroom)),
            (ProdOvercommit, nameof(ProdOvercommit)),
            (NonProdOvercommit, nameof(NonProdOvercommit))
        };

        foreach (var (obj, name) in nestedValidations)
        {
            if (obj is IValidatableObject validatable)
            {
                var context = new ValidationContext(obj);
                foreach (var result in validatable.Validate(context))
                {
                    yield return new ValidationResult(
                        $"{name}: {result.ErrorMessage}",
                        result.MemberNames.Select(m => $"{name}.{m}"));
                }
            }
        }

        // Validate EnvironmentApps if provided
        if (EnvironmentApps != null)
        {
            foreach (var kvp in EnvironmentApps)
            {
                if (kvp.Value is IValidatableObject validatable)
                {
                    var context = new ValidationContext(kvp.Value);
                    foreach (var result in validatable.Validate(context))
                    {
                        yield return new ValidationResult(
                            $"EnvironmentApps[{kvp.Key}]: {result.ErrorMessage}",
                            result.MemberNames.Select(m => $"EnvironmentApps.{kvp.Key}.{m}"));
                    }
                }
            }
        }
    }
}

/// <summary>
/// Marker attribute for complex type validation
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ValidateComplexTypeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        var results = new List<ValidationResult>();
        var context = new ValidationContext(value);
        Validator.TryValidateObject(value, context, results, validateAllProperties: true);

        if (results.Count > 0)
        {
            return new ValidationResult(
                string.Join("; ", results.Select(r => r.ErrorMessage)),
                results.SelectMany(r => r.MemberNames));
        }

        return ValidationResult.Success;
    }
}
