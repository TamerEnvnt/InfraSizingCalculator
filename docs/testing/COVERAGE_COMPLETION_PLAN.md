# Coverage Completion Plan - Infrastructure Sizing Calculator

## Current Status (December 25, 2025)
- **Total Tests:** 1822 passing
- **Line Coverage:** 49.5%
- **Method Coverage:** 62.1%
- **Branch Coverage:** 27%

---

## Phase 1: Layout Components (0% → 100%)

### 1.1 MainLayout.razor
- Test theme initialization and persistence
- Test sidebar toggle functionality
- Test responsive behavior
- Test child content rendering

### 1.2 HeaderBar.razor
- Test title rendering
- Test navigation links
- Test theme toggle button
- Test user actions/buttons

### 1.3 LeftSidebar.razor
- Test menu item rendering
- Test active state highlighting
- Test collapse/expand functionality
- Test navigation callbacks

### 1.4 RightStatsSidebar.razor
- Test stats display
- Test real-time updates
- Test responsive visibility

### 1.5 FullPageLayout.razor
- Test full-page content rendering
- Test header/footer slots

---

## Phase 2: Page Components (0% → 100%)

### 2.1 Home.razor (Main Calculator Page)
- Test wizard initialization
- Test step navigation
- Test input validation
- Test calculation trigger
- Test results display
- Test error handling
- Test loading states

### 2.2 Scenarios.razor
- Test scenario list rendering
- Test scenario loading
- Test scenario deletion with confirmation
- Test scenario comparison
- Test empty state
- Test search/filter functionality

### 2.3 Settings.razor
- Test settings form rendering
- Test settings persistence
- Test validation
- Test reset to defaults
- Test theme switching

### 2.4 Counter.razor (Demo page)
- Test increment functionality
- Test display updates

### 2.5 Weather.razor (Demo page)
- Test data fetching
- Test loading state
- Test error handling

### 2.6 Error.razor
- Test error display
- Test error details in development mode
- Test navigation back

---

## Phase 3: Wizard Step Components (0% → 100%)

### 3.1 PlatformStep.razor
- Test VM vs K8s selection
- Test selection card interactions
- Test OnPlatformSelected callback
- Test initial state

### 3.2 DeploymentStep.razor
- Test cluster mode selection (SharedCluster, PerEnvironment, MultiCluster)
- Test mode descriptions
- Test OnClusterModeSelected callback

### 3.3 DistributionStep.razor
- Test distribution options (OpenShift, Kubernetes, Rancher, etc.)
- Test distribution info display
- Test vendor logos
- Test OnDistributionSelected callback

### 3.4 TechnologyStep.razor
- Test technology selection (Mendix, .NET, Java, etc.)
- Test technology descriptions
- Test resource requirements display
- Test OnTechnologySelected callback

### 3.5 PricingStep.razor
- Test pricing mode selection
- Test cloud provider selection
- Test region selection
- Test pricing configuration

---

## Phase 4: Configuration Panels (0% → 100%)

### 4.1 GrowthSettingsPanel.razor
- Test growth rate inputs
- Test projection period selection
- Test custom growth rates toggle
- Test validation
- Test OnSettingsChanged callback

### 4.2 NodeSpecsPanel.razor
- Test CPU/RAM/Disk inputs
- Test preset selections
- Test custom specs toggle
- Test validation (min/max values)
- Test OnSpecsChanged callback

### 4.3 PricingSelector.razor
- Test pricing type selection
- Test discount inputs
- Test reserved instance options
- Test OnPricingChanged callback

---

## Phase 5: Pricing Components (0% → 100%)

### 5.1 CloudPricingPanel.razor
- Test provider tabs (AWS, Azure, GCP, OCI)
- Test region selection
- Test instance type display
- Test pricing calculation display
- Test refresh pricing button

### 5.2 OnPremPricingPanel.razor
- Test hardware cost inputs
- Test labor cost inputs
- Test data center cost inputs
- Test license cost inputs
- Test total calculation

### 5.3 CloudAlternativesPanel.razor
- Test alternatives display
- Test comparison metrics
- Test savings calculations
- Test provider recommendations

---

## Phase 6: Results Components (0% → 100%)

### 6.1 SizingResultsView.razor
- Test results table rendering
- Test environment breakdown
- Test node specifications
- Test export buttons

### 6.2 CostAnalysisView.razor
- Test cost breakdown display
- Test chart rendering
- Test comparison views
- Test TCO calculations

### 6.3 GrowthPlanningView.razor
- Test projection display
- Test timeline rendering
- Test scaling recommendations
- Test warning display

### 6.4 CostEstimationPanel.razor
- Test cost estimation workflow
- Test provider selection
- Test estimation results

### 6.5 GrowthProjectionChart.razor
- Test chart data binding
- Test axis labels
- Test tooltips
- Test responsive sizing

### 6.6 GrowthTimeline.razor
- Test timeline markers
- Test milestone display
- Test scaling events

---

## Phase 7: Shared Components (Remaining)

### 7.1 HorizontalAccordionPanel.razor
- Test expand/collapse
- Test content rendering
- Test header click handling

---

## Phase 8: Models with Partial Coverage

### 8.1 K8sSizingInput (68.7% → 100%)
- Test all property setters
- Test validation attributes
- Test default values
- Test Clone() method if exists

### 8.2 VMSizingInput (82.1% → 100%)
- Test remaining property paths
- Test validation

### 8.3 HeadroomSettings (59.3% → 100%)
- Test all headroom configurations
- Test percentage calculations

### 8.4 OvercommitSettings (28.5% → 100%)
- Test CPU overcommit ratios
- Test memory overcommit ratios
- Test validation

### 8.5 ReplicaSettings (27.5% → 100%)
- Test replica count configurations
- Test HA settings

### 8.6 DistributionConfig (42.4% → 100%)
- Test all distribution properties
- Test vendor-specific settings

### 8.7 NodeSpecsConfig (46.1% → 100%)
- Test node specification options
- Test preset configurations

### 8.8 SavedConfiguration (30.3% → 100%)
- Test serialization/deserialization
- Test all configuration properties

### 8.9 AppConfig (42.1% → 100%)
- Test app configuration loading
- Test default values

---

## Phase 9: Services with Partial Coverage

### 9.1 PricingService (70.4% → 100%)
- Test cloud pricing API calls
- Test caching behavior
- Test error handling for API failures
- Test all provider-specific paths

### 9.2 PricingSettingsService (63.6% → 100%)
- Test settings CRUD operations
- Test validation
- Test default settings

### 9.3 VMController (73.6% → 100%)
- Test all API endpoints
- Test validation errors
- Test edge cases

### 9.4 SaveScenarioModal (70.1% → 100%)
- Test VM scenario saving
- Test error states
- Test tag parsing edge cases

---

## Phase 10: Middleware & Infrastructure (0% → 100%)

### 10.1 GlobalExceptionHandler
- Test exception catching
- Test error response formatting
- Test logging

### 10.2 Program.cs
- Integration tests for startup
- Test service registration
- Test middleware pipeline

---

## Phase 11: Pricing Models with Partial Coverage

### 11.1 AwsPricing, AzurePricing, GcpPricing (42.8% each → 100%)
- Test all pricing tier calculations
- Test region-specific pricing
- Test discount applications

### 11.2 CloudAlternatives (58.5% → 100%)
- Test alternative generation
- Test comparison logic

### 11.3 DistributionLicensing (51.3% → 100%)
- Test all licensing models
- Test cost calculations

### 11.4 CostEstimate (72.2% → 100%)
- Test all cost categories
- Test aggregation methods

---

## Implementation Priority

### High Priority (Core Functionality)
1. Phase 3: Wizard Steps (critical user path)
2. Phase 2: Home.razor (main page)
3. Phase 6: Results Components

### Medium Priority (User Experience)
4. Phase 4: Configuration Panels
5. Phase 5: Pricing Components
6. Phase 1: Layout Components

### Lower Priority (Supporting)
7. Phase 7-11: Remaining models and services

---

## Testing Approach

### For Blazor Components (bUnit)
```csharp
// Pattern for testing components
public class ComponentTests : TestContext
{
    [Fact]
    public void Component_RendersCorrectly()
    {
        // Arrange - Setup services and mocks
        Services.AddSingleton(mockService);

        // Act - Render component
        var cut = RenderComponent<MyComponent>(parameters => parameters
            .Add(p => p.Property, value));

        // Assert - Verify rendering
        cut.Find(".expected-class").Should().NotBeNull();
    }
}
```

### For Services (Unit Tests)
```csharp
// Pattern for service tests
public class ServiceTests
{
    [Fact]
    public async Task Method_ShouldReturnExpectedResult()
    {
        // Arrange
        var service = new MyService(dependencies);

        // Act
        var result = await service.MethodAsync(input);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
```

### For API Controllers
```csharp
// Pattern for controller tests
public class ControllerTests
{
    [Fact]
    public async Task Endpoint_ReturnsOk_WithValidInput()
    {
        // Arrange
        var controller = new MyController(mockService);

        // Act
        var result = await controller.Action(validInput);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
```

---

## Estimated Test Counts per Phase

| Phase | Components | Est. Tests | Coverage Goal |
|-------|-----------|------------|---------------|
| 1 | Layout (5) | ~50 | 100% |
| 2 | Pages (6) | ~120 | 100% |
| 3 | Wizard Steps (5) | ~75 | 100% |
| 4 | Config Panels (3) | ~60 | 100% |
| 5 | Pricing (3) | ~80 | 100% |
| 6 | Results (6) | ~100 | 100% |
| 7 | Shared (1) | ~15 | 100% |
| 8 | Models (9) | ~150 | 100% |
| 9 | Services (4) | ~80 | 100% |
| 10 | Middleware (2) | ~30 | 100% |
| 11 | Pricing Models (5) | ~100 | 100% |
| **Total** | **49** | **~860** | **100%** |

---

## Files to Create

1. `tests/.../Components/Layout/MainLayoutTests.cs`
2. `tests/.../Components/Layout/HeaderBarTests.cs`
3. `tests/.../Components/Layout/LeftSidebarTests.cs`
4. `tests/.../Components/Layout/RightStatsSidebarTests.cs`
5. `tests/.../Components/Pages/HomeTests.cs`
6. `tests/.../Components/Pages/ScenariosTests.cs`
7. `tests/.../Components/Pages/SettingsTests.cs`
8. `tests/.../Components/Wizard/Steps/PlatformStepTests.cs`
9. `tests/.../Components/Wizard/Steps/DeploymentStepTests.cs`
10. `tests/.../Components/Wizard/Steps/DistributionStepTests.cs`
11. `tests/.../Components/Wizard/Steps/TechnologyStepTests.cs`
12. `tests/.../Components/Wizard/Steps/PricingStepTests.cs`
13. `tests/.../Components/Configuration/GrowthSettingsPanelTests.cs`
14. `tests/.../Components/Configuration/NodeSpecsPanelTests.cs`
15. `tests/.../Components/Configuration/PricingSelectorTests.cs`
16. `tests/.../Components/Pricing/CloudPricingPanelTests.cs`
17. `tests/.../Components/Pricing/OnPremPricingPanelTests.cs`
18. `tests/.../Components/Pricing/CloudAlternativesPanelTests.cs`
19. `tests/.../Components/Results/SizingResultsViewTests.cs`
20. `tests/.../Components/Results/CostAnalysisViewTests.cs`
21. `tests/.../Components/Results/GrowthPlanningViewTests.cs`
22. `tests/.../Components/Results/CostEstimationPanelTests.cs`
23. `tests/.../Components/Results/GrowthProjectionChartTests.cs`
24. `tests/.../Components/Results/GrowthTimelineTests.cs`
25. `tests/.../Middleware/GlobalExceptionHandlerTests.cs`
26. Additional model tests in existing files

---

## Session Resume Command

To continue this work in terminal:
```bash
cd /Users/tamer/Work/AI/Claude/InfraSizingCalculator
claude-code
# Then say: "Continue with the coverage completion plan in COVERAGE_COMPLETION_PLAN.md, starting with Phase 1"
```

---

## Current Test File Locations

Existing test files:
- `tests/InfraSizingCalculator.UnitTests/Components/` - UI component tests
- `tests/InfraSizingCalculator.UnitTests/Services/` - Service tests
- `tests/InfraSizingCalculator.UnitTests/` - Root level service tests

---

## Notes

- All tests use xUnit, FluentAssertions, NSubstitute, and bUnit
- EF Core InMemory is available for database tests
- IJSRuntime mocking pattern established in SettingsPersistenceServiceTests
- Component testing pattern established in existing UI tests

Last Updated: December 25, 2025
