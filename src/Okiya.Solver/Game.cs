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
}

public readonly record struct Game<TCardCollection>
    where TCardCollection : IReadOnlyList<int>
{
    private readonly TCardCollection _board;

    internal Game(TCardCollection board) => _board = board;
}
