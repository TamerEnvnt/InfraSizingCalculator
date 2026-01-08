/**
 * Test dynamic flow for all Technology + Platform combinations
 */
import { chromium } from 'playwright';

const BASE_URL = 'http://localhost:5062';

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
    console.log('Dynamic Flow Tests');
    console.log('========================================\n');

    // Navigate to dashboard
    await page.goto(BASE_URL + '/');
    await page.waitForLoadState('networkidle');
    await page.locator('[data-testid="btn-create-scenario"]').click();
    await page.waitForTimeout(1000);

    // Ensure Platform tab is active
    await page.locator('.panel-tab:has-text("Platform")').click();
    await page.waitForTimeout(300);

    // ========================================
    // Test 1: Mendix + K8s (Default)
    // ========================================
    console.log('\n--- Test 1: Mendix + K8s ---');

    await test('Mendix is selected by default', async () => {
      return await page.locator('[data-testid="tech-mendix"].selected').count() > 0;
    });

    await test('K8s is selected by default', async () => {
      return await page.locator('[data-testid="platform-k8s"].selected').count() > 0;
    });

    await test('Mendix K8s options are visible', async () => {
      return await page.locator('[data-testid="mendix-k8s-options"]').count() > 0;
    });

    await test('Section title is "Mendix Deployment"', async () => {
      const title = await page.locator('.section-title').last().textContent();
      return title.includes('Mendix Deployment');
    });

    await test('Mendix Cloud option exists', async () => {
      return await page.locator('[data-testid="mendix-k8s-mendix-cloud"]').count() > 0;
    });

    await test('Private Cloud option exists', async () => {
      return await page.locator('[data-testid="mendix-k8s-private-cloud"]').count() > 0;
    });

    await test('Other K8s option exists', async () => {
      return await page.locator('[data-testid="mendix-k8s-other-k8s"]').count() > 0;
    });

    // Select Private Cloud and check sub-options
    await page.locator('[data-testid="mendix-k8s-private-cloud"]').click();
    await page.waitForTimeout(200);

    await test('Provider sub-options appear for Private Cloud', async () => {
      return await page.locator('.sub-options').count() > 0;
    });

    await test('Azure AKS provider option exists', async () => {
      return await page.locator('[data-testid="mendix-provider-azure-aks"]').count() > 0;
    });

    await page.screenshot({ path: '/tmp/flow-01-mendix-k8s.png' });

    // ========================================
    // Test 2: Mendix + VMs
    // ========================================
    console.log('\n--- Test 2: Mendix + VMs ---');

    await page.locator('[data-testid="platform-vm"]').click();
    await page.waitForTimeout(300);

    await test('VM is now selected', async () => {
      return await page.locator('[data-testid="platform-vm"].selected').count() > 0;
    });

    await test('Mendix VM options are visible', async () => {
      return await page.locator('[data-testid="mendix-vm-options"]').count() > 0;
    });

    await test('Section title is "Mendix Deployment Type"', async () => {
      const title = await page.locator('.section-title').last().textContent();
      return title.includes('Mendix Deployment Type');
    });

    await test('Server option exists', async () => {
      return await page.locator('[data-testid="mendix-vm-server"]').count() > 0;
    });

    await test('StackIT option exists', async () => {
      return await page.locator('[data-testid="mendix-vm-stackit"]').count() > 0;
    });

    await test('SAP BTP option exists', async () => {
      return await page.locator('[data-testid="mendix-vm-sap-btp"]').count() > 0;
    });

    await test('Info note about non-K8s is visible', async () => {
      return await page.locator('.info-note').count() > 0;
    });

    await page.screenshot({ path: '/tmp/flow-02-mendix-vm.png' });

    // ========================================
    // Test 3: OutSystems + K8s
    // ========================================
    console.log('\n--- Test 3: OutSystems + K8s ---');

    await page.locator('[data-testid="platform-k8s"]').click();
    await page.waitForTimeout(200);
    await page.locator('[data-testid="tech-outsystems"]').click();
    await page.waitForTimeout(300);

    await test('OutSystems is now selected', async () => {
      return await page.locator('[data-testid="tech-outsystems"].selected').count() > 0;
    });

    await test('OutSystems K8s options are visible', async () => {
      return await page.locator('[data-testid="outsystems-k8s-options"]').count() > 0;
    });

    await test('Section title is "OutSystems Deployment"', async () => {
      const title = await page.locator('.section-title').last().textContent();
      return title.includes('OutSystems Deployment');
    });

    await test('OutSystems Cloud option exists', async () => {
      return await page.locator('[data-testid="outsystems-k8s-cloud"]').count() > 0;
    });

    await test('ODC option exists', async () => {
      return await page.locator('[data-testid="outsystems-k8s-odc"]').count() > 0;
    });

    await page.screenshot({ path: '/tmp/flow-03-outsystems-k8s.png' });

    // ========================================
    // Test 4: OutSystems + VMs
    // ========================================
    console.log('\n--- Test 4: OutSystems + VMs ---');

    await page.locator('[data-testid="platform-vm"]').click();
    await page.waitForTimeout(300);

    await test('OutSystems VM options are visible', async () => {
      return await page.locator('[data-testid="outsystems-vm-options"]').count() > 0;
    });

    await test('OutSystems Cloud VM option exists', async () => {
      return await page.locator('[data-testid="outsystems-vm-cloud"]').count() > 0;
    });

    await test('Self-Hosted option exists', async () => {
      return await page.locator('[data-testid="outsystems-vm-self-hosted"]').count() > 0;
    });

    await page.screenshot({ path: '/tmp/flow-04-outsystems-vm.png' });

    // ========================================
    // Test 5: Custom + K8s
    // ========================================
    console.log('\n--- Test 5: Custom + K8s ---');

    await page.locator('[data-testid="platform-k8s"]').click();
    await page.waitForTimeout(200);
    await page.locator('[data-testid="tech-custom"]').click();
    await page.waitForTimeout(300);

    await test('Custom is now selected', async () => {
      return await page.locator('[data-testid="tech-custom"].selected').count() > 0;
    });

    await test('Section title is "K8s Distribution"', async () => {
      const title = await page.locator('.section-title').last().textContent();
      return title.includes('K8s Distribution');
    });

    await test('Search input is visible', async () => {
      return await page.locator('.search-input').count() > 0;
    });

    await test('Filter tags are visible', async () => {
      return await page.locator('.tags-row .tag').count() > 0;
    });

    await test('Distribution grid is visible', async () => {
      return await page.locator('.dist-grid').count() > 0;
    });

    await test('Azure AKS distribution exists', async () => {
      return await page.locator('[data-testid="dist-azure-aks"]').count() > 0;
    });

    await page.screenshot({ path: '/tmp/flow-05-custom-k8s.png' });

    // ========================================
    // Test 6: Custom + VMs
    // ========================================
    console.log('\n--- Test 6: Custom + VMs ---');

    await page.locator('[data-testid="platform-vm"]').click();
    await page.waitForTimeout(300);

    await test('Section title is "Hypervisor"', async () => {
      const title = await page.locator('.section-title').last().textContent();
      return title.includes('Hypervisor');
    });

    await test('VMware vSphere hypervisor exists', async () => {
      return await page.locator('[data-testid="hyp-vmware-vsphere"]').count() > 0;
    });

    await test('Azure VMs hypervisor exists', async () => {
      return await page.locator('[data-testid="hyp-azure-vm"]').count() > 0;
    });

    await page.screenshot({ path: '/tmp/flow-06-custom-vm.png' });

    // ========================================
    // Results
    // ========================================
    console.log('\n========================================');
    console.log('TEST RESULTS');
    console.log('========================================');
    console.log('Passed: ' + passed);
    console.log('Failed: ' + failed);
    console.log('========================================\n');

    if (failed === 0) {
      console.log('ALL DYNAMIC FLOW TESTS PASSED!');
    } else {
      console.log('Some tests failed - review the output above');
    }

    console.log('\nScreenshots saved to /tmp/flow-*.png');

  } catch (error) {
    console.error('Test error:', error.message);
  } finally {
    await browser.close();
  }
}

runTest();
