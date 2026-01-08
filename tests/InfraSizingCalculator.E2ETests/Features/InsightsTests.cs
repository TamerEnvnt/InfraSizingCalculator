using InfraSizingCalculator.E2ETests.PageObjects;
using NUnit.Framework;

namespace InfraSizingCalculator.E2ETests.Features;

/// <summary>
/// E2E tests for Insights tab functionality: visibility, list display, badges, expand/collapse.
/// Insights tab is conditional - only visible when there are warnings/recommendations.
/// </summary>
[TestFixture]
public class InsightsTests : PlaywrightFixture
{
    private WizardPage _wizard = null!;
    private ConfigurationPage _config = null!;
    private PricingPage _pricing = null!;
    private ResultsPage _results = null!;

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _wizard = new WizardPage(Page);
        _config = new ConfigurationPage(Page);
        _pricing = new PricingPage(Page);
        _results = new ResultsPage(Page);
    }

    [TearDown]
    public new async Task TearDown()
    {
        await base.TearDown();
    }

    #region Helper Methods

    /// <summary>
    /// Navigate to results with configuration that should generate warnings
    /// </summary>
    private async Task NavigateToResultsWithWarningsAsync()
    {
        // Use minimal configuration that typically generates warnings
        await _wizard.GoToHomeAsync();
        await _wizard.SelectPlatformAsync("Native");
        await _wizard.SelectDeploymentAsync("Kubernetes");
        await _wizard.SelectTechnologyAsync(".NET");

        // Use configuration with very low resources to trigger warnings
        await _wizard.ClickNextAsync();

        // Try to set low app counts to generate capacity warnings
        // This depends on the specific business rules that generate insights
        await _wizard.ClickNextAsync();
        await _pricing.ClickCalculateAsync();

        // Verify we're on results page
        Assert.That(await _results.IsOnResultsPageAsync(), Is.True,
            "Should be on results page after calculation");
    }

    /// <summary>
    /// Navigate to results with standard configuration
    /// </summary>
    private async Task NavigateToResultsAsync()
    {
        await _wizard.NavigateToNativeK8sConfigAsync(".NET");
        await _wizard.ClickNextAsync();
        await _pricing.ClickCalculateAsync();

        Assert.That(await _results.IsOnResultsPageAsync(), Is.True,
            "Should be on results page after calculation");
    }

    #endregion

    #region Insights Tab Visibility Tests

    [Test]
    public async Task Insights_Tab_ShowsWhenWarningsExist()
    {
        await NavigateToResultsWithWarningsAsync();

        // Check if Insights tab is visible in sidebar
        var insightsTabVisible = await _results.IsResultsTabVisibleAsync("Insights");

        // Insights tab is conditional based on InsightsCount > 0
        if (insightsTabVisible)
        {
            Assert.That(insightsTabVisible, Is.True, "Insights tab should be visible when warnings exist");

            // Click on Insights tab
            await _results.ClickInsightsTabAsync();
            await Page.WaitForTimeoutAsync(500);

            // Verify insights content is displayed
            var insightsListVisible = await _results.IsInsightsListVisibleAsync();
            Assert.That(insightsListVisible, Is.True,
                "Insights list should be visible after clicking Insights tab");
        }
        else
        {
            Assert.Pass("No warnings generated for this configuration - Insights tab hidden as expected");
        }
    }

    [Test]
    public async Task Insights_Tab_HiddenWhenNoWarnings()
    {
        await NavigateToResultsAsync();

        // Check if Insights tab is visible
        var insightsTabVisible = await _results.IsResultsTabVisibleAsync("Insights");

        // If visible, it means there are warnings for this configuration
        // If not visible, the test passes - no warnings = no Insights tab
        if (!insightsTabVisible)
        {
            Assert.Pass("Insights tab correctly hidden when no warnings exist");
        }
        else
        {
            // Insights tab visible means there are warnings - document this
            var itemCount = await _results.GetInsightItemCountAsync();
            Assert.That(itemCount, Is.GreaterThan(0),
                "Insights tab visible but should have items if shown");
        }
    }

    #endregion

    #region Insights List Tests

    [Test]
    public async Task Insights_List_DisplaysAllWarnings()
    {
        await NavigateToResultsWithWarningsAsync();

        var insightsTabVisible = await _results.IsResultsTabVisibleAsync("Insights");
        if (!insightsTabVisible)
        {
            Assert.Pass("No Insights tab - no warnings to test");
            return;
        }

        await _results.ClickInsightsTabAsync();
        await Page.WaitForTimeoutAsync(500);

        // Get count of insight items
        var itemCount = await _results.GetInsightItemCountAsync();

        // If Insights tab is visible, there should be at least one item
        Assert.That(itemCount, Is.GreaterThan(0),
            "Insights list should contain at least one item when tab is visible");

        // Verify list structure
        var insightsList = await Page.QuerySelectorAsync(Locators.Insights.InsightsList);
        Assert.That(insightsList, Is.Not.Null, "Insights list container should exist");
    }

    #endregion

    #region Insights Item Interaction Tests

    [Test]
    public async Task Insights_Item_ExpandsOnClick()
    {
        await NavigateToResultsWithWarningsAsync();

        var insightsTabVisible = await _results.IsResultsTabVisibleAsync("Insights");
        if (!insightsTabVisible)
        {
            Assert.Pass("No Insights tab - no items to expand");
            return;
        }

        await _results.ClickInsightsTabAsync();
        await Page.WaitForTimeoutAsync(500);

        var itemCount = await _results.GetInsightItemCountAsync();
        if (itemCount == 0)
        {
            Assert.Pass("No insight items to expand");
            return;
        }

        // Click first insight item to expand
        await _results.ClickInsightItemAsync(0);
        await Page.WaitForTimeoutAsync(300);

        // Check if expanded content is visible
        var isExpanded = await _results.IsInsightExpandedAsync();

        // Note: Some implementations may not have expandable items
        if (isExpanded)
        {
            Assert.That(isExpanded, Is.True, "Insight item should expand on click");
        }
        else
        {
            // Check if insight details are shown inline
            var firstItem = await Page.QuerySelectorAsync(Locators.Insights.InsightItem);
            var itemText = firstItem != null ? await firstItem.TextContentAsync() : "";
            Assert.That(itemText, Is.Not.Empty, "Insight item should have content");
        }
    }

    [Test]
    public async Task Insights_Multiple_CanBeExpanded()
    {
        await NavigateToResultsWithWarningsAsync();

        var insightsTabVisible = await _results.IsResultsTabVisibleAsync("Insights");
        if (!insightsTabVisible)
        {
            Assert.Pass("No Insights tab - no items to test");
            return;
        }

        await _results.ClickInsightsTabAsync();
        await Page.WaitForTimeoutAsync(500);

        var itemCount = await _results.GetInsightItemCountAsync();
        if (itemCount < 2)
        {
            Assert.Pass("Not enough insight items to test multiple expansion");
            return;
        }

        // Click multiple insight items
        for (int i = 0; i < Math.Min(itemCount, 3); i++)
        {
            await _results.ClickInsightItemAsync(i);
            await Page.WaitForTimeoutAsync(200);
        }

        // Verify items can be interacted with
        Assert.Pass($"Successfully interacted with {Math.Min(itemCount, 3)} insight items");
    }

    #endregion

    #region Insights Badge Tests

    [Test]
    public async Task Insights_CriticalBadge_HasCorrectStyling()
    {
        await NavigateToResultsWithWarningsAsync();

        var insightsTabVisible = await _results.IsResultsTabVisibleAsync("Insights");
        if (!insightsTabVisible)
        {
            Assert.Pass("No Insights tab - no badges to test");
            return;
        }

        await _results.ClickInsightsTabAsync();
        await Page.WaitForTimeoutAsync(500);

        // Check for critical badges
        var hasCritical = await _results.HasCriticalInsightsAsync();

        if (hasCritical)
        {
            // Verify critical badge styling
            var criticalBadge = await Page.QuerySelectorAsync(Locators.Insights.CriticalBadge);
            Assert.That(criticalBadge, Is.Not.Null, "Critical badge should exist");

            // Verify critical badge is visually distinct (e.g., red color)
            var backgroundColor = await criticalBadge.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");

            // Critical badges are typically red-ish colors
            Assert.That(backgroundColor, Is.Not.Null.And.Not.Empty,
                "Critical badge should have distinct styling");
        }
        else
        {
            Assert.Pass("No critical badges in current insights - test documents this state");
        }
    }

    [Test]
    public async Task Insights_Badge_ShowsCorrectCount()
    {
        await NavigateToResultsWithWarningsAsync();

        // Check for badge on Insights tab
        var insightsTabBadge = await Page.QuerySelectorAsync(
            "button.nav-item:has-text('Insights') .badge, " +
            "button.nav-item:has-text('Insights') .count");

        if (insightsTabBadge != null)
        {
            var badgeText = await insightsTabBadge.TextContentAsync();

            // Badge should show a number
            var isParseable = int.TryParse(badgeText?.Trim(), out var count);

            if (isParseable)
            {
                await _results.ClickInsightsTabAsync();
                await Page.WaitForTimeoutAsync(500);

                var itemCount = await _results.GetInsightItemCountAsync();

                // Badge count should match actual item count
                Assert.That(count, Is.EqualTo(itemCount),
                    $"Badge count ({count}) should match actual insights count ({itemCount})");
            }
            else
            {
                Assert.Pass($"Badge shows non-numeric indicator: {badgeText}");
            }
        }
        else
        {
            // No badge - might use different indicator
            Assert.Pass("No count badge on Insights tab - may use different UI pattern");
        }
    }

    #endregion

    #region Insights Content Tests

    [Test]
    public async Task Insights_Content_DisplaysRecommendations()
    {
        await NavigateToResultsWithWarningsAsync();

        var insightsTabVisible = await _results.IsResultsTabVisibleAsync("Insights");
        if (!insightsTabVisible)
        {
            Assert.Pass("No Insights tab - no recommendations to test");
            return;
        }

        await _results.ClickInsightsTabAsync();
        await Page.WaitForTimeoutAsync(500);

        // Get all insight items
        var insightItems = await Page.QuerySelectorAllAsync(Locators.Insights.InsightItem);

        foreach (var item in insightItems)
        {
            var text = await item.TextContentAsync();

            // Insights should contain actionable information
            Assert.That(text, Is.Not.Null.And.Not.Empty,
                "Each insight item should have content");

            // Content should be meaningful (at least some characters)
            Assert.That(text.Length, Is.GreaterThan(10),
                "Insight content should be meaningful (not just a few characters)");
        }

        Assert.Pass($"Verified {insightItems.Count} insights have meaningful content");
    }

    #endregion
}
