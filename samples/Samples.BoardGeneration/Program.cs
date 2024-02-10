using System;
using System.Linq;

namespace Okiya;

internal static class Program
{
    private static readonly ConsoleColor[] s_suitColors =
        [ConsoleColor.Green, ConsoleColor.Blue, ConsoleColor.Red, ConsoleColor.Black];

    private static void Main()
    {
        Random random = new(1729);
        int[] cards = Enumerable.Range(0, Constants.CardCount).ToArray();
        random.Shuffle(cards);
        const int columnCount = 4;
        int rowCount = (cards.Length + columnCount - 1) / columnCount;
        for (int rowIndex = 0, cardIndex = 0; rowIndex < rowCount; ++rowIndex)
        {
            if (rowIndex > 0)
                Console.WriteLine();
            for (int columnIndex = 0; columnIndex < columnCount; ++columnIndex, ++cardIndex)
            {
                if (columnIndex > 0)
                    Console.Write(' ');

                int card = cards[cardIndex];
                string s = Int32CardConcept.Instance.ToString(card);
                ConsoleColor foregroundColor = s_suitColors[Int32CardConcept.Instance.Suit(card)];
                Write(s, foregroundColor);
            }
        }

        Console.WriteLine();
    }

    private static void Write(string s, ConsoleColor foregroundColor)
    {
        ConsoleColor backgroundColorToRestore = Console.BackgroundColor;
        ConsoleColor foregroundColorToRestore = Console.ForegroundColor;
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = foregroundColor;
        try
        {
            Console.Write(s);
        }
        finally
        {
            Console.ForegroundColor = foregroundColorToRestore;
            Console.BackgroundColor = backgroundColorToRestore;
        }
    }
}
