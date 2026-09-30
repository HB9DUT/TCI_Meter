// Copyright © 2026, HB9DUT
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Globalization;
using System.Windows;

namespace TciMeter;

public partial class SettingsWindow : Window
{
    public Settings Result { get; private set; }

    public SettingsWindow(Settings current)
    {
        InitializeComponent();
        DarkMode.Apply(this);
        CopyrightText.Text = AppInfo.CopyrightLine;
        Title = AppInfo.Title(current.Callsign) + " · Einstellungen";
        Result = current;
        Show(current);
    }

    private void Show(Settings s)
    {
        CallBox.Text = s.Callsign;
        HostBox.Text = s.Host;
        PortBox.Text = s.Port.ToString(CultureInfo.InvariantCulture);
        RxBox.Text = s.Receiver.ToString(CultureInfo.InvariantCulture);
        ChBox.Text = s.Channel.ToString(CultureInfo.InvariantCulture);
        PowerBox.Text = s.MaxPowerW.ToString(CultureInfo.InvariantCulture);
        TxSwrRadio.IsChecked = s.ShowSwr;
        TxPowerRadio.IsChecked = !s.ShowSwr;
        ErrorText.Text = "";
    }

    private void OnDefaults(object sender, RoutedEventArgs e) =>
        Show(new Settings { Callsign = Result.Callsign, MaxPowerW = Result.MaxPowerW, Receiver = Result.Receiver,
                            Channel = Result.Channel, TxMeter = Result.TxMeter });

    private void OnOk(object sender, RoutedEventArgs e)
    {
        var call = CallBox.Text.Trim().ToUpperInvariant();
        if (!call.All(c => char.IsAsciiLetterOrDigit(c) || c == '/'))
        { ErrorText.Text = "Rufzeichen darf nur Buchstaben, Ziffern und / enthalten."; return; }

        var host = HostBox.Text.Trim();
        if (host.Length == 0 || host.Contains(' ') || host.Contains('/') || host.Contains(':'))
        { ErrorText.Text = "Ungültiger Host (nur Name oder IP, ohne Port)."; return; }
        if (!int.TryParse(PortBox.Text, out var port) || port is < 1 or > 65535)
        { ErrorText.Text = "Port muss zwischen 1 und 65535 liegen."; return; }
        if (!int.TryParse(RxBox.Text, out var rx) || rx < 0)
        { ErrorText.Text = "Receiver muss eine Zahl ≥ 0 sein."; return; }
        if (!int.TryParse(ChBox.Text, out var ch) || ch is < 0 or > 1)
        { ErrorText.Text = "Kanal muss 0 oder 1 sein."; return; }
        if (!double.TryParse(PowerBox.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var pw) || pw <= 0)
        { ErrorText.Text = "PO max muss größer als 0 sein."; return; }

        Result = new Settings
        {
            Callsign = call, Host = host, Port = port, Receiver = rx, Channel = ch, MaxPowerW = pw,
            TxMeter = TxSwrRadio.IsChecked == true ? "Swr" : "Power",
            S9Dbm = Result.S9Dbm, SensorIntervalMs = Result.SensorIntervalMs,
        };
        DialogResult = true;
    }
}
