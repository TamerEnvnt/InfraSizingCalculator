import { chromium } from 'playwright';

const browser = await chromium.launch();
const context = await browser.newContext({ viewport: { width: 1400, height: 900 } });
const page = await context.newPage();

console.log('PANEL TAB FUNCTIONALITY TEST');
console.log('============================\n');

try {
  await page.goto('http://localhost:5211/');
  await page.waitForLoadState('networkidle');

  await page.locator('[data-testid="btn-create-scenario"]').click();
  await page.waitForTimeout(1000);

  const tabs = [
    { name: 'Platform', testId: 'platform-config-panel', expectedContent: 'Platform' },
    { name: 'Apps', testId: 'apps-config-panel', expectedContent: 'Application Distribution' },
    { name: 'Nodes', testId: 'nodes-config-panel', expectedContent: 'Node Specifications' },
    { name: 'Pricing', testId: 'pricing-config-panel', expectedContent: 'On-Premises Pricing' },
    { name: 'Growth', testId: 'growth-config-panel', expectedContent: 'Growth Planning' }
  ];

  let allPassed = true;

  for (const tab of tabs) {
    console.log('\n--- Testing "' + tab.name + '" tab ---');

    const tabBtn = page.locator('.panel-tab:has-text("' + tab.name + '")');
    await tabBtn.click();
    await page.waitForTimeout(500);

    // Check if panel is visible
    const panel = page.locator('[data-testid="' + tab.testId + '"]');
    const isVisible = await panel.count() > 0;

    // Check for expected content
    const content = await page.locator('.slide-panel-content').textContent().catch(() => '');
    const hasExpectedContent = content.includes(tab.expectedContent);

    const visibleStatus = isVisible ? 'YES' : 'NO';
    const contentStatus = hasExpectedContent ? 'YES' : 'NO';

    console.log('  Panel visible: ' + visibleStatus);
    console.log('  Has "' + tab.expectedContent + '": ' + contentStatus);

    if (!isVisible || !hasExpectedContent) {
      allPassed = false;
      console.log('  Content preview: "' + content.substring(0, 100) + '..."');
    }

    await page.screenshot({ path: '/tmp/tab-' + tab.name.toLowerCase() + '.png' });
  }

  console.log('\n============================');
  if (allPassed) {
    console.log('ALL TABS WORKING CORRECTLY!');
  } else {
    console.log('Some tabs have issues');
  }

} catch (error) {
  console.error('Error:', error.message);
} finally {
  await browser.close();
}
