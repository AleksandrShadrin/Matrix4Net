using LanguageExt;
using LanguageExt.SomeHelp;
using System.Collections;
using System.Text;

namespace Matrix4Net.ValueObjects
{
  /// <summary>
  /// Matrix representation
  /// </summary>
  public readonly struct Matrix : IEnumerable<double[]>, IEquatable<Matrix>
  {
    private readonly double[][] _data;
    private readonly int _rowsCount;
    private readonly int _columnsCount;

    internal Matrix(double[][] data)
    {
      _data = data;
      _rowsCount = data.Length;
      _columnsCount = data[0].Length;
    }

    #region Indexer
    public double[][] this[Range range]
    {
      get => _data[range];
    }

    public double[][] this[Range rows, Range columns]
    {
      get => _data[rows].Map(r => r[columns]).ToArray();
    }

    public double[] this[int row, Range columns]
    {
      get => _data[row][columns];
    }

    public double[] this[Range rows, int column]
    {
      get => _data[rows].Map(r => r[column]).ToArray();
    }

    public double this[int rowIdx, int columnIdx]
    {
      get => _data[rowIdx][columnIdx];
      set => _data[rowIdx][columnIdx] = value;
    }

    public Option<double> TryGetValue(int rowIdx, int columnIdx)
        => InRangeOfMatrix(rowIdx, columnIdx)
            ? _data[rowIdx][columnIdx]
            : Option<double>.None;

    public Either<IndexOutOfRangeException, Unit> TrySetValue(int rowIdx, int columnIdx, double value)
        => InRangeOfMatrix(rowIdx, columnIdx)
            ? SetValue(rowIdx, columnIdx, value)
            : new IndexOutOfRangeException("Index can't be out of matrix sizes.");

    private Unit SetValue(int rowIdx, int columnIdx, double value)
    {
      _data[rowIdx][columnIdx] = value;
      return Unit.Default;
    }

    private bool InRangeOfMatrix(int rowIdx, int columnIdx)
        => InRange(rowIdx, 0, _rowsCount - 1)
            && InRange(rowIdx, 0, _columnsCount - 1);
    #endregion

    #region Tools
    /// <summary>
    /// Check if matrix squared
    /// </summary>
    /// <returns>returns boolean value</returns>
    public bool IsSquared()
      => _rowsCount == _columnsCount;

    /// <summary>
    /// Try reshape matrix to size nrows by ncols
    /// </summary>
    /// <param name="nrows">number of rows</param>
    /// <param name="ncols">number of columns</param>
    /// <returns>None if can't reshape else reshaped matrix</returns>
    public Option<Matrix> Reshape(int nrows, int ncols)
      => _data.Flatten()
        .ToSome()
        .ToOption()
        .Bind(d => Build(d, nrows, ncols));

    /// <summary>
    /// Map all elements of matrix that match predicate
    /// </summary>
    /// <param name="predicate">predicate for single element of matrix</param>
    /// <param name="func">Map function for element</param>
    /// <returns>Unit as result of operation</returns>
    public Unit MapFor(Func<double, bool> predicate, Func<double, double> func)
    {
      var @this = this;

      this
        .Iter
          (
            (ri, r) => Enumerate(r)
              .Filter(t => predicate(t.item))
              .Map(t => t.idx)
              .Iter
                (
                  ci => @this[ri, ci] = func(@this[ri, ci])
                )
          );

      return Unit.Default;
    }

    /// <summary>
    /// Copy matrix
    /// </summary>
    /// <returns>New Matrix</returns>
    public Matrix CopyMatrix()
      => Build(_data).First();

    /// <summary>
    /// Swap matrix rows
    /// </summary>
    /// <param name="fIdx">First row index</param>
    /// <param name="sIdx">Second row index</param>
    /// <returns>Either with L is IndexOutOfRangeException and R is Unit</returns>
    public Either<IndexOutOfRangeException, Unit> SwapRows(int fIdx, int sIdx)
    {
      if
        (
          InRange(fIdx, 0, _rowsCount - 1)
          && InRange(sIdx, 0, _rowsCount - 1)
        )
      {
        (_data[fIdx], _data[sIdx]) = (_data[sIdx], _data[fIdx]);

        return Unit.Default;
      }

      return new IndexOutOfRangeException("Index can't be out of rows count");
    }

    /// <summary>
    /// Return tuple with matrix sizes
    /// </summary>
    /// <returns>rows - count of rows and same for columns</returns>
    public (int rows, int columns) GetMatrixShape()
        => (rows: _rowsCount, columns: _columnsCount);

    /// <summary>
    /// Return sorted matrix by key selector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns>Option of matrix</returns>
    public Option<Matrix> SortBy<T>(Func<double[], T> func, bool descending = false)
        => CopyData(_data).ToSome()
            .Map
                (
                    data => descending is true
                    ? data.OrderByDescending(func)
                    : data.OrderBy(func)
                )
            .Map(d => d.ToArray())
            .Map(Build)
            .First();

    /// <summary>
    /// Return matrix diagonal if it squared
    /// </summary>
    /// <returns></returns>
    public Either<InvalidOperationException, double[]> GetDiag()
        => _rowsCount == _columnsCount
        ? _data.Map((idx, row) => row[idx]).ToArray()
        : new InvalidOperationException("This op can be applied only to squared matrix.");

    /// <summary>
    /// Return column
    /// </summary>
    /// <param name="n">Index of column</param>
    /// <returns>Either with L is IndexOutOfRange and R is double array</returns>
    public Either<IndexOutOfRangeException, double[]> GetColumn(int n)
        => InRange(n, 0, _columnsCount - 1)
        ? ExtractColumnValues(n)
        : new IndexOutOfRangeException("Index not in range.");

    /// <summary>
    /// Return row
    /// </summary>
    /// <param name="n">Index of row</param>
    /// <returns>Either with L is IndexOutOfRange and R is double array</returns>
    public Either<IndexOutOfRangeException, double[]> GetRow(int n)
        => InRange(n, 0, _rowsCount - 1)
        ? GetRowCopy(n)
        : new IndexOutOfRangeException("Index not in range.");

    private double[] GetRowCopy(int n)
    {
      var copy = new double[_columnsCount];
      _data[n].CopyTo(copy, 0);

      return copy;
    }

    private double[] ExtractColumnValues(int n)
    { 
      var copy = new double[_rowsCount];
      for (int i = 0; i < _rowsCount; i++)
      {
        copy[i] = _data[i][n];
      }
      return copy;
    }

    private bool InRange(int n, int l, int r)
        => n >= l && n <= r;

    private static IEnumerable<(int idx, T item)> Enumerate<T>(IEnumerable<T> seq)
      => seq.Map((idx, item) => (idx, item));
    #endregion

    #region Multiplication Operations
    public static Option<Matrix> operator *(Matrix that, Matrix other)
        => MatrixesMultiplicationIsValid(that, other)
        ? Multiply(that, other)
            .ToSome()
            .Map(a => new Matrix(a))
            .First()
        : Option<Matrix>.None;

    public static Option<Matrix> operator *(Matrix that, double right)
        => that.Map(row => row.Map(el => el * right).ToArray())
            .ToArray()
            .ToSome()
            .Map(Build)
            .First();

    public static Option<Matrix> operator *(double left, Matrix that)
        => that * left;

    public static Option<Matrix> operator *(Matrix that, int right)
        => that * (double)right;

    public static Option<Matrix> operator *(int left, Matrix that)
        => that * left;

    private static double[][] Multiply(Matrix that, Matrix other)
      => that.Map
          (
            row => MultiplyRow(row, other)
          ).ToArray();

    private static double[] MultiplyRow(double[] row, Matrix other)
    {
      var res = new double[other._columnsCount];
      Array.Fill(res, 0);

      for (int i = 0; i < row.Length; i++)
      {
        for (int j = 0; j < other._columnsCount; j++)
        {
          res[j] += other[i, j] * row[i];
        }
      }

      return res;
    }

    private static bool MatrixesMultiplicationIsValid(Matrix that, Matrix other)
        => that._columnsCount == other._rowsCount;
    #endregion

    #region Sum Operations
    public static Option<Matrix> operator +(Matrix that, Matrix other)
        => IsMatrixesWithEqualSizes(that, other)
        ? that.Zip(other)
            .Map(Sum)
            .ToArray()
            .ToSome()
            .Map(arr => new Matrix(arr))
            .First()
        : Option<Matrix>.None;

    public static Option<Matrix> operator +(Matrix that, double right)
        => that.Map(row => row.Map(el => el + right).ToArray())
            .ToArray()
            .ToSome()
            .Map(Build)
            .First();

    public static Option<Matrix> operator +(double left, Matrix that)
        => that + left;

    public static Option<Matrix> operator +(Matrix that, int right)
        => that + (double)right;

    public static Option<Matrix> operator +(int left, Matrix that)
        => that + left;

    private static double[] Sum((double[], double[]) zipped)
        => zipped.Item1.Zip(zipped.Item2, (f, s) => f + s)
            .ToArray();
    #endregion

    #region Substract Operations
    public static Option<Matrix> operator -(Matrix that, Matrix other)
        => IsMatrixesWithEqualSizes(that, other)
        ? that.Zip(other)
            .Map(Substract)
            .ToArray()
            .ToSome()
            .Map(Build)
            .First()
        : Option<Matrix>.None;

    public static Matrix operator -(Matrix that)
        => new Matrix(that
            .Map(row => row.Map(el => -el).ToArray())
            .ToArray());

    public static Option<Matrix> operator -(Matrix that, double right)
        => that + (-right);

    public static Option<Matrix> operator -(double left, Matrix that)
        => -that + left;

    public static Option<Matrix> operator -(Matrix that, int right)
        => that - (double)right;

    public static Option<Matrix> operator -(int left, Matrix that)
        => -that + left;

    private static double[] Substract((double[], double[]) zipped)
        => zipped.Item1.Zip(zipped.Item2)
            .Map(z => z.Item1 - z.Item2)
            .ToArray();
    #endregion

    #region Implicit Convertions
    public static implicit operator double[][](Matrix matrix)
        => CopyData(matrix);
    #endregion

    #region Builder
    /// <summary>
    /// Build matrix based on copy of 2d array
    /// </summary>
    /// <param name="data">array copy of which will be used</param>
    /// <returns>Some(Matrix) if data is correct else None</returns>
    public static Option<Matrix> Build(double[][] data)
        => data.ToSome()
            .Filter(ValidateMatrix)
            .Map(CopyData)
            .ToOption()
            .Map(d => new Matrix(d));

    /// <summary>
    /// Build matrix based on values of 1d array
    /// </summary>
    /// <param name="data">1d array which values used</param>
    /// <param name="rowsCount">Count of rows that will have matrix</param>
    /// <param name="columnsCount">Count of columns that will have matrix</param>
    /// <returns>Some(Matrix) if data is correct else None</returns>
    public static Option<Matrix> Build(double[] data, int rowsCount, int columnsCount)
        => ValidateRowAndColumnSizes(rowsCount, columnsCount, data.Length) ?
            Enumerable.Range(0, data.Length / columnsCount)
                .Map(rowIdx => data[(rowIdx * columnsCount)..((rowIdx + 1) * columnsCount)])
                .ToArray()
                .ToSome()
                .Map(arr => new Matrix(arr))
                .ToOption()
            : Option<Matrix>.None;

    private static double[][] CopyData(double[][] data)
    {
      var copyOfData = new double[data.Length][];

      data.Iter
          (
              (idx, row) =>
              {
                copyOfData[idx] = new double[row.Length];
                row.CopyTo(copyOfData[idx], 0);
              }
          );

      return copyOfData;
    }

    #endregion

    #region Validation
    private static bool ValidateMatrix(double[][] data)
        => data.Length is not 0
            && Enumerable.Range(0, data.Length - 1)
            .Map(idx => data[idx].Length == data[idx + 1].Length)
            .ForAll(res => res is true);

    private static bool ValidateRowAndColumnSizes(int rowCount, int columnCount, int matrixSize)
        => rowCount is > 0
            && columnCount is > 0
            && rowCount * columnCount == matrixSize;

    private static bool IsMatrixesWithEqualSizes(Matrix first, Matrix second)
        => first._columnsCount == second._columnsCount
            && first._rowsCount == second._rowsCount;
    #endregion

    #region IEnumerable Implementation
    public IEnumerator<double[]> GetEnumerator()
    {
      var columnsCount = _columnsCount;

      return _data.Map(row =>
          {
            var data = new double[columnsCount];
            row.CopyTo(data, 0);
            return data;
          }).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion

    #region Equals implementation
    public bool Equals(Matrix other)
        => this.GetMatrixShape() == other.GetMatrixShape()
            && this.Zip(other)
            .All(values => values.Item1.SequenceEqual(values.Item2));
    #endregion

    #region ToString
    public override string ToString()
    {
      var sb = new StringBuilder();

      sb.AppendLine($"Matrix size = {_rowsCount}x{_columnsCount}");
      sb.AppendLine();

      _data
          .Map(RowToString)
          .Map(line => WrapByFigures(line))
          .Iter((line) => sb.AppendLine(line));

      return sb.ToString();
    }

    private static string RowToString(double[] row)
        => row.Length > 10
        ? row.Take(4).Aggregate("", (p, n) => p + n + ", ")
        + row.TakeLast(4).Reverse().Aggregate(", ", (p, n) => p + ", " + n)
        : row.SkipLast(1).Take(9).Aggregate("", (p, n) => p + n + ", ") + row.Last();

    private static string WrapByFigures(string text, string left = "[", string right = "]")
        => $"{left}{text}{right}";

    #endregion
  }
}