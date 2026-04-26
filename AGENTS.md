# BlueElements — Agent-Anweisungen

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
- **4 Leerzeichen Einrückung**, CRLF, keine Tabs
- **`readonly`-Felder** bevorzugt
- **Optionale Parameter** NICHT erwünscht!
- **`AsSpan()` / Range-Syntax `[x..y]`** statt `Substring`

Welche Code-Analyzers auf `error` stehen, ist der `.editorconfig` zu entnehmen — nicht in dieser Datei dupliziert.

## Selbstständige Reparatur

Wird eine Datei verändert, zusätzlich folgende Reparaturen durchführen:
- **XML-Doc-Kommentare** Veraltet, unklar oder fehlerhaft, reparieren
- **Felder statt redundanter Variablen** — wenn eine lokale Variable nur ein Feld kapselt, direkt das Feld nutzen. Neue Hilfsvariablen nur erstellen, wenn sie echte zusätzliche Logik enthalten
- **`AsSpan()` / Range-Syntax `[x..y]`** statt `Substring`
- **Überflüssige String-Checks vermeiden** — Muster wie `var f =... ;  If (string.IsNullOrEmpty(f)) {return f;}` unwandeln in `if (... is { Length: > 0 } f) {return f;}`. Ziel: Einzeiliger kürzerer Code
- **Nullable Prüfungen** fehlende Nullable Prüfungen hinzufügen. Niemals mit `!` unterdrücken — zur Not redundante Prüfung ausführen
- **unbenutzte Felder, Routinen, etc.** Kommentar `TODO: Unused` hinzufügen, außer es ist bereits ein Kommentar dabei mit dem Text: `Used`

## Leicht übersehbare Konventionen

- **Bestehende Kommentare erhalten** — Kommentare nicht löschen, sondern bei Bedarf überarbeiten/aktualisieren.
- **Kein redundanter Code** — vor dem Schreiben einer neuen Routine prüfen, ob eine bestehende genutzt oder leicht angepasst werden kann
- **Alle `.cs`-Dateien müssen UTF-8 mit BOM sein.** Das Skript `Convert-Encoding.ps1` normalisiert Kodierung und Stil.
- **Alle WinForms-Formulare müssen von `BlueControls.Forms.Form` erben**, nicht von `System.Windows.Forms.Form`. Das erzwingt `AutoScaleMode.None`.
- **DPI-Awareness ist global deaktiviert.** Niemals `DpiMode`-Setter aufrufen. Das Manifest setzt `dpiAware=false`. `Skin.Scale` ist hart auf `1.0f` kodiert.
- **LangVersion ist `preview`** — aktuelle C#-Features sind aktiviert.
- **Nullable Reference Types** sind aktiviert (`Nullable=enable`). Warnungen niemals mit `!` unterdrücken.

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

## Wichtige Hilfsklassen (andere Projekte)

| Klasse | Projekt | Zweck |
|--------|---------|-------|
| **`Skin`** | BlueControls | Zentrales Theming: `Draw_Back()`, `Draw_Border()`, `Draw_FormatedText()`. Alle Zeichenoperationen in Controls laufen über Skin. |
| **`BlueFont`** | BlueControls | Schriftverwaltung mit Caching: `Get()`, `GetBrush()`, `GetPen()`, `MeasureString()`. |
| **`FormManager`** | BlueControls | Verwaltet alle offenen Formulare: `Forms`, `CloseAllForms()`, `GetFormByType<T>()`. |
| **`Form`** | BlueControls | **Basisklasse für alle Formulare** (erbt `System.Windows.Forms.Form`, erzwingt `AutoScaleMode.None`). |
| **`FloatingForm`** | BlueControls | Leichtgewichtiges Popup für Tooltips und Benachrichtigungen. |
| **`Table`** | BlueTable | In-Memory-Tabelle: `Get()` (Singleton pro Key), `Save()`, `Load()`, Zellzugriff, Filter, Sortierung. |
| **`CsvHelper`** | BlueTable | CSV-Import/Export: `ExportCSV()`, `ImportCSV()`. |
| **`LanguageTool`** | BlueTable | Übersetzungssystem: `DoTranslate()`, `TranslateEnglish()`, `TranslateGerman()`. |
| **`Script`** | BlueScript | Skript-Ausführung: `Start()`, `Execute()`, `ParseLine()`. |
| **`VariableCollection`** | BlueScript | Skript-Variablen-Container: `Get()`, `Add()`, `Remove()`, `Exists()`. |
| **`ParseableItem`** | BlueBasics | Abstrakte Basisklasse für serialisierbare Items: `NewByParsing<T>()`, `ICloneable`. |
| **`BackupVerwalter`** | BlueBasics | Backup-Rotation (1, 2, 4, 8, …): `Add()`, `DoMaintenance()`. |

## Verhalten bei Verbesserungsvorschlägen

Wenn du während einer Sitzung eine Rückmeldung zum Stil, zu Konventionen oder zu Patterns erhältst (z.B. Code-Formatierung, Namenskonventionen, Architektur-Patterns), dann **nicht sofort übernehmen**. Stattdessen als ERSTES eine Frage mit Auswahlmöglichkeiten stellen, z.B.:

- *„Diesen Stil für die aktuelle Datei übernehmen und nicht merken"*
- *„Diesen Stil projektweit in die AGENTS.md aufnehmen und für diese Datei(en) anwenden"*
- *„Nicht übernehmen"*

## Offene Migrationen

- **`FromNonCritical` / `ToNonCritical` / `TagGet` entfernen** — Ziel: alles über JSON. Wenn bei einer Änderung eine einfache Gelegenheit besteht, diese Formate abzuschaffen, mit umsetzen. Danach den Nutzer fragen: *„Mit Rückwärtskompatibilität (alter Code funktioniert noch)"* oder *„Ohne — alter Code bricht"*.
