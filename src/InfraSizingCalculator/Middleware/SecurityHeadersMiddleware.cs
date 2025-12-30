namespace InfraSizingCalculator.Middleware;

/// <summary>
/// Adds security headers per OWASP recommendations.
/// Applied early in the pipeline to all responses.
/// Configurable via appsettings Security section.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _next = next;
        _environment = environment;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if upgrade-insecure-requests should be enabled (from config)
        var enableUpgradeInsecure = _configuration.GetValue<bool>("Security:EnableUpgradeInsecureRequests", false);

        // Content Security Policy
        // Note: Blazor Server requires 'unsafe-inline' and 'unsafe-eval' for scripts
        // SignalR requires ws: and wss: for WebSocket connections
        var cspValue = _environment.IsDevelopment()
            ? BuildDevelopmentCsp()
            : BuildProductionCsp(enableUpgradeInsecure);

        context.Response.Headers.Append("Content-Security-Policy", cspValue);

        // Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Clickjacking protection
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // XSS protection (legacy browsers)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Referrer policy - send referrer only to same-origin, or strict origin for cross-origin
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions policy - disable unnecessary browser features
        context.Response.Headers.Append("Permissions-Policy",
            "accelerometer=(), " +
            "camera=(), " +
            "geolocation=(), " +
            "gyroscope=(), " +
            "magnetometer=(), " +
            "microphone=(), " +
            "payment=(), " +
            "usb=()");

        // Cache control for sensitive pages (no caching by default)
        if (!context.Response.Headers.ContainsKey("Cache-Control"))
        {
            context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate");
        }

        // Cross-Origin policies - only add for HTTPS or localhost
        var isSecureContext = context.Request.IsHttps ||
                              context.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);

        if (isSecureContext)
        {
            context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");
            context.Response.Headers.Append("Cross-Origin-Resource-Policy", "same-origin");
        }

        await _next(context);
    }

    private static string BuildDevelopmentCsp()
    {
        return string.Join(" ",
            "default-src 'self';",
            "script-src 'self' 'unsafe-inline' 'unsafe-eval';",  // Required for Blazor
            "style-src 'self' 'unsafe-inline';",
            "img-src 'self' data: blob:;",
            "font-src 'self';",
            "connect-src 'self' ws: wss: http://localhost:* https://localhost:*;",  // SignalR + dev servers
            "frame-ancestors 'none';",
            "base-uri 'self';",
            "form-action 'self';");
    }

    private static string BuildProductionCsp(bool enableUpgradeInsecure)
    {
        var csp = string.Join(" ",
            "default-src 'self';",
            "script-src 'self' 'unsafe-inline' 'unsafe-eval';",  // Required for Blazor
            "style-src 'self' 'unsafe-inline';",
            "img-src 'self' data: blob:;",
            "font-src 'self';",
            "connect-src 'self' ws: wss:;",  // SignalR WebSocket
            "frame-ancestors 'none';",
            "base-uri 'self';",
            "form-action 'self';");

        // Only add upgrade-insecure-requests if HTTPS is configured
        if (enableUpgradeInsecure)
        {
            csp += " upgrade-insecure-requests;";
        }

        return csp;
    }
}

/// <summary>
/// Extension methods for SecurityHeadersMiddleware.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds the security headers middleware to the application pipeline.
    /// Should be called early in the pipeline, after exception handling.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
