// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Legt eine Zeile an; wiederholbares --set füllt Spalten (auch die Erstspalte) direkt beim Anlegen.
/// </summary>
public class TableAddRowCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-addrow";
    public override List<string> Options => ["set", "password"];
    public override string Syntax => "bcr table-addrow <tabelle> [--set <spalte>=<wert>] [--password <kennwort>]";

    public override string? HelpDetails =>
            "--set füllt Spalten direkt beim Anlegen und darf mehrfach angegeben werden: --set KATEGORIE=Regeln --set ANLEITUNG=\"Text mit Leerzeichen\". " +
            "Die --set-Werte dienen beim Anlegen zugleich als Filter: Alle Pflicht-Spalten (Erstspalte, Chunk- und Unique-Spalten) müssen daher per --set mitgegeben werden. " +
            "Für die --set-Spalten gelten dieselben Zellwerte-Rechte wie bei table-cellset ('Change all cell values' oder 'Change permitted cell values' mit Spalten-Freigabe; Systemspalten je eigenem Recht). " +
            "Format: <spalte>=<wert>; der Wert darf leer sein. Leerzeichen und Sonderzeichen in Anführungszeichen setzen. " +
            "Beispiel: bcr table-addrow Test.tblh --set <erstspalte>=NeuerEintrag --set KATEGORIE=Glossar.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        // --set-Angaben vollständig prüfen, bevor die Zeile angelegt wird.
        List<(ColumnItem Column, string Value)> sets = [];
        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        foreach (var set in args.AllOptions("set")) {
            var eq = set.IndexOf('=');

            if (eq <= 0) {
                return UsageError("--set erwartet <spalte>=<wert>, erhalten: " + set);
            }

            var columnName = set[..eq];
            var column = tbl.Column[columnName];

            if (column is not { IsDisposed: false }) {
                return UsageError("Spalte nicht gefunden: " + columnName);
            }

            var value = set[(eq + 1)..];
            var columnProblem = ColumnWriteProblem(tbl, column, value) ?? CellValueRightProblem(tbl, column);

            if (columnProblem is not null) {
                Console.Error.WriteLine(columnProblem);
                return 1;
            }

            sets.Add((column, value));
        }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        var rightProblem = RightProblem(tbl, CliRights.AddRow);

        if (rightProblem is not null) {
            Console.Error.WriteLine(rightProblem);
            return 1;
        }

        // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
        // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
        var fragmentProblem = FragmentEditProblem(tbl);

        if (fragmentProblem is not null) {
            Console.Error.WriteLine(fragmentProblem);
            return 2;
        }

        // Filter für GenerateAndAdd: alle --set-Spalten als Istgleich-Filter.
        // GenerateAndAdd verlangt, dass alle Pflicht-Spalten (Erstspalte, Chunk- und Unique-Spalten)
        // im Filter stehen; die --set-Werte dienen zugleich als Initialwerte und Unique-Abgleich.
        // Die Werte werden je Spalte in das Speicherformat überführt (z. B. HTML-Entities).
        List<FilterItem> filter = [];

        foreach (var (column, setValue) in sets) {
            filter.Add(new FilterItem(column, FilterType.Istgleich, StorageTextOf(column, setValue)));
        }

        var opr = tbl.Row.GenerateAndAdd([.. filter], "bcr table-addrow");

        if (opr.IsFailed || opr.Value is not RowItem row) {
            Console.Error.WriteLine("Zeile konnte nicht angelegt werden: " + opr.FailedReason);
            return 1;
        }

        foreach (var (column, setValue) in sets) {
            // Wert in das Speicherformat der Spalte überführen (z. B. HTML-Entities).
            var failed = row.CellSet(column, StorageTextOf(column, setValue), "bcr table-addrow");

            if (!string.IsNullOrEmpty(failed)) {
                Console.Error.WriteLine($"Spalte {column.KeyName} konnte nicht gesetzt werden: {failed}");
                return 1;
            }
        }

        Console.Out.WriteLine($"Key der neuen Zeile: {row.KeyName}");
        return SaveTable(tbl);
    }

    #endregion
}
