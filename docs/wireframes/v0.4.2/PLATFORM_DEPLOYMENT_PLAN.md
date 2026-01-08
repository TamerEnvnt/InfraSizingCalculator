# Platform & Deployment Options Expansion Plan

> **Purpose**: This document captures all research findings and planned changes for expanding the Technology + Platform combinations in the Infrastructure Sizing Calculator. Created to persist context across session compactions.

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Research Findings](#research-findings)
4. [Recommended Changes](#recommended-changes)
5. [Implementation Plan](#implementation-plan)
6. [Validation Checklist](#validation-checklist)

---

## Executive Summary

The V4 slide panel implements a dynamic flow where **Technology selection drives deployment options**. This plan expands coverage to include:
- All major K8s distributions (managed + self-hosted)
- All major VM hypervisors and cloud VM providers
- Correct defaults based on official vendor documentation
- Proper categorization for sizing relevance (managed vs self-hosted)

### Key Insight for Sizing Calculator Context
**Managed cloud services (AKS, EKS, GKE, etc.) don't require infrastructure sizing** - the cloud provider handles it. The calculator is most relevant for:
- Self-hosted K8s distributions
- On-premises VM hypervisors
- Private cloud deployments

---

## Current State Analysis

### File Location
`src/InfraSizingCalculator/Components/V4/Panels/PlatformConfigPanel.razor`

### Current K8s Distributions (12 total)

| ID | Name | Category | Tags |
|---|---|---|---|
| azure-aks | Azure AKS | Managed | Cloud, Managed, Azure |
| amazon-eks | Amazon EKS | Managed | Cloud, Managed, AWS |
| google-gke | Google GKE | Managed | Cloud, Managed, GCP |
| digitalocean-doks | DigitalOcean DOKS | Managed | Cloud, Managed |
| linode-lke | Linode LKE | Managed | Cloud, Managed |
| oracle-oke | Oracle OKE | Managed | Cloud, Managed |
| openshift | Red Hat OpenShift | Enterprise | On-Prem, Enterprise |
| rancher-rke2 | Rancher RKE2 | Self-Hosted | On-Prem, Self-Hosted |
| k3s | K3s | Lightweight | On-Prem, Lightweight |
| vanilla-k8s | Vanilla Kubernetes | Self-Hosted | On-Prem, Self-Hosted |
| microk8s | MicroK8s | Lightweight | On-Prem, Lightweight |
| tanzu | VMware Tanzu | Enterprise | On-Prem, Enterprise |

### Current VM Hypervisors (9 total)

| ID | Name | Category | Tags |
|---|---|---|---|
| vmware-vsphere | VMware vSphere | Enterprise | On-Prem, Enterprise |
| vmware-cloud | VMware Cloud | Managed | Cloud, Managed |
| hyperv | Microsoft Hyper-V | Enterprise | On-Prem, Enterprise |
| azure-vm | Azure Virtual Machines | Managed | Cloud, Managed, Azure |
| aws-ec2 | AWS EC2 | Managed | Cloud, Managed, AWS |
| gcp-compute | Google Compute Engine | Managed | Cloud, Managed, GCP |
| proxmox | Proxmox VE | Open Source | On-Prem, Open-Source |
| kvm-libvirt | KVM/libvirt | Open Source | On-Prem, Open-Source |
| nutanix | Nutanix AHV | Enterprise | On-Prem, Enterprise |

### Current Technology Combinations

#### 1. Mendix + K8s
- Mendix Cloud (fully managed - no sizing needed)
- Private Cloud (AKS, EKS, GKE, OpenShift - officially supported)
- Other K8s (custom distributions)

#### 2. Mendix + VMs
- Server (on-premises Windows/Linux)
- StackIT (German cloud)
- SAP BTP (Business Technology Platform)

#### 3. OutSystems + K8s
- OutSystems Cloud (managed - no sizing needed)
- ODC (OutSystems Developer Cloud - cloud-native)

#### 4. OutSystems + VMs
- OutSystems Cloud (managed)
- Self-Hosted (on-premises)

#### 5. Custom + K8s
- Full distribution selector with search and filtering

#### 6. Custom + VMs
- Full hypervisor selector with search and filtering

---

## Research Findings

### Mendix Official Documentation

**Officially Supported K8s for Mendix Private Cloud:**
1. Azure AKS
2. Amazon EKS
3. Google GKE
4. Red Hat OpenShift

**Mendix Deployment Options:**
| Option | Description | Sizing Needed? |
|---|---|---|
| Mendix Cloud | Fully managed by Mendix | No |
| Private Cloud | Customer-managed K8s | Yes |
| SAP BTP | SAP Business Technology Platform | Partial |
| StackIT | German sovereign cloud | Yes |

### OutSystems Official Documentation

**OutSystems Platform Options:**
| Platform | Description | Sizing Needed? |
|---|---|---|
| ODC (Developer Cloud) | Cloud-native, container-based | No (managed) |
| O11 Cloud | Traditional OutSystems Cloud | No (managed) |
| O11 Self-Hosted | Customer-managed servers | Yes |

**Key Insight**: For sizing calculator purposes, only "Self-Hosted O11" requires VM sizing.

### Kubernetes Distribution Market Analysis

#### Tier 1: Major Cloud Managed (No Sizing Needed)
| Distribution | Provider | Market Position |
|---|---|---|
| Azure AKS | Microsoft | ~30% managed K8s market |
| Amazon EKS | AWS | ~35% managed K8s market |
| Google GKE | Google | ~25% managed K8s market |

#### Tier 2: Enterprise Self-Managed (Sizing Needed)
| Distribution | Vendor | Target Market |
|---|---|---|
| Red Hat OpenShift | Red Hat/IBM | Enterprise, regulated industries |
| VMware Tanzu | Broadcom | VMware shops, hybrid cloud |
| Rancher RKE2 | SUSE | Multi-cluster management |
| D2iQ Konvoy | D2iQ | Air-gapped, high-security |

#### Tier 3: Lightweight/Edge (Sizing Needed)
| Distribution | Vendor | Use Case |
|---|---|---|
| K3s | SUSE/Rancher | Edge, IoT, resource-constrained |
| k0s | Mirantis | Minimal footprint |
| MicroK8s | Canonical | Developer, single-node |

#### Tier 4: Secondary Cloud Managed
| Distribution | Provider | Notes |
|---|---|---|
| DigitalOcean DOKS | DigitalOcean | SMB-focused |
| Linode LKE | Akamai | Developer-friendly |
| Oracle OKE | Oracle | Oracle ecosystem |
| Civo K8s | Civo | Fast provisioning |
| Vultr K8s | Vultr | Edge locations |

#### Tier 5: Hybrid/Multi-Cloud
| Distribution | Vendor | Notes |
|---|---|---|
| EKS Anywhere | AWS | On-prem EKS |
| Azure Arc | Microsoft | Multi-cloud K8s |
| Anthos | Google | GKE everywhere |
| OpenShift + ARO/ROSA | Red Hat | Managed OpenShift variants |

### VM Hypervisor Market Analysis

#### Market Share (On-Premises)
| Hypervisor | Market Share | Notes |
|---|---|---|
| VMware vSphere | ~41% | Enterprise leader |
| Microsoft Hyper-V | ~30% | Windows ecosystem |
| Nutanix AHV | ~8% | HCI bundled |
| KVM/QEMU | ~10% | Open source, basis for many |
| Proxmox VE | ~5% | Growing in SMB |
| Citrix XenServer | ~3% | Declining |

#### Cloud VM Providers
| Provider | Service | Notes |
|---|---|---|
| AWS | EC2 | Market leader |
| Azure | Virtual Machines | Enterprise adoption |
| Google Cloud | Compute Engine | Strong in ML/AI |
| Oracle Cloud | OCI Compute | Oracle workloads |
| DigitalOcean | Droplets | Developer-friendly |
| Vultr | Compute | Edge-optimized |

---

## Recommended Changes

### 1. Add Missing K8s Distributions

**To Add:**
```csharp
new("k0s", "k0s", "Lightweight", new[] { "On-Prem", "Lightweight" }),
new("eks-anywhere", "EKS Anywhere", "Hybrid", new[] { "On-Prem", "Hybrid", "AWS" }),
new("azure-arc", "Azure Arc-enabled K8s", "Hybrid", new[] { "Hybrid", "Azure" }),
new("anthos", "Google Anthos", "Hybrid", new[] { "Hybrid", "GCP" }),
new("civo", "Civo Kubernetes", "Managed", new[] { "Cloud", "Managed" }),
new("vultr-k8s", "Vultr Kubernetes", "Managed", new[] { "Cloud", "Managed" }),
new("d2iq-konvoy", "D2iQ Konvoy", "Enterprise", new[] { "On-Prem", "Enterprise", "Air-Gap" }),
new("charmed-k8s", "Charmed Kubernetes", "Enterprise", new[] { "On-Prem", "Enterprise" }),
```

**New Total: 20 K8s distributions**

### 2. Add Missing VM Hypervisors

**To Add:**
```csharp
new("oracle-oci", "Oracle Cloud Infrastructure", "Managed", new[] { "Cloud", "Managed", "Oracle" }),
new("digitalocean-droplets", "DigitalOcean Droplets", "Managed", new[] { "Cloud", "Managed" }),
new("vultr-compute", "Vultr Compute", "Managed", new[] { "Cloud", "Managed" }),
new("azure-stack-hci", "Azure Stack HCI", "Hybrid", new[] { "On-Prem", "Hybrid", "Azure" }),
new("xenserver", "Citrix XenServer", "Enterprise", new[] { "On-Prem", "Enterprise" }),
new("ovirt", "oVirt/Red Hat Virtualization", "Open Source", new[] { "On-Prem", "Open-Source" }),
```

**New Total: 15 VM hypervisors**

### 3. Update Mendix K8s Options

Keep current options but clarify sizing relevance:
- **Mendix Cloud**: Add "(No sizing needed)" subtitle
- **Private Cloud**: Emphasize this is the sizing-relevant option
- **Other K8s**: For unsupported distributions

### 4. Update OutSystems Options

**Current Problem**: "OutSystems Cloud" is default but doesn't need sizing.

**Fix for OutSystems + VMs:**
- Default to "Self-Hosted" instead of "Cloud"
- Add clear indication of which options need sizing

### 5. Add New Tag Category: "Needs Sizing"

Add a new filter tag to help users find options relevant to the calculator:
- `Needs-Sizing` - for self-hosted/on-prem options
- `Managed` - for fully managed cloud options (no sizing needed)

### 6. Update Default Presets

| Combination | Current Default | Recommended Default | Rationale |
|---|---|---|---|
| Mendix + K8s | Mendix Cloud | Private Cloud | Sizing relevant |
| Mendix + VMs | Server | Server | Correct |
| OutSystems + K8s | Cloud | ODC | User choice |
| OutSystems + VMs | Cloud | Self-Hosted | Sizing relevant |
| Custom + K8s | Azure AKS | OpenShift | Enterprise focus |
| Custom + VMs | VMware vSphere | VMware vSphere | Correct |

---

## Implementation Plan

### Phase 1: Wireframe Updates (HTML mockups)

**Files to modify:**
- `docs/wireframes/v0.4.2/html/slide-panel-platform.html`
- `docs/wireframes/v0.4.2/html/slide-panel-k8s-distribution.html`
- `docs/wireframes/v0.4.2/html/slide-panel-vm-hypervisor.html`

**Changes:**
1. Add missing K8s distributions to the distribution grid
2. Add missing VM hypervisors to the hypervisor grid
3. Add "Needs Sizing" badge/tag
4. Update filter tags to include new categories

### Phase 2: Component Updates

**File:** `PlatformConfigPanel.razor`

**Changes:**
1. Expand `_k8sDistributions` list (add 8 new)
2. Expand `_vmHypervisors` list (add 6 new)
3. Add `NeedsSizing` property to `DistributionOption` record
4. Update default selections for each combination
5. Add visual indicator for sizing-relevant options

### Phase 3: Business Rules Updates

**File:** `docs/business/business-rules.md`

**Changes:**
1. Add BR-D rules for new distributions
2. Update sizing algorithms for new platforms
3. Document which platforms require sizing vs managed

### Phase 4: Database Updates

**File:** Distribution configurations in database

**Changes:**
1. Add new distribution entries
2. Set correct defaults for each distribution
3. Configure resource multipliers

### Phase 5: Testing

1. Update E2E tests for new options
2. Add unit tests for new distributions
3. Validate sizing calculations for each platform

---

## Validation Checklist

### K8s Distributions Coverage

- [x] Azure AKS
- [x] Amazon EKS
- [x] Google GKE
- [x] Red Hat OpenShift
- [x] VMware Tanzu
- [x] Rancher RKE2
- [x] K3s
- [x] MicroK8s
- [x] Vanilla Kubernetes
- [x] DigitalOcean DOKS
- [x] Linode LKE
- [x] Oracle OKE
- [ ] k0s
- [ ] EKS Anywhere
- [ ] Azure Arc
- [ ] Google Anthos
- [ ] Civo
- [ ] Vultr K8s
- [ ] D2iQ Konvoy
- [ ] Charmed Kubernetes

### VM Hypervisors Coverage

- [x] VMware vSphere
- [x] VMware Cloud
- [x] Microsoft Hyper-V
- [x] Azure VMs
- [x] AWS EC2
- [x] GCP Compute Engine
- [x] Proxmox VE
- [x] KVM/libvirt
- [x] Nutanix AHV
- [ ] Oracle OCI
- [ ] DigitalOcean Droplets
- [ ] Vultr Compute
- [ ] Azure Stack HCI
- [ ] Citrix XenServer
- [ ] oVirt

### Mendix Compatibility

- [x] Mendix Cloud option
- [x] Private Cloud with AKS/EKS/GKE/OpenShift
- [x] Other K8s for unsupported distributions
- [x] Server deployment (VMs)
- [x] StackIT deployment
- [x] SAP BTP deployment

### OutSystems Compatibility

- [x] OutSystems Cloud (O11)
- [x] ODC (Developer Cloud)
- [x] Self-Hosted (O11)

---

## Appendix: Official Documentation Sources

### Mendix
- Mendix Private Cloud Supported Platforms: https://docs.mendix.com/developerportal/deploy/private-cloud-supported-environments/
- Mendix Deployment Options: https://docs.mendix.com/developerportal/deploy/

### OutSystems
- OutSystems Platform Editions: https://www.outsystems.com/platform/
- OutSystems Cloud vs Self-Hosted: https://success.outsystems.com/

### Kubernetes
- CNCF Landscape: https://landscape.cncf.io/
- K8s Distributions Comparison: Various vendor docs

### VM Hypervisors
- VMware vSphere: https://www.vmware.com/products/vsphere.html
- Microsoft Hyper-V: https://docs.microsoft.com/virtualization/hyper-v-on-windows/
- Proxmox VE: https://www.proxmox.com/proxmox-ve

---

## Change Log

| Date | Author | Change |
|---|---|---|
| 2025-01-01 | Claude Opus 4.5 | Initial plan creation |

---

*This document should be updated as implementation progresses.*
