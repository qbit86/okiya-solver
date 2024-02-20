using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text;
using static Okiya.TryHelpers;

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

    private uint GetAllTokens() => _playersTokens | (_playersTokens >> PlayerTokensBitCount);

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

    internal Node AddPlayerTokenUnchecked(int index) =>
        GetSideToMove() is 0 ? AddFirstPlayerTokenUnchecked(index) : AddSecondPlayerTokenUnchecked(index);

    internal bool TryAddPlayerToken(int index, out Node child) =>
        GetSideToMove() is 0 ? TryAddFirstPlayerToken(index, out child) : TryAddSecondPlayerToken(index, out child);

    private Node AddFirstPlayerTokenUnchecked(int index)
    {
        Debug.Assert(unchecked((uint)index < Constants.CardCount));
        uint playerTokenMask = 1u << index;
        Debug.Assert((GetAllTokens() & playerTokenMask) is 0);
        uint playersTokens = _playersTokens | playerTokenMask;
        uint cardIndexAndSideToMove = unchecked((1u << CardIndexBitCount) | (uint)index);
        return new(playersTokens, cardIndexAndSideToMove);
    }

    private Node AddSecondPlayerTokenUnchecked(int index)
    {
        Debug.Assert(unchecked((uint)index < Constants.CardCount));
        uint playerTokenMask = 1u << index;
        Debug.Assert((GetAllTokens() & playerTokenMask) is 0);
        uint playersTokens = _playersTokens | (playerTokenMask << PlayerTokensBitCount);
        uint cardIndexAndSideToMove = unchecked((uint)index);
        return new(playersTokens, cardIndexAndSideToMove);
    }

    private bool TryAddFirstPlayerToken(int index, out Node child)
    {
        if (unchecked((uint)index >= Constants.CardCount))
            return None(this, out child);
        uint playerTokenMask = 1u << index;
        if ((GetAllTokens() & playerTokenMask) is not 0)
            return None(this, out child);
        uint playersTokens = _playersTokens | playerTokenMask;
        uint cardIndexAndSideToMove = unchecked((1u << CardIndexBitCount) | (uint)index);
        child = new(playersTokens, cardIndexAndSideToMove);
        return true;
    }

    private bool TryAddSecondPlayerToken(int index, out Node child)
    {
        if (unchecked((uint)index >= Constants.CardCount))
            return None(this, out child);
        uint playerTokenMask = 1u << index;
        if ((GetAllTokens() & playerTokenMask) is not 0)
            return None(this, out child);
        uint playersTokens = _playersTokens | (playerTokenMask << PlayerTokensBitCount);
        uint cardIndexAndSideToMove = unchecked((uint)index);
        child = new(playersTokens, cardIndexAndSideToMove);
        return true;
    }
}
