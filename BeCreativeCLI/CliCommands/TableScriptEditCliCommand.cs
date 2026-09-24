// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Skripte: Erstellt oder überschreibt ein Tabellen-Skript mit dem Inhalt einer Textdatei (nur mit dem CLI-Recht 'Edit script').
/// </summary>
public class TableScriptEditCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-scriptedit";
    public override List<string> Options => ["name", "file", "password"];
    public override string Syntax => "bcr table-scriptedit <tabelle> --name <skript> --file <datei>";

    public override string? HelpDetails =>
            "Legt das Skript an, falls der Name noch nicht existiert, und ersetzt den Skripttext komplett durch den Dateiinhalt. " +
            "Enthält das neue Skript Syntax-Fehler, wird nichts geändert.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        if (!args.HasOption("name")) { return UsageError("Es fehlt die Option --name <skript>."); }

        if (!args.HasOption("file")) { return UsageError("Es fehlt die Option --file <datei>."); }

        var file = args.Option("file") ?? string.Empty;

        if (!FileExists(file)) {
            Console.Error.WriteLine("Datei nicht gefunden: " + file);
            return 1;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        var rightProblem = RightProblem(tbl, CliRights.EditScript);

        if (rightProblem is not null) {
            Console.Error.WriteLine(rightProblem);
            return 1;
        }

        var scriptText = ReadAllText(file);

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

        var name = args.Option("name") ?? string.Empty;
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

    #endregion
}
