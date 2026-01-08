/**
 * Debug script to check what's actually rendered
 */
import { chromium } from 'playwright';

const BASE_URL = 'http://localhost:5062';

async function runDebug() {
  const browser = await chromium.launch();
  const context = await browser.newContext({ viewport: { width: 1400, height: 900 } });
  const page = await context.newPage();

  try {
    console.log('Navigating to dashboard...');
    await page.goto(BASE_URL + '/');
    await page.waitForLoadState('networkidle');

    console.log('Clicking Create Scenario...');
    await page.locator('[data-testid="btn-create-scenario"]').click();
    await page.waitForTimeout(2000);

    // Get all data-testids on page
    const allTestIdsBeforePlatform = await page.evaluate(() => {
      return Array.from(document.querySelectorAll('[data-testid]')).map(el => el.getAttribute('data-testid'));
    });
    console.log('\nAll data-testids after Create Scenario:', allTestIdsBeforePlatform.join(', '));

    // Check if slide-panel-container exists
    const panelContainerCount = await page.locator('[data-testid="slide-panel-container"]').count();
    console.log('\nslide-panel-container count:', panelContainerCount);

    // Check if Platform tab exists
    const platformTabCount = await page.locator('.panel-tab:has-text("Platform")').count();
    console.log('Platform tab count:', platformTabCount);

    if (platformTabCount > 0) {
      console.log('Clicking Platform tab...');
      await page.locator('.panel-tab:has-text("Platform")').click();
      await page.waitForTimeout(500);
    }

    console.log('\n=== DEBUG INFO ===\n');

    // Check what panel content is showing using correct testid
    const panelContentCount = await page.locator('[data-testid="panel-content"]').count();
    console.log('panel-content count:', panelContentCount);

    if (panelContentCount > 0) {
      const panelHtml = await page.locator('[data-testid="panel-content"]').innerHTML();
      console.log('Panel content first 1500 chars:', panelHtml.substring(0, 1500));
    }

    // Check for the platform-config-panel
    const platformConfigPanel = await page.locator('[data-testid="platform-config-panel"]').count();
    console.log('\nplatform-config-panel count:', platformConfigPanel);

    // Check for technology buttons
    const techMendix = await page.locator('[data-testid="tech-mendix"]').count();
    const techOutSystems = await page.locator('[data-testid="tech-outsystems"]').count();
    const techCustom = await page.locator('[data-testid="tech-custom"]').count();

    console.log('\nTechnology buttons found:');
    console.log('  tech-mendix:', techMendix);
    console.log('  tech-outsystems:', techOutSystems);
    console.log('  tech-custom:', techCustom);

    // Check selected class
    const selectedTech = await page.locator('.radio-option.selected').count();
    console.log('  Selected technology count:', selectedTech);

    // If selected exists, get its testid
    if (selectedTech > 0) {
      const testId = await page.locator('.radio-option.selected').getAttribute('data-testid');
      console.log('  Selected technology testid:', testId);
    }

    // Check section titles
    const sectionTitles = await page.locator('.section-title').allTextContents();
    console.log('\nSection titles:', sectionTitles);

    // Check platform buttons
    const vmSelected = await page.locator('[data-testid="platform-vm"].selected').count();
    const k8sSelected = await page.locator('[data-testid="platform-k8s"].selected').count();
    console.log('\nPlatform selection:');
    console.log('  VM selected:', vmSelected);
    console.log('  K8s selected:', k8sSelected);

    // Check deployment categories visible
    const mendixK8sOptions = await page.locator('[data-testid="mendix-k8s-options"]').count();
    console.log('\nDeployment options:');
    console.log('  mendix-k8s-options:', mendixK8sOptions);

    // Get the full list of data-testids on page
    const allTestIds = await page.evaluate(() => {
      return Array.from(document.querySelectorAll('[data-testid]')).map(el => el.getAttribute('data-testid'));
    });
    console.log('\nAll data-testids on page:', allTestIds);

    await page.screenshot({ path: '/tmp/debug-flow.png' });
    console.log('\nScreenshot saved to /tmp/debug-flow.png');

  } catch (error) {
    console.error('Debug error:', error.message);
    await page.screenshot({ path: '/tmp/debug-flow-error.png' });
    console.log('Error screenshot saved to /tmp/debug-flow-error.png');
  } finally {
    await browser.close();
  }
}

runDebug();
