# Infrastructure Sizing Calculator - Documentation

## Overview

This documentation covers the Infrastructure Sizing Calculator, a tool for estimating infrastructure requirements for Kubernetes clusters and Virtual Machine deployments.

## Documentation Structure

```
docs/
├── README.md                    # This file
├── process/                     # Development process & sync workflow ⭐
│   ├── README.md               # Process overview
│   ├── SYNC_WORKFLOW.md        # Step-by-step sync workflow
│   └── CHANGE_IMPACT_MATRIX.md # What changes require what updates
├── architecture/                # Solution architecture
├── business/                    # Business requirements
├── diagrams/                    # Interactive HTML diagrams
├── guides/                      # How-to guides
├── setup/                       # Development setup
├── srs/                         # Software Requirements Specification
│   ├── index.html              # SRS book table of contents
│   ├── chapters/               # 21 chapter files (55 use cases)
│   └── templates/              # SRS document templates
├── technical/                   # Technical documentation
├── testing/                     # Testing documentation
└── vendor-specs/               # Vendor specifications
```

---

### Development Process (Start Here for Contributors)
- **[Process Overview](./process/README.md)** - How to keep code, docs, and tests in sync
- **[Team Workflow](./process/TEAM_WORKFLOW.md)** - Coordination when multiple people work in parallel
- [Sync Workflow](./process/SYNC_WORKFLOW.md) - Step-by-step individual workflow
- [Change Impact Matrix](./process/CHANGE_IMPACT_MATRIX.md) - What docs to update for each code change

---

### Software Requirements Specification (Book-Style)
The comprehensive SRS is available as a navigable HTML book:
- **[SRS Index](./srs/index.html)** - Main entry point with table of contents
- [Complete SRS (single file)](./srs/SRS_InfraSizingCalculator_Complete.html) - Full document
- 55 complete use cases organized by category
- Full traceability matrix linking use cases to requirements
- Non-functional requirements and verification criteria

**SRS Templates:**
- [SRS Template (HTML)](./srs/templates/SRS_TEMPLATE.html)
- [SRS Template (Markdown)](./srs/templates/SRS_TEMPLATE.md)
- [Use Case Template](./srs/templates/SRS_USE_CASE_TEMPLATE.html)
- [Complete SRS Template](./srs/templates/SRS_COMPLETE_TEMPLATE.html)

---

### Architecture
- [Solution Overview](./architecture/solution-overview.md) - High-level architecture
- [Component Diagram](./architecture/component-diagram.md) - Visual diagrams (Mermaid)
- [Data Flow](./architecture/data-flow.md) - Data flow documentation
- [Wizard Flow](./architecture/WIZARD_FLOW.md) - Application wizard navigation flow

---

### Business Requirements
- [BRD](./business/BRD.md) - Business Requirements Document
- [SRS (legacy)](./business/SRS.md) - Original Software Requirements Specification
- [Business Rules](./business/business-rules.md) - BR-xxx rules reference
- [Business Review Requirements](./business/BUSINESS_REVIEW_REQUIREMENTS.md) - Review checklist

---

### Testing Documentation
- [Testing Requirements](./testing/TESTING_REQUIREMENTS.md) - Test strategy and requirements
- [Coverage Completion Plan](./testing/COVERAGE_COMPLETION_PLAN.md) - Coverage targets
- [DatabasePricingSettingsService Tests](./testing/DatabasePricingSettingsServiceTests.README.md) - 100+ unit tests

---

### Technical Documentation
- [System Inventory](./technical/SYSTEM_INVENTORY.md) - Complete system components
- [Data Models](./technical/models.md) - All input/output models and enums
- [Services](./technical/services.md) - Business logic layer documentation
- [API Reference](./technical/api-reference.md) - REST API endpoints
- [UI Components](./technical/ui-components.md) - Blazor component guide

---

### Vendor Specifications
- [Kubernetes Distributions](./vendor-specs/kubernetes/) - Official specs per distribution
- [Technologies](./vendor-specs/technologies/) - Platform-specific sizing
- [Validation Report](./vendor-specs/validation-report.md) - Gap analysis
- [Mendix Deployment Research](./vendor-specs/mendix-deployment-research.md) - Mendix specs

---

### Setup & Guides
- [Development Setup](./setup/development.md) - Local development
- [GitHub Setup](./setup/github-setup.md) - Repository setup
- [Cloud Pricing API Setup](./guides/cloud-pricing-api-setup.md) - Cloud pricing integration
- [Mendix Pricing Guide](./guides/mendix-pricing.md) - Mendix platform pricing
- [Pricing Validation](./guides/PRICING_VALIDATION.md) - Pricing data validation

---

### Interactive Diagrams
Visual diagrams with vendor-specific icons (open in browser):
- [Solution Architecture](./diagrams/solution-architecture.html) - .NET/Blazor layered architecture
- [Kubernetes Distributions](./diagrams/kubernetes-distributions.html) - K8s distributions with specs
- [Technologies](./diagrams/technologies.html) - 7 technology stacks with tier sizing
- [Data Flow](./diagrams/data-flow.html) - K8s and VM sizing flow
- [VM Sizing](./diagrams/vm-sizing.html) - Server roles and HA patterns
- [Landscape](./diagrams/landscape.html) - Technology landscape

---

## Project Structure

| Folder | Description |
|--------|-------------|
| `/src/InfraSizingCalculator/` | Blazor Server application |
| `/tests/` | Unit tests and E2E tests |
| `/docs/` | Documentation (this folder) |
| `/coverage/` | Test coverage reports |

## Quick Links

- **Technologies Supported**: .NET, Java, Node.js, Python, Go, Mendix, OutSystems
- **K8s Distributions**: 38+ including OpenShift, EKS, AKS, GKE, Rancher, K3s, Tanzu
- **Deployment Models**: Kubernetes (3 cluster modes) and Virtual Machines
- **Cloud Pricing**: AWS, Azure, GCP, Oracle Cloud, IBM Cloud
