using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using InfraSizingCalculator.Data;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service for retrieving info modal content from the database.
/// Uses database-first approach with fallback to default content.
/// Implements caching for performance.
/// </summary>
public class InfoContentService : IInfoContentService
{
    private readonly InfraSizingDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<InfoContentService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public InfoContentService(
        InfraSizingDbContext dbContext,
        IMemoryCache cache,
        ILogger<InfoContentService> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<InfoContent> GetInfoTypeContentAsync(string infoType)
    {
        var cacheKey = $"info_type_{infoType}";

        if (_cache.TryGetValue(cacheKey, out InfoContent? cached) && cached != null)
            return cached;

        // Try database first
        var dbContent = await _dbContext.InfoTypeContents
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.InfoTypeKey == infoType && c.IsActive);

        if (dbContent != null && !string.IsNullOrEmpty(dbContent.ContentHtml))
        {
            var content = new InfoContent(dbContent.Title, dbContent.ContentHtml) { IsFromDatabase = true };
            _cache.Set(cacheKey, content, CacheDuration);
            return content;
        }

        // Fallback to default content
        var defaultContent = DefaultInfoTypeContent.Get(infoType);
        _cache.Set(cacheKey, defaultContent, CacheDuration);
        return defaultContent;
    }

    public async Task<InfoContent> GetDistributionInfoAsync(Distribution distribution)
    {
        var cacheKey = $"distro_info_{distribution}";

        if (_cache.TryGetValue(cacheKey, out InfoContent? cached) && cached != null)
            return cached;

        // Try database first
        var dbContent = await _dbContext.DistributionConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.DistributionKey == distribution.ToString() && c.IsActive);

        if (dbContent != null && !string.IsNullOrEmpty(dbContent.DetailedInfoHtml))
        {
            var content = new InfoContent(dbContent.Name, dbContent.DetailedInfoHtml) { IsFromDatabase = true };
            _cache.Set(cacheKey, content, CacheDuration);
            return content;
        }

        // Fallback to default content
        var defaultContent = DefaultDistributionContent.Get(distribution);
        _cache.Set(cacheKey, defaultContent, CacheDuration);
        return defaultContent;
    }

    public async Task<InfoContent> GetTechnologyInfoAsync(Technology technology)
    {
        var cacheKey = $"tech_info_{technology}";

        if (_cache.TryGetValue(cacheKey, out InfoContent? cached) && cached != null)
            return cached;

        // Try database first
        var dbContent = await _dbContext.TechnologyConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TechnologyKey == technology.ToString() && c.IsActive);

        if (dbContent != null && !string.IsNullOrEmpty(dbContent.DetailedInfoHtml))
        {
            var content = new InfoContent(dbContent.Name, dbContent.DetailedInfoHtml) { IsFromDatabase = true };
            _cache.Set(cacheKey, content, CacheDuration);
            return content;
        }

        // Fallback to default content
        var defaultContent = DefaultTechnologyContent.Get(technology);
        _cache.Set(cacheKey, defaultContent, CacheDuration);
        return defaultContent;
    }

    public async Task<bool> HasCustomContentAsync(string contentKey)
    {
        // Check if custom content exists in any of the content tables
        var hasInfoType = await _dbContext.InfoTypeContents
            .AsNoTracking()
            .AnyAsync(c => c.InfoTypeKey == contentKey && c.IsActive && !string.IsNullOrEmpty(c.ContentHtml));

        if (hasInfoType) return true;

        var hasDistro = await _dbContext.DistributionConfigs
            .AsNoTracking()
            .AnyAsync(c => c.DistributionKey == contentKey && c.IsActive && !string.IsNullOrEmpty(c.DetailedInfoHtml));

        if (hasDistro) return true;

        var hasTech = await _dbContext.TechnologyConfigs
            .AsNoTracking()
            .AnyAsync(c => c.TechnologyKey == contentKey && c.IsActive && !string.IsNullOrEmpty(c.DetailedInfoHtml));

        return hasTech;
    }
}

/// <summary>
/// Default content for general info types (Platform, Deployment, etc.)
/// </summary>
internal static class DefaultInfoTypeContent
{
    private static readonly Dictionary<string, InfoContent> Content = new()
    {
        ["Platform"] = new("Platform Types",
            "<h3>Native Applications</h3><p>Traditional applications built with .NET, Java, Node.js, Python, or Go. Full control over code and dependencies.</p>" +
            "<h3>Low-Code Platforms</h3><p>Platforms like Mendix that enable rapid development with visual modeling. Higher resource requirements but faster development.</p>")
            { IsFromDatabase = false },

        ["Deployment"] = new("Deployment Models",
            "<h3>Kubernetes</h3><p>Container orchestration with auto-scaling, self-healing, and rolling updates. Best for microservices and cloud-native apps.</p>" +
            "<h3>Virtual Machines</h3><p>Traditional VM deployment with dedicated servers. Better for legacy apps or specific compliance requirements.</p>")
            { IsFromDatabase = false },

        ["Technology"] = new("Technologies",
            "<h3>.NET</h3><p>Microsoft's cross-platform framework. Efficient memory usage, great for enterprise apps.</p>" +
            "<h3>Java</h3><p>Enterprise runtime with higher memory footprint. Excellent ecosystem and tooling.</p>" +
            "<h3>Node.js</h3><p>JavaScript runtime for event-driven applications. Lightweight but single-threaded.</p>" +
            "<h3>Python</h3><p>Versatile language for web, data science, and automation. GIL impacts CPU-bound workloads.</p>" +
            "<h3>Go</h3><p>Google's compiled language. Very efficient, ideal for microservices.</p>" +
            "<h3>Mendix</h3><p>Enterprise low-code platform. Higher resource requirements due to runtime overhead.</p>")
            { IsFromDatabase = false },

        ["Distribution"] = new("Kubernetes Distributions",
            "<h3>OpenShift</h3><p>Enterprise Kubernetes from Red Hat with built-in CI/CD, monitoring, and security. Includes dedicated infra nodes.</p>" +
            "<h3>Vanilla Kubernetes</h3><p>Standard CNCF Kubernetes. Maximum flexibility, self-managed.</p>" +
            "<h3>Rancher</h3><p>Multi-cluster management from SUSE. Easy deployment and management.</p>" +
            "<h3>K3s</h3><p>Lightweight Kubernetes for edge and IoT.</p>" +
            "<h3>Cloud Managed (EKS/AKS/GKE)</h3><p>Control plane managed by cloud provider. Reduced operational overhead.</p>")
            { IsFromDatabase = false },

        ["ClusterMode"] = new("Cluster Modes",
            "<h3>Multi-Cluster</h3><p>Separate cluster per environment. Maximum isolation but higher cost.</p>" +
            "<h3>Shared Cluster</h3><p>Single cluster with namespace isolation. Cost-optimized but shared resources.</p>" +
            "<h3>Per Environment</h3><p>Calculate for a single specific environment.</p>")
            { IsFromDatabase = false },

        ["NodeSpecs"] = new("Node Specifications",
            "<h3>Control Plane</h3><p>Master nodes running API server, scheduler, controller manager. 3 nodes for HA, 5 for large clusters.</p>" +
            "<h3>Infrastructure</h3><p>OpenShift-specific nodes for platform services (router, registry, monitoring). Not all distributions require these.</p>" +
            "<h3>Worker Nodes</h3><p>Nodes running application workloads. Sized based on application requirements.</p>")
            { IsFromDatabase = false },

        ["AppConfig"] = new("Application Configuration",
            "<h3>App Tiers</h3><p>Small, Medium, Large, XLarge - each tier has specific CPU and memory allocations based on the selected technology.</p>" +
            "<h3>Environments</h3><p>Dev/Test for development, Stage for pre-production, Prod for production, DR for disaster recovery.</p>")
            { IsFromDatabase = false }
    };

    public static InfoContent Get(string infoType)
        => Content.TryGetValue(infoType, out var content) ? content : InfoContent.Empty;
}

/// <summary>
/// Default content for Kubernetes distributions.
/// Extracted from Home.razor.cs for testability and maintainability.
/// </summary>
internal static class DefaultDistributionContent
{
    public static InfoContent Get(Distribution distribution)
    {
        return distribution switch
        {
            Distribution.OpenShift => new("Red Hat OpenShift (On-Prem)", GetOpenShiftContent()) { IsFromDatabase = false },
            Distribution.OpenShiftROSA => new("Red Hat OpenShift on AWS (ROSA)", GetROSAContent()) { IsFromDatabase = false },
            Distribution.OpenShiftARO => new("Azure Red Hat OpenShift (ARO)", GetAROContent()) { IsFromDatabase = false },
            Distribution.Kubernetes => new("Vanilla Kubernetes", GetVanillaK8sContent()) { IsFromDatabase = false },
            Distribution.Rancher => new("Rancher (On-Prem)", GetRancherContent()) { IsFromDatabase = false },
            Distribution.RancherHosted => new("Rancher Hosted (Cloud)", GetRancherHostedContent()) { IsFromDatabase = false },
            Distribution.K3s => new("K3s (Lightweight)", GetK3sContent()) { IsFromDatabase = false },
            Distribution.MicroK8s => new("MicroK8s (Canonical)", GetMicroK8sContent()) { IsFromDatabase = false },
            Distribution.Charmed => new("Charmed Kubernetes (Canonical)", GetCharmedContent()) { IsFromDatabase = false },
            Distribution.Tanzu => new("VMware Tanzu (On-Prem)", GetTanzuContent()) { IsFromDatabase = false },
            Distribution.TanzuCloud => new("VMware Tanzu Cloud", GetTanzuCloudContent()) { IsFromDatabase = false },
            Distribution.EKS => new("Amazon EKS", GetEKSContent()) { IsFromDatabase = false },
            Distribution.AKS => new("Azure AKS", GetAKSContent()) { IsFromDatabase = false },
            Distribution.GKE => new("Google GKE", GetGKEContent()) { IsFromDatabase = false },
            Distribution.OKE => new("Oracle OKE", GetOKEContent()) { IsFromDatabase = false },
            Distribution.RKE2 => new("RKE2 (Rancher Kubernetes Engine)", GetRKE2Content()) { IsFromDatabase = false },
            Distribution.OpenShiftDedicated => new("OpenShift Dedicated (GCP)", GetOSDContent()) { IsFromDatabase = false },
            Distribution.OpenShiftIBM => new("Red Hat OpenShift on IBM Cloud", GetOSIBMContent()) { IsFromDatabase = false },
            _ => new(distribution.ToString(), $"<p>Information for {distribution} distribution.</p>") { IsFromDatabase = false }
        };
    }

    private static string GetOpenShiftContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Red Hat (IBM)</p>
            <p class='info-desc'>Enterprise Kubernetes platform with integrated developer tools, CI/CD pipelines, and enhanced security features. Self-managed on-premises deployment.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> 3 master nodes (HA required)</li>
            <li><strong>Infrastructure:</strong> 3 dedicated nodes for router, registry, monitoring</li>
            <li><strong>Worker:</strong> Application workload nodes</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Built-in CI/CD with Tekton Pipelines</li>
            <li>Integrated container registry</li>
            <li>OperatorHub marketplace</li>
            <li>Enhanced security (SCC, SELinux)</li>
            <li>Web console for management</li>
        </ul>
        <h4>Sizing Notes</h4>
        <ul>
            <li>Master: 8 vCPU, 32 GB RAM, 200 GB disk (prod)</li>
            <li>Infra: 8 vCPU, 32 GB RAM, 500 GB disk (prod)</li>
            <li>Worker: 16 vCPU, 64 GB RAM, 200 GB disk (prod)</li>
        </ul>
        <p class='info-note'><em>Note: Requires RHEL CoreOS for nodes. Subscription-based licensing.</em></p>";

    private static string GetROSAContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Red Hat + AWS</p>
            <p class='info-desc'>Fully managed OpenShift service on AWS. AWS and Red Hat jointly manage the control plane - you focus on applications.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Managed by Red Hat/AWS (0 customer-managed masters)</li>
            <li><strong>Infrastructure:</strong> 3 dedicated nodes for router, registry, monitoring</li>
            <li><strong>Worker:</strong> Application workload nodes (EC2 instances)</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>SLA-backed managed control plane (99.95%)</li>
            <li>Native AWS integration (IAM, VPC, EBS, S3)</li>
            <li>AWS STS for secure token service</li>
            <li>Integrated Red Hat support</li>
            <li>Pay-as-you-go or annual commitment</li>
        </ul>
        <h4>Pricing</h4>
        <ul>
            <li>Cluster fee: ~$0.171/hour (~$125/month)</li>
            <li>Per-worker: ~$0.171/hour per 4 vCPU</li>
            <li>EC2 instances: Standard AWS pricing</li>
        </ul>
        <p class='info-note'><em>Note: Best for enterprises wanting OpenShift experience on AWS without managing control plane.</em></p>";

    private static string GetAROContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Red Hat + Microsoft</p>
            <p class='info-desc'>Jointly managed OpenShift service on Azure. First-party Azure service with integrated support from both vendors.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Managed by Red Hat/Microsoft (0 customer-managed masters)</li>
            <li><strong>Infrastructure:</strong> 3 dedicated nodes for router, registry, monitoring</li>
            <li><strong>Worker:</strong> Application workload nodes (Azure VMs)</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Azure AD integration for SSO</li>
            <li>Azure Monitor integration</li>
            <li>Private Link support</li>
            <li>Joint support from Microsoft and Red Hat</li>
            <li>Billed through Azure</li>
        </ul>
        <h4>Pricing</h4>
        <ul>
            <li>Cluster fee: Included in VM pricing</li>
            <li>Master VMs: Standard_D8s_v3 (managed)</li>
            <li>Worker VMs: Your choice of Azure VM sizes</li>
        </ul>
        <p class='info-note'><em>Note: Best for enterprises with Azure investment wanting full OpenShift experience.</em></p>";

    private static string GetVanillaK8sContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> CNCF (Cloud Native Computing Foundation)</p>
            <p class='info-desc'>Standard upstream Kubernetes - the foundation for all distributions. Maximum flexibility with full control.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> 3+ master nodes for HA</li>
            <li><strong>Worker:</strong> Application workload nodes</li>
            <li><em>No dedicated infra nodes (services run on workers)</em></li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Full CNCF conformance</li>
            <li>Choose your own CNI, CSI, ingress</li>
            <li>Maximum customization</li>
            <li>Large ecosystem and community</li>
        </ul>
        <h4>Deployment Options</h4>
        <ul>
            <li>Self-managed: kubeadm, kubespray</li>
            <li>Any infrastructure: bare metal, VMs, cloud</li>
        </ul>
        <p class='info-note'><em>Note: Requires more operational expertise. You manage everything.</em></p>";

    private static string GetRancherContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> SUSE</p>
            <p class='info-desc'>Multi-cluster Kubernetes management platform. Simplifies deployment and operations across any infrastructure. Self-managed on-premises.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> 3 master nodes for HA (RKE2/K3s)</li>
            <li><strong>Worker:</strong> Application workload nodes</li>
            <li><strong>Rancher Server:</strong> Management cluster (separate)</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Multi-cluster management UI</li>
            <li>App catalog and Helm integration</li>
            <li>Built-in monitoring (Prometheus/Grafana)</li>
            <li>Fleet for GitOps at scale</li>
            <li>Supports RKE2, K3s, EKS, AKS, GKE</li>
        </ul>
        <h4>Sizing Notes</h4>
        <ul>
            <li>Master: 4 vCPU, 16 GB RAM, 100 GB disk (prod)</li>
            <li>Worker: 8 vCPU, 32 GB RAM, 100 GB disk (prod)</li>
        </ul>
        <p class='info-note'><em>Note: Open source with enterprise support available.</em></p>";

    private static string GetRancherHostedContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> SUSE</p>
            <p class='info-desc'>Cloud-hosted Rancher management service. Control plane managed by SUSE - you manage downstream clusters.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Managed by SUSE (0 customer-managed masters)</li>
            <li><strong>Worker:</strong> Application workload nodes</li>
            <li><strong>Downstream Clusters:</strong> Can be any K8s distribution</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Zero control plane management overhead</li>
            <li>Same Rancher UI and features</li>
            <li>Multi-cluster management across clouds</li>
            <li>Fleet GitOps included</li>
            <li>Enterprise support from SUSE</li>
        </ul>
        <h4>Pricing</h4>
        <ul>
            <li>Per managed cluster fee</li>
            <li>Worker nodes: Your cloud provider pricing</li>
        </ul>
        <p class='info-note'><em>Note: Best for multi-cloud management without self-hosting Rancher.</em></p>";

    private static string GetK3sContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> SUSE</p>
            <p class='info-desc'>Lightweight Kubernetes designed for edge computing, IoT, CI/CD, and resource-constrained environments.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Server:</strong> Combined control plane + etcd (3 for HA)</li>
            <li><strong>Agent:</strong> Worker nodes</li>
            <li><em>Single binary ~50MB</em></li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Minimal resource footprint (512MB RAM)</li>
            <li>Built-in: Traefik, local-path-provisioner, CoreDNS</li>
            <li>ARM64 and ARMv7 support</li>
            <li>Embedded SQLite (single node) or etcd (HA)</li>
            <li>CNCF certified conformant</li>
        </ul>
        <h4>Deployment Options</h4>
        <ul>
            <li>Edge devices and IoT</li>
            <li>Development environments</li>
            <li>CI/CD runners</li>
        </ul>
        <p class='info-note'><em>Note: Great for dev/test but also production-ready for edge.</em></p>";

    private static string GetMicroK8sContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Canonical (Ubuntu)</p>
            <p class='info-desc'>Low-ops, minimal Kubernetes for developers, edge, and IoT. Single-package installation with optional add-ons.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Embedded in every node</li>
            <li><strong>HA Mode:</strong> 3+ nodes with dqlite (distributed SQLite)</li>
            <li><strong>Single Node:</strong> All-in-one deployment option</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Single snap package installation</li>
            <li>Built-in add-ons (dns, dashboard, storage, gpu, istio)</li>
            <li>Strict confinement with AppArmor</li>
            <li>Automatic security updates</li>
            <li>CNCF certified conformant</li>
        </ul>
        <h4>Sizing Notes</h4>
        <ul>
            <li>Minimum: 1 vCPU, 1 GB RAM</li>
            <li>Recommended: 2 vCPU, 4 GB RAM, 50 GB disk</li>
        </ul>
        <p class='info-note'><em>Note: Best for developers, CI/CD, and edge deployments on Ubuntu.</em></p>";

    private static string GetCharmedContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Canonical (Ubuntu)</p>
            <p class='info-desc'>Production-grade Kubernetes built with Juju operators. Full enterprise features with model-driven operations.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> 3 master nodes (HA with etcd)</li>
            <li><strong>Worker:</strong> Application workload nodes</li>
            <li><strong>Operators:</strong> Juju charms for each component</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Model-driven operations with Juju</li>
            <li>Automated day-2 operations (upgrades, scaling)</li>
            <li>Integration with OpenStack, VMware, MAAS</li>
            <li>Calico, Flannel, or Tigera CNI options</li>
            <li>Ubuntu Pro support available</li>
        </ul>
        <h4>Sizing Notes</h4>
        <ul>
            <li>Master: 4 vCPU, 16 GB RAM, 100 GB disk (prod)</li>
            <li>Worker: 8 vCPU, 32 GB RAM, 100 GB disk (prod)</li>
        </ul>
        <p class='info-note'><em>Note: Best for enterprises using Juju for infrastructure automation.</em></p>";

    private static string GetTanzuContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Broadcom (formerly VMware)</p>
            <p class='info-desc'>Enterprise Kubernetes platform integrated with vSphere. Kubernetes for VMware environments.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Supervisor Cluster:</strong> Runs on vSphere ESXi hosts</li>
            <li><strong>Workload Cluster:</strong> Tanzu Kubernetes Grid clusters</li>
            <li><strong>Control Plane:</strong> 3 master nodes per cluster</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>vSphere with Tanzu (integrated with vCenter)</li>
            <li>Harbor container registry included</li>
            <li>NSX-T networking integration</li>
            <li>vSphere Pod Service (run pods on ESXi)</li>
            <li>Tanzu Mission Control for multi-cluster</li>
        </ul>
        <h4>Sizing Notes</h4>
        <ul>
            <li>Master: 4 vCPU, 16 GB RAM, 100 GB disk (prod)</li>
            <li>Worker: 8 vCPU, 32 GB RAM, 100 GB disk (prod)</li>
        </ul>
        <p class='info-note'><em>Note: Best for existing VMware shops wanting Kubernetes on vSphere.</em></p>";

    private static string GetTanzuCloudContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Broadcom (formerly VMware)</p>
            <p class='info-desc'>Cloud-managed Tanzu Kubernetes service. Fully managed control plane with multi-cloud support.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Managed by VMware (0 customer-managed masters)</li>
            <li><strong>Worker:</strong> Customer-managed nodes on any cloud</li>
            <li><strong>Multi-cloud:</strong> AWS, Azure, or on-premises</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Tanzu Mission Control (TMC) management</li>
            <li>Policy-as-code enforcement</li>
            <li>Data protection with Velero</li>
            <li>Multi-cluster networking</li>
            <li>Integrated observability</li>
        </ul>
        <h4>Pricing</h4>
        <ul>
            <li>Subscription-based per cluster</li>
            <li>Worker nodes: Cloud provider pricing</li>
        </ul>
        <p class='info-note'><em>Note: Best for multi-cloud Kubernetes with centralized management.</em></p>";

    private static string GetEKSContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Amazon Web Services</p>
            <p class='info-desc'>Managed Kubernetes service on AWS. AWS handles the control plane - you manage worker nodes.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Managed by AWS (HA across 3 AZs)</li>
            <li><strong>Worker:</strong> EC2 instances or Fargate (serverless)</li>
            <li><em>No infra nodes needed - use AWS services</em></li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Native AWS integrations (IAM, VPC, ALB, EBS)</li>
            <li>Fargate for serverless containers</li>
            <li>Managed node groups with auto-scaling</li>
            <li>EKS Anywhere for on-premises</li>
        </ul>
        <h4>Pricing</h4>
        <ul>
            <li>Cluster: $0.10/hour (~$73/month)</li>
            <li>Worker nodes: EC2 pricing</li>
            <li>Fargate: per vCPU/GB-hour</li>
        </ul>
        <p class='info-note'><em>Note: Best for AWS-native workloads. Deep integration with AWS ecosystem.</em></p>";

    private static string GetAKSContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Microsoft Azure</p>
            <p class='info-desc'>Managed Kubernetes on Azure. Free control plane - pay only for worker nodes.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Managed by Azure (free)</li>
            <li><strong>System Pool:</strong> For cluster services</li>
            <li><strong>User Pool:</strong> Application workloads</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Azure AD integration</li>
            <li>Azure Monitor and Container Insights</li>
            <li>Virtual Nodes (ACI) for burst scaling</li>
            <li>Azure Arc for hybrid/multi-cloud</li>
            <li>Best .NET and Windows container support</li>
        </ul>
        <h4>Pricing</h4>
        <ul>
            <li>Cluster: FREE (control plane)</li>
            <li>Uptime SLA: $0.10/hour (optional)</li>
            <li>Worker nodes: VM pricing</li>
        </ul>
        <p class='info-note'><em>Note: Best for Microsoft shops and hybrid cloud with Azure Arc.</em></p>";

    private static string GetGKEContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Google Cloud</p>
            <p class='info-desc'>The original managed Kubernetes. Google invented Kubernetes (from Borg). Most mature managed offering.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Managed by Google</li>
            <li><strong>Node Pools:</strong> Groups of identical VMs</li>
            <li><strong>Autopilot:</strong> Fully managed (Google manages nodes too)</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>GKE Autopilot - zero node management</li>
            <li>Anthos for multi-cloud/hybrid</li>
            <li>Best-in-class auto-scaling</li>
            <li>Release channels (rapid, regular, stable)</li>
            <li>4-way autoscaling (HPA, VPA, CA, NAP)</li>
        </ul>
        <h4>Pricing</h4>
        <ul>
            <li>Standard: $0.10/hour (~$73/month)</li>
            <li>Autopilot: Per pod resource usage</li>
            <li>Worker nodes: Compute Engine pricing</li>
        </ul>
        <p class='info-note'><em>Note: Best for pure Kubernetes experience. Most innovative features.</em></p>";

    private static string GetOKEContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Oracle Cloud Infrastructure</p>
            <p class='info-desc'>Managed Kubernetes on Oracle Cloud. Optimized for Oracle workloads and databases.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Managed by Oracle (free)</li>
            <li><strong>Node Pools:</strong> Worker node groups</li>
            <li><strong>Virtual Nodes:</strong> Serverless option</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Native Oracle DB integration</li>
            <li>OCI Registry included</li>
            <li>Flexible shapes (custom CPU/RAM ratios)</li>
            <li>Arm-based Ampere nodes available</li>
            <li>Free control plane</li>
        </ul>
        <h4>Pricing</h4>
        <ul>
            <li>Cluster: FREE (control plane)</li>
            <li>Worker nodes: OCI Compute pricing</li>
            <li>Competitive pricing vs other clouds</li>
        </ul>
        <p class='info-note'><em>Note: Best for Oracle Database workloads and Java applications.</em></p>";

    private static string GetRKE2Content() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> SUSE</p>
            <p class='info-desc'>Security-focused Kubernetes distribution. Also known as RKE Government. FIPS 140-2 compliant with CIS hardening.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Server:</strong> Control plane + etcd (3 for HA)</li>
            <li><strong>Agent:</strong> Worker nodes</li>
            <li><em>Single binary deployment like K3s</em></li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>FIPS 140-2 compliance</li>
            <li>CIS Benchmark hardening out-of-box</li>
            <li>SELinux support</li>
            <li>Embedded etcd (no external dependency)</li>
            <li>Air-gap deployment support</li>
        </ul>
        <h4>Sizing Notes</h4>
        <ul>
            <li>Server: 4 vCPU, 8 GB RAM, 100 GB disk</li>
            <li>Agent: 4 vCPU, 8 GB RAM, 50 GB disk</li>
        </ul>
        <p class='info-note'><em>Note: Best for government, defense, and high-security environments.</em></p>";

    private static string GetOSDContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Red Hat</p>
            <p class='info-desc'>Fully managed OpenShift on Google Cloud Platform. Red Hat manages the entire stack.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Fully managed by Red Hat</li>
            <li><strong>Infrastructure:</strong> Managed nodes for cluster services</li>
            <li><strong>Worker:</strong> Application workload nodes (GCE)</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>SLA-backed managed service (99.95%)</li>
            <li>GCP native integrations</li>
            <li>Anthos compatibility</li>
            <li>Red Hat support included</li>
            <li>Customer Cloud Subscription (CCS) option</li>
        </ul>
        <h4>Pricing</h4>
        <ul>
            <li>Base cluster fee + per-worker pricing</li>
            <li>GCE instances: Standard GCP pricing</li>
        </ul>
        <p class='info-note'><em>Note: Best for enterprises wanting OpenShift on GCP with full management.</em></p>";

    private static string GetOSIBMContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Red Hat (IBM)</p>
            <p class='info-desc'>Managed OpenShift on IBM Cloud. Deep integration with IBM Cloud services and Watson AI.</p>
        </div>
        <h4>Node Architecture</h4>
        <ul>
            <li><strong>Control Plane:</strong> Managed by IBM</li>
            <li><strong>Worker:</strong> IBM Cloud virtual servers</li>
            <li><strong>Satellite:</strong> Extend to on-premises/edge</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Watson AI/ML integrations</li>
            <li>IBM Cloud Pak support</li>
            <li>IBM Cloud Satellite for hybrid</li>
            <li>Integrated IBM Cloud services</li>
            <li>Compliance certifications (SOC2, HIPAA)</li>
        </ul>
        <h4>Pricing</h4>
        <ul>
            <li>Cluster fee: IBM Cloud pricing</li>
            <li>Worker nodes: Virtual server pricing</li>
        </ul>
        <p class='info-note'><em>Note: Best for enterprises using IBM Cloud Paks and Watson services.</em></p>";
}

/// <summary>
/// Default content for technologies.
/// </summary>
internal static class DefaultTechnologyContent
{
    public static InfoContent Get(Technology technology)
    {
        return technology switch
        {
            Technology.DotNet => new(".NET", GetDotNetContent()) { IsFromDatabase = false },
            Technology.Java => new("Java", GetJavaContent()) { IsFromDatabase = false },
            Technology.NodeJs => new("Node.js", GetNodeJsContent()) { IsFromDatabase = false },
            Technology.Python => new("Python", GetPythonContent()) { IsFromDatabase = false },
            Technology.Go => new("Go", GetGoContent()) { IsFromDatabase = false },
            Technology.Mendix => new("Mendix", GetMendixContent()) { IsFromDatabase = false },
            Technology.OutSystems => new("OutSystems", GetOutSystemsContent()) { IsFromDatabase = false },
            _ => new(technology.ToString(), $"<p>Information for {technology}.</p>") { IsFromDatabase = false }
        };
    }

    private static string GetDotNetContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Microsoft</p>
            <p class='info-desc'>Cross-platform, high-performance framework for building modern cloud-based applications.</p>
        </div>
        <h4>Resource Profile</h4>
        <ul>
            <li><strong>CPU:</strong> Efficient - optimized for high-throughput</li>
            <li><strong>Memory:</strong> Low baseline, efficient GC</li>
            <li><strong>Startup:</strong> Fast with Native AOT option</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Cross-platform (Windows, Linux, macOS)</li>
            <li>High performance with minimal allocation</li>
            <li>Excellent tooling (Visual Studio, VS Code)</li>
            <li>Strong typing with C#</li>
            <li>Native container support</li>
        </ul>
        <h4>Best For</h4>
        <ul>
            <li>Enterprise applications</li>
            <li>Microservices</li>
            <li>High-performance APIs</li>
            <li>Windows integration scenarios</li>
        </ul>
        <p class='info-note'><em>Recommended for enterprise workloads requiring reliability and performance.</em></p>";

    private static string GetJavaContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Oracle / OpenJDK</p>
            <p class='info-desc'>Enterprise-grade runtime with excellent ecosystem and mature tooling.</p>
        </div>
        <h4>Resource Profile</h4>
        <ul>
            <li><strong>CPU:</strong> Good - JIT optimization over time</li>
            <li><strong>Memory:</strong> Higher baseline (JVM overhead)</li>
            <li><strong>Startup:</strong> Slower (JVM warm-up)</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Write once, run anywhere</li>
            <li>Massive ecosystem (Spring, Jakarta EE)</li>
            <li>Excellent IDE support</li>
            <li>GraalVM for native compilation</li>
            <li>Strong enterprise adoption</li>
        </ul>
        <h4>Best For</h4>
        <ul>
            <li>Legacy enterprise systems</li>
            <li>Spring Boot microservices</li>
            <li>Big data processing (Hadoop, Spark)</li>
            <li>Android development</li>
        </ul>
        <p class='info-note'><em>Allocate more memory for JVM. Consider -Xmx limits for containers.</em></p>";

    private static string GetNodeJsContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> OpenJS Foundation</p>
            <p class='info-desc'>JavaScript runtime for event-driven, non-blocking I/O applications.</p>
        </div>
        <h4>Resource Profile</h4>
        <ul>
            <li><strong>CPU:</strong> Single-threaded (use cluster mode)</li>
            <li><strong>Memory:</strong> Low-moderate</li>
            <li><strong>Startup:</strong> Very fast</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Event-driven, non-blocking I/O</li>
            <li>Huge npm ecosystem</li>
            <li>Full-stack JavaScript</li>
            <li>Great for real-time applications</li>
            <li>Easy to containerize</li>
        </ul>
        <h4>Best For</h4>
        <ul>
            <li>Real-time applications (chat, streaming)</li>
            <li>API gateways</li>
            <li>Server-side rendering</li>
            <li>Microservices with I/O-bound workloads</li>
        </ul>
        <p class='info-note'><em>Use cluster mode or multiple replicas for CPU-intensive workloads.</em></p>";

    private static string GetPythonContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Python Software Foundation</p>
            <p class='info-desc'>Versatile language for web, data science, ML, and automation.</p>
        </div>
        <h4>Resource Profile</h4>
        <ul>
            <li><strong>CPU:</strong> GIL limits parallelism</li>
            <li><strong>Memory:</strong> Moderate</li>
            <li><strong>Startup:</strong> Moderate</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Clean, readable syntax</li>
            <li>Excellent for data science/ML</li>
            <li>Large standard library</li>
            <li>Django, Flask, FastAPI frameworks</li>
            <li>Strong AI/ML ecosystem (TensorFlow, PyTorch)</li>
        </ul>
        <h4>Best For</h4>
        <ul>
            <li>Data science and analytics</li>
            <li>Machine learning applications</li>
            <li>Automation and scripting</li>
            <li>REST APIs with FastAPI</li>
        </ul>
        <p class='info-note'><em>Use async frameworks or multiprocessing for CPU-bound workloads.</em></p>";

    private static string GetGoContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Google</p>
            <p class='info-desc'>Compiled language designed for simplicity, efficiency, and concurrency.</p>
        </div>
        <h4>Resource Profile</h4>
        <ul>
            <li><strong>CPU:</strong> Excellent - native compilation</li>
            <li><strong>Memory:</strong> Very low footprint</li>
            <li><strong>Startup:</strong> Instant (static binary)</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Static compilation - single binary</li>
            <li>Built-in concurrency (goroutines)</li>
            <li>Fast compilation</li>
            <li>Excellent for CLI tools</li>
            <li>Kubernetes itself is written in Go</li>
        </ul>
        <h4>Best For</h4>
        <ul>
            <li>Cloud-native microservices</li>
            <li>CLI tools and utilities</li>
            <li>High-performance APIs</li>
            <li>Infrastructure tooling</li>
        </ul>
        <p class='info-note'><em>Ideal for Kubernetes workloads. Minimal container images possible.</em></p>";

    private static string GetMendixContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> Siemens (Mendix)</p>
            <p class='info-desc'>Enterprise low-code platform for rapid application development.</p>
        </div>
        <h4>Resource Profile</h4>
        <ul>
            <li><strong>CPU:</strong> Higher (runtime overhead)</li>
            <li><strong>Memory:</strong> Higher (Java-based runtime + model)</li>
            <li><strong>Startup:</strong> Slower (model loading)</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Visual development with Studio Pro</li>
            <li>Built-in CI/CD</li>
            <li>Pre-built connectors and widgets</li>
            <li>Enterprise security features</li>
            <li>Multi-cloud deployment options</li>
        </ul>
        <h4>Deployment Options</h4>
        <ul>
            <li>Mendix Cloud (SaaS)</li>
            <li>Mendix for Private Cloud (K8s)</li>
            <li>Self-managed on VMs</li>
            <li>SAP BTP integration</li>
        </ul>
        <p class='info-note'><em>Plan for 2-3x more resources compared to native applications.</em></p>";

    private static string GetOutSystemsContent() => @"
        <div class='info-section'>
            <p class='info-vendor'><strong>Vendor:</strong> OutSystems</p>
            <p class='info-desc'>High-performance low-code platform for enterprise applications.</p>
        </div>
        <h4>Resource Profile</h4>
        <ul>
            <li><strong>CPU:</strong> Higher (platform overhead)</li>
            <li><strong>Memory:</strong> Higher (runtime + database)</li>
            <li><strong>Startup:</strong> Moderate</li>
        </ul>
        <h4>Key Features</h4>
        <ul>
            <li>Full-stack visual development</li>
            <li>AI-assisted development</li>
            <li>Built-in security and compliance</li>
            <li>Native mobile app generation</li>
            <li>Enterprise integrations</li>
        </ul>
        <h4>Deployment Options</h4>
        <ul>
            <li>OutSystems Cloud</li>
            <li>Private Cloud (Kubernetes)</li>
            <li>Self-managed infrastructure</li>
        </ul>
        <p class='info-note'><em>Licensing is based on Application Objects (AOs). Plan capacity accordingly.</em></p>";
}
