namespace Okiya;

public interface ICovariantCardConcept<out TCard>
{
    public TCard Card(int suit, int rank);
}
