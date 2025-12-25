namespace InfraSizingCalculator.Models.Enums;

public enum Distribution
{
    // ========== On-Premises Distributions ==========
    OpenShift,          // OpenShift Container Platform (on-prem)
    Kubernetes,         // Vanilla Kubernetes (on-prem)
    Rancher,            // Rancher Manager (on-prem)
    RKE2,               // Rancher RKE2 (on-prem)
    K3s,                // K3s lightweight (on-prem/edge)
    MicroK8s,           // MicroK8s (on-prem)
    Charmed,            // Charmed Kubernetes (on-prem)
    Tanzu,              // VMware Tanzu (on-prem)

    // ========== OpenShift Cloud Variants ==========
    OpenShiftROSA,      // Red Hat OpenShift on AWS (ROSA)
    OpenShiftARO,       // Azure Red Hat OpenShift (ARO)
    OpenShiftDedicated, // OpenShift Dedicated on GCP
    OpenShiftIBM,       // Red Hat OpenShift on IBM Cloud

    // ========== Rancher/SUSE Cloud Variants ==========
    RancherHosted,      // Rancher Hosted on any cloud
    RancherEKS,         // Rancher on EKS
    RancherAKS,         // Rancher on AKS
    RancherGKE,         // Rancher on GKE

    // ========== Tanzu Cloud Variants ==========
    TanzuCloud,         // VMware Tanzu Cloud (generic)
    TanzuAWS,           // VMware Tanzu on AWS
    TanzuAzure,         // VMware Tanzu on Azure
    TanzuGCP,           // VMware Tanzu on GCP

    // ========== Canonical/Ubuntu Cloud Variants ==========
    CharmedAWS,         // Charmed Kubernetes on AWS
    CharmedAzure,       // Charmed Kubernetes on Azure
    CharmedGCP,         // Charmed Kubernetes on GCP
    MicroK8sAWS,        // MicroK8s on AWS
    MicroK8sAzure,      // MicroK8s on Azure
    MicroK8sGCP,        // MicroK8s on GCP

    // ========== K3s Cloud Variants ==========
    K3sAWS,             // K3s on AWS
    K3sAzure,           // K3s on Azure
    K3sGCP,             // K3s on GCP

    // ========== RKE2 Cloud Variants ==========
    RKE2AWS,            // RKE2 on AWS
    RKE2Azure,          // RKE2 on Azure
    RKE2GCP,            // RKE2 on GCP

    // ========== Major Cloud Managed K8s ==========
    EKS,                // Amazon Elastic Kubernetes Service
    AKS,                // Azure Kubernetes Service
    GKE,                // Google Kubernetes Engine
    OKE,                // Oracle Container Engine for Kubernetes
    IKS,                // IBM Kubernetes Service
    ACK,                // Alibaba Container Service for Kubernetes
    TKE,                // Tencent Kubernetes Engine
    CCE,                // Huawei Cloud Container Engine

    // ========== Developer/Smaller Cloud Managed K8s ==========
    DOKS,               // DigitalOcean Kubernetes
    LKE,                // Linode Kubernetes Engine (Akamai)
    VKE,                // Vultr Kubernetes Engine
    HetznerK8s,         // Hetzner Kubernetes
    OVHKubernetes,      // OVHcloud Managed Kubernetes
    ScalewayKapsule     // Scaleway Kapsule
}
