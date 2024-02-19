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

    private Solver(int[] board, Node rootNode)
    {
        _board = board;
        _currentNode = rootNode;
    }

    public static Solver Create(int[] board)
    {
        ArgumentNullException.ThrowIfNull(board);
        if (board.Length < Constants.CardCount)
            throw new ArgumentException($"Board length must be at least {Constants.CardCount}.", nameof(board));

        return new(board, new());
    }

    public static Solver Create(
        int[] board, int firstPlayerTokens, int secondPlayerTokens, int sideToMove, int lastCard = default)
    {
        ArgumentNullException.ThrowIfNull(board);
        if (board.Length < Constants.CardCount)
            throw new ArgumentException($"Board length must be at least {Constants.CardCount}.", nameof(board));
        uint firstPlayerTokensChecked = (uint)firstPlayerTokens;
        if (firstPlayerTokensChecked > Constants.PlayerTokensMask)
            throw new ArgumentOutOfRangeException(nameof(firstPlayerTokens));
        uint secondPlayerTokensChecked = (uint)secondPlayerTokens;
        if (secondPlayerTokensChecked > Constants.PlayerTokensMask)
            throw new ArgumentOutOfRangeException(nameof(secondPlayerTokens));
        if ((firstPlayerTokensChecked & secondPlayerTokensChecked) is not 0)
            throw new ArgumentException("Players' tokens may not overlap.", nameof(secondPlayerTokens));
        if (sideToMove is not (0 or 1))
            throw new ArgumentOutOfRangeException(nameof(sideToMove));
        if ((uint)lastCard >= Constants.CardCount)
            throw new ArgumentOutOfRangeException(nameof(lastCard));

        var rootNode = Node.CreateUnchecked(firstPlayerTokensChecked, secondPlayerTokensChecked, sideToMove, lastCard);
        return new(board, rootNode);
    }

    public bool TrySelectMove(out int move, out double score)
    {
        if (IsTerminalNode(_currentNode, out double relativeScore))
        {
            int sign = Sign(_currentNode.GetSideToMove());
            score = sign * relativeScore;
            return None(out move);
        }

        throw new NotImplementedException();
    }

    private static bool IsTerminalNode(Node node, out double score)
    {
        (int playerTokens, int opponentTokens) = node.GetPlayersTokens();
        if (RuleHelpers.IsWinning(opponentTokens))
        {
            double tokenCount = node.GetTokenCount();
            score = -sbyte.MaxValue + tokenCount;
            return true;
        }

        Debug.Assert(RuleHelpers.IsNotWinning(playerTokens));

        if (node.IsFull())
            return Some(0.0, out score);

        score = double.NaN;
        return false;
    }

    private static int Sign(int sideToMove) =>
        sideToMove switch { 0 => 1, 1 => -1, _ => ThrowUnreachableException() };

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static int ThrowUnreachableException() => throw new UnreachableException();
}
