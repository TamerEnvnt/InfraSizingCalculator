/**
 * Theme Manager - Single Source of Truth for Theme State
 * Handles dark/light theme switching with proper persistence
 */

const ThemeManager = {
    STORAGE_KEY: 'infra-sizing-theme',
    DEFAULT_THEME: 'dark',

    /**
     * Initialize theme on page load
     * Should be called as early as possible to prevent flash
     */
    init() {
        const saved = this.getSaved();
        this.apply(saved, false); // Don't persist on init
    },

    /**
     * Get saved theme from localStorage
     */
    getSaved() {
        try {
            return localStorage.getItem(this.STORAGE_KEY) || this.DEFAULT_THEME;
        } catch (e) {
            return this.DEFAULT_THEME;
        }
    },

    /**
     * Get current theme from DOM
     */
    get() {
        return document.documentElement.getAttribute('data-theme') || this.DEFAULT_THEME;
    },

    /**
     * Apply theme to DOM and optionally persist
     * @param {string} theme - 'light' or 'dark'
     * @param {boolean} persist - Whether to save to localStorage
     */
    apply(theme, persist = true) {
        // Validate theme
        if (theme !== 'light' && theme !== 'dark') {
            theme = this.DEFAULT_THEME;
        }

        // Apply to document root - single source of truth
        document.documentElement.setAttribute('data-theme', theme);

        // Also set on body for any legacy selectors
        if (document.body) {
            document.body.setAttribute('data-theme', theme);
        }

        // Persist to localStorage
        if (persist) {
            try {
                localStorage.setItem(this.STORAGE_KEY, theme);
            } catch (e) {
                console.warn('Could not save theme preference:', e);
            }
        }

        // Dispatch event for Blazor and other listeners
        window.dispatchEvent(new CustomEvent('themechange', {
            detail: { theme: theme }
        }));

        return theme;
    },

    /**
     * Toggle between light and dark themes
     */
    toggle() {
        const current = this.get();
        const newTheme = current === 'dark' ? 'light' : 'dark';
        return this.apply(newTheme);
    },

    /**
     * Set theme to dark
     */
    setDark() {
        return this.apply('dark');
    },

    /**
     * Set theme to light
     */
    setLight() {
        return this.apply('light');
    },

    /**
     * Check if current theme is dark
     */
    isDark() {
        return this.get() === 'dark';
    },

    /**
     * Check if current theme is light
     */
    isLight() {
        return this.get() === 'light';
    },

    /**
     * Use system preference (prefers-color-scheme)
     */
    useSystemPreference() {
        const prefersDark = window.matchMedia &&
                           window.matchMedia('(prefers-color-scheme: dark)').matches;
        return this.apply(prefersDark ? 'dark' : 'light');
    },

    /**
     * Listen for system theme changes
     * @param {boolean} enabled - Whether to follow system preference
     */
    followSystem(enabled) {
        if (!window.matchMedia) return;

        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

        if (enabled) {
            const handler = (e) => {
                this.apply(e.matches ? 'dark' : 'light');
            };
            mediaQuery.addEventListener('change', handler);
            // Store handler for potential removal
            this._systemHandler = handler;
            // Apply current system preference
            this.useSystemPreference();
        } else if (this._systemHandler) {
            mediaQuery.removeEventListener('change', this._systemHandler);
            delete this._systemHandler;
        }
    }
};

// Initialize immediately
ThemeManager.init();

// Re-apply after DOM is ready (in case body wasn't available)
document.addEventListener('DOMContentLoaded', () => {
    const theme = ThemeManager.getSaved();
    document.body.setAttribute('data-theme', theme);
});

// Expose for Blazor interop
window.ThemeManager = ThemeManager;

// Legacy function names for backward compatibility
window.getTheme = () => ThemeManager.get();
window.setTheme = (theme) => ThemeManager.apply(theme);
window.toggleTheme = () => ThemeManager.toggle();
