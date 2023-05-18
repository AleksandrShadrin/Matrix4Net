using BenchmarkDotNet.Attributes;
using Matrix4Net.ValueObjects;

namespace Matrix4Net.Benchmarks.MatrixOperations.Benchmarks
{
  public class GetBenchmark
  {
    private Matrix a;

    [Params(1000, 10_000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
      var data = Enumerable.Range(0, N * N).Map(x => Random.Shared.NextDouble()).ToArray();

      a = Matrix.Build(data, N, N).First();
    }

    [Benchmark]
    public void GetRows()
      => a.GetRow(Random.Shared.Next(0, N - 1));

    [Benchmark]
    public void GetColumns()
      => a.GetColumn(Random.Shared.Next(0, N - 1));
  }
}
