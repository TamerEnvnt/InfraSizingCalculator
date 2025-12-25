using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for SummaryCards component
/// </summary>
public class SummaryCardsTests : TestContext
{
    [Fact]
    public void SummaryCards_RendersK8sCards_WhenK8sResultProvided()
    {
        // Arrange
        var k8sResult = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult));

        // Assert
        cut.FindAll(".summary-card").Should().HaveCount(5);
    }

    [Fact]
    public void SummaryCards_RendersVMCards_WhenVMResultProvided()
    {
        // Arrange
        var vmResult = CreateVMResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.VMResult, vmResult));

        // Assert
        cut.FindAll(".summary-card").Should().HaveCount(4);
    }

    [Fact]
    public void SummaryCards_DisplaysTotalNodes_ForK8s()
    {
        // Arrange
        var k8sResult = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult));

        // Assert
        var values = cut.FindAll(".summary-value");
        values.Should().Contain(v => v.TextContent.Contains("15")); // TotalNodes
    }

    [Fact]
    public void SummaryCards_DisplaysTotalCpu_ForK8s()
    {
        // Arrange
        var k8sResult = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult));

        // Assert
        var values = cut.FindAll(".summary-value");
        values.Should().Contain(v => v.TextContent.Contains("120") && v.TextContent.Contains("cores"));
    }

    [Fact]
    public void SummaryCards_DisplaysTotalRam_ForK8s()
    {
        // Arrange
        var k8sResult = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult));

        // Assert
        var values = cut.FindAll(".summary-value");
        values.Should().Contain(v => v.TextContent.Contains("256") && v.TextContent.Contains("GB"));
    }

    [Fact]
    public void SummaryCards_DisplaysTotalDisk_ForK8s()
    {
        // Arrange
        var k8sResult = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult));

        // Assert
        var values = cut.FindAll(".summary-value");
        values.Should().Contain(v => v.TextContent.Contains("500") && v.TextContent.Contains("GB"));
    }

    [Fact]
    public void SummaryCards_DisplaysClusterCount()
    {
        // Arrange
        var k8sResult = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult)
            .Add(p => p.ClusterCount, 3));

        // Assert
        var values = cut.FindAll(".summary-value");
        values[0].TextContent.Should().Be("3");
    }

    [Fact]
    public void SummaryCards_DisplaysClusterIcon()
    {
        // Arrange
        var k8sResult = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult)
            .Add(p => p.ClusterIcon, "🔷"));

        // Assert
        cut.Find(".summary-icon").TextContent.Should().Be("🔷");
    }

    [Fact]
    public void SummaryCards_DisplaysClusterDescription()
    {
        // Arrange
        var k8sResult = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult)
            .Add(p => p.ClusterDescription, "Production Clusters"));

        // Assert
        cut.Find(".summary-desc").TextContent.Should().Be("Production Clusters");
    }

    [Fact]
    public void SummaryCards_DisplaysVMTotals()
    {
        // Arrange
        var vmResult = CreateVMResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.VMResult, vmResult));

        // Assert
        var values = cut.FindAll(".summary-value");
        values.Should().Contain(v => v.TextContent.Contains("10")); // TotalVMs
    }

    [Fact]
    public void SummaryCards_AppliesAdditionalClass()
    {
        // Arrange
        var k8sResult = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult)
            .Add(p => p.AdditionalClass, "custom-summary"));

        // Assert
        cut.Find(".summary-cards").ClassList.Should().Contain("custom-summary");
    }

    [Fact]
    public void SummaryCards_RendersEmpty_WhenNoResultProvided()
    {
        // Act
        var cut = RenderComponent<SummaryCards>();

        // Assert
        cut.FindAll(".summary-card").Should().BeEmpty();
    }

    [Fact]
    public void SummaryCards_PrefersK8sResult_WhenBothProvided()
    {
        // Arrange
        var k8sResult = CreateK8sResult();
        var vmResult = CreateVMResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult)
            .Add(p => p.VMResult, vmResult));

        // Assert - K8s has 5 cards, VM has 4
        cut.FindAll(".summary-card").Should().HaveCount(5);
    }

    private static K8sSizingResult CreateK8sResult()
    {
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new()
                {
                    EnvironmentName = "Production",
                    IsProd = true,
                    TotalNodes = 10,
                    TotalCpu = 80,
                    TotalRam = 160,
                    TotalDisk = 300
                },
                new()
                {
                    EnvironmentName = "Staging",
                    IsProd = false,
                    TotalNodes = 5,
                    TotalCpu = 40,
                    TotalRam = 96,
                    TotalDisk = 200
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 15,
                TotalCpu = 120,
                TotalRam = 256,
                TotalDisk = 500
            },
            Configuration = new K8sSizingInput
            {
                Technology = Models.Enums.Technology.Java,
                Distribution = Models.Enums.Distribution.OpenShift,
                ClusterMode = Models.Enums.ClusterMode.MultiCluster,
                ProdApps = new AppConfig(),
                NonProdApps = new AppConfig()
            }
        };
    }

    private static VMSizingResult CreateVMResult()
    {
        return new VMSizingResult
        {
            Environments = new List<VMEnvironmentResult>(),
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 10,
                TotalCpu = 80,
                TotalRam = 128,
                TotalDisk = 400
            }
        };
    }
}
