# Kubernetes Cloud Provider Sizing Research

This document provides comprehensive research on Kubernetes configuration best practices across major cloud providers, with a focus on node sizing, control plane configuration, and resource allocation formulas.

**Research Date**: January 2026
**Sources**: Official documentation from AWS, Azure, Google Cloud, and Kubernetes.io

---

## Table of Contents

1. [AWS EKS Configuration](#aws-eks-configuration)
2. [Azure AKS Configuration](#azure-aks-configuration)
3. [Google GKE Configuration](#google-gke-configuration)
4. [Common Kubernetes Patterns](#common-kubernetes-patterns)
5. [Resource Reservation Formulas](#resource-reservation-formulas)
6. [Sizing Recommendations Summary](#sizing-recommendations-summary)

---

## AWS EKS Configuration

### Control Plane

AWS EKS provides a fully managed control plane with two modes of operation:

| Mode | Description | Pricing | Best For |
|------|-------------|---------|----------|
| **Standard** | Auto-scales based on workload demands | $0.10/hr (standard support) | Most use cases |
| **Provisioned** | Pre-provisioned capacity from defined tiers | Additional cost per tier | High-demand, predictable workloads |

**Provisioned Control Plane Tiers:**
- `standard` (default)
- `tier-xl`
- `tier-2xl`
- `tier-4xl`
- Larger tiers available via AWS account team

**Pricing Notes:**
- Standard support (first 14 months): $0.10/hr
- Extended support (remaining 12 months): $0.60/hr

**Source:** [Amazon EKS Pricing](https://aws.amazon.com/eks/pricing/), [Amazon EKS Provisioned Control Plane](https://docs.aws.amazon.com/eks/latest/userguide/eks-provisioned-control-plane.html)

### Node Group Configuration

#### Instance Type Selection

| Instance Family | Best For | Example Types |
|----------------|----------|---------------|
| **M5/M6i/M7i** | General purpose, balanced workloads | m5.large, m6i.xlarge, m7i.2xlarge |
| **C5/C6i/C7i** | Compute-intensive workloads | c5.xlarge, c6i.2xlarge |
| **R5/R6i/R7i** | Memory-intensive workloads | r5.large, r6i.xlarge |
| **T3** | Burstable, dev/test (not recommended for production) | t3.medium, t3.large |

**Key Recommendations:**
- Default instance type if not specified: `t3.medium`
- **Avoid t2/t3 for CPU-bound workloads** - burstable instances don't provide deterministic CPU performance
- **Fewer, larger nodes** are generally better (reduces API server load, fewer DaemonSets)
- Use **Nitro System instances** for IPv6 clusters
- For ARM-based workloads, use **Graviton2 or later** (AL2023 doesn't support A1 instances)

**Source:** [Choose an optimal Amazon EC2 node instance type](https://docs.aws.amazon.com/eks/latest/userguide/choosing-instance-type.html)

#### Max Pods Per Node Calculation

The maximum number of pods per node is determined by ENI and IP address limits:

```
Standard Formula:
Max Pods = (Number of ENIs × (IPs per ENI - 1)) + 2

Example for t3.small:
Max Pods = 3 × (4 - 1) + 2 = 11 pods

Example for m5.large:
Max Pods = 3 × (10 - 1) + 2 = 29 pods
```

**With Prefix Delegation (higher pod density):**
- Enables /28 (16 IP) prefixes per ENI slot
- Requires Nitro System instances
- Karpenter defaults to `--max-pods=110` when using prefix delegation

**Source:** [Amazon VPC CNI plugin increases pods per node limits](https://aws.amazon.com/blogs/containers/amazon-vpc-cni-increases-pods-per-node-limits/), [EKS Best Practices - VPC CNI](https://aws.github.io/aws-eks-best-practices/networking/vpc-cni/)

#### Spot Instances

For cost optimization, use Spot instances with these best practices:
- **Diversify instance types** - specify multiple instance types to improve availability
- Use Spot for **fault-tolerant, stateless** workloads only
- Implement **graceful shutdown handling**
- Use separate node groups for Spot vs On-Demand

**Source:** [EKS Workshop - Instance Type Diversification](https://www.eksworkshop.com/docs/fundamentals/managed-node-groups/spot/instance-diversification)

### Auto Scaling

**Karpenter vs Cluster Autoscaler:**
- AWS recommends **Karpenter** for most new clusters
- Karpenter avoids the 450-node-per-group quota
- Provides greater instance selection flexibility
- Cluster Autoscaler requires node groups with similar instance sizes

**Source:** [EKS Best Practices - Data Plane](https://aws.github.io/aws-eks-best-practices/scalability/docs/data-plane/)

---

## Azure AKS Configuration

### Control Plane Tiers

| Tier | Price | Uptime SLA | Max Nodes | Features |
|------|-------|------------|-----------|----------|
| **Free** | $0 | Best-effort | 1,000 (recommended <10) | Basic management |
| **Standard** | $0.10/hr (~$72/mo) | 99.9% - 99.95% | 5,000 | Production SLA, auto-scaling |
| **Premium** | $0.60/hr | 99.9% - 99.95% | 5,000 | Long-term Support (LTS) |

**SLA Details:**
- 99.95% for clusters using Availability Zones
- 99.9% for clusters without Availability Zones
- Free tier has no financially-backed SLA

**Request Limits:**
- Free tier: 50 mutating + 100 read-only in-flight requests
- Standard/Premium: Auto-scales based on load

**Source:** [Azure AKS Pricing Tiers](https://learn.microsoft.com/en-us/azure/aks/free-standard-pricing-tiers), [AKS Pricing](https://azure.microsoft.com/en-us/pricing/details/kubernetes-service/)

### Node Pool Configuration

#### System vs User Node Pools

| Pool Type | Minimum Nodes | Purpose | VM Requirements |
|-----------|---------------|---------|-----------------|
| **System** | 2 (production: 3) | CoreDNS, metrics-server, critical system pods | ≥2 vCPUs, ≥4 GB RAM |
| **User** | 0 | Application workloads | ≥2 vCPUs, ≥2 GB RAM |

**New Feature: Managed System Node Pools (Preview, November 2025)**
- System pool fully managed by Microsoft
- Core components run on Microsoft-owned infrastructure
- No provisioning, patching, or scaling of system nodes required

**Source:** [Use system node pools in AKS](https://learn.microsoft.com/en-us/azure/aks/use-system-pools), [AKS Automatic managed system node pools](https://blog.aks.azure.com/2025/11/26/aks-automatic-managed-system-node-pools)

#### Recommended VM SKUs

| Use Case | Recommended SKU | vCPUs | Memory |
|----------|-----------------|-------|--------|
| General workloads | Standard_D2s_v3 | 2 | 8 GB |
| Medium workloads | Standard_D4s_v3 | 4 | 16 GB |
| Memory-intensive | Standard_E2s_v3 | 2 | 16 GB |
| Memory-heavy | Standard_E4s_v3 | 4 | 32 GB |

**VM Series Recommendations:**
- **Dsv3/Esv3**: SSD-backed, recommended for production
- **Dasv7/Dadsv7**: AMD EPYC 9005, up to 160 vCPUs, 640 GB RAM
- **Dv5/Dsv5**: Intel Ice Lake, no temporary storage (lower entry price)

**Avoid:**
- B-series (burstable)
- Av1-series
- VM sizes with <2 vCPUs or <4 GB RAM for system pools

**Source:** [Virtual Machine Sizes for AKS](https://learn.microsoft.com/en-us/azure/aks/aks-virtual-machine-sizes), [D-family size series](https://learn.microsoft.com/en-us/azure/virtual-machines/sizes/general-purpose/d-family)

#### Virtual Machines Node Pools (New Feature)

Allows mixing up to 5 different VM sizes within a single node pool:
- All sizes must be from the same VM family
- Enables easier resizing with single Azure CLI command
- Better workload-to-instance matching

**Source:** [Use Virtual Machines node pools in AKS](https://learn.microsoft.com/en-us/azure/aks/virtual-machines-node-pools)

### Resource Reservations

#### Memory Reservation Formula

```
Memory Reserved = MIN(
    (20 MB × Max Pods) + 50 MB,
    25% × Total Memory
)
```

**Example Calculations:**

| VM Memory | Max Pods | Formula Applied | Reserved | Allocatable |
|-----------|----------|-----------------|----------|-------------|
| 8 GB | 30 | (20×30)+50 = 650 MB | 650 MB + 100 MB eviction | 7.25 GB (90.6%) |
| 4 GB | 70 | 25% × 4GB = 1,000 MB | 1,000 MB + 100 MB eviction | ~2.9 GB |
| 7 GB | 110 | Tiered calculation | ~2.35 GB | ~4.65 GB (66.4%) |

**Eviction Thresholds:**
- AKS < 1.29: 750 MiB
- AKS ≥ 1.29: 100 MiB (default)

**Windows Nodes:** Additional 2 GB reserved for system processes

**Source:** [Node resource reservations in AKS](https://learn.microsoft.com/en-us/azure/aks/node-resource-reservations)

---

## Google GKE Configuration

### Cluster Modes

| Feature | Autopilot | Standard |
|---------|-----------|----------|
| **Node Management** | Fully managed by Google | User managed |
| **Billing** | Per-pod (CPU, memory, storage) | Per-node |
| **Cluster Type** | Regional only | Regional or Zonal |
| **Scaling** | Automatic | Manual or Cluster Autoscaler |
| **Security** | Built-in best practices | User configured |
| **Marketplace Apps** | Not supported | Supported |
| **Max Control** | Limited | Full control |

**Google Recommendation:** Use Autopilot unless you need fine-grained control, specific hardware (GPUs), or specialized kernel settings.

**Hybrid Option:** Run Autopilot-mode workloads in Standard clusters using ComputeClasses

**Source:** [GKE Autopilot overview](https://docs.cloud.google.com/kubernetes-engine/docs/concepts/autopilot-overview), [Compare Autopilot and Standard features](https://docs.cloud.google.com/kubernetes-engine/docs/resources/autopilot-standard-feature-comparison)

### Machine Type Recommendations

| Series | vCPUs | Memory | Best For |
|--------|-------|--------|----------|
| **E2** | Up to 32 | Up to 128 GB (8 GB/vCPU) | Dev/test, small databases, web serving, lowest cost |
| **N2** | 2-128 | 0.5-8 GB/vCPU | Enterprise apps, databases, gaming servers |
| **C2** | Up to 60 | 4 GB/vCPU | HPC, gaming, latency-sensitive APIs, AI/ML |
| **C2D** | Higher | Variable | Compute-heavy workloads |

**E2 Series:**
- Lowest TCO on Google Cloud
- Up to 31% savings vs N1
- Not suitable for GPU or local SSD requirements

**N2 Series:**
- Higher clock frequency for per-thread performance
- Good for VDI workloads
- Step up from E2 when more power needed

**C2 Series:**
- Up to 3.8 GHz sustained all-core turbo
- NUMA-aligned for optimal performance
- Best for real-time, latency-sensitive applications

**Source:** [Machine families resource and comparison guide](https://docs.cloud.google.com/compute/docs/machine-resource), [General-purpose machine family](https://cloud.google.com/compute/docs/general-purpose-machines)

### Regional vs Zonal Clusters

| Type | Control Plane | Worker Nodes | HA | Cost |
|------|---------------|--------------|-----|------|
| **Zonal** | Single zone | Single zone | No | Lower |
| **Regional** | Multi-zone (3) | Multi-zone | Yes | Higher |

**Production Recommendation:** Use regional clusters for high availability

---

## Common Kubernetes Patterns

### Control Plane Node Sizing

#### Number of Control Plane Nodes

| Nodes | Fault Tolerance | Use Case |
|-------|-----------------|----------|
| 1 | 0 failures | Development, edge |
| 3 | 1 failure | Production (standard) |
| 5 | 2 failures | Large-scale, critical |
| 7 | 3 failures | Maximum HA (etcd limit) |

**Why Odd Numbers?**
- etcd uses Raft consensus requiring majority (quorum)
- Formula: 2n + 1 nodes to tolerate n failures
- Even numbers provide no additional fault tolerance

**When to Use 5 Nodes:**
- Large-scale clusters with high API traffic
- Geographically distributed clusters
- Regulatory/SLA requirements
- Trade-off: increased complexity, latency, maintenance

**Source:** [How Many Nodes for Your Kubernetes Control Plane?](https://thenewstack.io/how-many-nodes-for-your-kubernetes-control-plane/), [Why a 3-Node Kubernetes Control Plane Is the Industry Standard](https://www.anantacloud.com/post/why-a-3-node-kubernetes-control-plane-is-the-industry-standard)

#### Control Plane Sizing by Workload

| Size | Clients | Requests/sec | Data Size | Example |
|------|---------|--------------|-----------|---------|
| **Small** | <100 | <200 | <100 MB | 50-node cluster |
| **Medium** | <500 | <1,000 | <500 MB | 200-node cluster |
| **Large** | <1,500 | <10,000 | <1 GB | 1,000-node cluster |
| **X-Large** | >1,500 | >10,000 | >1 GB | 3,000+ node cluster |

### Kubernetes Cluster Limits

The official Kubernetes scalability thresholds:

| Resource | Limit |
|----------|-------|
| Nodes | 5,000 |
| Pods per node | 110 |
| Total pods | 150,000 |
| Total containers | 300,000 |

**Source:** [Considerations for large clusters](https://kubernetes.io/docs/setup/best-practices/cluster-large/)

### etcd Requirements

| Cluster Size | CPUs | Memory | Storage | IOPS |
|--------------|------|--------|---------|------|
| Small | 2-4 cores | 8-16 GB | SSD | 50 sequential |
| Medium | 4-8 cores | 16-32 GB | SSD | 200 sequential |
| Large | 8-16 cores | 32-64 GB | NVMe SSD | 500 sequential |

**Storage Recommendations:**
- Always use SSD (or NVMe for high loads)
- Avoid NAS, SAN, and spinning disks
- Use striping RAID (not mirroring - etcd handles replication)
- Storage quota default: 2 GB (max recommended: 8 GB)
- Target: <10ms latency for 8KB writes including fdatasync

**Source:** [Hardware recommendations | etcd](https://etcd.io/docs/v3.3/op-guide/hardware/), [Operating etcd clusters for Kubernetes](https://kubernetes.io/docs/tasks/administer-cluster/configure-upgrade-etcd/)

### Infrastructure Nodes

Dedicated nodes for platform services:

| Component | Typical Resources | Notes |
|-----------|-------------------|-------|
| Ingress Controller | 2-4 vCPUs, 4-8 GB RAM | Scale based on traffic |
| Prometheus | 2-8 vCPUs, 8-32 GB RAM | Memory scales with time series |
| Grafana | 1-2 vCPUs, 2-4 GB RAM | Relatively lightweight |
| Logging (EFK/Loki) | 4-8 vCPUs, 8-16 GB RAM | Storage-intensive |

**Best Practices:**
- Use node selectors/taints to isolate infrastructure workloads
- Prevents resource contention with application pods
- Size based on cluster scale and retention requirements

---

## Resource Reservation Formulas

### General Kubernetes Formula

```
Allocatable = Capacity - kube-reserved - system-reserved - eviction-threshold
```

**Visualization:**
```
┌─────────────────────────────────┐
│         Node Capacity           │
├─────────────────────────────────┤
│         kube-reserved           │  ← kubelet, container runtime
├─────────────────────────────────┤
│        system-reserved          │  ← OS, sshd, NetworkManager
├─────────────────────────────────┤
│       eviction-threshold        │  ← Memory eviction buffer
├─────────────────────────────────┤
│                                 │
│         ALLOCATABLE             │  ← Available for pods
│     (available for pods)        │
│                                 │
└─────────────────────────────────┘
```

**Source:** [Reserve Compute Resources for System Daemons](https://kubernetes.io/docs/tasks/administer-cluster/reserve-compute-resources/)

### Provider-Specific Formulas

#### Azure AKS Memory

```python
def aks_memory_reserved(total_memory_gb, max_pods):
    formula1 = (20 * max_pods) + 50  # MB
    formula2 = total_memory_gb * 1024 * 0.25  # 25% in MB
    kube_reserved = min(formula1, formula2)
    eviction_threshold = 100  # MB (AKS 1.29+)
    return kube_reserved + eviction_threshold
```

#### Azure AKS CPU

CPU reservations scale with core count (see Microsoft documentation for exact tiers).

#### AWS EKS Max Pods

```python
def eks_max_pods(num_enis, ips_per_eni, prefix_delegation=False):
    if prefix_delegation:
        return 110  # Karpenter default with prefix delegation
    return (num_enis * (ips_per_eni - 1)) + 2
```

### Worker Node Sizing Formula (Generic)

```python
def calculate_workers(pod_count, pods_per_node=100, failure_tolerance=1):
    """
    Calculate minimum worker nodes needed.

    Args:
        pod_count: Total pods to run
        pods_per_node: Max pods per node (default 100, limit 110)
        failure_tolerance: Number of node failures to tolerate

    Returns:
        Recommended worker node count
    """
    base_nodes = math.ceil(pod_count / pods_per_node)
    return base_nodes + failure_tolerance

# Examples:
# 100 pods, 1 failure tolerance = 2 workers
# 200 pods, 1 failure tolerance = 3 workers
# 500 pods, 2 failure tolerance = 7 workers
```

### Available Resources (Kublr Formula)

```python
def available_memory_gb(num_nodes, memory_per_node_gb, has_logging=False, has_monitoring=False):
    """Calculate available memory for workloads."""
    base = num_nodes * memory_per_node_gb
    node_overhead = num_nodes * 0.7  # GB per node
    logging_overhead = 9 if has_logging else 0  # Self-hosted logging
    monitoring_overhead = 2.9 if has_monitoring else 0  # Self-hosted monitoring
    fixed_overhead = 0.4 + 2.0  # Central monitoring agent

    return base - node_overhead - logging_overhead - monitoring_overhead - fixed_overhead

def available_cpu(num_nodes, vcpu_per_node, has_logging=False, has_monitoring=False):
    """Calculate available CPU for workloads."""
    base = num_nodes * vcpu_per_node
    node_overhead = num_nodes * 0.5  # vCPU per node
    logging_overhead = 1 if has_logging else 0
    monitoring_overhead = 1.4 if has_monitoring else 0
    fixed_overhead = 0.1 + 0.7  # Central monitoring agent

    return base - node_overhead - logging_overhead - monitoring_overhead - fixed_overhead
```

**Source:** [Kubernetes Cluster Hardware Recommendations](https://docs.kublr.com/installation/hardware-recommendation/)

---

## Sizing Recommendations Summary

### Quick Reference: Instance Types by Provider

| Workload | AWS EKS | Azure AKS | GCP GKE |
|----------|---------|-----------|---------|
| Dev/Test | t3.medium | Standard_B2s | e2-medium |
| Small Production | m6i.large | Standard_D2s_v3 | e2-standard-2 |
| Medium Production | m6i.xlarge | Standard_D4s_v3 | n2-standard-4 |
| Large Production | m6i.2xlarge | Standard_D8s_v3 | n2-standard-8 |
| Memory-Intensive | r6i.xlarge | Standard_E4s_v3 | n2-highmem-4 |
| Compute-Intensive | c6i.xlarge | Standard_F4s_v2 | c2-standard-4 |

### Quick Reference: Control Plane Configuration

| Cluster Size | Nodes | Control Plane | etcd |
|--------------|-------|---------------|------|
| Development | 1-10 | Managed (free tier) | Included |
| Small Prod | 10-50 | Managed (standard) | 3 replicas |
| Medium Prod | 50-200 | Managed (standard) | 3-5 replicas |
| Large Prod | 200-1000 | Managed (premium) | 5 replicas |
| Enterprise | 1000+ | Provisioned tier | 5-7 replicas |

### Quick Reference: Resource Reservations

| Node Size | Approx. Memory Reserved | Approx. CPU Reserved |
|-----------|------------------------|---------------------|
| 4 GB RAM | 1.0-1.1 GB (25-27%) | 0.1-0.3 vCPU |
| 8 GB RAM | 0.7-0.8 GB (9-10%) | 0.2-0.4 vCPU |
| 16 GB RAM | 0.9-1.1 GB (6-7%) | 0.3-0.5 vCPU |
| 32 GB RAM | 1.2-1.5 GB (4-5%) | 0.4-0.6 vCPU |
| 64 GB RAM | 1.8-2.2 GB (3-4%) | 0.5-0.8 vCPU |

**Note:** Larger nodes have better resource efficiency (lower % overhead).

---

## Sources and References

### AWS EKS
- [Amazon EKS Pricing](https://aws.amazon.com/eks/pricing/)
- [Choose an optimal Amazon EC2 node instance type](https://docs.aws.amazon.com/eks/latest/userguide/choosing-instance-type.html)
- [Managed node groups](https://docs.aws.amazon.com/eks/latest/userguide/managed-node-groups.html)
- [EKS Best Practices - Data Plane](https://aws.github.io/aws-eks-best-practices/scalability/docs/data-plane/)
- [EKS Best Practices - VPC CNI](https://aws.github.io/aws-eks-best-practices/networking/vpc-cni/)
- [Amazon VPC CNI plugin increases pods per node limits](https://aws.amazon.com/blogs/containers/amazon-vpc-cni-increases-pods-per-node-limits/)
- [EKS Workshop - Instance Type Diversification](https://www.eksworkshop.com/docs/fundamentals/managed-node-groups/spot/instance-diversification)

### Azure AKS
- [Azure AKS Pricing Tiers](https://learn.microsoft.com/en-us/azure/aks/free-standard-pricing-tiers)
- [AKS Pricing](https://azure.microsoft.com/en-us/pricing/details/kubernetes-service/)
- [Virtual Machine Sizes for AKS](https://learn.microsoft.com/en-us/azure/aks/aks-virtual-machine-sizes)
- [Node resource reservations in AKS](https://learn.microsoft.com/en-us/azure/aks/node-resource-reservations)
- [Use system node pools in AKS](https://learn.microsoft.com/en-us/azure/aks/use-system-pools)
- [Use Virtual Machines node pools in AKS](https://learn.microsoft.com/en-us/azure/aks/virtual-machines-node-pools)
- [D-family size series](https://learn.microsoft.com/en-us/azure/virtual-machines/sizes/general-purpose/d-family)

### Google GKE
- [GKE Autopilot overview](https://docs.cloud.google.com/kubernetes-engine/docs/concepts/autopilot-overview)
- [Compare Autopilot and Standard features](https://docs.cloud.google.com/kubernetes-engine/docs/resources/autopilot-standard-feature-comparison)
- [About GKE modes of operation](https://docs.cloud.google.com/kubernetes-engine/docs/concepts/choose-cluster-mode)
- [Machine families resource and comparison guide](https://docs.cloud.google.com/compute/docs/machine-resource)
- [General-purpose machine family](https://cloud.google.com/compute/docs/general-purpose-machines)

### Kubernetes.io
- [Considerations for large clusters](https://kubernetes.io/docs/setup/best-practices/cluster-large/)
- [Reserve Compute Resources for System Daemons](https://kubernetes.io/docs/tasks/administer-cluster/reserve-compute-resources/)
- [Operating etcd clusters for Kubernetes](https://kubernetes.io/docs/tasks/administer-cluster/configure-upgrade-etcd/)
- [Node Autoscaling](https://kubernetes.io/docs/concepts/cluster-administration/node-autoscaling/)
- [Cluster Autoscaler FAQ](https://github.com/kubernetes/autoscaler/blob/master/cluster-autoscaler/FAQ.md)

### etcd
- [Hardware recommendations | etcd](https://etcd.io/docs/v3.3/op-guide/hardware/)

### Other Sources
- [How Many Nodes for Your Kubernetes Control Plane? - The New Stack](https://thenewstack.io/how-many-nodes-for-your-kubernetes-control-plane/)
- [Kubernetes Cluster Hardware Recommendations - Kublr](https://docs.kublr.com/installation/hardware-recommendation/)
- [Architecting Kubernetes clusters - choosing a worker node size](https://learnkube.com/kubernetes-node-size)
- [Allocatable memory and CPU in Kubernetes Nodes](https://learnkube.com/allocatable-resources)
