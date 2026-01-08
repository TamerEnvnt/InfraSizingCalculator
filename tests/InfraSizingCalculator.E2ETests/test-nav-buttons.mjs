/**
 * Test navigation buttons in the SlidePanel
 */
import { chromium } from 'playwright';

const BASE_URL = 'http://localhost:5211';

async function runTest() {
  const browser = await chromium.launch();
  const context = await browser.newContext({ viewport: { width: 1400, height: 900 } });
  const page = await context.newPage();

  let passed = 0;
  let failed = 0;

  const test = async (name, testFn) => {
    try {
      const result = await testFn();
      if (result) {
        console.log('  [PASS] ' + name);
        passed++;
      } else {
        console.log('  [FAIL] ' + name);
        failed++;
      }
    } catch (err) {
      console.log('  [FAIL] ' + name + ' - Error: ' + err.message);
      failed++;
    }
  };

  try {
    console.log('\n========================================');
    console.log('Navigation Button Tests');
    console.log('========================================\n');

    // Navigate to dashboard
    await page.goto(BASE_URL + '/');
    await page.waitForLoadState('networkidle');
    await page.locator('[data-testid="btn-create-scenario"]').click();
    await page.waitForTimeout(1000);

    // Test 1: Platform tab (first tab) - should have Next but NOT Previous
    console.log('\n--- Platform Tab (First) ---');
    await page.locator('.panel-tab:has-text("Platform")').click();
    await page.waitForTimeout(300);

    await test('Platform tab: NO Previous button', async () => {
      return await page.locator('[data-testid="panel-previous"]').count() === 0;
    });

    await test('Platform tab: Has Next button', async () => {
      return await page.locator('[data-testid="panel-next"]').count() > 0;
    });

    await test('Platform tab: NO Apply button (Next shown instead)', async () => {
      return await page.locator('[data-testid="panel-apply"]').count() === 0;
    });

    // Test 2: Click Next to go to Apps tab
    console.log('\n--- Apps Tab (Middle) ---');
    await page.locator('[data-testid="panel-next"]').click();
    await page.waitForTimeout(300);

    await test('After Next: Apps tab is now active', async () => {
      return await page.locator('.panel-tab.active:has-text("Apps")').count() > 0 ||
             await page.locator('[data-testid="apps-config-panel"]').count() > 0;
    });

    await test('Apps tab: Has Previous button', async () => {
      return await page.locator('[data-testid="panel-previous"]').count() > 0;
    });

    await test('Apps tab: Has Next button', async () => {
      return await page.locator('[data-testid="panel-next"]').count() > 0;
    });

    // Test 3: Click Previous to go back to Platform
    console.log('\n--- Previous Navigation ---');
    await page.locator('[data-testid="panel-previous"]').click();
    await page.waitForTimeout(300);

    await test('After Previous: Platform tab is active again', async () => {
      return await page.locator('.panel-tab.active:has-text("Platform")').count() > 0 ||
             await page.locator('[data-testid="platform-config-panel"]').count() > 0;
    });

    // Test 4: Navigate all the way to Growth (last tab)
    console.log('\n--- Growth Tab (Last) ---');
    await page.locator('.panel-tab:has-text("Growth")').click();
    await page.waitForTimeout(300);

    await test('Growth tab: Has Previous button', async () => {
      return await page.locator('[data-testid="panel-previous"]').count() > 0;
    });

    await test('Growth tab: NO Next button', async () => {
      return await page.locator('[data-testid="panel-next"]').count() === 0;
    });

    await test('Growth tab: Has Apply button (since no Next)', async () => {
      return await page.locator('[data-testid="panel-apply"]').count() > 0;
    });

    // Test 5: Navigate through all tabs
    console.log('\n--- Full Navigation Flow ---');
    await page.locator('.panel-tab:has-text("Platform")').click();
    await page.waitForTimeout(200);

    const tabSequence = ['Platform', 'Apps', 'Nodes', 'Pricing', 'Growth'];
    let currentIndex = 0;

    for (let i = 0; i < 4; i++) {
      await page.locator('[data-testid="panel-next"]').click();
      await page.waitForTimeout(200);
      currentIndex++;
    }

    await test('Navigated through all tabs to Growth', async () => {
      return await page.locator('[data-testid="growth-config-panel"]').count() > 0;
    });

    // Navigate all the way back
    for (let i = 0; i < 4; i++) {
      await page.locator('[data-testid="panel-previous"]').click();
      await page.waitForTimeout(200);
    }

    await test('Navigated back to Platform', async () => {
      return await page.locator('[data-testid="platform-config-panel"]').count() > 0;
    });

    // Take screenshot
    await page.screenshot({ path: '/tmp/nav-buttons-test.png' });

    // Results
    console.log('\n========================================');
    console.log('TEST RESULTS');
    console.log('========================================');
    console.log('Passed: ' + passed);
    console.log('Failed: ' + failed);
    console.log('========================================\n');

    if (failed === 0) {
      console.log('ALL NAVIGATION TESTS PASSED!');
    }

  } catch (error) {
    console.error('Test error:', error.message);
  } finally {
    await browser.close();
  }
}

runTest();
