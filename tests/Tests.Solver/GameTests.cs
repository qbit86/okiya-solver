using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Okiya;

public sealed class GameTests
{
    private static readonly int[] s_cards = CreateCards();

    public static TheoryData<int[], Node, int> IllegalMoveData { get; } = CreateIllegalMoveData();

    public static TheoryData<int[], Node, int, Node> LegalMoveData { get; } = CreateLegalMoveData();

    public static TheoryData<int[], Node, IEnumerable<int>> PopulateLegalMovesData { get; } =
        CreatePopulateLegalMovesData();

    [Theory]
    [MemberData(nameof(IllegalMoveData), MemberType = typeof(GameTests))]
    public void TryMakeMove_WhenIllegalMove_ReturnsFalse(int[] cards, Node node, int move)
    {
        var game = Game.Create(cards);
        bool actual = game.TryMakeMove(node, move, out _);
        Assert.False(actual);
    }

    [Theory]
    [MemberData(nameof(LegalMoveData), MemberType = typeof(GameTests))]
    public void TryMakeMove_WhenLegalMove_ReturnsTrue(int[] cards, Node node, int move, Node expectedChild)
    {
        var game = Game.Create(cards);
        bool actual = game.TryMakeMove(node, move, out Node actualChild);
        Assert.True(actual);
        Assert.Equal(expectedChild.FirstPlayerTokens, actualChild.FirstPlayerTokens);
        Assert.Equal(expectedChild.SecondPlayerTokens, actualChild.SecondPlayerTokens);
        Assert.Equal(expectedChild.SideToMove, actualChild.SideToMove);
        Assert.Equal(expectedChild.GetCardIndex(), actualChild.GetCardIndex());
    }

    [Theory]
    [MemberData(nameof(PopulateLegalMovesData))]
    public void PopulateLegalMoves(int[] cards, Node node, IEnumerable<int> expectedMoves)
    {
        var game = Game.Create(cards);
        HashSet<int> actualMoves = [];
        game.PopulateLegalMoves(node, actualMoves);
        Assert.True(actualMoves.SetEquals(expectedMoves));
    }

    private static int[] CreateCards()
    {
        string[] cardStrings =
        [
            "K♥", "J♠", "J♥", "Q♣",
            "Q♦", "J♣", "K♠", "A♠",
            "J♦", "Q♠", "K♦", "Q♥",
            "K♣", "A♦", "A♥", "A♣"
        ];
        return cardStrings.Select(Int32CardConcept.Instance.Parse).ToArray();
    }

    private static TheoryData<int[], Node, int> CreateIllegalMoveData() => new()
    {
        { s_cards, new(), 5 },
        { s_cards, Node.Create(1, 0, 1, 0), 5 },
        { s_cards, Node.Create(1, 0, 1, 0), 0 }
    };

    private static TheoryData<int[], Node, int, Node> CreateLegalMoveData() => new()
    {
        { s_cards, new(), 1, Node.Create(0b10, 0, 1, 1) },
        {
            s_cards,
            Node.Create(0b100000000010000, 0b100000000000, 1, 4),
            9,
            Node.Create(0b100000000010000, 0b101000000000, 0, 9)
        }
    };

    private static TheoryData<int[], Node, IEnumerable<int>> CreatePopulateLegalMovesData() => new()
    {
        { s_cards, new(), [0, 1, 2, 3, 4, 7, 8, 11, 12, 13, 14, 15] },
        { s_cards, Node.Create(1 << 14, 0, 1, 14), [0, 2, 7, 11, 13, 15] },
        { s_cards, Node.Create(1 << 14, 1 << 11, 0, 11), [0, 2, 3, 4, 9] }
    };
}
