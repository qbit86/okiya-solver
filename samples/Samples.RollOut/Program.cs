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
    private static readonly string[] s_suitClasses = ["suit0", "suit1", "suit2", "suit3"];

    private static void Main()
    {
        const int seed = 42;
        Random random = new(seed);
        int[] cards = Enumerable.Range(0, Constants.CardCount).ToArray();
        random.Shuffle(cards);
        var stopwatch = Stopwatch.StartNew();
#if true
        /*
        Solver solver = new(cards);
        Span<int> buffer = stackalloc int[cards.Length];
        double evaluation = solver.Solve(buffer, out int moveCount);
        ReadOnlySpan<int> moves = buffer[..moveCount];
        /*/
        var solver = Experimental.Solver.Create(cards);
        bool hasMove = solver.TrySelectMove(out int move, out double evaluation);
        if (hasMove)
            Console.WriteLine($"{nameof(move)}: {move}\t{Int32CardConcept.Instance.ToString(cards[move])}");
        //*/
#else
        const double evaluation = -2147483634.0;
        ReadOnlySpan<int> moves = stackalloc int[] { 0, 3, 2, 1, 5, 10, 4, 9, 6, 15, 11, 13, 12, 7 };
        int moveCount = moves.Length;
#endif
        stopwatch.Stop();
        Console.WriteLine($"Finished in {stopwatch.Elapsed}");
        Console.WriteLine(Invariant($"{nameof(evaluation)}: {evaluation}"));
#if false
        Console.WriteLine(Invariant($"{nameof(moveCount)}: {moveCount}"));
        Console.WriteLine($"{nameof(moves)}:");
        ReadOnlySpan<int>.Enumerator enumerator = moves.GetEnumerator();
        for (int i = 0; enumerator.MoveNext(); ++i)
        {
            int move = enumerator.Current;
            Console.WriteLine($"\t{i}.\t{move}\t{Int32CardConcept.Instance.ToString(cards[move])}");
        }
#endif

        XDocument document = HtmlHelpers.CreateHtmlDocument(out XElement title, out XElement body);

        title.Add($"{nameof(Okiya)} - {seed}");

        XElement boardTable = new("table", new XAttribute("class", "board"));
        body.Add(boardTable);

        const int columnCount = 4;
        int rowCount = (cards.Length + columnCount - 1) / columnCount;
        for (int rowIndex = 0, cardIndex = 0; rowIndex < rowCount; ++rowIndex)
        {
            XElement tr = new("tr");
            boardTable.Add(tr);
            for (int columnIndex = 0; columnIndex < columnCount; ++columnIndex, ++cardIndex)
            {
                int card = cards[cardIndex];
                string s = Int32CardConcept.Instance.ToString(card);
                string suitClass = s_suitClasses[Int32CardConcept.Instance.Suit(card)];
                XElement td = new("td");
                tr.Add(td);
                td.Add(new XElement("span", new XAttribute("class", $"monospace {suitClass}"), s));
            }
        }

        string outputDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            nameof(Okiya), Assembly.GetExecutingAssembly().GetName().Name);
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);
        string outputBaseName = $"{DateTime.Now:dd_HH-mm-ss}.html";
        string outputPath = Path.Join(outputDir, outputBaseName);
        Console.WriteLine($"{nameof(outputPath)}: {outputPath}");
        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true };
        using var writer = XmlWriter.Create(outputPath, settings);
        document.Save(writer);
    }
}
