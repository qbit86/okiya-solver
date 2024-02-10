using System;

namespace Okiya;

public sealed class Int32CardConcept : ICardConcept<int>
{
    private const string Suits = "\u2663\u2666\u2665\u2660";
    private const string Ranks = "JQKA";

    private static readonly string[] s_cardStrings = CreateCardStrings();

    public static Int32CardConcept Instance { get; } = new();

    public string ToString(int card) => s_cardStrings[card & Constants.Log2CardCountMask];

    public int Suit(int card) => card >> Constants.Log2RankCount;

    public int Rank(int card) => card & Constants.Log2RankCountMask;

    public int Card(int suit, int rank) => CreateCard(suit, rank);

    private static int CreateCard(int suit, int rank) =>
        (suit << Constants.Log2RankCount) | (rank & Constants.Log2RankCountMask);

    private static string[] CreateCardStrings()
    {
        string[] result = new string[Constants.CardCount];
        Span<char> buffer = stackalloc char[2];
        for (int suit = 0; suit < Constants.SuitCount; ++suit)
        {
            for (int rank = 0; rank < Constants.RankCount; ++rank)
            {
                buffer[0] = Ranks[rank];
                buffer[1] = Suits[suit];
                int card = CreateCard(suit, rank);
                result[card] = new(buffer);
            }
        }

        return result;
    }
}
