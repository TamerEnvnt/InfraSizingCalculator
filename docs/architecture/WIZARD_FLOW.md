# Infrastructure Sizing Calculator - Wizard Flow Documentation

## Overview

This document maps all possible paths through the sizing calculator wizard, documenting the business logic flow and identifying UI components used at each step.

---

## Wizard Flow Tree

```
START
│
├── Step 1: Platform Type
│   ├── [Native] ──────────────────────────────────────────┐
│   │   Technologies: .NET, Java, Node.js, Python, Go      │
│   │                                                       │
│   └── [LowCode] ─────────────────────────────────────────┐
│       Technologies: Mendix, OutSystems                    │
│                                                           │
│                                                           ▼
├── Step 2: Deployment Model ◄──────────────────────────────┘
│   ├── [Kubernetes] ──────────────────────────────────────┐
│   │                                                       │
│   └── [VMs] ─────────────────────────────────────────────┐
│                                                           │
│                                                           ▼
├── Step 3: Technology Selection ◄──────────────────────────┘
│   │
│   │  Native Platform:          LowCode Platform:
│   │  ├── .NET (Microsoft)      ├── Mendix (Siemens)
│   │  ├── Java (Oracle/OpenJDK) └── OutSystems
│   │  ├── Node.js (OpenJS)
│   │  ├── Python (PSF)
│   │  └── Go (Google)
│   │
│   ▼
├── Step 4: Distribution/Deployment Selection
│   │
│   ├── PATH A: K8s + Native Tech ─────────────────────────┐
│   │   K8s Distribution Selection                         │
│   │   ├── Self-Managed (On-Prem):                        │
│   │   │   ├── OpenShift (On-Prem)                        │
│   │   │   ├── Kubernetes (CNCF)                          │
│   │   │   ├── Rancher (On-Prem)                          │
│   │   │   ├── RKE2 (SUSE)                                │
│   │   │   ├── K3s (SUSE)                                 │
│   │   │   ├── MicroK8s (Canonical)                       │
│   │   │   ├── Charmed Kubernetes (Canonical)             │
│   │   │   └── VMware Tanzu (On-Prem)                     │
│   │   │                                                   │
│   │   └── Cloud Managed:                                  │
│   │       ├── Major (AKS, EKS, GKE)                      │
│   │       ├── OpenShift (ROSA, ARO, etc.)                │
│   │       ├── SUSE (Rancher variants)                    │
│   │       ├── Tanzu (TKG variants)                       │
│   │       ├── Canonical (MicroK8s Cloud)                 │
│   │       └── Developer (Kind, Minikube, etc.)           │
│   │                                                       │
│   ├── PATH B: K8s + Mendix ──────────────────────────────┤
│   │   Mendix K8s Deployment Selection                    │
│   │   ├── [Cloud] Mendix Cloud                           │
│   │   │   ├── SaaS (Multi-tenant)                        │
│   │   │   └── Dedicated (Single-tenant)                  │
│   │   │                                                   │
│   │   ├── [PrivateCloud] Officially Supported            │
│   │   │   ├── Mendix on Azure                            │
│   │   │   ├── Amazon EKS                                 │
│   │   │   ├── Azure AKS                                  │
│   │   │   ├── Google GKE                                 │
│   │   │   └── Red Hat OpenShift                          │
│   │   │                                                   │
│   │   └── [Other] Manual Setup (Unsupported)             │
│   │       ├── Rancher                                    │
│   │       ├── K3s                                        │
│   │       └── Generic Kubernetes                         │
│   │                                                       │
│   ├── PATH C: VMs + Mendix ──────────────────────────────┤
│   │   Mendix VM Deployment Selection                     │
│   │   ├── Server (VMs/Docker)                            │
│   │   ├── StackIT (Schwarz IT)                           │
│   │   └── SAP BTP                                        │
│   │                                                       │
│   └── PATH D: VMs + Native Tech ─────────────────────────┤
│       → Skip to Step 5 (no distribution selection)       │
│                                                           │
│                                                           ▼
├── Step 5: Configuration ◄─────────────────────────────────┘
│   │
│   ├── K8s Configuration ─────────────────────────────────┐
│   │   │                                                   │
│   │   │  SIDEBAR: Cluster Mode Selection                 │
│   │   │  ├── Multi-Cluster (one cluster per env)         │
│   │   │  └── Single Cluster (shared/dedicated)           │
│   │   │      ├── Shared (all environments)               │
│   │   │      └── Per-Environment scope                   │
│   │   │                                                   │
│   │   │  TABS:                                           │
│   │   │  ├── [Applications] ← K8sAppsConfig              │
│   │   │  │   ├── Mode Banner (Multi/Single)              │
│   │   │  │   ├── Workload Header + Stats      ← DUPE?    │
│   │   │  │   ├── Environment Checkboxes       ← DUPE?    │
│   │   │  │   └── <K8sAppsConfig>                         │
│   │   │  │       ├── Multi-Cluster Header     ← DUPE?    │
│   │   │  │       └── HorizontalAccordion                 │
│   │   │  │           └── Env panels with tier inputs     │
│   │   │  │                                               │
│   │   │  ├── [Node Specs] ← K8sNodeSpecsConfig           │
│   │   │  │   └── <K8sNodeSpecsConfig>                    │
│   │   │  │       └── HorizontalAccordion                 │
│   │   │  │           └── Env panels with node specs      │
│   │   │  │                                               │
│   │   │  ├── [Settings]                                  │
│   │   │  │   ├── Pod Overhead %                          │
│   │   │  │   ├── Storage per Environment                 │
│   │   │  │   └── Growth Projections                      │
│   │   │  │                                               │
│   │   │  └── [Mendix] (if Mendix selected)               │
│   │   │      └── GenAI, Resource Packs config            │
│   │   │                                                   │
│   └── VM Configuration ──────────────────────────────────┘
│       │
│       │  SIDEBAR: Environment Selection                  │
│       │  ├── Dev                                         │
│       │  ├── Test                                        │
│       │  ├── Stage                                       │
│       │  ├── Prod                                        │
│       │  └── DR                                          │
│       │                                                   │
│       │  TABS:                                           │
│       │  ├── [Server Roles] ← VMServerRolesConfig        │
│       │  │   └── <VMServerRolesConfig>                   │
│       │  │       └── HorizontalAccordion                 │
│       │  │           └── Env panels with role config     │
│       │  │                                               │
│       │  ├── [HA & DR] ← VMHADRConfig                    │
│       │  │   └── <VMHADRConfig>                          │
│       │  │       └── HorizontalAccordion                 │
│       │  │           └── Env panels with HA/DR settings  │
│       │  │                                               │
│       │  └── [Settings]                                  │
│       │      ├── System Overhead %                       │
│       │      └── Storage per Environment                 │
│       │
│       ▼
├── Step 6: Pricing
│   ├── K8s: Distribution licensing, infrastructure costs
│   └── VMs: Hardware, licensing, operational costs
│
│       ▼
└── Step 7: Results
    ├── K8s: Node counts, resource allocation, cost summary
    └── VMs: Server specs, role allocation, cost summary
```

---

## Identified Issues

### 1. Duplicate UI Elements in K8s Configuration (Step 5)

**Location:** Home.razor lines 446-517 + K8sAppsConfig.razor

| Element | Home.razor | K8sAppsConfig | Issue |
|---------|------------|---------------|-------|
| "Multi-Cluster Mode" header | Lines 448-465 | Lines 61-83 | **DUPLICATE** |
| Resource Stats (Apps/CPU/RAM) | Lines 475-488 | Lines 69-82 | **DUPLICATE with different values** |
| Environment Selection | Lines 494-508 (checkboxes) | Accordion panels | **REDUNDANT** |

**Current Stats Conflict:**
- Home.razor shows: Single environment stats (e.g., 70 Apps)
- K8sAppsConfig shows: Total across all envs (e.g., 350 Apps)

### 2. VM Configuration Has Similar Pattern

**Location:** Home.razor lines 710-812

| Element | Home.razor Sidebar | Component |
|---------|-------------------|-----------|
| Environment List | Lines 719-731 | VMServerRolesConfig has accordion panels |

The sidebar shows environments, AND the components use HorizontalAccordion with environment panels - potential redundancy.

---

## Component Inventory

### Horizontal Accordion Components

| Component | File | Used In |
|-----------|------|---------|
| K8sAppsConfig | Components/K8s/K8sAppsConfig.razor | K8s Step 5 - Applications tab |
| K8sNodeSpecsConfig | Components/K8s/K8sNodeSpecsConfig.razor | K8s Step 5 - Node Specs tab |
| VMServerRolesConfig | Components/VM/VMServerRolesConfig.razor | VM Step 5 - Server Roles tab |
| VMHADRConfig | Components/VM/VMHADRConfig.razor | VM Step 5 - HA & DR tab |

### Shared Components

| Component | File | Purpose |
|-----------|------|---------|
| HorizontalAccordion | Components/Shared/HorizontalAccordion.razor | Parent accordion container |
| HorizontalAccordionPanel | Components/Shared/HorizontalAccordionPanel.razor | Individual collapsible panel |
| HorizontalSlider | Components/Shared/HorizontalSlider.razor | **LEGACY** - being replaced |

---

## Environment Types

```
EnvironmentType Enum:
├── Dev      (Development)      - Non-Production
├── Test     (Testing)          - Non-Production
├── Stage    (Staging)          - Non-Production
├── Prod     (Production)       - Production ★ Required
└── DR       (Disaster Recovery)- Production
```

---

## Cluster Modes (K8s Only)

```
ClusterMode Enum:
├── MultiCluster    - Separate cluster per environment
├── SharedCluster   - Single cluster for all environments
└── PerEnvironment  - Dedicated cluster per environment
```

---

## Recommended Fixes

### Priority 1: Remove Duplicates in K8s Configuration

**Option A: Let K8sAppsConfig be self-contained**
- Remove "Multi-Cluster Mode" banner from Home.razor (lines 448-465)
- Remove "Workload Configuration" section from Home.razor (lines 468-489)
- Remove environment checkboxes from Home.razor (lines 494-508)
- Keep only `<K8sAppsConfig>` component which handles everything

**Option B: Strip K8sAppsConfig, move logic to Home.razor**
- Remove header/stats from K8sAppsConfig
- Keep only the HorizontalAccordion portion
- Home.razor controls all display elements

### Priority 2: Fix Accordion Height

- Increase `min-height` from 280px to accommodate content
- Ensure tier cards and inputs are fully visible
- Test with different viewport sizes

### Priority 3: Review VM Configuration for Similar Issues

- Check if sidebar environment list + accordion panels create redundancy
- Consider unifying the selection mechanism

---

## State Variables (Key)

```csharp
// Step tracking
private int currentStep = 1;

// Selections
private PlatformType? selectedPlatform;
private DeploymentModel? selectedDeployment;
private Technology? selectedTechnology;
private K8sDistribution? selectedDistribution;

// K8s specific
private ClusterMode selectedClusterMode = ClusterMode.MultiCluster;
private HashSet<EnvironmentType> enabledEnvironments;
private Dictionary<EnvironmentType, AppConfig> envApps;
private Dictionary<EnvironmentType, NodeSpecConfig> nodeSpecs;

// VM specific
private Dictionary<EnvironmentType, VMEnvironmentConfig> vmEnvironmentConfigs;

// Tab navigation
private string configTab = "apps"; // apps, nodes, settings, mendix, roles, ha
```

---

## Next Steps

1. [ ] Review and approve this flow documentation
2. [ ] Decide on fix approach (Option A or B)
3. [ ] Implement changes
4. [ ] Test all paths
5. [ ] Update documentation if flow changes
