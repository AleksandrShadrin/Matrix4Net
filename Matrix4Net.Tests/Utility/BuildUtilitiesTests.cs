using FluentAssertions;
using Matrix4Net.Utility;

namespace Matrix4Net.Tests.Utility
{
  public class BuildUtilitiesTests
  {
    [Theory]
    [InlineData(0, -1)]
    [InlineData(-4, -1)]
    [InlineData(-4, 0)]
    [InlineData(4, 0)]
    [InlineData(0, 0)]
    [InlineData(0, 3)]
    public void WhenSizeOfMatrixNotPositiveOrZeroBuildFilledMatrixShouldReturnNone(int n, int m)
    {
      // ARRANGE
      // ACT
      var res = BuildUtilities.BuildFilledMatrix(n, m, 1);

      // ASSERT
      res.IsNone.Should().BeTrue();
    }

    [Fact]
    public void WhenSizeIsRightBuildFilledMatrixShouldReturnMatrixWithSameValues()
    {
      // ARRANGE
      int n = 10;
      int m = 5;

      // ACT
      var res = BuildUtilities.BuildFilledMatrix(n, m, 1);

      // ASSERT
      res.First()
        .Should().HaveCount(n)
        .And
        .AllSatisfy
        (
          (r) => r
            .Should()
            .AllSatisfy(v => v.Should().Be(1))
            .And.HaveCount(m)
        );
    }

    [Fact]
    public void WhenSizeOfMatrixNotPositiveBuildDiagonalShouldReturnNone()
    {
      // ARRANGE
      var n = -1;

      // ACT
      var res = BuildUtilities.BuildDiagMatrix(n, 1);

      // ASSERT
      res.IsNone.Should().BeTrue();
    }

    [Fact]
    public void WhenSizeOfMatrixIsZeroBuildDiagonalShouldReturnNone()
    {
      // ARRANGE
      var n = 0;

      // ACT
      var res = BuildUtilities.BuildDiagMatrix(n, 1);

      // ASSERT
      res.IsNone.Should().BeTrue();
    }

    [Fact]
    public void DiagMatrixElementsShouldBeCorrect()
    {
      // ARRANGE
      var n = 10;

      // ACT
      var res = BuildUtilities.BuildDiagMatrix(n, 1);

      // ASSERT
      res.First()
        .Iter
          (
            (idx, r) =>
            {
              r.Filter(v => v is not 0).Count().Should().Be(1);
              r[idx].Should().Be(1);
            }
          );
    }
  }
}
