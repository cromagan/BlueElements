// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using BlueScript.EventArgs;
using BlueScript.ScriptCommands;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Skripte: Tabellen-Skripte mit Dateien im Tabellen-Ordner importieren, exportieren, vergleichen, prüfen, ausführen und auflisten.
/// </summary>
public class TableScriptCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-script";

    public override string? HelpDetails =>
            "Aktionen, alle mit derselben Syntax (<tabelle> und <skript>; nur names braucht kein <skript>): " +
            "import übernimmt den Inhalt der Datei <tabelle>.<skript>.txt aus dem Ordner der Tabelle in das Skript " +
            "(legt es an, falls neu; bei Syntax-Fehlern wird nichts geändert). " +
            "export schreibt den Skripttext zurück in diese Datei. " +
            "compare vergleicht Datei und Skript und meldet 'Identisch' oder 'Nicht identisch' (Exit-Code 1 bei Abweichung). " +
            "execute führt das Skript aus (Zeilen-Skripte per --rowkey oder --filtercolumn/--filtervalue [--filtertype]; ein Filter führt das Skript je getroffener Zeile aus und läuft bei Fehlern weiter). " +
            "check prüft das Skript in der Tabelle und meldet den Fehler. " +
            "names listet alle Skripte mit ihrem Compare-Ergebnis. " +
            "Execute akzeptiert --debugoutput: DebugPrint-Ausgaben des Skripts erscheinen live auf stdout. " +
            "Ansehen und Fehlertesten (export, compare, check, names) ist ohne CLI-Recht erlaubt. " +
            "Import verlangt das CLI-Recht 'Edit script', execute 'Execute script'.";

    public override List<string> Flags => ["debugoutput"];
    public override List<string> Options => [.. AddressingOptions, "password"];
    public override string Syntax => "bcr table-script <tabelle> <import|export|compare|execute|check|names> [<skript>] (bei execute zusätzlich: --rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>]) [--password <kennwort>]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        var action = (args[1] ?? string.Empty).ToUpperInvariant();

        switch (action) {
            case "IMPORT":
            case "EXPORT":
            case "COMPARE":
            case "EXECUTE":
            case "CHECK":
                if (args.PositionalCount != 3) {
                    return UsageError($"Erwartet werden genau 3 Positionsargumente (<tabelle> <aktion> <skript>), erhalten: {args.PositionalCount}.");
                }
                break;

            case "NAMES":
                if (args.PositionalCount != 2) {
                    return UsageError($"Die Aktion 'names' erwartet genau 2 Positionsargumente (<tabelle> <aktion>), erhalten: {args.PositionalCount}.");
                }
                break;

            default:
                return UsageError("Unbekannte Aktion '" + args[1] + "'. Gültig: import, export, compare, execute, check, names.");
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        // Ansehen und Fehlertesten (export, compare, check, names) sind ohne Recht erlaubt.
        string? neededRight = null;

        switch (action) {
            case "IMPORT":
                neededRight = CliRights.EditScript;
                break;
            case "EXECUTE":
                neededRight = CliRights.ExecuteScript;
                break;
        }

        if (neededRight is not null) {
            var rightProblem = RightProblem(tbl, neededRight);
            if (rightProblem is not null) {
                Console.Error.WriteLine(rightProblem);
                return 1;
            }
        }

        if (action == "CHECK") { return Check(tbl, args); }

        if (action != "EXECUTE") {
            if (tbl is not TableFile fileTable || fileTable.Filename is not { Length: > 0 }) {
                Console.Error.WriteLine("Die Tabelle ist nicht dateibasiert — Skript-Dateien sind nicht verfügbar.");
                return 1;
            }

            switch (action) {
                case "IMPORT":
                    return Import(fileTable, args);

                case "EXPORT":
                    return Export(fileTable, args);

                case "COMPARE":
                    return Compare(fileTable, args);

                default:
                    return Names(fileTable);
            }
        }

        return Execute(tbl, args);
    }

    private static int Check(Table tbl, CliArgs args) {
        var script = GetScriptOrError(tbl, args);

        if (script is null) { return 1; }

        if (script.ErrorReason() is { Length: > 0 } reason) {
            Console.Error.WriteLine(script.KeyName + ": " + reason);
            return 1;
        }

        var precheck = ScriptPreCheck.Check(script.Script);

        if (precheck.HasSyntaxErrors) {
            Console.Error.WriteLine("Das Skript enthält Syntax-Fehler:");
            foreach (var error in precheck.SyntaxErrors) {
                Console.Error.WriteLine(error);
            }
            return 1;
        }

        Console.Out.WriteLine("Keine Fehler gefunden: " + script.KeyName);
        return 0;
    }

    private static int Compare(TableFile tbl, CliArgs args) {
        var script = GetScriptOrError(tbl, args);

        if (script is null) { return 1; }

        var file = ScriptFile(tbl, script.KeyName);

        if (!FileExists(file)) {
            Console.Error.WriteLine("Datei nicht gefunden: " + file);
            return 1;
        }

        var identical = NormalizeScript(script.Script) == ScriptTextOfFile(file);

        Console.Out.WriteLine(identical ? "Identisch" : "Nicht identisch");
        return identical ? 0 : 1;
    }

    /// <summary>
    /// Liefert das Compare-Ergebnis des Skripts gegen seine Datei im Tabellen-Ordner.
    /// </summary>
    private static string CompareStateOf(TableFile tbl, TableScriptDescription script) {
        var file = ScriptFile(tbl, script.KeyName);

        if (!FileExists(file)) { return "Keine Datei"; }

        return ScriptTextOfFile(file) == NormalizeScript(script.Script) ? "Identisch" : "Nicht identisch";
    }

    private static int Export(TableFile tbl, CliArgs args) {
        var script = GetScriptOrError(tbl, args);

        if (script is null) { return 1; }

        var file = ScriptFile(tbl, script.KeyName);
        var saved = WriteAllText(file, script.Script, Encoding.UTF8, false);

        if (saved.IsFailed) {
            Console.Error.WriteLine(saved.FailedReason);
            return 1;
        }

        Console.Out.WriteLine("Skript exportiert: " + file);

        // Hinweis, wenn die Datei ohne Recht zum Rückimport exportiert wurde.
        if (!HasRight(tbl, CliRights.EditScript)) {
            Console.Out.WriteLine("Hinweis: Das CLI-Recht '" + CliRights.EditScript + "' fehlt — die Datei kann nicht per table-script import zurückgespielt werden.");
        }

        return 0;
    }

    /// <summary>
    /// Liefert das adressierte Skript oder null nach Fehlerausgabe inkl. vorhandener Skriptnamen.
    /// </summary>
    private static TableScriptDescription? GetScriptOrError(Table tbl, CliArgs args) {
        var name = args[2] ?? string.Empty;
        var script = tbl.EventScript.GetByKey(name);

        if (script is null) {
            Console.Error.WriteLine("Skript nicht gefunden: " + name + ". Vorhandene Skripte: " + string.Join(", ", tbl.EventScript.Select(s => s.KeyName)));
        }

        return script;
    }

    private static int Import(TableFile tbl, CliArgs args) {
        var name = args[2] ?? string.Empty;
        var file = ScriptFile(tbl, name);

        if (!FileExists(file)) {
            Console.Error.WriteLine("Datei nicht gefunden: " + file);
            return 1;
        }

        var scriptText = ScriptTextOfFile(file);

        if (scriptText.Length == 0) {
            Console.Error.WriteLine("Die Datei enthält keinen Skripttext: " + file);
            return 2;
        }

        var precheck = ScriptPreCheck.Check(scriptText);

        if (precheck.HasSyntaxErrors) {
            Console.Error.WriteLine("Das Skript enthält Syntax-Fehler und wurde nicht gespeichert:");
            foreach (var error in precheck.SyntaxErrors) {
                Console.Error.WriteLine(error);
            }
            return 2;
        }

        // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
        // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
        var fragmentProblem = FragmentEditProblem(tbl);

        if (fragmentProblem is not null) {
            Console.Error.WriteLine(fragmentProblem);
            return 2;
        }

        var existing = tbl.EventScript.GetByKey(name);

        if (existing is not { IsDisposed: false } && !ScriptDescription.IsValidName(name)) {
            Console.Error.WriteLine("Ungültiger Skriptname: " + name);
            return 2;
        }

        List<TableScriptDescription> scripts = [.. tbl.EventScript.Where(s => s != existing)];

        TableScriptDescription script;

        if (existing is { IsDisposed: false }) {
            // Kopie des bestehenden Skripts mit neuem Text — die Instanz selbst ist
            // Teil der serialisierten Liste und darf nicht mutiert werden.
            script = new(tbl, existing.ParseableItems().FinishParseable());
            script.Script = scriptText;
            script.FailedReason = string.Empty;
        } else {
            script = new(tbl, name, scriptText);
        }

        scripts.Add(script);
        tbl.EventScript = new(scripts);

        Console.Out.WriteLine("Skript gespeichert: " + script.KeyName);
        return SaveTable(tbl);
    }

    private static int Names(TableFile tbl) {
        if (tbl.EventScript.Count == 0) {
            Console.Out.WriteLine("Keine Skripte vorhanden.");
            return 0;
        }

        foreach (var script in tbl.EventScript.OrderBy(s => s.KeyName, StringComparer.OrdinalIgnoreCase)) {
            Console.Out.WriteLine(script.KeyName + ": " + CompareStateOf(tbl, script));
        }

        return 0;
    }

    /// <summary>
    /// Normalisiert einen Skripttext (CRLF zu CR, nachlaufender Leerraum verworfen),
    /// damit gespeicherter Text und Datei-Inhalt symmetrisch vergleichbar sind.
    /// </summary>
    private static string NormalizeScript(string scriptText) => scriptText.Replace("\r\n", "\r").TrimEnd();

    /// <summary>
    /// Datei eines Skripts im Ordner der Tabelle: &lt;tabelle&gt;.&lt;skript&gt;.txt.
    /// </summary>
    private static string ScriptFile(TableFile tbl, string scriptName) =>
        tbl.Filename.FilePath() + tbl.Filename.FileNameWithoutSuffix() + "." + scriptName.ToNonCritical() + ".txt";

    /// <summary>
    /// Liest eine Skript-Datei und normalisiert den Inhalt mit NormalizeScript.
    /// </summary>
    private static string ScriptTextOfFile(string file) => NormalizeScript(ReadAllText(file));

    /// <summary>
    /// Gibt eine DebugPrint-Zeile des laufenden Skripts auf stdout aus. Wird auch von table-refresh genutzt.
    /// </summary>
    internal static void DebugPrint_LineAdded(object? sender, TextEventArgs e) => Console.Out.WriteLine("DebugPrint: " + e.Text);

    private int Execute(Table tbl, CliArgs args) {
        var script = GetScriptOrError(tbl, args);

        if (script is null) { return 1; }

        var hasRowKey = args.HasOption("rowkey");
        var hasFilter = args.HasOption("filtercolumn") || args.HasOption("filtervalue");

        if (!script.NeedRow) {
            return hasRowKey || hasFilter
                ? UsageError("Das Skript ist ein Tabellen-Skript — keine Zeilenadressierung (--rowkey, --filtercolumn/--filtervalue) erforderlich.")
                : ExecuteOnce(tbl, script, args, null);
        }

        if (!hasRowKey && !hasFilter) {
            return UsageError("Das Skript ist ein Zeilen-Skript — Zeilenadressierung fehlt: --rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert>.");
        }

        var addressingProblem = RowAddressingProblem(args);

        if (addressingProblem is not null) {
            Console.Error.WriteLine(addressingProblem);
            return 2;
        }

        if (hasRowKey) {
            var row = tbl.Row.GetByKey(args.Option("rowkey") ?? string.Empty);

            if (row is null) {
                Console.Error.WriteLine("Zeile nicht gefunden: " + args.Option("rowkey"));
                return 1;
            }

            return ExecuteOnce(tbl, script, args, row);
        }

        var (rows, error) = ResolveRows(tbl, args);

        if (error is not null) {
            Console.Error.WriteLine(error);
            return 1;
        }

        if (rows.Count == 0) {
            Console.Error.WriteLine("Keine Zeile getroffen.");
            return 1;
        }

        return ExecuteBatch(tbl, script, args, rows);
    }

    private static int ExecuteOnce(Table tbl, TableScriptDescription script, CliArgs args, RowItem? row) {
        var debugOutput = args.Flag("debugoutput");

        if (debugOutput) { DebugPrintScriptCommand.LineAdded += DebugPrint_LineAdded; }

        ScriptEndedFeedback feedback;

        try {
            feedback = tbl.ExecuteScript(script, !script.ValuesReadOnly, row, null, true, true, false);
        } finally {
            if (debugOutput) { DebugPrintScriptCommand.LineAdded -= DebugPrint_LineAdded; }
        }

        if (feedback.Failed) {
            Console.Error.WriteLine("Skript abgebrochen:\r\n" + feedback.ProtocolText);
            return 1;
        }

        Console.Out.WriteLine("Skript ausgeführt: " + script.KeyName);

        // Werte-Änderungen des Skripts speichern; reine Lese-Skripte nicht.
        return script.ValuesReadOnly ? 0 : SaveTable(tbl);
    }

    /// <summary>
    /// Führt das Skript je adressierter Zeile aus, läuft bei Fehlern weiter und
    /// speichert einmal am Ende, wenn mindestens ein Lauf erfolgreich war.
    /// </summary>
    private static int ExecuteBatch(Table tbl, TableScriptDescription script, CliArgs args, List<RowItem> rows) {
        var debugOutput = args.Flag("debugoutput");

        if (debugOutput) { DebugPrintScriptCommand.LineAdded += DebugPrint_LineAdded; }

        var done = 0;

        try {
            foreach (var row in rows) {
                var feedback = tbl.ExecuteScript(script, !script.ValuesReadOnly, row, null, true, true, false);

                if (feedback.Failed) {
                    Console.Error.WriteLine($"Zeile {row.KeyName} — Skript abgebrochen:\r\n" + feedback.ProtocolText);
                } else {
                    Console.Out.WriteLine($"Skript ausgeführt in {row.KeyName}: " + script.KeyName);
                    done++;
                }
            }
        } finally {
            if (debugOutput) { DebugPrintScriptCommand.LineAdded -= DebugPrint_LineAdded; }
        }

        if (done == 0) { return 1; }

        // Werte-Änderungen des Skripts speichern; reine Lese-Skripte nicht.
        return script.ValuesReadOnly ? 0 : SaveTable(tbl);
    }

    #endregion
}