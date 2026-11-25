// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueTable.ColumnFormats;

namespace BlueTable.Tests;

/// <summary>
/// Globale Routine zum Anlegen, Befüllen und Prüfen von Test-Tabellen für alle Datei-Layouts.
/// </summary>
public static class TableFactory {

    #region Methods

    /// <summary>
    /// Befüllt eine leere Tabelle: Spalten name/menge/notiz und optional eine Chunk-Spalte,
    /// dazu rowCount Zeilen mit festen Werten inkl. Sonderzeichen. Liefert leer bei Erfolg, sonst den Fehlertext.
    /// </summary>
    public static string Fill(Table tbl, int rowCount, bool withChunk) {
        var format = TextOneLineFormat();

        if (format is null) { return "Format 'TextOneLine' nicht gefunden."; }

        var c1 = tbl.Column.GenerateAndAdd("name", "Name", format, string.Empty);
        var c2 = tbl.Column.GenerateAndAdd("menge", "Menge", format, string.Empty);
        var c3 = tbl.Column.GenerateAndAdd("notiz", "Notiz", format, string.Empty);

        if (c1 is null || c2 is null || c3 is null) { return "Spalten konnten nicht angelegt werden."; }

        if (withChunk) {
            var chunkSpalte = tbl.Column.GenerateAndAdd("chunk", "Chunk", format, string.Empty);

            if (chunkSpalte is not { IsDisposed: false } cvc) { return "Chunk-Spalte konnte nicht angelegt werden."; }

            cvc.Value_for_Chunk = ChunkType.ByName;
            tbl.Column.GetSystems();
        }

        for (var i = 1; i <= rowCount; i++) {
            List<FilterItem> filters = [new(c1, FilterType.Istgleich, c1.AutoCorrect("wert" + i.ToString1(), true))];

            // Muss-Spalte: Die Chunk-Spalte je Zeile auf drei Werte verteilen; der Filterwert
            // wird beim Anlegen als Initialwert der Zelle übernommen.
            if (withChunk && tbl.Column["chunk"] is { IsDisposed: false } cvcZeile) {
                filters.Add(new FilterItem(cvcZeile, FilterType.Istgleich, ChunkValue(i)));
            }

            var opr = tbl.Row.GenerateAndAdd([.. filters], "bluetable.tests befüllen");

            if (opr.IsFailed || opr.Value is not RowItem row) { return "Zeile " + i.ToString1() + ": " + opr.FailedReason; }

            var e2 = row.CellSet(c2, (i * 10).ToString1(), "bluetable.tests befüllen");

            if (e2 is { Length: > 0 }) { return "menge, Zeile " + i.ToString1() + ": " + e2; }

            var e3 = row.CellSet(c3, NotizValue(i, rowCount), "bluetable.tests befüllen");

            if (e3 is { Length: > 0 }) { return "notiz, Zeile " + i.ToString1() + ": " + e3; }
        }

        RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
        return string.Empty;
    }

    /// <summary>
    /// Trägt ein Tabellen-Skript in die Tabelle ein; liefert leer bei Erfolg, sonst den Fehlertext.
    /// </summary>
    public static string AddScript(Table tbl, string scriptName, string scriptText) {
        if (!ScriptDescription.IsValidName(scriptName)) { return "Skriptname ungültig: " + scriptName; }

        var precheck = ScriptPreCheck.Check(scriptText);

        if (precheck.HasSyntaxErrors) { return "Syntaxfehler im Skript: " + string.Join("; ", precheck.SyntaxErrors); }

        if (tbl is TableFileNew) {
            // Binärformat: Erst leer aufnehmen und dann den Text über die beobachtete
            // Instanz setzen — nur so werden die Skript-Blätter als Property-Journal
            // erfasst; ein vorgefülltes Item würde nur den ItemAdd-Kopf ohne Inhalt journale.
            var script = new TableScriptDescription(tbl, scriptName, string.Empty);
            tbl.EventScript = new TableScriptCollection(tbl, [.. tbl.EventScript, script]);

            var stored = tbl.EventScript.GetByKey(scriptName);

            if (stored is not { IsDisposed: false }) { return "Skript nach der Aufnahme nicht gefunden: " + scriptName; }

            stored.Script = scriptText;
            return string.Empty;
        }

        // Legacy-Formate: Die Sammlung wird komplett überschrieben; der EventScript-Setter
        // journalt den gesamten Skript-Satz.
        var legacy = new TableScriptDescription(tbl, scriptName, scriptText);
        tbl.EventScript = new TableScriptCollection(tbl, [.. tbl.EventScript, legacy]);
        return string.Empty;
    }

    /// <summary>
    /// Sucht eine Zeile über die Namens-Spalte und optional deren Chunk-Wert (bei Chunk-Spalten zwingend).
    /// </summary>
    public static RowItem? FindRow(Table tbl, ColumnItem nameColumn, string rowName, string? chunkValue) {
        List<FilterItem> filters = [new(nameColumn, FilterType.Istgleich, nameColumn.AutoCorrect(rowName, true))];

        if (chunkValue is { Length: > 0 } && tbl.Column["chunk"] is { IsDisposed: false } cc) {
            filters.Add(new FilterItem(cc, FilterType.Istgleich, chunkValue));
        }

        var filter = filters.ToArray();
        return tbl.Row[filter];
    }

    /// <summary>
    /// Vergleicht den gespeicherten Zellwert gegen den erwarteten Rohwert (AutoCorrect der Spalte) und trägt Abweichungen ein.
    /// </summary>
    public static void CompareCell(RowItem row, ColumnItem column, string expectedValue, string columnKey, string rowName, List<string> problems) {
        var stored = row.CellGetStringCore(column);
        var expected = column.AutoCorrect(expectedValue, true);

        if (!string.Equals(stored, expected, StringComparison.Ordinal)) {
            problems.Add("Zeile '" + rowName + "', Spalte '" + columnKey + "': '" + stored + "', erwartet '" + expected + "'");
        }
    }

    /// <summary>
    /// Fester Notiz-Wert einer Befüll-Zeile inkl. Sonderzeichen; die letzte Zeile trägt zusätzlich einen langen Text.
    /// </summary>
    public static string NotizValue(int index, int rowCount) {
        var text = "ÄÖÜäöüß " + index.ToString1() + " |Zitat \"X\"| ✓";

        if (index == rowCount) { text += new string('L', 500); }

        return text;
    }

    /// <summary>
    /// Fester Chunk-Wert einer Befüll-Zeile; drei Werte, damit sich die Zeilen nachweislich verteilen.
    /// </summary>
    public static string ChunkValue(int index) => "cb" + (index % 3).ToString1();

    /// <summary>
    /// Sucht das Spalten-Format 'TextOneLine' über den Instanzen-Cache.
    /// </summary>
    private static ColumnFormat? TextOneLineFormat() {
        foreach (var format in ColumnFormat.AllFormats.Instances) {
            if (string.Equals(format.KeyName, "TextOneLine", StringComparison.OrdinalIgnoreCase)) { return format; }
        }

        return null;
    }

    #endregion
}
