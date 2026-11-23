// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Speichert eine Tabelle im durch die Ziel-Endung bestimmten Format als neue Datei (z. B. .mbdb -> .bdb).
/// </summary>
public class TableSaveAsCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-saveas";

    public override string? HelpDetails =>
            "Legt eine neue, vollständige Tabellendatei an und übernimmt die kompletten Daten der Quelltabelle — " +
            "ohne deren Fragment-Historie. Das Zielformat folgt der Endung: .bdb, .btb, .mbdb, .tblh oder .tblt. " +
            "Beispiel: bcr table-saveas Test.mbdb Test2.bdb";

    public override List<string> Options => ["password"];
    public override string Syntax => "bcr table-saveas <tabelle> <zieldatei> [--password <kennwort>]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 2) {
            return UsageError($"Erwartet werden 2 Positionsargumente (<tabelle> <zieldatei>), erhalten: {args.PositionalCount}.");
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var targetPath = args[1] ?? string.Empty;

        // Relative Ziele auflösen: Die Datei-Routinen arbeiten mit absoluten Pfaden.
        if (targetPath is { Length: > 0 } && !Path.IsPathRooted(targetPath)) {
            try {
                targetPath = Path.GetFullPath(targetPath);
            } catch {
                Console.Error.WriteLine("Zieldatei ungültig: " + targetPath);
                return 2;
            }
        }

        // Über Table.Create: .btb-Ziele landen im Fragment-Layout (TableFileNew),
        // alle übrigen Endungen im klassischen Format.
        var opr = Table.Create(targetPath, tbl);

        if (opr.IsFailed) {
            Console.Error.WriteLine("Speichern unter fehlgeschlagen: " + opr.FailedReason);
            return 1;
        }

        Console.Out.WriteLine("Gespeichert als: " + targetPath.NormalizeFile());
        return 0;
    }

    #endregion
}