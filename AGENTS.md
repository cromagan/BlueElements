# BlueElements — Agent-Anweisungen


## WICHTIGSTE REGEL
Beginne JEDE Antwort mit meinem Namen: Christian


## ProjectAtlas (Atlas) benutzen

Das Repo ist mit ProjectAtlas indexiert (`.projectatlas/`). Für Code-Navigation Atlas-Tools VOR den eingebauten grep/glob/read benutzen:

- Vor JEDER Aufgabe: `atlas_scan` oder `atlas_watch_once` ausführen, damit der Index aktuell ist (ich arbeite parallel am Repo — der Index veraltet schnell)
- Session-Start: `atlas_session_brief` mit der Aufgabe als Query — liefert Startpunkte statt blindem Suchen
- Dateien/Symbole/Inhalte finden: `atlas_files`, `atlas_symbols`, `atlas_search` statt grep/glob
- Quelltext lesen: `atlas_outline` und `atlas_slice` (Symbol- oder Zeilenbereiche) statt ganzer Dateien lesen
- Nach eigenen Datei-Änderungen: `atlas_watch_once` ausführen, damit der Index aktuell bleibt

Nur wenn Atlas nichts liefert (z. B. Index veraltet oder Typ nicht erfasst), auf grep/glob/read zurückfallen.

- Geklappt hat es, wenn die Ausgabe unter `scan:` Abschnitte wie `overview`, `text_index` und `symbols` mit Dateizahlen zeigt (z. B. `files: 1057`, `symbols.parsed: 802`).
- Referenzwerte vom 25.08.2026: 1057 Dateien, 70 Ordner, 890 textindiziert, 9452 Symbole, 33262 Relationen.


## Build & Ausführung

```bash
dotnet build BeCreative.sln               # Alles bauen
dotnet build BeCreative\BeCreative.csproj # Nur die Haupt-App bauen
```

Es gibt keine Testsuite und keine CI/CD-Pipeline. Alle Prüfungen erfolgen manuell.

## Projektstruktur

7 Projekte in einer Solution. Die Abhängigkeitsreihenfolge ist wichtig:

```
BlueBasics          (Basis, keine Abhängigkeiten)
  ← BlueScript        (Skript-Engine, Variablen, Methoden)
    ← BlueTable       (Tabellen-Datenhaltung, CSV, Lokalisierung)
      ← BlueControls  (WinForms-Controls, Skin-Engine, Forms) ← BlueInternet (KI-Integration)
      ← BluePaint     (Bildbearbeitung)                       ← BlueInternet (KI-Integration)
  ← BlueControls
```

`BeCreative` ist der WinExe-Einstiegspunkt und referenziert alle anderen Projekte.

Für isolierte Entwicklung existieren einzelne Library-Solutions (BlueBasics.sln, BlueScript.sln usw.).

**Projektbeschreibungen:**

| Projekt | Zweck |
|---------|-------|
| **BlueBasics** | Grundlegende Utility-Klassen: Datei-I/O (`IO`), Typkonvertierung (`Converter`), Logging (`Develop`), Erweiterungsmethoden für Strings, Zahlen, Farben, Bitmaps, Collections. Keine externen Abhängigkeiten. |
| **BlueScript** | Eigene Skriptsprache mit Variablen (`Variable`, `VariableCollection`), ~80+ integrierte Methoden (`Method_*`) und Script-Execution (`Script`). |
| **BlueTable** | In-Memory-Tabellensystem (`Table`, `ColumnItem`, `RowItem`), CSV-Import/Export (`CsvHelper`), Übersetzungssystem (`LanguageTool`). |
| **BlueControls** | WinForms-Basisklassen und Controls. Zentrales Theming über `Skin`, eigene Form-Basisklasse (`BlueControls.Forms.Form`), Schriftverwaltung (`BlueFont`), Rechtschreibprüfung (`Dictionary`). |
| **BluePaint** | Bildbearbeitung mit Werkzeugen (`GenericTool`-Basisklasse) und eigenem Hauptfenster (`MainWindow`). |
| **BlueInternet** | KI-Integration für die Skript-Engine (`Method_Ai`, `VariableAi`). Wird von BlueControls und BluePaint referenziert. |
| **BeCreative** | Einstiegspunkt der Anwendung (`Program`). Referenziert alle anderen Projekte. |

## Code-Stil (abweichend von C#-Standards)

Alles wird über `.editorconfig` erzwungen:

- **File-scoped Namespaces** (`namespace Foo;`), nicht block-scoped
- **Ägyptische Klammern**: `void Foo() {` — keine neue Zeile vor `{`
- **Keine neue Zeile vor** `else`, `catch`, `finally`
- **`using` außerhalb des Namespace** (als Error erzwungen)
- **`var` bevorzugt** überall, auch bei Built-in-Typen
- **Primary Constructors** bevorzugt
- **Switch Expressions** NICHT bevorzugt — Switch-Statements vorziehen
- **Geschweifte Klammern immer** auch bei Single-Statement-Bodies
- **XML-Doc-Kommentare** nicht erforderlich
- **Expression-bodied Members** bevorzugt
- **Event-Handler klassisch verdrahten** — keine Lambda-Ausdrücke beim Abonnieren von Events (`obj.Event += HandlerMethod;` statt `obj.Event += (_, e) => { ... };`). Ausnahme: die Logik erfordert zwingend den Zugriff auf lokale Variablen und eine Methode wäre unnatürlich; dann ist ein Lambda erlaubt. Benötigte Zustände stattdessen als Felder halten.
- **4 Leerzeichen Einrückung**, CRLF, keine Tabs
- **`readonly`-Felder** bevorzugt
- **Optionale Parameter** NICHT erwünscht!
- **`AsSpan()` / Range-Syntax `[x..y]`** statt `Substring`
- **Optionale Parameter**: Keine optionalen Parameter neu einführen

Welche Code-Analyzers auf `error` stehen, ist der `.editorconfig` zu entnehmen — nicht in dieser Datei dupliziert.

## Selbstständige Reparatur

Wird eine Datei verändert, zusätzlich folgende Reparaturen durchführen:
- **XML-Doc-Kommentare und Inline-Kommentare prägnant halten** — Veraltet, unklar oder fehlerhaft, reparieren. Jeder Kommentar auf das Wesentliche reduziert: eine Summary beschreibt WAS die Methode macht, nicht WIE oder WER sie aufruft. Keine Querverweise auf Aufrufer, keine `<see cref="..."/>`-Ketten ins Detail. Keine historischen Bezüge wie „ersetzt das alte", „früher", „bisher", „ursprünglich". Eine bis zwei Zeilen reichen fast immer.
- **Felder statt redundanter Variablen** — wenn eine lokale Variable nur ein Feld kapselt, direkt das Feld nutzen. Neue Hilfsvariablen nur erstellen, wenn sie echte zusätzliche Logik enthalten
- **`AsSpan()` / Range-Syntax `[x..y]`** statt `Substring`
- **Überflüssige String-Checks vermeiden** — Muster wie `var f =... ;  If (string.IsNullOrEmpty(f)) {return f;}` unwandeln in `if (... is { Length: > 0 } f) {return f;}`. Ziel: Einzeiliger kürzerer Code
- **Nullable Prüfungen** fehlende Nullable Prüfungen hinzufügen. Niemals mit `!` unterdrücken — zur Not redundante Prüfung ausführen
- **unbenutzte Felder, Routinen, etc.** Kommentar `TODO: Unused` hinzufügen, außer es ist bereits ein Kommentar dabei mit dem Text: `Used`

## Leicht übersehbare Konventionen

- **Bestehende Kommentare erhalten** — Kommentare nicht löschen, sondern bei Bedarf überarbeiten/aktualisieren.
- **Kein redundanter Code** — vor dem Schreiben einer neuen Routine prüfen, ob eine bestehende genutzt oder leicht angepasst werden kann. Auch die Sichtbarkeit darf angepasst werden.
- **Eine Datei pro Typ** — jede Klasse, Struktur, jedes Enum und jedes Interface kommt in eine eigene Datei (Dateiname = Typname).
  Ausnahme: Ein Interface `IXxx` soll zusammen mit der zugehörigen statischen Erweiterungsklasse `IXxxExtension` (die es per `Extensions`-Muster erweitert) in derselben Datei stehen.
- **Forms/UserControls brauchen eine Designer-Datei** — visuelle WinForms-Typen besitzen immer eine begleitende `.Designer.cs`-Datei (partial class), auch wenn sie leer ist.
- **Alle `.cs`-Dateien müssen UTF-8 mit BOM sein.** Das Skript `Convert-Encoding.ps1` normalisiert Kodierung und Stil.
- **Alle WinForms-Formulare müssen von `BlueControls.Forms.Form` erben**, nicht von `System.Windows.Forms.Form`. Das erzwingt `AutoScaleMode.None`.
- **DPI-Awareness ist global deaktiviert.** Niemals `DpiMode`-Setter aufrufen. Das Manifest setzt `dpiAware=false`. `Skin.Scale` ist hart auf `1.0f` kodiert.
- **LangVersion ist `preview`** — aktuelle C#-Features sind aktiviert.
- **Nullable Reference Types** sind aktiviert (`Nullable=enable`). Warnungen niemals mit `!` unterdrücken.
- **Lambda Ausdrücke** - Lambda Ausdrücke wenn möglich vermeiden. Sind sie von Vorteil informiere den User vorab, ob er das so will
- **Events** - nicht als Action oder Funct
- **Events auslösen über `OnXxx`-Methoden** — niemals `MyEvent?.Invoke(this, e)` direkt aufrufen. Stattdessen eine geschützte `protected virtual void OnXxx(...)`-Methode definieren, die den Invoke kapselt (inkl. `IsDisposed`-Prüfung), und überall diese Methode aufrufen. So können abgeleitete Klassen das Verhalten überschreiben.

## Wichtige Hilfsklassen (BlueBasics)

Diese Klassen sind die bevorzugten Einstiegspunkte für gängige Aufgaben. Niemals die direkten System-Äquivalente verwenden.

| Klasse | Zweck | Wichtigste Methoden |
|--------|-------|---------------------|
| **`Develop`** | Zentrales Logging, App-Start, Prozess-Steuerung | `DebugPrint()`, `Message()`, `StartService()`, `AbortExe()`, `AppPath()`, `DoEvents()` |
| **`Converter`** | Sichere Typkonvertierung (wirft nie) | `IntParse()`, `DoubleParse()`, `FloatParse()`, `LongParse()`, `DateTimeParse()`, `ColorParse()`, `Base64ToBitmap()` |
| **`IO`** | Datei-I/O mit Retry-Logik | `FileExists()`, `ReadAllText()`, `ReadAllBytes()`, `DeleteFile()`, `FileCopy()`, `MoveFile()`, `TempFile()`, `CanWriteFile()` |
| **`Generic`** | Allgemeine Hilfsmittel | `GetUniqueKey()`, `CopytoClipboard()`, `GetMD5Hash()`, `GetSHA256HashString()`, `LaunchBrowser()`, `Download()` |
| **`Geometry`** | Geometrie- und Mathematik-Hilfen | `GetLength()`, `GetAngle()`, `LinesIntersect()`, `PointOnLine()`, `Sinus()`, `Cosinus()` |
| **`Constants`** | Globale Konstanten, Zeichensätze, DateTime-Formate | `Char_az`, `Char_AZ`, `Char_Numerals`, `DateTimeFormats`, `Replacements` (Umlaut→ASCII) |
| **`OperationResult`** | Ergebnis-Typ für Dateioperationen | `IsSuccessful`, `IsFailed`, `FailedReason`, `Success`, `FailedInternalError` |

### Erweiterungsmethoden (partial class `Extensions` in `namespace BlueBasics`)

| Datei | Zieltyp | Wichtigste Methoden |
|-------|---------|---------------------|
| **StringExtension.cs** | `string` | `Left()`, `Right()`, `Mid()`, `SplitBy()`, `CountChar()`, `IsDateTime()`, `IsDouble()`, `CompareKey()`, `Encrypt()`/`Decrypt()`, `FromNonCritical()`/`ToNonCritical()` |
| **NumeralExtensions.cs** | `int`, `double`, `float`, `long` | `ToString1()` … `ToString1_5()` (invariant culture), `ToString10()` (festbreite für Sortierung) |
| **DateExtensions.cs** | `DateTime` | `ToString1()` … `ToString7()` (normierte Datumsformate) |
| **ListOfExtension.cs** | `List<T>`, `ICollection<T>` | `AddIfNotExists()`, `IsDifferentTo()`, `Load()`, `WriteAllText()`, `TagGet()`/`TagSet()`, `RemoveNull()`, `Shuffle()` |
| **ColorExtension.cs** | `Color` | `ToHtmlCode()`, `Name()`, `IsMagentaOrTransparent()` |
| **BitmapExtensions.cs** | `Bitmap` | `ApplyFilter()`, `IsValid()`, `FillCircle()`, `Magnify()` |
| **RectangleExtension.cs** | `Rectangle` | `Center()`, `Zoom()`, `Scale()`, `PointOf()` |
| **JsonExtensions.cs** | `JsonObject`, `JsonElement` | `GetBool()`, `GetInt()`, `GetString()`, `IsArray()`, `IsObject()` |
| **ByteArrayExtension.cs** | `byte[]` | `IsZipped()`, `UnzipIt()`, `ZipIt()` |

### Caching & Instanz-Verwaltung (BlueBasics)

| Klasse | Kurzbeschreibung |
|--------|------------------|
| **`AssemblyAwareCache<T>`** | Thread-sicherer Cache, der über alle geladenen Assemblies hinweg konkrete `T`-kompatible Typen mit parameterlosem Konstruktor sammelt, Instanzen lazy erzeugt (Key = `IHasKeyName.KeyName` oder voller Typname) und sich bei neu geladenen Assemblies automatisch erneuert. |
| **`ConcurrentCache<TKey, TValue>`** | Thread-sicherer Cache auf `ConcurrentDictionary`-Basis mit konfigurierbarem Eintragsmaximum; `IDisposable`-Werte werden beim Entfernen/`Trim` automatisch disposed, jede Instanz registriert sich bei `Generic.RegisterCacheTrim` (Reaktion auf Memory-Pressure). |
| **`LiveInstanceCache<T>`** | Abstrakte CRTP-Basis für Typen mit eigenem Live-Register (z. B. `Chunk`, `ConnectedFormula`, `Table`); liefert statische `LiveInstances`/`AllInstances()`, ein `Added`-Event, das Sync-Root `AllFilesLocker` und eine race-safe `GetOrCreate`-Factory. |
| **`ParseableItem`** | Abstrakte Basis für serialisierbare Objekte (`IParseable`/`ICloneable`/`INotifyPropertyChanged`/`IDisposableExtended`) mit `ParseableItems()`/`ParseThis()`/`Parse()` sowie statischen Factories `NewByParsing`/`NewByParsingJson`/`NewByTypeName` anhand der statischen `ClassId`-Property. |


## GIT
Mache niemals einen Git eigenständig rückgängig! Auch keine Änderungen von dir! Ich arbeite paralell - du verwirfst so auch meine Arbeit! Wenn das nötig sein sollte, frage mich!
Nutze NIEMALS den Stash! Damit machst du meine paralelle Arbeit kaputt. Lieber beende deine Arbeit und frage nach!
Du darfst ohne meine Erlaubniss auch nicht im Git nachschauen. Du solltst die Aufgaben mit Logik lösen
Kurzgesagt: Alle GIT Befehle sind für dich tabu, außer ich erlaube es dir. Du darfst mich aber um Erlaubniss fragen.

## Ratlos
Wenn du ratlos bist, frage den User, ob du Develop.Diagnose Aufrufe einbauen sollst - und DiagFlag auf true setzen.
Verschwende nicht zu viel Zeit, oft geht es schneller mit Diagnose-Aufrufen.
Kein Console.Writeline und auch ein Develop.DebugPrint
Bevor die diese wieder löscht, frag den Benutzer, ob sie gelöscht werden sollen.

**Develop.DiagStack()**: Liefert einen kompakten Aufruf-Stack als String — Methodennamen der Aufrufkette, durch `" <- "` getrennt (max. 8 Frames). Gedacht zum Einbetten in `Develop.Diagnose()`-Meldungen, um zu sehen, woher ein Code-Pfad aufgerufen wurde: `Develop.Diagnose("Typ", $"Stack: {Develop.DiagStack()}")`. Ist `DiagFlag` false, wird ein leerer String geliefert — verursacht also im Normalbetrieb keine Kosten.

## Sonstiges
Erinnere mich im Oktober daran, diese Werte zu entfernen und  zu den obsoleten Werten hinzuzufügen.
ValueRequired = 143 
EditableWithDropdown=  142 und _legacyDropdown
TextFormatingAllowed = 199 und _legacyTextFormating
SpellCheckingEnabled = 156 und _legacySpellChecking


