using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service providing technology-specific server role templates for VM sizing.
/// Each technology has a pre-defined set of server roles with recommended configurations.
/// </summary>
public class TechnologyTemplateService : ITechnologyTemplateService
{
    private readonly Dictionary<Technology, TechnologyRoleTemplate> _templates;

    public TechnologyTemplateService()
    {
        _templates = InitializeTemplates();
    }

    public TechnologyRoleTemplate GetTemplate(Technology technology)
    {
        if (_templates.TryGetValue(technology, out var template))
        {
            return template;
        }

        throw new ArgumentException($"No template defined for technology: {technology}", nameof(technology));
    }

    public IEnumerable<TechnologyRoleTemplate> GetAllTemplates()
    {
        return _templates.Values;
    }

    public IEnumerable<TechnologyRoleTemplate> GetTemplatesByPlatformType(PlatformType platformType)
    {
        var isLowCode = platformType == PlatformType.LowCode;
        return _templates.Values.Where(t => t.IsLowCode == isLowCode);
    }

    public VMEnvironmentConfig ApplyTemplate(
        Technology technology,
        EnvironmentType environment,
        bool includeOptionalRoles = false)
    {
        var template = GetTemplate(technology);
        var isProd = environment == EnvironmentType.Prod || environment == EnvironmentType.DR;

        var roles = GetDefaultRoles(technology, isProd, includeOptionalRoles);

        return new VMEnvironmentConfig
        {
            Environment = environment,
            Enabled = true,
            Roles = roles,
            HAPattern = isProd ? HAPattern.ActivePassive : HAPattern.None,
            DRPattern = environment == EnvironmentType.Prod ? DRPattern.WarmStandby : DRPattern.None,
            LoadBalancer = roles.Count(r => r.Role == ServerRole.Web || r.Role == ServerRole.App) > 1
                ? LoadBalancerOption.HAPair
                : LoadBalancerOption.None,
            StorageGB = CalculateDefaultStorage(roles)
        };
    }

    public List<VMRoleConfig> GetDefaultRoles(
        Technology technology,
        bool isProd,
        bool includeOptional = false)
    {
        var template = GetTemplate(technology);

        var rolesToInclude = includeOptional
            ? template.Roles
            : template.Roles.Where(r => r.IsRequired);

        return rolesToInclude
            .Select(r => r.ToRoleConfig(isProd))
            .ToList();
    }

    public bool IsLowCodePlatform(Technology technology)
    {
        return technology == Technology.Mendix || technology == Technology.OutSystems;
    }

    private static int CalculateDefaultStorage(List<VMRoleConfig> roles)
    {
        // Base storage calculation: sum of role disk + 20% overhead
        var roleDisk = roles.Sum(r => r.DiskGB * r.InstanceCount);
        return (int)(roleDisk * 1.2);
    }

    private static Dictionary<Technology, TechnologyRoleTemplate> InitializeTemplates()
    {
        return new Dictionary<Technology, TechnologyRoleTemplate>
        {
            [Technology.DotNet] = CreateDotNetTemplate(),
            [Technology.Java] = CreateJavaTemplate(),
            [Technology.NodeJs] = CreateNodeJsTemplate(),
            [Technology.Python] = CreatePythonTemplate(),
            [Technology.Go] = CreateGoTemplate(),
            [Technology.Mendix] = CreateMendixTemplate(),
            [Technology.OutSystems] = CreateOutSystemsTemplate()
        };
    }

    #region Template Definitions

    private static TechnologyRoleTemplate CreateDotNetTemplate()
    {
        return new TechnologyRoleTemplate
        {
            Technology = Technology.DotNet,
            TemplateName = ".NET Web Application Stack",
            Description = "Standard .NET deployment with IIS/Kestrel web servers, application servers, and SQL Server database",
            Icon = "dotnet",
            IsLowCode = false,
            Roles = new List<VMRoleTemplateItem>
            {
                new()
                {
                    Role = ServerRole.Web,
                    RoleId = "dotnet-web",
                    RoleName = "Web Server (IIS/Kestrel)",
                    Description = "Handles HTTP requests, reverse proxy, static content",
                    Icon = "web",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 80,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.App,
                    RoleId = "dotnet-app",
                    RoleName = "Application Server",
                    Description = "Business logic processing, API endpoints",
                    Icon = "app",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 80,
                    MemoryMultiplier = 1.0,
                    IsRequired = false,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.Database,
                    RoleId = "dotnet-db",
                    RoleName = "Database Server (SQL Server)",
                    Description = "SQL Server database for application data",
                    Icon = "database",
                    DefaultSize = AppTier.Large,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 200,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = false,
                    MaxInstances = 2
                },
                new()
                {
                    Role = ServerRole.Cache,
                    RoleId = "dotnet-cache",
                    RoleName = "Cache Server (Redis)",
                    Description = "In-memory caching for session state and data",
                    Icon = "cache",
                    DefaultSize = AppTier.Small,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 50,
                    MemoryMultiplier = 1.0,
                    IsRequired = false,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.MessageQueue,
                    RoleId = "dotnet-mq",
                    RoleName = "Message Queue (RabbitMQ)",
                    Description = "Asynchronous message processing",
                    Icon = "queue",
                    DefaultSize = AppTier.Small,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 50,
                    MemoryMultiplier = 1.0,
                    IsRequired = false,
                    IsScalable = true
                }
            }
        };
    }

    private static TechnologyRoleTemplate CreateJavaTemplate()
    {
        return new TechnologyRoleTemplate
        {
            Technology = Technology.Java,
            TemplateName = "Java Enterprise Application Stack",
            Description = "Java EE/Spring deployment with Apache/Nginx, Tomcat/WildFly, and PostgreSQL",
            Icon = "java",
            IsLowCode = false,
            Roles = new List<VMRoleTemplateItem>
            {
                new()
                {
                    Role = ServerRole.Web,
                    RoleId = "java-web",
                    RoleName = "Web Server (Apache/Nginx)",
                    Description = "Reverse proxy, load balancing, static content",
                    Icon = "web",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 80,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.App,
                    RoleId = "java-app",
                    RoleName = "Application Server (Tomcat/WildFly)",
                    Description = "Java application container with business logic",
                    Icon = "app",
                    DefaultSize = AppTier.Large,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 100,
                    MemoryMultiplier = 1.5, // Java typically needs more memory
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.Database,
                    RoleId = "java-db",
                    RoleName = "Database Server (PostgreSQL/MySQL)",
                    Description = "Relational database for application data",
                    Icon = "database",
                    DefaultSize = AppTier.Large,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 200,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = false,
                    MaxInstances = 2
                },
                new()
                {
                    Role = ServerRole.Cache,
                    RoleId = "java-cache",
                    RoleName = "Cache Server (Redis/Hazelcast)",
                    Description = "Distributed caching and session management",
                    Icon = "cache",
                    DefaultSize = AppTier.Small,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 50,
                    MemoryMultiplier = 1.0,
                    IsRequired = false,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.MessageQueue,
                    RoleId = "java-mq",
                    RoleName = "Message Queue (Kafka/RabbitMQ)",
                    Description = "Event streaming and async messaging",
                    Icon = "queue",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 100,
                    MemoryMultiplier = 1.0,
                    IsRequired = false,
                    IsScalable = true
                }
            }
        };
    }

    private static TechnologyRoleTemplate CreateNodeJsTemplate()
    {
        return new TechnologyRoleTemplate
        {
            Technology = Technology.NodeJs,
            TemplateName = "Node.js Application Stack",
            Description = "Node.js deployment with Express/Fastify, MongoDB/PostgreSQL, and Redis",
            Icon = "nodejs",
            IsLowCode = false,
            Roles = new List<VMRoleTemplateItem>
            {
                new()
                {
                    Role = ServerRole.App,
                    RoleId = "nodejs-api",
                    RoleName = "API Server (Express/Fastify)",
                    Description = "Node.js application handling API requests",
                    Icon = "api",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 80,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.Database,
                    RoleId = "nodejs-db",
                    RoleName = "Database Server (MongoDB/PostgreSQL)",
                    Description = "Primary data store for application",
                    Icon = "database",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 150,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = false,
                    MaxInstances = 3
                },
                new()
                {
                    Role = ServerRole.Cache,
                    RoleId = "nodejs-cache",
                    RoleName = "Cache Server (Redis)",
                    Description = "Session store, caching, and pub/sub",
                    Icon = "cache",
                    DefaultSize = AppTier.Small,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 50,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.MessageQueue,
                    RoleId = "nodejs-queue",
                    RoleName = "Queue Server (Bull/RabbitMQ)",
                    Description = "Background job processing",
                    Icon = "queue",
                    DefaultSize = AppTier.Small,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 50,
                    MemoryMultiplier = 1.0,
                    IsRequired = false,
                    IsScalable = true
                }
            }
        };
    }

    private static TechnologyRoleTemplate CreatePythonTemplate()
    {
        return new TechnologyRoleTemplate
        {
            Technology = Technology.Python,
            TemplateName = "Python Web Application Stack",
            Description = "Python deployment with Gunicorn/uWSGI, Django/Flask, PostgreSQL, and Celery",
            Icon = "python",
            IsLowCode = false,
            Roles = new List<VMRoleTemplateItem>
            {
                new()
                {
                    Role = ServerRole.Web,
                    RoleId = "python-web",
                    RoleName = "Web Server (Gunicorn/uWSGI)",
                    Description = "WSGI server handling HTTP requests",
                    Icon = "web",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 80,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.App,
                    RoleId = "python-app",
                    RoleName = "Application Server (Django/Flask)",
                    Description = "Python application with business logic",
                    Icon = "app",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 80,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.Database,
                    RoleId = "python-db",
                    RoleName = "Database Server (PostgreSQL)",
                    Description = "PostgreSQL database for application data",
                    Icon = "database",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 150,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = false,
                    MaxInstances = 2
                },
                new()
                {
                    Role = ServerRole.App,
                    RoleId = "python-celery",
                    RoleName = "Celery Workers",
                    Description = "Background task processing workers",
                    Icon = "worker",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 50,
                    MemoryMultiplier = 1.0,
                    IsRequired = false,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.Cache,
                    RoleId = "python-redis",
                    RoleName = "Redis (Broker/Cache)",
                    Description = "Celery broker and application cache",
                    Icon = "cache",
                    DefaultSize = AppTier.Small,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 50,
                    MemoryMultiplier = 1.0,
                    IsRequired = false,
                    IsScalable = true
                }
            }
        };
    }

    private static TechnologyRoleTemplate CreateGoTemplate()
    {
        return new TechnologyRoleTemplate
        {
            Technology = Technology.Go,
            TemplateName = "Go Application Stack",
            Description = "Lightweight Go deployment with compiled binaries and PostgreSQL",
            Icon = "go",
            IsLowCode = false,
            Roles = new List<VMRoleTemplateItem>
            {
                new()
                {
                    Role = ServerRole.App,
                    RoleId = "go-api",
                    RoleName = "API Server",
                    Description = "Compiled Go binary serving API requests",
                    Icon = "api",
                    DefaultSize = AppTier.Small, // Go is efficient
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 50,
                    MemoryMultiplier = 0.8, // Go uses less memory
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.Database,
                    RoleId = "go-db",
                    RoleName = "Database Server (PostgreSQL)",
                    Description = "PostgreSQL database for application data",
                    Icon = "database",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 150,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = false,
                    MaxInstances = 2
                },
                new()
                {
                    Role = ServerRole.Cache,
                    RoleId = "go-cache",
                    RoleName = "Cache Server (Redis)",
                    Description = "Distributed caching layer",
                    Icon = "cache",
                    DefaultSize = AppTier.Small,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 50,
                    MemoryMultiplier = 1.0,
                    IsRequired = false,
                    IsScalable = true
                }
            }
        };
    }

    private static TechnologyRoleTemplate CreateMendixTemplate()
    {
        return new TechnologyRoleTemplate
        {
            Technology = Technology.Mendix,
            TemplateName = "Mendix Low-Code Platform",
            Description = "Mendix runtime deployment with dedicated database and file storage",
            Icon = "mendix",
            IsLowCode = true,
            Roles = new List<VMRoleTemplateItem>
            {
                new()
                {
                    Role = ServerRole.App,
                    RoleId = "mendix-runtime",
                    RoleName = "Mendix Runtime Server",
                    Description = "Mendix application runtime engine",
                    Icon = "mendix",
                    DefaultSize = AppTier.Large,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 100,
                    MemoryMultiplier = 1.5, // Mendix needs more memory
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.Database,
                    RoleId = "mendix-db",
                    RoleName = "Database Server (PostgreSQL)",
                    Description = "PostgreSQL database for Mendix application data",
                    Icon = "database",
                    DefaultSize = AppTier.Large,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 200,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = false,
                    MaxInstances = 1
                },
                new()
                {
                    Role = ServerRole.Storage,
                    RoleId = "mendix-storage",
                    RoleName = "File Storage Server",
                    Description = "Shared file storage for Mendix applications",
                    Icon = "storage",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 500,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = false,
                    MaxInstances = 1
                },
                new()
                {
                    Role = ServerRole.App,
                    RoleId = "mendix-scheduler",
                    RoleName = "Scheduled Events Server",
                    Description = "Handles scheduled microflows and events",
                    Icon = "scheduler",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 0,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 80,
                    MemoryMultiplier = 1.0,
                    IsRequired = false,
                    IsScalable = false,
                    MaxInstances = 1
                }
            }
        };
    }

    private static TechnologyRoleTemplate CreateOutSystemsTemplate()
    {
        return new TechnologyRoleTemplate
        {
            Technology = Technology.OutSystems,
            TemplateName = "OutSystems Enterprise Platform",
            Description = "OutSystems self-managed deployment with full platform components",
            Icon = "outsystems",
            IsLowCode = true,
            Roles = new List<VMRoleTemplateItem>
            {
                new()
                {
                    Role = ServerRole.App,
                    RoleId = "os-controller",
                    RoleName = "Deployment Controller",
                    Description = "OutSystems platform controller for deployments",
                    Icon = "controller",
                    DefaultSize = AppTier.Large,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 150,
                    MemoryMultiplier = 1.2,
                    IsRequired = true,
                    IsScalable = false,
                    MaxInstances = 1
                },
                new()
                {
                    Role = ServerRole.Web,
                    RoleId = "os-frontend",
                    RoleName = "Front-End Server",
                    Description = "OutSystems application front-end servers",
                    Icon = "web",
                    DefaultSize = AppTier.Large,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 2,
                    DefaultDiskGB = 100,
                    MemoryMultiplier = 1.5, // OutSystems needs more memory
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.Database,
                    RoleId = "os-database",
                    RoleName = "Database Server (SQL Server)",
                    Description = "SQL Server database for OutSystems platform and apps",
                    Icon = "database",
                    DefaultSize = AppTier.XLarge,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 500,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = false,
                    MaxInstances = 1
                },
                new()
                {
                    Role = ServerRole.Cache,
                    RoleId = "os-cache",
                    RoleName = "Cache Server (Redis/InProc)",
                    Description = "Session and cache invalidation service",
                    Icon = "cache",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 50,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = true
                },
                new()
                {
                    Role = ServerRole.App,
                    RoleId = "os-scheduler",
                    RoleName = "Scheduler Service",
                    Description = "OutSystems scheduler for timers and BPT",
                    Icon = "scheduler",
                    DefaultSize = AppTier.Medium,
                    DefaultInstancesNonProd = 1,
                    DefaultInstancesProd = 1,
                    DefaultDiskGB = 80,
                    MemoryMultiplier = 1.0,
                    IsRequired = true,
                    IsScalable = false,
                    MaxInstances = 1
                }
            }
        };
    }

    #endregion
}
