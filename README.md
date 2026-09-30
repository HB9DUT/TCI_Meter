# TCI-Meter

Analoges S-/PO-/SWR-Meter für Transceiver mit [TCI-Schnittstelle](https://github.com/ExpertSDR3/TCI)
(Expert Electronics, z. B. SunSDR mit ExpertSDR3) – für Windows.

Das Meter zeigt im **Empfang** den S-Wert und im **Senden** (auch bei Tune) wahlweise die Sendeleistung oder das SWR.
Darunter stehen Band, Mode, Frequenz, Pegel und SNR. Die Daten kommen live über TCI.

## Installation

1. Im Bereich **Releases** dieses Repositories die aktuelle `TciMeter-Setup-x.y.z.exe` herunterladen.
2. Die Datei ausführen und dem Assistenten folgen.
   - Es sind **keine Administratorrechte** nötig, die Installation erfolgt standardmässig nur für den aktuellen Benutzer.
     Auf Wunsch kann für alle Benutzer installiert werden.
   - Es muss **kein .NET** installiert sein.
   - Optional wird ein Desktop-Symbol angelegt, im Startmenü gibt es immer einen Eintrag.
3. Beim ersten Start die Verbindung zum Transceiver einrichten (siehe unten).

**Hinweis zu Windows SmartScreen:** Der Installer ist nicht digital signiert. Windows kann deshalb beim ersten Start
«Der Computer wurde durch Windows geschützt» anzeigen. Über **Weitere Informationen → Trotzdem ausführen** lässt sich
die Installation fortsetzen.

Voraussetzung: Windows 10 oder 11 (64 Bit).

## Erster Start

Der TCI-Server muss im Transceiver-Programm (z. B. ExpertSDR3) aktiv sein. Standardmässig verbindet sich das Meter mit
`localhost`, Port `50001`. Läuft das Programm auf einem anderen Rechner, die Adresse unter **Einstellungen** anpassen.

Solange keine Verbindung besteht, steht unten links «Verbinde mit …»; das Meter verbindet sich automatisch neu,
sobald der Server erreichbar ist.

## Bedienung

- **Einstellungen:** Zahnrad unten rechts oder Rechtsklick auf das Meter.
  - *Rufzeichen* – erscheint im Fenstertitel («HB9XYZ - TCI-Meter»)
  - *Host / Port* des TCI-Servers
  - *Receiver / Kanal* – welcher Empfänger und welches VFO angezeigt werden (0 = A, 1 = B)
  - *PO max (W)* – Vollausschlag der Leistungsanzeige, passend zur Sendeleistung des Geräts
  - *Senden (auch Tune)* – Zeiger zeigt die Sendeleistung **oder** das SWR
- **Rechtsklick:** Einstellungen, *Immer im Vordergrund*, Info
- Die Einstellungen werden pro Windows-Benutzer gespeichert und bleiben bei Updates erhalten.

Der **SNR** ist eine Schätzung: TCI liefert nur den Pegel, das Rauschniveau wird aus dem Pegelverlauf der letzten
30 Sekunden bestimmt.

## Update und Deinstallation

- **Update:** Neuen Installer ausführen – er ersetzt die installierte Version, die Einstellungen bleiben erhalten.
- **Deinstallation:** Über *Einstellungen → Apps* oder das Startmenü. Auf Wunsch werden dabei auch die gespeicherten
  Einstellungen gelöscht.

## Lizenz

Copyright © 2026, HB9DUT

Dieses Programm ist freie Software: Sie können es unter den Bedingungen der GNU General Public License, wie von der
Free Software Foundation veröffentlicht, Version 3 oder (nach Ihrer Wahl) jeder späteren Version, weitergeben und/oder
ändern. Es wird in der Hoffnung verbreitet, dass es nützlich ist, aber **ohne jede Gewährleistung**. Der vollständige
Lizenztext steht in der Datei [LICENSE](LICENSE).

SPDX-License-Identifier: GPL-3.0-or-later
