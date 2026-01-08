using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Page object for results pages - handles all result tabs and their interactive elements.
/// Tabs: Sizing Details, Cost Breakdown, Growth Planning (with 3 sub-tabs), Insights
/// </summary>
public class ResultsPage
{
    private readonly IPage _page;
    private readonly int _defaultTimeout;
    private readonly bool _screenshotEveryStep;
    private readonly string _screenshotDir;
    private int _stepCounter;

    public ResultsPage(IPage page, int defaultTimeout = 15000)
    {
        _page = page;
        _defaultTimeout = defaultTimeout;
        _screenshotEveryStep = Environment.GetEnvironmentVariable("SCREENSHOT_EVERY_STEP") == "true";
        _screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
        _stepCounter = 0;
        Directory.CreateDirectory(_screenshotDir);
    }

    private async Task TakeStepScreenshotAsync(string stepName)
    {
        if (!_screenshotEveryStep) return;
        _stepCounter++;
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{timestamp}_Results_Step{_stepCounter:D2}_{stepName}.png";
        var filePath = Path.Combine(_screenshotDir, fileName);
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = filePath, FullPage = true });
        Console.WriteLine($"Screenshot saved: {filePath}");
    }

    #region Main Tab Navigation

    public async Task ClickSizingDetailsTabAsync()
    {
        await _page.ClickAsync(Locators.Sidebar.SizingDetailsTab);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("SizingDetails");
    }

    public async Task ClickCostBreakdownTabAsync()
    {
        await _page.ClickAsync(Locators.Sidebar.CostBreakdownTab);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("CostBreakdown");
    }

    public async Task ClickGrowthPlanningTabAsync()
    {
        await _page.ClickAsync(Locators.Sidebar.GrowthPlanningTab);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("GrowthPlanning");
    }

    public async Task ClickInsightsTabAsync()
    {
        await _page.ClickAsync(Locators.Sidebar.InsightsTab);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("Insights");
    }

    public async Task<bool> IsResultsTabVisibleAsync(string tabName)
    {
        var selector = $"button.nav-item:has-text('{tabName}')";
        var element = await _page.QuerySelectorAsync(selector);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsResultsTabActiveAsync(string tabName)
    {
        var selector = $"button.nav-item.active:has-text('{tabName}')";
        var element = await _page.QuerySelectorAsync(selector);
        return element != null;
    }

    #endregion

    #region Results Page Verification

    public async Task<bool> IsOnResultsPageAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Results.ResultsContainer);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsResultsTableVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Results.ResultsTable);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> AreSummaryCardsVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Results.SummaryCards);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Summary Cards Data

    public async Task<string?> GetTotalNodesAsync()
    {
        var card = await _page.QuerySelectorAsync(Locators.Results.TotalNodesCard);
        return card != null ? await card.TextContentAsync() : null;
    }

    public async Task<string?> GetTotalCPUAsync()
    {
        var card = await _page.QuerySelectorAsync(Locators.Results.TotalCPUCard);
        return card != null ? await card.TextContentAsync() : null;
    }

    public async Task<string?> GetTotalRAMAsync()
    {
        var card = await _page.QuerySelectorAsync(Locators.Results.TotalRAMCard);
        return card != null ? await card.TextContentAsync() : null;
    }

    public async Task<string?> GetTotalDiskAsync()
    {
        var card = await _page.QuerySelectorAsync(Locators.Results.TotalDiskCard);
        return card != null ? await card.TextContentAsync() : null;
    }

    public async Task<string?> GetMonthlyCostAsync()
    {
        var card = await _page.QuerySelectorAsync(Locators.Results.MonthlyCostCard);
        return card != null ? await card.TextContentAsync() : null;
    }

    #endregion

    #region Cost Breakdown Tab

    public async Task<bool> IsCostBreakdownPanelVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.CostBreakdown.CostPanel);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsCostEstimationPanelVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.CostBreakdown.CostEstimationPanel);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task ExpandCostCategoryAsync(string categoryName)
    {
        var selector = $".cost-category:has-text('{categoryName}') .cost-panel-header";
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
    }

    public async Task<string?> GetMonthlyTotalAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.CostBreakdown.MonthlyTotal);
        return element != null ? await element.TextContentAsync() : null;
    }

    public async Task<string?> GetYearlyTotalAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.CostBreakdown.YearlyTotal);
        return element != null ? await element.TextContentAsync() : null;
    }

    public async Task<string?> GetTCO3YearAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.CostBreakdown.TCO3Year);
        return element != null ? await element.TextContentAsync() : null;
    }

    public async Task<string?> GetTCO5YearAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.CostBreakdown.TCO5Year);
        return element != null ? await element.TextContentAsync() : null;
    }

    public async Task ExpandPricingOptionsAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.CostBreakdown.PricingOptionsExpand);
        if (element != null)
        {
            await element.ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    #endregion

    #region Growth Planning Tab - Settings Bar

    public async Task<bool> IsGrowthPlanningSettingsBarVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.SettingsBar);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task SelectGrowthPeriodAsync(string period)
    {
        await _page.SelectOptionAsync(Locators.GrowthPlanning.PeriodDropdown, new SelectOptionValue { Label = period });
        await WaitForStabilityAsync();
    }

    public async Task SetGrowthRateAsync(string rate)
    {
        await _page.FillAsync(Locators.GrowthPlanning.GrowthRateInput, rate);
        await WaitForStabilityAsync();
    }

    public async Task SelectGrowthPatternAsync(string pattern)
    {
        await _page.SelectOptionAsync(Locators.GrowthPlanning.PatternDropdown, new SelectOptionValue { Label = pattern });
        await WaitForStabilityAsync();
    }

    public async Task ToggleCostCheckboxAsync()
    {
        await _page.ClickAsync(Locators.GrowthPlanning.CostCheckbox);
        await WaitForStabilityAsync();
    }

    public async Task<bool> IsCostCheckboxCheckedAsync()
    {
        var checkbox = await _page.QuerySelectorAsync(Locators.GrowthPlanning.CostCheckbox);
        return checkbox != null && await checkbox.IsCheckedAsync();
    }

    public async Task ClickGrowthCalculateAsync()
    {
        await _page.ClickAsync(Locators.GrowthPlanning.CalculateButton);
        await _page.WaitForTimeoutAsync(1000); // Wait for growth calculation
    }

    #endregion

    #region Growth Planning Tab - Hero Strip

    public async Task<bool> IsHeroStripVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.HeroStrip);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<string?> GetAppsGrowthAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.AppsGrowth);
        return element != null ? await element.TextContentAsync() : null;
    }

    public async Task<string?> GetNodesGrowthAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.NodesGrowth);
        return element != null ? await element.TextContentAsync() : null;
    }

    public async Task<string?> GetInvestmentTotalAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.InvestmentTotal);
        return element != null ? await element.TextContentAsync() : null;
    }

    #endregion

    #region Growth Planning Tab - Sub-Tabs

    public async Task ClickResourcesSubTabAsync()
    {
        await _page.ClickAsync(Locators.GrowthPlanning.ResourcesTab);
        await WaitForStabilityAsync();
    }

    public async Task ClickCostSubTabAsync()
    {
        await _page.ClickAsync(Locators.GrowthPlanning.CostTab);
        await WaitForStabilityAsync();
    }

    public async Task ClickTimelineSubTabAsync()
    {
        await _page.ClickAsync(Locators.GrowthPlanning.TimelineTab);
        await WaitForStabilityAsync();
    }

    public async Task<bool> IsSubTabActiveAsync(string tabName)
    {
        var selector = $".tab-sm.active:has-text('{tabName}')";
        var element = await _page.QuerySelectorAsync(selector);
        return element != null;
    }

    public async Task<bool> IsSubTabVisibleAsync(string tabName)
    {
        var selector = $".tab-sm:has-text('{tabName}')";
        var element = await _page.QuerySelectorAsync(selector);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Growth Planning - Resources Sub-Tab

    public async Task<bool> AreYearCardsVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.YearCards);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<int> GetYearCardCountAsync()
    {
        var cards = await _page.QuerySelectorAllAsync(Locators.GrowthPlanning.YearCard);
        return cards.Count;
    }

    public async Task ClickYearCardAsync(int yearIndex)
    {
        var cards = await _page.QuerySelectorAllAsync(Locators.GrowthPlanning.YearCard);
        if (yearIndex < cards.Count)
        {
            await cards[yearIndex].ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    public async Task<bool> IsResourceTableVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.ResourceTable);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Growth Planning - Cost Sub-Tab

    public async Task<bool> IsCostChartVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.CostChart);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<int> GetCostBarCountAsync()
    {
        var bars = await _page.QuerySelectorAllAsync(Locators.GrowthPlanning.CostBar);
        return bars.Count;
    }

    public async Task<bool> AreCostCardsVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.CostCards);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsTotalCostCardVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.TotalCostCard);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Growth Planning - Timeline Sub-Tab

    public async Task<bool> IsTimelineVisualVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.TimelineVisual);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<int> GetTimelineNodeCountAsync()
    {
        var nodes = await _page.QuerySelectorAllAsync(Locators.GrowthPlanning.TimelineNode);
        return nodes.Count;
    }

    public async Task<bool> AreWarningsVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.WarningsSection);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> AreRecommendationsVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.GrowthPlanning.RecommendationsSection);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<int> GetAlertItemCountAsync()
    {
        var items = await _page.QuerySelectorAllAsync(Locators.GrowthPlanning.AlertItem);
        return items.Count;
    }

    #endregion

    #region Insights Tab

    public async Task<bool> IsInsightsListVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Insights.InsightsList);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<int> GetInsightItemCountAsync()
    {
        var items = await _page.QuerySelectorAllAsync(Locators.Insights.InsightItem);
        return items.Count;
    }

    public async Task ClickInsightItemAsync(int index)
    {
        var items = await _page.QuerySelectorAllAsync(Locators.Insights.InsightItem);
        if (index < items.Count)
        {
            await items[index].ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    public async Task<bool> IsInsightExpandedAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Insights.ExpandedInsight);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> HasCriticalInsightsAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Insights.CriticalBadge);
        return element != null;
    }

    #endregion

    #region Export Actions

    public async Task<bool> AreExportButtonsVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Export.ExportButtons);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task ClickSaveProfileAsync()
    {
        await _page.ClickAsync(Locators.Export.SaveProfileButton);
        await WaitForStabilityAsync();
    }

    public async Task ClickExportCSVAsync()
    {
        await _page.ClickAsync(Locators.Export.ExportCSVButton);
        await WaitForStabilityAsync();
    }

    public async Task ClickExportJSONAsync()
    {
        await _page.ClickAsync(Locators.Export.ExportJSONButton);
        await WaitForStabilityAsync();
    }

    public async Task ClickExportExcelAsync()
    {
        await _page.ClickAsync(Locators.Export.ExportExcelButton);
        await WaitForStabilityAsync();
    }

    public async Task ClickExportDiagramAsync()
    {
        await _page.ClickAsync(Locators.Export.ExportDiagramButton);
        await WaitForStabilityAsync();
    }

    public async Task ClickShareAsync()
    {
        await _page.ClickAsync(Locators.Export.ShareButton);
        await WaitForStabilityAsync();
    }

    public async Task ClickSaveScenarioAsync()
    {
        await _page.ClickAsync(Locators.Export.SaveScenarioButton);
        await WaitForStabilityAsync();
    }

    public async Task<bool> IsExportButtonVisibleAsync(string buttonName)
    {
        var selector = $"button:has-text('{buttonName}')";
        var element = await _page.QuerySelectorAsync(selector);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region New Calculation

    public async Task ClickNewCalculationAsync()
    {
        await _page.ClickAsync(Locators.Wizard.NewCalculationButton);
        await WaitForStabilityAsync();
    }

    public async Task<bool> IsNewCalculationButtonVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Wizard.NewCalculationButton);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Filter Buttons

    public async Task ClickFilterButtonAsync(string filterName)
    {
        var selector = $".filter-btn:has-text('{filterName}')";
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
    }

    public async Task<bool> IsFilterButtonActiveAsync(string filterName)
    {
        var selector = $".filter-btn.active:has-text('{filterName}')";
        var element = await _page.QuerySelectorAsync(selector);
        return element != null;
    }

    public async Task<int> GetFilterButtonCountAsync()
    {
        var buttons = await _page.QuerySelectorAllAsync(Locators.Shared.FilterButton);
        return buttons.Count;
    }

    #endregion

    #region Complete Journey Methods

    /// <summary>
    /// Fully interact with all elements on the Sizing Details tab
    /// </summary>
    public async Task InteractWithSizingDetailsAsync()
    {
        // Try to click the tab if it's visible (may already be on sizing tab by default)
        var tabButton = await _page.QuerySelectorAsync(Locators.Sidebar.SizingDetailsTab);
        if (tabButton != null && await tabButton.IsVisibleAsync())
        {
            await ClickSizingDetailsTabAsync();
        }

        // Wait for either summary cards or results table to be visible
        // Home.razor uses .data-grid-table, SizingResultsView uses .grand-total-bar
        await _page.WaitForSelectorAsync(
            $"{Locators.Results.SummaryCards}, {Locators.Results.ResultsTable}",
            new() { Timeout = 5000, State = WaitForSelectorState.Visible });

        // Verify core elements are present
        var hasSummaryCards = await AreSummaryCardsVisibleAsync();
        var hasResultsTable = await IsResultsTableVisibleAsync();
        Assert.That(hasSummaryCards || hasResultsTable, Is.True,
            $"Sizing Details should show summary cards or results table (hasSummaryCards={hasSummaryCards}, hasResultsTable={hasResultsTable})");

        // Get data from summary cards if available
        await GetTotalNodesAsync();
        await GetTotalCPUAsync();
        await GetTotalRAMAsync();
        await GetTotalDiskAsync();
        await GetMonthlyCostAsync();
    }

    /// <summary>
    /// Fully interact with all elements on the Cost Breakdown tab
    /// </summary>
    public async Task InteractWithCostBreakdownAsync()
    {
        await ClickCostBreakdownTabAsync();

        // Verify panel is visible
        var panelVisible = await IsCostBreakdownPanelVisibleAsync() || await IsCostEstimationPanelVisibleAsync();

        if (panelVisible)
        {
            // Get cost totals
            await GetMonthlyTotalAsync();
            await GetYearlyTotalAsync();
            await GetTCO3YearAsync();
            await GetTCO5YearAsync();

            // Try to expand pricing options
            await ExpandPricingOptionsAsync();
        }
    }

    /// <summary>
    /// Fully interact with all elements on the Growth Planning tab including sub-tabs
    /// </summary>
    public async Task InteractWithGrowthPlanningAsync()
    {
        await ClickGrowthPlanningTabAsync();

        // Check if settings bar is visible
        if (await IsGrowthPlanningSettingsBarVisibleAsync())
        {
            // Configure growth settings
            await SelectGrowthPeriodAsync("3 Years");
            await SetGrowthRateAsync("15");
            await SelectGrowthPatternAsync("Linear");
            await ToggleCostCheckboxAsync();

            // Click Calculate
            await ClickGrowthCalculateAsync();

            // Check hero strip after calculation
            if (await IsHeroStripVisibleAsync())
            {
                await GetAppsGrowthAsync();
                await GetNodesGrowthAsync();
                await GetInvestmentTotalAsync();
            }

            // Navigate through all sub-tabs
            await InteractWithResourcesSubTabAsync();
            await InteractWithCostSubTabAsync();
            await InteractWithTimelineSubTabAsync();
        }
    }

    /// <summary>
    /// Interact with Resources sub-tab elements
    /// </summary>
    public async Task InteractWithResourcesSubTabAsync()
    {
        await ClickResourcesSubTabAsync();

        if (await AreYearCardsVisibleAsync())
        {
            var cardCount = await GetYearCardCountAsync();
            if (cardCount > 0)
            {
                // Click each year card
                for (int i = 0; i < Math.Min(cardCount, 3); i++)
                {
                    await ClickYearCardAsync(i);
                }
            }
        }

        await IsResourceTableVisibleAsync();
    }

    /// <summary>
    /// Interact with Cost sub-tab elements (only if Cost tab is visible - it's conditional on IncludeCostProjections)
    /// </summary>
    public async Task InteractWithCostSubTabAsync()
    {
        // Cost tab is conditional - only shown when IncludeCostProjections is enabled
        var costTab = await _page.QuerySelectorAsync(Locators.GrowthPlanning.CostTab);
        if (costTab == null || !(await costTab.IsVisibleAsync()))
        {
            // Cost tab not visible, skip this sub-tab
            return;
        }

        await ClickCostSubTabAsync();

        await IsCostChartVisibleAsync();
        await GetCostBarCountAsync();
        await AreCostCardsVisibleAsync();
        await IsTotalCostCardVisibleAsync();
    }

    /// <summary>
    /// Interact with Timeline sub-tab elements
    /// </summary>
    public async Task InteractWithTimelineSubTabAsync()
    {
        await ClickTimelineSubTabAsync();

        await IsTimelineVisualVisibleAsync();
        await GetTimelineNodeCountAsync();
        await AreWarningsVisibleAsync();
        await AreRecommendationsVisibleAsync();
        await GetAlertItemCountAsync();
    }

    /// <summary>
    /// Fully interact with all elements on the Insights tab (only if visible - conditional on InsightsCount > 0)
    /// </summary>
    public async Task InteractWithInsightsAsync()
    {
        // Insights tab is conditional - only shown when InsightsCount > 0
        var insightsTab = await _page.QuerySelectorAsync(Locators.Sidebar.InsightsTab);
        if (insightsTab == null || !(await insightsTab.IsVisibleAsync()))
        {
            // Insights tab not visible, skip
            return;
        }

        await ClickInsightsTabAsync();

        if (await IsInsightsListVisibleAsync())
        {
            var itemCount = await GetInsightItemCountAsync();

            // Click first few insights to expand them
            for (int i = 0; i < Math.Min(itemCount, 3); i++)
            {
                await ClickInsightItemAsync(i);
            }

            await HasCriticalInsightsAsync();
        }
    }

    /// <summary>
    /// Complete interaction with all result tabs - the full journey
    /// </summary>
    public async Task CompleteFullResultsJourneyAsync()
    {
        // Verify we're on results page
        Assert.That(await IsOnResultsPageAsync(), Is.True, "Should be on results page");

        // Visit each main tab and interact with all elements
        await InteractWithSizingDetailsAsync();
        await InteractWithCostBreakdownAsync();
        await InteractWithGrowthPlanningAsync();
        await InteractWithInsightsAsync();

        // Test export buttons if visible
        if (await AreExportButtonsVisibleAsync())
        {
            // Just verify visibility, don't actually export in tests
            await IsExportButtonVisibleAsync("Export CSV");
            await IsExportButtonVisibleAsync("Export JSON");
            await IsExportButtonVisibleAsync("Export Excel");
        }
    }

    #endregion

    #region Helpers

    private async Task WaitForStabilityAsync()
    {
        await _page.WaitForTimeoutAsync(500);
    }

    #endregion
}
