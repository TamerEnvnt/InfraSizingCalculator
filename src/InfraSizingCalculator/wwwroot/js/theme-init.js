// Theme initialization - must run before CSS loads
(function() {
    try {
        var saved = localStorage.getItem('infra-sizing-theme');
        var theme = saved || 'dark'; // Default to dark, not system preference
        document.documentElement.setAttribute('data-theme', theme);
        if (document.body) {
            document.body.setAttribute('data-theme', theme);
        }
        // Store the theme globally so Blazor components can access it
        window.__initialTheme = theme;
    } catch(e) {
        document.documentElement.setAttribute('data-theme', 'dark');
        window.__initialTheme = 'dark';
    }
})();

// After DOM is ready, also update app-container if it exists
document.addEventListener('DOMContentLoaded', function() {
    var theme = window.__initialTheme || 'dark';
    document.body.setAttribute('data-theme', theme);
    var appContainer = document.querySelector('.app-container');
    if (appContainer) {
        appContainer.setAttribute('data-theme', theme);
    }
});
