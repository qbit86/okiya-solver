using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Okiya;

using SeedScorePair = (int Seed, double Score);

internal static class Program
{
    private static async Task Main()
    {
        const int initialSeed = 1;
        var stopwatch = Stopwatch.StartNew();
        List<SeedScorePair> seedScorePairs = [];
        using CancellationTokenSource cts = new();
        CancellationToken cancellationToken = cts.Token;
        int[] cards = GC.AllocateUninitializedArray<int>(Constants.CardCount);
        var countFuture = Task.Run(Enumerate, default);
        Console.ReadLine();
        await cts.CancelAsync().ConfigureAwait(false);
        int gameCount = await countFuture.ConfigureAwait(false);
        stopwatch.Stop();
        Console.WriteLine($"Finished in {stopwatch.Elapsed}");
        Console.WriteLine($"{nameof(gameCount)}: {gameCount}");
        return;

        int Enumerate()
        {
            int iterationCount = 0;
            for (int seed = initialSeed; !cancellationToken.IsCancellationRequested; ++seed, ++iterationCount)
            {
                Random random = new(seed);
                for (int i = 0; i < cards.Length; ++i)
                    cards[i] = i;
                random.Shuffle(cards);
                var game = Game.Create(cards);
                Node rootNode = new();
                var solver = Solver.Create(game, rootNode);
                if (!solver.TrySelectMove(out _, out double score))
                    throw new InvalidOperationException();
                seedScorePairs.Add((seed, score));
            }

            return iterationCount;
        }
    }
}
