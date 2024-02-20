using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using static Okiya.TryHelpers;

namespace Okiya;

// https://en.wikipedia.org/wiki/Negamax

[Obsolete("Use Okiya.Experimental.Solver instead.")]
public sealed class Solver
{
    private readonly int[] _board;

    public Solver(int[] board)
    {
        ArgumentNullException.ThrowIfNull(board);
        _board = board;
    }

    public override string ToString() => string.Join(' ', _board.Select(Int32CardConcept.Instance.ToString));

    public double Solve(Span<int> moves, out int movesWritten)
    {
        double result = Solve(new(), out ImmutableStack<int> moveStack);
        ImmutableStack<int>.Enumerator enumerator = moveStack.GetEnumerator();
        for (movesWritten = 0; movesWritten < moves.Length && enumerator.MoveNext(); ++movesWritten)
            moves[movesWritten] = enumerator.Current;
        return result;
    }

    public double Solve<TMoveCollection>(TMoveCollection moves)
        where TMoveCollection : ICollection<int>
    {
        if (moves is null)
            throw new ArgumentNullException(nameof(moves));
        double result = Solve(new(), out ImmutableStack<int> moveStack);
        foreach (int move in moveStack)
            moves.Add(move);
        return result;
    }

    private double Solve(Node rootNode, out ImmutableStack<int> moves) =>
        Negamax(rootNode, ImmutableStack<int>.Empty, out moves);

    private double Negamax(Node node, ImmutableStack<int> inputStack,
        out ImmutableStack<int> outputStack)
    {
        if (IsTerminalNode(node, out double evaluation))
        {
            outputStack = inputStack;
            return evaluation;
        }

        int[] buffer = ArrayPool<int>.Shared.Rent(_board.Length);
        try
        {
            int possibleMoveCount = PopulatePossibleMoves(node, buffer);
            if (possibleMoveCount is 0)
            {
                int tokenCount = node.GetTokenCount();
                outputStack = inputStack;
                return node.GetSideToMove() is 0 ? int.MinValue + tokenCount : int.MaxValue - tokenCount;
            }

            ReadOnlySpan<int> possibleMoves = buffer.AsSpan()[..possibleMoveCount];
            double bestEvaluation = double.NegativeInfinity;
            int bestMove = int.MinValue;
            ImmutableStack<int> bestMoveStack = ImmutableStack<int>.Empty;
            foreach (int move in possibleMoves)
            {
                Node child = node.AddPlayerToken(move, _board[move]);
                double candidateEvaluation = -Negamax(child, inputStack, out ImmutableStack<int> candidateMoveStack);
                if (candidateEvaluation > bestEvaluation)
                {
                    bestEvaluation = candidateEvaluation;
                    bestMove = move;
                    bestMoveStack = candidateMoveStack;
                }
            }

            outputStack = bestMoveStack.Push(bestMove);
            return bestEvaluation;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer);
        }
    }

    private int PopulatePossibleMoves(Node node, Span<int> destination)
    {
        if (!node.TryGetCard(out int lastCard))
            return PopulatePossibleFirstMoves(destination);
        Int32CardConcept c = Int32CardConcept.Instance;
        int tokensPlayed = node.GetPlayerTokens(0) | node.GetPlayerTokens(1);
        int moveCount = 0;
        for (int i = 0; i < _board.Length; ++i)
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

    private int PopulatePossibleFirstMoves(Span<int> destination)
    {
        int moveCount = 0;
        for (int i = 0; i < _board.Length; ++i)
        {
            if (i is 5 or 6 or 9 or 10)
                continue;
            destination[moveCount++] = i;
        }

        return moveCount;
    }

    private static bool IsTerminalNode(Node node, out double evaluation)
    {
        (int sideToMove, int playerTokens) = node.GetSideToMoveAndPlayerTokens();
        int opponent = (sideToMove ^ 1) & 1;
        int opponentTokens = node.GetPlayerTokens(opponent);
        if (RuleHelpers.IsWinning(opponentTokens))
        {
            double tokenCount = node.GetTokenCount();
            evaluation = sideToMove is 0 ? int.MinValue - tokenCount : int.MaxValue + tokenCount;
            return true;
        }

        Debug.Assert(RuleHelpers.IsNotWinning(playerTokens));

        if (node.IsFull())
            return Some(0.0, out evaluation);

        evaluation = double.NaN;
        return false;
    }
}
