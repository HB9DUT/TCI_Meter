// Copyright © 2026, HB9DUT
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace TciMeter;

public partial class MainWindow : Window
{
    private Settings _settings = Settings.Load();
    private TciClient _tci;
    private readonly bool _demo;
    private readonly Stopwatch _clock = Stopwatch.StartNew();

    private Needle _main = MeterGeometry.S(-130, -73);
    private double _lastFrame;
    private double _lastText;

    public MainWindow()
    {
        InitializeComponent();
        DarkMode.Apply(this);
        Title = AppInfo.Title(_settings.Callsign);
        var args = Environment.GetCommandLineArgs();
        _demo = args.Contains("--demo");
        if (args.Contains("--swr")) _settings.TxMeter = "Swr"; // override for this run only
        _tci = new TciClient(_settings);
        if (!_demo) _tci.Start();

        CompositionTarget.Rendering += OnRender;
        Closed += (_, _) => { CompositionTarget.Rendering -= OnRender; _tci.Stop(); };
    }

    private void OnSettings(object sender, RoutedEventArgs e)
    {
        var dlg = new SettingsWindow(_settings) { Owner = this };
        if (dlg.ShowDialog() != true) return;

        _settings = dlg.Result;
        Title = AppInfo.Title(_settings.Callsign);
        try { _settings.Save(); }
        catch (Exception ex) { MessageBox.Show(this, "Einstellungen konnten nicht gespeichert werden:\n" + ex.Message); }

        // reconnect with the new server
        _tci.Stop();
        _tci = new TciClient(_settings);
        if (!_demo) _tci.Start();
    }

    private void OnAbout(object sender, RoutedEventArgs e) =>
        new AboutWindow(_settings.Callsign) { Owner = this }.ShowDialog();

    private void OnTopmost(object sender, RoutedEventArgs e) =>
        Topmost = ((System.Windows.Controls.MenuItem)sender).IsChecked;

    // Exponential ballistics: fast attack (needle moving right), slower release. Angle and length share one factor
    // so the tip stays on the scale arc while moving.
    private static Needle Ease(Needle cur, Needle target, double dt, double attack, double release)
    {
        var k = 1 - Math.Exp(-dt / (target.Angle > cur.Angle ? attack : release));
        return new Needle(cur.Angle + (target.Angle - cur.Angle) * k, cur.Length + (target.Length - cur.Length) * k);
    }

    // Tapered needle pointing up from the pivot (origin): tip half-width, base half-width, tail length.
    private static PointCollection NeedlePoints(double length, double tipHalf, double baseHalf, double tail) =>
        new()
        {
            new Point(-tipHalf, -length), new Point(tipHalf, -length),
            new Point(baseHalf, tail), new Point(-baseHalf, tail),
        };

    private void OnRender(object? sender, EventArgs e)
    {
        var now = _clock.Elapsed.TotalSeconds;
        var dt = Math.Clamp(now - _lastFrame, 0.001, 0.1);
        _lastFrame = now;

        if (_demo) SimulateDemo(now);

        var tx = _tci.Transmitting;
        var mainTarget = !tx
            ? MeterGeometry.S(_tci.LevelDbm, _settings.S9Dbm)
            : _settings.ShowSwr
                ? MeterGeometry.Swr(_tci.Swr)
                : MeterGeometry.Po(_tci.PowerW, _settings.MaxPowerW);
        _main = Ease(_main, mainTarget, dt, 0.04, 0.25);
        MainRot.Angle = _main.Angle;
        MainNeedle.Points = NeedlePoints(_main.Length, 0.6, 3.5, 30);

        if (now - _lastText > 0.1)
        {
            _lastText = now;
            UpdateText(tx);
        }
    }

    private void UpdateText(bool tx)
    {
        var hz = Interlocked.Read(ref _tci.FrequencyHz);
        TrxBadge.Text = tx ? "TX" : "RX";
        TrxBadge.Foreground = tx ? Brushes.Red : (Brush)FindResource("Orange");

        BandText.Text = hz > 0 ? Bands.FromFrequency(hz).ToUpperInvariant().Replace(" ", "") : "--";
        ModeText.Text = string.IsNullOrEmpty(_tci.Mode) ? "--" : _tci.Mode;
        FreqText.Text = hz > 0
            ? $"{hz / 1_000_000}.{hz / 1000 % 1000:000}.{hz % 1000:000}"
            : "-.---.---";

        var inv = CultureInfo.InvariantCulture;
        if (tx)
        {
            // the quantity shown on the scale comes first
            var pwr = string.Create(inv, $"{_tci.PowerW:0.0} W");
            var swr = string.Create(inv, $"SWR {_tci.Swr:0.0}");
            LevelText.Text = _settings.ShowSwr ? swr : pwr;
            SnrText.Text = _settings.ShowSwr ? pwr : swr;
        }
        else
        {
            LevelText.Text = string.Create(inv, $"{_tci.LevelDbm:0.0} dBm");
            SnrText.Text = string.Create(inv, $"SNR {Math.Max(0, _tci.LevelDbm - _tci.NoiseDbm):0.0} dB");
        }

        var dev = string.IsNullOrEmpty(_tci.Device) ? "" : _tci.Device + " · ";
        FooterText.Text = _demo ? "DEMO"
            : _tci.Connected
                ? string.Create(inv, $"{dev}PO max {_settings.MaxPowerW:0} W")
                : $"Verbinde mit {_settings.Host}:{_settings.Port} ...";
    }

    private void SimulateDemo(double t)
    {
        _tci.Connected = true;
        _tci.Device = "Demo";
        _tci.Mode = "LSB";
        Interlocked.Exchange(ref _tci.FrequencyHz, 7_176_500);
        var tx = t % 16 > 10;
        _tci.Transmitting = tx;
        var sweep = (Math.Sin(t * 0.6) + 1) / 2;
        _tci.PushLevel(-115 + sweep * 75);
        _tci.PowerW = sweep * _settings.MaxPowerW;
        _tci.PeakPowerW = _tci.PowerW * 1.2;
        _tci.Swr = 1 + sweep * 2;
    }
}
