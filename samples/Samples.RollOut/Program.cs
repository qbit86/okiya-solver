using System;
using System.Diagnostics;
using System.Linq;
using static System.FormattableString;

namespace Okiya;

internal static class Program
{
    private static void Main()
    {
        Random random = new(42);
        int[] cards = Enumerable.Range(0, Constants.CardCount).ToArray();
        random.Shuffle(cards);
        var stopwatch = Stopwatch.StartNew();
#if true
        Solver solver = new(cards);
        Span<int> buffer = stackalloc int[cards.Length];
        double evaluation = solver.Solve(buffer, out int moveCount);
        ReadOnlySpan<int> moves = buffer[..moveCount];
#else
        const double evaluation = -2147483634.0;
        ReadOnlySpan<int> moves = stackalloc int[] { 0, 3, 2, 1, 5, 10, 4, 9, 6, 15, 11, 13, 12, 7 };
        int moveCount = moves.Length;
#endif
        stopwatch.Stop();
        Console.WriteLine($"Finished in {stopwatch.Elapsed}");
        Console.WriteLine(Invariant($"{nameof(evaluation)}: {evaluation}"));
        Console.WriteLine(Invariant($"{nameof(moveCount)}: {moveCount}"));
        Console.WriteLine($"{nameof(moves)}:");
        ReadOnlySpan<int>.Enumerator enumerator = moves.GetEnumerator();
        for (int i = 0; enumerator.MoveNext(); ++i)
        {
            int move = enumerator.Current;
            Console.WriteLine($"\t{i}.\t{move}\t{Int32CardConcept.Instance.ToString(cards[move])}");
        }
    }
}
