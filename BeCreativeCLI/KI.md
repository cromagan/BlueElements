# BeCreativeCLI (bcr) — Hinweise für KI-Agenten

`bcr` ist eine Konsolen-App (steht in den Umgebungsvariablen, kein Pfad nötig).
Bei „BCR“, „ShellCommand bcr“ etc. ist IMMER diese Exe gemeint — direkt in der Shell ausführen.

## Was ist das?

Kommandozeilen-Werkzeug für BeCreative-Dateien. Aktuell werden nur Tabellen
unterstützt (Datenbank-Formate wie `.bdb`, `.tblh`, `.mbdb`) — diese Befehle
tragen das Präfix `table-`. Für Skripte, Automatisierung und schnelle
Änderungen ohne GUI.

## Wichtig

- Tabellenname ohne Pfad = aktuelles Arbeitsverzeichnis. Shell also per workdir in den Tabellen-Ordner setzen.
- Exit-Codes: 0 = Erfolg, 1 = Fehler, 2 = Benutzungsfehler (`$LASTEXITCODE` prüfen).
- Daten auf stdout (CSV, UTF-8), Fehler auf stderr.
- Die CLI läuft mit Benutzergruppe `#CLI` (nicht Administrator): Bearbeitungen brauchen `#CLI` als Recht (Spalten-Bearbeitung, Neue Zeilen, Tabellen-Admin für table-delrow).
- Tabellen mit Kennwort: außerhalb der Shell nicht benutzbar (sofortiger Fehler). In einer Shell-Session zuerst mit `table-password` entsperren, dann normal bearbeiten.
- Die CLI kann KEINE Spalten anlegen oder löschen — nur Zellwerte setzen, Zeilen anlegen/löschen. Fehlende Spalten müssen in der GUI erstellt werden.
- Kapitel sind kein eigenes CLI-Konzept: Die Kapitelspalte enthält je Zeile den Text des Kapitels, zu dem die Zeile gehört, und ist mit `table-cellset` bearbeitbar wie jeder Zellwert (Rechte für `#CLI` vorausgesetzt). Bei Aufgaben wie „Zeile(n) unter Kapitel X anlegen/einfügen“ gehört dazu BEIDES: Position unter der Kapitelzeile UND Kapitelspalte der neuen Zeilen mit dem Kapiteltext setzen (`table-addrow`, danach `table-cellset`). Eines allein ist unvollständig.
- Kapitel sind mehrstufig: Stufe 1\Stufe 2\Stufe 3
- Ist die Spalte SYS_ROWSORTINDEX vorhanden, ist nur EIN Kapitel möglich. Ansonsten kann eine Zeile mehreren Kapiteln zugeordner werden. Getrennt mit \r
- Mehrdeutige Aufträge (z. B. „unter Kapitel X“ = nur Position oder auch Kapitelwert setzen?) niemals raten — vor der Ausführung kurz nachfragen. Betroffene Spalten/Werte explizit nennen lassen, wenn der Auftrag sie nicht nennt.

## Struktur erkunden — NUR table-info verwenden (kein export, kein help nötig)

`bcr table-info <tabelle>` kennt alle Anzeige-Modi:

| Befehl | Liefert |
|---|---|
| `bcr table-info <tabelle>` | Übersicht: Name, Typ, Datei, Zeilen-/Spaltenzahl |
| `bcr table-info <tabelle> --columnnames` | Alle Spaltennamen mit Beschriftung; Funktion markiert: `Erstspalte`, `Chunkspalte`, `Kapitelspalte` |
| `bcr table-info <tabelle> --rowkeys` | Nur die Zeilen-Keys |
| `bcr table-info <tabelle> --rows [--max <n>]` | Alle Zeilen: Key + FirstValue (Wert der Erstspalte) |
| `bcr table-info <tabelle> --firstvalues [--max <n>]` | Nur die Erstwerte |
| `bcr table-info <tabelle> --column <spalte>` | Spaltenmetadaten: KeyName, Bezeichnung, Mehrzeilig, ErsteSpalte, Schluesselspalte, Kapitelspalte, WirdGespeichert, AdminInfo, QuickInfo |
| `bcr table-info <tabelle> --row` + Zeilenadressierung `[--max <n>]` | Tab-getrennte Werte der adressierten Zeilen, mit Spaltennamen-Kopfzeile |

Wichtige Stolperfalle:
- **FirstValue = Wert der Erstspalte (Primärschlüssel), NICHT die erste Spalte eines CSV-Exports** — der Export nutzt die Speicherreihenfolge und lässt Spalten mit `WirdGespeichert: nein` weg.
- Die Systemspalte `SYS_ROWSORTINDEX` existiert nur, wenn die benutzerdefinierte Sortierung aktiviert ist. Dann ist sie eine ganz normale Spalte (in `--columnnames` sichtbar, per `--column` abfragbar, mit im Export); ohne aktive Sortierung existiert sie nicht.
- Zeilen-Keys sind Zeitstempel-artige Longs, sie ändern sich nie — nach einmaligem Ermitteln wiederverwendbar.

## Kurzbefehle (Tabellen)

- `bcr table-addrow <tabelle> [--firstvalue <w>]` — Zeile anlegen, Wert setzt die Erstspalte (speichert)
- `bcr table-cellget <tabelle> --column <c>` + Zeilenadressierung — Zelle lesen (Zeile muss eindeutig adressiert sein)
- `bcr table-cellset <tabelle> --column <c> --value <w>` + Zeilenadressierung — Zelle setzen (speichert)
- `bcr table-delrow <tabelle>` + Zeilenadressierung — Zeilen löschen (speichert)
- `bcr table-search <tabelle> --value <w> [--column <c>]` — Suche (Groß-/Kleinschreibung egal); Ausgabe pro Treffer: `Spalte <c> Zeile <key>: <treffer>` — der Treffer erscheint mit bis zu drei Wörtern Kontext davor und danach (gekürzte Seiten als `...`); Key direkt als `--rowkey` verwendbar
- `bcr table-columncontent <tabelle> --column <c> [--max <n>]` — alle Werte einer Spalte
- `bcr table-export <tabelle> [--sep <trennzeichen>] [--noheader]` — CSV auf stdout (nur für Exportzwecke, nicht zur Analyse nötig)

Zeilenadressierung: `--rowkey <key>` ODER `--filtercolumn <c> --filtervalue <w>` (optional `--filtertype equals|exact|contains|startswith`).

## Standard-Rezept für Aufgaben

1. `table-info <tabelle>` — gibt es die Tabelle, wie groß?
2. `table-info <tabelle> --columnnames` — welche Spalten, wo ist die Erstspalte?
3. `table-search <tabelle> --value <gesuchter Wert>` oder `table-info <tabelle> --rows` — Zeile(n) + Key(s) ermitteln
4. `table-info <tabelle> --row --rowkey <key>` — Ist-Zustand der Zeile prüfen
5. Ändern: `table-cellset` / `table-addrow` / `table-delrow`
6. Schritte 3–4 zur Kontrolle wiederholen, `$LASTEXITCODE` prüfen

## Shell (bevorzugt)

- `bcr shell` startet eine Session (Befehle zeilenweise, `exit` oder `quit` beendet, `#` kommentiert eine Zeile aus). Geladene Tabellen bleiben im Speicher: eine Fragment-Datei pro Tabelle statt pro Befehl — mehrere Befehle nacheinander sind schneller und schonen die Platte.
- In der Session: `table-password <tabelle> --password <kennwort>` — lädt und entsperrt eine passwortgeschützte Tabelle. Alle Folgebefehle derselben Session können sie dann lesen und bearbeiten.
- Außerhalb der Shell ist `table-password` nicht verfügbar (Exit-Code 2).