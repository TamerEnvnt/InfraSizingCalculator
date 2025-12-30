using InfraSizingCalculator.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public SecurityHeadersMiddlewareTests()
    {
        _environment = Substitute.For<IWebHostEnvironment>();
        _configuration = CreateConfiguration();
    }

    [Fact]
    public async Task InvokeAsync_AddsCspHeader()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Content-Security-Policy"));
    }

    [Fact]
    public async Task InvokeAsync_AddsXContentTypeOptionsHeader()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Content-Type-Options"));
        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
    }

    [Fact]
    public async Task InvokeAsync_AddsXFrameOptionsHeader()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Frame-Options"));
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
    }

    [Fact]
    public async Task InvokeAsync_AddsXssProtectionHeader()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-XSS-Protection"));
        Assert.Equal("1; mode=block", context.Response.Headers["X-XSS-Protection"]);
    }

    [Fact]
    public async Task InvokeAsync_AddsReferrerPolicyHeader()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"]);
    }

    [Fact]
    public async Task InvokeAsync_AddsPermissionsPolicyHeader()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Permissions-Policy"));
        var policy = context.Response.Headers["Permissions-Policy"].ToString();
        Assert.Contains("camera=()", policy);
        Assert.Contains("microphone=()", policy);
        Assert.Contains("geolocation=()", policy);
    }

    [Fact]
    public async Task InvokeAsync_AddsCacheControlHeader_WhenNotSet()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Cache-Control"));
        Assert.Equal("no-store, no-cache, must-revalidate", context.Response.Headers["Cache-Control"]);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotOverrideCacheControl_WhenAlreadySet()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        context.Response.Headers["Cache-Control"] = "max-age=3600";
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("max-age=3600", context.Response.Headers["Cache-Control"]);
    }

    [Fact]
    public async Task InvokeAsync_AddsCrossOriginOpenerPolicyHeader_ForHttps()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpsContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Cross-Origin-Opener-Policy"));
        Assert.Equal("same-origin", context.Response.Headers["Cross-Origin-Opener-Policy"]);
    }

    [Fact]
    public async Task InvokeAsync_AddsCrossOriginResourcePolicyHeader_ForHttps()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpsContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Cross-Origin-Resource-Policy"));
        Assert.Equal("same-origin", context.Response.Headers["Cross-Origin-Resource-Policy"]);
    }

    [Fact]
    public async Task InvokeAsync_AddsCrossOriginHeaders_ForLocalhost()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateLocalhostContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Cross-Origin-Opener-Policy"));
        Assert.True(context.Response.Headers.ContainsKey("Cross-Origin-Resource-Policy"));
    }

    [Fact]
    public async Task InvokeAsync_DoesNotAddCrossOriginHeaders_ForPlainHttp()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext(); // plain HTTP, not localhost
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Response.Headers.ContainsKey("Cross-Origin-Opener-Policy"));
        Assert.False(context.Response.Headers.ContainsKey("Cross-Origin-Resource-Policy"));
    }

    [Fact]
    public async Task InvokeAsync_Production_CspIncludesUpgradeInsecureRequests_WhenConfigured()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var config = CreateConfiguration(enableUpgradeInsecure: true);
        var middleware = CreateMiddleware(_environment, config);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        Assert.Contains("upgrade-insecure-requests", csp);
    }

    [Fact]
    public async Task InvokeAsync_Production_CspDoesNotIncludeUpgradeInsecureRequests_WhenNotConfigured()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var config = CreateConfiguration(enableUpgradeInsecure: false);
        var middleware = CreateMiddleware(_environment, config);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        Assert.DoesNotContain("upgrade-insecure-requests", csp);
    }

    [Fact]
    public async Task InvokeAsync_Development_CspDoesNotIncludeUpgradeInsecureRequests()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Development");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        Assert.DoesNotContain("upgrade-insecure-requests", csp);
    }

    [Fact]
    public async Task InvokeAsync_Development_CspIncludesLocalhost()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Development");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        Assert.Contains("localhost", csp);
    }

    [Fact]
    public async Task InvokeAsync_CspIncludesBlazorRequirements()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        // Blazor requires unsafe-inline and unsafe-eval
        Assert.Contains("'unsafe-inline'", csp);
        Assert.Contains("'unsafe-eval'", csp);
    }

    [Fact]
    public async Task InvokeAsync_CspIncludesWebSocketSupport()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_environment, _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        // SignalR requires WebSocket support
        Assert.Contains("wss:", csp);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var nextCalled = false;
        var middleware = new SecurityHeadersMiddleware(
            ctx => { nextCalled = true; return Task.CompletedTask; },
            _environment,
            _configuration);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        // Set a non-localhost host to test plain HTTP behavior
        context.Request.Host = new HostString("example.com", 8080);
        context.Request.Scheme = "http";
        return context;
    }

    private static HttpContext CreateHttpsContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com", 443);
        return context;
    }

    private static HttpContext CreateLocalhostContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost", 8080);
        return context;
    }

    private static IConfiguration CreateConfiguration(bool enableUpgradeInsecure = false)
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Security:EnableUpgradeInsecureRequests", enableUpgradeInsecure.ToString().ToLowerInvariant() }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private static SecurityHeadersMiddleware CreateMiddleware(IWebHostEnvironment environment, IConfiguration configuration)
    {
        return new SecurityHeadersMiddleware(
            ctx => Task.CompletedTask,
            environment,
            configuration);
    }
}
