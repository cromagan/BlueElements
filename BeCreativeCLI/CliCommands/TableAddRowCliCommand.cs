// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Legt eine neue Zeile an. Der Wert setzt die erste Spalte (Primärschlüssel) der Tabelle.
/// Weitere Spalten füllt --set Spalte=Wert direkt beim Anlegen.
/// </summary>
public class TableAddRowCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-addrow";
    public override List<string> Options => ["firstvalue", "set", "password"];
    public override string Syntax => "bcr table-addrow <tabelle> [--firstvalue <wert>] [--set <spalte>=<wert>]";

    public override string? HelpDetails =>
            "--set füllt weitere Spalten direkt beim Anlegen und darf mehrfach angegeben werden: --set KATEGORIE=Regeln --set ANLEITUNG=\"Text mit Leerzeichen\". " +
            "Format: <spalte>=<wert>; der Wert darf leer sein. Leerzeichen und Sonderzeichen in Anführungszeichen setzen. " +
            "Beispiel: bcr table-addrow Test.tblh --firstvalue NeuerEintrag --set KATEGORIE=Glossar.";

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

            var columnProblem = ColumnWriteProblem(tbl, column);

            if (columnProblem is not null) {
                Console.Error.WriteLine(columnProblem);
                return 1;
            }

            sets.Add((column, set[(eq + 1)..]));
        }

        // Bevorzugt die als 'First' markierte Spalte, ansonsten die erste Spalte der Speicherreihenfolge.
        var firstColumn = tbl.Column.First ?? tbl.ColumnsInSaveOrder().FirstOrDefault();

        if (firstColumn is not { IsDisposed: false }) {
            Console.Error.WriteLine("Die Tabelle hat keine erste Spalte.");
            return 1;
        }

        var firstProblem = ColumnWriteProblem(tbl, firstColumn);

        if (firstProblem is not null) {
            Console.Error.WriteLine(firstProblem);
            return 1;
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

        // Wert in das Speicherformat der ersten Spalte überführen (z. B. HTML-Entities).
        var value = StorageTextOf(firstColumn, args.Option("firstvalue") ?? string.Empty);

        var opr = tbl.Row.GenerateAndAdd([new FilterItem(firstColumn, FilterType.Istgleich, value)], "bcr table-addrow");

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
