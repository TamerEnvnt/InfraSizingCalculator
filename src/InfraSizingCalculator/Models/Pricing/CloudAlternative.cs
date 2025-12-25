using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// Represents a cloud alternative for an on-premises distribution.
/// Used to show users what managed cloud services they could use instead.
/// </summary>
public class CloudAlternative
{
    /// <summary>
    /// The cloud provider offering this alternative.
    /// </summary>
    public CloudProvider Provider { get; set; }

    /// <summary>
    /// Short display name (e.g., "ROSA", "ARO", "EKS").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full service name (e.g., "Red Hat OpenShift on AWS").
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the service and its benefits.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a distribution-specific alternative (true) or generic (false).
    /// Distribution-specific: ROSA for OpenShift, ARO for OpenShift
    /// Generic: EKS, AKS, GKE for any on-prem distribution
    /// </summary>
    public bool IsDistributionSpecific { get; set; }

    /// <summary>
    /// The on-premises distribution this is an alternative for.
    /// Null for generic alternatives that apply to any distribution.
    /// </summary>
    public Distribution? SourceDistribution { get; set; }

    /// <summary>
    /// Whether this alternative is recommended for the source distribution.
    /// </summary>
    public bool IsRecommended { get; set; }

    /// <summary>
    /// URL to the service's documentation or pricing page.
    /// </summary>
    public string? DocumentationUrl { get; set; }

    /// <summary>
    /// Key features or benefits of this alternative.
    /// </summary>
    public List<string> Features { get; set; } = new();

    /// <summary>
    /// Any considerations or caveats when migrating to this alternative.
    /// </summary>
    public List<string> Considerations { get; set; } = new();
}

/// <summary>
/// Static factory for creating cloud alternatives.
/// </summary>
public static class CloudAlternatives
{
    /// <summary>
    /// Gets distribution-specific cloud alternatives for an on-prem distribution.
    /// </summary>
    public static List<CloudAlternative> GetForDistribution(Distribution distribution)
    {
        return distribution switch
        {
            Distribution.OpenShift => GetOpenShiftAlternatives(),
            Distribution.Rancher => GetRancherAlternatives(),
            Distribution.RKE2 => GetRKE2Alternatives(),
            Distribution.K3s => GetK3sAlternatives(),
            Distribution.Tanzu => GetTanzuAlternatives(),
            Distribution.Charmed => GetCharmedAlternatives(),
            Distribution.Kubernetes => GetVanillaK8sAlternatives(),
            Distribution.MicroK8s => GetMicroK8sAlternatives(),
            _ => GetGenericAlternatives()
        };
    }

    /// <summary>
    /// Gets generic cloud alternatives that apply to any on-prem distribution.
    /// </summary>
    public static List<CloudAlternative> GetGenericAlternatives()
    {
        return new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Amazon Elastic Kubernetes Service",
                Description = "Fully managed Kubernetes service on AWS",
                IsDistributionSpecific = false,
                IsRecommended = false,
                DocumentationUrl = "https://aws.amazon.com/eks/",
                Features = new() { "Managed control plane", "Deep AWS integration", "Fargate serverless option" }
            },
            new()
            {
                Provider = CloudProvider.Azure,
                Name = "AKS",
                ServiceName = "Azure Kubernetes Service",
                Description = "Fully managed Kubernetes service on Azure",
                IsDistributionSpecific = false,
                IsRecommended = false,
                DocumentationUrl = "https://azure.microsoft.com/services/kubernetes-service/",
                Features = new() { "Free control plane", "Azure AD integration", "Virtual nodes" }
            },
            new()
            {
                Provider = CloudProvider.GCP,
                Name = "GKE",
                ServiceName = "Google Kubernetes Engine",
                Description = "Fully managed Kubernetes service on Google Cloud",
                IsDistributionSpecific = false,
                IsRecommended = false,
                DocumentationUrl = "https://cloud.google.com/kubernetes-engine",
                Features = new() { "Autopilot mode", "GKE Enterprise", "Multi-cluster management" }
            },
            new()
            {
                Provider = CloudProvider.OCI,
                Name = "OKE",
                ServiceName = "Oracle Container Engine for Kubernetes",
                Description = "Managed Kubernetes on Oracle Cloud",
                IsDistributionSpecific = false,
                IsRecommended = false,
                DocumentationUrl = "https://www.oracle.com/cloud/cloud-native/container-engine-kubernetes/",
                Features = new() { "Free control plane", "ARM instances available", "OCI integration" }
            }
        };
    }

    private static List<CloudAlternative> GetOpenShiftAlternatives()
    {
        return new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "ROSA",
                ServiceName = "Red Hat OpenShift Service on AWS",
                Description = "Managed OpenShift on AWS with joint Red Hat and AWS support",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.OpenShift,
                IsRecommended = true,
                DocumentationUrl = "https://www.redhat.com/en/technologies/cloud-computing/openshift/aws",
                Features = new() { "Native AWS integration", "Joint support from Red Hat and AWS", "PrivateLink support" },
                Considerations = new() { "Requires OpenShift subscription", "AWS account required" }
            },
            new()
            {
                Provider = CloudProvider.Azure,
                Name = "ARO",
                ServiceName = "Azure Red Hat OpenShift",
                Description = "Managed OpenShift on Azure with joint Red Hat and Microsoft support",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.OpenShift,
                IsRecommended = true,
                DocumentationUrl = "https://azure.microsoft.com/services/openshift/",
                Features = new() { "Native Azure integration", "Joint support", "Azure AD integration" },
                Considerations = new() { "Requires OpenShift subscription", "Azure account required" }
            },
            new()
            {
                Provider = CloudProvider.GCP,
                Name = "OpenShift Dedicated",
                ServiceName = "OpenShift Dedicated on GCP",
                Description = "Red Hat managed OpenShift on Google Cloud",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.OpenShift,
                IsRecommended = false,
                DocumentationUrl = "https://www.redhat.com/en/technologies/cloud-computing/openshift/dedicated",
                Features = new() { "Fully managed by Red Hat", "GCP integration", "SLA guarantees" }
            },
            new()
            {
                Provider = CloudProvider.IBM,
                Name = "ROKS",
                ServiceName = "Red Hat OpenShift on IBM Cloud",
                Description = "Managed OpenShift on IBM Cloud",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.OpenShift,
                IsRecommended = false,
                DocumentationUrl = "https://www.ibm.com/cloud/openshift",
                Features = new() { "IBM Cloud Pak integration", "Watson AI services", "Satellite support" }
            }
        };
    }

    private static List<CloudAlternative> GetRancherAlternatives()
    {
        return new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS + Rancher",
                ServiceName = "Amazon EKS with Rancher",
                Description = "Use Rancher to manage EKS clusters",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.Rancher,
                IsRecommended = true,
                Features = new() { "Multi-cluster management", "Rancher UI on EKS", "SUSE support available" }
            },
            new()
            {
                Provider = CloudProvider.Azure,
                Name = "AKS + Rancher",
                ServiceName = "Azure AKS with Rancher",
                Description = "Use Rancher to manage AKS clusters",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.Rancher,
                IsRecommended = true,
                Features = new() { "Multi-cluster management", "Azure integration", "SUSE support available" }
            },
            new()
            {
                Provider = CloudProvider.GCP,
                Name = "GKE + Rancher",
                ServiceName = "Google GKE with Rancher",
                Description = "Use Rancher to manage GKE clusters",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.Rancher,
                IsRecommended = false,
                Features = new() { "Multi-cluster management", "GCP integration", "Anthos compatibility" }
            }
        };
    }

    private static List<CloudAlternative> GetRKE2Alternatives()
    {
        // RKE2 users typically want lightweight/simple K8s - suggest managed options
        var alternatives = GetGenericAlternatives();
        foreach (var alt in alternatives)
        {
            alt.SourceDistribution = Distribution.RKE2;
            alt.Considerations.Add("RKE2-specific configurations may need adaptation");
        }
        return alternatives;
    }

    private static List<CloudAlternative> GetK3sAlternatives()
    {
        // K3s users often want lightweight options
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.DigitalOcean,
                Name = "DOKS",
                ServiceName = "DigitalOcean Kubernetes",
                Description = "Simple, cost-effective managed Kubernetes",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.K3s,
                IsRecommended = true,
                DocumentationUrl = "https://www.digitalocean.com/products/kubernetes",
                Features = new() { "Free control plane", "Simple pricing", "Developer-friendly" },
                Considerations = new() { "Fewer enterprise features than major clouds" }
            },
            new()
            {
                Provider = CloudProvider.Linode,
                Name = "LKE",
                ServiceName = "Linode Kubernetes Engine",
                Description = "Affordable managed Kubernetes on Akamai/Linode",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.K3s,
                IsRecommended = true,
                Features = new() { "Free control plane", "Low egress costs", "Simple setup" }
            },
            new()
            {
                Provider = CloudProvider.Hetzner,
                Name = "Hetzner K8s",
                ServiceName = "Hetzner Cloud Kubernetes",
                Description = "Very cost-effective Kubernetes in Europe",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.K3s,
                IsRecommended = false,
                Features = new() { "Extremely low costs", "European data centers", "Good for dev/test" }
            }
        };
        alternatives.AddRange(GetGenericAlternatives());
        return alternatives;
    }

    private static List<CloudAlternative> GetTanzuAlternatives()
    {
        return new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "Tanzu on AWS",
                ServiceName = "VMware Tanzu on AWS",
                Description = "Run Tanzu workloads on AWS with VMware Cloud",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.Tanzu,
                IsRecommended = true,
                Features = new() { "VMware Cloud integration", "vSphere compatibility", "Tanzu Mission Control" }
            },
            new()
            {
                Provider = CloudProvider.Azure,
                Name = "Tanzu on Azure",
                ServiceName = "VMware Tanzu on Azure VMware Solution",
                Description = "Run Tanzu on Azure VMware Solution",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.Tanzu,
                IsRecommended = true,
                Features = new() { "Azure VMware Solution", "Hybrid connectivity", "Azure services integration" }
            },
            new()
            {
                Provider = CloudProvider.GCP,
                Name = "Tanzu on GCP",
                ServiceName = "VMware Tanzu on Google Cloud VMware Engine",
                Description = "Run Tanzu on Google Cloud VMware Engine",
                IsDistributionSpecific = true,
                SourceDistribution = Distribution.Tanzu,
                IsRecommended = false,
                Features = new() { "GCVE integration", "Google Cloud services", "VMware compatibility" }
            }
        };
    }

    private static List<CloudAlternative> GetCharmedAlternatives()
    {
        // Charmed Kubernetes users might want Ubuntu-friendly options
        var alternatives = GetGenericAlternatives();
        foreach (var alt in alternatives)
        {
            alt.SourceDistribution = Distribution.Charmed;
        }
        return alternatives;
    }

    private static List<CloudAlternative> GetVanillaK8sAlternatives()
    {
        return GetGenericAlternatives();
    }

    private static List<CloudAlternative> GetMicroK8sAlternatives()
    {
        // Similar to K3s - lightweight options
        return GetK3sAlternatives().Select(a =>
        {
            a.SourceDistribution = Distribution.MicroK8s;
            return a;
        }).ToList();
    }
}
