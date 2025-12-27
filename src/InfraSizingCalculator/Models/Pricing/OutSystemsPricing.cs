namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// OutSystems edition types
/// </summary>
public enum OutSystemsEdition
{
    Standard,       // Starting tier for small-medium deployments
    Enterprise      // Full-featured for large enterprise deployments
}

/// <summary>
/// OutSystems deployment types
/// </summary>
public enum OutSystemsDeploymentType
{
    Cloud,          // OutSystems Cloud (managed PaaS)
    SelfManaged     // Self-managed on-premises or private cloud
}

/// <summary>
/// OutSystems user license types
/// </summary>
public enum OutSystemsUserLicenseType
{
    Named,          // Named users - each user has dedicated license
    Concurrent,     // Concurrent users - shared floating licenses
    External        // External/Anonymous - session-based for public apps
}

/// <summary>
/// OutSystems support tier
/// </summary>
public enum OutSystemsSupportTier
{
    Standard,       // Included in subscription
    Premium,        // 24/7 with faster SLAs
    Elite           // Dedicated support with custom SLAs
}

/// <summary>
/// Complete OutSystems pricing configuration
/// Based on OutSystems ODC (Developer Cloud) pricing model
/// </summary>
public class OutSystemsPricingSettings
{
    // ==================== EDITION PRICING ====================

    /// <summary>
    /// Standard Edition base subscription (annual)
    /// Includes 1 AO pack (150 AOs), 100 internal users, 3 environments
    /// </summary>
    public decimal StandardEditionBasePrice { get; set; } = 36300m;
    public int StandardEditionAOsIncluded { get; set; } = 150;
    public int StandardEditionInternalUsersIncluded { get; set; } = 100;

    /// <summary>
    /// Enterprise Edition base subscription (annual)
    /// Includes 3 AO packs (450 AOs), 500 internal users, additional features
    /// </summary>
    public decimal EnterpriseEditionBasePrice { get; set; } = 72600m;
    public int EnterpriseEditionAOsIncluded { get; set; } = 450;
    public int EnterpriseEditionInternalUsersIncluded { get; set; } = 500;

    // ==================== APPLICATION OBJECTS ====================

    /// <summary>
    /// AO pack size (150 AOs per pack - typical medium app)
    /// AOs = screens + database tables + API methods
    /// </summary>
    public int AOPackSize { get; set; } = 150;

    /// <summary>
    /// Price per additional AO pack (150 AOs) per year
    /// </summary>
    public decimal AdditionalAOPackPrice { get; set; } = 18000m;

    // ==================== USER LICENSING ====================

    /// <summary>
    /// Internal user pack size (100 users per pack)
    /// </summary>
    public int InternalUserPackSize { get; set; } = 100;

    /// <summary>
    /// Price per additional internal user pack (100 users) per year
    /// </summary>
    public decimal AdditionalInternalUserPackPrice { get; set; } = 6000m;

    /// <summary>
    /// External user pack size (sessions per month)
    /// </summary>
    public int ExternalUserPackSize { get; set; } = 10000;

    /// <summary>
    /// External user pack price per year
    /// </summary>
    public decimal ExternalUserPackPricePerYear { get; set; } = 12000m;

    // ==================== CLOUD ADD-ONS ====================

    public decimal CloudAdditionalProdEnvPrice { get; set; } = 12000m;
    public decimal CloudAdditionalNonProdEnvPrice { get; set; } = 6000m;
    public decimal CloudHAAddOnPrice { get; set; } = 24000m;
    public decimal CloudDRAddOnPrice { get; set; } = 18000m;

    // ==================== SELF-MANAGED PRICING ====================

    public decimal SelfManagedBasePrice { get; set; } = 48000m;
    public decimal SelfManagedPerEnvironmentPrice { get; set; } = 9600m;
    public decimal SelfManagedPerFrontEndPrice { get; set; } = 4800m;

    // ==================== SUPPORT ====================

    public decimal PremiumSupportPercent { get; set; } = 15m;
    public decimal EliteSupportPercent { get; set; } = 25m;

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Get edition base price
    /// </summary>
    public decimal GetEditionBasePrice(OutSystemsEdition edition) => edition switch
    {
        OutSystemsEdition.Standard => StandardEditionBasePrice,
        OutSystemsEdition.Enterprise => EnterpriseEditionBasePrice,
        _ => StandardEditionBasePrice
    };

    /// <summary>
    /// Get included AOs for edition
    /// </summary>
    public int GetIncludedAOs(OutSystemsEdition edition) => edition switch
    {
        OutSystemsEdition.Standard => StandardEditionAOsIncluded,
        OutSystemsEdition.Enterprise => EnterpriseEditionAOsIncluded,
        _ => StandardEditionAOsIncluded
    };

    /// <summary>
    /// Get included internal users for edition
    /// </summary>
    public int GetIncludedInternalUsers(OutSystemsEdition edition) => edition switch
    {
        OutSystemsEdition.Standard => StandardEditionInternalUsersIncluded,
        OutSystemsEdition.Enterprise => EnterpriseEditionInternalUsersIncluded,
        _ => StandardEditionInternalUsersIncluded
    };

    /// <summary>
    /// Calculate cost for additional AOs beyond what's included in edition
    /// </summary>
    public decimal CalculateAdditionalAOsCost(OutSystemsEdition edition, int totalAOs)
    {
        var included = GetIncludedAOs(edition);
        if (totalAOs <= included) return 0;

        var additional = totalAOs - included;
        var packs = (int)Math.Ceiling(additional / (double)AOPackSize);
        return packs * AdditionalAOPackPrice;
    }

    /// <summary>
    /// Calculate cost for additional internal users beyond what's included
    /// </summary>
    public decimal CalculateAdditionalInternalUsersCost(OutSystemsEdition edition, int totalUsers)
    {
        var included = GetIncludedInternalUsers(edition);
        if (totalUsers <= included) return 0;

        var additional = totalUsers - included;
        var packs = (int)Math.Ceiling(additional / (double)InternalUserPackSize);
        return packs * AdditionalInternalUserPackPrice;
    }

    /// <summary>
    /// Calculate external user pack cost
    /// </summary>
    public decimal CalculateExternalUsersCost(int monthlySessions)
    {
        if (monthlySessions <= 0) return 0;
        var packs = (int)Math.Ceiling(monthlySessions / (double)ExternalUserPackSize);
        return packs * ExternalUserPackPricePerYear;
    }

    /// <summary>
    /// Calculate support cost as percentage of license cost
    /// </summary>
    public decimal CalculateSupportCost(OutSystemsSupportTier tier, decimal licenseCost) => tier switch
    {
        OutSystemsSupportTier.Standard => 0, // Included
        OutSystemsSupportTier.Premium => licenseCost * (PremiumSupportPercent / 100),
        OutSystemsSupportTier.Elite => licenseCost * (EliteSupportPercent / 100),
        _ => 0
    };
}

/// <summary>
/// OutSystems deployment configuration selected by user
/// </summary>
public class OutSystemsDeploymentConfig
{
    public OutSystemsEdition Edition { get; set; } = OutSystemsEdition.Standard;
    public OutSystemsDeploymentType DeploymentType { get; set; } = OutSystemsDeploymentType.SelfManaged;

    // Application Objects (complexity measure)
    public int TotalApplicationObjects { get; set; } = 20;

    // Environment counts
    public int ProductionEnvironments { get; set; } = 1;
    public int NonProductionEnvironments { get; set; } = 3; // Dev, Test, Stage

    // Self-managed specifics
    public int FrontEndServers { get; set; } = 2;

    // High Availability / DR
    public bool IncludeHA { get; set; } = false;
    public bool IncludeDR { get; set; } = false;

    // User licensing
    public OutSystemsUserLicenseType UserLicenseType { get; set; } = OutSystemsUserLicenseType.Named;
    public int NamedUsers { get; set; } = 100;
    public int ConcurrentUsers { get; set; } = 0;
    public int ExternalSessions { get; set; } = 0; // Monthly session count

    // Support
    public OutSystemsSupportTier SupportTier { get; set; } = OutSystemsSupportTier.Standard;
}

/// <summary>
/// Calculated OutSystems pricing result
/// </summary>
public class OutSystemsPricingResult
{
    public OutSystemsEdition Edition { get; set; }
    public OutSystemsDeploymentType DeploymentType { get; set; }
    public string DeploymentTypeName { get; set; } = string.Empty;

    // Cost breakdown (per year)
    public decimal EditionBaseCost { get; set; }
    public decimal AdditionalAOsCost { get; set; }
    public decimal EnvironmentCost { get; set; }
    public decimal FrontEndCost { get; set; }
    public decimal HACost { get; set; }
    public decimal DRCost { get; set; }
    public decimal UserLicenseCost { get; set; }
    public decimal SupportCost { get; set; }

    // Totals
    public decimal TotalPerYear => EditionBaseCost + AdditionalAOsCost + EnvironmentCost +
                                    FrontEndCost + HACost + DRCost + UserLicenseCost + SupportCost;
    public decimal TotalPerMonth => TotalPerYear / 12;
    public decimal TotalThreeYear => TotalPerYear * 3;

    // Details
    public int TotalAOs { get; set; }
    public int IncludedAOs { get; set; }
    public int AdditionalAOs { get; set; }
    public string? EnvironmentDetails { get; set; }
    public string? UserLicenseDetails { get; set; }
}
