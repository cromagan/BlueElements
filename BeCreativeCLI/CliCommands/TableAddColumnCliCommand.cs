// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueTable.ColumnFormats;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Legt eine neue Spalte an (nur als Tabellen-Administrator). Standardformat TextOneLine;
/// Format, Beschriftung und Quickinfo per Option. Alle Formate listet 'bcr table-columnformats'.
/// </summary>
public class TableAddColumnCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-addcolumn";
    public override string Syntax => "bcr table-addcolumn <tabelle> <spaltenname> [--caption <text>] [--format <formatkey>] [--quickinfo <text>]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 2) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var name = args[1] ?? string.Empty;

        if (!ColumnItem.IsValidColumnKey(name)) {
            Console.Error.WriteLine("Ungültiger Spaltenname: '" + name + "'");
            return 2;
        }

        var formatKey = args.Option("format") ?? "TextOneLine";
        var format = ColumnFormat.AllFormats.Instances.FirstOrDefault(f => f.KeyName.Equals(formatKey, StringComparison.OrdinalIgnoreCase));

        if (format is null) {
            Console.Error.WriteLine("Unbekanntes Format: '" + formatKey + "'. Gültige Formate: " + string.Join(", ", ColumnFormat.AllFormats.Instances.Select(f => f.KeyName).OrderBy(f => f)));
            Console.Error.WriteLine("Alle Formate listet: bcr table-columnformats");
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
            var fragmentProblem = FragmentEditProblem(tbl);

            if (fragmentProblem is not null) {
                Console.Error.WriteLine(fragmentProblem);
                return 2;
            }

            // Der Tabellenkopf wird verändert: Nur ein Administrator der Tabelle darf Spalten anlegen.
            if (!tbl.IsAdministrator()) {
                Console.Error.WriteLine("Keine Rechte zum Anlegen von Spalten: #CLI in den Tabellen-Admin-Gruppen der Tabelle ergänzen.");
                return 1;
            }

            if (tbl.Column[name] is not null) {
                Console.Error.WriteLine("Spalte existiert bereits: " + name);
                return 1;
            }

            var column = tbl.Column.GenerateAndAdd(name, args.Option("caption") ?? string.Empty, format, args.Option("quickinfo") ?? string.Empty);

            if (column is not { IsDisposed: false }) {
                Console.Error.WriteLine("Spalte konnte nicht angelegt werden: " + name);
                return 1;
            }

            Console.Out.WriteLine($"Key der neuen Spalte: {column.KeyName}");
            return SaveTable(tbl);
        } finally {
            Release(tbl);
        }
    }

    #endregion
}
