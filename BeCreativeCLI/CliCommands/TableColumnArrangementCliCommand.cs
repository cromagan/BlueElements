// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Liest oder setzt die Spalten einer bestehenden Spaltenanordnung.
/// Setzen nur mit dem CLI-Recht 'Change column arrangement'; Ansicht 0 ist
/// schreibgeschützt, neue Ansichten sind per CLI nicht möglich.
/// </summary>
public class TableColumnArrangementCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-columnarrangement";

    public override string? HelpDetails =>
            "Ohne <ansicht> werden alle Ansichten aufgelistet (Nr, Name, Spalten, tab-getrennt). Mit <ansicht> und ohne " +
            "<spalten> wird die Spaltenliste der Ansicht als 'spalte1|spalte2|...' ausgegeben — direkt als <spalten>-Argument " +
            "wiederverwendbar. Lesen benötigt kein CLI-Recht; Ansicht 0 (alle Spalten) ist ausschließlich lesbar. " +
            "Mit <spalten> werden die sichtbaren Spalten der Ansicht in der angegebenen Reihenfolge gesetzt (NUR mit dem " +
            "CLI-Recht 'Change column arrangement'); Spalten, die nicht genannt werden, werden ausgeblendet. " +
            "Unbekannte Spalten und Doppel-Eintragungen werden abgelehnt.";

    public override List<string> Options => ["password"];

    public override string Syntax => "bcr table-columnarrangement <tabelle> [<ansicht> [<spalten, mit | getrennt>]]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        switch (args.PositionalCount) {
            case 1:
                return ListArrangements(args);

            case 2:
                return ShowArrangement(args);

            case 3:
                return SetArrangement(args);

            default:
                return UsageError($"Erwartet werden 1 bis 3 Positionsargumente (<tabelle> [<ansicht> [<spalten, mit | getrennt>]]), erhalten: {args.PositionalCount}.");
        }
    }

    /// <summary>
    /// Listet alle Ansichten auf: Nummer, Name und Spaltenliste je Zeile (tab-getrennt).
    /// </summary>
    private static int ListArrangements(CliArgs args) {
        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var tcvc = ColumnViewCollection.ParseAll(tbl);

        for (var z = 0; z < tcvc.Count; z++) {
            Console.Out.WriteLine(z + "\t" + tcvc[z].KeyName + "\t" + ColumnList(tcvc[z]));
        }

        return 0;
    }

    /// <summary>
    /// Gibt die Spalten einer Ansicht als 'spalte1|spalte2|...' aus — in derselben Form,
    /// die das Setzen als Spaltenliste erwartet.
    /// </summary>
    private int ShowArrangement(CliArgs args) {
        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var viewProblem = ReadViewProblem(tbl, args[1] ?? string.Empty, out var viewIndex);

        if (viewProblem is not null) { return UsageError(viewProblem); }

        Console.Out.WriteLine(ColumnList(ColumnViewCollection.ParseAll(tbl)[viewIndex]));
        return 0;
    }

    /// <summary>
    /// Setzt die sichtbaren Spalten der Ansicht in der angegebenen Reihenfolge.
    /// </summary>
    private int SetArrangement(CliArgs args) {
        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        var rightProblem = RightProblem(tbl, CliRights.ChangeColumnArrangement);

        if (rightProblem is not null) {
            Console.Error.WriteLine(rightProblem);
            return 1;
        }

        var viewProblem = ArrangementProblem(tbl, args[1] ?? string.Empty, out var viewIndex);

        if (viewProblem is not null) { return UsageError(viewProblem); }

        var columnProblem = ColumnsProblem(tbl, args[2] ?? string.Empty, out var columns);

        if (columnProblem is not null) { return UsageError(columnProblem); }

        // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
        // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
        var fragmentProblem = FragmentEditProblem(tbl);

        if (fragmentProblem is not null) {
            Console.Error.WriteLine(fragmentProblem);
            return 2;
        }

        // ParseAll liefert frische Kopien; die Tabelle ändert sich erst durch die Zuweisung unten.
        var tcvc = ColumnViewCollection.ParseAll(tbl);
        var target = tcvc[viewIndex];

        target.RemoveAll();
        foreach (var column in columns) {
            target.Add(new ColumnViewItem(column));
        }

        tbl.ColumnArrangements = tcvc.AsReadOnly();

        Console.Out.WriteLine("Spaltenanordnung '" + target.KeyName + "' gesetzt: " + string.Join(", ", columns.Select(c => c.KeyName)));
        return SaveTable(tbl);
    }

    /// <summary>
    /// Löst die Ansicht für das Lesen über Nummer (inklusive 0) oder Namen auf.
    /// </summary>
    private static string? ReadViewProblem(Table tbl, string viewSpec, out int viewIndex) {
        viewIndex = -1;

        if (viewSpec.Length == 0) { return "Es muss eine Ansicht (Nummer ab 0 oder Name) angegeben werden."; }

        var existing = tbl.ColumnArrangements.Count - 1;

        if (viewSpec.IsLong()) {
            var nr = IntParse(viewSpec);

            if (nr < 0 || nr > existing) { return $"Ansicht {nr} existiert nicht — vorhanden: 0 bis {existing} (Nummer oder Name)."; }

            viewIndex = nr;
            return null;
        }

        for (var z = 0; z < tbl.ColumnArrangements.Count; z++) {
            if (string.Equals(tbl.ColumnArrangements[z].KeyName, viewSpec, StringComparison.OrdinalIgnoreCase)) {
                viewIndex = z;
                return null;
            }
        }

        return $"Keine Spaltenanordnung namens '{viewSpec}' — vorhanden: 0 bis {existing} (Nummer oder Name).";
    }

    /// <summary>
    /// Liefert die Spaltennamen der Ansicht in Anzeigereihenfolge, mit | getrennt.
    /// </summary>
    private static string ColumnList(ColumnViewCollection view) {
        List<string> names = [];

        foreach (var item in view) {
            if (item?.ColumnName is { Length: > 0 } name) { names.Add(name); }
        }

        return string.Join("|", names);
    }

    /// <summary>
    /// Löst die Ansicht über Nummer oder Namen auf. Ansicht 0 ist geschützt; unbekannte
    /// Ansichten werden abgelehnt, da neue Ansichten per CLI nicht angelegt werden können.
    /// </summary>
    private static string? ArrangementProblem(Table tbl, string viewSpec, out int viewIndex) {
        viewIndex = -1;

        if (viewSpec.Length == 0) { return "Es muss eine Ansicht (Nummer ab 1 oder Name) angegeben werden."; }

        var existing = tbl.ColumnArrangements.Count - 1;

        if (viewSpec.IsLong()) {
            var nr = IntParse(viewSpec);

            if (nr <= 0) { return "Ansicht 0 zeigt immer alle Spalten und darf nicht verändert werden."; }
            if (nr > existing) { return $"Ansicht {nr} existiert nicht. Neue Ansichten sind per CLI nicht möglich — vorhanden: 1 bis {existing}."; }

            viewIndex = nr;
            return null;
        }

        for (var z = 1; z < tbl.ColumnArrangements.Count; z++) {
            if (string.Equals(tbl.ColumnArrangements[z].KeyName, viewSpec, StringComparison.OrdinalIgnoreCase)) {
                viewIndex = z;
                return null;
            }
        }

        return $"Keine Spaltenanordnung namens '{viewSpec}'. Neue Ansichten sind per CLI nicht möglich — vorhanden: 1 bis {existing} (Nummer oder Name).";
    }

    /// <summary>
    /// Löst die gewünschten Spalten in der Anzeigereihenfolge auf. Die Liste ist vollständig:
    /// Nicht genannte Spalten verschwinden aus der Ansicht. Unbekannte Namen, Systemspalten
    /// und Doppel-Eintragungen sind Fehler.
    /// </summary>
    private static string? ColumnsProblem(Table tbl, string columnList, out List<ColumnItem> columns) {
        columns = [];

        var names = columnList.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (names.Length == 0) { return "Es muss mindestens eine Spalte angegeben werden (z. B. 'Name|Datum|Status')."; }

        foreach (var name in names) {
            var column = tbl.Column[name];

            if (column is null) { return "Spalte nicht gefunden: " + name; }
            //if (column.IsSystemColumn()) { return "Systemspalte '" + name + "' kann nicht in eine Ansicht aufgenommen werden."; }
            if (columns.Contains(column)) { return "Spalte doppelt angegeben: " + name; }

            columns.Add(column);
        }

        return null;
    }

    #endregion
}