# Testing Patterns

**Analysis Date:** 2026-01-11

## Test Framework

**Runner:**
- xUnit 2.x
- bUnit for Blazor components
- Stryker.NET for mutation testing

**Assertion Library:**
- FluentAssertions
- xUnit built-in assertions

**Run Commands:**
```bash
dotnet test                              # Run all tests
dotnet test --filter "FullyQualifiedName~VMSizing"  # Filter
dotnet test /p:CollectCoverage=true      # With coverage
```

## Test File Organization

**Location:**
- `tests/InfraSizingCalculator.UnitTests/` - Unit tests (100+ files)
- `tests/InfraSizingCalculator.E2ETests/` - Playwright E2E

**Structure:**
```
tests/InfraSizingCalculator.UnitTests/
├── Services/
│   ├── VMSizingServiceTests.cs         # 30+ tests
│   ├── K8sSizingServiceTests.cs
│   ├── TechnologyServiceTests.cs
│   └── CostEstimationServiceTests.cs
├── Controllers/
│   ├── VMControllerTests.cs            # 18+ tests
│   └── K8sControllerTests.cs
├── Components/
│   └── VM/VMHADRConfigTests.cs
└── Models/
    └── ModelValidationTests.cs
```

## Test Structure

**VMSizingServiceTests Example:**
```csharp
public class VMSizingServiceTests
{
    private readonly VMSizingService _sut;

    public VMSizingServiceTests()
    {
        var settings = Options.Create(TestSettings.Default);
        _sut = new VMSizingService(settings);
    }

    [Fact]
    public void Calculate_ValidInput_ReturnsResult()
    {
        // Arrange
        var input = CreateVMInput();

        // Act
        var result = _sut.Calculate(input);

        // Assert
        result.Should().NotBeNull();
        result.GrandTotal.TotalVMs.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(ServerRole.Web, AppTier.Small, 2, 4)]
    [InlineData(ServerRole.Database, AppTier.Large, 16, 64)]
    public void GetRoleSpecs_RoleAndTier_ReturnsCorrectSpecs(
        ServerRole role, AppTier tier, int expectedCpu, int expectedRam)
    {
        var (cpu, ram) = _sut.GetRoleSpecs(role, tier, Technology.DotNet);
        cpu.Should().Be(expectedCpu);
        ram.Should().Be(expectedRam);
    }
}
```

## Business Rules Tested

**Role Specifications (CPU/RAM):**
| Role | Small | Medium | Large | XLarge |
|------|-------|--------|-------|--------|
| Web/App | 2/4 | 4/8 | 8/16 | 16/32 |
| Database | 4/16 | 8/32 | 16/64 | 32/128 |
| Cache | 2/8 | 4/16 | 8/32 | 16/64 |
| Bastion | 2/4 (fixed) | | | |

**HA Multipliers:**
| Pattern | Multiplier | Test Case |
|---------|------------|-----------|
| None | 1.0 | `GetHAMultiplier_None_Returns1` |
| ActiveActive | 2.0 | `GetHAMultiplier_ActiveActive_Returns2` |
| ActivePassive | 2.0 | `GetHAMultiplier_ActivePassive_Returns2` |
| NPlus1 | 1.5 | `GetHAMultiplier_NPlus1_Returns1_5` |
| NPlus2 | 1.67 | `GetHAMultiplier_NPlus2_Returns1_67` |

**Technology Memory Multipliers:**
| Technology | Multiplier | Test Case |
|------------|------------|-----------|
| Java, Mendix, OutSystems | 1.5x | `GetRoleSpecs_Java_AppliesMemoryMultiplier` |
| DotNet, Python, Go, NodeJs | 1.0x | `GetRoleSpecs_DotNet_NoMemoryMultiplier` |

## E2E Tests

**Location:** `tests/InfraSizingCalculator.E2ETests/`

**VM Flow Tests:**
- `VMFlowTests.cs` - Complete VM wizard flow
- Tests all steps from input to results

**K8s Flow Tests:**
- `MultiClusterTests.cs`
- `SharedClusterTests.cs`
- `PerEnvironmentTests.cs`

**Feature Tests:**
- `CostAnalysisTests.cs`
- `ExportFunctionalityTests.cs`
- `GrowthPlanningTests.cs`
- `ScenarioManagementTests.cs`

## Coverage

**Focus Areas:**
- VMSizingService (high coverage)
- K8sSizingService (high coverage)
- Controllers (API endpoints)
- Business rule calculations

**Run Coverage:**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=html
```

---

*Testing analysis: 2026-01-11*
*Update when test patterns change*
