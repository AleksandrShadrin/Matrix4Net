using BenchmarkDotNet.Attributes;
using Matrix4Net.Utility;
using Matrix4Net.ValueObjects;

namespace Matrix4Net.Benchmarks.Utitlities.Benchmarks
{
  public class LUBenchmark
  {
    private Matrix a;

    [Params(500)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
      var data = new double[N * N];
      Array.Fill(data, 0);

      for (int i = 0; i < N * N; i++)
      {
        data[i] = Random.Shared.NextDouble();
      }

      a = Matrix.Build(data, N, N).First();
    }

    [Benchmark]
    public void QR()
      => Linalg.QR(a);

    [Benchmark]
    public void PLU()
      => Linalg.PLU(a);

    //[Benchmark]
    //public void HouseHolder()
    //  => Linalg.HouseholderTransform(a);

    [Benchmark]
    public void DoolittleLU()
      => Linalg.DoolittleLU(a);
  }
}
