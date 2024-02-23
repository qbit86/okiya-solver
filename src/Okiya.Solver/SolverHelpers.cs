using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
            int possibleMoveCount = game.PopulatePossibleMoves(node, buffer.AsSpan());
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

    internal static int Sign(int sideToMove) =>
        sideToMove switch { 0 => 1, 1 => -1, _ => ThrowUnreachableException() };

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static int ThrowUnreachableException() => throw new UnreachableException();
}
