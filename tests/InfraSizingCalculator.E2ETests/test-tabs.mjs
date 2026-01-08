import { chromium } from 'playwright';

const browser = await chromium.launch();
const context = await browser.newContext({ viewport: { width: 1400, height: 900 } });
const page = await context.newPage();

try {
  await page.goto('http://localhost:5211/');
  await page.waitForLoadState('networkidle');
  
  // Click Create New Scenario
  await page.locator('[data-testid="btn-create-scenario"]').click();
  await page.waitForTimeout(1000);
  
  console.log('=== TESTING TAB SWITCHING ===\n');
  
  const tabs = ['Platform', 'Apps', 'Nodes', 'Pricing', 'Growth'];
  
  for (const tab of tabs) {
    console.log(`\n--- Clicking "${tab}" tab ---`);
    
    // Click the tab
    const tabBtn = page.locator(`.panel-tab:has-text("${tab}")`);
    if (await tabBtn.count() > 0) {
      await tabBtn.click();
      await page.waitForTimeout(500);
      
      // Check what's visible in the panel content
      const platformPanel = await page.locator('[data-testid="platform-config-panel"]').count();
      const appsPanel = await page.locator('[data-testid="apps-config-panel"]').count();
      const nodesPanel = await page.locator('[data-testid="nodes-config-panel"]').count();
      const pricingPanel = await page.locator('[data-testid="pricing-config-panel"]').count();
      const growthPanel = await page.locator('[data-testid="growth-config-panel"]').count();
      const placeholder = await page.locator('.panel-placeholder').count();
      
      console.log(`  Platform panel: ${platformPanel > 0}`);
      console.log(`  Apps panel: ${appsPanel > 0}`);
      console.log(`  Nodes panel: ${nodesPanel > 0}`);
      console.log(`  Pricing panel: ${pricingPanel > 0}`);
      console.log(`  Growth panel: ${growthPanel > 0}`);
      console.log(`  Placeholder: ${placeholder > 0}`);
      
      // Get any visible content
      const panelContent = page.locator('.slide-panel-content');
      const innerText = await panelContent.innerText().catch(() => 'N/A');
      console.log(`  Content preview: "${innerText.substring(0, 100).replace(/\n/g, ' ')}..."`);
      
      // Take screenshot
      await page.screenshot({ path: `/tmp/tab-${tab.toLowerCase()}.png` });
    } else {
      console.log(`  Tab not found!`);
    }
  }
  
  console.log('\n\nScreenshots saved to /tmp/tab-*.png');

} catch (error) {
  console.error('Error:', error.message);
} finally {
  await browser.close();
}
