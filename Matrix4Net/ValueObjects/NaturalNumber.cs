using LanguageExt;
using System.Diagnostics.CodeAnalysis;

namespace Matrix4Net.ValueObjects
{
  internal readonly struct NaturalNumber
  {
    private readonly int _value;

    private NaturalNumber(int value)
    {
      _value = value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
      => obj switch
      {
        NaturalNumber number => number._value == this._value,
        _ => false
      };


    public static implicit operator int(NaturalNumber index)
            => index._value;

    public static Option<NaturalNumber> Build(int value)
        => IsValid(value)
        ? new NaturalNumber(value)
        : Option<NaturalNumber>.None;

    private static bool IsValid(int value)
        => value >= 0;
  }
}