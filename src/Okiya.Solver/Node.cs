using System.Globalization;
using System.Numerics;
using System.Text;
#if DEBUG
using System.Diagnostics;
#endif

namespace Okiya;

internal readonly record struct Node
{
    private const int PlayerTokensBitCount = 16;

    private const int PlayerTokensMask = 0xFFFF;

    private const int CardBitCount = 31;

    private const int CardMask = 0x7fffffff;

    private readonly uint _cardAndSideToMove;
    private readonly uint _playersTokens;

#if DEBUG
    static Node()
    {
        Debug.Assert(PlayerTokensBitCount is Constants.CardCount);
        Debug.Assert(PlayerTokensBitCount > 0);
        Debug.Assert(1 << PlayerTokensBitCount is PlayerTokensMask + 1);
        Debug.Assert(CardBitCount > 0);
        Debug.Assert(1u << CardBitCount is CardMask + 1u);
    }
#endif

    private Node(uint playersTokens, uint cardAndSideToMove)
    {
        _playersTokens = playersTokens;
        _cardAndSideToMove = cardAndSideToMove;
    }

    private bool PrintMembers(StringBuilder builder)
    {
        CultureInfo p = CultureInfo.InvariantCulture;
        builder.Append(p, $"Players = {GetMinPlayerTokens():b16}|{GetMaxPlayerTokens():b16}");
        int card = GetCardOrDefault();
        builder.Append(p, $", Card = {card} ({Int32CardConcept.Instance.ToString(card)})");
        return true;
    }

    internal bool IsFull() => BitOperations.PopCount(_playersTokens) is PlayerTokensBitCount;

    internal int GetPlayerTokens() => GetPlayerTokens(GetSideToMove());

    internal int GetPlayerTokens(int side) => side is 0 ? GetMaxPlayerTokens() : GetMinPlayerTokens();

    private int GetMaxPlayerTokens() => unchecked((int)(_playersTokens & PlayerTokensMask));

    private int GetMinPlayerTokens() => unchecked((int)(_playersTokens >> PlayerTokensBitCount));

    private int GetCardOrDefault() => unchecked((int)(_cardAndSideToMove & CardMask));

    internal bool TryGetCard(out int card)
    {
        card = GetCardOrDefault();
        return _playersTokens is not 0u;
    }

    internal int GetSideToMove() => unchecked((int)(_cardAndSideToMove >> CardBitCount));

    internal (int SideToMove, int PlayerTokens) GetSideToMoveAndPlayerTokens()
    {
        int sideToMove = GetSideToMove();
        int playerTokens = GetPlayerTokens(sideToMove);
        return (sideToMove, playerTokens);
    }

    internal Node AddPlayerToken(int index, int card) =>
        GetSideToMove() is 0 ? AddMaxPlayerToken(index, card) : AddMinPlayerToken(index, card);

    private Node AddMaxPlayerToken(int index, int card)
    {
        uint playerTokenMask = 1u << index;
        uint playersTokens = _playersTokens | playerTokenMask;
        uint cardAndSideToMove = unchecked((uint)card);
        return new(playersTokens, cardAndSideToMove);
    }

    private Node AddMinPlayerToken(int index, int card)
    {
        uint playerTokenMask = 1u << index;
        uint playersTokens = _playersTokens | (playerTokenMask << PlayerTokensBitCount);
        uint cardAndSideToMove = unchecked((1u << CardBitCount) | (uint)card);
        return new(playersTokens, cardAndSideToMove);
    }
}
