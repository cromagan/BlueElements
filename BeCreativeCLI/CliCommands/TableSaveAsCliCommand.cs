// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Speichert eine Tabelle im durch die Ziel-Endung bestimmten Format als neue Datei (z. B. .mbdb -> .mtblj).
/// </summary>
public class TableSaveAsCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-saveas";
    public override List<string> Options => ["password"];
    public override string Syntax => "bcr table-saveas <tabelle> <zieldatei>";

    public override string? HelpDetails =>
            "Legt eine neue, vollständige Tabellendatei an und übernimmt die kompletten Daten der Quelltabelle — " +
            "ohne deren Fragment-Historie. Das Zielformat folgt der Endung: .bdb, .mbdb, .tblh, .tblj oder .mtblj. " +
            "Beispiel: bcr table-saveas Test.mbdb Test2.mtblj";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 2) {
            return UsageError($"Erwartet werden 2 Positionsargumente (<tabelle> <zieldatei>), erhalten: {args.PositionalCount}.");
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var targetPath = args[1] ?? string.Empty;

        var opr = TableFile.Create(targetPath, tbl);

        if (opr.IsFailed) {
            Console.Error.WriteLine("Speichern unter fehlgeschlagen: " + opr.FailedReason);
            return 1;
        }

        Console.Out.WriteLine("Gespeichert als: " + targetPath.NormalizeFile());
        return 0;
    }

    #endregion
}
