# Test Coverage Gaps - Task List for Future Sessions

**Generated:** 2026-01-04
**Current Coverage:** 67.7% line / 51.9% branch
**Target Coverage:** 80%+ line / 70%+ branch

---

## Priority 1: High-Impact Components (Critical Path)

### Task 1.1: PricingSelector Component Tests
**File:** `src/.../Components/Configuration/PricingSelector.razor`
**Current:** 41.6% (95/228 lines)
**Target:** 85%+
**Estimated Tests:** 35-40

```
Tests needed:
- [ ] AutoSelectProviderFromDistribution() - all distribution cases
- [ ] GetProviderDisplayName() - all provider types
- [ ] GetCloudAlternatives() - various distributions
- [ ] Provider selection callbacks
- [ ] Distribution change handling
- [ ] Cloud vs On-Prem mode switching
- [ ] Managed cloud detection
- [ ] Region selection behavior
```

### Task 1.2: K8sNodeSpecsConfig Component Tests
**File:** `src/.../Components/K8s/K8sNodeSpecsConfig.razor`
**Current:** 45.7% (109/238 lines)
**Target:** 85%+
**Estimated Tests:** 40-45

```
Tests needed:
- [ ] SetMasterCpu/Ram/Disk handlers
- [ ] SetWorkerCpu/Ram/Disk handlers
- [ ] SetInfraCpu/Ram/Disk handlers
- [ ] Slider value changes
- [ ] Input validation boundaries
- [ ] Environment-specific configurations
- [ ] Reset to defaults functionality
- [ ] Cross-environment sync behavior
```

### Task 1.3: K8sSettingsConfig Component Tests
**File:** `src/.../Components/K8s/K8sSettingsConfig.razor`
**Current:** 26.6% (16/60 lines)
**Target:** 90%+
**Estimated Tests:** 20-25

```
Tests needed:
- [ ] Settings panel rendering
- [ ] Toggle switches behavior
- [ ] Configuration persistence
- [ ] Default value loading
- [ ] Settings change callbacks
- [ ] Validation messaging
```

### Task 1.4: CostEstimationPanel Component Tests
**File:** `src/.../Components/Results/CostEstimationPanel.razor`
**Current:** 34.6% (26/75 lines)
**Target:** 85%+
**Estimated Tests:** 25-30

```
Tests needed:
- [ ] HandleCalculateCosts() - success path
- [ ] HandleCalculateCosts() - error handling
- [ ] Cost breakdown display
- [ ] Loading state handling
- [ ] Empty state handling
- [ ] Refresh behavior
- [ ] Provider-specific cost display
```

---

## Priority 2: Model Validation Tests

### Task 2.1: NodeSpecsConfig Model Tests
**File:** `src/.../Models/NodeSpecsConfig.cs`
**Current:** 27.6% (54/195 lines)
**Target:** 90%+
**Estimated Tests:** 30-35

```
Tests needed:
- [ ] GetControlPlaneSpecs() - all environments
- [ ] GetWorkerSpecs() - all environments
- [ ] GetInfraSpecs() - all environments
- [ ] Default value initialization
- [ ] Validation rules
- [ ] Copy/clone behavior
- [ ] JSON serialization roundtrip
```

### Task 2.2: ReplicaSettings Model Tests
**File:** `src/.../Models/ReplicaSettings.cs`
**Current:** 27.5% (8/29 lines)
**Target:** 90%+
**Estimated Tests:** 15-20

```
Tests needed:
- [ ] Validate() - valid inputs
- [ ] Validate() - boundary conditions
- [ ] Validate() - invalid inputs
- [ ] Default values verification
- [ ] Property change behavior
```

### Task 2.3: OvercommitSettings Model Tests
**File:** `src/.../Models/OvercommitSettings.cs`
**Current:** 28.5% (6/21 lines)
**Target:** 90%+
**Estimated Tests:** 12-15

```
Tests needed:
- [ ] Validate() - valid ratios
- [ ] Validate() - boundary conditions
- [ ] Validate() - invalid ratios
- [ ] CPU/Memory ratio calculations
- [ ] Default values
```

### Task 2.4: SavedConfiguration Model Tests
**File:** `src/.../Models/SavedConfiguration.cs`
**Current:** 30.3% (10/33 lines)
**Target:** 85%+
**Estimated Tests:** 15-18

```
Tests needed:
- [ ] GenerateDescription() - K8s mode
- [ ] GenerateDescription() - VM mode
- [ ] GenerateDescription() - various configurations
- [ ] Serialization/deserialization
- [ ] Timestamp handling
```

### Task 2.5: DistributionConfig Model Tests
**File:** `src/.../Models/DistributionConfig.cs`
**Current:** 42.4% (14/33 lines)
**Target:** 85%+
**Estimated Tests:** 15-20

```
Tests needed:
- [ ] GetControlPlaneForEnv() - all environments
- [ ] GetWorkerForEnv() - all environments
- [ ] GetInfraForEnv() - all environments
- [ ] Default configuration creation
- [ ] Environment-specific overrides
```

### Task 2.6: PricingStepResult Model Tests
**File:** `src/.../Models/Pricing/PricingStepResult.cs`
**Current:** 13.7% (4/29 lines)
**Target:** 90%+
**Estimated Tests:** 20-25

```
Tests needed:
- [ ] get_MonthlyCost() - all scenarios
- [ ] get_YearlyCost() - calculation verification
- [ ] get_ThreeYearTCO() - calculation verification
- [ ] Cost aggregation logic
- [ ] Null/empty handling
```

### Task 2.7: HeadroomSettings Model Tests
**File:** `src/.../Models/HeadroomSettings.cs`
**Current:** 59.3% (19/32 lines)
**Target:** 90%+
**Estimated Tests:** 10-12

```
Tests needed:
- [ ] Validation rules
- [ ] Boundary conditions
- [ ] Default values
- [ ] Percentage calculations
```

### Task 2.8: AppConfig Model Tests
**File:** `src/.../Models/AppConfig.cs`
**Current:** 42.1% (16/38 lines)
**Target:** 85%+
**Estimated Tests:** 12-15

```
Tests needed:
- [ ] Property validation
- [ ] Default initialization
- [ ] Boundary conditions
- [ ] Tier-specific configurations
```

---

## Priority 3: Service Layer Gaps

### Task 3.1: PricingSettingsService Tests
**File:** `src/.../Services/Pricing/PricingSettingsService.cs`
**Current:** 75.5% (516/683 lines)
**Target:** 90%+
**Estimated Tests:** 40-50

```
Tests needed:
- [ ] BuildK8sEnvironmentDetails() - edge cases
- [ ] CalculateO11AddOnCosts() - all add-on combinations
- [ ] CalculateCloudCost() - all providers
- [ ] CalculateOtherDeploymentCost() - various scenarios
- [ ] Caching behavior
- [ ] Error handling paths
```

### Task 3.2: PricingService Tests
**File:** `src/.../Services/Pricing/PricingService.cs`
**Current:** 70.4% (74/105 lines)
**Target:** 90%+
**Estimated Tests:** 20-25

```
Tests needed:
- [ ] Price calculation edge cases
- [ ] Provider-specific pricing
- [ ] Region-based pricing
- [ ] Error handling
- [ ] Null input handling
```

### Task 3.3: DistributionService Gaps
**File:** `src/.../Services/DistributionService.cs`
**Current:** 86.4% (537/621 lines)
**Target:** 95%+
**Estimated Tests:** 15-20

```
Tests needed:
- [ ] Constructor initialization paths
- [ ] Less common distribution configurations
- [ ] Edge case handling
- [ ] All 46 distribution validations
```

### Task 3.4: AuthService Gaps
**File:** `src/.../Services/Auth/AuthService.cs`
**Current:** 89.1% (90/101 lines)
**Target:** 95%+
**Estimated Tests:** 8-10

```
Tests needed:
- [ ] GetCurrentUserAsync() - edge cases
- [ ] Error handling paths
- [ ] Session timeout scenarios
```

---

## Priority 4: Page Component Tests

### Task 4.1: Scenarios Page Tests
**File:** `src/.../Components/Pages/Scenarios.razor`
**Current:** 50.4% (108/214 lines)
**Target:** 80%+
**Estimated Tests:** 35-40

```
Tests needed:
- [ ] ViewScenario() - navigation
- [ ] Scenario list rendering
- [ ] Delete confirmation
- [ ] Empty state handling
- [ ] Error state handling
- [ ] Sorting/filtering
- [ ] Comparison feature
```

### Task 4.2: NodeSpecsPanel Component Tests
**File:** `src/.../Components/Configuration/NodeSpecsPanel.razor`
**Current:** 50% (16/32 lines)
**Target:** 85%+
**Estimated Tests:** 15-18

```
Tests needed:
- [ ] Panel rendering
- [ ] Input field bindings
- [ ] Validation display
- [ ] Reset functionality
- [ ] Environment switching
```

---

## Priority 5: Pricing Model Extensions

### Task 5.1: CloudProviderExtensions Tests
**File:** `src/.../Models/Pricing/CloudProviderExtensions.cs`
**Current:** 52.3% (55/105 lines)
**Target:** 90%+
**Estimated Tests:** 35-40

```
Tests needed:
- [ ] GetCloudProvider() - all distribution mappings
- [ ] GetControlPlaneCostPerHour() - all providers
- [ ] GetCrossAZCostMultiplier() - all providers
- [ ] Edge cases and null handling
```

### Task 5.2: Cloud Pricing Models (AWS/Azure/GCP)
**Files:** `AwsPricing.cs`, `AzurePricing.cs`, `GcpPricing.cs`
**Current:** 42.8% each (6/14 lines)
**Target:** 90%+
**Estimated Tests:** 25-30 (combined)

```
Tests needed:
- [ ] GetMonthlyInstanceCost() - all instance types
- [ ] Region-specific pricing
- [ ] Boundary conditions
- [ ] Invalid input handling
```

---

## Priority 6: Infrastructure/Telemetry (Lower Priority)

### Task 6.1: CalculatorMetrics Tests
**File:** `src/.../Services/Telemetry/CalculatorMetrics.cs`
**Current:** 0% (0/46 lines)
**Target:** 70%+
**Estimated Tests:** 15-20

```
Tests needed:
- [ ] Metric recording
- [ ] Counter increments
- [ ] Histogram observations
- [ ] Tag handling
```

### Task 6.2: ApplicationDbContext Tests
**File:** `src/.../Data/Identity/ApplicationDbContext.cs`
**Current:** 0% (0/34 lines)
**Target:** 60%+ (integration)
**Estimated Tests:** 8-10

```
Tests needed:
- [ ] Entity configuration
- [ ] Migration compatibility
- [ ] Seed data verification
```

---

## Excluded from Coverage Targets

The following are intentionally excluded or deprioritized:

1. **Home.razor (0%, 4,765 lines)** - Main orchestration page
   - Requires E2E/integration testing
   - Too complex for unit testing
   - Consider breaking into smaller components in future refactor

2. **Program.cs (0%, 224 lines)** - Startup configuration
   - Integration test territory
   - Consider startup validation tests

3. **BuildRenderTree methods** - Auto-generated by Blazor compiler
   - Covered indirectly through component tests

4. **Extension method wrappers** (e.g., `GlobalExceptionHandlerExtensions`)
   - Simple wrappers, low risk

---

## Session Execution Plan

### Session A: Priority 1 Components (Est. 2-3 hours)
1. PricingSelector tests (35-40 tests)
2. K8sNodeSpecsConfig tests (40-45 tests)
3. K8sSettingsConfig tests (20-25 tests)
4. CostEstimationPanel tests (25-30 tests)

**Expected Coverage Gain:** +3-4%

### Session B: Priority 2 Models (Est. 2 hours)
1. NodeSpecsConfig tests (30-35 tests)
2. ReplicaSettings tests (15-20 tests)
3. OvercommitSettings tests (12-15 tests)
4. SavedConfiguration tests (15-18 tests)
5. DistributionConfig tests (15-20 tests)
6. PricingStepResult tests (20-25 tests)
7. HeadroomSettings tests (10-12 tests)
8. AppConfig tests (12-15 tests)

**Expected Coverage Gain:** +2-3%

### Session C: Priority 3 Services (Est. 1.5 hours)
1. PricingSettingsService gaps (40-50 tests)
2. PricingService gaps (20-25 tests)
3. DistributionService gaps (15-20 tests)
4. AuthService gaps (8-10 tests)

**Expected Coverage Gain:** +2%

### Session D: Priority 4-5 (Est. 1.5 hours)
1. Scenarios page tests (35-40 tests)
2. NodeSpecsPanel tests (15-18 tests)
3. CloudProviderExtensions tests (35-40 tests)
4. Cloud pricing model tests (25-30 tests)

**Expected Coverage Gain:** +2-3%

### Session E: Priority 6 + Cleanup (Est. 1 hour)
1. CalculatorMetrics tests (15-20 tests)
2. ApplicationDbContext tests (8-10 tests)
3. Review and fix any failing tests
4. Final coverage report

**Expected Coverage Gain:** +1%

---

## Total Estimated New Tests: ~450-550

## Projected Final Coverage: 80-82% line / 68-72% branch

---

## Quick Reference: Files by Coverage

| Coverage | File | Lines Uncovered |
|----------|------|-----------------|
| 0% | Home.razor | 4,765 (excluded) |
| 0% | Program.cs | 224 (excluded) |
| 0% | CalculatorMetrics | 46 |
| 0% | ApplicationDbContext | 34 |
| 13.7% | PricingStepResult | 25 |
| 26.6% | K8sSettingsConfig | 44 |
| 27.5% | ReplicaSettings | 21 |
| 27.6% | NodeSpecsConfig | 141 |
| 28.5% | OvercommitSettings | 15 |
| 30.3% | SavedConfiguration | 23 |
| 34.6% | CostEstimationPanel | 49 |
| 41.6% | PricingSelector | 133 |
| 42.1% | AppConfig | 22 |
| 42.4% | DistributionConfig | 19 |
| 42.8% | AwsPricing/AzurePricing/GcpPricing | 8 each |
| 45.7% | K8sNodeSpecsConfig | 129 |
| 50% | NodeSpecsPanel | 16 |
| 50.4% | Scenarios | 106 |
| 52.3% | CloudProviderExtensions | 50 |
| 59.3% | HeadroomSettings | 13 |
| 70.4% | PricingService | 31 |
| 75.5% | PricingSettingsService | 167 |

---

## Notes for Future Sessions

1. **Test Pattern**: Follow existing test patterns in `VMHADRConfigTests.cs` for bUnit component tests
2. **Mocking**: Use NSubstitute for service mocks
3. **Assertions**: Use FluentAssertions for readable assertions
4. **Naming**: Follow `MethodName_Scenario_ExpectedResult` convention
5. **Coverage Tool**: Run `dotnet test --collect:"XPlat Code Coverage"` then `reportgenerator`
