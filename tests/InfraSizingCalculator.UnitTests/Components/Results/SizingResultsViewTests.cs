using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Results;

/// <summary>
/// Tests for SizingResultsView component - Environment sizing results display
/// </summary>
public class SizingResultsViewTests : TestContext
{
    #region Helper Methods

    private static K8sSizingResult CreateK8sResult()
    {
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new()
                {
                    Environment = EnvironmentType.Prod,
                    Masters = 3,
                    Infra = 3,
                    Workers = 6,
                    TotalNodes = 12,
                    TotalCpu = 96,
                    TotalRam = 384,
                    TotalDisk = 1200
                },
                new()
                {
                    Environment = EnvironmentType.Dev,
                    Masters = 3,
                    Infra = 0,
                    Workers = 3,
                    TotalNodes = 6,
                    TotalCpu = 48,
                    TotalRam = 192,
                    TotalDisk = 600
                }
            },
            Configuration = new K8sSizingInput(),
            GrandTotal = new GrandTotal
            {
                TotalNodes = 18,
                TotalCpu = 144,
                TotalRam = 576,
                TotalDisk = 1800
            }
        };
    }

    private static VMSizingResult CreateVMResult()
    {
        return new VMSizingResult
        {
            TechnologyName = "Mendix",
            Environments = new List<VMEnvironmentResult>
            {
                new()
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    HAPattern = HAPattern.ActiveActive,
                    TotalVMs = 6,
                    TotalCpu = 48,
                    TotalRam = 192,
                    TotalDisk = 600,
                    Roles = new List<VMRoleResult>
                    {
                        new() { RoleName = "Web Server", Size = AppTier.Large, TotalInstances = 2, TotalCpu = 16, TotalRam = 64, TotalDisk = 200 },
                        new() { RoleName = "App Server", Size = AppTier.XLarge, TotalInstances = 2, TotalCpu = 24, TotalRam = 96, TotalDisk = 300 }
                    }
                }
            },
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 6,
                TotalCpu = 48,
                TotalRam = 192,
                TotalDisk = 600
            }
        };
    }

    #endregion

    #region Rendering Tests

    [Fact]
    public void SizingResultsView_RendersContainer()
    {
        // Act
        var cut = RenderComponent<SizingResultsView>();

        // Assert
        cut.Find(".sizing-results-view").Should().NotBeNull();
    }

    [Fact]
    public void SizingResultsView_NoResults_ShowsNoResultsMessage()
    {
        // Act
        var cut = RenderComponent<SizingResultsView>();

        // Assert
        cut.Find(".no-results").Should().NotBeNull();
        // Component now uses CSS icon class instead of emoji
        cut.Find(".no-results-icon").ClassList.Should().Contain("icon-chart");
    }

    #endregion

    #region K8s Results Tests

    [Fact]
    public void SizingResultsView_K8sResult_ShowsGrandTotalBar()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.K8sResults, result));

        // Assert
        cut.Find(".grand-total-bar").Should().NotBeNull();
    }

    [Fact]
    public void SizingResultsView_K8sResult_ShowsTotalNodes()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.K8sResults, result));

        // Assert
        var totalItems = cut.FindAll(".total-item");
        totalItems.Should().Contain(t => t.TextContent.Contains("18") && t.TextContent.Contains("Nodes"));
    }

    [Fact]
    public void SizingResultsView_K8sResult_ShowsTotalCPU()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.K8sResults, result));

        // Assert
        var totalItems = cut.FindAll(".total-item");
        totalItems.Should().Contain(t => t.TextContent.Contains("144") && t.TextContent.Contains("vCPU"));
    }

    [Fact]
    public void SizingResultsView_K8sResult_ShowsEnvironmentCards()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.K8sResults, result));

        // Assert
        cut.FindAll(".env-card").Should().HaveCount(2); // Prod and Dev
    }

    [Fact]
    public void SizingResultsView_K8sResult_AutoExpandsProd()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.K8sResults, result));

        // Assert
        var prodCard = cut.FindAll(".env-card").First(c => c.ClassList.Contains("env-prod"));
        prodCard.ClassList.Should().Contain("expanded");
    }

    [Fact]
    public void SizingResultsView_K8sResult_ExpandedShowsMetrics()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.K8sResults, result));

        // Assert
        cut.Find(".env-card-details").Should().NotBeNull();
        cut.Find(".env-metrics-grid").Should().NotBeNull();
    }

    [Fact]
    public async Task SizingResultsView_ClickingHeader_TogglesExpansion()
    {
        // Arrange
        var result = CreateK8sResult();
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.K8sResults, result));

        // Act - Click the Dev card header (not expanded by default)
        var devHeader = cut.FindAll(".env-card-header")[1];
        await cut.InvokeAsync(() => devHeader.Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            var devCard = cut.FindAll(".env-card")[1];
            devCard.ClassList.Should().Contain("expanded");
        });
    }

    [Fact]
    public void SizingResultsView_K8sResult_ShowsClusterModeBanner()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.K8sResults, result)
            .Add(p => p.ClusterModeDescription, "Multi-cluster deployment")
            .Add(p => p.ClusterModeIcon, "🌐"));

        // Assert
        cut.Find(".cluster-mode-banner").Should().NotBeNull();
        cut.Find(".banner-text").TextContent.Should().Contain("Multi-cluster");
    }

    #endregion

    #region VM Results Tests

    [Fact]
    public void SizingResultsView_VMResult_ShowsGrandTotalBar()
    {
        // Arrange
        var result = CreateVMResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.VMResults, result));

        // Assert
        cut.Find(".grand-total-bar.vm-mode").Should().NotBeNull();
    }

    [Fact]
    public void SizingResultsView_VMResult_ShowsTotalVMs()
    {
        // Arrange
        var result = CreateVMResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.VMResults, result));

        // Assert
        var totalItems = cut.FindAll(".total-item");
        totalItems.Should().Contain(t => t.TextContent.Contains("6") && t.TextContent.Contains("VMs"));
    }

    [Fact]
    public void SizingResultsView_VMResult_ShowsTechnologyName()
    {
        // Arrange
        var result = CreateVMResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.VMResults, result));

        // Assert
        cut.Find(".cluster-mode-banner.vm-mode").TextContent.Should().Contain("Mendix");
    }

    [Fact]
    public void SizingResultsView_VMResult_ShowsRolesTable()
    {
        // Arrange
        var result = CreateVMResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.VMResults, result));

        // Assert
        cut.Find(".vm-roles-table").Should().NotBeNull();
        cut.FindAll(".vm-roles-table tbody tr").Should().HaveCount(2); // Two roles
    }

    [Fact]
    public void SizingResultsView_VMResult_ShowsHAPattern()
    {
        // Arrange
        var result = CreateVMResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.VMResults, result));

        // Assert
        cut.Find(".info-badge").TextContent.Should().Contain("Active-Active");
    }

    #endregion

    #region Shared Cluster Mode Tests

    [Fact]
    public void SizingResultsView_SharedClusterMode_HidesMastersAndInfra()
    {
        // Arrange
        var result = CreateK8sResult();

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.K8sResults, result)
            .Add(p => p.IsSharedClusterMode, true));

        // Assert - Masters and Infra metrics should not be shown
        var metricLabels = cut.FindAll(".metric-label");
        metricLabels.Should().NotContain(l => l.TextContent == "Masters");
        metricLabels.Should().NotContain(l => l.TextContent == "Infra");
        metricLabels.Should().Contain(l => l.TextContent == "Workers");
    }

    #endregion

    #region HA Pattern Display Tests

    [Theory]
    [InlineData(HAPattern.None, "None")]
    [InlineData(HAPattern.ActivePassive, "Active-Passive")]
    [InlineData(HAPattern.ActiveActive, "Active-Active")]
    [InlineData(HAPattern.NPlus1, "N+1")]
    [InlineData(HAPattern.NPlus2, "N+2")]
    public void SizingResultsView_DisplaysCorrectHAPattern(HAPattern pattern, string expectedDisplay)
    {
        // Arrange
        var result = CreateVMResult();
        result.Environments[0].HAPattern = pattern;

        // Act
        var cut = RenderComponent<SizingResultsView>(parameters => parameters
            .Add(p => p.VMResults, result));

        // Assert
        cut.Find(".info-badge").TextContent.Should().Contain(expectedDisplay);
    }

    #endregion
}
