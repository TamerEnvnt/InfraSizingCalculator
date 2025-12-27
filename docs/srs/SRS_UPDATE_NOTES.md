# SRS Update Notes - SYNC Phase

## Date: December 2025

This document tracks required updates to the SRS documentation following implementation changes.

---

## Required K8s HA/DR Updates

### New Use Case Required: UC-xxx Configure K8s HA/DR

**Description:** Configure High Availability and Disaster Recovery settings for Kubernetes clusters.

**Components:** `K8sHADRPanel.razor`

**Functionality:**
- Control Plane HA selection (Managed/Single/StackedHA/ExternalEtcd)
- Node Distribution configuration (SingleAZ/DualAZ/MultiAZ/MultiRegion)
- Availability Zone count selection
- DR Strategy selection (None/BackupRestore/WarmStandby/HotStandby/ActiveActive)
- Backup Strategy selection (None/Velero/Kasten/Portworx/CloudNative/Custom)
- Pod Disruption Budgets toggle
- Topology Spread Constraints toggle

**Input Data:**
| Field | Type | Options |
|-------|------|---------|
| ControlPlaneHA | Enum | Managed, Single, StackedHA, ExternalEtcd |
| ControlPlaneNodes | Integer | 3, 5, 7 (for HA modes) |
| NodeDistribution | Enum | SingleAZ, DualAZ, MultiAZ, MultiRegion |
| AvailabilityZones | Integer | 1-5 |
| DRPattern | Enum | None, BackupRestore, WarmStandby, HotStandby, ActiveActive |
| DRRegion | String | e.g., "us-west-2" |
| RTOMinutes | Integer | 5, 15, 60, 240, 480, 1440 |
| BackupStrategy | Enum | None, Velero, Kasten, Portworx, CloudNative, Custom |
| BackupFrequencyHours | Integer | 1, 4, 12, 24, 168 |
| BackupRetentionDays | Integer | 7, 14, 30, 90, 365 |
| EnablePodDisruptionBudgets | Boolean | Default: false |
| EnableTopologySpread | Boolean | Default: false |

---

### UC-012 Output Fields Update

The Calculate K8s Sizing use case (UC-012) output fields need updates:

**New Fields:**
| Field | Type | Description |
|-------|------|-------------|
| Environment.EtcdNodes | Integer | Separate etcd nodes (ExternalEtcd mode) |
| Environment.DRNodes | Integer | DR cluster node count |
| Environment.DRCostMultiplier | Double | Cost multiplier for DR strategy |
| Environment.AvailabilityZones | Integer | Number of AZs in use |

---

### New Business Rules Required

#### Control Plane HA Rules (BR-HADR-xxx)

| Rule ID | Description |
|---------|-------------|
| BR-HADR-001 | Managed distributions auto-set ControlPlaneHA to Managed |
| BR-HADR-002 | StackedHA requires 3, 5, or 7 control plane nodes |
| BR-HADR-003 | ExternalEtcd adds separate etcd node count to total |
| BR-HADR-004 | Production environments default to Multi-AZ (3 zones) |

#### Node Distribution Rules

| Rule ID | Description |
|---------|-------------|
| BR-HADR-005 | SingleAZ = 1 availability zone |
| BR-HADR-006 | DualAZ = 2 availability zones |
| BR-HADR-007 | MultiAZ = 3-5 availability zones |
| BR-HADR-008 | MultiRegion adds cross-region data transfer costs |
| BR-HADR-009 | Minimum nodes per AZ = ceiling(total_nodes / az_count) |

#### DR Pattern Rules

| Rule ID | Description |
|---------|-------------|
| BR-HADR-010 | BackupRestore: 0.1x cost multiplier (storage only) |
| BR-HADR-011 | WarmStandby: 0.4x cost multiplier |
| BR-HADR-012 | HotStandby: 0.9x cost multiplier |
| BR-HADR-013 | ActiveActive: 1.0x cost multiplier (full duplicate) |

#### Cost Multiplier Rules

| Rule ID | Description |
|---------|-------------|
| BR-HADR-014 | Azure cross-AZ traffic is FREE |
| BR-HADR-015 | AWS/GCP cross-AZ traffic adds ~0.01 per GB |
| BR-HADR-016 | Cross-region traffic adds ~0.02 per GB |

---

## Existing VM Use Cases (Already Aligned)

The VM use cases (UC-013 to UC-016) already cover HA/DR configuration:
- UC-014: Configure HA Pattern ✓
- UC-015: Configure DR Pattern ✓
- UC-016: Configure Load Balancer ✓

The `VMHADRConfig.razor` component implements these existing use cases.

---

## Priority

| Update | Priority | Effort |
|--------|----------|--------|
| Add UC-xxx K8s HA/DR | High | Medium |
| Update UC-012 output fields | High | Low |
| Add BR-HADR business rules | High | Medium |
| Update traceability matrix | Medium | Low |

---

## Notes

- The implementation in K8sHADRPanel.razor is complete and working
- Technical documentation (models.md, services.md, ui-components.md) has been updated
- SRS formal requirements update is tracked as follow-up work
