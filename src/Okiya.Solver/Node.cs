using System.Globalization;
using System.Text;

namespace Okiya;

internal readonly record struct Node
{
    private const uint CardMask = ~(1u << 31);

    private readonly uint _cardAndSideToMove;
    private readonly uint _playersTokenBits;

    private Node(uint playersTokenBits, uint cardAndSideToMove)
    {
        _playersTokenBits = playersTokenBits;
        _cardAndSideToMove = cardAndSideToMove;
    }

    private bool PrintMembers(StringBuilder builder)
    {
        CultureInfo p = CultureInfo.InvariantCulture;
        builder.Append(p, $"Players = {GetMinPlayerTokenBits():b16}|{GetMaxPlayerTokenBits():b16}");
        int card = GetCardOrDefault();
        builder.Append(p, $", Card = {card} ({Int32CardConcept.Instance.ToString(card)})");
        return true;
    }

    internal int GetMaxPlayerTokenBits() => unchecked((int)(_playersTokenBits & 0xFFFF));

    internal int GetMinPlayerTokenBits() => unchecked((int)(_playersTokenBits >> Constants.CardCount));

    internal int GetCardOrDefault() => unchecked((int)(_cardAndSideToMove & CardMask));

    internal bool TryGetCard(out int card)
    {
        card = GetCardOrDefault();
        return _playersTokenBits is not 0;
    }

    internal int SideToMove() => unchecked((int)(_cardAndSideToMove >> 31));

    internal Node AddMaxPlayerToken(int index, int card)
    {
        uint playerTokenMask = 1u << index;
        uint playersTokenBits = _playersTokenBits | playerTokenMask;
        uint cardAndSideToMove = unchecked((uint)card);
        return new(playersTokenBits, cardAndSideToMove);
    }

    internal Node AddMinPlayerToken(int index, int card)
    {
        uint playerTokenMask = 1u << index;
        uint playersTokenBits = _playersTokenBits | (playerTokenMask << Constants.CardCount);
        uint cardAndSideToMove = unchecked((1u << 31) | (uint)card);
        return new(playersTokenBits, cardAndSideToMove);
    }
}
