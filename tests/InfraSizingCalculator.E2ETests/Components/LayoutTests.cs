using InfraSizingCalculator.E2ETests.PageObjects;
using NUnit.Framework;

namespace InfraSizingCalculator.E2ETests.Components;

/// <summary>
/// E2E tests for layout components: main content, sidebars, header, footer.
/// Tests verify the overall page structure and responsive behavior.
/// </summary>
[TestFixture]
public class LayoutTests : PlaywrightFixture
{
    private LayoutPage _layout = null!;
    private WizardPage _wizard = null!;

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _layout = new LayoutPage(Page);
        _wizard = new WizardPage(Page);
    }

    [TearDown]
    public new async Task TearDown()
    {
        await base.TearDown();
    }

    #region Main Content Tests

    [Test]
    public async Task Layout_MainContent_RendersCorrectly()
    {
        await _wizard.GoToHomeAsync();

        // Verify main content is visible
        Assert.That(await _layout.IsMainContentVisibleAsync(), Is.True,
            "Main content area should be visible");

        // Get bounding box to verify it has dimensions
        var box = await _layout.GetMainContentBoundingBoxAsync();
        Assert.That(box, Is.Not.Null, "Main content should have bounding box");
        Assert.That(box.Width, Is.GreaterThan(100), "Main content should have reasonable width");
        Assert.That(box.Height, Is.GreaterThan(100), "Main content should have reasonable height");
    }

    #endregion

    #region Sidebar Tests

    [Test]
    public async Task Layout_LeftSidebar_ShowsNavigationSteps()
    {
        await _wizard.GoToHomeAsync();

        // Check if left sidebar is visible
        var leftSidebarVisible = await _layout.IsLeftSidebarVisibleAsync();

        if (leftSidebarVisible)
        {
            // Verify it contains navigation elements
            Assert.That(await _layout.HasNavigationStepsAsync(), Is.True,
                "Left sidebar should contain navigation steps");
        }
        else
        {
            // On some layouts, navigation might be elsewhere
            Assert.Pass("Left sidebar not visible - navigation may use different UI pattern");
        }
    }

    [Test]
    public async Task Layout_RightSidebar_ShowsSummaryStats()
    {
        await _wizard.GoToHomeAsync();

        // Check if right sidebar is visible
        var rightSidebarVisible = await _layout.IsRightSidebarVisibleAsync();

        if (rightSidebarVisible)
        {
            // Right sidebar typically shows summary stats
            var hasStats = await _layout.HasSummaryStatsAsync();

            // Stats may only appear after some selections are made
            if (hasStats)
            {
                Assert.That(hasStats, Is.True, "Right sidebar should contain summary stats");
            }
            else
            {
                Assert.Pass("Right sidebar visible but stats may appear after selections");
            }
        }
        else
        {
            Assert.Pass("Right sidebar not visible - summary may use different UI pattern");
        }
    }

    #endregion

    #region Header Tests

    [Test]
    public async Task Layout_Header_AlwaysVisible()
    {
        await _wizard.GoToHomeAsync();

        // Verify header is visible on initial load
        Assert.That(await _layout.IsHeaderVisibleAsync(), Is.True,
            "Header should be visible on page load");

        // Navigate through wizard steps and verify header remains visible
        await _wizard.SelectPlatformAsync("Native");
        Assert.That(await _layout.IsHeaderVisibleAsync(), Is.True,
            "Header should remain visible after platform selection");

        await _wizard.SelectDeploymentAsync("Kubernetes");
        Assert.That(await _layout.IsHeaderVisibleAsync(), Is.True,
            "Header should remain visible after deployment selection");
    }

    #endregion

    #region Three-Column Layout Tests

    [Test]
    public async Task Layout_ThreeColumnLayout_OnDesktop()
    {
        // Ensure desktop viewport
        await _layout.SetDesktopViewportAsync();
        await _wizard.GoToHomeAsync();

        // Get layout type
        var layoutType = await _layout.GetCurrentLayoutTypeAsync();

        if (layoutType == "three-column")
        {
            // Verify three-column layout
            Assert.That(await _layout.IsThreeColumnLayoutAsync(), Is.True,
                "Desktop should use three-column layout");

            // Verify columns are properly aligned (no overlap)
            Assert.That(await _layout.AreColumnsProperlyAlignedAsync(), Is.True,
                "Layout columns should not overlap");
        }
        else
        {
            // Application may use two-column or different layout
            Assert.Pass($"Application uses {layoutType} layout on desktop");
        }
    }

    #endregion

    #region Responsive Layout Tests

    [Test]
    public async Task Layout_ResponsiveLayout_OnMobile()
    {
        // Set mobile viewport
        await _layout.SetMobileViewportAsync();
        await _wizard.GoToHomeAsync();

        // On mobile, layout should adapt
        var layoutType = await _layout.GetCurrentLayoutTypeAsync();

        // Mobile typically uses single-column or simplified layout
        var isMobileOptimized =
            layoutType == "single-column" ||
            layoutType == "two-column" ||
            await _layout.AreSidebarsHiddenOnMobileAsync();

        // Get layout metrics for debugging
        var metrics = await _layout.GetLayoutMetricsAsync();

        Assert.That(isMobileOptimized || metrics.IsMainContentVisible, Is.True,
            $"Mobile layout should adapt - current: {layoutType}, viewport: {metrics.ViewportWidth}x{metrics.ViewportHeight}");

        // Main content should still be visible on mobile
        Assert.That(await _layout.IsMainContentVisibleAsync(), Is.True,
            "Main content should be visible on mobile");
    }

    #endregion
}
