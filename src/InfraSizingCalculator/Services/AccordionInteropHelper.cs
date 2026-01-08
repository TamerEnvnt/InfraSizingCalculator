using Microsoft.JSInterop;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Static JS interop helper for accordion panels.
/// Provides a workaround for Playwright/E2E tests where normal Blazor click events don't work.
/// </summary>
public static class AccordionInteropHelper
{
    private static readonly Dictionary<string, Func<Task>> _clickHandlers = new();

    /// <summary>
    /// Register a click handler for an accordion panel key.
    /// </summary>
    public static void RegisterClickHandler(string key, Func<Task> handler)
    {
        _clickHandlers[key] = handler;
        Console.WriteLine($"[AccordionInteropHelper] Registered handler for key={key}");
    }

    /// <summary>
    /// Unregister a click handler.
    /// </summary>
    public static void UnregisterClickHandler(string key)
    {
        _clickHandlers.Remove(key);
        Console.WriteLine($"[AccordionInteropHelper] Unregistered handler for key={key}");
    }

    /// <summary>
    /// Called from JavaScript to toggle an accordion panel.
    /// </summary>
    [JSInvokable("ToggleAccordionPanel")]
    public static async Task ToggleAccordionPanel(string key)
    {
        Console.WriteLine($"[AccordionInteropHelper] ToggleAccordionPanel called with key={key}");

        if (_clickHandlers.TryGetValue(key, out var handler))
        {
            Console.WriteLine($"[AccordionInteropHelper] Found handler for key={key}, invoking...");
            try
            {
                await handler();
                Console.WriteLine($"[AccordionInteropHelper] Handler invoked successfully for key={key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AccordionInteropHelper] Handler error for key={key}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"[AccordionInteropHelper] No handler found for key={key}. Available keys: {string.Join(", ", _clickHandlers.Keys)}");
        }
    }
}
