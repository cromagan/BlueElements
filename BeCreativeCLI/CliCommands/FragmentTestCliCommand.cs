// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;
using System.Text.Json.Nodes;
using BlueScript.ScriptVariables;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Fragment-Test: Ändert alle internen Tabellenklassen (Skripte inkl. gespeicherter
/// Variablen, Sortierung, Spaltenanordnungen, Spalten, Unique-Definitionen) über die
/// offiziellen APIs und prüft, dass die JSON-Fragmente nur die tatsächlich geänderten
/// Werte enthalten. Anschließend werden alle Fragmente in eine frische zweite
/// Tabellen-Instanz eingespielt und das Ergebnis verifiziert. Die Originaldatei
/// bleibt unverändert; gearbeitet wird auf Kopien im Temp-Bereich.
/// </summary>
public class FragmentTestCliCommand : CliCommand {

    #region Fields

    private readonly List<string> _failures = [];
    private int _checkCount;
    private List<string> _expectedColumnOrder = [];

    #endregion

    #region Properties

    public override string Command => "fragmenttest";
    public override List<string> Flags => ["dev"];
    public override bool Hidden => true;

    public override string? HelpDetails =>
        "Prüft pro Änderung die geschriebenen Fragment-Zeilen auf minimale Granularität " +
        "(z. B. nur 'eventscript[Key].script' bei einer Skript-Änderung) und spielt alle " +
        "Fragmente anschließend in eine zweite Instanz ein. Exit-Code 0 = alle Prüfungen bestanden. " +
        "Nur JSON-Fragment-Tabellen (.mtblj) werden unterstützt.";

    public override string Syntax => "bcr fragmenttest <tabelle.mtblj>";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        var filename = args[0] ?? string.Empty;

        if (!filename.IsValidFilepathAndName()) {
            try {
                filename = Path.GetFullPath(filename);
            } catch {
                Console.Error.WriteLine("Datei nicht gefunden: " + filename);
                return 1;
            }
        }

        if (!FileExists(filename)) {
            Console.Error.WriteLine("Datei nicht gefunden: " + filename);
            return 1;
        }

        if (!filename.FileSuffix().Equals("mtblj", StringComparison.OrdinalIgnoreCase)) {
            Console.Error.WriteLine("Der Fragment-Test unterstützt nur .mtblj-Tabellen (JSON-Fragmente), erhalten: ." + filename.FileSuffix());
            return 2;
        }

        var tempRoot = Path.Combine(Path.GetTempPath(), "FragmentTest", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture));
        var dirA = Path.Combine(tempRoot, "A");
        var baseName = filename.FileNameWithoutSuffix();

        Out("=== Fragment-Test ===");
        Out("Vorlage       : " + filename);
        Out("Arbeitskopie A: " + dirA);
        Out(string.Empty);

        if (!CreateWorkingCopy(filename, dirA)) { return 1; }

        var fileA = Path.Combine(dirA, baseName + ".mtblj");
        Table? tbl = null;

        try {
            tbl = Table.Get(fileA);

            if (tbl is not { IsDisposed: false }) {
                Console.Error.WriteLine("Arbeitskopie konnte nicht geladen werden: " + fileA);
                return 1;
            }

            tbl.PauseTimer();

            TestScriptText(tbl, dirA);
            TestScriptSavedVariable(tbl, dirA);
            TestSortDefinition(tbl, dirA);
            TestViewQuickInfo(tbl, dirA);
            TestViewColumnOrder(tbl, dirA);
            TestViewColumnPermanent(tbl, dirA);
            TestColumnQuickInfo(tbl, dirA);
            TestWhiteBoxDiffs(tbl);
            TestWhiteBoxTypeChange(tbl);
            TestScriptAddRemove(tbl, dirA);
            TestViewRename(tbl, dirA);
            TestViewColumnAdded(tbl, dirA);
            TestColumnRenamed(tbl, dirA);
            TestScriptVariablesCleared(tbl, dirA);
        } finally {
            tbl?.ResumeTimer();
            tbl?.Dispose();
        }

        if (_failures.Count > 0) {
            Out(string.Empty);
            Out("Abbruch — Replay wird nicht ausgeführt, da bereits Prüfungen fehlgeschlagen sind.");
            PrintSummary();
            return 1;
        }

        Out(string.Empty);
        Out("--- Einspielen in eine zweite Instanz ---");

        return ReplayTest(fileA, dirB: Path.Combine(tempRoot, "B"));
    }

    /// <summary>
    /// Kopiert die Hauptdatei samt Fragment-Ordner in ein neues Verzeichnis.
    /// </summary>
    private static bool CreateWorkingCopy(string sourceFile, string targetDir) {
        if (!CreateDirectory(targetDir).IsSuccessful) {
            Console.Error.WriteLine("Temp-Verzeichnis konnte nicht erstellt werden: " + targetDir);
            return false;
        }

        var targetFile = Path.Combine(targetDir, sourceFile.FileNameWithoutSuffix() + "." + sourceFile.FileSuffix());

        if (!FileCopy(sourceFile, targetFile, false)) {
            Console.Error.WriteLine("Arbeitskopie konnte nicht erstellt werden: " + targetFile);
            return false;
        }

        var sourceFrgm = sourceFile.FilePath() + "FrgmJ";

        if (!DirectoryExists(sourceFrgm)) { return true; }

        var targetFrgm = Path.Combine(targetDir, "FrgmJ");

        if (!CreateDirectory(targetFrgm).IsSuccessful) {
            Console.Error.WriteLine("Fragment-Ordner konnte nicht erstellt werden: " + targetFrgm);
            return false;
        }

        foreach (var f in GetFiles(sourceFrgm, "*." + TableJsonFragments.SuffixOfJsonFragments, SearchOption.TopDirectoryOnly)) {
            if (!FileCopy(f, Path.Combine(targetFrgm, f.FileNameWithoutSuffix() + "." + TableJsonFragments.SuffixOfJsonFragments), false)) {
                Console.Error.WriteLine("Fragment-Datei konnte nicht kopiert werden: " + f);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Liefert die Anzahl der Pfad-Zeilen pro Pfad über alle Fragment-Dateien hinweg.
    /// </summary>
    private static Dictionary<string, int> FragmentPathCounts(string dir) {
        Dictionary<string, int> result = [];
        var frgmDir = Path.Combine(dir, "FrgmJ");

        if (!DirectoryExists(frgmDir)) { return result; }

        foreach (var f in GetFiles(frgmDir, "*." + TableJsonFragments.SuffixOfJsonFragments, SearchOption.TopDirectoryOnly)) {
            foreach (var line in ReadAllText(f, Encoding.UTF8).SplitAndCutByCr()) {
                if (line is not { Length: > 0 } || line[0] != '{') { continue; }

                try {
                    if (JsonNode.Parse(line) is not JsonObject jo) { continue; }
                    if (jo["path"] is not JsonValue pv || !pv.TryGetValue(out string? p) || p is not { Length: > 0 }) { continue; }

                    result[p] = result.TryGetValue(p, out var c) ? c + 1 : 1;
                } catch {
                    // Halbe Zeilen (gleichzeitiges Schreiben) ignorieren.
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Liefert die Kommandos aller UndoItem-Zeilen über alle Fragment-Dateien hinweg.
    /// </summary>
    private static List<int> FragmentCommandCounts(string dir) {
        List<int> result = [];
        var frgmDir = Path.Combine(dir, "FrgmJ");

        if (!DirectoryExists(frgmDir)) { return result; }

        foreach (var f in GetFiles(frgmDir, "*." + TableJsonFragments.SuffixOfJsonFragments, SearchOption.TopDirectoryOnly)) {
            foreach (var line in ReadAllText(f, Encoding.UTF8).SplitAndCutByCr()) {
                if (line is not { Length: > 0 } || line[0] != '{') { continue; }

                try {
                    if (JsonNode.Parse(line) is not JsonObject jo) { continue; }
                    if (jo["_meta"] is not null || jo["path"] is not null) { continue; }

                    var cmd = jo.GetInt("command", -1);
                    if (cmd >= 0) { result.Add(cmd); }
                } catch {
                    // Halbe Zeilen ignorieren.
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Liefert die Pfad-Zeilen, die seit <paramref name="before" /> neu hinzukamen.
    /// </summary>
    private static List<string> NewPaths(Dictionary<string, int> before, string dir) {
        var now = FragmentPathCounts(dir);
        List<string> result = [];

        foreach (var kvp in now) {
            before.TryGetValue(kvp.Key, out var oldCount);

            for (var i = oldCount; i < kvp.Value; i++) {
                result.Add(kvp.Key);
            }
        }

        return result;
    }

    private static void Out(string text) => Console.Out.WriteLine(text);

    private static List<string> PathsOf(List<(string Path, JsonNode? Value)> changes) => [.. changes.Select(c => c.Path)];

    private static string TextOf(IParseable item) => item.ParseableItems().FinishParseable();

    /// <summary>
    /// Erzeugt aus dem EventScript der Tabelle einen veränderten Klon des
    /// benannten Skripts und weist die Liste der Tabellen erneut zu.
    /// </summary>
    private static bool ReplaceScript(Table tbl, string scriptKeyName, Action<TableScriptDescription> change, string context) {
        TableScriptDescription? target = null;
        List<TableScriptDescription> scripts = [];

        foreach (var s in tbl.EventScript) {
            if (string.Equals(s.KeyName, scriptKeyName, StringComparison.OrdinalIgnoreCase)) {
                target = new TableScriptDescription(tbl, s.ParseableItems().FinishParseable());
                scripts.Add(target);
            } else {
                scripts.Add(s);
            }
        }

        if (target is null) {
            Console.Error.WriteLine(context + ": Skript nicht gefunden: " + scriptKeyName);
            return false;
        }

        change(target);
        tbl.EventScript = new(scripts);
        return true;
    }

    private void Check(string name, bool ok, string detail) {
        _checkCount++;

        if (ok) {
            Out("  PASS  " + name);
            return;
        }

        _failures.Add(name + ": " + detail);
        Out("  FAIL  " + name + " — " + detail);
    }

    private void CheckPaths(string name, List<string> paths, List<string> expected) {
        var ok = paths.Count == expected.Count;

        if (ok) {
            foreach (var e in expected) {
                if (!paths.Contains(e)) { ok = false; break; }
            }
        }

        Check(name, ok, "erwartet [" + string.Join("; ", expected) + "], erhalten [" + string.Join("; ", paths) + "]");
    }

    private void PrintSummary() {
        Out(string.Empty);
        Out("--- Ergebnis ---");
        Out("Prüfungen: " + _checkCount + ", fehlgeschlagen: " + _failures.Count);

        foreach (var f in _failures) {
            Out("  FEHLER: " + f);
        }

        Out(_failures.Count == 0 ? "*** ERFOLG: Alle Prüfungen bestanden. ***" : "*** FEHLER: Prüfungen sind fehlgeschlagen. ***");
    }

    #endregion

    #region Einzelprüfungen (Schreibseite)

    /// <summary>
    /// T1: Skripttext ändern — nur die Script-Zeile dieses Skripts.
    /// </summary>
    private void TestScriptText(Table tbl, string dir) {
        Out("T1: Skripttext ändern");
        var before = FragmentPathCounts(dir);

        if (!ReplaceScript(tbl, "CLITest", s => s.Script = s.Script + "\rvar Count = 55;", "T1")) {
            _failures.Add("T1: Vorbereitung fehlgeschlagen.");
            return;
        }

        CheckPaths("T1 Fragment-Zeilen", NewPaths(before, dir), ["eventscript[CLITest].script"]);
    }

    /// <summary>
    /// T2: Kommentar einer gespeicherten Skript-Variable ändern — nur diese eine
    /// Variable-Attribut-Zeile, nicht das ganze savedvariables-Objekt.
    /// </summary>
    private void TestScriptSavedVariable(Table tbl, string dir) {
        Out("T2: Gespeicherte Skript-Variable ändern (Kommentar)");
        var before = FragmentPathCounts(dir);
        var variableFound = false;

        var ok = ReplaceScript(tbl, "Komplette Berechung", s => {
            var v = s.SavedVariables.FirstOrDefault(x => x.KeyName == "ATTRIBUT0");

            if (v is null) { return; }
            v.Comment = "Fragment-Testkommentar";
            variableFound = true;
        }, "T2");

        if (!ok) {
            _failures.Add("T2: Skript 'Komplette Berechung' fehlt.");
            return;
        }

        if (!variableFound) {
            _failures.Add("T2: Variable ATTRIBUT0 fehlt im Skript 'Komplette Berechung'.");
            return;
        }

        CheckPaths("T2 Fragment-Zeilen", NewPaths(before, dir), ["eventscript[Komplette Berechung].savedvariables.variables[ATTRIBUT0].comment"]);
    }

    /// <summary>
    /// T3: Sortierung ändern — nur Sortier-Zeilen.
    /// </summary>
    private void TestSortDefinition(Table tbl, string dir) {
        Out("T3: Sortierung ändern");
        var before = FragmentPathCounts(dir);

        var c1 = tbl.Column["UEBERSCHRIFT"];
        var c2 = tbl.Column["KATEGORIE"];

        if (c1 is null || c2 is null) {
            _failures.Add("T3: Spalten UEBERSCHRIFT/KATEGORIE fehlen.");
            return;
        }

        tbl.SortDefinition = new RowSortDefinition(tbl, [c1, c2], true);

        CheckPaths("T3 Fragment-Zeilen", NewPaths(before, dir), ["sortdefinition[x].columns", "sortdefinition[x].reverse"]);
    }

    /// <summary>
    /// T4: Quickinfo einer Spaltenanordnung ändern — nur diese Ansicht-Eigenschaft.
    /// </summary>
    private void TestViewQuickInfo(Table tbl, string dir) {
        Out("T4: Ansicht-Quickinfo ändern");
        var before = FragmentPathCounts(dir);
        var changed = false;

        var views = CloneViews(tbl, "Standard", v => {
            v.QuickInfo = "Fragment-Test-QI";
            changed = true;
        });

        if (views is null) { return; }

        if (!changed) {
            _failures.Add("T4: Die Änderung wurde nicht ausgeführt.");
            return;
        }

        tbl.ColumnArrangements = new(views);

        CheckPaths("T4 Fragment-Zeilen", NewPaths(before, dir), ["columnarrangements[Standard].quickinfo"]);
    }

    /// <summary>
    /// T5: Spaltenreihenfolge einer Ansicht ändern — nur das Spalten-Array dieser Ansicht.
    /// </summary>
    private void TestViewColumnOrder(Table tbl, string dir) {
        Out("T5: Spaltenreihenfolge einer Ansicht ändern");
        var before = FragmentPathCounts(dir);
        var changed = false;

        var views = CloneViews(tbl, "Standard", v => {
            v.Move(0, v.Count - 1);
            changed = true;
        });

        if (views is null || !changed) { return; }

        tbl.ColumnArrangements = new(views);

        // Merken, welche Reihenfolge das Replay später liefern muss.
        var view = views.First(v => v.KeyName.Equals("Standard", StringComparison.OrdinalIgnoreCase));
        _expectedColumnOrder = [.. view.Select(i => i.ColumnName ?? string.Empty)];

        CheckPaths("T5 Fragment-Zeilen", NewPaths(before, dir), ["columnarrangements[Standard].columns"]);
    }

    /// <summary>
    /// T6: Permanent-Flag einer Ansichtsspalte ändern — nur diese Spalten-Eigenschaft.
    /// </summary>
    private void TestViewColumnPermanent(Table tbl, string dir) {
        Out("T6: Permanent-Flag einer Ansichtsspalte ändern");
        var before = FragmentPathCounts(dir);
        var changed = false;

        var views = CloneViews(tbl, "Standard", v => {
            if (v["KATEGORIE"] is not { } cvi) { return; }
            cvi.Permanent = true;
            changed = true;
        });

        if (views is null) { return; }

        if (!changed) {
            _failures.Add("T6: Spalte KATEGORIE fehlt in der Ansicht 'Standard'.");
            return;
        }

        tbl.ColumnArrangements = new(views);

        CheckPaths("T6 Fragment-Zeilen", NewPaths(before, dir), ["columnarrangements[Standard].columns[KATEGORIE].permanent"]);
    }

    /// <summary>
    /// T7: Quickinfo einer Spalte ändern — als UndoItem-Zeile (Spalten sind bereits
    /// attributweise gespeichert) und ohne Pfad-Zeilen.
    /// </summary>
    private void TestColumnQuickInfo(Table tbl, string dir) {
        Out("T7: Spalten-Quickinfo ändern");
        var before = FragmentPathCounts(dir);
        var beforeCommands = FragmentCommandCounts(dir);

        var column = tbl.Column["UEBERSCHRIFT"];

        if (column is null) {
            _failures.Add("T7: Spalte UEBERSCHRIFT fehlt.");
            return;
        }

        column.QuickInfo = "Fragment-Test-Spalten-QI";

        var newPaths = NewPaths(before, dir);
        Check("T7 Keine Pfad-Zeilen", newPaths.Count == 0, "erhalten [" + string.Join("; ", newPaths) + "]");

        var nowCommands = FragmentCommandCounts(dir);
        var added = nowCommands.Count - beforeCommands.Count;
        Check("T7 Genau eine UndoItem-Zeile", added == 1, "Differenz der Undo-Zeilen: " + added);
    }

    #endregion

    #region Einzelprüfungen (Diff, White-Box)

    /// <summary>
    /// T8-T13: White-Box-Prüfungen des Diffs für gespeicherte Skript-Variablen,
    /// Tabellen-Variablen und Ansichts-Eigenschaften ohne Schreibvorgänge.
    /// </summary>
    private void TestWhiteBoxDiffs(Table tbl) {
        Out("T8-T13: Diff-Prüfungen (White-Box)");

        #region T8: Tabellen-Variable, ein Attribut geändert

        var oldVar = new StringScriptVariable("FRAGMENTTESTVAR", "Wert", false, "Alter Kommentar");
        var newVar = new StringScriptVariable("FRAGMENTTESTVAR", "Wert", false, "Neuer Kommentar");

        var changes = TableJsonFragmentDiff.GetChanges(tbl, TableDataType.TableVariables, TextOf(oldVar), TextOf(newVar));
        CheckPaths("T8 Tabellen-Variable: ein Attribut", PathsOf(changes), ["variables[FRAGMENTTESTVAR].comment"]);

        #endregion

        #region T9: Tabellen-Variable hinzugefügt — komplette Liste

        var extraVar = new StringScriptVariable("FRAGMENTTESTVAR2", "X", false, "Neu");
        changes = TableJsonFragmentDiff.GetChanges(tbl, TableDataType.TableVariables, TextOf(oldVar), TextOf(oldVar) + "\r" + TextOf(extraVar));
        CheckPaths("T9 Tabellen-Variable: hinzugefügt", PathsOf(changes), ["variables"]);

        #endregion

        #region T10: Zwei Skript-Variablen geändert — je eine Attribute-Zeile

        // Hinweis: Der ReadOnly-Flag gespeicherter Variablen wird beim Text-Parsen
        // normiert (ParseVariable erzwingt true) und ist im Blob-Diff dadurch
        // nicht beobachtbar — Kommentare sind das griffige veränderbare Attribut.
        var scriptOld = CloneScript(tbl, "Komplette Berechung");
        var scriptNew = CloneScript(tbl, "Komplette Berechung");

        if (scriptOld is null || scriptNew is null) {
            _failures.Add("T10: Skript 'Komplette Berechung' fehlt.");
        } else {
            var vA = scriptNew.SavedVariables.FirstOrDefault(x => x.KeyName == "ATTRIBUT3");
            var vB = scriptNew.SavedVariables.FirstOrDefault(x => x.KeyName == "ATTRIBUT7");

            if (vA is null || vB is null) {
                _failures.Add("T10: Variablen ATTRIBUT3/ATTRIBUT7 fehlen.");
            } else {
                vA.Comment = "T10-A";
                vB.Comment = "T10-B";
                changes = TableJsonFragmentDiff.GetChanges(tbl, TableDataType.EventScript, TextOf(scriptOld), TextOf(scriptNew));
                CheckPaths("T10 Zwei Skript-Variablen", PathsOf(changes), [
                    "eventscript[Komplette Berechung].savedvariables.variables[ATTRIBUT3].comment",
                    "eventscript[Komplette Berechung].savedvariables.variables[ATTRIBUT7].comment"
                ]);
            }
        }

        #endregion

        #region T11: Skript-Variable hinzugefügt — nur das Variablen-Array

        var scriptAddOld = CloneScript(tbl, "Prüfung");
        var scriptAddNew = CloneScript(tbl, "Prüfung");

        if (scriptAddOld is null || scriptAddNew is null) {
            _failures.Add("T11: Skript 'Prüfung' fehlt.");
        } else {
            scriptAddNew.SavedVariables.ReplaceWith([new StringScriptVariable("FRAGMENTTESTVAR3", "1", true, "Neu")]);
            changes = TableJsonFragmentDiff.GetChanges(tbl, TableDataType.EventScript, TextOf(scriptAddOld), TextOf(scriptAddNew));
            CheckPaths("T11 Skript-Variable: hinzugefügt", PathsOf(changes), ["eventscript[Prüfung].savedvariables.variables"]);
        }

        #endregion

        #region T12: Kapitelspalte einer Ansicht entfernen — Skalar auf leer

        var viewOld = CloneOneView(tbl, "Standard");

        if (viewOld is null) {
            _failures.Add("T12: Ansicht 'Standard' fehlt.");
        } else {
            var oldText = TextOf(viewOld);
            var viewNew = new ColumnViewCollection(tbl, oldText);
            viewNew.ColumnForChapter = null;

            var newText = TextOf(viewNew);

            if (oldText == newText) {
                Out("T12 DEBUG: Kapitel der Ansichten: " + string.Join(", ", tbl.ColumnArrangements.Select(v => v.KeyName + "=" + (v.ColumnForChapter?.KeyName ?? "<null>"))));
                _failures.Add("T12: Das Leeren der Kapitelspalte ändert den Blob nicht. Old: " + oldText);
                return;
            }

            changes = TableJsonFragmentDiff.GetChanges(tbl, TableDataType.ColumnArrangement, oldText, newText);
            CheckPaths("T12 Ansicht: Kapitelspalte geleert", PathsOf(changes), ["columnarrangements[Standard].chaptercolumn"]);
        }

        #endregion

        #region T13: Benutzergruppen eines Skripts entfernen — verschwundener Array-Key

        var grpOld = CloneScript(tbl, "Export");
        var grpNew = CloneScript(tbl, "Export");

        if (grpOld is null || grpNew is null) {
            _failures.Add("T13: Skript 'Export' fehlt.");
        } else {
            var oldText = TextOf(grpOld);
            grpNew.UserGroups = new List<string>().AsReadOnly();

            changes = TableJsonFragmentDiff.GetChanges(tbl, TableDataType.EventScript, oldText, TextOf(grpNew));
            CheckPaths("T13 Skript: Benutzergruppen geleert", PathsOf(changes), ["eventscript[Export].usergroups"]);
        }

        #endregion
    }

    #endregion

    #region Einzelprüfungen (Struktur-Änderungen)

    /// <summary>
    /// T14/T15: Skript hinzufügen und wieder entfernen — beides synchronisiert
    /// die komplette Skript-Liste (Struktur-Änderung).
    /// </summary>
    private void TestScriptAddRemove(Table tbl, string dir) {
        Out("T14: Skript hinzufügen");
        var before = FragmentPathCounts(dir);

        var scripts = new List<TableScriptDescription>();
        scripts.AddRange(tbl.EventScript);
        scripts.Add(new TableScriptDescription(tbl, "FrgmNeu", "SoftMessage(\"FrgmNeu\");"));
        tbl.EventScript = new(scripts);

        CheckPaths("T14 Fragment-Zeilen", NewPaths(before, dir), ["eventscript"]);

        Out("T15: Skript entfernen");
        before = FragmentPathCounts(dir);

        var rest = new List<TableScriptDescription>();
        rest.AddRange(tbl.EventScript.Where(s => !s.KeyName.Equals("FrgmNeu", StringComparison.OrdinalIgnoreCase)));
        tbl.EventScript = new(rest);

        CheckPaths("T15 Fragment-Zeilen", NewPaths(before, dir), ["eventscript"]);
    }

    /// <summary>
    /// T16: Ansicht umbenennen — die Folgelines laufen index-basiert, hier
    /// ändert sich nur der Name.
    /// </summary>
    private void TestViewRename(Table tbl, string dir) {
        Out("T16: Ansicht umbenennen");
        var before = FragmentPathCounts(dir);

        ColumnViewCollection? target = null;
        List<ColumnViewCollection> views = [];

        foreach (var v in tbl.ColumnArrangements) {
            var c = new ColumnViewCollection(tbl, v.ParseableItems().FinishParseable());

            if (c.KeyName.Equals("Export", StringComparison.OrdinalIgnoreCase)) {
                c.KeyName = "FrgmExport";
                target = c;
            }

            views.Add(c);
        }

        if (target is null) {
            _failures.Add("T16: Ansicht 'Export' fehlt.");
            return;
        }

        tbl.ColumnArrangements = new(views);

        CheckPaths("T16 Fragment-Zeilen", NewPaths(before, dir), ["columnarrangements[2].name"]);
    }

    /// <summary>
    /// T17: Spalte zur Ansicht hinzufügen — Spalten-Array der Ansicht.
    /// </summary>
    private void TestViewColumnAdded(Table tbl, string dir) {
        Out("T17: Spalte zur Ansicht hinzufügen");
        var before = FragmentPathCounts(dir);

        var added = false;
        ColumnViewCollection? standard = null;
        List<ColumnViewCollection> views = [];

        foreach (var v in tbl.ColumnArrangements) {
            var c = new ColumnViewCollection(tbl, v.ParseableItems().FinishParseable());

            if (c.KeyName.Equals("Standard", StringComparison.OrdinalIgnoreCase)) {
                var column = tbl.Column["SYS_CORRECT"];

                if (column is null) {
                    _failures.Add("T17: Spalte SYS_CORRECT fehlt.");
                    return;
                }

                c.Add(new ColumnViewItem(column));
                standard = c;
                added = true;
            }

            views.Add(c);
        }

        if (!added || standard is null) {
            _failures.Add("T17: Ansicht 'Standard' fehlt.");
            return;
        }

        _expectedColumnOrder = [.. standard.Select(i => i.ColumnName ?? string.Empty)];
        tbl.ColumnArrangements = new(views);

        CheckPaths("T17 Fragment-Zeilen", NewPaths(before, dir), ["columnarrangements[Standard].columns"]);
    }

    /// <summary>
    /// T20: Spalte umbenennen — als ColumnKey-UndoItem plus eine
    /// Spalten-Array-Zeile je Ansicht, die die Spalte enthält.
    /// </summary>
    private void TestColumnRenamed(Table tbl, string dir) {
        Out("T20: Spalte umbenennen");
        var before = FragmentPathCounts(dir);

        var column = tbl.Column["BEISPIEL"];

        if (column is null) {
            _failures.Add("T20: Spalte BEISPIEL fehlt.");
            return;
        }

        column.KeyName = "FRGM_BEISPIEL";

        var newPaths = NewPaths(before, dir);
        List<string> expected = [
            "columnarrangements[Alle Spalten].columns",
            "columnarrangements[Standard].columns",
            "columnarrangements[FrgmExport].columns"
        ];
        CheckPaths("T20 Fragment-Zeilen", newPaths, expected);

        // Die Replay-Prüfungen R5/R10 vergleichen gegen die erwartete
        // Reihenfolge — nach der Umbenennung unter dem neuen Namen.
        _expectedColumnOrder = [.. _expectedColumnOrder.Select(n => n.Equals("BEISPIEL", StringComparison.OrdinalIgnoreCase) ? "FRGM_BEISPIEL" : n)];
    }

    /// <summary>
    /// T18: Alle gespeicherten Variablen eines Skripts entfernen — das
    /// Variablen-Array wird explizit leer gesendet. Das Temp-Skript wird
    /// zuvor mit einer Variablen angelegt; das Skript "Komplette Berechung"
    /// bleibt unangetastet, damit die Replay-Prüfung R2 den Stand nach T2 prüft.
    /// </summary>
    private void TestScriptVariablesCleared(Table tbl, string dir) {
        Out("T18: Gespeicherte Variablen eines Skripts leeren");

        var scripts = new List<TableScriptDescription>();
        scripts.AddRange(tbl.EventScript);
        var temp = new TableScriptDescription(tbl, "FrgmVars", "SoftMessage(\"FrgmVars\");");
        temp.SavedVariables.ReplaceWith([new StringScriptVariable("FRGMVAR1", "X", true, "Alter Kommentar")]);
        scripts.Add(temp);
        tbl.EventScript = new(scripts);

        var before = FragmentPathCounts(dir);

        if (!ReplaceScript(tbl, "FrgmVars", s => s.SavedVariables.ReplaceWith([]), "T18")) {
            _failures.Add("T18: Temp-Skript 'FrgmVars' fehlt.");
            return;
        }

        CheckPaths("T18 Fragment-Zeilen", NewPaths(before, dir), ["eventscript[FrgmVars].savedvariables.variables"]);
    }

    /// <summary>
    /// T19: Tabellen-Variable mit Typwechsel — nicht prop-weise ausdrückbar,
    /// deshalb komplette Listen-Sync.
    /// </summary>
    private void TestWhiteBoxTypeChange(Table tbl) {
        Out("T19: Tabellen-Variable: Typwechsel");
        var oldVar = new StringScriptVariable("FRAGMENTTESTVAR", "42", false, "c");
        var newVar = new DoubleScriptVariable("FRAGMENTTESTVAR", 42.5, false, "c");

        var changes = TableJsonFragmentDiff.GetChanges(tbl, TableDataType.TableVariables, TextOf(oldVar), TextOf(newVar));
        CheckPaths("T19 Tabellen-Variable: Typwechsel", PathsOf(changes), ["variables"]);
    }

    #endregion

    #region Replay-Prüfung

    /// <summary>
    /// Verifiziert in der zweiten Instanz, dass alle granularen Änderungen
    /// korrekt eingespielt wurden.
    /// </summary>
    private int ReplayTest(string fileA, string dirB) {
        var baseName = fileA.FileNameWithoutSuffix();
        var fileB = Path.Combine(dirB, baseName + "." + fileA.FileSuffix());

        if (!CreateWorkingCopy(fileA, dirB)) {
            PrintSummary();
            return 1;
        }

        // Der Replay überspringt Fragment-Zeilen, die sicher vor der letzten
        // Hauptdatei-Speicherung liegen (60 s Toleranz). Die Kopie trägt das
        // aktuelle Datum — der Zeitstempel wird deshalb auf einen Stand vor
        // allen Fragmenten zurückgesetzt (realistisches Szenario: Hauptdatei
        // alt, Fragmente neu).
        File.SetLastWriteTimeUtc(fileB, new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        Table? tbl = null;

        try {
            tbl = Table.Get(fileB);

            if (tbl is not { IsDisposed: false }) {
                _failures.Add("Zweite Instanz konnte nicht geladen werden: " + fileB);
                PrintSummary();
                return 1;
            }

            tbl.PauseTimer();
            VerifyReplay(tbl);
        } finally {
            tbl?.ResumeTimer();
            tbl?.Dispose();
        }

        PrintSummary();

        if (_failures.Count == 0) {
            Out(string.Empty);
            Out("Temp-Dateien (bleiben zur Analyse liegen): " + Directory.GetParent(dirB)?.FullName);
        }

        return _failures.Count == 0 ? 0 : 1;
    }

    /// <summary>
    /// R-Prüfungen: Spiel alle Änderungen der zweiten Instanz nach.
    /// </summary>
    private void VerifyReplay(Table tbl) {
        Out("Replay-Prüfungen");

        Check("R0 Tabelle nicht eingefroren", !tbl.IsFreezed, tbl.FreezedReason);
        Check("R0 Keine Skript-Fehler", tbl.CheckScriptError().Length == 0, tbl.CheckScriptError());

        var cliTest = tbl.EventScript.FirstOrDefault(s => s.KeyName.Equals("CLITest", StringComparison.OrdinalIgnoreCase));
        Check("R1 Skripttext eingespielt", cliTest?.Script.Contains("var Count = 55;") == true, "Skript: " + cliTest?.Script);

        var big = tbl.EventScript.FirstOrDefault(s => s.KeyName.Equals("Komplette Berechung", StringComparison.OrdinalIgnoreCase));
        var v0 = big?.SavedVariables.FirstOrDefault(x => x.KeyName == "ATTRIBUT0");
        Check("R2 Skript-Variable eingespielt", v0?.Comment == "Fragment-Testkommentar", "Kommentar: " + (v0?.Comment ?? "<null>"));

        var vCount = big?.SavedVariables.Count ?? 0;
        Check("R2 Alle Skript-Variablen erhalten", vCount >= 25, "Anzahl Variablen: " + vCount);

        var sd = tbl.SortDefinition;
        var sdOk = sd is { Reverse: true }
            && sd.SortColumns.Count == 2
            && sd.SortColumns[0].ColumnName == "UEBERSCHRIFT"
            && sd.SortColumns[1].ColumnName == "KATEGORIE";
        Check("R3 Sortierung eingespielt", sdOk, "Reverse=" + sd?.Reverse + ", Spalten=" + string.Join(",", sd?.SortColumns.Select(c => c.ColumnName) ?? []));

        var view = tbl.ColumnArrangements.FirstOrDefault(v => v.KeyName.Equals("Standard", StringComparison.OrdinalIgnoreCase));

        if (view is null) {
            _failures.Add("R4: Ansicht 'Standard' fehlt.");
            Out("  FAIL  R4 Ansicht 'Standard' — fehlt");
            _checkCount++;
            return;
        }

        Check("R4 Ansicht-Quickinfo eingespielt", view.QuickInfo == "Fragment-Test-QI", "Quickinfo: " + view.QuickInfo);

        var order = string.Join(",", view.Select(i => i.ColumnName ?? string.Empty));
        Check("R5 Spaltenreihenfolge eingespielt", view.Select(i => i.ColumnName ?? string.Empty).SequenceEqual(_expectedColumnOrder), "erwartet [" + string.Join(",", _expectedColumnOrder) + "], erhalten [" + order + "]");

        Check("R6 Permanent eingespielt", view["KATEGORIE"]?.Permanent == true, "KATEGORIE Permanent=" + view["KATEGORIE"]?.Permanent);
        Check("R7 Spalten-Quickinfo eingespielt", tbl.Column["UEBERSCHRIFT"]?.QuickInfo == "Fragment-Test-Spalten-QI", "Quickinfo: " + tbl.Column["UEBERSCHRIFT"]?.QuickInfo);

        // Partial-Zeilen (z. B. "…columns" der realen Fragmente) dürfen die
        // Kapitelspalte der Ansicht nicht leeren.
        Check("R13 Kapitelspalte nach Partial-Replay erhalten", view.ColumnForChapter?.KeyName == "KATEGORIE", "Kapitelspalte: " + (view.ColumnForChapter?.KeyName ?? "<null>"));

        #region T14/T15: Skript angelegt und wieder entfernt

        Check("R8 Temporäres Skript entfernt", tbl.EventScript.FirstOrDefault(s => s.KeyName.Equals("FrgmNeu", StringComparison.OrdinalIgnoreCase)) is null, "Skript 'FrgmNeu' noch vorhanden");

        #endregion

        #region T16: Ansicht umbenannt

        Check("R9 Ansicht umbenannt", tbl.ColumnArrangements.FirstOrDefault(v => v.KeyName.Equals("FrgmExport", StringComparison.OrdinalIgnoreCase)) is not null
            && tbl.ColumnArrangements.FirstOrDefault(v => v.KeyName.Equals("Export", StringComparison.OrdinalIgnoreCase)) is null, "Ansicht 'FrgmExport' fehlt oder 'Export' noch vorhanden");

        #endregion

        #region T17: Spalte zur Ansicht hinzugefügt

        var stdOrder = string.Join(",", view.Select(i => i.ColumnName ?? string.Empty));
        Check("R10 Spalte der Ansicht hinzugefügt", view.Select(i => i.ColumnName ?? string.Empty).SequenceEqual(_expectedColumnOrder), "erwartet [" + string.Join(",", _expectedColumnOrder) + "], erhalten [" + stdOrder + "]");

        #endregion

        #region T20: Spalte umbenannt

        Check("R14 Spalte umbenannt", tbl.Column["FRGM_BEISPIEL"] is { IsDisposed: false } && tbl.Column["BEISPIEL"] is null
            && view["FRGM_BEISPIEL"] is not null, "Umbenennung nicht korrekt eingespielt");

        #endregion

        #region T18: Gespeicherte Variablen geleert

        var varsScript = tbl.EventScript.FirstOrDefault(s => s.KeyName.Equals("FrgmVars", StringComparison.OrdinalIgnoreCase));
        Check("R11 Gespeicherte Variablen geleert", varsScript is not null && varsScript.SavedVariables is not { Count: > 0 }, "Skript vorhanden: " + (varsScript is not null) + ", Anzahl Variablen: " + (varsScript?.SavedVariables.Count ?? 0));

        #endregion

        #region Doppeltes Nachladen darf nichts verändern

        if (!tbl.BeSureToBeUpToDate(false)) {
            _failures.Add("R12: Zweites Nachladen fehlgeschlagen.");
            Out("  FAIL  R12 Zweites Nachladen");
            _checkCount++;
            return;
        }

        var cliTestAfterReload = tbl.EventScript.FirstOrDefault(s => s.KeyName.Equals("CLITest", StringComparison.OrdinalIgnoreCase));
        Check("R12 Zweites Nachladen unverändert", !tbl.IsFreezed && cliTestAfterReload?.Script.Contains("var Count = 55;") == true, "Freeze: " + tbl.FreezedReason);

        #endregion
    }

    #endregion

    #region Helper

    private static TableScriptDescription? CloneScript(Table tbl, string keyName) {
        var s = tbl.EventScript.FirstOrDefault(x => x.KeyName.Equals(keyName, StringComparison.OrdinalIgnoreCase));
        return s is null ? null : new TableScriptDescription(tbl, s.ParseableItems().FinishParseable());
    }

    /// <summary>
    /// Klont alle Ansichten der Tabelle und lässt eine Änderung auf der
    /// benannten Ansicht ausführen.
    /// </summary>
    private List<ColumnViewCollection>? CloneViews(Table tbl, string viewKeyName, Action<ColumnViewCollection> change) {
        ColumnViewCollection? target = null;
        List<ColumnViewCollection> views = [];

        foreach (var v in tbl.ColumnArrangements) {
            var c = new ColumnViewCollection(tbl, v.ParseableItems().FinishParseable());

            if (c.KeyName.Equals(viewKeyName, StringComparison.OrdinalIgnoreCase)) { target = c; }

            views.Add(c);
        }

        if (target is null) {
            _failures.Add("Ansicht nicht gefunden: " + viewKeyName);
            return null;
        }

        change(target);
        return views;
    }

    private static ColumnViewCollection? CloneOneView(Table tbl, string viewKeyName) {
        var v = tbl.ColumnArrangements.FirstOrDefault(x => x.KeyName.Equals(viewKeyName, StringComparison.OrdinalIgnoreCase));
        return v is null ? null : new ColumnViewCollection(tbl, v.ParseableItems().FinishParseable());
    }

    #endregion
}

