using Microsoft.EntityFrameworkCore;
using InfraSizingCalculator.Data.Entities;

namespace InfraSizingCalculator.Data;

/// <summary>
/// Database context for the Infrastructure Sizing Calculator.
/// Stores all settings, pricing defaults, and API credentials.
///
/// IMPORTANT: Seed data is managed by SeedDataService using JSON files.
/// Do NOT add HasData() calls here - use Data/Seeds/*.json instead.
/// </summary>
public class InfraSizingDbContext : DbContext
{
    public InfraSizingDbContext(DbContextOptions<InfraSizingDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationSettingsEntity> ApplicationSettings { get; set; } = null!;
    public DbSet<CloudApiCredentialsEntity> CloudApiCredentials { get; set; } = null!;
    public DbSet<OnPremPricingEntity> OnPremPricing { get; set; } = null!;
    public DbSet<MendixPricingEntity> MendixPricing { get; set; } = null!;
    public DbSet<OutSystemsPricingEntity> OutSystemsPricing { get; set; } = null!;
    public DbSet<DistributionConfigEntity> DistributionConfigs { get; set; } = null!;
    public DbSet<TechnologyConfigEntity> TechnologyConfigs { get; set; } = null!;
    public DbSet<InfoTypeContentEntity> InfoTypeContents { get; set; } = null!;
    public DbSet<SeedMetadataEntity> SeedMetadata { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationSettings - single row table
        modelBuilder.Entity<ApplicationSettingsEntity>(entity =>
        {
            entity.ToTable("ApplicationSettings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DefaultCurrency).HasMaxLength(10);
        });

        // CloudApiCredentials - one row per provider
        modelBuilder.Entity<CloudApiCredentialsEntity>(entity =>
        {
            entity.ToTable("CloudApiCredentials");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProviderName).IsUnique();
            entity.Property(e => e.ProviderName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.SecretKey).HasMaxLength(500);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.TenantId).HasMaxLength(100);
            entity.Property(e => e.SubscriptionId).HasMaxLength(100);
            entity.Property(e => e.ProjectId).HasMaxLength(100);
            entity.Property(e => e.ValidationStatus).HasMaxLength(200);
        });

        // OnPremPricing - single row table with defaults
        modelBuilder.Entity<OnPremPricingEntity>(entity =>
        {
            entity.ToTable("OnPremPricing");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServerCostPerUnit).HasPrecision(18, 2);
            entity.Property(e => e.CostPerCore).HasPrecision(18, 2);
            entity.Property(e => e.CostPerGBRam).HasPrecision(18, 2);
            entity.Property(e => e.StorageCostPerTB).HasPrecision(18, 2);
            entity.Property(e => e.RackUnitCostPerMonth).HasPrecision(18, 2);
            entity.Property(e => e.PowerCostPerKwhMonth).HasPrecision(18, 4);
            entity.Property(e => e.CoolingPUE).HasPrecision(5, 2);
            entity.Property(e => e.NetworkCostPerMonth).HasPrecision(18, 2);
            entity.Property(e => e.DevOpsSalaryPerYear).HasPrecision(18, 2);
            entity.Property(e => e.SysAdminSalaryPerYear).HasPrecision(18, 2);
            entity.Property(e => e.OpenShiftPerNodeYear).HasPrecision(18, 2);
            entity.Property(e => e.TanzuPerCoreYear).HasPrecision(18, 2);
            entity.Property(e => e.RancherEnterprisePerNodeYear).HasPrecision(18, 2);
            entity.Property(e => e.CharmedK8sPerNodeYear).HasPrecision(18, 2);
            entity.Property(e => e.RKE2PerNodeYear).HasPrecision(18, 2);
            entity.Property(e => e.K3sPerNodeYear).HasPrecision(18, 2);
        });

        // MendixPricing - single row table with all Mendix pricing from June 2025 Pricebook
        modelBuilder.Entity<MendixPricingEntity>(entity =>
        {
            entity.ToTable("MendixPricing");
            entity.HasKey(e => e.Id);

            // Cloud Token
            entity.Property(e => e.CloudTokenPrice).HasPrecision(18, 2);
            entity.Property(e => e.CloudDedicatedPrice).HasPrecision(18, 2);

            // Additional Storage
            entity.Property(e => e.AdditionalFileStoragePer100GB).HasPrecision(18, 2);
            entity.Property(e => e.AdditionalDatabaseStoragePer100GB).HasPrecision(18, 2);

            // Azure Pricing
            entity.Property(e => e.AzureBasePackagePrice).HasPrecision(18, 2);
            entity.Property(e => e.AzureAdditionalEnvironmentPrice).HasPrecision(18, 2);

            // K8s Pricing
            entity.Property(e => e.K8sBasePackagePrice).HasPrecision(18, 2);
            entity.Property(e => e.K8sEnvTier1Price).HasPrecision(18, 2);
            entity.Property(e => e.K8sEnvTier2Price).HasPrecision(18, 2);
            entity.Property(e => e.K8sEnvTier3Price).HasPrecision(18, 2);
            entity.Property(e => e.K8sEnvTier4Price).HasPrecision(18, 2);

            // Server/StackIT/SAP Pricing
            entity.Property(e => e.ServerPerAppPrice).HasPrecision(18, 2);
            entity.Property(e => e.ServerUnlimitedAppsPrice).HasPrecision(18, 2);
            entity.Property(e => e.StackITPerAppPrice).HasPrecision(18, 2);
            entity.Property(e => e.StackITUnlimitedAppsPrice).HasPrecision(18, 2);
            entity.Property(e => e.SapBtpPerAppPrice).HasPrecision(18, 2);
            entity.Property(e => e.SapBtpUnlimitedAppsPrice).HasPrecision(18, 2);

            // GenAI Pricing
            entity.Property(e => e.GenAIModelPackSPrice).HasPrecision(18, 2);
            entity.Property(e => e.GenAIModelPackMPrice).HasPrecision(18, 2);
            entity.Property(e => e.GenAIModelPackLPrice).HasPrecision(18, 2);
            entity.Property(e => e.GenAIKnowledgeBasePrice).HasPrecision(18, 2);
            entity.Property(e => e.GenAIKnowledgeBaseDiskGB).HasPrecision(18, 2);
        });

        // OutSystemsPricing - single row table with OutSystems subscription pricing
        modelBuilder.Entity<OutSystemsPricingEntity>(entity =>
        {
            entity.ToTable("OutSystemsPricing");
            entity.HasKey(e => e.Id);

            // Edition Pricing
            entity.Property(e => e.StandardEditionBase).HasPrecision(18, 2);
            entity.Property(e => e.EnterpriseEditionBase).HasPrecision(18, 2);
            entity.Property(e => e.AdditionalAOPackPrice).HasPrecision(18, 2);

            // Cloud Add-ons
            entity.Property(e => e.CloudAdditionalProdEnv).HasPrecision(18, 2);
            entity.Property(e => e.CloudAdditionalNonProdEnv).HasPrecision(18, 2);
            entity.Property(e => e.CloudHAAddOn).HasPrecision(18, 2);
            entity.Property(e => e.CloudDRAddOn).HasPrecision(18, 2);

            // Self-Managed
            entity.Property(e => e.SelfManagedBase).HasPrecision(18, 2);
            entity.Property(e => e.SelfManagedPerEnvironment).HasPrecision(18, 2);
            entity.Property(e => e.SelfManagedPerFrontEnd).HasPrecision(18, 2);

            // User Licensing
            entity.Property(e => e.AdditionalInternalUserPackPrice).HasPrecision(18, 2);
            entity.Property(e => e.ExternalUserPackPerYear).HasPrecision(18, 2);

            // Support
            entity.Property(e => e.StandardSupportPercent).HasPrecision(5, 2);
            entity.Property(e => e.PremiumSupportPercent).HasPrecision(5, 2);
            entity.Property(e => e.EliteSupportPercent).HasPrecision(5, 2);

            // Professional Services
            entity.Property(e => e.ProfessionalServicesDayRate).HasPrecision(18, 2);
            entity.Property(e => e.TrainingPerPersonPerDay).HasPrecision(18, 2);
        });

        // DistributionConfigs - one row per Kubernetes distribution
        modelBuilder.Entity<DistributionConfigEntity>(entity =>
        {
            entity.ToTable("DistributionConfigs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DistributionKey).IsUnique();
            entity.Property(e => e.DistributionKey).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Vendor).HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.BrandColor).HasMaxLength(20);
            entity.Property(e => e.Tags).HasMaxLength(200);
            // Info content fields
            entity.Property(e => e.ShortDescription).HasMaxLength(500);
            entity.Property(e => e.DetailedInfoHtml).HasMaxLength(10000);
            entity.Property(e => e.KeyFeaturesJson).HasMaxLength(2000);
            entity.Property(e => e.PricingNotes).HasMaxLength(1000);
            entity.Property(e => e.AdditionalNotes).HasMaxLength(1000);
        });

        // TechnologyConfigs - one row per technology
        modelBuilder.Entity<TechnologyConfigEntity>(entity =>
        {
            entity.ToTable("TechnologyConfigs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TechnologyKey).IsUnique();
            entity.Property(e => e.TechnologyKey).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Vendor).HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.BrandColor).HasMaxLength(20);
            entity.Property(e => e.Category).HasMaxLength(50);
            // Info content fields
            entity.Property(e => e.ShortDescription).HasMaxLength(500);
            entity.Property(e => e.DetailedInfoHtml).HasMaxLength(10000);
            entity.Property(e => e.KeyFeaturesJson).HasMaxLength(2000);
            entity.Property(e => e.PerformanceNotes).HasMaxLength(1000);
            entity.Property(e => e.UseCases).HasMaxLength(1000);
        });

        // InfoTypeContents - configurable info modal content
        modelBuilder.Entity<InfoTypeContentEntity>(entity =>
        {
            entity.ToTable("InfoTypeContents");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InfoTypeKey).IsUnique();
            entity.Property(e => e.InfoTypeKey).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ContentHtml).HasMaxLength(10000).IsRequired();
        });

        // SeedMetadata - tracks seed data version for efficient startup checks
        modelBuilder.Entity<SeedMetadataEntity>(entity =>
        {
            entity.ToTable("SeedMetadata");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SeedHash).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Version).HasMaxLength(20).IsRequired();
        });

        // NOTE: Seed data is now managed by SeedDataService using JSON files.
        // See: src/InfraSizingCalculator/Data/Seeds/*.json
    }
}
