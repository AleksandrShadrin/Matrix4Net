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
      var (l, u) = Linalg.DoolittleLU(matrix).First();

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
        new double[] { 4, 1, -2, 2},
        new double[] { 1, 2, 0, 1},
        new double[] { -2, 0, 3, -2},
        new double[] { 2, 1, -2, -1},
      };
      
      var matrix = Matrix.Build(data).First();

      // ACT
      var det = Linalg.MatrixDeterminantByPLU(matrix).First();

      // ASSERT
      det.Should().BeApproximately(-37, 6);
    }
  }
}
