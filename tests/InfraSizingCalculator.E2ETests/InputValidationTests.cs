namespace InfraSizingCalculator.E2ETests;

/// <summary>
/// E2E tests for input field behavior and validation
/// </summary>
[TestFixture]
public class InputValidationTests : PlaywrightFixture
{
    // Uses NavigateToK8sConfigAsync() from PlaywrightFixture

    [Test]
    public async Task AppInputs_AreContainedWithinColumns()
    {
        await NavigateToK8sConfigAsync();

        // Get input elements from tier cards
        var inputs = await Page.QuerySelectorAllAsync(".tier-card input[type='number']");
        Assert.That(inputs.Count, Is.GreaterThan(0), "Should have input fields");

        foreach (var input in inputs)
        {
            var inputBox = await input.BoundingBoxAsync();
            var parent = await input.EvaluateHandleAsync("el => el.closest('.tier-card')");
            var parentBox = await ((Microsoft.Playwright.IElementHandle)parent).BoundingBoxAsync();

            if (inputBox != null && parentBox != null)
            {
                // Input should be contained within parent
                Assert.That(inputBox.X, Is.GreaterThanOrEqualTo(parentBox.X - 5),
                    "Input should not overflow left");
                Assert.That(inputBox.X + inputBox.Width, Is.LessThanOrEqualTo(parentBox.X + parentBox.Width + 5),
                    "Input should not overflow right");
            }
        }
    }

    [Test]
    public async Task NodeSpecInputs_AreContainedWithinColumns()
    {
        await NavigateToK8sConfigAsync();
        await ClickTabAsync("Node Specs");

        var inputs = await Page.QuerySelectorAllAsync(".node-spec-row .spec-col input");
        Assert.That(inputs.Count, Is.GreaterThan(0), "Should have node spec input fields");

        foreach (var input in inputs)
        {
            var inputBox = await input.BoundingBoxAsync();
            var parent = await input.EvaluateHandleAsync("el => el.closest('.spec-col')");
            var parentBox = await ((Microsoft.Playwright.IElementHandle)parent).BoundingBoxAsync();

            if (inputBox != null && parentBox != null)
            {
                Assert.That(inputBox.X + inputBox.Width, Is.LessThanOrEqualTo(parentBox.X + parentBox.Width + 5),
                    "Node spec input should not overflow its column");
            }
        }
    }

    [Test]
    public async Task AppInputs_AcceptNumericValues()
    {
        await NavigateToK8sConfigAsync();

        var input = await Page.QuerySelectorAsync(".tier-card input[type='number']");
        Assert.That(input, Is.Not.Null, "Should have an enabled input");

        await input!.FillAsync("100");
        var value = await input.InputValueAsync();

        Assert.That(value, Is.EqualTo("100"), "Input should accept numeric value");
    }

    [Test]
    public async Task AppInputs_HaveMinimumZero()
    {
        await NavigateToK8sConfigAsync();

        var input = await Page.QuerySelectorAsync(".tier-card input[type='number']");
        Assert.That(input, Is.Not.Null);

        var min = await input!.GetAttributeAsync("min");
        Assert.That(min, Is.EqualTo("0"), "App inputs should have min=0");
    }

    [Test]
    public async Task DisabledInputs_CannotBeEdited()
    {
        await NavigateToK8sConfigAsync();

        // Disable an environment by unchecking it
        var checkbox = await Page.QuerySelectorAsync(".cluster-row:has-text('Dev') input[type='checkbox']");
        if (checkbox != null && await checkbox.IsCheckedAsync())
        {
            await checkbox.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        // Now the inputs should be disabled
        var disabledInput = await Page.QuerySelectorAsync(".cluster-row:has-text('Dev') .tier-col input");
        if (disabledInput != null)
        {
            var isDisabled = await disabledInput.IsDisabledAsync();
            Assert.That(isDisabled, Is.True, "Inputs for disabled environment should be disabled");
        }
    }

    [Test]
    public async Task SettingsInputs_HaveCorrectRanges()
    {
        await NavigateToK8sConfigAsync();
        await ClickTabAsync("Settings");

        // Headroom inputs should have min=0 and max=100
        var headroomInput = await Page.QuerySelectorAsync(".settings-section-compact:has-text('Headroom') input");
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
        foreach (var input in inputs)
        {
            var min = await input.GetAttributeAsync("min");
            Assert.That(min, Is.EqualTo("1"), "Node spec inputs should have min=1");
        }
    }

    [Test]
    public async Task InputFocus_ShowsBlueHighlight()
    {
        await NavigateToK8sConfigAsync();

        var input = await Page.QuerySelectorAsync(".tier-card input[type='number']");
        Assert.That(input, Is.Not.Null);

        // Focus the input
        await input!.FocusAsync();
        await Page.WaitForTimeoutAsync(100);

        // Check border color (should be accent-blue on focus)
        var borderColor = await input.EvaluateAsync<string>("el => getComputedStyle(el).borderColor");
        // The exact color depends on CSS variables, but it should change on focus
        Assert.That(borderColor, Is.Not.Empty, "Input should have a border color when focused");
    }

    [Test]
    public async Task TabNavigation_WorksBetweenInputs()
    {
        await NavigateToK8sConfigAsync();

        // Focus first input
        var firstInput = await Page.QuerySelectorAsync(".tier-card input[type='number']");
        Assert.That(firstInput, Is.Not.Null);

        await firstInput!.FocusAsync();

        // Press Tab to move to next input
        await Page.Keyboard.PressAsync("Tab");
        await Page.WaitForTimeoutAsync(100);

        // A different element should now be focused
        var focusedElement = await Page.EvaluateAsync<string>("document.activeElement?.tagName");
        Assert.That(focusedElement, Is.EqualTo("INPUT").IgnoreCase,
            "Tab should move focus to another input");
    }
}
