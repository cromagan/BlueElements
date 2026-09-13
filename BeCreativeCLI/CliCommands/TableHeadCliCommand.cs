// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Bearbeitet den Tabellenkopf. Aktuell: Tags setzen (nur als Tabellen-Administrator); leerer Wert entfernt alle Tags.
/// </summary>
public class TableHeadCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-head";
    public override List<string> Options => ["password"];
    public override string Syntax => "bcr table-head <tabelle> tags <tags, mit | getrennt>";

    public override string? HelpDetails =>
            "Leerer Wert entfernt alle Tags. Je Shell ist ein leeres Argument anders zu übergeben: " +
            "cmd.exe: bcr table-head X.mbdb tags \"\" — PowerShell 5.1 verwirft leere Argumente, dort eine Dateiumleitung oder cmd /c nutzen.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 3) {
            return UsageError($"Erwartet werden 3 Positionsargumente (<tabelle> tags <tags, mit | getrennt>), erhalten: {args.PositionalCount}.");
        }

        var what = args[1] ?? string.Empty;

        if (!string.Equals(what, "tags", StringComparison.OrdinalIgnoreCase)) {
            Console.Error.WriteLine($"Unbekannte Angabe: '{what}' — unterstützt wird: tags");
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
            // Tags liegen am Tabellenkopf: Nur ein Administrator der Tabelle darf sie ändern.
            if (!tbl.IsAdministrator()) {
                Console.Error.WriteLine("Keine Rechte zum Ändern der Tags: #CLI in den Tabellen-Admin-Gruppen der Tabelle ergänzen.");
                return 1;
            }

            // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
            // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
            var fragmentProblem = FragmentEditProblem(tbl);

            if (fragmentProblem is not null) {
                Console.Error.WriteLine(fragmentProblem);
                return 2;
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
