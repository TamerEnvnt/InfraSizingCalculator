using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using InfraSizingCalculator.Benchmarks;

// Run all benchmarks
// Usage: dotnet run -c Release
// For quick test: dotnet run -c Release -- --job short

var config = DefaultConfig.Instance
    .WithOptions(ConfigOptions.DisableOptimizationsValidator);

BenchmarkRunner.Run<SizingBenchmarks>(config, args);
