using System.Collections.Immutable;

namespace Okiya;

internal static class RuleHelpers
{
    internal static ImmutableArray<int> WinPatterns { get; } = CreateWinPatters();

    private static ImmutableArray<int> CreateWinPatters()
    {
        int[] winPatterns =
        [
            0b11110000_00000000,
            0b00001111_00000000,
            0b00000000_11110000,
            0b00000000_00001111,

            0b10001000_10001000,
            0b01000100_01000100,
            0b00100010_00100010,
            0b00010001_00010001,

            0b10000100_00100001,
            0b00010010_01001000,

            0b00000000_00110011,
            0b00000000_01100110,
            0b00000000_11001100,

            0b00000011_00110000,
            0b00000110_01100000,
            0b00001100_11000000,

            0b00110011_00000000,
            0b01100110_00000000,
            0b11001100_00000000
        ];
        return winPatterns.ToImmutableArray();
    }
}
