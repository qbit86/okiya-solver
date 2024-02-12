using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using static Okiya.TryHelpers;

namespace Okiya;

// https://en.wikipedia.org/wiki/Negamax

public sealed class Solver
{
    private static readonly int[] s_winPatterns = CreateWinPatters();
    private readonly int[] _board;

    public Solver(int[] board)
    {
        ArgumentNullException.ThrowIfNull(board);
        _board = board;
    }

    public IReadOnlyList<int> Board => _board;

    public double Solve(out ImmutableStack<int> moveEvaluationStack) =>
        Solve(new(), out moveEvaluationStack);

    private double Solve(Node rootNode, out ImmutableStack<int> moveEvaluationStack) =>
        Negamax(rootNode, ImmutableStack<int>.Empty, out moveEvaluationStack);

    private double Negamax(Node node, ImmutableStack<int> inputStack,
        out ImmutableStack<int> outputStack)
    {
        if (IsTerminalNode(node, out double evaluation))
        {
            outputStack = inputStack;
            return evaluation;
        }

        int[] buffer = ArrayPool<int>.Shared.Rent(Constants.CardCount);
        try
        {
            int possibleMoveCount = PopulatePossibleMoves(node, buffer);
            if (possibleMoveCount is 0)
            {
                outputStack = inputStack;
                return node.GetSideToMove() is 0 ? double.NegativeInfinity : double.PositiveInfinity;
            }

            ReadOnlySpan<int> possibleMoves = buffer.AsSpan()[..possibleMoveCount];
            double currentMaxEvaluation = double.NegativeInfinity;
            int currentBestMove = int.MinValue;
            ImmutableStack<int> currentMoveStack = ImmutableStack<int>.Empty;
            foreach (int move in possibleMoves)
            {
                Node child = node.AddPlayerToken(move, _board[move]);
                double candidateEvaluation = -Negamax(child, inputStack, out ImmutableStack<int> candidateMoveStack);
                if (candidateEvaluation > currentMaxEvaluation)
                {
                    currentMaxEvaluation = candidateEvaluation;
                    currentBestMove = move;
                    currentMoveStack = candidateMoveStack;
                }
            }

            outputStack = currentMoveStack.Push(currentBestMove);
            return currentMaxEvaluation;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer);
        }
    }

    private int PopulatePossibleMoves(Node node, Span<int> destination) => throw new NotImplementedException();

    private static bool IsTerminalNode(Node node, out double evaluation)
    {
        (int sideToMove, int playerTokens) = node.GetSideToMoveAndPlayerTokens();
        int opponent = (sideToMove ^ 1) & 1;
        int opponentTokens = node.GetPlayerTokens(opponent);
        if (s_winPatterns.Any(p => (p & opponentTokens) == p))
        {
            evaluation = sideToMove is 0 ? double.NegativeInfinity : double.PositiveInfinity;
            return true;
        }

        Debug.Assert(s_winPatterns.All(p => (p & playerTokens) < p));

        if (node.IsFull())
            return Some(0.0, out evaluation);

        evaluation = double.NaN;
        return false;
    }

    private static int[] CreateWinPatters() =>
    [
        0b11110000_00000000,
        0b00001111_00000000,
        0b00000000_11110000,
        0b00000000_00001111,

        0b10001000_10001000,
        0b01000100_01000100,
        0b00100010_00100010,
        0b00010001_00010001,

        0b10000100_00100001,
        0b00010010_01001000,

        0b00000000_00110011,
        0b00000000_01100110,
        0b00000000_11001100,

        0b00000011_00110000,
        0b00000110_01100000,
        0b00001100_11000000,

        0b00110011_00000000,
        0b01100110_00000000,
        0b11001100_00000000
    ];
}
