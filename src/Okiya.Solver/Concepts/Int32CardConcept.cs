using System;

namespace Okiya;

public sealed class Int32CardConcept : ICardConcept<int>
{
    private const string Suits = "\u2663\u2666\u2665\u2660";
    private const string Ranks = "JQKA";

    private static readonly string[] s_cardStrings = CreateCardStrings();

    public static Int32CardConcept Instance { get; } = new();

    public string ToString(int card) => s_cardStrings[card & 0b1111];

    public int Suit(int card) => (card >> 2) & 0b11;

    public int Rank(int card) => card & 0b11;

    private static string[] CreateCardStrings()
    {
        string[] result = new string[16];
        Span<char> buffer = stackalloc char[2];
        for (int suit = 0; suit < 4; ++suit)
        {
            for (int rank = 0; rank < 4; ++rank)
            {
                buffer[0] = Ranks[rank];
                buffer[1] = Suits[suit];
                int card = (suit << 2) + rank;
                result[card] = new(buffer);
            }
        }

        return result;
    }
}
