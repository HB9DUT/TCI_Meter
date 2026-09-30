// Copyright © 2026, HB9DUT
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Reflection;

namespace TciMeter;

/// <summary>Product name, version and copyright, taken from the assembly metadata (see TciMeter.csproj).</summary>
public static class AppInfo
{
    public const string ProductName = "TCI-Meter";

    private static readonly Assembly Asm = typeof(AppInfo).Assembly;

    public static string Version => Asm.GetName().Version?.ToString(3) ?? "";
    public static string Copyright =>
        Asm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Copyright © 2026, HB9DUT";

    public const string LicenseName = "GNU GPL v3 oder später";
    public const string LicenseUrl = "https://www.gnu.org/licenses/gpl-3.0.html";

    /// <summary>GPL "interactive program" notice (section 0 of the GPL's how-to-apply text).</summary>
    public const string WarrantyNotice =
        "Dieses Programm ist freie Software und kommt OHNE JEDE GARANTIE. " +
        "Sie dürfen es unter den Bedingungen der GNU General Public License, Version 3 oder (nach Ihrer Wahl) " +
        "jeder späteren Version, weitergeben und ändern.";

    /// <summary>One-liner for footers, e.g. "TCI-Meter 1.0.0 · © 2026, HB9DUT · GPL-3.0+".</summary>
    public static string CopyrightLine => $"{ProductName} {Version} · © 2026, HB9DUT · GPL-3.0+";

    /// <summary>Window title: "CALL - TCI-Meter", or just "TCI-Meter" without a callsign.</summary>
    public static string Title(string? callsign) =>
        string.IsNullOrWhiteSpace(callsign) ? ProductName : $"{callsign.Trim()} - {ProductName}";
}
