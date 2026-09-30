// Copyright © 2026, HB9DUT
// SPDX-License-Identifier: GPL-3.0-or-later

namespace TciMeter;

/// <summary>Needle pose: rotation around the pivot (degrees, 0 = straight up) and visible length (px).</summary>
public readonly record struct Needle(double Angle, double Length);

/// <summary>
/// Maps measured values to needle poses for the 1200x600 meter background (assets/meter.png).
/// All coordinates are in image pixels. The needle pivots around <see cref="Pivot"/>; for a value we find the
/// point on the (measured) scale arc, and derive angle and length from pivot to that point, so the tip runs along the scale.
/// </summary>
public static class MeterGeometry
{
    public const double ImageW = 1200, ImageH = 600;
    public static readonly (double X, double Y) Pivot = (603, 548);

    /// <summary>Extra needle length beyond the S arc centerline (reference: needle length at S9 = 462 px).</summary>
    private static readonly double SOvershoot = 462 - (Pivot.Y - Arcs.S.Y(603));

    private static Needle At(double x, Arc arc, double overshoot)
    {
        var dx = x - Pivot.X;
        var dy = Pivot.Y - arc.Y(x);
        return new Needle(Math.Atan2(dx, dy) * 180 / Math.PI, Math.Sqrt(dx * dx + dy * dy) + overshoot);
    }

    /// <summary>
    /// The needle is one physical part: whatever scale it reads, its length is the one it has when it
    /// reaches the top (S) arc at that angle. Only the angle is taken from the scale being read.
    /// </summary>
    private static Needle Rigid(Needle n)
    {
        double lo = 112, hi = 1096; // x range of the measured S arc; angle increases with x
        var a = AngleAtS(lo);
        var b = AngleAtS(hi);
        if (n.Angle <= a) return new Needle(n.Angle, LengthAtS(lo));
        if (n.Angle >= b) return new Needle(n.Angle, LengthAtS(hi));
        for (var i = 0; i < 40; i++)
        {
            var mid = (lo + hi) / 2;
            if (AngleAtS(mid) < n.Angle) lo = mid; else hi = mid;
        }
        return new Needle(n.Angle, LengthAtS((lo + hi) / 2));
    }

    private static double AngleAtS(double x) => Math.Atan2(x - Pivot.X, Pivot.Y - Arcs.S.Y(x)) * 180 / Math.PI;
    private static double LengthAtS(double x) => At(x, Arcs.S, SOvershoot).Length;

    private static Needle Interp(double v, (double V, double X)[] pts, Arc arc, double overshoot)
    {
        if (v <= pts[0].V) return At(pts[0].X, arc, overshoot);
        for (var i = 1; i < pts.Length; i++)
            if (v <= pts[i].V)
            {
                var t = (v - pts[i - 1].V) / (pts[i].V - pts[i - 1].V);
                return At(pts[i - 1].X + t * (pts[i].X - pts[i - 1].X), arc, overshoot);
            }
        return At(pts[^1].X, arc, overshoot);
    }

    // S-scale: value in "S units" (0..9), then 10 dB per step above S9 (+60 dB -> 15)
    private static readonly (double V, double X)[] SPts =
        { (0, 115), (9, 603), (11, 752), (13, 913), (15, 1095) };

    /// <summary>Signal level (dBm) to needle pose. S9 = <paramref name="s9Dbm"/>, 6 dB per S unit.</summary>
    public static Needle S(double dbm, double s9Dbm)
    {
        var over = dbm - s9Dbm;
        var v = over <= 0 ? 9 + over / 6.0 : 9 + over / 10.0;
        return Interp(Math.Max(v, 0), SPts, Arcs.S, SOvershoot);
    }

    /// <summary>Power (W) to needle pose on the PO arc, linear from left end (0) to right end (max).</summary>
    public static Needle Po(double watts, double maxW)
    {
        var f = Math.Clamp(watts / maxW, 0, 1);
        return Rigid(At(190 + f * (1020 - 190), Arcs.Po, 0));  // unlabeled PO scale: full thick arc
    }

    // SWR scale: labels 1 / 1.5 / 2 / 3 with the red dashes sit on the thick arc (same arc as PO).
    // 1..3 are the measured major ticks; above 3 the scale has no ticks and runs to inf at the right end of the
    // arc (spacing follows the reflection coefficient, which matches the printed 1 / 1.5 / 2 / 3 spacing).
    private static readonly (double V, double X)[] SwrPts =
        { (1, 196), (1.5, 345.5), (2, 477), (3, 600), (5, 740), (10, 867), (20, 940), (1000, 1020) };

    public static Needle Swr(double swr) => Rigid(Interp(Math.Min(swr, 1000), SwrPts, Arcs.Po, 0));
}
