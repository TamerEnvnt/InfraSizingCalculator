# Solution Overview

## Purpose

The Infrastructure Sizing Calculator helps organizations estimate infrastructure requirements for deploying applications on:
- **Kubernetes clusters** (46 distributions supported including cloud variants)
- **Virtual Machines** (technology-specific server roles)

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                      │
│  ┌─────────────────┐  ┌─────────────────────────────────┐   │
│  │   Blazor UI     │  │         REST API                │   │
│  │   (Home.razor)  │  │   (Controllers/Api/)            │   │
│  └─────────────────┘  └─────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────┤
│                      Business Logic Layer                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                     Services (23 total)               │   │
│  │  K8sSizingService    │  VMSizingService              │   │
│  │  TechnologyService   │  DistributionService          │   │
│  │  ExportService       │  WizardStateService           │   │
│  │  CostEstimationService│ GrowthPlanningService        │   │
│  │  ScenarioService     │  PricingService               │   │
│  │  AuthService         │  InputValidationService       │   │
│  │  CalculatorMetrics   │  Health Checks (3)            │   │
│  └──────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────┤
│                        Data Layer                            │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                      Models                           │   │
│  │  K8sSizingInput/Result  │  VMSizingInput/Result      │   │
│  │  TechnologyConfig       │  DistributionConfig        │   │
│  │  Enums (15 types)       │  Supporting models         │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
InfraSizingCalculator/
├── src/InfraSizingCalculator/          # Main application
│   ├── Program.cs                      # Entry point, DI setup
│   ├── Components/                     # Blazor UI
│   │   ├── Pages/Home.razor           # Main wizard (7500+ lines)
│   │   ├── Layout/                    # MainLayout, NavMenu
│   │   ├── Shared/                    # Reusable components
│   │   ├── Wizard/                    # Wizard framework
│   │   ├── Modals/                    # Modal dialogs
│   │   ├── Results/                   # Results display
│   │   └── Configuration/             # Config panels
│   ├── Controllers/Api/               # REST endpoints
│   ├── Services/                      # Business logic
│   ├── Models/                        # Data models
│   │   └── Enums/                     # 15 enumeration types
│   └── wwwroot/                       # Static assets
│       ├── app.css                    # Main stylesheet
│       └── js/site.js                 # JavaScript interop
└── tests/
    ├── InfraSizingCalculator.UnitTests/     # xUnit + bUnit
    └── InfraSizingCalculator.E2ETests/      # Playwright
```

## Key Features

### Kubernetes Sizing
- **Multi-Cluster Mode**: Separate cluster per environment with individual node configs
- **Single Cluster Mode**: Shared cluster with namespace isolation
- **Per-Environment Mode**: Calculate for specific environment

### VM Sizing
- Technology-specific server roles (Controller, Frontend, DB, etc.)
- HA patterns (Active-Active, Active-Passive, N+1, N+2)
- DR patterns (Warm Standby, Hot Standby, Multi-Region)
- Load balancer configurations

### Supported Technologies (7)

| Technology | Type | Vendor |
|-----------|------|--------|
| .NET | Native | Microsoft |
| Java | Native | Oracle/Eclipse |
| Node.js | Native | OpenJS Foundation |
| Python | Native | Python Software Foundation |
| Go | Native | Google |
| Mendix | Low-Code | Siemens |
| OutSystems | Low-Code | OutSystems |

### Supported K8s Distributions (46)

**On-Premises (8):**

| Distribution | Type | Vendor |
|-------------|------|--------|
| OpenShift | Enterprise | Red Hat |
| Kubernetes | Open Source | CNCF |
| Rancher/RKE2 | Enterprise | SUSE |
| K3s | Lightweight | SUSE |
| MicroK8s | Lightweight | Canonical |
| Charmed | Enterprise | Canonical |
| Tanzu | Enterprise | VMware/Broadcom |

**Cloud Managed (8 major + variants):**

| Distribution | Provider |
|-------------|----------|
| EKS | AWS |
| AKS | Microsoft Azure |
| GKE | Google Cloud |
| OKE | Oracle Cloud |
| IKS | IBM Cloud |
| ACK | Alibaba Cloud |
| TKE | Tencent Cloud |
| CCE | Huawei Cloud |

**Plus 30+ cloud variants** including ROSA, ARO, Rancher on EKS/AKS/GKE, Tanzu on AWS/Azure/GCP, and developer-focused options (DOKS, LKE, VKE, Hetzner, OVH, Scaleway).

See [System Inventory](../technical/SYSTEM_INVENTORY.md) for complete distribution list.

## Technology Stack

- **Framework**: .NET 10.0
- **UI**: Blazor Server with InteractiveServer render mode
- **Styling**: Custom CSS with CSS variables (dark theme)
- **Testing**: xUnit, bUnit, Playwright
- **Export**: ClosedXML for Excel export
