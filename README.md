LogMask

A high-throughput log processing module. It masks credit card numbers and appends a processing date. The method takes an array of strings as input, where each string is a single, separate log. The service runs on a single thread and is designed to sustain a throughput of 10,000 RPS.

Running the benchmark

Requires the .NET 10 SDK. Run in Release mode:

bashdotnet run -c Release --project demo/LogMaskBenchmarks


| Method  | BatchSize | Mean     | Error    | StdDev   | Median   | Gen0     | Gen1     | Allocated |
|-------- |---------- |---------:|---------:|---------:|---------:|---------:|---------:|----------:|
| Process | 10        | 0.0000 s | 0.0000 s | 0.0000 s | 0.0000 s |   0.3586 |   0.0019 |    2.2 KB |
| Process | 1000      | 0.0002 s | 0.0000 s | 0.0000 s | 0.0002 s |  40.0391 |   0.2441 | 245.45 KB |
| Process | 10000     | 0.0026 s | 0.0000 s | 0.0001 s | 0.0026 s | 398.4375 | 199.2188 | 2455.2 KB |
