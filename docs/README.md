# Infrastructure Sizing Calculator - Documentation

## Overview

This documentation covers the Infrastructure Sizing Calculator, a tool for estimating infrastructure requirements for Kubernetes clusters and Virtual Machine deployments.

## Documentation Structure

### Architecture
- [Solution Overview](./architecture/solution-overview.md) - High-level architecture
- [Component Diagram](./architecture/component-diagram.md) - Visual diagrams (Mermaid)
- [Data Flow](./architecture/data-flow.md) - Data flow documentation

### Business Requirements
- [BRD](./business/BRD.md) - Business Requirements Document
- [SRS](./business/SRS.md) - Software Requirements Specification
- [Business Rules](./business/business-rules.md) - BR-xxx rules reference

### Vendor Specifications
- [Kubernetes Distributions](./vendor-specs/kubernetes/) - Official specs per distribution
- [Technologies](./vendor-specs/technologies/) - Platform-specific sizing
- [Validation Report](./vendor-specs/validation-report.md) - Gap analysis

### Technical Documentation
- [Data Models](./technical/models.md) - All input/output models and enums
- [Services](./technical/services.md) - Business logic layer documentation
- [API Reference](./technical/api-reference.md) - REST API endpoints
- [UI Components](./technical/ui-components.md) - Blazor component guide

### Setup Guides
- [Development Setup](./setup/development.md) - Local development
- [GitHub Setup](./setup/github-setup.md) - Repository setup (for later)

### Interactive Diagrams
Visual diagrams with vendor-specific icons (open in browser):
- [Solution Architecture](./diagrams/solution-architecture.html) - .NET/Blazor layered architecture
- [Kubernetes Distributions](./diagrams/kubernetes-distributions.html) - 11 K8s distributions with specs
- [Technologies](./diagrams/technologies.html) - 7 technology stacks with tier sizing
- [Data Flow](./diagrams/data-flow.html) - K8s and VM sizing flow
- [VM Sizing](./diagrams/vm-sizing.html) - Server roles and HA patterns

## Solutions

This repository contains two implementations:

| Version | Technology | Location | Purpose |
|---------|------------|----------|---------|
| Portable | HTML/JS | `/portable/` | Standalone, no server required |
| .NET | Blazor Server | `/dotnet/` | Full-featured web application |

## Quick Links

- **Technologies Supported**: .NET, Java, Node.js, Python, Go, Mendix, OutSystems
- **K8s Distributions**: OpenShift, Kubernetes, Rancher, K3s, MicroK8s, Charmed, Tanzu, EKS, AKS, GKE, OKE
- **Deployment Models**: Kubernetes (3 cluster modes) and Virtual Machines
