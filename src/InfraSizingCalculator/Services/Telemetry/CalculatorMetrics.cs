using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace InfraSizingCalculator.Services.Telemetry;

/// <summary>
/// Custom metrics for the Infrastructure Sizing Calculator.
/// Exposes counters and histograms for Prometheus scraping.
/// </summary>
public class CalculatorMetrics
{
    // Meter name must match the name registered in OpenTelemetry configuration
    public const string MeterName = "InfraSizingCalculator";

    private readonly Counter<long> _calculationsPerformed;
    private readonly Histogram<double> _calculationDuration;
    private readonly Counter<long> _exportsByFormat;
    private readonly UpDownCounter<int> _activeSessions;
    private readonly Counter<long> _scenarioOperations;
    private readonly Counter<long> _distributionUsage;
    private readonly Histogram<double> _exportDuration;

    public CalculatorMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _calculationsPerformed = meter.CreateCounter<long>(
            "infrasizing_calculations_total",
            description: "Total number of sizing calculations performed");

        _calculationDuration = meter.CreateHistogram<double>(
            "infrasizing_calculation_duration_ms",
            unit: "ms",
            description: "Duration of sizing calculations in milliseconds");

        _exportsByFormat = meter.CreateCounter<long>(
            "infrasizing_exports_total",
            description: "Total exports by format");

        _activeSessions = meter.CreateUpDownCounter<int>(
            "infrasizing_sessions_active",
            description: "Number of active Blazor sessions");

        _scenarioOperations = meter.CreateCounter<long>(
            "infrasizing_scenario_operations_total",
            description: "Scenario operations (save, load, compare, delete)");

        _distributionUsage = meter.CreateCounter<long>(
            "infrasizing_distribution_usage_total",
            description: "Usage count per Kubernetes distribution");

        _exportDuration = meter.CreateHistogram<double>(
            "infrasizing_export_duration_ms",
            unit: "ms",
            description: "Duration of export operations in milliseconds");
    }

    /// <summary>
    /// Records a sizing calculation with deployment type and duration.
    /// </summary>
    /// <param name="deploymentType">k8s or vm</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    public void RecordCalculation(string deploymentType, double durationMs)
    {
        _calculationsPerformed.Add(1,
            new KeyValuePair<string, object?>("deployment_type", deploymentType));
        _calculationDuration.Record(durationMs,
            new KeyValuePair<string, object?>("deployment_type", deploymentType));
    }

    /// <summary>
    /// Records an export operation with format.
    /// </summary>
    /// <param name="format">Export format (xlsx, csv, pdf)</param>
    /// <param name="durationMs">Optional duration in milliseconds</param>
    public void RecordExport(string format, double? durationMs = null)
    {
        _exportsByFormat.Add(1, new KeyValuePair<string, object?>("format", format));

        if (durationMs.HasValue)
        {
            _exportDuration.Record(durationMs.Value,
                new KeyValuePair<string, object?>("format", format));
        }
    }

    /// <summary>
    /// Records when a Blazor session starts.
    /// </summary>
    public void SessionStarted() => _activeSessions.Add(1);

    /// <summary>
    /// Records when a Blazor session ends.
    /// </summary>
    public void SessionEnded() => _activeSessions.Add(-1);

    /// <summary>
    /// Records a scenario operation.
    /// </summary>
    /// <param name="operation">Operation type (save, load, compare, delete)</param>
    public void RecordScenarioOperation(string operation) =>
        _scenarioOperations.Add(1, new KeyValuePair<string, object?>("operation", operation));

    /// <summary>
    /// Records usage of a Kubernetes distribution.
    /// </summary>
    /// <param name="distribution">Distribution name</param>
    public void RecordDistributionUsage(string distribution) =>
        _distributionUsage.Add(1, new KeyValuePair<string, object?>("distribution", distribution));

    /// <summary>
    /// Creates an activity source for distributed tracing.
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new(MeterName);
}
