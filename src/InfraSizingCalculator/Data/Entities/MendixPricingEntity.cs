namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// Mendix pricing settings stored in database - based on June 2025 Pricebook
/// All prices are annual (per year) unless otherwise noted
/// </summary>
public class MendixPricingEntity
{
    public int Id { get; set; }

    // ===========================================
    // Mendix Cloud Token Pricing
    // ===========================================
    public decimal CloudTokenPrice { get; set; } = 51.60m;

    // ===========================================
    // Mendix Cloud Dedicated (single tenant AWS VPC)
    // ===========================================
    public decimal CloudDedicatedPrice { get; set; } = 368100m;

    // ===========================================
    // Mendix Cloud Additional Storage (per 100 GB/year)
    // ===========================================
    public decimal AdditionalFileStoragePer100GB { get; set; } = 123m;
    public decimal AdditionalDatabaseStoragePer100GB { get; set; } = 246m;

    // ===========================================
    // Mendix on Azure Pricing
    // ===========================================
    public decimal AzureBasePackagePrice { get; set; } = 6612m;  // Includes 3 environments
    public int AzureBaseEnvironmentsIncluded { get; set; } = 3;
    public decimal AzureAdditionalEnvironmentPrice { get; set; } = 722.40m;
    public int AzureAdditionalEnvironmentTokens { get; set; } = 14;

    // ===========================================
    // Mendix on Kubernetes Pricing
    // ===========================================
    public decimal K8sBasePackagePrice { get; set; } = 6360m;  // Includes 3 environments
    public int K8sBaseEnvironmentsIncluded { get; set; } = 3;

    // K8s Environment Tiered Pricing (per additional env/year)
    public decimal K8sEnvTier1Price { get; set; } = 552m;    // Environments 1-50
    public int K8sEnvTier1Max { get; set; } = 50;

    public decimal K8sEnvTier2Price { get; set; } = 408m;    // Environments 51-100
    public int K8sEnvTier2Max { get; set; } = 100;

    public decimal K8sEnvTier3Price { get; set; } = 240m;    // Environments 101-150
    public int K8sEnvTier3Max { get; set; } = 150;

    public decimal K8sEnvTier4Price { get; set; } = 0m;      // Environments 151+ (free)

    // ===========================================
    // Mendix on Server (VMs/Docker) Pricing
    // ===========================================
    public decimal ServerPerAppPrice { get; set; } = 6612m;
    public decimal ServerUnlimitedAppsPrice { get; set; } = 33060m;

    // ===========================================
    // Mendix on StackIT Pricing
    // ===========================================
    public decimal StackITPerAppPrice { get; set; } = 6612m;
    public decimal StackITUnlimitedAppsPrice { get; set; } = 33060m;

    // ===========================================
    // Mendix on SAP BTP Pricing
    // ===========================================
    public decimal SapBtpPerAppPrice { get; set; } = 6612m;
    public decimal SapBtpUnlimitedAppsPrice { get; set; } = 33060m;

    // ===========================================
    // GenAI Model Resource Packs (Claude v3.5 Sonnet + Cohere Embed v3)
    // ===========================================
    public decimal GenAIModelPackSPrice { get; set; } = 1857.60m;
    public int GenAIModelPackSTokens { get; set; } = 36;

    public decimal GenAIModelPackMPrice { get; set; } = 3715.20m;
    public int GenAIModelPackMTokens { get; set; } = 72;

    public decimal GenAIModelPackLPrice { get; set; } = 7430.40m;
    public int GenAIModelPackLTokens { get; set; } = 144;

    // ===========================================
    // GenAI Knowledge Base Resource Pack (OpenSearch)
    // ===========================================
    public decimal GenAIKnowledgeBasePrice { get; set; } = 2476.80m;
    public int GenAIKnowledgeBaseTokens { get; set; } = 48;
    public decimal GenAIKnowledgeBaseDiskGB { get; set; } = 10m;

    // ===========================================
    // Resource Pack Pricing JSON
    // Contains Standard, Premium, and Premium Plus packs
    // ===========================================
    public string ResourcePackPricingJson { get; set; } = GetDefaultResourcePackPricing();

    // ===========================================
    // Supported Provider Configurations
    // ===========================================
    public string SupportedProvidersJson { get; set; } = "[\"Azure\",\"EKS\",\"AKS\",\"GKE\",\"OpenShift\"]";

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Default Resource Pack pricing from June 2025 Pricebook
    /// </summary>
    private static string GetDefaultResourcePackPricing()
    {
        return @"{
  ""standard"": {
    ""XS"":        { ""price"": 516,    ""tokens"": 10,   ""mxMemory"": 1,   ""mxVCpu"": 0.25, ""dbMemory"": 1,   ""dbVCpu"": 2,  ""dbStorage"": 5,    ""fileStorage"": 10,   ""sla"": 99.5 },
    ""S"":         { ""price"": 1032,   ""tokens"": 20,   ""mxMemory"": 2,   ""mxVCpu"": 0.5,  ""dbMemory"": 2,   ""dbVCpu"": 2,  ""dbStorage"": 10,   ""fileStorage"": 20,   ""sla"": 99.5 },
    ""M"":         { ""price"": 2064,   ""tokens"": 40,   ""mxMemory"": 4,   ""mxVCpu"": 1,    ""dbMemory"": 4,   ""dbVCpu"": 2,  ""dbStorage"": 20,   ""fileStorage"": 40,   ""sla"": 99.5 },
    ""L"":         { ""price"": 4128,   ""tokens"": 80,   ""mxMemory"": 8,   ""mxVCpu"": 2,    ""dbMemory"": 8,   ""dbVCpu"": 2,  ""dbStorage"": 40,   ""fileStorage"": 80,   ""sla"": 99.5 },
    ""XL"":        { ""price"": 8256,   ""tokens"": 160,  ""mxMemory"": 16,  ""mxVCpu"": 4,    ""dbMemory"": 16,  ""dbVCpu"": 4,  ""dbStorage"": 80,   ""fileStorage"": 160,  ""sla"": 99.5 },
    ""2XL"":       { ""price"": 16512,  ""tokens"": 320,  ""mxMemory"": 32,  ""mxVCpu"": 8,    ""dbMemory"": 32,  ""dbVCpu"": 4,  ""dbStorage"": 160,  ""fileStorage"": 320,  ""sla"": 99.5 },
    ""3XL"":       { ""price"": 33024,  ""tokens"": 640,  ""mxMemory"": 64,  ""mxVCpu"": 16,   ""dbMemory"": 64,  ""dbVCpu"": 8,  ""dbStorage"": 320,  ""fileStorage"": 640,  ""sla"": 99.5 },
    ""4XL"":       { ""price"": 66048,  ""tokens"": 1280, ""mxMemory"": 128, ""mxVCpu"": 32,   ""dbMemory"": 128, ""dbVCpu"": 16, ""dbStorage"": 640,  ""fileStorage"": 1280, ""sla"": 99.5 },
    ""4XL-5XLDB"": { ""price"": 115584, ""tokens"": 2240, ""mxMemory"": 128, ""mxVCpu"": 32,   ""dbMemory"": 256, ""dbVCpu"": 32, ""dbStorage"": 1280, ""fileStorage"": 1280, ""sla"": 99.5 }
  },
  ""premium"": {
    ""S"":         { ""price"": 1548,   ""tokens"": 30,   ""mxMemory"": 2,   ""mxVCpu"": 0.5,  ""dbMemory"": 2,   ""dbVCpu"": 2,  ""dbStorage"": 10,   ""fileStorage"": 20,   ""sla"": 99.95, ""fallback"": true },
    ""M"":         { ""price"": 3096,   ""tokens"": 60,   ""mxMemory"": 4,   ""mxVCpu"": 1,    ""dbMemory"": 4,   ""dbVCpu"": 2,  ""dbStorage"": 20,   ""fileStorage"": 40,   ""sla"": 99.95, ""fallback"": true },
    ""L"":         { ""price"": 6192,   ""tokens"": 120,  ""mxMemory"": 8,   ""mxVCpu"": 2,    ""dbMemory"": 8,   ""dbVCpu"": 2,  ""dbStorage"": 40,   ""fileStorage"": 80,   ""sla"": 99.95, ""fallback"": true },
    ""XL"":        { ""price"": 12384,  ""tokens"": 240,  ""mxMemory"": 16,  ""mxVCpu"": 4,    ""dbMemory"": 16,  ""dbVCpu"": 4,  ""dbStorage"": 80,   ""fileStorage"": 160,  ""sla"": 99.95, ""fallback"": true },
    ""2XL"":       { ""price"": 24768,  ""tokens"": 480,  ""mxMemory"": 32,  ""mxVCpu"": 8,    ""dbMemory"": 32,  ""dbVCpu"": 4,  ""dbStorage"": 160,  ""fileStorage"": 320,  ""sla"": 99.95, ""fallback"": true },
    ""3XL"":       { ""price"": 49536,  ""tokens"": 960,  ""mxMemory"": 64,  ""mxVCpu"": 16,   ""dbMemory"": 64,  ""dbVCpu"": 8,  ""dbStorage"": 320,  ""fileStorage"": 640,  ""sla"": 99.95, ""fallback"": true },
    ""4XL"":       { ""price"": 99072,  ""tokens"": 1920, ""mxMemory"": 128, ""mxVCpu"": 32,   ""dbMemory"": 128, ""dbVCpu"": 16, ""dbStorage"": 640,  ""fileStorage"": 1280, ""sla"": 99.95, ""fallback"": true },
    ""4XL-5XLDB"": { ""price"": 173376, ""tokens"": 3360, ""mxMemory"": 128, ""mxVCpu"": 32,   ""dbMemory"": 256, ""dbVCpu"": 32, ""dbStorage"": 1280, ""fileStorage"": 1280, ""sla"": 99.95, ""fallback"": true }
  },
  ""premiumPlus"": {
    ""XL"":        { ""price"": 20640,  ""tokens"": 400,  ""mxMemory"": 16,  ""mxVCpu"": 4,    ""dbMemory"": 16,  ""dbVCpu"": 4,  ""dbStorage"": 80,   ""fileStorage"": 160,  ""sla"": 99.95, ""fallback"": true, ""multiRegion"": true },
    ""XXL"":       { ""price"": 41280,  ""tokens"": 800,  ""mxMemory"": 32,  ""mxVCpu"": 8,    ""dbMemory"": 32,  ""dbVCpu"": 4,  ""dbStorage"": 160,  ""fileStorage"": 320,  ""sla"": 99.95, ""fallback"": true, ""multiRegion"": true },
    ""3XL"":       { ""price"": 82560,  ""tokens"": 1600, ""mxMemory"": 64,  ""mxVCpu"": 16,   ""dbMemory"": 64,  ""dbVCpu"": 8,  ""dbStorage"": 320,  ""fileStorage"": 640,  ""sla"": 99.95, ""fallback"": true, ""multiRegion"": true },
    ""4XL"":       { ""price"": 165120, ""tokens"": 3200, ""mxMemory"": 128, ""mxVCpu"": 32,   ""dbMemory"": 128, ""dbVCpu"": 16, ""dbStorage"": 640,  ""fileStorage"": 1280, ""sla"": 99.95, ""fallback"": true, ""multiRegion"": true },
    ""4XL-5XLDB"": { ""price"": 288960, ""tokens"": 5600, ""mxMemory"": 128, ""mxVCpu"": 32,   ""dbMemory"": 128, ""dbVCpu"": 32, ""dbStorage"": 1280, ""fileStorage"": 1280, ""sla"": 99.95, ""fallback"": true, ""multiRegion"": true }
  }
}";
    }
}
