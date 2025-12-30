# Security Guide

**Version:** 1.0
**Created:** 2025-12-27
**Status:** Active

---

## Overview

The Infrastructure Sizing Calculator implements security measures following OWASP recommendations:

- **Security Headers** - OWASP-recommended HTTP headers
- **Rate Limiting** - Protection against abuse and DoS
- **Input Validation** - Centralized validation service
- **CORS** - Controlled cross-origin access

---

## Security Headers

All responses include security headers added by `SecurityHeadersMiddleware`:

| Header | Value | Purpose |
|--------|-------|---------|
| Content-Security-Policy | See below | Controls resource loading |
| X-Content-Type-Options | nosniff | Prevents MIME sniffing |
| X-Frame-Options | DENY | Prevents clickjacking |
| X-XSS-Protection | 1; mode=block | XSS filter (legacy browsers) |
| Referrer-Policy | strict-origin-when-cross-origin | Controls referrer info |
| Permissions-Policy | Restrictive | Disables unnecessary APIs |
| Cache-Control | no-store, no-cache, must-revalidate | Prevents caching |
| Cross-Origin-Opener-Policy | same-origin | Isolates browsing context |
| Cross-Origin-Resource-Policy | same-origin | Restricts resource access |

### Content Security Policy

**Development:**
```
default-src 'self';
script-src 'self' 'unsafe-inline' 'unsafe-eval';
style-src 'self' 'unsafe-inline';
img-src 'self' data: blob:;
font-src 'self';
connect-src 'self' ws: wss: http://localhost:* https://localhost:*;
frame-ancestors 'none';
base-uri 'self';
form-action 'self';
```

**Production:**
- Adds `upgrade-insecure-requests`
- Restricts `connect-src` to `wss:` only

### Verification

```bash
curl -I http://localhost:5062 | grep -iE "content-security|x-frame|x-content-type"
```

---

## Rate Limiting

Rate limiting protects against abuse:

### Global Limits

| Limit | Value | Description |
|-------|-------|-------------|
| Requests/IP | 100/minute | Global limit |
| Queue Size | 10 | Requests queued when limit hit |

### API Limits

| Limit | Value | Description |
|-------|-------|-------------|
| API Requests/IP | 50/minute | Stricter for API endpoints |
| Queue Size | 5 | Smaller queue for API |

### Rate Limit Response

When rate limited, clients receive:
- HTTP Status: 429 Too Many Requests
- Body: "Too many requests. Please try again later."

### Configuration

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));
});
```

---

## Input Validation

The `InputValidationService` provides centralized validation:

### Validation Methods

| Method | Purpose | Business Rule |
|--------|---------|---------------|
| `ValidateAppCounts` | Validates app count inputs | BR-V001, BR-V002 |
| `ValidateNodeSpecs` | Validates node specifications | BR-V003 |
| `ValidatePricing` | Validates pricing inputs | BR-V004 |
| `ValidateGrowthRate` | Validates growth percentages | BR-V005 |
| `SanitizeScenarioName` | Sanitizes scenario names | BR-V008 |
| `SanitizeText` | General text sanitization | - |

### Validation Limits

| Input | Min | Max | Rule |
|-------|-----|-----|------|
| Small Apps | 0 | 1,000 | BR-V001 |
| Medium Apps | 0 | 500 | BR-V001 |
| Large Apps | 0 | 100 | BR-V001 |
| CPU Cores | 1 | 256 | BR-V003 |
| Memory (GB) | 1 | 1,024 | BR-V003 |
| Storage (GB) | 1 | 10,000 | BR-V003 |
| Growth Rate (%) | 0 | 500 | BR-V005 |
| Scenario Name | - | 100 chars | BR-V008 |

### Usage Example

```csharp
public class MyService
{
    private readonly IInputValidationService _validation;

    public MyService(IInputValidationService validation)
    {
        _validation = validation;
    }

    public void Process(int small, int medium, int large)
    {
        var result = _validation.ValidateAppCounts(small, medium, large);
        if (!result.IsValid)
        {
            throw new ValidationException(string.Join(", ", result.Errors));
        }
        // Continue processing...
    }
}
```

### Sanitization

Text inputs are sanitized to prevent XSS:
- HTML tags removed
- Dangerous characters (`<`, `>`, `"`, `'`, `&`, `;`, `\`) removed
- Whitespace normalized
- Length limited

---

## CORS Configuration

### Development

```csharp
options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

### Production

```csharp
options.AddPolicy("Production", policy =>
{
    policy.WithOrigins(/* configured origins */)
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials();
});
```

Configure allowed origins in `appsettings.Production.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://your-domain.com"
    ]
  }
}
```

---

## Anti-Forgery Protection

Blazor's anti-forgery protection is enabled:

```csharp
app.UseAntiforgery();
```

This protects against CSRF attacks in forms.

---

## Secrets Management

### Development

Secrets can be stored in `appsettings.Development.json` (not committed).

### Production

Use environment variables or secret management:

```csharp
if (builder.Environment.IsProduction())
{
    // Azure Key Vault
    builder.Configuration.AddAzureKeyVault(...);

    // Or environment variables
    builder.Configuration.AddEnvironmentVariables("INFRASIZING_");
}
```

---

## Security Audit Checklist

### Headers
- [ ] Content-Security-Policy present
- [ ] X-Content-Type-Options: nosniff
- [ ] X-Frame-Options: DENY
- [ ] Referrer-Policy set
- [ ] Cache-Control for sensitive pages

### Rate Limiting
- [ ] Global rate limit configured
- [ ] API rate limit configured
- [ ] Rejection logging enabled

### Input Validation
- [ ] All user inputs validated
- [ ] Text inputs sanitized
- [ ] Validation errors logged

### CORS
- [ ] Development policy configured
- [ ] Production policy restricts origins
- [ ] Credentials properly handled

---

## Security Scanning

### OWASP ZAP Scan

```bash
# Start the application
dotnet run --project src/InfraSizingCalculator &

# Run ZAP baseline scan
docker run -t zaproxy/zap-stable zap-baseline.py \
  -t http://host.docker.internal:5062
```

### Header Verification

```bash
curl -I http://localhost:5062 | head -30
```

---

## Known Considerations

### Blazor Server Requirements

Due to Blazor Server architecture, some CSP relaxations are required:
- `'unsafe-inline'` for scripts (Blazor runtime)
- `'unsafe-eval'` for scripts (Blazor runtime)
- `ws:` / `wss:` for SignalR WebSocket

### HSTS

HSTS is enabled in production only:
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
```

---

## Related Documentation

- [Observability Guide](OBSERVABILITY.md)
- [Business Rules](../business/business-rules.md)
- [API Documentation](api.md)
