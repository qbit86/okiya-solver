namespace Okiya;

public interface ICovariantCardConcept<out TCard>
{
    public TCard Card(int suit, int rank);

    public int Parse(string s);

    public bool TryParse(string s, out int card);
}
