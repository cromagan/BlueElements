# BeCreativeCLI (bcr) — Hinweise für KI-Agenten

`bcr` ist eine Konsolen-App (steht in den Umgebungsvariablen, kein Pfad nötig).
Bei „BCR“, „ShellCommand bcr“ etc. ist IMMER diese Exe gemeint — direkt in der Shell ausführen.

## Was ist das?

Kommandozeilen-Werkzeug zum Lesen und Bearbeiten von BlueElements-Tabellen
(CSV und Datenbank-Formate wie `.bdb`, `.tblh`) — für Skripte, Automatisierung
und schnelle Änderungen ohne GUI.

## Wichtig

- Tabellenname ohne Pfad = aktuelles Arbeitsverzeichnis. Shell also per workdir in den Tabellen-Ordner setzen.
- Unsicher bei Befehlen? Erst `bcr help` bzw. `bcr help <befehl>` aufrufen.
- Exit-Codes: 0 = Erfolg, 1 = Fehler, 2 = Benutzungsfehler (`$LASTEXITCODE` prüfen).
- Daten auf stdout (CSV, UTF-8), Fehler auf stderr.
- Die CLI läuft mit Benutzergruppe `#CLI` (nicht Administrator): Bearbeitungen brauchen `#CLI` als Recht (Spalten-Bearbeitung, Neue Zeilen, Tabellen-Admin für delrow).
- Tabellen mit Kennwort sind über die CLI grundsätzlich nicht benutzbar (sofortiger Fehler in allen Befehlen).

## Kurzbefehle

- `bcr info <tabelle>` — Übersicht
- `bcr cellget <tabelle> --column <c>` + Zeilenadressierung — Zelle lesen
- `bcr cellset <tabelle> --column <c> --value <w>` + Zeilenadressierung — Zelle setzen (speichert)
- `bcr addrow <tabelle> [--firstvalue <w>]` — Zeile anlegen (speichert)
- `bcr delrow <tabelle>` + Zeilenadressierung — Zeilen löschen (speichert)
- `bcr export <tabelle>` — CSV auf stdout

Zeilenadressierung: `--rowkey <key>` ODER `--filtercolumn <c> --filtervalue <w>` (optional `--filtertype equals|exact|contains|startswith`).
