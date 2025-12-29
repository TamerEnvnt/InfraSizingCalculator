/**
 * Interactive Navigation Component for v0.4.2 Wireframes
 * Adds flow-aware navigation to wireframe pages
 * Uses safe DOM methods - no innerHTML
 */

// Define all user flows with their screen sequences
const FLOWS = {
    guest: {
        name: 'Guest User Flow',
        color: '#3fb950',
        screens: [
            { id: '00-landing-guest', name: 'Landing (Guest)', file: '00-landing-guest.html' },
            { id: '02-configure-guest', name: 'Configure', file: '02-configure-guest.html' },
            { id: '04-apps-panel-guest', name: 'Apps Panel', file: '04-apps-panel-guest.html' },
            { id: '05-node-specs-guest', name: 'Node Specs', file: '05-node-specs-guest.html' },
            { id: '06-pricing-panel-guest', name: 'Pricing', file: '06-pricing-panel-guest.html' },
            { id: '07-growth-panel-guest', name: 'Growth', file: '07-growth-panel-guest.html' },
            { id: '01-results-guest', name: 'Results (Guest)', file: '01-results-guest.html' },
            { id: '09-export-guest', name: 'Export', file: '09-export-guest.html' }
        ]
    },
    auth: {
        name: 'Authentication Flow',
        color: '#58a6ff',
        screens: [
            { id: '00-landing-guest', name: 'Landing', file: '00-landing-guest.html' },
            { id: '11-login', name: 'Login', file: '11-login.html' },
            { id: '01-dashboard', name: 'Dashboard', file: '01-dashboard.html' }
        ]
    },
    register: {
        name: 'Registration Flow',
        color: '#58a6ff',
        screens: [
            { id: '00-landing-guest', name: 'Landing', file: '00-landing-guest.html' },
            { id: '12-register', name: 'Register', file: '12-register.html' },
            { id: '01-dashboard', name: 'Dashboard', file: '01-dashboard.html' }
        ]
    },
    dashboard: {
        name: 'Dashboard Flow',
        color: '#f0883e',
        screens: [
            { id: '01-dashboard', name: 'Dashboard', file: '01-dashboard.html' },
            { id: '02-dashboard-with-panel', name: 'Config Panel', file: '02-dashboard-with-panel.html' },
            { id: '04-apps-panel', name: 'Apps Panel', file: '04-apps-panel.html' },
            { id: '05-node-specs-panel', name: 'Node Specs', file: '05-node-specs-panel.html' },
            { id: '06-pricing-panel', name: 'Pricing', file: '06-pricing-panel.html' },
            { id: '07-growth-panel', name: 'Growth', file: '07-growth-panel.html' }
        ]
    },
    scenarios: {
        name: 'Scenarios Flow',
        color: '#a371f7',
        screens: [
            { id: '01-dashboard', name: 'Dashboard', file: '01-dashboard.html' },
            { id: '08-scenarios', name: 'Scenarios', file: '08-scenarios.html' },
            { id: '03-comparison-dashboard', name: 'Compare', file: '03-comparison-dashboard.html' }
        ]
    },
    export: {
        name: 'Export Flow',
        color: '#f778ba',
        screens: [
            { id: '01-dashboard', name: 'Dashboard', file: '01-dashboard.html' },
            { id: '09-export-modal', name: 'Export Modal', file: '09-export-modal.html' }
        ]
    },
    error: {
        name: 'Error States',
        color: '#f85149',
        screens: [
            { id: 'error-01-api-failure', name: 'API Failure', file: 'error-01-api-failure.html' },
            { id: 'error-02-validation', name: 'Validation', file: 'error-02-validation.html' },
            { id: 'error-03-session-timeout', name: 'Session Timeout', file: 'error-03-session-timeout.html' },
            { id: 'error-04-empty-scenarios', name: 'Empty Scenarios', file: 'error-04-empty-scenarios.html' },
            { id: 'error-05-export-failed', name: 'Export Failed', file: 'error-05-export-failed.html' }
        ]
    },
    loading: {
        name: 'Loading States',
        color: '#58a6ff',
        screens: [
            { id: 'loading-01-dashboard-skeleton', name: 'Dashboard Skeleton', file: 'loading-01-dashboard-skeleton.html' },
            { id: 'loading-02-panel-skeleton', name: 'Panel Skeleton', file: 'loading-02-panel-skeleton.html' },
            { id: 'loading-03-calculating', name: 'Calculating', file: 'loading-03-calculating.html' },
            { id: 'loading-04-charts', name: 'Charts Loading', file: 'loading-04-charts.html' }
        ]
    },
    mobile: {
        name: 'Mobile Screens',
        color: '#79c0ff',
        screens: [
            { id: 'mobile-01-dashboard', name: 'Dashboard', file: 'mobile-01-dashboard.html' },
            { id: 'mobile-02-fab-expanded', name: 'FAB Menu', file: 'mobile-02-fab-expanded.html' },
            { id: 'mobile-03-fullscreen-panel', name: 'Full Panel', file: 'mobile-03-fullscreen-panel.html' },
            { id: 'mobile-04-scenarios', name: 'Scenarios', file: 'mobile-04-scenarios.html' },
            { id: 'mobile-05-error', name: 'Error', file: 'mobile-05-error.html' },
            { id: 'mobile-06-nav-menu', name: 'Nav Menu', file: 'mobile-06-nav-menu.html' }
        ]
    },
    micro: {
        name: 'Micro-interactions',
        color: '#d2a8ff',
        screens: [
            { id: 'micro-01-hover-effects', name: 'Hover Effects', file: 'micro-01-hover-effects.html' },
            { id: 'micro-02-success-progress', name: 'Success/Progress', file: 'micro-02-success-progress.html' }
        ]
    }
};

// Get current page ID from filename
function getCurrentPageId() {
    const path = window.location.pathname;
    const filename = path.substring(path.lastIndexOf('/') + 1);
    return filename.replace('.html', '');
}

// Find which flows contain the current page
function findCurrentFlows(pageId) {
    const flows = [];
    for (const [flowKey, flow] of Object.entries(FLOWS)) {
        const screenIndex = flow.screens.findIndex(s => s.id === pageId);
        if (screenIndex !== -1) {
            flows.push({
                key: flowKey,
                name: flow.name,
                color: flow.color,
                screens: flow.screens,
                currentIndex: screenIndex
            });
        }
    }
    return flows;
}

// Create element helper
function createElement(tag, className, textContent) {
    const el = document.createElement(tag);
    if (className) el.className = className;
    if (textContent) el.textContent = textContent;
    return el;
}

// Add styles to document
function addStyles() {
    const style = document.createElement('style');
    style.textContent = `
        #wireframe-nav {
            position: fixed;
            top: 0;
            right: 0;
            width: 280px;
            max-height: 100vh;
            background: #0d1117;
            border-left: 1px solid #30363d;
            z-index: 9999;
            overflow-y: auto;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            transition: transform 0.2s;
        }
        #wireframe-nav.collapsed { transform: translateX(100%); }
        #wireframe-nav .nav-header {
            padding: 16px;
            background: #161b22;
            border-bottom: 1px solid #30363d;
            position: sticky;
            top: 0;
        }
        #wireframe-nav .nav-title {
            font-size: 14px;
            font-weight: 600;
            color: #c9d1d9;
            margin-bottom: 8px;
        }
        #wireframe-nav .nav-current {
            font-size: 12px;
            color: #8b949e;
        }
        #wireframe-nav .nav-current-id {
            color: #58a6ff;
            font-weight: 500;
        }
        #wireframe-nav .nav-home {
            display: block;
            padding: 10px 16px;
            background: #21262d;
            color: #58a6ff;
            text-decoration: none;
            font-size: 13px;
            font-weight: 500;
            border-bottom: 1px solid #30363d;
        }
        #wireframe-nav .nav-home:hover { background: #30363d; }
        #wireframe-nav .flow-section { border-bottom: 1px solid #30363d; }
        #wireframe-nav .flow-header {
            padding: 12px 16px;
            background: #161b22;
            display: flex;
            align-items: center;
            gap: 8px;
            cursor: pointer;
        }
        #wireframe-nav .flow-header:hover { background: #1c2128; }
        #wireframe-nav .flow-dot {
            width: 10px;
            height: 10px;
            border-radius: 50%;
        }
        #wireframe-nav .flow-name {
            font-size: 13px;
            font-weight: 500;
            color: #c9d1d9;
            flex: 1;
        }
        #wireframe-nav .flow-arrow {
            color: #6e7681;
            font-size: 12px;
            transition: transform 0.2s;
        }
        #wireframe-nav .flow-section.expanded .flow-arrow { transform: rotate(90deg); }
        #wireframe-nav .flow-screens { display: none; padding: 8px 0; }
        #wireframe-nav .flow-section.expanded .flow-screens { display: block; }
        #wireframe-nav .flow-screen {
            display: flex;
            align-items: center;
            padding: 8px 16px 8px 34px;
            text-decoration: none;
            color: #8b949e;
            font-size: 12px;
            transition: all 0.15s;
        }
        #wireframe-nav .flow-screen:hover {
            background: #21262d;
            color: #c9d1d9;
        }
        #wireframe-nav .flow-screen.current {
            background: rgba(88, 166, 255, 0.1);
            color: #58a6ff;
            font-weight: 500;
        }
        #wireframe-nav .screen-marker {
            width: 6px;
            height: 6px;
            border-radius: 50%;
            background: #30363d;
            margin-right: 10px;
        }
        #wireframe-nav .flow-screen.current .screen-marker { background: #58a6ff; }
        #wireframe-nav .nav-actions {
            padding: 16px;
            display: flex;
            gap: 8px;
        }
        #wireframe-nav .nav-btn {
            flex: 1;
            padding: 10px 12px;
            border-radius: 6px;
            font-size: 12px;
            font-weight: 500;
            text-decoration: none;
            text-align: center;
            transition: all 0.15s;
        }
        #wireframe-nav .nav-btn.prev {
            background: #21262d;
            color: #c9d1d9;
            border: 1px solid #30363d;
        }
        #wireframe-nav .nav-btn.prev:hover { background: #30363d; }
        #wireframe-nav .nav-btn.next {
            background: #238636;
            color: white;
            border: 1px solid #238636;
        }
        #wireframe-nav .nav-btn.next:hover { background: #2ea043; }
        #wireframe-nav .nav-btn.disabled {
            opacity: 0.5;
            pointer-events: none;
        }
        .wireframe-toggle-btn {
            position: fixed;
            top: 10px;
            right: 290px;
            width: 36px;
            height: 36px;
            background: #161b22;
            border: 1px solid #30363d;
            border-radius: 6px;
            color: #8b949e;
            cursor: pointer;
            font-size: 16px;
            z-index: 10000;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: right 0.2s;
        }
        .wireframe-toggle-btn:hover {
            background: #21262d;
            color: #c9d1d9;
        }
        .wireframe-toggle-btn.collapsed { right: 10px; }
    `;
    document.head.appendChild(style);
}

// Create flow section using DOM methods
function createFlowSection(flow, currentPageId) {
    const isExpanded = flow.screens.some(s => s.id === currentPageId);

    const section = createElement('div', 'flow-section' + (isExpanded ? ' expanded' : ''));

    // Header
    const header = createElement('div', 'flow-header');
    const dot = createElement('span', 'flow-dot');
    dot.style.background = flow.color;
    const name = createElement('span', 'flow-name', flow.name);
    const arrow = createElement('span', 'flow-arrow', '\u25B6');
    header.appendChild(dot);
    header.appendChild(name);
    header.appendChild(arrow);
    header.onclick = () => section.classList.toggle('expanded');

    // Screens list
    const screensList = createElement('div', 'flow-screens');
    flow.screens.forEach(screen => {
        const link = createElement('a', 'flow-screen' + (screen.id === currentPageId ? ' current' : ''));
        link.href = screen.file;
        const marker = createElement('span', 'screen-marker');
        link.appendChild(marker);
        link.appendChild(document.createTextNode(screen.name));
        screensList.appendChild(link);
    });

    section.appendChild(header);
    section.appendChild(screensList);
    return section;
}

// Create navigation actions
function createNavActions(flows, currentPageId) {
    const actions = createElement('div', 'nav-actions');
    const primaryFlow = flows[0];

    if (!primaryFlow) {
        return actions;
    }

    const currentIndex = primaryFlow.currentIndex;
    const prevScreen = currentIndex > 0 ? primaryFlow.screens[currentIndex - 1] : null;
    const nextScreen = currentIndex < primaryFlow.screens.length - 1 ? primaryFlow.screens[currentIndex + 1] : null;

    const prevBtn = createElement('a', 'nav-btn prev' + (!prevScreen ? ' disabled' : ''));
    prevBtn.href = prevScreen ? prevScreen.file : '#';
    prevBtn.textContent = '\u2190 ' + (prevScreen ? prevScreen.name : 'Start');

    const nextBtn = createElement('a', 'nav-btn next' + (!nextScreen ? ' disabled' : ''));
    nextBtn.href = nextScreen ? nextScreen.file : '#';
    nextBtn.textContent = (nextScreen ? nextScreen.name : 'End') + ' \u2192';

    actions.appendChild(prevBtn);
    actions.appendChild(nextBtn);
    return actions;
}

// Create navigation panel
function createNavigation() {
    const pageId = getCurrentPageId();
    const flows = findCurrentFlows(pageId);

    addStyles();

    // Create nav container
    const nav = createElement('div');
    nav.id = 'wireframe-nav';

    // Header
    const header = createElement('div', 'nav-header');
    const title = createElement('div', 'nav-title', 'Wireframe Navigation');
    const current = createElement('div', 'nav-current', 'Current: ');
    const currentId = createElement('span', 'nav-current-id', pageId);
    current.appendChild(currentId);
    header.appendChild(title);
    header.appendChild(current);

    // Home link
    const homeLink = createElement('a', 'nav-home', '\u2190 Back to Flow Index');
    homeLink.href = 'index.html';

    nav.appendChild(header);
    nav.appendChild(homeLink);

    // Add flow sections
    flows.forEach(flow => {
        nav.appendChild(createFlowSection(flow, pageId));
    });

    // Add nav actions
    nav.appendChild(createNavActions(flows, pageId));

    // Toggle button
    const toggle = createElement('button', 'wireframe-toggle-btn', '\u25C0');
    toggle.onclick = () => {
        nav.classList.toggle('collapsed');
        toggle.classList.toggle('collapsed');
        toggle.textContent = nav.classList.contains('collapsed') ? '\u25B6' : '\u25C0';
    };

    document.body.appendChild(nav);
    document.body.appendChild(toggle);
}

// Initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', createNavigation);
} else {
    createNavigation();
}
