namespace Okiya;

public interface ICardConcept<in TCard>
{
    public string ToString(TCard card);

    public int Suit(TCard card);

    public int Rank(TCard card);
}
