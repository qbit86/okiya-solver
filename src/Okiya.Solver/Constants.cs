#if DEBUG
using System.Diagnostics;
using System.Numerics;
#endif

namespace Okiya;

public static class Constants
{
    public const int CardCount = 16;

#if DEBUG
    private const int CardBitCount = 4;
#endif

    internal const int CardMask = 0b1111;

    internal const int SuitCount = 4;

#if DEBUG
    private const int SuitBitCount = 2;

    private const int SuitMask = 0b11;
#endif

    internal const int RankCount = 4;

    internal const int RankBitCount = 2;

    internal const int RankMask = 0b11;

    internal const int PlayerTokensMask = 0xFFFF;

#if DEBUG
    static Constants()
    {
        Debug.Assert(CardCount > 0);
        Debug.Assert(CardBitCount > 0);
        Debug.Assert(BitOperations.Log2(CardCount) + 1 is CardBitCount);
        Debug.Assert(1 << CardBitCount is CardMask + 1);

        Debug.Assert(SuitCount > 0);
        Debug.Assert(SuitBitCount > 0);
        Debug.Assert(BitOperations.Log2(SuitCount) + 1 is SuitBitCount);
        Debug.Assert(1 << SuitBitCount is SuitMask + 1);

        Debug.Assert(RankCount > 0);
        Debug.Assert(RankBitCount > 0);
        Debug.Assert(BitOperations.Log2(RankCount) + 1 is RankBitCount);
        Debug.Assert(1 << RankBitCount is RankMask + 1);

        Debug.Assert(SuitCount * RankCount is CardCount);

        Debug.Assert(1 << CardCount is PlayerTokensMask + 1);
    }
#endif
}
