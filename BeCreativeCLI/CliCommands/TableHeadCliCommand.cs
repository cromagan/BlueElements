// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class TableHeadCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-head";
    public override string Description => "Tabellen: Bearbeitet den Tabellenkopf. Aktuell: Tags setzen (nur als Tabellen-Administrator); leerer Wert entfernt alle Tags.";
    public override string Syntax => "bcr table-head <tabelle> tags <tags, mit | getrennt>";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 3) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var what = args[1] ?? string.Empty;

        if (!string.Equals(what, "tags", StringComparison.OrdinalIgnoreCase)) {
            Console.Error.WriteLine($"Unbekannte Angabe: '{what}' — unterstützt wird: tags");
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

            // Tags liegen am Tabellenkopf: Nur ein Administrator der Tabelle darf sie ändern.
            if (!tbl.IsAdministrator()) {
                Console.Error.WriteLine("Keine Rechte zum Ändern der Tags: #CLI in den Tabellen-Admin-Gruppen der Tabelle ergänzen.");
                return 1;
            }

            var value = args[2] ?? string.Empty;

            tbl.Tags = new(value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

            Console.Out.WriteLine(tbl.Tags.Count > 0
                                              ? "Tags gesetzt: " + string.Join(", ", tbl.Tags)
                                              : "Alle Tags entfernt.");
            return SaveTable(tbl);
        } finally {
            Release(tbl);
        }
    }

    #endregion
}
