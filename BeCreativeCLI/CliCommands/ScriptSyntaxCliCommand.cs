// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.ScriptCommands;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Skripte: Listet die Syntax aller Skript-Befehle auf; optional auf Treffer des Filters eingeschränkt.
/// </summary>
public class ScriptSyntaxCliCommand : CliCommand {

    #region Properties

    public override string Command => "script-syntax";
    public override string Syntax => "bcr script-syntax [filter]";

    public override string? HelpDetails =>
            "Der Filter ist ein Teiltext (Groß-/Kleinschreibung egal), z. B. 'bcr script-syntax cell' für alle Zell-Befehle.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount > 1) {
            return UsageError($"Erwartet wird maximal 1 Positionsargument (<filter>), erhalten: {args.PositionalCount}.");
        }

        var filter = args.PositionalCount == 1 ? args[0] ?? string.Empty : string.Empty;

        foreach (var syntax in ScriptCommand.AllMethods.Instances
                     .Select(m => m.Syntax)
                     .Where(s => s.Contains(filter, StringComparison.OrdinalIgnoreCase))
                     .SortedDistinctList()) {
            Console.Out.WriteLine(syntax);
        }

        return 0;
    }

    #endregion
}
