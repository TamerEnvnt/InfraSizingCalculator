using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Page object for layout verification - main content, sidebars, header, footer.
/// Tests the overall page structure and responsive behavior.
/// </summary>
public class LayoutPage
{
    private readonly IPage _page;
    private readonly int _defaultTimeout;

    public LayoutPage(IPage page, int defaultTimeout = 15000)
    {
        _page = page;
        _defaultTimeout = defaultTimeout;
    }

    #region Locators

    // Layout containers
    private const string MainContent = ".main-content";
    private const string LeftSidebar = ".left-sidebar";
    private const string RightSidebar = ".right-sidebar, .summary-panel";
    private const string HeaderBar = ".header-bar, .app-header, header";
    private const string Footer = ".app-footer, footer";

    // Responsive breakpoints
    private const int MobileBreakpoint = 768;
    private const int TabletBreakpoint = 1024;
    private const int DesktopBreakpoint = 1280;

    #endregion

    #region Main Content Verification

    public async Task<bool> IsMainContentVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(MainContent);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<ElementHandleBoundingBoxResult?> GetMainContentBoundingBoxAsync()
    {
        var element = await _page.QuerySelectorAsync(MainContent);
        return element != null ? await element.BoundingBoxAsync() : null;
    }

    public async Task WaitForMainContentAsync()
    {
        await _page.WaitForSelectorAsync(MainContent, new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = _defaultTimeout
        });
    }

    #endregion

    #region Left Sidebar Verification

    public async Task<bool> IsLeftSidebarVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(LeftSidebar);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<ElementHandleBoundingBoxResult?> GetLeftSidebarBoundingBoxAsync()
    {
        var element = await _page.QuerySelectorAsync(LeftSidebar);
        return element != null ? await element.BoundingBoxAsync() : null;
    }

    public async Task<bool> HasNavigationStepsAsync()
    {
        var steps = await _page.QuerySelectorAllAsync($"{LeftSidebar} .nav-step, {LeftSidebar} .nav-section");
        return steps.Count > 0;
    }

    #endregion

    #region Right Sidebar Verification

    public async Task<bool> IsRightSidebarVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(RightSidebar);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<ElementHandleBoundingBoxResult?> GetRightSidebarBoundingBoxAsync()
    {
        var element = await _page.QuerySelectorAsync(RightSidebar);
        return element != null ? await element.BoundingBoxAsync() : null;
    }

    public async Task<bool> HasSummaryStatsAsync()
    {
        var stats = await _page.QuerySelectorAllAsync($"{RightSidebar} .stat-item, {RightSidebar} .summary-stat, {RightSidebar} .stat-label");
        return stats.Count > 0;
    }

    #endregion

    #region Header Verification

    public async Task<bool> IsHeaderVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(HeaderBar);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<ElementHandleBoundingBoxResult?> GetHeaderBoundingBoxAsync()
    {
        var element = await _page.QuerySelectorAsync(HeaderBar);
        return element != null ? await element.BoundingBoxAsync() : null;
    }

    public async Task<bool> IsHeaderStickyAsync()
    {
        var position = await _page.EvaluateAsync<string>(
            $"() => getComputedStyle(document.querySelector('{HeaderBar}')).position");
        return position == "fixed" || position == "sticky";
    }

    #endregion

    #region Footer Verification

    public async Task<bool> IsFooterVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Footer);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> HasFooterAsync()
    {
        var element = await _page.QuerySelectorAsync(Footer);
        return element != null;
    }

    #endregion

    #region Three-Column Layout Verification

    public async Task<bool> IsThreeColumnLayoutAsync()
    {
        var leftVisible = await IsLeftSidebarVisibleAsync();
        var mainVisible = await IsMainContentVisibleAsync();
        var rightVisible = await IsRightSidebarVisibleAsync();

        return leftVisible && mainVisible && rightVisible;
    }

    public async Task<LayoutMetrics> GetLayoutMetricsAsync()
    {
        var viewportSize = _page.ViewportSize;

        return new LayoutMetrics
        {
            ViewportWidth = viewportSize?.Width ?? 0,
            ViewportHeight = viewportSize?.Height ?? 0,
            HeaderBox = await GetHeaderBoundingBoxAsync(),
            LeftSidebarBox = await GetLeftSidebarBoundingBoxAsync(),
            MainContentBox = await GetMainContentBoundingBoxAsync(),
            RightSidebarBox = await GetRightSidebarBoundingBoxAsync(),
            IsLeftSidebarVisible = await IsLeftSidebarVisibleAsync(),
            IsRightSidebarVisible = await IsRightSidebarVisibleAsync(),
            IsMainContentVisible = await IsMainContentVisibleAsync(),
            IsHeaderVisible = await IsHeaderVisibleAsync()
        };
    }

    public async Task<bool> AreColumnsProperlyAlignedAsync()
    {
        var metrics = await GetLayoutMetricsAsync();

        // Check if layout components don't overlap
        if (metrics.LeftSidebarBox != null && metrics.MainContentBox != null)
        {
            // Left sidebar should be to the left of main content
            var leftEnd = metrics.LeftSidebarBox.X + metrics.LeftSidebarBox.Width;
            var mainStart = metrics.MainContentBox.X;

            if (leftEnd > mainStart)
            {
                return false; // Overlap detected
            }
        }

        if (metrics.MainContentBox != null && metrics.RightSidebarBox != null)
        {
            // Main content should be to the left of right sidebar
            var mainEnd = metrics.MainContentBox.X + metrics.MainContentBox.Width;
            var rightStart = metrics.RightSidebarBox.X;

            if (mainEnd > rightStart)
            {
                return false; // Overlap detected
            }
        }

        return true;
    }

    #endregion

    #region Responsive Layout

    public async Task SetViewportSizeAsync(int width, int height)
    {
        await _page.SetViewportSizeAsync(width, height);
        await _page.WaitForTimeoutAsync(300); // Wait for responsive changes
    }

    public async Task SetMobileViewportAsync()
    {
        await SetViewportSizeAsync(375, 812); // iPhone X dimensions
    }

    public async Task SetTabletViewportAsync()
    {
        await SetViewportSizeAsync(768, 1024); // iPad dimensions
    }

    public async Task SetDesktopViewportAsync()
    {
        await SetViewportSizeAsync(1920, 1080); // Full HD desktop
    }

    public async Task<bool> IsMobileLayoutAsync()
    {
        var viewportSize = _page.ViewportSize;
        return viewportSize?.Width < MobileBreakpoint;
    }

    public async Task<bool> IsTabletLayoutAsync()
    {
        var viewportSize = _page.ViewportSize;
        return viewportSize?.Width >= MobileBreakpoint && viewportSize?.Width < DesktopBreakpoint;
    }

    public async Task<bool> IsDesktopLayoutAsync()
    {
        var viewportSize = _page.ViewportSize;
        return viewportSize?.Width >= DesktopBreakpoint;
    }

    public async Task<bool> AreSidebarsHiddenOnMobileAsync()
    {
        await SetMobileViewportAsync();

        // On mobile, sidebars might be hidden or collapsed
        var leftVisible = await IsLeftSidebarVisibleAsync();
        var rightVisible = await IsRightSidebarVisibleAsync();

        // Expect at least one sidebar to be hidden/collapsed on mobile
        return !leftVisible || !rightVisible;
    }

    #endregion

    #region Layout Structure Tests

    public async Task<int> GetVisibleSectionCountAsync()
    {
        var count = 0;

        if (await IsHeaderVisibleAsync()) count++;
        if (await IsLeftSidebarVisibleAsync()) count++;
        if (await IsMainContentVisibleAsync()) count++;
        if (await IsRightSidebarVisibleAsync()) count++;
        if (await IsFooterVisibleAsync()) count++;

        return count;
    }

    public async Task<string> GetCurrentLayoutTypeAsync()
    {
        var left = await IsLeftSidebarVisibleAsync();
        var right = await IsRightSidebarVisibleAsync();
        var main = await IsMainContentVisibleAsync();

        if (left && right && main) return "three-column";
        if ((left || right) && main) return "two-column";
        if (main) return "single-column";

        return "unknown";
    }

    #endregion
}

/// <summary>
/// Metrics about the current layout state
/// </summary>
public class LayoutMetrics
{
    public int ViewportWidth { get; set; }
    public int ViewportHeight { get; set; }
    public ElementHandleBoundingBoxResult? HeaderBox { get; set; }
    public ElementHandleBoundingBoxResult? LeftSidebarBox { get; set; }
    public ElementHandleBoundingBoxResult? MainContentBox { get; set; }
    public ElementHandleBoundingBoxResult? RightSidebarBox { get; set; }
    public bool IsLeftSidebarVisible { get; set; }
    public bool IsRightSidebarVisible { get; set; }
    public bool IsMainContentVisible { get; set; }
    public bool IsHeaderVisible { get; set; }
}
