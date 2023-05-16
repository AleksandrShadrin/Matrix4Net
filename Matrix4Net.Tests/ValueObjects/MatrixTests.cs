using FluentAssertions;
using LanguageExt;
using Matrix4Net.ValueObjects;

namespace Matrix4Net.Tests.ValueObjects
{
  public class MatrixTests
  {
    [Theory]
    [InlineData(new double[] { 1, 2, 3, 4 }, 2, 3)]
    [InlineData(new double[] { 1 }, 1, 0)]
    [InlineData(new double[] { }, 2, 0)]
    public void WhenDataLengthNotEqualToMatrixSizeShouldBeNone
        (
        double[] data,
        int rowCount,
        int columnCount
        )
    {
      // ARRANGE
      // ACT
      Matrix.Build(data, rowCount, columnCount)
          // ASSERT
          .Should<Option<Matrix>>()
          .Be(Option<Matrix>.None);
    }

    [Theory]
    [InlineData(new double[] { 1, 2, 3, 4 }, 2, 2)]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 3, 2)]
    [InlineData(new double[] { 1, 2, 3, 4 }, 1, 4)]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 2, 4)]
    public void WhenDataLengthEqualToMatrixSizeShouldBeSome
        (
        double[] data,
        int rowCount,
        int columnCount
        )
    {
      // ARRANGE
      // ACT
      var matrix = Matrix
          .Build(data, rowCount, columnCount);

      // ASSERT
      matrix.IsSome.Should().BeTrue();
    }

    [Theory]
    [InlineData(new double[] { 1, 2, 3, 4 }, 2, 2)]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 3, 2)]
    [InlineData(new double[] { 1, 2, 3, 4 }, 1, 4)]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 2, 4)]
    public void WhenMatrixesAreEqualEqualsShouldReturnTrue
        (
        double[] data,
        int rowCount,
        int columnCount
        )
    {
      // ARRANGE
      var matrix1 = Matrix
          .Build(data, rowCount, columnCount)
          .First();

      var matrix2 = Matrix
          .Build(data, rowCount, columnCount)
          .First();

      // ACT
      var res = matrix1.Equals(matrix2);

      // ASSERT
      res.Should().BeTrue();
    }

    public static IEnumerable<object[]> GetDataForBuildFromRowDataShouldBeCorrect()
    {
      yield return new object[]
          {
                    new double[] { 1, 2, 3, 4 },
                    2, 2,
                    new double[][]
                        {
                            new double[] { 1, 2 },
                            new double[] { 3, 4 }
                        }
          };

      yield return new object[]
          {
                    new double[] { 1, 2, 3, 4, 5, 6 },
                    3, 2,
                    new double[][]
                        {
                            new double[] { 1, 2 },
                            new double[] { 3, 4 },
                            new double[] { 5, 6 },
                        }
          };

      yield return new object[]
          {
                    new double[] { 1 },
                    1, 1,
                    new double[][]
                        {
                            new double[] { 1 },
                        }
          };
    }

    [Theory]
    [MemberData(nameof(GetDataForBuildFromRowDataShouldBeCorrect))]
    public void BuildFromRowDataShouldBeCorrect
        (
        double[] data,
        int rowCount,
        int columnCount,
        double[][] shouldBe
        )
    {
      // ARRANGE
      // ACT
      var matrix = Matrix
          .Build(data, rowCount, columnCount)
          .First();

      // ASSERT
      matrix.Zip(shouldBe)
          .Iter
              (
                  zipped =>
                  zipped.Item1.Should().BeEquivalentTo(zipped.Item2)
              );
    }

    [Theory]
    [InlineData(new double[] { 1, 2, 3, 4 }, 2, 2, 3, new double[] { 4, 5, 6, 7 })]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 3, 2, 0, new double[] { 1, 2, 3, 4, 5, 6 })]
    [InlineData(new double[] { 1, 2, 3, 4 }, 1, 4, -5, new double[] { -4, -3, -2, -1 })]
    public void SumBySingleValueShouldBeCorrect
        (
        double[] data,
        int rowCount,
        int columnCount,
        int sumBy,
        double[] shouldBe
        )
    {
      // ARRANGE
      var matrix = Matrix
          .Build(data, rowCount, columnCount)
          .First();

      var matrixShouldBe = Matrix
          .Build(shouldBe, rowCount, columnCount)
          .First();

      // ACT
      var result = matrix + sumBy;


      // ASSERT
      result.First()
          .Equals(matrixShouldBe)
          .Should().BeTrue();
    }

    [Theory]
    [InlineData(new double[] { 1, 2, 3, 4 }, 2, 2, 1, new double[] { 2, 4 })]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 3, 2, 0, new double[] { 1, 3, 5 })]
    [InlineData(new double[] { 1, 2, 3, 4 }, 1, 4, 2, new double[] { 3 })]
    public void GetColumnShouldReturnExactColumn
        (
        double[] data,
        int rowCount,
        int columnCount,
        int columnIdx,
        double[] shouldBe
        )
    {
      // ARRANGE
      var matrix = Matrix
          .Build(data, rowCount, columnCount)
          .First();

      // ACT
      var result = matrix.GetColumn(columnIdx);


      // ASSERT
      result
          .IsRight
          .Should().BeTrue();

      result
          .IfRight((arr) => arr.Should().BeEquivalentTo(shouldBe));
    }

    [Theory]
    [InlineData(new double[] { 1, 2, 3, 4 }, 2, 2, 2)]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 3, 2, -1)]
    [InlineData(new double[] { 1, 2, 3, 4 }, 1, 4, 10)]
    public void GetColumnShouldReturnErrorWhenIndexOutOfRange
        (
        double[] data,
        int rowCount,
        int columnCount,
        int columnIdx
        )
    {
      // ARRANGE
      var matrix = Matrix
          .Build(data, rowCount, columnCount)
          .First();

      // ACT
      var result = matrix.GetColumn(columnIdx);

      // ASSERT
      result
          .IsRight
          .Should().BeFalse();
    }

    [Theory]
    [InlineData(new double[] { 1, 2, 3, 4 }, 2, 2, 1, new double[] { 3, 4 })]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 3, 2, 0, new double[] { 1, 2 })]
    [InlineData(new double[] { 1, 2, 3, 4 }, 1, 4, 0, new double[] { 1, 2, 3, 4 })]
    public void GetRowShouldReturnExactRow
        (
        double[] data,
        int rowCount,
        int columnCount,
        int columnIdx,
        double[] shouldBe
        )
    {
      // ARRANGE
      var matrix = Matrix
          .Build(data, rowCount, columnCount)
          .First();

      // ACT
      var result = matrix.GetRow(columnIdx);


      // ASSERT
      result
          .IsRight
          .Should().BeTrue();

      result
          .IfRight((arr) => arr.Should().BeEquivalentTo(shouldBe));
    }

    [Theory]
    [InlineData(new double[] { 1, 2, 3, 4 }, 2, 2, 2)]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 3, 2, -1)]
    [InlineData(new double[] { 1, 2, 3, 4 }, 1, 4, 10)]
    public void GetRowShouldReturnErrorWhenIndexOutOfRange
        (
        double[] data,
        int rowCount,
        int columnCount,
        int columnIdx
        )
    {
      // ARRANGE
      var matrix = Matrix
          .Build(data, rowCount, columnCount)
          .First();

      // ACT
      var result = matrix.GetRow(columnIdx);

      // ASSERT
      result
          .IsRight
          .Should().BeFalse();
    }

    [Theory]
    [InlineData(new double[] { 1, 2, 3, 4 }, 2, 2, new double[] { 1, 4 })]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, 3, new double[] { 1, 5, 9 })]
    [InlineData(new double[] { 1 }, 1, 1, new double[] { 1 })]
    public void GetDiagShouldReturnExactDiagonal
        (
        double[] data,
        int rowCount,
        int columnCount,
        double[] shouldBe
        )
    {
      // ARRANGE
      var matrix = Matrix
          .Build(data, rowCount, columnCount)
          .First();

      // ACT
      var result = matrix.GetDiag();


      // ASSERT
      result
          .IsRight
          .Should().BeTrue();

      result
          .IfRight((arr) => arr.Should().BeEquivalentTo(shouldBe));
    }

    [Theory]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 2, 3)]
    [InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 3, 2)]
    [InlineData(new double[] { 1, 2, 3, 4 }, 1, 4)]
    public void GetDiagShouldReturnErrorWhenMatrixIsNotSquared
        (
        double[] data,
        int rowCount,
        int columnCount
        )
    {
      // ARRANGE
      var matrix = Matrix
          .Build(data, rowCount, columnCount)
          .First();

      // ACT
      var result = matrix.GetDiag();

      // ASSERT
      result
          .IsRight
          .Should().BeFalse();
    }
    [Fact]
    public void SortByZeroesCountShouldWorkCorrectly()
    {
      // ARRANGE
      var data = new double[][]
      {
                new double[] { 0, 1, 2, 3},
                new double[] { 0, 0, 0, 0},
                new double[] { 0, 1, 0, 3},
                new double[] { 0, 1, 0, 0},
      };

      var shouldBe = new double[][]
      {
                new double[] { 0, 1, 2, 3},
                new double[] { 0, 1, 0, 3},
                new double[] { 0, 1, 0, 0},
                new double[] { 0, 0, 0, 0},
      };

      var matrix = Matrix
          .Build(data)
          .First();

      var shouldBeMatrix = Matrix
          .Build(shouldBe)
          .First();

      var countZeroes = (double[] x) => x.Filter(x => x is 0).Count();

      // ACT
      var sortedMatrix = matrix.SortBy(countZeroes).First();

      // ASSERT
      sortedMatrix.Equals(shouldBeMatrix)
          .Should().BeTrue();
    }

    [Fact]
    public void MatrixMultiplicationShouldBeCorrect()
    {
      // ARRANGE
      var data1 = new double[][]
      {
        new double[] { 0, 1, 2, 3},
        new double[] { 0, 0, 0, 0},
        new double[] { 0, 1, 0, 3}
      };

      var data2 = new double[][]
      {
        new double[] { 0, 1 },
        new double[] { 0, 1 },
        new double[] { 0, 1 },
        new double[] { 0, 0 },
      };

      var shouldBe = new double[][]
      {
        new double[] { 0, 3 },
        new double[] { 0, 0 },
        new double[] { 0, 1 }
      };

      var matrix1 = Matrix
          .Build(data1)
          .First();

      var matrix2 = Matrix
          .Build(data2)
          .First();

      var shouldBeMatrix = Matrix
          .Build(shouldBe)
          .First();

      // ACT
      var multipliedMatrix = (matrix1 * matrix2).First();

      // ASSERT
      multipliedMatrix
        .Equals(shouldBeMatrix)
          .Should().BeTrue();
    }
  }
}
