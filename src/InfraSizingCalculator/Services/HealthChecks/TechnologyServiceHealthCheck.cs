using Microsoft.Extensions.Diagnostics.HealthChecks;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services.HealthChecks;

/// <summary>
/// Health check for the Technology Service.
/// Verifies that technologies are available and the service is functional.
/// </summary>
public class TechnologyServiceHealthCheck : IHealthCheck
{
    private readonly ITechnologyService _technologyService;
    private const int MinimumExpectedTechnologies = 5; // We have 7 technologies

    public TechnologyServiceHealthCheck(ITechnologyService technologyService)
    {
        _technologyService = technologyService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var technologies = _technologyService.GetAll().ToList();
            var count = technologies.Count;

            if (count == 0)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "No technologies available",
                    data: new Dictionary<string, object>
                    {
                        { "technology_count", 0 }
                    }));
            }

            if (count < MinimumExpectedTechnologies)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Only {count} technologies available, expected at least {MinimumExpectedTechnologies}",
                    data: new Dictionary<string, object>
                    {
                        { "technology_count", count },
                        { "minimum_expected", MinimumExpectedTechnologies }
                    }));
            }

            // Verify we can get configurations for technologies
            var sampleConfig = technologies.First();
            var config = _technologyService.GetConfig(sampleConfig.Technology);

            if (config == null)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    "Technology configuration retrieval failed",
                    data: new Dictionary<string, object>
                    {
                        { "technology_count", count },
                        { "sample_technology", sampleConfig.Technology.ToString() }
                    }));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"{count} technologies available and functional",
                data: new Dictionary<string, object>
                {
                    { "technology_count", count },
                    { "sample_check_passed", true }
                }));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Technology service check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message }
                }));
        }
    }
}
