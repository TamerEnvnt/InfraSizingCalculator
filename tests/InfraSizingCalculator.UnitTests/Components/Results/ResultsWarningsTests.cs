using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Results;

/// <summary>
/// Tests for ResultsWarnings component - Validation warnings display
/// </summary>
public class ResultsWarningsTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void ResultsWarnings_RendersContainer()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>();

        // Assert
        cut.Find(".results-warnings").Should().NotBeNull();
    }

    [Fact]
    public void ResultsWarnings_NoWarnings_ShowsEmpty()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>();

        // Assert
        cut.FindAll(".warnings-summary-bar").Should().BeEmpty();
        cut.FindAll(".severity-tabs").Should().BeEmpty();
    }

    [Fact]
    public void ResultsWarnings_EmptyList_ShowsEmpty()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, new List<ValidationWarning>()));

        // Assert
        cut.FindAll(".warnings-summary-bar").Should().BeEmpty();
    }

    #endregion

    #region Summary Bar Tests

    [Fact]
    public void ResultsWarnings_WithWarnings_ShowsSummaryBar()
    {
        // Arrange
        var warnings = CreateMixedWarnings();

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".warnings-summary-bar").Should().NotBeNull();
    }

    [Fact]
    public void ResultsWarnings_ShowsCriticalBadge()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Critical, "Critical issue")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".severity-badge.critical").TextContent.Should().Contain("1 Critical");
    }

    [Fact]
    public void ResultsWarnings_ShowsWarningBadge()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Warning, "Warning issue"),
            CreateWarning("2", WarningSeverity.Warning, "Another warning")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".severity-badge.warning").TextContent.Should().Contain("2 Warnings");
    }

    [Fact]
    public void ResultsWarnings_ShowsInfoBadge()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Info, "Tip")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".severity-badge.info").TextContent.Should().Contain("1 Tips");
    }

    [Fact]
    public void ResultsWarnings_ShowsSuccessBadge()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Success, "OK")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".severity-badge.success").TextContent.Should().Contain("1 OK");
    }

    #endregion

    #region Severity Tabs Tests

    [Fact]
    public void ResultsWarnings_ShowsSeverityTabs()
    {
        // Arrange
        var warnings = CreateMixedWarnings();

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".severity-tabs").Should().NotBeNull();
    }

    [Fact]
    public void ResultsWarnings_AllTabActiveByDefault()
    {
        // Arrange
        var warnings = CreateMixedWarnings();

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        var tabs = cut.FindAll(".severity-tab");
        tabs[0].ClassList.Should().Contain("active");
        tabs[0].TextContent.Should().Contain("All");
    }

    [Fact]
    public void ResultsWarnings_ShowsCriticalTab_WhenCriticalExists()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Critical, "Critical")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        var tabs = cut.FindAll(".severity-tab");
        tabs.Should().Contain(t => t.TextContent.Contains("Critical"));
    }

    [Fact]
    public void ResultsWarnings_ClickingTab_FiltersWarnings()
    {
        // Arrange
        var warnings = CreateMixedWarnings();
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Act - Click critical tab
        var criticalTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("Critical"));
        criticalTab.Click();

        // Assert - Re-query after click since component re-renders
        cut.WaitForAssertion(() =>
        {
            var activeTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("Critical"));
            activeTab.ClassList.Should().Contain("active");
        });
    }

    #endregion

    #region Warnings List Tests

    [Fact]
    public void ResultsWarnings_ShowsWarningCards()
    {
        // Arrange
        var warnings = CreateMixedWarnings();

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.FindAll(".warning-compact-card").Should().NotBeEmpty();
    }

    [Fact]
    public void ResultsWarnings_LimitsVisibleWarnings()
    {
        // Arrange - Create 5 warnings, should show 3 by default
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Warning, "Warning 1"),
            CreateWarning("2", WarningSeverity.Warning, "Warning 2"),
            CreateWarning("3", WarningSeverity.Warning, "Warning 3"),
            CreateWarning("4", WarningSeverity.Warning, "Warning 4"),
            CreateWarning("5", WarningSeverity.Warning, "Warning 5")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.FindAll(".warning-compact-card").Should().HaveCount(3);
    }

    [Fact]
    public void ResultsWarnings_ShowsShowMoreButton_WhenMoreExist()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Warning, "Warning 1"),
            CreateWarning("2", WarningSeverity.Warning, "Warning 2"),
            CreateWarning("3", WarningSeverity.Warning, "Warning 3"),
            CreateWarning("4", WarningSeverity.Warning, "Warning 4")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".show-more-btn").TextContent.Should().Contain("1 more");
    }

    [Fact]
    public void ResultsWarnings_ClickingShowMore_ShowsMoreWarnings()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Warning, "Warning 1"),
            CreateWarning("2", WarningSeverity.Warning, "Warning 2"),
            CreateWarning("3", WarningSeverity.Warning, "Warning 3"),
            CreateWarning("4", WarningSeverity.Warning, "Warning 4"),
            CreateWarning("5", WarningSeverity.Warning, "Warning 5")
        };
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Act
        cut.Find(".show-more-btn").Click();

        // Assert
        cut.FindAll(".warning-compact-card").Should().HaveCount(5);
    }

    [Fact]
    public void ResultsWarnings_ShowsWarningTitle()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Warning, "Test Title", "Test Message")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".warning-title").TextContent.Should().Contain("Test Title");
    }

    [Fact]
    public void ResultsWarnings_ShowsWarningIcon()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            new ValidationWarning
            {
                Id = "1",
                Severity = WarningSeverity.Warning,
                Title = "Test",
                Message = "Test",
                Icon = "⚠️"
            }
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".warning-icon").TextContent.Should().Be("⚠️");
    }

    #endregion

    #region Expand/Collapse Tests

    [Fact]
    public void ResultsWarnings_WarningsCollapsed_ByDefault()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Warning, "Test", "Message", "Suggestion")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.FindAll(".warning-compact-body").Should().BeEmpty();
    }

    [Fact]
    public void ResultsWarnings_ClickingWarning_Expands()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Warning, "Test", "Full Message", "Try this")
        };
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Act
        cut.Find(".warning-compact-header").Click();

        // Assert
        cut.Find(".warning-compact-body").Should().NotBeNull();
        cut.Find(".warning-message").TextContent.Should().Contain("Full Message");
    }

    [Fact]
    public void ResultsWarnings_ExpandedWarning_ShowsSuggestion()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Warning, "Test", "Message", "Try this suggestion")
        };
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Act
        cut.Find(".warning-compact-header").Click();

        // Assert
        cut.Find(".warning-suggestion").TextContent.Should().Contain("Try this suggestion");
    }

    [Fact]
    public void ResultsWarnings_ClickingExpanded_Collapses()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Warning, "Test", "Message")
        };
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Act - Expand then collapse
        cut.Find(".warning-compact-header").Click();
        cut.Find(".warning-compact-body").Should().NotBeNull();
        cut.Find(".warning-compact-header").Click();

        // Assert
        cut.FindAll(".warning-compact-body").Should().BeEmpty();
    }

    #endregion

    #region Severity Styling Tests

    [Fact]
    public void ResultsWarnings_CriticalWarning_HasCriticalClass()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Critical, "Critical")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".warning-compact-card").ClassList.Should().Contain("severity-critical");
    }

    [Fact]
    public void ResultsWarnings_WarningLevel_HasWarningClass()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Warning, "Warning")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".warning-compact-card").ClassList.Should().Contain("severity-warning");
    }

    [Fact]
    public void ResultsWarnings_InfoLevel_HasInfoClass()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Info, "Info")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.Find(".warning-compact-card").ClassList.Should().Contain("severity-info");
    }

    #endregion

    #region Helper Methods

    private static ValidationWarning CreateWarning(
        string id,
        WarningSeverity severity,
        string title,
        string? message = null,
        string? suggestion = null)
    {
        return new ValidationWarning
        {
            Id = id,
            Severity = severity,
            Title = title,
            Message = message ?? title,
            Suggestion = suggestion,
            Icon = severity switch
            {
                WarningSeverity.Critical => "🚨",
                WarningSeverity.Warning => "⚠️",
                WarningSeverity.Info => "💡",
                WarningSeverity.Success => "✅",
                _ => "ℹ️"
            }
        };
    }

    private static List<ValidationWarning> CreateMixedWarnings()
    {
        return new List<ValidationWarning>
        {
            CreateWarning("1", WarningSeverity.Critical, "Critical issue"),
            CreateWarning("2", WarningSeverity.Warning, "Warning issue"),
            CreateWarning("3", WarningSeverity.Info, "Info tip")
        };
    }

    #endregion
}
