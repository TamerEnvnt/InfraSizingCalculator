using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services.Pricing;

/// <summary>
/// Default pricing data for all providers (used as fallback when API unavailable)
/// Prices based on public pricing as of 2025
/// </summary>
public static class DefaultPricingData
{
    /// <summary>
    /// Get default AWS pricing
    /// </summary>
    public static PricingModel GetAWSPricing(string region = "us-east-1")
    {
        return new PricingModel
        {
            Provider = CloudProvider.AWS,
            Region = region,
            RegionDisplayName = CloudRegions.AWSRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (AWS Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                // Based on m6i.xlarge pricing (~$0.192/hour for 4 vCPU, 16GB)
                CpuPerHour = 0.048m,      // $0.192 / 4 vCPU
                RamGBPerHour = 0.006m,    // ~$0.006/GB/hour
                ManagedControlPlanePerHour = 0.10m, // EKS control plane
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["t3.medium"] = 0.0416m,    // 2 vCPU, 4 GB
                    ["t3.large"] = 0.0832m,     // 2 vCPU, 8 GB
                    ["m6i.large"] = 0.096m,     // 2 vCPU, 8 GB
                    ["m6i.xlarge"] = 0.192m,    // 4 vCPU, 16 GB
                    ["m6i.2xlarge"] = 0.384m,   // 8 vCPU, 32 GB
                    ["m6i.4xlarge"] = 0.768m,   // 16 vCPU, 64 GB
                    ["c6i.xlarge"] = 0.17m,     // 4 vCPU, 8 GB (compute optimized)
                    ["r6i.xlarge"] = 0.252m     // 4 vCPU, 32 GB (memory optimized)
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.10m,          // gp3 SSD
                HddPerGBMonth = 0.045m,         // st1 HDD
                ObjectStoragePerGBMonth = 0.023m, // S3 Standard
                BackupPerGBMonth = 0.05m,       // EBS Snapshots
                RegistryPerGBMonth = 0.10m      // ECR
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.09m,            // First 10TB
                LoadBalancerPerHour = 0.0225m,  // ALB
                NatGatewayPerHour = 0.045m,
                VpnPerHour = 0.05m,
                PublicIpPerHour = 0.005m
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,          // Red Hat Standard
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 3,
                BusinessSupportPercent = 10,
                EnterpriseSupportPercent = 15
            }
        };
    }

    /// <summary>
    /// Get default Azure pricing
    /// </summary>
    public static PricingModel GetAzurePricing(string region = "eastus")
    {
        return new PricingModel
        {
            Provider = CloudProvider.Azure,
            Region = region,
            RegionDisplayName = CloudRegions.AzureRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (Azure Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                // Based on Standard_D4s_v5 (~$0.192/hour for 4 vCPU, 16GB)
                CpuPerHour = 0.048m,
                RamGBPerHour = 0.006m,
                ManagedControlPlanePerHour = 0.10m, // AKS is free, but assume some cost
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["Standard_B2ms"] = 0.0832m,     // 2 vCPU, 8 GB
                    ["Standard_D2s_v5"] = 0.096m,    // 2 vCPU, 8 GB
                    ["Standard_D4s_v5"] = 0.192m,    // 4 vCPU, 16 GB
                    ["Standard_D8s_v5"] = 0.384m,    // 8 vCPU, 32 GB
                    ["Standard_D16s_v5"] = 0.768m,   // 16 vCPU, 64 GB
                    ["Standard_F4s_v2"] = 0.169m,    // 4 vCPU, 8 GB (compute)
                    ["Standard_E4s_v5"] = 0.252m     // 4 vCPU, 32 GB (memory)
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.10m,          // Premium SSD P10
                HddPerGBMonth = 0.04m,          // Standard HDD
                ObjectStoragePerGBMonth = 0.0184m, // Blob Storage Hot
                BackupPerGBMonth = 0.05m,
                RegistryPerGBMonth = 0.10m      // ACR
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.087m,
                LoadBalancerPerHour = 0.025m,   // Standard LB
                NatGatewayPerHour = 0.045m,
                VpnPerHour = 0.05m,
                PublicIpPerHour = 0.004m
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 3,
                BusinessSupportPercent = 10,
                EnterpriseSupportPercent = 15
            }
        };
    }

    /// <summary>
    /// Get default GCP pricing
    /// </summary>
    public static PricingModel GetGCPPricing(string region = "us-central1")
    {
        return new PricingModel
        {
            Provider = CloudProvider.GCP,
            Region = region,
            RegionDisplayName = CloudRegions.GCPRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (GCP Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                // Based on n2-standard-4 (~$0.194/hour for 4 vCPU, 16GB)
                CpuPerHour = 0.0485m,
                RamGBPerHour = 0.006m,
                ManagedControlPlanePerHour = 0.10m, // GKE Autopilot management fee
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["e2-medium"] = 0.0335m,       // 2 vCPU, 4 GB
                    ["e2-standard-2"] = 0.067m,    // 2 vCPU, 8 GB
                    ["n2-standard-4"] = 0.194m,    // 4 vCPU, 16 GB
                    ["n2-standard-8"] = 0.388m,    // 8 vCPU, 32 GB
                    ["n2-standard-16"] = 0.776m,   // 16 vCPU, 64 GB
                    ["c2-standard-4"] = 0.209m,    // 4 vCPU, 16 GB (compute)
                    ["n2-highmem-4"] = 0.262m      // 4 vCPU, 32 GB (memory)
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.17m,          // SSD PD
                HddPerGBMonth = 0.04m,          // Standard PD
                ObjectStoragePerGBMonth = 0.020m, // GCS Standard
                BackupPerGBMonth = 0.05m,
                RegistryPerGBMonth = 0.026m     // Artifact Registry
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.12m,            // Premium tier
                LoadBalancerPerHour = 0.025m,
                NatGatewayPerHour = 0.044m,
                VpnPerHour = 0.05m,
                PublicIpPerHour = 0.004m
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 3,
                BusinessSupportPercent = 9,
                EnterpriseSupportPercent = 12
            }
        };
    }

    /// <summary>
    /// Get default Oracle OCI pricing
    /// </summary>
    public static PricingModel GetOCIPricing(string region = "us-ashburn-1")
    {
        return new PricingModel
        {
            Provider = CloudProvider.OCI,
            Region = region,
            RegionDisplayName = CloudRegions.OCIRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (OCI Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                // OCI is typically 40-50% cheaper than AWS
                // Based on VM.Standard.E4.Flex
                CpuPerHour = 0.025m,           // OCPU ~= 2 vCPU
                RamGBPerHour = 0.0015m,
                ManagedControlPlanePerHour = 0m, // OKE control plane is free
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["VM.Standard.E4.Flex"] = 0.025m,     // per OCPU
                    ["VM.Standard3.Flex"] = 0.054m,      // per OCPU
                    ["VM.Optimized3.Flex"] = 0.054m,     // compute optimized
                    ["VM.Standard.A1.Flex"] = 0.01m      // ARM-based (Ampere)
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.0255m,       // Block Volume Performance
                HddPerGBMonth = 0.0255m,       // Block Volume Balanced
                ObjectStoragePerGBMonth = 0.0255m, // Object Storage Standard
                BackupPerGBMonth = 0.05m,
                RegistryPerGBMonth = 0.0255m   // Container Registry
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.0085m,         // OCI has very low egress
                LoadBalancerPerHour = 0.0125m, // Flexible LB
                NatGatewayPerHour = 0.0126m,   // NAT Gateway
                VpnPerHour = 0.05m,
                PublicIpPerHour = 0m           // Reserved public IPs are free
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 0,   // Included in OCI
                BusinessSupportPercent = 0,    // Included in Premier
                EnterpriseSupportPercent = 10
            }
        };
    }

    /// <summary>
    /// Get default IBM Cloud pricing
    /// </summary>
    public static PricingModel GetIBMPricing(string region = "us-south")
    {
        return new PricingModel
        {
            Provider = CloudProvider.IBM,
            Region = region,
            RegionDisplayName = CloudRegions.IBMRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (IBM Cloud Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.046m,
                RamGBPerHour = 0.006m,
                ManagedControlPlanePerHour = 0.10m, // IKS management fee
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["bx2-4x16"] = 0.188m,     // 4 vCPU, 16 GB
                    ["bx2-8x32"] = 0.376m,     // 8 vCPU, 32 GB
                    ["bx2-16x64"] = 0.752m,    // 16 vCPU, 64 GB
                    ["cx2-4x8"] = 0.156m,      // 4 vCPU, 8 GB (compute)
                    ["mx2-4x32"] = 0.244m      // 4 vCPU, 32 GB (memory)
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.10m,
                HddPerGBMonth = 0.04m,
                ObjectStoragePerGBMonth = 0.022m,
                BackupPerGBMonth = 0.05m,
                RegistryPerGBMonth = 0.10m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.09m,
                LoadBalancerPerHour = 0.025m,
                NatGatewayPerHour = 0.045m,
                VpnPerHour = 0.05m,
                PublicIpPerHour = 0.004m
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 3,
                BusinessSupportPercent = 10,
                EnterpriseSupportPercent = 15
            }
        };
    }

    /// <summary>
    /// Get default Alibaba Cloud pricing
    /// </summary>
    public static PricingModel GetAlibabaPricing(string region = "cn-hangzhou")
    {
        return new PricingModel
        {
            Provider = CloudProvider.Alibaba,
            Region = region,
            RegionDisplayName = CloudRegions.AlibabaRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (Alibaba Cloud Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.042m,
                RamGBPerHour = 0.005m,
                ManagedControlPlanePerHour = 0.07m, // ACK management fee
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["ecs.g6.xlarge"] = 0.168m,    // 4 vCPU, 16 GB
                    ["ecs.g6.2xlarge"] = 0.336m,   // 8 vCPU, 32 GB
                    ["ecs.c6.xlarge"] = 0.145m,    // 4 vCPU, 8 GB (compute)
                    ["ecs.r6.xlarge"] = 0.21m      // 4 vCPU, 32 GB (memory)
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.09m,
                HddPerGBMonth = 0.035m,
                ObjectStoragePerGBMonth = 0.02m,
                BackupPerGBMonth = 0.04m,
                RegistryPerGBMonth = 0.08m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.08m,
                LoadBalancerPerHour = 0.02m,
                NatGatewayPerHour = 0.04m,
                VpnPerHour = 0.04m,
                PublicIpPerHour = 0.003m
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 3,
                BusinessSupportPercent = 8,
                EnterpriseSupportPercent = 12
            }
        };
    }

    /// <summary>
    /// Get default Tencent Cloud pricing
    /// </summary>
    public static PricingModel GetTencentPricing(string region = "ap-guangzhou")
    {
        return new PricingModel
        {
            Provider = CloudProvider.Tencent,
            Region = region,
            RegionDisplayName = region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (Tencent Cloud Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.040m,
                RamGBPerHour = 0.005m,
                ManagedControlPlanePerHour = 0.06m,
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["S5.LARGE8"] = 0.16m,      // 4 vCPU, 8 GB
                    ["S5.LARGE16"] = 0.18m,     // 4 vCPU, 16 GB
                    ["S5.2XLARGE32"] = 0.36m    // 8 vCPU, 32 GB
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.085m,
                HddPerGBMonth = 0.03m,
                ObjectStoragePerGBMonth = 0.018m,
                BackupPerGBMonth = 0.04m,
                RegistryPerGBMonth = 0.07m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.07m,
                LoadBalancerPerHour = 0.018m,
                NatGatewayPerHour = 0.035m,
                VpnPerHour = 0.04m,
                PublicIpPerHour = 0.003m
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 3,
                BusinessSupportPercent = 8,
                EnterpriseSupportPercent = 12
            }
        };
    }

    /// <summary>
    /// Get default Huawei Cloud pricing
    /// </summary>
    public static PricingModel GetHuaweiPricing(string region = "cn-north-4")
    {
        return new PricingModel
        {
            Provider = CloudProvider.Huawei,
            Region = region,
            RegionDisplayName = region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (Huawei Cloud Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.038m,
                RamGBPerHour = 0.005m,
                ManagedControlPlanePerHour = 0.06m,
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["s6.xlarge.2"] = 0.15m,    // 4 vCPU, 8 GB
                    ["s6.xlarge.4"] = 0.18m,    // 4 vCPU, 16 GB
                    ["s6.2xlarge.4"] = 0.36m    // 8 vCPU, 32 GB
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.08m,
                HddPerGBMonth = 0.03m,
                ObjectStoragePerGBMonth = 0.018m,
                BackupPerGBMonth = 0.04m,
                RegistryPerGBMonth = 0.07m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.065m,
                LoadBalancerPerHour = 0.016m,
                NatGatewayPerHour = 0.03m,
                VpnPerHour = 0.04m,
                PublicIpPerHour = 0.003m
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 3,
                BusinessSupportPercent = 8,
                EnterpriseSupportPercent = 12
            }
        };
    }

    /// <summary>
    /// Get default DigitalOcean pricing
    /// </summary>
    public static PricingModel GetDigitalOceanPricing(string region = "nyc1")
    {
        return new PricingModel
        {
            Provider = CloudProvider.DigitalOcean,
            Region = region,
            RegionDisplayName = CloudRegions.DigitalOceanRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (DigitalOcean Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                // DigitalOcean is simpler with fixed droplet sizes
                CpuPerHour = 0.03m,
                RamGBPerHour = 0.0075m,
                ManagedControlPlanePerHour = 0m, // DOKS control plane is free
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["s-2vcpu-4gb"] = 0.03m,      // $24/month
                    ["s-4vcpu-8gb"] = 0.06m,      // $48/month
                    ["g-4vcpu-16gb"] = 0.126m,    // $96/month (General Purpose)
                    ["gd-8vcpu-32gb"] = 0.252m,   // $192/month
                    ["c-4"] = 0.084m,             // $63/month (CPU-Optimized)
                    ["m-4vcpu-32gb"] = 0.168m     // $126/month (Memory-Optimized)
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.10m,          // Block Storage
                HddPerGBMonth = 0.10m,          // Same as SSD on DO
                ObjectStoragePerGBMonth = 0.02m, // Spaces
                BackupPerGBMonth = 0.05m,
                RegistryPerGBMonth = 0.02m      // Container Registry
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.01m,            // Very competitive egress
                LoadBalancerPerHour = 0.015m,   // ~$12/month
                NatGatewayPerHour = 0m,         // No separate NAT cost
                VpnPerHour = 0m,
                PublicIpPerHour = 0.006m        // ~$5/month reserved IP
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 0,
                BusinessSupportPercent = 5,
                EnterpriseSupportPercent = 10
            }
        };
    }

    /// <summary>
    /// Get default Linode/Akamai pricing
    /// </summary>
    public static PricingModel GetLinodePricing(string region = "us-east")
    {
        return new PricingModel
        {
            Provider = CloudProvider.Linode,
            Region = region,
            RegionDisplayName = CloudRegions.LinodeRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (Linode/Akamai Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.025m,
                RamGBPerHour = 0.006m,
                ManagedControlPlanePerHour = 0m, // LKE control plane is free
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["g6-standard-2"] = 0.018m,   // 2 vCPU, 4 GB - $12/month
                    ["g6-standard-4"] = 0.036m,   // 4 vCPU, 8 GB - $24/month
                    ["g6-dedicated-4"] = 0.054m,  // 4 vCPU, 8 GB Dedicated
                    ["g6-dedicated-8"] = 0.108m,  // 8 vCPU, 16 GB Dedicated
                    ["g6-dedicated-16"] = 0.216m  // 16 vCPU, 32 GB Dedicated
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.10m,
                HddPerGBMonth = 0.10m,
                ObjectStoragePerGBMonth = 0.02m,
                BackupPerGBMonth = 0.03m,        // 25% of Linode cost
                RegistryPerGBMonth = 0.02m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.01m,            // Very competitive
                LoadBalancerPerHour = 0.015m,   // NodeBalancer ~$10/month
                NatGatewayPerHour = 0m,
                VpnPerHour = 0m,
                PublicIpPerHour = 0m            // IPs included
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 0,
                BusinessSupportPercent = 5,
                EnterpriseSupportPercent = 10
            }
        };
    }

    /// <summary>
    /// Get default Vultr pricing
    /// </summary>
    public static PricingModel GetVultrPricing(string region = "ewr")
    {
        return new PricingModel
        {
            Provider = CloudProvider.Vultr,
            Region = region,
            RegionDisplayName = CloudRegions.VultrRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (Vultr Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.024m,
                RamGBPerHour = 0.006m,
                ManagedControlPlanePerHour = 0m, // VKE is free
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["vc2-2c-4gb"] = 0.03m,       // 2 vCPU, 4 GB - $20/month
                    ["vc2-4c-8gb"] = 0.06m,       // 4 vCPU, 8 GB - $40/month
                    ["vc2-6c-16gb"] = 0.12m,      // 6 vCPU, 16 GB - $80/month
                    ["vhf-4c-16gb"] = 0.09m,      // 4 vCPU, 16 GB High Freq
                    ["vdc-4c-16gb"] = 0.18m       // 4 vCPU, 16 GB Dedicated
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.10m,
                HddPerGBMonth = 0.10m,
                ObjectStoragePerGBMonth = 0.02m,
                BackupPerGBMonth = 0.03m,
                RegistryPerGBMonth = 0.02m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.01m,
                LoadBalancerPerHour = 0.015m,
                NatGatewayPerHour = 0m,
                VpnPerHour = 0m,
                PublicIpPerHour = 0.004m
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 0,
                BusinessSupportPercent = 5,
                EnterpriseSupportPercent = 10
            }
        };
    }

    /// <summary>
    /// Get default Hetzner pricing (very cost-effective European provider)
    /// </summary>
    public static PricingModel GetHetznerPricing(string region = "fsn1")
    {
        return new PricingModel
        {
            Provider = CloudProvider.Hetzner,
            Region = region,
            RegionDisplayName = CloudRegions.HetznerRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (Hetzner Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                // Hetzner is VERY cost-effective
                CpuPerHour = 0.012m,
                RamGBPerHour = 0.003m,
                ManagedControlPlanePerHour = 0m, // No managed K8s (use k3s/RKE2)
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["cx21"] = 0.008m,           // 2 vCPU, 4 GB - €5.51/month
                    ["cx31"] = 0.016m,           // 2 vCPU, 8 GB - €10.59/month
                    ["cx41"] = 0.03m,            // 4 vCPU, 16 GB - €20.11/month
                    ["cx51"] = 0.06m,            // 8 vCPU, 32 GB - €38.56/month
                    ["ccx13"] = 0.036m,          // 2 Dedicated, 8 GB
                    ["ccx23"] = 0.072m           // 4 Dedicated, 16 GB
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.044m,         // Very cheap
                HddPerGBMonth = 0.044m,
                ObjectStoragePerGBMonth = 0.0052m, // Extremely cheap
                BackupPerGBMonth = 0.01m,
                RegistryPerGBMonth = 0.02m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.001m,           // 1€/TB - extremely cheap
                LoadBalancerPerHour = 0.008m,   // ~$5.88/month
                NatGatewayPerHour = 0m,
                VpnPerHour = 0m,
                PublicIpPerHour = 0.001m        // €1/month
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 0,
                BusinessSupportPercent = 5,
                EnterpriseSupportPercent = 10
            }
        };
    }

    /// <summary>
    /// Get default OVHcloud pricing
    /// </summary>
    public static PricingModel GetOVHPricing(string region = "gra")
    {
        return new PricingModel
        {
            Provider = CloudProvider.OVH,
            Region = region,
            RegionDisplayName = region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (OVHcloud Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.02m,
                RamGBPerHour = 0.004m,
                ManagedControlPlanePerHour = 0m, // OVH Managed K8s is free
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["b2-15"] = 0.06m,           // 4 vCPU, 15 GB
                    ["b2-30"] = 0.12m,           // 8 vCPU, 30 GB
                    ["b2-60"] = 0.24m,           // 16 vCPU, 60 GB
                    ["c2-15"] = 0.08m,           // 4 vCPU, 15 GB (compute)
                    ["r2-30"] = 0.15m            // 2 vCPU, 30 GB (memory)
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.04m,
                HddPerGBMonth = 0.02m,
                ObjectStoragePerGBMonth = 0.01m,
                BackupPerGBMonth = 0.02m,
                RegistryPerGBMonth = 0.04m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.01m,
                LoadBalancerPerHour = 0.01m,
                NatGatewayPerHour = 0m,
                VpnPerHour = 0.02m,
                PublicIpPerHour = 0m            // Included
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 0,
                BusinessSupportPercent = 5,
                EnterpriseSupportPercent = 10
            }
        };
    }

    /// <summary>
    /// Get default Scaleway pricing
    /// </summary>
    public static PricingModel GetScalewayPricing(string region = "fr-par")
    {
        return new PricingModel
        {
            Provider = CloudProvider.Scaleway,
            Region = region,
            RegionDisplayName = region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (Scaleway Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.018m,
                RamGBPerHour = 0.004m,
                ManagedControlPlanePerHour = 0m, // Kapsule is free
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["DEV1-M"] = 0.018m,         // 2 vCPU, 4 GB
                    ["DEV1-L"] = 0.03m,          // 4 vCPU, 8 GB
                    ["GP1-S"] = 0.06m,           // 4 vCPU, 16 GB
                    ["GP1-M"] = 0.12m,           // 8 vCPU, 32 GB
                    ["GP1-L"] = 0.24m            // 16 vCPU, 64 GB
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.08m,
                HddPerGBMonth = 0.03m,
                ObjectStoragePerGBMonth = 0.01m, // Very competitive
                BackupPerGBMonth = 0.02m,
                RegistryPerGBMonth = 0.02m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.01m,
                LoadBalancerPerHour = 0.01m,
                NatGatewayPerHour = 0m,
                VpnPerHour = 0m,
                PublicIpPerHour = 0.003m
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 0,
                BusinessSupportPercent = 5,
                EnterpriseSupportPercent = 10
            }
        };
    }

    /// <summary>
    /// Get default on-premises pricing
    /// </summary>
    public static PricingModel GetOnPremPricing()
    {
        return new PricingModel
        {
            Provider = CloudProvider.OnPrem,
            Region = "On-Premises",
            RegionDisplayName = "On-Premises Data Center",
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default On-Premises Estimates",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                // Amortized hardware cost + operations
                CpuPerHour = 0.03m,
                RamGBPerHour = 0.004m,
                ManagedControlPlanePerHour = 0m
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.05m,
                HddPerGBMonth = 0.02m,
                ObjectStoragePerGBMonth = 0.02m,
                BackupPerGBMonth = 0.03m,
                RegistryPerGBMonth = 0.05m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.01m,           // ISP costs
                LoadBalancerPerHour = 0.01m,   // Amortized hardware
                NatGatewayPerHour = 0m,
                VpnPerHour = 0.01m,
                PublicIpPerHour = 0.001m
            },
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 2500m,
                RancherEnterprisePerNodeYear = 1000m,
                TanzuPerCoreYear = 1500m,
                CharmedK8sPerNodeYear = 500m
            },
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 5,
                BusinessSupportPercent = 10,
                EnterpriseSupportPercent = 15
            }
        };
    }

    /// <summary>
    /// Get default pricing for any provider
    /// </summary>
    /// <summary>
    /// Get default ROSA (Red Hat OpenShift Service on AWS) pricing
    /// </summary>
    public static PricingModel GetROSAPricing(string region = "us-east-1")
    {
        var awsPricing = GetAWSPricing(region);
        return new PricingModel
        {
            Provider = CloudProvider.ROSA,
            Region = region,
            RegionDisplayName = CloudRegions.AWSRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (ROSA Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = awsPricing.Compute.CpuPerHour,
                RamGBPerHour = awsPricing.Compute.RamGBPerHour,
                ManagedControlPlanePerHour = 0.171m, // ROSA control plane
                OpenShiftServiceFeePerWorkerHour = 0.03m, // ROSA service fee per worker
                InstanceTypePrices = awsPricing.Compute.InstanceTypePrices
            },
            Storage = awsPricing.Storage,
            Network = awsPricing.Network,
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 0m, // Included in service fee
                RancherEnterprisePerNodeYear = 0m,
                TanzuPerCoreYear = 0m,
                CharmedK8sPerNodeYear = 0m
            },
            Support = awsPricing.Support
        };
    }

    /// <summary>
    /// Get default ARO (Azure Red Hat OpenShift) pricing
    /// </summary>
    public static PricingModel GetAROPricing(string region = "eastus")
    {
        var azurePricing = GetAzurePricing(region);
        return new PricingModel
        {
            Provider = CloudProvider.ARO,
            Region = region,
            RegionDisplayName = CloudRegions.AzureRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (ARO Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = azurePricing.Compute.CpuPerHour,
                RamGBPerHour = azurePricing.Compute.RamGBPerHour,
                ManagedControlPlanePerHour = 0m, // Free ARO control plane
                OpenShiftServiceFeePerWorkerHour = 0.03m, // ARO service fee per worker
                InstanceTypePrices = azurePricing.Compute.InstanceTypePrices
            },
            Storage = azurePricing.Storage,
            Network = azurePricing.Network,
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 0m, // Included in service fee
                RancherEnterprisePerNodeYear = 0m,
                TanzuPerCoreYear = 0m,
                CharmedK8sPerNodeYear = 0m
            },
            Support = azurePricing.Support
        };
    }

    /// <summary>
    /// Get default OSD (OpenShift Dedicated on GCP) pricing
    /// </summary>
    public static PricingModel GetOSDPricing(string region = "us-central1")
    {
        var gcpPricing = GetGCPPricing(region);
        return new PricingModel
        {
            Provider = CloudProvider.OSD,
            Region = region,
            RegionDisplayName = CloudRegions.GCPRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (OSD Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = gcpPricing.Compute.CpuPerHour,
                RamGBPerHour = gcpPricing.Compute.RamGBPerHour,
                ManagedControlPlanePerHour = 0.171m, // OSD control plane
                OpenShiftServiceFeePerWorkerHour = 0.03m, // OSD service fee per worker
                InstanceTypePrices = gcpPricing.Compute.InstanceTypePrices
            },
            Storage = gcpPricing.Storage,
            Network = gcpPricing.Network,
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 0m, // Included in service fee
                RancherEnterprisePerNodeYear = 0m,
                TanzuPerCoreYear = 0m,
                CharmedK8sPerNodeYear = 0m
            },
            Support = gcpPricing.Support
        };
    }

    /// <summary>
    /// Get default ROKS (Red Hat OpenShift on IBM Cloud) pricing
    /// </summary>
    public static PricingModel GetROKSPricing(string region = "us-south")
    {
        var ibmPricing = GetIBMPricing(region);
        return new PricingModel
        {
            Provider = CloudProvider.ROKS,
            Region = region,
            RegionDisplayName = CloudRegions.IBMRegions.FirstOrDefault(r => r.Code == region)?.DisplayName ?? region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (ROKS Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = ibmPricing.Compute.CpuPerHour,
                RamGBPerHour = ibmPricing.Compute.RamGBPerHour,
                ManagedControlPlanePerHour = 0m, // Free ROKS control plane
                OpenShiftServiceFeePerWorkerHour = 0.025m, // ROKS service fee per worker
                InstanceTypePrices = ibmPricing.Compute.InstanceTypePrices
            },
            Storage = ibmPricing.Storage,
            Network = ibmPricing.Network,
            Licenses = new LicensePricing
            {
                OpenShiftPerNodeYear = 0m, // Included in service fee
                RancherEnterprisePerNodeYear = 0m,
                TanzuPerCoreYear = 0m,
                CharmedK8sPerNodeYear = 0m
            },
            Support = ibmPricing.Support
        };
    }

    /// <summary>
    /// Get default Civo Cloud pricing
    /// </summary>
    public static PricingModel GetCivoPricing(string region = "lon1")
    {
        return new PricingModel
        {
            Provider = CloudProvider.Civo,
            Region = region,
            RegionDisplayName = region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (Civo Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.015m,     // Competitive pricing
                RamGBPerHour = 0.003m,
                ManagedControlPlanePerHour = 0m, // Free K3s control plane
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["small"] = 0.0074m,       // 1 vCPU, 2 GB ($5.50/month)
                    ["medium"] = 0.0135m,      // 2 vCPU, 4 GB ($10/month)
                    ["large"] = 0.027m,        // 4 vCPU, 8 GB ($20/month)
                    ["xlarge"] = 0.054m,       // 8 vCPU, 16 GB ($40/month)
                    ["2xlarge"] = 0.108m       // 16 vCPU, 32 GB ($80/month)
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.10m,
                HddPerGBMonth = 0.05m,
                ObjectStoragePerGBMonth = 0.02m,
                BackupPerGBMonth = 0.05m,
                RegistryPerGBMonth = 0.10m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.01m,           // Very competitive egress
                LoadBalancerPerHour = 0.014m,  // $10/month
                NatGatewayPerHour = 0m,
                VpnPerHour = 0.014m,
                PublicIpPerHour = 0.004m
            },
            Licenses = new LicensePricing(),
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 0,
                BusinessSupportPercent = 10,
                EnterpriseSupportPercent = 15
            }
        };
    }

    /// <summary>
    /// Get default Exoscale (SKS) pricing
    /// </summary>
    public static PricingModel GetExoscalePricing(string region = "ch-gva-2")
    {
        return new PricingModel
        {
            Provider = CloudProvider.Exoscale,
            Region = region,
            RegionDisplayName = region,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = "Default (Exoscale Public Pricing 2025)",
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.02m,      // Swiss quality hosting
                RamGBPerHour = 0.004m,
                ManagedControlPlanePerHour = 0m, // Free SKS control plane
                InstanceTypePrices = new Dictionary<string, decimal>
                {
                    ["small"] = 0.0149m,       // 2 vCPU, 2 GB
                    ["medium"] = 0.0298m,      // 2 vCPU, 4 GB
                    ["large"] = 0.0596m,       // 4 vCPU, 8 GB
                    ["xlarge"] = 0.1192m,      // 4 vCPU, 16 GB
                    ["2xlarge"] = 0.2384m      // 8 vCPU, 32 GB
                }
            },
            Storage = new StoragePricing
            {
                SsdPerGBMonth = 0.12m,
                HddPerGBMonth = 0.06m,
                ObjectStoragePerGBMonth = 0.025m,
                BackupPerGBMonth = 0.05m,
                RegistryPerGBMonth = 0.10m
            },
            Network = new NetworkPricing
            {
                EgressPerGB = 0.011m,
                LoadBalancerPerHour = 0.028m,
                NatGatewayPerHour = 0.028m,
                VpnPerHour = 0.04m,
                PublicIpPerHour = 0.004m
            },
            Licenses = new LicensePricing(),
            Support = new SupportPricing
            {
                BasicSupportPercent = 0,
                DeveloperSupportPercent = 5,
                BusinessSupportPercent = 10,
                EnterpriseSupportPercent = 15
            }
        };
    }

    public static PricingModel GetDefaultPricing(CloudProvider provider, string? region = null)
    {
        return provider switch
        {
            // Major Cloud Providers
            CloudProvider.AWS => GetAWSPricing(region ?? "us-east-1"),
            CloudProvider.Azure => GetAzurePricing(region ?? "eastus"),
            CloudProvider.GCP => GetGCPPricing(region ?? "us-central1"),
            CloudProvider.OCI => GetOCIPricing(region ?? "us-ashburn-1"),
            CloudProvider.IBM => GetIBMPricing(region ?? "us-south"),
            CloudProvider.Alibaba => GetAlibabaPricing(region ?? "cn-hangzhou"),
            CloudProvider.Tencent => GetTencentPricing(region ?? "ap-guangzhou"),
            CloudProvider.Huawei => GetHuaweiPricing(region ?? "cn-north-4"),

            // Managed OpenShift Services
            CloudProvider.ROSA => GetROSAPricing(region ?? "us-east-1"),
            CloudProvider.ARO => GetAROPricing(region ?? "eastus"),
            CloudProvider.OSD => GetOSDPricing(region ?? "us-central1"),
            CloudProvider.ROKS => GetROKSPricing(region ?? "us-south"),

            // Developer-Friendly Cloud Providers
            CloudProvider.DigitalOcean => GetDigitalOceanPricing(region ?? "nyc1"),
            CloudProvider.Linode => GetLinodePricing(region ?? "us-east"),
            CloudProvider.Vultr => GetVultrPricing(region ?? "ewr"),
            CloudProvider.Hetzner => GetHetznerPricing(region ?? "fsn1"),
            CloudProvider.OVH => GetOVHPricing(region ?? "gra"),
            CloudProvider.Scaleway => GetScalewayPricing(region ?? "fr-par"),
            CloudProvider.Civo => GetCivoPricing(region ?? "lon1"),
            CloudProvider.Exoscale => GetExoscalePricing(region ?? "ch-gva-2"),

            // On-Premises
            CloudProvider.OnPrem => GetOnPremPricing(),

            // Default to AWS for unknown providers
            _ => GetAWSPricing(region ?? "us-east-1")
        };
    }
}
