using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.FormattableString;

namespace Okiya;

using SeedMoveScore = (int Seed, int Move, double Score);

internal static class Program
{
    private static async Task Main()
    {
        const int initialSeed = 2642;
        var stopwatch = new Stopwatch();
        List<SeedMoveScore> smsTuples = [];
        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromMinutes(10.0));
        CancellationToken cancellationToken = cts.Token;
        int[] cards = GC.AllocateUninitializedArray<int>(Constants.CardCount);
        var countFuture = Task.Run(Enumerate, cancellationToken);
#if false
        Task readLineFuture = Console.In.ReadLineAsync(cancellationToken).AsTask();
        await Task.WhenAny(countFuture, readLineFuture).ConfigureAwait(false);
        await cts.CancelAsync().ConfigureAwait(false);
#endif
        int gameCount = await countFuture.ConfigureAwait(false);
        Console.WriteLine($"Finished in {stopwatch.Elapsed}");
        Console.WriteLine($"{nameof(initialSeed)}: {initialSeed}");
        Console.WriteLine($"{nameof(gameCount)}: {gameCount}");
        Console.WriteLine(Invariant($"Games per minute: {gameCount / stopwatch.Elapsed.TotalMinutes}"));
        Console.WriteLine(Invariant($"Seconds per game: {stopwatch.Elapsed.TotalSeconds / gameCount}"));
        ILookup<int, SeedMoveScore> smsTuplesByOutcome = smsTuples.ToLookup(it => it.Score.CompareTo(0.0));
        Console.WriteLine("Outcomes:");
        foreach (IGrouping<int, SeedMoveScore> grouping in smsTuplesByOutcome)
        {
            int groupingCount = grouping.Count();
            decimal percentage = decimal.Divide(groupingCount, gameCount);
            Console.WriteLine($"\t{grouping.Key}:\t{groupingCount} ({percentage:P})");
        }

        SeedMoveScore worstResult = smsTuples.MinBy(it => it.Score);
        Console.WriteLine($"{nameof(worstResult)}: {worstResult}");

        ILookup<int, SeedMoveScore> smsTuplesByMove = smsTuplesByOutcome[1].ToLookup(it => it.Move);
        IOrderedEnumerable<IGrouping<int, SeedMoveScore>> smsTuplesByMoveOrdered =
            smsTuplesByMove.OrderBy(it => it.Key);
        Console.WriteLine("Moves:");
        foreach (IGrouping<int, SeedMoveScore> grouping in smsTuplesByMoveOrdered)
            Console.WriteLine($"\t{grouping.Key}:\t{grouping.Count()}");

        ILookup<bool, SeedMoveScore> smsTuplesByMoveClass = smsTuplesByOutcome[1].ToLookup(it => IsCorner(it.Move));
        Console.WriteLine("Move classes:");
        foreach (IGrouping<bool, SeedMoveScore> grouping in smsTuplesByMoveClass)
        {
            string moveClass = grouping.Key ? "Corner" : "Other";
            int groupingCount = grouping.Count();
            decimal percentage = decimal.Divide(groupingCount, smsTuplesByOutcome[1].Count());
            Console.WriteLine($"\t{moveClass}:\t{groupingCount} ({percentage:P})");
        }

        return;

        int Enumerate()
        {
            stopwatch.Restart();
            try
            {
                int iterationCount = 0;
                for (int seed = initialSeed; !cancellationToken.IsCancellationRequested; ++seed, ++iterationCount)
                {
                    for (int i = 0; i < cards.Length; ++i)
                        cards[i] = i;
                    Random prng = new(seed);
                    prng.Shuffle(cards);
                    var game = Game.Create(cards);
                    Node rootNode = new();
                    var solver = Solver.Create(game, rootNode);
                    if (!solver.TrySelectMove(out int move, out double score))
                        throw new InvalidOperationException();
                    smsTuples.Add((seed, move, score));
                }

                return iterationCount;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        static bool IsCorner(int m)
        {
            return m is 0 or 3 or 12 or 15;
        }
    }
}
