#if DEBUG
using System.Diagnostics;
#endif

namespace Okiya;

public static class Constants
{
    public const int CardCount = 16;

    private const int Log2CardCount = 4;

    internal const int Log2CardCountMask = 0b1111;

#if DEBUG
    static Constants()
    {
        Debug.Assert(Log2CardCount > 0);
        Debug.Assert(1 << Log2CardCount == CardCount);
        Debug.Assert((1 << Log2CardCount) - 1 == Log2CardCountMask);
    }
#endif
}
