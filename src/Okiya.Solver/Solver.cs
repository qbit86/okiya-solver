using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Okiya;

// https://en.wikipedia.org/wiki/Negamax

public sealed class Solver
{
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

    private bool IsTerminalNode(Node node, out double evaluation) => throw new NotImplementedException();
}
