// Accordion click interop - bypasses Blazor's event delegation for Playwright compatibility
console.log('[Accordion-Interop] Script loaded');

// Store dotNetRefs by key for later use
window._accordionRefs = window._accordionRefs || {};

// Pending click queue - Blazor polls this to handle clicks that JS interop can't process
window._pendingAccordionClicks = window._pendingAccordionClicks || [];

window.registerAccordionClickHandler = function(key, dotNetRef) {
    console.log('[Accordion-Interop] registerAccordionClickHandler called for key=' + key);

    // Store the reference for potential re-use
    window._accordionRefs[key] = dotNetRef;

    // Small delay to ensure DOM is ready
    setTimeout(function() {
        const btn = document.querySelector('[data-accordion-key="' + key + '"]');
        console.log('[Accordion-Interop] Looking for button with key=' + key + ', found=' + !!btn);

        if (btn && !btn._accordionClickRegistered) {
            btn._accordionClickRegistered = true;
            btn.addEventListener('click', function(e) {
                console.log('[Accordion-Interop] Click detected for key=' + key);

                // Queue the click for Blazor to pick up via polling
                // This bypasses the JS->.NET interop which doesn't work with Playwright
                window._pendingAccordionClicks.push(key);
                console.log('[Accordion-Interop] Queued click for key=' + key + ', queue length=' + window._pendingAccordionClicks.length);
            });
            console.log('[Accordion-Interop] Registered click handler for key=' + key);
        } else if (!btn) {
            console.warn('[Accordion-Interop] Button not found for key=' + key);
        } else {
            console.log('[Accordion-Interop] Handler already registered for key=' + key);
        }
    }, 100);
};

// Called by Blazor to check for pending clicks for a specific key (Blazor-initiated JS interop works fine)
window.getPendingAccordionClick = function() {
    if (window._pendingAccordionClicks.length > 0) {
        // Return the first pending key but DON'T remove it yet - caller decides
        var key = window._pendingAccordionClicks[0];
        console.log('[Accordion-Interop] Peeking pending click: key=' + key);
        return key;
    }
    return null;
};

// Called by Blazor to check if there's a pending click for a SPECIFIC key
window.hasPendingAccordionClickForKey = function(key) {
    var hasPending = window._pendingAccordionClicks.indexOf(key) !== -1;
    if (hasPending) {
        console.log('[Accordion-Interop] Found pending click for key=' + key);
    }
    return hasPending;
};

// Called by Blazor when it has handled a pending click
window.consumePendingAccordionClick = function(key) {
    var index = window._pendingAccordionClicks.indexOf(key);
    if (index !== -1) {
        window._pendingAccordionClicks.splice(index, 1);
        console.log('[Accordion-Interop] Consumed pending click for key=' + key);
        return true;
    }
    return false;
};

// Direct trigger function (can be called from Playwright via page.evaluate)
window.triggerAccordionClick = function(key) {
    console.log('[Accordion-Interop] triggerAccordionClick called for key=' + key);
    window._pendingAccordionClicks.push(key);
    return true;
};

// Direct Blazor state manipulation - for Playwright when event delegation fails
// This function directly calls into Blazor's circuit to toggle a panel
window.forceExpandAccordionPanel = async function(key) {
    console.log('[Accordion-Interop] forceExpandAccordionPanel called for key=' + key);

    // Find the accordion header button and force-click it via Blazor
    var btn = document.querySelector('[data-accordion-key="' + key + '"]');
    if (!btn) {
        console.error('[Accordion-Interop] Button not found for key=' + key);
        return false;
    }

    // Dispatch a proper click event that Blazor might catch
    var clickEvent = new MouseEvent('click', {
        bubbles: true,
        cancelable: true,
        view: window
    });
    btn.dispatchEvent(clickEvent);

    // Also queue for polling as backup
    window._pendingAccordionClicks.push(key);

    console.log('[Accordion-Interop] Dispatched click and queued for key=' + key);
    return true;
};

// Direct panel expansion function for E2E tests
// Call this from Playwright: await page.EvaluateAsync("window.expandAccordionPanel('Test')");
window.expandAccordionPanel = function(key) {
    console.log('[Accordion-Interop] expandAccordionPanel called for key=' + key);

    var btn = document.querySelector('[data-accordion-key="' + key + '"]');
    if (!btn) {
        console.log('[Accordion-Interop] Button not found for key=' + key);
        return false;
    }

    var panel = btn.closest('.h-accordion-panel');
    if (!panel) {
        console.log('[Accordion-Interop] Panel not found for key=' + key);
        return false;
    }

    // In single-expand mode, collapse other panels first
    var accordion = panel.closest('.h-accordion');
    if (accordion && accordion.classList.contains('single-expand')) {
        accordion.querySelectorAll('.h-accordion-panel.expanded').forEach(function(p) {
            if (p !== panel) {
                p.classList.remove('expanded');
            }
        });
    }

    // Expand this panel
    panel.classList.add('expanded');
    console.log('[Accordion-Interop] Panel ' + key + ' expanded');
    return true;
};

// Collapse a specific panel
window.collapseAccordionPanel = function(key) {
    var btn = document.querySelector('[data-accordion-key="' + key + '"]');
    if (!btn) return false;

    var panel = btn.closest('.h-accordion-panel');
    if (!panel) return false;

    panel.classList.remove('expanded');
    return true;
};

// Check if a panel is expanded
window.isAccordionPanelExpanded = function(key) {
    var btn = document.querySelector('[data-accordion-key="' + key + '"]');
    if (!btn) return false;

    var panel = btn.closest('.h-accordion-panel');
    if (!panel) return false;

    return panel.classList.contains('expanded');
};
