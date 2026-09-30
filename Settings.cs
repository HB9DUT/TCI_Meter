// Copyright © 2026, HB9DUT
// SPDX-License-Identifier: GPL-3.0-or-later

using System.IO;
using System.Text.Json;

namespace TciMeter;

public sealed class Settings
{
    /// <summary>Amateur radio callsign shown in the window title ("CALL - TCI-Meter"); empty = "TCI-Meter".</summary>
    public string Callsign { get; set; } = "HB9DUT";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 50001;
    public int Receiver { get; set; } = 0;
    public int Channel { get; set; } = 0;
    public double MaxPowerW { get; set; } = 100;
    /// <summary>What the needle shows while transmitting (incl. TUNE): "Power" or "Swr".</summary>
    public string TxMeter { get; set; } = "Power";
    [System.Text.Json.Serialization.JsonIgnore]
    public bool ShowSwr => TxMeter.Equals("Swr", StringComparison.OrdinalIgnoreCase);
    public double S9Dbm { get; set; } = -73;
    public int SensorIntervalMs { get; set; } = 50;

    private static string UserPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TciMeter", "settings.json");

    /// <summary>Loads the user's settings, falling back to the defaults shipped next to the exe.</summary>
    public static Settings Load()
    {
        foreach (var path in new[] { UserPath, Path.Combine(AppContext.BaseDirectory, "settings.json") })
        {
            try
            {
                if (File.Exists(path))
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText(path)) ?? new Settings();
            }
            catch { }
        }
        return new Settings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(UserPath)!);
        File.WriteAllText(UserPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
