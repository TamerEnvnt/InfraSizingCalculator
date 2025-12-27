using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Layout;
using Microsoft.AspNetCore.Components;
using Xunit;
using static InfraSizingCalculator.Components.Layout.RightStatsSidebar;

namespace InfraSizingCalculator.UnitTests.Components.Layout;

/// <summary>
/// Tests for RightStatsSidebar component - Summary statistics and actions
/// </summary>
public class RightStatsSidebarTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void RightStatsSidebar_RendersAsideElement()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>();

        // Assert
        cut.Find("aside.right-stats").Should().NotBeNull();
    }

    [Fact]
    public void RightStatsSidebar_RendersSummaryTitle()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>();

        // Assert
        cut.Find(".stats-title").TextContent.Should().Contain("Summary");
    }

    [Fact]
    public void RightStatsSidebar_RendersQuickActions()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>();

        // Assert
        cut.Find(".quick-actions").Should().NotBeNull();
    }

    #endregion

    #region Empty State Tests

    [Fact]
    public void RightStatsSidebar_NoResults_ShowsEmptyState()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, false));

        // Assert
        cut.Find(".stats-empty").Should().NotBeNull();
        cut.Find(".empty-text").TextContent.Should().Contain("Configure your sizing");
    }

    [Fact]
    public void RightStatsSidebar_NoResults_ShowsEmptyIcon()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, false));

        // Assert - Component now uses CSS icon class instead of .empty-icon
        cut.Find(".stats-empty .icon-chart").Should().NotBeNull();
    }

    [Fact]
    public void RightStatsSidebar_NoResults_StatsCardsHidden()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, false));

        // Assert
        cut.FindAll(".stats-card").Should().BeEmpty();
    }

    #endregion

    #region Stats Cards Tests

    [Fact]
    public void RightStatsSidebar_WithResults_ShowsStatsCards()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.TotalNodes, 6)
            .Add(p => p.TotalCPU, 48)
            .Add(p => p.TotalRAM, 192)
            .Add(p => p.TotalDisk, 600));

        // Assert
        cut.FindAll(".stats-card").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void RightStatsSidebar_ShowsTotalNodes()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.TotalNodes, 12));

        // Assert
        var nodesCard = cut.FindAll(".stats-card").First(c => c.QuerySelector(".stats-label")?.TextContent.Contains("Total Nodes") == true);
        nodesCard.QuerySelector(".stats-value")!.TextContent.Should().Be("12");
    }

    [Fact]
    public void RightStatsSidebar_ShowsTotalCPU()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.TotalCPU, 96));

        // Assert
        var cpuCard = cut.FindAll(".stats-card").First(c => c.QuerySelector(".stats-label")?.TextContent.Contains("vCPU") == true);
        cpuCard.QuerySelector(".stats-value")!.TextContent.Should().Be("96");
    }

    [Fact]
    public void RightStatsSidebar_ShowsTotalRAM()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.TotalRAM, 384));

        // Assert
        var ramCard = cut.FindAll(".stats-card").First(c => c.QuerySelector(".stats-label")?.TextContent.Contains("RAM") == true);
        ramCard.QuerySelector(".stats-value")!.TextContent.Should().Contain("384");
        ramCard.QuerySelector(".stats-value")!.TextContent.Should().Contain("GB");
    }

    [Fact]
    public void RightStatsSidebar_ShowsTotalDisk()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.TotalDisk, 1200));

        // Assert
        var diskCard = cut.FindAll(".stats-card").First(c => c.QuerySelector(".stats-label")?.TextContent.Contains("Disk") == true);
        diskCard.QuerySelector(".stats-value").Should().NotBeNull();
    }

    [Fact]
    public void RightStatsSidebar_ShowsDelta_WhenNotZero()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.TotalNodes, 12)
            .Add(p => p.TotalNodesDelta, 3));

        // Assert
        var nodesCard = cut.FindAll(".stats-card").First(c => c.QuerySelector(".stats-label")?.TextContent.Contains("Total Nodes") == true);
        nodesCard.QuerySelector(".stats-delta")!.TextContent.Should().Contain("from baseline");
    }

    [Fact]
    public void RightStatsSidebar_HidesDelta_WhenZero()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.TotalNodes, 12)
            .Add(p => p.TotalNodesDelta, 0));

        // Assert
        var nodesCard = cut.FindAll(".stats-card").First(c => c.QuerySelector(".stats-label")?.TextContent.Contains("Total Nodes") == true);
        nodesCard.QuerySelector(".stats-delta").Should().BeNull();
    }

    #endregion

    #region Cost Card Tests

    [Fact]
    public void RightStatsSidebar_ShowsMonthlyEstimate()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.MonthlyEstimate, 15000m));

        // Assert
        var costCard = cut.Find(".stats-card.highlight");
        costCard.QuerySelector(".stats-label")!.TextContent.Should().Contain("Monthly Cost");
    }

    [Fact]
    public void RightStatsSidebar_ShowsCostProvider()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.MonthlyEstimate, 15000m)
            .Add(p => p.CostProvider, "Azure"));

        // Assert
        var costCard = cut.Find(".stats-card.highlight");
        costCard.QuerySelector(".stats-delta")!.TextContent.Should().Contain("Azure pricing");
    }

    [Fact]
    public void RightStatsSidebar_ShowsPricingNotAvailable()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.ShowPricingNotAvailable, true));

        // Assert
        var mutedCard = cut.Find(".stats-card.muted");
        mutedCard.QuerySelector(".stats-value")!.TextContent.Should().Contain("N/A");
        mutedCard.QuerySelector(".stats-delta")!.TextContent.Should().Contain("not configured");
    }

    #endregion

    #region Warnings Badge Tests

    [Fact]
    public void RightStatsSidebar_ShowsWarningsBadge()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.WarningsCount, 3));

        // Assert
        cut.Find(".stats-warnings").Should().NotBeNull();
        cut.Find(".warning-badge.warning").TextContent.Should().Contain("3 Warnings");
    }

    [Fact]
    public void RightStatsSidebar_ShowsCriticalBadge()
    {
        // Note: The warnings section is shown when WarningsCount > 0 OR RecommendationsCount > 0
        // CriticalCount badge appears inside that section
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.CriticalCount, 2)
            .Add(p => p.WarningsCount, 1)); // Need at least one warning for section to show

        // Assert
        cut.Find(".warning-badge.critical").TextContent.Should().Contain("2 Critical");
    }

    [Fact]
    public void RightStatsSidebar_ShowsRecommendationsBadge()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.RecommendationsCount, 5));

        // Assert
        cut.Find(".warning-badge.info").TextContent.Should().Contain("5 Tips");
    }

    [Fact]
    public void RightStatsSidebar_HidesWarningsSection_WhenNoWarnings()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.WarningsCount, 0)
            .Add(p => p.RecommendationsCount, 0));

        // Assert
        cut.FindAll(".stats-warnings").Should().BeEmpty();
    }

    #endregion

    #region Quick Actions Tests

    [Fact]
    public void RightStatsSidebar_HasExportButton()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>();

        // Assert
        cut.Find(".btn-primary").TextContent.Should().Contain("Export");
    }

    [Fact]
    public void RightStatsSidebar_HasShareButton()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>();

        // Assert
        var buttons = cut.FindAll(".btn-secondary");
        buttons.Should().Contain(b => b.TextContent.Contains("Share"));
    }

    [Fact]
    public void RightStatsSidebar_HasSaveButton()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>();

        // Assert
        var buttons = cut.FindAll(".btn-secondary");
        buttons.Should().Contain(b => b.TextContent.Contains("Save"));
    }

    [Fact]
    public void RightStatsSidebar_ButtonsDisabled_WhenNoResults()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, false));

        // Assert
        cut.Find(".btn-primary").GetAttribute("disabled").Should().NotBeNull();
    }

    [Fact]
    public void RightStatsSidebar_ButtonsEnabled_WhenHasResults()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true));

        // Assert
        cut.Find(".btn-primary").GetAttribute("disabled").Should().BeNull();
    }

    #endregion

    #region Export Menu Tests

    [Fact]
    public void RightStatsSidebar_ExportMenu_HiddenByDefault()
    {
        // Act
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true));

        // Assert
        cut.FindAll(".export-menu").Should().BeEmpty();
    }

    [Fact]
    public void RightStatsSidebar_ClickingExport_ShowsMenu()
    {
        // Arrange
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true));

        // Act
        cut.Find(".btn-primary").Click();

        // Assert
        cut.Find(".export-menu").Should().NotBeNull();
    }

    [Fact]
    public void RightStatsSidebar_ExportMenu_HasAllFormats()
    {
        // Arrange
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true));
        cut.Find(".btn-primary").Click();

        // Assert
        var menuButtons = cut.FindAll(".export-menu button");
        menuButtons.Should().HaveCount(5); // CSV, JSON, Excel, PDF, Diagram
    }

    [Fact]
    public async Task RightStatsSidebar_ClickingExportFormat_InvokesCallback()
    {
        // Arrange
        ExportFormat? exportedFormat = null;
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.OnExport, EventCallback.Factory.Create<ExportFormat>(this, f => exportedFormat = f)));

        // Act
        cut.Find(".btn-primary").Click(); // Open menu
        var csvButton = cut.FindAll(".export-menu button")[0];
        await cut.InvokeAsync(() => csvButton.Click());

        // Assert
        exportedFormat.Should().Be(ExportFormat.CSV);
    }

    [Fact]
    public async Task RightStatsSidebar_ExportingFormat_ClosesMenu()
    {
        // Arrange
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true));

        // Act
        cut.Find(".btn-primary").Click(); // Open menu
        cut.FindAll(".export-menu button")[0].Click(); // Click CSV

        // Assert
        cut.FindAll(".export-menu").Should().BeEmpty();
    }

    #endregion

    #region Callback Tests

    [Fact]
    public async Task RightStatsSidebar_ClickingShare_InvokesCallback()
    {
        // Arrange
        bool shareClicked = false;
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.OnShare, EventCallback.Factory.Create(this, () => shareClicked = true)));

        // Act
        var shareBtn = cut.FindAll(".btn-secondary").First(b => b.TextContent.Contains("Share"));
        await cut.InvokeAsync(() => shareBtn.Click());

        // Assert
        shareClicked.Should().BeTrue();
    }

    [Fact]
    public async Task RightStatsSidebar_ClickingSave_InvokesCallback()
    {
        // Arrange
        bool saveClicked = false;
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.OnSave, EventCallback.Factory.Create(this, () => saveClicked = true)));

        // Act
        var saveBtn = cut.FindAll(".btn-secondary").First(b => b.TextContent.Contains("Save"));
        await cut.InvokeAsync(() => saveBtn.Click());

        // Assert
        saveClicked.Should().BeTrue();
    }

    [Fact]
    public async Task RightStatsSidebar_NoCallback_DoesNotThrow()
    {
        // Arrange
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true));

        // Act & Assert
        var action = async () =>
        {
            var shareBtn = cut.FindAll(".btn-secondary").First(b => b.TextContent.Contains("Share"));
            await cut.InvokeAsync(() => shareBtn.Click());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void RightStatsSidebar_UpdatesStats_WhenParametersChange()
    {
        // Arrange
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.TotalNodes, 6));

        // Assert initial
        var nodesCard = cut.FindAll(".stats-card").First(c => c.QuerySelector(".stats-label")?.TextContent.Contains("Total Nodes") == true);
        nodesCard.QuerySelector(".stats-value")!.TextContent.Should().Be("6");

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.TotalNodes, 12));

        // Assert updated
        nodesCard = cut.FindAll(".stats-card").First(c => c.QuerySelector(".stats-label")?.TextContent.Contains("Total Nodes") == true);
        nodesCard.QuerySelector(".stats-value")!.TextContent.Should().Be("12");
    }

    [Fact]
    public void RightStatsSidebar_TransitionsToResults_ShowsStats()
    {
        // Arrange
        var cut = RenderComponent<RightStatsSidebar>(parameters => parameters
            .Add(p => p.HasResults, false));

        // Assert initial
        cut.Find(".stats-empty").Should().NotBeNull();

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.HasResults, true)
            .Add(p => p.TotalNodes, 6));

        // Assert
        cut.FindAll(".stats-empty").Should().BeEmpty();
        cut.FindAll(".stats-card").Should().NotBeEmpty();
    }

    #endregion
}
