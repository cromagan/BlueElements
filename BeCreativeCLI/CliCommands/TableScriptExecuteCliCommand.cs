// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Skripte: Führt ein Tabellen-Skript aus (nur mit dem CLI-Recht 'Execute script'). Zeilen-Skripte benötigen --rowkey.
/// </summary>
public class TableScriptExecuteCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-scriptexecute";
    public override List<string> Options => ["name", "rowkey", "password"];
    public override string Syntax => "bcr table-scriptexecute <tabelle> --name <skript> [--rowkey <key>]";

    public override string? HelpDetails =>
            "Führt das Skript im Produktivmodus aus und speichert die Tabelle, wenn das Skript Werte ändern darf. " +
            "Zeilen-Skripte benötigen --rowkey <key>. Vorhandene Skripte listet 'bcr table-scriptexecute <tabelle>' mit fehlerhaftem --name.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        if (!args.HasOption("name")) { return UsageError("Es fehlt die Option --name <skript>."); }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        var rightProblem = RightProblem(tbl, CliRights.ExecuteScript);

        if (rightProblem is not null) {
            Console.Error.WriteLine(rightProblem);
            return 1;
        }

        var name = args.Option("name") ?? string.Empty;
        var script = tbl.EventScript.GetByKey(name);

        if (script is null) {
            Console.Error.WriteLine("Skript nicht gefunden: " + name + ". Vorhandene Skripte: " + string.Join(", ", tbl.EventScript.Select(s => s.KeyName)));
            return 1;
        }

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

        // Werte-Änderungen des Skripts gespeichern; reine Lese-Skripte nicht.
        return script.ValuesReadOnly ? 0 : SaveTable(tbl);
    }

    #endregion
}
