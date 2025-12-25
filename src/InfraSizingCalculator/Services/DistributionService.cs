using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service providing distribution-specific configurations
/// BR-D001 through BR-D008
/// </summary>
public class DistributionService : IDistributionService
{
    private static readonly Dictionary<Distribution, DistributionConfig> Configs = new()
    {
        // BR-D002: OpenShift On-Prem - Red Hat Red #EE0000
        [Distribution.OpenShift] = new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "OpenShift (On-Prem)",
            Vendor = "Red Hat",
            Icon = "🔴",
            BrandColor = "#EE0000",
            Tags = new[] { "enterprise", "on-prem" },
            ProdControlPlane = new NodeSpecs(8, 32, 200),
            NonProdControlPlane = new NodeSpecs(8, 32, 100),
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(8, 32, 200),
            HasInfraNodes = true,           // BR-D007: Only OpenShift
            HasManagedControlPlane = false
        },

        // OpenShift ROSA (Red Hat OpenShift on AWS)
        [Distribution.OpenShiftROSA] = new DistributionConfig
        {
            Distribution = Distribution.OpenShiftROSA,
            Name = "OpenShift ROSA (AWS)",
            Vendor = "Red Hat / AWS",
            Icon = "🔴",
            BrandColor = "#EE0000",
            Tags = new[] { "enterprise", "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,  // Managed control plane
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(8, 32, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = true
        },

        // OpenShift ARO (Azure Red Hat OpenShift)
        [Distribution.OpenShiftARO] = new DistributionConfig
        {
            Distribution = Distribution.OpenShiftARO,
            Name = "OpenShift ARO (Azure)",
            Vendor = "Red Hat / Microsoft",
            Icon = "🔴",
            BrandColor = "#EE0000",
            Tags = new[] { "enterprise", "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,  // Managed control plane
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(16, 64, 200),
            NonProdWorker = new NodeSpecs(8, 32, 100),
            ProdInfra = new NodeSpecs(8, 32, 500),
            NonProdInfra = new NodeSpecs(8, 32, 200),
            HasInfraNodes = true,
            HasManagedControlPlane = true
        },

        // BR-D003: Vanilla Kubernetes - CNCF Blue #326CE5
        [Distribution.Kubernetes] = new DistributionConfig
        {
            Distribution = Distribution.Kubernetes,
            Name = "Vanilla Kubernetes",
            Vendor = "CNCF",
            Icon = "☸️",
            BrandColor = "#326CE5",
            Tags = new[] { "on-prem", "open-source" },
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        },

        // Rancher On-Prem - SUSE/Rancher Blue #0075A8
        [Distribution.Rancher] = new DistributionConfig
        {
            Distribution = Distribution.Rancher,
            Name = "Rancher (On-Prem)",
            Vendor = "SUSE",
            Icon = "🐄",
            BrandColor = "#0075A8",
            Tags = new[] { "on-prem", "enterprise" },
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        },

        // Rancher Hosted (Cloud)
        [Distribution.RancherHosted] = new DistributionConfig
        {
            Distribution = Distribution.RancherHosted,
            Name = "Rancher Hosted (Cloud)",
            Vendor = "SUSE",
            Icon = "🐄",
            BrandColor = "#0075A8",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,  // Managed control plane
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // BR-D004: K3s - Yellow #FFC61C
        [Distribution.K3s] = new DistributionConfig
        {
            Distribution = Distribution.K3s,
            Name = "K3s",
            Vendor = "SUSE",
            Icon = "🚀",
            BrandColor = "#FFC61C",
            Tags = new[] { "on-prem", "lightweight", "edge" },
            ProdControlPlane = new NodeSpecs(2, 4, 50),
            NonProdControlPlane = new NodeSpecs(1, 2, 25),
            ProdWorker = new NodeSpecs(4, 8, 50),
            NonProdWorker = new NodeSpecs(2, 4, 25),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        },

        // MicroK8s - Ubuntu Orange #E95420
        [Distribution.MicroK8s] = new DistributionConfig
        {
            Distribution = Distribution.MicroK8s,
            Name = "MicroK8s",
            Vendor = "Canonical",
            Icon = "🔬",
            BrandColor = "#E95420",
            Tags = new[] { "on-prem", "lightweight" },
            ProdControlPlane = new NodeSpecs(2, 4, 50),
            NonProdControlPlane = new NodeSpecs(1, 2, 25),
            ProdWorker = new NodeSpecs(4, 8, 50),
            NonProdWorker = new NodeSpecs(2, 4, 25),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        },

        // Charmed Kubernetes - Canonical Purple #772953
        [Distribution.Charmed] = new DistributionConfig
        {
            Distribution = Distribution.Charmed,
            Name = "Charmed Kubernetes",
            Vendor = "Canonical",
            Icon = "✨",
            BrandColor = "#772953",
            Tags = new[] { "on-prem", "enterprise" },
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        },

        // Tanzu On-Prem - VMware Blue #1D428A
        [Distribution.Tanzu] = new DistributionConfig
        {
            Distribution = Distribution.Tanzu,
            Name = "VMware Tanzu (On-Prem)",
            Vendor = "Broadcom",
            Icon = "🌀",
            BrandColor = "#1D428A",
            Tags = new[] { "on-prem", "enterprise" },
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        },

        // Tanzu Cloud (VMware Tanzu as a Service - generic)
        [Distribution.TanzuCloud] = new DistributionConfig
        {
            Distribution = Distribution.TanzuCloud,
            Name = "VMware Tanzu Cloud",
            Vendor = "Broadcom",
            Icon = "🌀",
            BrandColor = "#1D428A",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,  // Managed control plane
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // BR-D005, BR-D006: EKS - AWS Orange #FF9900
        [Distribution.EKS] = new DistributionConfig
        {
            Distribution = Distribution.EKS,
            Name = "Amazon EKS",
            Vendor = "AWS",
            Icon = "🟠",
            BrandColor = "#FF9900",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,  // BR-D006: Managed control plane
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true  // BR-D006
        },

        // BR-D005, BR-D006: AKS - Azure Blue #0078D4
        [Distribution.AKS] = new DistributionConfig
        {
            Distribution = Distribution.AKS,
            Name = "Azure AKS",
            Vendor = "Microsoft",
            Icon = "🔵",
            BrandColor = "#0078D4",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,  // BR-D006: Managed control plane
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true  // BR-D006
        },

        // BR-D005, BR-D006: GKE - Google Blue #4285F4
        [Distribution.GKE] = new DistributionConfig
        {
            Distribution = Distribution.GKE,
            Name = "Google GKE",
            Vendor = "Google",
            Icon = "🔷",
            BrandColor = "#4285F4",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,  // BR-D006: Managed control plane
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true  // BR-D006
        },

        // OKE - Oracle Kubernetes Engine (Oracle Cloud) - Oracle Red #C74634
        [Distribution.OKE] = new DistributionConfig
        {
            Distribution = Distribution.OKE,
            Name = "Oracle OKE",
            Vendor = "Oracle",
            Icon = "🔶",
            BrandColor = "#C74634",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,  // BR-D006: Managed control plane
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true  // BR-D006
        },

        // OpenShift Dedicated on GCP
        [Distribution.OpenShiftDedicated] = new DistributionConfig
        {
            Distribution = Distribution.OpenShiftDedicated,
            Name = "OpenShift Dedicated (GCP)",
            Vendor = "Red Hat / Google",
            Icon = "🔴",
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
        },

        // OpenShift on IBM Cloud
        [Distribution.OpenShiftIBM] = new DistributionConfig
        {
            Distribution = Distribution.OpenShiftIBM,
            Name = "OpenShift on IBM Cloud",
            Vendor = "Red Hat / IBM",
            Icon = "🔴",
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
        },

        // Rancher on EKS
        [Distribution.RancherEKS] = new DistributionConfig
        {
            Distribution = Distribution.RancherEKS,
            Name = "Rancher on EKS",
            Vendor = "SUSE / AWS",
            Icon = "🐄",
            BrandColor = "#0075A8",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Rancher on AKS
        [Distribution.RancherAKS] = new DistributionConfig
        {
            Distribution = Distribution.RancherAKS,
            Name = "Rancher on AKS",
            Vendor = "SUSE / Microsoft",
            Icon = "🐄",
            BrandColor = "#0075A8",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Rancher on GKE
        [Distribution.RancherGKE] = new DistributionConfig
        {
            Distribution = Distribution.RancherGKE,
            Name = "Rancher on GKE",
            Vendor = "SUSE / Google",
            Icon = "🐄",
            BrandColor = "#0075A8",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Tanzu on AWS
        [Distribution.TanzuAWS] = new DistributionConfig
        {
            Distribution = Distribution.TanzuAWS,
            Name = "VMware Tanzu on AWS",
            Vendor = "Broadcom / AWS",
            Icon = "🌀",
            BrandColor = "#1D428A",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Tanzu on Azure
        [Distribution.TanzuAzure] = new DistributionConfig
        {
            Distribution = Distribution.TanzuAzure,
            Name = "VMware Tanzu on Azure",
            Vendor = "Broadcom / Microsoft",
            Icon = "🌀",
            BrandColor = "#1D428A",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Tanzu on GCP
        [Distribution.TanzuGCP] = new DistributionConfig
        {
            Distribution = Distribution.TanzuGCP,
            Name = "VMware Tanzu on GCP",
            Vendor = "Broadcom / Google",
            Icon = "🌀",
            BrandColor = "#1D428A",
            Tags = new[] { "cloud", "managed", "enterprise" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // RKE2 - Rancher Kubernetes Engine 2 (On-Prem)
        [Distribution.RKE2] = new DistributionConfig
        {
            Distribution = Distribution.RKE2,
            Name = "RKE2",
            Vendor = "SUSE",
            Icon = "🐄",
            BrandColor = "#0075A8",
            Tags = new[] { "on-prem", "enterprise", "security" },
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false
        },

        // IBM Kubernetes Service (IKS)
        [Distribution.IKS] = new DistributionConfig
        {
            Distribution = Distribution.IKS,
            Name = "IBM Kubernetes Service",
            Vendor = "IBM",
            Icon = "🔷",
            BrandColor = "#054ADA",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Alibaba Container Service for Kubernetes (ACK)
        [Distribution.ACK] = new DistributionConfig
        {
            Distribution = Distribution.ACK,
            Name = "Alibaba ACK",
            Vendor = "Alibaba",
            Icon = "🟠",
            BrandColor = "#FF6A00",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Tencent Kubernetes Engine (TKE)
        [Distribution.TKE] = new DistributionConfig
        {
            Distribution = Distribution.TKE,
            Name = "Tencent TKE",
            Vendor = "Tencent",
            Icon = "🔵",
            BrandColor = "#00A4FF",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Huawei Cloud Container Engine (CCE)
        [Distribution.CCE] = new DistributionConfig
        {
            Distribution = Distribution.CCE,
            Name = "Huawei CCE",
            Vendor = "Huawei",
            Icon = "🔴",
            BrandColor = "#CF0A2C",
            Tags = new[] { "cloud", "managed" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // DigitalOcean Kubernetes (DOKS)
        [Distribution.DOKS] = new DistributionConfig
        {
            Distribution = Distribution.DOKS,
            Name = "DigitalOcean Kubernetes",
            Vendor = "DigitalOcean",
            Icon = "🔵",
            BrandColor = "#0080FF",
            Tags = new[] { "cloud", "managed", "developer" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Linode Kubernetes Engine (LKE)
        [Distribution.LKE] = new DistributionConfig
        {
            Distribution = Distribution.LKE,
            Name = "Linode/Akamai LKE",
            Vendor = "Akamai",
            Icon = "🟢",
            BrandColor = "#00A95C",
            Tags = new[] { "cloud", "managed", "developer" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Vultr Kubernetes Engine (VKE)
        [Distribution.VKE] = new DistributionConfig
        {
            Distribution = Distribution.VKE,
            Name = "Vultr VKE",
            Vendor = "Vultr",
            Icon = "🔵",
            BrandColor = "#007BFC",
            Tags = new[] { "cloud", "managed", "developer" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Hetzner Kubernetes (K3s/RKE2 based)
        [Distribution.HetznerK8s] = new DistributionConfig
        {
            Distribution = Distribution.HetznerK8s,
            Name = "Hetzner Kubernetes",
            Vendor = "Hetzner",
            Icon = "🔴",
            BrandColor = "#D50C2D",
            Tags = new[] { "cloud", "developer", "cost-effective" },
            ProdControlPlane = new NodeSpecs(2, 4, 50),
            NonProdControlPlane = new NodeSpecs(1, 2, 25),
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = false  // Uses K3s/RKE2
        },

        // OVHcloud Managed Kubernetes
        [Distribution.OVHKubernetes] = new DistributionConfig
        {
            Distribution = Distribution.OVHKubernetes,
            Name = "OVHcloud Kubernetes",
            Vendor = "OVH",
            Icon = "🔵",
            BrandColor = "#000E9C",
            Tags = new[] { "cloud", "managed", "developer" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        },

        // Scaleway Kapsule
        [Distribution.ScalewayKapsule] = new DistributionConfig
        {
            Distribution = Distribution.ScalewayKapsule,
            Name = "Scaleway Kapsule",
            Vendor = "Scaleway",
            Icon = "🟣",
            BrandColor = "#4F0599",
            Tags = new[] { "cloud", "managed", "developer" },
            ProdControlPlane = NodeSpecs.Zero,
            NonProdControlPlane = NodeSpecs.Zero,
            ProdWorker = new NodeSpecs(4, 16, 100),
            NonProdWorker = new NodeSpecs(2, 8, 50),
            HasInfraNodes = false,
            HasManagedControlPlane = true
        }
    };

    public DistributionConfig GetConfig(Distribution distribution)
    {
        return Configs.TryGetValue(distribution, out var config)
            ? config
            : Configs[Distribution.OpenShift]; // Default fallback
    }

    public IEnumerable<DistributionConfig> GetAll()
    {
        return Configs.Values;
    }

    public IEnumerable<DistributionConfig> GetByTag(string tag)
    {
        return Configs.Values.Where(c => c.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
    }
}
