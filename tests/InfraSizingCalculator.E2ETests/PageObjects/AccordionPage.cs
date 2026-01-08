using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Page object for accordion components - expandable panels used throughout the app.
/// </summary>
public class AccordionPage
{
    private readonly IPage _page;
    private readonly int _defaultTimeout;

    public AccordionPage(IPage page, int defaultTimeout = 15000)
    {
        _page = page;
        _defaultTimeout = defaultTimeout;
    }

    #region Locators

    // Accordion container
    private const string AccordionContainer = ".h-accordion";
    private const string AccordionPanel = ".h-accordion-panel";
    private const string AccordionHeader = ".accordion-header";
    private const string AccordionContent = ".accordion-content";
    private const string AccordionExpanded = ".h-accordion-panel.expanded";
    private const string AccordionCollapsed = ".h-accordion-panel:not(.expanded)";

    // Environment-specific accordions
    private const string EnvAccordion = ".env-apps-accordion";
    private const string EnvPanel = ".h-accordion-panel";
    private const string EnvPanelTitle = ".accordion-header .panel-title, .accordion-header h3";
    private const string EnvAppCount = ".app-count, .total-apps";

    #endregion

    #region Panel Visibility

    public async Task<bool> IsAccordionContainerVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(AccordionContainer);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<int> GetPanelCountAsync()
    {
        var panels = await _page.QuerySelectorAllAsync(AccordionPanel);
        return panels.Count;
    }

    public async Task<int> GetExpandedPanelCountAsync()
    {
        var panels = await _page.QuerySelectorAllAsync(AccordionExpanded);
        return panels.Count;
    }

    public async Task<int> GetCollapsedPanelCountAsync()
    {
        var panels = await _page.QuerySelectorAllAsync(AccordionCollapsed);
        return panels.Count;
    }

    #endregion

    #region Panel Interaction

    public async Task<bool> IsPanelExpandedAsync(int panelIndex)
    {
        var panels = await _page.QuerySelectorAllAsync(AccordionPanel);
        if (panelIndex >= panels.Count) return false;

        return await panels[panelIndex].EvaluateAsync<bool>(
            "el => el.classList.contains('expanded')");
    }

    public async Task ClickPanelHeaderAsync(int panelIndex)
    {
        var panels = await _page.QuerySelectorAllAsync(AccordionPanel);
        if (panelIndex < panels.Count)
        {
            // Get the panel key from data attribute
            var key = await panels[panelIndex].EvaluateAsync<string?>(
                "el => el.querySelector('[data-accordion-key]')?.getAttribute('data-accordion-key')");

            if (!string.IsNullOrEmpty(key))
            {
                // Use JavaScript to toggle the panel directly (bypasses Blazor event system)
                await _page.EvaluateAsync($@"
                    (function() {{
                        var panel = document.querySelector('.h-accordion-panel:has([data-accordion-key=""{key}""])');
                        if (!panel) return;

                        var wasExpanded = panel.classList.contains('expanded');
                        var accordion = panel.closest('.h-accordion');

                        // In single-expand mode, collapse others first
                        if (accordion && accordion.classList.contains('single-expand')) {{
                            accordion.querySelectorAll('.h-accordion-panel.expanded').forEach(function(p) {{
                                if (p !== panel) p.classList.remove('expanded');
                            }});
                        }}

                        // Toggle this panel
                        if (wasExpanded) {{
                            panel.classList.remove('expanded');
                        }} else {{
                            panel.classList.add('expanded');
                        }}
                    }})();
                ");
                await WaitForAnimationAsync();
            }
            else
            {
                // Fallback to regular click
                var headers = await _page.QuerySelectorAllAsync(AccordionHeader);
                if (panelIndex < headers.Count)
                {
                    await headers[panelIndex].ClickAsync();
                    await WaitForAnimationAsync();
                }
            }
        }
    }

    public async Task ExpandPanelAsync(int panelIndex)
    {
        if (!await IsPanelExpandedAsync(panelIndex))
        {
            await ClickPanelHeaderAsync(panelIndex);
        }
    }

    public async Task ExpandPanelByKeyAsync(string key)
    {
        // Use the JavaScript function to expand panel by key
        await _page.EvaluateAsync($"window.expandAccordionPanel && window.expandAccordionPanel('{key}')");
        await WaitForAnimationAsync();
    }

    public async Task CollapsePanelAsync(int panelIndex)
    {
        if (await IsPanelExpandedAsync(panelIndex))
        {
            await ClickPanelHeaderAsync(panelIndex);
        }
    }

    public async Task TogglePanelAsync(int panelIndex)
    {
        await ClickPanelHeaderAsync(panelIndex);
    }

    #endregion

    #region Panel Content

    public async Task<string?> GetPanelTitleAsync(int panelIndex)
    {
        var headers = await _page.QuerySelectorAllAsync(AccordionHeader);
        if (panelIndex >= headers.Count) return null;

        var title = await headers[panelIndex].QuerySelectorAsync(".panel-title, h3, .title");
        return title != null ? await title.TextContentAsync() : await headers[panelIndex].TextContentAsync();
    }

    public async Task<bool> IsPanelContentVisibleAsync(int panelIndex)
    {
        var panels = await _page.QuerySelectorAllAsync(AccordionPanel);
        if (panelIndex >= panels.Count) return false;

        var content = await panels[panelIndex].QuerySelectorAsync(AccordionContent);
        return content != null && await content.IsVisibleAsync();
    }

    public async Task<string?> GetPanelContentTextAsync(int panelIndex)
    {
        var panels = await _page.QuerySelectorAllAsync(AccordionPanel);
        if (panelIndex >= panels.Count) return null;

        var content = await panels[panelIndex].QuerySelectorAsync(AccordionContent);
        return content != null ? await content.TextContentAsync() : null;
    }

    public async Task<IReadOnlyList<string>> GetAllPanelTitlesAsync()
    {
        var titles = new List<string>();
        var headers = await _page.QuerySelectorAllAsync(AccordionHeader);

        foreach (var header in headers)
        {
            var text = await header.TextContentAsync();
            if (!string.IsNullOrEmpty(text))
            {
                titles.Add(text.Trim());
            }
        }

        return titles;
    }

    #endregion

    #region Environment Panels (K8s Configuration)

    public async Task<int> GetEnvironmentPanelCountAsync()
    {
        var panels = await _page.QuerySelectorAllAsync($"{EnvAccordion} {EnvPanel}");
        return panels.Count;
    }

    public async Task<IReadOnlyList<string>> GetEnvironmentNamesAsync()
    {
        var names = new List<string>();
        var titles = await _page.QuerySelectorAllAsync(EnvPanelTitle);

        foreach (var title in titles)
        {
            var text = await title.TextContentAsync();
            if (!string.IsNullOrEmpty(text))
            {
                names.Add(text.Trim());
            }
        }

        return names;
    }

    public async Task<string?> GetEnvironmentAppCountAsync(int panelIndex)
    {
        var panels = await _page.QuerySelectorAllAsync(AccordionPanel);
        if (panelIndex >= panels.Count) return null;

        var countElement = await panels[panelIndex].QuerySelectorAsync(EnvAppCount);
        return countElement != null ? await countElement.TextContentAsync() : null;
    }

    public async Task ExpandEnvironmentPanelByNameAsync(string envName)
    {
        // Map environment names to keys (Dev, Test, Stage, Prod, LifeTime)
        var keyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Development", "Dev" },
            { "Testing", "Test" },
            { "Staging", "Stage" },
            { "Production", "Prod" },
            { "LifeTime", "LifeTime" },
            { "Dev", "Dev" },
            { "Test", "Test" },
            { "Stage", "Stage" },
            { "Prod", "Prod" }
        };

        string? key = null;
        foreach (var kvp in keyMap)
        {
            if (envName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                key = kvp.Value;
                break;
            }
        }

        if (!string.IsNullOrEmpty(key))
        {
            // Use JavaScript to expand panel by key
            await _page.EvaluateAsync($@"
                (function() {{
                    var btn = document.querySelector('[data-accordion-key=""{key}""]');
                    if (!btn) return;

                    var panel = btn.closest('.h-accordion-panel');
                    if (!panel) return;

                    if (!panel.classList.contains('expanded')) {{
                        // In single-expand mode, collapse others first
                        var accordion = panel.closest('.h-accordion');
                        if (accordion && accordion.classList.contains('single-expand')) {{
                            accordion.querySelectorAll('.h-accordion-panel.expanded').forEach(function(p) {{
                                p.classList.remove('expanded');
                            }});
                        }}
                        panel.classList.add('expanded');
                    }}
                }})();
            ");
            await WaitForAnimationAsync();
        }
        else
        {
            // Fallback to header search
            var headers = await _page.QuerySelectorAllAsync(AccordionHeader);
            for (int i = 0; i < headers.Count; i++)
            {
                var text = await headers[i].TextContentAsync();
                if (text?.Contains(envName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    var panel = await headers[i].EvaluateHandleAsync("el => el.closest('.h-accordion-panel')");
                    var isExpanded = await panel.EvaluateAsync<bool>("el => el.classList.contains('expanded')");

                    if (!isExpanded)
                    {
                        await headers[i].ClickAsync();
                        await WaitForAnimationAsync();
                    }
                    break;
                }
            }
        }
    }

    #endregion

    #region Accordion Behavior

    public async Task<bool> IsOnlyOneExpandedAsync()
    {
        var expandedCount = await GetExpandedPanelCountAsync();
        return expandedCount == 1;
    }

    public async Task<bool> AreAllCollapsedAsync()
    {
        var expandedCount = await GetExpandedPanelCountAsync();
        return expandedCount == 0;
    }

    public async Task<bool> DoesExpandOneCollapseOthersAsync()
    {
        var panelCount = await GetPanelCountAsync();
        if (panelCount < 2) return true; // Can't test with less than 2 panels

        // Expand first panel
        await ExpandPanelAsync(0);
        var initialExpanded = await IsPanelExpandedAsync(0);

        // Expand second panel
        await ExpandPanelAsync(1);

        // Check if first panel collapsed (single-expand behavior)
        var firstStillExpanded = await IsPanelExpandedAsync(0);
        var secondExpanded = await IsPanelExpandedAsync(1);

        // If first collapsed when second opened = single-expand mode
        // If both expanded = multi-expand mode
        return !firstStillExpanded && secondExpanded;
    }

    #endregion

    #region Helpers

    private async Task WaitForAnimationAsync()
    {
        await _page.WaitForTimeoutAsync(300);
    }

    #endregion
}
