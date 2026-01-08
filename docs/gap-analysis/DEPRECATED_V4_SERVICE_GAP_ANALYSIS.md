# V4 Dashboard Service Gap Analysis

**Date**: 2026-01-07
**Analysis**: Comparison of services used by Legacy Home.razor vs V4 Dashboard

## Executive Summary

The V4 Dashboard is a **UI shell with no business logic connected**. All core functionality (sizing calculations, cost estimation, export, growth planning) is unreachable from the new design.

| Metric | Legacy Home | V4 Dashboard |
|--------|-------------|--------------|
| Services Injected | 23 | 3 |
| Core Calculation Services | 7 | 0 |
| Export/Sharing Services | 2 | 0 |
| Scenario Management | 1 | 0 |
| Uses Actual Data | Yes | Demo/Hardcoded |

## Critical: Services NOT Injected in V4

### Core Calculation Services (MISSING)

| Service | Purpose | Impact |
|---------|---------|--------|
| `IK8sSizingService` | K8s cluster sizing calculations | **No sizing works** |
| `IVMSizingService` | VM deployment sizing | **No VM sizing** |
| `IDistributionService` | 46 K8s distributions data | **No distribution selection** |
| `ITechnologyService` | 7 low-code platform data | **No platform selection** |

### Cost & Pricing Services (MISSING)

| Service | Purpose | Impact |
|---------|---------|--------|
| `ICostEstimationService` | Cost calculations | **No cost estimates** |
| `IPricingService` | Cloud/on-prem pricing data | **No pricing data** |
| `IPricingSettingsService` | User pricing preferences | **No pricing config** |

### Export & Sharing Services (MISSING)

| Service | Purpose | Impact |
|---------|---------|--------|
| `IExportService` | PDF, Excel, CSV, JSON export | **No export works** |
| `ConfigurationSharingService` | Share configs via link/QR | **No sharing works** |

### Scenario Management (MISSING from Dashboard)

| Service | Purpose | Impact |
|---------|---------|--------|
| `IScenarioService` | Save/load/compare scenarios | **Dashboard can't save** |

### Growth & Validation Services (MISSING)

| Service | Purpose | Impact |
|---------|---------|--------|
| `IGrowthPlanningService` | Capacity projections | **No growth planning** |
| `ValidationRecommendationService` | Input validation | **No validation** |
| `ITierConfigurationService` | App tier configs | **No tier settings** |

### State Management Services (MISSING)

| Service | Purpose | Impact |
|---------|---------|--------|
| `IHomePageStateService` | Page state management | **No state persistence** |
| `IHomePageCalculationService` | Calculation orchestration | **No calculations** |
| `IHomePageCostService` | Cost display logic | **No cost display** |
| `IHomePageDistributionService` | Distribution UI logic | **No distribution UI** |
| `IHomePageUIHelperService` | UI helper functions | **No UI helpers** |
| `IHomePageCloudAlternativeService` | Cloud comparison logic | **No cloud comparison** |
| `IHomePageVMService` | VM-specific logic | **No VM support** |
| `IInfoContentService` | Help/info content | **No help content** |
| `IAppStateService` | App-wide state | **No state sync** |

## Services Currently Injected in V4

### Dashboard.razor
```csharp
@inject NavigationManager NavigationManager
@inject IAuthService AuthService
@inject AuthenticationStateProvider AuthStateProvider
```

### V4 Config Panels (3 panels)
```csharp
@inject IWizardStateService WizardState  // State only, no calculations
```

### Not Injected in Any V4 Panel
- PricingConfigPanel.razor - Uses hardcoded cloud provider data
- GrowthConfigPanel.razor - Uses simple local math, not IGrowthPlanningService

## Functionality Status by Feature

### 1. Platform Selection
| Feature | V4 Status | Reason |
|---------|-----------|--------|
| K8s Distribution List | Hardcoded | No IDistributionService |
| Distribution Details | Missing | No IDistributionService |
| Technology List | Hardcoded | No ITechnologyService |
| Platform Limits | Missing | No IDistributionService |

### 2. Application Configuration
| Feature | V4 Status | Reason |
|---------|-----------|--------|
| App Counts Input | UI Only | No calculation triggered |
| Tier Configuration | Missing | No ITierConfigurationService |
| Environment Toggle | UI Only | No state sync |

### 3. Node Specifications
| Feature | V4 Status | Reason |
|---------|-----------|--------|
| Node Specs Input | UI Only | Not used in calculations |
| Custom Node Types | Missing | No service integration |

### 4. Sizing Calculations
| Feature | V4 Status | Reason |
|---------|-----------|--------|
| K8s Sizing | NOT WORKING | No IK8sSizingService |
| VM Sizing | NOT WORKING | No IVMSizingService |
| Apply Button | Does Nothing | No calculation service call |

### 5. Cost Estimation
| Feature | V4 Status | Reason |
|---------|-----------|--------|
| On-Prem Costs | Local Math Only | No ICostEstimationService |
| Cloud Costs | Hardcoded Values | No IPricingService |
| TCO Calculation | Missing | No ICostEstimationService |
| Cloud Comparison | Hardcoded | No real pricing data |

### 6. Growth Planning
| Feature | V4 Status | Reason |
|---------|-----------|--------|
| Growth Projections | Simple Math | No IGrowthPlanningService |
| Cluster Limits | Missing | No distribution limits |
| Recommendations | Missing | No GenerateRecommendations |

### 7. Export & Sharing
| Feature | V4 Status | Reason |
|---------|-----------|--------|
| PDF Export | NOT WORKING | No IExportService |
| Excel Export | NOT WORKING | No IExportService |
| CSV Export | NOT WORKING | No IExportService |
| Share Link | NOT WORKING | No ConfigurationSharingService |

### 8. Scenario Management
| Feature | V4 Status | Reason |
|---------|-----------|--------|
| Save Scenario | NOT WORKING | No IScenarioService in Dashboard |
| Load Scenario | Partial | Scenarios page works |
| Compare Scenarios | Works | Compare.razor has IScenarioService |
| Recent Scenarios | Demo Data | Not loading from database |

## Pages with Correct Service Integration

| Page | Services Used | Status |
|------|---------------|--------|
| `/scenarios` | IScenarioService, ConfigurationSharingService | Working |
| `/compare` | IScenarioService | Working |
| `/settings` | IPricingSettingsService, IScenarioRepository, IAuthService, IAuthenticationSettingsService | Working |
| `/login` | IAuthService, IExternalAuthenticationService, ILdapAuthenticationService | Working |
| `/register` | IAuthService | Working |

## Pages with Missing Service Integration

| Page | Missing Services | Impact |
|------|------------------|--------|
| V4 Dashboard (`/`, `/dashboard`) | All core services | **Primary UI broken** |
| PlatformConfigPanel | IDistributionService, ITechnologyService | No real data |
| AppsConfigPanel | ITierConfigurationService | No tier configs |
| NodeSpecsConfigPanel | None needed (just UI) | OK |
| PricingConfigPanel | IPricingService, ICostEstimationService | Hardcoded data |
| GrowthConfigPanel | IGrowthPlanningService | Simple local math |

## Recommendations

### Priority 1: Critical Services (Dashboard Must Have)

1. **Inject calculation services into Dashboard.razor**:
   ```csharp
   @inject IK8sSizingService K8sSizingService
   @inject IVMSizingService VMSizingService
   @inject IDistributionService DistributionService
   @inject ITechnologyService TechnologyService
   @inject ICostEstimationService CostEstimationService
   @inject IExportService ExportService
   @inject IScenarioService ScenarioService
   ```

2. **Wire up ApplyPanelChanges to actual calculations**:
   - Currently does: `await Task.Delay(500)` (fake delay)
   - Should do: Call sizing services and update results

### Priority 2: Panel Service Integration

1. **PlatformConfigPanel** needs:
   - `IDistributionService.GetFiltered()` for distribution list
   - `ITechnologyService.GetAll()` for technology list

2. **PricingConfigPanel** needs:
   - `IPricingService.GetPricingAsync()` for cloud pricing
   - `IPricingService.GetOnPremPricing()` for on-prem defaults

3. **GrowthConfigPanel** needs:
   - `IGrowthPlanningService.CalculateK8sGrowthProjection()`
   - `IGrowthPlanningService.GetClusterLimits()`

### Priority 3: State Management

1. **Use IAppStateService** for results persistence
2. **Connect IWizardStateService** to calculation flow
3. **Load recent scenarios from database**, not demo data

### Implementation Approach

**Option A: Incremental Integration**
- Add services one-by-one to V4 Dashboard
- Wire up panels to use real services
- Replace hardcoded data with service calls
- Estimated: 3-5 days of work

**Option B: Service Facade**
- Create a single `IDashboardService` that orchestrates all services
- Dashboard injects only this facade
- Facade handles all calculation, export, scenario logic
- Estimated: 2-3 days of work

**Option C: Bridge to Legacy**
- V4 Dashboard calls Legacy Home.razor logic
- Reuse existing working code
- Gradually migrate
- Estimated: 1-2 days of work (quickest)

## Files to Modify

1. `Components/V4/Dashboard/Dashboard.razor` - Add service injections
2. `Components/V4/Dashboard/Dashboard.razor` - Implement real calculations
3. `Components/V4/Panels/PlatformConfigPanel.razor` - Add IDistributionService
4. `Components/V4/Panels/PricingConfigPanel.razor` - Add IPricingService
5. `Components/V4/Panels/GrowthConfigPanel.razor` - Add IGrowthPlanningService
6. `Components/V4/Panels/AppsConfigPanel.razor` - Add ITierConfigurationService

## Conclusion

The V4 Dashboard is currently a non-functional UI mockup. All core business functionality exists in the codebase but is not wired to the new design. The legacy `/legacy` route has full functionality and should be used as the reference implementation.

**Next Steps**:
1. Decide on implementation approach (A, B, or C)
2. Prioritize which features to wire up first
3. Create integration tests to verify service connectivity
