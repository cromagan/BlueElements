// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.IO;

namespace BeCreativeCLI;

/// <summary>
/// Basisklasse aller CLI-Befehle. Ein Befehl entspricht einer Datei im
/// Ordner CliCommands und wird über den Typ-Cache automatisch
/// gefunden. DoIt liefert den Exit-Code:
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

    /// <summary>
    /// True, solange eine Shell-Session läuft. Geladene Tabellen werden dann
    /// nicht freigegeben, damit Folgebefehle dieselbe Instanz — und damit
    /// denselben Fragment-Writer — nutzen.
    /// </summary>
    protected static bool SessionActive { get; set; }

    #endregion

    #region Methods

    public static CliCommand? ByName(string name) =>
                            All.Find(c => string.Equals(c.Command, name, StringComparison.OrdinalIgnoreCase));

    public abstract int DoIt(CliArgs args);

    /// <summary>
    /// Liefert die Kapitelspalte der Ansicht 1 oder null, wenn keine definiert ist.
    /// </summary>
    protected static ColumnItem? ChapterColumnOfView1(Table tbl) =>
                        tbl.ColumnArrangements.Count > 1 ? tbl.ColumnArrangements[1].ColumnForChapter : null;

    /// <summary>
    /// Löst eine Spalte über die Option --column auf.
    /// </summary>
    protected static ColumnItem? ColumnOfOption(Table tbl, CliArgs args) => tbl.Column[args.Option("column") ?? string.Empty];

    /// <summary>
    /// Prüft, ob die Tabelle außerhalb einer Shell-Session bearbeitet werden darf.
    /// Fragment-Tabellen sind Multi-User-Formate: Nur in einer Session bleibt der
    /// Fragment-Writer über alle Befehle hinweg offen — Einzelaufrufe sind gesperrt.
    /// Liefert null, wenn die Bearbeitung erlaubt ist, ansonsten die Fehlermeldung.
    /// </summary>
    protected static string? FragmentEditProblem(Table tbl) {
        if (SessionActive || tbl is not (TableFragments or TableJsonFragments)) { return null; }

        return "Bearbeitungen an Fragment-Tabellen (Typ " + tbl.GetType().Name + ") sind nur in einer Shell-Session möglich: 'bcr shell' starten und den Befehl dort ausführen.";
    }

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
    /// Lädt die Tabelle aus dem ersten Positionsargument. Ohne Pfadangabe wird
    /// das aktuelle Verzeichnis als Suchpfad ergänzt. Gibt bei Problemen
    /// (nicht gefunden, passwortgeschützt) eine Fehlermeldung aus und liefert null.
    /// </summary>
    protected static Table? LoadTable(CliArgs args) {
        var tbl = LoadTableIgnoreLock(args);

        if (tbl is null) { return null; }

        if (!tbl.Unlocked) {
            Console.Error.WriteLine("Tabelle '" + tbl.KeyName + "' ist passwortgeschützt — in einer Shell-Session mit 'table-password' entsperren.");
            tbl.Dispose();
            return null;
        }

        return tbl;
    }

    /// <summary>
    /// Lädt die Tabelle wie LoadTable, akzeptiert aber auch gesperrte
    /// Tabellen. Benötigt für 'table-password', das vor der Prüfung entsperren muss.
    /// </summary>
    protected static Table? LoadTableIgnoreLock(CliArgs args) {
        if (args[0] is not { Length: > 0 } name) { return null; }

        if (!name.IsValidFilepathAndName() && !name.Contains('|')) {
            try {
                name = Path.GetFullPath(name);
            } catch {
                Console.Error.WriteLine("Tabelle nicht gefunden: " + name);
                return null;
            }
        }

        var tbl = Table.Get(name);

        if (tbl is not { IsDisposed: false }) {
            Console.Error.WriteLine("Tabelle nicht gefunden: " + name);
            return null;
        }

        // Bei JEDEM Aufruf aktualisieren: lädt z. B. Fragmente nach, die andere
        // Prozesse seit dem letzten Laden geschrieben haben. Gesperrte Tabellen
        // überspringen die Aktualisierung — table-password holt sie nach dem
        // Entsperren nach.
        if (tbl.Unlocked && !tbl.BeSureToBeUpToDate(false)) {
            Console.Error.WriteLine("Tabelle '" + tbl.KeyName + "' konnte nicht auf den aktuellen Stand gebracht werden (Fragmentspeicher nicht lesbar).");
            tbl.Dispose();
            return null;
        }

        return tbl;
    }

    /// <summary>
    /// Gibt eine geladene Tabelle frei. Zuvor laufen die Datenüberprüfung veralteter Zeilen
    /// und die Verarbeitung explizit invalidierter Zeilen.
    /// In einer Shell-Session bleibt die Instanz im Live-Cache erhalten,
    /// sonst wird sie disposet (Writer wird geschlossen).
    /// </summary>
    protected static void Release(Table tbl) {
        if (!SessionActive) {
            RowCollection.ExecuteValueChangedEvent();
            RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
            tbl.Dispose();
        }
    }

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
    /// RowAddressingProblem geprüfte Zeilenadressierung.
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