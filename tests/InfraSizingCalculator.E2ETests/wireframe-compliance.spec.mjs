/**
 * Wireframe Compliance E2E Tests
 * Tests the v0.4.2 dashboard design against wireframe specifications
 *
 * Wireframes tested:
 * - 00-landing-guest.html: Guest landing page
 * - 02-dashboard-with-panel.html: Dashboard with configuration panel
 */

import { chromium } from 'playwright';

const BASE_URL = 'http://localhost:5211';

async function runTests() {
  const browser = await chromium.launch();
  const context = await browser.newContext({ viewport: { width: 1400, height: 900 } });
  const page = await context.newPage();

  let passed = 0;
  let failed = 0;
  const failures = [];

  const test = async (name, testFn) => {
    try {
      const result = await testFn();
      if (result) {
        console.log('  [PASS] ' + name);
        passed++;
      } else {
        console.log('  [FAIL] ' + name);
        failed++;
        failures.push(name);
      }
    } catch (err) {
      console.log('  [FAIL] ' + name + ' - Error: ' + err.message);
      failed++;
      failures.push(name + ' (error)');
    }
  };

  try {
    // ============================================
    // TEST SUITE 1: Guest Landing Page
    // ============================================
    console.log('\n========================================');
    console.log('SUITE 1: Guest Landing Page (00-landing-guest.html)');
    console.log('========================================\n');

    await page.goto(BASE_URL + '/');
    await page.waitForLoadState('networkidle');

    await test('Guest landing page renders', async () => {
      return await page.locator('[data-testid="guest-landing"]').count() > 0;
    });

    await test('Logo displays "InfraSizing Calculator"', async () => {
      const logo = await page.locator('.logo').textContent();
      return logo.includes('InfraSizing Calculator');
    });

    await test('Sign In button visible', async () => {
      return await page.locator('.btn-login:has-text("Sign In")').count() > 0;
    });

    await test('Sign Up button visible', async () => {
      return await page.locator('.btn-register:has-text("Sign Up")').count() > 0;
    });

    await test('Landing title displays correctly', async () => {
      const title = await page.locator('.landing-title').textContent();
      return title.includes('Infrastructure Sizing Calculator');
    });

    await test('Create New Scenario button visible', async () => {
      return await page.locator('[data-testid="btn-create-scenario"]').count() > 0;
    });

    await test('Login prompt section visible', async () => {
      return await page.locator('.login-prompt').count() > 0;
    });

    await test('Features list shows 3 features', async () => {
      const features = await page.locator('.feature-item').count();
      return features === 3;
    });

    await test('Footer with links visible', async () => {
      return await page.locator('.landing-footer').count() > 0;
    });

    // Take screenshot
    await page.screenshot({ path: '/tmp/e2e-01-guest-landing.png' });

    // ============================================
    // TEST SUITE 2: Dashboard Layout
    // ============================================
    console.log('\n========================================');
    console.log('SUITE 2: Dashboard Layout (02-dashboard-with-panel.html)');
    console.log('========================================\n');

    // Click Create New Scenario to enter dashboard
    await page.locator('[data-testid="btn-create-scenario"]').click();
    await page.waitForTimeout(1000);

    await test('Dashboard layout renders', async () => {
      return await page.locator('[data-testid="dashboard-layout"]').count() > 0;
    });

    await test('Dashboard content area visible', async () => {
      return await page.locator('[data-testid="dashboard-content"]').count() > 0;
    });

    await test('Dashboard content is dimmed (opacity 0.5)', async () => {
      const content = page.locator('[data-testid="dashboard-content"]');
      const hasDimmed = await content.evaluate(el => el.classList.contains('dimmed'));
      const opacity = await content.evaluate(el => getComputedStyle(el).opacity);
      return hasDimmed && opacity === '0.5';
    });

    await test('Config panel visible (inline, not overlay)', async () => {
      return await page.locator('[data-testid="config-panel"]').count() > 0;
    });

    await test('Dashboard grid with 2 placeholder cards', async () => {
      const grid = await page.locator('[data-testid="dashboard-grid"]').count() > 0;
      const cards = await page.locator('.dashboard-card').count();
      return grid && cards === 2;
    });

    await test('NO ConfigBar (removed per wireframe)', async () => {
      return await page.locator('[data-testid="config-bar"]').count() === 0;
    });

    await test('NO SaveBanner (removed per wireframe)', async () => {
      return await page.locator('text=Sign Up Free').count() === 0;
    });

    await test('NO ActionBar (removed per wireframe)', async () => {
      return await page.locator('[data-testid="action-bar"]').count() === 0;
    });

    await page.screenshot({ path: '/tmp/e2e-02-dashboard-layout.png' });

    // ============================================
    // TEST SUITE 3: Summary Cards
    // ============================================
    console.log('\n========================================');
    console.log('SUITE 3: Summary Cards');
    console.log('========================================\n');

    await test('5 summary cards displayed', async () => {
      return await page.locator('.summary-card').count() === 5;
    });

    const expectedCards = [
      { label: 'Total Nodes', value: '12' },
      { label: 'Total vCPUs', value: '96' },
      { label: 'Total RAM (GB)', value: '384' },
      { label: 'Storage (TB)', value: '2.4' },
      { label: 'Monthly Cost', value: '$4,200' }
    ];

    for (const card of expectedCards) {
      await test('Card "' + card.label + '" = ' + card.value, async () => {
        const cardEl = page.locator('.summary-card:has(.card-label:text("' + card.label + '"))');
        const value = await cardEl.locator('.card-value').textContent().catch(() => '');
        return value.trim() === card.value;
      });
    }

    // ============================================
    // TEST SUITE 4: Panel Tabs
    // ============================================
    console.log('\n========================================');
    console.log('SUITE 4: Panel Tabs Navigation');
    console.log('========================================\n');

    const tabs = ['Platform', 'Apps', 'Nodes', 'Pricing', 'Growth'];
    for (const tab of tabs) {
      await test('Tab "' + tab + '" exists', async () => {
        return await page.locator('.panel-tab:has-text("' + tab + '")').count() > 0;
      });
    }

    // ============================================
    // TEST SUITE 5: Platform Configuration Panel
    // ============================================
    console.log('\n========================================');
    console.log('SUITE 5: Platform Configuration Panel');
    console.log('========================================\n');

    await page.locator('.panel-tab:has-text("Platform")').click();
    await page.waitForTimeout(300);

    await test('Platform config panel visible', async () => {
      return await page.locator('[data-testid="platform-config-panel"]').count() > 0;
    });

    await test('PLATFORM section header', async () => {
      return await page.locator('h3:text("Platform")').count() > 0;
    });

    await test('VM option button visible', async () => {
      return await page.locator('[data-testid="platform-vm"]').count() > 0;
    });

    await test('K8s option button visible', async () => {
      return await page.locator('[data-testid="platform-k8s"]').count() > 0;
    });

    await test('K8s selected by default', async () => {
      return await page.locator('[data-testid="platform-k8s"].selected').count() > 0;
    });

    await test('VM click changes selection', async () => {
      await page.locator('[data-testid="platform-vm"]').click();
      await page.waitForTimeout(200);
      const vmSelected = await page.locator('[data-testid="platform-vm"].selected').count() > 0;
      const k8sNotSelected = await page.locator('[data-testid="platform-k8s"].selected').count() === 0;
      return vmSelected && k8sNotSelected;
    });

    await test('K8s click changes selection back', async () => {
      await page.locator('[data-testid="platform-k8s"]').click();
      await page.waitForTimeout(200);
      const k8sSelected = await page.locator('[data-testid="platform-k8s"].selected').count() > 0;
      const vmNotSelected = await page.locator('[data-testid="platform-vm"].selected').count() === 0;
      return k8sSelected && vmNotSelected;
    });

    await test('DISTRIBUTION section header', async () => {
      return await page.locator('h3:text("Distribution")').count() > 0;
    });

    await test('Search input present', async () => {
      return await page.locator('.search-input').count() > 0;
    });

    await test('Active filters (Cloud, Managed) present', async () => {
      const cloud = await page.locator('.tag-active:has-text("Cloud")').count();
      const managed = await page.locator('.tag-active:has-text("Managed")').count();
      return cloud > 0 && managed > 0;
    });

    await test('Distribution grid visible', async () => {
      return await page.locator('.dist-grid').count() > 0;
    });

    await test('Azure AKS distribution option', async () => {
      return await page.locator('[data-testid="dist-azure-aks"]').count() > 0;
    });

    await test('TECHNOLOGY section header', async () => {
      return await page.locator('h3:text("Technology")').count() > 0;
    });

    await test('Mendix technology option', async () => {
      return await page.locator('[data-testid="tech-mendix"]').count() > 0;
    });

    await page.screenshot({ path: '/tmp/e2e-05-platform-panel.png' });

    // ============================================
    // TEST SUITE 6: Apps Configuration Panel
    // ============================================
    console.log('\n========================================');
    console.log('SUITE 6: Apps Configuration Panel');
    console.log('========================================\n');

    await page.locator('.panel-tab:has-text("Apps")').click();
    await page.waitForTimeout(300);

    await test('Apps config panel visible', async () => {
      return await page.locator('[data-testid="apps-config-panel"]').count() > 0;
    });

    await test('Application Distribution header', async () => {
      return await page.locator('h4:text("Application Distribution")').count() > 0;
    });

    await test('Quick presets available', async () => {
      return await page.locator('.preset-chip').count() >= 3;
    });

    await test('App size sliders present', async () => {
      return await page.locator('.app-slider-row').count() >= 3;
    });

    await test('Total Applications summary', async () => {
      return await page.locator('.total-apps-summary').count() > 0;
    });

    await page.screenshot({ path: '/tmp/e2e-06-apps-panel.png' });

    // ============================================
    // TEST SUITE 7: Nodes Configuration Panel
    // ============================================
    console.log('\n========================================');
    console.log('SUITE 7: Nodes Configuration Panel');
    console.log('========================================\n');

    await page.locator('.panel-tab:has-text("Nodes")').click();
    await page.waitForTimeout(300);

    await test('Nodes config panel visible', async () => {
      return await page.locator('[data-testid="nodes-config-panel"]').count() > 0;
    });

    await test('Node Specifications header', async () => {
      return await page.locator('h4:text("Node Specifications")').count() > 0;
    });

    await test('Size presets (Small, Medium, Large, X-Large)', async () => {
      return await page.locator('.size-preset').count() === 4;
    });

    await test('vCPU slider present', async () => {
      return await page.locator('.spec-label:text("vCPU Cores")').count() > 0;
    });

    await test('Memory slider present', async () => {
      return await page.locator('.spec-label:text("Memory")').count() > 0;
    });

    await test('Storage slider present', async () => {
      return await page.locator('.spec-label:text("Storage")').count() > 0;
    });

    await test('Node counter visible', async () => {
      return await page.locator('.node-counter').count() > 0;
    });

    await test('Resource summary grid', async () => {
      return await page.locator('.resource-summary').count() > 0;
    });

    await page.screenshot({ path: '/tmp/e2e-07-nodes-panel.png' });

    // ============================================
    // TEST SUITE 8: Pricing Configuration Panel
    // ============================================
    console.log('\n========================================');
    console.log('SUITE 8: Pricing Configuration Panel');
    console.log('========================================\n');

    await page.locator('.panel-tab:has-text("Pricing")').click();
    await page.waitForTimeout(300);

    await test('Pricing config panel visible', async () => {
      return await page.locator('[data-testid="pricing-config-panel"]').count() > 0;
    });

    await test('On-Premises Pricing header', async () => {
      return await page.locator('h4:text("On-Premises Pricing")').count() > 0;
    });

    await test('Hardware Cost input', async () => {
      return await page.locator('text=Hardware Cost per Node').count() > 0;
    });

    await test('Monthly Maintenance input', async () => {
      return await page.locator('text=Monthly Maintenance').count() > 0;
    });

    await test('Power & Cooling input', async () => {
      return await page.locator('text=Power & Cooling').count() > 0;
    });

    await test('Amortization period options', async () => {
      return await page.locator('.radio-option').count() >= 3;
    });

    await test('Cost summary section', async () => {
      return await page.locator('.cost-summary').count() > 0;
    });

    await page.screenshot({ path: '/tmp/e2e-08-pricing-panel.png' });

    // ============================================
    // TEST SUITE 9: Growth Configuration Panel
    // ============================================
    console.log('\n========================================');
    console.log('SUITE 9: Growth Configuration Panel');
    console.log('========================================\n');

    await page.locator('.panel-tab:has-text("Growth")').click();
    await page.waitForTimeout(300);

    await test('Growth config panel visible', async () => {
      return await page.locator('[data-testid="growth-config-panel"]').count() > 0;
    });

    await test('Growth Planning header', async () => {
      return await page.locator('h4:text("Growth Planning")').count() > 0;
    });

    await test('Planning horizon selector (1-5 years)', async () => {
      return await page.locator('.horizon-btn').count() === 5;
    });

    await test('Growth rate slider', async () => {
      return await page.locator('.growth-slider').count() > 0;
    });

    await test('Projected growth timeline', async () => {
      return await page.locator('.preview-timeline').count() > 0;
    });

    await test('Capacity recommendations', async () => {
      return await page.locator('.capacity-recommendations').count() > 0;
    });

    await page.screenshot({ path: '/tmp/e2e-09-growth-panel.png' });

    // ============================================
    // TEST SUITE 10: Panel Actions
    // ============================================
    console.log('\n========================================');
    console.log('SUITE 10: Panel Actions');
    console.log('========================================\n');

    await test('Reset button visible', async () => {
      return await page.locator('[data-testid="panel-reset"]').count() > 0;
    });

    await test('Apply Changes button visible', async () => {
      return await page.locator('[data-testid="panel-apply"]').count() > 0;
    });

    await test('Close button visible', async () => {
      return await page.locator('[data-testid="panel-close"]').count() > 0;
    });

    await test('Close button closes panel', async () => {
      await page.locator('[data-testid="panel-close"]').click();
      await page.waitForTimeout(300);
      const panelClosed = await page.locator('[data-testid="config-panel"]').count() === 0;
      const contentNotDimmed = await page.locator('[data-testid="dashboard-content"].dimmed').count() === 0;
      return panelClosed && contentNotDimmed;
    });

    await page.screenshot({ path: '/tmp/e2e-10-panel-closed.png' });

    // ============================================
    // RESULTS
    // ============================================
    console.log('\n========================================');
    console.log('TEST RESULTS');
    console.log('========================================');
    console.log('Passed: ' + passed);
    console.log('Failed: ' + failed);
    console.log('Total:  ' + (passed + failed));
    console.log('========================================\n');

    if (failed > 0) {
      console.log('Failed tests:');
      failures.forEach(f => console.log('  - ' + f));
      console.log('');
    }

    if (failed === 0) {
      console.log('ALL TESTS PASSED - Wireframe compliance verified!');
    } else {
      console.log('SOME TESTS FAILED - Review issues above');
    }

    console.log('\nScreenshots saved to /tmp/e2e-*.png');

  } catch (error) {
    console.error('Test suite error:', error.message);
  } finally {
    await browser.close();
  }
}

runTests();
