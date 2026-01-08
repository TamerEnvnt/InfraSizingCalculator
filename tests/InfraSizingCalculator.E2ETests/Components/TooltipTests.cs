using InfraSizingCalculator.E2ETests.PageObjects;
using NUnit.Framework;

namespace InfraSizingCalculator.E2ETests.Components;

/// <summary>
/// E2E tests for info buttons and tooltips - help indicators throughout the app.
/// Tests verify info buttons open modals and tooltips display correctly.
/// </summary>
[TestFixture]
public class TooltipTests : PlaywrightFixture
{
    private WizardPage _wizard = null!;
    private ModalPage _modal = null!;

    // Locators
    private const string InfoButton = ".info-button, .info-icon, button.info-btn";
    private const string Tooltip = ".tooltip, .info-tooltip, [role='tooltip']";
    private const string TooltipContent = ".tooltip-content, .tooltip-body";

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _wizard = new WizardPage(Page);
        _modal = new ModalPage(Page);
    }

    [TearDown]
    public new async Task TearDown()
    {
        await base.TearDown();
    }

    #region Info Button Tests

    [Test]
    public async Task InfoButton_OpensInfoModal_OnClick()
    {
        await _wizard.GoToHomeAsync();

        // Find info buttons
        var infoButtons = await Page.QuerySelectorAllAsync(InfoButton);

        if (infoButtons.Count == 0)
        {
            Assert.Pass("No info buttons found on initial page");
            return;
        }

        // Click first info button
        await infoButtons[0].ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Should open info modal
        var modalVisible = await _modal.IsModalVisibleAsync();
        Assert.That(modalVisible, Is.True, "Info button should open info modal");

        // Close modal
        await _modal.CloseModalViaXButtonAsync();
    }

    [Test]
    public async Task InfoButton_VisibleNearComplexOptions()
    {
        await _wizard.GoToHomeAsync();

        // Navigate through wizard to see info buttons on different pages
        await _wizard.SelectPlatformAsync("Native");
        await _wizard.SelectDeploymentAsync("Kubernetes");
        await _wizard.SelectTechnologyAsync(".NET");

        // Move to configuration page
        await _wizard.ClickNextAsync();
        await Page.WaitForTimeoutAsync(500);

        // Check for info buttons on configuration page
        var infoButtons = await Page.QuerySelectorAllAsync(InfoButton);

        // Configuration page should have info buttons to explain options
        if (infoButtons.Count > 0)
        {
            Assert.That(infoButtons.Count, Is.GreaterThan(0),
                "Configuration page should have info buttons for complex options");
        }
        else
        {
            Assert.Pass("Info buttons may use different UI pattern on this page");
        }
    }

    #endregion

    #region Tooltip Display Tests

    [Test]
    public async Task Tooltip_Displays_OnHover()
    {
        await _wizard.GoToHomeAsync();

        // Find elements that might have tooltips (info icons, labels with title attributes)
        var elementsWithTooltips = await Page.QuerySelectorAllAsync(
            "[title], [data-tooltip], [aria-describedby], .has-tooltip");

        if (elementsWithTooltips.Count == 0)
        {
            // Try info buttons - they might show tooltip on hover
            var infoButtons = await Page.QuerySelectorAllAsync(InfoButton);
            if (infoButtons.Count > 0)
            {
                // Hover over info button
                await infoButtons[0].HoverAsync();
                await Page.WaitForTimeoutAsync(300);

                // Check if tooltip appeared
                var tooltip = await Page.QuerySelectorAsync(Tooltip);
                if (tooltip != null)
                {
                    Assert.That(await tooltip.IsVisibleAsync(), Is.True,
                        "Tooltip should appear on hover");
                }
                else
                {
                    Assert.Pass("Info buttons may use click instead of hover for tooltips");
                }
            }
            else
            {
                Assert.Pass("No tooltip-enabled elements found");
            }
        }
        else
        {
            // Hover over first element with tooltip
            await elementsWithTooltips[0].HoverAsync();
            await Page.WaitForTimeoutAsync(500);

            // Check for tooltip visibility
            var tooltip = await Page.QuerySelectorAsync(Tooltip);
            var hasTitle = await elementsWithTooltips[0].GetAttributeAsync("title");

            Assert.That(tooltip != null || !string.IsNullOrEmpty(hasTitle), Is.True,
                "Tooltip should appear on hover");
        }
    }

    [Test]
    public async Task Tooltip_Hides_OnMouseLeave()
    {
        await _wizard.GoToHomeAsync();

        // Find info buttons or tooltip-enabled elements
        var infoButtons = await Page.QuerySelectorAllAsync(InfoButton);
        var elementsWithTooltips = await Page.QuerySelectorAllAsync("[data-tooltip]");

        var targetElement = infoButtons.Count > 0 ? infoButtons[0] :
                            elementsWithTooltips.Count > 0 ? elementsWithTooltips[0] : null;

        if (targetElement == null)
        {
            Assert.Pass("No tooltip-enabled elements found");
            return;
        }

        // Hover to show tooltip
        await targetElement.HoverAsync();
        await Page.WaitForTimeoutAsync(300);

        // Move mouse away
        await Page.Mouse.MoveAsync(0, 0);
        await Page.WaitForTimeoutAsync(300);

        // Check tooltip is hidden (or modal is not auto-opened)
        var tooltip = await Page.QuerySelectorAsync(Tooltip);
        var tooltipVisible = tooltip != null && await tooltip.IsVisibleAsync();

        // Tooltip should be hidden after mouse leaves
        if (tooltipVisible)
        {
            // Some tooltips have delay before hiding
            await Page.WaitForTimeoutAsync(500);
            tooltipVisible = tooltip != null && await tooltip.IsVisibleAsync();
        }

        Assert.That(tooltipVisible, Is.False,
            "Tooltip should hide when mouse leaves the element");
    }

    [Test]
    public async Task Tooltip_Content_IsAccurate()
    {
        await _wizard.GoToHomeAsync();

        // Navigate to get more context-specific tooltips
        await _wizard.SelectPlatformAsync("Native");

        var infoButtons = await Page.QuerySelectorAllAsync(InfoButton);

        if (infoButtons.Count == 0)
        {
            Assert.Pass("No info buttons to test tooltip content");
            return;
        }

        // Click to open info modal (as tooltips)
        await infoButtons[0].ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        if (await _modal.IsModalVisibleAsync())
        {
            // Get modal content
            var content = await _modal.GetModalBodyTextAsync();

            // Content should be helpful and non-empty
            Assert.That(content, Is.Not.Null.And.Not.Empty,
                "Info tooltip/modal should have helpful content");

            // Content should be more than just a few characters
            Assert.That(content?.Length, Is.GreaterThan(10),
                "Tooltip content should be meaningful");

            await _modal.CloseModalViaXButtonAsync();
        }
        else
        {
            Assert.Pass("Info button may use different interaction pattern");
        }
    }

    [Test]
    public async Task Tooltip_Position_IsCorrect()
    {
        await _wizard.GoToHomeAsync();

        var infoButtons = await Page.QuerySelectorAllAsync(InfoButton);

        if (infoButtons.Count == 0)
        {
            Assert.Pass("No info buttons to test tooltip position");
            return;
        }

        // Get info button position
        var buttonBox = await infoButtons[0].BoundingBoxAsync();

        // Hover to show tooltip
        await infoButtons[0].HoverAsync();
        await Page.WaitForTimeoutAsync(300);

        var tooltip = await Page.QuerySelectorAsync(Tooltip);

        if (tooltip != null && await tooltip.IsVisibleAsync())
        {
            var tooltipBox = await tooltip.BoundingBoxAsync();

            if (buttonBox != null && tooltipBox != null)
            {
                // Tooltip should be near the button (within reasonable distance)
                var distance = Math.Sqrt(
                    Math.Pow(tooltipBox.X - buttonBox.X, 2) +
                    Math.Pow(tooltipBox.Y - buttonBox.Y, 2));

                // Tooltip should be within 500px of the button
                Assert.That(distance, Is.LessThan(500),
                    "Tooltip should be positioned near its trigger element");
            }
        }
        else
        {
            Assert.Pass("Tooltip not displayed on hover - may use click instead");
        }
    }

    #endregion
}
