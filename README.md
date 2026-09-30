# TCI-Meter

Analoges S-/PO-/SWR-Meter für Transceiver mit dem [TCI-Protokoll](https://github.com/ExpertSDR3/TCI)
(Expert Electronics, z. B. SunSDR). Windows, WPF, .NET 8.

Das Meter zeigt im Empfang den S-Wert, im Senden (auch bei Tune) wahlweise Leistung oder SWR, dazu Band, Mode,
Frequenz, Pegel und SNR (aus dem Pegelverlauf geschätzt).

## Bedienung

- Zahnrad unten rechts oder Rechtsklick → **Einstellungen** (Rufzeichen, TCI-Host/Port, Receiver/Kanal, PO max, Sendeanzeige)
- Rechtsklick → **Info**, **Immer im Vordergrund**
- Einstellungen liegen unter `%AppData%\TciMeter\settings.json`
- `--demo` startet mit simulierten Werten, `--swr` erzwingt die SWR-Anzeige beim Senden (nur für diesen Lauf)

## Bauen

```
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

## Lizenz

Copyright © 2026, HB9DUT

Dieses Programm ist freie Software: Sie können es unter den Bedingungen der GNU General Public License, wie von der
Free Software Foundation veröffentlicht, Version 3 oder (nach Ihrer Wahl) jeder späteren Version, weitergeben und/oder
ändern. Es wird in der Hoffnung verbreitet, dass es nützlich ist, aber **ohne jede Gewährleistung**. Der vollständige
Lizenztext steht in der Datei [LICENSE](LICENSE).

SPDX-License-Identifier: GPL-3.0-or-later
