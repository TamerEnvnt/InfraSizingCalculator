/**
 * Comprehensive E2E Test Suite for Platform Configuration Panel
 *
 * Coverage:
 * - All 20 K8s distributions
 * - All 15 VM hypervisors
 * - All filter tag combinations
 * - Search functionality
 * - Technology + Platform combinations
 * - Default selection validation
 */
import { chromium } from 'playwright';

const BASE_URL = 'http://localhost:5062';

// ========== TEST DATA ==========

const K8S_DISTRIBUTIONS = [
  // Managed Cloud
  { id: 'azure-aks', name: 'Azure AKS', type: 'Managed', tags: ['Cloud', 'Managed', 'Azure'] },
  { id: 'amazon-eks', name: 'Amazon EKS', type: 'Managed', tags: ['Cloud', 'Managed', 'AWS'] },
  { id: 'google-gke', name: 'Google GKE', type: 'Managed', tags: ['Cloud', 'Managed', 'GCP'] },
  { id: 'digitalocean-doks', name: 'DigitalOcean DOKS', type: 'Managed', tags: ['Cloud', 'Managed'] },
  { id: 'linode-lke', name: 'Linode LKE', type: 'Managed', tags: ['Cloud', 'Managed'] },
  { id: 'oracle-oke', name: 'Oracle OKE', type: 'Managed', tags: ['Cloud', 'Managed'] },
  { id: 'civo', name: 'Civo Kubernetes', type: 'Managed', tags: ['Cloud', 'Managed'] },
  { id: 'vultr-k8s', name: 'Vultr Kubernetes', type: 'Managed', tags: ['Cloud', 'Managed'] },
  // Enterprise
  { id: 'openshift', name: 'Red Hat OpenShift', type: 'Enterprise', tags: ['On-Prem', 'Enterprise', 'Needs-Sizing'] },
  { id: 'tanzu', name: 'VMware Tanzu', type: 'Enterprise', tags: ['On-Prem', 'Enterprise', 'Needs-Sizing'] },
  { id: 'd2iq-konvoy', name: 'D2iQ Konvoy', type: 'Enterprise', tags: ['On-Prem', 'Enterprise', 'Air-Gap', 'Needs-Sizing'] },
  { id: 'charmed-k8s', name: 'Charmed Kubernetes', type: 'Enterprise', tags: ['On-Prem', 'Enterprise', 'Needs-Sizing'] },
  // Self-Hosted
  { id: 'rancher-rke2', name: 'Rancher RKE2', type: 'Self-Hosted', tags: ['On-Prem', 'Self-Hosted', 'Needs-Sizing'] },
  { id: 'vanilla-k8s', name: 'Vanilla Kubernetes', type: 'Self-Hosted', tags: ['On-Prem', 'Self-Hosted', 'Needs-Sizing'] },
  // Lightweight
  { id: 'k3s', name: 'K3s', type: 'Lightweight', tags: ['On-Prem', 'Lightweight', 'Needs-Sizing'] },
  { id: 'k0s', name: 'k0s', type: 'Lightweight', tags: ['On-Prem', 'Lightweight', 'Needs-Sizing'] },
  { id: 'microk8s', name: 'MicroK8s', type: 'Lightweight', tags: ['On-Prem', 'Lightweight', 'Needs-Sizing'] },
  // Hybrid
  { id: 'eks-anywhere', name: 'EKS Anywhere', type: 'Hybrid', tags: ['On-Prem', 'Hybrid', 'AWS', 'Needs-Sizing'] },
  { id: 'azure-arc', name: 'Azure Arc-enabled K8s', type: 'Hybrid', tags: ['Hybrid', 'Azure', 'Needs-Sizing'] },
  { id: 'anthos', name: 'Google Anthos', type: 'Hybrid', tags: ['Hybrid', 'GCP', 'Needs-Sizing'] },
];

const VM_HYPERVISORS = [
  // Enterprise On-Prem
  { id: 'vmware-vsphere', name: 'VMware vSphere', type: 'Enterprise', tags: ['On-Prem', 'Enterprise', 'Needs-Sizing'] },
  { id: 'hyperv', name: 'Microsoft Hyper-V', type: 'Enterprise', tags: ['On-Prem', 'Enterprise', 'Needs-Sizing'] },
  { id: 'nutanix', name: 'Nutanix AHV', type: 'Enterprise', tags: ['On-Prem', 'Enterprise', 'Needs-Sizing'] },
  { id: 'xenserver', name: 'Citrix XenServer', type: 'Enterprise', tags: ['On-Prem', 'Enterprise', 'Needs-Sizing'] },
  // Open Source
  { id: 'proxmox', name: 'Proxmox VE', type: 'Open Source', tags: ['On-Prem', 'Open-Source', 'Needs-Sizing'] },
  { id: 'kvm-libvirt', name: 'KVM/libvirt', type: 'Open Source', tags: ['On-Prem', 'Open-Source', 'Needs-Sizing'] },
  { id: 'ovirt', name: 'oVirt/RHV', type: 'Open Source', tags: ['On-Prem', 'Open-Source', 'Needs-Sizing'] },
  // Hybrid
  { id: 'azure-stack-hci', name: 'Azure Stack HCI', type: 'Hybrid', tags: ['On-Prem', 'Hybrid', 'Azure', 'Needs-Sizing'] },
  { id: 'vmware-cloud', name: 'VMware Cloud', type: 'Hybrid', tags: ['Hybrid', 'Needs-Sizing'] },
  // Managed Cloud
  { id: 'azure-vm', name: 'Azure Virtual Machines', type: 'Managed', tags: ['Cloud', 'Managed', 'Azure'] },
  { id: 'aws-ec2', name: 'AWS EC2', type: 'Managed', tags: ['Cloud', 'Managed', 'AWS'] },
  { id: 'gcp-compute', name: 'Google Compute Engine', type: 'Managed', tags: ['Cloud', 'Managed', 'GCP'] },
  { id: 'oracle-oci', name: 'Oracle OCI', type: 'Managed', tags: ['Cloud', 'Managed'] },
  { id: 'digitalocean-droplets', name: 'DigitalOcean Droplets', type: 'Managed', tags: ['Cloud', 'Managed'] },
  { id: 'vultr-compute', name: 'Vultr Compute', type: 'Managed', tags: ['Cloud', 'Managed'] },
];

const K8S_FILTER_TAGS = ['Cloud', 'On-Prem', 'Managed', 'Self-Hosted', 'Enterprise', 'Lightweight', 'Hybrid', 'Air-Gap', 'Needs-Sizing'];
const VM_FILTER_TAGS = ['Cloud', 'On-Prem', 'Managed', 'Enterprise', 'Open-Source', 'Hybrid', 'Needs-Sizing'];

const TECHNOLOGIES = ['mendix', 'outsystems', 'custom'];
const PLATFORMS = ['k8s', 'vm'];

// Mendix K8s categories and providers
const MENDIX_K8S_CATEGORIES = ['mendix-cloud', 'private-cloud', 'other-k8s'];
const MENDIX_PRIVATE_CLOUD_PROVIDERS = ['azure-aks', 'amazon-eks', 'google-gke', 'openshift'];
const MENDIX_OTHER_K8S = ['rancher', 'k3s', 'generic-k8s'];

// Mendix VM options
const MENDIX_VM_OPTIONS = ['server', 'stackit', 'sap-btp'];

// OutSystems options
const OUTSYSTEMS_K8S_OPTIONS = ['cloud', 'odc', 'private-cloud'];
const OUTSYSTEMS_VM_OPTIONS = ['cloud', 'self-hosted'];

// ========== TEST RUNNER ==========

class TestRunner {
  constructor() {
    this.passed = 0;
    this.failed = 0;
    this.skipped = 0;
    this.results = [];
    this.startTime = Date.now();
  }

  async test(category, name, testFn) {
    const fullName = `[${category}] ${name}`;
    try {
      const result = await testFn();
      if (result === 'skip') {
        this.skipped++;
        this.results.push({ name: fullName, status: 'SKIP' });
      } else if (result) {
        this.passed++;
        this.results.push({ name: fullName, status: 'PASS' });
      } else {
        this.failed++;
        this.results.push({ name: fullName, status: 'FAIL', error: 'Assertion failed' });
        console.log(`  [FAIL] ${fullName}`);
      }
    } catch (err) {
      this.failed++;
      this.results.push({ name: fullName, status: 'FAIL', error: err.message });
      console.log(`  [FAIL] ${fullName} - ${err.message}`);
    }
  }

  printSummary() {
    const duration = ((Date.now() - this.startTime) / 1000).toFixed(2);
    console.log('\n========================================');
    console.log('COMPREHENSIVE TEST RESULTS');
    console.log('========================================');
    console.log(`Total Tests: ${this.passed + this.failed + this.skipped}`);
    console.log(`Passed: ${this.passed}`);
    console.log(`Failed: ${this.failed}`);
    console.log(`Skipped: ${this.skipped}`);
    console.log(`Duration: ${duration}s`);
    console.log('========================================\n');

    if (this.failed > 0) {
      console.log('FAILED TESTS:');
      this.results.filter(r => r.status === 'FAIL').forEach(r => {
        console.log(`  - ${r.name}: ${r.error}`);
      });
      console.log('');
    }

    return this.failed === 0;
  }
}

// ========== TEST SUITES ==========

async function runAllTests() {
  const browser = await chromium.launch();
  const context = await browser.newContext({ viewport: { width: 1400, height: 900 } });
  const page = await context.newPage();
  const runner = new TestRunner();

  try {
    // Navigate and setup
    console.log('Setting up test environment...\n');
    await page.goto(BASE_URL + '/');
    await page.waitForLoadState('networkidle');
    await page.locator('[data-testid="btn-create-scenario"]').click();
    await page.waitForTimeout(1000);
    await page.locator('.panel-tab:has-text("Platform")').click();
    await page.waitForTimeout(300);

    // ========== 1. K8S DISTRIBUTION TESTS ==========
    console.log('--- Testing K8s Distributions (20 distributions) ---');

    // Switch to Custom + K8s to access distribution selector
    await page.locator('[data-testid="platform-k8s"]').click();
    await page.waitForTimeout(200);
    await page.locator('[data-testid="tech-custom"]').click();
    await page.waitForTimeout(300);

    for (const dist of K8S_DISTRIBUTIONS) {
      // Test distribution exists
      await runner.test('K8s-Dist', `${dist.name} exists`, async () => {
        return await page.locator(`[data-testid="dist-${dist.id}"]`).count() > 0;
      });

      // Test distribution can be selected
      await runner.test('K8s-Dist', `${dist.name} can be selected`, async () => {
        await page.locator(`[data-testid="dist-${dist.id}"]`).click();
        await page.waitForTimeout(100);
        return await page.locator(`[data-testid="dist-${dist.id}"].selected`).count() > 0;
      });

      // Test distribution shows correct type
      await runner.test('K8s-Dist', `${dist.name} shows type "${dist.type}"`, async () => {
        const typeText = await page.locator(`[data-testid="dist-${dist.id}"] .dist-type`).textContent();
        return typeText.includes(dist.type);
      });
    }

    // ========== 2. K8S FILTER TAG TESTS ==========
    console.log('\n--- Testing K8s Filter Tags ---');

    for (const tag of K8S_FILTER_TAGS) {
      // Clear any existing filters first by refreshing section
      await page.locator('[data-testid="tech-mendix"]').click();
      await page.waitForTimeout(100);
      await page.locator('[data-testid="tech-custom"]').click();
      await page.waitForTimeout(200);

      await runner.test('K8s-Filter', `Tag "${tag}" can be activated`, async () => {
        const tagEl = page.locator(`.tag:has-text("${tag}")`).first();
        if (await tagEl.count() === 0) return false;
        await tagEl.click();
        await page.waitForTimeout(100);
        return await page.locator(`.tag-active:has-text("${tag}")`).count() > 0;
      });

      await runner.test('K8s-Filter', `Tag "${tag}" filters results correctly`, async () => {
        // Get expected distributions for this tag
        const expected = K8S_DISTRIBUTIONS.filter(d => d.tags.includes(tag));
        const resultsText = await page.locator('.results-count').textContent();
        const match = resultsText.match(/Showing (\d+)/);
        if (!match) return false;
        const shown = parseInt(match[1]);
        return shown === expected.length;
      });
    }

    // ========== 3. K8S SEARCH TESTS ==========
    console.log('\n--- Testing K8s Search Functionality ---');

    // Reset filters
    await page.locator('[data-testid="tech-mendix"]').click();
    await page.waitForTimeout(100);
    await page.locator('[data-testid="tech-custom"]').click();
    await page.waitForTimeout(200);

    const k8sSearchTerms = [
      { term: 'Azure', expectedMin: 2 },  // Azure AKS, Azure Arc
      { term: 'EKS', expectedMin: 2 },    // Amazon EKS, EKS Anywhere (search by name not tag)
      { term: 'Red Hat', expectedMin: 1 },
      { term: 'K3s', expectedMin: 1 },
      { term: 'Rancher', expectedMin: 1 },
      { term: 'xyz123', expectedMin: 0 }, // No results
    ];

    for (const search of k8sSearchTerms) {
      await runner.test('K8s-Search', `Search "${search.term}" returns ${search.expectedMin}+ results`, async () => {
        await page.locator('.search-input').fill(search.term);
        await page.waitForTimeout(200);
        const resultsText = await page.locator('.results-count').textContent();
        const match = resultsText.match(/Showing (\d+)/);
        if (!match) return false;
        const shown = parseInt(match[1]);
        await page.locator('.search-input').fill(''); // Clear search
        await page.waitForTimeout(100);
        return shown >= search.expectedMin;
      });
    }

    // ========== 4. VM HYPERVISOR TESTS ==========
    console.log('\n--- Testing VM Hypervisors (15 hypervisors) ---');

    // Switch to VMs
    await page.locator('[data-testid="platform-vm"]').click();
    await page.waitForTimeout(300);

    for (const hyp of VM_HYPERVISORS) {
      // Test hypervisor exists
      await runner.test('VM-Hyp', `${hyp.name} exists`, async () => {
        return await page.locator(`[data-testid="hyp-${hyp.id}"]`).count() > 0;
      });

      // Test hypervisor can be selected
      await runner.test('VM-Hyp', `${hyp.name} can be selected`, async () => {
        await page.locator(`[data-testid="hyp-${hyp.id}"]`).click();
        await page.waitForTimeout(100);
        return await page.locator(`[data-testid="hyp-${hyp.id}"].selected`).count() > 0;
      });

      // Test hypervisor shows correct type
      await runner.test('VM-Hyp', `${hyp.name} shows type "${hyp.type}"`, async () => {
        const typeText = await page.locator(`[data-testid="hyp-${hyp.id}"] .dist-type`).textContent();
        return typeText.includes(hyp.type);
      });
    }

    // ========== 5. VM FILTER TAG TESTS ==========
    console.log('\n--- Testing VM Filter Tags ---');

    for (const tag of VM_FILTER_TAGS) {
      // Reset filters
      await page.locator('[data-testid="tech-mendix"]').click();
      await page.waitForTimeout(100);
      await page.locator('[data-testid="tech-custom"]').click();
      await page.waitForTimeout(200);

      await runner.test('VM-Filter', `Tag "${tag}" can be activated`, async () => {
        const tagEl = page.locator(`.tag:has-text("${tag}")`).first();
        if (await tagEl.count() === 0) return false;
        await tagEl.click();
        await page.waitForTimeout(100);
        return await page.locator(`.tag-active:has-text("${tag}")`).count() > 0;
      });

      await runner.test('VM-Filter', `Tag "${tag}" filters results correctly`, async () => {
        const expected = VM_HYPERVISORS.filter(h => h.tags.includes(tag));
        const resultsText = await page.locator('.results-count').textContent();
        const match = resultsText.match(/Showing (\d+)/);
        if (!match) return false;
        const shown = parseInt(match[1]);
        return shown === expected.length;
      });
    }

    // ========== 6. VM SEARCH TESTS ==========
    console.log('\n--- Testing VM Search Functionality ---');

    // Reset filters
    await page.locator('[data-testid="tech-mendix"]').click();
    await page.waitForTimeout(100);
    await page.locator('[data-testid="tech-custom"]').click();
    await page.waitForTimeout(200);

    const vmSearchTerms = [
      { term: 'VMware', expectedMin: 2 },
      { term: 'Azure', expectedMin: 2 },
      { term: 'Proxmox', expectedMin: 1 },
      { term: 'Nutanix', expectedMin: 1 },
      { term: 'xyz123', expectedMin: 0 },
    ];

    for (const search of vmSearchTerms) {
      await runner.test('VM-Search', `Search "${search.term}" returns ${search.expectedMin}+ results`, async () => {
        await page.locator('.search-input').fill(search.term);
        await page.waitForTimeout(200);
        const resultsText = await page.locator('.results-count').textContent();
        const match = resultsText.match(/Showing (\d+)/);
        if (!match) return false;
        const shown = parseInt(match[1]);
        await page.locator('.search-input').fill('');
        await page.waitForTimeout(100);
        return shown >= search.expectedMin;
      });
    }

    // ========== 7. MENDIX K8S OPTIONS TESTS ==========
    console.log('\n--- Testing Mendix + K8s Options ---');

    await page.locator('[data-testid="platform-k8s"]').click();
    await page.waitForTimeout(200);
    await page.locator('[data-testid="tech-mendix"]').click();
    await page.waitForTimeout(300);

    for (const category of MENDIX_K8S_CATEGORIES) {
      await runner.test('Mendix-K8s', `Category "${category}" exists`, async () => {
        return await page.locator(`[data-testid="mendix-k8s-${category}"]`).count() > 0;
      });

      await runner.test('Mendix-K8s', `Category "${category}" can be selected`, async () => {
        await page.locator(`[data-testid="mendix-k8s-${category}"]`).click();
        await page.waitForTimeout(200);
        return await page.locator(`[data-testid="mendix-k8s-${category}"].selected`).count() > 0;
      });
    }

    // Test Private Cloud providers
    await page.locator('[data-testid="mendix-k8s-private-cloud"]').click();
    await page.waitForTimeout(200);

    for (const provider of MENDIX_PRIVATE_CLOUD_PROVIDERS) {
      await runner.test('Mendix-K8s-Provider', `Provider "${provider}" exists`, async () => {
        return await page.locator(`[data-testid="mendix-provider-${provider}"]`).count() > 0;
      });

      await runner.test('Mendix-K8s-Provider', `Provider "${provider}" can be selected`, async () => {
        await page.locator(`[data-testid="mendix-provider-${provider}"]`).click();
        await page.waitForTimeout(100);
        return await page.locator(`[data-testid="mendix-provider-${provider}"].selected`).count() > 0;
      });
    }

    // Test Other K8s options
    await page.locator('[data-testid="mendix-k8s-other-k8s"]').click();
    await page.waitForTimeout(200);

    for (const dist of MENDIX_OTHER_K8S) {
      await runner.test('Mendix-K8s-Other', `Distribution "${dist}" exists`, async () => {
        return await page.locator(`[data-testid="mendix-other-${dist}"]`).count() > 0;
      });
    }

    // ========== 8. MENDIX VM OPTIONS TESTS ==========
    console.log('\n--- Testing Mendix + VM Options ---');

    await page.locator('[data-testid="platform-vm"]').click();
    await page.waitForTimeout(300);

    for (const option of MENDIX_VM_OPTIONS) {
      await runner.test('Mendix-VM', `Option "${option}" exists`, async () => {
        return await page.locator(`[data-testid="mendix-vm-${option}"]`).count() > 0;
      });

      await runner.test('Mendix-VM', `Option "${option}" can be selected`, async () => {
        await page.locator(`[data-testid="mendix-vm-${option}"]`).click();
        await page.waitForTimeout(100);
        return await page.locator(`[data-testid="mendix-vm-${option}"].selected`).count() > 0;
      });
    }

    // ========== 9. OUTSYSTEMS K8S OPTIONS TESTS ==========
    console.log('\n--- Testing OutSystems + K8s Options ---');

    await page.locator('[data-testid="platform-k8s"]').click();
    await page.waitForTimeout(200);
    await page.locator('[data-testid="tech-outsystems"]').click();
    await page.waitForTimeout(300);

    for (const option of OUTSYSTEMS_K8S_OPTIONS) {
      await runner.test('OutSystems-K8s', `Option "${option}" exists`, async () => {
        return await page.locator(`[data-testid="outsystems-k8s-${option}"]`).count() > 0;
      });

      await runner.test('OutSystems-K8s', `Option "${option}" can be selected`, async () => {
        await page.locator(`[data-testid="outsystems-k8s-${option}"]`).click();
        await page.waitForTimeout(100);
        return await page.locator(`[data-testid="outsystems-k8s-${option}"].selected`).count() > 0;
      });
    }

    // ========== 10. OUTSYSTEMS VM OPTIONS TESTS ==========
    console.log('\n--- Testing OutSystems + VM Options ---');

    await page.locator('[data-testid="platform-vm"]').click();
    await page.waitForTimeout(300);

    for (const option of OUTSYSTEMS_VM_OPTIONS) {
      await runner.test('OutSystems-VM', `Option "${option}" exists`, async () => {
        return await page.locator(`[data-testid="outsystems-vm-${option}"]`).count() > 0;
      });

      await runner.test('OutSystems-VM', `Option "${option}" can be selected`, async () => {
        await page.locator(`[data-testid="outsystems-vm-${option}"]`).click();
        await page.waitForTimeout(100);
        return await page.locator(`[data-testid="outsystems-vm-${option}"].selected`).count() > 0;
      });
    }

    // ========== 11. DEFAULT SELECTION TESTS ==========
    console.log('\n--- Testing Default Selections ---');

    // Reload to get fresh defaults
    await page.goto(BASE_URL + '/');
    await page.waitForLoadState('networkidle');
    await page.locator('[data-testid="btn-create-scenario"]').click();
    await page.waitForTimeout(1000);
    await page.locator('.panel-tab:has-text("Platform")').click();
    await page.waitForTimeout(300);

    await runner.test('Defaults', 'Mendix is default technology', async () => {
      return await page.locator('[data-testid="tech-mendix"].selected').count() > 0;
    });

    await runner.test('Defaults', 'K8s is default platform', async () => {
      return await page.locator('[data-testid="platform-k8s"].selected').count() > 0;
    });

    await runner.test('Defaults', 'Private Cloud is default for Mendix+K8s', async () => {
      return await page.locator('[data-testid="mendix-k8s-private-cloud"].selected').count() > 0;
    });

    // Test OutSystems defaults
    await page.locator('[data-testid="tech-outsystems"]').click();
    await page.waitForTimeout(200);
    await page.locator('[data-testid="platform-vm"]').click();
    await page.waitForTimeout(300);

    await runner.test('Defaults', 'Self-Hosted is default for OutSystems+VM', async () => {
      return await page.locator('[data-testid="outsystems-vm-self-hosted"].selected').count() > 0;
    });

    // Test Custom K8s defaults
    await page.locator('[data-testid="platform-k8s"]').click();
    await page.waitForTimeout(200);
    await page.locator('[data-testid="tech-custom"]').click();
    await page.waitForTimeout(300);

    await runner.test('Defaults', 'OpenShift is default for Custom+K8s', async () => {
      return await page.locator('[data-testid="dist-openshift"].selected').count() > 0;
    });

    // Test Custom VM defaults
    await page.locator('[data-testid="platform-vm"]').click();
    await page.waitForTimeout(300);

    await runner.test('Defaults', 'VMware vSphere is default for Custom+VM', async () => {
      return await page.locator('[data-testid="hyp-vmware-vsphere"].selected').count() > 0;
    });

    // ========== 12. TECHNOLOGY + PLATFORM MATRIX TESTS ==========
    console.log('\n--- Testing Technology + Platform Matrix ---');

    for (const tech of TECHNOLOGIES) {
      for (const platform of PLATFORMS) {
        await runner.test('Matrix', `${tech} + ${platform} shows correct section`, async () => {
          // Navigate
          await page.locator(`[data-testid="platform-${platform}"]`).click();
          await page.waitForTimeout(200);
          await page.locator(`[data-testid="tech-${tech}"]`).click();
          await page.waitForTimeout(300);

          // Verify correct section is shown
          if (tech === 'mendix' && platform === 'k8s') {
            return await page.locator('[data-testid="mendix-k8s-options"]').count() > 0;
          } else if (tech === 'mendix' && platform === 'vm') {
            return await page.locator('[data-testid="mendix-vm-options"]').count() > 0;
          } else if (tech === 'outsystems' && platform === 'k8s') {
            return await page.locator('[data-testid="outsystems-k8s-options"]').count() > 0;
          } else if (tech === 'outsystems' && platform === 'vm') {
            return await page.locator('[data-testid="outsystems-vm-options"]').count() > 0;
          } else if (tech === 'custom' && platform === 'k8s') {
            return await page.locator('.dist-grid').count() > 0 &&
                   await page.locator('.section-title:has-text("K8s Distribution")').count() > 0;
          } else if (tech === 'custom' && platform === 'vm') {
            return await page.locator('.dist-grid').count() > 0 &&
                   await page.locator('.section-title:has-text("Hypervisor")').count() > 0;
          }
          return false;
        });
      }
    }

    // ========== 13. MULTI-TAG FILTER COMBINATION TESTS ==========
    console.log('\n--- Testing Multi-Tag Filter Combinations ---');

    // Switch to Custom + K8s
    await page.locator('[data-testid="platform-k8s"]').click();
    await page.waitForTimeout(200);
    await page.locator('[data-testid="tech-custom"]').click();
    await page.waitForTimeout(300);

    // Test On-Prem + Enterprise
    await runner.test('Multi-Filter', 'On-Prem + Enterprise shows 4 results', async () => {
      // Reset
      await page.locator('[data-testid="tech-mendix"]').click();
      await page.waitForTimeout(100);
      await page.locator('[data-testid="tech-custom"]').click();
      await page.waitForTimeout(200);

      await page.locator('.tag:has-text("On-Prem")').first().click();
      await page.waitForTimeout(100);
      await page.locator('.tag:has-text("Enterprise")').first().click();
      await page.waitForTimeout(100);

      const resultsText = await page.locator('.results-count').textContent();
      const match = resultsText.match(/Showing (\d+)/);
      return match && parseInt(match[1]) === 4; // OpenShift, Tanzu, D2iQ, Charmed
    });

    // Test Cloud + Managed (no Azure tag in filter UI - it's only in data)
    await runner.test('Multi-Filter', 'Cloud + Managed shows 8 managed K8s distributions', async () => {
      await page.locator('[data-testid="tech-mendix"]').click();
      await page.waitForTimeout(100);
      await page.locator('[data-testid="tech-custom"]').click();
      await page.waitForTimeout(200);

      await page.locator('.tag:has-text("Cloud")').first().click();
      await page.waitForTimeout(100);
      await page.locator('.tag:has-text("Managed")').first().click();
      await page.waitForTimeout(100);

      const resultsText = await page.locator('.results-count').textContent();
      const match = resultsText.match(/Showing (\d+)/);
      return match && parseInt(match[1]) === 8; // All 8 managed cloud K8s
    });

    // Test Needs-Sizing filter
    await runner.test('Multi-Filter', 'Needs-Sizing shows 12 self-hosted K8s options', async () => {
      await page.locator('[data-testid="tech-mendix"]').click();
      await page.waitForTimeout(100);
      await page.locator('[data-testid="tech-custom"]').click();
      await page.waitForTimeout(200);

      await page.locator('.tag:has-text("Needs-Sizing")').first().click();
      await page.waitForTimeout(100);

      const resultsText = await page.locator('.results-count').textContent();
      const match = resultsText.match(/Showing (\d+)/);
      return match && parseInt(match[1]) === 12; // All non-managed options
    });

    // Print summary
    console.log('');
    const success = runner.printSummary();

    // Save screenshot
    await page.screenshot({ path: '/tmp/comprehensive-test-final.png' });
    console.log('Final screenshot saved to /tmp/comprehensive-test-final.png');

    return success;

  } catch (error) {
    console.error('Test suite error:', error.message);
    await page.screenshot({ path: '/tmp/comprehensive-test-error.png' });
    return false;
  } finally {
    await browser.close();
  }
}

// Run tests
runAllTests().then(success => {
  process.exit(success ? 0 : 1);
});
