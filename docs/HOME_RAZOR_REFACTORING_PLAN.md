# Home.razor Refactoring Plan

## Current State
- **File Size**: 4,799 lines
- **Coverage**: 0%
- **CRAP Score**: 300,852
- **Impact**: Represents 23% of codebase, blocking 80% coverage target

## Refactoring Strategy

### Phase 1: Extract Self-Contained UI Components (High Priority)

These components have minimal dependencies and can be tested immediately:

| Component | Lines | Priority | Testability |
|-----------|-------|----------|-------------|
| `WizardStepIndicator.razor` | ~50 | P1 | High |
| `PlatformSelector.razor` | ~80 | P1 | High |
| `DeploymentModelSelector.razor` | ~60 | P1 | High |
| `TechnologySelector.razor` | ~120 | P1 | High |
| `DistributionSelector.razor` | ~200 | P1 | High |

### Phase 2: Extract Configuration Components (Medium Priority)

These components manage environment configuration:

| Component | Lines | Priority | Testability |
|-----------|-------|----------|-------------|
| `EnvironmentAppsConfig.razor` | ~150 | P2 | Medium |
| `NodeSpecsPanel.razor` | ~100 | P2 | Medium |
| `ClusterModeSelector.razor` | ~60 | P2 | High |
| `MendixDeploymentSelector.razor` | ~100 | P2 | Medium |

### Phase 3: Extract Modal Components (Medium Priority)

The settings modal is massive and should be split:

| Component | Lines | Priority | Testability |
|-----------|-------|----------|-------------|
| `SettingsModal.razor` (container) | ~100 | P3 | Medium |
| `InfraSettingsTab.razor` | ~80 | P3 | High |
| `K8sSettingsTab.razor` | ~60 | P3 | High |
| `HeadroomSettingsTab.razor` | ~80 | P3 | High |
| `ReplicasSettingsTab.razor` | ~60 | P3 | High |
| `NodeDefaultsTab.razor` | ~100 | P3 | Medium |
| `TierSettingsTab.razor` | ~200 | P3 | Medium |
| `PricingSettingsTab.razor` | ~80 | P3 | High |
| `MendixSettingsTab.razor` | ~150 | P3 | Medium |

### Phase 4: Extract Results Components (Medium Priority)

| Component | Lines | Priority | Testability |
|-----------|-------|----------|-------------|
| `ResultsPanel.razor` (container) | ~80 | P4 | Medium |
| `SizingResultsTab.razor` | ~200 | P4 | Medium |
| `CostResultsTab.razor` | ~150 | P4 | Medium |
| `InsightsResultsTab.razor` | ~100 | P4 | Medium |
| `GrowthPlanningTab.razor` | ~150 | P4 | Medium |

### Phase 5: Extract Service Logic (High Priority for Testability)

Move business logic from @code section to services:

| Service | Methods to Move | Priority |
|---------|-----------------|----------|
| `WizardStateService` | Step management, navigation | P1 |
| `TierConfigurationService` | GetTierCpu/Ram, SetTierCpu/Ram | P1 |
| `EnvironmentConfigService` | Environment enable/disable, app config | P2 |
| `ExportService` | Export functions | P3 |

## Execution Order

1. **Create WizardStateService** - Extract wizard step logic (reduces complexity significantly)
2. **Create TierConfigurationService** - Extract repetitive tier getters/setters
3. **Extract PlatformSelector** - Simple, self-contained
4. **Extract DeploymentModelSelector** - Simple, self-contained
5. **Extract TechnologySelector** - Self-contained with cards
6. **Extract DistributionSelector** - Self-contained with filters
7. **Extract WizardStepIndicator** - UI-only component
8. **Extract EnvironmentAppsConfig** - Configuration panel
9. **Extract SettingsModal** with sub-tabs
10. **Extract ResultsPanel** with sub-tabs

## Expected Coverage Impact

After refactoring:
- Home.razor should reduce to ~500-800 lines (orchestration only)
- Each extracted component: 80-95% coverage target
- Services: 90%+ coverage target
- **Overall project coverage**: Should exceed 80% goal

## Component Interface Patterns

### Event Callbacks Pattern
```csharp
[Parameter] public EventCallback<Technology> OnTechnologySelected { get; set; }
[Parameter] public Technology? SelectedTechnology { get; set; }
```

### Cascading Parameters Pattern
```csharp
[CascadingParameter] public WizardState? WizardState { get; set; }
```

### Service Injection Pattern
```csharp
[Inject] public required ITierConfigurationService TierService { get; set; }
```

## Testing Strategy

Each extracted component should have:
1. **Rendering tests** - Component renders correctly
2. **Parameter binding tests** - Values propagate correctly
3. **Event callback tests** - User actions trigger callbacks
4. **Edge case tests** - Null/empty states handled
