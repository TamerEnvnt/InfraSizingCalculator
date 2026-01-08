# Kubernetes On-Premises Distribution Sizing Guide

This document provides comprehensive node sizing recommendations for on-premises and private cloud Kubernetes distributions. All information is sourced from official vendor documentation.

---

## Table of Contents

1. [Red Hat OpenShift](#red-hat-openshift)
2. [Rancher (RKE/RKE2/K3s)](#rancher-rkerke2k3s)
3. [VMware Tanzu](#vmware-tanzu)
4. [Canonical MicroK8s](#canonical-microk8s)
5. [Mirantis Kubernetes Engine (MKE)](#mirantis-kubernetes-engine-mke)
6. [k0s](#k0s)
7. [Private Cloud Patterns](#private-cloud-patterns)
8. [Low-Code Platform Sizing](#low-code-platform-sizing-mendixoutsystems)
9. [General Best Practices](#general-best-practices)

---

## Red Hat OpenShift

### Minimum Node Requirements (OpenShift 4.x)

| Node Type | vCPU | Memory | Storage |
|-----------|------|--------|---------|
| Control Plane (Minimum) | 4 | 16 GB | 100 GB |
| Worker Node (Minimum) | 2 | 8 GB | 100 GB |
| Infrastructure Node | 4 | 16 GB | 100 GB |
| Single Node OpenShift (SNO) | 8 | 16 GB | 120 GB |

**Source**: [Red Hat OpenShift Documentation](https://docs.openshift.com/container-platform/3.11/install/prerequisites.html)

### Control Plane Node Sizing by Cluster Size

The control plane node size is linked to the number of worker nodes. Red Hat recommends the following sizing based on cluster density testing:

| Worker Nodes | Control Plane vCPU | Control Plane Memory |
|--------------|-------------------|---------------------|
| Up to 25 | 4 | 16 GB |
| 26-100 | 8 | 32 GB |
| 101-250 | 16 | 64 GB |
| 251-500 | 16 | 96 GB |

**Important**: Keep overall CPU and memory usage on control plane nodes at **60% or less** of total capacity to handle failover scenarios.

**Source**: [OpenShift Scalability and Performance](https://docs.openshift.com/container-platform/4.8/scalability_and_performance/recommended-host-practices.html)

### Infrastructure Node Requirements

Infrastructure nodes host platform services such as:
- Ingress Router
- Container Image Registry
- Cluster Metrics/Monitoring (Prometheus, Grafana, Alertmanager)
- Cluster Logging (Elasticsearch)

**Recommendations**:
- Deploy **3 infrastructure nodes** for production (one per availability zone)
- Infrastructure nodes are **not counted** toward Red Hat subscription vCPU counts
- Prometheus is highly memory-intensive; size accordingly based on:
  - Number of nodes
  - Number of objects
  - Metrics scraping interval
  - Cluster age

**Source**: [Red Hat Infrastructure Nodes](https://access.redhat.com/solutions/5034771)

### High Availability Configuration

| Configuration | Control Plane Nodes | Infrastructure Nodes | Worker Nodes |
|---------------|--------------------|--------------------|--------------|
| Minimum HA | 3 | 2 | 2+ |
| Production Recommended | 3 | 3 | 3+ |
| Three-Node Compact (SNO-HA) | 3 (schedulable) | N/A | N/A |

### OpenShift Data Foundation Storage Nodes

For clusters with OpenShift Data Foundation (ODF):

| Component | Minimum Nodes | CPU per Node | Memory per Node |
|-----------|---------------|--------------|-----------------|
| ODF Storage | 3 | 10 cores | 24 GB |

**Source**: [Red Hat OpenShift Data Foundation](https://docs.redhat.com/en/documentation/red_hat_openshift_data_foundation/4.14/html/planning_your_deployment/infrastructure-requirements_rhodf)

---

## Rancher (RKE/RKE2/K3s)

### RKE2 Requirements

**Minimum Requirements**:

| Component | CPU | Memory | Notes |
|-----------|-----|--------|-------|
| Server Node (Minimum) | 2 | 4 GB | Recommended: 4 CPU, 8 GB RAM |
| Agent Node (Minimum) | 1 | 2 GB | Based on workload |

**Server Sizing by Agent Count**:

| Server CPU | Server RAM | Maximum Agent Nodes |
|------------|-----------|---------------------|
| 2 | 4 GB | 0-225 |
| 4 | 8 GB | 226-450 |
| 8 | 16 GB | 451-1,300 |
| 16+ | 32 GB | 1,300+ |

**Source**: [RKE2 Requirements](https://docs.rke2.io/install/requirements)

**Storage**: SSD recommended for etcd performance
**Network Ports**:
- 6443/TCP: Kubernetes API
- 9345/TCP: RKE2 supervisor API
- 10250/TCP: Kubelet metrics
- 2379-2381/TCP: etcd
- 8472/UDP: VXLAN (Flannel)

### K3s Requirements (Lightweight)

**Minimum Requirements**:

| Node Type | CPU | Memory | Notes |
|-----------|-----|--------|-------|
| Server (Control Plane) | 2 cores | 2 GB | Minimum for control plane |
| Agent (Worker) | 1 core | 512 MB | Absolute minimum |

**Server Sizing by Deployment Scale**:

| Deployment Size | Node Count | Server vCPU | Server RAM |
|-----------------|------------|-------------|------------|
| Small | Up to 10 | 2 | 4 GB |
| Medium | Up to 100 | 4 | 8 GB |
| Large | Up to 250 | 8 | 16 GB |
| X-Large | Up to 500 | 16 | 32 GB |
| XX-Large | 500+ | 32 | 64 GB |

**HA Scaling**: Three-node HA clusters can support approximately 50% more agents than single-server setups.

**Source**: [K3s Requirements](https://docs.k3s.io/installation/requirements)

**Important Notes**:
- K3s binary is less than 100 MB
- Runs in as little as 540 MB memory
- **SSD required** - SD cards and eMMC cannot handle etcd I/O
- Supports x86_64, ARMv7, and ARM64

### Rancher Management Server Requirements

For running Rancher Management:

| Component | CPU | Memory | Notes |
|-----------|-----|--------|-------|
| Rancher Node (Minimum) | 2 | 8 GB | Dedicated cluster recommended |
| Rancher Node (Recommended) | 4 | 16 GB | For larger deployments |

**Best Practice**: The Rancher management cluster should be dedicated to running only Rancher.

**Source**: [Rancher Installation Requirements](https://ranchermanager.docs.rancher.com/getting-started/installation-and-upgrade/installation-requirements)

---

## VMware Tanzu

### Tanzu Kubernetes Grid Integrated Edition (TKGI)

**Control Plane Node Sizing**:

The control plane node VM size is linked to the number of worker nodes:

| Maximum Worker Nodes | Control Plane vCPU | Control Plane Memory | Control Plane Disk |
|---------------------|-------------------|---------------------|-------------------|
| Up to 5 | 2 | 8 GB | 40 GB |
| 6-50 | 4 | 16 GB | 40 GB |
| 51-100 | 8 | 32 GB | 80 GB |
| 101-250 | 16 | 64 GB | 160 GB |

**Worker Node Sizing Guidelines**:
- Maximum **100 pods per worker node** (Kubernetes recommendation)
- Rule of thumb: **1 vCPU and 10 GB memory per 10 running pods** for generic workloads

**Source**: [TKGI VM Sizing](https://docs.vmware.com/en/VMware-Tanzu-Kubernetes-Grid-Integrated-Edition/1.17/tkgi/GUID-vm-sizing.html)

### Tanzu Kubernetes Grid (TKG) Plans

| Plan | Control Plane Nodes | Worker Nodes | Use Case |
|------|--------------------|--------------| ---------|
| dev | 1 | 1 | Development |
| prod | 3 | 3 | Production |
| Custom | Uneven number | Variable | Special requirements |

### Tanzu Service Mesh Requirements

For clusters running Tanzu Service Mesh:

| Component | Nodes | CPU per Node | Memory per Node |
|-----------|-------|--------------|-----------------|
| Data Plane | 3+ | 3 | 6 GB |
| DaemonSets | Per worker | 250m | 650 MB |

**Source**: [VMware Tanzu Documentation](https://docs.vmware.com/en/VMware-Tanzu-Kubernetes-Grid/index.html)

### vSphere with Tanzu Requirements

**Supervisor Cluster**:
- Minimum 3 ESXi hosts
- NSX-T networking (recommended) or vSphere Distributed Switch
- vSphere 7.0 U3+ for CSI snapshot capability

**Storage**:
- vSAN, NFS, or iSCSI datastores
- Each persistent volume maps to a VMDK
- Maximum 15 PVs per SCSI controller per node

**Source**: [vSphere with Tanzu](https://docs.vmware.com/en/VMware-vSphere/7.0/vmware-vsphere-with-tanzu/GUID-B1388E77-2EEC-41E2-8681-5AE549D50C77.html)

---

## Canonical MicroK8s

### System Requirements

| Deployment Type | CPU | Memory | Disk | Notes |
|-----------------|-----|--------|------|-------|
| Absolute Minimum | 1 | 540 MB | 10 GB | Control plane only |
| Single Node | 2 | 2 GB | 20 GB | With basic workloads |
| Production Node | 2+ | 4 GB+ | 20 GB+ | Recommended minimum |

**Key Features**:
- Memory footprint reduced by **32.5%** since v1.21
- Runs on devices with less than 1 GB memory
- Idle usage: ~500-600 MB RAM (control plane)
- Single binary package: 192 MB

**Source**: [MicroK8s Documentation](https://microk8s.io/docs)

### Add-on Specific Requirements

| Add-on | Additional Memory | Additional CPU | Notes |
|--------|------------------|----------------|-------|
| Mayastor (Storage) | 4 GB per node | 1 dedicated core | High-performance storage |
| Istio | 2 GB | 1 core | Service mesh |
| GPU | Per GPU requirements | N/A | NVIDIA drivers required |

---

## Mirantis Kubernetes Engine (MKE)

### MKE 3.x Requirements

**Manager Nodes**:

| Size | vCPU | Memory | Storage | Maximum Workers |
|------|------|--------|---------|-----------------|
| Small | 2 | 8 GB | 40 GB | 100 |
| Medium | 4 | 16 GB | 80 GB | 250 |
| Large | 8 | 32 GB | 160 GB | 500 |

**Worker Nodes**:

| Size | vCPU | Memory | Storage | Notes |
|------|------|--------|---------|-------|
| Small | 2 | 4 GB | 40 GB | Light workloads |
| Medium | 4 | 8 GB | 80 GB | Standard workloads |
| Large | 8 | 16 GB | 160 GB | Heavy workloads |

**Source**: [MKE Hardware Requirements](https://docs.mirantis.com/mke/3.4/common/mke-hw-reqs.html)

### MKE 4k Requirements

MKE 4k uses k0s as the underlying Kubernetes distribution.

**Supported Operating Systems**:
- Ubuntu 20.04 Linux
- Ubuntu 22.04 Linux
- RHEL 8.10
- Rocky Linux 9.4

**Kubelet Resource Reservations**:

| Node Type | CPU | Ephemeral Storage | Memory |
|-----------|-----|-------------------|--------|
| Worker | 50m | 500 Mi | 300 Mi |
| Manager | 250m | 4 Gi | 2 Gi |

**Source**: [MKE 4k System Requirements](https://docs.mirantis.com/mke-docs/docs/getting-started/system-requirements/)

---

## k0s

### System Requirements

**Controller Node**:

| Cluster Size | CPU | Memory | Notes |
|--------------|-----|--------|-------|
| Small (<10 nodes) | 1 | 1 GB | Minimum |
| Medium (10-100 nodes) | 2 | 2 GB | Recommended |
| Large (100+ nodes) | 4+ | 4 GB+ | Scale accordingly |

**Worker Node**:

| Component | CPU | Memory |
|-----------|-----|--------|
| Kubelet + k0s agent | 200m | 256 MB |
| System overhead | 100m | 128 MB |
| Workload dependent | Variable | Variable |

**Storage**: SSD required for control plane (etcd performance critical)

**Source**: [k0s System Requirements](https://docs.k0sproject.io/v1.21.0+k0s.0/system-requirements/)

---

## Private Cloud Patterns

### vSphere Integration Requirements

**vSphere Version**: 6.7U3 or later required for CSI/CPI support

**Node Configuration**:
- Enable `disk.EnableUUID` option on all worker nodes
- Minimum 3 control plane nodes for HA
- vSphere 7.0 U3+ for CSI snapshot capability

**Networking**:
- Port 443/TCP to vCenter API required
- NSX-T recommended for advanced networking
- vSphere Distributed Switch minimum

**Storage (CSI)**:
- Each PV maps to a VMDK file
- Maximum 15 PVs per SCSI controller
- VM Storage Policies define datastore mapping

**Source**: [vSphere Cloud Provider](https://cloud-provider-vsphere.sigs.k8s.io/)

### Bare Metal Best Practices

**Hardware Recommendations**:

| Component | Specification | Notes |
|-----------|---------------|-------|
| CPU | 8-16 cores per node | Single-socket servers for scale-out |
| Memory | 64-128 GB per node | Higher memory-to-CPU ratio preferred |
| Network | 10 Gbps minimum | 25 Gbps for high-throughput |
| Storage (OS) | 1-2 TB SSD | For OS and container runtime |
| Storage (etcd) | Dedicated NVMe | Critical for control plane |

**Source**: [Portainer Bare Metal Guide](https://www.portainer.io/blog/building-a-bare-metal-kubernetes-cluster-hardware-specifications-and-best-practices)

### N+1 Redundancy Planning

**Capacity Guidelines**:
- Keep nodes at **80% capacity maximum** to handle failover
- Reserve one node's worth of capacity for failures
- Minimum **3 control plane nodes** for HA
- Minimum **3 worker nodes** for fault tolerance

**Anti-Affinity Rules**:
- Use pod anti-affinity to spread critical workloads
- Use node affinity to control placement
- Avoid single points of failure

### etcd Storage Requirements

etcd is the most storage-sensitive component of Kubernetes.

| Cluster Size | Sequential IOPS | Latency Target |
|--------------|-----------------|----------------|
| Small (<50 nodes) | 50 IOPS | 20 ms |
| Medium (50-250 nodes) | 500 IOPS | 10 ms |
| Large (250+ nodes) | 500+ IOPS | 2 ms |

**Storage Recommendations**:
- **NVMe SSDs** preferred
- **SSD minimum** required
- **Avoid**: NAS, SAN, spinning disks, network-attached storage
- Dedicated etcd drives recommended

**Source**: [etcd Hardware Recommendations](https://etcd.io/docs/v3.3/op-guide/hardware/)

---

## Low-Code Platform Sizing (Mendix/OutSystems)

### Mendix on Kubernetes

**Resource Configuration**:
- CPU specified in millicores (e.g., 500m = 0.5 CPU)
- Memory specified in MB/GB
- Higher memory-to-CPU ratio is cost-efficient

**Node Sizing Considerations**:
- Mendix containers need relatively more memory vs. CPU
- Choose instance types with higher memory-to-CPU ratio (e.g., AWS E2s_v3)
- Mendix Runtime is Java-based; pre-allocates memory

**Autoscaling**:
- HPA (Horizontal Pod Autoscaler) supported
- VPA (Vertical Pod Autoscaler) **not recommended** for Mendix
- Memory-based autoscaling discouraged (Java memory behavior)

**Source**: [Mendix on Kubernetes](https://docs.mendix.com/developerportal/deploy/private-cloud/)

### OutSystems on Kubernetes

**Windows Container Requirements**:
- Windows Server 2019 or later worker nodes
- Linux control plane nodes
- Flannel CNI plugin recommended

**Sizing Guidelines**:
- Standard Kubernetes limits apply (110 pods per node)
- Windows containers have larger image footprint
- Plan for additional storage for Windows container images

**Source**: [OutSystems Container Deployment](https://www.outsystems.com/blog/posts/deploying-kubernetes-to-run-windows-containers/)

---

## General Best Practices

### Node Sizing Philosophy

1. **Prefer smaller nodes over large servers**
   - Better fault isolation
   - More granular scaling
   - Reduced blast radius

2. **Standardize hardware**
   - Simplifies management and automation
   - Improves performance predictability
   - Easier spare parts management

3. **Capacity planning**
   - 80% utilization maximum
   - N+1 redundancy for critical workloads
   - Reserve headroom for upgrades

### Resource Reservations

| Component | CPU Reservation | Memory Reservation |
|-----------|----------------|-------------------|
| System (kubelet, container runtime) | 100-500m | 500 MB - 2 GB |
| Kubernetes system pods | 100-250m | 256 MB - 1 GB |
| Workload buffer | 10-20% | 10-20% |

### Storage Guidelines

| Storage Type | Use Case | Performance |
|--------------|----------|-------------|
| NVMe SSD | etcd, control plane | Best |
| SSD | Worker node OS, container runtime | Good |
| HDD | Archival, cold storage only | Avoid for active workloads |
| NAS/SAN | Persistent volumes (with caution) | Variable |

### Network Requirements

| Traffic Type | Bandwidth | Notes |
|--------------|-----------|-------|
| Control plane | 1 Gbps | Minimum |
| East-West (pod-to-pod) | 10 Gbps | Recommended |
| North-South (ingress/egress) | 10-25 Gbps | Based on workload |

---

## Quick Reference Summary

### Minimum Production Cluster

| Component | Nodes | vCPU/Node | Memory/Node | Storage/Node |
|-----------|-------|-----------|-------------|--------------|
| Control Plane | 3 | 4 | 16 GB | 100 GB SSD |
| Worker | 3 | 4 | 16 GB | 200 GB SSD |
| Infrastructure | 3 | 4 | 16 GB | 100 GB SSD |

### Distribution Comparison

| Distribution | Minimum Memory | Target Use Case | HA Minimum |
|--------------|---------------|-----------------|------------|
| OpenShift | 16 GB | Enterprise | 3 control + 3 worker |
| RKE2 | 4 GB | General purpose | 3 server + 3 agent |
| K3s | 512 MB | Edge/IoT | 3 server |
| MicroK8s | 540 MB | Development/Edge | 3 nodes |
| Tanzu (TKGI) | 8 GB | Enterprise/vSphere | 3 control + 3 worker |
| MKE | 8 GB | Enterprise | 3 manager + 3 worker |
| k0s | 1 GB | Lightweight/Edge | 3 controller |

---

## Sources

- [Red Hat OpenShift Documentation](https://docs.openshift.com/)
- [RKE2 Documentation](https://docs.rke2.io/)
- [K3s Documentation](https://docs.k3s.io/)
- [VMware Tanzu Documentation](https://docs.vmware.com/en/VMware-Tanzu-Kubernetes-Grid/index.html)
- [MicroK8s Documentation](https://microk8s.io/docs)
- [Mirantis MKE Documentation](https://docs.mirantis.com/mke/)
- [k0s Documentation](https://docs.k0sproject.io/)
- [etcd Hardware Guide](https://etcd.io/docs/v3.3/op-guide/hardware/)
- [Kubernetes Large Cluster Considerations](https://kubernetes.io/docs/setup/best-practices/cluster-large/)
- [Mendix Private Cloud](https://docs.mendix.com/developerportal/deploy/private-cloud/)

---

*Last Updated: January 2026*
