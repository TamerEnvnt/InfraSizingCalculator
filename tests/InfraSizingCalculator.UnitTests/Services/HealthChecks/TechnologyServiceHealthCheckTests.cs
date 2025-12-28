using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.HealthChecks;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services.HealthChecks;

public class TechnologyServiceHealthCheckTests
{
    private readonly ITechnologyService _technologyService;
    private readonly TechnologyServiceHealthCheck _healthCheck;

    public TechnologyServiceHealthCheckTests()
    {
        _technologyService = Substitute.For<ITechnologyService>();
        _healthCheck = new TechnologyServiceHealthCheck(_technologyService);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenNoTechnologies_ReturnsUnhealthy()
    {
        // Arrange
        _technologyService.GetAll().Returns(Enumerable.Empty<TechnologyConfig>());

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("No technologies available", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenFewTechnologies_ReturnsDegraded()
    {
        // Arrange - only 3 technologies when expecting at least 5
        var technologies = new List<TechnologyConfig>
        {
            CreateTechnologyConfig(Technology.DotNet, ".NET"),
            CreateTechnologyConfig(Technology.Java, "Java"),
            CreateTechnologyConfig(Technology.NodeJs, "Node.js")
        };
        _technologyService.GetAll().Returns(technologies);
        _technologyService.GetConfig(Technology.DotNet).Returns(technologies[0]);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.Contains("Only 3 technologies available", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenEnoughTechnologies_ReturnsHealthy()
    {
        // Arrange - 7 technologies (enough to pass)
        var technologies = new List<TechnologyConfig>
        {
            CreateTechnologyConfig(Technology.DotNet, ".NET"),
            CreateTechnologyConfig(Technology.Java, "Java"),
            CreateTechnologyConfig(Technology.NodeJs, "Node.js"),
            CreateTechnologyConfig(Technology.Python, "Python"),
            CreateTechnologyConfig(Technology.Go, "Go"),
            CreateTechnologyConfig(Technology.Mendix, "Mendix"),
            CreateTechnologyConfig(Technology.OutSystems, "OutSystems")
        };
        _technologyService.GetAll().Returns(technologies);
        _technologyService.GetConfig(Technology.DotNet).Returns(technologies[0]);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("7 technologies available", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenConfigRetrievalFails_ReturnsDegraded()
    {
        // Arrange
        var technologies = new List<TechnologyConfig>
        {
            CreateTechnologyConfig(Technology.DotNet, ".NET"),
            CreateTechnologyConfig(Technology.Java, "Java"),
            CreateTechnologyConfig(Technology.NodeJs, "Node.js"),
            CreateTechnologyConfig(Technology.Python, "Python"),
            CreateTechnologyConfig(Technology.Go, "Go")
        };
        _technologyService.GetAll().Returns(technologies);
        _technologyService.GetConfig(Technology.DotNet).Returns((TechnologyConfig?)null);

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
        _technologyService.GetAll().Returns(_ => throw new Exception("Service error"));

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("check failed", result.Description);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsDataWithTechnologyCount()
    {
        // Arrange
        var technologies = new List<TechnologyConfig>
        {
            CreateTechnologyConfig(Technology.DotNet, ".NET"),
            CreateTechnologyConfig(Technology.Java, "Java"),
            CreateTechnologyConfig(Technology.NodeJs, "Node.js"),
            CreateTechnologyConfig(Technology.Python, "Python"),
            CreateTechnologyConfig(Technology.Go, "Go")
        };
        _technologyService.GetAll().Returns(technologies);
        _technologyService.GetConfig(Technology.DotNet).Returns(technologies[0]);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("technology_count"));
        Assert.Equal(5, result.Data["technology_count"]);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenHealthy_IncludesSampleCheckPassed()
    {
        // Arrange
        var technologies = new List<TechnologyConfig>
        {
            CreateTechnologyConfig(Technology.DotNet, ".NET"),
            CreateTechnologyConfig(Technology.Java, "Java"),
            CreateTechnologyConfig(Technology.NodeJs, "Node.js"),
            CreateTechnologyConfig(Technology.Python, "Python"),
            CreateTechnologyConfig(Technology.Go, "Go")
        };
        _technologyService.GetAll().Returns(technologies);
        _technologyService.GetConfig(Technology.DotNet).Returns(technologies[0]);

        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("sample_check_passed"));
        Assert.Equal(true, result.Data["sample_check_passed"]);
    }

    private static TechnologyConfig CreateTechnologyConfig(Technology technology, string name)
    {
        return new TechnologyConfig
        {
            Technology = technology,
            Name = name,
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new TierSpecs(0.5, 1),
                [AppTier.Medium] = new TierSpecs(1, 2),
                [AppTier.Large] = new TierSpecs(2, 4)
            }
        };
    }
}
