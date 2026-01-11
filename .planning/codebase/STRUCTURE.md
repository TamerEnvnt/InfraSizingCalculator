# Codebase Structure

**Analysis Date:** 2026-01-11

## Directory Layout

```
InfraSizingCalculator/
├── src/InfraSizingCalculator/          # Main application
│   ├── Components/                      # Blazor UI (68 files)
│   │   ├── Pages/                       # Routable pages
│   │   ├── Configuration/               # Config panels
│   │   ├── K8s/                         # K8s components
│   │   ├── VM/                          # VM components
│   │   ├── Pricing/                     # Pricing panels
│   │   ├── Results/                     # Result views
│   │   ├── Wizard/                      # Wizard framework
│   │   ├── Shared/                      # Reusable UI
│   │   ├── Modals/                      # Modal dialogs
│   │   └── Layout/                      # Layouts, nav
│   ├── Controllers/Api/                 # REST endpoints (4)
│   ├── Services/                        # Business logic (20+)
│   │   └── Interfaces/                  # Service contracts
│   ├── Models/                          # Domain models (27+)
│   │   └── Enums/                       # 11 enumerations
│   ├── Data/                            # EF contexts
│   ├── Helpers/                         # Utility classes
│   ├── Middleware/                      # HTTP middleware
│   └── wwwroot/                         # Static assets
└── tests/                               # All tests (100+ files)
    ├── InfraSizingCalculator.UnitTests/ # xUnit + bUnit
    │   ├── Services/                    # Service tests
    │   ├── Controllers/                 # Controller tests
    │   ├── Components/                  # Component tests
    │   ├── Models/                      # Model tests
    │   └── Middleware/                  # Middleware tests
    └── InfraSizingCalculator.E2ETests/  # Playwright
        ├── VM/                          # VM flow tests
        ├── K8s/                         # K8s flow tests
        ├── Features/                    # Feature tests
        └── PageObjects/                 # Page object models
```

## Directory Purposes

**Services/**
- Purpose: Business logic layer
- Contains: Sizing, pricing, export, state services
- Key files: `VMSizingService.cs`, `K8sSizingService.cs`, `TechnologyService.cs`
- Subdirectories: `Interfaces/` for contracts

**Models/**
- Purpose: Domain models and DTOs
- Contains: Input/Result models, configurations
- Key files: `VMSizingInput.cs`, `VMSizingResult.cs`, `VMRoleConfig.cs`
- Subdirectories: `Enums/` (11 enumerations)

**Components/VM/**
- Purpose: VM-specific UI components
- Contains: Role config, HA/DR config components
- Key files: `VMServerRolesConfig.razor`, `VMHADRConfig.razor`

**tests/InfraSizingCalculator.UnitTests/Services/**
- Purpose: Service unit tests
- Key files: `VMSizingServiceTests.cs` (30+ tests), `K8sSizingServiceTests.cs`

## Key File Locations

**Entry Points:**
- `src/InfraSizingCalculator/Program.cs` - Application startup

**Configuration:**
- `appsettings.json` - CalculatorSettings configuration
- `*.csproj` - Project configuration

**Core VM Logic:**
- `src/InfraSizingCalculator/Services/VMSizingService.cs` - VM calculations
- `src/InfraSizingCalculator/Services/Interfaces/IVMSizingService.cs` - Interface
- `src/InfraSizingCalculator/Controllers/Api/VMController.cs` - REST API

**VM Models:**
- `src/InfraSizingCalculator/Models/VMSizingInput.cs` - Input model
- `src/InfraSizingCalculator/Models/VMSizingResult.cs` - Result model
- `src/InfraSizingCalculator/Models/VMEnvironmentConfig.cs` - Environment config
- `src/InfraSizingCalculator/Models/VMRoleConfig.cs` - Role config

**Testing:**
- `tests/InfraSizingCalculator.UnitTests/Services/VMSizingServiceTests.cs` - VM service tests
- `tests/InfraSizingCalculator.UnitTests/Controllers/VMControllerTests.cs` - Controller tests
- `tests/InfraSizingCalculator.E2ETests/VM/VMFlowTests.cs` - E2E flow tests

## Naming Conventions

**Files:**
- PascalCase.cs for C# source files
- PascalCase.razor for Blazor components
- {ClassName}Tests.cs for test files
- I{Name}.cs for interfaces

**Directories:**
- PascalCase for component directories
- Plural names for collections (Services, Models, Enums)

## Where to Add New Code

**New VM Feature:**
- Service: `src/InfraSizingCalculator/Services/`
- Tests: `tests/InfraSizingCalculator.UnitTests/Services/`
- Component: `src/InfraSizingCalculator/Components/VM/`

**New Server Role:**
- Enum: `src/InfraSizingCalculator/Models/Enums/ServerRole.cs`
- Specs: Update `CalculatorSettings.VMRoles` in appsettings.json
- Tests: Add to `VMSizingServiceTests.cs`

**New API Endpoint:**
- Controller: `src/InfraSizingCalculator/Controllers/Api/`
- Tests: `tests/InfraSizingCalculator.UnitTests/Controllers/`

---

*Structure analysis: 2026-01-11*
*Update when directory structure changes*
