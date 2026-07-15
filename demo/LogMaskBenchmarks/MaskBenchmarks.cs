using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LogMask.Core;

namespace LogMaskBenchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class MaskBenchmarks
{
    private CardMaskingProcessor _processor = null!;
    private string?[] _batch = null!;

    [Params(10, 1000, 10000)]
    public int BatchSize;

    [GlobalSetup]
    public void Setup()
    {
        _processor = new CardMaskingProcessor();
        _batch = LogGenerator.Generate(BatchSize, seed: 42);
    }

    [Benchmark]
    public string?[] Process() => _processor.Process(_batch);
}