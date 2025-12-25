// Infrastructure Sizing Calculator - Site JavaScript

/**
 * Download a file with the given content
 * @param {string} filename - The name of the file to download
 * @param {string} content - The file content (can be base64 for binary files)
 * @param {string} contentType - The MIME type of the file
 */
window.downloadFile = function (filename, content, contentType) {
    // Create a blob from the content
    let blob;

    // Check if it's base64 encoded (for binary files like Excel)
    if (contentType.includes('spreadsheet') || contentType.includes('excel')) {
        // Decode base64 to binary
        const byteCharacters = atob(content);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        blob = new Blob([byteArray], { type: contentType });
    } else {
        // Text content
        blob = new Blob([content], { type: contentType });
    }

    // Create download link
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;

    // Trigger download
    document.body.appendChild(link);
    link.click();

    // Cleanup
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

/**
 * Open content in a new window (for HTML diagrams)
 * @param {string} content - The HTML content to display
 * @param {string} title - The window title
 */
window.openInNewWindow = function (content, title) {
    const newWindow = window.open('', '_blank');
    if (newWindow) {
        newWindow.document.write(content);
        newWindow.document.title = title || 'Infrastructure Diagram';
        newWindow.document.close();
    }
};

/**
 * Scroll to an element by ID with smooth scrolling
 * @param {string} elementId - The ID of the element to scroll to
 */
window.scrollToElement = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};

/**
 * Download a file from base64 content
 * @param {string} filename - The name of the file
 * @param {string} base64Content - Base64 encoded content
 * @param {string} contentType - MIME type
 */
window.downloadFileBase64 = function (filename, base64Content, contentType) {
    const byteCharacters = atob(base64Content);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });

    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

/**
 * Download a file from byte array (used by Blazor for binary files like Excel/PDF)
 * @param {string} filename - The name of the file
 * @param {Uint8Array} byteArray - The byte array from Blazor
 * @param {string} contentType - MIME type
 */
window.downloadFileBytes = function (filename, byteArray, contentType) {
    const blob = new Blob([byteArray], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// ===== Settings Persistence & URL Sharing =====

/**
 * Check if localStorage is available
 * @returns {boolean} True if localStorage is available
 */
window.localStorageAvailable = function () {
    try {
        const test = '__localStorage_test__';
        localStorage.setItem(test, test);
        localStorage.removeItem(test);
        return true;
    } catch (e) {
        return false;
    }
};

/**
 * Copy text to clipboard
 * @param {string} text - The text to copy
 * @returns {Promise<boolean>} True if successful
 */
window.copyToClipboard = async function (text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (e) {
        // Fallback for older browsers
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.left = '-9999px';
        document.body.appendChild(textArea);
        textArea.select();
        try {
            document.execCommand('copy');
            document.body.removeChild(textArea);
            return true;
        } catch (err) {
            document.body.removeChild(textArea);
            return false;
        }
    }
};

/**
 * Get URL query parameter
 * @param {string} name - Parameter name
 * @returns {string|null} Parameter value or null
 */
window.getUrlParameter = function (name) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(name);
};

/**
 * Update URL with a parameter without page reload
 * @param {string} name - Parameter name
 * @param {string} value - Parameter value (null to remove)
 */
window.setUrlParameter = function (name, value) {
    const url = new URL(window.location.href);
    if (value === null || value === '') {
        url.searchParams.delete(name);
    } else {
        url.searchParams.set(name, value);
    }
    window.history.replaceState({}, '', url.toString());
};

/**
 * Get the full current URL
 * @returns {string} Current URL
 */
window.getCurrentUrl = function () {
    return window.location.href;
};

/**
 * Get the base URL (without query parameters)
 * @returns {string} Base URL
 */
window.getBaseUrl = function () {
    return window.location.origin + window.location.pathname;
};

// ===== Theme Management =====

/**
 * Set the theme attribute on the document
 * @param {string} theme - Theme name (dark/light)
 */
window.setTheme = function (theme) {
    document.documentElement.setAttribute('data-theme', theme);
    document.body.setAttribute('data-theme', theme);
    // Also update app-container which has its own data-theme attribute
    var appContainer = document.querySelector('.app-container');
    if (appContainer) {
        appContainer.setAttribute('data-theme', theme);
    }
    // Save to localStorage for persistence
    try {
        localStorage.setItem('infra-sizing-theme', theme);
    } catch (e) {
        // localStorage may not be available
    }
};

/**
 * Get the current theme from document
 * @returns {string} Current theme
 */
window.getTheme = function () {
    return document.documentElement.getAttribute('data-theme') || 'dark';
};

/**
 * Detect system color scheme preference
 * @returns {string} 'dark' or 'light'
 */
window.getSystemTheme = function () {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches) {
        return 'light';
    }
    return 'dark';
};

/**
 * Add listener for system theme changes
 * @param {DotNetObjectReference} dotnetHelper - .NET object reference
 * @param {string} methodName - Method to call on theme change
 */
window.addThemeChangeListener = function (dotnetHelper, methodName) {
    if (window.matchMedia) {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
            const newTheme = e.matches ? 'dark' : 'light';
            dotnetHelper.invokeMethodAsync(methodName, newTheme);
        });
    }
};
