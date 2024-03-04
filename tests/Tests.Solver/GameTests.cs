using System.Linq;
using Xunit;

namespace Okiya;

public sealed class GameTests
{
    public static TheoryData<int[], Node, int> IllegalMoveTheoryData { get; } = CreateIllegalMoveTheoryData();

    [Theory]
    [MemberData(nameof(IllegalMoveTheoryData), MemberType = typeof(GameTests))]
    public void TryMakeMove_WhenIllegalMove_ReturnsFalse(int[] cards, Node node, int move)
    {
        var game = Game.Create(cards);
        bool actual = game.TryMakeMove(node, move, out _);
        Assert.False(actual);
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
}
