using System;
using System.Collections.Generic;
using System.Diagnostics;

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
