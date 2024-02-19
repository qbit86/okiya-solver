using System;

namespace Okiya.Experimental;

// https://en.wikipedia.org/wiki/Negamax

public sealed class Solver
{
    private readonly int[] _board;
    private Node _currentNode;

    private Solver(int[] board, Node initialNode)
    {
        _board = board;
        _currentNode = initialNode;
    }

    public static Solver Create(
        int[] board, int maximizingPlayerTokens, int minimizingPlayerTokens, int sideToMove, int lastCard = default)
    {
        ArgumentNullException.ThrowIfNull(board);
        if (board.Length < Constants.CardCount)
            throw new ArgumentException($"Board length must be at least {Constants.CardCount}.", nameof(board));
        uint maxPlayerTokens = (uint)maximizingPlayerTokens;
        if (maxPlayerTokens > Constants.PlayerTokensMask)
            throw new ArgumentOutOfRangeException(nameof(maximizingPlayerTokens));
        uint minPlayerTokens = (uint)minimizingPlayerTokens;
        if (minPlayerTokens > Constants.PlayerTokensMask)
            throw new ArgumentOutOfRangeException(nameof(minimizingPlayerTokens));
        if ((maxPlayerTokens & minPlayerTokens) is not 0)
            throw new ArgumentException("Players' tokens may not overlap.", nameof(minimizingPlayerTokens));
        if (sideToMove is not (0 or 1))
            throw new ArgumentOutOfRangeException(nameof(sideToMove));
        if ((uint)lastCard >= Constants.CardCount)
            throw new ArgumentOutOfRangeException(nameof(lastCard));

        return new(board, Node.CreateUnchecked(maxPlayerTokens, minPlayerTokens, sideToMove, lastCard));
    }
}
