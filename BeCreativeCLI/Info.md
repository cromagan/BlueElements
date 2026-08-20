# BeCreativeCLI (bcr)

Konsolen-Anwendung (CLI) für Table-Bearbeitungen. Bedienbar über PowerShell und CMD.
Exe-Name: `bcr` (AssemblyName), Projektname: `BeCreativeCLI`.

## Architektur

- Referenziert nur `BlueTable` (transitiv `BlueBasics` + `BlueScript`), kein `BlueControls`.
- Ein Befehl = eine Datei = eine Klasse, orientiert am `ScriptCommands`-Muster in BlueTable.
- Abstrakte Basis: `CliCommand` (Properties: `Command`, `Description`, `Syntax`; Methode: `DoIt`).
- Befehls-Discovery automatisch über `AssemblyAwareCache<CliCommand>` — `bcr help` listet
  dadurch alle Befehle auf, ohne dass eine Stelle manuell gepflegt werden muss.
- Keine externen NuGet-Pakete.

## Exit-Codes

| Code | Bedeutung |
|------|-----------|
| 0    | Erfolg |
| 1    | Fehler (z. B. Tabelle nicht gefunden, Zelle nicht editierbar) |
| 2    | Benutzungsfehler (falsche Argumente) |

Daten-Ausgabe erfolgt als CSV auf stdout, Fehler und Develop-Meldungen auf stderr.

## Zeilenadressierung

Alle Befehle, die Zeilen ansprechen (`get`, `set`, `delrow`, `info --row`, `content`),
adressieren Zeilen ausschließlich über:

- `--rowkey <key>` — genau eine Zeile via `Row.GetByKey()`
- `--filtercolumn <name> --filtervalue <wert>` — alle Zeilen, deren Zellwert passt
- `--filtertype <typ>` — optional, Vergleichsart:
  - `equals` (Default): Istgleich, Groß-/Kleinschreibung ignoriert
  - `exact`: Istgleich, Groß-/Kleinschreibung beachtet
  - `contains`: Wert ist im Zellwert enthalten (Groß/klein egal)
  - `startswith`: Zellwert beginnt mit Wert (Groß/klein egal)

Genau eine Adressierungsart pro Aufruf (entweder `--rowkey` ODER Filter), sonst Fehler (Exit 2).
Bei `get` muss die Trefferzahl exakt 1 sein, sonst Fehler mit Trefferanzahl.

## Befehle

| Befehl | Syntax | Beschreibung |
|--------|--------|--------------|
| help | `bcr help [befehl]` | Listet alle Befehle bzw. Details zu einem Befehl |
| info | `bcr info <tabelle>` | Name, Typ, Datei, Zeilen-/Spaltenanzahl |
| | `--columnnames` | Alle Spalten-KeyNames |
| | `--rowkeys` | Alle Zeilen-Keys |
| | `--column <name>` | Metadaten einer Spalte (Name, Format, Editierbarkeit) |
| | `--row` + Zeilenadressierung | Werte aller adressierten Zeilen als CSV (Header + Datenzeilen), optional `--max <n>` |
| content | `bcr content <tabelle> --column <name> [--max <n>]` | Werte einer Spalte, optional Obergrenze |
| export | `bcr export <tabelle> [--sep <c>] [--noheader]` | CSV-Export der Tabelle auf stdout (Datei-Ausgabe bewusst über Umleitung, z. B. `> datei.csv`) |
| get | `bcr get <tabelle> --column <name>` + Zeilenadressierung | Einzelner Zellwert |
| set | `bcr set <tabelle> --column <name> --value <wert>` + Zeilenadressierung | Zellwert setzen, alle Treffer |
| addrow | `bcr addrow <tabelle> [--firstvalue <wert>]` | Zeile anlegen |
| delrow | `bcr delrow <tabelle>` + Zeilenadressierung | Alle Treffer löschen |

`tabelle` ist ein Dateipfad oder Tabellenname; unterstützte Suffixe über `Table.Get()`:
`.bdb`, `.csv`, `.hbdb`, `.mbdb`, `.tblh`, `.tblj`, `.mtblj`

## Fragment-Tabellen (.mbdb / .mtblj)

Änderungen landen im Fragment (`Frgm\`-Ordner) — pro CLI-Prozess eine Fragment-Datei
(Standard-Mechanik des Systems). Die Komplettierung in die Hauptdatei übernimmt später
der temporäre Master-Mechanismus. Die CLI disposen die Tabelleninstanz sauber, damit
der Writer ordentlich geschlossen wird.

## Code-Stil

Wie im Repository üblich (siehe AGENTS.md): UTF-8 mit BOM, file-scoped Namespaces,
ägyptische Klammern, `var`, keine optionalen Parameter, Events klassisch.
`Console.WriteLine` ist hier bewusst das Ausgabe-Medium (Konsolen-App, kein Debug-Code).

## Zeichenkodierung

Die Exe gibt UTF-8 (ohne BOM) aus. In der klassischen Konsole/PowerShell 5.1 mit OEM-Codepage
sollte dafür vor der Nutzung gesetzt werden:

```powershell
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
```

Windows Terminal und PowerShell 7 zeigen die Ausgabe ohne Anpassung korrekt.
