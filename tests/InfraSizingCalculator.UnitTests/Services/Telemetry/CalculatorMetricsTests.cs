using System.Diagnostics.Metrics;
using FluentAssertions;
using InfraSizingCalculator.Services.Telemetry;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services.Telemetry;

public class CalculatorMetricsTests : IDisposable
{
    private readonly IMeterFactory _meterFactory;
    private readonly CalculatorMetrics _metrics;
    private readonly MeterListener _meterListener;
    private readonly List<(string Name, object? Value, KeyValuePair<string, object?>[] Tags)> _recordedMeasurements;

    public CalculatorMetricsTests()
    {
        _meterFactory = new TestMeterFactory();
        _metrics = new CalculatorMetrics(_meterFactory);
        _recordedMeasurements = new List<(string Name, object? Value, KeyValuePair<string, object?>[] Tags)>();

        // Set up listener to capture measurements
        _meterListener = new MeterListener();
        _meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == CalculatorMetrics.MeterName)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        _meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            _recordedMeasurements.Add((instrument.Name, measurement, tags.ToArray()));
        });

        _meterListener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
        {
            _recordedMeasurements.Add((instrument.Name, measurement, tags.ToArray()));
        });

        _meterListener.SetMeasurementEventCallback<int>((instrument, measurement, tags, state) =>
        {
            _recordedMeasurements.Add((instrument.Name, measurement, tags.ToArray()));
        });

        _meterListener.Start();
    }

    public void Dispose()
    {
        _meterListener.Dispose();
    }

    [Fact]
    public void MeterName_IsCorrect()
    {
        // Assert
        CalculatorMetrics.MeterName.Should().Be("InfraSizingCalculator");
    }

    [Fact]
    public void ActivitySource_IsAvailable()
    {
        // Assert
        CalculatorMetrics.ActivitySource.Should().NotBeNull();
        CalculatorMetrics.ActivitySource.Name.Should().Be(CalculatorMetrics.MeterName);
    }

    [Theory]
    [InlineData("k8s")]
    [InlineData("vm")]
    public void RecordCalculation_RecordsCounterAndHistogram(string deploymentType)
    {
        // Act
        _metrics.RecordCalculation(deploymentType, 100.5);

        // Assert - verify both counter and histogram are recorded
        _recordedMeasurements.Should().Contain(m => m.Name == "infrasizing_calculations_total");
        _recordedMeasurements.Should().Contain(m => m.Name == "infrasizing_calculation_duration_ms");
    }

    [Theory]
    [InlineData("xlsx")]
    [InlineData("csv")]
    [InlineData("pdf")]
    public void RecordExport_RecordsCounter(string format)
    {
        // Act
        _metrics.RecordExport(format);

        // Assert
        _recordedMeasurements.Should().Contain(m => m.Name == "infrasizing_exports_total");
    }

    [Fact]
    public void RecordExport_WithDuration_RecordsHistogram()
    {
        // Act
        _metrics.RecordExport("xlsx", 250.0);

        // Assert
        _recordedMeasurements.Should().Contain(m => m.Name == "infrasizing_exports_total");
        _recordedMeasurements.Should().Contain(m => m.Name == "infrasizing_export_duration_ms");
    }

    [Fact]
    public void RecordExport_WithoutDuration_DoesNotRecordHistogram()
    {
        // Act
        _metrics.RecordExport("csv");

        // Assert
        _recordedMeasurements.Should().Contain(m => m.Name == "infrasizing_exports_total");
        _recordedMeasurements.Should().NotContain(m => m.Name == "infrasizing_export_duration_ms");
    }

    [Fact]
    public void SessionStarted_IncreasesCounter()
    {
        // Act
        _metrics.SessionStarted();

        // Assert
        _recordedMeasurements.Should().Contain(m =>
            m.Name == "infrasizing_sessions_active" && (int)m.Value! == 1);
    }

    [Fact]
    public void SessionEnded_DecreasesCounter()
    {
        // Act
        _metrics.SessionEnded();

        // Assert
        _recordedMeasurements.Should().Contain(m =>
            m.Name == "infrasizing_sessions_active" && (int)m.Value! == -1);
    }

    [Theory]
    [InlineData("save")]
    [InlineData("load")]
    [InlineData("compare")]
    [InlineData("delete")]
    public void RecordScenarioOperation_RecordsCounter(string operation)
    {
        // Act
        _metrics.RecordScenarioOperation(operation);

        // Assert
        _recordedMeasurements.Should().Contain(m => m.Name == "infrasizing_scenario_operations_total");
    }

    [Theory]
    [InlineData("openshift")]
    [InlineData("k3s")]
    [InlineData("aks")]
    [InlineData("eks")]
    public void RecordDistributionUsage_RecordsCounter(string distribution)
    {
        // Act
        _metrics.RecordDistributionUsage(distribution);

        // Assert
        _recordedMeasurements.Should().Contain(m => m.Name == "infrasizing_distribution_usage_total");
    }

    [Fact]
    public void Constructor_CreatesAllInstruments()
    {
        // Record all types of metrics
        _metrics.RecordCalculation("k8s", 100);
        _metrics.RecordExport("xlsx", 50);
        _metrics.SessionStarted();
        _metrics.SessionEnded();
        _metrics.RecordScenarioOperation("save");
        _metrics.RecordDistributionUsage("openshift");

        // Assert all instrument names are present
        var instrumentNames = _recordedMeasurements.Select(m => m.Name).Distinct().ToList();
        instrumentNames.Should().Contain("infrasizing_calculations_total");
        instrumentNames.Should().Contain("infrasizing_calculation_duration_ms");
        instrumentNames.Should().Contain("infrasizing_exports_total");
        instrumentNames.Should().Contain("infrasizing_export_duration_ms");
        instrumentNames.Should().Contain("infrasizing_sessions_active");
        instrumentNames.Should().Contain("infrasizing_scenario_operations_total");
        instrumentNames.Should().Contain("infrasizing_distribution_usage_total");
    }

    [Fact]
    public void MultipleMeasurements_AllRecorded()
    {
        // Act - perform multiple operations
        _metrics.RecordCalculation("k8s", 100);
        _metrics.RecordCalculation("vm", 150);
        _metrics.RecordCalculation("k8s", 200);

        // Assert
        var calculationMeasurements = _recordedMeasurements
            .Where(m => m.Name == "infrasizing_calculations_total")
            .ToList();

        calculationMeasurements.Should().HaveCount(3);
    }
}

/// <summary>
/// Simple test meter factory implementation
/// </summary>
public class TestMeterFactory : IMeterFactory
{
    private readonly List<Meter> _meters = new();

    public Meter Create(MeterOptions options)
    {
        var meter = new Meter(options);
        _meters.Add(meter);
        return meter;
    }

    public void Dispose()
    {
        foreach (var meter in _meters)
        {
            meter.Dispose();
        }
        _meters.Clear();
    }
}
