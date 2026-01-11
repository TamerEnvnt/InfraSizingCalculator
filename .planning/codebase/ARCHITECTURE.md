# Architecture

**Analysis Date:** 2026-01-11

## Pattern Overview

**Overall:** Blazor Server Application with Layered Architecture

**Key Characteristics:**
- Interactive server-side UI with SignalR
- Service-oriented business logic layer
- REST API for external access
- Comprehensive test coverage focus

## Layers

**Presentation Layer (Components/):**
- Purpose: Blazor UI components and pages
- Contains: 68 Blazor components (Wizard, VM, K8s, Results)
- Location: `src/InfraSizingCalculator/Components/`
- Depends on: Services layer
- Used by: Users via browser

**API Layer (Controllers/):**
- Purpose: REST API endpoints
- Contains: VM, K8s, Technologies, Distributions controllers
- Location: `src/InfraSizingCalculator/Controllers/Api/`
- Depends on: Services layer
- Used by: External clients

**Service Layer (Services/):**
- Purpose: Business logic and calculations
- Contains: 20+ services
- Location: `src/InfraSizingCalculator/Services/`
- Key files: `VMSizingService.cs`, `K8sSizingService.cs`
- Depends on: Models, CalculatorSettings
- Used by: Components, Controllers

**Data Layer (Models/):**
- Purpose: Domain models, DTOs, enumerations
- Contains: 27+ models
- Location: `src/InfraSizingCalculator/Models/`
- Key files: `VMSizingInput.cs`, `VMSizingResult.cs`, `VMRoleConfig.cs`

## Data Flow

**VM Sizing Calculation:**

1. User configures VM settings in UI (`Components/VM/*.razor`)
2. Input collected in `VMSizingInput` model
3. `VMSizingService.Calculate()` processes input
4. Role specs retrieved via `GetRoleSpecs()`
5. HA multiplier applied via `GetHAMultiplier()`
6. Results returned as `VMSizingResult`
7. UI displays in Results components

**State Management:**
- `AppStateService` - Global application state
- `WizardStateService` - Wizard step management
- CalculatorSettings - Configuration from appsettings.json

## Key Abstractions

**Services:**
- Purpose: Encapsulate business logic
- Examples: `VMSizingService`, `K8sSizingService`, `TechnologyService`
- Pattern: Interface-based DI (`IVMSizingService`)

**Settings:**
- Purpose: Configuration from appsettings.json
- Class: `CalculatorSettings`
- Contains: VMRoles specs, HAPatterns multipliers, LoadBalancers specs

**Role Configuration:**
- Purpose: Server role definitions
- Models: `VMRoleConfig`, `VMRoleResult`, `TechnologyServerRole`
- Pattern: Template-based role generation

## Entry Points

**Application Entry:**
- Location: `src/InfraSizingCalculator/Program.cs`
- Triggers: Application startup
- Responsibilities: DI configuration, middleware setup

**VM API:**
- Location: `src/InfraSizingCalculator/Controllers/Api/VMController.cs`
- Endpoints: `/api/vm/calculate`, `/api/vm/specs/{role}/{size}`
- Responsibilities: VM sizing REST API

## Error Handling

**Strategy:** Exception-based with validation

**Patterns:**
- Input validation before calculation
- ArgumentNullException for null inputs
- Custom validation in services

## Cross-Cutting Concerns

**Logging:**
- Serilog for structured logging

**Validation:**
- Input validation in services
- Model validation attributes

**Configuration:**
- CalculatorSettings loaded from appsettings.json
- VMRoles, HAPatterns, LoadBalancers configuration

---

*Architecture analysis: 2026-01-11*
*Update when major patterns change*
