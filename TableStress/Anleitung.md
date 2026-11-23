# TableStress — Anleitung

Stresstest-Console für BlueTable-Tabellen: Probiert in kurzen Abständen alle üblichen Änderungen durch und schreibt dabei ein Protokoll, anhand dessen eine KI Fehlerursachen und Reparaturen ableiten kann.

## Wenn diese Datei "Ausgeführt" werden soll:
Öffne die angeforderte Datei. Die Dateien sind im Unterordner Test-Tabellen. Starte das Programm 10 mal mit der gleichen Datei - im Versatz von 3 Sekunden. Jeweils mit der Usernummer 0 bis 9.
Nachdem alle 10 Programme fertig sind, prüfe die erste Zeile des Protokolls - dort findest du, welche Protokoll- und Debug-Protokolldatei dazugehört.
Alle Fehler, alle Warnungen, alle Infos reparieren. Dann so lange neu starten, bis keine Fehler mehr auftreten.
Das gilt ausdrücklich auch für das Stresstest-Programm selbst: Es ist **gewollt und Teil der Aufgabe**, TableStress laufend zu verbessern — etwa wenn Protokollzeilen fehlen (zu wenige Meldungen), das Protokoll flutet (zu viele Meldungen), Kontexte zur Fehlerursache fehlen oder neue Testszenarien und bessere Auswertbarkeit helfen würden.
Wenn das Programm nicht funktioniert wie es soll oder du nicht weiterkommst, baue in TableStress neue Tests, neue Ausgaben oder was auch immer ein, dass es klappt.

## Aufruf

```
TableStress.exe <pfad-zur-tabelle> <usernummer> [--chunk | --nochunk]
```

- `<pfad-zur-tabelle>`: vollständiger Pfad **mit Dateiendung** (z. B. `C:\Test\TEST.mbdb`).
- `<usernummer>`: Zahl; der Benutzer wird auf `B<usernummer>` gesetzt (z. B. `B7`).
- `--chunk`: stellt sicher, dass eine Chunk-Spalte existiert; fehlt sie, wird `CHUNK` (ChunkType.ByName) angelegt — damit übt der Test die Chunk-Verteilung. Für das explizite Anlegen **vor** der Runde (bevorzugt, weil dann der komplette Lauf inkl. Neuanlage von Zeilen mit Chunk-Werten übt): `bcr table-addcolumn <tabelle> CHUNK --chunk`.
- `--nochunk`: stellt sicher, dass **keine** Chunk-Spalte existiert; es wird nichts angelegt. Existiert doch eine, meldet das Protokoll einen `FEHLER` und der Lauf endet (ExitCode 1), statt still falsch zu testen.
- Ohne Schalter: die Tabelle bleibt, wie sie ist (keine automatische Erstellung mehr).

Beim Start startet sich dieselbe exe zusätzlich als **Watchdog-Prozess** (`--watchdog <pid> <protokoll>`). Der Watchdog tötet das Programm nach 11 Minuten (Laufzeit + 1 Minute) hart und schreibt dann einen Eintrag ins Protokoll — er schlägt also nur zu, wenn das Programm hängt; endet das Programm regulär, beendet sich der Watchdog selbst.

## Ablauf

1. Protokoll anlegen: Dateiname `Benutzer-Uhrzeit.txt` (z. B. `B7-14-05-09.txt`) im **Ordner der Tabelle**; Develop-Meldungen ab Warning laufen dorthin um. Die erste Protokollzeile (`DEBUGLOG`) nennt das Debug-Protokoll; es wird von Anfang an im **Verzeichnis des Stresstests** als `Trace-<Benutzer>-<Uhrzeit>.html` geführt.
2. Tabelle laden (`Table.Get` + `BeSureToBeUpToDate`).
3. 2-Sekunden-Timer: pro Tick eine Zufalls-Aktion (Wert 0-37). Master setzen und Komplettierung anstoßen sind nur in den **ersten 2 Minuten** erlaubt — danach laufen diese Ticks als Idle (wird protokolliert, ohne die Tabelle zu ändern).
4. Alle 15 Ticks: invalidierte Zeilen abarbeiten und speichern — so bleiben Änderungen auch bei einem Kill erhalten.
5. Nach 10 Minuten reguläres Ende: speichern, Tabelle freigeben, ExitCode 0.

## Zufalls-Aktionen

| Wert | Aktion |
|------|--------|
| 0-3 | Neue Zeile anlegen (Muss-Spalten: Erstspalte, Chunk und Unique werden — falls vorhanden — mit Werten gefüllt; die Chunk-Spalte erhält einen Pool-Wert, die übrigen Zufallswerte) |
| 4 | Zufällige Zeile löschen |
| 5-20 | Zufälligen Zellwert einer zufälligen Zeile ändern (Systemspalten `SYS_*` ausgenommen; trifft die Änderung die Chunk-Spalte, erhält sie einen Pool-Wert — die Zeile wechselt den Chunk-Ordner) |
| 21 | Master-User setzen (`MasterMe()`) — nur in den ersten 2 Minuten, danach Idle |
| 22 | Komplettierung anstoßen (Master + Refresh-Zyklus; ein neuer Snapshot entsteht nur, wenn der letzte > 60 min alt ist) — nur in den ersten 2 Minuten, danach Idle |
| 23 | Table-Script erstellen (Syntax vorab per `ScriptPreCheck` geprüft) |
| 24-26 | Alle Daten laden (`LoadTableRows(false, -1)`) |
| 27-28 | Stress-Spalte anlegen (`STRESS_<user>_<nr>`); ab 25 Stress-Spalten wird die älteste vorher entfernt |
| 29 | Älteste Stress-Spalte löschen |
| 30 | Zelle mit kritischen Sonderzeichen füllen (Umbruch, Pipe, Anführungszeichen, Unicode, Tabs, leer, sehr lang) |
| 31 | Zeile duplizieren (alle Werte einer Zufallszeile; Unique-Spalten und die Chunk-Spalte erhalten neue Werte — die Chunk-Spalte einen Pool-Wert) |
| 32 | Caption und QuickInfo einer Stress-Spalte ändern |
| 33 | Sortierung einer Zufallsspalte lesen (`RowSortDefinition`, ohne Änderung) |
| 34 | Duplikat-Suche auf einer Zufallsspalte (`RowCollection.DuplicateGroups`, ohne Änderung) |
| 35 | Eigenes Stress-Script löschen |
| 36 | In-Memory-Konsistenz: doppelte Zeilen-Keys, leere Chunk-Ids (nur mit Chunk-Spalte; ohne Chunk-Spalte sind leere Chunk-Ids korrekt), leere Spalten-Keys (ohne Änderung); protokolliert auch die Zahl der bekannten Chunk-Ids |
| 37 | CSV-Export ausführen (Zeichen- und Zeilenzahl im Protokoll, ohne Änderung) |
| 38-39 | Chunk-Wechsel: CHUNK-Zelle einer Zufallszeile auf einen anderen Pool-Wert setzen — die Zeile wandert beim Speichern in einen anderen Chunk-Ordner (ohne Chunk-Spalte: Idle-Meldung) |

Laufzeit gesamt: **10 Minuten**.

Chunk-Werte kommen aus einem festen Pool (`Alpha`, `Beta`, `Gamma`, `Delta`, `Epsilon`, `Zeta`), damit die Zahl der Chunk-Ordner überschaubar bleibt und sich die Zeilen nachweisbar zwischen den Ordnern verteilen und wandern. Kritische Sonderzeichen erreichen die Chunk-Spalte weiterhin über Aktion 30 — die Chunk-Id-Normierung (lowercase, max. 12 Zeichen) wird damit mitgeprüft.

Alle neuen Werte tragen den Benutzernamen (`B<n>-Zeit-Zufall`); Zeilen-Ersteller und Zellen-Änderer setzt das System ohnehin auf den aktuellen Benutzer.

## Protokollformat

Jede Zeile: `Protokolldateiname|Uhrzeit|AKTION|Detail`

Aktionen: `DEBUGLOG` (erste Zeile: Pfad des Debug-Protokolls im Verzeichnis des Stresstests), `START`, `MASTER`, `KOMPLETTIERUNG`, `ZELLE`, `NEUEZEILE`, `LÖSCHEN`, `SCRIPT`, `SCRIPT-WEG`, `SPALTE-NEU`, `SPALTE-WEG`, `SPALTE-META`, `ZELLE-SPEZIAL`, `ZEILE-DUP`, `CHUNK-WECHSEL`, `SORTIERUNG`, `DUPLIKATE`, `PRÜFUNG`, `CSV`, `DATENLADEN`, `SPEICHERN`, `MELDUNG` (Develop ab Warning), `FRAGMENTE` (Datei-Zähler der Chunk-Ordner vor dem Ende, nur bei TableFileNew — System-Ordner und Daten-Ordner je mit Datei-Zahl, darin ist die Chunk-Verteilung nachweisbar), `FEHLER`, `ENDE`, `WATCHDOG`.

`FEHLER`-Zeilen enthalten Aktion, Exception-Typ, Meldung und `Develop.DiagStack()` — genug Kontext zur Reparatur, aber bewusst keine kompletten Stacktraces oder Zellinhalte.

## Auswertung durch eine KI

- Zeilen mit `FEHLER` zuerst prüfen; dazu die umliegenden `SPEICHERN`/`ZELLE`/`NEUEZEILE`-Zeilen (Key + Spalte sind immer mitgeloggt).
- `SPEICHERN … FEHLER:` deutet auf Fragment-/Dateiprobleme hin; betroffene Zeilen-Keys stehen in den vorherigen Aktions-Zeilen.
- ExitCodes: `0` reguläres Ende, `1` Fehler beim Laden/Lauf, `2` falsche Argumente, Watchdog-Kill → Protokoll endet ohne `ENDE`; das Debug-Protokoll liegt trotzdem im Verzeichnis des Stresstests.
- Der Test **verändert die Tabelle** — immer nur gegen Kopien laufen lassen.

## Projektstruktur

- `Program.cs` — Einstieg, Argumente, Watchdog-Start; Chunk-Modus per `--chunk` (Chunk-Spalte sicherstellen) oder `--nochunk` (FEHLER bei vorhandener Chunk-Spalte); ohne Schalter keine automatische Spalten-Erstellung
- `StressRunner.cs` — Timer, Zufalls-Aktionen, Speichern
- `StressProtocol.cs` — Protokolldatei, Develop-Umleitung, Watchdog-Anhang
- `Watchdog.cs` — eigener Prozess derselben exe
- `StressAction.cs` — Enum der Aktionen
- `Verify.cs` — Prüf-Lauf ohne Änderungen: Verzeichnis-, Chunk-, Undo-, Speicher-/Lade-Rundtrip- und Reload-Prüfung
- `FragmentDump.ps1` — parst eine Fragment-Datei (.tblc/.tbls/.tblf) und listet die Sätze
- `ProtokollAuswertung.ps1` — fasst Stress-Protokolle zusammen (FEHLER, Warnungen, Aktionen, FRAGMENTE)

## Prüf-Lauf (--verify)

```
TableStress --verify <pfad-zur-tabelle> [dump-datei]
```

Lädt die Tabelle, prüft Kopf-Datei, Arbeitsverzeichnis, Ordner- und Dateinamen,
EOF-Marker jeder Fragment-Datei, die Zuordnung Zeilen → Chunk-Ordner (`CHUNK-VERTEILUNG`
listet je Chunk-Id die Zeilen- und Datei-Zahl) und die
Änderungs-Historie gegen die Festplatte (je Eintrag der Ziel-Chunk; Fragment-Layout:
Snapshots und Journale zusammen). Danach Speichern-Neuladen-Rundtrip: Datenstand
und Historie müssen unverändert bleiben. Schreibt einen Bericht `VERIFY-*.txt`
im Ordner der Tabelle. ExitCodes: 0 = keine FEHLER, 1 = FEHLER, 2 = Argumente,
3 = abgebrochen.


## Bereits abgeklärte Konventionen

Entscheidungen aus bisherigen Läufen — nicht neu diskutieren, sondern beachten:

- Die `_000`-Prüfung in den `StartWriter`-Routinen (BlueBasics/BlueTable) **muss bleiben**: Ein Zähler-Anhang macht den Namen unbrauchbar, weil die Aufräumarbeit den Zeitstempel aus den letzten 19 Zeichen des Namens liest. Namen mit `_000` führen bewusst zum Freeze mit klarer Ursache.
- Undo-Einträge für Zeilen **ohne Chunk-Wert liegen im Haupt-Daten-Chunk `_`**, nicht im Kopf: `ChunkId()` normiert den leeren Chunk-Wert zu `_`, und die SaveableFile-Verteilung nutzt genau diese IDs. Die Kopf-Snapshots/Journale halten nur die Nicht-Chunk-Bereiche. Prüfungen auf Undo-Historie müssen deshalb je Chunk-Ordner die Union **aller** `.tblc` lesen (der Ordner hält je Benutzer eine write-once Datei), nicht nur die neueste.
- **Die Chunk-Spalte ist zugleich Unique-Spalte:** `UniqueValueDefinition.Repair()` nimmt sie per `ColumnsIncludingChunk` in jede Unique-Definition auf (Unique gilt je Chunk). Code, der Muss-Spalten füllt, muss die Chunk-Spalte deshalb **vor** den Unique-Spalten prüfen — sonst erhält sie Zufallswerte statt Pool-Werte (sichtbar an Einzel-Chunks wie `b0_174747`).

## WICHTIG
Bereits abgeklärte Konventionen nur nach Anweisung ergänzen.
Niemals Rückfragen machen, der Stresstest und deine Auswertung laufen ohne Aufsicht.
Formate, die mit BTB enden (TableFileNew), dürfen intern verändert werden. Diese Formate sind noch nicht aktiv.
Andere Formate dürfen nicht verändert werden. (TableFile)
Wiederhole den Stresstest, bis alles wie gewünscht funktioniert.

