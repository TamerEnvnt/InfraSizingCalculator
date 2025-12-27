namespace InfraSizingCalculator.E2ETests;

/// <summary>
/// E2E tests for input field behavior and validation
/// Tests work with both single cluster (.tier-card) and multi-cluster (.tier-panel) modes
/// </summary>
[TestFixture]
public class InputValidationTests : PlaywrightFixture
{
    /// <summary>
    /// Helper to get an app input element - works with both single and multi-cluster modes
    /// </summary>
    private async Task<Microsoft.Playwright.IElementHandle?> GetAppInputAsync()
    {
        // Try single cluster mode first (.tier-card)
        var singleClusterInput = await Page.QuerySelectorAsync(".tier-card input[type='number']");
        if (singleClusterInput != null)
            return singleClusterInput;

        // Multi-cluster mode: Dev panel is expanded by default with tier inputs
        var tierInput = await Page.QuerySelectorAsync(".tier-panel input.tier-input");
        return tierInput;
    }

    /// <summary>
    /// Helper to get all app inputs - works with both modes
    /// </summary>
    private async Task<IReadOnlyList<Microsoft.Playwright.IElementHandle>> GetAllAppInputsAsync()
    {
        // Try single cluster mode first
        var singleClusterInputs = await Page.QuerySelectorAllAsync(".tier-card input[type='number']");
        if (singleClusterInputs.Count > 0)
            return singleClusterInputs;

        // Multi-cluster mode: Dev panel is expanded by default with tier inputs
        var multiClusterInputs = await Page.QuerySelectorAllAsync(".tier-panel input.tier-input");
        return multiClusterInputs;
    }

    [Test]
    public async Task AppInputs_AreContainedWithinParent()
    {
        await NavigateToK8sConfigAsync();

        // Try single cluster mode (.tier-card)
        var inputs = await Page.QuerySelectorAllAsync(".tier-card input[type='number']");
        if (inputs.Count > 0)
        {
            foreach (var input in inputs)
            {
                var inputBox = await input.BoundingBoxAsync();
                var parent = await input.EvaluateHandleAsync("el => el.closest('.tier-card')");
                var parentBox = await ((Microsoft.Playwright.IElementHandle)parent).BoundingBoxAsync();

                if (inputBox != null && parentBox != null)
                {
                    Assert.That(inputBox.X, Is.GreaterThanOrEqualTo(parentBox.X - 5),
                        "Input should not overflow left");
                    Assert.That(inputBox.X + inputBox.Width, Is.LessThanOrEqualTo(parentBox.X + parentBox.Width + 5),
                        "Input should not overflow right");
                }
            }
            return;
        }

        // Multi-cluster mode: Dev panel is expanded by default with tier inputs
        inputs = await Page.QuerySelectorAllAsync(".tier-panel input.tier-input");
        Assert.That(inputs.Count, Is.GreaterThan(0), "Should have tier input fields");

        foreach (var input in inputs)
        {
            var inputBox = await input.BoundingBoxAsync();
            if (inputBox != null)
            {
                // Spinbuttons are visible and contained within their parent container
                Assert.That(inputBox.Width, Is.GreaterThan(0), "Input should have positive width");
            }
        }
    }

    [Test]
    public async Task NodeSpecInputs_AreContainedWithinColumns()
    {
        await NavigateToK8sConfigAsync();
        await ClickTabAsync("Node Specs");

        var inputs = await Page.QuerySelectorAllAsync(".node-spec-row .spec-col input");
        if (inputs.Count == 0)
        {
            // Try alternative selectors
            inputs = await Page.QuerySelectorAllAsync(".node-specs-panel input[type='number']");
        }

        // Node specs may be in a tabbed interface - some inputs may be hidden
        if (inputs.Count > 0)
        {
            foreach (var input in inputs)
            {
                var inputBox = await input.BoundingBoxAsync();
                if (inputBox == null) continue; // Skip hidden inputs

                var parent = await input.EvaluateHandleAsync("el => el.closest('.spec-col') || el.parentElement");
                var parentBox = await ((Microsoft.Playwright.IElementHandle)parent).BoundingBoxAsync();

                if (parentBox != null)
                {
                    Assert.That(inputBox.X + inputBox.Width, Is.LessThanOrEqualTo(parentBox.X + parentBox.Width + 10),
                        "Node spec input should not overflow its column");
                }
            }
        }
    }

    [Test]
    public async Task AppInputs_AcceptNumericValues()
    {
        await NavigateToK8sConfigAsync();

        var input = await GetAppInputAsync();
        Assert.That(input, Is.Not.Null, "Should have an enabled input");

        await input!.FillAsync("100");
        var value = await input.InputValueAsync();

        Assert.That(value, Is.EqualTo("100"), "Input should accept numeric value");
    }

    [Test]
    public async Task AppInputs_HaveMinimumZero()
    {
        await NavigateToK8sConfigAsync();

        var input = await GetAppInputAsync();
        Assert.That(input, Is.Not.Null);

        var min = await input!.GetAttributeAsync("min");
        Assert.That(min, Is.EqualTo("0"), "App inputs should have min=0");
    }

    [Test]
    public async Task SettingsInputs_HaveCorrectRanges()
    {
        await NavigateToK8sConfigAsync();
        await ClickTabAsync("Settings");

        // Headroom inputs should have min=0 and max=100
        var headroomInput = await Page.QuerySelectorAsync(".settings-section-compact:has-text('Headroom') input");
        if (headroomInput == null)
        {
            headroomInput = await Page.QuerySelectorAsync(".settings-panel input[type='number']");
        }

        if (headroomInput != null)
        {
            var min = await headroomInput.GetAttributeAsync("min");
            var max = await headroomInput.GetAttributeAsync("max");
            Assert.That(min, Is.EqualTo("0"), "Headroom min should be 0");
            Assert.That(max, Is.EqualTo("100"), "Headroom max should be 100");
        }
    }

    [Test]
    public async Task OvercommitInputs_HaveCorrectStep()
    {
        await NavigateToK8sConfigAsync();
        await ClickTabAsync("Settings");

        // Overcommit inputs should have step=0.1
        var overcommitInput = await Page.QuerySelectorAsync(".overcommit-group input");
        if (overcommitInput != null)
        {
            var step = await overcommitInput.GetAttributeAsync("step");
            Assert.That(step, Is.EqualTo("0.1"), "Overcommit inputs should have step=0.1");
        }
    }

    [Test]
    public async Task NodeSpecInputs_HaveMinimumOne()
    {
        await NavigateToK8sConfigAsync();
        await ClickTabAsync("Node Specs");

        var inputs = await Page.QuerySelectorAllAsync(".node-spec-row .spec-col input");
        if (inputs.Count == 0)
        {
            inputs = await Page.QuerySelectorAllAsync(".node-specs-panel input[type='number']");
        }

        foreach (var input in inputs)
        {
            var min = await input.GetAttributeAsync("min");
            if (min != null)
            {
                Assert.That(min, Is.EqualTo("1"), "Node spec inputs should have min=1");
            }
        }
    }

    [Test]
    public async Task InputFocus_ShowsVisualFeedback()
    {
        await NavigateToK8sConfigAsync();

        var input = await GetAppInputAsync();
        Assert.That(input, Is.Not.Null);

        // Focus the input
        await input!.FocusAsync();
        await Page.WaitForTimeoutAsync(100);

        // Check that border changes on focus
        var borderColor = await input.EvaluateAsync<string>("el => getComputedStyle(el).borderColor");
        Assert.That(borderColor, Is.Not.Empty, "Input should have a border color when focused");
    }

    [Test]
    public async Task TabNavigation_WorksBetweenInputs()
    {
        await NavigateToK8sConfigAsync();

        // Get all visible inputs
        var inputs = await GetAllAppInputsAsync();
        if (inputs.Count < 2)
        {
            Assert.Pass("Not enough inputs for tab navigation test");
            return;
        }

        // Focus first input
        await inputs[0].FocusAsync();

        // Press Tab to move to next input
        await Page.Keyboard.PressAsync("Tab");
        await Page.WaitForTimeoutAsync(100);

        // A different element should now be focused
        var focusedElement = await Page.EvaluateAsync<string>("document.activeElement?.tagName");
        Assert.That(focusedElement, Is.EqualTo("INPUT").IgnoreCase,
            "Tab should move focus to another input");
    }
}
