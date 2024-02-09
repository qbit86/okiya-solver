using System.Globalization;
using System.Text;

namespace Okiya;

internal readonly record struct Node
{
    private readonly int _card;
    private readonly uint _playersTokenBits;

    private Node(uint playersTokenBits, int card)
    {
        _playersTokenBits = playersTokenBits;
        _card = card;
    }

    private bool PrintMembers(StringBuilder builder)
    {
        CultureInfo p = CultureInfo.InvariantCulture;
        builder.Append(p, $"Players = {GetMinPlayerTokenBits():b16}|{GetMaxPlayerTokenBits():b16}");
        builder.Append(p, $", Card = {_card} ({Int32CardConcept.Instance.ToString(_card)})");
        return true;
    }

    internal int GetMaxPlayerTokenBits() => unchecked((int)(_playersTokenBits & 0xFFFF));

    internal int GetMinPlayerTokenBits() => unchecked((int)(_playersTokenBits >> 16));

    internal int GetCardOrDefault() => _card;

    internal bool TryGetCard(out int card)
    {
        card = _card;
        return _playersTokenBits is not 0;
    }

    internal Node AddMaxPlayerToken(int offset, int card)
    {
        uint playerTokenMask = 1u << offset;
        uint playersTokenBits = _playersTokenBits | playerTokenMask;
        return new(playersTokenBits, card);
    }

    internal Node AddMinPlayerToken(int offset, int card)
    {
        uint playerTokenMask = 1u << offset;
        uint playersTokenBits = _playersTokenBits | (playerTokenMask << 16);
        return new(playersTokenBits, card);
    }
}
