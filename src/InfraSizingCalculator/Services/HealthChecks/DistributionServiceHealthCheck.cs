using Microsoft.Extensions.Diagnostics.HealthChecks;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services.HealthChecks;

/// <summary>
/// Health check for the Distribution Service.
/// Verifies that distributions are available and the service is functional.
/// </summary>
public class DistributionServiceHealthCheck : IHealthCheck
{
    private readonly IDistributionService _distributionService;
    private const int MinimumExpectedDistributions = 30; // Service filters distributions (34 active out of 46 total)

    public DistributionServiceHealthCheck(IDistributionService distributionService)
    {
        _distributionService = distributionService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var distributions = _distributionService.GetAll().ToList();
            var count = distributions.Count;

            if (count == 0)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "No distributions available",
                    data: new Dictionary<string, object>
                    {
                        { "distribution_count", 0 }
                    }));
            }

            if (count < MinimumExpectedDistributions)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Only {count} distributions available, expected at least {MinimumExpectedDistributions}",
                    data: new Dictionary<string, object>
                    {
                        { "distribution_count", count },
                        { "minimum_expected", MinimumExpectedDistributions }
                    }));
            }

            // Verify we can get configurations for distributions
            var sampleConfig = distributions.First();
            var config = _distributionService.GetConfig(sampleConfig.Distribution);

            if (config == null)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    "Distribution configuration retrieval failed",
                    data: new Dictionary<string, object>
                    {
                        { "distribution_count", count },
                        { "sample_distribution", sampleConfig.Distribution.ToString() }
                    }));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"{count} distributions available and functional",
                data: new Dictionary<string, object>
                {
                    { "distribution_count", count },
                    { "sample_check_passed", true }
                }));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Distribution service check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message }
                }));
        }
    }
}
