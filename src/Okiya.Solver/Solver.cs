using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        throw new NotImplementedException();
    }

    private int PopulatePossibleMoves(Node node, Span<int> moves) => throw new NotImplementedException();

    private static bool IsTerminalNode(Node node, out double evaluation)
    {
        (int sideToMove, int playerTokens) = node.GetSideToMoveAndPlayerTokens();
        if (s_winPatterns.Any(p => (p & playerTokens) == p))
        {
            evaluation = sideToMove is 0 ? double.PositiveInfinity : double.NegativeInfinity;
            return true;
        }

        if (node.IsFull())
        {
            evaluation = 0;
            return true;
        }

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
