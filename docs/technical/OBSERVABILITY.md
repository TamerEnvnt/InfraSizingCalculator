# Observability Guide

**Version:** 1.0
**Created:** 2025-12-27
**Status:** Active

---

## Overview

The Infrastructure Sizing Calculator uses OpenTelemetry for observability, providing:

- **Metrics** - Prometheus-compatible metrics at `/metrics`
- **Tracing** - Distributed tracing with console exporter (configurable for OTLP)
- **Logging** - Structured JSON logging with Serilog

---

## Endpoints

| Endpoint | Purpose | Response |
|----------|---------|----------|
| `/health` | Full health check | JSON with all checks |
| `/health/ready` | Readiness probe | For K8s orchestrators |
| `/health/live` | Liveness probe | Basic alive check |
| `/metrics` | Prometheus metrics | Prometheus text format |

---

## Health Checks

### Available Checks

| Check | Tags | Description |
|-------|------|-------------|
| `database` | db, ready | SQLite connectivity and basic queries |
| `distribution-service` | service, ready | Verifies distribution data availability |
| `technology-service` | service, ready | Verifies technology data availability |

### Response Format

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0181902",
  "entries": {
    "database": {
      "data": {
        "database_provider": "SQLite",
        "can_connect": true
      },
      "description": "Database connection successful",
      "duration": "00:00:00.0171316",
      "status": "Healthy",
      "tags": ["db", "ready"]
    }
  }
}
```

### Status Codes

| Status | HTTP Code | Meaning |
|--------|-----------|---------|
| Healthy | 200 | All checks pass |
| Degraded | 200 | Some checks have warnings |
| Unhealthy | 503 | Critical checks failing |

---

## Prometheus Metrics

### ASP.NET Core Metrics (Auto-instrumented)

| Metric | Type | Description |
|--------|------|-------------|
| `http_server_request_duration_seconds` | Histogram | Request duration |
| `http_server_active_requests` | Gauge | Active requests |
| `kestrel_active_connections` | Gauge | Open connections |
| `kestrel_connection_duration_seconds` | Histogram | Connection lifetime |

### Custom Application Metrics

| Metric | Type | Labels | Description |
|--------|------|--------|-------------|
| `infrasizing_calculations_total` | Counter | deployment_type | Total sizing calculations |
| `infrasizing_calculation_duration_ms` | Histogram | deployment_type | Calculation duration |
| `infrasizing_exports_total` | Counter | format | Exports by format |
| `infrasizing_sessions_active` | Gauge | - | Active Blazor sessions |
| `infrasizing_scenario_operations_total` | Counter | operation | Scenario operations |
| `infrasizing_distribution_usage_total` | Counter | distribution | Distribution usage |
| `infrasizing_export_duration_ms` | Histogram | format | Export duration |

### Prometheus Configuration

Add to your `prometheus.yml`:

```yaml
scrape_configs:
  - job_name: 'infrasizing-calculator'
    scrape_interval: 15s
    static_configs:
      - targets: ['localhost:5062']
    metrics_path: /metrics
```

---

## Structured Logging

### Configuration

Logging uses Serilog with:
- Console output (compact JSON)
- File output (`logs/infrasizing-YYYYMMDD.log`)
- 30-day retention
- Enrichers for environment, machine, and thread

### Log Format

```json
{
  "@t": "2025-12-27T18:59:44.5227430Z",
  "@m": "Starting Infrastructure Sizing Calculator",
  "@i": "51c44612",
  "EnvironmentName": "Development",
  "MachineName": "Tamers-MacBook-Pro",
  "ThreadId": 1
}
```

### Log Levels

| Level | Override | Purpose |
|-------|----------|---------|
| Information | Default | Application events |
| Warning | Microsoft.AspNetCore | Framework noise reduction |
| Warning | Microsoft.EntityFrameworkCore | EF noise reduction |

### Log File Location

```
logs/
├── infrasizing-20251227.log
├── infrasizing-20251228.log
└── ...
```

---

## Distributed Tracing

### Configuration

Tracing is enabled with:
- ASP.NET Core instrumentation
- HttpClient instrumentation
- Custom activity source (`InfraSizingCalculator`)

### Development Export

Console exporter is configured for development. For production, uncomment OTLP:

```csharp
.AddOtlpExporter(options =>
{
    options.Endpoint = new Uri("http://jaeger:4317");
})
```

### Creating Custom Spans

```csharp
using var activity = CalculatorMetrics.ActivitySource.StartActivity("CalculateSizing");
activity?.SetTag("deployment_type", "k8s");
activity?.SetTag("app_count", config.TotalApps);

// ... do work ...

activity?.SetStatus(ActivityStatusCode.Ok);
```

---

## Using Custom Metrics

### Recording Calculations

```csharp
public class K8sSizingService : IK8sSizingService
{
    private readonly CalculatorMetrics _metrics;

    public K8sSizingService(CalculatorMetrics metrics)
    {
        _metrics = metrics;
    }

    public K8sSizingResult Calculate(K8sConfig config)
    {
        var sw = Stopwatch.StartNew();

        // ... calculation logic ...

        sw.Stop();
        _metrics.RecordCalculation("k8s", sw.ElapsedMilliseconds);
        _metrics.RecordDistributionUsage(config.Distribution.ToString());

        return result;
    }
}
```

### Recording Exports

```csharp
public class ExportService : IExportService
{
    private readonly CalculatorMetrics _metrics;

    public byte[] ExportToExcel(...)
    {
        var sw = Stopwatch.StartNew();

        // ... export logic ...

        sw.Stop();
        _metrics.RecordExport("xlsx", sw.ElapsedMilliseconds);

        return bytes;
    }
}
```

---

## Kubernetes Integration

### Readiness Probe

```yaml
readinessProbe:
  httpGet:
    path: /health/ready
    port: 80
  initialDelaySeconds: 5
  periodSeconds: 10
```

### Liveness Probe

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 80
  initialDelaySeconds: 15
  periodSeconds: 20
```

---

## Grafana Dashboard

Sample Grafana panels for monitoring:

### Request Rate

```promql
rate(http_server_request_duration_seconds_count{job="infrasizing-calculator"}[5m])
```

### Request Latency (p95)

```promql
histogram_quantile(0.95, rate(http_server_request_duration_seconds_bucket{job="infrasizing-calculator"}[5m]))
```

### Calculation Rate by Type

```promql
rate(infrasizing_calculations_total{job="infrasizing-calculator"}[5m])
```

### Export Rate by Format

```promql
sum by (format) (rate(infrasizing_exports_total{job="infrasizing-calculator"}[5m]))
```

---

## Troubleshooting

### Metrics Not Appearing

1. Verify `/metrics` endpoint returns data
2. Check Prometheus target status
3. Verify meter name matches registration

### Health Checks Failing

1. Check `/health` for detailed errors
2. Verify database connectivity
3. Check service dependencies

### Logs Not Writing

1. Check `logs/` directory exists
2. Verify write permissions
3. Check Serilog configuration

---

## NuGet Packages

```xml
<!-- OpenTelemetry -->
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.10.0-beta.1" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.10.0" />

<!-- Serilog -->
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="3.0.1" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="4.0.0" />
<PackageReference Include="Serilog.Formatting.Compact" Version="3.0.0" />

<!-- Health Checks -->
<PackageReference Include="AspNetCore.HealthChecks.UI.Client" Version="8.0.1" />
<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="9.0.0" />
```

---

## Related Documentation

- [Services Documentation](services.md)
- [API Documentation](api.md)
- [Security Documentation](SECURITY.md) (Phase 2)
