namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// OutSystems pricing settings stored in database
/// Based on OutSystems public pricing model with subscription-based licensing
/// All prices are annual (per year) unless otherwise noted
/// </summary>
public class OutSystemsPricingEntity
{
    public int Id { get; set; }

    // ===========================================
    // OutSystems Standard Subscription Pricing (ODC)
    // ===========================================

    /// <summary>
    /// Standard Edition - base annual subscription
    /// Includes 1 AO pack (150 AOs), 100 internal users, 3 environments
    /// Starting price for small-medium deployments
    /// </summary>
    public decimal StandardEditionBase { get; set; } = 36300m;

    /// <summary>
    /// Number of Application Objects included in Standard Edition (1 pack = 150 AOs)
    /// A typical medium-sized app is about 150 AOs
    /// </summary>
    public int StandardEditionAOsIncluded { get; set; } = 150;

    /// <summary>
    /// Number of internal users included in Standard Edition
    /// </summary>
    public int StandardEditionInternalUsersIncluded { get; set; } = 100;

    // ===========================================
    // OutSystems Enterprise Edition Pricing
    // ===========================================

    /// <summary>
    /// Enterprise Edition - base annual subscription
    /// Includes 3 AO packs (450 AOs), 500 internal users, additional features
    /// </summary>
    public decimal EnterpriseEditionBase { get; set; } = 72600m;

    /// <summary>
    /// Number of Application Objects included in Enterprise Edition (3 packs = 450 AOs)
    /// </summary>
    public int EnterpriseEditionAOsIncluded { get; set; } = 450;

    /// <summary>
    /// Number of internal users included in Enterprise Edition
    /// </summary>
    public int EnterpriseEditionInternalUsersIncluded { get; set; } = 500;

    // ===========================================
    // Additional Application Objects Pricing
    // ===========================================

    /// <summary>
    /// AO pack size (150 AOs per pack)
    /// AOs = screens + database tables + API methods
    /// </summary>
    public int AOPackSize { get; set; } = 150;

    /// <summary>
    /// Price per additional AO pack (150 AOs) per year
    /// </summary>
    public decimal AdditionalAOPackPrice { get; set; } = 18000m;

    // ===========================================
    // Cloud Infrastructure Add-ons
    // ===========================================

    /// <summary>
    /// OutSystems Cloud - additional production environment (per year)
    /// </summary>
    public decimal CloudAdditionalProdEnv { get; set; } = 12000m;

    /// <summary>
    /// OutSystems Cloud - additional non-production environment (per year)
    /// </summary>
    public decimal CloudAdditionalNonProdEnv { get; set; } = 6000m;

    /// <summary>
    /// OutSystems Cloud - high availability add-on (per year)
    /// </summary>
    public decimal CloudHAAddOn { get; set; } = 24000m;

    /// <summary>
    /// OutSystems Cloud - disaster recovery add-on (per year)
    /// </summary>
    public decimal CloudDRAddOn { get; set; } = 18000m;

    // ===========================================
    // Self-Managed (On-Premises/Private Cloud)
    // ===========================================

    /// <summary>
    /// Self-Managed - base platform license (includes LifeTime)
    /// </summary>
    public decimal SelfManagedBase { get; set; } = 48000m;

    /// <summary>
    /// Self-Managed - per additional runtime environment
    /// </summary>
    public decimal SelfManagedPerEnvironment { get; set; } = 9600m;

    /// <summary>
    /// Self-Managed - per front-end server license
    /// </summary>
    public decimal SelfManagedPerFrontEnd { get; set; } = 4800m;

    // ===========================================
    // User Licensing
    // ===========================================

    /// <summary>
    /// Internal user pack size (100 users per pack)
    /// </summary>
    public int InternalUserPackSize { get; set; } = 100;

    /// <summary>
    /// Price per additional internal user pack (100 users) per year
    /// </summary>
    public decimal AdditionalInternalUserPackPrice { get; set; } = 6000m;

    /// <summary>
    /// External user pack size (10,000 sessions per month)
    /// </summary>
    public int ExternalUserPackSize { get; set; } = 10000;

    /// <summary>
    /// External user pack price per year (per 10,000 monthly sessions)
    /// For public-facing applications
    /// </summary>
    public decimal ExternalUserPackPerYear { get; set; } = 12000m;

    // ===========================================
    // Support Tiers
    // ===========================================

    /// <summary>
    /// Standard Support - included in base subscription
    /// </summary>
    public decimal StandardSupportPercent { get; set; } = 0m; // Included

    /// <summary>
    /// Premium Support - percentage of license cost
    /// 24/7 support with faster SLAs
    /// </summary>
    public decimal PremiumSupportPercent { get; set; } = 15m;

    /// <summary>
    /// Elite Support - percentage of license cost
    /// Dedicated support with custom SLAs
    /// </summary>
    public decimal EliteSupportPercent { get; set; } = 25m;

    // ===========================================
    // Professional Services (One-time)
    // ===========================================

    /// <summary>
    /// Implementation services - per day rate
    /// </summary>
    public decimal ProfessionalServicesDayRate { get; set; } = 2400m;

    /// <summary>
    /// Training - per person per day
    /// </summary>
    public decimal TrainingPerPersonPerDay { get; set; } = 600m;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
