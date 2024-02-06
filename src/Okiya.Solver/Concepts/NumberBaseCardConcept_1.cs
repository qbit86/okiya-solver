using System.Numerics;

namespace Okiya;

public sealed class NumberBaseCardConcept<T> : ICardConcept<T>
    where T : INumberBase<T>
{
    public string ToString(T card) => Int32CardConcept.Instance.ToString(int.CreateTruncating(card));

    public int Suit(T card) => Int32CardConcept.Instance.Suit(int.CreateTruncating(card));

    public int Rank(T card) => Int32CardConcept.Instance.Rank(int.CreateTruncating(card));
}
