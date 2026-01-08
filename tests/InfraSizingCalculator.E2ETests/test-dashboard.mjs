import { chromium } from 'playwright';

const browser = await chromium.launch();
const context = await browser.newContext({ viewport: { width: 1400, height: 900 } });
const page = await context.newPage();

console.log('╔══════════════════════════════════════════════════════════════╗');
console.log('║     WIREFRAME COMPLIANCE TEST - 02-dashboard-with-panel      ║');
console.log('╚══════════════════════════════════════════════════════════════╝\n');

try {
  await page.goto('http://localhost:5211/');
  await page.waitForLoadState('networkidle');
  
  // Click Create New Scenario
  const createBtn = await page.locator('[data-testid="btn-create-scenario"]');
  await createBtn.click();
  await page.waitForTimeout(1500);
  
  let passed = 0;
  let failed = 0;
  
  const test = async (name, condition) => {
    const result = await condition();
    if (result) {
      console.log(`✅ ${name}`);
      passed++;
    } else {
      console.log(`❌ ${name}`);
      failed++;
    }
  };
  
  console.log('─── LAYOUT STRUCTURE ───────────────────────────────────────────');
  await test('Dashboard layout visible', async () => 
    await page.locator('[data-testid="dashboard-layout"]').count() > 0);
  await test('Dashboard content area visible', async () => 
    await page.locator('[data-testid="dashboard-content"]').count() > 0);
  await test('Config panel visible (inline)', async () => 
    await page.locator('[data-testid="config-panel"]').count() > 0);
  await test('Dashboard grid with placeholder cards', async () => 
    await page.locator('[data-testid="dashboard-grid"]').count() > 0);
  
  console.log('\n─── REMOVED COMPONENTS (should NOT exist) ─────────────────────');
  await test('NO ConfigBar (horizontal buttons)', async () => 
    await page.locator('[data-testid="config-bar"]').count() === 0);
  await test('NO SaveBanner ("Sign Up Free")', async () => 
    await page.locator('text=Sign Up Free').count() === 0);
  await test('NO ActionBar (bottom bar)', async () => 
    await page.locator('[data-testid="action-bar"]').count() === 0);
  
  console.log('\n─── SUMMARY CARDS (wireframe values) ──────────────────────────');
  const cards = await page.locator('.summary-card').all();
  await test('5 summary cards present', async () => cards.length === 5);
  
  const expectedValues = [
    { label: 'Total Nodes', value: '12' },
    { label: 'Total vCPUs', value: '96' },
    { label: 'Total RAM (GB)', value: '384' },
    { label: 'Storage (TB)', value: '2.4' },
    { label: 'Monthly Cost', value: '$4,200' }
  ];
  
  for (const expected of expectedValues) {
    await test(`Card "${expected.label}" = ${expected.value}`, async () => {
      const card = page.locator(`.summary-card:has(.card-label:text("${expected.label}"))`);
      const value = await card.locator('.card-value').textContent().catch(() => '');
      return value.trim() === expected.value;
    });
  }
  
  console.log('\n─── DASHBOARD DIMMING ──────────────────────────────────────────');
  const content = page.locator('[data-testid="dashboard-content"]');
  await test('Has "dimmed" class when panel open', async () => 
    await content.evaluate(el => el.classList.contains('dimmed')));
  await test('Opacity is 0.5 (50%)', async () => {
    const opacity = await content.evaluate(el => getComputedStyle(el).opacity);
    return opacity === '0.5';
  });
  
  console.log('\n─── PANEL CONTENT (Platform Config) ───────────────────────────');
  await test('Platform config panel rendered', async () => 
    await page.locator('[data-testid="platform-config-panel"]').count() > 0);
  await test('PLATFORM section header', async () => 
    await page.locator('h3:text("Platform")').count() > 0);
  await test('VMs option button', async () => 
    await page.locator('[data-testid="platform-vm"]').count() > 0);
  await test('K8s option button', async () => 
    await page.locator('[data-testid="platform-k8s"]').count() > 0);
  await test('K8s is selected by default', async () => 
    await page.locator('[data-testid="platform-k8s"].selected').count() > 0);
  await test('DISTRIBUTION section header', async () => 
    await page.locator('h3:text("Distribution")').count() > 0);
  await test('Search input present', async () => 
    await page.locator('.search-input').count() > 0);
  await test('Active filters (Cloud, Managed)', async () => {
    const cloud = await page.locator('.tag-active:has-text("Cloud")').count();
    const managed = await page.locator('.tag-active:has-text("Managed")').count();
    return cloud > 0 && managed > 0;
  });
  await test('TECHNOLOGY section header', async () => 
    await page.locator('h3:text("Technology")').count() > 0);
  
  console.log('\n─── PANEL TABS ─────────────────────────────────────────────────');
  const tabs = ['Platform', 'Apps', 'Nodes', 'Pricing', 'Growth'];
  for (const tab of tabs) {
    await test(`Tab "${tab}" present`, async () => 
      await page.locator(`.panel-tab:has-text("${tab}")`).count() > 0);
  }
  
  // Take screenshots
  await page.screenshot({ path: '/tmp/test-full-dashboard.png', fullPage: false });
  
  // Scroll panel to show all content
  const panelContent = page.locator('.slide-panel-content');
  if (await panelContent.count() > 0) {
    await panelContent.evaluate(el => el.scrollTop = 500);
    await page.waitForTimeout(300);
    await page.screenshot({ path: '/tmp/test-panel-scrolled.png', fullPage: false });
  }
  
  console.log('\n════════════════════════════════════════════════════════════════');
  console.log(`RESULTS: ${passed} passed, ${failed} failed`);
  console.log('════════════════════════════════════════════════════════════════');
  
  if (failed === 0) {
    console.log('\n🎉 ALL TESTS PASSED - Dashboard matches wireframe!');
  } else {
    console.log('\n⚠️  Some tests failed - review issues above');
  }
  
  console.log('\nScreenshots saved:');
  console.log('  /tmp/test-full-dashboard.png');
  console.log('  /tmp/test-panel-scrolled.png');

} catch (error) {
  console.error('Error:', error.message);
} finally {
  await browser.close();
}
