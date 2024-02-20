using System.Globalization;
using System.Numerics;
using System.Text;
#if DEBUG
using System.Diagnostics;
#endif

namespace Okiya;

public readonly record struct Node
{
    private const int PlayerTokensBitCount = 16;

    private const int CardBitCount = 31;

    private const int CardMask = 0x7fffffff;

    private readonly uint _cardAndSideToMove;
    private readonly uint _playersTokens;

#if DEBUG
    static Node()
    {
        Debug.Assert(PlayerTokensBitCount is Constants.CardCount);
        Debug.Assert(PlayerTokensBitCount > 0);
        Debug.Assert(1 << PlayerTokensBitCount is Constants.PlayerTokensMask + 1);
        Debug.Assert(CardBitCount > 0);
        Debug.Assert(1u << CardBitCount is CardMask + 1u);
    }
#endif

    private Node(uint playersTokens, uint cardAndSideToMove)
    {
        _playersTokens = playersTokens;
        _cardAndSideToMove = cardAndSideToMove;
    }

    internal static Node CreateUnchecked(uint firstPlayerTokens, uint secondPlayerTokens, int sideToMove, int card)
    {
        Debug.Assert(firstPlayerTokens <= Constants.PlayerTokensMask);
        Debug.Assert(secondPlayerTokens <= Constants.PlayerTokensMask);
        Debug.Assert((firstPlayerTokens & secondPlayerTokens) is 0);
        Debug.Assert(sideToMove is 0 or 1);
        Debug.Assert((uint)card < Constants.CardCount);
        uint playersTokens = firstPlayerTokens | (secondPlayerTokens << Constants.CardCount);
        uint sideBit = sideToMove is 0 ? 0 : 1u << CardBitCount;
        uint cardAndSideToMove = sideBit | unchecked((uint)card);
        return new(playersTokens, cardAndSideToMove);
    }

    private bool PrintMembers(StringBuilder builder)
    {
        CultureInfo p = CultureInfo.InvariantCulture;
        builder.Append(p, $"Players = {GetSecondPlayerTokens():b16}|{GetFirstPlayerTokens():b16}");
        int card = GetCardOrDefault();
        builder.Append(p, $", Card = {card} ({Int32CardConcept.Instance.ToString(card)})");
        builder.Append(p, $", SideToMove = {GetSideToMove()}");
        return true;
    }

    internal bool IsFull() => BitOperations.PopCount(_playersTokens) is PlayerTokensBitCount;

    internal int GetTokenCount() => BitOperations.PopCount(_playersTokens);

    internal int GetPlayerTokens(int side) => side is 0 ? GetFirstPlayerTokens() : GetSecondPlayerTokens();

    private int GetFirstPlayerTokens() => unchecked((int)(_playersTokens & Constants.PlayerTokensMask));

    private int GetSecondPlayerTokens() => unchecked((int)(_playersTokens >> PlayerTokensBitCount));

    private int GetCardOrDefault() => unchecked((int)(_cardAndSideToMove & CardMask));

    internal bool TryGetCard(out int card)
    {
        card = GetCardOrDefault();
        return _playersTokens is not 0u;
    }

    internal int GetSideToMove() => unchecked((int)(_cardAndSideToMove >> CardBitCount));

    internal (int CurrentPlayerTokens, int OpponentPlayerTokens) GetPlayersTokens()
    {
        int firstPlayerTokens = GetFirstPlayerTokens();
        int secondPlayerTokens = GetSecondPlayerTokens();
        return GetSideToMove() is 0 ? (firstPlayerTokens, secondPlayerTokens) : (secondPlayerTokens, firstPlayerTokens);
    }

    internal Node AddPlayerToken(int index, int card) =>
        GetSideToMove() is 0 ? AddFirstPlayerToken(index, card) : AddSecondPlayerToken(index, card);

    private Node AddFirstPlayerToken(int index, int card)
    {
        uint playerTokenMask = 1u << index;
        uint playersTokens = _playersTokens | playerTokenMask;
        uint cardAndSideToMove = unchecked((1u << CardBitCount) | (uint)card);
        return new(playersTokens, cardAndSideToMove);
    }

    private Node AddSecondPlayerToken(int index, int card)
    {
        uint playerTokenMask = 1u << index;
        uint playersTokens = _playersTokens | (playerTokenMask << PlayerTokensBitCount);
        uint cardAndSideToMove = unchecked((uint)card);
        return new(playersTokens, cardAndSideToMove);
    }
}
