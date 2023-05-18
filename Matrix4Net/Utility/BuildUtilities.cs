using LanguageExt;
using LanguageExt.SomeHelp;
using Matrix4Net.Constants;
using Matrix4Net.ValueObjects;

namespace Matrix4Net.Utility
{
  public static class BuildUtilities
  {
    public static Matrix TransposeMatrix(Matrix a)
    {
      var (rows, columns) = a.GetMatrixShape();

      var matrix = BuildZeroMatrix(columns, rows).First();

      for (int i = 0; i < rows; i++)
      {
        for (int j = 0; j < columns; j++)
        {
          matrix[j, i] = a[i, j];
        }
      }

      return matrix;
    }

    public static Option<Matrix> ConcatMatrix(Matrix a, Matrix b, ConcatenateMode mode)
      => mode switch
      {
        ConcatenateMode.CONCATENATE_ROWS => ConcatRows(a, b),
        ConcatenateMode.CONCATENATE_COLUMNS => ConcatColumns(a, b),
      };

    private static Option<Matrix> ConcatColumns(Matrix a, Matrix b)
      => a.GetMatrixShape().rows == b.GetMatrixShape().rows
      ? a.Zip(b, (f, s) => ConcatArrays(f, s))
        .ToArray()
        .ToSome()
        .ToOption().Bind(Matrix.Build)
      : Option<Matrix>.None;

    private static double[] ConcatArrays(double[] a, double[] b)
    {
      var res = new double[a.Length + b.Length];
      a.CopyTo(res, 0);
      b.CopyTo(res, a.Length);

      return res;
    }

    private static Option<Matrix> ConcatRows(Matrix a, Matrix b)
      => a.GetMatrixShape().columns == b.GetMatrixShape().columns
      ? a.ConcatFast(b)
        .ToArray()
        .ToSome()
        .ToOption().Bind(Matrix.Build)
      : Option<Matrix>.None;

    public static Option<Matrix> BuildFilledMatrix(int rows, int columns, double fillBy)
      => NaturalNumber.Build(rows)
        .Bind
          (
            r => NaturalNumber
              .Build(columns)
              .Bind(c =>
              {
                var arr = new double[r * c];
                Array.Fill(arr, fillBy);
                return Matrix.Build(arr, r, c);
              })
          );

    public static Option<Matrix> BuildZeroMatrix(int rows, int columns)
        => BuildFilledMatrix(rows, columns, 0);

    public static Option<Matrix> BuildDiagMatrix(int size, double value)
        => NaturalNumber.Build(size)
                .Map
                (
                  n =>
                    Enumerable.Range(0, n)
                    .Map(i =>
                    {
                      var arr = new double[n];
                      Array.Fill(arr, 0);
                      arr[i] = value;

                      return arr;
                    }).ToArray()
                ).Bind(Matrix.Build);

    public static Option<Matrix> BuildEyeMatrix(int n)
      => BuildDiagMatrix(n, 1);
  }
}
