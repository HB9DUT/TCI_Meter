// Copyright © 2026, HB9DUT
// SPDX-License-Identifier: GPL-3.0-or-later

namespace TciMeter;

public static class Bands
{
    private static readonly (string Name, long Lo, long Hi)[] Table =
    {
        ("160 m", 1_800_000, 2_000_000),
        ("80 m", 3_500_000, 4_000_000),
        ("60 m", 5_250_000, 5_450_000),
        ("40 m", 7_000_000, 7_300_000),
        ("30 m", 10_100_000, 10_150_000),
        ("20 m", 14_000_000, 14_350_000),
        ("17 m", 18_068_000, 18_168_000),
        ("15 m", 21_000_000, 21_450_000),
        ("12 m", 24_890_000, 24_990_000),
        ("10 m", 28_000_000, 29_700_000),
        ("6 m", 50_000_000, 54_000_000),
        ("4 m", 70_000_000, 70_500_000),
        ("2 m", 144_000_000, 148_000_000),
        ("70 cm", 430_000_000, 440_000_000),
    };

    public static string FromFrequency(long hz)
    {
        foreach (var b in Table)
            if (hz >= b.Lo && hz <= b.Hi) return b.Name;
        return hz > 0 ? "GEN" : "--";
    }
}
