// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class TableInfoCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-info";
    public override string Description => "Tabellen: Zeigt Informationen zur Tabelle an: Übersicht, Spaltennamen, Zeilen-Keys, Zeilen mit Erstwert, Erstwerte, Spaltenmetadaten oder Werte adressierter Zeilen.";
    public override List<string> Flags => ["columnnames", "rowkeys", "row", "firstvalues", "rows"];
    public override string Syntax => "bcr table-info <tabelle> [--columnnames] | [--rowkeys] | [--rows [--max <anzahl>]] | [--firstvalues [--max <anzahl>]] | [--column <spalte>] | [--row + Zeilenadressierung [--max <anzahl>]]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
            var detailCount = 0;

            if (args.Flag("columnnames")) { detailCount++; }
            if (args.Flag("rowkeys")) { detailCount++; }
            if (args.Flag("rows")) { detailCount++; }
            if (args.Flag("firstvalues")) { detailCount++; }
            if (args.HasOption("column")) { detailCount++; }
            if (args.Flag("row")) { detailCount++; }

            if (detailCount > 1) {
                Console.Error.WriteLine("Die Optionen dürfen nicht kombiniert werden, bitte genau eine wählen.");
                return 2;
            }

            if (args.Flag("columnnames")) { WriteColumnNames(tbl); return 0; }
            if (args.Flag("rowkeys")) { WriteRowKeys(tbl); return 0; }
            if (args.Flag("rows")) { return WriteRowsWithFirstValue(tbl, args); }
            if (args.Flag("firstvalues")) { return WriteFirstValues(tbl, args); }
            if (args.HasOption("column")) { return WriteColumnDetails(tbl, args); }
            if (args.Flag("row")) { return WriteRowValues(tbl, args); }

            WriteSummary(tbl);
            return 0;
        } finally {
            Release(tbl);
        }
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
        Console.Out.WriteLine("WirdGespeichert: " + (column.SaveContent ? "ja" : "nein"));
        Console.Out.WriteLine("AdminInfo: " + column.AdminInfo);
        Console.Out.WriteLine("QuickInfo: " + column.QuickInfo);
        return 0;
    }

    private static void WriteColumnNames(Table tbl) {
        foreach (var column in tbl.Column.Where(c => c is { IsDisposed: false })) {
            var z = string.Empty;
            if (column.IsFirst) { z = " (Erstspalte)"; }
            if (column.Value_for_Chunk != ChunkType.None) { z = " (Chunkspalte)"; }

            Console.Out.WriteLine($"{column.KeyName}{z}");
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

        var columns = tbl.Column.Where(c => c is { IsDisposed: false }).ToList();
        Console.Out.WriteLine(string.Join("\t", columns.Select(c => c.KeyName)));

        var count = 0;

        foreach (var row in rows) {
            if (max > 0 && count >= max) { break; }

            Console.Out.WriteLine(string.Join("\t", columns.Select(c => row.CellGetString(c))));
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
    }

    #endregion
}
