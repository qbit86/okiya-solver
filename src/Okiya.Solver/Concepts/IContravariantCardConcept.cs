namespace Okiya;

public interface IContravariantCardConcept<in TCard>
{
    public string ToString(TCard card);

    public int Suit(TCard card);

    public int Rank(TCard card);
}
