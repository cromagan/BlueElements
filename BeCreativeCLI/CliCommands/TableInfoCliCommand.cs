// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Zeigt die Tabellen-Übersicht oder Details: Spaltennamen, Zeilen-Keys, Zeilen mit Erstwert, Erstwerte, Spaltenmetadaten oder Zellwerte adressierter Zeilen.
/// </summary>
public class TableInfoCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-info";
    public override List<string> Flags => ["columnnames", "rowkeys", "rowvalues", "firstvalues", "rows", "withrowkey"];
    public override List<string> Options => [.. AddressingOptions, "column", "max", "password"];
    public override string Syntax => "bcr table-info <tabelle> [--columnnames] | [--rowkeys] | [--rows [--max <anzahl>]] | [--firstvalues [--max <anzahl>]] | [--column <spalte>] | [--rowvalues + Zeilenadressierung [--max <anzahl>] [--withrowkey]] [--password <kennwort>]";

    public override string? HelpDetails =>
            "Modi (genau einen wählen, ohne Angabe gibt es die Übersicht):\n" +
            "  (kein Modus)                Übersicht: Name, Typ, Datei, Zeilen- und Spaltenzahl, Tags.\n" +
            "  --columnnames               Spaltennamen mit Beschriftung und Funktion (Erstspalte, Chunkspalte, Kapitelspalte).\n" +
            "  --rowkeys                   nur die Zeilen-Keys — direkt als --rowkey anderer Befehle verwendbar.\n" +
            "  --rows [--max <anzahl>]     je Zeile Key und FirstValue.\n" +
            "  --firstvalues [--max <anzahl>]  nur die Erstwerte.\n" +
            "  --column <spalte>           Metadaten der Spalte (Bezeichnung, Mehrzeilig, ErsteSpalte, Schluesselspalte, Kapitelspalte, WirdGespeichert, QuickInfo, ...).\n" +
            "  --rowvalues + Zeilenadressierung [--max <anzahl>] [--withrowkey]  Zellwerte der adressierten Zeilen, tab-getrennt mit Spaltennamen-Kopfzeile.\n" +
            "Zeilenadressierung:\n" +
            "  --rowkey <key>                genau eine Zeile; der Key ist der numerische Zeitstempel-Key aus --rowkeys (nicht KEY=Wert).\n" +
            "  --filtercolumn <spalte> --filtervalue <wert> [--filtertype equals|exact|contains|startswith]\n" +
            "--withrowkey wirkt nur mit --rowvalues und stellt den Zeilen-Key als erste Tab-Spalte (SYS_ROWKEY) voran.\n" +
            "Beispiele:\n" +
            "  bcr table-info X --rowkeys\n" +
            "  bcr table-info X --rowvalues --rowkey 638009530362930000\n" +
            "  bcr table-info X --rowvalues --filtercolumn KATEGORIE --filtervalue Glossar\n" +
            "  bcr table-info X --rows --max 20";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var detailCount = 0;

        if (args.Flag("columnnames")) { detailCount++; }
        if (args.Flag("rowkeys")) { detailCount++; }
        if (args.Flag("rows")) { detailCount++; }
        if (args.Flag("firstvalues")) { detailCount++; }
        if (args.HasOption("column")) { detailCount++; }
        if (args.Flag("rowvalues")) { detailCount++; }

        if (detailCount > 1) {
            Console.Error.WriteLine("Die Optionen dürfen nicht kombiniert werden, bitte genau eine wählen.");
            return 2;
        }

        if (!args.Flag("rowvalues") && (args.HasOption("rowkey") || args.HasOption("filtercolumn") || args.HasOption("filtervalue") || args.HasOption("filtertype"))) {
            Console.Error.WriteLine("Zeilenadressierung wirkt nur zusammen mit --rowvalues.");
            return 2;
        }

        if (args.Flag("withrowkey") && !args.Flag("rowvalues")) {
            Console.Error.WriteLine("--withrowkey wirkt nur zusammen mit --rowvalues.");
            return 2;
        }

        if (args.Flag("columnnames")) { WriteColumnNames(tbl); return 0; }
        if (args.Flag("rowkeys")) { WriteRowKeys(tbl); return 0; }
        if (args.Flag("rows")) { return WriteRowsWithFirstValue(tbl, args); }
        if (args.Flag("firstvalues")) { return WriteFirstValues(tbl, args); }
        if (args.HasOption("column")) { return WriteColumnDetails(tbl, args); }
        if (args.Flag("rowvalues")) { return WriteRowValues(tbl, args); }

        WriteSummary(tbl);
        return 0;
    }

    /// <summary>
    /// Liefert die externen Tabellen, die diese Spalte per Ext. Skript-'Set' befüllen, ohne Skriptname.
    /// </summary>
    private static List<string> SetByScriptTables(ColumnItem column) {
        if (column.IsDisposed) { return []; }

        List<string> t = [];

        foreach (var v in column.ColumnSystemInfo.SplitAndCutByCr().TagGetAll("Edit with Script")) {
            var i = v.LastIndexOf(" -> ", StringComparison.Ordinal);
            t.Add(i > 0 ? v[..i] : v);
        }

        return t.SortedDistinctList();
    }

    private static int WriteColumnDetails(Table tbl, CliArgs args) {
        var column = ColumnOfOption(tbl, args);

        if (column is null) {
            Console.Error.WriteLine("Spalte nicht gefunden: " + args.Option("column"));
            return 1;
        }

        Console.Out.WriteLine("KeyName: " + column.KeyName);
        Console.Out.WriteLine("Bezeichnung: " + column.ReadableText());
        Console.Out.WriteLine("Mehrzeilig: " + (column.MultiLine ? "ja" : "nein"));
        Console.Out.WriteLine("ErsteSpalte: " + (column.IsFirst ? "ja" : "nein"));
        Console.Out.WriteLine("Schluesselspalte: " + (column.IsKeyColumn ? "ja" : "nein"));
        Console.Out.WriteLine("Kapitelspalte: " + (column == ChapterColumnOfView1(tbl) ? "ja" : "nein"));
        Console.Out.WriteLine("WirdGespeichert: " + (column.SaveContent ? "ja" : "nein"));
        Console.Out.WriteLine("AdminInfo: " + column.AdminInfo);
        Console.Out.WriteLine("QuickInfo: " + column.QuickInfo);

        var quellen = SetByScriptTables(column);
        Console.Out.WriteLine("Befüllt durch externe Tabellen: " + (quellen.Count > 0 ? string.Join("; ", quellen) : "-"));
        return 0;
    }

    private static void WriteColumnNames(Table tbl) {
        var kapitelspalte = ChapterColumnOfView1(tbl);

        foreach (var column in tbl.Column.Where(c => c is { IsDisposed: false })) {
            List<string> funktionen = [];
            if (column.IsFirst) { funktionen.Add("Erstspalte"); }
            if (column.Value_for_Chunk != ChunkType.None) { funktionen.Add("Chunkspalte"); }
            if (column == kapitelspalte) { funktionen.Add("Kapitelspalte"); }

            var line = $"KeyName: {column.KeyName}, Beschriftung: \"{column.Caption.Replace("\r", " ")}\"";
            if (funktionen.Count > 0) { line += $", Funktion: {string.Join(", ", funktionen)}"; }

            Console.Out.WriteLine(line);
        }
    }

    private static int WriteFirstValues(Table tbl, CliArgs args) {
        var (max, maxError) = ResolveMax(args);

        if (maxError is not null) {
            Console.Error.WriteLine(maxError);
            return 2;
        }

        var count = 0;

        foreach (var row in tbl.RowsInSaveOrder()) {
            if (max > 0 && count >= max) { break; }

            Console.Out.WriteLine(row.CellFirstString());
            count++;
        }

        return 0;
    }

    private static void WriteRowKeys(Table tbl) {
        foreach (var row in tbl.RowsInSaveOrder()) {
            Console.Out.WriteLine(row.KeyName);
        }
    }

    private static int WriteRowsWithFirstValue(Table tbl, CliArgs args) {
        var (max, maxError) = ResolveMax(args);

        if (maxError is not null) {
            Console.Error.WriteLine(maxError);
            return 2;
        }

        var count = 0;

        foreach (var row in tbl.RowsInSaveOrder()) {
            if (max > 0 && count >= max) { break; }

            Console.Out.WriteLine($"Key: {row.KeyName}");
            Console.Out.WriteLine($"FirstValue: '{row.CellFirstString()}'");
            count++;
        }

        return 0;
    }

    private static int WriteRowValues(Table tbl, CliArgs args) {
        var problem = RowAddressingProblem(args);

        if (problem is not null) {
            Console.Error.WriteLine(problem);
            return 2;
        }

        var (max, maxError) = ResolveMax(args);

        if (maxError is not null) {
            Console.Error.WriteLine(maxError);
            return 2;
        }

        var (rows, error) = ResolveRows(tbl, args);

        if (error is not null) {
            Console.Error.WriteLine(error);
            return 1;
        }

        if (rows.Count == 0) {
            Console.Error.WriteLine("Keine Zeile getroffen.");
            return 1;
        }

        var withRowKey = args.Flag("withrowkey");
        var columns = tbl.Column.Where(c => c is { IsDisposed: false }).ToList();

        if (withRowKey) {
            // Der Key steht vorne; die Systemspalte wäre doppelt in der Ausgabe.
            columns.RemoveAll(c => tbl.Column.SysRowKey == c);
        }

        List<string> head = [];

        if (withRowKey) { head.Add(SystemColumnKeys.RowKey); }

        head.AddRange(columns.Select(c => c.KeyName));
        Console.Out.WriteLine(string.Join("\t", head));

        var count = 0;

        foreach (var row in rows) {
            if (max > 0 && count >= max) { break; }

            List<string> values = [];

            if (withRowKey) { values.Add(row.KeyName); }

            values.AddRange(columns.Select(c => row.CellGetString(c).Replace("\r", "\n")));
            Console.Out.WriteLine(string.Join("\t", values));
            count++;
        }

        return 0;
    }

    private static void WriteSummary(Table tbl) {
        var filename = tbl is TableFile tableFile ? tableFile.Filename : string.Empty;
        var columns = tbl.Column.Count(c => c is { IsDisposed: false });
        var rows = tbl.Row.Count(r => r is { IsDisposed: false });

        Console.Out.WriteLine("Name: " + tbl.KeyName);
        Console.Out.WriteLine("Typ: " + tbl.GetType().Name);
        Console.Out.WriteLine("Datei: " + filename);
        Console.Out.WriteLine("Zeilen: " + rows.ToString1());
        Console.Out.WriteLine("Spalten: " + columns.ToString1());

        var tags = tbl.Tags.Where(t => t is { Length: > 0 });
        Console.Out.WriteLine("Tags: " + (tags.Any() ? string.Join("; ", tags) : "-"));
    }

    #endregion
}
