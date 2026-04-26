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
- **XML-Doc-Kommentare** nicht erforderlich
- **Expression-bodied Members** bevorzugt
- **4 Leerzeichen Einrückung**, CRLF, keine Tabs
- **`readonly`-Felder** bevorzugt
- **Optionale Parameter** NICHT erwünscht!

## Selbstständige Reparatur

Wird eine Datei verändert, zusätzlich folgende Reparaturen durchführen
- **XML-Doc-Kommentare** Veraltet, unklar oder fehlerhaft, reparieren
- **In Properties** bevorzugt direkt auf das backing field zugreifen statt `this.PropertyName`
- **`AsSpan()` / Range-Syntax `[x..y]`** statt `Substring`
- **Überflüssige String-Checks vermeiden** — Muster wie `var f =... ;  If (string.IsNullOrEmpty(f)) {return f;}` unwandeln in `if (... is { Length: > 0 } f) {return f;}`. Ziel: Einzeiliger kürzerer Code
- **Nullable Prüfungen** fehlende Nullable Prüfungen hinzufügen. Niemals mit ! unterdrücken - zur Not redundante Prüfung ausführen
- **unbenutzte Felder, Routinen, etc.** Komentar TODO: Unused hinzufügen, außer es ist bereits ein Kommentare dabei mit dem Text: Used

## Leicht übersehbare Konventionen

- **Bestehende Kommentare erhalten** — Kommentare nicht löschen, sondern bei Bedarf überarbeiten/aktualisieren.
- **Kein redundanter Code** — vor dem Schreiben einer neuen Routine prüfen, ob eine bestehende genutzt oder leicht angepasst werden kann
- **Alle `.cs`-Dateien müssen UTF-8 mit BOM sein.** Das Skript `Convert-Encoding.ps1` normalisiert Kodierung und Stil.
- **Alle WinForms-Formulare müssen von `BlueControls.Forms.Form` erben**, nicht von `System.Windows.Forms.Form`. Das erzwingt `AutoScaleMode.None`.
- **DPI-Awareness ist global deaktiviert.** Niemals `DpiMode`-Setter aufrufen. Das Manifest setzt `dpiAware=false`. `Skin.Scale` ist hart auf `1.0f` kodiert.
- **Code-Analyzers** (SonarAnalyzer, NetAnalyzers) sind in jedem `.csproj` eingebunden. Einige Regeln stehen auf `error` (S1871, S4220, CA1868, CA1012, CS0649).
- **LangVersion ist `preview`** — aktuelle C#-Features sind aktiviert.
- **Nullable Reference Types** sind aktiviert (`Nullable=enable`).
- **Nullable Prüfungen** Warnungen niemals mit ! unterdrücken

## Verhalten bei Verbesserungsvorschlägen

Wenn du während einer Sitzung eine Rückmeldung zum Stil, zu Konventionen oder zu Patterns erhältst (z.B. Code-Formatierung, Namenskonventionen, Architektur-Patterns), dann **nicht sofort übernehmen**. Stattdessen als ERSTES eine Frage mit Auswahlmöglichkeiten stellen, z.B.:

- *„Diesen Stil für die aktuelle Datei übernehmen und nicht merken"*
- *„Diesen Stil projektweit in die AGENTS.md aufnehmen und für diese Datei(en) anwenden"*
- *„Nicht übernehmen"*

## Offene Migrationen

- **`FromNonCritical` / `ToNonCritical` / `TagGet` entfernen** — Ziel: alles über JSON. Wenn bei einer Änderung eine einfache Gelegenheit besteht, diese Formate abzuschaffen, mit umsetzen. Danach den Nutzer fragen: *„Mit Rückwärtskompatibilität (alter Code funktioniert noch)"* oder *„Ohne — alter Code bricht"*.