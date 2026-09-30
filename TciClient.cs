// Copyright © 2026, HB9DUT
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Globalization;
using System.Net.WebSockets;
using System.Text;

namespace TciMeter;

/// <summary>
/// Minimal TCI (ExpertSDR3) WebSocket client. Values are published as volatile fields
/// so the UI thread can poll them each frame without locking.
/// </summary>
public sealed class TciClient
{
    private readonly Settings _s;
    private readonly CancellationTokenSource _cts = new();

    public volatile bool Connected;
    private volatile bool _trx, _tune;

    /// <summary>True while transmitting, including TUNE (which the server reports separately from TRX).</summary>
    public bool Transmitting
    {
        get => _trx || _tune;
        set { _trx = value; if (!value) _tune = false; }
    }
    public volatile string Device = "";
    public volatile string Mode = "";
    public long FrequencyHz;           // written/read via Interlocked
    public double LevelDbm = -140;     // RX level, dBm
    public double NoiseDbm = -140;     // estimated noise floor, dBm
    public double PowerW;              // TX power (RMS), W
    public double PeakPowerW;
    public double Swr = 1;

    // sliding window for noise-floor estimate
    private readonly Queue<double> _window = new();
    private int _sinceRecalc;

    public TciClient(Settings s) => _s = s;

    public void Start() => Task.Run(() => RunAsync(_cts.Token));
    public void Stop() => _cts.Cancel();

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri($"ws://{_s.Host}:{_s.Port}"), ct);
                Connected = true;
                await ReceiveLoop(ws, ct);
            }
            catch (OperationCanceledException) { return; }
            catch { /* fall through to reconnect */ }

            Connected = false;
            Transmitting = false;
            try { await Task.Delay(2000, ct); } catch { return; }
        }
    }

    private static Task SendAsync(ClientWebSocket ws, string text, CancellationToken ct) =>
        ws.SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, ct);

    private async Task ReceiveLoop(ClientWebSocket ws, CancellationToken ct)
    {
        var buf = new byte[16 * 1024];
        var sb = new StringBuilder();
        while (ws.State == WebSocketState.Open)
        {
            sb.Clear();
            WebSocketReceiveResult r;
            do
            {
                r = await ws.ReceiveAsync(buf, ct);
                if (r.MessageType == WebSocketMessageType.Close) return;
                if (r.MessageType == WebSocketMessageType.Text)
                    sb.Append(Encoding.UTF8.GetString(buf, 0, r.Count));
            } while (!r.EndOfMessage);

            foreach (var cmd in sb.ToString().Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var c = cmd.Trim();
                // The server only accepts sensor subscriptions after its initial state dump ("ready").
                if (c.Equals("ready", StringComparison.OrdinalIgnoreCase))
                    await SendAsync(ws, $"rx_sensors_enable:true,{_s.SensorIntervalMs};" +
                                        $"tx_sensors_enable:true,{_s.SensorIntervalMs};", ct);
                else
                    Handle(c);
            }
        }
    }

    private static double D(string s) => double.Parse(s.Trim(), CultureInfo.InvariantCulture);
    private static bool TryD(string s, out double v) =>
        double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);

    private void Handle(string cmd)
    {
        var i = cmd.IndexOf(':');
        var name = (i < 0 ? cmd : cmd[..i]).ToLowerInvariant();
        var a = i < 0 ? Array.Empty<string>() : cmd[(i + 1)..].Split(',');

        try
        {
            switch (name)
            {
                case "device" when a.Length >= 1:
                    Device = a[0];
                    break;

                case "vfo" when a.Length >= 3 && int.Parse(a[0]) == _s.Receiver && int.Parse(a[1]) == _s.Channel:
                    Interlocked.Exchange(ref FrequencyHz, long.Parse(a[2], CultureInfo.InvariantCulture));
                    ResetNoise();
                    break;

                case "modulation" when a.Length >= 2 && int.Parse(a[0]) == _s.Receiver:
                    Mode = a[1].ToUpperInvariant();
                    ResetNoise();
                    break;

                case "trx" when a.Length >= 2 && int.Parse(a[0]) == _s.Receiver:
                    _trx = a[1].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                    break;

                case "tune" when a.Length >= 2 && int.Parse(a[0]) == _s.Receiver:
                    _tune = a[1].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                    break;

                case "rx_channel_sensors" when a.Length >= 3 && int.Parse(a[0]) == _s.Receiver && int.Parse(a[1]) == _s.Channel:
                    PushLevel(D(a[2]));
                    break;

                case "tx_sensors" when a.Length >= 5 && int.Parse(a[0]) == _s.Receiver:
                    PowerW = D(a[2]);
                    PeakPowerW = D(a[3]);
                    Swr = Math.Max(1, D(a[4]));
                    break;
            }
        }
        catch { /* ignore malformed command */ }
    }

    private void ResetNoise() { lock (_window) { _window.Clear(); _sinceRecalc = 0; } }

    /// <summary>Feeds a new RX level and updates the noise floor (10th percentile over ~30 s).</summary>
    public void PushLevel(double dbm)
    {
        LevelDbm = dbm;
        if (Transmitting) return;
        lock (_window)
        {
            _window.Enqueue(dbm);
            var max = Math.Max(20, 30000 / Math.Max(_s.SensorIntervalMs, 30));
            while (_window.Count > max) _window.Dequeue();

            if (++_sinceRecalc >= 10 || _window.Count < 10)
            {
                _sinceRecalc = 0;
                var sorted = _window.OrderBy(x => x).ToArray();
                NoiseDbm = sorted[(int)(sorted.Length * 0.10)];
            }
        }
    }
}
