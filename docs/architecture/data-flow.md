# Data Flow Documentation

## Overview

This document describes how data flows through the Infrastructure Sizing Calculator from user input to calculated results.

## K8s Sizing Flow

### Input Collection

```
User Selection Flow:
1. Platform Type (Native/LowCode)
2. Deployment Model (Kubernetes)
3. Technology (.NET, Java, etc.)
4. Distribution (OpenShift, EKS, etc.)
5. Cluster Mode:
   - Multi-Cluster: Checkbox per environment
   - Single Cluster: Dropdown (Shared, Dev, Test, Stage, Prod, DR)
6. Configuration:
   - App counts per tier (Small, Medium, Large, XLarge)
   - Node specs (Control Plane, Infra, Worker)
   - Settings (Overcommit, Headroom, Replicas)
```

### Input Model Structure

```
K8sSizingInput
├── Distribution: Distribution enum
├── Technology: Technology enum
├── ClusterMode: ClusterMode enum
├── EnabledEnvironments: HashSet<EnvironmentType>
├── EnvironmentApps: Dictionary<EnvironmentType, AppConfig>
│   └── AppConfig: { Small, Medium, Large, XLarge }
├── Replicas: ReplicaSettings
│   └── { Prod, NonProd, Stage }
├── Headroom: HeadroomSettings
│   └── { Dev, Test, Stage, Prod, DR } (percentages)
├── ProdOvercommit/NonProdOvercommit: OvercommitSettings
│   └── { Cpu, Memory } (ratios)
└── CustomNodeSpecs: DistributionConfig (optional)
```

### Calculation Process

```
K8sSizingService.Calculate(input):
│
├── 1. Get DistributionConfig
│   └── DistributionService.GetConfig(distribution)
│       └── Returns: NodeSpecs for CP, Infra, Worker
│
├── 2. Get TechnologyConfig
│   └── TechnologyService.GetConfig(technology)
│       └── Returns: TierSpecs (CPU/RAM per pod size)
│
├── 3. For each enabled environment:
│   │
│   ├── a. Get app config (or use fallback Prod/NonProd)
│   │
│   ├── b. Calculate pods:
│   │   └── Total Pods = Sum(apps × tier_count) × replicas
│   │
│   ├── c. Calculate Master Nodes (BR-M001 to BR-M004):
│   │   ├── workers < 10  → 3 masters
│   │   ├── workers < 100 → 3 masters
│   │   ├── workers < 200 → 5 masters
│   │   └── workers >= 200 → 5 masters
│   │   └── Managed CP → 0 masters
│   │
│   ├── d. Calculate Infra Nodes (BR-I001 to BR-I006):
│   │   └── OpenShift only, based on app count
│   │   └── Min 3, Max 10, 1 per 25 apps
│   │
│   ├── e. Calculate Worker Nodes (BR-W001 to BR-W006):
│   │   ├── Sum resource requirements per tier
│   │   ├── Apply overcommit ratios
│   │   ├── Apply headroom percentage
│   │   ├── Calculate nodes needed (CPU-limited or RAM-limited)
│   │   └── Min 3 workers
│   │
│   └── f. Calculate total resources:
│       └── CPU = Σ(node_count × node_cpu)
│       └── RAM = Σ(node_count × node_ram)
│       └── Disk = Σ(node_count × node_disk)
│
└── 4. Aggregate to GrandTotal:
    └── Sum all environments
```

### Output Model Structure

```
K8sSizingResult
├── Environments: List<EnvironmentResult>
│   └── EnvironmentResult:
│       ├── Environment: EnvironmentType
│       ├── IsProd: bool
│       ├── Apps, Replicas, Pods: int
│       ├── Masters, Infra, Workers, TotalNodes: int
│       └── TotalCpu, TotalRam, TotalDisk: int
├── GrandTotal: GrandTotal
│   ├── TotalNodes, TotalMasters, TotalInfra, TotalWorkers
│   └── TotalCpu, TotalRam, TotalDisk
├── Configuration: K8sSizingInput (original)
├── NodeSpecs: DistributionConfig (used)
└── CalculatedAt: DateTime
```

## VM Sizing Flow

### Input Collection

```
User Selection Flow:
1. Platform Type (Native/LowCode)
2. Deployment Model (VMs)
3. Technology (.NET, Java, OutSystems, etc.)
4. Per-Environment Configuration:
   - Server Roles (technology-specific)
   - VM Size per role (Small, Medium, Large, XLarge)
   - Instance count per role
   - HA Pattern
   - DR Pattern
   - Load Balancer option
```

### Input Model Structure

```
VMSizingInput
├── Technology: Technology enum
├── EnabledEnvironments: HashSet<EnvironmentType>
├── EnvironmentConfigs: Dictionary<EnvironmentType, VMEnvironmentConfig>
│   └── VMEnvironmentConfig:
│       ├── Environment: EnvironmentType
│       ├── Enabled: bool
│       ├── Roles: List<VMRoleConfig>
│       │   └── VMRoleConfig:
│       │       ├── Role: ServerRole (generic)
│       │       ├── RoleId: string (tech-specific)
│       │       ├── RoleName, RoleIcon, RoleDescription
│       │       ├── Size: AppTier
│       │       ├── InstanceCount: int (1-100)
│       │       ├── CustomCpu, CustomRam: int? (overrides)
│       │       ├── DiskGB: int
│       │       └── MemoryMultiplier: double
│       ├── HAPattern: HAPattern enum
│       ├── DRPattern: DRPattern enum
│       ├── LoadBalancer: LoadBalancerOption enum
│       └── StorageGB: int
└── SystemOverheadPercent: double (0-50%)
```

### Calculation Process

```
VMSizingService.Calculate(input):
│
├── 1. Get TechnologyConfig
│   └── TechnologyService.GetConfig(technology)
│
├── 2. For each enabled environment:
│   │
│   ├── a. Get HA multiplier:
│   │   ├── None → 1
│   │   ├── ActiveActive → 2
│   │   ├── ActivePassive → 2
│   │   ├── NPlus1 → 1 + 1/instances
│   │   └── NPlus2 → 1 + 2/instances
│   │
│   ├── b. For each role:
│   │   ├── Get base specs from TierSpecs
│   │   ├── Apply MemoryMultiplier
│   │   ├── Apply CustomCpu/CustomRam overrides
│   │   ├── Apply SystemOverheadPercent
│   │   ├── Calculate total instances (base × HA multiplier)
│   │   └── Calculate role totals (CPU, RAM, Disk)
│   │
│   ├── c. Calculate Load Balancer:
│   │   ├── None → 0 VMs
│   │   ├── Single → 1 VM
│   │   ├── HAPair → 2 VMs
│   │   └── CloudLB → 0 VMs (managed)
│   │
│   └── d. Sum environment totals
│
└── 3. Aggregate to VMGrandTotal
```

### Output Model Structure

```
VMSizingResult
├── Environments: List<VMEnvironmentResult>
│   └── VMEnvironmentResult:
│       ├── Environment: EnvironmentType
│       ├── IsProd: bool
│       ├── HAPattern, DRPattern, LoadBalancer
│       ├── Roles: List<VMRoleResult>
│       │   └── VMRoleResult:
│       │       ├── Role, RoleName, Size
│       │       ├── BaseInstances, TotalInstances
│       │       ├── CpuPerInstance, RamPerInstance, DiskPerInstance
│       │       └── TotalCpu, TotalRam, TotalDisk
│       ├── TotalVMs, TotalCpu, TotalRam, TotalDisk
│       └── LoadBalancerVMs, LoadBalancerCpu, LoadBalancerRam
├── GrandTotal: VMGrandTotal
│   └── TotalVMs, TotalCpu, TotalRam, TotalDisk, TotalLoadBalancerVMs
├── Configuration: VMSizingInput (original)
└── CalculatedAt: DateTime
```

## Export Flow

```
ExportService:
│
├── ExportToCsv(result):
│   └── Returns: string (CSV content)
│
├── ExportToJson(result):
│   └── Returns: string (JSON content)
│
├── ExportToExcel(result):
│   ├── Creates workbook with ClosedXML
│   ├── Summary sheet (metadata, grand totals)
│   ├── Per-environment sheets
│   └── Returns: byte[] (Excel file)
│
└── ExportToHtmlDiagram(result):
    └── Returns: string (HTML visualization)
```

## State Management (WizardStateService)

```
WizardStateService:
├── CurrentStep: int
├── Selections:
│   ├── SelectedPlatform, SelectedDeployment
│   ├── SelectedTechnology, SelectedDistribution
│   └── SelectedClusterMode
├── Configuration:
│   ├── EnabledEnvironments
│   ├── EnvApps, Headroom, Replicas
│   └── Overcommit settings
├── Results:
│   └── K8sSizingResult, VMSizingResult
└── Methods:
    ├── Reset() - Clear all state
    ├── NotifyStateChanged() - Trigger UI update
    ├── CanNavigateToStep() - Validate navigation
    └── GetStepLabel() - Get step display name
```
