# Codebase Concerns

**Analysis Date:** 2026-01-11

## Tech Debt

**None significant identified:**
- Branch focuses on comprehensive test coverage
- VM implementation is well-tested

## Known Bugs

**VM Sizing Bugs (Branch Focus):**
- Issue: Various calculation edge cases being fixed
- Files: `src/InfraSizingCalculator/Services/VMSizingService.cs`
- Status: Active fixes with test coverage

## Security Considerations

**Input Validation:**
- Risk: Sizing inputs need validation
- Current mitigation: ArgumentNullException, validation logic
- Recommendations: Continue comprehensive input validation

## Performance Bottlenecks

**No significant issues identified:**
- Calculations are synchronous and fast
- No database bottlenecks

## Fragile Areas

**CalculatorSettings Configuration:**
- Files: `appsettings.json`, `Models/CalculatorSettings.cs`
- Why fragile: Role specs, multipliers are configuration-driven
- Safe modification: Always add tests for new configurations
- Test coverage: Good coverage via parameterized tests

## Scaling Limits

**SQLite:**
- Current capacity: Single-user/small team
- Scaling path: PostgreSQL for multi-user

## Dependencies at Risk

**None identified:**
- All dependencies actively maintained

## Missing Critical Features

**Technology Templates:**
- Problem: Need complete role templates per technology
- Current workaround: Generic role configs
- Status: Being implemented in this branch

## Test Coverage Gaps

**Branch Focus - Expanding Coverage:**
- VMSizingService: 30+ tests (good)
- VMController: 18+ tests (good)
- Components: Some coverage (expanding)

**Areas Being Added:**
- DR pattern calculations
- Custom override handling
- Edge cases

---

*Concerns audit: 2026-01-11*
*Update as issues are fixed*
