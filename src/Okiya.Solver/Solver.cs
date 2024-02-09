using System;
using System.Collections.Generic;

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

    public double Solve(Stack<KeyValuePair<int, double>> moveEvaluationStack) => Solve(new(), moveEvaluationStack);

    private double Solve(Node rootNode, Stack<KeyValuePair<int, double>> moveEvaluationStack) =>
        throw new NotImplementedException();
}
