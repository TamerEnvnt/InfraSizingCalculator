namespace InfraSizingCalculator.Models.Enums;

/// <summary>
/// Load balancer options for VM deployments
/// </summary>
public enum LoadBalancerOption
{
    /// <summary>No load balancer - Direct access to VMs</summary>
    None,

    /// <summary>Single load balancer - Non-redundant</summary>
    Single,

    /// <summary>HA Pair - Two load balancers in active-passive configuration</summary>
    HAPair,

    /// <summary>Cloud Load Balancer - Managed cloud service (AWS ALB/NLB, Azure LB, etc.)</summary>
    CloudLB
}
