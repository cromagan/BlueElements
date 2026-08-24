// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class TableExportCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-export";
    public override string Description => "Tabellen: Exportiert die Tabelle als CSV auf die Standardausgabe.";
    public override List<string> Flags => ["noheader"];
    public override string Syntax => "bcr table-export <tabelle> [--sep <trennzeichen>] [--noheader]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        if (args.HasOption("out")) {
            Console.Error.WriteLine("--out wird nicht unterstützt. Die Ausgabe erfolgt ausschließlich auf stdout, z. B. mit einer Umleitung in eine Datei.");
            return 2;
        }

        var separator = ';';

        if (args.Option("sep") is { Length: > 0 } sep) { separator = sep[0]; }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
            Console.Out.Write(CsvHelper.ExportCSV(tbl, separator, !args.Flag("noheader")));
            return 0;
        } finally {
            Release(tbl);
        }
    }

    #endregion
}
