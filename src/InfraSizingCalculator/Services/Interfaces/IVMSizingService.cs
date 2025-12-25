using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Service for VM sizing calculations
/// </summary>
public interface IVMSizingService
{
    /// <summary>
    /// Calculate VM sizing based on input parameters
    /// </summary>
    VMSizingResult Calculate(VMSizingInput input);

    /// <summary>
    /// Get default VM specs for a role and size tier
    /// </summary>
    (int Cpu, int Ram) GetRoleSpecs(ServerRole role, AppTier size, Technology technology);

    /// <summary>
    /// Get HA multiplier for a given HA pattern
    /// </summary>
    double GetHAMultiplier(HAPattern pattern);

    /// <summary>
    /// Get load balancer specs
    /// </summary>
    (int VMs, int CpuPerVM, int RamPerVM) GetLoadBalancerSpecs(LoadBalancerOption option);
}
