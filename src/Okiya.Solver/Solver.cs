using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Okiya;

using MoveEvalPair = KeyValuePair<int, double>;

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

    public double Solve(out ImmutableStack<MoveEvalPair> moveEvaluationStack) =>
        Solve(new(), out moveEvaluationStack);

    private double Solve(Node rootNode, out ImmutableStack<MoveEvalPair> moveEvaluationStack) =>
        Negamax(rootNode, 1, ImmutableStack<MoveEvalPair>.Empty, out moveEvaluationStack);

    private double Negamax(Node node, int sideToMove, ImmutableStack<MoveEvalPair> inputStack,
        out ImmutableStack<MoveEvalPair> outputStack)
    {
        if (IsTerminalNode(node, out double evaluation))
            throw new NotImplementedException();

        throw new NotImplementedException();
    }

    private bool IsTerminalNode(Node node, out double evaluation) => throw new NotImplementedException();
}
