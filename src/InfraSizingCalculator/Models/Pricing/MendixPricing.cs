namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// Mendix deployment categories
/// </summary>
public enum MendixDeploymentCategory
{
    Cloud,          // Mendix Cloud SaaS or Dedicated
    PrivateCloud,   // Mendix on Azure, Kubernetes (supported providers)
    Other           // Server (VMs), StackIT, SAP BTP, Docker
}

/// <summary>
/// Mendix Cloud deployment types
/// </summary>
public enum MendixCloudType
{
    SaaS,           // Multi-tenant Mendix Cloud
    Dedicated       // Single-tenant AWS VPC
}

/// <summary>
/// Mendix Private Cloud providers
/// </summary>
public enum MendixPrivateCloudProvider
{
    // Officially Supported
    Azure,          // Mendix on Azure (managed)
    EKS,            // Amazon EKS
    AKS,            // Azure AKS
    GKE,            // Google GKE
    OpenShift,      // Red Hat OpenShift

    // Manual Setup / Not Officially Supported
    GenericK8s,     // Generic Kubernetes 1.19+
    Rancher,        // Rancher / RKE2
    K3s,            // K3s lightweight
    Docker          // Docker standalone
}

/// <summary>
/// Mendix other deployment options (Server-based, Partner clouds)
/// </summary>
public enum MendixOtherDeployment
{
    Server,         // Windows/Linux VMs + Docker
    StackIT,        // German sovereign cloud
    SapBtp          // SAP Business Technology Platform
}

/// <summary>
/// Mendix Cloud Resource Pack tiers (SLA levels)
/// </summary>
public enum MendixResourcePackTier
{
    Standard,       // 99.5% SLA
    Premium,        // 99.95% SLA + Fallback
    PremiumPlus     // 99.95% SLA + Fallback + Multi-region Failover
}

/// <summary>
/// Mendix Cloud Resource Pack sizes
/// Verified from Mendix Deployment Options PriceBook (June 2025)
/// </summary>
public enum MendixResourcePackSize
{
    // Standard compute sizes
    XS,
    S,
    M,
    L,
    XL,
    XXL,
    XXXL,
    FourXL,

    // DB-enhanced sizes (compute + enhanced database)
    XS_SDB,      // XS with Small DB
    S_MDB,       // S with Medium DB
    M_LDB,       // M with Large DB
    L_XLDB,      // L with XL DB
    XL_XXLDB,    // XL with XXL DB
    XXL_XXXLDB,  // XXL with XXXL DB
    XXXL_4XLDB,  // XXXL with 4XL DB
    FourXL_5XLDB // 4XL with 5XL DB
}

/// <summary>
/// Mendix Resource Pack specifications
/// </summary>
public class MendixResourcePackSpec
{
    public MendixResourcePackSize Size { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    // Mendix Runtime Resources
    public decimal MxMemoryGB { get; set; }
    public decimal MxVCpu { get; set; }

    // Database Resources
    public decimal DbMemoryGB { get; set; }
    public int DbVCpu { get; set; }
    public decimal DbStorageGB { get; set; }

    // Storage
    public decimal FileStorageGB { get; set; }

    // Pricing
    public decimal PricePerYear { get; set; }
    public int CloudTokens { get; set; }

    // Features
    public decimal UptimeSla { get; set; } = 99.5m;
    public bool HasFallback { get; set; }
    public bool HasMultiRegionFailover { get; set; }
}

/// <summary>
/// Mendix Kubernetes environment tier pricing
/// </summary>
public class MendixK8sEnvironmentTier
{
    public int MinEnvironments { get; set; }
    public int MaxEnvironments { get; set; } // -1 for unlimited
    public decimal PricePerEnvironment { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Mendix GenAI Resource Pack
/// </summary>
public class MendixGenAIModelPack
{
    public string Size { get; set; } = string.Empty; // S, M, L
    public long ClaudeTokensInPerMonth { get; set; }
    public long ClaudeTokensOutPerMonth { get; set; }
    public long CohereTokensInPerMonth { get; set; }
    public decimal PricePerYear { get; set; }
    public int CloudTokens { get; set; }
}

/// <summary>
/// Complete Mendix pricing configuration - All values from official pricebook
/// </summary>
public class MendixPricingSettings
{
    // Token System
    public decimal CloudTokenPrice { get; set; } = 51.60m;

    // ==================== MENDIX CLOUD (SaaS) ====================

    /// <summary>
    /// Standard Resource Packs (99.5% SLA)
    /// Verified from Mendix Deployment Options PriceBook (June 2025)
    /// </summary>
    public List<MendixResourcePackSpec> StandardResourcePacks { get; set; } = new()
    {
        // Compute-only sizes
        new() { Size = MendixResourcePackSize.XS, DisplayName = "XS", MxMemoryGB = 1, MxVCpu = 0.25m, DbMemoryGB = 1, DbVCpu = 2, DbStorageGB = 5, FileStorageGB = 10, PricePerYear = 516, CloudTokens = 10, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.S, DisplayName = "S", MxMemoryGB = 2, MxVCpu = 0.5m, DbMemoryGB = 2, DbVCpu = 2, DbStorageGB = 10, FileStorageGB = 20, PricePerYear = 1032, CloudTokens = 20, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.M, DisplayName = "M", MxMemoryGB = 4, MxVCpu = 1, DbMemoryGB = 4, DbVCpu = 2, DbStorageGB = 20, FileStorageGB = 40, PricePerYear = 2580, CloudTokens = 50, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.L, DisplayName = "L", MxMemoryGB = 8, MxVCpu = 2, DbMemoryGB = 8, DbVCpu = 2, DbStorageGB = 40, FileStorageGB = 80, PricePerYear = 5160, CloudTokens = 100, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.XL, DisplayName = "XL", MxMemoryGB = 16, MxVCpu = 4, DbMemoryGB = 16, DbVCpu = 4, DbStorageGB = 80, FileStorageGB = 160, PricePerYear = 10320, CloudTokens = 200, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.XXL, DisplayName = "XXL", MxMemoryGB = 32, MxVCpu = 8, DbMemoryGB = 32, DbVCpu = 4, DbStorageGB = 160, FileStorageGB = 320, PricePerYear = 20640, CloudTokens = 400, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.XXXL, DisplayName = "XXXL", MxMemoryGB = 64, MxVCpu = 16, DbMemoryGB = 64, DbVCpu = 8, DbStorageGB = 320, FileStorageGB = 640, PricePerYear = 41280, CloudTokens = 800, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.FourXL, DisplayName = "4XL", MxMemoryGB = 128, MxVCpu = 32, DbMemoryGB = 128, DbVCpu = 16, DbStorageGB = 640, FileStorageGB = 1280, PricePerYear = 82560, CloudTokens = 1600, UptimeSla = 99.5m },

        // DB-enhanced sizes (compute + enhanced database)
        new() { Size = MendixResourcePackSize.XS_SDB, DisplayName = "XS-SDB", MxMemoryGB = 1, MxVCpu = 0.25m, DbMemoryGB = 2, DbVCpu = 2, DbStorageGB = 10, FileStorageGB = 10, PricePerYear = 1032, CloudTokens = 20, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.S_MDB, DisplayName = "S-MDB", MxMemoryGB = 2, MxVCpu = 0.5m, DbMemoryGB = 4, DbVCpu = 2, DbStorageGB = 20, FileStorageGB = 20, PricePerYear = 2580, CloudTokens = 50, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.M_LDB, DisplayName = "M-LDB", MxMemoryGB = 4, MxVCpu = 1, DbMemoryGB = 8, DbVCpu = 2, DbStorageGB = 40, FileStorageGB = 40, PricePerYear = 5160, CloudTokens = 100, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.L_XLDB, DisplayName = "L-XLDB", MxMemoryGB = 8, MxVCpu = 2, DbMemoryGB = 16, DbVCpu = 4, DbStorageGB = 80, FileStorageGB = 80, PricePerYear = 10320, CloudTokens = 200, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.XL_XXLDB, DisplayName = "XL-XXLDB", MxMemoryGB = 16, MxVCpu = 4, DbMemoryGB = 32, DbVCpu = 4, DbStorageGB = 160, FileStorageGB = 160, PricePerYear = 20640, CloudTokens = 400, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.XXL_XXXLDB, DisplayName = "XXL-XXXLDB", MxMemoryGB = 32, MxVCpu = 8, DbMemoryGB = 64, DbVCpu = 8, DbStorageGB = 320, FileStorageGB = 320, PricePerYear = 41280, CloudTokens = 800, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.XXXL_4XLDB, DisplayName = "XXXL-4XLDB", MxMemoryGB = 64, MxVCpu = 16, DbMemoryGB = 128, DbVCpu = 16, DbStorageGB = 640, FileStorageGB = 640, PricePerYear = 82560, CloudTokens = 1600, UptimeSla = 99.5m },
        new() { Size = MendixResourcePackSize.FourXL_5XLDB, DisplayName = "4XL-5XLDB", MxMemoryGB = 128, MxVCpu = 32, DbMemoryGB = 256, DbVCpu = 32, DbStorageGB = 1280, FileStorageGB = 1280, PricePerYear = 115584, CloudTokens = 2240, UptimeSla = 99.5m }
    };

    /// <summary>
    /// Premium Resource Packs (99.95% SLA + Fallback)
    /// Verified from Mendix Deployment Options PriceBook (June 2025)
    /// </summary>
    public List<MendixResourcePackSpec> PremiumResourcePacks { get; set; } = new()
    {
        // Compute-only sizes
        new() { Size = MendixResourcePackSize.S, DisplayName = "S", MxMemoryGB = 2, MxVCpu = 0.5m, DbMemoryGB = 2, DbVCpu = 2, DbStorageGB = 10, FileStorageGB = 20, PricePerYear = 1548, CloudTokens = 30, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.M, DisplayName = "M", MxMemoryGB = 4, MxVCpu = 1, DbMemoryGB = 4, DbVCpu = 2, DbStorageGB = 20, FileStorageGB = 40, PricePerYear = 3870, CloudTokens = 75, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.L, DisplayName = "L", MxMemoryGB = 8, MxVCpu = 2, DbMemoryGB = 8, DbVCpu = 2, DbStorageGB = 40, FileStorageGB = 80, PricePerYear = 7740, CloudTokens = 150, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.XL, DisplayName = "XL", MxMemoryGB = 16, MxVCpu = 4, DbMemoryGB = 16, DbVCpu = 4, DbStorageGB = 80, FileStorageGB = 160, PricePerYear = 15480, CloudTokens = 300, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.XXL, DisplayName = "XXL", MxMemoryGB = 32, MxVCpu = 8, DbMemoryGB = 32, DbVCpu = 4, DbStorageGB = 160, FileStorageGB = 320, PricePerYear = 30960, CloudTokens = 600, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.XXXL, DisplayName = "XXXL", MxMemoryGB = 64, MxVCpu = 16, DbMemoryGB = 64, DbVCpu = 8, DbStorageGB = 320, FileStorageGB = 640, PricePerYear = 61920, CloudTokens = 1200, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.FourXL, DisplayName = "4XL", MxMemoryGB = 128, MxVCpu = 32, DbMemoryGB = 128, DbVCpu = 16, DbStorageGB = 640, FileStorageGB = 1280, PricePerYear = 123840, CloudTokens = 2400, UptimeSla = 99.95m, HasFallback = true },

        // DB-enhanced sizes (compute + enhanced database)
        new() { Size = MendixResourcePackSize.S_MDB, DisplayName = "S-MDB", MxMemoryGB = 2, MxVCpu = 0.5m, DbMemoryGB = 4, DbVCpu = 2, DbStorageGB = 20, FileStorageGB = 20, PricePerYear = 3870, CloudTokens = 75, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.M_LDB, DisplayName = "M-LDB", MxMemoryGB = 4, MxVCpu = 1, DbMemoryGB = 8, DbVCpu = 2, DbStorageGB = 40, FileStorageGB = 40, PricePerYear = 7740, CloudTokens = 150, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.L_XLDB, DisplayName = "L-XLDB", MxMemoryGB = 8, MxVCpu = 2, DbMemoryGB = 16, DbVCpu = 4, DbStorageGB = 80, FileStorageGB = 80, PricePerYear = 15480, CloudTokens = 300, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.XL_XXLDB, DisplayName = "XL-XXLDB", MxMemoryGB = 16, MxVCpu = 4, DbMemoryGB = 32, DbVCpu = 4, DbStorageGB = 160, FileStorageGB = 160, PricePerYear = 30960, CloudTokens = 600, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.XXL_XXXLDB, DisplayName = "XXL-XXXLDB", MxMemoryGB = 32, MxVCpu = 8, DbMemoryGB = 64, DbVCpu = 8, DbStorageGB = 320, FileStorageGB = 320, PricePerYear = 61920, CloudTokens = 1200, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.XXXL_4XLDB, DisplayName = "XXXL-4XLDB", MxMemoryGB = 64, MxVCpu = 16, DbMemoryGB = 128, DbVCpu = 16, DbStorageGB = 640, FileStorageGB = 640, PricePerYear = 123840, CloudTokens = 2400, UptimeSla = 99.95m, HasFallback = true },
        new() { Size = MendixResourcePackSize.FourXL_5XLDB, DisplayName = "4XL-5XLDB", MxMemoryGB = 128, MxVCpu = 32, DbMemoryGB = 256, DbVCpu = 32, DbStorageGB = 1280, FileStorageGB = 1280, PricePerYear = 173376, CloudTokens = 3360, UptimeSla = 99.95m, HasFallback = true }
    };

    /// <summary>
    /// Premium Plus Resource Packs (99.95% SLA + Fallback + Multi-region Failover)
    /// Verified from Mendix Deployment Options PriceBook (June 2025)
    /// </summary>
    public List<MendixResourcePackSpec> PremiumPlusResourcePacks { get; set; } = new()
    {
        // Compute-only sizes (start at XL for Premium Plus)
        new() { Size = MendixResourcePackSize.XL, DisplayName = "XL", MxMemoryGB = 16, MxVCpu = 4, DbMemoryGB = 16, DbVCpu = 4, DbStorageGB = 80, FileStorageGB = 160, PricePerYear = 20640, CloudTokens = 400, UptimeSla = 99.95m, HasFallback = true, HasMultiRegionFailover = true },
        new() { Size = MendixResourcePackSize.XXL, DisplayName = "XXL", MxMemoryGB = 32, MxVCpu = 8, DbMemoryGB = 32, DbVCpu = 4, DbStorageGB = 160, FileStorageGB = 320, PricePerYear = 41280, CloudTokens = 800, UptimeSla = 99.95m, HasFallback = true, HasMultiRegionFailover = true },
        new() { Size = MendixResourcePackSize.XXXL, DisplayName = "XXXL", MxMemoryGB = 64, MxVCpu = 16, DbMemoryGB = 64, DbVCpu = 8, DbStorageGB = 320, FileStorageGB = 640, PricePerYear = 82560, CloudTokens = 1600, UptimeSla = 99.95m, HasFallback = true, HasMultiRegionFailover = true },
        new() { Size = MendixResourcePackSize.FourXL, DisplayName = "4XL", MxMemoryGB = 128, MxVCpu = 32, DbMemoryGB = 128, DbVCpu = 16, DbStorageGB = 640, FileStorageGB = 1280, PricePerYear = 165120, CloudTokens = 3200, UptimeSla = 99.95m, HasFallback = true, HasMultiRegionFailover = true },

        // DB-enhanced sizes
        new() { Size = MendixResourcePackSize.XL_XXLDB, DisplayName = "XL-XXLDB", MxMemoryGB = 16, MxVCpu = 4, DbMemoryGB = 32, DbVCpu = 4, DbStorageGB = 160, FileStorageGB = 160, PricePerYear = 41280, CloudTokens = 800, UptimeSla = 99.95m, HasFallback = true, HasMultiRegionFailover = true },
        new() { Size = MendixResourcePackSize.XXL_XXXLDB, DisplayName = "XXL-XXXLDB", MxMemoryGB = 32, MxVCpu = 8, DbMemoryGB = 64, DbVCpu = 8, DbStorageGB = 320, FileStorageGB = 320, PricePerYear = 82560, CloudTokens = 1600, UptimeSla = 99.95m, HasFallback = true, HasMultiRegionFailover = true },
        new() { Size = MendixResourcePackSize.XXXL_4XLDB, DisplayName = "XXXL-4XLDB", MxMemoryGB = 64, MxVCpu = 16, DbMemoryGB = 128, DbVCpu = 16, DbStorageGB = 640, FileStorageGB = 640, PricePerYear = 165120, CloudTokens = 3200, UptimeSla = 99.95m, HasFallback = true, HasMultiRegionFailover = true },
        new() { Size = MendixResourcePackSize.FourXL_5XLDB, DisplayName = "4XL-5XLDB", MxMemoryGB = 128, MxVCpu = 32, DbMemoryGB = 256, DbVCpu = 32, DbStorageGB = 1280, FileStorageGB = 1280, PricePerYear = 288960, CloudTokens = 5600, UptimeSla = 99.95m, HasFallback = true, HasMultiRegionFailover = true }
    };

    /// <summary>
    /// Additional Storage pricing (per 100 GB per year)
    /// </summary>
    public decimal AdditionalFileStoragePer100GB { get; set; } = 123m;
    public decimal AdditionalDatabaseStoragePer100GB { get; set; } = 246m;

    /// <summary>
    /// Mendix Cloud Dedicated (single tenant AWS VPC)
    /// </summary>
    public decimal CloudDedicatedPricePerYear { get; set; } = 368100m;

    // ==================== MENDIX ON AZURE ====================

    /// <summary>
    /// Mendix on Azure - Base Package (includes 3 environments)
    /// </summary>
    public decimal AzureBasePricePerYear { get; set; } = 6612m;
    public int AzureBaseEnvironmentsIncluded { get; set; } = 3;

    /// <summary>
    /// Mendix on Azure - Additional Environment pricing
    /// </summary>
    public decimal AzureAdditionalEnvironmentPrice { get; set; } = 722.40m;
    public int AzureAdditionalEnvironmentTokens { get; set; } = 14;

    // ==================== MENDIX ON KUBERNETES ====================

    /// <summary>
    /// Mendix on Kubernetes - Base Package (includes 3 environments)
    /// </summary>
    public decimal K8sBasePricePerYear { get; set; } = 6360m;
    public int K8sBaseEnvironmentsIncluded { get; set; } = 3;

    /// <summary>
    /// Mendix on Kubernetes - Environment tier pricing
    /// Per MENDIX_PRICING_SPEC.md - tiers based on TOTAL environment number:
    /// - Tier 1: environments 4-50 (47 environments at $552)
    /// - Tier 2: environments 51-100 (50 environments at $408)
    /// - Tier 3: environments 101-150 (50 environments at $240)
    /// - Tier 4: environments 151+ (free)
    /// </summary>
    public List<MendixK8sEnvironmentTier> K8sEnvironmentTiers { get; set; } = new()
    {
        new() { MinEnvironments = 4, MaxEnvironments = 50, PricePerEnvironment = 552m, Description = "Environments 4-50" },
        new() { MinEnvironments = 51, MaxEnvironments = 100, PricePerEnvironment = 408m, Description = "Environments 51-100" },
        new() { MinEnvironments = 101, MaxEnvironments = 150, PricePerEnvironment = 240m, Description = "Environments 101-150" },
        new() { MinEnvironments = 151, MaxEnvironments = -1, PricePerEnvironment = 0m, Description = "Environments 151+ (Free)" }
    };

    // ==================== MENDIX ON SERVER (VMs/Docker) ====================

    /// <summary>
    /// Mendix on Server - Per application pricing
    /// </summary>
    public decimal ServerPerAppPricePerYear { get; set; } = 6612m;

    /// <summary>
    /// Mendix on Server - Unlimited applications pricing
    /// </summary>
    public decimal ServerUnlimitedAppsPricePerYear { get; set; } = 33060m;

    // ==================== MENDIX ON STACKIT ====================

    /// <summary>
    /// Mendix on StackIT - Per application pricing
    /// </summary>
    public decimal StackITPerAppPricePerYear { get; set; } = 6612m;

    /// <summary>
    /// Mendix on StackIT - Unlimited applications pricing
    /// </summary>
    public decimal StackITUnlimitedAppsPricePerYear { get; set; } = 33060m;

    // ==================== MENDIX ON SAP BTP ====================

    /// <summary>
    /// Mendix on SAP BTP - Per application pricing
    /// </summary>
    public decimal SapBtpPerAppPricePerYear { get; set; } = 6612m;

    /// <summary>
    /// Mendix on SAP BTP - Unlimited applications pricing
    /// </summary>
    public decimal SapBtpUnlimitedAppsPricePerYear { get; set; } = 33060m;

    // ==================== GENAI RESOURCE PACKS (Optional) ====================

    /// <summary>
    /// GenAI Model Resource Packs (Claude v3.5 Sonnet + Cohere Embed v3)
    /// </summary>
    public List<MendixGenAIModelPack> GenAIModelPacks { get; set; } = new()
    {
        new() { Size = "S", ClaudeTokensInPerMonth = 2500000, ClaudeTokensOutPerMonth = 1250000, CohereTokensInPerMonth = 5000000, PricePerYear = 1857.60m, CloudTokens = 36 },
        new() { Size = "M", ClaudeTokensInPerMonth = 5000000, ClaudeTokensOutPerMonth = 2500000, CohereTokensInPerMonth = 10000000, PricePerYear = 3715.20m, CloudTokens = 72 },
        new() { Size = "L", ClaudeTokensInPerMonth = 10000000, ClaudeTokensOutPerMonth = 5000000, CohereTokensInPerMonth = 20000000, PricePerYear = 7430.40m, CloudTokens = 144 }
    };

    /// <summary>
    /// GenAI Knowledge Base Resource Pack (OpenSearch)
    /// </summary>
    public decimal GenAIKnowledgeBasePricePerYear { get; set; } = 2476.80m;
    public int GenAIKnowledgeBaseTokens { get; set; } = 48;
    public decimal GenAIKnowledgeBaseDiskGB { get; set; } = 10m;

    // ==================== PLATFORM LICENSING (from Commercial Proposal) ====================

    /// <summary>
    /// Platform license tiers
    /// </summary>
    public decimal PlatformPremiumUnlimitedPerYear { get; set; } = 65400m;

    /// <summary>
    /// User licensing
    /// </summary>
    public decimal InternalUsersPer100PerYear { get; set; } = 40800m;
    public decimal ExternalUsersPer250KPerYear { get; set; } = 60000m;

    /// <summary>
    /// Volume discount percentage
    /// </summary>
    public decimal VolumeDiscountPercent { get; set; } = 10m;

    // ==================== SERVICES (Optional) ====================

    /// <summary>
    /// Customer Enablement from Expert Services (one-time)
    /// </summary>
    public decimal CustomerEnablementPrice { get; set; } = 45000m;

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Calculate K8s environment cost based on tiered pricing.
    /// Tiers are based on TOTAL environment count (not additional):
    /// - Environments 1-3: Included (free)
    /// - Environments 4-50: Tier 1 ($552/env)
    /// - Environments 51-100: Tier 2 ($408/env)
    /// - Environments 101-150: Tier 3 ($240/env)
    /// - Environments 151+: Tier 4 (free)
    /// </summary>
    public decimal CalculateK8sEnvironmentCost(int totalEnvironments)
    {
        if (totalEnvironments <= K8sBaseEnvironmentsIncluded)
            return 0;

        decimal totalCost = 0;
        int firstBillableEnv = K8sBaseEnvironmentsIncluded + 1; // Environment 4

        foreach (var tier in K8sEnvironmentTiers.OrderBy(t => t.MinEnvironments))
        {
            // Skip tiers entirely before our billable range
            if (tier.MaxEnvironments != -1 && tier.MaxEnvironments < firstBillableEnv)
                continue;

            // Skip tiers entirely after our total
            if (tier.MinEnvironments > totalEnvironments)
                break;

            // Calculate how many environments fall into this tier
            int tierStart = Math.Max(tier.MinEnvironments, firstBillableEnv);
            int tierEnd = tier.MaxEnvironments == -1
                ? totalEnvironments
                : Math.Min(tier.MaxEnvironments, totalEnvironments);

            int envsInTier = tierEnd - tierStart + 1;

            if (envsInTier > 0)
            {
                totalCost += envsInTier * tier.PricePerEnvironment;
            }
        }

        return totalCost;
    }

    /// <summary>
    /// Get resource pack by tier and size
    /// </summary>
    public MendixResourcePackSpec? GetResourcePack(MendixResourcePackTier tier, MendixResourcePackSize size)
    {
        return tier switch
        {
            MendixResourcePackTier.Standard => StandardResourcePacks.FirstOrDefault(p => p.Size == size),
            MendixResourcePackTier.Premium => PremiumResourcePacks.FirstOrDefault(p => p.Size == size),
            MendixResourcePackTier.PremiumPlus => PremiumPlusResourcePacks.FirstOrDefault(p => p.Size == size),
            _ => null
        };
    }

    /// <summary>
    /// Get available sizes for a tier
    /// </summary>
    public List<MendixResourcePackSpec> GetAvailablePacks(MendixResourcePackTier tier)
    {
        return tier switch
        {
            MendixResourcePackTier.Standard => StandardResourcePacks,
            MendixResourcePackTier.Premium => PremiumResourcePacks,
            MendixResourcePackTier.PremiumPlus => PremiumPlusResourcePacks,
            _ => new List<MendixResourcePackSpec>()
        };
    }
}

/// <summary>
/// Mendix deployment configuration selected by user
/// </summary>
public class MendixDeploymentConfig
{
    public MendixDeploymentCategory Category { get; set; }

    // Cloud options
    public MendixCloudType? CloudType { get; set; }
    public MendixResourcePackTier? ResourcePackTier { get; set; }
    public MendixResourcePackSize? ResourcePackSize { get; set; }
    public int ResourcePackQuantity { get; set; } = 1;

    // Private Cloud options
    public MendixPrivateCloudProvider? PrivateCloudProvider { get; set; }
    public int NumberOfEnvironments { get; set; } = 3;

    // Other deployment options
    public MendixOtherDeployment? OtherDeployment { get; set; }
    public bool IsUnlimitedApps { get; set; } = true;
    public int NumberOfApps { get; set; } = 1;

    // User licensing
    public int InternalUsers { get; set; } = 100;
    public int ExternalUsers { get; set; } = 0;

    // Optional add-ons
    public bool IncludeGenAI { get; set; }
    public string? GenAIModelPackSize { get; set; }
    public bool IncludeGenAIKnowledgeBase { get; set; }
    public bool IncludeCustomerEnablement { get; set; }

    // Additional storage (in GB)
    public decimal AdditionalFileStorageGB { get; set; }
    public decimal AdditionalDatabaseStorageGB { get; set; }
}

/// <summary>
/// Calculated Mendix pricing result
/// </summary>
public class MendixPricingResult
{
    public MendixDeploymentCategory Category { get; set; }
    public string DeploymentTypeName { get; set; } = string.Empty;

    // Cost breakdown (per year)
    public decimal PlatformLicenseCost { get; set; }
    public decimal UserLicenseCost { get; set; }
    public decimal DeploymentFeeCost { get; set; }
    public decimal EnvironmentCost { get; set; }
    public decimal StorageCost { get; set; }
    public decimal GenAICost { get; set; }
    public decimal ServicesCost { get; set; }

    // Discount
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }

    // Totals
    public decimal TotalPerYear => PlatformLicenseCost + UserLicenseCost + DeploymentFeeCost +
                                    EnvironmentCost + StorageCost + GenAICost + ServicesCost - DiscountAmount;
    public decimal TotalPerMonth => TotalPerYear / 12;
    public decimal TotalThreeYear => TotalPerYear * 3;

    // Cloud Tokens (if applicable)
    public int TotalCloudTokens { get; set; }

    // Details
    public string? ResourcePackDetails { get; set; }
    public string? EnvironmentDetails { get; set; }
}
