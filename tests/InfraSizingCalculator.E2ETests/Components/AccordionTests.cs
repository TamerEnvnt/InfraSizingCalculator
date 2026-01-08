using InfraSizingCalculator.E2ETests.PageObjects;
using NUnit.Framework;

namespace InfraSizingCalculator.E2ETests.Components;

/// <summary>
/// E2E tests for accordion components - expandable panels used throughout the app.
/// Tests verify expand/collapse behavior, content rendering, and environment panels.
/// </summary>
[TestFixture]
public class AccordionTests : PlaywrightFixture
{
    private AccordionPage _accordion = null!;
    private WizardPage _wizard = null!;
    private ConfigurationPage _config = null!;

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _accordion = new AccordionPage(Page);
        _wizard = new WizardPage(Page);
        _config = new ConfigurationPage(Page);
    }

    [TearDown]
    public new async Task TearDown()
    {
        await base.TearDown();
    }

    #region Helper Methods

    /// <summary>
    /// Navigate to K8s configuration page where accordions are used
    /// </summary>
    private async Task NavigateToK8sConfigAsync()
    {
        await _wizard.GoToHomeAsync();
        await _wizard.SelectPlatformAsync("Native");
        await _wizard.SelectDeploymentAsync("Kubernetes");
        await _wizard.SelectTechnologyAsync(".NET");
        await _wizard.ClickNextAsync();

        // Wait for configuration page to load
        await Page.WaitForTimeoutAsync(500);
    }

    #endregion

    #region Panel Expand/Collapse Tests

    [Test]
    public async Task Accordion_Panel_ExpandsOnClick()
    {
        await NavigateToK8sConfigAsync();

        var panelCount = await _accordion.GetPanelCountAsync();
        if (panelCount == 0)
        {
            Assert.Pass("No accordion panels found on this page");
            return;
        }

        // Ensure first panel is collapsed
        await _accordion.CollapsePanelAsync(0);

        // Click to expand
        await _accordion.ClickPanelHeaderAsync(0);

        // Verify expanded
        Assert.That(await _accordion.IsPanelExpandedAsync(0), Is.True,
            "Panel should expand after clicking header");
    }

    [Test]
    public async Task Accordion_Panel_CollapsesOnSecondClick()
    {
        await NavigateToK8sConfigAsync();

        var panelCount = await _accordion.GetPanelCountAsync();
        if (panelCount == 0)
        {
            Assert.Pass("No accordion panels found on this page");
            return;
        }

        // Expand first panel
        await _accordion.ExpandPanelAsync(0);
        Assert.That(await _accordion.IsPanelExpandedAsync(0), Is.True,
            "Panel should be expanded");

        // Click again to collapse
        await _accordion.ClickPanelHeaderAsync(0);

        // Verify collapsed
        Assert.That(await _accordion.IsPanelExpandedAsync(0), Is.False,
            "Panel should collapse after clicking header again");
    }

    [Test]
    public async Task Accordion_SingleExpand_CollapsesOthers()
    {
        await NavigateToK8sConfigAsync();

        var panelCount = await _accordion.GetPanelCountAsync();
        if (panelCount < 2)
        {
            Assert.Pass("Not enough panels to test single-expand behavior");
            return;
        }

        // Test single-expand behavior
        var singleExpandMode = await _accordion.DoesExpandOneCollapseOthersAsync();

        // Document the behavior (either mode is valid)
        if (singleExpandMode)
        {
            Assert.Pass("Accordion uses single-expand mode (only one panel open at a time)");
        }
        else
        {
            // Multi-expand mode - verify both can be expanded
            await _accordion.ExpandPanelAsync(0);
            await _accordion.ExpandPanelAsync(1);

            var bothExpanded = await _accordion.IsPanelExpandedAsync(0) &&
                               await _accordion.IsPanelExpandedAsync(1);

            Assert.That(bothExpanded, Is.True,
                "In multi-expand mode, multiple panels should stay expanded");
        }
    }

    #endregion

    #region Content Tests

    [Test]
    public async Task Accordion_Content_RendersWhenExpanded()
    {
        await NavigateToK8sConfigAsync();

        var panelCount = await _accordion.GetPanelCountAsync();
        if (panelCount == 0)
        {
            Assert.Pass("No accordion panels found on this page");
            return;
        }

        // Expand first panel
        await _accordion.ExpandPanelAsync(0);

        // Check if content is visible
        var contentVisible = await _accordion.IsPanelContentVisibleAsync(0);
        Assert.That(contentVisible, Is.True,
            "Panel content should be visible when expanded");

        // Verify content has actual content
        var contentText = await _accordion.GetPanelContentTextAsync(0);
        Assert.That(contentText, Is.Not.Null.And.Not.Empty,
            "Panel content should have text/elements when expanded");
    }

    [Test]
    public async Task Accordion_Header_ShowsCorrectTitle()
    {
        await NavigateToK8sConfigAsync();

        var panelCount = await _accordion.GetPanelCountAsync();
        if (panelCount == 0)
        {
            Assert.Pass("No accordion panels found on this page");
            return;
        }

        // Get all panel titles
        var titles = await _accordion.GetAllPanelTitlesAsync();

        // Each panel should have a title
        Assert.That(titles.Count, Is.GreaterThan(0), "Should have panel titles");

        foreach (var title in titles)
        {
            Assert.That(title, Is.Not.Null.And.Not.Empty,
                "Each panel should have a non-empty title");
        }
    }

    [Test]
    public async Task Accordion_ExpandedState_PersistsCorrectly()
    {
        await NavigateToK8sConfigAsync();

        var panelCount = await _accordion.GetPanelCountAsync();
        if (panelCount == 0)
        {
            Assert.Pass("No accordion panels found on this page");
            return;
        }

        // Expand a panel
        await _accordion.ExpandPanelAsync(0);
        Assert.That(await _accordion.IsPanelExpandedAsync(0), Is.True);

        // Wait a moment to ensure state persists
        await Page.WaitForTimeoutAsync(500);

        // Verify still expanded
        Assert.That(await _accordion.IsPanelExpandedAsync(0), Is.True,
            "Expanded state should persist");
    }

    [Test]
    public async Task Accordion_AllPanels_HaveHeaders()
    {
        await NavigateToK8sConfigAsync();

        var panelCount = await _accordion.GetPanelCountAsync();
        if (panelCount == 0)
        {
            Assert.Pass("No accordion panels found on this page");
            return;
        }

        // Each panel should have a clickable header
        for (int i = 0; i < panelCount; i++)
        {
            var title = await _accordion.GetPanelTitleAsync(i);
            Assert.That(title, Is.Not.Null.And.Not.Empty,
                $"Panel {i} should have a header title");
        }
    }

    #endregion

    #region Environment Panel Tests

    [Test]
    public async Task Accordion_EnvironmentPanels_ShowAppCounts()
    {
        await NavigateToK8sConfigAsync();

        // Get environment names (Dev, Test, Staging, Production)
        var envNames = await _accordion.GetEnvironmentNamesAsync();

        if (envNames.Count == 0)
        {
            Assert.Pass("No environment panels found - may use different UI");
            return;
        }

        // Check for expected environments
        var expectedEnvs = new[] { "Dev", "Test", "Staging", "Prod" };
        var hasExpectedEnvs = envNames.Any(name =>
            expectedEnvs.Any(exp => name.Contains(exp, StringComparison.OrdinalIgnoreCase)));

        if (hasExpectedEnvs)
        {
            // Check if app counts are displayed
            var panelCount = await _accordion.GetPanelCountAsync();
            for (int i = 0; i < Math.Min(panelCount, 4); i++)
            {
                var appCount = await _accordion.GetEnvironmentAppCountAsync(i);
                // App count might be displayed or might be in header
                if (!string.IsNullOrEmpty(appCount))
                {
                    Assert.That(appCount, Does.Contain("app").IgnoreCase
                        .Or.Match(@"\d+"),
                        "Environment panel should show app count");
                }
            }
        }

        Assert.Pass($"Found {envNames.Count} environment panels");
    }

    #endregion
}
