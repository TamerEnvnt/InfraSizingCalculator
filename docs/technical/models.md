# Data Models Reference

This document describes all data models used in the Infrastructure Sizing Calculator.

---

## Input Models

### K8sSizingInput

Primary input model for Kubernetes cluster sizing calculations.

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| Distribution | Distribution | K8s distribution | Required |
| Technology | Technology | Application technology | Required |
| ClusterMode | ClusterMode | Multi-Cluster, Shared, or Per-Environment | MultiCluster |
| EnabledEnvironments | HashSet\<EnvironmentType\> | Active environments | {Prod} |
| EnvironmentApps | Dictionary\<EnvironmentType, AppConfig\> | App counts per env | {} |
| ProdApps | AppConfig | Production app counts | Empty |
| NonProdApps | AppConfig | Non-prod app counts | Empty |
| Replicas | ReplicaSettings | Replica configuration | Default |
| Headroom | HeadroomSettings | Headroom percentages | Default |
| HeadroomEnabled | bool | Enable/disable headroom | true |
| ProdOvercommit | OvercommitSettings | Production overcommit | Default |
| NonProdOvercommit | OvercommitSettings | Non-prod overcommit | Default |
| CustomNodeSpecs | DistributionConfig? | Custom node specs | null |
| HADRConfig | K8sHADRConfig? | Default HA/DR configuration | null |
| EnvironmentHADRConfigs | Dictionary\<EnvironmentType, K8sHADRConfig\>? | Per-env HA/DR overrides | null |

**Validation:**
- Production environment always enabled (BR-E002)
- At least one app count must be > 0

---

### VMSizingInput

Primary input model for Virtual Machine sizing calculations.

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| Technology | Technology | Application technology | Required |
| EnabledEnvironments | HashSet\<EnvironmentType\> | Active environments | {Prod} |
| EnvironmentConfigs | Dictionary\<EnvironmentType, VMEnvironmentConfig\> | Per-env configs | {} |
| SystemOverheadPercent | double | System overhead (0-50%) | 10.0 |

---

### VMEnvironmentConfig

Per-environment configuration for VM sizing.

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| Environment | EnvironmentType | Environment type | Required |
| Enabled | bool | Is environment enabled | true |
| Roles | List\<VMRoleConfig\> | Server role configs | [] |
| HAPattern | HAPattern | HA pattern | None |
| DRPattern | DRPattern | DR pattern | None |
| LoadBalancer | LoadBalancerOption | LB option | None |
| StorageGB | int | Additional storage | 0 |

---

### VMRoleConfig

Configuration for a specific server role.

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| Role | ServerRole | Generic role type | Required |
| RoleId | string | Technology-specific ID | "" |
| RoleName | string | Display name | "" |
| RoleIcon | string | Icon emoji | "" |
| RoleDescription | string | Description | "" |
| Size | AppTier | VM size tier | Medium |
| InstanceCount | int | Number of instances | 1 |
| CustomCpu | int? | Override CPU cores | null |
| CustomRam | int? | Override RAM GB | null |
| DiskGB | int | Disk per instance | 100 |
| MemoryMultiplier | double | RAM multiplier | 1.0 |

---

### AppConfig

Application count configuration per tier.

| Property | Type | Description | Validation |
|----------|------|-------------|------------|
| Small | int | Small tier count | >= 0 |
| Medium | int | Medium tier count | >= 0 |
| Large | int | Large tier count | >= 0 |
| XLarge | int | XLarge tier count | >= 0 |
| TotalApps | int | Computed total | readonly |

---

### ReplicaSettings

Replica count settings by environment type.

| Property | Type | Description | Default | Validation |
|----------|------|-------------|---------|------------|
| Prod | int | Production replicas | 3 | 1-10 |
| NonProd | int | Dev/Test replicas | 1 | 1-10 |
| Stage | int | Staging replicas | 2 | 1-10 |

---

### HeadroomSettings

Headroom percentage settings by environment.

| Property | Type | Description | Default | Validation |
|----------|------|-------------|---------|------------|
| Dev | double | Development headroom | 33.0% | 0-100 |
| Test | double | Test headroom | 33.0% | 0-100 |
| Stage | double | Staging headroom | 0.0% | 0-100 |
| Prod | double | Production headroom | 37.5% | 0-100 |
| DR | double | DR headroom | 37.5% | 0-100 |

---

### OvercommitSettings

Resource overcommit ratio settings.

| Property | Type | Description | Default | Validation |
|----------|------|-------------|---------|------------|
| Cpu | double | CPU overcommit ratio | 1.0 | 1.0-10.0 |
| Memory | double | RAM overcommit ratio | 1.0 | 1.0-4.0 |

---

## Output Models

### K8sSizingResult

Complete Kubernetes sizing calculation results.

| Property | Type | Description |
|----------|------|-------------|
| Environments | List\<EnvironmentResult\> | Per-environment results |
| GrandTotal | GrandTotal | Aggregated totals |
| Configuration | K8sSizingInput | Original input |
| NodeSpecs | DistributionConfig | Specs used |
| CalculatedAt | DateTime | Timestamp |

---

### EnvironmentResult

Results for a single environment (K8s).

| Property | Type | Description |
|----------|------|-------------|
| Environment | EnvironmentType | Environment type |
| IsProd | bool | Is production type |
| Apps | int | Total applications |
| Replicas | int | Replica count |
| Pods | int | Total pods |
| Masters | int | Master node count |
| Infra | int | Infra node count |
| Workers | int | Worker node count |
| EtcdNodes | int | External etcd node count |
| DRNodes | int | DR site node count |
| DRCostMultiplier | decimal | DR cost multiplier |
| AvailabilityZones | int | Number of AZs used |
| TotalNodes | int | Sum of all nodes |
| TotalCpu | int | Total vCPU |
| TotalRam | int | Total RAM (GB) |
| TotalDisk | int | Total Disk (GB) |

---

### VMSizingResult

Complete VM sizing calculation results.

| Property | Type | Description |
|----------|------|-------------|
| Environments | List\<VMEnvironmentResult\> | Per-environment results |
| GrandTotal | VMGrandTotal | Aggregated totals |
| Configuration | VMSizingInput | Original input |
| CalculatedAt | DateTime | Timestamp |

---

### VMEnvironmentResult

Results for a single environment (VM).

| Property | Type | Description |
|----------|------|-------------|
| Environment | EnvironmentType | Environment type |
| IsProd | bool | Is production type |
| HAPattern | HAPattern | HA pattern used |
| DRPattern | DRPattern | DR pattern used |
| LoadBalancer | LoadBalancerOption | LB option used |
| Roles | List\<VMRoleResult\> | Per-role results |
| TotalVMs | int | Total VM count |
| TotalCpu | int | Total vCPU |
| TotalRam | int | Total RAM (GB) |
| TotalDisk | int | Total Disk (GB) |
| LoadBalancerVMs | int | LB VM count |
| LoadBalancerCpu | int | LB vCPU |
| LoadBalancerRam | int | LB RAM (GB) |

---

### GrandTotal

Aggregated totals across all environments (K8s).

| Property | Type | Description |
|----------|------|-------------|
| TotalNodes | int | All nodes |
| TotalMasters | int | All master nodes |
| TotalInfra | int | All infra nodes |
| TotalWorkers | int | All worker nodes |
| TotalCpu | int | Total vCPU |
| TotalRam | int | Total RAM (GB) |
| TotalDisk | int | Total Disk (GB) |

---

### VMGrandTotal

Aggregated totals across all environments (VM).

| Property | Type | Description |
|----------|------|-------------|
| TotalVMs | int | All VMs |
| TotalCpu | int | Total vCPU |
| TotalRam | int | Total RAM (GB) |
| TotalDisk | int | Total Disk (GB) |
| TotalLoadBalancerVMs | int | All LB VMs |

---

## Configuration Models

### TechnologyConfig

Technology-specific configuration.

| Property | Type | Description |
|----------|------|-------------|
| Technology | Technology | Technology enum |
| Name | string | Display name |
| Icon | string | Icon emoji |
| BrandColor | string | Hex color |
| Vendor | string | Vendor name |
| Description | string | Description |
| PlatformType | PlatformType | Native or LowCode |
| Tiers | Dictionary\<AppTier, TierSpecs\> | Per-tier specs |
| VMRoles | TechnologyVMRoles? | VM server roles |

---

### TechnologyVMRoles

VM deployment template for a technology.

| Property | Type | Description |
|----------|------|-------------|
| Technology | Technology | Technology enum |
| DeploymentName | string | Deployment name |
| Description | string | Description |
| Roles | List\<TechnologyServerRole\> | Server roles |

---

### TechnologyServerRole

Definition of a server role for VM deployments.

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| Id | string | Unique identifier | Required |
| Name | string | Display name | Required |
| Icon | string | Icon emoji | Required |
| Description | string | Description | Required |
| DefaultSize | AppTier | Default VM size | Medium |
| DefaultDiskGB | int | Default disk size | 100 |
| Required | bool | Is role required | false |
| ScaleHorizontally | bool | Can add instances | false |
| MinInstances | int | Minimum instances | 1 |
| MaxInstances | int? | Maximum instances | null |
| MemoryMultiplier | double | RAM multiplier | 1.0 |

---

### DistributionConfig

Kubernetes distribution configuration.

| Property | Type | Description |
|----------|------|-------------|
| Distribution | Distribution | Distribution enum |
| Name | string | Display name |
| Icon | string | Icon emoji |
| BrandColor | string | Hex color |
| Vendor | string | Vendor name |
| Description | string | Description |
| ProdControlPlane | NodeSpecs | Prod master specs |
| NonProdControlPlane | NodeSpecs | Non-prod master specs |
| ProdInfra | NodeSpecs | Prod infra specs |
| NonProdInfra | NodeSpecs | Non-prod infra specs |
| ProdWorker | NodeSpecs | Prod worker specs |
| NonProdWorker | NodeSpecs | Non-prod worker specs |
| HasManagedControlPlane | bool | Cloud-managed CP |
| HasInfraNodes | bool | Requires infra nodes |

---

### NodeSpecs

Specifications for a node type.

| Property | Type | Description |
|----------|------|-------------|
| Cpu | int | CPU cores |
| Ram | int | RAM in GB |
| Disk | int | Disk in GB |

**Static Members:**
- `Zero` - NodeSpecs(0, 0, 0)
- `Default` - NodeSpecs(8, 32, 100)

---

### TierSpecs

Resource specifications for an application tier.

| Property | Type | Description |
|----------|------|-------------|
| Cpu | double | CPU cores (can be fractional) |
| Ram | double | RAM in GB |

---

## Enumerations

### PlatformType
```csharp
Native,     // Code-first platforms (.NET, Java, etc.)
LowCode     // Low-code platforms (Mendix, OutSystems)
```

### DeploymentModel
```csharp
Kubernetes, // Container orchestration
VMs         // Traditional virtual machines
```

### Technology
```csharp
DotNet,     // .NET Framework/Core
Java,       // Java/JVM
NodeJs,     // Node.js
Python,     // Python
Go,         // Golang
Mendix,     // Mendix Low-Code
OutSystems  // OutSystems Low-Code (VM only)
```

### Distribution (46 total)

**On-Premises (8):**
```csharp
OpenShift,      // Red Hat OpenShift Container Platform
Kubernetes,     // Vanilla Kubernetes (CNCF)
Rancher,        // SUSE Rancher
RKE2,           // SUSE RKE2 (Rancher Kubernetes Engine 2)
K3s,            // Lightweight K3s
MicroK8s,       // Canonical MicroK8s
Charmed,        // Charmed Kubernetes
Tanzu           // VMware Tanzu
```

**OpenShift Variants (4):**
```csharp
ROSA,               // Red Hat OpenShift on AWS
ARO,                // Azure Red Hat OpenShift
OpenShiftDedicated, // OpenShift Dedicated (managed)
OpenShiftOnline     // OpenShift Online (SaaS)
```

**Major Cloud Managed (8):**
```csharp
EKS,    // Amazon EKS
AKS,    // Azure AKS
GKE,    // Google GKE
OKE,    // Oracle OKE
IKS,    // IBM Cloud Kubernetes
ACK,    // Alibaba Cloud ACK
TKE,    // Tencent TKE
CCE     // Huawei CCE
```

**Cloud Variants (12):**
```csharp
// Rancher on Cloud
RancherEKS, RancherAKS, RancherGKE,
// Tanzu on Cloud
TanzuAWS, TanzuAzure, TanzuGCP,
// EKS Variants
EKSAnywhere, EKSDistro, EKSOutposts,
// Other
AKSArc, GKEAutopilot, GKEAnthos
```

**Developer-Focused (14):**
```csharp
DOKS,               // DigitalOcean Kubernetes
LKE,                // Linode/Akamai LKE
VKE,                // Vultr Kubernetes
CivoK8s,            // Civo Kubernetes
HetznerK8s,         // Hetzner Kubernetes
OVHKubernetes,      // OVH Managed Kubernetes
ScalewayKapsule,    // Scaleway Kapsule
UpcloudKubernetes,  // UpCloud Kubernetes
ExoscaleSKS,        // Exoscale SKS
IonosK8s,           // IONOS Kubernetes
NaverCloudKubernetes, // Naver Cloud
KakaoiCloud,        // Kakao i Cloud
Minikube,           // Local development
Kind                // Kubernetes in Docker
```

### EnvironmentType
```csharp
Dev,        // Development
Test,       // Testing
Stage,      // Staging
Prod,       // Production
DR          // Disaster Recovery
```

### ClusterMode
```csharp
MultiCluster,     // Separate cluster per environment (default)
SharedCluster,    // Shared cluster with namespace isolation
PerEnvironment    // Calculate for specific environment only
```

### AppTier
```csharp
Small,      // Smallest resource profile
Medium,     // Standard resource profile
Large,      // High resource profile
XLarge      // Maximum resource profile
```

### HAPattern
```csharp
None,           // No HA (1x)
ActiveActive,   // Both active (2x)
ActivePassive,  // Standby (2x)
NPlus1,         // N + 1 spare
NPlus2          // N + 2 spares
```

### DRPattern
```csharp
None,           // No DR
WarmStandby,    // Minimal standby
HotStandby,     // Full standby
MultiRegion     // Geographic distribution
```

### LoadBalancerOption
```csharp
None,       // No LB (0 VMs)
Single,     // Single LB (1 VM)
HAPair,     // HA pair (2 VMs)
CloudLB     // Cloud-managed (0 VMs)
```

### ServerRole
```csharp
WebServer,      // Web/Proxy server
AppServer,      // Application server
Database,       // Database server
Cache,          // Cache server
Controller,     // Platform controller
LifeTime,       // OutSystems LifeTime
FileStorage     // File storage server
```

---

## K8s HA/DR Enumerations

### K8sControlPlaneHA
```csharp
Managed,      // Cloud-managed control plane (EKS, AKS, GKE) - automatic HA
Single,       // Single control plane node (dev/test only)
StackedHA,    // 3+ control planes with co-located etcd
ExternalEtcd  // 3+ control planes with separate etcd cluster
```

### K8sNodeDistribution
```csharp
SingleAZ,     // All nodes in one AZ (lowest cost, no AZ redundancy)
DualAZ,       // Nodes spread across 2 AZs
MultiAZ,      // Nodes spread across 3+ AZs (recommended for prod)
MultiRegion   // Nodes across multiple regions (highest availability)
```

### K8sDRPattern
```csharp
None,           // No DR - single cluster with multi-AZ
BackupRestore,  // Regular backups with Velero/Kasten, manual restore
WarmStandby,    // Minimal standby cluster, scales up on failover
HotStandby,     // Fully provisioned standby (RTO < 15min)
ActiveActive    // Multiple clusters with global load balancing
```

### K8sBackupStrategy
```csharp
None,         // No automated backup
Velero,       // Open source CNCF project
Kasten,       // Kasten K10 - enterprise-grade
Portworx,     // Portworx PX-Backup - storage-integrated
CloudNative,  // AWS Backup, Azure Backup, GCP Backup
Custom        // Organization-specific tooling
```

---

## K8s HA/DR Configuration Model

### K8sHADRConfig

High Availability and Disaster Recovery configuration for K8s clusters.

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| ControlPlaneHA | K8sControlPlaneHA | Control plane HA mode | Managed |
| ControlPlaneNodes | int | Control plane node count | 3 |
| NodeDistribution | K8sNodeDistribution | Worker node distribution | MultiAZ |
| AvailabilityZones | int | Number of AZs | 3 |
| DRPattern | K8sDRPattern | Disaster recovery pattern | None |
| BackupStrategy | K8sBackupStrategy | Backup strategy | None |
| BackupFrequencyHours | int | Backup frequency (0 = none) | 24 |
| BackupRetentionDays | int | Backup retention period | 30 |
| EnablePodDisruptionBudgets | bool | Enable PDBs | true |
| MinAvailableReplicas | int | Min replicas during disruptions | 1 |
| EnableTopologySpread | bool | Use topology spread constraints | true |
| DRRegion | string? | Target DR region | null |
| RTOMinutes | int? | Recovery Time Objective | null |
| RPOMinutes | int? | Recovery Point Objective | null |

**Methods:**
- `GetCostMultiplier()` - Returns decimal cost multiplier based on HA/DR settings
- `GetCostMultiplier(Distribution)` - Provider-aware cost multiplier (Azure FREE cross-AZ)
- `GetSummary()` - Returns human-readable summary

**Cost Multiplier Sources (Dec 2024):**
- Multi-AZ: AWS $0.01/GB, Azure FREE, GCP $0.01/GB (~2-3% overhead)
- Multi-Region: AWS $0.02-0.09/GB, Azure/GCP $0.05-0.12/GB (~20% overhead)
- DR patterns: AWS Well-Architected DR pillar ratios
- Backup costs: Velero free, Kasten ~$225/node/yr, Portworx ~$2100/node/yr

---

## OutSystems Pricing Models

Verified against OutSystems Partner Calculator (January 2026).

### OutSystemsPlatform
```csharp
ODC,  // OutSystems Developer Cloud - Cloud-native, Kubernetes-based, fully managed
O11   // OutSystems 11 - Traditional .NET platform
```

### OutSystemsDeployment
```csharp
Cloud,       // OutSystems managed cloud infrastructure (O11 only)
SelfManaged  // Customer-managed infrastructure (O11 only)
```

### OutSystemsRegion
```csharp
Africa,      // Africa region
MiddleEast,  // Middle East region (higher Bootcamp/Expert rates)
Americas,    // Americas region
Europe,      // Europe region
AsiaPacific  // Asia-Pacific region
```

### OutSystemsDiscountType
```csharp
Percentage,   // Percentage discount (0-100)
FixedAmount   // Fixed dollar amount discount
```

### OutSystemsDiscountScope
```csharp
Total,        // Apply to entire quote (License + Add-Ons + Services)
LicenseOnly,  // Apply only to License costs
AddOnsOnly,   // Apply only to Add-Ons costs
ServicesOnly  // Apply only to Services costs
```

### OutSystemsCloudProvider
```csharp
OnPremises,  // On-premises data center
Azure,       // Microsoft Azure VMs
AWS          // Amazon Web Services EC2
```

### OutSystemsAzureInstanceType
```csharp
F4s_v2,    // 4 vCPU, 8 GB RAM - Default ($0.169/hr)
D4s_v3,    // 4 vCPU, 16 GB RAM ($0.192/hr)
D8s_v3,    // 8 vCPU, 32 GB RAM ($0.384/hr)
D16s_v3    // 16 vCPU, 64 GB RAM ($0.768/hr)
```

### OutSystemsAwsInstanceType
```csharp
M5Large,    // 2 vCPU, 8 GB RAM ($0.096/hr)
M5XLarge,   // 4 vCPU, 16 GB RAM ($0.192/hr)
M52XLarge   // 8 vCPU, 32 GB RAM ($0.384/hr)
```

### OutSystemsPricingSettings

Complete OutSystems pricing configuration with platform-specific rates.

**ODC Platform Pricing:**

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| OdcPlatformBasePrice | decimal | ODC annual base (includes 150 AOs, 100 users, Dev+Prod) | $30,250 |
| OdcAOPackPrice | decimal | Additional AO pack (150 AOs) | $18,150 |
| OdcInternalUserPackPrice | decimal | Internal user pack (100 users) | $6,050 |
| OdcExternalUserPackPrice | decimal | External user pack (1000 users) | $6,050 |

**O11 Platform Pricing:**

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| O11EnterpriseBasePrice | decimal | O11 annual base (includes 150 AOs, 100 users, 3 envs) | $36,300 |
| O11AOPackPrice | decimal | Additional AO pack (150 AOs) | $36,300 |
| O11InternalUserTiers | List | Tiered pricing for internal users | See tiers below |
| O11ExternalUserTiers | List | Tiered pricing for external users | See tiers below |

**Unlimited Users (Both Platforms):**

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| UnlimitedUsersPerAOPack | decimal | Unlimited users price per AO pack | $60,500 |

**ODC Add-Ons (Per AO Pack):**

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| OdcSupport24x7ExtendedPerPack | decimal | 24x7 Extended support | $6,050/pack |
| OdcSupport24x7PremiumPerPack | decimal | 24x7 Premium support | $9,680/pack |
| OdcHighAvailabilityPerPack | decimal | High Availability | $18,150/pack |
| OdcNonProdRuntimePerPack | decimal | Non-Prod Runtime | $6,050/pack |
| OdcPrivateGatewayPerPack | decimal | Private Gateway | $1,210/pack |
| OdcSentryPerPack | decimal | Sentry monitoring | $6,050/pack |

**O11 Add-Ons (Per AO Pack):**

| Property | Type | Description | Default | Availability |
|----------|------|-------------|---------|--------------|
| O11Support24x7PremiumPerPack | decimal | Premium support | $3,630/pack | Both |
| O11HighAvailabilityPerPack | decimal | High Availability | $12,100/pack | Cloud only |
| O11SentryPerPack | decimal | Sentry (includes HA) | $24,200/pack | Cloud only |
| O11NonProdEnvPerPack | decimal | Non-Prod Environment | $3,630/pack | Both |
| O11LoadTestEnvPerPack | decimal | Load Test Environment | $6,050/pack | Cloud only |
| O11EnvironmentPackPerPack | decimal | Environment Pack | $9,680/pack | Both |
| O11DisasterRecoveryPerPack | decimal | Disaster Recovery | $12,100/pack | Self-Managed only |
| O11LogStreamingFlat | decimal | Log Streaming (flat) | $7,260 | Cloud only |
| O11DatabaseReplicaFlat | decimal | Database Replica (flat) | $96,800 | Cloud only |

**O11 Internal User Tiers (per 100 users):**

| Tier | User Range | Price per Pack |
|------|------------|----------------|
| 1 | 200-1,000 | $12,100 |
| 2 | 1,100-10,000 | $2,420 |
| 3 | 10,100+ | $242 |

Note: First 100 users included in base price.

**O11 External User Tiers (per 1,000 users):**

| Tier | User Range | Price per Pack |
|------|------------|----------------|
| 1 | 1-10,000 | $4,840 |
| 2 | 11,000-250,000 | $1,452 |
| 3 | 251,000+ | $30.25 |

**AppShield Tiers (19 tiers, 0-15M users):**

| Tier | User Range | Flat Price |
|------|------------|------------|
| 1 | 0-10,000 | $18,150 |
| 2 | 10,001-50,000 | $32,670 |
| 3 | 50,001-100,000 | $54,450 |
| 4 | 100,001-500,000 | $96,800 |
| ... | ... | ... |
| 19 | 14,000,001-15,000,000 | $1,476,200 |

**Services Pricing by Region:**

| Service | All Regions | Middle East |
|---------|-------------|-------------|
| Essential Success Plan | $30,250 | $30,250 |
| Premier Success Plan | $60,500 | $60,500 |
| Dedicated Group Session | $2,670 | $3,820 |
| Public Session | $480 | $720 |
| Expert Day | $1,400 | $2,130 |

### OutSystemsDeploymentConfig

User-selected OutSystems deployment configuration.

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| Platform | OutSystemsPlatform | ODC or O11 | O11 |
| Deployment | OutSystemsDeployment | Cloud or Self-Managed (O11) | Cloud |
| Region | OutSystemsRegion | Service pricing region | Africa |
| TotalApplicationObjects | int | Total AOs | 150 |
| UseUnlimitedUsers | bool | Use unlimited users pricing | false |
| InternalUsers | int | Internal user count | 100 |
| ExternalUsers | int | External user count | 0 |
| AppShieldUserVolume | int? | Expected users for AppShield (if Unlimited) | null |
| CloudProvider | OutSystemsCloudProvider | VM provider (Self-Managed) | OnPremises |
| AzureInstanceType | OutSystemsAzureInstanceType | Azure VM type | F4s_v2 |
| AwsInstanceType | OutSystemsAwsInstanceType | AWS instance type | M5XLarge |
| FrontEndServersPerEnvironment | int | Front-ends per env | 2 |
| TotalEnvironments | int | Total environments | 4 |
| Discount | OutSystemsDiscount? | Optional discount | null |

**ODC Add-On Toggles:** OdcSupport24x7Extended, OdcSupport24x7Premium, OdcAppShield, OdcHighAvailability, OdcNonProdRuntimeQuantity, OdcPrivateGateway, OdcSentry

**O11 Add-On Toggles:** O11Support24x7Premium, O11AppShield, O11HighAvailability, O11Sentry, O11LogStreamingQuantity, O11NonProdEnvQuantity, O11LoadTestEnvQuantity, O11EnvPackQuantity, O11DisasterRecovery, O11DatabaseReplicaQuantity

**Services:** EssentialSuccessPlanQuantity, PremierSuccessPlanQuantity, DedicatedGroupSessionQuantity, PublicSessionQuantity, ExpertDayQuantity

### OutSystemsPricingResult

Calculated OutSystems pricing result with detailed breakdown.

| Property | Type | Description |
|----------|------|-------------|
| Platform | OutSystemsPlatform | ODC or O11 |
| Deployment | OutSystemsDeployment | Cloud or Self-Managed |
| Region | OutSystemsRegion | Service pricing region |
| AOPackCount | int | Number of AO packs |
| EditionCost | decimal | Base platform cost |
| AOPacksCost | decimal | Additional AO packs |
| InternalUsersCost | decimal | Internal user packs (O11 tiered) |
| ExternalUsersCost | decimal | External user packs (O11 tiered) |
| UnlimitedUsersCost | decimal | Unlimited users cost |
| LicenseSubtotal | decimal | Total license costs |
| AddOnCosts | Dictionary | Per add-on costs |
| AddOnsSubtotal | decimal | Total add-on costs |
| ServiceCosts | Dictionary | Per service costs |
| ServicesSubtotal | decimal | Total service costs |
| MonthlyVMCost | decimal | Monthly cloud VM cost (Self-Managed) |
| AnnualVMCost | decimal | Annual VM cost |
| InfrastructureSubtotal | decimal | Total infrastructure costs |
| GrossTotal | decimal | Total before discount |
| DiscountAmount | decimal | Discount applied |
| NetTotal | decimal | Final annual total |
| TotalPerMonth | decimal | Monthly average |
| TotalThreeYear | decimal | 3-year projection |
| TotalFiveYear | decimal | 5-year projection |
| Warnings | List\<string\> | Validation warnings |
| LineItems | List\<OutSystemsCostLineItem\> | Detailed line items |
