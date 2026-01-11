# Roadmap: VM Implementation

## Overview

Complete VM sizing implementation with comprehensive test coverage. This roadmap takes the existing VMSizingService foundation and systematically addresses edge cases, adds DR pattern support, completes technology templates, and expands test coverage across all VM-related components.

## Domain Expertise

None

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

- [ ] **Phase 1: Foundation Review** - Validate existing tests, understand current state
- [ ] **Phase 2: Role Specs Edge Cases** - Fix CPU/RAM calculation edge cases per role/tier
- [ ] **Phase 3: HA Multiplier Validation** - Ensure all HA patterns calculate correctly
- [ ] **Phase 4: Technology Memory Multipliers** - Complete 1.5x multiplier implementation
- [ ] **Phase 5: DR Pattern Implementation** - Full DR pattern calculation support
- [ ] **Phase 6: Custom Override Handling** - Validation and processing of user overrides
- [ ] **Phase 7: Technology Templates** - Complete role templates per technology
- [ ] **Phase 8: VMController API Coverage** - Expand controller test coverage
- [ ] **Phase 9: Component Test Coverage** - bUnit tests for VM components
- [ ] **Phase 10: Integration & Polish** - End-to-end validation, cleanup

## Phase Details

### Phase 1: Foundation Review
**Goal**: Verify all existing tests pass, document current test coverage gaps
**Depends on**: Nothing (first phase)
**Research**: Unlikely (understanding existing code)
**Plans**: TBD

Plans:
- [ ] 01-01: Run test suite, document failures
- [ ] 01-02: Inventory existing test coverage

### Phase 2: Role Specs Edge Cases
**Goal**: Fix CPU/RAM calculation edge cases for all role/tier combinations
**Depends on**: Phase 1
**Research**: Unlikely (internal calculations)
**Plans**: TBD

Plans:
- [ ] 02-01: Web/App tier edge cases
- [ ] 02-02: Database tier edge cases
- [ ] 02-03: Cache/Bastion tier edge cases

### Phase 3: HA Multiplier Validation
**Goal**: Verify all HA patterns (None, ActiveActive, ActivePassive, NPlus1, NPlus2) calculate correctly
**Depends on**: Phase 2
**Research**: Unlikely (existing patterns)
**Plans**: TBD

Plans:
- [ ] 03-01: Validate HA multiplier calculations
- [ ] 03-02: Add missing HA test scenarios

### Phase 4: Technology Memory Multipliers
**Goal**: Complete 1.5x memory multiplier for Java/Mendix/OutSystems
**Depends on**: Phase 3
**Research**: Unlikely (configuration-driven)
**Plans**: TBD

Plans:
- [ ] 04-01: Verify technology multiplier logic
- [ ] 04-02: Add technology-specific test coverage

### Phase 5: DR Pattern Implementation
**Goal**: Full DR pattern calculation support with proper test coverage
**Depends on**: Phase 4
**Research**: Likely (new capability, architecture decision)
**Research topics**: DR pattern types, calculation strategies, industry standards
**Plans**: TBD

Plans:
- [ ] 05-01: Design DR pattern model
- [ ] 05-02: Implement DR calculations
- [ ] 05-03: Add DR test coverage

### Phase 6: Custom Override Handling
**Goal**: Proper validation and processing of user custom overrides
**Depends on**: Phase 5
**Research**: Unlikely (internal validation)
**Plans**: TBD

Plans:
- [ ] 06-01: Implement override validation
- [ ] 06-02: Add override test scenarios

### Phase 7: Technology Templates
**Goal**: Complete role templates per technology (7 technologies)
**Depends on**: Phase 6
**Research**: Unlikely (configuration extension)
**Plans**: TBD

Plans:
- [ ] 07-01: Define technology-specific role templates
- [ ] 07-02: Implement template loading
- [ ] 07-03: Add template test coverage

### Phase 8: VMController API Coverage
**Goal**: Expand VMController test coverage to match VMSizingService
**Depends on**: Phase 7
**Research**: Unlikely (existing patterns)
**Plans**: TBD

Plans:
- [ ] 08-01: Add missing endpoint tests
- [ ] 08-02: Add error handling tests

### Phase 9: Component Test Coverage
**Goal**: bUnit tests for VM Blazor components
**Depends on**: Phase 8
**Research**: Unlikely (bUnit patterns exist)
**Plans**: TBD

Plans:
- [ ] 09-01: VMHADRConfig component tests
- [ ] 09-02: VM input component tests
- [ ] 09-03: VM result component tests

### Phase 10: Integration & Polish
**Goal**: End-to-end validation, documentation sync, cleanup
**Depends on**: Phase 9
**Research**: Unlikely (validation only)
**Plans**: TBD

Plans:
- [ ] 10-01: E2E test validation
- [ ] 10-02: Documentation sync
- [ ] 10-03: Final cleanup

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 9 → 10

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation Review | 0/2 | Not started | - |
| 2. Role Specs Edge Cases | 0/3 | Not started | - |
| 3. HA Multiplier Validation | 0/2 | Not started | - |
| 4. Technology Memory Multipliers | 0/2 | Not started | - |
| 5. DR Pattern Implementation | 0/3 | Not started | - |
| 6. Custom Override Handling | 0/2 | Not started | - |
| 7. Technology Templates | 0/3 | Not started | - |
| 8. VMController API Coverage | 0/2 | Not started | - |
| 9. Component Test Coverage | 0/3 | Not started | - |
| 10. Integration & Polish | 0/3 | Not started | - |
