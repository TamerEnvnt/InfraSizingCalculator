using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Page object for modal dialogs - InfoModal, SaveScenarioModal, and generic modals.
/// </summary>
public class ModalPage
{
    private readonly IPage _page;
    private readonly int _defaultTimeout;

    public ModalPage(IPage page, int defaultTimeout = 15000)
    {
        _page = page;
        _defaultTimeout = defaultTimeout;
    }

    #region Locators

    // Generic modal elements
    private const string ModalOverlay = ".modal-overlay";
    private const string ModalContent = ".modal-content";
    private const string ModalHeader = ".modal-header";
    private const string ModalBody = ".modal-body";
    private const string ModalFooter = ".modal-footer";
    private const string ModalClose = ".modal-close";
    private const string ModalConfirmButton = ".modal-btn.primary, .modal-footer button.primary";
    private const string ModalCancelButton = ".modal-btn:not(.primary), .modal-footer button:not(.primary)";

    // Info modal specific
    private const string InfoModalTitle = ".modal-header h2";
    private const string InfoModalContent = ".modal-body.info-modal, .modal-body";

    // Save scenario modal specific
    private const string SaveScenarioModal = ".save-scenario-modal";
    private const string ScenarioNameInput = "input[placeholder*='name'], input#scenarioName";
    private const string ScenarioDescriptionInput = "textarea[placeholder*='description'], textarea#description";
    private const string ScenarioTagsInput = "input[placeholder*='tag'], input#tags";
    private const string SaveAsDraftCheckbox = "input[type='checkbox']";
    private const string SaveButton = "button:has-text('Save')";
    private const string ScenarioPreview = ".scenario-preview, .preview-section";

    // Info buttons that trigger modals
    private const string InfoButton = ".info-button, .info-icon, button.info-btn";

    #endregion

    #region Modal Visibility

    public async Task<bool> IsModalVisibleAsync()
    {
        var overlay = await _page.QuerySelectorAsync(ModalOverlay);
        if (overlay == null) return false;

        var isVisible = await overlay.IsVisibleAsync();
        var hasVisibleClass = await overlay.EvaluateAsync<bool>("el => el.classList.contains('visible') || getComputedStyle(el).display !== 'none'");
        return isVisible || hasVisibleClass;
    }

    public async Task<bool> IsModalContentVisibleAsync()
    {
        var content = await _page.QuerySelectorAsync(ModalContent);
        return content != null && await content.IsVisibleAsync();
    }

    public async Task<bool> IsSaveScenarioModalVisibleAsync()
    {
        var modal = await _page.QuerySelectorAsync(SaveScenarioModal);
        return modal != null && await modal.IsVisibleAsync();
    }

    public async Task WaitForModalToAppearAsync()
    {
        await _page.WaitForSelectorAsync(ModalOverlay, new() { State = WaitForSelectorState.Visible, Timeout = _defaultTimeout });
    }

    public async Task WaitForModalToDisappearAsync()
    {
        await _page.WaitForSelectorAsync(ModalOverlay, new() { State = WaitForSelectorState.Hidden, Timeout = _defaultTimeout });
    }

    #endregion

    #region Modal Content

    public async Task<string?> GetModalTitleAsync()
    {
        var title = await _page.QuerySelectorAsync(InfoModalTitle);
        return title != null ? await title.TextContentAsync() : null;
    }

    public async Task<string?> GetModalBodyTextAsync()
    {
        var body = await _page.QuerySelectorAsync(ModalBody);
        return body != null ? await body.TextContentAsync() : null;
    }

    public async Task<bool> ModalContainsTextAsync(string text)
    {
        var bodyText = await GetModalBodyTextAsync();
        return bodyText?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    #endregion

    #region Close Actions

    public async Task CloseModalViaXButtonAsync()
    {
        var closeButton = await _page.QuerySelectorAsync(ModalClose);
        if (closeButton != null && await closeButton.IsVisibleAsync())
        {
            await closeButton.ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    public async Task CloseModalViaOverlayAsync()
    {
        // Click on the overlay (outside the modal content)
        var overlay = await _page.QuerySelectorAsync(ModalOverlay);
        if (overlay != null)
        {
            // Click at overlay position that's outside the modal content
            var box = await overlay.BoundingBoxAsync();
            if (box != null)
            {
                // Click near the edge of the overlay
                await _page.Mouse.ClickAsync(box.X + 10, box.Y + 10);
                await WaitForStabilityAsync();
            }
        }
    }

    public async Task CloseModalViaEscapeAsync()
    {
        await _page.Keyboard.PressAsync("Escape");
        await WaitForStabilityAsync();
    }

    public async Task ClickCancelButtonAsync()
    {
        var cancelButton = await _page.QuerySelectorAsync(ModalCancelButton);
        if (cancelButton != null && await cancelButton.IsVisibleAsync())
        {
            await cancelButton.ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    public async Task ClickConfirmButtonAsync()
    {
        var confirmButton = await _page.QuerySelectorAsync(ModalConfirmButton);
        if (confirmButton != null && await confirmButton.IsVisibleAsync())
        {
            await confirmButton.ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    #endregion

    #region Info Button Actions

    public async Task<int> GetInfoButtonCountAsync()
    {
        var buttons = await _page.QuerySelectorAllAsync(InfoButton);
        return buttons.Count;
    }

    public async Task ClickInfoButtonAsync(int index = 0)
    {
        var buttons = await _page.QuerySelectorAllAsync(InfoButton);
        if (index < buttons.Count)
        {
            await buttons[index].ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    public async Task ClickInfoButtonNearTextAsync(string nearText)
    {
        // Find info button near specific text
        var button = await _page.QuerySelectorAsync($":text('{nearText}') ~ {InfoButton}, {InfoButton}:near(:text('{nearText}'))");
        if (button != null)
        {
            await button.ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    #endregion

    #region Save Scenario Modal

    public async Task FillScenarioNameAsync(string name)
    {
        var input = await _page.QuerySelectorAsync(ScenarioNameInput);
        if (input != null)
        {
            await input.FillAsync(name);
        }
    }

    public async Task FillScenarioDescriptionAsync(string description)
    {
        var input = await _page.QuerySelectorAsync(ScenarioDescriptionInput);
        if (input != null)
        {
            await input.FillAsync(description);
        }
    }

    public async Task FillScenarioTagsAsync(string tags)
    {
        var input = await _page.QuerySelectorAsync(ScenarioTagsInput);
        if (input != null)
        {
            await input.FillAsync(tags);
        }
    }

    public async Task ToggleSaveAsDraftAsync()
    {
        var checkbox = await _page.QuerySelectorAsync(SaveAsDraftCheckbox);
        if (checkbox != null)
        {
            await checkbox.ClickAsync();
        }
    }

    public async Task ClickSaveButtonAsync()
    {
        var button = await _page.QuerySelectorAsync(SaveButton);
        if (button != null && await button.IsEnabledAsync())
        {
            await button.ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    public async Task<bool> IsSaveButtonEnabledAsync()
    {
        var button = await _page.QuerySelectorAsync(SaveButton);
        return button != null && await button.IsEnabledAsync();
    }

    public async Task<bool> HasScenarioPreviewAsync()
    {
        var preview = await _page.QuerySelectorAsync(ScenarioPreview);
        return preview != null && await preview.IsVisibleAsync();
    }

    public async Task SaveScenarioAsync(string name, string? description = null, string? tags = null)
    {
        await FillScenarioNameAsync(name);

        if (!string.IsNullOrEmpty(description))
        {
            await FillScenarioDescriptionAsync(description);
        }

        if (!string.IsNullOrEmpty(tags))
        {
            await FillScenarioTagsAsync(tags);
        }

        await ClickSaveButtonAsync();
    }

    #endregion

    #region Focus Verification

    public async Task<bool> IsFocusTrapActiveAsync()
    {
        // Check if focus is within the modal
        var activeElement = await _page.EvaluateAsync<string>("() => document.activeElement?.closest('.modal-content, .save-scenario-modal') ? 'inside' : 'outside'");
        return activeElement == "inside";
    }

    public async Task<bool> CanTabThroughModalElementsAsync()
    {
        // Press Tab a few times and verify focus stays in modal
        for (int i = 0; i < 5; i++)
        {
            await _page.Keyboard.PressAsync("Tab");
            await _page.WaitForTimeoutAsync(100);

            var isInside = await IsFocusTrapActiveAsync();
            if (!isInside && await IsModalVisibleAsync())
            {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region Complete Interactions

    public async Task OpenInfoModalAndVerifyAsync()
    {
        // Click first info button
        await ClickInfoButtonAsync(0);

        // Wait for modal
        await WaitForModalToAppearAsync();

        // Verify modal is visible
        Assert.That(await IsModalVisibleAsync(), Is.True, "Info modal should be visible");
    }

    public async Task<bool> OpenCloseInfoModalAsync()
    {
        await ClickInfoButtonAsync(0);
        await WaitForModalToAppearAsync();

        var wasVisible = await IsModalVisibleAsync();
        await CloseModalViaXButtonAsync();

        await _page.WaitForTimeoutAsync(500);
        var isClosedNow = !(await IsModalVisibleAsync());

        return wasVisible && isClosedNow;
    }

    #endregion

    #region Helpers

    private async Task WaitForStabilityAsync()
    {
        await _page.WaitForTimeoutAsync(300);
    }

    #endregion
}
