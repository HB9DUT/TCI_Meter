// Copyright © 2026, HB9DUT
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Globalization;
using System.Windows.Markup;

namespace TciMeter;

/// <summary>
/// Minimal localization (German / English). The language follows the Windows UI language unless it is set
/// explicitly in the settings ("auto", "de", "en").
/// </summary>
public static class Loc
{
    public static string Language { get; private set; } = "en";

    public static void SetLanguage(string? setting)
    {
        Language = (setting ?? "auto").Trim().ToLowerInvariant() switch
        {
            "de" => "de",
            "en" => "en",
            _ => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de" ? "de" : "en",
        };
    }

    public static string T(string key) =>
        Table.TryGetValue(key, out var v) ? (Language == "de" ? v.De : v.En) : key;

    public static string T(string key, params object[] args) => string.Format(T(key), args);

    private static readonly Dictionary<string, (string De, string En)> Table = new()
    {
        // main window
        ["menu.settings"] = ("Einstellungen ...", "Settings ..."),
        ["menu.topmost"] = ("Immer im Vordergrund", "Always on top"),
        ["menu.about"] = ("Info ...", "About ..."),
        ["tip.settings"] = ("Einstellungen", "Settings"),
        ["status.connecting"] = ("Verbinde mit {0}:{1} ...", "Connecting to {0}:{1} ..."),
        ["err.save"] = ("Einstellungen konnten nicht gespeichert werden:\n{0}", "Settings could not be saved:\n{0}"),

        // settings window
        ["settings.title"] = ("Einstellungen", "Settings"),
        ["settings.station"] = ("Station", "Station"),
        ["settings.callsign"] = ("Rufzeichen", "Callsign"),
        ["settings.callsign.tip"] = ("Wird im Fenstertitel angezeigt", "Shown in the window title"),
        ["settings.language"] = ("Sprache", "Language"),
        ["settings.lang.auto"] = ("Auto", "Auto"),
        ["settings.server"] = ("TCI-Server", "TCI server"),
        ["settings.host"] = ("Host", "Host"),
        ["settings.port"] = ("Port", "Port"),
        ["settings.receiver"] = ("Receiver (RX)", "Receiver (RX)"),
        ["settings.channel"] = ("Kanal (0 = A, 1 = B)", "Channel (0 = A, 1 = B)"),
        ["settings.pomax"] = ("PO max (W)", "PO max (W)"),
        ["settings.tx"] = ("Senden (auch Tune)", "Transmit (incl. Tune)"),
        ["settings.tx.power"] = ("Sendeleistung (PO)", "Output power (PO)"),
        ["settings.tx.swr"] = ("SWR", "SWR"),
        ["settings.defaults"] = ("Standard", "Defaults"),
        ["settings.cancel"] = ("Abbrechen", "Cancel"),
        ["err.callsign"] = ("Rufzeichen darf nur Buchstaben, Ziffern und / enthalten.", "The callsign may only contain letters, digits and /."),
        ["err.host"] = ("Ungültiger Host (nur Name oder IP, ohne Port).", "Invalid host (name or IP only, without port)."),
        ["err.port"] = ("Port muss zwischen 1 und 65535 liegen.", "Port must be between 1 and 65535."),
        ["err.receiver"] = ("Receiver muss eine Zahl ≥ 0 sein.", "Receiver must be a number ≥ 0."),
        ["err.channel"] = ("Kanal muss 0 oder 1 sein.", "Channel must be 0 or 1."),
        ["err.pomax"] = ("PO max muss größer als 0 sein.", "PO max must be greater than 0."),

        // about window
        ["about.title"] = ("Info", "About"),
        ["about.description"] = ("Analoges S-/PO-/SWR-Meter für TCI-Transceiver", "Analog-style S / PO / SWR meter for TCI transceivers"),
        ["about.protocol"] = ("(Expert Electronics TCI-Protokoll)", "(Expert Electronics TCI protocol)"),
        ["about.version"] = ("Version {0}", "Version {0}"),
        ["about.license"] = ("Lizenz: ", "License: "),
        ["about.licensename"] = ("GNU GPL v3 oder später", "GNU GPL v3 or later"),
        ["about.warranty"] = (
            "Dieses Programm ist freie Software und kommt OHNE JEDE GARANTIE. Sie dürfen es unter den Bedingungen der " +
            "GNU General Public License, Version 3 oder (nach Ihrer Wahl) jeder späteren Version, weitergeben und ändern.",
            "This program is free software and comes WITHOUT ANY WARRANTY. You may redistribute and/or modify it under " +
            "the terms of the GNU General Public License, version 3 or (at your option) any later version."),
    };
}

/// <summary>XAML usage: <c>Text="{loc:T settings.callsign}"</c> (namespace <c>xmlns:loc="clr-namespace:TciMeter"</c>).</summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class TExtension : MarkupExtension
{
    public string Key { get; set; } = "";

    public TExtension() { }
    public TExtension(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider) => Loc.T(Key);
}
