// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Zeigt Zellwerte; --column und --rowkey akzeptieren Komma-getrennte Listen.
/// Ein einzelner Wert wird roh ausgegeben, mehrere Werte als Tab-Tabelle mit Kopfzeile.
/// </summary>
public class TableCellGetCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-cellget";
    public override List<string> Flags => ["decode"];
    public override List<string> Options => [.. AddressingOptions, "column", "password"];
    public override string Syntax => "bcr table-cellget <tabelle> --column <spalte>[,<spalte>...] [--decode] + Zeilenadressierung (--rowkey <key>[,<key>...] oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>]) [--password <kennwort>]";

    public override string? HelpDetails =>
            "--decode gibt echte Umlaute statt HTML-Entities aus (wie table-export --decode); bei Spalten mit HTML-Inhalt gilt <br> zusätzlich als Zeilenumbruch. " +
            "--column akzeptiert Komma-getrennte Spaltenlisten, --rowkey Komma-getrennte Key-Listen — Werte mehrerer Spalten/Zeilen in einem Aufruf. " +
            "Bei genau einer Spalte und genau einer Zeile erscheint der Rohwert (Zeilentrenner innerhalb der Zelle als \\n). " +
            "Bei mehreren Spalten oder Zeilen folgt eine Kopfzeile und je Zeile eine Tab-Zeile mit SYS_ROWKEY als erster Spalte; " +
            "Umbrüche sind als Literal \\n escaped (Backslash verdoppelt), damit jede Ausgabezeile stabil parsebar bleibt.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        if (!args.HasOption("column")) { return UsageError("Es fehlt die Option --column <spalte>."); }

        var problem = RowAddressingProblem(args);

        if (problem is not null) {
            Console.Error.WriteLine(problem);
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var (columns, columnError) = ResolveColumns(tbl, args.Option("column") ?? string.Empty);

        if (columnError is not null) {
            Console.Error.WriteLine(columnError);
            return 1;
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

        var decode = args.Flag("decode");

        // Genau ein Wert: Rohwert mit echten Umbrüchen (bisheriges Verhalten, ideal für Shell-Variablen).
        if (columns.Count == 1 && rows.Count == 1) {
            var value = rows[0].CellGetString(columns[0]);

            if (decode) { value = DecodedCellText(columns[0], value); }

            Console.Out.WriteLine(value.Replace("\r", "\n"));
            return 0;
        }

        // Batch: Kopfzeile plus je Ergebniszeile eine Tab-Zeile; Umbrüche als Literal \n.
        List<string> head = [SystemColumnKeys.RowKey];
        head.AddRange(columns.Select(c => c.KeyName));
        Console.Out.WriteLine(string.Join("\t", head));

        foreach (var row in rows) {
            List<string> values = [row.KeyName];
            values.AddRange(columns.Select(c => CellTextOf(row, c, decode, true)));
            Console.Out.WriteLine(string.Join("\t", values));
        }

        return 0;
    }

    #endregion
}
