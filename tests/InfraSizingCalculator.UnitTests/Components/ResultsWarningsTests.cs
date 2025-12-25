using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for ResultsWarnings component
/// </summary>
public class ResultsWarningsTests : TestContext
{
    private List<ValidationWarning> CreateTestWarnings()
    {
        return new List<ValidationWarning>
        {
            ValidationWarning.Info("info1", "Info Warning 1", "This is an informational message", "Consider this suggestion"),
            ValidationWarning.Info("info2", "Info Warning 2", "Another info message"),
            ValidationWarning.Warning("warn1", "Warning 1", "This is a warning message", "Fix this issue"),
            ValidationWarning.Warning("warn2", "Warning 2", "Another warning"),
            new ValidationWarning
            {
                Id = "critical1",
                Severity = WarningSeverity.Critical,
                Title = "Critical Issue",
                Message = "This is critical",
                Icon = "🚨"
            },
            ValidationWarning.Success("success1", "Success Message", "Everything is good")
        };
    }

    #region Rendering Tests

    [Fact]
    public void ResultsWarnings_DoesNotRender_WhenWarningsIsNull()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, (List<ValidationWarning>?)null));

        // Assert
        cut.FindAll(".warnings-summary-bar").Should().BeEmpty();
        cut.FindAll(".severity-tabs").Should().BeEmpty();
    }

    [Fact]
    public void ResultsWarnings_DoesNotRender_WhenWarningsIsEmpty()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, new List<ValidationWarning>()));

        // Assert
        cut.FindAll(".warnings-summary-bar").Should().BeEmpty();
        cut.FindAll(".severity-tabs").Should().BeEmpty();
    }

    [Fact]
    public void ResultsWarnings_RendersSummaryBar()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var summaryBar = cut.Find(".warnings-summary-bar");
        summaryBar.Should().NotBeNull();
    }

    [Fact]
    public void ResultsWarnings_DisplaysCorrectSeverityCounts()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var badges = cut.FindAll(".severity-badge");
        badges.Should().HaveCount(4); // Critical, Warning, Info, Success

        var criticalBadge = badges.First(b => b.ClassList.Contains("critical"));
        criticalBadge.TextContent.Should().Contain("1 Critical");

        var warningBadge = badges.First(b => b.ClassList.Contains("warning"));
        warningBadge.TextContent.Should().Contain("2 Warnings");

        var infoBadge = badges.First(b => b.ClassList.Contains("info"));
        infoBadge.TextContent.Should().Contain("2 Tips");

        var successBadge = badges.First(b => b.ClassList.Contains("success"));
        successBadge.TextContent.Should().Contain("1 OK");
    }

    [Fact]
    public void ResultsWarnings_RendersSeverityTabs()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var tabs = cut.FindAll(".severity-tab");
        tabs.Should().HaveCountGreaterThan(0);

        // Should have All, Critical, Warnings, Tips tabs
        var allTab = tabs.First(t => t.TextContent.Contains("All"));
        allTab.Should().NotBeNull();
    }

    [Fact]
    public void ResultsWarnings_AllTabIsActiveByDefault()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var allTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("All"));
        allTab.ClassList.Should().Contain("active");
    }

    [Fact]
    public void ResultsWarnings_DisplaysWarningCards()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var cards = cut.FindAll(".warning-compact-card");
        cards.Should().HaveCountGreaterThan(0);
        cards.Should().HaveCountLessThanOrEqualTo(3); // Initial visible count is 3
    }

    [Fact]
    public void ResultsWarnings_WarningCardsShowTitle()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var firstCard = cut.Find(".warning-compact-card");
        firstCard.QuerySelector(".warning-title").Should().NotBeNull();
    }

    [Fact]
    public void ResultsWarnings_WarningCardsShowIcon()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var cards = cut.FindAll(".warning-compact-card");
        foreach (var card in cards)
        {
            card.QuerySelector(".warning-icon").Should().NotBeNull();
        }
    }

    [Fact]
    public void ResultsWarnings_LimitsInitialDisplayToThreeWarnings()
    {
        // Arrange - Create many warnings
        var manyWarnings = Enumerable.Range(1, 10)
            .Select(i => ValidationWarning.Info($"info{i}", $"Warning {i}", $"Message {i}"))
            .ToList();

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, manyWarnings));

        // Assert
        var cards = cut.FindAll(".warning-compact-card");
        cards.Should().HaveCount(3);
    }

    #endregion

    #region Severity Filtering Tests

    [Fact]
    public void ResultsWarnings_FiltersByCriticalSeverity()
    {
        // Arrange
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Act - Click Critical tab
        var criticalTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("Critical"));
        criticalTab.Click();

        // Assert - Re-query after click to get updated state
        var updatedCriticalTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("Critical"));
        updatedCriticalTab.ClassList.Should().Contain("active");
        var cards = cut.FindAll(".warning-compact-card");
        cards.Should().HaveCount(1); // Only 1 critical warning
    }

    [Fact]
    public void ResultsWarnings_FiltersByWarningSeverity()
    {
        // Arrange
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Act - Click Warnings tab
        var warningTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("Warnings"));
        warningTab.Click();

        // Assert - Re-query after click to get updated state
        var updatedWarningTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("Warnings"));
        updatedWarningTab.ClassList.Should().Contain("active");
        var cards = cut.FindAll(".warning-compact-card");
        cards.Should().HaveCount(2); // 2 warning messages
    }

    [Fact]
    public void ResultsWarnings_FiltersByInfoSeverity()
    {
        // Arrange
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Act - Click Tips tab
        var infoTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("Tips"));
        infoTab.Click();

        // Assert - Re-query after click to get updated state
        var updatedInfoTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("Tips"));
        updatedInfoTab.ClassList.Should().Contain("active");
        var cards = cut.FindAll(".warning-compact-card");
        cards.Should().HaveCount(2); // 2 info messages
    }

    [Fact]
    public void ResultsWarnings_ShowsAllWhenAllTabClicked()
    {
        // Arrange
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // First click a filter
        var criticalTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("Critical"));
        criticalTab.Click();

        // Act - Click All tab
        var allTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("All"));
        allTab.Click();

        // Assert - Re-query after click to get updated state
        var updatedAllTab = cut.FindAll(".severity-tab").First(t => t.TextContent.Contains("All"));
        updatedAllTab.ClassList.Should().Contain("active");
        var cards = cut.FindAll(".warning-compact-card");
        cards.Should().HaveCount(3); // Limited to initial visible count
    }

    [Fact]
    public void ResultsWarnings_TabCountsAreCorrect()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var tabs = cut.FindAll(".severity-tab");

        var allTab = tabs.First(t => t.TextContent.Contains("All"));
        allTab.TextContent.Should().Contain("(6)"); // All 6 warnings

        var criticalTab = tabs.First(t => t.TextContent.Contains("Critical"));
        criticalTab.TextContent.Should().Contain("(1)");

        var warningTab = tabs.First(t => t.TextContent.Contains("Warnings"));
        warningTab.TextContent.Should().Contain("(2)");

        var tipsTab = tabs.First(t => t.TextContent.Contains("Tips"));
        tipsTab.TextContent.Should().Contain("(2)");
    }

    #endregion

    #region Expand/Collapse Tests

    [Fact]
    public void ResultsWarnings_WarningsAreCollapsedByDefault()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var firstCard = cut.Find(".warning-compact-card");
        firstCard.ClassList.Should().NotContain("expanded");
        firstCard.QuerySelector(".warning-compact-body").Should().BeNull();
    }

    [Fact]
    public void ResultsWarnings_ExpandsWarningWhenClicked()
    {
        // Arrange
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        var firstCard = cut.Find(".warning-compact-card");

        // Act - Click to expand
        firstCard.QuerySelector(".warning-compact-header")!.Click();

        // Assert
        firstCard = cut.Find(".warning-compact-card"); // Re-query after state change
        firstCard.ClassList.Should().Contain("expanded");
        firstCard.QuerySelector(".warning-compact-body").Should().NotBeNull();
    }

    [Fact]
    public void ResultsWarnings_CollapsesWarningWhenClickedAgain()
    {
        // Arrange
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        var firstCard = cut.Find(".warning-compact-card");
        var header = firstCard.QuerySelector(".warning-compact-header")!;

        // Act - Expand then collapse
        header.Click();
        firstCard = cut.Find(".warning-compact-card");
        firstCard.ClassList.Should().Contain("expanded");

        header = firstCard.QuerySelector(".warning-compact-header")!;
        header.Click();

        // Assert
        firstCard = cut.Find(".warning-compact-card");
        firstCard.ClassList.Should().NotContain("expanded");
    }

    [Fact]
    public void ResultsWarnings_ShowsExpandIcon()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var expandIcon = cut.Find(".warning-compact-header .expand-icon");
        expandIcon.TextContent.Should().Be("▶");
    }

    [Fact]
    public void ResultsWarnings_ChangesExpandIcon_WhenExpanded()
    {
        // Arrange
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Act
        cut.Find(".warning-compact-header").Click();

        // Assert
        var expandIcon = cut.Find(".warning-compact-header .expand-icon");
        expandIcon.TextContent.Should().Be("▼");
    }

    [Fact]
    public void ResultsWarnings_DisplaysMessageWhenExpanded()
    {
        // Arrange
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Act
        cut.Find(".warning-compact-header").Click();

        // Assert
        var body = cut.Find(".warning-compact-body");
        var message = body.QuerySelector(".warning-message");
        message.Should().NotBeNull();
        message!.TextContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ResultsWarnings_DisplaysSuggestion_WhenProvided()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            ValidationWarning.Info("info1", "Info", "Message", "This is a suggestion")
        };

        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Act
        cut.Find(".warning-compact-header").Click();

        // Assert
        var suggestion = cut.Find(".warning-suggestion");
        suggestion.Should().NotBeNull();
        suggestion.TextContent.Should().Contain("Suggestion:");
        suggestion.TextContent.Should().Contain("This is a suggestion");
    }

    [Fact]
    public void ResultsWarnings_DoesNotDisplaySuggestion_WhenNull()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            ValidationWarning.Info("info1", "Info", "Message")
        };

        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Act
        cut.Find(".warning-compact-header").Click();

        // Assert
        var suggestions = cut.FindAll(".warning-suggestion");
        suggestions.Should().BeEmpty();
    }

    #endregion

    #region Show More Tests

    [Fact]
    public void ResultsWarnings_ShowsMoreButton_WhenMoreWarningsExist()
    {
        // Arrange - Create more than 3 warnings
        var manyWarnings = Enumerable.Range(1, 5)
            .Select(i => ValidationWarning.Info($"info{i}", $"Warning {i}", $"Message {i}"))
            .ToList();

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, manyWarnings));

        // Assert
        var showMoreBtn = cut.Find(".show-more-btn");
        showMoreBtn.Should().NotBeNull();
        showMoreBtn.TextContent.Should().Contain("Show 2 more");
    }

    [Fact]
    public void ResultsWarnings_DoesNotShowMoreButton_WhenAllWarningsVisible()
    {
        // Arrange - Only 2 warnings
        var warnings = new List<ValidationWarning>
        {
            ValidationWarning.Info("info1", "Warning 1", "Message 1"),
            ValidationWarning.Info("info2", "Warning 2", "Message 2")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.FindAll(".show-more-btn").Should().BeEmpty();
    }

    [Fact]
    public void ResultsWarnings_ShowsMoreWarnings_WhenButtonClicked()
    {
        // Arrange
        var manyWarnings = Enumerable.Range(1, 10)
            .Select(i => ValidationWarning.Info($"info{i}", $"Warning {i}", $"Message {i}"))
            .ToList();

        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, manyWarnings));

        // Assert - Initially 3 visible
        cut.FindAll(".warning-compact-card").Should().HaveCount(3);

        // Act - Click show more
        cut.Find(".show-more-btn").Click();

        // Assert - Now 8 visible (3 + 5)
        cut.FindAll(".warning-compact-card").Should().HaveCount(8);
    }

    [Fact]
    public void ResultsWarnings_UpdatesShowMoreCount_AfterClicking()
    {
        // Arrange
        var manyWarnings = Enumerable.Range(1, 10)
            .Select(i => ValidationWarning.Info($"info{i}", $"Warning {i}", $"Message {i}"))
            .ToList();

        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, manyWarnings));

        // Assert - Initially shows "7 more"
        cut.Find(".show-more-btn").TextContent.Should().Contain("Show 7 more");

        // Act - Click show more
        cut.Find(".show-more-btn").Click();

        // Assert - Now shows "2 more"
        cut.Find(".show-more-btn").TextContent.Should().Contain("Show 2 more");
    }

    #endregion

    #region Severity Styling Tests

    [Fact]
    public void ResultsWarnings_AppliesCorrectSeverityClasses()
    {
        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, CreateTestWarnings()));

        // Assert
        var cards = cut.FindAll(".warning-compact-card");

        // Check that different severity classes exist
        var criticalCards = cards.Where(c => c.ClassList.Contains("severity-critical"));
        var warningCards = cards.Where(c => c.ClassList.Contains("severity-warning"));
        var infoCards = cards.Where(c => c.ClassList.Contains("severity-info"));

        (criticalCards.Count() + warningCards.Count() + infoCards.Count()).Should().BeGreaterThan(0);
    }

    [Fact]
    public void ResultsWarnings_OnlyShowsTabsForExistingSeverities()
    {
        // Arrange - Only info warnings
        var warnings = new List<ValidationWarning>
        {
            ValidationWarning.Info("info1", "Info 1", "Message 1"),
            ValidationWarning.Info("info2", "Info 2", "Message 2")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        var tabs = cut.FindAll(".severity-tab");

        // Should have All and Tips tabs
        tabs.Should().Contain(t => t.TextContent.Contains("All"));
        tabs.Should().Contain(t => t.TextContent.Contains("Tips"));

        // Should NOT have Critical or Warnings tabs
        tabs.Should().NotContain(t => t.TextContent.Contains("Critical"));
        tabs.Should().NotContain(t => t.TextContent.Contains("Warnings") && !t.TextContent.Contains("All"));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ResultsWarnings_HandlesSingleWarning()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            ValidationWarning.Info("info1", "Only Warning", "This is the only warning")
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert
        cut.FindAll(".warning-compact-card").Should().HaveCount(1);
        cut.FindAll(".show-more-btn").Should().BeEmpty();
    }

    [Fact]
    public void ResultsWarnings_HandlesWarningsWithEmptyTitle()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            new ValidationWarning { Id = "test", Title = "", Message = "Message without title", Severity = WarningSeverity.Info }
        };

        // Act
        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Assert - Should render without error
        cut.FindAll(".warning-compact-card").Should().HaveCount(1);
    }

    [Fact]
    public void ResultsWarnings_HandlesWarningsWithEmptyMessage()
    {
        // Arrange
        var warnings = new List<ValidationWarning>
        {
            new ValidationWarning { Id = "test", Title = "Title", Message = "", Severity = WarningSeverity.Info }
        };

        var cut = RenderComponent<ResultsWarnings>(parameters => parameters
            .Add(p => p.Warnings, warnings));

        // Act - Expand the warning
        cut.Find(".warning-compact-header").Click();

        // Assert - Should render without error
        var body = cut.Find(".warning-compact-body");
        body.Should().NotBeNull();
    }

    #endregion
}
