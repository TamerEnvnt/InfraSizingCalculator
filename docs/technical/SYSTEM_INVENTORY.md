# Infrastructure Sizing Calculator - System Inventory

## Complete Inventory for Documentation Study

This document lists all distributions, cloud providers, technologies, and deployment options implemented in the system, along with the official documentation URLs that need to be studied for business logic validation.

---

## 1. Platform Types (2)

| Type | Description | Technologies |
|------|-------------|--------------|
| **Native** | Traditional development platforms | .NET, Java, Node.js, Python, Go |
| **LowCode** | Enterprise low-code platforms | Mendix, OutSystems |

---

## 2. Deployment Models (2)

| Model | Description |
|-------|-------------|
| **Kubernetes** | Container orchestration |
| **VMs** | Virtual Machine deployments |

---

## 3. Technologies (7)

### Native Technologies (5)

| Technology | Vendor | Official Docs |
|------------|--------|---------------|
| **.NET** | Microsoft | https://docs.microsoft.com/dotnet |
| **Java** | Oracle/OpenJDK | https://docs.oracle.com/javase |
| **Node.js** | OpenJS Foundation | https://nodejs.org/docs |
| **Python** | PSF | https://docs.python.org |
| **Go** | Google | https://go.dev/doc |

### LowCode Technologies (2)

| Technology | Vendor | Official Docs |
|------------|--------|---------------|
| **Mendix** | Siemens | https://docs.mendix.com |
| **OutSystems** | OutSystems | https://www.outsystems.com/help |

---

## 4. Kubernetes Distributions (46 total)

### 4.1 Self-Managed On-Premises (8)

| Distribution | Vendor | Official Docs |
|--------------|--------|---------------|
| **OpenShift** (On-Prem) | Red Hat | https://docs.openshift.com |
| **Kubernetes** (Vanilla) | CNCF | https://kubernetes.io/docs |
| **Rancher** | SUSE | https://ranchermanager.docs.rancher.com |
| **RKE2** | SUSE | https://docs.rke2.io |
| **K3s** | SUSE | https://docs.k3s.io |
| **MicroK8s** | Canonical | https://microk8s.io/docs |
| **Charmed Kubernetes** | Canonical | https://ubuntu.com/kubernetes/docs |
| **VMware Tanzu** | VMware | https://docs.vmware.com/en/VMware-Tanzu |

### 4.2 OpenShift Cloud Variants (4)

| Distribution | Vendor/Cloud | Official Docs |
|--------------|--------------|---------------|
| **ROSA** | Red Hat / AWS | https://docs.openshift.com/rosa |
| **ARO** | Red Hat / Azure | https://docs.openshift.com/aro |
| **OpenShift Dedicated** | Red Hat / GCP | https://docs.openshift.com/dedicated |
| **OpenShift IBM** | Red Hat / IBM | https://cloud.ibm.com/docs/openshift |

### 4.3 Rancher/SUSE Cloud Variants (4)

| Distribution | Cloud | Official Docs |
|--------------|-------|---------------|
| **Rancher Hosted** | Any | https://ranchermanager.docs.rancher.com |
| **Rancher on EKS** | AWS | https://ranchermanager.docs.rancher.com/how-to-guides/new-user-guides/kubernetes-clusters-in-rancher-setup/set-up-clusters-from-hosted-kubernetes-providers/eks |
| **Rancher on AKS** | Azure | https://ranchermanager.docs.rancher.com/how-to-guides/new-user-guides/kubernetes-clusters-in-rancher-setup/set-up-clusters-from-hosted-kubernetes-providers/aks |
| **Rancher on GKE** | GCP | https://ranchermanager.docs.rancher.com/how-to-guides/new-user-guides/kubernetes-clusters-in-rancher-setup/set-up-clusters-from-hosted-kubernetes-providers/gke |

### 4.4 Tanzu Cloud Variants (4)

| Distribution | Cloud | Official Docs |
|--------------|-------|---------------|
| **Tanzu Cloud** | Generic | https://docs.vmware.com/en/VMware-Tanzu-Kubernetes-Grid |
| **Tanzu on AWS** | AWS | https://docs.vmware.com/en/VMware-Tanzu-Kubernetes-Grid |
| **Tanzu on Azure** | Azure | https://docs.vmware.com/en/VMware-Tanzu-Kubernetes-Grid |
| **Tanzu on GCP** | GCP | https://docs.vmware.com/en/VMware-Tanzu-Kubernetes-Grid |

### 4.5 Canonical/Ubuntu Cloud Variants (6)

| Distribution | Cloud | Official Docs |
|--------------|-------|---------------|
| **Charmed K8s on AWS** | AWS | https://ubuntu.com/kubernetes/docs/aws |
| **Charmed K8s on Azure** | Azure | https://ubuntu.com/kubernetes/docs/azure |
| **Charmed K8s on GCP** | GCP | https://ubuntu.com/kubernetes/docs/gcp |
| **MicroK8s on AWS** | AWS | https://microk8s.io/docs/aws |
| **MicroK8s on Azure** | Azure | https://microk8s.io/docs/azure |
| **MicroK8s on GCP** | GCP | https://microk8s.io/docs/gcp |

### 4.6 K3s Cloud Variants (3)

| Distribution | Cloud | Official Docs |
|--------------|-------|---------------|
| **K3s on AWS** | AWS | https://docs.k3s.io |
| **K3s on Azure** | Azure | https://docs.k3s.io |
| **K3s on GCP** | GCP | https://docs.k3s.io |

### 4.7 RKE2 Cloud Variants (3)

| Distribution | Cloud | Official Docs |
|--------------|-------|---------------|
| **RKE2 on AWS** | AWS | https://docs.rke2.io |
| **RKE2 on Azure** | Azure | https://docs.rke2.io |
| **RKE2 on GCP** | GCP | https://docs.rke2.io |

### 4.8 Major Cloud Managed Kubernetes (8)

| Distribution | Cloud | Official Docs | Pricing Docs |
|--------------|-------|---------------|--------------|
| **EKS** | AWS | https://docs.aws.amazon.com/eks | https://aws.amazon.com/eks/pricing |
| **AKS** | Azure | https://docs.microsoft.com/azure/aks | https://azure.microsoft.com/pricing/details/kubernetes-service |
| **GKE** | GCP | https://cloud.google.com/kubernetes-engine/docs | https://cloud.google.com/kubernetes-engine/pricing |
| **OKE** | Oracle | https://docs.oracle.com/iaas/Content/ContEng | https://www.oracle.com/cloud/compute/container-engine-kubernetes |
| **IKS** | IBM | https://cloud.ibm.com/docs/containers | https://cloud.ibm.com/kubernetes/catalog/about |
| **ACK** | Alibaba | https://www.alibabacloud.com/help/ack | https://www.alibabacloud.com/product/kubernetes/pricing |
| **TKE** | Tencent | https://www.tencentcloud.com/document/product/457 | https://www.tencentcloud.com/pricing/tke |
| **CCE** | Huawei | https://support.huaweicloud.com/cce | https://www.huaweicloud.com/pricing.html |

### 4.9 Developer/Smaller Cloud K8s (6)

| Distribution | Cloud | Official Docs | Pricing Docs |
|--------------|-------|---------------|--------------|
| **DOKS** | DigitalOcean | https://docs.digitalocean.com/products/kubernetes | https://www.digitalocean.com/pricing/kubernetes |
| **LKE** | Linode/Akamai | https://www.linode.com/docs/products/compute/kubernetes | https://www.linode.com/pricing/#kubernetes |
| **VKE** | Vultr | https://docs.vultr.com/vultr-kubernetes-engine | https://www.vultr.com/pricing/#kubernetes |
| **Hetzner K8s** | Hetzner | https://docs.hetzner.cloud | https://www.hetzner.com/cloud |
| **OVH Kubernetes** | OVHcloud | https://help.ovhcloud.com/csm/en-documentation-public-cloud-kubernetes | https://www.ovhcloud.com/en/public-cloud/kubernetes |
| **Scaleway Kapsule** | Scaleway | https://www.scaleway.com/en/docs/containers/kubernetes | https://www.scaleway.com/en/pricing |

---

## 5. Cloud Providers (20)

### 5.1 Major Cloud Providers (8)

| Provider | K8s Service | Official Docs | Pricing Calculator |
|----------|-------------|---------------|-------------------|
| **AWS** | EKS | https://docs.aws.amazon.com/eks | https://calculator.aws |
| **Azure** | AKS | https://docs.microsoft.com/azure/aks | https://azure.microsoft.com/pricing/calculator |
| **GCP** | GKE | https://cloud.google.com/kubernetes-engine/docs | https://cloud.google.com/products/calculator |
| **Oracle (OCI)** | OKE | https://docs.oracle.com/iaas/Content/ContEng | https://www.oracle.com/cloud/costestimator.html |
| **IBM Cloud** | IKS | https://cloud.ibm.com/docs/containers | https://cloud.ibm.com/estimator |
| **Alibaba Cloud** | ACK | https://www.alibabacloud.com/help/ack | https://www.alibabacloud.com/pricing-calculator |
| **Tencent Cloud** | TKE | https://www.tencentcloud.com/document/product/457 | https://buy.tencentcloud.com/calculator |
| **Huawei Cloud** | CCE | https://support.huaweicloud.com/cce | https://www.huaweicloud.com/pricing/calculator.html |

### 5.2 Managed OpenShift Services (4)

| Provider | Service | Official Docs | Pricing |
|----------|---------|---------------|---------|
| **ROSA** | Red Hat + AWS | https://docs.openshift.com/rosa | https://aws.amazon.com/rosa/pricing |
| **ARO** | Red Hat + Azure | https://docs.openshift.com/aro | https://azure.microsoft.com/pricing/details/openshift |
| **OSD** | Red Hat + GCP | https://docs.openshift.com/dedicated | https://www.redhat.com/en/technologies/cloud-computing/openshift/dedicated |
| **ROKS** | Red Hat + IBM | https://cloud.ibm.com/docs/openshift | https://cloud.ibm.com/docs/openshift?topic=openshift-costs |

### 5.3 Developer/Smaller Cloud Providers (6)

| Provider | K8s Service | Official Docs | Pricing |
|----------|-------------|---------------|---------|
| **DigitalOcean** | DOKS | https://docs.digitalocean.com/products/kubernetes | https://www.digitalocean.com/pricing/kubernetes |
| **Linode/Akamai** | LKE | https://www.linode.com/docs/products/compute/kubernetes | https://www.linode.com/pricing |
| **Vultr** | VKE | https://docs.vultr.com/vultr-kubernetes-engine | https://www.vultr.com/pricing |
| **Hetzner** | Hetzner K8s | https://docs.hetzner.cloud | https://www.hetzner.com/cloud |
| **OVHcloud** | OVH K8s | https://help.ovhcloud.com | https://www.ovhcloud.com/en/public-cloud |
| **Scaleway** | Kapsule | https://www.scaleway.com/en/docs/containers/kubernetes | https://www.scaleway.com/en/pricing |

### 5.4 Additional Providers (2)

| Provider | Type | Purpose |
|----------|------|---------|
| **Civo** | Cloud Provider | https://www.civo.com/docs |
| **Exoscale** | Cloud Provider | https://community.exoscale.com/documentation |

---

## 6. Mendix Deployment Options (12)

### 6.1 Mendix Cloud (SaaS) - 2 Types

| Type | Description | Official Docs |
|------|-------------|---------------|
| **SaaS (Multi-tenant)** | Shared Mendix Cloud | https://docs.mendix.com/developerportal/deploy/mendix-cloud-deploy |
| **Dedicated (Single-tenant)** | Single-tenant AWS VPC | https://docs.mendix.com/developerportal/deploy/mendix-cloud-dedicated |

### 6.2 Private Cloud (Officially Supported) - 5 Providers

| Provider | Description | Official Docs |
|----------|-------------|---------------|
| **Mendix on Azure** | Managed Azure deployment | https://docs.mendix.com/developerportal/deploy/private-cloud-azure-deployment |
| **Amazon EKS** | AWS EKS deployment | https://docs.mendix.com/developerportal/deploy/private-cloud |
| **Azure AKS** | Azure AKS deployment | https://docs.mendix.com/developerportal/deploy/private-cloud |
| **Google GKE** | GCP GKE deployment | https://docs.mendix.com/developerportal/deploy/private-cloud |
| **Red Hat OpenShift** | OpenShift deployment | https://docs.mendix.com/developerportal/deploy/private-cloud |

### 6.3 Private Cloud (Manual/Unsupported) - 4 Options

| Option | Description | Official Docs |
|--------|-------------|---------------|
| **Generic K8s** | Kubernetes 1.19+ | https://docs.mendix.com/developerportal/deploy/private-cloud |
| **Rancher/RKE2** | SUSE Rancher | https://docs.mendix.com/developerportal/deploy/private-cloud |
| **K3s** | Lightweight K8s | https://docs.mendix.com/developerportal/deploy/private-cloud |
| **Docker** | Docker standalone | https://docs.mendix.com/developerportal/deploy/docker-deploy |

### 6.4 Other Deployments (VMs/Partner Clouds) - 3 Options

| Option | Description | Official Docs |
|--------|-------------|---------------|
| **Server (VMs)** | Windows/Linux VMs + Docker | https://docs.mendix.com/developerportal/deploy/on-premises-design |
| **StackIT** | German sovereign cloud (Schwarz IT) | https://docs.mendix.com/developerportal/deploy/private-cloud-stackit |
| **SAP BTP** | SAP Business Technology Platform | https://docs.mendix.com/developerportal/deploy/sap-cloud-platform |

### 6.5 Mendix Pricing Components

| Component | Description | Official Source |
|-----------|-------------|-----------------|
| **Resource Packs** | Standard/Premium/Premium Plus | https://www.mendix.com/pricing |
| **K8s Environment Tiers** | Tiered pricing per environment | Mendix Pricebook |
| **GenAI Model Packs** | S/M/L AI token bundles | Mendix Commercial Docs |
| **User Licensing** | Internal/External users | Mendix Commercial Docs |

---

## 7. OutSystems Deployment Options

| Option | Description | Official Docs |
|--------|-------------|---------------|
| **OutSystems Cloud** | Managed PaaS | https://www.outsystems.com/evaluation-guide/outsystems-cloud-architecture |
| **OutSystems Private Cloud** | Self-managed | https://www.outsystems.com/evaluation-guide/outsystems-vs-on-premises |
| **OutSystems on Kubernetes** | K8s deployment | https://www.outsystems.com/forge/component-overview/12425/outsystems-cloud-deploy |

---

## 8. Summary Statistics

| Category | Count |
|----------|-------|
| Platform Types | 2 |
| Deployment Models | 2 |
| Technologies | 7 |
| K8s Distributions (Self-managed) | 8 |
| K8s Distributions (Cloud Variants) | 38 |
| **Total K8s Distributions** | **46** |
| Cloud Providers (Major) | 8 |
| Cloud Providers (OpenShift) | 4 |
| Cloud Providers (Smaller) | 8 |
| **Total Cloud Providers** | **20** |
| Mendix Deployment Options | 12 |
| OutSystems Deployment Options | 3 |

---

## 9. Documentation Study Task List

### Phase 1: Core Kubernetes Distributions (Priority: High)

| # | Task | URL | Status |
|---|------|-----|--------|
| 1.1 | Study OpenShift sizing requirements | https://docs.openshift.com/container-platform/latest/scalability_and_performance/recommended-cluster-scaling-practices.html | Pending |
| 1.2 | Study vanilla Kubernetes best practices | https://kubernetes.io/docs/setup/best-practices | Pending |
| 1.3 | Study Rancher/RKE2 requirements | https://ranchermanager.docs.rancher.com/reference-guides/rancher-manager-architecture/architecture-recommendations | Pending |
| 1.4 | Study K3s requirements | https://docs.k3s.io/installation/requirements | Pending |
| 1.5 | Study MicroK8s requirements | https://microk8s.io/docs/high-availability | Pending |
| 1.6 | Study Charmed K8s requirements | https://ubuntu.com/kubernetes/docs/high-availability | Pending |
| 1.7 | Study VMware Tanzu requirements | https://docs.vmware.com/en/VMware-Tanzu-Kubernetes-Grid/index.html | Pending |

### Phase 2: Major Cloud Provider K8s Services (Priority: High)

| # | Task | URL | Status |
|---|------|-----|--------|
| 2.1 | Study EKS pricing & sizing | https://aws.amazon.com/eks/pricing & https://docs.aws.amazon.com/eks/latest/best-practices | Pending |
| 2.2 | Study AKS pricing & sizing | https://azure.microsoft.com/pricing/details/kubernetes-service | Pending |
| 2.3 | Study GKE pricing & sizing | https://cloud.google.com/kubernetes-engine/pricing | Pending |
| 2.4 | Study OKE pricing & sizing | https://www.oracle.com/cloud/compute/container-engine-kubernetes/pricing | Pending |
| 2.5 | Study IKS pricing & sizing | https://cloud.ibm.com/kubernetes/catalog/about | Pending |

### Phase 3: Managed OpenShift Services (Priority: Medium)

| # | Task | URL | Status |
|---|------|-----|--------|
| 3.1 | Study ROSA pricing | https://aws.amazon.com/rosa/pricing | Pending |
| 3.2 | Study ARO pricing | https://azure.microsoft.com/pricing/details/openshift | Pending |
| 3.3 | Study OpenShift Dedicated pricing | https://www.redhat.com/en/technologies/cloud-computing/openshift/dedicated | Pending |

### Phase 4: Mendix Documentation (Priority: High)

| # | Task | URL | Status |
|---|------|-----|--------|
| 4.1 | Study Mendix Cloud deployment | https://docs.mendix.com/developerportal/deploy/mendix-cloud-deploy | Pending |
| 4.2 | Study Mendix Private Cloud | https://docs.mendix.com/developerportal/deploy/private-cloud | Pending |
| 4.3 | Study Mendix pricing guide | https://www.mendix.com/pricing | Pending |
| 4.4 | Study Mendix resource packs | Commercial documentation | Pending |
| 4.5 | Study Mendix on Azure | https://docs.mendix.com/developerportal/deploy/private-cloud-azure-deployment | Pending |
| 4.6 | Study Mendix Docker deployment | https://docs.mendix.com/developerportal/deploy/docker-deploy | Pending |

### Phase 5: OutSystems Documentation (Priority: Medium)

| # | Task | URL | Status |
|---|------|-----|--------|
| 5.1 | Study OutSystems architecture | https://www.outsystems.com/evaluation-guide/outsystems-architecture | Pending |
| 5.2 | Study OutSystems deployment options | https://www.outsystems.com/evaluation-guide/outsystems-deployment-options | Pending |
| 5.3 | Study OutSystems pricing | https://www.outsystems.com/pricing | Pending |

### Phase 6: Native Technologies (Priority: Low - for container sizing)

| # | Task | URL | Status |
|---|------|-----|--------|
| 6.1 | Study .NET container best practices | https://docs.microsoft.com/dotnet/architecture/microservices | Pending |
| 6.2 | Study Java container best practices | https://docs.oracle.com/javase/8/docs/technotes/guides/vm/gctuning | Pending |
| 6.3 | Study Node.js container best practices | https://nodejs.org/en/docs/guides/nodejs-docker-webapp | Pending |

### Phase 7: Smaller Cloud Providers (Priority: Low)

| # | Task | URL | Status |
|---|------|-----|--------|
| 7.1 | Study DigitalOcean DOKS pricing | https://www.digitalocean.com/pricing/kubernetes | Pending |
| 7.2 | Study Linode LKE pricing | https://www.linode.com/pricing | Pending |
| 7.3 | Study Vultr VKE pricing | https://www.vultr.com/pricing | Pending |
| 7.4 | Study Hetzner K8s pricing | https://www.hetzner.com/cloud | Pending |
| 7.5 | Study OVH Kubernetes pricing | https://www.ovhcloud.com/en/public-cloud | Pending |
| 7.6 | Study Scaleway Kapsule pricing | https://www.scaleway.com/en/pricing | Pending |

---

## 10. Next Steps

1. Review this inventory for completeness
2. Prioritize documentation study by business impact
3. Study each documentation source
4. Validate business logic against official sources
5. Update pricing models as needed
6. Document any discrepancies found

---

Last Updated: December 25, 2025
