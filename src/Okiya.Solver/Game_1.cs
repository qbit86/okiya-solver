using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using static Okiya.TryHelpers;

namespace Okiya;

public readonly record struct Game<TCardCollection>
    where TCardCollection : IReadOnlyList<int>
{
    private readonly TCardCollection _cards;

    internal Game(TCardCollection cards)
    {
        Debug.Assert(cards.Count >= Constants.CardCount);
        _cards = cards;
    }

    public bool IsValid => _cards is not null;

    public bool TryGetCard(int index, out int card) =>
        unchecked((uint)index < Constants.CardCount) ? Some(_cards[index], out card) : None(out card);

    public int GetCard(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Constants.CardCount);

        return _cards[index];
    }

    public Node MakeMove(Node node, int move)
    {
        if (unchecked((uint)move >= Constants.CardCount))
            throw new ArgumentOutOfRangeException(nameof(move));

        if (node.TryGetCardIndex(out int cardIndex))
        {
            Int32CardConcept c = Int32CardConcept.Instance;
            int lastCard = _cards[cardIndex];
            int newCard = _cards[move];
            if (c.Rank(newCard) != c.Rank(lastCard) && c.Suit(newCard) != c.Suit(lastCard))
                throw new ArgumentException("Both rank and suit do not match the previous card.", nameof(move));
        }
        else if (Game.IsCentralBlock(move))
            throw new ArgumentException("The first move cannot be to the center block.", nameof(move));

        if (!node.TryAddPlayerToken(move, out Node child))
            throw new ArgumentException("The move is already done.", nameof(move));

        return child;
    }

#pragma warning disable CA1822 // Mark members as static
    internal Node MakeMoveUnchecked(Node node, int move)
#pragma warning restore CA1822
    {
        Debug.Assert(unchecked((uint)move < Constants.CardCount));
#if DEBUG
        if (node.TryGetCardIndex(out int cardIndex))
        {
            Int32CardConcept c = Int32CardConcept.Instance;
            int lastCard = _cards[cardIndex];
            int newCard = _cards[move];
            Debug.Assert(c.Rank(newCard) == c.Rank(lastCard) || c.Suit(newCard) == c.Suit(lastCard));
        }
        else
        {
            Debug.Assert(!Game.IsCentralBlock(move));
        }
#endif

        return node.AddPlayerTokenUnchecked(move);
    }

    public bool TryMakeMove(Node node, int move, out Node child)
    {
        if (unchecked((uint)move >= Constants.CardCount))
            return None(node, out child);

        if (node.TryGetCardIndex(out int cardIndex))
        {
            Int32CardConcept c = Int32CardConcept.Instance;
            int lastCard = _cards[cardIndex];
            int newCard = _cards[move];
            if (c.Rank(newCard) != c.Rank(lastCard) && c.Suit(newCard) != c.Suit(lastCard))
                return None(node, out child);
        }
        else if (Game.IsCentralBlock(move))
            return None(node, out child);

        return node.TryAddPlayerToken(move, out child);
    }

    public void PopulateLegalMoves<TCollection>(Node node, TCollection moves)
        where TCollection : ICollection<int>
    {
        if (moves is null)
            throw new ArgumentNullException(nameof(moves));

        int[] buffer = ArrayPool<int>.Shared.Rent(Constants.CardCount);
        int moveCount = PopulateLegalMoves(node, buffer.AsSpan());
        Span<int> span = buffer.AsSpan(0, moveCount);
        foreach (int move in span)
            moves.Add(move);
        ArrayPool<int>.Shared.Return(buffer);
    }

    internal int PopulateLegalMoves(Node node, Span<int> destination)
    {
        if (!node.TryGetCardIndex(out int lastCardIndex))
            return Game.PopulateLegalFirstMoves(destination);
        int lastCard = _cards[lastCardIndex];
        Int32CardConcept c = Int32CardConcept.Instance;
        int tokensPlayed = node.GetPlayerTokens(0) | node.GetPlayerTokens(1);
        int moveCount = 0;
        int maxMoveCount = int.Min(destination.Length, Constants.CardCount);
        for (int i = 0; i < maxMoveCount; ++i)
        {
            int mask = 1 << i;
            if ((tokensPlayed & mask) is not 0)
                continue;
            int candidateCard = _cards[i];
            if (c.Rank(candidateCard) != c.Rank(lastCard) && c.Suit(candidateCard) != c.Suit(lastCard))
                continue;
            destination[moveCount++] = i;
        }

        return moveCount;
    }
}
