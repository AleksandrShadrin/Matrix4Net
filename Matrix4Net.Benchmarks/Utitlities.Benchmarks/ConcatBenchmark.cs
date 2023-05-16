using BenchmarkDotNet.Attributes;
using LanguageExt;
using Matrix4Net.Utility;
using Matrix4Net.ValueObjects;

namespace Matrix4Net.Benchmarks.Utitlities.Benchmarks
{
  public class ConcatBenchmark
  {
    private Matrix a;
    private Matrix b;

    [Params(1000, 10_000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
      var data = new double[N * N];
      Array.Fill(data, 0);

      a = Matrix.Build(data, N, N).First();
      b = Matrix.Build(data, N, N).First();
    }

    [Benchmark]
    public Option<Matrix> ConcatRows()
      => BuildUtilities
        .ConcatMatrix(a, b, Constants.ConcatenateMode.CONCATENATE_ROWS);

    [Benchmark]
    public Option<Matrix> ConcatColumns()
      => BuildUtilities
        .ConcatMatrix(a, b, Constants.ConcatenateMode.CONCATENATE_COLUMNS);

  }
}
