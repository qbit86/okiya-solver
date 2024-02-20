using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Okiya;

public readonly record struct Node
{
    private const int PlayerTokensBitCount = 16;

    private const int CardIndexBitCount = 31;

    private const int CardIndexMask = 0x7fffffff;

    private readonly uint _cardIndexAndSideToMove;
    private readonly uint _playersTokens;

#if DEBUG
    static Node()
    {
        Debug.Assert(PlayerTokensBitCount is Constants.CardCount);
        Debug.Assert(PlayerTokensBitCount > 0);
        Debug.Assert(1 << PlayerTokensBitCount is Constants.PlayerTokensMask + 1);
        Debug.Assert(CardIndexBitCount > 0);
        Debug.Assert(1u << CardIndexBitCount is CardIndexMask + 1u);
    }
#endif

    private Node(uint playersTokens, uint cardIndexAndSideToMove)
    {
        _playersTokens = playersTokens;
        _cardIndexAndSideToMove = cardIndexAndSideToMove;
    }

    internal static Node CreateUnchecked(uint firstPlayerTokens, uint secondPlayerTokens, int sideToMove, int cardIndex)
    {
        Debug.Assert(firstPlayerTokens <= Constants.PlayerTokensMask);
        Debug.Assert(secondPlayerTokens <= Constants.PlayerTokensMask);
        Debug.Assert((firstPlayerTokens & secondPlayerTokens) is 0);
        Debug.Assert(sideToMove is 0 or 1);
        Debug.Assert((uint)cardIndex < Constants.CardCount);
        uint playersTokens = firstPlayerTokens | (secondPlayerTokens << Constants.CardCount);
        uint sideBit = sideToMove is 0 ? 0 : 1u << CardIndexBitCount;
        uint cardIndexAndSideToMove = sideBit | unchecked((uint)cardIndex);
        return new(playersTokens, cardIndexAndSideToMove);
    }

    private bool PrintMembers(StringBuilder builder)
    {
        CultureInfo p = CultureInfo.InvariantCulture;
        builder.Append(p, $"Players = {GetSecondPlayerTokens():b16}|{GetFirstPlayerTokens():b16}");
        builder.Append(p, $", CardIndex = {GetCardIndexOrDefault()}");
        builder.Append(p, $", SideToMove = {GetSideToMove()}");
        return true;
    }

    internal bool IsFull() => BitOperations.PopCount(_playersTokens) is PlayerTokensBitCount;

    internal int GetTokenCount() => BitOperations.PopCount(_playersTokens);

    internal int GetPlayerTokens(int side) => side is 0 ? GetFirstPlayerTokens() : GetSecondPlayerTokens();

    private int GetFirstPlayerTokens() => unchecked((int)(_playersTokens & Constants.PlayerTokensMask));

    private int GetSecondPlayerTokens() => unchecked((int)(_playersTokens >> PlayerTokensBitCount));

    private int GetCardIndexOrDefault() => unchecked((int)(_cardIndexAndSideToMove & CardIndexMask));

    internal bool TryGetCardIndex(out int cardIndex)
    {
        cardIndex = GetCardIndexOrDefault();
        return _playersTokens is not 0u;
    }

    internal int GetSideToMove() => unchecked((int)(_cardIndexAndSideToMove >> CardIndexBitCount));

    internal (int CurrentPlayerTokens, int OpponentPlayerTokens) GetPlayersTokens()
    {
        int firstPlayerTokens = GetFirstPlayerTokens();
        int secondPlayerTokens = GetSecondPlayerTokens();
        return GetSideToMove() is 0 ? (firstPlayerTokens, secondPlayerTokens) : (secondPlayerTokens, firstPlayerTokens);
    }

    internal Node AddPlayerToken(int index) =>
        GetSideToMove() is 0 ? AddFirstPlayerToken(index) : AddSecondPlayerToken(index);

    private Node AddFirstPlayerToken(int index)
    {
        uint playerTokenMask = 1u << index;
        uint playersTokens = _playersTokens | playerTokenMask;
        uint cardIndexAndSideToMove = unchecked((1u << CardIndexBitCount) | (uint)index);
        return new(playersTokens, cardIndexAndSideToMove);
    }

    private Node AddSecondPlayerToken(int index)
    {
        uint playerTokenMask = 1u << index;
        uint playersTokens = _playersTokens | (playerTokenMask << PlayerTokensBitCount);
        uint cardIndexAndSideToMove = unchecked((uint)index);
        return new(playersTokens, cardIndexAndSideToMove);
    }
}
