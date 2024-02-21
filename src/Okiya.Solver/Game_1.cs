using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Okiya.TryHelpers;

namespace Okiya;

public readonly record struct Game<TCardCollection>
    where TCardCollection : IReadOnlyList<int>
{
    private readonly TCardCollection _board;

    internal Game(TCardCollection board)
    {
        Debug.Assert(board.Count >= Constants.CardCount);
        _board = board;
    }

    internal bool IsValid => _board is not null;

    public Node MakeMove(Node node, int move)
    {
        if (unchecked((uint)move >= Constants.CardCount))
            throw new ArgumentOutOfRangeException(nameof(move));

        if (node.TryGetCardIndex(out int cardIndex))
        {
            Int32CardConcept c = Int32CardConcept.Instance;
            int lastCard = _board[cardIndex];
            int newCard = _board[move];
            if (c.Rank(newCard) != c.Rank(lastCard) && c.Suit(newCard) != c.Suit(lastCard))
                throw new ArgumentException("Both rank and suit do not match the previous card.", nameof(move));
        }

        if (!node.TryAddPlayerToken(move, out Node child))
            throw new ArgumentException("The move is already done.", nameof(move));

        return child;
    }

    internal Node MakeMoveUnchecked(Node node, int move)
    {
        Debug.Assert(unchecked((uint)move < Constants.CardCount));
#if DEBUG
        if (node.TryGetCardIndex(out int cardIndex))
        {
            Int32CardConcept c = Int32CardConcept.Instance;
            int lastCard = _board[cardIndex];
            int newCard = _board[move];
            Debug.Assert(c.Rank(newCard) == c.Rank(lastCard) || c.Suit(newCard) == c.Suit(lastCard));
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
            int lastCard = _board[cardIndex];
            int newCard = _board[move];
            if (c.Rank(newCard) != c.Rank(lastCard) && c.Suit(newCard) != c.Suit(lastCard))
                return None(node, out child);
        }

        return node.TryAddPlayerToken(move, out child);
    }

    internal int PopulatePossibleMoves(Node node, Span<int> destination)
    {
        if (!node.TryGetCardIndex(out int lastCardIndex))
            return Game.PopulatePossibleFirstMoves(destination);
        int lastCard = _board[lastCardIndex];
        Int32CardConcept c = Int32CardConcept.Instance;
        int tokensPlayed = node.GetPlayerTokens(0) | node.GetPlayerTokens(1);
        int moveCount = 0;
        int maxMoveCount = int.Min(destination.Length, Constants.CardCount);
        for (int i = 0; i < maxMoveCount; ++i)
        {
            int mask = 1 << i;
            if ((tokensPlayed & mask) is not 0)
                continue;
            int candidateCard = _board[i];
            if (c.Rank(candidateCard) != c.Rank(lastCard) && c.Suit(candidateCard) != c.Suit(lastCard))
                continue;
            destination[moveCount++] = i;
        }

        return moveCount;
    }
}
