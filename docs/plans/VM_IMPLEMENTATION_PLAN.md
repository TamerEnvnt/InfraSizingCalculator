# VM Implementation Plan

## Document Information

| Item | Value |
|------|-------|
| Feature Branch | `feature/vm-implementation` |
| Base Branch | `develop` |
| Status | Planning |
| Created | 2025-12-30 |

---

## 1. Executive Summary

This plan addresses the VM sizing feature gaps identified when comparing K8s (fully implemented) vs VM (partially implemented). The goal is to achieve feature parity between K8s and VM deployment models.

### Current State

| Component | K8s | VM | Gap |
|-----------|-----|-----|-----|
| Apps/Workload Input | K8sAppsConfig | - | **Missing** |
| Settings Configuration | K8sSettingsConfig | - | **Missing** |
| Node/Server Specs | K8sNodeSpecsConfig | VMServerRolesConfig | Partial |
| HA/DR Configuration | K8sHADRPanel | VMHADRConfig | Done |
| Sizing Service | K8sSizingService | VMSizingService | Done |
| Results Display | Full | Partial | **Needs Enhancement** |
| Technology Templates | Generic | - | **Missing** |

---

## 2. Implementation Phases

### Phase 1: Technology-Specific Role Templates

**Priority**: High
**Estimated Effort**: 2-3 days

Create technology-specific server role templates that pre-populate the VM configuration based on selected technology.

#### 2.1.1 New Model: `TechnologyRoleTemplate`

```csharp
// Models/TechnologyRoleTemplate.cs
public class TechnologyRoleTemplate
{
    public Technology Technology { get; set; }
    public string TemplateName { get; set; }
    public List<VMRoleTemplateItem> Roles { get; set; }
}

public class VMRoleTemplateItem
{
    public ServerRole Role { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public string RoleIcon { get; set; }
    public string Description { get; set; }
    public AppTier DefaultSize { get; set; }
    public int DefaultInstances { get; set; }
    public int DefaultDiskGB { get; set; }
    public bool IsRequired { get; set; }
    public bool IsOptional { get; set; }
}
```

#### 2.1.2 Technology Templates to Implement

| Technology | Server Roles |
|------------|-------------|
| **.NET** | Web Server, App Server, Database, Cache (Redis), Message Queue |
| **Java** | Web Server, App Server (Tomcat/JBoss), Database, Cache, Message Queue |
| **Node.js** | Web/API Server, Database (MongoDB/Postgres), Cache (Redis), Queue |
| **Python** | Web Server (Gunicorn/uWSGI), App Server, Database, Celery Workers |
| **Go** | API Server, Database, Cache |
| **Mendix** | Mendix Runtime Server, Database, File Storage, Scheduling Server |
| **OutSystems** | Deployment Controller, Front-End Server, Database, Cache, Scheduler |

#### 2.1.3 New Service: `ITechnologyTemplateService`

```csharp
public interface ITechnologyTemplateService
{
    TechnologyRoleTemplate GetTemplate(Technology technology);
    List<TechnologyRoleTemplate> GetAllTemplates();
    VMEnvironmentConfig ApplyTemplate(Technology technology, EnvironmentType env, bool isProd);
}
```

#### Files to Create
- [ ] `src/InfraSizingCalculator/Models/TechnologyRoleTemplate.cs`
- [ ] `src/InfraSizingCalculator/Services/TechnologyTemplateService.cs`
- [ ] `src/InfraSizingCalculator/Services/Interfaces/ITechnologyTemplateService.cs`
- [ ] `tests/.../TechnologyTemplateServiceTests.cs`

---

### Phase 2: VM Apps Configuration Component

**Priority**: High
**Estimated Effort**: 2-3 days

Create `VMAppsConfig.razor` to allow users to input workload information per environment.

#### 2.2.1 Component Design

Unlike K8s (which uses app tiers for pods), VM sizing uses **server roles with instance counts**. The VMAppsConfig should:

1. Display available role templates for selected technology
2. Allow adding/removing roles per environment
3. Configure instance counts and sizes per role
4. Support multi-environment configuration (like K8sAppsConfig)

#### 2.2.2 Component Structure

```
VMAppsConfig.razor
├── Single-Environment Mode
│   └── Role Cards Grid
│       ├── Role Card (Web Server)
│       ├── Role Card (App Server)
│       └── Role Card (Database)
│
└── Multi-Environment Mode
    └── Environment Accordion
        ├── Dev Panel → Role Configuration
        ├── Test Panel → Role Configuration
        ├── Stage Panel → Role Configuration
        ├── Prod Panel → Role Configuration
        └── DR Panel → Role Configuration
```

#### 2.2.3 Parameters

```csharp
[Parameter] public Technology Technology { get; set; }
[Parameter] public HashSet<EnvironmentType> EnabledEnvironments { get; set; }
[Parameter] public Dictionary<EnvironmentType, VMEnvironmentConfig> EnvironmentConfigs { get; set; }
[Parameter] public EventCallback<Dictionary<EnvironmentType, VMEnvironmentConfig>> EnvironmentConfigsChanged { get; set; }
```

#### Files to Create
- [ ] `src/InfraSizingCalculator/Components/VM/VMAppsConfig.razor`
- [ ] `src/InfraSizingCalculator/Components/VM/VMAppsConfig.razor.css`
- [ ] `tests/.../Components/VM/VMAppsConfigTests.cs`

---

### Phase 3: VM Settings Configuration Component

**Priority**: Medium
**Estimated Effort**: 1-2 days

Create `VMSettingsConfig.razor` for VM-specific configuration settings.

#### 2.3.1 Settings to Include

| Setting | Description | Default |
|---------|-------------|---------|
| System Overhead % | OS, agents, monitoring overhead | 15% |
| Storage Overhead % | Additional storage for logs, temp | 20% |
| Backup Storage Multiplier | Factor for backup storage | 1.5x |
| Network Bandwidth | Estimated per VM | Auto |
| License Type | Windows/Linux | Linux |

#### 2.3.2 Component Structure

```
VMSettingsConfig.razor
├── Resource Settings
│   ├── System Overhead % (slider)
│   ├── Storage Overhead % (slider)
│   └── Backup Multiplier (input)
│
├── OS Settings
│   ├── License Type (toggle: Linux/Windows)
│   └── Include Monitoring Agent (checkbox)
│
└── Environment Overrides (optional)
    └── Per-environment override capability
```

#### Files to Create
- [ ] `src/InfraSizingCalculator/Components/VM/VMSettingsConfig.razor`
- [ ] `src/InfraSizingCalculator/Components/VM/VMSettingsConfig.razor.css`
- [ ] `tests/.../Components/VM/VMSettingsConfigTests.cs`

---

### Phase 4: VM Results Enhancement

**Priority**: Medium
**Estimated Effort**: 1-2 days

Enhance VM results display to match K8s detail level.

#### 2.4.1 Current K8s Results Include
- Per-environment breakdown
- Node counts by type (master/infra/worker)
- Resource totals with visual indicators
- Comparison between environments

#### 2.4.2 VM Results Should Include
- Per-environment breakdown
- Server role breakdown with instance counts
- Resource totals per role type
- HA/DR impact visualization
- Load balancer resources

#### Files to Modify/Create
- [ ] `src/InfraSizingCalculator/Components/Results/VMResultsView.razor` (new)
- [ ] `src/InfraSizingCalculator/Components/Results/VMResultsView.razor.css` (new)
- [ ] Update `SizingResultsView.razor` to use new VM view

---

### Phase 5: Integration with Home.razor

**Priority**: High
**Estimated Effort**: 1 day

Update the main wizard to properly integrate VM components.

#### 2.5.1 Current Wizard Steps (K8s)
1. Platform/Technology Selection
2. Environment Selection
3. Distribution Selection
4. Apps Configuration (K8sAppsConfig)
5. Node Specs (K8sNodeSpecsConfig)
6. HA/DR (K8sHADRPanel)
7. Settings (K8sSettingsConfig)
8. Results

#### 2.5.2 VM Wizard Steps (Target)
1. Platform/Technology Selection
2. Environment Selection
3. **Role Templates** (new - VMAppsConfig)
4. **Server Roles** (VMServerRolesConfig - enhanced)
5. HA/DR (VMHADRConfig)
6. **Settings** (new - VMSettingsConfig)
7. Results (VMResultsView)

#### Files to Modify
- [ ] `src/InfraSizingCalculator/Components/Pages/Home.razor`

---

## 3. Business Rules to Implement

### VM-Specific Rules (BR-VM)

| Rule ID | Description | Implementation |
|---------|-------------|----------------|
| BR-VM001 | Minimum 1 instance per role in Production | VMSizingService |
| BR-VM002 | HA pattern doubles App/Web server instances | VMSizingService |
| BR-VM003 | Database always requires dedicated storage | VMRoleConfig |
| BR-VM004 | Load balancer required for 2+ web servers | Validation |
| BR-VM005 | DR environment mirrors Prod roles | Template logic |
| BR-VM006 | Low-code platforms have fixed role requirements | TechnologyTemplateService |
| BR-VM007 | Windows license adds cost multiplier | CostEstimationService |

---

## 4. Model Updates

### 4.1 VMSizingInput Enhancements

```csharp
public class VMSizingInput
{
    // Existing properties...

    // New properties
    public OSLicenseType LicenseType { get; set; } = OSLicenseType.Linux;
    public double StorageOverheadPercent { get; set; } = 20;
    public double BackupStorageMultiplier { get; set; } = 1.5;
    public bool IncludeMonitoringAgent { get; set; } = true;
}

public enum OSLicenseType
{
    Linux,
    WindowsServer,
    WindowsServerDatacenter
}
```

---

## 5. Testing Requirements

### Unit Tests

| Component | Test Count (Est.) |
|-----------|-------------------|
| TechnologyTemplateService | ~15 tests |
| VMAppsConfig | ~20 tests |
| VMSettingsConfig | ~10 tests |
| VMResultsView | ~15 tests |
| Updated VMSizingService | ~10 tests |
| **Total** | ~70 tests |

### E2E Tests

| Scenario | Description |
|----------|-------------|
| E2E-VM-001 | Complete VM sizing workflow for .NET |
| E2E-VM-002 | OutSystems with HA configuration |
| E2E-VM-003 | Mendix multi-environment sizing |
| E2E-VM-004 | VM scenario save and load |

---

## 6. Documentation Updates

After implementation, update:

- [ ] `docs/technical/services.md` - Add TechnologyTemplateService
- [ ] `docs/technical/ui-components.md` - Add VM components
- [ ] `docs/technical/models.md` - Add new models/enums
- [ ] `docs/business/business-rules.md` - Add BR-VM* rules
- [ ] `docs/architecture/solution-overview.md` - Update counts

---

## 7. Implementation Order

```
Week 1:
├── Day 1-2: Phase 1 - Technology Templates
│   ├── Create models
│   ├── Implement TechnologyTemplateService
│   └── Add unit tests
│
├── Day 3-4: Phase 2 - VMAppsConfig
│   ├── Create component
│   ├── Style matching K8s
│   └── Add bUnit tests
│
└── Day 5: Phase 5 - Integration
    └── Update Home.razor wizard flow

Week 2:
├── Day 1: Phase 3 - VMSettingsConfig
│   └── Component + tests
│
├── Day 2: Phase 4 - VMResultsView
│   └── Enhanced results display
│
├── Day 3: Testing & Polish
│   ├── E2E tests
│   └── Visual QA
│
└── Day 4-5: Documentation Sync
    └── Update all docs per SYNC workflow
```

---

## 8. Acceptance Criteria

### Functional
- [ ] User can select any of 7 technologies for VM deployment
- [ ] Technology selection auto-populates appropriate server roles
- [ ] User can customize roles per environment
- [ ] HA/DR settings correctly multiply instances
- [ ] Results show detailed role breakdown
- [ ] VM sizing can be saved as scenario

### Non-Functional
- [ ] VM workflow matches K8s visual quality
- [ ] Calculations complete < 500ms
- [ ] All new components have > 80% test coverage

---

## 9. Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Technology template complexity | High | Start with .NET and OutSystems, add others incrementally |
| UI inconsistency with K8s | Medium | Use shared components and CSS patterns |
| Test coverage gaps | Medium | Write tests alongside components |

---

## 10. Dependencies

- Feature branch: `feature/ux-wireframes` (UX team may have designs)
- Shared components in `/Components/Shared/`
- Existing VMSizingService (stable)
