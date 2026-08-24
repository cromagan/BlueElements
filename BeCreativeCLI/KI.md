# BeCreativeCLI (bcr) — Hinweise für KI-Agenten

`bcr` ist eine Konsolen-App (steht in den Umgebungsvariablen, kein Pfad nötig).
Bei „BCR“, „ShellCommand bcr“ etc. ist IMMER diese Exe gemeint — direkt in der Shell ausführen.

## Was ist das?

Kommandozeilen-Werkzeug für BeCreative-Dateien. Aktuell werden nur Tabellen
unterstützt (CSV und Datenbank-Formate wie `.bdb`, `.tblh`) — diese Befehle
tragen das Präfix `table-`. Für Skripte, Automatisierung und schnelle
Änderungen ohne GUI.

## Wichtig

- Tabellenname ohne Pfad = aktuelles Arbeitsverzeichnis. Shell also per workdir in den Tabellen-Ordner setzen.
- Unsicher bei Befehlen? Erst `bcr help` bzw. `bcr help <befehl>` aufrufen.
- Bevorzugt in der Shell arbeiten (`bcr shell`): Tabellen bleiben in der Session im Speicher — unnötiges Laden und Speichern pro Einzelbefehl entfällt. Einzelbefehle nur für einmalige Aktionen.
- Exit-Codes: 0 = Erfolg, 1 = Fehler, 2 = Benutzungsfehler (`$LASTEXITCODE` prüfen).
- Daten auf stdout (CSV, UTF-8), Fehler auf stderr.
- Die CLI läuft mit Benutzergruppe `#CLI` (nicht Administrator): Bearbeitungen brauchen `#CLI` als Recht (Spalten-Bearbeitung, Neue Zeilen, Tabellen-Admin für table-delrow).
- Tabellen mit Kennwort: außerhalb der Shell nicht benutzbar (sofortiger Fehler). In einer Shell-Session zuerst mit `table-password` entsperren, dann normal bearbeiten.

## Kurzbefehle (Tabellen)

- `bcr table-info <tabelle>` — Übersicht
- `bcr table-cellget <tabelle> --column <c>` + Zeilenadressierung — Zelle lesen
- `bcr table-cellset <tabelle> --column <c> --value <w>` + Zeilenadressierung — Zelle setzen (speichert)
- `bcr table-addrow <tabelle> [--firstvalue <w>]` — Zeile anlegen (speichert)
- `bcr table-delrow <tabelle>` + Zeilenadressierung — Zeilen löschen (speichert)
- `bcr table-search <tabelle> --value <w> [--column <c>]` — Volltextsuche in allen Spalten oder nur `--column`; pro Treffer eine Zeile mit Spalte, Zeilen-Key und Kontext
- `bcr table-export <tabelle>` — CSV auf stdout

Zeilenadressierung: `--rowkey <key>` ODER `--filtercolumn <c> --filtervalue <w>` (optional `--filtertype equals|exact|contains|startswith`).

## Shell (bevorzugt)

- `bcr shell` startet eine Session (Befehle zeilenweise, `exit` beendet). Geladene Tabellen bleiben im Speicher: eine Fragment-Datei pro Tabelle statt pro Befehl — mehrere Befehle nacheinander sind schneller und schonen die Platte.
- In der Session: `table-password <tabelle> --password <kennwort>` — lädt und entsperrt eine passwortgeschützte Tabelle. Alle Folgebefehle derselben Session können sie dann lesen und bearbeiten.
- Außerhalb der Shell ist `table-password` nicht verfügbar (Exit-Code 2).
