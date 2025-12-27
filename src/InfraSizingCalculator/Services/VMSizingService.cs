using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service for VM sizing calculations
/// </summary>
public class VMSizingService : IVMSizingService
{
    private readonly ITechnologyService _technologyService;
    private readonly CalculatorSettings _settings;

    public VMSizingService(ITechnologyService technologyService, CalculatorSettings? settings = null)
    {
        _technologyService = technologyService;
        _settings = settings ?? new CalculatorSettings();
    }

    public VMSizingResult Calculate(VMSizingInput input)
    {
        var results = new List<VMEnvironmentResult>();
        var techConfig = _technologyService.GetConfig(input.Technology);

        foreach (var env in input.EnabledEnvironments.OrderBy(e => e))
        {
            if (input.EnvironmentConfigs.TryGetValue(env, out var envConfig) && envConfig.Enabled)
            {
                var envResult = CalculateEnvironment(env, envConfig, input, techConfig);
                results.Add(envResult);
            }
        }

        return new VMSizingResult
        {
            Environments = results,
            GrandTotal = CalculateGrandTotal(results),
            Configuration = input,
            TechnologyName = techConfig.Name,
            CalculatedAt = DateTime.UtcNow
        };
    }

    private VMEnvironmentResult CalculateEnvironment(
        EnvironmentType env,
        VMEnvironmentConfig envConfig,
        VMSizingInput input,
        TechnologyConfig techConfig)
    {
        var isProd = env == EnvironmentType.Prod || env == EnvironmentType.DR;
        var haMultiplier = GetHAMultiplier(envConfig.HAPattern);
        var roleResults = new List<VMRoleResult>();

        foreach (var roleConfig in envConfig.Roles)
        {
            var roleResult = CalculateRole(roleConfig, haMultiplier, techConfig, input.SystemOverheadPercent);
            roleResults.Add(roleResult);
        }

        // Calculate load balancer resources
        var (lbVMs, lbCpu, lbRam) = GetLoadBalancerSpecs(envConfig.LoadBalancer);
        var lbTotalCpu = lbVMs * lbCpu;
        var lbTotalRam = lbVMs * lbRam;

        var totalVMs = roleResults.Sum(r => r.TotalInstances) + lbVMs;
        var totalCpu = roleResults.Sum(r => r.TotalCpu) + lbTotalCpu;
        var totalRam = roleResults.Sum(r => r.TotalRam) + lbTotalRam;
        var totalDisk = roleResults.Sum(r => r.TotalDisk) + envConfig.StorageGB;

        return new VMEnvironmentResult
        {
            Environment = env,
            EnvironmentName = GetEnvironmentName(env),
            IsProd = isProd,
            HAPattern = envConfig.HAPattern,
            DRPattern = envConfig.DRPattern,
            LoadBalancer = envConfig.LoadBalancer,
            Roles = roleResults,
            TotalVMs = totalVMs,
            TotalCpu = totalCpu,
            TotalRam = totalRam,
            TotalDisk = totalDisk,
            LoadBalancerVMs = lbVMs,
            LoadBalancerCpu = lbTotalCpu,
            LoadBalancerRam = lbTotalRam
        };
    }

    private VMRoleResult CalculateRole(
        VMRoleConfig roleConfig,
        double haMultiplier,
        TechnologyConfig techConfig,
        double systemOverheadPercent)
    {
        var (baseCpu, baseRam) = GetRoleSpecs(roleConfig.Role, roleConfig.Size, techConfig.Technology);

        // Apply custom overrides if provided
        var cpuPerInstance = roleConfig.CustomCpu ?? baseCpu;
        var ramPerInstance = roleConfig.CustomRam ?? baseRam;

        // Apply system overhead
        var overheadMultiplier = 1 + (systemOverheadPercent / 100);
        cpuPerInstance = (int)Math.Ceiling(cpuPerInstance * overheadMultiplier);
        ramPerInstance = (int)Math.Ceiling(ramPerInstance * overheadMultiplier);

        // Calculate total instances with HA
        var baseInstances = roleConfig.InstanceCount;
        var totalInstances = (int)Math.Ceiling(baseInstances * haMultiplier);

        return new VMRoleResult
        {
            Role = roleConfig.Role,
            RoleName = roleConfig.RoleName ?? GetRoleName(roleConfig.Role),
            Size = roleConfig.Size,
            BaseInstances = baseInstances,
            TotalInstances = totalInstances,
            CpuPerInstance = cpuPerInstance,
            RamPerInstance = ramPerInstance,
            DiskPerInstance = roleConfig.DiskGB,
            TotalCpu = totalInstances * cpuPerInstance,
            TotalRam = totalInstances * ramPerInstance,
            TotalDisk = totalInstances * roleConfig.DiskGB
        };
    }

    public (int Cpu, int Ram) GetRoleSpecs(ServerRole role, AppTier size, Technology technology)
    {
        var baseSpecs = _settings.VMRoles.GetSpecs(role, size);

        // Java, Mendix, and OutSystems need more memory
        if (technology == Technology.Java || technology == Technology.Mendix || technology == Technology.OutSystems)
        {
            return (baseSpecs.Cpu, (int)(baseSpecs.Ram * _settings.VMRoles.HighMemoryMultiplier));
        }

        return baseSpecs;
    }

    public double GetHAMultiplier(HAPattern pattern)
    {
        return _settings.HAPatterns.GetMultiplier(pattern);
    }

    public (int VMs, int CpuPerVM, int RamPerVM) GetLoadBalancerSpecs(LoadBalancerOption option)
    {
        return _settings.LoadBalancers.GetSpecs(option);
    }

    private VMGrandTotal CalculateGrandTotal(List<VMEnvironmentResult> results)
    {
        return new VMGrandTotal
        {
            TotalVMs = results.Sum(r => r.TotalVMs),
            TotalCpu = results.Sum(r => r.TotalCpu),
            TotalRam = results.Sum(r => r.TotalRam),
            TotalDisk = results.Sum(r => r.TotalDisk),
            TotalLoadBalancerVMs = results.Sum(r => r.LoadBalancerVMs)
        };
    }

    private string GetEnvironmentName(EnvironmentType env)
    {
        return env switch
        {
            EnvironmentType.Dev => "Development",
            EnvironmentType.Test => "Test",
            EnvironmentType.Stage => "Staging",
            EnvironmentType.Prod => "Production",
            EnvironmentType.DR => "Disaster Recovery",
            _ => env.ToString()
        };
    }

    private string GetRoleName(ServerRole role)
    {
        return role switch
        {
            ServerRole.Web => "Web Server",
            ServerRole.App => "Application Server",
            ServerRole.Database => "Database Server",
            ServerRole.Cache => "Cache Server",
            ServerRole.MessageQueue => "Message Queue",
            ServerRole.Search => "Search Server",
            ServerRole.Storage => "Storage Server",
            ServerRole.Monitoring => "Monitoring Server",
            ServerRole.Bastion => "Bastion Host",
            _ => role.ToString()
        };
    }
}
