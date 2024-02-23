using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Okiya;

using SeedMoveScore = (int Seed, int Move, double Score);

internal static class Program
{
    private static async Task Main()
    {
        const int initialSeed = 141;
        var stopwatch = new Stopwatch();
        List<SeedMoveScore> smsTuples = [];
        using CancellationTokenSource cts = new();
        CancellationToken cancellationToken = cts.Token;
        int[] cards = GC.AllocateUninitializedArray<int>(Constants.CardCount);
        var countFuture = Task.Run(Enumerate, cancellationToken);
        Task readLineFuture = Console.In.ReadLineAsync(cancellationToken).AsTask();
        await Task.WhenAny(countFuture, readLineFuture).ConfigureAwait(false);
        await cts.CancelAsync().ConfigureAwait(false);
        int gameCount = await countFuture.ConfigureAwait(false);
        Console.WriteLine($"Finished in {stopwatch.Elapsed}");
        Console.WriteLine($"{nameof(gameCount)}: {gameCount}");
        ILookup<int, SeedMoveScore> lookup = smsTuples.ToLookup(it => it.Score.CompareTo(0.0));
        foreach (IGrouping<int, SeedMoveScore> grouping in lookup)
            Console.WriteLine($"{grouping.Key}:\t{grouping.Count()}");

        SeedMoveScore worstSeedScorePair = smsTuples.MinBy(it => it.Score);
        Console.WriteLine($"{nameof(worstSeedScorePair)}: {worstSeedScorePair}");

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
                    Random random = new(seed);
                    random.Shuffle(cards);
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
    }
}
