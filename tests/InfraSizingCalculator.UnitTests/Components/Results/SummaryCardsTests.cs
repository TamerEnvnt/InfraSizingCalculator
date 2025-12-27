using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Results;

/// <summary>
/// Tests for SummaryCards component - Summary metric cards for results display
/// </summary>
public class SummaryCardsTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void SummaryCards_RendersContainer()
    {
        // Act
        var cut = RenderComponent<SummaryCards>();

        // Assert
        cut.Find(".summary-cards").Should().NotBeNull();
    }

    [Fact]
    public void SummaryCards_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-class"));

        // Assert
        cut.Find(".summary-cards").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void SummaryCards_NoResults_ShowsEmpty()
    {
        // Act
        var cut = RenderComponent<SummaryCards>();

        // Assert
        cut.FindAll(".summary-card").Should().BeEmpty();
    }

    #endregion

    #region K8s Results Tests

    [Fact]
    public void SummaryCards_K8sResult_ShowsFiveCards()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, result));

        // Assert
        cut.FindAll(".summary-card").Should().HaveCount(5);
    }

    [Fact]
    public void SummaryCards_K8sResult_ShowsClusters()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, result)
            .Add(p => p.ClusterCount, 3));

        // Assert
        var cards = cut.FindAll(".summary-card");
        cards[0].QuerySelector(".summary-label")!.TextContent.Should().Contain("Clusters");
        cards[0].QuerySelector(".summary-value")!.TextContent.Should().Be("3");
    }

    [Fact]
    public void SummaryCards_K8sResult_ShowsTotalNodes()
    {
        // Arrange
        var result = CreateK8sResult(totalNodes: 12);

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, result));

        // Assert
        var cards = cut.FindAll(".summary-card");
        cards[1].QuerySelector(".summary-label")!.TextContent.Should().Contain("Total Nodes");
        cards[1].QuerySelector(".summary-value")!.TextContent.Should().Be("12");
    }

    [Fact]
    public void SummaryCards_K8sResult_ShowsTotalCPU()
    {
        // Arrange
        var result = CreateK8sResult(totalCpu: 96);

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, result));

        // Assert
        var cards = cut.FindAll(".summary-card");
        cards[2].QuerySelector(".summary-label")!.TextContent.Should().Contain("Total CPU");
        cards[2].QuerySelector(".summary-value")!.TextContent.Should().Contain("96");
    }

    [Fact]
    public void SummaryCards_K8sResult_ShowsTotalRAM()
    {
        // Arrange
        var result = CreateK8sResult(totalRam: 384);

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, result));

        // Assert
        var cards = cut.FindAll(".summary-card");
        cards[3].QuerySelector(".summary-label")!.TextContent.Should().Contain("Total RAM");
        cards[3].QuerySelector(".summary-value")!.TextContent.Should().Contain("384");
    }

    [Fact]
    public void SummaryCards_K8sResult_ShowsTotalDisk()
    {
        // Arrange
        var result = CreateK8sResult(totalDisk: 1200);

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, result));

        // Assert
        var cards = cut.FindAll(".summary-card");
        cards[4].QuerySelector(".summary-label")!.TextContent.Should().Contain("Total Disk");
        cards[4].QuerySelector(".summary-value")!.TextContent.Should().Contain("1200");
    }

    [Fact]
    public void SummaryCards_K8sResult_ShowsCustomClusterIcon()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, result)
            .Add(p => p.ClusterIcon, "☸️"));

        // Assert
        cut.Find(".summary-card .summary-icon").TextContent.Should().Be("☸️");
    }

    [Fact]
    public void SummaryCards_K8sResult_ShowsClusterDescription()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, result)
            .Add(p => p.ClusterDescription, "Multi-cluster deployment"));

        // Assert
        cut.Find(".summary-card .summary-desc").TextContent.Should().Be("Multi-cluster deployment");
    }

    #endregion

    #region VM Results Tests

    [Fact]
    public void SummaryCards_VMResult_ShowsFourCards()
    {
        // Arrange
        var result = CreateVMResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.VMResult, result));

        // Assert
        cut.FindAll(".summary-card").Should().HaveCount(4);
    }

    [Fact]
    public void SummaryCards_VMResult_ShowsTotalVMs()
    {
        // Arrange
        var result = CreateVMResult(totalVMs: 8);

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.VMResult, result));

        // Assert
        var cards = cut.FindAll(".summary-card");
        cards[0].QuerySelector(".summary-label")!.TextContent.Should().Contain("Total VMs");
        cards[0].QuerySelector(".summary-value")!.TextContent.Should().Be("8");
    }

    [Fact]
    public void SummaryCards_VMResult_ShowsTotalCPU()
    {
        // Arrange
        var result = CreateVMResult(totalCpu: 64);

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.VMResult, result));

        // Assert
        var cards = cut.FindAll(".summary-card");
        cards[1].QuerySelector(".summary-label")!.TextContent.Should().Contain("Total CPU");
        cards[1].QuerySelector(".summary-value")!.TextContent.Should().Contain("64");
    }

    [Fact]
    public void SummaryCards_VMResult_ShowsTotalRAM()
    {
        // Arrange
        var result = CreateVMResult(totalRam: 256);

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.VMResult, result));

        // Assert
        var cards = cut.FindAll(".summary-card");
        cards[2].QuerySelector(".summary-label")!.TextContent.Should().Contain("Total RAM");
        cards[2].QuerySelector(".summary-value")!.TextContent.Should().Contain("256");
    }

    [Fact]
    public void SummaryCards_VMResult_ShowsTotalDisk()
    {
        // Arrange
        var result = CreateVMResult(totalDisk: 800);

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.VMResult, result));

        // Assert
        var cards = cut.FindAll(".summary-card");
        cards[3].QuerySelector(".summary-label")!.TextContent.Should().Contain("Total Disk");
        cards[3].QuerySelector(".summary-value")!.TextContent.Should().Contain("800");
    }

    #endregion

    #region Icon Tests

    [Fact]
    public void SummaryCards_HasCorrectIcons_ForK8s()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, result)
            .Add(p => p.ClusterIcon, "Multi"));

        // Assert - Component now uses CSS icon classes and text instead of emojis
        var icons = cut.FindAll(".summary-icon");
        icons.Should().HaveCount(5);
        icons[0].TextContent.Should().Be("Multi"); // Clusters - passed via parameter
        icons[1].ClassList.Should().Contain("icon-server"); // Nodes - CSS icon
        icons[2].TextContent.Should().Be("CPU"); // CPU - text
        icons[3].TextContent.Should().Be("RAM"); // RAM - text
        icons[4].TextContent.Should().Be("Disk"); // Disk - text
    }

    [Fact]
    public void SummaryCards_HasCorrectIcons_ForVM()
    {
        // Arrange
        var result = CreateVMResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.VMResult, result));

        // Assert - Component now uses CSS icon classes and text instead of emojis
        var icons = cut.FindAll(".summary-icon");
        icons.Should().HaveCount(4);
        icons[0].ClassList.Should().Contain("icon-server"); // VMs - CSS icon
        icons[1].TextContent.Should().Be("CPU"); // CPU - text
        icons[2].TextContent.Should().Be("RAM"); // RAM - text
        icons[3].TextContent.Should().Be("Disk"); // Disk - text
    }

    #endregion

    #region Parameter Priority Tests

    [Fact]
    public void SummaryCards_K8sResultTakesPrecedence_OverVMResult()
    {
        // Arrange
        var k8sResult = CreateK8sResult();
        var vmResult = CreateVMResult();

        // Act
        var cut = RenderComponent<SummaryCards>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult)
            .Add(p => p.VMResult, vmResult));

        // Assert - Should show K8s format (5 cards including clusters)
        cut.FindAll(".summary-card").Should().HaveCount(5);
        cut.FindAll(".summary-card")[0].TextContent.Should().Contain("Clusters");
    }

    #endregion

    #region Helper Methods

    private static K8sSizingResult CreateK8sResult(
        int totalNodes = 6,
        int totalCpu = 48,
        int totalRam = 192,
        int totalDisk = 600)
    {
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            Configuration = new K8sSizingInput(),
            GrandTotal = new GrandTotal
            {
                TotalNodes = totalNodes,
                TotalCpu = totalCpu,
                TotalRam = totalRam,
                TotalDisk = totalDisk
            }
        };
    }

    private static VMSizingResult CreateVMResult(
        int totalVMs = 4,
        int totalCpu = 32,
        int totalRam = 128,
        int totalDisk = 400)
    {
        return new VMSizingResult
        {
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = totalVMs,
                TotalCpu = totalCpu,
                TotalRam = totalRam,
                TotalDisk = totalDisk
            }
        };
    }

    #endregion
}
