using System;
using System.Collections.Generic;
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
        const int seed = 310;
        Random prng = new(seed);
        int[] cards = Enumerable.Range(0, Constants.CardCount).ToArray();
        prng.Shuffle(cards);

        var stopwatch = Stopwatch.StartNew();
        var game = Game.Create(cards);
        Node rootNode = new();
        var solver = Solver.Create(game, rootNode, true);
        Span<int> buffer = stackalloc int[cards.Length];
        double score = solver.MakeMoves(buffer, out int moveCount);
        ReadOnlySpan<int> moves = buffer[..moveCount];
        stopwatch.Stop();

        Console.WriteLine($"Finished in {stopwatch.Elapsed}");
        Console.WriteLine(Invariant($"{nameof(score)}: {score}"));
        Console.WriteLine(Invariant($"{nameof(moveCount)}: {moveCount}"));
        Console.WriteLine($"{nameof(moves)}:");
        for (int i = 0; i < moves.Length; ++i)
        {
            int move = moves[i];
            Console.WriteLine($"\t{i}.\t{move}\t{Int32CardConcept.Instance.ToString(cards[move])}");
        }

        XDocument document = HtmlHelpers.CreateHtmlDocument(out XElement title, out XElement body);

        title.Add($"{nameof(Okiya)} - {seed}, {OutcomeString(score)}");

        {
            XElement table = CreatePositionTable(game, rootNode);
            body.Add(table);
        }

        Node currentNode = rootNode;
        foreach (int move in moves)
        {
            currentNode = game.MakeMove(currentNode, move);
            XElement table = CreatePositionTable(game, currentNode, true);
            body.Add(table);
        }

        string outputDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            nameof(Okiya), Assembly.GetExecutingAssembly().GetName().Name);
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);
        string outputBaseName = $"{DateTime.Now:dd_HH-mm-ss}-{seed}-{OutcomeChar(score)}.html";
        string outputPath = Path.Join(outputDir, outputBaseName);
        Console.WriteLine($"{nameof(outputPath)}: {outputPath}");
        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true };
        using var writer = XmlWriter.Create(outputPath, settings);
        document.Save(writer);
    }

    private static XElement CreatePositionTable(Game<int[]> game, Node node, bool addVerticalSpace = false)
    {
        Debug.Assert(game.IsValid);
        XElement table = new("table", new XAttribute("class", "board"));
        if (addVerticalSpace)
            table.Add(new XAttribute("style", "margin-top: 3em;"));
        XElement tableBody = new("tbody");
        table.Add(tableBody);

        const int columnCount = 4;

        {
            XElement tableFooter = new("thead");
            table.Add(tableFooter);
            XElement tr = new("tr");
            tableFooter.Add(tr);
            int targetColumnIndex = node.SideToMove is 0 ? 3 : 0;
            for (int columnIndex = 0; columnIndex < columnCount; ++columnIndex)
            {
                XElement td = new("td");
                tr.Add(td);
                if (columnIndex != targetColumnIndex)
                    continue;
                if (!node.TryGetCardIndex(out int cardIndex))
                {
                    td.Add(new XElement("span", new XAttribute("class", "monospace unavailable"), "\u00a0\u00a0"));
                    continue;
                }

                int card = game.GetCard(cardIndex);
                string s = Int32CardConcept.Instance.ToString(card);
                string suitClass = s_suitClasses[Int32CardConcept.Instance.Suit(card)];
                td.Add(new XElement("span", new XAttribute("class", $"monospace {suitClass}"), s));
            }
        }

        const int rowCount = (Constants.CardCount + columnCount - 1) / columnCount;
        HashSet<int> moves = new(Constants.CardCount);
        game.PopulateLegalMoves(node, moves);
        for (int rowIndex = 0, cardIndex = 0; rowIndex < rowCount; ++rowIndex)
        {
            XElement tr = new("tr");
            tableBody.Add(tr);
            for (int columnIndex = 0; columnIndex < columnCount; ++columnIndex, ++cardIndex)
            {
                XElement td = new("td");
                tr.Add(td);
                int card = game.GetCard(cardIndex);
                string s = Int32CardConcept.Instance.ToString(card);
                string suitClass = s_suitClasses[Int32CardConcept.Instance.Suit(card)];
                int cardMask = 1 << cardIndex;
                bool hasFirst = (node.FirstPlayerTokens & cardMask) is not 0;
                bool hasSecond = (node.SecondPlayerTokens & cardMask) is not 0;
                Debug.Assert(!hasFirst || !hasSecond);
                if (hasFirst)
                    td.Add(new XElement("span", new XAttribute("class", "monospace"), '\u274c'));
                else if (hasSecond)
                    td.Add(new XElement("span", new XAttribute("class", "monospace"), '\u2b55'));
                else if (!moves.Contains(cardIndex))
                    td.Add(new XElement("span", new XAttribute("class", "monospace unavailable"), s));
                else
                    td.Add(new XElement("span", new XAttribute("class", $"monospace {suitClass}"), s));
            }
        }

        return table;
    }

    private static char OutcomeChar(double score) => score switch
    {
        < 0.0 => 'O',
        > 0.0 => 'X',
        _ => '='
    };

    private static string OutcomeString(double score) => score switch
    {
        < 0.0 => "O wins",
        > 0.0 => "X wins",
        _ => "draw"
    };
}
