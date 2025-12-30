using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using InfraSizingCalculator.Data;

namespace InfraSizingCalculator.Services.HealthChecks;

/// <summary>
/// Health check for the SQLite database connection.
/// Verifies database connectivity and basic query capability.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly InfraSizingDbContext _dbContext;

    public DatabaseHealthCheck(InfraSizingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Test database connectivity
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy(
                    "Cannot connect to database",
                    data: new Dictionary<string, object>
                    {
                        { "database_provider", "SQLite" }
                    });
            }

            // Get database file info if available
            var connectionString = _dbContext.Database.GetConnectionString();
            var data = new Dictionary<string, object>
            {
                { "database_provider", "SQLite" },
                { "can_connect", true }
            };

            // Try to count records in key tables
            try
            {
                var settingsCount = await _dbContext.ApplicationSettings.CountAsync(cancellationToken);
                var pricingCount = await _dbContext.OnPremPricing.CountAsync(cancellationToken);

                data["application_settings_count"] = settingsCount;
                data["on_prem_pricing_count"] = pricingCount;
            }
            catch
            {
                // Table counts are optional - connection test is sufficient
            }

            return HealthCheckResult.Healthy(
                "Database connection successful",
                data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message }
                });
        }
    }
}
