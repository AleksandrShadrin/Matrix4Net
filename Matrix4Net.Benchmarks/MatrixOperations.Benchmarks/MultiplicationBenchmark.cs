using BenchmarkDotNet.Attributes;
using LanguageExt;
using Matrix4Net.Utility;
using Matrix4Net.ValueObjects;

namespace Matrix4Net.Benchmarks.MatrixOperations.Benchmarks
{
  public class MultiplicationBenchmark
  {
    private Matrix a;
    private Matrix b;

    [Params(100, 1000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
      var data = Enumerable.Range(0, N * N).Map(x => Random.Shared.NextDouble()).ToArray();

      a = Matrix.Build(data, N, N).First();
      b = Matrix.Build(data, N, N).First();
    }

    [Benchmark]
    public Option<Matrix> Operation()
      => a * b;
  }
}
