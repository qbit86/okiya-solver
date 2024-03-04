using System.Linq;
using Xunit;

namespace Okiya;

public sealed class GameTests
{
    public static TheoryData<int[], Node, int> IllegalMoveTheoryData { get; } = CreateIllegalMoveTheoryData();

    public static TheoryData<int[], Node, int, Node> LegalMoveTheoryData { get; } = CreateLegalMoveTheoryData();

    [Theory]
    [MemberData(nameof(IllegalMoveTheoryData), MemberType = typeof(GameTests))]
    public void TryMakeMove_WhenIllegalMove_ReturnsFalse(int[] cards, Node node, int move)
    {
        var game = Game.Create(cards);
        bool actual = game.TryMakeMove(node, move, out _);
        Assert.False(actual);
    }

    [Theory]
    [MemberData(nameof(LegalMoveTheoryData), MemberType = typeof(GameTests))]
    public void TryMakeMove_WhenLegalMove_ReturnsFalse(int[] cards, Node node, int move, Node expectedChild)
    {
        var game = Game.Create(cards);
        bool actual = game.TryMakeMove(node, move, out Node actualChild);
        Assert.True(actual);
        Assert.Equal(expectedChild, actualChild);
    }

    private static TheoryData<int[], Node, int> CreateIllegalMoveTheoryData()
    {
        string[] cardStrings =
        [
            "K♥", "J♠", "J♥", "Q♣",
            "Q♦", "J♣", "K♠", "A♠",
            "J♦", "Q♠", "K♦", "Q♥",
            "K♣", "A♦", "A♥", "A♣"
        ];
        int[] cards = cardStrings.Select(Int32CardConcept.Instance.Parse).ToArray();

        return new()
        {
            { cards, new(), 5 },
            { cards, Node.Create(1, 0, 1, 0), 5 },
            { cards, Node.Create(1, 0, 1, 0), 0 }
        };
    }

    private static TheoryData<int[], Node, int, Node> CreateLegalMoveTheoryData()
    {
        string[] cardStrings =
        [
            "K♥", "J♠", "J♥", "Q♣",
            "Q♦", "J♣", "K♠", "A♠",
            "J♦", "Q♠", "K♦", "Q♥",
            "K♣", "A♦", "A♥", "A♣"
        ];
        int[] cards = cardStrings.Select(Int32CardConcept.Instance.Parse).ToArray();

        return new()
        {
            { cards, new(), 1, Node.Create(0b10, 0, 1, 1) }
        };
    }
}
