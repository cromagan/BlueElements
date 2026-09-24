# BeCreativeCLI (bcr) — Hinweise für KI-Agenten

`bcr` ist eine Konsolen-App (steht in den Umgebungsvariablen, kein Pfad nötig).
Bei „BCR“, „ShellCommand bcr“ etc. ist IMMER diese Exe gemeint — direkt in der Shell ausführen.

## Was ist das?

Kommandozeilen-Werkzeug für BeCreative-Dateien. Tabellen (Datenbank-Formate
wie `.bdb`, `.tblh`, `.mbdb`) tragen das Präfix `table-`; zusätzlich gibt es
den Roundtrip-Test `roundtrip` für Layout- und Tabellendateien. Für Skripte,
Automatisierung und schnelle Änderungen ohne GUI.

## Wichtig

- Tabellenname ohne Pfad = aktuelles Arbeitsverzeichnis. Konsole also per workdir in den Tabellen-Ordner setzen.
- Exit-Codes: 0 = Erfolg, 1 = Fehler, 2 = Benutzungsfehler (`$LASTEXITCODE` prüfen).
- Daten auf stdout (CSV, UTF-8), Fehler auf stderr.
- Rechte-Modell der CLI: Die CLI ist kein Tabellen-Benutzer und ignoriert die Benutzergruppen-Rechte (Spalten-Rechte, Neue Zeilen, Administratoren). Erlaubte Aktionen stehen als CLI-Rechte (englische Texte) in den Tabellen-Eigenschaften (Tab 'CLI-Rechte') und werden direkt als String verglichen: `Create row` (table-addrow), `Delete row` (table-delrow), `Move rows` (table-cellset auf die Sortierindex-Spalte SYS_ROWSORTINDEX — verschiebt die Zeile), `Change cell values` (table-cellset, table-replace), `Remove row lock` (Schreiben der Sperrspalte SYS_LOCKED), `Edit script` (table-scriptedit), `Execute script` (table-scriptexecute), `Add column` (table-addcolumn), `Delete column` (table-delcolumn), `Edit table head` (table-head). Ohne das jeweilige Recht meldet der Befehl sofort einen Fehler.
- Tabellen mit Kennwort: per `--password <kennwort>` mitgeben (gilt für alle table-Befehle, die die Tabelle laden). Ohne korrektes Kennwort sofortiger Fehler.
- Dateiendungen: Nur `.bdb`, `.mbdb`, `.tblh`, `.tblj`, `.mtblj` werden akzeptiert (oder der Name ganz ohne Endung). Ein anderer Name wird abgelehnt, statt stillschweigend eine gleichnamige Tabelle zu öffnen.
- Nicht beschreibbare Spalten: Spalten, die auf Werte einer anderen Tabelle verknüpft sind, und berechnete Spalten (`WirdGespeichert: nein`) lehnen Schreibversuche (`table-cellset`, `table-replace --column`, `table-addrow --set`) mit Fehler ab — in der GUI oder in der Quell-Tabelle ändern. `table-replace` ohne `--column` überspringt diese Spalten und nennt sie auf stderr.
- Spalten anlegen/löschen: `table-addcolumn` und `table-delcolumn` (nur mit den CLI-Rechten `Add column` bzw. `Delete column`); gültige Formate listet `table-columnformats`. Systemspalten sind geschützt.
- Skripte: `table-scriptexecute <tabelle> --name <skript>` führt ein Tabellen-Skript aus (Zeilen-Skripte mit `--rowkey`), `table-scriptedit <tabelle> --name <skript> --file <datei>` legt ein Skript an oder ersetzt den Text komplett (bei Syntax-Fehlern wird nichts geändert), `script-syntax [filter]` listet die Syntax aller Skript-Befehle auf.
- Kapitel sind kein eigenes CLI-Konzept: Die Kapitelspalte enthält je Zeile den Text des Kapitels, zu dem die Zeile gehört, und ist mit `table-cellset` bearbeitbar wie jeder Zellwert (CLI-Recht `Change cell values` vorausgesetzt). Bei Aufgaben wie „Zeile(n) unter Kapitel X anlegen/einfügen“ gehört dazu BEIDES: Position unter der Kapitelzeile UND Kapitelspalte der neuen Zeilen mit dem Kapiteltext setzen (`table-addrow`, danach `table-cellset`). Eines allein ist unvollständig.
- Kapitel sind mehrstufig: Stufe 1\Stufe 2\Stufe 3
- Eine Zeile kann mehreren Kapiteln zugeordnet werden. Getrennt mit \r
- Fragment-Tabellen: `.mbdb` (TableFragments) ist bearbeitbar — das System erkennt selbst, ob eine geeignete Fragment-Datei fortgeführt wird (siehe eigenen Abschnitt unten); `.mtblj` (TableJsonFragments) bleibt für Bearbeitungen gesperrt. Lesen (`table-info` etc.) geht überall.
- Datenüberprüfung: Geänderte Zeilen werden invalidiert und beim Freigeben der Tabelle geprüft — vor dem Entladen, ohne Neuladen. Row-Skripte laufen dabei; Meldungen erscheinen über das normale Meldungswesen auf stderr.
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
- Die Systemspalte `SYS_ROWSORTINDEX` hält die Sortiernummern der Zeilen lückenlos: Werte werden beim Anlegen neuer Zeilen aufgefüllt und beim Setzen verschoben. Sie ist eine ganz normale Spalte (in `--columnnames` sichtbar, per `--column` abfragbar, mit im Export) und existiert nur, wenn sie angelegt wurde. Ändern ihres Werts braucht das CLI-Recht `Move rows`; sie sortiert die Ansicht NICHT automatisch.
- Zeilen-Keys sind Zeitstempel-artige Longs, sie ändern sich nie — nach einmaligem Ermitteln wiederverwendbar.

## Kurzbefehle (Tabellen)

- `bcr table-addrow <tabelle> [--firstvalue <w>] [--set <spalte>=<wert>]` — Zeile anlegen, Wert setzt die Erstspalte; `--set` ist wiederholbar und setzt weitere Spalten direkt beim Anlegen (speichert)
- `bcr table-cellget <tabelle> --column <c>` + Zeilenadressierung — Zelle lesen (Zeile muss eindeutig adressiert sein)
- `bcr table-cellset <tabelle> --column <c> --value <w>` + Zeilenadressierung — Zelle setzen (speichert); `--dry-run` zeigt nur die betroffenen Zeilen-Keys; mit `--column SYS_ROWSORTINDEX` wird die Sortiernummer gesetzt und Nachbarzeilen werden lückenlos verschoben (CLI-Recht `Move rows`)
- `bcr table-replace <tabelle> --find <text> --replace <ersatz> [--column <c>] [+ Zeilenadressierung]` — Suchen & Ersetzen über Zellen, entity-bewusst (Suche auf dekodiertem Text: 'gewürfelten' trifft auch gew&#252;rfelten); `--dry-run` zeigt Fundstellen ohne zu ändern (speichert)
- `bcr table-delrow <tabelle>` + Zeilenadressierung — Zeilen löschen (speichert); `--dry-run` zeigt nur die Keys
- `bcr table-search <tabelle> --value <w> [--column <c>] [--max <n>] [--context <zeichen>]` — Suche auf dekodiertem Text (Umlaut-Entities werden mitgefunden, Groß-/Kleinschreibung egal); Ausgabe pro Treffer: `Spalte <c> Zeile <key>: <treffer>` — zeichenbasierter Kontext (Standard 40 Zeichen je Seite, reißt nicht mitten im Wort ab); Key direkt als `--rowkey` verwendbar
- `bcr table-columncontent <tabelle> --column <c> [--max <n>]` — alle Werte einer Spalte
- `bcr table-export <tabelle> [--sep <trennzeichen>] [--noheader] [--no-system-columns] [--decode] [+ Zeilenadressierung]` — CSV auf stdout; `--decode` gibt echte Umlaute statt Entities aus (gut für Diffs/Reviews, nicht für Rückverarbeitung); Zeilenadressierung begrenzt die Auswahl
- `bcr table-head <tabelle> tags <tags mit | getrennt>` — Tags des Tabellenkopfs setzen (leerer Wert entfernt alle; speichert). NUR mit dem CLI-Recht `Edit table head`.
- `bcr table-addcolumn <tabelle> <spaltenname> [--caption <text>] [--format <formatkey>] [--quickinfo <text>]` — Spalte anlegen (NUR mit dem CLI-Recht `Add column`; Standardformat TextOneLine; speichert)
- `bcr table-delcolumn <tabelle> <spaltenname>` — Spalte löschen (NUR mit dem CLI-Recht `Delete column`; Systemspalten sind geschützt; speichert)
- `bcr table-scriptexecute <tabelle> --name <skript> [--rowkey <key>]` — Tabellen-Skript ausführen (NUR mit dem CLI-Recht `Execute script`; Zeilen-Skripte brauchen `--rowkey`; speichert bei Änderungen)
- `bcr table-scriptedit <tabelle> --name <skript> --file <datei>` — Skript anlegen oder Text ersetzen (NUR mit dem CLI-Recht `Edit script`; bei Syntax-Fehlern nichts ändernd)
- `bcr script-syntax [filter]` — Syntax aller Skript-Befehle auflisten, optional gefiltert
- `bcr table-rowerrors <tabelle>` + Zeilenadressierung — Datenüberprüfung adressierter Zeilen; Exit-Code 1 bei Fehlern

Zeilenadressierung: `--rowkey <key>` ODER `--filtercolumn <c> --filtervalue <w>` (optional `--filtertype equals|exact|contains|startswith`). Der Zeilen-Key ist numerisch (Zeitstempel-artiger Long); `--row 123` mit `KEY=Wert`-Syntax ist falsch — Beispiele: `bcr help table-info`.

## Fragment-Tabellen bearbeiten (.mbdb)

Alle Bearbeitungs-Befehle (`table-addrow`, `table-cellset`, `table-replace`, `table-delrow`, `table-addcolumn`, `table-delcolumn`, `table-head`) arbeiten auch auf `.mbdb` (TableFragments) — ohne Zusatzschalter:

- Das System erkennt selbst, ob eine geeignete Fragment-Datei fortgeführt werden kann: Die letzte sauber geschlossene Fragment-Datei des eigenen Benutzers (jünger als 5 Minuten, endend mit `- EOF`) wird wiederaufgenommen: Der Writer wird direkt im Append-Modus auf diese Datei geöffnet und die neuen Änderungen werden angehängt. Damit bleiben alle Änderungen einer Aufgabenkette in EINER Fragment-Datei gebündelt.
- Gibt es keine geeignete Fragment-Datei (zu alt, kein EOF, fremder Benutzer), legt der Befehl beim ersten Schreiben selbst eine neue Fragment-Datei an.
- `--dry-run` öffnet den Fragment-Writer nicht und lässt die Dateien unangetastet; wird ein Befehl nach der Fortführung abgebrochen, erhält die Datei lediglich einen frischen `- EOF`-Marker.
- Auch bei Fragment-Tabellen gilt: Die CLI-Rechte der Tabelle werden als String verglichen und erzwingen dieselben Grenzen wie bei normalen Tabellen.

## Roundtrip-Test (roundtrip)

`bcr roundtrip <datei> [--full]` — lädt eine Datei, speichert sie ins Gegenstück-Format (altes Format <-> JSON), lädt sie zurück, speichert sie wieder im Originalformat und vergleicht bit-genau mit dem Original. Unterstützt Layouts (`.cfo`, `.bcr`) und Tabellen (`.bdb`<->`.tblj`, `.mbdb`<->`.mtblj`).

- Protokoll auf stdout; Exit-Code 0 = bit-genau identisch, 1 = Abweichung/Fehler, 2 = Benutzungsfehler.
- Bei Abweichung: Diff-Analyse mit erster abweichender Byte-Position, Kontext davor/danach als Text (Steuerzeichen escaped: `\r`, `\n`, `\t`, `\0`) UND als Hex-Dump — damit die Ausgabe verlustfrei kopierbar ist.
- `--full`: zusätzlich den kompletten Inhalt beider Dateien, zeilenweise (an CR getrennt) mit Byte-Offset und escapeden Steuerzeichen.
- `.bdb`/`.mbdb` sind ZIP-Container: Deren Eintrags-Zeitstempel verhindern Bit-Identität — der Befehl vergleicht deshalb zusätzlich den entpackten Inhalt (Main.bin) separat und meldet, ob nur Container-Metadaten oder echter Inhalt abweichen.
- Das Original wird nur gelesen; alle Zwischenschritte laufen auf temporären Kopien.

## Standard-Rezept für Aufgaben

1. `table-info <tabelle>` — gibt es die Tabelle, wie groß?
2. `table-info <tabelle> --columnnames` — welche Spalten, wo ist die Erstspalte?
3. `table-search <tabelle> --value <gesuchter Wert>` oder `table-info <tabelle> --rows` — Zeile(n) + Key(s) ermitteln
4. `table-info <tabelle> --row --rowkey <key>` — Ist-Zustand der Zeile prüfen
5. Ändern: `table-cellset` / `table-addrow` (mit `--set`) / `table-replace` / `table-delrow` — bei Unsicherheit vorher mit `--dry-run` prüfen; bei `.mbdb` (Fragment-Tabelle) wird eine geeignete Fragment-Datei automatisch fortgeführt bzw. neu angelegt
6. Schritte 3–4 zur Kontrolle wiederholen, `$LASTEXITCODE` prüfen