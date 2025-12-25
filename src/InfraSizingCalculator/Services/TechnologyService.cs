using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service providing technology-specific configurations
/// BR-T001 through BR-T006
/// </summary>
public class TechnologyService : ITechnologyService
{
    private static readonly Dictionary<Technology, TechnologyConfig> Configs = new()
    {
        // BR-T002: .NET - Microsoft Purple #512BD4
        [Technology.DotNet] = new TechnologyConfig
        {
            Technology = Technology.DotNet,
            Name = ".NET",
            Icon = "🟣",
            BrandColor = "#512BD4",
            Vendor = "Microsoft",
            Description = "Cross-platform framework for building modern applications",
            PlatformType = PlatformType.Native,
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new TierSpecs(0.25, 0.5),
                [AppTier.Medium] = new TierSpecs(0.5, 1),
                [AppTier.Large] = new TierSpecs(1, 2),
                [AppTier.XLarge] = new TierSpecs(2, 4)
            },
            VMRoles = new TechnologyVMRoles
            {
                Technology = Technology.DotNet,
                DeploymentName = ".NET on Windows/IIS",
                Description = "ASP.NET on Windows Server with IIS",
                Roles = new List<TechnologyServerRole>
                {
                    new() { Id = "dotnet-iis", Name = "IIS Web Server", Icon = "🌐", Description = "Windows Server with IIS for hosting ASP.NET applications", DefaultSize = AppTier.Medium, DefaultDiskGB = 100, Required = true, ScaleHorizontally = true },
                    new() { Id = "dotnet-db", Name = "SQL Server", Icon = "🗄️", Description = "Microsoft SQL Server database", DefaultSize = AppTier.Large, DefaultDiskGB = 500, Required = true, MemoryMultiplier = 1.5 },
                    new() { Id = "dotnet-cache", Name = "Cache Server", Icon = "⚡", Description = "Redis cache for session state and caching (optional)", DefaultSize = AppTier.Small, DefaultDiskGB = 50, Required = false }
                }
            }
        },

        // BR-T003: Java - Oracle Blue #007396
        [Technology.Java] = new TechnologyConfig
        {
            Technology = Technology.Java,
            Name = "Java",
            Icon = "☕",
            BrandColor = "#007396",
            Vendor = "Oracle",
            Description = "Enterprise-grade runtime with extensive ecosystem",
            PlatformType = PlatformType.Native,
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new TierSpecs(0.5, 1),
                [AppTier.Medium] = new TierSpecs(1, 2),
                [AppTier.Large] = new TierSpecs(2, 4),
                [AppTier.XLarge] = new TierSpecs(4, 8)
            },
            VMRoles = new TechnologyVMRoles
            {
                Technology = Technology.Java,
                DeploymentName = "Java Enterprise",
                Description = "WebLogic, JBoss/WildFly, or Tomcat deployment",
                Roles = new List<TechnologyServerRole>
                {
                    new() { Id = "java-web", Name = "Web Server", Icon = "🌐", Description = "Apache/Nginx reverse proxy for load balancing and SSL termination", DefaultSize = AppTier.Small, DefaultDiskGB = 50, Required = false },
                    new() { Id = "java-app", Name = "Application Server", Icon = "☕", Description = "WebLogic, JBoss/WildFly, or Tomcat for running Java applications", DefaultSize = AppTier.Large, DefaultDiskGB = 100, Required = true, ScaleHorizontally = true, MemoryMultiplier = 1.5 },
                    new() { Id = "java-db", Name = "Database Server", Icon = "🗄️", Description = "PostgreSQL, MySQL, or Oracle database", DefaultSize = AppTier.Medium, DefaultDiskGB = 500, Required = true, MemoryMultiplier = 1.5 },
                    new() { Id = "java-cache", Name = "Cache Server", Icon = "⚡", Description = "Redis or Memcached for caching (optional)", DefaultSize = AppTier.Small, DefaultDiskGB = 50, Required = false }
                }
            }
        },

        // BR-T004: Node.js - Official Green #339933
        // Note: Small tier RAM increased to 1 GB per official docs (V8 heap management overhead)
        [Technology.NodeJs] = new TechnologyConfig
        {
            Technology = Technology.NodeJs,
            Name = "Node.js",
            Icon = "🟢",
            BrandColor = "#339933",
            Vendor = "OpenJS Foundation",
            Description = "JavaScript runtime for event-driven applications",
            PlatformType = PlatformType.Native,
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new TierSpecs(0.25, 1),
                [AppTier.Medium] = new TierSpecs(0.5, 1),
                [AppTier.Large] = new TierSpecs(1, 2),
                [AppTier.XLarge] = new TierSpecs(2, 4)
            },
            VMRoles = new TechnologyVMRoles
            {
                Technology = Technology.NodeJs,
                DeploymentName = "Node.js Application",
                Description = "Node.js with PM2 or similar process manager",
                Roles = new List<TechnologyServerRole>
                {
                    new() { Id = "node-web", Name = "Web/Proxy Server", Icon = "🌐", Description = "Nginx reverse proxy for load balancing and static files", DefaultSize = AppTier.Small, DefaultDiskGB = 50, Required = false },
                    new() { Id = "node-app", Name = "Application Server", Icon = "🟢", Description = "Node.js runtime with PM2 process manager", DefaultSize = AppTier.Medium, DefaultDiskGB = 100, Required = true, ScaleHorizontally = true },
                    new() { Id = "node-db", Name = "Database Server", Icon = "🗄️", Description = "MongoDB, PostgreSQL, or MySQL", DefaultSize = AppTier.Medium, DefaultDiskGB = 500, Required = true },
                    new() { Id = "node-cache", Name = "Cache Server", Icon = "⚡", Description = "Redis for caching and sessions (optional)", DefaultSize = AppTier.Small, DefaultDiskGB = 50, Required = false }
                }
            }
        },

        // BR-T007: Python - Python Yellow/Blue #3776AB
        // Note: Small tier RAM increased to 1 GB per official docs (WSGI/Django overhead)
        [Technology.Python] = new TechnologyConfig
        {
            Technology = Technology.Python,
            Name = "Python",
            Icon = "🐍",
            BrandColor = "#3776AB",
            Vendor = "Python Software Foundation",
            Description = "Versatile language for web, data science, and automation",
            PlatformType = PlatformType.Native,
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new TierSpecs(0.25, 1),
                [AppTier.Medium] = new TierSpecs(0.5, 1),
                [AppTier.Large] = new TierSpecs(1, 2),
                [AppTier.XLarge] = new TierSpecs(2, 4)
            },
            VMRoles = new TechnologyVMRoles
            {
                Technology = Technology.Python,
                DeploymentName = "Python Web Application",
                Description = "Django/Flask with Gunicorn/uWSGI",
                Roles = new List<TechnologyServerRole>
                {
                    new() { Id = "python-web", Name = "Web Server", Icon = "🌐", Description = "Nginx reverse proxy for static files and load balancing", DefaultSize = AppTier.Small, DefaultDiskGB = 50, Required = false },
                    new() { Id = "python-app", Name = "Application Server", Icon = "🐍", Description = "Gunicorn/uWSGI running Django or Flask", DefaultSize = AppTier.Medium, DefaultDiskGB = 100, Required = true, ScaleHorizontally = true },
                    new() { Id = "python-db", Name = "Database Server", Icon = "🗄️", Description = "PostgreSQL or MySQL database", DefaultSize = AppTier.Medium, DefaultDiskGB = 500, Required = true },
                    new() { Id = "python-cache", Name = "Cache Server", Icon = "⚡", Description = "Redis for caching (optional)", DefaultSize = AppTier.Small, DefaultDiskGB = 50, Required = false }
                }
            }
        },

        // BR-T005: Go - Gopher Cyan #00ADD8
        [Technology.Go] = new TechnologyConfig
        {
            Technology = Technology.Go,
            Name = "Go",
            Icon = "🔵",
            BrandColor = "#00ADD8",
            Vendor = "Google",
            Description = "Efficient compiled language for microservices",
            PlatformType = PlatformType.Native,
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new TierSpecs(0.125, 0.25),
                [AppTier.Medium] = new TierSpecs(0.25, 0.5),
                [AppTier.Large] = new TierSpecs(0.5, 1),
                [AppTier.XLarge] = new TierSpecs(1, 2)
            },
            VMRoles = new TechnologyVMRoles
            {
                Technology = Technology.Go,
                DeploymentName = "Go Application",
                Description = "Compiled Go binary deployment",
                Roles = new List<TechnologyServerRole>
                {
                    new() { Id = "go-app", Name = "Application Server", Icon = "🔵", Description = "Go compiled binary with built-in HTTP server", DefaultSize = AppTier.Medium, DefaultDiskGB = 50, Required = true, ScaleHorizontally = true },
                    new() { Id = "go-db", Name = "Database Server", Icon = "🗄️", Description = "PostgreSQL, MySQL, or CockroachDB", DefaultSize = AppTier.Medium, DefaultDiskGB = 500, Required = true },
                    new() { Id = "go-cache", Name = "Cache Server", Icon = "⚡", Description = "Redis for caching (optional)", DefaultSize = AppTier.Small, DefaultDiskGB = 50, Required = false }
                }
            }
        },

        // BR-T006: Mendix - Mendix Blue #0CABF9
        [Technology.Mendix] = new TechnologyConfig
        {
            Technology = Technology.Mendix,
            Name = "Mendix",
            Icon = "🔷",
            BrandColor = "#0CABF9",
            Vendor = "Siemens",
            Description = "Enterprise low-code platform for rapid development",
            PlatformType = PlatformType.LowCode,
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new TierSpecs(1, 2),
                [AppTier.Medium] = new TierSpecs(2, 4),
                [AppTier.Large] = new TierSpecs(4, 8),
                [AppTier.XLarge] = new TierSpecs(8, 16)
            },
            VMRoles = new TechnologyVMRoles
            {
                Technology = Technology.Mendix,
                DeploymentName = "Mendix On-Premises",
                Description = "Traditional VM-based Mendix deployment",
                Roles = new List<TechnologyServerRole>
                {
                    new() { Id = "mendix-app", Name = "Mendix Application Server", Icon = "🔷", Description = "Runs Mendix runtime and applications", DefaultSize = AppTier.Medium, DefaultDiskGB = 100, Required = true, ScaleHorizontally = true, MemoryMultiplier = 1.5 },
                    new() { Id = "mendix-db", Name = "Database Server", Icon = "🗄️", Description = "PostgreSQL or SQL Server for application data", DefaultSize = AppTier.Medium, DefaultDiskGB = 500, Required = true, MemoryMultiplier = 1.5 },
                    new() { Id = "mendix-file", Name = "File Storage Server", Icon = "💾", Description = "Stores uploaded files and attachments (optional - can use cloud storage)", DefaultSize = AppTier.Small, DefaultDiskGB = 1000, Required = false }
                }
            }
        },

        // BR-T008: OutSystems - OutSystems Red #FF6B35
        [Technology.OutSystems] = new TechnologyConfig
        {
            Technology = Technology.OutSystems,
            Name = "OutSystems",
            Icon = "🟠",
            BrandColor = "#FF6B35",
            Vendor = "OutSystems",
            Description = "Enterprise low-code platform for rapid application development",
            PlatformType = PlatformType.LowCode,
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new TierSpecs(1, 2),
                [AppTier.Medium] = new TierSpecs(2, 4),
                [AppTier.Large] = new TierSpecs(4, 8),
                [AppTier.XLarge] = new TierSpecs(8, 16)
            },
            VMRoles = new TechnologyVMRoles
            {
                Technology = Technology.OutSystems,
                DeploymentName = "OutSystems On-Premises",
                Description = "OutSystems platform on traditional infrastructure",
                Roles = new List<TechnologyServerRole>
                {
                    new() { Id = "os-controller", Name = "Deployment Controller", Icon = "🎛️", Description = "Manages app compilation, staging, and deployment. One per environment.", DefaultSize = AppTier.Large, DefaultDiskGB = 200, Required = true, MaxInstances = 1, MemoryMultiplier = 1.5 },
                    new() { Id = "os-frontend", Name = "Front-End Server", Icon = "🌐", Description = "Runs applications and serves end users. Scale horizontally for load.", DefaultSize = AppTier.Medium, DefaultDiskGB = 100, Required = true, ScaleHorizontally = true, MinInstances = 2, MemoryMultiplier = 1.5 },
                    new() { Id = "os-db", Name = "Database Server", Icon = "🗄️", Description = "SQL Server (Always On) or Oracle (RAC) for platform and application data. Can be clustered.", DefaultSize = AppTier.Large, DefaultDiskGB = 500, Required = true, ScaleHorizontally = true, MemoryMultiplier = 1.5 },
                    new() { Id = "os-lifetime", Name = "LifeTime Server", Icon = "🔄", Description = "Environment management & CI/CD console. Single instance shared across all environments.", DefaultSize = AppTier.Medium, DefaultDiskGB = 100, Required = true, MaxInstances = 1 }
                }
            }
        }
    };

    public TechnologyConfig GetConfig(Technology technology)
    {
        return Configs.TryGetValue(technology, out var config)
            ? config
            : Configs[Technology.DotNet]; // Default fallback
    }

    public IEnumerable<TechnologyConfig> GetAll()
    {
        return Configs.Values;
    }

    public IEnumerable<TechnologyConfig> GetByPlatformType(PlatformType platformType)
    {
        return Configs.Values.Where(c => c.PlatformType == platformType);
    }

    /// <summary>
    /// Gets the VM roles template for a technology
    /// </summary>
    public TechnologyVMRoles? GetVMRoles(Technology technology)
    {
        return Configs.TryGetValue(technology, out var config) ? config.VMRoles : null;
    }
}
