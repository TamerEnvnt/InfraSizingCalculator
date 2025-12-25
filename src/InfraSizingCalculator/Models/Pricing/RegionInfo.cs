namespace InfraSizingCalculator.Models.Pricing;

/// <summary>
/// Cloud region information
/// </summary>
public class RegionInfo
{
    /// <summary>
    /// Region code (e.g., us-east-1, eastus, us-central1)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name (e.g., "US East (N. Virginia)")
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Cloud provider
    /// </summary>
    public CloudProvider Provider { get; set; }

    /// <summary>
    /// Geographic location
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a preferred/popular region
    /// </summary>
    public bool IsPreferred { get; set; }
}

/// <summary>
/// Static region data for all providers
/// </summary>
public static class CloudRegions
{
    /// <summary>
    /// AWS regions
    /// </summary>
    public static readonly List<RegionInfo> AWSRegions = new()
    {
        new() { Code = "us-east-1", DisplayName = "US East (N. Virginia)", Provider = CloudProvider.AWS, Location = "Virginia, USA", IsPreferred = true },
        new() { Code = "us-east-2", DisplayName = "US East (Ohio)", Provider = CloudProvider.AWS, Location = "Ohio, USA" },
        new() { Code = "us-west-1", DisplayName = "US West (N. California)", Provider = CloudProvider.AWS, Location = "California, USA" },
        new() { Code = "us-west-2", DisplayName = "US West (Oregon)", Provider = CloudProvider.AWS, Location = "Oregon, USA", IsPreferred = true },
        new() { Code = "eu-west-1", DisplayName = "Europe (Ireland)", Provider = CloudProvider.AWS, Location = "Ireland", IsPreferred = true },
        new() { Code = "eu-west-2", DisplayName = "Europe (London)", Provider = CloudProvider.AWS, Location = "UK" },
        new() { Code = "eu-west-3", DisplayName = "Europe (Paris)", Provider = CloudProvider.AWS, Location = "France" },
        new() { Code = "eu-central-1", DisplayName = "Europe (Frankfurt)", Provider = CloudProvider.AWS, Location = "Germany", IsPreferred = true },
        new() { Code = "ap-southeast-1", DisplayName = "Asia Pacific (Singapore)", Provider = CloudProvider.AWS, Location = "Singapore" },
        new() { Code = "ap-southeast-2", DisplayName = "Asia Pacific (Sydney)", Provider = CloudProvider.AWS, Location = "Australia" },
        new() { Code = "ap-northeast-1", DisplayName = "Asia Pacific (Tokyo)", Provider = CloudProvider.AWS, Location = "Japan" },
        new() { Code = "ap-south-1", DisplayName = "Asia Pacific (Mumbai)", Provider = CloudProvider.AWS, Location = "India" },
        new() { Code = "me-south-1", DisplayName = "Middle East (Bahrain)", Provider = CloudProvider.AWS, Location = "Bahrain" },
        new() { Code = "me-central-1", DisplayName = "Middle East (UAE)", Provider = CloudProvider.AWS, Location = "UAE" }
    };

    /// <summary>
    /// Azure regions
    /// </summary>
    public static readonly List<RegionInfo> AzureRegions = new()
    {
        new() { Code = "eastus", DisplayName = "East US", Provider = CloudProvider.Azure, Location = "Virginia, USA", IsPreferred = true },
        new() { Code = "eastus2", DisplayName = "East US 2", Provider = CloudProvider.Azure, Location = "Virginia, USA" },
        new() { Code = "westus", DisplayName = "West US", Provider = CloudProvider.Azure, Location = "California, USA" },
        new() { Code = "westus2", DisplayName = "West US 2", Provider = CloudProvider.Azure, Location = "Washington, USA", IsPreferred = true },
        new() { Code = "westeurope", DisplayName = "West Europe", Provider = CloudProvider.Azure, Location = "Netherlands", IsPreferred = true },
        new() { Code = "northeurope", DisplayName = "North Europe", Provider = CloudProvider.Azure, Location = "Ireland" },
        new() { Code = "uksouth", DisplayName = "UK South", Provider = CloudProvider.Azure, Location = "UK" },
        new() { Code = "germanywestcentral", DisplayName = "Germany West Central", Provider = CloudProvider.Azure, Location = "Germany" },
        new() { Code = "southeastasia", DisplayName = "Southeast Asia", Provider = CloudProvider.Azure, Location = "Singapore" },
        new() { Code = "australiaeast", DisplayName = "Australia East", Provider = CloudProvider.Azure, Location = "Australia" },
        new() { Code = "japaneast", DisplayName = "Japan East", Provider = CloudProvider.Azure, Location = "Japan" },
        new() { Code = "centralindia", DisplayName = "Central India", Provider = CloudProvider.Azure, Location = "India" },
        new() { Code = "uaenorth", DisplayName = "UAE North", Provider = CloudProvider.Azure, Location = "UAE" }
    };

    /// <summary>
    /// GCP regions
    /// </summary>
    public static readonly List<RegionInfo> GCPRegions = new()
    {
        new() { Code = "us-central1", DisplayName = "Iowa", Provider = CloudProvider.GCP, Location = "Iowa, USA", IsPreferred = true },
        new() { Code = "us-east1", DisplayName = "South Carolina", Provider = CloudProvider.GCP, Location = "South Carolina, USA" },
        new() { Code = "us-east4", DisplayName = "Northern Virginia", Provider = CloudProvider.GCP, Location = "Virginia, USA" },
        new() { Code = "us-west1", DisplayName = "Oregon", Provider = CloudProvider.GCP, Location = "Oregon, USA", IsPreferred = true },
        new() { Code = "europe-west1", DisplayName = "Belgium", Provider = CloudProvider.GCP, Location = "Belgium", IsPreferred = true },
        new() { Code = "europe-west2", DisplayName = "London", Provider = CloudProvider.GCP, Location = "UK" },
        new() { Code = "europe-west3", DisplayName = "Frankfurt", Provider = CloudProvider.GCP, Location = "Germany" },
        new() { Code = "asia-southeast1", DisplayName = "Singapore", Provider = CloudProvider.GCP, Location = "Singapore" },
        new() { Code = "australia-southeast1", DisplayName = "Sydney", Provider = CloudProvider.GCP, Location = "Australia" },
        new() { Code = "asia-northeast1", DisplayName = "Tokyo", Provider = CloudProvider.GCP, Location = "Japan" },
        new() { Code = "asia-south1", DisplayName = "Mumbai", Provider = CloudProvider.GCP, Location = "India" },
        new() { Code = "me-west1", DisplayName = "Tel Aviv", Provider = CloudProvider.GCP, Location = "Israel" }
    };

    /// <summary>
    /// Oracle OCI regions
    /// </summary>
    public static readonly List<RegionInfo> OCIRegions = new()
    {
        new() { Code = "us-ashburn-1", DisplayName = "US East (Ashburn)", Provider = CloudProvider.OCI, Location = "Virginia, USA", IsPreferred = true },
        new() { Code = "us-phoenix-1", DisplayName = "US West (Phoenix)", Provider = CloudProvider.OCI, Location = "Arizona, USA" },
        new() { Code = "uk-london-1", DisplayName = "UK South (London)", Provider = CloudProvider.OCI, Location = "UK", IsPreferred = true },
        new() { Code = "eu-frankfurt-1", DisplayName = "Germany Central (Frankfurt)", Provider = CloudProvider.OCI, Location = "Germany" },
        new() { Code = "eu-amsterdam-1", DisplayName = "Netherlands Northwest (Amsterdam)", Provider = CloudProvider.OCI, Location = "Netherlands" },
        new() { Code = "ap-sydney-1", DisplayName = "Australia East (Sydney)", Provider = CloudProvider.OCI, Location = "Australia" },
        new() { Code = "ap-tokyo-1", DisplayName = "Japan East (Tokyo)", Provider = CloudProvider.OCI, Location = "Japan" },
        new() { Code = "ap-mumbai-1", DisplayName = "India West (Mumbai)", Provider = CloudProvider.OCI, Location = "India" },
        new() { Code = "me-dubai-1", DisplayName = "UAE East (Dubai)", Provider = CloudProvider.OCI, Location = "UAE" },
        new() { Code = "me-jeddah-1", DisplayName = "Saudi Arabia West (Jeddah)", Provider = CloudProvider.OCI, Location = "Saudi Arabia" }
    };

    /// <summary>
    /// IBM Cloud regions
    /// </summary>
    public static readonly List<RegionInfo> IBMRegions = new()
    {
        new() { Code = "us-south", DisplayName = "Dallas", Provider = CloudProvider.IBM, Location = "Texas, USA", IsPreferred = true },
        new() { Code = "us-east", DisplayName = "Washington DC", Provider = CloudProvider.IBM, Location = "Virginia, USA" },
        new() { Code = "eu-de", DisplayName = "Frankfurt", Provider = CloudProvider.IBM, Location = "Germany", IsPreferred = true },
        new() { Code = "eu-gb", DisplayName = "London", Provider = CloudProvider.IBM, Location = "UK" },
        new() { Code = "jp-tok", DisplayName = "Tokyo", Provider = CloudProvider.IBM, Location = "Japan" },
        new() { Code = "au-syd", DisplayName = "Sydney", Provider = CloudProvider.IBM, Location = "Australia" }
    };

    /// <summary>
    /// Alibaba Cloud regions
    /// </summary>
    public static readonly List<RegionInfo> AlibabaRegions = new()
    {
        new() { Code = "cn-hangzhou", DisplayName = "China (Hangzhou)", Provider = CloudProvider.Alibaba, Location = "China", IsPreferred = true },
        new() { Code = "cn-shanghai", DisplayName = "China (Shanghai)", Provider = CloudProvider.Alibaba, Location = "China" },
        new() { Code = "ap-southeast-1", DisplayName = "Singapore", Provider = CloudProvider.Alibaba, Location = "Singapore", IsPreferred = true },
        new() { Code = "us-west-1", DisplayName = "US (Silicon Valley)", Provider = CloudProvider.Alibaba, Location = "California, USA" },
        new() { Code = "eu-central-1", DisplayName = "Germany (Frankfurt)", Provider = CloudProvider.Alibaba, Location = "Germany" },
        new() { Code = "me-east-1", DisplayName = "UAE (Dubai)", Provider = CloudProvider.Alibaba, Location = "UAE" }
    };

    /// <summary>
    /// DigitalOcean regions
    /// </summary>
    public static readonly List<RegionInfo> DigitalOceanRegions = new()
    {
        new() { Code = "nyc1", DisplayName = "New York 1", Provider = CloudProvider.DigitalOcean, Location = "New York, USA", IsPreferred = true },
        new() { Code = "nyc3", DisplayName = "New York 3", Provider = CloudProvider.DigitalOcean, Location = "New York, USA" },
        new() { Code = "sfo3", DisplayName = "San Francisco 3", Provider = CloudProvider.DigitalOcean, Location = "California, USA", IsPreferred = true },
        new() { Code = "ams3", DisplayName = "Amsterdam 3", Provider = CloudProvider.DigitalOcean, Location = "Netherlands" },
        new() { Code = "lon1", DisplayName = "London 1", Provider = CloudProvider.DigitalOcean, Location = "UK", IsPreferred = true },
        new() { Code = "fra1", DisplayName = "Frankfurt 1", Provider = CloudProvider.DigitalOcean, Location = "Germany" },
        new() { Code = "sgp1", DisplayName = "Singapore 1", Provider = CloudProvider.DigitalOcean, Location = "Singapore" },
        new() { Code = "blr1", DisplayName = "Bangalore 1", Provider = CloudProvider.DigitalOcean, Location = "India" },
        new() { Code = "syd1", DisplayName = "Sydney 1", Provider = CloudProvider.DigitalOcean, Location = "Australia" }
    };

    /// <summary>
    /// Linode/Akamai regions
    /// </summary>
    public static readonly List<RegionInfo> LinodeRegions = new()
    {
        new() { Code = "us-east", DisplayName = "Newark, NJ", Provider = CloudProvider.Linode, Location = "New Jersey, USA", IsPreferred = true },
        new() { Code = "us-central", DisplayName = "Dallas, TX", Provider = CloudProvider.Linode, Location = "Texas, USA" },
        new() { Code = "us-west", DisplayName = "Fremont, CA", Provider = CloudProvider.Linode, Location = "California, USA", IsPreferred = true },
        new() { Code = "eu-west", DisplayName = "London, UK", Provider = CloudProvider.Linode, Location = "UK", IsPreferred = true },
        new() { Code = "eu-central", DisplayName = "Frankfurt, DE", Provider = CloudProvider.Linode, Location = "Germany" },
        new() { Code = "ap-south", DisplayName = "Singapore", Provider = CloudProvider.Linode, Location = "Singapore" },
        new() { Code = "ap-northeast", DisplayName = "Tokyo, JP", Provider = CloudProvider.Linode, Location = "Japan" },
        new() { Code = "ap-southeast", DisplayName = "Sydney, AU", Provider = CloudProvider.Linode, Location = "Australia" }
    };

    /// <summary>
    /// Vultr regions
    /// </summary>
    public static readonly List<RegionInfo> VultrRegions = new()
    {
        new() { Code = "ewr", DisplayName = "New Jersey", Provider = CloudProvider.Vultr, Location = "New Jersey, USA", IsPreferred = true },
        new() { Code = "dfw", DisplayName = "Dallas", Provider = CloudProvider.Vultr, Location = "Texas, USA" },
        new() { Code = "lax", DisplayName = "Los Angeles", Provider = CloudProvider.Vultr, Location = "California, USA", IsPreferred = true },
        new() { Code = "lhr", DisplayName = "London", Provider = CloudProvider.Vultr, Location = "UK", IsPreferred = true },
        new() { Code = "fra", DisplayName = "Frankfurt", Provider = CloudProvider.Vultr, Location = "Germany" },
        new() { Code = "ams", DisplayName = "Amsterdam", Provider = CloudProvider.Vultr, Location = "Netherlands" },
        new() { Code = "sgp", DisplayName = "Singapore", Provider = CloudProvider.Vultr, Location = "Singapore" },
        new() { Code = "nrt", DisplayName = "Tokyo", Provider = CloudProvider.Vultr, Location = "Japan" },
        new() { Code = "syd", DisplayName = "Sydney", Provider = CloudProvider.Vultr, Location = "Australia" }
    };

    /// <summary>
    /// Hetzner regions
    /// </summary>
    public static readonly List<RegionInfo> HetznerRegions = new()
    {
        new() { Code = "fsn1", DisplayName = "Falkenstein", Provider = CloudProvider.Hetzner, Location = "Germany", IsPreferred = true },
        new() { Code = "nbg1", DisplayName = "Nuremberg", Provider = CloudProvider.Hetzner, Location = "Germany" },
        new() { Code = "hel1", DisplayName = "Helsinki", Provider = CloudProvider.Hetzner, Location = "Finland", IsPreferred = true },
        new() { Code = "ash", DisplayName = "Ashburn", Provider = CloudProvider.Hetzner, Location = "Virginia, USA" }
    };

    /// <summary>
    /// Civo Cloud regions
    /// </summary>
    public static readonly List<RegionInfo> CivoRegions = new()
    {
        new() { Code = "lon1", DisplayName = "London", Provider = CloudProvider.Civo, Location = "UK", IsPreferred = true },
        new() { Code = "nyc1", DisplayName = "New York", Provider = CloudProvider.Civo, Location = "New York, USA", IsPreferred = true },
        new() { Code = "fra1", DisplayName = "Frankfurt", Provider = CloudProvider.Civo, Location = "Germany" },
        new() { Code = "phx1", DisplayName = "Phoenix", Provider = CloudProvider.Civo, Location = "Arizona, USA" }
    };

    /// <summary>
    /// Exoscale regions
    /// </summary>
    public static readonly List<RegionInfo> ExoscaleRegions = new()
    {
        new() { Code = "ch-gva-2", DisplayName = "Geneva", Provider = CloudProvider.Exoscale, Location = "Switzerland", IsPreferred = true },
        new() { Code = "ch-dk-2", DisplayName = "Zurich", Provider = CloudProvider.Exoscale, Location = "Switzerland" },
        new() { Code = "de-fra-1", DisplayName = "Frankfurt", Provider = CloudProvider.Exoscale, Location = "Germany", IsPreferred = true },
        new() { Code = "de-muc-1", DisplayName = "Munich", Provider = CloudProvider.Exoscale, Location = "Germany" },
        new() { Code = "at-vie-1", DisplayName = "Vienna", Provider = CloudProvider.Exoscale, Location = "Austria" },
        new() { Code = "bg-sof-1", DisplayName = "Sofia", Provider = CloudProvider.Exoscale, Location = "Bulgaria" }
    };

    /// <summary>
    /// Generic regions for providers without specific region data
    /// </summary>
    public static readonly List<RegionInfo> GenericRegions = new()
    {
        new() { Code = "default", DisplayName = "Default Region", Location = "Default", IsPreferred = true }
    };

    /// <summary>
    /// Get all regions for a provider
    /// </summary>
    public static List<RegionInfo> GetRegions(CloudProvider provider)
    {
        return provider switch
        {
            // Major Cloud Providers
            CloudProvider.AWS => AWSRegions,
            CloudProvider.Azure => AzureRegions,
            CloudProvider.GCP => GCPRegions,
            CloudProvider.OCI => OCIRegions,
            CloudProvider.IBM => IBMRegions,
            CloudProvider.Alibaba => AlibabaRegions,
            CloudProvider.Tencent => GenericRegions,
            CloudProvider.Huawei => GenericRegions,

            // Managed OpenShift Services (use underlying provider regions)
            CloudProvider.ROSA => AWSRegions,
            CloudProvider.ARO => AzureRegions,
            CloudProvider.OSD => GCPRegions,
            CloudProvider.ROKS => IBMRegions,

            // Developer-Friendly Clouds
            CloudProvider.DigitalOcean => DigitalOceanRegions,
            CloudProvider.Linode => LinodeRegions,
            CloudProvider.Vultr => VultrRegions,
            CloudProvider.Hetzner => HetznerRegions,
            CloudProvider.OVH => GenericRegions,
            CloudProvider.Scaleway => GenericRegions,
            CloudProvider.Civo => CivoRegions,
            CloudProvider.Exoscale => ExoscaleRegions,

            CloudProvider.OnPrem => new List<RegionInfo>(),
            _ => GenericRegions
        };
    }

    /// <summary>
    /// Get preferred regions for a provider
    /// </summary>
    public static List<RegionInfo> GetPreferredRegions(CloudProvider provider)
    {
        return GetRegions(provider).Where(r => r.IsPreferred).ToList();
    }
}
