using System;
using System.Collections.Immutable;
using System.Linq;
using static System.FormattableString;

namespace Okiya;

internal static class Program
{
    private static void Main()
    {
        Random random = new(1729);
        int[] cards = Enumerable.Range(0, Constants.CardCount).ToArray();
        random.Shuffle(cards);
        Solver solver = new(cards);
        double evaluation = solver.Solve(out ImmutableStack<int> moves);
        Console.WriteLine(Invariant($"{nameof(evaluation)}: {evaluation}"));
        Console.WriteLine(Invariant($"{nameof(moves)}: {moves.Count()}"));
    }
}
