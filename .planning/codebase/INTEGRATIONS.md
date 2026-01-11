# External Integrations

**Analysis Date:** 2026-01-11

## APIs & External Services

**Payment Processing:**
- Not integrated

**External APIs:**
- None (self-contained calculator)

## Data Storage

**Databases:**
- SQLite - Primary data store
  - Connection: `infrasizing.db`
  - Client: Entity Framework Core
  - Purpose: Scenarios, settings

- SQLite - Identity store
  - Connection: `identity.db`
  - Purpose: User authentication

**File Storage:**
- Local file system for exports
  - Excel via ClosedXML
  - PDF via iText

**Caching:**
- In-memory caching (IMemoryCache)

## Authentication & Identity

**Auth Provider:**
- ASP.NET Core Identity
  - Cookie-based sessions
  - Local user accounts

## Monitoring & Observability

**Logging:**
- Serilog - Structured logging

**Metrics:**
- OpenTelemetry
- `CalculatorMetrics` service

## CI/CD & Deployment

**Hosting:**
- Self-hosted or containerized
- Docker support

**Testing:**
- GitHub Actions (if configured)
- xUnit, bUnit, Playwright

## Environment Configuration

**Development:**
- SQLite databases (embedded)
- No external dependencies

**Production:**
- Same configuration
- Environment variables for secrets

## Configuration (CalculatorSettings)

**VMRoles:**
- Role specifications per AppTier
- HighMemoryMultiplier: 1.5

**HAPatterns:**
- Multipliers for HA patterns

**LoadBalancers:**
- Specs per LoadBalancerOption

---

*Integration audit: 2026-01-11*
*Update when adding/removing services*
