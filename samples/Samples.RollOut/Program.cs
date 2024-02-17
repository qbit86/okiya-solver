using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
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

        string outputDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            nameof(Okiya), Assembly.GetExecutingAssembly().GetName().Name);
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);
        string outputBaseName = $"{DateTime.Now:dd_HH-mm-ss}.html";
        string outputPath = Path.Join(outputDir, outputBaseName);
        Console.WriteLine($"{nameof(outputPath)}: {outputPath}");

        XDocument document = HtmlHelpers.CreateHtmlDocument(out _, out _);
        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true };
        using var writer = XmlWriter.Create(outputPath, settings);
        document.Save(writer);
    }
}
