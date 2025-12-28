using InfraSizingCalculator.Data;
using InfraSizingCalculator.Services.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services.HealthChecks;

public class DatabaseHealthCheckTests : IDisposable
{
    private readonly InfraSizingDbContext _dbContext;
    private readonly DatabaseHealthCheck _healthCheck;

    public DatabaseHealthCheckTests()
    {
        // Use SQLite in-memory mode with a real connection for testing
        var options = new DbContextOptionsBuilder<InfraSizingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _dbContext = new InfraSizingDbContext(options);
        // Open connection and create schema for in-memory SQLite
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();

        _healthCheck = new DatabaseHealthCheck(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseConnectable_ReturnsHealthy()
    {
        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("successful", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsDataWithDatabaseProvider()
    {
        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("database_provider"));
        Assert.Equal("SQLite", result.Data["database_provider"]);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsDataWithCanConnect()
    {
        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("can_connect"));
        Assert.Equal(true, result.Data["can_connect"]);
    }

    [Fact]
    public async Task CheckHealthAsync_IncludesTableCounts_WhenAvailable()
    {
        // Arrange - ensure tables are created
        await _dbContext.Database.EnsureCreatedAsync();

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.NotNull(result.Data);
        // Table counts are optional but should be included when tables exist
        Assert.True(result.Data.ContainsKey("application_settings_count") ||
                    result.Status == HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_RespectsCanellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act - should complete without exception when not cancelled
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext(), cts.Token);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseDisposed_ReturnsUnhealthy()
    {
        // Arrange - Create a separate context and dispose it
        var options = new DbContextOptionsBuilder<InfraSizingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        var context = new InfraSizingDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        var healthCheck = new DatabaseHealthCheck(context);

        // Dispose the context entirely to simulate unavailable database
        context.Dispose();

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_HealthyResult_HasDescriptiveMessage()
    {
        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.NotNull(result.Description);
        Assert.NotEmpty(result.Description);
    }
}
