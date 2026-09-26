// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

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
            "execute führt das Skript aus (Zeilen-Skripte mit --rowkey). " +
            "check prüft das Skript in der Tabelle und meldet den Fehler. " +
            "names listet alle Skripte mit ihrem Compare-Ergebnis. " +
            "Alle Aktionen außer execute verlangen das CLI-Recht 'Edit script', execute 'Execute script'.";

    public override List<string> Options => ["rowkey", "password"];
    public override string Syntax => "bcr table-script <tabelle> <import|export|compare|execute|check|names> [<skript>]";

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
        var rightProblem = RightProblem(tbl, action == "EXECUTE" ? CliRights.ExecuteScript : CliRights.EditScript);

        if (rightProblem is not null) {
            Console.Error.WriteLine(rightProblem);
            return 1;
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
    /// Datei eines Skripts im Ordner der Tabelle: &lt;tabelle&gt;.&lt;skript&gt;.txt.
    /// </summary>
    private static string ScriptFile(TableFile tbl, string scriptName) =>
        tbl.Filename.FilePath() + tbl.Filename.FileNameWithoutSuffix() + "." + scriptName.ToNonCritical() + ".txt";

    /// <summary>
    /// Normalisiert einen Skripttext (CRLF zu CR, nachlaufender Leerraum verworfen),
    /// damit gespeicherter Text und Datei-Inhalt symmetrisch vergleichbar sind.
    /// </summary>
    private static string NormalizeScript(string scriptText) => scriptText.Replace("\r\n", "\r").TrimEnd();

    /// <summary>
    /// Liest eine Skript-Datei und normalisiert den Inhalt mit NormalizeScript.
    /// </summary>
    private static string ScriptTextOfFile(string file) => NormalizeScript(ReadAllText(file));

    private int Execute(Table tbl, CliArgs args) {
        var script = GetScriptOrError(tbl, args);

        if (script is null) { return 1; }

        RowItem? row = null;

        if (script.NeedRow) {
            if (!args.HasOption("rowkey")) { return UsageError("Das Skript ist ein Zeilen-Skript — es fehlt --rowkey <key>."); }

            row = tbl.Row.GetByKey(args.Option("rowkey") ?? string.Empty);

            if (row is null) {
                Console.Error.WriteLine("Zeile nicht gefunden: " + args.Option("rowkey"));
                return 1;
            }
        }

        var feedback = tbl.ExecuteScript(script, !script.ValuesReadOnly, row, null, true, true, false);

        if (feedback.Failed) {
            Console.Error.WriteLine("Skript abgebrochen:\r\n" + feedback.ProtocolText);
            return 1;
        }

        Console.Out.WriteLine("Skript ausgeführt: " + script.KeyName);

        // Werte-Änderungen des Skripts speichern; reine Lese-Skripte nicht.
        return script.ValuesReadOnly ? 0 : SaveTable(tbl);
    }

    #endregion
}