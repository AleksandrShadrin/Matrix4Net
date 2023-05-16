using LanguageExt;
using Matrix4Net.ValueObjects;

namespace Matrix4Net.Utility;

public static class Linalg
{
  public static Option<double> MatrixDeterminant(Matrix a)
    => LU(a)
      .Bind
        (
          lu => lu.l.GetDiag()
            .ToOption()
            .Map(d => d.Fold(1d, (s, n) => s * n))
            .Bind
              (
                ldet => lu.u.GetDiag()
                  .ToOption()
                  .Map(d => d.Fold(ldet, (s, n) => s * n))
              )
        );

  public static Option<(Matrix l, Matrix u)> LU(Matrix a)
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

  #region Lu decomposition tools
  private static Either<DivideByZeroException, Unit> EvaluateLower(Matrix a, int n, Matrix l, Matrix u, int i)
    => Enumerable.Range(i, n - i)
      .Map<int, Either<DivideByZeroException, Unit>>
        (
          k =>
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

            return Unit.Default;
          }
        ).Aggregate
          (
            (p, n) => p.IsRight ? n : p
          );

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