# BlueElements — Agent-Anweisungen

## Build & Ausführung

```bash
dotnet build BeCreative.sln              # Alles bauen
dotnet build BeCreative\BeCreative.csproj # Nur die Haupt-App bauen
```

Es gibt keine Testsuite und keine CI/CD-Pipeline. Alle Prüfungen erfolgen manuell.

Die VS Code `launch.json` verweist auf einen veralteten `net48`-Ausgabepfad — die Projekte verwenden jetzt `net8.0-windows`.

## Projektstruktur

7 Projekte in einer Solution. Die Abhängigkeitsreihenfolge ist wichtig:

```
BlueBasics          (Basis, keine Abhängigkeiten)
  ← BlueScript
    ← BlueTable
      ← BlueControls ← BlueInternet
      ← BluePaint    ← BlueInternet
  ← BlueControls
```

`BeCreative` ist der WinExe-Einstiegspunkt und referenziert alle anderen Projekte.

Für isolierte Entwicklung existieren einzelne Library-Solutions (BlueBasics.sln, BlueScript.sln usw.).

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
- **XML-Doc-Kommentare** nicht erforderlich (CS1591 severity: none)
- **Expression-bodied Members** bevorzugt
- **4 Leerzeichen Einrückung**, CRLF, keine Tabs
- **`readonly`-Felder** bevorzugt

Unterdrückte Diagnostics (nicht "reparieren"):
- CA2000 (Dispose), CA1062 (Null-Prüfungen), CA1031 (spezifische Catches), CA1852 (seal) — alle `none`
- IDE0058 (ungenutzter Ausdruckswert) — `none`
- IDE0055 (Formatierung) — `none`
- IDE1006 (Benennungsstil) — `none`

## Leicht übersehbare Konventionen

- **Alle `.cs`-Dateien müssen UTF-8 mit BOM sein.** Das Skript `Convert-Encoding.ps1` normalisiert Kodierung und Stil.
- **Alle WinForms-Formulare müssen von `BlueControls.Forms.Form` erben**, nicht von `System.Windows.Forms.Form`. Das erzwingt `AutoScaleMode.None`.
- **DPI-Awareness ist global deaktiviert.** Niemals `DpiMode`-Setter aufrufen. Das Manifest setzt `dpiAware=false`. `Skin.Scale` ist hart auf `1.0f` kodiert.
- **BlueScript-Methoden** folgen dem Muster `Method_*.cs` (z.B. `Method_MathAdd.cs`). Jede ist eine Klasse, die von der Method-Basis erbt.
- **Code-Analyzers** (SonarAnalyzer, NetAnalyzers) sind in jedem `.csproj` eingebunden. Einige Regeln stehen auf `error` (S1871, S4220, CA1868, CA1012, CS0649).
- **LangVersion ist `preview`** — aktuelle C#-Features sind aktiviert.
- **Nullable Reference Types** sind aktiviert (`Nullable=enable`).
- **Teile der Codebasis und Dokumentation sind auf Deutsch** (z.B. DPI.md, Bezeichner wie `SortierTyp`, `Ressourcen`).