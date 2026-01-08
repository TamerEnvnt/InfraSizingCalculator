# Infrastructure Sizing Research

> **Created**: 2025-01-04
> **Purpose**: Document the fundamental differences between K8s and VM sizing methodologies
> **Status**: Research Complete - Ready for Design

---

## Executive Summary

K8s and VM sizing are **fundamentally different** - not just in terminology but in the entire resource model, calculation methodology, and configuration flow. The current implementation incorrectly uses the same sizing model for both, just with different labels.

---

## K8s Sizing Methodology

### 1. Resource Model: Requests vs Limits

Unlike VMs where you allocate fixed resources, Kubernetes uses a **Requests/Limits model**:

| Concept | Description | Scheduling Impact |
|---------|-------------|-------------------|
| **Request** | Guaranteed minimum resources | Used by scheduler for placement |
| **Limit** | Maximum burst allowed | Enforced by kubelet at runtime |

**Key Insight**: Pods are scheduled based on REQUESTS, not limits. A node must have enough allocatable resources to satisfy all pod requests.

### 2. Resource Units

- **CPU**: Measured in **millicores** (1000m = 1 core)
  - `100m` = 0.1 core (10% of a core)
  - `500m` = 0.5 core (half a core)
  - `2000m` = 2 cores

- **Memory**: Measured in **Mi/Gi** (mebibytes/gibibytes)
  - `256Mi` = 256 MiB
  - `1Gi` = 1 GiB
  - `4Gi` = 4 GiB

### 3. Node Allocatable Resources

**Critical**: Not all node resources are available for pods!

```
Node Capacity
├── OS Overhead (systemd, SSH, etc.)           ~100-500MB
├── Kubelet Reserved (kube-reserved)           ~100-200MB + CPU
├── Container Runtime                          ~100MB
├── Eviction Threshold                         ~100MB
├── System Pods (kube-system)
│   ├── kube-proxy (per node)                  ~128Mi, 100m
│   ├── CNI (Calico/Flannel)                   ~128-256Mi
│   ├── CoreDNS (cluster-wide)                 ~170Mi
│   └── Monitoring agents                      ~256Mi
└── ALLOCATABLE (for user pods)
```

**Allocatable Percentage by Node Size**:
| Node Size | Memory Allocatable | CPU Allocatable |
|-----------|-------------------|-----------------|
| Small (4GB) | ~75% | ~94% |
| Medium (8GB) | ~85% | ~96% |
| Large (16GB+) | ~90-93% | ~97% |

**Formula**:
```
Allocatable = Total Capacity - OS - Kube-Reserved - Eviction - SystemPods
```

### 4. K8s Sizing Calculation Flow

```
Step 1: Define Pod Resources
─────────────────────────────
For each application type, define:
- CPU Request (guaranteed)
- CPU Limit (max burst)
- Memory Request (guaranteed)
- Memory Limit (max burst)

Example:
  Small App:  requests: 100m CPU, 256Mi  | limits: 200m, 512Mi
  Medium App: requests: 500m CPU, 1Gi    | limits: 1000m, 2Gi
  Large App:  requests: 2000m CPU, 4Gi   | limits: 4000m, 8Gi

Step 2: Calculate Replicas per Environment
──────────────────────────────────────────
Environment     | Min Replicas | HA Replicas
----------------|--------------|------------
Development     | 1            | 1
Test            | 1            | 2
Staging         | 2            | 2
Production      | 3            | 3+

Step 3: Calculate Total Resource Requests
─────────────────────────────────────────
For each environment:
  Total CPU Requests = Σ(pod_cpu_request × replica_count)
  Total Memory Requests = Σ(pod_memory_request × replica_count)

Step 4: Add System Overhead
───────────────────────────
System overhead per node:
- kube-proxy: 100m CPU, 128Mi
- CNI: 100m CPU, 128Mi
- Monitoring: 100m CPU, 256Mi
- Total: ~300m CPU, 512Mi per node

Step 5: Calculate Worker Nodes Needed
─────────────────────────────────────
Workers = ceil(Total Requests / Allocatable per Node)
Add redundancy: Workers × 1.2 (for N+1 failover)

Step 6: Control Plane Sizing (Separate!)
────────────────────────────────────────
Cluster Size        | Control Plane Nodes | Specs per Node
--------------------|--------------------|-----------------
< 100 nodes         | 1 (dev) or 3 (prod)| 4 vCPU, 16GB
100-500 nodes       | 3                  | 8 vCPU, 32GB
500-1000 nodes      | 3-5                | 16 vCPU, 64GB
> 1000 nodes        | 5                  | 32 vCPU, 128GB

Step 7: Storage (Decoupled!)
────────────────────────────
Storage is NOT part of node sizing in K8s:
- PersistentVolumeClaims (PVCs)
- StorageClasses
- External storage (EBS, Azure Disk, NFS)
```

### 5. K8s Sizing Best Practices

1. **CPU Requests**: Keep at 1 core or below per pod; scale horizontally with replicas
2. **Memory Requests = Limits**: For stability (prevents OOM kills)
3. **Limits vs Requests Ratio**:
   - Memory: 1:1 (for stability)
   - CPU: 1.1:1 to 1.5:1 (allow some burst)
4. **Node Size Trade-offs**:
   - Larger nodes: Better resource utilization, worse HA
   - Smaller nodes: Better HA, more overhead
5. **System Pods**: Budget 10-15% of cluster for kube-system

---

## VM Sizing Methodology

### 1. Resource Model: Fixed Allocation

VMs use **fixed resource allocation** - what you assign is what the VM gets (minus OS overhead):

| Resource | Allocation Type | Overcommit Possible |
|----------|-----------------|---------------------|
| vCPU | Fixed cores | Yes (hypervisor level) |
| RAM | Fixed GB | Yes, but risky |
| Storage | Fixed GB | No (physical disk) |

### 2. Resource Units

- **CPU**: Whole **vCPUs** (virtual CPUs)
  - 1 vCPU = 1 virtual core
  - 2 vCPU = 2 virtual cores

- **Memory**: Measured in **GB** (gigabytes)
  - 2GB, 4GB, 8GB, 16GB, etc.

- **Storage**: Measured in **GB** (coupled with VM)
  - System disk: 50-100GB
  - Data disk: varies

### 3. VM Overhead

Each VM has OS overhead:

```
VM Capacity
├── OS (Windows/Linux)
│   ├── Windows: ~2-4GB RAM, ~40GB disk
│   └── Linux: ~512MB-2GB RAM, ~10-20GB disk
├── VM Tools (VMware Tools, etc.)     ~100MB
└── AVAILABLE (for applications)
```

### 4. Overcommit Ratios

Hypervisors allow overcommitting resources (assigning more than physical capacity):

| Environment | CPU Overcommit | Memory Overcommit | Storage |
|-------------|---------------|-------------------|---------|
| Production | 1:1 (no overcommit) | 1:1 | 1:1 |
| Staging | 2:1 | 1.25:1 | 1:1 |
| Test | 4:1 | 1.5:1 | 1:1 |
| Development | 4:1 - 8:1 | 1.5:1 - 2:1 | 1:1 |

### 5. VM Sizing Calculation Flow

```
Step 1: Define Application Resources
─────────────────────────────────────
For each application type, define:
- vCPU required
- RAM required (GB)
- Storage required (GB)

Example:
  Small App:  1 vCPU, 2GB RAM, 20GB storage
  Medium App: 2 vCPU, 4GB RAM, 50GB storage
  Large App:  4 vCPU, 8GB RAM, 100GB storage

Step 2: Calculate Instances per Environment
───────────────────────────────────────────
Environment     | Instances per App
----------------|------------------
Development     | 1
Test            | 1
Staging         | 1-2
Production      | 2+ (behind LB)

Step 3: Add OS Overhead per VM
──────────────────────────────
Each VM needs:
- Linux: +1GB RAM, +15GB storage
- Windows: +4GB RAM, +40GB storage

Step 4: Calculate Total Resources
─────────────────────────────────
For each environment:
  Total vCPU = Σ(app_vcpu × instances)
  Total RAM = Σ((app_ram + os_overhead) × instances)
  Total Storage = Σ((app_storage + os_storage) × instances)

Step 5: Apply Overcommit Ratios
───────────────────────────────
Physical Resources = Total / Overcommit Ratio

Example (non-prod with 4:1 CPU, 1.5:1 RAM):
  Physical vCPU = 32 / 4 = 8 physical cores
  Physical RAM = 64GB / 1.5 = 43GB physical

Step 6: Calculate VMs Needed
────────────────────────────
VMs = ceil(Total Resources / VM Capacity)

OR for apps-per-VM model:
  VMs = ceil(Total Apps / Apps per VM)

Step 7: High Availability
─────────────────────────
For production:
- Load balancer in front
- N+1 redundancy (extra VM for failover)
- No automatic failover (manual/scripted)
```

---

## Key Differences Summary

| Aspect | Kubernetes | Virtual Machines |
|--------|------------|------------------|
| **Resource Model** | Requests/Limits (burst) | Fixed allocation |
| **CPU Units** | Millicores (500m) | vCPUs (whole cores) |
| **Memory Units** | Mi/Gi | GB |
| **Scheduling** | Automatic (kube-scheduler) | Manual placement |
| **Overhead Location** | Node-level (shared) | Per-VM (isolated) |
| **Overhead Amount** | 7-25% per node | 0.5-4GB per VM |
| **Storage** | Decoupled (PVCs) | Coupled (local disk) |
| **Scaling** | HPA, Cluster Autoscaler | Manual |
| **HA** | Built-in (replicas, anti-affinity) | Load balancer + scripts |
| **Failover** | Automatic | Manual/scripted |
| **Multi-tenancy** | Namespaces, resource quotas | Separate VMs |

---

## Configuration Panel Design Requirements

### K8s Apps Panel Should Include:

1. **Pod Resource Configuration**
   - CPU Request (millicores slider: 50m - 4000m)
   - CPU Limit (millicores slider: 100m - 8000m)
   - Memory Request (Mi slider: 128Mi - 16Gi)
   - Memory Limit (Mi slider: 256Mi - 32Gi)

2. **Pod Templates/Presets**
   - Micro: 100m/200m CPU, 256Mi/512Mi RAM
   - Small: 250m/500m CPU, 512Mi/1Gi RAM
   - Medium: 500m/1000m CPU, 1Gi/2Gi RAM
   - Large: 1000m/2000m CPU, 2Gi/4Gi RAM
   - XLarge: 2000m/4000m CPU, 4Gi/8Gi RAM

3. **Replicas per Environment**
   - Slider for each environment
   - HA indicator (< 3 replicas warning)

4. **No Storage Here** (separate panel)

### K8s Nodes Panel Should Include:

1. **Worker Node Configuration**
   - Node size presets (Small/Medium/Large/XLarge)
   - Custom: vCPU slider, Memory slider
   - Show allocatable % based on size

2. **Worker Node Count**
   - Auto-calculate based on pod requests
   - Manual override option
   - N+1 redundancy toggle

3. **Control Plane Configuration**
   - Separate section
   - 1/3/5 node selector
   - Fixed specs per tier

4. **Cluster Summary**
   - Total cluster capacity
   - Total allocatable
   - System overhead breakdown

### VM Apps Panel Should Include:

1. **Application Resource Configuration**
   - vCPU (whole numbers: 1-32)
   - RAM in GB (1-128)
   - Storage in GB (20-2000)

2. **Application Presets**
   - Small: 1 vCPU, 2GB, 50GB
   - Medium: 2 vCPU, 4GB, 100GB
   - Large: 4 vCPU, 8GB, 200GB
   - XLarge: 8 vCPU, 16GB, 500GB

3. **Instances per Environment**
   - Count per app type per environment
   - HA indicator for production

4. **OS Selection** (affects overhead)
   - Linux (~1GB overhead)
   - Windows (~4GB overhead)

### VM Nodes Panel Should Include:

1. **VM Size Configuration**
   - vCPU per VM
   - RAM per VM (GB)
   - Storage per VM (GB)

2. **VM Presets**
   - Small: 4 vCPU, 16GB, 250GB
   - Medium: 8 vCPU, 32GB, 500GB
   - Large: 16 vCPU, 64GB, 1TB
   - XLarge: 32 vCPU, 128GB, 2TB

3. **VM Count**
   - Total VMs needed (auto-calculated)
   - Manual override

4. **Overcommit Settings**
   - Per-environment ratios
   - CPU overcommit slider
   - Memory overcommit slider

5. **Summary**
   - Total physical resources needed
   - With and without overcommit

---

## Sources

### Kubernetes
- [Kubernetes Resource Management](https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/)
- [Reserve Compute Resources for System Daemons](https://kubernetes.io/docs/tasks/administer-cluster/reserve-compute-resources/)
- [Considerations for Large Clusters](https://kubernetes.io/docs/setup/best-practices/cluster-large/)
- [GKE Node Sizing](https://cloud.google.com/kubernetes-engine/docs/concepts/plan-node-sizes)
- [Understanding Kubernetes Limits and Requests - Sysdig](https://www.sysdig.com/blog/kubernetes-limits-requests/)
- [Allocatable Resources - LearnKube](https://learnkube.com/allocatable-resources)
- [Reserved CPU and Memory - Daniele Polencic](https://medium.com/@danielepolencic/reserved-cpu-and-memory-in-kubernetes-nodes-65aee1946afd)

### VMs
- [Nutanix Sizing and Capacity Planning](https://www.nutanix.com/tech-center/blog/hybrid-cloud-sizing-and-capacity-planning)
- [VMware Sizing Tools](https://vtechstation.wordpress.com/2024/01/08/vmware-sizing-and-capacity-planning-tools/)
- [SolarWinds VM Capacity Planning](https://www.solarwinds.com/virtualization-manager/use-cases/vm-capacity-planning)
- [VMware Aria Operations Capacity](https://blogs.vmware.com/cloud-foundation/2024/07/31/vmware-aria-operations-provides-new-capacity-and-cost-functionality/)

---

## Next Steps

1. Create wireframe HTML files for:
   - K8s Apps Panel (with requests/limits)
   - K8s Nodes Panel (with allocatable calculation)
   - VM Apps Panel (with fixed resources)
   - VM Nodes Panel (with overcommit)

2. Update the Blazor components to implement the correct flows

3. Update the sizing calculation services to use the correct formulas

4. Add E2E tests for both flows
