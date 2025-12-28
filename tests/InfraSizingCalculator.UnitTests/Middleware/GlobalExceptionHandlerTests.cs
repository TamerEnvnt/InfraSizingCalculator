using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using InfraSizingCalculator.Middleware;
using InfraSizingCalculator.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerTests()
    {
        _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        _environment = Substitute.For<IHostEnvironment>();
        _environment.EnvironmentName.Returns("Production");
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNextAndReturnsNormally()
    {
        // Arrange
        var context = CreateHttpContext();
        var nextCalled = false;
        var handler = CreateHandler(ctx => { nextCalled = true; return Task.CompletedTask; });

        // Act
        await handler.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new ValidationException("Validation failed"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ReturnsJsonContentType()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new ValidationException("Validation failed"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new ArgumentException("Invalid argument", "paramName"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_Returns404()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new KeyNotFoundException("Not found"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        Assert.Equal(404, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns401()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new UnauthorizedAccessException());

        // Act
        await handler.InvokeAsync(context);

        // Assert
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_NotSupportedException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new NotSupportedException("Operation not supported"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_InvalidOperationWithBusinessRule_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new InvalidOperationException("BR-001: Business rule violated"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_InvalidOperationWithoutBusinessRule_Returns500()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new InvalidOperationException("Some internal error"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        Assert.Equal(500, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new Exception("Unexpected error"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        Assert.Equal(500, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_DoesNotExposeDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new Exception("Secret internal details"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.DoesNotContain("Secret internal details", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_Exception_LogsError()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new Exception("Test error"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_Exception_IncludesTraceId()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new Exception("Test error"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(response);
        Assert.False(string.IsNullOrEmpty(response.TraceId));
    }

    [Fact]
    public async Task InvokeAsync_Exception_IncludesPath()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/test";
        var handler = CreateHandler(_ => throw new Exception("Test error"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(response);
        Assert.Equal("/api/test", response.Path);
    }

    [Fact]
    public async Task InvokeAsync_SanitizesFilePaths()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new ArgumentException(
            "Error at C:\\Users\\secret\\project\\file.cs", "param"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.DoesNotContain("C:\\Users\\secret\\project\\file.cs", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_SanitizesConnectionStrings()
    {
        // Arrange
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new ArgumentException(
            "Connection failed: Server=myserver;Password=secret123", "connection"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.DoesNotContain("secret123", responseBody);
        Assert.DoesNotContain("Password=secret123", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ReturnsValidationDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var validationResult = new ValidationResult("Email is required", new[] { "Email" });
        var handler = CreateHandler(_ => throw new ValidationException(validationResult, null, null));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Contains("Email", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_Development_WritesIndentedJson()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Development");
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new Exception("Test error"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        // Indented JSON has newlines
        Assert.Contains("\n", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_Production_WritesCompactJson()
    {
        // Arrange
        _environment.EnvironmentName.Returns("Production");
        var context = CreateHttpContext();
        var handler = CreateHandler(_ => throw new Exception("Test error"));

        // Act
        await handler.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        // Compact JSON doesn't have newlines in simple objects (may have them in arrays though)
        var lines = responseBody.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        // Should be minimal lines (1 or 2 if there's a trailing newline)
        Assert.True(lines.Length <= 2);
    }

    private HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private GlobalExceptionHandler CreateHandler(RequestDelegate next)
    {
        return new GlobalExceptionHandler(next, _logger, _environment);
    }
}
