using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Page object for the Scenarios page - manages saved configurations and comparisons.
/// </summary>
public class ScenariosPage
{
    private readonly IPage _page;

    // Locators
    private const string ScenariosPageIndicator = ".scenarios-page, [data-page='scenarios'], .saved-scenarios";
    private const string ScenariosList = ".scenarios-list, .scenario-cards, .saved-list";
    private const string ScenarioCard = ".scenario-card, .saved-scenario, .scenario-item";
    private const string ScenarioName = ".scenario-name, .card-title, .scenario-title";
    private const string ScenarioDate = ".scenario-date, .saved-date, .created-date";
    private const string EmptyState = ".empty-state, .no-scenarios, .placeholder-message";
    private const string DeleteButton = ".delete-btn, button[aria-label='Delete'], button:has-text('Delete')";
    private const string FavoriteButton = ".favorite-btn, button[aria-label='Favorite'], .star-btn";
    private const string CopyButton = ".copy-btn, button:has-text('Copy'), button:has-text('Duplicate')";
    private const string CompareCheckbox = ".compare-checkbox, input[type='checkbox'].compare";
    private const string CompareButton = "button:has-text('Compare'), .compare-btn";
    private const string FilterInput = "input[type='search'], .filter-input, .search-scenarios";
    private const string BulkDeleteButton = ".bulk-delete, button:has-text('Delete Selected')";

    public ScenariosPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Navigate to the Scenarios page
    /// </summary>
    public async Task NavigateToAsync()
    {
        // Try clicking Scenarios nav item
        var scenariosNav = await _page.QuerySelectorAsync(
            "a[href*='scenarios'], button:has-text('Scenarios'), .nav-item:has-text('Scenarios')");

        if (scenariosNav != null)
        {
            await scenariosNav.ClickAsync();
            await _page.WaitForTimeoutAsync(500);
        }
        else
        {
            // Try direct navigation
            await _page.GotoAsync("/scenarios");
            await _page.WaitForTimeoutAsync(500);
        }
    }

    /// <summary>
    /// Check if on Scenarios page
    /// </summary>
    public async Task<bool> IsOnScenariosPageAsync()
    {
        var indicator = await _page.QuerySelectorAsync(ScenariosPageIndicator);
        if (indicator != null) return true;

        // Check URL
        var url = _page.Url;
        return url.Contains("scenarios", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get count of saved scenarios
    /// </summary>
    public async Task<int> GetScenarioCountAsync()
    {
        var scenarios = await _page.QuerySelectorAllAsync(ScenarioCard);
        return scenarios.Count;
    }

    /// <summary>
    /// Check if empty state is shown
    /// </summary>
    public async Task<bool> IsEmptyStateVisibleAsync()
    {
        var emptyState = await _page.QuerySelectorAsync(EmptyState);
        return emptyState != null && await emptyState.IsVisibleAsync();
    }

    /// <summary>
    /// Get empty state message
    /// </summary>
    public async Task<string?> GetEmptyStateMessageAsync()
    {
        var emptyState = await _page.QuerySelectorAsync(EmptyState);
        return emptyState != null ? await emptyState.TextContentAsync() : null;
    }

    /// <summary>
    /// Get all scenario names
    /// </summary>
    public async Task<List<string>> GetScenarioNamesAsync()
    {
        var names = new List<string>();
        var nameElements = await _page.QuerySelectorAllAsync(ScenarioName);

        foreach (var element in nameElements)
        {
            var text = await element.TextContentAsync();
            if (!string.IsNullOrEmpty(text))
                names.Add(text);
        }

        return names;
    }

    /// <summary>
    /// Click on a scenario to view details
    /// </summary>
    public async Task ClickScenarioAsync(int index)
    {
        var scenarios = await _page.QuerySelectorAllAsync(ScenarioCard);
        if (index < scenarios.Count)
        {
            await scenarios[index].ClickAsync();
            await _page.WaitForTimeoutAsync(300);
        }
    }

    /// <summary>
    /// Click delete button for a scenario
    /// </summary>
    public async Task ClickDeleteAsync(int index)
    {
        var scenarios = await _page.QuerySelectorAllAsync(ScenarioCard);
        if (index < scenarios.Count)
        {
            var deleteBtn = await scenarios[index].QuerySelectorAsync(DeleteButton);
            if (deleteBtn != null)
            {
                await deleteBtn.ClickAsync();
                await _page.WaitForTimeoutAsync(300);
            }
        }
    }

    /// <summary>
    /// Toggle favorite for a scenario
    /// </summary>
    public async Task ToggleFavoriteAsync(int index)
    {
        var scenarios = await _page.QuerySelectorAllAsync(ScenarioCard);
        if (index < scenarios.Count)
        {
            var favoriteBtn = await scenarios[index].QuerySelectorAsync(FavoriteButton);
            if (favoriteBtn != null)
            {
                await favoriteBtn.ClickAsync();
                await _page.WaitForTimeoutAsync(300);
            }
        }
    }

    /// <summary>
    /// Check if scenario is favorited
    /// </summary>
    public async Task<bool> IsScenarioFavoritedAsync(int index)
    {
        var scenarios = await _page.QuerySelectorAllAsync(ScenarioCard);
        if (index < scenarios.Count)
        {
            var favoriteBtn = await scenarios[index].QuerySelectorAsync(FavoriteButton);
            if (favoriteBtn != null)
            {
                return await favoriteBtn.EvaluateAsync<bool>(
                    "el => el.classList.contains('active') || el.classList.contains('favorited') || el.getAttribute('aria-pressed') === 'true'");
            }
        }
        return false;
    }

    /// <summary>
    /// Click copy button for a scenario
    /// </summary>
    public async Task ClickCopyAsync(int index)
    {
        var scenarios = await _page.QuerySelectorAllAsync(ScenarioCard);
        if (index < scenarios.Count)
        {
            var copyBtn = await scenarios[index].QuerySelectorAsync(CopyButton);
            if (copyBtn != null)
            {
                await copyBtn.ClickAsync();
                await _page.WaitForTimeoutAsync(300);
            }
        }
    }

    /// <summary>
    /// Select scenarios for comparison
    /// </summary>
    public async Task SelectForCompareAsync(params int[] indices)
    {
        var scenarios = await _page.QuerySelectorAllAsync(ScenarioCard);

        foreach (var index in indices)
        {
            if (index < scenarios.Count)
            {
                var checkbox = await scenarios[index].QuerySelectorAsync(CompareCheckbox);
                if (checkbox != null)
                {
                    var isChecked = await checkbox.IsCheckedAsync();
                    if (!isChecked)
                    {
                        await checkbox.ClickAsync();
                        await _page.WaitForTimeoutAsync(100);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Click compare button
    /// </summary>
    public async Task ClickCompareAsync()
    {
        var compareBtn = await _page.QuerySelectorAsync(CompareButton);
        if (compareBtn != null)
        {
            await compareBtn.ClickAsync();
            await _page.WaitForTimeoutAsync(500);
        }
    }

    /// <summary>
    /// Filter scenarios by text
    /// </summary>
    public async Task FilterScenariosAsync(string searchText)
    {
        var filterInput = await _page.QuerySelectorAsync(FilterInput);
        if (filterInput != null)
        {
            await filterInput.FillAsync("");
            await filterInput.FillAsync(searchText);
            await _page.WaitForTimeoutAsync(300);
        }
    }

    /// <summary>
    /// Click bulk delete button
    /// </summary>
    public async Task ClickBulkDeleteAsync()
    {
        var bulkDeleteBtn = await _page.QuerySelectorAsync(BulkDeleteButton);
        if (bulkDeleteBtn != null)
        {
            await bulkDeleteBtn.ClickAsync();
            await _page.WaitForTimeoutAsync(300);
        }
    }

    /// <summary>
    /// Check if compare button is enabled
    /// </summary>
    public async Task<bool> IsCompareButtonEnabledAsync()
    {
        var compareBtn = await _page.QuerySelectorAsync(CompareButton);
        if (compareBtn != null)
        {
            return await compareBtn.IsEnabledAsync();
        }
        return false;
    }

    /// <summary>
    /// Get scenario details from a card
    /// </summary>
    public async Task<ScenarioInfo?> GetScenarioInfoAsync(int index)
    {
        var scenarios = await _page.QuerySelectorAllAsync(ScenarioCard);
        if (index >= scenarios.Count) return null;

        var scenario = scenarios[index];

        var nameElement = await scenario.QuerySelectorAsync(ScenarioName);
        var dateElement = await scenario.QuerySelectorAsync(ScenarioDate);

        return new ScenarioInfo
        {
            Name = nameElement != null ? await nameElement.TextContentAsync() ?? "" : "",
            Date = dateElement != null ? await dateElement.TextContentAsync() ?? "" : "",
            IsFavorited = await IsScenarioFavoritedAsync(index)
        };
    }
}

/// <summary>
/// Represents scenario information
/// </summary>
public class ScenarioInfo
{
    public string Name { get; set; } = "";
    public string Date { get; set; } = "";
    public bool IsFavorited { get; set; }
}
