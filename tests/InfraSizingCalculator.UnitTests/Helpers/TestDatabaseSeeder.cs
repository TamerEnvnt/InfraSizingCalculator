using InfraSizingCalculator.Data;
using InfraSizingCalculator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraSizingCalculator.UnitTests.Helpers;

/// <summary>
/// Provides database seeding for unit tests using in-memory databases.
/// Mirrors the production SeedDataService but works without file system access.
/// </summary>
public static class TestDatabaseSeeder
{
    /// <summary>
    /// Seeds all required entities for testing.
    /// Call this after EnsureCreated() in test setup.
    /// </summary>
    public static async Task SeedAsync(InfraSizingDbContext dbContext)
    {
        await SeedApplicationSettingsAsync(dbContext);
        await SeedCloudApiCredentialsAsync(dbContext);
        await SeedOnPremPricingAsync(dbContext);
        await SeedMendixPricingAsync(dbContext);
        await SeedOutSystemsPricingAsync(dbContext);
        await SeedDistributionsAsync(dbContext);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Synchronous version for simpler test setup.
    /// </summary>
    public static void Seed(InfraSizingDbContext dbContext)
    {
        SeedAsync(dbContext).GetAwaiter().GetResult();
    }

    private static async Task SeedApplicationSettingsAsync(InfraSizingDbContext dbContext)
    {
        if (await dbContext.ApplicationSettings.AnyAsync())
            return;

        dbContext.ApplicationSettings.Add(new ApplicationSettingsEntity
        {
            Id = 1,
            IncludePricingInResults = false,
            DefaultCurrency = "USD",
            PricingCacheDurationHours = 24,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private static async Task SeedCloudApiCredentialsAsync(InfraSizingDbContext dbContext)
    {
        if (await dbContext.CloudApiCredentials.AnyAsync())
            return;

        var providers = new[] { "AWS", "Azure", "GCP", "Oracle" };
        var id = 1;
        foreach (var provider in providers)
        {
            dbContext.CloudApiCredentials.Add(new CloudApiCredentialsEntity
            {
                Id = id++,
                ProviderName = provider,
                IsConfigured = false
            });
        }
    }

    private static async Task SeedOnPremPricingAsync(InfraSizingDbContext dbContext)
    {
        if (await dbContext.OnPremPricing.AnyAsync())
            return;

        dbContext.OnPremPricing.Add(new OnPremPricingEntity
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private static async Task SeedMendixPricingAsync(InfraSizingDbContext dbContext)
    {
        if (await dbContext.MendixPricing.AnyAsync())
            return;

        dbContext.MendixPricing.Add(new MendixPricingEntity
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private static async Task SeedOutSystemsPricingAsync(InfraSizingDbContext dbContext)
    {
        if (await dbContext.OutSystemsPricing.AnyAsync())
            return;

        dbContext.OutSystemsPricing.Add(new OutSystemsPricingEntity
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private static async Task SeedDistributionsAsync(InfraSizingDbContext dbContext)
    {
        if (await dbContext.DistributionConfigs.AnyAsync())
            return;

        // Seed a minimal set of distributions for testing
        var distributions = new[]
        {
            CreateDistribution(1, "openshift", "OpenShift", "Red Hat", "fas fa-hat-cowboy", "#EE0000", "enterprise,production", 1, true, false),
            CreateDistribution(2, "k3s", "K3s", "SUSE", "fas fa-feather", "#00C7B7", "lightweight,edge", 2, false, false),
            CreateDistribution(3, "aks", "AKS", "Microsoft", "fab fa-microsoft", "#0078D4", "managed,cloud", 3, false, true),
            CreateDistribution(4, "eks", "EKS", "Amazon", "fab fa-aws", "#FF9900", "managed,cloud", 4, false, true),
            CreateDistribution(5, "gke", "GKE", "Google", "fab fa-google", "#4285F4", "managed,cloud", 5, false, true),
            CreateDistribution(6, "rke2", "RKE2", "SUSE", "fas fa-dharmachakra", "#00A4A6", "enterprise,production", 6, true, false),
            CreateDistribution(7, "tanzu", "Tanzu", "VMware", "fas fa-cube", "#1D428A", "enterprise,vmware", 7, true, false),
        };

        foreach (var dist in distributions)
        {
            dbContext.DistributionConfigs.Add(dist);
        }
    }

    private static DistributionConfigEntity CreateDistribution(
        int id, string key, string name, string vendor, string icon, string brandColor,
        string tags, int sortOrder, bool hasInfraNodes, bool hasManagedControlPlane)
    {
        return new DistributionConfigEntity
        {
            Id = id,
            DistributionKey = key,
            Name = name,
            Vendor = vendor,
            Icon = icon,
            BrandColor = brandColor,
            Tags = tags,
            SortOrder = sortOrder,
            HasInfraNodes = hasInfraNodes,
            HasManagedControlPlane = hasManagedControlPlane,
            // Standard node specs for testing
            ProdControlPlaneCpu = hasManagedControlPlane ? 0 : 4,
            ProdControlPlaneRam = hasManagedControlPlane ? 0 : 16,
            ProdControlPlaneDisk = hasManagedControlPlane ? 0 : 100,
            NonProdControlPlaneCpu = hasManagedControlPlane ? 0 : 2,
            NonProdControlPlaneRam = hasManagedControlPlane ? 0 : 8,
            NonProdControlPlaneDisk = hasManagedControlPlane ? 0 : 50,
            ProdWorkerCpu = 8,
            ProdWorkerRam = 32,
            ProdWorkerDisk = 200,
            NonProdWorkerCpu = 4,
            NonProdWorkerRam = 16,
            NonProdWorkerDisk = 100,
            ProdInfraCpu = hasInfraNodes ? 4 : 0,
            ProdInfraRam = hasInfraNodes ? 16 : 0,
            ProdInfraDisk = hasInfraNodes ? 100 : 0,
            NonProdInfraCpu = hasInfraNodes ? 2 : 0,
            NonProdInfraRam = hasInfraNodes ? 8 : 0,
            NonProdInfraDisk = hasInfraNodes ? 50 : 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
