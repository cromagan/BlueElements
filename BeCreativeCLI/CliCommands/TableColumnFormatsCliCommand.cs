// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueTable.ColumnFormats;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Listet alle Spaltenformate auf, die 'table-addcolumn' über --format akzeptiert.
/// </summary>
public class TableColumnFormatsCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-columnformats";
    public override string Syntax => "bcr table-columnformats";

    public override string? HelpDetails => "Je Zeile der Format-Key, bei vorhandenem Quickinfo nach einem Doppelpunkt; der Key ist der Wert für table-addcolumn --format.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 0) {
            return UsageError($"Dieser Befehl nimmt keine Argumente, erhalten: {args.PositionalCount}.");
        }

        foreach (var format in ColumnFormat.AllFormats.Instances.OrderBy(f => f.KeyName)) {
            Console.Out.WriteLine(format.QuickInfo is { Length: > 0 } q
                                      ? format.KeyName + ": " + q
                                      : format.KeyName);
        }

        return 0;
    }

    #endregion
}
