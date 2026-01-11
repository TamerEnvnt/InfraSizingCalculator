# Project: VM Implementation - InfraSizingCalculator

**Created:** 2026-01-11
**Type:** Brownfield (existing codebase with enhancements)

## Vision

Complete the VM sizing implementation with comprehensive test coverage, fixing calculation edge cases and ensuring all business rules are properly validated.

## Problem Statement

The Infrastructure Sizing Calculator needs robust VM sizing capabilities alongside its K8s sizing features. This branch focuses on completing the VM implementation with:
- Accurate role-based specifications
- Proper HA/DR pattern calculations
- Technology-specific memory multipliers
- Comprehensive test coverage for all calculation scenarios

## Requirements

### Validated

- VM sizing calculations via VMSizingService — existing
- Role specifications (Web/App, Database, Cache, Bastion) — existing
- AppTier sizing (Small, Medium, Large, XLarge) — existing
- HA multiplier patterns (None, ActiveActive, ActivePassive, NPlus1, NPlus2) — existing
- Technology memory multipliers (Java/Mendix/OutSystems: 1.5x) — existing
- REST API endpoints via VMController — existing
- Blazor UI components for VM configuration — existing
- Unit test framework (xUnit, bUnit, FluentAssertions) — existing
- 30+ VMSizingService tests — existing
- 18+ VMController tests — existing

### Active

- [ ] DR pattern calculations - complete implementation
- [ ] Custom override handling - proper validation
- [ ] Edge case fixes - calculation accuracy
- [ ] Technology templates - complete role templates per technology
- [ ] Component test coverage - expand bUnit tests

### Out of Scope

- K8s sizing changes — separate functionality, not in scope
- New technology additions — focus on existing 7 technologies
- Database migration — SQLite sufficient for current needs

## Constraints

**Technical:**
- Must maintain backwards compatibility with existing API
- Tests must pass before merging
- Follow existing architecture patterns (layered, service-oriented)

**Quality:**
- All new code requires test coverage
- Business rules must be documented with BR-* IDs
- Follow existing code conventions (see CONVENTIONS.md)

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Test-first approach | Branch focuses on comprehensive coverage | Active |
| Configuration-driven specs | VMRoles in CalculatorSettings | Existing |
| Technology multipliers | Java/Mendix/OutSystems need more memory | Existing (1.5x) |

## Success Metrics

- All VMSizingService tests passing
- All VMController tests passing
- DR pattern calculations verified
- Custom override scenarios covered
- No regression in existing functionality

---

*Last updated: 2026-01-11 after initialization*
