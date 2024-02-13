﻿using System;
using System.Collections.Immutable;
using System.Diagnostics;
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
        var stopwatch = Stopwatch.StartNew();
        double evaluation = solver.Solve(out ImmutableStack<int> moves);
        stopwatch.Stop();
        Console.WriteLine($"Finished in {stopwatch.Elapsed}");
        Console.WriteLine(Invariant($"{nameof(evaluation)}: {evaluation}"));
        Console.WriteLine(Invariant($"{nameof(moves)}.Count: {moves.Count()}"));
        Console.WriteLine($"{nameof(moves)}:");
        foreach (int move in moves)
            Console.WriteLine($"\t{move} ({Int32CardConcept.Instance.ToString(cards[move])})");
    }
}