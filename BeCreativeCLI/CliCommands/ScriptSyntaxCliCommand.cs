// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.ScriptCommands;
using BlueScript.ScriptVariables;

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

        Console.Out.WriteLine("Hinweis: Skripte unterstützen keine selbst erstellten Funktionen.");
        Console.Out.WriteLine();

        foreach (var syntax in ScriptCommand.AllMethods.Instances
                     .Select(m => m.Syntax)
                     .Where(s => s.Contains(filter, StringComparison.OrdinalIgnoreCase))
                     .SortedDistinctList()) {
            Console.Out.WriteLine(syntax);
        }

        Console.Out.WriteLine();
        Console.Out.WriteLine("Variablen erstellen:");
        Console.Out.WriteLine();

        foreach (var v in ScriptVariable.VarTypes.Instances
                     .Where(v => v.InitializationSample is { Length: > 0 })
                     .Where(v => v.InitializationSample.Contains(filter, StringComparison.OrdinalIgnoreCase)
                             || v.MyClassId.Contains(filter, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(v => v.MyClassId)) {
            Console.Out.WriteLine(v.InitializationSample + "  (" + v.MyClassId + ")");
        }

        return 0;
    }

    #endregion
}
