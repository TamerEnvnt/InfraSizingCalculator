# Software Requirements Specification (SRS)

## Document Information

| Item | Value |
|------|-------|
| Project | Infrastructure Sizing Calculator |
| Version | 1.0 |
| Status | Draft |

---

## 1. Introduction

### 1.1 Purpose

This Software Requirements Specification (SRS) describes the functional and non-functional requirements for the Infrastructure Sizing Calculator application. It serves as a reference for development, testing, and validation.

### 1.2 Scope

The Infrastructure Sizing Calculator is a web application that calculates infrastructure requirements for:
- Kubernetes cluster deployments (11 distributions)
- Virtual Machine deployments (7 technology platforms)

### 1.3 Definitions

| Term | Definition |
|------|------------|
| Pod | Smallest deployable unit in Kubernetes |
| Node | Physical or virtual machine in a cluster |
| Tier | Application size category (Small/Medium/Large/XLarge) |
| Distribution | Kubernetes platform variant (OpenShift, EKS, etc.) |

---

## 2. System Overview

### 2.1 Architecture

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
│  │  K8sSizingService  │  VMSizingService                │   │
│  │  TechnologyService │  DistributionService            │   │
│  │  ExportService     │  WizardStateService             │   │
│  └──────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────┤
│                        Data Layer                            │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Input Models      │  Output Models                  │   │
│  │  Config Models     │  Enumerations (11 types)        │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Technology Stack

| Layer | Technology |
|-------|------------|
| Framework | .NET 10.0 |
| UI | Blazor Server (InteractiveServer) |
| Styling | Custom CSS with CSS Variables |
| Testing | xUnit, bUnit, Playwright |
| Export | ClosedXML (Excel) |

---

## 3. Functional Requirements

### 3.1 Wizard Navigation (SRS-WIZ)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-WIZ-001 | System shall display a step-by-step wizard interface | Must |
| SRS-WIZ-002 | System shall validate each step before allowing navigation | Must |
| SRS-WIZ-003 | System shall display current step indicator | Must |
| SRS-WIZ-004 | System shall allow backward navigation to previous steps | Should |
| SRS-WIZ-005 | System shall preserve user selections during navigation | Must |

### 3.2 Platform Selection (SRS-PLT)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-PLT-001 | System shall offer Native and Low-Code platform types | Must |
| SRS-PLT-002 | System shall filter technologies based on platform type | Must |
| SRS-PLT-003 | System shall display platform descriptions and icons | Should |

### 3.3 Deployment Model Selection (SRS-DEP)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-DEP-001 | System shall offer Kubernetes and VMs deployment models | Must |
| SRS-DEP-002 | System shall show appropriate next steps based on selection | Must |
| SRS-DEP-003 | System shall exclude OutSystems from Kubernetes options | Must |

### 3.4 Technology Selection (SRS-TEC)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-TEC-001 | System shall display technologies with brand colors | Should |
| SRS-TEC-002 | System shall show technology vendor information | Should |
| SRS-TEC-003 | System shall provide info button with detailed specifications | Should |
| SRS-TEC-004 | System shall support 7 technologies: .NET, Java, Node.js, Python, Go, Mendix, OutSystems | Must |

### 3.5 Distribution Selection - K8s Only (SRS-DIS)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-DIS-001 | System shall display 11 Kubernetes distributions | Must |
| SRS-DIS-002 | System shall categorize distributions by type (Enterprise, Open Source, Managed, Lightweight) | Should |
| SRS-DIS-003 | System shall indicate managed control plane distributions | Must |
| SRS-DIS-004 | System shall indicate which distributions have infra nodes | Must |

### 3.6 Cluster Mode Configuration - K8s Only (SRS-CLU)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-CLU-001 | System shall support Multi-Cluster mode | Must |
| SRS-CLU-002 | System shall support Single Cluster mode | Must |
| SRS-CLU-003 | Multi-Cluster mode shall use checkboxes per environment | Must |
| SRS-CLU-004 | Multi-Cluster mode shall show node config for each checked cluster | Must |
| SRS-CLU-005 | Single Cluster mode shall use dropdown (Shared, Dev, Test, Stage, Prod, DR) | Must |
| SRS-CLU-006 | Single Cluster mode shall show config specific to selected scope | Must |

### 3.7 Environment Configuration (SRS-ENV)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-ENV-001 | System shall support 5 environments: Dev, Test, Stage, Prod, DR | Must |
| SRS-ENV-002 | Production environment shall always be enabled | Must |
| SRS-ENV-003 | Each environment shall have independent app configuration | Must |
| SRS-ENV-004 | Environments shall have configurable headroom percentages | Must |

### 3.8 Application Configuration (SRS-APP)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-APP-001 | System shall accept app counts for 4 tiers: Small, Medium, Large, XLarge | Must |
| SRS-APP-002 | App counts shall be non-negative integers | Must |
| SRS-APP-003 | System shall show tier CPU/RAM specs from technology config | Should |
| SRS-APP-004 | System shall calculate total applications per environment | Must |

### 3.9 K8s Calculation (SRS-K8S)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-K8S-001 | System shall calculate master nodes based on cluster size | Must |
| SRS-K8S-002 | System shall calculate infrastructure nodes (OpenShift only) | Must |
| SRS-K8S-003 | System shall calculate worker nodes based on resource requirements | Must |
| SRS-K8S-004 | System shall apply overcommit ratios to resource calculations | Must |
| SRS-K8S-005 | System shall apply headroom percentage to node counts | Must |
| SRS-K8S-006 | System shall calculate total CPU, RAM, Disk per environment | Must |
| SRS-K8S-007 | System shall aggregate grand totals across all environments | Must |

### 3.10 VM Calculation (SRS-VM)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-VM-001 | System shall calculate VM counts based on server roles | Must |
| SRS-VM-002 | System shall apply HA multipliers to instance counts | Must |
| SRS-VM-003 | System shall calculate load balancer requirements | Must |
| SRS-VM-004 | System shall support technology-specific server roles | Must |
| SRS-VM-005 | System shall enforce MaxInstances constraints (e.g., OS Controller = 1) | Must |
| SRS-VM-006 | System shall apply memory multipliers per role | Should |

### 3.11 Results Display (SRS-RES)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-RES-001 | System shall display summary cards with grand totals | Must |
| SRS-RES-002 | System shall display per-environment breakdown table | Must |
| SRS-RES-003 | System shall show node type breakdown (Masters, Infra, Workers) | Must |
| SRS-RES-004 | System shall show resource totals (CPU, RAM, Disk) | Must |

### 3.12 Export (SRS-EXP)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-EXP-001 | System shall export to CSV format | Must |
| SRS-EXP-002 | System shall export to JSON format | Must |
| SRS-EXP-003 | System shall export to Excel with formatted worksheets | Must |
| SRS-EXP-004 | System shall export to HTML diagram | Should |
| SRS-EXP-005 | Exports shall include calculation timestamp | Should |
| SRS-EXP-006 | Exports shall include input configuration | Should |

### 3.13 REST API (SRS-API)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-API-001 | System shall expose POST /api/k8s/calculate endpoint | Must |
| SRS-API-002 | System shall expose POST /api/vm/calculate endpoint | Must |
| SRS-API-003 | System shall expose GET /api/technologies endpoint | Must |
| SRS-API-004 | System shall expose GET /api/distributions endpoint | Must |
| SRS-API-005 | API shall validate input and return appropriate errors | Must |

---

## 4. Non-Functional Requirements

### 4.1 Performance (SRS-PER)

| ID | Requirement | Target |
|----|-------------|--------|
| SRS-PER-001 | Calculation response time | < 500ms |
| SRS-PER-002 | Initial page load time | < 2s |
| SRS-PER-003 | Excel export generation | < 5s |
| SRS-PER-004 | UI interaction response | < 100ms |

### 4.2 Reliability (SRS-REL)

| ID | Requirement | Target |
|----|-------------|--------|
| SRS-REL-001 | System availability | 99.9% uptime |
| SRS-REL-002 | Error handling | Graceful degradation |
| SRS-REL-003 | Input validation | All inputs validated |

### 4.3 Security (SRS-SEC)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-SEC-001 | No sensitive data storage | Must |
| SRS-SEC-002 | Input sanitization | Must |
| SRS-SEC-003 | HTTPS in production | Must |

### 4.4 Maintainability (SRS-MNT)

| ID | Requirement | Priority |
|----|-------------|----------|
| SRS-MNT-001 | Service interfaces for all business logic | Must |
| SRS-MNT-002 | Separation of concerns (UI/Logic/Data) | Must |
| SRS-MNT-003 | Configuration externalization | Should |
| SRS-MNT-004 | Unit test coverage > 80% | Should |

---

## 5. Data Requirements

### 5.1 Input Models

#### K8sSizingInput
```
├── Distribution: Distribution enum
├── Technology: Technology enum
├── ClusterMode: ClusterMode enum
├── EnabledEnvironments: HashSet<EnvironmentType>
├── EnvironmentApps: Dictionary<EnvironmentType, AppConfig>
├── Replicas: ReplicaSettings
├── Headroom: HeadroomSettings
├── ProdOvercommit: OvercommitSettings
├── NonProdOvercommit: OvercommitSettings
└── CustomNodeSpecs: DistributionConfig (optional)
```

#### VMSizingInput
```
├── Technology: Technology enum
├── EnabledEnvironments: HashSet<EnvironmentType>
├── EnvironmentConfigs: Dictionary<EnvironmentType, VMEnvironmentConfig>
└── SystemOverheadPercent: double
```

### 5.2 Output Models

#### K8sSizingResult
```
├── Environments: List<EnvironmentResult>
├── GrandTotal: GrandTotal
├── Configuration: K8sSizingInput
├── NodeSpecs: DistributionConfig
└── CalculatedAt: DateTime
```

#### VMSizingResult
```
├── Environments: List<VMEnvironmentResult>
├── GrandTotal: VMGrandTotal
├── Configuration: VMSizingInput
└── CalculatedAt: DateTime
```

### 5.3 Enumerations

| Enum | Values |
|------|--------|
| PlatformType | Native, LowCode |
| DeploymentModel | Kubernetes, VMs |
| Technology | DotNet, Java, NodeJs, Python, Go, Mendix, OutSystems |
| Distribution | OpenShift, Kubernetes, Rancher, K3s, MicroK8s, Charmed, Tanzu, EKS, AKS, GKE, OKE |
| EnvironmentType | Dev, Test, Stage, Prod, DR |
| ClusterMode | MultiCluster, SingleCluster |
| AppTier | Small, Medium, Large, XLarge |
| HAPattern | None, ActiveActive, ActivePassive, NPlus1, NPlus2 |
| DRPattern | None, WarmStandby, HotStandby, MultiRegion |
| LoadBalancerOption | None, Single, HAPair, CloudLB |
| ServerRole | WebServer, AppServer, Database, Cache, Controller, LifeTime, FileStorage |

---

## 6. Interface Requirements

### 6.1 User Interface

The UI follows a wizard pattern with the following steps:

1. **Platform Selection** - Native or Low-Code
2. **Deployment Model** - Kubernetes or VMs
3. **Technology** - Platform-specific options
4. **Distribution** (K8s) or **VM Config** (VMs)
5. **Configuration** - Apps, Nodes, Settings
6. **Results** - Summary, Table, Export

### 6.2 API Interface

| Endpoint | Method | Description |
|----------|--------|-------------|
| /api/k8s/calculate | POST | Calculate K8s sizing |
| /api/vm/calculate | POST | Calculate VM sizing |
| /api/technologies | GET | List all technologies |
| /api/technologies/{id} | GET | Get technology details |
| /api/distributions | GET | List all distributions |
| /api/distributions/{id} | GET | Get distribution details |

---

## 7. Constraints

### 7.1 Technical Constraints

- Must run on .NET 10.0 or later
- Must use Blazor Server (not WebAssembly)
- No database required
- No external API dependencies at runtime

### 7.2 Business Constraints

- All specifications must match vendor documentation
- OutSystems limited to VM deployment only
- OpenShift is only distribution with infrastructure nodes

---

## 8. Traceability Matrix

| Requirement | Business Rule | Test Case |
|-------------|---------------|-----------|
| SRS-K8S-001 | BR-M001 to BR-M004 | TC-K8S-MASTER-* |
| SRS-K8S-002 | BR-I001 to BR-I006 | TC-K8S-INFRA-* |
| SRS-K8S-003 | BR-W001 to BR-W006 | TC-K8S-WORKER-* |
| SRS-K8S-005 | BR-H001 to BR-H009 | TC-K8S-HEADROOM-* |
| SRS-ENV-002 | BR-E002 | TC-ENV-PROD-* |
| SRS-APP-002 | BR-V001, BR-V002 | TC-APP-VALID-* |
