// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Zeigt die Werte einer Spalte an, optional begrenzt auf eine maximale Anzahl.
/// </summary>
public class TableColumnContentCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-columncontent";
    public override List<string> Options => ["column", "max", "password"];
    public override string Syntax => "bcr table-columncontent <tabelle> --column <spalte> [--max <anzahl>]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        if (!args.HasOption("column")) { return UsageError("Es fehlt die Option --column <spalte>."); }

        var (max, maxError) = ResolveMax(args);

        if (maxError is not null) {
            Console.Error.WriteLine(maxError);
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var column = ColumnOfOption(tbl, args);

        if (column is null) {
            Console.Error.WriteLine("Spalte nicht gefunden: " + args.Option("column"));
            return 1;
        }

        var count = 0;

        foreach (var value in column.Contents()) {
            if (max > 0 && count >= max) { break; }

            // Zellen trennen Zeilen mit \r; für die Konsole/Ausgabe in \n überführen.
            Console.Out.WriteLine(value.Replace("\r", "\n"));
            count++;
        }

        return 0;
    }

    #endregion
}
