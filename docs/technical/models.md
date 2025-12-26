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
