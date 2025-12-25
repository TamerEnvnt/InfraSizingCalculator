# Solution Overview

## Purpose

The Infrastructure Sizing Calculator helps organizations estimate infrastructure requirements for deploying applications on:
- **Kubernetes clusters** (11 distributions supported)
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
│  │                     Services                          │   │
│  │  K8sSizingService  │  VMSizingService                │   │
│  │  TechnologyService │  DistributionService            │   │
│  │  ExportService     │  WizardStateService             │   │
│  └──────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────┤
│                        Data Layer                            │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                      Models                           │   │
│  │  K8sSizingInput/Result  │  VMSizingInput/Result      │   │
│  │  TechnologyConfig       │  DistributionConfig        │   │
│  │  Enums (11 types)       │  Supporting models         │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
InfraSizingCalculator/
├── src/InfraSizingCalculator/          # Main application
│   ├── Program.cs                      # Entry point, DI setup
│   ├── Components/                     # Blazor UI
│   │   ├── Pages/Home.razor           # Main wizard (3700+ lines)
│   │   ├── Layout/                    # MainLayout, NavMenu
│   │   ├── Shared/                    # Reusable components
│   │   ├── Wizard/                    # Wizard framework
│   │   ├── Modals/                    # Modal dialogs
│   │   ├── Results/                   # Results display
│   │   └── Configuration/             # Config panels
│   ├── Controllers/Api/               # REST endpoints
│   ├── Services/                      # Business logic
│   ├── Models/                        # Data models
│   │   └── Enums/                     # 11 enumeration types
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

### Supported K8s Distributions (11)

| Distribution | Type | Vendor |
|-------------|------|--------|
| OpenShift | Enterprise | Red Hat |
| Kubernetes | Open Source | CNCF |
| Rancher | Open Source | SUSE |
| K3s | Lightweight | SUSE |
| MicroK8s | Lightweight | Canonical |
| Charmed | Enterprise | Canonical |
| Tanzu | Enterprise | VMware/Broadcom |
| EKS | Managed | AWS |
| AKS | Managed | Microsoft Azure |
| GKE | Managed | Google Cloud |
| OKE | Managed | Oracle Cloud |

## Technology Stack

- **Framework**: .NET 10.0
- **UI**: Blazor Server with InteractiveServer render mode
- **Styling**: Custom CSS with CSS variables (dark theme)
- **Testing**: xUnit, bUnit, Playwright
- **Export**: ClosedXML for Excel export
