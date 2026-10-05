// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueTable.ColumnFormats;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Legt eine neue Spalte an (nur mit dem CLI-Recht 'Add column'); Format, Beschriftung und Quickinfo per Option.
/// </summary>
public class TableAddColumnCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-addcolumn";
    public override List<string> Options => ["caption", "format", "quickinfo", "password"];
    public override string Syntax => "bcr table-addcolumn <tabelle> <spaltenname> [--caption <text>] [--format <formatkey>] [--quickinfo <text>] [--password <kennwort>]";

    public override string? HelpDetails =>
            "--caption setzt die Beschriftung, --quickinfo die Quickinfo (ohne Angabe leer). " +
            "--format erwartet einen Format-Key, Standard ist TextOneLine; alle Formate listet bcr table-columnformats. " +
            "stdout: der Key der neuen Spalte. " +
            "Beispiel: bcr table-addcolumn Test.tblh PREIS --format Double --caption Preis --quickinfo \"Netto in Euro\".";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 2) {
            return UsageError($"Erwartet werden 2 Positionsargumente (<tabelle> <spaltenname>), erhalten: {args.PositionalCount}.");
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

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        var rightProblem = RightProblem(tbl, CliRights.AddColumn);

        if (rightProblem is not null) {
            Console.Error.WriteLine(rightProblem);
            return 1;
        }

        if (tbl.Column[name] is not null) {
            Console.Error.WriteLine("Spalte existiert bereits: " + name);
            return 1;
        }

        // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
        // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
        var fragmentProblem = FragmentEditProblem(tbl);

        if (fragmentProblem is not null) {
            Console.Error.WriteLine(fragmentProblem);
            return 2;
        }

        var column = tbl.Column.GenerateAndAdd(name, args.Option("caption") ?? string.Empty, format, args.Option("quickinfo") ?? string.Empty);

        if (column is not { IsDisposed: false }) {
            Console.Error.WriteLine("Spalte konnte nicht angelegt werden: " + name);
            return 1;
        }

        Console.Out.WriteLine($"Key der neuen Spalte: {column.KeyName}");
        return SaveTable(tbl);
    }

    #endregion
}
