# Infrastructure Sizing Calculator

A Blazor Server application for calculating infrastructure sizing requirements for Kubernetes clusters and VM deployments, with integrated cost estimation and growth planning.

---

## Overview

The Infrastructure Sizing Calculator helps organizations estimate infrastructure requirements for deploying applications on:

- **Kubernetes clusters** — 46 distributions supported (cloud, on-premises, edge)
- **Virtual Machines** — Technology-specific server roles with HA/DR patterns

### Key Features

| Feature | Description |
|---------|-------------|
| **Multi-Cluster Mode** | Separate cluster per environment with individual node configurations |
| **Single Cluster Mode** | Shared cluster with namespace isolation |
| **Per-Environment Mode** | Calculate sizing for specific environments |
| **Cost Estimation** | Cloud pricing, on-premises TCO, licensing costs |
| **Growth Planning** | Multi-year projections with app/user growth scenarios |
| **Scenario Comparison** | Save and compare multiple sizing scenarios |
| **Export Options** | Excel, PDF, JSON export capabilities |

---

## Supported Technologies

| Technology | Type | Platform |
|-----------|------|----------|
| .NET | Native | Microsoft |
| Java | Native | Oracle/Eclipse |
| Node.js | Native | OpenJS Foundation |
| Python | Native | Python Software Foundation |
| Go | Native | Google |
| Mendix | Low-Code | Siemens |
| OutSystems | Low-Code | OutSystems |

---

## Supported Kubernetes Distributions (46)

**Cloud Managed (20+):** AWS EKS, Azure AKS, Google GKE, Oracle OKE, IBM IKS, DigitalOcean, Linode, Vultr, and more

**On-Premises (8):** OpenShift, Kubernetes (vanilla), Rancher/RKE2, K3s, MicroK8s, Tanzu, Harvester, Talos

**Edge (10+):** AWS Outposts, Azure Arc, Google Anthos, K3s Edge, and more

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Framework | Blazor Server (.NET 10.0) |
| Database | SQLite with Entity Framework Core |
| Authentication | ASP.NET Core Identity with role-based access |
| Observability | OpenTelemetry, Serilog, Prometheus metrics |
| Export | ClosedXML (Excel), QuestPDF (PDF) |
| Testing | xUnit, bUnit, Playwright, BenchmarkDotNet, Stryker.NET |
| CI/CD | GitHub Actions (CI, Security Scanning, Mutation Testing) |

---

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Git

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/TamerEnvnt/InfraSizingCalculator.git
cd InfraSizingCalculator
```

### 2. Setup Git Hooks (Required for Contributors)

```bash
./scripts/setup-hooks.sh
```

This configures commit message validation and templates.

### 3. Build the Application

```bash
dotnet build
```

### 4. Run the Application

```bash
cd src/InfraSizingCalculator
dotnet run
```

Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`.

### 5. Run Tests

```bash
dotnet test
```

---

## Project Structure

```
InfraSizingCalculator/
├── src/InfraSizingCalculator/     # Main application
│   ├── Components/                # Blazor UI components
│   │   ├── Pages/                # Routable pages (Home, Login, Register, etc.)
│   │   ├── Configuration/        # Config panels
│   │   ├── Results/              # Result views
│   │   ├── Wizard/               # Wizard framework
│   │   └── Shared/               # Reusable components
│   ├── Services/                  # Business logic (20+ services)
│   │   ├── Auth/                 # Authentication services
│   │   ├── Pricing/              # Pricing calculation services
│   │   ├── Telemetry/            # Metrics and observability
│   │   ├── HealthChecks/         # Health check implementations
│   │   └── Validation/           # Input validation services
│   ├── Data/                      # Database contexts
│   │   └── Identity/             # Identity models and context
│   ├── Middleware/                # Security and exception handling
│   ├── Models/                    # Data models and enums
│   └── Controllers/Api/           # REST API endpoints
├── tests/
│   ├── InfraSizingCalculator.UnitTests/    # Unit tests (2700+)
│   ├── InfraSizingCalculator.E2ETests/     # E2E tests (Playwright)
│   └── InfraSizingCalculator.Benchmarks/   # Performance benchmarks
├── .github/workflows/             # CI/CD pipelines
│   ├── ci.yml                    # Main CI pipeline
│   └── scheduled-quality.yml     # Weekly quality checks
└── docs/                          # Documentation
    ├── architecture/              # Solution architecture
    ├── business/                  # Business requirements
    ├── technical/                 # Technical documentation
    ├── testing/                   # Test documentation
    ├── baselines/                 # Quality baselines
    ├── srs/                       # Software Requirements Spec
    └── process/                   # Development workflow
```

---

## Branching Strategy

This project uses **Simplified Git Flow**:

```
main ─────────────────────────────────────────────► (stable/production)
  │                                      ▲
  │                                      │ (release merges)
  ▼                                      │
develop ──┬──────────────────────────────┴───────► (integration)
          │         ▲         ▲         ▲
          ▼         │         │         │
     feature/*   feature/*  bugfix/*  hotfix/*
```

| Branch | Purpose |
|--------|---------|
| `main` | Production-ready, stable code |
| `develop` | Integration and testing |
| `feature/*` | New features |
| `bugfix/*` | Bug fixes |
| `hotfix/*` | Urgent production fixes |

See [CONTRIBUTING.md](./CONTRIBUTING.md) for detailed branching guidelines.

---

## Development Workflow

This project follows a **phase-based workflow** to keep code, documentation, and tests synchronized:

```
SPEC → IMPL → SYNC → TEST
```

| Phase | Purpose | Commit Tag |
|-------|---------|------------|
| **SPEC** | Define requirements | `[SPEC]` |
| **IMPL** | Write implementation code | `[IMPL]` |
| **SYNC** | Update documentation to match code | `[SYNC]` |
| **TEST** | Write and run tests | `[TEST]` |

See [docs/process/TEAM_WORKFLOW.md](./docs/process/TEAM_WORKFLOW.md) for full workflow details.

---

## Contributing

We welcome contributions! Please read [CONTRIBUTING.md](./CONTRIBUTING.md) before submitting pull requests.

### Quick Start for Contributors

1. Fork the repository
2. Create a feature branch from `develop`
3. Follow the phase-based commit format
4. Submit a PR to `develop`

---

## Documentation

| Document | Description |
|----------|-------------|
| [Solution Overview](./docs/architecture/solution-overview.md) | Architecture and design |
| [Business Rules](./docs/business/business-rules.md) | Calculation rules and logic |
| [Services](./docs/technical/services.md) | Service layer documentation |
| [UI Components](./docs/technical/ui-components.md) | Component reference |
| [API Reference](./docs/technical/api-reference.md) | REST API documentation |
| [Observability](./docs/technical/OBSERVABILITY.md) | Logging, metrics, and tracing |
| [Security](./docs/technical/SECURITY.md) | Security headers and hardening |
| [Testing Requirements](./docs/testing/TESTING_REQUIREMENTS.md) | Test coverage and quality |

---

## License

This project is proprietary software. All rights reserved.

---

## Authors

- **Tamer** — Project Lead
- **Claude Opus 4.5** — AI Development Partner

---

## Acknowledgments

- Built with [Blazor Server](https://docs.microsoft.com/aspnet/core/blazor/)
- Export functionality powered by [ClosedXML](https://github.com/ClosedXML/ClosedXML) and [QuestPDF](https://www.questpdf.com/)
- Developed with assistance from [Claude Code](https://claude.ai/claude-code)
