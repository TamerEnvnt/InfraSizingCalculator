using Microsoft.AspNetCore.Mvc;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class DistributionsController : ControllerBase
{
    private readonly IDistributionService _distributionService;

    public DistributionsController(IDistributionService distributionService)
    {
        _distributionService = distributionService;
    }

    /// <summary>
    /// Get all available Kubernetes distributions
    /// </summary>
    /// <returns>List of all distribution configurations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DistributionInfo>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<DistributionInfo>> GetAll()
    {
        var distributions = Enum.GetValues<Distribution>()
            .Select(d =>
            {
                var config = _distributionService.GetConfig(d);
                return new DistributionInfo
                {
                    Distribution = d,
                    Name = GetDistributionName(d),
                    Category = GetDistributionCategory(d),
                    HasManagedControlPlane = config.HasManagedControlPlane,
                    HasInfraNodes = config.HasInfraNodes,
                    ProdWorkerSpecs = config.ProdWorker,
                    NonProdWorkerSpecs = config.NonProdWorker
                };
            })
            .ToList();

        return Ok(distributions);
    }

    /// <summary>
    /// Get a specific distribution configuration
    /// </summary>
    /// <param name="distribution">The distribution to get</param>
    /// <returns>Distribution configuration</returns>
    [HttpGet("{distribution}")]
    [ProducesResponseType(typeof(DistributionInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<DistributionInfo> Get(Distribution distribution)
    {
        try
        {
            var config = _distributionService.GetConfig(distribution);
            return Ok(new DistributionInfo
            {
                Distribution = distribution,
                Name = GetDistributionName(distribution),
                Category = GetDistributionCategory(distribution),
                HasManagedControlPlane = config.HasManagedControlPlane,
                HasInfraNodes = config.HasInfraNodes,
                ProdWorkerSpecs = config.ProdWorker,
                NonProdWorkerSpecs = config.NonProdWorker
            });
        }
        catch (ArgumentException)
        {
            return NotFound($"Distribution '{distribution}' not found");
        }
    }

    private static string GetDistributionName(Distribution distribution) => distribution switch
    {
        Distribution.OpenShift => "Red Hat OpenShift",
        Distribution.Kubernetes => "Vanilla Kubernetes",
        Distribution.Rancher => "Rancher (RKE2)",
        Distribution.K3s => "K3s (Lightweight)",
        Distribution.MicroK8s => "MicroK8s",
        Distribution.Charmed => "Charmed Kubernetes",
        Distribution.Tanzu => "VMware Tanzu",
        Distribution.EKS => "Amazon EKS",
        Distribution.AKS => "Azure AKS",
        Distribution.GKE => "Google GKE",
        _ => distribution.ToString()
    };

    private static string GetDistributionCategory(Distribution distribution) => distribution switch
    {
        Distribution.OpenShift or Distribution.Tanzu => "enterprise",
        Distribution.EKS or Distribution.AKS or Distribution.GKE => "managed",
        Distribution.K3s or Distribution.MicroK8s => "lightweight",
        _ => "standard"
    };
}

/// <summary>
/// Distribution information DTO
/// </summary>
public class DistributionInfo
{
    public Distribution Distribution { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool HasManagedControlPlane { get; set; }
    public bool HasInfraNodes { get; set; }
    public NodeSpecs ProdWorkerSpecs { get; set; } = default!;
    public NodeSpecs NonProdWorkerSpecs { get; set; } = default!;
}
