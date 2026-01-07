# Infrastructure Sizing Calculator - Wireframe Redesign Plan v0.4.4

**Created:** January 7, 2026
**Status:** In Progress
**Version:** 0.4.4

---

## Overview

This document defines the complete wireframe redesign for the Infrastructure Sizing Calculator, linking each design decision to vendor research for traceability. The redesign addresses TWO distinct modes (K8s and VM) with completely different configurations.

---

## Research References

All design decisions link to these vendor research documents:

| Reference ID | Document | Path |
|--------------|----------|------|
| **[UX-REF]** | Enterprise Dashboard UX/UI Best Practices | `docs/vendor-specs/ux-ui/enterprise-dashboard-best-practices.md` |
| **[K8S-CLOUD]** | K8s Cloud Provider Sizing | `docs/vendor-specs/k8s/K8S_CLOUD_SIZING_RESEARCH.md` |
| **[K8S-ONPREM]** | K8s On-Premises Distributions | `docs/vendor-specs/k8s/ON_PREMISES_K8S_DISTRIBUTIONS.md` |
| **[VM-REF]** | VM/Hypervisor Best Practices | `docs/vendor-specs/VM_HYPERVISOR_BEST_PRACTICES.md` |
| **[LOWCODE]** | Low-Code Platform Sizing | `docs/vendor-specs/lowcode/LOW_CODE_SIZING_RESEARCH.md` |

---

## Table of Contents

1. [Layout Architecture](#1-layout-architecture)
2. [Page Definitions](#2-page-definitions)
3. [K8s Mode Configuration](#3-k8s-mode-configuration)
4. [VM Mode Configuration](#4-vm-mode-configuration)
5. [Tab Structure](#5-tab-structure)
6. [Panel Definitions](#6-panel-definitions)
7. [Results Display](#7-results-display)
8. [Responsive Design](#8-responsive-design)
9. [Implementation Sequence](#9-implementation-sequence)

---

## 1. Layout Architecture

### 1.1 Grid System
**Reference:** [UX-REF] Section 1.1 - Multi-Column Grid Systems

| Screen Width | Columns | Strategy |
|--------------|---------|----------|
| < 768px | 4 | Single column, stacked |
| 768-1024px | 8 | Two-column hierarchy |
| 1024-1440px | 12 | Full dashboard layout |
| > 1440px | 12 + high-density | Side panels inline |

**Why 12 columns:** Divisible by 1, 2, 3, 4, 6 for maximum flexibility.

### 1.2 Dashboard Zones
**Reference:** [UX-REF] Section 4.5 - PatternFly Dashboard Zones

```
+------------------------------------------------------------------+
|  HEADER: Logo | Scenario Name | User Menu | Settings              |
+------------------------------------------------------------------+
|  ACTION BAR: Platform Toggle (K8s/VM) | Quick Actions | Export    |
+------------------------------------------------------------------+
|                                                                    |
|  +----------------+  +----------------+  +----------------+        |
|  | Summary Card 1 |  | Summary Card 2 |  | Summary Card 3 |       |
|  | (Primary KPI)  |  | (Cost KPI)     |  | (Growth KPI)   |       |
|  +----------------+  +----------------+  +----------------+        |
|                                                                    |
|  +------------------+  +--------------------------------------+    |
|  | CONFIG BAR       |  |                                      |    |
|  | (4-5 items)      |  |    RESULTS AREA                     |    |
|  | - Platform       |  |    (Tabs: Sizing | Cost | Growth)   |    |
|  | - Applications   |  |                                      |    |
|  | - Node Specs     |  |                                      |    |
|  | - Pricing        |  |                                      |    |
|  | - Growth         |  |                                      |    |
|  +------------------+  +--------------------------------------+    |
|                                                                    |
+------------------------------------------------------------------+
|  FOOTER: Last Calculated | Status | Version                       |
+------------------------------------------------------------------+
```

### 1.3 F-Pattern Scanning
**Reference:** [UX-REF] Section 4.1 - F-Pattern/Z-Pattern Rule

- **Top-left:** Most critical KPI (Total Nodes/VMs)
- **Left column:** Config bar for primary inputs
- **Right area:** Results and detailed analysis
- **80% rule:** Critical info in left half and top of page

### 1.4 Above-the-Fold Priorities
**Reference:** [UX-REF] Section 3.1

**MUST be visible:**
- Summary cards (3-5 max)
- Platform toggle (K8s/VM)
- Primary action buttons
- Config bar access

**CAN scroll:**
- Detailed results tables
- Historical charts
- Growth projections
- Export options

---

## 2. Page Definitions

### 2.1 All Pages (Complete List)

| ID | Page | Route | Auth Required | Description |
|----|------|-------|---------------|-------------|
| P00 | Landing (Guest) | `/` | No | Single action card + login prompt |
| P01 | Landing (Auth) | `/` | Yes | Two action cards + recent scenarios |
| P02 | Dashboard | `/dashboard` | No (guest mode) | Main sizing interface |
| P03 | Scenarios | `/scenarios` | Yes | Saved scenarios list |
| P04 | Settings | `/settings` | Yes | User preferences, auth providers |
| P05 | Login | `/login` | No | Login form with social |
| P06 | Register | `/register` | No | Registration with password strength |
| P07 | Export Modal | Modal overlay | No | Export format selection |
| P08 | Platform Panel | Slide panel | No | K8s distribution / Hypervisor selection |
| P09 | Apps Panel | Slide panel | No | Application counts by type |
| P10 | Node Specs Panel | Slide panel | No | Node/VM specifications |
| P11 | Pricing Panel | Slide panel | No | Cost configuration |
| P12 | Growth Panel | Slide panel | No | Growth planning parameters |

### 2.2 Page States

Each page can have multiple states:

| Page | State | Trigger |
|------|-------|---------|
| Landing | Guest | Not authenticated |
| Landing | Auth | Authenticated |
| Dashboard | Empty | No active scenario |
| Dashboard | Active | Has configuration |
| Dashboard | Calculating | After Apply click |

---

## 3. K8s Mode Configuration

**Reference:** [K8S-CLOUD], [K8S-ONPREM]

### 3.1 Platform Selection (K8s)

#### Distribution Categories

| Category | Distributions | Reference |
|----------|---------------|-----------|
| **Cloud Managed** | AWS EKS, Azure AKS, GKE, DigitalOcean DOKS, Linode LKE, Vultr VKE | [K8S-CLOUD] |
| **Enterprise On-Prem** | OpenShift, Tanzu (TKGI/TKG), Mirantis MKE | [K8S-ONPREM] |
| **Lightweight** | K3s, MicroK8s, k0s | [K8S-ONPREM] |
| **Rancher Family** | RKE, RKE2 | [K8S-ONPREM] |
| **Other** | Vanilla K8s, KIND, Minikube | Standard K8s |

#### Distribution-Specific Fields

**AWS EKS:**
- Control plane tier: Standard, tier-xl, tier-2xl, tier-4xl
- Fargate vs EC2 node groups
- Prefix delegation toggle
- Max pods formula: `(ENIs × (IPs per ENI - 1)) + 2`

**Azure AKS:**
- Control plane tier: Free, Standard ($72/mo), Premium
- System vs User node pools
- Memory reservation formula: `MIN((20 MB × Max Pods) + 50 MB, 25% × Total Memory)`

**Google GKE:**
- Mode: Autopilot vs Standard
- Regional vs Zonal cluster

**OpenShift:**
- Control plane sizing by worker count:
  - Up to 25 workers: 4 vCPU, 16 GB
  - 26-100 workers: 8 vCPU, 32 GB
  - 101-250 workers: 16 vCPU, 64 GB

**K3s/MicroK8s:**
- Minimal requirements (512 MB - 2 GB RAM)
- Server sizing by agent count

### 3.2 Node Types (K8s)

| Node Type | Purpose | Typical Count | Reference |
|-----------|---------|---------------|-----------|
| **Control Plane** | API server, etcd, scheduler | 1, 3, 5, or 7 | [K8S-ONPREM] |
| **Infrastructure** | Ingress, monitoring, logging | 2-3 | [K8S-ONPREM] |
| **Worker** | Application workloads | Variable | [K8S-CLOUD] |

**Control Plane Count Rules:**
- 1: Development only (no HA)
- 3: Production standard (tolerates 1 failure)
- 5: Large clusters (tolerates 2 failures)
- 7: Maximum HA (tolerates 3 failures, etcd limit)

### 3.3 K8s Node Specifications

| Field | Description | Reference |
|-------|-------------|-----------|
| Instance Type | Cloud instance type (e.g., m6i.xlarge) | [K8S-CLOUD] |
| vCPU | Virtual CPU cores | [K8S-CLOUD] |
| Memory | RAM in GB | [K8S-CLOUD] |
| Max Pods | Pods per node (default 110) | [K8S-CLOUD] |
| Storage Class | SSD, NVMe, HDD | [K8S-ONPREM] |

### 3.4 etcd Requirements (K8s)
**Reference:** [K8S-ONPREM] Section: etcd Storage Requirements

| Cluster Size | CPU | Memory | Storage | IOPS |
|--------------|-----|--------|---------|------|
| Small (<50 nodes) | 2-4 | 8-16 GB | SSD | 50 |
| Medium (50-250) | 4-8 | 16-32 GB | SSD | 500 |
| Large (250+) | 8-16 | 32-64 GB | NVMe | 500+ |

---

## 4. VM Mode Configuration

**Reference:** [VM-REF]

### 4.1 Platform Selection (VM)

#### Hypervisor Options

| Hypervisor | Type | Reference Section |
|------------|------|-------------------|
| **VMware vSphere 8** | Type 1 | [VM-REF] Section 1 |
| **Microsoft Hyper-V** | Type 1 | [VM-REF] Section 2 |
| **KVM/Proxmox** | Type 1 | [VM-REF] Section 3 |
| **Citrix XenServer** | Type 1 | Standard |
| **Oracle VM** | Type 1 | Standard |

#### Hypervisor-Specific Fields

**VMware vSphere:**
- DRS enabled (Yes/No)
- HA enabled (Yes/No)
- vSAN storage (Yes/No)
- Memory reclamation: TPS, Ballooning, Compression

**Hyper-V:**
- Dynamic Memory (Yes/No)
- Failover Clustering (Yes/No)
- Storage Spaces Direct (Yes/No)

**Proxmox:**
- Ceph storage (Yes/No)
- HA mode (Yes/No)
- KSM enabled (Yes/No)

### 4.2 VM Node Types

| Node Type | Purpose | Reference |
|-----------|---------|-----------|
| **Application Server** | App runtime | [VM-REF] Section 5.4 |
| **Database Server** | Data storage | [VM-REF] Section 5.2 |
| **Web Server** | HTTP serving | [VM-REF] Section 5.3 |
| **Utility Server** | Supporting services | [VM-REF] Section 5.1 |

### 4.3 Overcommit Ratios
**Reference:** [VM-REF] Section 4

| Environment | CPU Ratio | Memory Strategy |
|-------------|-----------|-----------------|
| **Production (Critical)** | 1:1 to 2:1 | No overcommit |
| **Production (General)** | 2:1 to 4:1 | Conservative with monitoring |
| **Staging** | 2:1 to 3:1 | Match production |
| **Test/QA** | 4:1 to 6:1 | Moderate overcommit |
| **Development** | 6:1 to 8:1 | Higher flexibility |
| **VDI** | 8:1 to 10:1 | Light workloads |

### 4.4 VM Sizing Templates
**Reference:** [VM-REF] Section 5

| Template | vCPU | Memory | Storage | Use Case |
|----------|------|--------|---------|----------|
| Small | 2 | 4-8 GB | 60 GB | Utility, lightweight |
| Medium | 4 | 8-16 GB | 120 GB | Web, app servers |
| Large | 8-16 | 32-64 GB | 200 GB | Large apps, CI/CD |
| DB Small | 4 | 32 GB | 200 GB | Small OLTP |
| DB Large | 16 | 128-256 GB | 500+ GB | Large databases |

### 4.5 Host Sizing
**Reference:** [VM-REF] Section 6

**VMs per Host Formula:**
```
VMs = (Host_Memory - Hypervisor_Overhead - HA_Reserve) / Avg_VM_Memory

Where:
- Hypervisor_Overhead: 4-8 GB
- HA_Reserve: 20-25% of total
```

---

## 5. Tab Structure

### 5.1 Results Tabs (Both Modes)

| Tab | Content | Reference |
|-----|---------|-----------|
| **Sizing** | Node/VM counts, specifications | [K8S-CLOUD], [VM-REF] |
| **Cost** | Monthly/yearly cost breakdown | [K8S-CLOUD], [VM-REF] |
| **Growth** | Projection timeline | [LOWCODE] Growth factors |

### 5.2 K8s-Specific Tabs (in Sizing)

| Sub-Tab | Content |
|---------|---------|
| Control Plane | CP nodes, etcd sizing |
| Infrastructure | Infra nodes, services |
| Workers | Worker nodes, app pods |
| Summary | Totals, cluster overview |

### 5.3 VM-Specific Tabs (in Sizing)

| Sub-Tab | Content |
|---------|---------|
| Application VMs | App server VMs |
| Database VMs | DB server VMs |
| Web/Utility VMs | Other VMs |
| Host Capacity | Physical host requirements |

---

## 6. Panel Definitions

### 6.1 Panel Behavior
**Reference:** [UX-REF] Section 4.3 - Slide-Out Panels vs Modals

| Property | Value | Reference |
|----------|-------|-----------|
| Width | 400-500px | [UX-REF] Section 9.2 |
| Animation | Slide from right | [UX-REF] |
| Background | Dim main content | [UX-REF] |
| Close triggers | X button, click outside, Escape | [UX-REF] |
| Persistence | Auto-save on change | [UX-REF] |

### 6.2 Platform Panel

**Header:** "Platform Configuration"

**K8s Mode Fields:**
| Field | Type | Options/Range | Reference |
|-------|------|---------------|-----------|
| Distribution | Select | 46 options | [K8S-CLOUD], [K8S-ONPREM] |
| Control Plane Tier | Select | Varies by distribution | [K8S-CLOUD] |
| Control Plane Nodes | Select | 1, 3, 5, 7 | [K8S-ONPREM] |
| Infrastructure Nodes | Number | 0-10 | [K8S-ONPREM] |
| High Availability | Toggle | Yes/No | [K8S-ONPREM] |

**VM Mode Fields:**
| Field | Type | Options/Range | Reference |
|-------|------|---------------|-----------|
| Hypervisor | Select | 5 options | [VM-REF] |
| HA Enabled | Toggle | Yes/No | [VM-REF] |
| DRS/Load Balancing | Toggle | Yes/No | [VM-REF] |
| Shared Storage | Toggle | Yes/No | [VM-REF] |
| Environment | Select | Production, Staging, Dev, Test | [VM-REF] |

### 6.3 Applications Panel

**Header:** "Application Configuration"

**K8s Mode Fields:**
| Field | Type | Reference |
|-------|------|-----------|
| Mendix Apps | Number (with size selector) | [LOWCODE] |
| OutSystems Apps | Number (with size selector) | [LOWCODE] |
| Native Apps | Number (with resource spec) | Standard |
| Total Pods | Calculated | [K8S-CLOUD] |

**VM Mode Fields:**
| Field | Type | Reference |
|-------|------|-----------|
| Mendix Servers | Number | [LOWCODE] |
| OutSystems Front-Ends | Number | [LOWCODE] |
| Database Servers | Number | [LOWCODE] |
| Web Servers | Number | [VM-REF] |
| App Servers | Number | [VM-REF] |

### 6.4 Node Specs Panel

**Header:** K8s: "Node Specifications" / VM: "VM Specifications"

**K8s Mode Fields:**
| Field | Type | Reference |
|-------|------|-----------|
| Worker Node Size | Preset selector | [K8S-CLOUD] |
| Custom vCPU | Number | [K8S-CLOUD] |
| Custom Memory | Number | [K8S-CLOUD] |
| Max Pods | Number (default 110) | [K8S-CLOUD] |
| Storage Class | Select | [K8S-ONPREM] |

**VM Mode Fields:**
| Field | Type | Reference |
|-------|------|-----------|
| VM Template | Preset selector | [VM-REF] |
| Custom vCPU | Number | [VM-REF] |
| Custom Memory | Number | [VM-REF] |
| CPU Overcommit | Slider/Select | [VM-REF] |
| Memory Overcommit | Slider/Select | [VM-REF] |

### 6.5 Pricing Panel

**Header:** "Pricing Configuration"

**Common Fields:**
| Field | Type | Reference |
|-------|------|-----------|
| Currency | Select | USD, EUR, GBP |
| Billing Period | Select | Monthly, Yearly |
| Support Level | Select | Basic, Standard, Premium |

**K8s Mode Fields:**
| Field | Reference |
|-------|-----------|
| Control Plane Cost | [K8S-CLOUD] |
| Node Hourly Rate | [K8S-CLOUD] |
| Storage Cost per GB | [K8S-CLOUD] |
| Network Egress | [K8S-CLOUD] |

**VM Mode Fields:**
| Field | Reference |
|-------|-----------|
| Hypervisor License | [VM-REF] |
| Host Hardware Cost | [VM-REF] |
| Storage Cost per TB | [VM-REF] |
| Power/Cooling | [VM-REF] |

### 6.6 Growth Panel

**Header:** "Growth Planning"

**Common Fields:**
| Field | Type | Reference |
|-------|------|-----------|
| Projection Period | Select | 1, 2, 3, 5 years | [LOWCODE] |
| User Growth Rate | Percentage | [LOWCODE] |
| App Growth Rate | Percentage | [LOWCODE] |
| Data Growth Rate | Percentage | [LOWCODE] |
| Traffic Growth Rate | Percentage | [LOWCODE] |

**Growth Factors (Default):**
| Type | Annual | 3-Year |
|------|--------|--------|
| Users | 1.2x | 1.7x |
| Apps | 1.3x | 2.2x |
| Data | 1.5x | 3.4x |
| Traffic | 1.3x | 2.2x |

---

## 7. Results Display

### 7.1 Summary Cards
**Reference:** [UX-REF] Section 4.1 - 5-Second Rule

| Card | K8s Content | VM Content | Position |
|------|-------------|------------|----------|
| **Primary** | Total Nodes | Total VMs | Top-left |
| **Cost** | Monthly Cost | Monthly Cost | Top-center |
| **Growth** | Year-End Projection | Year-End Projection | Top-right |

### 7.2 K8s Sizing Results

**Sizing Tab Content:**

```
┌─────────────────────────────────────────────────────────────┐
│ CLUSTER OVERVIEW                                             │
├─────────────────────────────────────────────────────────────┤
│ Distribution: Amazon EKS                                     │
│ Control Plane: Standard tier                                 │
│ Total Nodes: 12                                              │
├─────────────────────────────────────────────────────────────┤
│ NODE BREAKDOWN                                               │
│ ┌─────────────┬───────┬────────┬────────┬──────────┐       │
│ │ Node Type   │ Count │ vCPU   │ Memory │ Storage  │       │
│ ├─────────────┼───────┼────────┼────────┼──────────┤       │
│ │ Control     │ 3     │ 4      │ 16 GB  │ 100 GB   │       │
│ │ Infra       │ 3     │ 4      │ 16 GB  │ 100 GB   │       │
│ │ Worker      │ 6     │ 8      │ 32 GB  │ 200 GB   │       │
│ └─────────────┴───────┴────────┴────────┴──────────┘       │
└─────────────────────────────────────────────────────────────┘
```

### 7.3 VM Sizing Results

**Sizing Tab Content:**

```
┌─────────────────────────────────────────────────────────────┐
│ INFRASTRUCTURE OVERVIEW                                      │
├─────────────────────────────────────────────────────────────┤
│ Hypervisor: VMware vSphere 8                                │
│ Environment: Production                                      │
│ Total VMs: 18                                                │
├─────────────────────────────────────────────────────────────┤
│ VM BREAKDOWN                                                 │
│ ┌─────────────┬───────┬────────┬────────┬──────────┐       │
│ │ VM Type     │ Count │ vCPU   │ Memory │ Storage  │       │
│ ├─────────────┼───────┼────────┼────────┼──────────┤       │
│ │ App Server  │ 6     │ 4      │ 16 GB  │ 120 GB   │       │
│ │ Database    │ 4     │ 8      │ 64 GB  │ 500 GB   │       │
│ │ Web Server  │ 4     │ 2      │ 8 GB   │ 60 GB    │       │
│ │ Utility     │ 4     │ 2      │ 4 GB   │ 60 GB    │       │
│ └─────────────┴───────┴────────┴────────┴──────────┘       │
├─────────────────────────────────────────────────────────────┤
│ HOST REQUIREMENTS                                            │
│ Physical Hosts: 4 (with 25% HA reserve)                     │
│ Host Specs: 32 vCPU, 256 GB RAM each                        │
│ CPU Overcommit: 2:1                                          │
└─────────────────────────────────────────────────────────────┘
```

---

## 8. Responsive Design

**Reference:** [UX-REF] Section 9.3 - Responsive Breakpoints

### 8.1 Breakpoint Definitions

| Breakpoint | Width | Layout Changes |
|------------|-------|----------------|
| **Mobile** | < 768px | Stack all, hamburger nav, full-width panels |
| **Tablet** | 768-1024px | 2-column summary cards, overlay panels |
| **Desktop** | 1024-1440px | Full layout, side panels |
| **Wide** | > 1440px | High-density mode option |

### 8.2 Component Adaptations

| Component | Mobile | Tablet | Desktop |
|-----------|--------|--------|---------|
| Summary Cards | Stacked | 2 + 1 | 3 across |
| Config Bar | Collapsed | Icon-only | Full |
| Results Tabs | Scrollable | Scrollable | Fixed |
| Panels | Full-screen | 80% width | 400-500px |

---

## 9. Implementation Sequence

### Phase 1: Foundation (Week 1-2)
1. Create v0.4.4 HTML wireframes
2. Implement layout grid system
3. Create header/footer components
4. Build summary card component

### Phase 2: Platform Toggle (Week 2-3)
1. Implement K8s/VM mode switch
2. Create platform panel (K8s)
3. Create platform panel (VM)
4. Connect mode to all panels

### Phase 3: Configuration Panels (Week 3-4)
1. Applications panel (K8s + VM variants)
2. Node/VM specs panel (both modes)
3. Pricing panel (both modes)
4. Growth panel (shared)

### Phase 4: Results Display (Week 4-5)
1. Sizing results (K8s + VM tabs)
2. Cost analysis (both modes)
3. Growth projections (shared)
4. Export functionality

### Phase 5: Polish (Week 5-6)
1. Responsive testing
2. Accessibility audit
3. Performance optimization
4. User testing

---

## Appendix A: Complete Field Reference

### K8s Distribution-Specific Fields

| Distribution | Unique Fields |
|--------------|---------------|
| AWS EKS | Control plane tier, Prefix delegation, Fargate toggle |
| Azure AKS | Pricing tier, System pool toggle, VM node pools |
| GKE | Autopilot toggle, Regional toggle |
| OpenShift | ODF storage toggle, Infrastructure nodes |
| RKE2/K3s | Server sizing, Agent scaling |
| MicroK8s | Add-ons selection |

### VM Hypervisor-Specific Fields

| Hypervisor | Unique Fields |
|------------|---------------|
| vSphere | DRS mode, HA admission control, vSAN toggle |
| Hyper-V | Dynamic memory, Failover clustering, S2D |
| Proxmox | Ceph toggle, HA mode, KSM toggle, Fencing |

---

## Appendix B: Validation Rules

### K8s Mode

| Field | Rule | Source |
|-------|------|--------|
| Control Plane Nodes | Must be 1, 3, 5, or 7 | [K8S-ONPREM] etcd quorum |
| Max Pods | Default 110, max 250 | [K8S-CLOUD] K8s limit |
| Infrastructure Nodes | 0-10, recommend 2-3 for HA | [K8S-ONPREM] |

### VM Mode

| Field | Rule | Source |
|-------|------|--------|
| CPU Overcommit | 1:1 to 10:1 | [VM-REF] Section 4 |
| Memory Overcommit | Not recommended for production | [VM-REF] Section 4 |
| Host HA Reserve | 20-25% of capacity | [VM-REF] Section 6 |

---

## Appendix C: Formulas Reference

### K8s Formulas

**AKS Memory Reservation:**
```
Reserved = MIN((20 MB × Max_Pods) + 50 MB, 25% × Total_Memory)
```

**EKS Max Pods:**
```
Max_Pods = (ENIs × (IPs_per_ENI - 1)) + 2
```

**Worker Node Count:**
```
Workers = CEILING(Total_Pods / Pods_per_Node) + Failure_Tolerance
```

### VM Formulas

**VMs per Host:**
```
VMs = (Host_Memory - Hypervisor_Overhead - HA_Reserve) / Avg_VM_Memory
```

**Total vCPU Capacity:**
```
vCPU_Capacity = Sockets × Cores × Threads × Overcommit_Ratio
```

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 0.4.4 | 2026-01-07 | Claude + Tamer | Initial creation |

---

*This document links to vendor research for all design decisions. After context compaction, re-read the referenced documents for detailed specifications.*
