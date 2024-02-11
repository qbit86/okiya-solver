using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace Okiya;

// https://en.wikipedia.org/wiki/Negamax

public sealed class Solver
{
    private static readonly uint[] s_winPatterns = CreateWinPatters();
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

    private static bool IsTerminalNode(Node node, out double evaluation)
    {
        (int sideToMove, int playerTokens) = node.GetSideToMoveAndPlayerTokens();
        if (!s_winPatterns.Any(p => BitOperations.PopCount(unchecked(p & (uint)playerTokens)) is 4))
        {
            evaluation = 0;
            return false;
        }

        evaluation = sideToMove is 0 ? double.MaxValue : double.MinValue;
        return true;
    }

    private static uint[] CreateWinPatters() =>
    [
        0b11110000_00000000u,
        0b00001111_00000000u,
        0b00000000_11110000u,
        0b00000000_00001111u,

        0b10001000_10001000u,
        0b01000100_01000100u,
        0b00100010_00100010u,
        0b00010001_00010001u,

        0b10000100_00100001u,
        0b00010010_01001000u,

        0b00000000_00110011u,
        0b00000000_01100110u,
        0b00000000_11001100u,

        0b00000011_00110000u,
        0b00000110_01100000u,
        0b00001100_11000000u,

        0b00110011_00000000u,
        0b01100110_00000000u,
        0b11001100_00000000u
    ];
}
