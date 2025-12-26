# Business Requirements Document (BRD)

## Document Information

| Item | Value |
|------|-------|
| Project | Infrastructure Sizing Calculator |
| Version | 1.0 |
| Status | Draft |

---

## 1. Executive Summary

The Infrastructure Sizing Calculator is a tool designed to help organizations estimate infrastructure requirements for deploying applications. It supports two deployment models:

1. **Kubernetes Clusters** - Containerized deployments across 46 distribution options
2. **Virtual Machines** - Traditional VM-based deployments with technology-specific server roles

The tool provides accurate sizing recommendations based on application counts, technology choices, and organizational requirements for high availability and disaster recovery.

---

## 2. Business Objectives

### 2.1 Primary Objectives

| ID | Objective | Success Metric |
|----|-----------|----------------|
| OBJ-001 | Reduce infrastructure planning time | 80% reduction in sizing estimation time |
| OBJ-002 | Improve accuracy of sizing estimates | Within 15% of actual deployment requirements |
| OBJ-003 | Standardize infrastructure configurations | 100% adherence to vendor recommendations |
| OBJ-004 | Support multiple technology stacks | Support all major enterprise platforms |

### 2.2 Secondary Objectives

- Provide exportable sizing reports for procurement
- Enable what-if scenario comparisons
- Support both cloud-native and traditional deployments
- Maintain vendor-accurate specifications

---

## 3. Scope

### 3.1 In Scope

| Category | Items |
|----------|-------|
| Deployment Models | Kubernetes, Virtual Machines |
| K8s Distributions | 46 distributions including on-premises (OpenShift, Kubernetes, Rancher, K3s, MicroK8s, Charmed, Tanzu, RKE2), major cloud (EKS, AKS, GKE, OKE, IKS, ACK, TKE, CCE), OpenShift variants (ROSA, ARO), cloud variants, and developer platforms |
| Technologies | .NET, Java, Node.js, Python, Go, Mendix, OutSystems |
| Environments | Development, Test, Staging, Production, DR |
| Cluster Modes | Multi-Cluster, Shared Cluster, Per-Environment |
| Export Formats | CSV, JSON, Excel, HTML Diagram |
| Cost Estimation | On-premises hardware costs, cloud provider pricing (AWS, Azure, GCP, Oracle, IBM), Mendix platform licensing, growth projections |
| Scenario Management | Save, load, compare sizing scenarios with SQLite persistence |

### 3.2 Out of Scope

- Automated provisioning of infrastructure
- Network topology planning
- Storage performance optimization
- Security configuration recommendations

---

## 4. Stakeholders

| Role | Responsibility |
|------|----------------|
| Infrastructure Architects | Primary users for sizing calculations |
| DevOps Engineers | Use sizing data for cluster planning |
| Platform Engineers | Configure Kubernetes distributions |
| Procurement Teams | Use export reports for hardware acquisition |
| Finance | Budget planning based on sizing estimates |

---

## 5. Functional Requirements

### 5.1 Platform Selection (FR-001)

The system shall allow users to select:
- **Platform Type**: Native (code-first) or Low-Code
- **Deployment Model**: Kubernetes or Virtual Machines
- **Technology**: Platform-appropriate technology options

### 5.2 Kubernetes Sizing (FR-002)

The system shall calculate:
- Control Plane (Master) node counts and specifications
- Infrastructure node counts (OpenShift only)
- Worker node counts and specifications
- Total CPU, RAM, and Disk requirements per environment

### 5.3 Cluster Mode Configuration (FR-003)

**Multi-Cluster Mode:**
- Checkbox selection for each environment (Dev, Test, Stage, Prod, DR)
- Independent node configuration per checked cluster
- Each cluster calculated separately with dedicated control plane

**Shared Cluster Mode:**
- All enabled environments share a single cluster
- Namespace isolation between environments
- Single control plane with combined worker pool

**Per-Environment Mode:**
- Dropdown selection for specific environment (Dev, Test, Stage, Prod, DR)
- Calculate resources for single environment only
- Used for focused sizing analysis

### 5.4 VM Sizing (FR-004)

The system shall calculate:
- Technology-specific server roles
- Per-role CPU, RAM, and Disk requirements
- HA pattern multipliers
- Load balancer requirements

### 5.5 Export Functionality (FR-005)

The system shall export results to:
- CSV format for data analysis
- JSON format for API integration
- Excel format with formatted worksheets
- HTML diagram for visualization

---

## 6. Non-Functional Requirements

### 6.1 Performance

| Metric | Requirement |
|--------|-------------|
| Calculation Time | < 500ms for any configuration |
| Page Load Time | < 2 seconds initial load |
| Export Generation | < 5 seconds for Excel export |

### 6.2 Usability

- Single-page wizard interface
- Progressive disclosure of options
- Real-time calculation updates
- Mobile-responsive design

### 6.3 Accessibility

- Keyboard navigation support
- High contrast dark theme
- Screen reader compatible

### 6.4 Browser Support

- Chrome (latest 2 versions)
- Firefox (latest 2 versions)
- Safari (latest 2 versions)
- Edge (latest 2 versions)

---

## 7. Business Rules Summary

The system implements business rules for:

| Category | Rule IDs | Description |
|----------|----------|-------------|
| Master Nodes | BR-M001 to BR-M004 | Control plane sizing based on cluster size |
| Infrastructure | BR-I001 to BR-I006 | Infra nodes for OpenShift |
| Workers | BR-W001 to BR-W006 | Worker node calculations |
| Headroom | BR-H001 to BR-H009 | Capacity headroom by environment |
| Replicas | BR-R001 to BR-R005 | Pod replica settings |
| Technologies | BR-T001 to BR-T008 | Technology-specific specs |
| Distributions | BR-D001 to BR-D008 | Distribution-specific configs |
| Environments | BR-E001 to BR-E004 | Environment classifications |
| Validation | BR-V001 to BR-V007 | Input validation rules |

See [Business Rules](./business-rules.md) for complete rule documentation.

---

## 8. Assumptions

1. Users have basic understanding of Kubernetes or VM infrastructure
2. Application sizing tiers (Small, Medium, Large, XLarge) are consistent across organizations
3. Vendor specifications remain relatively stable between versions
4. Network bandwidth is not a primary constraint

---

## 9. Constraints

1. Must work in air-gapped environments (no external API dependencies)
2. Must not require database for operation
3. Must support export without server-side file storage

---

## 10. Dependencies

| Dependency | Type | Description |
|------------|------|-------------|
| .NET 10.0 Runtime | Technical | Required for Blazor Server |
| Modern Browser | Technical | JavaScript enabled |
| ClosedXML | Library | Excel export functionality |

---

## 11. Success Criteria

1. Calculator produces sizing estimates within 15% of actual deployments
2. All 46 K8s distributions have accurate specifications
3. All 7 technologies have accurate resource profiles
4. Export functionality works for all supported formats
5. UI wizard completes in under 2 minutes for typical use cases
6. Cost estimation provides accurate pricing for on-premises and cloud deployments
7. Scenario save/load functionality works reliably with SQLite persistence

---

## 12. Glossary

| Term | Definition |
|------|------------|
| Control Plane | Kubernetes master nodes managing cluster state |
| Worker Node | Kubernetes nodes running application workloads |
| Infra Node | OpenShift-specific nodes for platform services |
| Headroom | Extra capacity reserved for growth and burst |
| Overcommit | Ratio allowing more pods than physical capacity |
| HA | High Availability - redundancy for fault tolerance |
| DR | Disaster Recovery - secondary site for business continuity |
