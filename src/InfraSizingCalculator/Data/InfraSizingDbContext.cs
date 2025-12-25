using Microsoft.EntityFrameworkCore;
using InfraSizingCalculator.Data.Entities;

namespace InfraSizingCalculator.Data;

/// <summary>
/// Database context for the Infrastructure Sizing Calculator
/// Stores all settings, pricing defaults, and API credentials
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

        // Seed default data
        SeedDefaultData(modelBuilder);
    }

    private static void SeedDefaultData(ModelBuilder modelBuilder)
    {
        // Seed default application settings
        modelBuilder.Entity<ApplicationSettingsEntity>().HasData(new ApplicationSettingsEntity
        {
            Id = 1,
            IncludePricingInResults = false,
            DefaultCurrency = "USD",
            PricingCacheDurationHours = 24,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Seed default cloud API credentials (empty but ready to configure)
        modelBuilder.Entity<CloudApiCredentialsEntity>().HasData(
            new CloudApiCredentialsEntity { Id = 1, ProviderName = "AWS", IsConfigured = false },
            new CloudApiCredentialsEntity { Id = 2, ProviderName = "Azure", IsConfigured = false },
            new CloudApiCredentialsEntity { Id = 3, ProviderName = "GCP", IsConfigured = false },
            new CloudApiCredentialsEntity { Id = 4, ProviderName = "Oracle", IsConfigured = false }
        );

        // Seed default on-prem pricing
        modelBuilder.Entity<OnPremPricingEntity>().HasData(new OnPremPricingEntity
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Seed default Mendix pricing (from June 2025 Pricebook)
        modelBuilder.Entity<MendixPricingEntity>().HasData(new MendixPricingEntity
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }
}
