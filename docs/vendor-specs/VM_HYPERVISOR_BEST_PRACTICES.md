# Virtual Machine and Hypervisor Configuration Best Practices

This document consolidates best practices from VMware, Microsoft, Red Hat, and Proxmox for VM and hypervisor configuration.

---

## Table of Contents

1. [VMware vSphere](#1-vmware-vsphere)
2. [Microsoft Hyper-V](#2-microsoft-hyper-v)
3. [KVM/Proxmox](#3-kvmproxmox)
4. [Resource Overcommit Best Practices](#4-resource-overcommit-best-practices)
5. [VM Sizing Templates](#5-vm-sizing-templates)
6. [Host Sizing Guidelines](#6-host-sizing-guidelines)
7. [Low-Code Platform VM Requirements](#7-low-code-platform-vm-requirements)
8. [Sources](#8-sources)

---

## 1. VMware vSphere

### 1.1 Host Sizing Recommendations

#### Minimum ESXi 8.0 Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| CPU Cores | 2 cores | 4+ cores |
| RAM | 8 GB | 12+ GB (production) |
| Boot Device | 32 GB | 128 GB SSD/NVMe |
| Network | 1 GbE | Multiple 10+ GbE |
| Boot Mode | UEFI | UEFI (Legacy BIOS limited support) |

#### CPU Requirements

- Intel Skylake or newer, AMD EPYC (Naples) or newer
- NX/XD bit must be enabled in BIOS
- Hardware virtualization (Intel VT-x or AMD RVI) must be enabled
- ESXi 8.0 Update 2 requires XSAVE instruction support

#### Storage Requirements

- Boot device must deliver at least 100 MB/s write speed
- Support 128 TBW (terabytes written) for reliability
- Boot device must not be shared between ESXi hosts
- USB sticks and SD cards (4-8 GB) are no longer sufficient

### 1.2 VM Sizing Best Practices

#### General Guidelines

1. **Right-size VMs**: Start small and scale up based on actual usage
2. **vCPU allocation**: Begin with fewer vCPUs than physical cores
3. **Memory**: Avoid over-provisioning; monitor active memory usage
4. **NUMA awareness**: Keep multiprocessor VMs sized to fit within a single NUMA node

#### NUMA Considerations

- ESXi NUMA scheduling activates on systems with at least 4 CPU cores
- At least 2 CPU cores per NUMA node required for optimization
- VMs spanning NUMA nodes have reduced performance
- For most workloads, limit VMs to 4-24 cores for optimal capacity returns

#### vCPU Recommendations

| Workload Type | Recommended vCPU Range |
|---------------|------------------------|
| General workloads | 2-8 vCPUs |
| Database servers | 4-16 vCPUs |
| Application servers | 2-8 vCPUs |
| Large workloads | 8-24 vCPUs (diminishing returns above 16) |

### 1.3 CPU Overcommit Ratios

| Environment | Ratio | Notes |
|-------------|-------|-------|
| Production (Critical) | 1:1 to 2:1 | No contention acceptable |
| Production (General) | 2:1 to 4:1 | Monitor CPU ready time |
| Test/QA | 4:1 to 6:1 | Can tolerate some contention |
| Development | 6:1 to 8:1 | Higher flexibility acceptable |
| VDI | 8:1 to 10:1 | Light desktop workloads |

#### Key Metrics to Monitor

- **CPU utilization**: Should be ≤80% on average; >90% triggers alert
- **CPU ready**: Should be <5% (indicates scheduling delays)
- **Co-stop**: Monitor for multi-vCPU VMs

### 1.4 Memory Overcommit

#### Memory Reclamation Techniques (In Order of Performance Impact)

1. **Transparent Page Sharing (TPS)**: Minimal impact, deduplicates identical memory pages
2. **Ballooning**: ~3% performance impact, guest-aware memory reclamation
3. **Memory Compression**: Moderate impact, compresses pages before swapping
4. **Host Swapping**: ~34% throughput loss - avoid at all costs

#### Memory Recommendations

| Environment | Recommendation |
|-------------|----------------|
| Production (Critical) | No overcommit; use memory reservations |
| Production (General) | Conservative overcommit with ballooning |
| Test/QA | Moderate overcommit acceptable |
| Development | Higher overcommit with monitoring |

#### Best Practices

- Monitor "Swapped" and "Compressed" values in vSphere Client
- Non-zero swap values indicate memory pressure
- Reserve 4-8 physical cores for ESXi system processes
- Use flash devices for vSphere Flash Infrastructure layer if overcommitting

### 1.5 DRS (Distributed Resource Scheduler) Configuration

#### Automation Levels

| Level | Initial Placement | Migration |
|-------|-------------------|-----------|
| Manual | Last DRS location | Requires approval |
| Partially Automated | Automated | Requires approval |
| Fully Automated | Automated | Automated |

#### DRS Best Practices

1. **Enable DRS on all production clusters** for automatic load balancing
2. **Set migration threshold** based on workload sensitivity:
   - Conservative (Priority 1-2): Critical workloads
   - Moderate (Priority 1-3): General production
   - Aggressive (Priority 1-5): Development/test
3. **Create DRS rules** for:
   - Affinity: Keep related VMs together
   - Anti-affinity: Separate VMs for fault tolerance
   - VM-to-Host: Pin VMs to specific hosts if needed

#### Advanced Options

- **VM Distribution**: Spread VMs evenly for availability
- **Memory Metric for Load Balancing**: Use consumed memory (not active) when memory is not overcommitted
- **Predictive DRS**: Requires VMware Aria Operations for forecast-based migrations

### 1.6 HA (High Availability) Configuration

#### Cluster Requirements

- Minimum 2 ESXi hosts (3+ recommended)
- Identical hardware configuration
- Same ESXi version and patch level
- Shared storage accessible by all nodes

#### HA Architecture

- **Master-slave model**: One host coordinates, others are secondary
- **Heartbeat mechanisms**: Network and datastore heartbeats
- **VM monitoring**: Leverages VMware Tools for guest OS health

#### HA Settings

| Setting | Recommendation |
|---------|----------------|
| Host Isolation Response | Power off and restart VMs |
| VM Restart Priority | High for critical VMs |
| Host Failure Response | Restart VMs |
| Admission Control | Enable; reserve capacity for failures |

#### Admission Control Policy Options

1. **Cluster resource percentage**: Reserve % of cluster capacity
2. **Dedicated failover hosts**: Designate specific hosts
3. **Slot policy**: Calculate slots based on VM requirements

---

## 2. Microsoft Hyper-V

### 2.1 Host Requirements

#### Windows Server 2022 Hyper-V Minimum Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| CPU | 64-bit with SLAT | Intel VT-x or AMD-V enabled |
| RAM | 4 GB | 8+ GB for host + VM memory |
| Storage | 32 GB | SSD for optimal performance |
| Network | 1 GbE | Multiple NICs for traffic separation |

#### CPU Features Required

- Hardware-assisted virtualization (Intel VT or AMD-V)
- Hardware-enforced Data Execution Prevention (DEP)
- Intel XD bit or AMD NX bit enabled

#### Host Memory Reservation

- Reserve at least 512 MB for host OS (2 GB recommended)
- Add 32 MB per VM for first 1 GB of RAM assigned
- Add 8 MB for each additional GB per VM

### 2.2 VM Sizing

#### Memory Allocation

- Size VM memory as you would for physical servers
- Account for expected load at ordinary and peak times
- Insufficient memory significantly increases response times and CPU/I/O usage

#### Dynamic Memory vs Static Memory

| Feature | Dynamic Memory | Static Memory |
|---------|----------------|---------------|
| Use Case | Variable workloads | Predictable workloads |
| Memory Allocation | Adjusts automatically | Fixed allocation |
| Startup RAM | Configurable minimum | Full allocation at boot |
| Buffer | Configurable percentage | Not applicable |

#### Dynamic Memory Limitations

- Cannot use Virtual NUMA with Dynamic Memory
- Not recommended for database applications
- Guest must support Integration Services

### 2.3 Overcommit Capabilities

#### Key Difference from VMware

Microsoft Hyper-V does **not** allow memory overcommitment in the same way VMware does:
- Hyper-V reserved memory is part of host's total available memory
- VMware allows allocating more memory than physically available

#### Dynamic Memory as Alternative

- Reclaims unused memory from idle VMs
- Redistributes memory to VMs that need it
- Host distributes memory in one-second intervals
- Configurable buffer percentage per VM

#### Best Practices

1. Start with minimal acceptable memory
2. Increase only after demonstrating need
3. Monitor memory pressure indicators
4. Avoid overcommitment for SQL Server and databases

### 2.4 Failover Clustering

#### Cluster Requirements

- All nodes should be configured identically (except names/IPs)
- Same patch levels, networks, drivers, firmware
- Storage options: SAN, DAS, Storage Spaces Direct (S2D), SMB 3.0

#### Network Configuration

Essential networks for Hyper-V clustering:

| Network | Purpose |
|---------|---------|
| Management | Host administration |
| Cluster | Node-to-node communication |
| Live Migration | VM movement between hosts |
| Storage | iSCSI/SMB traffic |
| VM Network | Guest VM traffic |

#### Network Best Practices

1. **Team physical NICs** in management OS
2. **Isolate management traffic** with firewall/IPsec
3. **X-pattern cabling**: No single point of failure
4. Use separate networks for different traffic types

#### Domain Controller Considerations

- **Never place DCs on cluster nodes** hosting production VMs
- DCs should run on dedicated servers or separate highly-available hosts
- Cluster nodes require AD authentication at all times

#### Cluster-Aware Updating (CAU)

- Automates patching of cluster nodes
- Migrates VMs off host before patching
- Rehydrates host after successful update
- Reduces maintenance window impact

### 2.5 Storage Options

| Option | Description | Use Case |
|--------|-------------|----------|
| SAN (FC/iSCSI) | Traditional shared storage | Enterprise environments |
| Storage Spaces Direct | Software-defined storage | HCI deployments |
| SMB 3.0 File Shares | Network-based shared storage | File server integration |
| Direct-Attached Storage | Local disks aggregated | Small clusters |

---

## 3. KVM/Proxmox

### 3.1 Host Sizing

#### Proxmox VE Minimum Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| CPU | 64-bit with VT-x/AMD-V | Multi-core server CPU |
| RAM | 2 GB | 8+ GB for host + VMs |
| Storage | 32 GB | SSD/NVMe for OS + separate VM storage |
| Network | 1 GbE | Multiple NICs, 10 GbE for production |

#### For HA Clusters

- High-end server hardware with no single point of failure
- Redundant disks (Hardware RAID)
- Redundant power supplies
- UPS systems
- Network bonding for all critical paths

### 3.2 Overcommit Settings

#### CPU Overcommit

KVM/Proxmox allows CPU overcommitment:
- Safe to have total VM cores exceed physical cores
- Host balances QEMU threads across physical cores
- Proxmox prevents single VM from having more vCPUs than physical cores

#### CPU Best Practices

| Guideline | Recommendation |
|-----------|----------------|
| Target system use | Max 80% CPU |
| Per-VM allocation | Minimum needed vCPUs |
| Best performance | Single vCPU per guest |
| Overcommit limit | Monitor actual usage |

#### Memory Overcommit

**General Recommendation**: Avoid memory overcommit in production

Memory issues in KVM/Proxmox:
- OOM (Out of Memory) killer may terminate VMs unexpectedly
- Ballooning may not always work as expected
- Performance degradation can be severe

#### Memory Management Tools

**Kernel Samepage Merging (KSM)**:
- Works with KVM VMs that set MADV_MERGEABLE flag
- Does not work with LXC containers
- Requires CPU cycles for page scanning
- Most effective with multiple identical OS installations

**Memory Ballooning**:
- Guest-aware memory reclamation
- Activates at ≥80% memory usage
- Gradual memory adjustment
- Does not work with PCI/GPU passthrough

### 3.3 HA Configuration

#### Node Requirements

- **Minimum 3 nodes** for reliable quorum
- For 2-node clusters, a QDevice is required (can be Raspberry Pi or NAS)
- All nodes should have identical Proxmox versions

#### Storage Requirements

| Requirement | Notes |
|-------------|-------|
| Shared storage | Required for HA |
| Ceph | Optional, needs 3+ nodes |
| Storage latency | Target <10ms |
| Throughput | Plan for 5+ simultaneous VM moves |

#### Network Configuration

**Corosync Network** (Critical):
- Use redundant connections (active/backup bond)
- Dedicated port recommended (no user/storage traffic)
- Sensitive to latency - prioritize over other traffic

**Recommended NIC Configuration (with Ceph)**:
- 6 NICs (3x 2-port)
- Bond in three groups: Public, Cluster interconnect, Ceph

#### Fencing

- **Required** for Proxmox HA to function
- Each node needs at least one fencing device
- Prevents split-brain scenarios

#### HA Groups and Priorities

- Create groups like "critical-core" for important VMs
- Define failover priorities for resource allocation during multi-host failures
- Use "low-priority" groups for test/dev VMs

### 3.4 Update Best Practices

1. **Rolling upgrades**: One node at a time
2. **Drain node** before upgrading
3. **Verify cluster health** after each node
4. **Maintain change log** with versions and dates
5. **Never update all nodes simultaneously**

---

## 4. Resource Overcommit Best Practices

### 4.1 CPU Overcommit Ratios by Environment

| Environment | Recommended Ratio | Notes |
|-------------|-------------------|-------|
| Production (Mission Critical) | 1:1 | No overcommit for payments, healthcare |
| Production (General) | 2:1 to 4:1 | Monitor CPU ready <5% |
| Staging | 2:1 to 3:1 | Match production behavior |
| Test/QA | 4:1 to 6:1 | Can tolerate some delays |
| Development | 6:1 to 8:1 | Higher flexibility |
| VDI (Desktop) | 8:1 to 10:1 | Light workloads |

### 4.2 When NOT to Overcommit

#### CPU

- Real-time or latency-sensitive applications
- High-frequency trading systems
- Database servers with heavy OLTP workloads
- Scientific computing / HPC
- Applications with sustained high CPU usage

#### Memory

- Production database servers (SQL Server, Oracle, PostgreSQL)
- In-memory databases (Redis, SAP HANA)
- Java applications with large heaps
- Applications sensitive to paging/swapping
- Mission-critical production workloads

### 4.3 Environment-Specific Recommendations

```
┌─────────────────────────────────────────────────────────────────┐
│                    OVERCOMMIT RATIOS BY ENVIRONMENT             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Development    ████████████████████████████████  8:1 CPU       │
│                 ████████████████████████████████  Moderate mem  │
│                                                                 │
│  Test/QA        ████████████████████            4:1 CPU         │
│                 ████████████████                Low mem         │
│                                                                 │
│  Staging        ████████████                    2:1 CPU         │
│                 ████████                        Minimal mem     │
│                                                                 │
│  Production     ████████                        1:1 to 2:1 CPU  │
│                 ████                            No mem          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 4.4 Workload-Specific CPU Ratios

| Workload Type | Safe Ratio | Maximum Ratio |
|---------------|------------|---------------|
| Web servers | 4:1 | 8:1 |
| Application servers | 3:1 | 6:1 |
| Database servers | 1:1 | 2:1 |
| File servers | 4:1 | 8:1 |
| Email servers | 2:1 | 4:1 |
| Domain controllers | 2:1 | 3:1 |
| Low-code platforms | 2:1 | 4:1 |

---

## 5. VM Sizing Templates

### 5.1 Standard VM Templates

#### Small VM (General Purpose)

| Resource | Development | Production |
|----------|-------------|------------|
| vCPU | 1-2 | 2 |
| Memory | 2-4 GB | 4 GB |
| OS Disk | 40 GB | 60 GB |
| Data Disk | As needed | 100 GB |

**Use Cases**: Utility servers, lightweight services, monitoring agents

#### Medium VM (General Purpose)

| Resource | Development | Production |
|----------|-------------|------------|
| vCPU | 2-4 | 4 |
| Memory | 4-8 GB | 8-16 GB |
| OS Disk | 60 GB | 80 GB |
| Data Disk | 100 GB | 250 GB |

**Use Cases**: Web servers, application servers, microservices

#### Large VM (General Purpose)

| Resource | Development | Production |
|----------|-------------|------------|
| vCPU | 4-8 | 8-16 |
| Memory | 16-32 GB | 32-64 GB |
| OS Disk | 80 GB | 100 GB |
| Data Disk | 250 GB | 500+ GB |

**Use Cases**: Large applications, middleware, CI/CD servers

### 5.2 Database Server VM Requirements

#### Memory-to-vCPU Ratios

| Environment Size | Ratio | Example Configuration |
|------------------|-------|----------------------|
| Small | 8:1 | 4 vCPU / 32 GB RAM |
| Medium | 16:1 | 8 vCPU / 128 GB RAM |
| Large | 32:1 | 16 vCPU / 512 GB RAM |
| Data Warehouse | 32:1+ | 32 vCPU / 1 TB+ RAM |

#### SQL Server VM Sizing

| Workload | vCPU | Memory | Storage IOPS |
|----------|------|--------|--------------|
| Small OLTP | 4 | 32 GB | 5,000 |
| Medium OLTP | 8 | 64-128 GB | 10,000 |
| Large OLTP | 16 | 128-256 GB | 20,000+ |
| Data Warehouse | 16-32 | 256 GB - 1 TB | 50,000+ |
| Mission Critical | 32+ | 512 GB - 4 TB | 100,000+ |

#### Database Best Practices

1. **Never overcommit memory** for database VMs
2. **Use memory reservations** equal to allocated memory
3. **Enable vNUMA** for VMs with >8 vCPUs
4. **Separate disks** for OS, data, logs, and tempdb
5. **Use RAID 10** equivalent for database storage

### 5.3 Web Server VM Requirements

#### Compute-Optimized (Higher CPU-to-Memory Ratio)

| Size | vCPU | Memory | Storage |
|------|------|--------|---------|
| Small | 2 | 4 GB | 60 GB |
| Medium | 4 | 8 GB | 100 GB |
| Large | 8 | 16 GB | 150 GB |
| X-Large | 16 | 32 GB | 200 GB |

#### Web Server Best Practices

1. Scale horizontally (more VMs) rather than vertically
2. Use load balancers for high availability
3. Keep stateless where possible
4. Consider containerization for dynamic scaling

### 5.4 Application Server VM Requirements

#### Balanced Configuration (1:4 vCPU to Memory Ratio)

| Size | vCPU | Memory | Storage |
|------|------|--------|---------|
| Small | 2 | 8 GB | 80 GB |
| Medium | 4 | 16 GB | 120 GB |
| Large | 8 | 32 GB | 200 GB |
| X-Large | 16 | 64 GB | 300 GB |

---

## 6. Host Sizing Guidelines

### 6.1 Physical Core to vCPU Ratios

#### Calculation Formula

```
vCPU Capacity = Sockets × Cores per Socket × Threads per Core × Overcommit Ratio
```

#### Example Calculation

For a dual-socket server with 16 cores per socket, 2 threads per core:
- Physical threads: 2 × 16 × 2 = 64 threads
- At 4:1 overcommit: 64 × 4 = 256 vCPUs possible
- **Recommended**: Reserve 10-20% for hypervisor overhead

#### Practical Guidelines

| Server Class | Cores | Conservative VMs | Moderate VMs | Aggressive VMs |
|--------------|-------|------------------|--------------|----------------|
| Entry | 8 | 4-8 | 8-16 | 16-24 |
| Midrange | 16 | 8-16 | 16-32 | 32-48 |
| High-End | 32 | 16-32 | 32-64 | 64-96 |
| Enterprise | 64+ | 32-64 | 64-128 | 128+ |

### 6.2 Memory Planning

#### Host Memory Formula

```
Total Host RAM = Sum of VM Memory + Hypervisor Overhead + HA Reserve
```

#### Memory Allocation Guidelines

| Component | Allocation |
|-----------|------------|
| Hypervisor OS | 4-8 GB |
| Per-VM overhead | 50-100 MB per VM |
| HA Reserve | 20-25% of total |
| VM Memory | Remaining capacity |

#### Example: 512 GB Host

- Hypervisor: 8 GB
- 50 VMs overhead: 5 GB
- HA Reserve (25%): 125 GB
- Available for VMs: ~374 GB

### 6.3 Storage Planning

#### Storage Tiers

| Tier | Technology | Use Case | IOPS/GB |
|------|------------|----------|---------|
| Tier 0 | NVMe SSD | Databases, high-performance | 100+ |
| Tier 1 | SSD | Production workloads | 50-100 |
| Tier 2 | 10K/15K SAS | General production | 10-50 |
| Tier 3 | 7.2K SATA | Archive, backup | 1-10 |

#### Storage Sizing Guidelines

1. **OS Storage**: 60-100 GB per VM
2. **Data Storage**: Based on application requirements
3. **Swap/Page File**: 1-2x RAM for traditional; less for modern systems
4. **Growth Buffer**: Plan for 20-30% annual growth

#### IOPS Planning

| Workload | IOPS per VM | Latency Target |
|----------|-------------|----------------|
| Light (web, file) | 50-200 | <10 ms |
| Medium (app server) | 200-500 | <5 ms |
| Heavy (database) | 1,000-10,000+ | <2 ms |
| Extreme (OLTP) | 10,000-100,000+ | <1 ms |

### 6.4 Network Considerations

#### Minimum NIC Requirements

| Environment | NICs | Purpose |
|-------------|------|---------|
| Small | 2 | Management + VM traffic |
| Medium | 4 | Management, VM, vMotion, Storage |
| Large | 6+ | Dedicated per traffic type |
| Enterprise | 8+ | Full redundancy |

#### Network Bandwidth Planning

| Traffic Type | Minimum | Recommended |
|--------------|---------|-------------|
| Management | 1 GbE | 10 GbE |
| VM Traffic | 1 GbE | 10 GbE |
| vMotion | 1 GbE | 10-25 GbE |
| iSCSI/NFS Storage | 10 GbE | 25 GbE |
| vSAN/HCI | 10 GbE | 25-100 GbE |

#### Network Best Practices

1. **Use NIC teaming** for redundancy
2. **Separate traffic types** on different VLANs
3. **Jumbo frames (9000 MTU)** for storage traffic
4. **Quality of Service (QoS)** for traffic prioritization

### 6.5 VMs Per Host Guidelines

#### Factors Affecting VM Density

1. **VM size** (vCPU, memory requirements)
2. **Workload type** (CPU-bound, memory-bound, I/O-bound)
3. **Overcommit ratios** used
4. **HA requirements** (N+1, N+2 capacity)
5. **Storage IOPS** availability

#### General Guidelines

| Host Configuration | Light VMs | Medium VMs | Heavy VMs |
|-------------------|-----------|------------|-----------|
| 8 cores, 64 GB | 15-25 | 8-12 | 3-5 |
| 16 cores, 128 GB | 30-50 | 15-25 | 6-10 |
| 32 cores, 256 GB | 60-100 | 30-50 | 12-20 |
| 64 cores, 512 GB | 120-200 | 60-100 | 24-40 |

#### Best Practice

- Keep 20% capacity reserved for:
  - ESXi/hypervisor overhead
  - vMotion operations
  - HA failover capacity
  - Performance headroom

---

## 7. Low-Code Platform VM Requirements

### 7.1 OutSystems

#### Architecture Components

| Component | Purpose |
|-----------|---------|
| Platform Server | Hosts applications |
| Deployment Controller | Code compilation (optional dedicated) |
| Database Server | Application data storage |
| LifeTime | Infrastructure management (dedicated) |

#### Sizing Factors

1. **Application logic complexity**
2. **Concurrent users** (requests per second)
3. **Data size growth**
4. **Integration response times**

#### Recommended Configurations

**Development Environment**:
| Component | vCPU | Memory | Storage |
|-----------|------|--------|---------|
| Platform Server | 4 | 8 GB | 100 GB |
| Database | 4 | 16 GB | 200 GB |

**Production Environment (Small)**:
| Component | vCPU | Memory | Storage |
|-----------|------|--------|---------|
| Platform Server (x2) | 4 | 16 GB | 150 GB |
| Database | 8 | 32 GB | 500 GB |
| LifeTime | 4 | 8 GB | 100 GB |

**Production Environment (Large/Farm)**:
| Component | vCPU | Memory | Storage |
|-----------|------|--------|---------|
| Platform Server (x4+) | 8 | 32 GB | 200 GB |
| Deployment Controller | 4 | 16 GB | 150 GB |
| Database (Clustered) | 16+ | 64+ GB | 1 TB+ |
| LifeTime | 4 | 8 GB | 100 GB |

#### Redis Requirements (for in-memory sessions)

For production high-availability:
- 3 dedicated servers
- 3-4 vCPUs each (>2.6 GHz)
- 8 GB RAM per server
- 1 Gbps network
- 10 GB disk for OS/logs

### 7.2 Mendix

#### Deployment Options

1. **Mendix Cloud**: Fully managed (recommended)
2. **Private Cloud (Kubernetes)**: Self-hosted on K8s
3. **On-Premises**: Traditional VM deployment

#### On-Premises Guidelines

**Small Deployment** (15-20 users):
| Component | vCPU | Memory | Storage |
|-----------|------|--------|---------|
| App + Database | 2 | 4 GB | 100 GB |
| Java Heap | - | 256 MB | - |

**Medium Deployment**:
| Component | vCPU | Memory | Storage |
|-----------|------|--------|---------|
| App Server | 4 | 8 GB | 100 GB |
| Database | 4 | 16 GB | 250 GB |

**Large Deployment**:
| Component | vCPU | Memory | Storage |
|-----------|------|--------|---------|
| App Server (x2+) | 8 | 16 GB | 150 GB |
| Database | 8 | 32 GB | 500 GB |

#### Scaling Considerations

- Mendix Runtime is Java-based (pre-allocates memory)
- Memory-based autoscaling not recommended
- Use CPU-based autoscaling (target 80% CPU)
- Each environment is fully isolated (compute, memory, storage)

### 7.3 General Low-Code Platform Guidelines

| Environment | vCPU | Memory | Storage | Notes |
|-------------|------|--------|---------|-------|
| Development | 2-4 | 8-16 GB | 100 GB | Single instance OK |
| Test/QA | 4 | 8-16 GB | 150 GB | Mirror production config |
| Production (Small) | 4-8 | 16-32 GB | 200 GB | Consider HA |
| Production (Large) | 8-16 | 32-64 GB | 500+ GB | Farm architecture |

---

## 8. Sources

### VMware Documentation

- [Performance Best Practices for VMware vSphere 8.0](https://www.vmware.com/docs/vsphere-esxi-vcenter-server-80-performance-best-practices)
- [Performance Best Practices for vSphere 8.0 Update 3](https://www.vmware.com/docs/vsphere-esxi-vcenter-server-80U3-performance-best-practices)
- [ESXi Hardware Requirements](https://techdocs.broadcom.com/us/en/vmware-cis/vsphere/vsphere/8-0/esxi-upgrade-8-0/upgrading-esxi-hosts-upgrade/esxi-requirements-upgrade/esxi-hardware-requirements-upgrade.html)
- [Memory Overcommitment](https://docs.vmware.com/en/VMware-vSphere/7.0/com.vmware.vsphere.resmgmt.doc/GUID-895D25BA-3929-495A-825B-D2A468741682.html)
- [vSphere DRS and High Availability](https://techdocs.broadcom.com/us/en/vmware-tanzu/data-solutions/tanzu-greenplum/7/greenplum-database/vsphere-vsphere-drs-ha-setup.html)

### Microsoft Hyper-V Documentation

- [Hyper-V Memory Performance](https://learn.microsoft.com/en-us/windows-server/administration/performance-tuning/role/hyper-v-server/memory-performance)
- [Hyper-V Processor Performance](https://learn.microsoft.com/en-us/windows-server/administration/performance-tuning/role/hyper-v-server/processor-performance)
- [Hyper-V Maximum Scale Limits](https://learn.microsoft.com/en-us/windows-server/virtualization/hyper-v/plan/plan-hyper-v-scalability-in-windows-server)
- [Failover Clustering Requirements](https://learn.microsoft.com/en-us/windows-server/failover-clustering/clustering-requirements)
- [Network Recommendations for Hyper-V Failover Cluster](https://learn.microsoft.com/en-us/windows-server/virtualization/hyper-v/failover-cluster-network-recommendations)

### KVM/Proxmox Documentation

- [Proxmox High Availability](https://pve.proxmox.com/wiki/High_Availability)
- [Proxmox Dynamic Memory Management](https://pve.proxmox.com/wiki/Dynamic_Memory_Management)
- [Red Hat Overcommitting with KVM](https://docs.redhat.com/en/documentation/red_hat_enterprise_linux/6/html/virtualization_administration_guide/form-virtualization-overcommitting_with_kvm-overcommitting_virtualized_cpus)

### Database Sizing

- [SQL Server on Azure VMs - VM Size Best Practices](https://learn.microsoft.com/en-us/azure/azure-sql/virtual-machines/windows/performance-guidelines-best-practices-vm-size?view=azuresql)
- [Azure VM Sizes Overview](https://learn.microsoft.com/en-us/azure/virtual-machines/sizes/overview)

### Low-Code Platforms

- [OutSystems System Requirements](https://success.outsystems.com/documentation/11/setup_outsystems_infrastructure_and_platform/setting_up_outsystems/outsystems_system_requirements/)
- [Sizing OutSystems Platform](https://success.outsystems.com/Support/Enterprise_Customers/Maintenance_and_Operations/Designing_OutSystems_Infrastructures/02_Sizing_OutSystems_Platform)
- [Mendix System Requirements](https://docs.mendix.com/refguide/system-requirements/)
- [Private Mendix Platform Prerequisites](https://docs.mendix.com/private-mendix-platform/prerequisites/)

### Industry Best Practices

- [CPU Overcommit Best Practices - UMA Technology](https://umatechnology.org/cpu-overcommit-ratio-vmware-best-practices/)
- [Guidelines for Overcommitting VMware Resources - Heroix](https://www.heroix.com/download/Guidelines_for_Overcommitting_VMware_Resources.pdf)
- [Hyper-V Dynamic Memory Best Practices - Nakivo](https://www.nakivo.com/blog/full-overview-hyper-v-dynamic-memory-best-practices/)
- [19 Best Practices for Hyper-V Cluster - Virtualization Dojo](https://virtualizationdojo.com/hyper-v/19-best-practices-hyper-v-cluster/)
- [High-Availability Proxmox Clusters: Do's & Don'ts - Virtualization Howto](https://www.virtualizationhowto.com/2025/07/high-availability-ha-proxmox-clusters-dos-donts/)

---

## Document Information

| Field | Value |
|-------|-------|
| Version | 1.0 |
| Created | January 2026 |
| Last Updated | January 2026 |
| Author | Research compilation from official vendor documentation |

---

## Appendix: Quick Reference Tables

### A. CPU Overcommit Quick Reference

| Scenario | VMware | Hyper-V | KVM/Proxmox |
|----------|--------|---------|-------------|
| Production Critical | 1:1 | 1:1 | 1:1 |
| Production General | 2:1-4:1 | 2:1-3:1 | 2:1-4:1 |
| Test/QA | 4:1-6:1 | 3:1-4:1 | 4:1-6:1 |
| Development | 6:1-8:1 | 4:1-6:1 | 6:1-8:1 |

### B. Memory Overcommit Quick Reference

| Scenario | VMware | Hyper-V | KVM/Proxmox |
|----------|--------|---------|-------------|
| Production | Avoid/Minimal | Use Dynamic Memory | Avoid |
| Test/QA | With monitoring | Dynamic Memory OK | KSM + Ballooning |
| Development | Acceptable | Dynamic Memory OK | Acceptable |

### C. Minimum HA Cluster Nodes

| Platform | Minimum Nodes | Recommended |
|----------|---------------|-------------|
| VMware vSphere | 2 | 3+ |
| Hyper-V | 2 | 3+ |
| Proxmox | 3 (or 2 + QDevice) | 3+ |
