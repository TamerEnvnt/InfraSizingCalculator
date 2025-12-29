```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.1 (25B78) [Darwin 25.1.0]
Apple M5, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.101
  [Host]   : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  ShortRun : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                             | Mean      | Error     | StdDev    | Rank | Allocated |
|----------------------------------- |----------:|----------:|----------:|-----:|----------:|
| &#39;Distribution: Get config by enum&#39; | 0.7981 ns | 0.3379 ns | 0.0185 ns |    1 |         - |
