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
    public DbSet<OutSystemsPricingEntity> OutSystemsPricing { get; set; } = null!;
    public DbSet<DistributionConfigEntity> DistributionConfigs { get; set; } = null!;

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
        // Based on OutSystems Partner Calculator (January 2026)
        // Supports: ODC, O11 Cloud, O11 Self-Managed
        modelBuilder.Entity<OutSystemsPricingEntity>(entity =>
        {
            entity.ToTable("OutSystemsPricing");
            entity.HasKey(e => e.Id);

            // ODC Platform Pricing
            entity.Property(e => e.OdcPlatformBasePrice).HasPrecision(18, 2);
            entity.Property(e => e.OdcAOPackPrice).HasPrecision(18, 2);
            entity.Property(e => e.OdcInternalUserPackPrice).HasPrecision(18, 2);
            entity.Property(e => e.OdcExternalUserPackPrice).HasPrecision(18, 2);

            // O11 Platform Pricing
            entity.Property(e => e.O11EnterpriseBasePrice).HasPrecision(18, 2);
            entity.Property(e => e.O11AOPackPrice).HasPrecision(18, 2);

            // Unlimited Users (per AO pack - not flat!)
            entity.Property(e => e.UnlimitedUsersPerAOPack).HasPrecision(18, 2);

            // ODC Add-ons (per AO pack)
            entity.Property(e => e.OdcSupport24x7ExtendedPerPack).HasPrecision(18, 2);
            entity.Property(e => e.OdcSupport24x7PremiumPerPack).HasPrecision(18, 2);
            entity.Property(e => e.OdcHighAvailabilityPerPack).HasPrecision(18, 2);
            entity.Property(e => e.OdcNonProdRuntimePerPack).HasPrecision(18, 2);
            entity.Property(e => e.OdcPrivateGatewayPerPack).HasPrecision(18, 2);
            entity.Property(e => e.OdcSentryPerPack).HasPrecision(18, 2);

            // O11 Add-ons (per AO pack)
            entity.Property(e => e.O11Support24x7PremiumPerPack).HasPrecision(18, 2);
            entity.Property(e => e.O11HighAvailabilityPerPack).HasPrecision(18, 2);
            entity.Property(e => e.O11SentryPerPack).HasPrecision(18, 2);
            entity.Property(e => e.O11NonProdEnvPerPack).HasPrecision(18, 2);
            entity.Property(e => e.O11LoadTestEnvPerPack).HasPrecision(18, 2);
            entity.Property(e => e.O11EnvironmentPackPerPack).HasPrecision(18, 2);
            entity.Property(e => e.O11DisasterRecoveryPerPack).HasPrecision(18, 2);

            // O11 Add-ons (flat fee)
            entity.Property(e => e.O11LogStreamingFlat).HasPrecision(18, 2);
            entity.Property(e => e.O11DatabaseReplicaFlat).HasPrecision(18, 2);

            // Tiered Pricing JSON fields
            entity.Property(e => e.O11InternalUserTiersJson).HasMaxLength(2000);
            entity.Property(e => e.O11ExternalUserTiersJson).HasMaxLength(2000);
            entity.Property(e => e.AppShieldTiersJson).HasMaxLength(4000);
            entity.Property(e => e.ServicesPricingByRegionJson).HasMaxLength(4000);

            // Cloud VM Pricing JSON
            entity.Property(e => e.AzureVMPricingJson).HasMaxLength(1000);
            entity.Property(e => e.AwsEC2PricingJson).HasMaxLength(1000);

            // Feature Availability JSON
            entity.Property(e => e.O11CloudOnlyFeaturesJson).HasMaxLength(500);
            entity.Property(e => e.O11SelfManagedOnlyFeaturesJson).HasMaxLength(500);

            // Data source tracking
            entity.Property(e => e.DataSourceVersion).HasMaxLength(100);
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

        // Seed default OutSystems pricing
        modelBuilder.Entity<OutSystemsPricingEntity>().HasData(new OutSystemsPricingEntity
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Seed distribution configurations
        SeedDistributionConfigs(modelBuilder);
    }

    private static void SeedDistributionConfigs(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow;
        var distributions = new List<DistributionConfigEntity>
        {
            // 1. OpenShift On-Prem
            new()
            {
                Id = 1, DistributionKey = "OpenShift", Name = "OpenShift (On-Prem)",
                Vendor = "Red Hat", Icon = "openshift", BrandColor = "#EE0000",
                Tags = "enterprise,on-prem", SortOrder = 1,
                ProdControlPlaneCpu = 8, ProdControlPlaneRam = 32, ProdControlPlaneDisk = 200,
                NonProdControlPlaneCpu = 8, NonProdControlPlaneRam = 32, NonProdControlPlaneDisk = 100,
                ProdWorkerCpu = 16, ProdWorkerRam = 64, ProdWorkerDisk = 200,
                NonProdWorkerCpu = 8, NonProdWorkerRam = 32, NonProdWorkerDisk = 100,
                ProdInfraCpu = 8, ProdInfraRam = 32, ProdInfraDisk = 500,
                NonProdInfraCpu = 8, NonProdInfraRam = 32, NonProdInfraDisk = 200,
                HasInfraNodes = true, HasManagedControlPlane = false,
                CreatedAt = now, UpdatedAt = now
            },
            // 2. OpenShift ROSA (AWS)
            new()
            {
                Id = 2, DistributionKey = "OpenShiftROSA", Name = "OpenShift ROSA (AWS)",
                Vendor = "Red Hat / AWS", Icon = "openshift", BrandColor = "#EE0000",
                Tags = "enterprise,cloud,managed", SortOrder = 2,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 16, ProdWorkerRam = 64, ProdWorkerDisk = 200,
                NonProdWorkerCpu = 8, NonProdWorkerRam = 32, NonProdWorkerDisk = 100,
                ProdInfraCpu = 8, ProdInfraRam = 32, ProdInfraDisk = 500,
                NonProdInfraCpu = 8, NonProdInfraRam = 32, NonProdInfraDisk = 200,
                HasInfraNodes = true, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 3. OpenShift ARO (Azure)
            new()
            {
                Id = 3, DistributionKey = "OpenShiftARO", Name = "OpenShift ARO (Azure)",
                Vendor = "Red Hat / Microsoft", Icon = "openshift", BrandColor = "#EE0000",
                Tags = "enterprise,cloud,managed", SortOrder = 3,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 16, ProdWorkerRam = 64, ProdWorkerDisk = 200,
                NonProdWorkerCpu = 8, NonProdWorkerRam = 32, NonProdWorkerDisk = 100,
                ProdInfraCpu = 8, ProdInfraRam = 32, ProdInfraDisk = 500,
                NonProdInfraCpu = 8, NonProdInfraRam = 32, NonProdInfraDisk = 200,
                HasInfraNodes = true, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 4. Vanilla Kubernetes
            new()
            {
                Id = 4, DistributionKey = "Kubernetes", Name = "Vanilla Kubernetes",
                Vendor = "CNCF", Icon = "k8s", BrandColor = "#326CE5",
                Tags = "on-prem,open-source", SortOrder = 4,
                ProdControlPlaneCpu = 4, ProdControlPlaneRam = 16, ProdControlPlaneDisk = 100,
                NonProdControlPlaneCpu = 2, NonProdControlPlaneRam = 8, NonProdControlPlaneDisk = 50,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = false,
                CreatedAt = now, UpdatedAt = now
            },
            // 5. Rancher On-Prem
            new()
            {
                Id = 5, DistributionKey = "Rancher", Name = "Rancher (On-Prem)",
                Vendor = "SUSE", Icon = "rancher", BrandColor = "#0075A8",
                Tags = "on-prem,enterprise", SortOrder = 5,
                ProdControlPlaneCpu = 4, ProdControlPlaneRam = 16, ProdControlPlaneDisk = 100,
                NonProdControlPlaneCpu = 2, NonProdControlPlaneRam = 8, NonProdControlPlaneDisk = 50,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = false,
                CreatedAt = now, UpdatedAt = now
            },
            // 6. Rancher Hosted
            new()
            {
                Id = 6, DistributionKey = "RancherHosted", Name = "Rancher Hosted (Cloud)",
                Vendor = "SUSE", Icon = "rancher", BrandColor = "#0075A8",
                Tags = "cloud,managed,enterprise", SortOrder = 6,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 7. K3s
            new()
            {
                Id = 7, DistributionKey = "K3s", Name = "K3s",
                Vendor = "SUSE", Icon = "k3s", BrandColor = "#FFC61C",
                Tags = "on-prem,lightweight,edge", SortOrder = 7,
                ProdControlPlaneCpu = 2, ProdControlPlaneRam = 4, ProdControlPlaneDisk = 50,
                NonProdControlPlaneCpu = 1, NonProdControlPlaneRam = 2, NonProdControlPlaneDisk = 25,
                ProdWorkerCpu = 4, ProdWorkerRam = 8, ProdWorkerDisk = 50,
                NonProdWorkerCpu = 2, NonProdWorkerRam = 4, NonProdWorkerDisk = 25,
                HasInfraNodes = false, HasManagedControlPlane = false,
                CreatedAt = now, UpdatedAt = now
            },
            // 8. MicroK8s
            new()
            {
                Id = 8, DistributionKey = "MicroK8s", Name = "MicroK8s",
                Vendor = "Canonical", Icon = "microk8s", BrandColor = "#E95420",
                Tags = "on-prem,lightweight", SortOrder = 8,
                ProdControlPlaneCpu = 2, ProdControlPlaneRam = 4, ProdControlPlaneDisk = 50,
                NonProdControlPlaneCpu = 1, NonProdControlPlaneRam = 2, NonProdControlPlaneDisk = 25,
                ProdWorkerCpu = 4, ProdWorkerRam = 8, ProdWorkerDisk = 50,
                NonProdWorkerCpu = 2, NonProdWorkerRam = 4, NonProdWorkerDisk = 25,
                HasInfraNodes = false, HasManagedControlPlane = false,
                CreatedAt = now, UpdatedAt = now
            },
            // 9. Charmed Kubernetes
            new()
            {
                Id = 9, DistributionKey = "Charmed", Name = "Charmed Kubernetes",
                Vendor = "Canonical", Icon = "charmed", BrandColor = "#772953",
                Tags = "on-prem,enterprise", SortOrder = 9,
                ProdControlPlaneCpu = 4, ProdControlPlaneRam = 16, ProdControlPlaneDisk = 100,
                NonProdControlPlaneCpu = 2, NonProdControlPlaneRam = 8, NonProdControlPlaneDisk = 50,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = false,
                CreatedAt = now, UpdatedAt = now
            },
            // 10. VMware Tanzu On-Prem
            new()
            {
                Id = 10, DistributionKey = "Tanzu", Name = "VMware Tanzu (On-Prem)",
                Vendor = "Broadcom", Icon = "tanzu", BrandColor = "#1D428A",
                Tags = "on-prem,enterprise", SortOrder = 10,
                ProdControlPlaneCpu = 4, ProdControlPlaneRam = 16, ProdControlPlaneDisk = 100,
                NonProdControlPlaneCpu = 2, NonProdControlPlaneRam = 8, NonProdControlPlaneDisk = 50,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = false,
                CreatedAt = now, UpdatedAt = now
            },
            // 11. VMware Tanzu Cloud
            new()
            {
                Id = 11, DistributionKey = "TanzuCloud", Name = "VMware Tanzu Cloud",
                Vendor = "Broadcom", Icon = "tanzu", BrandColor = "#1D428A",
                Tags = "cloud,managed,enterprise", SortOrder = 11,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 12. Amazon EKS
            new()
            {
                Id = 12, DistributionKey = "EKS", Name = "Amazon EKS",
                Vendor = "AWS", Icon = "eks", BrandColor = "#FF9900",
                Tags = "cloud,managed", SortOrder = 12,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 13. Azure AKS
            new()
            {
                Id = 13, DistributionKey = "AKS", Name = "Azure AKS",
                Vendor = "Microsoft", Icon = "aks", BrandColor = "#0078D4",
                Tags = "cloud,managed", SortOrder = 13,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 14. Google GKE
            new()
            {
                Id = 14, DistributionKey = "GKE", Name = "Google GKE",
                Vendor = "Google", Icon = "gke", BrandColor = "#4285F4",
                Tags = "cloud,managed", SortOrder = 14,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 15. Oracle OKE
            new()
            {
                Id = 15, DistributionKey = "OKE", Name = "Oracle OKE",
                Vendor = "Oracle", Icon = "oke", BrandColor = "#C74634",
                Tags = "cloud,managed", SortOrder = 15,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 16. OpenShift Dedicated (GCP)
            new()
            {
                Id = 16, DistributionKey = "OpenShiftDedicated", Name = "OpenShift Dedicated (GCP)",
                Vendor = "Red Hat / Google", Icon = "openshift", BrandColor = "#EE0000",
                Tags = "enterprise,cloud,managed", SortOrder = 16,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 16, ProdWorkerRam = 64, ProdWorkerDisk = 200,
                NonProdWorkerCpu = 8, NonProdWorkerRam = 32, NonProdWorkerDisk = 100,
                ProdInfraCpu = 8, ProdInfraRam = 32, ProdInfraDisk = 500,
                NonProdInfraCpu = 8, NonProdInfraRam = 32, NonProdInfraDisk = 200,
                HasInfraNodes = true, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 17. OpenShift on IBM Cloud
            new()
            {
                Id = 17, DistributionKey = "OpenShiftIBM", Name = "OpenShift on IBM Cloud",
                Vendor = "Red Hat / IBM", Icon = "openshift", BrandColor = "#EE0000",
                Tags = "enterprise,cloud,managed", SortOrder = 17,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 16, ProdWorkerRam = 64, ProdWorkerDisk = 200,
                NonProdWorkerCpu = 8, NonProdWorkerRam = 32, NonProdWorkerDisk = 100,
                ProdInfraCpu = 8, ProdInfraRam = 32, ProdInfraDisk = 500,
                NonProdInfraCpu = 8, NonProdInfraRam = 32, NonProdInfraDisk = 200,
                HasInfraNodes = true, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 18. Rancher on EKS
            new()
            {
                Id = 18, DistributionKey = "RancherEKS", Name = "Rancher on EKS",
                Vendor = "SUSE / AWS", Icon = "rancher", BrandColor = "#0075A8",
                Tags = "cloud,managed,enterprise", SortOrder = 18,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 19. Rancher on AKS
            new()
            {
                Id = 19, DistributionKey = "RancherAKS", Name = "Rancher on AKS",
                Vendor = "SUSE / Microsoft", Icon = "rancher", BrandColor = "#0075A8",
                Tags = "cloud,managed,enterprise", SortOrder = 19,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 20. Rancher on GKE
            new()
            {
                Id = 20, DistributionKey = "RancherGKE", Name = "Rancher on GKE",
                Vendor = "SUSE / Google", Icon = "rancher", BrandColor = "#0075A8",
                Tags = "cloud,managed,enterprise", SortOrder = 20,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 21. VMware Tanzu on AWS
            new()
            {
                Id = 21, DistributionKey = "TanzuAWS", Name = "VMware Tanzu on AWS",
                Vendor = "Broadcom / AWS", Icon = "tanzu", BrandColor = "#1D428A",
                Tags = "cloud,managed,enterprise", SortOrder = 21,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 22. VMware Tanzu on Azure
            new()
            {
                Id = 22, DistributionKey = "TanzuAzure", Name = "VMware Tanzu on Azure",
                Vendor = "Broadcom / Microsoft", Icon = "tanzu", BrandColor = "#1D428A",
                Tags = "cloud,managed,enterprise", SortOrder = 22,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 23. VMware Tanzu on GCP
            new()
            {
                Id = 23, DistributionKey = "TanzuGCP", Name = "VMware Tanzu on GCP",
                Vendor = "Broadcom / Google", Icon = "tanzu", BrandColor = "#1D428A",
                Tags = "cloud,managed,enterprise", SortOrder = 23,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 24. RKE2
            new()
            {
                Id = 24, DistributionKey = "RKE2", Name = "RKE2",
                Vendor = "SUSE", Icon = "rancher", BrandColor = "#0075A8",
                Tags = "on-prem,enterprise,security", SortOrder = 24,
                ProdControlPlaneCpu = 4, ProdControlPlaneRam = 16, ProdControlPlaneDisk = 100,
                NonProdControlPlaneCpu = 2, NonProdControlPlaneRam = 8, NonProdControlPlaneDisk = 50,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = false,
                CreatedAt = now, UpdatedAt = now
            },
            // 25. IBM Kubernetes Service
            new()
            {
                Id = 25, DistributionKey = "IKS", Name = "IBM Kubernetes Service",
                Vendor = "IBM", Icon = "iks", BrandColor = "#054ADA",
                Tags = "cloud,managed", SortOrder = 25,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 26. Alibaba ACK
            new()
            {
                Id = 26, DistributionKey = "ACK", Name = "Alibaba ACK",
                Vendor = "Alibaba", Icon = "ack", BrandColor = "#FF6A00",
                Tags = "cloud,managed", SortOrder = 26,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 27. Tencent TKE
            new()
            {
                Id = 27, DistributionKey = "TKE", Name = "Tencent TKE",
                Vendor = "Tencent", Icon = "tke", BrandColor = "#00A4FF",
                Tags = "cloud,managed", SortOrder = 27,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 28. Huawei CCE
            new()
            {
                Id = 28, DistributionKey = "CCE", Name = "Huawei CCE",
                Vendor = "Huawei", Icon = "cce", BrandColor = "#CF0A2C",
                Tags = "cloud,managed", SortOrder = 28,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 8, ProdWorkerRam = 32, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 4, NonProdWorkerRam = 16, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 29. DigitalOcean Kubernetes
            new()
            {
                Id = 29, DistributionKey = "DOKS", Name = "DigitalOcean Kubernetes",
                Vendor = "DigitalOcean", Icon = "doks", BrandColor = "#0080FF",
                Tags = "cloud,managed,developer", SortOrder = 29,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 4, ProdWorkerRam = 16, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 2, NonProdWorkerRam = 8, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 30. Linode/Akamai LKE
            new()
            {
                Id = 30, DistributionKey = "LKE", Name = "Linode/Akamai LKE",
                Vendor = "Akamai", Icon = "lke", BrandColor = "#00A95C",
                Tags = "cloud,managed,developer", SortOrder = 30,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 4, ProdWorkerRam = 16, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 2, NonProdWorkerRam = 8, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 31. Vultr VKE
            new()
            {
                Id = 31, DistributionKey = "VKE", Name = "Vultr VKE",
                Vendor = "Vultr", Icon = "vke", BrandColor = "#007BFC",
                Tags = "cloud,managed,developer", SortOrder = 31,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 4, ProdWorkerRam = 16, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 2, NonProdWorkerRam = 8, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 32. Hetzner Kubernetes
            new()
            {
                Id = 32, DistributionKey = "HetznerK8s", Name = "Hetzner Kubernetes",
                Vendor = "Hetzner", Icon = "hetzner", BrandColor = "#D50C2D",
                Tags = "cloud,developer,cost-effective", SortOrder = 32,
                ProdControlPlaneCpu = 2, ProdControlPlaneRam = 4, ProdControlPlaneDisk = 50,
                NonProdControlPlaneCpu = 1, NonProdControlPlaneRam = 2, NonProdControlPlaneDisk = 25,
                ProdWorkerCpu = 4, ProdWorkerRam = 16, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 2, NonProdWorkerRam = 8, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = false,
                CreatedAt = now, UpdatedAt = now
            },
            // 33. OVHcloud Kubernetes
            new()
            {
                Id = 33, DistributionKey = "OVHKubernetes", Name = "OVHcloud Kubernetes",
                Vendor = "OVH", Icon = "ovh", BrandColor = "#000E9C",
                Tags = "cloud,managed,developer", SortOrder = 33,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 4, ProdWorkerRam = 16, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 2, NonProdWorkerRam = 8, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            },
            // 34. Scaleway Kapsule
            new()
            {
                Id = 34, DistributionKey = "ScalewayKapsule", Name = "Scaleway Kapsule",
                Vendor = "Scaleway", Icon = "scaleway", BrandColor = "#4F0599",
                Tags = "cloud,managed,developer", SortOrder = 34,
                ProdControlPlaneCpu = 0, ProdControlPlaneRam = 0, ProdControlPlaneDisk = 0,
                NonProdControlPlaneCpu = 0, NonProdControlPlaneRam = 0, NonProdControlPlaneDisk = 0,
                ProdWorkerCpu = 4, ProdWorkerRam = 16, ProdWorkerDisk = 100,
                NonProdWorkerCpu = 2, NonProdWorkerRam = 8, NonProdWorkerDisk = 50,
                HasInfraNodes = false, HasManagedControlPlane = true,
                CreatedAt = now, UpdatedAt = now
            }
        };

        modelBuilder.Entity<DistributionConfigEntity>().HasData(distributions);
    }
}
