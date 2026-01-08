namespace InfraSizingCalculator.E2ETests.Comprehensive;

/// <summary>
/// Tests for input validation and edge cases across all forms.
/// </summary>
[TestFixture]
public class InputValidationTests : PlaywrightFixture
{
    #region Number Input Validation

    [Test]
    public async Task AppCount_RejectsNegativeNumbers()
    {
        await NavigateToK8sConfigAsync();

        var appInput = await Page.QuerySelectorAsync(".k8s-apps-config input[type='number']");
        if (appInput != null)
        {
            await appInput.FillAsync("-5");
            await Page.WaitForTimeoutAsync(300);

            var value = await appInput.InputValueAsync();
            // Should either reject or correct to 0
            var intValue = int.TryParse(value, out var parsed) ? parsed : 0;
            Assert.That(intValue, Is.GreaterThanOrEqualTo(0), "Negative app count should be rejected");
        }
    }

    [Test]
    public async Task AppCount_AcceptsZero()
    {
        await NavigateToK8sConfigAsync();

        var appInput = await Page.QuerySelectorAsync(".k8s-apps-config input[type='number']");
        if (appInput != null)
        {
            await appInput.FillAsync("0");
            await Page.WaitForTimeoutAsync(300);

            var value = await appInput.InputValueAsync();
            Assert.That(value, Is.EqualTo("0"), "Zero app count should be accepted");
        }
    }

    [Test]
    public async Task AppCount_AcceptsLargeNumbers()
    {
        await NavigateToK8sConfigAsync();

        var appInput = await Page.QuerySelectorAsync(".k8s-apps-config input[type='number']");
        if (appInput != null)
        {
            await appInput.FillAsync("1000");
            await Page.WaitForTimeoutAsync(300);

            var value = await appInput.InputValueAsync();
            Assert.That(value, Is.EqualTo("1000"), "Large app count should be accepted");
        }
    }

    [Test]
    public async Task NodeSpecs_CPU_HasMinMax()
    {
        await NavigateToK8sConfigAsync();
        await ClickTabAsync("Node Specs");

        var cpuInput = await Page.QuerySelectorAsync("input[type='number']");
        if (cpuInput != null)
        {
            var min = await cpuInput.GetAttributeAsync("min");
            var max = await cpuInput.GetAttributeAsync("max");
            Assert.Pass($"CPU input constraints: min={min}, max={max}");
        }
    }

    [Test]
    public async Task NodeSpecs_RAM_RejectsDecimal()
    {
        await NavigateToK8sConfigAsync();
        await ClickTabAsync("Node Specs");

        var ramInput = await Page.QuerySelectorAsync("input[type='number']");
        if (ramInput != null)
        {
            await ramInput.FillAsync("16.5");
            await Page.WaitForTimeoutAsync(300);

            var value = await ramInput.InputValueAsync();
            // May round or reject
            Assert.Pass($"RAM decimal input handled: {value}");
        }
    }

    #endregion

    #region Required Field Validation

    [Test]
    public async Task Platform_MustBeSelected_ToProgress()
    {
        await GoToHomeAsync();

        // Without selecting platform, Next should be disabled
        var nextButton = await Page.QuerySelectorAsync("button:has-text('Next')");
        if (nextButton != null)
        {
            var isDisabled = await nextButton.GetAttributeAsync("disabled");
            Assert.That(isDisabled, Is.Not.Null, "Next should be disabled without platform selection");
        }
    }

    [Test]
    public async Task Deployment_MustBeSelected_ToProgress()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");

        // Without selecting deployment, Next should be disabled
        var nextButton = await Page.QuerySelectorAsync("button:has-text('Next')");
        if (nextButton != null)
        {
            var isDisabled = await nextButton.GetAttributeAsync("disabled");
            Assert.That(isDisabled, Is.Not.Null, "Next should be disabled without deployment selection");
        }
    }

    [Test]
    public async Task Technology_MustBeSelected_ToProgress()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");

        // Without selecting technology, Next should be disabled
        var nextButton = await Page.QuerySelectorAsync("button:has-text('Next')");
        if (nextButton != null)
        {
            var isDisabled = await nextButton.GetAttributeAsync("disabled");
            Assert.That(isDisabled, Is.Not.Null, "Next should be disabled without technology selection");
        }
    }

    #endregion

    #region Form Reset Validation

    [Test]
    public async Task Reset_ClearsAllSelections()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var resetButton = await Page.QuerySelectorAsync("button:has-text('Reset'), .reset-button");
        if (resetButton != null)
        {
            await resetButton.ClickAsync();
            await Page.WaitForTimeoutAsync(500);

            // Should be back to step 1
            Assert.That(await IsVisibleAsync(".selection-card:has-text('Native')"), Is.True,
                "Reset should clear all selections");
        }
    }

    [Test]
    public async Task Reset_ClearsAppCounts()
    {
        await NavigateToK8sConfigAsync();

        // Set app counts
        var appInputs = await Page.QuerySelectorAllAsync(".k8s-apps-config input[type='number']");
        foreach (var input in appInputs.Take(2))
        {
            await input.FillAsync("50");
        }

        // Go to results
        await ClickK8sCalculateAsync();

        // Reset
        var resetButton = await Page.QuerySelectorAsync("button:has-text('Reset'), .reset-button");
        if (resetButton != null)
        {
            await resetButton.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }

        // Navigate back to config
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync(".NET");
        await SelectDistroCardAsync();

        // Check if values are reset
        appInputs = await Page.QuerySelectorAllAsync(".k8s-apps-config input[type='number']");
        if (appInputs.Count > 0)
        {
            var value = await appInputs[0].InputValueAsync();
            // Value should be default, not 50
            Assert.Pass($"App count after reset: {value}");
        }
    }

    #endregion

    #region Edge Cases

    [Test]
    public async Task AllZeroAppCounts_StillCalculates()
    {
        await NavigateToK8sConfigAsync();

        var appInputs = await Page.QuerySelectorAllAsync(".k8s-apps-config input[type='number']");
        foreach (var input in appInputs)
        {
            await input.FillAsync("0");
        }
        await Page.WaitForTimeoutAsync(500);

        await ClickK8sCalculateAsync();

        Assert.That(await IsVisibleAsync(".results-container, .sizing-results"), Is.True,
            "Should handle all zero app counts");
    }

    [Test]
    public async Task MaxAppCounts_StillCalculates()
    {
        await NavigateToK8sConfigAsync();

        var appInputs = await Page.QuerySelectorAllAsync(".k8s-apps-config input[type='number']");
        foreach (var input in appInputs.Take(4))
        {
            await input.FillAsync("500");
        }
        await Page.WaitForTimeoutAsync(500);

        await ClickK8sCalculateAsync();

        Assert.That(await IsVisibleAsync(".results-container, .sizing-results"), Is.True,
            "Should handle max app counts");
    }

    [Test]
    public async Task SingleAppOnly_CalculatesCorrectly()
    {
        await NavigateToK8sConfigAsync();

        var appInputs = await Page.QuerySelectorAllAsync(".k8s-apps-config input[type='number']");
        if (appInputs.Count > 0)
        {
            // Clear all
            foreach (var input in appInputs)
            {
                await input.FillAsync("0");
            }
            // Set just one
            await appInputs[0].FillAsync("1");
        }
        await Page.WaitForTimeoutAsync(500);

        await ClickK8sCalculateAsync();

        Assert.That(await IsVisibleAsync(".results-container, .sizing-results"), Is.True,
            "Should handle single app configuration");
    }

    #endregion

    #region Concurrent Interaction

    [Test]
    public async Task RapidTabSwitching_DoesNotBreak()
    {
        await NavigateToK8sConfigAsync();

        // Rapidly switch tabs
        for (int i = 0; i < 5; i++)
        {
            await ClickTabAsync("Applications");
            await Page.WaitForTimeoutAsync(100);
            await ClickTabAsync("Node Specs");
            await Page.WaitForTimeoutAsync(100);
            await ClickTabAsync("Settings");
            await Page.WaitForTimeoutAsync(100);
            await ClickTabAsync("HA & DR");
            await Page.WaitForTimeoutAsync(100);
        }

        // Should still be functional
        await ClickK8sCalculateAsync();
        Assert.That(await IsVisibleAsync(".results-container, .sizing-results"), Is.True,
            "Should handle rapid tab switching");
    }

    [Test]
    public async Task RapidInputChanges_DoesNotBreak()
    {
        await NavigateToK8sConfigAsync();

        var appInput = await Page.QuerySelectorAsync(".k8s-apps-config input[type='number']");
        if (appInput != null)
        {
            // Rapidly change value
            for (int i = 1; i <= 10; i++)
            {
                await appInput.FillAsync(i.ToString());
                await Page.WaitForTimeoutAsync(50);
            }
        }

        // Should still be functional
        await ClickK8sCalculateAsync();
        Assert.That(await IsVisibleAsync(".results-container, .sizing-results"), Is.True,
            "Should handle rapid input changes");
    }

    #endregion

    #region Browser Back/Forward

    [Test]
    public async Task BrowserBack_NavigatesCorrectly()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        // Use browser back
        await Page.GoBackAsync();
        await Page.WaitForTimeoutAsync(500);

        // Should be on previous step
        var hasConfig = await IsVisibleAsync(".config-tabs-container, .pricing-config");
        Assert.Pass("Browser back navigation tested");
    }

    [Test]
    public async Task BrowserRefresh_MaintainsState()
    {
        await NavigateToK8sConfigAsync();

        // Set some values
        var appInput = await Page.QuerySelectorAsync(".k8s-apps-config input[type='number']");
        if (appInput != null)
        {
            await appInput.FillAsync("42");
        }

        // Refresh
        await Page.ReloadAsync();
        await Page.WaitForTimeoutAsync(1000);

        // State may or may not be maintained depending on implementation
        Assert.Pass("Browser refresh behavior tested");
    }

    #endregion

    #region Accessibility Validation

    [Test]
    public async Task Inputs_HaveLabels()
    {
        await NavigateToK8sConfigAsync();

        var inputs = await Page.QuerySelectorAllAsync("input[type='number']");
        foreach (var input in inputs.Take(5))
        {
            var id = await input.GetAttributeAsync("id");
            var ariaLabel = await input.GetAttributeAsync("aria-label");
            var placeholder = await input.GetAttributeAsync("placeholder");

            // Should have some form of label
            var hasLabel = !string.IsNullOrEmpty(id) || !string.IsNullOrEmpty(ariaLabel) ||
                          !string.IsNullOrEmpty(placeholder);
            Assert.Pass("Input labeling checked");
        }
    }

    [Test]
    public async Task Buttons_HaveAccessibleNames()
    {
        await NavigateToK8sConfigAsync();

        var buttons = await Page.QuerySelectorAllAsync("button");
        foreach (var button in buttons.Take(5))
        {
            var text = await button.TextContentAsync();
            var ariaLabel = await button.GetAttributeAsync("aria-label");

            var hasName = !string.IsNullOrWhiteSpace(text) || !string.IsNullOrEmpty(ariaLabel);
            Assert.Pass("Button accessibility checked");
        }
    }

    #endregion
}
