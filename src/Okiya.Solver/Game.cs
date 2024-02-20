using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Okiya.TryHelpers;

namespace Okiya;

public static class Game
{
    private static readonly int[] s_winPatterns = CreateWinPatters();

    public static Game<TCardCollection> Create<TCardCollection>(TCardCollection board)
        where TCardCollection : IReadOnlyList<int>
    {
        if (board is null)
            throw new ArgumentNullException(nameof(board));
        if (board.Count < Constants.CardCount)
            throw new ArgumentException($"Board length must be at least {Constants.CardCount}.", nameof(board));

        return new(board);
    }

    internal static int PopulatePossibleFirstMoves(Span<int> destination)
    {
        int maxMoveCount = int.Min(destination.Length, Constants.CardCount);
        int moveCount = 0;
        for (int i = 0; i < maxMoveCount; ++i)
        {
            if (i is 5 or 6 or 9 or 10)
                continue;
            destination[moveCount++] = i;
        }

        return moveCount;
    }

    internal static bool IsTerminalNode(Node node, out double score)
    {
        (int playerTokens, int opponentTokens) = node.GetPlayersTokens();
        Debug.Assert(IsNotWinning(playerTokens));
        if (IsWinning(opponentTokens))
            return Some(-sbyte.MaxValue + node.GetTokenCount(), out score);

        if (node.IsFull())
            return Some(0.0, out score);

        score = double.NaN;
        return false;
    }

    internal static bool IsTerminalRoot(Node node, out double score)
    {
        (int playerTokens, int opponentTokens) = node.GetPlayersTokens();
        if (IsWinning(playerTokens))
            return Some(sbyte.MaxValue - node.GetTokenCount(), out score);

        if (IsWinning(opponentTokens))
            return Some(-sbyte.MaxValue + node.GetTokenCount(), out score);

        if (node.IsFull())
            return Some(0.0, out score);

        score = double.NaN;
        return false;
    }

    private static bool IsWinning(int tokens)
    {
        // return s_winPatterns.Any(pattern => (pattern & tokens) == pattern);
        foreach (int pattern in s_winPatterns)
        {
            if ((pattern & tokens) == pattern)
                return true;
        }

        return false;
    }

    private static bool IsNotWinning(int tokens)
    {
        // return s_winPatterns.All(pattern => (pattern & tokens) < pattern);
        foreach (int pattern in s_winPatterns)
        {
            if ((pattern & tokens) == pattern)
                return false;
        }

        return true;
    }

    private static int[] CreateWinPatters() =>
    [
        0b11110000_00000000,
        0b00001111_00000000,
        0b00000000_11110000,
        0b00000000_00001111,

        0b10001000_10001000,
        0b01000100_01000100,
        0b00100010_00100010,
        0b00010001_00010001,

        0b10000100_00100001,
        0b00010010_01001000,

        0b00000000_00110011,
        0b00000000_01100110,
        0b00000000_11001100,

        0b00000011_00110000,
        0b00000110_01100000,
        0b00001100_11000000,

        0b00110011_00000000,
        0b01100110_00000000,
        0b11001100_00000000
    ];
}
