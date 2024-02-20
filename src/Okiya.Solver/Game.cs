using System;
using System.Collections.Generic;

namespace Okiya;

public static class Game
{
    public static Game<TCardCollection> Create<TCardCollection>(TCardCollection board)
        where TCardCollection : IReadOnlyList<int>
    {
        if (board is null)
            throw new ArgumentNullException(nameof(board));
        if (board.Count < Constants.CardCount)
            throw new ArgumentException($"Board length must be at least {Constants.CardCount}.", nameof(board));

        return new(board);
    }

    internal static int PopulatePossibleFirstMoves(Span<int> destination)
    {
        int maxMoveCount = int.Min(destination.Length, Constants.CardCount);
        int moveCount = 0;
        for (int i = 0; i < maxMoveCount; ++i)
        {
            if (i is 5 or 6 or 9 or 10)
                continue;
            destination[moveCount++] = i;
        }

        return moveCount;
    }
}
