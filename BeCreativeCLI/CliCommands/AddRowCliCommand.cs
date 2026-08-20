// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class AddRowCliCommand : CliCommand {

    #region Properties

    public override string Command => "addrow";
    public override string Description => "Legt eine neue Zeile an. Der Wert setzt die erste Spalte (Primärschlüssel) der Tabelle.";
    public override string Syntax => "bcr addrow <tabelle> [--firstvalue <wert>]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        if (args.HasOption("rowkey")) {
            Console.Error.WriteLine("--rowkey wird von addrow nicht unterstützt. Stattdessen --firstvalue verwenden.");
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) {
            Console.Error.WriteLine("Tabelle nicht gefunden: " + args[0]);
            return 1;
        }

        try {
            if (tbl.Column.First is not { IsDisposed: false } firstColumn) {
                Console.Error.WriteLine("Die Tabelle hat keine erste Spalte.");
                return 1;
            }

            var value = args.Option("firstvalue") ?? string.Empty;

            var opr = tbl.Row.GenerateAndAdd([new FilterItem(firstColumn, FilterType.Istgleich, value)], "bcr addrow");

            if (opr.IsFailed || opr.Value is not RowItem row) {
                Console.Error.WriteLine("Zeile konnte nicht angelegt werden: " + opr.FailedReason);
                return 1;
            }

            Console.Out.WriteLine($"Key der neuen Zeile: {row.KeyName}");
            return SaveTable(tbl);
        } finally {
            tbl.Dispose();
        }
    }

    #endregion
}