// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Zeigt alle Befehle oder die Details eines Befehls an.
/// </summary>
public class HelpCliCommand : CliCommand {

    #region Properties

    public override string Command => "help";
    public override string Syntax => "bcr help [befehl]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount == 0) {
            foreach (var command in All) {
                Console.Out.WriteLine(command.Command + ": " + Generic.Summary(command.GetType()));
            }

            return 0;
        }

        var cmd = ByName(args[0] ?? string.Empty);

        if (cmd is null) {
            Console.Error.WriteLine("Unbekannter Befehl: " + args[0]);
            return 2;
        }

        Console.Out.WriteLine("Befehl: " + cmd.Command);
        Console.Out.WriteLine("Syntax: " + cmd.Syntax);
        Console.Out.WriteLine("Beschreibung: " + Generic.Summary(cmd.GetType()));

        if (cmd.Flags.Count > 0) {
            Console.Out.WriteLine("Schalter: " + string.Join(", ", cmd.Flags.Select(f => "--" + f)));
        }

        return 0;
    }

    #endregion
}
