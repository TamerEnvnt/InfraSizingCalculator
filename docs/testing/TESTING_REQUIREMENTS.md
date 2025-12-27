# Complete Testing Requirements - Infrastructure Sizing Calculator

## Current Test Stack (Already Installed)

| Package | Version | Purpose |
|---------|---------|---------|
| xunit | 2.9.3 | Test framework |
| xunit.runner.visualstudio | 3.1.4 | VS test runner |
| Microsoft.NET.Test.Sdk | 17.14.1 | Test SDK |
| bunit | 1.31.3 | Blazor component testing |
| FluentAssertions | 6.12.2 | Readable assertions |
| NSubstitute | 5.3.0 | Mocking framework |
| coverlet.collector | 6.0.4 | Code coverage |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.0 | In-memory database testing |

---

## Additional Packages Needed

### 1. Security Testing
```xml
<!-- Add to test project .csproj -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
<PackageReference Include="AngleSharp" Version="1.1.2" />
```

**Purpose:**
- `Microsoft.AspNetCore.Mvc.Testing` - Integration tests, API security testing
- `AngleSharp` - HTML parsing for XSS detection tests

### 2. API/Controller Testing
```xml
<PackageReference Include="Microsoft.AspNetCore.TestHost" Version="9.0.0" />
<PackageReference Include="Moq" Version="4.20.72" />
```

**Purpose:**
- `Microsoft.AspNetCore.TestHost` - In-memory test server
- `Moq` - Alternative mocking (optional, NSubstitute is sufficient)

### 3. Snapshot Testing (UI Regression)
```xml
<PackageReference Include="Verify.Xunit" Version="26.6.0" />
<PackageReference Include="Verify.Bunit" Version="2.0.0" />
```

**Purpose:**
- Snapshot testing for UI components
- Detect unintended UI changes

### 4. Performance/Load Testing
```xml
<PackageReference Include="NBomber" Version="5.6.0" />
<PackageReference Include="BenchmarkDotNet" Version="0.14.0" />
```

**Purpose:**
- `NBomber` - Load testing
- `BenchmarkDotNet` - Performance benchmarking

### 5. Mutation Testing (Test Quality)
```bash
# Install as global tool
dotnet tool install --global dotnet-stryker
```

**Purpose:**
- Verify test effectiveness by introducing mutations

---

## Required NuGet Packages - Complete List

### Update test project .csproj:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core Testing -->
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />

    <!-- Blazor Testing -->
    <PackageReference Include="bunit" Version="1.31.3" />

    <!-- Assertions & Mocking -->
    <PackageReference Include="FluentAssertions" Version="6.12.2" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="NSubstitute.Analyzers.CSharp" Version="1.0.17" />

    <!-- Database Testing -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />

    <!-- Integration/API Testing -->
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.TestHost" Version="9.0.0" />

    <!-- Security Testing -->
    <PackageReference Include="AngleSharp" Version="1.1.2" />

    <!-- Snapshot Testing -->
    <PackageReference Include="Verify.Xunit" Version="26.6.0" />
    <PackageReference Include="Verify.Bunit" Version="2.0.0" />

    <!-- Coverage -->
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="coverlet.msbuild" Version="6.0.4" />

    <!-- Performance (Optional) -->
    <PackageReference Include="BenchmarkDotNet" Version="0.14.0" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\InfraSizingCalculator\InfraSizingCalculator.csproj" />
  </ItemGroup>

</Project>
```

---

## Global Tools Needed

```bash
# Coverage report generator (already installed)
dotnet tool install --global dotnet-reportgenerator-globaltool

# Mutation testing
dotnet tool install --global dotnet-stryker

# Security scanning
dotnet tool install --global security-scan

# Code analysis
dotnet tool install --global dotnet-format
```

---

## Test Categories by Type

### 1. Unit Tests (Current Focus)
**Tools:** xunit, FluentAssertions, NSubstitute, bunit

**Coverage Areas:**
- Services (business logic)
- Models (validation, serialization)
- Components (rendering, interactions)
- Controllers (API endpoints)

### 2. Integration Tests
**Tools:** Microsoft.AspNetCore.Mvc.Testing, TestHost

**Coverage Areas:**
```csharp
// Example: API Integration Test
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task K8sCalculate_ReturnsValidResult()
    {
        var input = new K8sSizingInput { /* ... */ };
        var response = await _client.PostAsJsonAsync("/api/k8s/calculate", input);
        response.EnsureSuccessStatusCode();
    }
}
```

### 3. Security Tests
**Tools:** AngleSharp, custom validators

**Coverage Areas:**
- XSS prevention
- Input validation
- SQL injection prevention
- CSRF protection
- Authentication/Authorization

```csharp
// Example: XSS Prevention Test
[Fact]
public void Input_SanitizesHtmlContent()
{
    var maliciousInput = "<script>alert('xss')</script>";
    var result = InputSanitizer.Sanitize(maliciousInput);
    result.Should().NotContain("<script>");
}
```

### 4. UI/UX Tests
**Tools:** bunit, Verify.Bunit

**Coverage Areas:**
- Component rendering
- User interactions (clicks, inputs)
- State management
- Accessibility
- Responsive behavior

```csharp
// Example: Accessibility Test
[Fact]
public void Button_HasAriaLabel()
{
    var cut = RenderComponent<SubmitButton>();
    var button = cut.Find("button");
    button.GetAttribute("aria-label").Should().NotBeNullOrEmpty();
}
```

### 5. Snapshot Tests
**Tools:** Verify.Xunit, Verify.Bunit

**Coverage Areas:**
- UI regression detection
- Component structure verification

```csharp
// Example: Snapshot Test
[Fact]
public async Task WizardStepper_MatchesSnapshot()
{
    var cut = RenderComponent<WizardStepper>(p => p
        .Add(x => x.CurrentStep, 3)
        .Add(x => x.TotalSteps, 5));

    await Verify(cut.Markup);
}
```

### 6. Performance Tests
**Tools:** BenchmarkDotNet

**Coverage Areas:**
- Calculation performance
- Memory usage
- API response times

```csharp
// Example: Performance Benchmark
[MemoryDiagnoser]
public class K8sSizingBenchmarks
{
    [Benchmark]
    public K8sSizingResult CalculateSizing()
    {
        var service = new K8sSizingService(...);
        return service.CalculateSizing(largeInput);
    }
}
```

---

## Test File Structure

```
tests/
└── InfraSizingCalculator.UnitTests/
    ├── Components/
    │   ├── Configuration/
    │   │   ├── AppCountsPanelTests.cs ✓
    │   │   ├── GrowthSettingsPanelTests.cs (NEW)
    │   │   ├── NodeSpecsPanelTests.cs (NEW)
    │   │   └── PricingSelectorTests.cs (NEW)
    │   ├── Layout/
    │   │   ├── MainLayoutTests.cs (NEW)
    │   │   ├── HeaderBarTests.cs (NEW)
    │   │   ├── LeftSidebarTests.cs (NEW)
    │   │   └── RightStatsSidebarTests.cs (NEW)
    │   ├── Modals/
    │   │   ├── InfoModalTests.cs ✓
    │   │   └── SaveScenarioModalTests.cs ✓
    │   ├── Pages/
    │   │   ├── HomeTests.cs (NEW)
    │   │   ├── ScenariosTests.cs (NEW)
    │   │   └── SettingsTests.cs (NEW)
    │   ├── Pricing/
    │   │   ├── CloudPricingPanelTests.cs (NEW)
    │   │   ├── OnPremPricingPanelTests.cs (NEW)
    │   │   └── CloudAlternativesPanelTests.cs (NEW)
    │   ├── Results/
    │   │   ├── CostSummaryTests.cs ✓
    │   │   ├── ResultsWarningsTests.cs ✓
    │   │   ├── SizingResultsViewTests.cs (NEW)
    │   │   ├── CostAnalysisViewTests.cs (NEW)
    │   │   └── GrowthPlanningViewTests.cs (NEW)
    │   ├── Shared/
    │   │   ├── FilterButtonsTests.cs ✓
    │   │   └── SelectionCardTests.cs ✓
    │   └── Wizard/
    │       ├── WizardContainerTests.cs ✓
    │       ├── WizardStepperTests.cs ✓
    │       └── Steps/
    │           ├── PlatformStepTests.cs (NEW)
    │           ├── DeploymentStepTests.cs (NEW)
    │           ├── DistributionStepTests.cs (NEW)
    │           ├── TechnologyStepTests.cs (NEW)
    │           └── PricingStepTests.cs (NEW)
    ├── Controllers/
    │   ├── K8sControllerTests.cs ✓
    │   ├── VMControllerTests.cs ✓
    │   └── Api/
    │       └── DistributionsControllerTests.cs ✓
    ├── Services/
    │   ├── K8sSizingServiceTests.cs ✓
    │   ├── VMSizingServiceTests.cs ✓
    │   ├── CostEstimationServiceTests.cs ✓
    │   ├── SettingsPersistenceServiceTests.cs ✓
    │   ├── DatabasePricingSettingsServiceTests.cs ✓
    │   └── Pricing/
    │       ├── PricingServiceTests.cs (EXPAND)
    │       └── PricingSettingsServiceTests.cs (EXPAND)
    ├── Models/
    │   ├── K8sSizingInputTests.cs (EXPAND)
    │   ├── VMSizingInputTests.cs (EXPAND)
    │   └── ValidationTests.cs (NEW)
    ├── Security/
    │   ├── InputValidationTests.cs (NEW)
    │   ├── XssPreventionTests.cs (NEW)
    │   └── ApiSecurityTests.cs (NEW)
    ├── Integration/
    │   ├── ApiIntegrationTests.cs (NEW)
    │   └── DatabaseIntegrationTests.cs (NEW)
    ├── Performance/
    │   └── SizingBenchmarks.cs (NEW)
    └── Snapshots/
        └── ComponentSnapshotTests.cs (NEW)
```

---

## Mocking Patterns Needed

### 1. IJSRuntime (localStorage, sessionStorage)
```csharp
var mockJs = Substitute.For<IJSRuntime>();
mockJs.InvokeAsync<string>("localStorage.getItem", Arg.Any<object[]>())
    .Returns(new ValueTask<string>("stored-value"));
```

### 2. HttpClient (API calls)
```csharp
var mockHandler = new MockHttpMessageHandler();
mockHandler.When("/api/*").Respond("application/json", "{}");
var client = new HttpClient(mockHandler);
```

### 3. NavigationManager
```csharp
var mockNav = Substitute.For<NavigationManager>();
mockNav.Uri.Returns("http://localhost/");
```

### 4. ILogger
```csharp
var mockLogger = Substitute.For<ILogger<MyService>>();
```

### 5. DbContext
```csharp
var options = new DbContextOptionsBuilder<InfraSizingDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;
var context = new InfraSizingDbContext(options);
```

---

## Security Test Checklist

### Input Validation
- [ ] Numeric inputs reject non-numeric values
- [ ] String inputs have max length limits
- [ ] Enum values are validated
- [ ] Required fields are enforced
- [ ] Range validations work correctly

### XSS Prevention
- [ ] User input is HTML encoded in output
- [ ] JavaScript injection is blocked
- [ ] SVG injection is blocked
- [ ] CSS injection is blocked

### API Security
- [ ] Invalid JSON returns 400
- [ ] Missing required fields returns 400
- [ ] Oversized payloads are rejected
- [ ] Rate limiting works (if implemented)
- [ ] CORS is properly configured

### Data Protection
- [ ] Sensitive data is not logged
- [ ] Passwords are hashed (if applicable)
- [ ] API keys are not exposed in responses

---

## UI/UX Test Checklist

### Accessibility
- [ ] All images have alt text
- [ ] Forms have proper labels
- [ ] Color contrast meets WCAG
- [ ] Keyboard navigation works
- [ ] Screen reader compatible

### Responsiveness
- [ ] Mobile layout renders correctly
- [ ] Tablet layout renders correctly
- [ ] Desktop layout renders correctly
- [ ] Touch interactions work

### User Interactions
- [ ] Buttons respond to clicks
- [ ] Forms validate on submit
- [ ] Loading states are shown
- [ ] Error messages are clear
- [ ] Success feedback is provided

### State Management
- [ ] State persists across navigation
- [ ] Undo/redo works (if applicable)
- [ ] Form state is preserved
- [ ] Dirty state is tracked

---

## Logic Test Checklist

### Calculations
- [ ] K8s sizing formulas are correct
- [ ] VM sizing formulas are correct
- [ ] Cost calculations are accurate
- [ ] Growth projections are correct
- [ ] Rounding is consistent

### Edge Cases
- [ ] Zero values handled
- [ ] Maximum values handled
- [ ] Negative values rejected
- [ ] Null inputs handled
- [ ] Empty collections handled

### Business Rules
- [ ] Environment constraints enforced
- [ ] Resource limits respected
- [ ] Licensing rules applied
- [ ] Pricing tiers correct

---

## Commands Quick Reference

```bash
# Install all new packages
dotnet add tests/InfraSizingCalculator.UnitTests package Microsoft.AspNetCore.Mvc.Testing --version 9.0.0
dotnet add tests/InfraSizingCalculator.UnitTests package Microsoft.AspNetCore.TestHost --version 9.0.0
dotnet add tests/InfraSizingCalculator.UnitTests package AngleSharp --version 1.1.2
dotnet add tests/InfraSizingCalculator.UnitTests package Verify.Xunit --version 26.6.0
dotnet add tests/InfraSizingCalculator.UnitTests package Verify.Bunit --version 2.0.0
dotnet add tests/InfraSizingCalculator.UnitTests package coverlet.msbuild --version 6.0.4
dotnet add tests/InfraSizingCalculator.UnitTests package NSubstitute.Analyzers.CSharp --version 1.0.17

# Install global tools
dotnet tool install --global dotnet-stryker
dotnet tool install --global security-scan

# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Generate HTML report
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:"Html"

# Run mutation testing
dotnet stryker

# Run security scan
security-scan ./src/InfraSizingCalculator/
```

---

## Summary of Required Additions

| Category | Packages to Add | Tests to Create |
|----------|-----------------|-----------------|
| Integration | Mvc.Testing, TestHost | ~30 tests |
| Security | AngleSharp | ~40 tests |
| Snapshots | Verify.Xunit, Verify.Bunit | ~50 tests |
| Performance | BenchmarkDotNet | ~20 benchmarks |
| UI/UX | (use existing bunit) | ~200 tests |
| Logic | (use existing) | ~300 tests |
| **Total** | 7 new packages | ~640 new tests |

Combined with existing 1822 tests = **~2,462 total tests** for 100% coverage.

---

Last Updated: December 25, 2025
