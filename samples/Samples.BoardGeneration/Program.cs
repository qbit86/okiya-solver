using System;
using System.Linq;

namespace Okiya;

internal static class Program
{
    private static readonly ConsoleColor[] s_suitColors =
        [ConsoleColor.Green, ConsoleColor.Blue, ConsoleColor.Red, ConsoleColor.Black];

    private static void Main()
    {
        int[] cards = Enumerable.Range(0, Constants.CardCount).ToArray();
        Random.Shared.Shuffle(cards);
        for (int i = 0; i < cards.Length; ++i)
        {
            (int quotient, int remainder) = Math.DivRem(i, 4);
            if (remainder is 0)
            {
                if (quotient > 0)
                    Console.WriteLine();
            }
            else
            {
                Console.Write(' ');
            }

            int card = cards[i];
            string s = Int32CardConcept.Instance.ToString(card);
            ConsoleColor foregroundColor = s_suitColors[Int32CardConcept.Instance.Suit(card)];
            Write(s, foregroundColor);
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
