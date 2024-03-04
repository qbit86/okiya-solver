using System;
using System.Diagnostics;
using static Okiya.TryHelpers;

namespace Okiya;

public sealed class Int32CardConcept : ICardConcept<int>
{
    private const string Suits = "\u2663\u2666\u2665\u2660"; // ♣♦♥♠
    private const string SuitLetters = "CDHS"; // Clubs, Diamonds, Hearts, Spades
    private const string Ranks = "JQKA";

    private static readonly string[] s_cardStrings = CreateCardStrings();

    public static Int32CardConcept Instance { get; } = new();

    public string ToString(int card) => s_cardStrings[card & Constants.CardMask];

    public int Parse(string s)
    {
        if (TryParse(s, out int result))
            return result;

        throw new FormatException($"An invalid card was specified: '{s}'.");
    }

    public bool TryParse(string s, out int card)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (s.Length < 2)
            return None(out card);

        int rank = Ranks.IndexOf(s[0], StringComparison.Ordinal);
        Debug.Assert(rank < Ranks.Length);
        if (rank < 0)
            return None(out card);

        int suit = Suits.IndexOf(s[1], StringComparison.Ordinal);
        Debug.Assert(suit < Suits.Length);
        if (suit < 0)
        {
            suit = SuitLetters.IndexOf(s[1], StringComparison.Ordinal);
            Debug.Assert(suit < SuitLetters.Length);
            if (suit < 0)
                return None(out card);
        }

        card = CreateCard(suit, rank);
        return true;
    }

    public int Suit(int card) => card >> Constants.RankBitCount;

    public int Rank(int card) => card & Constants.RankMask;

    public int Card(int suit, int rank) => CreateCard(suit, rank);

    private static int CreateCard(int suit, int rank) =>
        (suit << Constants.RankBitCount) | (rank & Constants.RankMask);

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
