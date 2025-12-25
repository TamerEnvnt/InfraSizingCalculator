# Component Diagrams

## Solution Architecture

```mermaid
graph TB
    subgraph "Presentation Layer"
        UI[Blazor UI<br/>Home.razor]
        API[REST API<br/>Controllers]
    end

    subgraph "Business Logic"
        K8S[K8sSizingService]
        VM[VMSizingService]
        TECH[TechnologyService]
        DIST[DistributionService]
        EXP[ExportService]
        WIZ[WizardStateService]
    end

    subgraph "Data Models"
        INPUT[Input Models<br/>K8sSizingInput<br/>VMSizingInput]
        OUTPUT[Output Models<br/>K8sSizingResult<br/>VMSizingResult]
        CONFIG[Config Models<br/>TechnologyConfig<br/>DistributionConfig]
        ENUMS[Enums<br/>11 types]
    end

    UI --> K8S
    UI --> VM
    UI --> EXP
    UI --> WIZ
    API --> K8S
    API --> VM
    API --> TECH
    API --> DIST

    K8S --> TECH
    K8S --> DIST
    VM --> TECH

    K8S --> INPUT
    VM --> INPUT
    K8S --> OUTPUT
    VM --> OUTPUT
    TECH --> CONFIG
    DIST --> CONFIG
    CONFIG --> ENUMS
```

## Service Dependencies

```mermaid
graph LR
    subgraph "Core Services"
        K8S[K8sSizingService]
        VM[VMSizingService]
        PRICING[PricingService]
        GROWTH[GrowthService]
    end

    subgraph "Supporting Services"
        TECH[TechnologyService]
        DIST[DistributionService]
    end

    subgraph "State Services"
        APP_STATE[AppStateService<br/>Centralized State]
        WIZ[WizardStateService]
    end

    subgraph "Utility Services"
        EXP[ExportService]
    end

    K8S --> TECH
    K8S --> DIST
    VM --> TECH
    PRICING --> K8S
    PRICING --> VM
    GROWTH --> K8S
    GROWTH --> PRICING
    EXP --> K8S
    EXP --> VM
```

## State Management Architecture

```mermaid
graph TB
    subgraph "AppStateService (Single Source of Truth)"
        NAV[Navigation State<br/>ActiveSection, ExpandedCard]
        RESULTS[Results State<br/>K8sResults, VMResults]
        COSTS[Cost State<br/>K8sCostEstimate, VMCostEstimate]
        COMPUTED[Computed Properties<br/>TotalNodes, TotalCPU, TotalRAM]
    end

    subgraph "UI Components"
        SIDEBAR[LeftSidebar]
        MAIN[Home.razor]
        STATS[RightStatsSidebar]
        VIEWS[Result Views]
    end

    NAV --> SIDEBAR
    NAV --> MAIN
    RESULTS --> MAIN
    RESULTS --> VIEWS
    COSTS --> VIEWS
    COMPUTED --> STATS

    SIDEBAR --"NavigateToSection()"--> NAV
    MAIN --"SetK8sResults()"--> RESULTS
    VIEWS --"SetCostEstimate()"--> COSTS
```

### State Management Principles

1. **Single Source of Truth**: All state lives in `AppStateService`
2. **Results Persist**: Navigation does NOT reset results
3. **Explicit Reset**: Only `ResetResults()` clears data
4. **Event-Driven Updates**: Components subscribe to `OnStateChanged`
5. **Computed Properties**: Summary stats derived from current state

## UI Component Hierarchy

```mermaid
graph TB
    APP[App.razor]
    MAIN[MainLayout.razor]

    subgraph "Layout Components"
        HEADER[HeaderBar.razor]
        LEFT[LeftSidebar.razor<br/>Navigation Controller]
        RIGHT[RightStatsSidebar.razor<br/>Summary Stats Only]
    end

    HOME[Home.razor<br/>Main Content]

    subgraph "Wizard Steps"
        S1[Step 1: Platform]
        S2[Step 2: Deployment]
        S3[Step 3: Technology]
        S4A[Step 4a: Distribution<br/>K8s Only]
        S4B[Step 4b: VM Config<br/>VMs Only]
        S5[Step 5: Configuration]
        S6[Step 6: Results]
    end

    subgraph "Configuration Tabs"
        APPS[Applications Tab]
        NODES[Node Specs Tab]
        SETTINGS[Settings Tab]
    end

    subgraph "Results View Components"
        SIZING[SizingResultsView<br/>Expandable Cards]
        COST[CostAnalysisView<br/>Progressive Disclosure]
        GROWTH[GrowthPlanningView<br/>Inline Settings]
    end

    subgraph "Modals"
        INFO[InfoModal]
        PROFILE[ProfilesModal]
        SAVE[SaveProfileModal]
    end

    APP --> MAIN
    MAIN --> HEADER
    MAIN --> LEFT
    MAIN --> HOME
    MAIN --> RIGHT

    HOME --> S1
    S1 --> S2
    S2 --> S3
    S3 --> S4A
    S3 --> S4B
    S4A --> S5
    S4B --> S5
    S5 --> S6

    S5 --> APPS
    S5 --> NODES
    S5 --> SETTINGS

    S6 --> SIZING
    S6 --> COST
    S6 --> GROWTH

    HOME --> INFO
    HOME --> PROFILE
    HOME --> SAVE
```

### Three-Panel Layout

```
┌─────────────────────────────────────────────────────────────────────┐
│ HEADER: Logo | Context Breadcrumb | Theme Toggle              56px │
├────────────┬──────────────────────────────────────┬─────────────────┤
│            │                                      │                 │
│  SIDEBAR   │         MAIN CONTENT                 │  STATS PANEL    │
│  (200px)   │         (flexible)                   │  (260px)        │
│            │                                      │                 │
│ Navigation │  Content based on sidebar            │  Summary Only   │
│ ONLY       │  selection:                          │  (no duplication│
│            │  - Config steps                      │  of main content│
│            │  - SizingResultsView                 │                 │
│            │  - CostAnalysisView                  │  - Total Nodes  │
│            │  - GrowthPlanningView                │  - Total CPU    │
│            │                                      │  - Total RAM    │
│            │  Uses progressive disclosure         │  - Monthly Cost │
│            │  and expand/collapse                 │  - Warnings     │
│            │                                      │                 │
└────────────┴──────────────────────────────────────┴─────────────────┘
```

## Cluster Mode Configuration

```mermaid
graph TB
    subgraph "Cluster Modes"
        MULTI[Multi-Cluster Mode]
        SINGLE[Single Cluster Mode]
    end

    subgraph "Multi-Cluster Config"
        CB_DEV[☐ Dev Cluster]
        CB_TEST[☑ Test Cluster]
        CB_STAGE[☑ Stage Cluster]
        CB_PROD[☑ Prod Cluster]
        CB_DR[☐ DR Cluster]
    end

    subgraph "Single Cluster Config"
        DD[Dropdown: Scope]
        OPT_SHARED[Shared]
        OPT_DEV[Dev]
        OPT_TEST[Test]
        OPT_STAGE[Stage]
        OPT_PROD[Prod]
        OPT_DR[DR]
    end

    subgraph "Node Configuration"
        CP[Control Plane<br/>CPU/RAM/Disk]
        INFRA[Infra Nodes<br/>OpenShift only]
        WORKER[Worker Nodes<br/>CPU/RAM/Disk]
    end

    MULTI --> CB_DEV
    MULTI --> CB_TEST
    MULTI --> CB_STAGE
    MULTI --> CB_PROD
    MULTI --> CB_DR

    CB_TEST --> CP
    CB_TEST --> INFRA
    CB_TEST --> WORKER

    SINGLE --> DD
    DD --> OPT_SHARED
    DD --> OPT_DEV
    DD --> OPT_TEST
    DD --> OPT_STAGE
    DD --> OPT_PROD
    DD --> OPT_DR

    OPT_SHARED --> CP
    OPT_SHARED --> INFRA
    OPT_SHARED --> WORKER
```

## Data Flow

```mermaid
sequenceDiagram
    participant User
    participant UI as Home.razor
    participant Service as K8sSizingService
    participant Tech as TechnologyService
    participant Dist as DistributionService
    participant Export as ExportService

    User->>UI: Select options (wizard)
    UI->>UI: Build K8sSizingInput
    UI->>Service: Calculate(input)
    Service->>Tech: GetConfig(technology)
    Tech-->>Service: TechnologyConfig
    Service->>Dist: GetConfig(distribution)
    Dist-->>Service: DistributionConfig
    Service->>Service: Calculate nodes & resources
    Service-->>UI: K8sSizingResult
    UI->>UI: Display SummaryCards + ResultsTable

    User->>UI: Click Export
    UI->>Export: ExportToExcel(result)
    Export-->>UI: Excel file bytes
    UI->>User: Download file
```

## Enum Relationships

```mermaid
graph TB
    subgraph "Platform Selection"
        PT[PlatformType]
        PT_N[Native]
        PT_L[LowCode]
    end

    subgraph "Technologies"
        T[Technology]
        T_NET[.NET]
        T_JAVA[Java]
        T_NODE[Node.js]
        T_PY[Python]
        T_GO[Go]
        T_MX[Mendix]
        T_OS[OutSystems]
    end

    subgraph "Deployment"
        DM[DeploymentModel]
        DM_K8S[Kubernetes]
        DM_VM[VMs]
    end

    subgraph "K8s Distributions"
        D[Distribution]
        D_OCP[OpenShift]
        D_K8S[Kubernetes]
        D_RAN[Rancher]
        D_CLOUD[EKS/AKS/GKE/OKE]
    end

    subgraph "Environments"
        E[EnvironmentType]
        E_DEV[Dev]
        E_TEST[Test]
        E_STAGE[Stage]
        E_PROD[Prod]
        E_DR[DR]
    end

    PT --> PT_N
    PT --> PT_L
    PT_N --> T_NET
    PT_N --> T_JAVA
    PT_N --> T_NODE
    PT_N --> T_PY
    PT_N --> T_GO
    PT_L --> T_MX
    PT_L --> T_OS

    DM_K8S --> D
    D --> D_OCP
    D --> D_K8S
    D --> D_RAN
    D --> D_CLOUD
```
