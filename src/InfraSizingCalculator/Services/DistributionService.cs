using InfraSizingCalculator.Data;
using InfraSizingCalculator.Data.Entities;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Database-backed service providing distribution-specific configurations.
/// Loads configurations from database at startup and caches them in memory.
/// BR-D001 through BR-D008
/// </summary>
public class DistributionService : IDistributionService
{
    private readonly Dictionary<Distribution, DistributionConfig> _configs;
    private readonly ILogger<DistributionService>? _logger;

    /// <summary>
    /// Creates a DistributionService that loads configurations from the database.
    /// Used by dependency injection.
    /// </summary>
    public DistributionService(IServiceProvider serviceProvider, ILogger<DistributionService> logger)
    {
        _logger = logger;
        _configs = new Dictionary<Distribution, DistributionConfig>();

        // Load configurations from database
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

            var entities = dbContext.DistributionConfigs
                .Where(d => d.IsActive)
                .OrderBy(d => d.SortOrder)
                .ToList();

            foreach (var entity in entities)
            {
                if (Enum.TryParse<Distribution>(entity.DistributionKey, out var distribution))
                {
                    _configs[distribution] = MapEntityToConfig(entity, distribution);
                }
                else
                {
                    _logger.LogWarning(
                        "Unknown distribution key in database: {Key}",
                        entity.DistributionKey);
                }
            }

            _logger.LogInformation(
                "Loaded {Count} distribution configurations from database",
                _configs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load distributions from database, using fallback");
            LoadFallbackConfigurations();
        }

        // Ensure we have at least the fallback if database is empty
        if (_configs.Count == 0)
        {
            _logger.LogWarning("No distributions loaded from database, using fallback configurations");
            LoadFallbackConfigurations();
        }
    }

    /// <summary>
    /// Creates a DistributionService with fallback (hardcoded) configurations.
    /// Used for testing and when database is unavailable.
    /// </summary>
    public DistributionService()
    {
        _logger = null;
        _configs = new Dictionary<Distribution, DistributionConfig>();
        LoadAllDistributions();
    }

    /// <summary>
    /// Creates a test instance with fallback configurations.
    /// </summary>
    public static DistributionService CreateForTesting() => new();

    /// <summary>
    /// Loads all distribution configurations (hardcoded fallback).
    /// This mirrors the database seed data for consistency.
    /// </summary>
    private void LoadAllDistributions()
    {
        // OpenShift family
        _configs[Distribution.OpenShift] = new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "OpenShift (On-Prem)",
            Vendor = "Red Hat",
            Icon = "openshift",
            BrandColor = "#EE0000",
            Tags = new[] { "enterprise", "on-prem" },
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(8, 32, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(8, 32, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = false
        };

        _configs[Distribution.OpenShiftROSA] = new DistributionConfig
        {
            Distribution = Distribution.OpenShiftROSA,
            Name = "OpenShift ROSA (AWS)",
            Vendor = "Red Hat / AWS",
            Icon = "openshift",
            BrandColor = "#EE0000",
            Tags = new[] { "enterprise", "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(8, 32, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = true
        };

        _configs[Distribution.OpenShiftARO] = new DistributionConfig
        {
            Distribution = Distribution.OpenShiftARO,
            Name = "OpenShift ARO (Azure)",
            Vendor = "Red Hat / Microsoft",
            Icon = "openshift",
            BrandColor = "#EE0000",
            Tags = new[] { "enterprise", "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(8, 32, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = true
        };

        _configs[Distribution.OpenShiftDedicated] = new DistributionConfig
        {
            Distribution = Distribution.OpenShiftDedicated,
            Name = "OpenShift Dedicated (GCP)",
            Vendor = "Red Hat / Google",
            Icon = "openshift",
            BrandColor = "#EE0000",
            Tags = new[] { "enterprise", "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(8, 32, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = true
        };

        _configs[Distribution.OpenShiftIBM] = new DistributionConfig
        {
            Distribution = Distribution.OpenShiftIBM,
            Name = "OpenShift on IBM Cloud",
            Vendor = "Red Hat / IBM",
            Icon = "openshift",
            BrandColor = "#EE0000",
            Tags = new[] { "enterprise", "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(8, 32, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = true
        };

        // Vanilla Kubernetes
        _configs[Distribution.Kubernetes] = new DistributionConfig
        {
            Distribution = Distribution.Kubernetes,
            Name = "Vanilla Kubernetes",
            Vendor = "CNCF",
            Icon = "k8s",
            BrandColor = "#326CE5",
            Tags = new[] { "on-prem", "open-source" },
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        };

        // Rancher family
        _configs[Distribution.Rancher] = new DistributionConfig
        {
            Distribution = Distribution.Rancher,
            Name = "Rancher (On-Prem)",
            Vendor = "SUSE",
            Icon = "rancher",
            BrandColor = "#0075A8",
            Tags = new[] { "on-prem", "enterprise" },
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        };

        _configs[Distribution.RancherHosted] = new DistributionConfig
        {
            Distribution = Distribution.RancherHosted,
            Name = "Rancher Hosted (Cloud)",
            Vendor = "SUSE",
            Icon = "rancher",
            BrandColor = "#0075A8",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.RancherEKS] = new DistributionConfig
        {
            Distribution = Distribution.RancherEKS,
            Name = "Rancher on EKS",
            Vendor = "SUSE / AWS",
            Icon = "rancher",
            BrandColor = "#0075A8",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.RancherAKS] = new DistributionConfig
        {
            Distribution = Distribution.RancherAKS,
            Name = "Rancher on AKS",
            Vendor = "SUSE / Microsoft",
            Icon = "rancher",
            BrandColor = "#0075A8",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.RancherGKE] = new DistributionConfig
        {
            Distribution = Distribution.RancherGKE,
            Name = "Rancher on GKE",
            Vendor = "SUSE / Google",
            Icon = "rancher",
            BrandColor = "#0075A8",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.RKE2] = new DistributionConfig
        {
            Distribution = Distribution.RKE2,
            Name = "RKE2",
            Vendor = "SUSE",
            Icon = "rancher",
            BrandColor = "#0075A8",
            Tags = new[] { "on-prem", "enterprise", "security" },
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        };

        // K3s
        _configs[Distribution.K3s] = new DistributionConfig
        {
            Distribution = Distribution.K3s,
            Name = "K3s",
            Vendor = "SUSE",
            Icon = "k3s",
            BrandColor = "#FFC61C",
            Tags = new[] { "on-prem", "lightweight", "edge" },
            ProdControlPlane = new NodeSpecs(2, 4, 50),
            NonProdControlPlane = new NodeSpecs(1, 2, 25),
            ProdWorker = new NodeSpecs(4, 8, 50),
            NonProdWorker = new NodeSpecs(2, 4, 25),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        };

        // Canonical
        _configs[Distribution.MicroK8s] = new DistributionConfig
        {
            Distribution = Distribution.MicroK8s,
            Name = "MicroK8s",
            Vendor = "Canonical",
            Icon = "microk8s",
            BrandColor = "#E95420",
            Tags = new[] { "on-prem", "lightweight" },
            ProdControlPlane = new NodeSpecs(2, 4, 50),
            NonProdControlPlane = new NodeSpecs(1, 2, 25),
            ProdWorker = new NodeSpecs(4, 8, 50),
            NonProdWorker = new NodeSpecs(2, 4, 25),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        };

        _configs[Distribution.Charmed] = new DistributionConfig
        {
            Distribution = Distribution.Charmed,
            Name = "Charmed Kubernetes",
            Vendor = "Canonical",
            Icon = "charmed",
            BrandColor = "#772953",
            Tags = new[] { "on-prem", "enterprise" },
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        };

        // VMware Tanzu family
        _configs[Distribution.Tanzu] = new DistributionConfig
        {
            Distribution = Distribution.Tanzu,
            Name = "VMware Tanzu (On-Prem)",
            Vendor = "Broadcom",
            Icon = "tanzu",
            BrandColor = "#1D428A",
            Tags = new[] { "on-prem", "enterprise" },
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        };

        _configs[Distribution.TanzuCloud] = new DistributionConfig
        {
            Distribution = Distribution.TanzuCloud,
            Name = "VMware Tanzu Cloud",
            Vendor = "Broadcom",
            Icon = "tanzu",
            BrandColor = "#1D428A",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.TanzuAWS] = new DistributionConfig
        {
            Distribution = Distribution.TanzuAWS,
            Name = "VMware Tanzu on AWS",
            Vendor = "Broadcom / AWS",
            Icon = "tanzu",
            BrandColor = "#1D428A",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.TanzuAzure] = new DistributionConfig
        {
            Distribution = Distribution.TanzuAzure,
            Name = "VMware Tanzu on Azure",
            Vendor = "Broadcom / Microsoft",
            Icon = "tanzu",
            BrandColor = "#1D428A",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.TanzuGCP] = new DistributionConfig
        {
            Distribution = Distribution.TanzuGCP,
            Name = "VMware Tanzu on GCP",
            Vendor = "Broadcom / Google",
            Icon = "tanzu",
            BrandColor = "#1D428A",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        // Major cloud providers
        _configs[Distribution.EKS] = new DistributionConfig
        {
            Distribution = Distribution.EKS,
            Name = "Amazon EKS",
            Vendor = "AWS",
            Icon = "eks",
            BrandColor = "#FF9900",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.AKS] = new DistributionConfig
        {
            Distribution = Distribution.AKS,
            Name = "Azure AKS",
            Vendor = "Microsoft",
            Icon = "aks",
            BrandColor = "#0078D4",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.GKE] = new DistributionConfig
        {
            Distribution = Distribution.GKE,
            Name = "Google GKE",
            Vendor = "Google",
            Icon = "gke",
            BrandColor = "#4285F4",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.OKE] = new DistributionConfig
        {
            Distribution = Distribution.OKE,
            Name = "Oracle OKE",
            Vendor = "Oracle",
            Icon = "oke",
            BrandColor = "#C74634",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.IKS] = new DistributionConfig
        {
            Distribution = Distribution.IKS,
            Name = "IBM Kubernetes Service",
            Vendor = "IBM",
            Icon = "iks",
            BrandColor = "#054ADA",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        // Asian cloud providers
        _configs[Distribution.ACK] = new DistributionConfig
        {
            Distribution = Distribution.ACK,
            Name = "Alibaba ACK",
            Vendor = "Alibaba",
            Icon = "ack",
            BrandColor = "#FF6A00",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.TKE] = new DistributionConfig
        {
            Distribution = Distribution.TKE,
            Name = "Tencent TKE",
            Vendor = "Tencent",
            Icon = "tke",
            BrandColor = "#00A4FF",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.CCE] = new DistributionConfig
        {
            Distribution = Distribution.CCE,
            Name = "Huawei CCE",
            Vendor = "Huawei",
            Icon = "cce",
            BrandColor = "#CF0A2C",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        // Developer-focused providers
        _configs[Distribution.DOKS] = new DistributionConfig
        {
            Distribution = Distribution.DOKS,
            Name = "DigitalOcean Kubernetes",
            Vendor = "DigitalOcean",
            Icon = "doks",
            BrandColor = "#0080FF",
            Tags = new[] { "cloud", "managed", "developer" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.LKE] = new DistributionConfig
        {
            Distribution = Distribution.LKE,
            Name = "Linode/Akamai LKE",
            Vendor = "Akamai",
            Icon = "lke",
            BrandColor = "#00A95C",
            Tags = new[] { "cloud", "managed", "developer" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.VKE] = new DistributionConfig
        {
            Distribution = Distribution.VKE,
            Name = "Vultr VKE",
            Vendor = "Vultr",
            Icon = "vke",
            BrandColor = "#007BFC",
            Tags = new[] { "cloud", "managed", "developer" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        // European providers
        _configs[Distribution.HetznerK8s] = new DistributionConfig
        {
            Distribution = Distribution.HetznerK8s,
            Name = "Hetzner Kubernetes",
            Vendor = "Hetzner",
            Icon = "hetzner",
            BrandColor = "#D50C2D",
            Tags = new[] { "cloud", "developer", "cost-effective" },
            ProdControlPlane = new NodeSpecs(2, 4, 50),
            NonProdControlPlane = new NodeSpecs(1, 2, 25),
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        };

        _configs[Distribution.OVHKubernetes] = new DistributionConfig
        {
            Distribution = Distribution.OVHKubernetes,
            Name = "OVHcloud Kubernetes",
            Vendor = "OVH",
            Icon = "ovh",
            BrandColor = "#000E9C",
            Tags = new[] { "cloud", "managed", "developer" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };

        _configs[Distribution.ScalewayKapsule] = new DistributionConfig
        {
            Distribution = Distribution.ScalewayKapsule,
            Name = "Scaleway Kapsule",
            Vendor = "Scaleway",
            Icon = "scaleway",
            BrandColor = "#4F0599",
            Tags = new[] { "cloud", "managed", "developer" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        };
    }

    public DistributionConfig GetConfig(Distribution distribution)
    {
        return _configs.TryGetValue(distribution, out var config)
            ? config
            : _configs.GetValueOrDefault(Distribution.OpenShift)
              ?? CreateDefaultConfig(distribution);
    }

    public IEnumerable<DistributionConfig> GetAll()
    {
        return _configs.Values;
    }

    public IEnumerable<DistributionConfig> GetByTag(string tag)
    {
        return _configs.Values.Where(c => c.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Maps a database entity to a domain model.
    /// </summary>
    private static DistributionConfig MapEntityToConfig(DistributionConfigEntity entity, Distribution distribution)
    {
        return new DistributionConfig
        {
            Distribution = distribution,
            Name = entity.Name,
            Vendor = entity.Vendor,
            Icon = entity.Icon,
            BrandColor = entity.BrandColor,
            Tags = entity.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            ProdControlPlane = new NodeSpecs(entity.ProdControlPlaneCpu, entity.ProdControlPlaneRam, entity.ProdControlPlaneDisk),
            NonProdControlPlane = new NodeSpecs(entity.NonProdControlPlaneCpu, entity.NonProdControlPlaneRam, entity.NonProdControlPlaneDisk),
            ProdWorker = new NodeSpecs(entity.ProdWorkerCpu, entity.ProdWorkerRam, entity.ProdWorkerDisk),
            NonProdWorker = new NodeSpecs(entity.NonProdWorkerCpu, entity.NonProdWorkerRam, entity.NonProdWorkerDisk),
            ProdInfra = new NodeSpecs(entity.ProdInfraCpu, entity.ProdInfraRam, entity.ProdInfraDisk),
            NonProdInfra = new NodeSpecs(entity.NonProdInfraCpu, entity.NonProdInfraRam, entity.NonProdInfraDisk),
            HasInfraNodes = entity.HasInfraNodes,
            HasManagedControlPlane = entity.HasManagedControlPlane
        };
    }

    /// <summary>
    /// Creates a default configuration for unknown distributions.
    /// </summary>
    private static DistributionConfig CreateDefaultConfig(Distribution distribution)
    {
        return new DistributionConfig
        {
            Distribution = distribution,
            Name = distribution.ToString(),
            Vendor = "Unknown",
            Icon = "k8s",
            BrandColor = "#326CE5",
            Tags = Array.Empty<string>(),
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        };
    }

    /// <summary>
    /// Loads fallback configurations when database is unavailable.
    /// </summary>
    private void LoadFallbackConfigurations()
    {
        LoadAllDistributions();
        _logger?.LogInformation(
            "Loaded {Count} fallback distribution configurations",
            _configs.Count);
    }
}
