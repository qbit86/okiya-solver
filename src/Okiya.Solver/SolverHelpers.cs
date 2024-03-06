using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Okiya;

internal static class SolverHelpers
{
    internal static double Negamax(Game<int[]> game, Node node)
    {
        if (Game.IsTerminalNode(node, out double score))
            return score;

        int[] buffer = ArrayPool<int>.Shared.Rent(Constants.CardCount);
        try
        {
            int possibleMoveCount = game.PopulateLegalMoves(node, buffer.AsSpan());
            if (possibleMoveCount is 0)
                return -sbyte.MaxValue + node.GetTokenCount();

            ReadOnlySpan<int> possibleMoves = buffer.AsSpan()[..possibleMoveCount];
            double bestScore = double.NegativeInfinity;
            foreach (int moveCandidate in possibleMoves)
            {
                Node child = game.MakeMoveUnchecked(node, moveCandidate);
                double scoreCandidate = -Negamax(game, child);
                if (scoreCandidate > bestScore)
                    bestScore = scoreCandidate;
            }

            return bestScore;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer);
        }
    }

    internal static T Mod<T>(T dividend, T divisor)
        where T : IAdditionOperators<T, T, T>, IModulusOperators<T, T, T> =>
        (dividend % divisor + divisor) % divisor;

    internal static int Sign(int sideToMove) =>
        sideToMove switch { 0 => 1, 1 => -1, _ => ThrowArgumentOutOfRangeException(sideToMove) };

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ThrowArgumentOutOfRangeException(int sideToMove)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sideToMove);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sideToMove, 1);
        return default;
    }
}
