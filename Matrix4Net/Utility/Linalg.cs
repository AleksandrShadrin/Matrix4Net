using LanguageExt;
using LanguageExt.SomeHelp;
using Matrix4Net.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Matrix4Net.Utility;

public static class Linalg
{
  public static Option<double> MatrixDeterminantByPLU(Matrix a)
    => PLU(a)
      .Bind
        (
          item => item.lu.GetDiag()
            .ToOption()
            .Map
              (
                d => d
                .Fold
                  (item.p.Last() % 2 == 0 
                    ? 1d
                    : -1d, 
                    (s, n) => s * n
                  )
              )
        );

  #region Hauseholder transformation

  public static Option<Matrix> HouseholderTransform(Matrix a)
  {
    if (a.IsSquared() is false)
      return Option<Matrix>.None;

    var n = a.GetMatrixShape().rows;

    Matrix res = a;

    for (int k = 0; k < n - 2; k++)
    {
      var alpha = CalcAlpha(res, k);
      var r = CalcR(res, k, alpha);

      var v = EvalV(res, alpha, r, k);

      var p = v
        .Bind(m => m * BuildUtilities.TransposeMatrix(m))
        .Bind(m => m * 2)
        .Bind(m => BuildUtilities.BuildEyeMatrix(n).First() - m);

      if (p.IsNone)
        return Option<Matrix>.None;

      res = p
        .Bind(l => l * res)
        .Bind(l => l * p.First())
        .First();
    }

    return res;
  }

  private static Option<Matrix> EvalV(Matrix a, double alpha, double r, int k)
  {
    var v = new double[a.GetMatrixShape().rows];
    Array.Fill(v, 0);

    var two_r = 2 * r;

    if (two_r is 0)
      return Option<Matrix>.None;

    v[k + 1] = a[k + 1, k] - alpha;
    v[k + 1] /= two_r;

    for (int i = k + 2; i < v.Length; i++)
    {
      v[i] = a[i, k] / two_r;
    }

    return Matrix.Build(v, v.Length, 1);
  }

  private static double CalcAlpha(Matrix a, int k)
    => -Sign(a[k+1, k]) 
      * a[(k + 1).., k]
      .Map(x => x * x)
      .Sum()
      .Apply(x => Math.Sqrt(x));

  private static double CalcR(Matrix a, int k, double alpha)
    => (alpha * 0.5 * (alpha - a[k+1, k]))
      .Apply(res => Math.Sqrt(res));

  private static int Sign(double x)
    => x >= 0 
    ? 1
    : -1;


  #endregion

  #region QRFactorization
  public static Option<(Matrix q, Matrix r)> QR(Matrix that)
  {
    var (_, columns) = that.GetMatrixShape();
    var b = new double[columns][];

    for (int i = 0; i < columns; i++)
    {
      b[i] = that[.., i];
      var a = that[.., i];

      for (int j = 0; j < i; j++)
      {
        SubtractFrom
          (
            b[i],
            GetProjection(a, b[j])
          );
      }
    }

    for (int i = 0; i < columns; i++)
    {
      Normalize(b[i]);
    }

    var q = BuildUtilities.BuildZeroMatrix(columns, columns).First();
    FillMatrixColumns(q, b);

    var rq = BuildUtilities
      .TransposeMatrix(q)
      .ToSome()
      .ToOption()
      .Bind(x => x * that)
      .Map(r => (r, q));

    return rq;
  }
  private static void FillMatrixColumns(Matrix a, double[][] columns)
  {
    for (int i = 0; i < columns.Length; i++)
    {
      for (int j = 0; j < columns[0].Length; j++)
      {
        a[j, i] = columns[i][j];
      }
    }
  }

  private static void Normalize(double[] arr)
  {
    var norm = Math.Sqrt(arr.Map(x => x * x).Sum());

    for (int i = 0; i < arr.Length; i++)
    {
      arr[i] /= norm;
    }
  }

  private static void SubtractFrom(double[] arr, double[] subtractor)
  {
    for (int i = 0; i < arr.Length; i++)
    {
      arr[i] -= subtractor[i];
    }
  }

  private static double[] GetProjection(double[] a, double[] b)
  {
    var coef = GetZippedProduct(a, b).Sum()
      / GetZippedProduct(b, b).Sum();

    return b.Map(d => coef * d).ToArray();
  }

  #endregion

  #region LUDecomposition
  /// <summary>
  /// Calculate PLU decomposition
  /// </summary>
  /// <param name="a">matrix to decompose</param>
  /// <param name="tol">small tolerance number to detect failure when the matrix is near degenerate
  /// </param>
  /// <returns>lu matrix with permutations p, where last item is number of permutations</returns>
  public static Option<(Matrix lu, int[] p)> PLU(Matrix a, double tol = 10e-16)
  {
    if (a.IsSquared() is false)
      return Option<(Matrix lu, int[] p)>.None;

    var matrix = a.CopyMatrix();
    var n = a.GetMatrixShape().rows;
    var p = new int[n + 1];
    p[n] = 0;

    for (int i = 0; i < n; i++)
    {
      p[i] = 0;
    }

    for (int i = 0; i < n; i++)
    {
      var (imax, maxVal) = matrix[i.., i].Map
        (
          (idx ,val) => (idx + i, val: Math.Abs(val))
        ).MaxBy(t => t.val);

      if(maxVal < tol) 
        return Option<(Matrix lu, int[] p)>.None;

      if (imax != i)
      {
        matrix.SwapRows(imax, i);
        (p[i], p[imax]) = (p[imax], p[i]);

        p[n]+=1;
      }

      for (int j = i + 1; j < n; j++)
      {
        matrix[j, i] /= matrix[i, i];

        for (int k = i + 1; k < n; k++)
        {
          matrix[j, k] -= matrix[j, i] * matrix[i, k];
        }
      }
    }

    return (matrix, p);
  }

  public static Option<(Matrix l, Matrix u)> DoolittleLU(Matrix a)
  {
    if (a.IsSquared() is false)
      return Option<(Matrix l, Matrix u)>.None;

    var n = a.GetMatrixShape().rows;

    var l = BuildUtilities
      .BuildZeroMatrix(n, n)
      .First();

    var u = BuildUtilities
      .BuildZeroMatrix(n, n)
      .First();

    for (int i = 0; i < n; i++)
    {
      EvaluateUpper(a, n, l, u, i);

      if (EvaluateLower(a, n, l, u, i).IsLeft)
        return Option<(Matrix l, Matrix u)>.None;
    }

    return (l, u);
  }
  private static Either<DivideByZeroException, Unit> EvaluateLower(Matrix a, int n, Matrix l, Matrix u, int i)
  {
    for (int k = i; k < n; k++)
    {
      if (k == i)
        l[i, k] = 1;
      else if (u[i, i] == 0)
        return new DivideByZeroException($"Can't divide by a[{i}][{i}] cause it 0.");
      else
        l[k, i] =
        (
          a[k, i]
          - GetZippedProduct
            (
              l[k, 0..i],
              u[0..i, i]
            ).Sum()
        ) / u[i, i];
    }
    return Unit.Default;
  }

  private static void EvaluateUpper(Matrix a, int n, Matrix l, Matrix u, int i)
    => Enumerable.Range(i, n - i)
      .Iter
        (
          k => u[i, k] = a[i, k]
            - GetZippedProduct
              (
                l[i, 0..i],
                u[0..i, k]
              ).Sum()
        );

  private static IEnumerable<double> GetZippedProduct
    (
      IEnumerable<double> first,
      IEnumerable<double> second
    )
    => first.Zip(second, (f, s) => f * s);
  #endregion
}