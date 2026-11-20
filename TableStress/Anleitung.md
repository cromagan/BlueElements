# TableStress — Anleitung

Stresstest-Console für BlueTable-Tabellen: Probiert in kurzen Abständen alle üblichen Änderungen durch und schreibt dabei ein Protokoll, anhand dessen eine KI Fehlerursachen und Reparaturen ableiten kann.

## Wenn diese Datei "Ausgeführt" werden soll:
Öffne die angeforderte Datei. Die Dateien sind im Unterordner Test-Tabellen. Starte das Programm 10 mal mit der gleichen Datei. Jeweils mit der Usernummer 0 bis 9.
Nachdem alle 10 Programme fertig sind, prüfen die erste Zeile des Protokolls - dort findest du, welche Protokolldatei dazugehört.
Alle Fehler, alle Warnungen, alle Infos reparieren. Dann so lange neu starten, bis keine Fehler mehr auftreten.
Das gilt ausdrücklich auch für das Stresstest-Programm selbst: Es ist **gewollt und Teil der Aufgabe**, TableStress laufend zu verbessern — etwa wenn Protokollzeilen fehlen (zu wenige Meldungen), das Protokoll flutet (zu viele Meldungen), Kontexte zur Fehlerursache fehlen oder neue Testszenarien und bessere Auswertbarkeit helfen würden.
Wenn das Programm nicht funktioniert wie es soll oder du nicht weiterkommst, baue in TableStress neue Tests, neue Ausgaben oder was auch immer ein, dass es klappt.

## Aufruf

```
TableStress.exe <pfad-zur-tabelle> <usernummer>
```

- `<pfad-zur-tabelle>`: vollständiger Pfad **mit Dateiendung** (z. B. `C:\Test\TEST.mbdb`).
- `<usernummer>`: Zahl; der Benutzer wird auf `B<usernummer>` gesetzt (z. B. `B7`).

Beim Start startet sich dieselbe exe zusätzlich als **Watchdog-Prozess** (`--watchdog <pid> <protokoll>`). Der Watchdog tötet das Programm nach 11 Minuten (Laufzeit + 1 Minute) hart und schreibt dann einen Eintrag ins Protokoll — er schlägt also nur zu, wenn das Programm hängt; endet das Programm regulär, beendet sich der Watchdog selbst.

## Ablauf

1. Protokoll anlegen: Dateiname `Benutzer-Uhrzeit.txt` (z. B. `B7-14-05-09.txt`) im **Ordner der Tabelle**; Develop-Meldungen ab Warning laufen dorthin um.
2. Tabelle laden (`Table.Get` + `BeSureToBeUpToDate`).
3. 2-Sekunden-Timer: pro Tick eine Zufalls-Aktion (Wert 0-26). Master setzen und Komplettierung anstoßen sind nur in den **ersten 2 Minuten** erlaubt — danach laufen diese Ticks als Idle (wird protokolliert, ohne die Tabelle zu ändern).
4. Alle 15 Ticks: invalidierte Zeilen abarbeiten und speichern — so bleiben Änderungen auch bei einem Kill erhalten.
5. Nach 10 Minuten reguläres Ende: speichern, Tabelle freigeben, ExitCode 0.

## Zufalls-Aktionen

| Wert | Aktion |
|------|--------|
| 0-3 | Neue Zeile anlegen (Muss-Spalten: Erstspalte, Chunk, Unique werden mit Zufallswerten gefüllt) |
| 4 | Zufällige Zeile löschen |
| 5-20 | Zufälligen Zellwert einer zufälligen Zeile ändern (Systemspalten `SYS_*` ausgenommen) |
| 21 | Master-User setzen (`MasterMe()`) — nur in den ersten 2 Minuten, danach Idle |
| 22 | Komplettierung anstoßen (Master + Refresh-Zyklus; ein neuer Snapshot entsteht nur, wenn der letzte > 60 min alt ist) — nur in den ersten 2 Minuten, danach Idle |
| 23 | Table-Script erstellen (Syntax vorab per `ScriptPreCheck` geprüft) |
| 24-26 | Alle Daten laden (`LoadTableRows(false, -1)`) |

Laufzeit gesamt: **10 Minuten**.

Alle neuen Werte tragen den Benutzernamen (`B<n>-Zeit-Zufall`); Zeilen-Ersteller und Zellen-Änderer setzt das System ohnehin auf den aktuellen Benutzer.

## Protokollformat

Jede Zeile: `Protokolldateiname|Uhrzeit|AKTION|Detail`

Aktionen: `START`, `MASTER`, `KOMPLETTIERUNG`, `ZELLE`, `NEUEZEILE`, `LÖSCHEN`, `SCRIPT`, `DATENLADEN`, `SPEICHERN`, `MELDUNG` (Develop ab Warning), `FEHLER`, `ENDE`, `WATCHDOG`.

`FEHLER`-Zeilen enthalten Aktion, Exception-Typ, Meldung und `Develop.DiagStack()` — genug Kontext zur Reparatur, aber bewusst keine kompletten Stacktraces oder Zellinhalte.

## Auswertung durch eine KI

- Zeilen mit `FEHLER` zuerst prüfen; dazu die umliegenden `SPEICHERN`/`ZELLE`/`NEUEZEILE`-Zeilen (Key + Spalte sind immer mitgeloggt).
- `SPEICHERN … FEHLER:` deutet auf Fragment-/Dateiprobleme hin; betroffene Zeilen-Keys stehen in den vorherigen Aktions-Zeilen.
- ExitCodes: `0` reguläres Ende, `1` Fehler beim Laden/Lauf, `2` falsche Argumente, Watchdog-Kill → Protokoll endet ohne `ENDE`.
- Der Test **verändert die Tabelle** — immer nur gegen Kopien laufen lassen.

## Projektstruktur

- `Program.cs` — Einstieg, Argumente, Watchdog-Start
- `StressRunner.cs` — Timer, Zufalls-Aktionen, Speichern
- `StressProtocol.cs` — Protokolldatei, Develop-Umleitung, Watchdog-Anhang
- `Watchdog.cs` — eigener Prozess derselben exe
- `StressAction.cs` — Enum der Aktionen


## Bereits abgeklärte Konventionen

Entscheidungen aus bisherigen Läufen — nicht neu diskutieren, sondern beachten:

- Die `_000`-Prüfung in den `StartWriter`-Routinen (BlueBasics/BlueTable) **muss bleiben**: Ein Zähler-Anhang macht den Namen unbrauchbar, weil die Aufräumarbeit den Zeitstempel aus den letzten 19 Zeichen des Namens liest. Namen mit `_000` führen bewusst zum Freeze mit klarer Ursache.

## WICHTIG
Bereits abgeklärte Konventionen nur nach Anweisung ergänzen.
Niemals Rückfragen machen, der Stresstest und deine Auswertung laufen ohne Aufsicht.
Formate, die mit BTB enden (TableFileNew), dürfen intern verändert werden. Diese Formate sind noch nicht aktiv.
Andere Formate dürfen nicht verändert werden. (TableFile)
Wiederhole den Stresstest, bis alles wie gewünscht funktioniert.

