using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.HealthChecks;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services.HealthChecks;

public class DistributionServiceHealthCheckTests
{
    private readonly IDistributionService _distributionService;
    private readonly DistributionServiceHealthCheck _healthCheck;

    public DistributionServiceHealthCheckTests()
    {
        _distributionService = Substitute.For<IDistributionService>();
        _healthCheck = new DistributionServiceHealthCheck(_distributionService);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenNoDistributions_ReturnsUnhealthy()
    {
        // Arrange
        _distributionService.GetAll().Returns(Enumerable.Empty<DistributionConfig>());

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("No distributions available", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenFewDistributions_ReturnsDegraded()
    {
        // Arrange - only 10 distributions when expecting at least 30
        var distributions = Enumerable.Range(1, 10)
            .Select(i => CreateDistributionConfig(Distribution.Kubernetes, $"K8s-{i}"))
            .ToList();
        _distributionService.GetAll().Returns(distributions);
        _distributionService.GetConfig(Distribution.Kubernetes).Returns(distributions[0]);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.Contains("Only 10 distributions available", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenEnoughDistributions_ReturnsHealthy()
    {
        // Arrange - 35 distributions (enough to pass)
        var distributions = Enumerable.Range(1, 35)
            .Select(i => CreateDistributionConfig(Distribution.Kubernetes, $"K8s-{i}"))
            .ToList();
        _distributionService.GetAll().Returns(distributions);
        _distributionService.GetConfig(Distribution.Kubernetes).Returns(distributions[0]);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("35 distributions available", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenConfigRetrievalFails_ReturnsDegraded()
    {
        // Arrange
        var distributions = Enumerable.Range(1, 35)
            .Select(i => CreateDistributionConfig(Distribution.Kubernetes, $"K8s-{i}"))
            .ToList();
        _distributionService.GetAll().Returns(distributions);
        _distributionService.GetConfig(Distribution.Kubernetes).Returns((DistributionConfig?)null);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.Contains("configuration retrieval failed", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenExceptionThrown_ReturnsUnhealthy()
    {
        // Arrange
        _distributionService.GetAll().Returns(_ => throw new Exception("Service error"));

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("check failed", result.Description);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsDataWithDistributionCount()
    {
        // Arrange
        var distributions = Enumerable.Range(1, 35)
            .Select(i => CreateDistributionConfig(Distribution.Kubernetes, $"K8s-{i}"))
            .ToList();
        _distributionService.GetAll().Returns(distributions);
        _distributionService.GetConfig(Distribution.Kubernetes).Returns(distributions[0]);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("distribution_count"));
        Assert.Equal(35, result.Data["distribution_count"]);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenHealthy_IncludesSampleCheckPassed()
    {
        // Arrange
        var distributions = Enumerable.Range(1, 35)
            .Select(i => CreateDistributionConfig(Distribution.Kubernetes, $"K8s-{i}"))
            .ToList();
        _distributionService.GetAll().Returns(distributions);
        _distributionService.GetConfig(Distribution.Kubernetes).Returns(distributions[0]);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("sample_check_passed"));
        Assert.Equal(true, result.Data["sample_check_passed"]);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenUnhealthy_IncludesZeroCount()
    {
        // Arrange
        _distributionService.GetAll().Returns(Enumerable.Empty<DistributionConfig>());

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("distribution_count"));
        Assert.Equal(0, result.Data["distribution_count"]);
    }

    private static DistributionConfig CreateDistributionConfig(Distribution distribution, string name)
    {
        var defaultSpecs = new NodeSpecs(4, 8, 100);
        return new DistributionConfig
        {
            Distribution = distribution,
            Name = name,
            Vendor = "CNCF",
            ProdControlPlane = defaultSpecs,
            NonProdControlPlane = defaultSpecs,
            ProdWorker = defaultSpecs,
            NonProdWorker = defaultSpecs
        };
    }
}
