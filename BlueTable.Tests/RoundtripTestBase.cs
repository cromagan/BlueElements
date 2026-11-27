// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;

namespace BlueTable.Tests;

/// <summary>
/// Gemeinsamer Ablauf aller Layout-Tests: anlegen und befüllen, initial speichern (die Reparatur wird damit
/// einmalig in die Datei übernommen), Roundtrip mit Byte-Vergleich (identisch bis auf das Zip-Eintrag-Datum),
/// Änderungsphasen (Skript, Spalten, Zeilen, Werte) mit Speichern und Nachladen, Abschluss mit Stabilitäts-Roundtrip.
/// Jede Abweichung wird mit Datei, Datenbereich und Byte-Position gemeldet.
/// </summary>
public abstract class RoundtripTestBase {

    #region Properties

    /// <summary>
    /// Anzeigename des Layouts für Ordner und Meldungen.
    /// </summary>
    protected abstract string LayoutName { get; }

    /// <summary>
    /// Datei-Suffix der Hauptdatei.
    /// </summary>
    protected abstract string Suffix { get; }

    /// <summary>
    /// True: Das Befüllen legt eine Chunk-Spalte an und verteilt die Zeilen auf Chunk-Werte.
    /// </summary>
    protected abstract bool WithChunkColumn { get; }

    /// <summary>
    /// Suffixe der write-once-Datenbereiche; ohne Angabe wird nur die Hauptdatei verglichen.
    /// </summary>
    protected abstract string[] AreaFileSuffixes { get; }

    /// <summary>
    /// Datei-Suffixe, die der Vergleich ignoriert (Sitzungs-Journale).
    /// </summary>
    protected abstract string[] IgnoredFileSuffixes { get; }

    /// <summary>
    /// Ordner-Marker, deren Inhalt der Vergleich ignoriert (z. B. Master-Verwaltung).
    /// </summary>
    protected abstract string[] IgnoredFolderMarkers { get; }

    /// <summary>
    /// True: Fragment-Kopf-Sätze (Erzeuger-/Speicher-Stempel) werden vor dem Vergleich abgetrennt.
    /// </summary>
    protected abstract bool StripFragmentHead { get; }

    /// <summary>
    /// Anzahl der Zeilen der Test-Tabelle.
    /// </summary>
    protected virtual int RowCount => 6;

    /// <summary>
    /// True: Der erste Roundtrip darf Laden-Reparaturen enthalten und wird nicht byte-geglichen;
    /// gefordert ist nur die Byte-Gleichheit der Folge-Speicherungen untereinander.
    /// </summary>
    protected virtual bool FirstRoundtripMayRepair => false;

    #endregion

    #region Methods

    /// <summary>
    /// Der Gesamt-Ablauf eines Layouts: Anlegen, Initial-Speicherung, Roundtrip, Änderungen, Abschluss-Roundtrip.
    /// </summary>
    [TestMethod]
    public void Roundtrip_Und_Aenderungen() {
        var dir = TestDirs.NewLayoutFolder(LayoutName);
        var baseName = "rt" + Suffix + TestSetup.RunId;
        var mainFile = Path.Combine(dir, baseName + "." + Suffix);
        var mainFileRel = (baseName + "." + Suffix).ToLowerInvariant();

        var error = CreateFilled(mainFile);

        if (error is { Length: > 0 }) { Assert.Fail(LayoutName + " / Anlegen+Befüllen: " + error + "\r\nOrdner behalten: " + dir); }

        error = InitialRepairSave(mainFile);

        if (error is { Length: > 0 }) { Assert.Fail(LayoutName + " / Initial-Speicherung: " + error + "\r\nOrdner behalten: " + dir); }

        List<string> problems = [];
        var expected = new ExpectedContent(RowCount, WithChunkColumn);

        RunRoundtrip(mainFile, mainFileRel, expected, problems);
        FailPhase(dir, "Roundtrip (Laden/Speichern mit Byte-Vergleich)", problems);

        problems.Clear();
        RunChanges(mainFile, expected, problems);
        FailPhase(dir, "Änderungen (Skript, Spalten, Zeilen, Werte)", problems);

        problems.Clear();
        RunRoundtrip(mainFile, mainFileRel, expected, problems);
        FailPhase(dir, "Abschluss-Roundtrip nach den Änderungen", problems);

        if (!TestDirs.KeepRequested()) { TestDirs.TryDeleteFolder(dir); }
    }

    /// <summary>
    /// Legt die Tabelle an, befüllt sie über die globale Routine und speichert.
    /// </summary>
    private string CreateFilled(string mainFile) {
        var opr = Table.Create(mainFile, null);

        if (opr.IsFailed) { return "Anlegen fehlgeschlagen: " + opr.FailedReason; }

        List<string> problems = [];
        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return JoinProblems(problems, "Tabelle nach dem Anlegen nicht benutzbar"); }

        var error = TableFactory.Fill(tbl, RowCount, WithChunkColumn);

        if (error is { Length: > 0 }) {
            tbl.Dispose();
            return error;
        }

        error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();
        return error;
    }

    /// <summary>
    /// Lädt die Datei und speichert sofort: Die beim Laden gelaufene Reparatur wird damit einmalig in die Datei übernommen.
    /// </summary>
    private static string InitialRepairSave(string mainFile) {
        List<string> problems = [];
        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return JoinProblems(problems, "Laden fehlgeschlagen"); }

        TableAccess.ForceSaveRequired(tbl);

        var error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();

        return error is { Length: > 0 } ? error : JoinProblems(problems, string.Empty);
    }

    /// <summary>
    /// Roundtrip: Laden, Speichern erzwingen, Datei-Bestände vergleichen; danach erneut laden, Inhalt prüfen
    /// und ein zweites Mal speichern — auch dieser Stand muss byte-identisch bleiben (bis auf das Zip-Datum).
    /// </summary>
    private void RunRoundtrip(string mainFile, string mainFileRel, ExpectedContent expected, List<string> problems) {
        var dir = Path.GetDirectoryName(mainFile) ?? string.Empty;

        var before = FileInventory.Of(dir);

        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        TableAccess.ForceSaveRequired(tbl);

        var error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();

        if (error is { Length: > 0 }) {
            problems.Add("Erste Speicherung: " + error);
            return;
        }

        var after = FileInventory.Of(dir);

        if (!FirstRoundtripMayRepair) {
            FileInventory.CompareRoundtrip(LayoutName + " / erster Roundtrip", mainFileRel, before, after, AreaFileSuffixes, IgnoredFileSuffixes, IgnoredFolderMarkers, StripFragmentHead, problems);
        }

        tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        problems.AddRange(expected.Check(tbl));

        TableAccess.ForceSaveRequired(tbl);

        error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();

        if (error is { Length: > 0 }) {
            problems.Add("Zweite Speicherung: " + error);
            return;
        }

        var final = FileInventory.Of(dir);
        FileInventory.CompareRoundtrip(LayoutName + " / zweiter Roundtrip", mainFileRel, after, final, AreaFileSuffixes, IgnoredFileSuffixes, IgnoredFolderMarkers, StripFragmentHead, problems);
    }

    /// <summary>
    /// Führt alle Änderungsphasen aus; jede Phase speichert, lädt nach und prüft die übernommenen Änderungen.
    /// </summary>
    private void RunChanges(string mainFile, ExpectedContent expected, List<string> problems) {
        AddScript(mainFile, problems);
        ModifyScript(mainFile, problems);
        AddColumn(mainFile, problems);
        AddRow(mainFile, expected, problems);
        ChangeValue(mainFile, expected, problems);
        DeleteRow(mainFile, expected, problems);
        DeleteColumn(mainFile, expected, problems);
    }

    /// <summary>
    /// Phase: Tabellen-Skript anlegen, speichern, nachladen und prüfen.
    /// </summary>
    private void AddScript(string mainFile, List<string> problems) {
        var scriptName = SkriptName;
        var scriptText = "DebugPrint(\"Roundtrip " + LayoutName + "\");";

        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        var error = TableFactory.AddScript(tbl, scriptName, scriptText);

        if (error is { Length: > 0 }) {
            problems.Add("Skript anlegen: " + error);
            tbl.Dispose();
            return;
        }

        error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();

        if (error is { Length: > 0 }) {
            problems.Add("Skript anlegen / Speichern: " + error);
            return;
        }

        VerifyScript(mainFile, scriptName, scriptText, "Skript anlegen", problems);
    }

    /// <summary>
    /// Phase: Skript-Inhalt ändern, speichern, nachladen und prüfen.
    /// </summary>
    private void ModifyScript(string mainFile, List<string> problems) {
        var scriptName = SkriptName;
        var newText = "DebugPrint(\"Roundtrip geändert " + LayoutName + "\");";

        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        var script = FindScript(tbl, scriptName);

        if (script is null) {
            problems.Add("Skript ändern: Skript nicht gefunden: " + scriptName);
            tbl.Dispose();
            return;
        }

        if (!Table.UpdateScript(script, scriptContent: newText)) {
            problems.Add("Skript ändern: UpdateScript verweigert");
            tbl.Dispose();
            return;
        }

        var error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();

        if (error is { Length: > 0 }) {
            problems.Add("Skript ändern / Speichern: " + error);
            return;
        }

        VerifyScript(mainFile, scriptName, newText, "Skript ändern", problems);
    }

    /// <summary>
    /// Phase: Spalte 'zusatz' anlegen, Werte setzen, speichern, nachladen und prüfen.
    /// </summary>
    private void AddColumn(string mainFile, List<string> problems) {
        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        var neueSpalte = tbl.Column.GenerateAndAdd("zusatz", "Zusatz", null, string.Empty);
        var c1 = tbl.Column["name"];

        if (neueSpalte is not { IsDisposed: false } spalte || c1 is not { IsDisposed: false } nameSpalte) {
            problems.Add("Spalte anlegen: Spalte 'zusatz' oder 'name' konnte nicht angelegt bzw. gefunden werden.");
            tbl.Dispose();
            return;
        }

        var e1 = SetCell(tbl, nameSpalte, "wert2", ChunkOf(2), spalte, "z2");
        var e2 = SetCell(tbl, nameSpalte, "wert5", ChunkOf(5), spalte, "z5");

        if (e1 is { Length: > 0 } || e2 is { Length: > 0 }) {
            problems.Add("Spalte anlegen / Zellen setzen: " + e1 + " " + e2);
            tbl.Dispose();
            return;
        }

        var error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();

        if (error is { Length: > 0 }) {
            problems.Add("Spalte anlegen / Speichern: " + error);
            return;
        }

        tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        var spalteNeu = tbl.Column["zusatz"];
        var c1Neu = tbl.Column["name"];

        if (spalteNeu is not { IsDisposed: false } zusatz || c1Neu is not { IsDisposed: false } nameNeu) {
            problems.Add("Spalte anlegen: Spalte 'zusatz' oder 'name' nach dem Laden nicht gefunden.");
            tbl.Dispose();
            return;
        }

        var row2 = TableFactory.FindRow(tbl, nameNeu, "wert2", ChunkOf(2));
        var row5 = TableFactory.FindRow(tbl, nameNeu, "wert5", ChunkOf(5));
        var row3 = TableFactory.FindRow(tbl, nameNeu, "wert3", ChunkOf(3));

        if (row2 is not { IsDisposed: false } || row5 is not { IsDisposed: false } || row3 is not { IsDisposed: false }) {
            problems.Add("Spalte anlegen: Zeilen nach dem Laden nicht vollständig gefunden.");
            tbl.Dispose();
            return;
        }

        TableFactory.CompareCell(row2, zusatz, "z2", "zusatz", "wert2", problems);
        TableFactory.CompareCell(row5, zusatz, "z5", "zusatz", "wert5", problems);

        var leer = row3.CellGetStringCore(zusatz);

        if (leer is { Length: > 0 }) { problems.Add("Spalte anlegen: wert3/zusatz soll leer sein, ist '" + leer + "'"); }

        tbl.Dispose();
    }

    /// <summary>
    /// Phase: Zeile anlegen (mit Chunk-Filter, wenn eine Chunk-Spalte existiert), speichern, nachladen und prüfen.
    /// </summary>
    private void AddRow(string mainFile, ExpectedContent expected, List<string> problems) {
        const string newName = "wertneu";
        const string newMenge = "33";
        const string newNotiz = "ÄÖÜ ergänzt ✓";
        const string newChunk = "cb2";

        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        var c1 = tbl.Column["name"];
        var c2 = tbl.Column["menge"];
        var c3 = tbl.Column["notiz"];
        var cc = tbl.Column["chunk"];

        if (c1 is not { IsDisposed: false } nameSpalte || c2 is not { IsDisposed: false } mengeSpalte || c3 is not { IsDisposed: false } notizSpalte) {
            problems.Add("Zeile anlegen: Basis-Spalten fehlen.");
            tbl.Dispose();
            return;
        }

        List<FilterItem> filters = [new(nameSpalte, FilterType.Istgleich, nameSpalte.AutoCorrect(newName, true))];

        if (WithChunkColumn) {
            if (cc is not { IsDisposed: false } chunkSpalte) {
                problems.Add("Zeile anlegen: Chunk-Spalte fehlt.");
                tbl.Dispose();
                return;
            }

            filters.Add(new FilterItem(chunkSpalte, FilterType.Istgleich, newChunk));
        }

        var opr = tbl.Row.GenerateAndAdd([.. filters], "bluetable.tests Zeile anlegen");

        if (opr.IsFailed || opr.Value is not RowItem row) {
            problems.Add("Zeile anlegen: " + opr.FailedReason);
            tbl.Dispose();
            return;
        }

        var e1 = row.CellSet(mengeSpalte, newMenge, "bluetable.tests Zeile anlegen");
        var e2 = row.CellSet(notizSpalte, newNotiz, "bluetable.tests Zeile anlegen");

        if (e1 is { Length: > 0 } || e2 is { Length: > 0 }) {
            problems.Add("Zeile anlegen / Zellen setzen: " + e1 + " " + e2);
            tbl.Dispose();
            return;
        }

        RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);

        var error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();

        if (error is { Length: > 0 }) {
            problems.Add("Zeile anlegen / Speichern: " + error);
            return;
        }

        expected.AddRow(newName, newMenge, newNotiz, newChunk);

        tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        problems.AddRange(expected.Check(tbl));
        tbl.Dispose();
    }

    /// <summary>
    /// Phase: Zellwert ändern (menge von wert2), speichern, nachladen und prüfen; Nachbarwerte müssen unverändert bleiben.
    /// </summary>
    private void ChangeValue(string mainFile, ExpectedContent expected, List<string> problems) {
        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        var c1 = tbl.Column["name"];
        var c2 = tbl.Column["menge"];

        if (c1 is not { IsDisposed: false } nameSpalte || c2 is not { IsDisposed: false } mengeSpalte) {
            problems.Add("Wert ändern: Spalten 'name' oder 'menge' fehlen.");
            tbl.Dispose();
            return;
        }

        var error = SetCell(tbl, nameSpalte, "wert2", ChunkOf(2), mengeSpalte, "4711");

        if (error is { Length: > 0 }) {
            problems.Add("Wert ändern: " + error);
            tbl.Dispose();
            return;
        }

        RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);

        error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();

        if (error is { Length: > 0 }) {
            problems.Add("Wert ändern / Speichern: " + error);
            return;
        }

        expected.ChangeValue("wert2", "menge", "4711");

        tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        problems.AddRange(expected.Check(tbl));
        tbl.Dispose();
    }

    /// <summary>
    /// Phase: Zeile löschen (wert1; der Chunk cb1 bleibt durch wert4 belegt), speichern, nachladen und prüfen.
    /// </summary>
    private void DeleteRow(string mainFile, ExpectedContent expected, List<string> problems) {
        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        var c1 = tbl.Column["name"];

        if (c1 is not { IsDisposed: false } nameSpalte) {
            problems.Add("Zeile löschen: Spalte 'name' fehlt.");
            tbl.Dispose();
            return;
        }

        var row = TableFactory.FindRow(tbl, nameSpalte, "wert1", ChunkOf(1));

        if (row is not { IsDisposed: false } ziel) {
            problems.Add("Zeile löschen: Zeile 'wert1' nicht gefunden.");
            tbl.Dispose();
            return;
        }

        var opr = RowCollection.Remove(ziel, "bluetable.tests Zeile löschen");

        if (opr.IsFailed) {
            problems.Add("Zeile löschen: " + opr.FailedReason);
            tbl.Dispose();
            return;
        }

        var error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();

        if (error is { Length: > 0 }) {
            problems.Add("Zeile löschen / Speichern: " + error);
            return;
        }

        expected.RemoveRow("wert1");

        tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        problems.AddRange(expected.Check(tbl));
        tbl.Dispose();
    }

    /// <summary>
    /// Phase: Spalte 'zusatz' löschen, speichern, nachladen und prüfen; die übrigen Daten müssen unangetastet sein.
    /// </summary>
    private static void DeleteColumn(string mainFile, ExpectedContent expected, List<string> problems) {
        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        var spalte = tbl.Column["zusatz"];

        if (spalte is not { IsDisposed: false } ziel) {
            problems.Add("Spalte löschen: Spalte 'zusatz' nicht gefunden.");
            tbl.Dispose();
            return;
        }

        if (!tbl.Column.Remove(ziel, "bluetable.tests Spalte löschen")) {
            problems.Add("Spalte löschen: Remove verweigert.");
            tbl.Dispose();
            return;
        }

        var error = TableAccess.SaveAndWait(tbl);
        tbl.Dispose();

        if (error is { Length: > 0 }) {
            problems.Add("Spalte löschen / Speichern: " + error);
            return;
        }

        tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        if (tbl.Column["zusatz"] is { IsDisposed: false }) {
            problems.Add("Spalte löschen: Spalte 'zusatz' nach dem Laden noch vorhanden.");
        }

        problems.AddRange(expected.Check(tbl));
        tbl.Dispose();
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Schlägt mit allen gesammelten Problemen fehl; der Ordner bleibt zur Analyse bestehen.
    /// </summary>
    private void FailPhase(string dir, string phase, List<string> problems) {
        if (problems.Count == 0) { return; }

        Assert.Fail(LayoutName + " / " + phase + ":\r\n  - " + string.Join("\r\n  - ", problems) + "\r\nOrdner behalten: " + dir);
    }

    /// <summary>
    /// Sucht ein Tabellen-Skript über seinen Namen.
    /// </summary>
    private static TableScriptDescription? FindScript(Table tbl, string keyName) {
        foreach (var script in tbl.EventScript) {
            if (string.Equals(script.KeyName, keyName, StringComparison.Ordinal)) { return script; }
        }

        return null;
    }

    /// <summary>
    /// Lädt die Tabelle erneut und prüft, dass das Skript mit dem erwarteten Inhalt vorhanden ist.
    /// </summary>
    private static void VerifyScript(string mainFile, string scriptName, string expectedText, string phase, List<string> problems) {
        var tbl = TableAccess.LoadExisting(mainFile, problems);

        if (tbl is null) { return; }

        var script = FindScript(tbl, scriptName);

        if (script is null) {
            problems.Add(phase + ": Skript nach dem Laden nicht gefunden: " + scriptName);
            tbl.Dispose();
            return;
        }

        var stored = NormalizeLineBreaks(script.Script);
        var expected = NormalizeLineBreaks(expectedText);

        if (!string.Equals(stored, expected, StringComparison.Ordinal)) {
            problems.Add(phase + ": Skript-Inhalt abweichend: '" + stored + "', erwartet '" + expected + "'");
        }

        tbl.Dispose();
    }

    /// <summary>
    /// Setzt eine Zelle über die Namens-Spalte; liefert leer bei Erfolg, sonst den Fehlertext.
    /// </summary>
    private static string SetCell(Table tbl, ColumnItem nameColumn, string rowName, string? chunkValue, ColumnItem column, string value) {
        var row = TableFactory.FindRow(tbl, nameColumn, rowName, chunkValue);

        if (row is not { IsDisposed: false } ziel) { return "Zeile nicht gefunden: " + rowName; }

        return ziel.CellSet(column, value, "bluetable.tests ändern");
    }

    /// <summary>
    /// Chunk-Wert einer Befüll-Zeile; ohne Chunk-Spalte null, damit kein Chunk-Filter entsteht.
    /// </summary>
    private string? ChunkOf(int index) => WithChunkColumn ? TableFactory.ChunkValue(index) : null;

    /// <summary>
    /// Fester Name des Test-Skripts, gültig als Skript-Systemname.
    /// </summary>
    private const string SkriptName = "rt_script";

    private static string NormalizeLineBreaks(string text) => text.Replace("\r\n", "\n").Replace('\r', '\n');

    private static string JoinProblems(List<string> problems, string fallback) => problems is { Count: > 0 } ? string.Join("; ", problems) : fallback;

    #endregion
}
