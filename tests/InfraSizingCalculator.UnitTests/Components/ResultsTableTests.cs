using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for ResultsTable component
/// </summary>
public class ResultsTableTests : TestContext
{
    [Fact]
    public void ResultsTable_RendersEnvironmentRows()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal()));

        // Assert
        cut.FindAll("tbody tr").Should().HaveCount(3);
    }

    [Fact]
    public void ResultsTable_DisplaysEnvironmentNames()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal()));

        // Assert
        var badges = cut.FindAll(".env-badge");
        badges[0].TextContent.Trim().Should().Be("Production");
        badges[1].TextContent.Trim().Should().Be("Staging");
        badges[2].TextContent.Trim().Should().Be("Development");
    }

    [Fact]
    public void ResultsTable_AppliesProdClass_ForProductionEnv()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal()));

        // Assert
        var rows = cut.FindAll("tbody tr");
        rows[0].ClassList.Should().Contain("prod-row");
    }

    [Fact]
    public void ResultsTable_AppliesNonProdClass_ForNonProdEnv()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal()));

        // Assert
        var rows = cut.FindAll("tbody tr");
        rows[1].ClassList.Should().Contain("nonprod-row");
        rows[2].ClassList.Should().Contain("nonprod-row");
    }

    [Fact]
    public void ResultsTable_ShowsClusterColumn_WhenShowClusterInfoIsTrue()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal())
            .Add(p => p.ShowClusterInfo, true));

        // Assert
        cut.FindAll("th").Should().Contain(th => th.TextContent == "Cluster");
    }

    [Fact]
    public void ResultsTable_HidesClusterColumn_WhenShowClusterInfoIsFalse()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal())
            .Add(p => p.ShowClusterInfo, false));

        // Assert
        cut.FindAll("th").Should().NotContain(th => th.TextContent == "Cluster");
    }

    [Fact]
    public void ResultsTable_ShowsMasterColumn_WhenShowMastersIsTrue()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal())
            .Add(p => p.ShowMasters, true));

        // Assert
        cut.FindAll("th").Should().Contain(th => th.TextContent == "Masters");
    }

    [Fact]
    public void ResultsTable_HidesMasterColumn_WhenShowMastersIsFalse()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal())
            .Add(p => p.ShowMasters, false));

        // Assert
        cut.FindAll("th").Should().NotContain(th => th.TextContent == "Masters");
    }

    [Fact]
    public void ResultsTable_ShowsInfraColumn_WhenShowInfraIsTrue()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal())
            .Add(p => p.ShowInfra, true));

        // Assert
        cut.FindAll("th").Should().Contain(th => th.TextContent == "Infra");
    }

    [Fact]
    public void ResultsTable_HidesInfraColumn_WhenShowInfraIsFalse()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal())
            .Add(p => p.ShowInfra, false));

        // Assert
        cut.FindAll("th").Should().NotContain(th => th.TextContent == "Infra");
    }

    [Fact]
    public void ResultsTable_DisplaysGrandTotalRow()
    {
        // Arrange
        var environments = CreateEnvironments();
        var grandTotal = CreateGrandTotal();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, grandTotal));

        // Assert
        var footer = cut.Find("tfoot tr");
        footer.TextContent.Should().Contain("GRAND TOTAL");
        footer.TextContent.Should().Contain("15"); // TotalNodes
    }

    [Fact]
    public void ResultsTable_DisplaysSharedClusterName_WhenIsSharedCluster()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal())
            .Add(p => p.ShowClusterInfo, true)
            .Add(p => p.IsSharedCluster, true));

        // Assert
        cut.Markup.Should().Contain("shared-cluster");
    }

    [Fact]
    public void ResultsTable_DisplaysEnvClusterName_WhenNotSharedCluster()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal())
            .Add(p => p.ShowClusterInfo, true)
            .Add(p => p.IsSharedCluster, false));

        // Assert
        cut.Markup.Should().Contain("production-cluster");
    }

    [Fact]
    public void ResultsTable_AppliesAdditionalClass()
    {
        // Arrange
        var environments = CreateEnvironments();

        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, environments)
            .Add(p => p.GrandTotal, CreateGrandTotal())
            .Add(p => p.AdditionalClass, "custom-table"));

        // Assert
        cut.Find(".results-table-container").ClassList.Should().Contain("custom-table");
    }

    [Fact]
    public void ResultsTable_RendersEmpty_WhenNoEnvironments()
    {
        // Act
        var cut = RenderComponent<ResultsTable>(parameters => parameters
            .Add(p => p.Environments, new List<EnvironmentResult>())
            .Add(p => p.GrandTotal, new GrandTotal()));

        // Assert
        cut.FindAll("tbody tr").Should().BeEmpty();
    }

    private static List<EnvironmentResult> CreateEnvironments()
    {
        return new List<EnvironmentResult>
        {
            new()
            {
                EnvironmentName = "Production",
                IsProd = true,
                Apps = 50,
                Pods = 100,
                Masters = 3,
                Infra = 5,
                Workers = 7,
                TotalNodes = 15,
                TotalCpu = 120,
                TotalRam = 256,
                TotalDisk = 500
            },
            new()
            {
                EnvironmentName = "Staging",
                IsProd = false,
                Apps = 30,
                Pods = 60,
                Masters = 3,
                Infra = 3,
                Workers = 4,
                TotalNodes = 10,
                TotalCpu = 80,
                TotalRam = 160,
                TotalDisk = 300
            },
            new()
            {
                EnvironmentName = "Development",
                IsProd = false,
                Apps = 20,
                Pods = 40,
                Masters = 3,
                Infra = 3,
                Workers = 3,
                TotalNodes = 9,
                TotalCpu = 72,
                TotalRam = 144,
                TotalDisk = 270
            }
        };
    }

    private static GrandTotal CreateGrandTotal()
    {
        return new GrandTotal
        {
            TotalNodes = 15,
            TotalCpu = 120,
            TotalRam = 256,
            TotalDisk = 500
        };
    }
}
