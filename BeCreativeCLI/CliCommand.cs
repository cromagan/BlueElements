// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI;

/// <summary>
/// Basisklasse aller CLI-Befehle. Ein Befehl entspricht einer Datei im
/// Ordner CliCommands und wird über den Typ-Cache automatisch
/// gefunden. <see cref="DoIt" /> liefert den Exit-Code:
/// 0 = Erfolg, 1 = Fehler, 2 = Benutzungsfehler.
/// </summary>
public abstract class CliCommand : IHasKeyName {

    #region Fields

    private static readonly AssemblyAwareCache<CliCommand> Cache = new();

    #endregion

    #region Properties

    public static List<CliCommand> All => [.. Cache.Instances.OrderBy(c => c.Command)];

    public abstract string Command { get; }

    public abstract string Description { get; }

    /// <summary>
    /// Schalter (Optionen ohne Wert), die dieser Befehl kennt.
    /// Alle übrigen "--"-Optionen erwarten einen Wert.
    /// </summary>
    public virtual List<string> Flags => [];

    string IHasKeyName.KeyName => Command.ToUpperInvariant();

    public abstract string Syntax { get; }

    #endregion

    #region Methods

    public static CliCommand? ByName(string name) =>
                            All.Find(c => string.Equals(c.Command, name, StringComparison.OrdinalIgnoreCase));

    public abstract int DoIt(CliArgs args);

    /// <summary>
    /// Löst eine Spalte über die Option --column auf.
    /// </summary>
    protected static ColumnItem? ColumnOfOption(Table tbl, CliArgs args) => tbl.Column[args.Option("column") ?? string.Empty];

    /// <summary>
    /// Liest den Vergleichstyp der Option --filtertype. Standard: equals (Groß-/Kleinschreibung egal).
    /// </summary>
    protected static FilterType GetFilterType(CliArgs args) {
        var type = args.Option("filtertype");

        if (string.IsNullOrEmpty(type)) { return FilterType.Istgleich_GroßKleinEgal; }

        switch (type.ToUpperInvariant()) {
            case "EQUALS":
                return FilterType.Istgleich_GroßKleinEgal;

            case "EXACT":
                return FilterType.Istgleich;

            case "CONTAINS":
                return FilterType.Instr_GroßKleinEgal;

            case "STARTSWITH":
                return FilterType.BeginntMit_GroßKleinEgal;

            default:
                return FilterType.Istgleich_GroßKleinEgal;
        }
    }

    /// <summary>
    /// Lädt die Tabelle aus dem ersten Positionsargument.
    /// </summary>
    protected static Table? LoadTable(CliArgs args) => args[0] is { Length: > 0 } name ? Table.Get(name) : null;

    /// <summary>
    /// Liest die Option --max (0 = unbegrenzt). Liefert null, wenn die Angabe gültig ist, ansonsten die Fehlerbeschreibung.
    /// </summary>
    protected static (int Max, string? Error) ResolveMax(CliArgs args) {
        if (!args.HasOption("max")) { return (0, null); }

        var max = IntParse(args.Option("max") ?? string.Empty);

        return max <= 0 ? (0, "--max erwartet eine positive Zahl.") : (max, null);
    }

    /// <summary>
    /// Ermittelt die adressierten Zeilen. Vorausgesetzt wird eine zuvor mit
    /// <see cref="RowAddressingProblem" /> geprüfte Zeilenadressierung.
    /// Liefert null als Fehler, wenn keine Zeilen gefunden wurden.
    /// </summary>
    protected static (List<RowItem> Rows, string? Error) ResolveRows(Table tbl, CliArgs args) {
        if (args.HasOption("rowkey")) {
            var key = args.Option("rowkey") ?? string.Empty;
            var row = tbl.Row.GetByKey(key);
            return row is null ? ([], $"Zeile nicht gefunden: {key}") : ([row], null);
        }

        var columnName = args.Option("filtercolumn") ?? string.Empty;
        var column = tbl.Column[columnName];

        if (column is null) { return ([], $"Spalte nicht gefunden: {columnName}"); }

        var rows = FilterCollection.CalculateFilteredRows(tbl, new FilterItem(column, GetFilterType(args), args.Option("filtervalue") ?? string.Empty));
        return (rows, null);
    }

    /// <summary>
    /// Prüft die Zeilenadressierung (--rowkey oder --filtercolumn/--filtervalue).
    /// Liefert null, wenn die Angabe gültig ist, ansonsten die Fehlerbeschreibung.
    /// </summary>
    protected static string? RowAddressingProblem(CliArgs args) {
        var hasRowKey = args.HasOption("rowkey");
        var hasFilter = args.HasOption("filtercolumn") || args.HasOption("filtervalue");

        if (hasRowKey && hasFilter) { return "--rowkey darf nicht mit --filtercolumn/--filtervalue kombiniert werden."; }

        if (!hasRowKey && !hasFilter) { return "Es muss --rowkey <key> oder --filtercolumn <spalte> mit --filtervalue <wert> angegeben werden."; }

        if (hasRowKey && args.HasOption("filtertype")) { return "--filtertype benötigt --filtercolumn/--filtervalue."; }

        if (hasFilter && !(args.HasOption("filtercolumn") && args.HasOption("filtervalue"))) { return "--filtercolumn und --filtervalue müssen zusammen angegeben werden."; }

        return null;
    }

    /// <summary>
    /// Speichert die Tabelle (sofern dateibasiert) und gibt sie anschließend frei.
    /// Liefert den Exit-Code: 0 = Erfolg, 1 = Fehler beim Speichern.
    /// </summary>
    protected static int SaveTable(Table tbl) {
        if (tbl is TableFile tableFile) {
            var opr = tableFile.Save();

            if (opr.IsFailed) {
                Console.Error.WriteLine("Speichern fehlgeschlagen: " + opr.FailedReason);
                return 1;
            }
        }

        return 0;
    }

    /// <summary>
    /// Schreibt eine Zeile als CSV auf die Standardausgabe.
    /// </summary>
    protected static void WriteCsvLine(char separator, params string[] fields) =>
        Console.Out.WriteLine(string.Join(separator, CsvHelper.EscapeCSVFields([.. fields], separator)));

    #endregion
}