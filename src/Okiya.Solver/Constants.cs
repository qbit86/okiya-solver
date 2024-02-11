#if DEBUG
using System.Diagnostics;
#endif

namespace Okiya;

public static class Constants
{
    public const int CardCount = 16;

    private const int CardBitCount = 4;

    internal const int CardMask = 0b1111;

    internal const int SuitCount = 4;

    private const int SuitBitCount = 2;

    internal const int SuitMask = 0b11;

    internal const int RankCount = 4;

    internal const int RankBitCount = 2;

    internal const int RankMask = 0b11;

#if DEBUG
    static Constants()
    {
        Debug.Assert(CardBitCount > 0);
        Debug.Assert(1 << CardBitCount >= CardCount);
        Debug.Assert((1 << CardBitCount) - 1 is CardMask);
        Debug.Assert(SuitCount * RankCount is CardCount);
        Debug.Assert(1 << SuitBitCount >= SuitCount);
        Debug.Assert((1 << SuitBitCount) - 1 is SuitMask);
        Debug.Assert(1 << RankBitCount >= RankCount);
        Debug.Assert((1 << RankBitCount) - 1 is RankMask);
    }
#endif
}
