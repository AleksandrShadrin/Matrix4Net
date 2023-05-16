using FluentAssertions;
using Matrix4Net.Utility;
using Matrix4Net.ValueObjects;

namespace Matrix4Net.Tests.Utility
{
  public class LinalgTests
  {
    [Fact]
    public void LUMethodShouldWorkCorrect()
    {
      // ARRANGE
      var data = new double[][]
      {
        new double[] { 1, 1, 2},
        new double[] { 1, 2, 2},
        new double[] { 1, 2, 3},
      };

      var matrix = Matrix.Build(data).First();
      var ldata = new double[][]
      {
        new double[] { 1, 0, 0},
        new double[] { 1, 1, 0},
        new double[] { 1, 1, 1},
      };

      var udata = new double[][]
      {
        new double[] { 1, 1, 2},
        new double[] { 0, 1, 0},
        new double[] { 0, 0, 1},
      };

      var lmatrix = Matrix.Build(ldata).First();
      var umatrix = Matrix.Build(udata).First();

      // ACT
      var (l, u) = Linalg.LU(matrix).First();

      // ASSERT
      umatrix.Equals(u).Should().BeTrue();
      lmatrix.Equals(l).Should().BeTrue();
    }

    [Fact]
    public void DeterminantMethodShouldWorkCorrect()
    {
      // ARRANGE
      var data = new double[][]
      {
        new double[] { 1, 2, 3, 4},
        new double[] { 5, 6, 7, 8},
        new double[] { 9, 0, 1, 2},
        new double[] { 3, 4, 5, 6},
      };

      var matrix = Matrix.Build(data).First();

      // ACT
      var det = Linalg.MatrixDeterminant(matrix).First();

      // ASSERT
      det.Should().Be(0);
    }
  }
}
