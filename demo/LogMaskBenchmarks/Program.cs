using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;

namespace LogMaskBenchmarks;

public static class Program
{
    //public static void Main() => BenchmarkRunner.Run<MaskBenchmarks>();

    public static void Main() =>
        BenchmarkRunner.Run<MaskBenchmarks>(
            DefaultConfig.Instance.WithSummaryStyle(
                SummaryStyle.Default.WithTimeUnit(TimeUnit.Second)));
    //static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
    
}

