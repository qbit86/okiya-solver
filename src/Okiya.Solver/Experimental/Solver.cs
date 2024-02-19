using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static Okiya.TryHelpers;

namespace Okiya.Experimental;

// https://en.wikipedia.org/wiki/Negamax

public sealed class Solver
{
    private readonly int[] _board;
    private readonly Node _currentNode;

    private Solver(int[] board, Node initialNode)
    {
        _board = board;
        _currentNode = initialNode;
    }

    public static Solver Create(int[] board)
    {
        ArgumentNullException.ThrowIfNull(board);
        if (board.Length < Constants.CardCount)
            throw new ArgumentException($"Board length must be at least {Constants.CardCount}.", nameof(board));

        return new(board, new());
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

    public bool TrySelectMove(out int move, out double evaluation)
    {
        int sign = Sign(_currentNode.GetSideToMove());
        if (IsTerminalNode(_currentNode, out double evaluationFromCurrentPlayerPerspective))
        {
            evaluation = sign * evaluationFromCurrentPlayerPerspective;
            return None(out move);
        }

        throw new NotImplementedException();
    }

    private static bool IsTerminalNode(Node node, out double evaluation) => throw new NotImplementedException();

    private static int Sign(int sideToMove) =>
        sideToMove switch { 0 => 1, 1 => -1, _ => ThrowUnreachableException() };

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static int ThrowUnreachableException() => throw new UnreachableException();
}
