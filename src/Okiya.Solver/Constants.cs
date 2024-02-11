#if DEBUG
using System.Diagnostics;
#endif

namespace Okiya;

public static class Constants
{
    public const int CardCount = 16;

    private const int Log2CardCount = 4;

    internal const int CardMask = 0b1111;

    internal const int SuitCount = 4;

    private const int Log2SuitCount = 2;

    internal const int SuitMask = 0b11;

    internal const int RankCount = 4;

    internal const int Log2RankCount = 2;

    internal const int RankMask = 0b11;

#if DEBUG
    static Constants()
    {
        Debug.Assert(Log2CardCount > 0);
        Debug.Assert(1 << Log2CardCount >= CardCount);
        Debug.Assert((1 << Log2CardCount) - 1 is CardMask);
        Debug.Assert(SuitCount * RankCount is CardCount);
        Debug.Assert(1 << Log2SuitCount >= SuitCount);
        Debug.Assert((1 << Log2SuitCount) - 1 is SuitMask);
        Debug.Assert(1 << Log2RankCount >= RankCount);
        Debug.Assert((1 << Log2RankCount) - 1 is RankMask);
    }
#endif
}
