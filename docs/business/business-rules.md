# Business Rules Reference

This document catalogs all business rules implemented in the Infrastructure Sizing Calculator.

---

## Rule Naming Convention

Rules follow the pattern: `BR-{Category}{Number}`

| Prefix | Category |
|--------|----------|
| BR-M | Master/Control Plane Nodes |
| BR-I | Infrastructure Nodes |
| BR-W | Worker Nodes |
| BR-H | Headroom Settings |
| BR-R | Replica Settings |
| BR-T | Technology Configurations |
| BR-D | Distribution Configurations |
| BR-E | Environment Settings |
| BR-V | Validation Rules |
| BR-RC | Resource Calculations |
| BR-C | Cost Estimation |
| BR-S | Scenario Management |
| BR-G | Growth Planning |

---

## Master Node Rules (BR-M)

Control plane node calculations for Kubernetes clusters.

| Rule ID | Description | Implementation |
|---------|-------------|----------------|
| BR-M001 | Managed control plane distributions (EKS, AKS, GKE, OKE) require 0 master nodes | `K8sSizingService.CalculateMasters()` |
| BR-M002 | Standard Kubernetes HA requires minimum 3 master nodes | `K8sSizingService.CalculateMasters()` |
| BR-M003 | Large clusters (100+ workers) require 5 master nodes | `K8sSizingService.CalculateMasters()` |
| BR-M004 | Non-production can use 3 masters regardless of size | `K8sSizingService.CalculateMasters()` |

### Master Node Logic
```
if (HasManagedControlPlane) return 0;      // BR-M001
if (workers >= 100) return 5;               // BR-M003
return 3;                                   // BR-M002, BR-M004
```

---

## Infrastructure Node Rules (BR-I)

Infrastructure node calculations (OpenShift only).

| Rule ID | Description | Implementation |
|---------|-------------|----------------|
| BR-I001 | Only OpenShift requires infrastructure nodes | `K8sSizingService.CalculateInfra()` |
| BR-I002 | Minimum infrastructure nodes = 3 | `MinInfra = 3` |
| BR-I003 | Maximum infrastructure nodes = 10 | `MaxInfra = 10` |
| BR-I004 | Scale infrastructure at 1 node per 25 applications | `AppsPerInfra = 25` |
| BR-I005 | Large production (>=50 apps) requires minimum 5 infra nodes | `LargeDeploymentThreshold = 50` |
| BR-I006 | Small production (<50 apps) can use 3 infra nodes | Default minimum applies |

### Infrastructure Node Logic
```
if (!IsOpenShift) return 0;                 // BR-I001
calculated = Ceiling(apps / 25);            // BR-I004
if (isProd && apps >= 50) min = 5;          // BR-I005, BR-I006
return Clamp(calculated, min, max);         // BR-I002, BR-I003
```

---

## Worker Node Rules (BR-W)

Worker node calculations based on resource requirements.

| Rule ID | Description | Implementation |
|---------|-------------|----------------|
| BR-W001 | Minimum workers per cluster = 3 | `MinWorkers = 3` |
| BR-W002 | 15% system reserve (85% available for pods) | `SystemReserve = 0.85` |
| BR-W003 | Calculate workers needed by CPU | `workersByCpu = totalCpu / (nodeSpecs.Cpu * overcommit * reserve)` |
| BR-W004 | Calculate workers needed by RAM | `workersByRam = totalRam / (nodeSpecs.Ram * overcommit * reserve)` |
| BR-W005 | Final workers = MAX(byCpu, byRam, minimum) | `Math.Max(workersByCpu, workersByRam, MinWorkers)` |
| BR-W006 | Worker specs ALWAYS use ProdWorker | All environments use same worker resources |

### Worker Calculation Logic
```
cpuCapacity = nodeSpecs.Cpu * cpuOvercommit * SystemReserve;
ramCapacity = nodeSpecs.Ram * ramOvercommit * SystemReserve;
workersByCpu = Ceiling(totalCpu / cpuCapacity);        // BR-W003
workersByRam = Ceiling(totalRam / ramCapacity);        // BR-W004
return Max(workersByCpu, workersByRam, MinWorkers);    // BR-W005, BR-W001
```

---

## Headroom Rules (BR-H)

Capacity headroom configuration by environment.

| Rule ID | Description | Default Value |
|---------|-------------|---------------|
| BR-H001 | Headroom is applied as percentage to worker count | Multiplier calculation |
| BR-H002 | Headroom formula: workers = baseWorkers * (1 + headroom%) | `ApplyHeadroom()` |
| BR-H003 | Default headroom for Development | 33% |
| BR-H004 | Default headroom for Test | 33% |
| BR-H005 | Default headroom for Staging | 0% |
| BR-H006 | Default headroom for Production | 37.5% |
| BR-H007 | Default headroom for DR | 37.5% |
| BR-H008 | Headroom percentage must be between 0 and 100 | Validation range |
| BR-H009 | When headroom disabled globally, all environments use 0% | `HeadroomEnabled` flag |

### Default Headroom Values
```csharp
public static HeadroomSettings Default => new()
{
    Dev = 33.0,     // BR-H003
    Test = 33.0,    // BR-H004
    Stage = 0.0,    // BR-H005
    Prod = 37.5,    // BR-H006
    DR = 37.5       // BR-H007
};
```

---

## Replica Rules (BR-R)

Pod replica settings by environment type.

| Rule ID | Description | Default Value |
|---------|-------------|---------------|
| BR-R001 | Default replicas for Production and DR | 3 |
| BR-R002 | Default replicas for Development and Test | 1 |
| BR-R003 | Default replicas for Staging | 2 |
| BR-R004 | Replica count must be between 1 and 10 | Validation range |
| BR-R005 | Total pods = Applications × Replicas | Calculation formula |

### Default Replica Values
```csharp
public static ReplicaSettings Default => new()
{
    Prod = 3,       // BR-R001
    NonProd = 1,    // BR-R002
    Stage = 2       // BR-R003
};
```

---

## Technology Rules (BR-T)

Technology-specific configurations and tier specifications.

| Rule ID | Technology | Platform | Tier Specs (CPU/RAM in GB) |
|---------|------------|----------|---------------------------|
| BR-T001 | Base tier specs | All | Per-technology definitions |
| BR-T002 | .NET | Native | S: 0.25/0.5, M: 0.5/1, L: 1/2, XL: 2/4 |
| BR-T003 | Java | Native | S: 0.5/1, M: 1/2, L: 2/4, XL: 4/8 |
| BR-T004 | Node.js | Native | S: 0.25/1*, M: 0.5/1, L: 1/2, XL: 2/4 |
| BR-T005 | Go | Native | S: 0.125/0.25, M: 0.25/0.5, L: 0.5/1, XL: 1/2 |
| BR-T006 | Mendix | Low-Code | S: 1/2, M: 2/4, L: 4/8, XL: 8/16 |
| BR-T007 | Python | Native | S: 0.25/1*, M: 0.5/1, L: 1/2, XL: 2/4 |
| BR-T008 | OutSystems | Low-Code | S: 1/2, M: 2/4, L: 4/8, XL: 8/16 |

*Note: Node.js and Python Small tier RAM increased to 1 GB per official docs (V8 heap management and WSGI/Django overhead respectively).

### Technology Platform Mapping
```
Native: .NET, Java, Node.js, Python, Go
Low-Code: Mendix, OutSystems

Note: OutSystems is VM-only (no Kubernetes deployment on-premises)
```

---

## Distribution Rules (BR-D)

Kubernetes distribution configurations (46 distributions total).

| Rule ID | Distribution | Type | Control Plane | Infra Nodes |
|---------|--------------|------|---------------|-------------|
| BR-D001 | Base distribution config | All | Per-distribution | Per-distribution |
| BR-D002 | OpenShift (OCP, ROSA, ARO, Dedicated) | Enterprise | Self-managed or Managed | Yes |
| BR-D003 | Kubernetes (vanilla) | Open Source | Self-managed | No |
| BR-D004 | Lightweight (K3s, MicroK8s, Minikube, Kind) | Dev/Edge | Self-managed | No |
| BR-D005 | Enterprise on-prem (Rancher, RKE2, Tanzu, Charmed) | Enterprise | Self-managed | No |
| BR-D006 | Major cloud (EKS, AKS, GKE, OKE, IKS, ACK, TKE, CCE) | Managed | Managed (0 nodes) | No |
| BR-D007 | OpenShift infra requirement | OpenShift variants | N/A | Required |
| BR-D008 | Distribution node specs | All | CPU/RAM/Disk per type | N/A |
| BR-D009 | Cloud variants (EKS Anywhere, AKS Arc, GKE Autopilot/Anthos) | Hybrid | Varies | No |
| BR-D010 | Developer platforms (DOKS, LKE, VKE, Civo, Hetzner, etc.) | Managed | Managed (0 nodes) | No |

### Managed Control Plane Distributions (20+)
```
Major Cloud:
  EKS, AKS, GKE, OKE, IKS, ACK, TKE, CCE

Developer Platforms:
  DOKS, LKE, VKE, CivoK8s, HetznerK8s, OVHKubernetes,
  ScalewayKapsule, UpcloudKubernetes, ExoscaleSKS, IonosK8s

OpenShift Managed:
  ROSA, ARO, OpenShiftDedicated, OpenShiftOnline
```

### Infrastructure Node Requirement (OpenShift only)
```
Distributions with HasInfraNodes = true:
  OpenShift, ROSA, ARO, OpenShiftDedicated

Minimum infra nodes: 3
Scale: 1 per 25 applications
```

---

## Environment Rules (BR-E)

Environment classification and requirements.

| Rule ID | Description | Implementation |
|---------|-------------|----------------|
| BR-E001 | Five environment types supported | Dev, Test, Stage, Prod, DR |
| BR-E002 | Production environment is always enabled | Validation in K8sSizingInput |
| BR-E003 | Production and DR are "Production" type | `IsProd` classification |
| BR-E004 | DR uses Production replica and overcommit settings | Same as Prod |

### Environment Classification
```
Non-Production: Dev, Test
Staging: Stage
Production: Prod, DR
```

---

## Validation Rules (BR-V)

Input validation constraints.

| Rule ID | Description | Range |
|---------|-------------|-------|
| BR-V001 | Application counts must be >= 0 | [0, int.MaxValue] |
| BR-V002 | Application counts must be integers | Whole numbers only |
| BR-V003 | Node specs must be positive | > 0 |
| BR-V004 | Disk specs must be positive | > 0 |
| BR-V005 | Instance counts must be >= 1 | [1, 100] |
| BR-V006 | CPU overcommit must be between 1 and 10 | [1.0, 10.0] |
| BR-V007 | Memory overcommit must be between 1 and 4 | [1.0, 4.0] |

---

## Resource Calculation Rules (BR-RC)

Rules for calculating total cluster resources.

| Rule ID | Description | Formula |
|---------|-------------|---------|
| BR-RC001 | Total CPU = Sum of all node CPUs | Σ(node_count × node_cpu) |
| BR-RC002 | Total RAM = Sum of all node RAM | Σ(node_count × node_ram) |
| BR-RC003 | Total Disk = Sum of all node Disks | Σ(node_count × node_disk) |
| BR-RC004 | Grand total aggregates all environments | Sum per-environment totals |
| BR-RC005 | Per-environment CPU calculation | masters×cpuM + infra×cpuI + workers×cpuW |
| BR-RC006 | Per-environment RAM calculation | masters×ramM + infra×ramI + workers×ramW |
| BR-RC007 | Per-environment Disk calculation | masters×diskM + infra×diskI + workers×diskW |

---

## VM-Specific Rules (BR-VM)

Rules specific to Virtual Machine sizing.

| Rule ID | Description | Implementation |
|---------|-------------|----------------|
| BR-VM001 | Technology-specific server roles | TechnologyVMRoles |
| BR-VM002 | MaxInstances constraint enforcement | OS Controller = 1 |
| BR-VM003 | MinInstances for HA roles | OS Frontend >= 2 |
| BR-VM004 | MemoryMultiplier for memory-intensive roles | DB roles = 1.5× |
| BR-VM005 | ScaleHorizontally flag for scalable roles | Frontend servers |
| BR-VM006 | HA pattern multipliers | ActiveActive = 2× |
| BR-VM007 | Load balancer VM calculations | HAPair = 2 VMs |

### HA Pattern Multipliers
```
None = 1.0
ActiveActive = 2.0
ActivePassive = 2.0
NPlus1 = 1 + (1 / instances)
NPlus2 = 1 + (2 / instances)
```

---

## Cost Estimation Rules (BR-C)

Rules for infrastructure cost calculations.

| Rule ID | Description | Implementation |
|---------|-------------|----------------|
| BR-C001 | Support on-premises hardware pricing | `CostEstimationService.CalculateOnPremCosts()` |
| BR-C002 | Support cloud provider pricing (AWS, Azure, GCP, Oracle, IBM) | `CostEstimationService.CalculateCloudCosts()` |
| BR-C003 | Mendix platform licensing costs based on app tier and count | `PricingService.GetMendixPricing()` |
| BR-C004 | Monthly costs from VM/container requirements | Hours × Hourly Rate |
| BR-C005 | Annual costs = Monthly × 12 | Standard annual projection |
| BR-C006 | Reserved Instance discounts (1-year, 3-year) | Per cloud provider rates |
| BR-C007 | Pricing data from database settings | SQLite persistence |

### Cloud Pricing Sources
```
AWS       - EC2 and EKS pricing
Azure     - VMs and AKS pricing
GCP       - Compute Engine and GKE pricing
Oracle    - OCI and OKE pricing
IBM       - IBM Cloud Kubernetes pricing
```

---

## Scenario Management Rules (BR-S)

Rules for saving and comparing sizing scenarios.

| Rule ID | Description | Implementation |
|---------|-------------|----------------|
| BR-S001 | Scenarios persist to SQLite database | `ScenarioService` |
| BR-S002 | Scenario includes sizing input and results | Full configuration saved |
| BR-S003 | Scenarios support naming and description | User-defined metadata |
| BR-S004 | Scenario comparison side-by-side | Up to 4 scenarios |
| BR-S005 | Scenario export to JSON format | Portable sharing |
| BR-S006 | Scenario import from JSON | Configuration restore |

---

## Growth Planning Rules (BR-G)

Rules for capacity growth projections.

| Rule ID | Description | Implementation |
|---------|-------------|----------------|
| BR-G001 | Support linear growth projections | `GrowthPlanningService.CalculateLinearGrowth()` |
| BR-G002 | Support percentage-based growth | Compound growth formula |
| BR-G003 | Projection periods: 1-5 years | Configurable timeframe |
| BR-G004 | Show when scaling events required | Capacity threshold alerts |
| BR-G005 | Growth affects node counts and costs | Cascading calculations |

### Growth Projection Formula
```
Linear: resources_year_n = base + (growth_per_year × n)
Compound: resources_year_n = base × (1 + growth_rate)^n
```

---

## Implementation Files

| Rule Category | Primary File |
|---------------|--------------|
| BR-M* | `Services/K8sSizingService.cs` |
| BR-I* | `Services/K8sSizingService.cs` |
| BR-W* | `Services/K8sSizingService.cs` |
| BR-H* | `Models/HeadroomSettings.cs`, `Services/K8sSizingService.cs` |
| BR-R* | `Models/ReplicaSettings.cs`, `Services/K8sSizingService.cs` |
| BR-T* | `Services/TechnologyService.cs` |
| BR-D* | `Services/DistributionService.cs` |
| BR-E* | `Models/K8sSizingInput.cs` |
| BR-V* | `Models/AppConfig.cs`, `Models/OvercommitSettings.cs` |
| BR-RC* | `Services/K8sSizingService.cs` |
| BR-VM* | `Services/VMSizingService.cs` |
| BR-C* | `Services/CostEstimationService.cs`, `Services/PricingService.cs` |
| BR-S* | `Services/ScenarioService.cs` |
| BR-G* | `Services/GrowthPlanningService.cs` |
