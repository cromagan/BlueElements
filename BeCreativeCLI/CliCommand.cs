// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;
using System.Text.Json.Nodes;

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

    /// <summary>
    /// Dateiendungen, die als Tabelle geladen werden dürfen — oder der Name ganz ohne Endung.
    /// </summary>
    private static readonly HashSet<string> LoadableTableSuffixes = new(StringComparer.OrdinalIgnoreCase) { "bdb", "mbdb", "tblh", "tblj", "mtblj" };

    #endregion

    #region Properties

    public static List<CliCommand> All => [.. Cache.Instances.OrderBy(c => c.Command)];

    public abstract string Command { get; }

    /// <summary>
    /// Schalter (Optionen ohne Wert), die dieser Befehl kennt.
    /// Alle übrigen "--"-Optionen erwarten einen Wert.
    /// </summary>
    public virtual List<string> Flags => [];

    /// <summary>
    /// Ergänzender Hilfetext (Quoting, Beispiele), angezeigt von 'bcr help &lt;befehl&gt;'.
    /// </summary>
    public virtual string? HelpDetails => null;

    string IHasKeyName.KeyName => Command.ToUpperInvariant();

    /// <summary>
    /// Optionen mit Wert, die dieser Befehl kennt. Alle übrigen "--"-Optionen
    /// gelten als unbekannt und erzeugen einen ParseError.
    /// </summary>
    public virtual List<string> Options => [];

    public abstract string Syntax { get; }

    /// <summary>
    /// Optionen der Zeilenadressierung, die alle zeilenadressierenden Befehle kennen.
    /// </summary>
    protected static List<string> AddressingOptions => ["rowkey", "filtercolumn", "filtervalue", "filtertype"];

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
    /// Liefert null, wenn die Spalte per CLI beschreibbar ist, ansonsten die Fehlermeldung.
    /// Datenverwaltete Systemspalten sind tabu, da die Datenprüfung ihre Werte selbst führt.
    /// Verknüpfte Spalten (Werte aus anderer Tabelle) leiten Schreibvorgänge in die
    /// Fremdtabelle um und nicht gespeicherte Spalten verlieren Änderungen beim Entladen —
    /// beides meldet CellSet fälschlich als Erfolg. Die CLI lehnt solche Spalten ab.
    /// </summary>
    protected static string? ColumnWriteProblem(ColumnItem column) {
        if (column.IsSystemColumn()) {
            return "Die Spalte '" + column.KeyName + "' wird von der Datenverwaltung geführt und kann nicht per CLI geändert werden.";
        }

        if (column.RelationType == RelationType.CellValues) {
            return "Die Spalte '" + column.KeyName + "' zeigt auf Werte einer anderen Tabelle und kann nicht per CLI geändert werden. Bitte die Zeile in der Quell-Tabelle pflegen.";
        }

        if (!column.SaveContent) {
            return "Die Spalte '" + column.KeyName + "' wird nicht gespeichert (berechnete Spalte) — ein Schreiben per CLI würde nichts bewirken.";
        }

        return null;
    }

    /// <summary>
    /// Liefert null, wenn --filtertype einen bekannten Vergleichstyp enthält,
    /// ansonsten die Fehlerbeschreibung.
    /// </summary>
    protected static string? FilterTypeProblem(CliArgs args) {
        if (!args.HasOption("filtertype")) { return null; }

        switch ((args.Option("filtertype") ?? string.Empty).ToUpperInvariant()) {
            case "EQUALS":
            case "EXACT":
            case "CONTAINS":
            case "STARTSWITH":
                return null;

            default:
                return "Unbekannter --filtertype: '" + args.Option("filtertype") + "'. Gültig: equals, exact, contains, startswith.";
        }
    }

    /// <summary>
    /// Prüft, ob die Tabelle per Kommandozeile bearbeitet werden kann.
    /// Fragment-Tabellen sind Multi-User-Formate: TableFragments (.mbdb) führt automatisch die
    /// jüngste, sauber mit EOF abgeschlossene Fragment-Datei des eigenen Benutzers fort
    /// (jünger als 5 Minuten; neue Änderungen werden per Append angehängt).
    /// Gibt es keine, legt das System beim ersten Schreiben eine neue Fragment-Datei an.
    /// Das Öffnen hängt sofort "- CONTINUED" an, damit kein zweiter Prozess dieselbe
    /// Datei zum Schreiben öffnen kann.
    /// Liefert null, wenn die Bearbeitung erlaubt ist, ansonsten die Fehlermeldung.
    /// </summary>
    protected static string? FragmentEditProblem(Table tbl) {
        if (tbl is not (TableFragments or TableJsonFragments)) { return null; }

        if (tbl is not TableFragments) {
            return "Bearbeitungen an JSON-Fragment-Tabellen (TableJsonFragments) werden von der Kommandozeile nicht unterstützt.";
        }

        // Geeignete Fragment-Datei automatisch erkennen (EOF am Ende, eigener Benutzer,
        // jünger als 5 Minuten); ohne Fund startet der Writer eine neue Datei.
        var fragmentFile = NewestFragmentOf((TableFragments)tbl);

        if (fragmentFile is null) { return null; }

        var f = ((TableFragments)tbl).ContinueFragment(fragmentFile);

        return f is { Length: > 0 } ? f : null;
    }

    /// <summary>
    /// Prüft, ob die Fragment-Datei mit dem EOF-Marker endet: binär die Zeile "- EOF",
    /// JSON ein Objekt mit "_meta": "eof".
    /// </summary>
    protected static bool FragmentEndsAtEof(string filename, string suffix) {
        try {
            using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            if (stream.Length == 0) { return false; }

            // Nur das Dateiende lesen; die erste (evtl. mittendrin beginnende) Zeile liefert der Loop automatisch mit.
            var tailLength = (int)Math.Min(stream.Length, 4096);
            stream.Seek(-tailLength, SeekOrigin.End);

            using var reader = new StreamReader(stream, Encoding.UTF8);
            string? line = null;

            while (reader.ReadLine() is { } l) {
                if (l.Length > 0) { line = l; }
            }

            if (line is null) { return false; }

            if (suffix == "frj") {
                return JsonNode.Parse(line) is JsonObject eof && eof["_meta"]?.GetValue<string>() == "eof";
            }

            return line.TrimEnd() == "- EOF";
        } catch {
            // Unlesbare Datei (z. B. exklusiv gesperrt) überspringen.
            return false;
        }
    }

    /// <summary>
    /// Liest den Benutzernamen aus dem Kopf der Fragment-Datei (erste Zeilen) und vergleicht ihn.
    /// </summary>
    protected static bool FragmentIsMine(string filename, string suffix, string user) {
        try {
            using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            for (var i = 0; i < 10; i++) {
                if (reader.ReadLine() is not { Length: > 0 } line) { break; }

                if (suffix == "frj") {
                    // NDJSON-Steuerzeile: {"_meta":"header","version":...,"filename":...,"user":...}
                    if (JsonNode.Parse(line) is JsonObject header && header["user"] is { } jsonUser) {
                        return jsonUser.GetValue<string>().Equals(user, StringComparison.OrdinalIgnoreCase);
                    }
                } else if (line.StartsWith("- User ", StringComparison.OrdinalIgnoreCase)) {
                    return line["- User ".Length..].Trim().Equals(user, StringComparison.OrdinalIgnoreCase);
                }
            }
        } catch {
            // Unlesbare Datei (z. B. exklusiv gesperrt) überspringen.
        }

        return false;
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
    /// True, wenn Zellinhalte der Spalte als HTML gelesen werden (RichText-Renderer
    /// oder erlaubte Textformatierung) und Sonderzeichen daher als HTML-Entities
    /// gespeichert werden — so legt die App die Werte selbst ab.
    /// </summary>
    protected static bool IsHtmlContent(ColumnItem column) =>
        column.TextFormatingAllowed || column.DefaultRenderer.Equals("RichText", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Lädt die Tabelle aus dem ersten Positionsargument und entsperrt sie bei Bedarf
    /// mit --password. Ohne Pfadangabe wird das aktuelle Verzeichnis als Suchpfad ergänzt.
    /// Gibt bei Problemen (nicht gefunden, falsches Kennwort) eine Fehlermeldung aus und liefert null.
    /// </summary>
    protected static Table? LoadTable(CliArgs args) {
        var tbl = LoadTableIgnoreLock(args);

        if (tbl is null) { return null; }

        if (!tbl.Unlocked) {
            var pwd = args.Option("password");

            if (pwd is null) {
                Console.Error.WriteLine("Tabelle '" + tbl.KeyName + "' ist passwortgeschützt — --password <kennwort> angeben.");
                tbl.Dispose();
                return null;
            }

            if (!string.Equals(pwd, tbl.GlobalShowPass, StringComparison.Ordinal)) {
                Console.Error.WriteLine("Falsches Passwort für Tabelle '" + tbl.KeyName + "'.");
                tbl.Dispose();
                return null;
            }

            tbl.Unlocked = true;
        }

        // Bei JEDEM Aufruf aktualisieren: lädt z. B. Fragmente nach, die andere
        // Prozesse seit dem letzten Laden geschrieben haben.
        if (!tbl.BeSureToBeUpToDate(false)) {
            Console.Error.WriteLine("Tabelle '" + tbl.KeyName + "' konnte nicht auf den aktuellen Stand gebracht werden (Fragmentspeicher nicht lesbar).");
            tbl.Dispose();
            return null;
        }

        return tbl;
    }

    /// <summary>
    /// Lädt die Tabelle aus dem ersten Positionsargument, ohne den Kennwortschutz zu prüfen.
    /// Ohne Pfadangabe wird das aktuelle Verzeichnis als Suchpfad ergänzt.
    /// Gibt bei Problemen (nicht gefunden) eine Fehlermeldung aus und liefert null.
    /// </summary>
    protected static Table? LoadTableIgnoreLock(CliArgs args) {
        if (args[0] is not { Length: > 0 } name) { return null; }

        // Bewusst ablehnen statt stillschweigend eine gleichnamige Tabelle zu öffnen.
        var suffix = name.FileSuffix();

        if (suffix is { Length: > 0 } && !LoadableTableSuffixes.Contains(suffix)) {
            Console.Error.WriteLine("Ungültige Dateiendung '." + suffix + "' — unterstützt werden .bdb, .mbdb, .tblh, .tblj, .mtblj (oder ganz ohne Endung).");
            return null;
        }

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

        return tbl;
    }

    /// <summary>
    /// Liefert die jüngste, sauber mit EOF abgeschlossene Fragment-Datei der Tabelle,
    /// die dieser Benutzer zuletzt genutzt hat — oder null. Nur Dateien jünger als
    /// 5 Minuten gelten.
    /// </summary>
    protected static string? NewestFragmentOf(TableFragments tbl) {
        var suffix = TableFragments.SuffixOfFragments;
        var directory = tbl.Filename.FilePath() + "Frgm\\";
        string? newest = null;
        var newestUtc = DateTime.MinValue;
        var limitUtc = DateTime.UtcNow.AddMinutes(-5);

        foreach (var file in GetFiles(directory, tbl.KeyName.ToUpperInvariant() + "-*." + suffix, SearchOption.TopDirectoryOnly)) {
            var info = GetFileInfo(file);

            if (info is null || info.LastWriteTimeUtc < limitUtc) { continue; }
            if (!FragmentEndsAtEof(file, suffix)) { continue; }
            if (!FragmentIsMine(file, suffix, UserName)) { continue; }
            if (info.LastWriteTimeUtc <= newestUtc) { continue; }

            newestUtc = info.LastWriteTimeUtc;
            newest = file;
        }

        return newest;
    }

    /// <summary>
    /// Gibt eine geladene Tabelle frei. Zuvor laufen die Datenüberprüfung veralteter Zeilen
    /// und die Verarbeitung explizit invalidierter Zeilen.
    /// </summary>
    protected static void Release(Table tbl) {
        RowCollection.ExecuteValueChangedEvent();
        RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);
        tbl.Dispose();
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
    /// RowAddressingProblem geprüfte Zeilenadressierung. Eine leere Liste bedeutet:
    /// keine Zeile getroffen — ob das ein Fehler ist, entscheidet der Aufrufer.
    /// </summary>
    protected static (List<RowItem> Rows, string? Error) ResolveRows(Table tbl, CliArgs args) {
        if (args.HasOption("rowkey")) {
            var key = args.Option("rowkey") ?? string.Empty;
            var row = tbl.Row.GetByKey(key);

            if (row is not null) { return ([row], null); }

            return key.IsLong()
                ? ([], $"Zeile nicht gefunden: {key}")
                : ([], $"Zeilen-Key muss numerisch sein (Zeilen-Keys sind Zeitstempel-artige Longs): {key}");
        }

        var columnName = args.Option("filtercolumn") ?? string.Empty;
        var column = tbl.Column[columnName];

        if (column is null) { return ([], $"Spalte nicht gefunden: {columnName}"); }

        var rows = FilterCollection.CalculateFilteredRows(tbl, false, new FilterItem(column, GetFilterType(args), args.Option("filtervalue") ?? string.Empty));
        return (rows, null);
    }

    /// <summary>
    /// Prüft die Zeilenadressierung (--rowkey oder --filtercolumn/--filtervalue)
    /// und den Vergleichstyp --filtertype. Liefert null, wenn alle Angaben gültig
    /// sind, ansonsten die Fehlerbeschreibung.
    /// </summary>
    protected static string? RowAddressingProblem(CliArgs args) {
        var hasRowKey = args.HasOption("rowkey");
        var hasFilter = args.HasOption("filtercolumn") || args.HasOption("filtervalue");

        if (hasRowKey && hasFilter) { return "--rowkey darf nicht mit --filtercolumn/--filtervalue kombiniert werden."; }

        if (!hasRowKey && !hasFilter) { return "Es muss --rowkey <key> oder --filtercolumn <spalte> mit --filtervalue <wert> angegeben werden."; }

        if (hasRowKey && args.HasOption("filtertype")) { return "--filtertype benötigt --filtercolumn/--filtervalue."; }

        if (hasFilter && !(args.HasOption("filtercolumn") && args.HasOption("filtervalue"))) { return "--filtercolumn und --filtervalue müssen zusammen angegeben werden."; }

        if (args.HasOption("filtertype")) {
            var ftProblem = FilterTypeProblem(args);

            if (ftProblem is not null) { return ftProblem; }
        }

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
    /// Liefert den Zelltext, in dem gesucht wird: Bei Spalten mit HTML-Inhalt
    /// dekodiert (Entities wie &#252; werden zu ü), sonst der Rohtext.
    /// </summary>
    protected static string SearchTextOf(ColumnItem column, string cellValue) =>
        IsHtmlContent(column) ? System.Net.WebUtility.HtmlDecode(cellValue) : cellValue;

    /// <summary>
    /// Überführt einen geänderten Zelltext in das Speicherformat: Bei Spalten mit
    /// HTML-Inhalt werden Sonderzeichen als HTML-Entities codiert (so speichert
    /// die App selbst), sonst bleibt es beim Rohtext.
    /// </summary>
    protected static string StorageTextOf(ColumnItem column, string text) =>
        IsHtmlContent(column) ? text.CreateHtmlCodes() : text;

    /// <summary>
    /// Gibt eine konkrete Fehlerursache plus die Syntaxzeile aus. Exit-Code 2.
    /// </summary>
    protected int UsageError(string message) {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine("Syntax: " + Syntax);
        return 2;
    }

    #endregion
}