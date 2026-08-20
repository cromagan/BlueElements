// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class HelpCliCommand : CliCommand {

    #region Properties

    public override string Command => "help";
    public override string Description => "Zeigt alle Befehle oder die Details eines Befehls an.";
    public override string Syntax => "bcr help [befehl]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount == 0) {
            foreach (var command in CliCommand.All) {
                Console.Out.WriteLine(command.Command + ": " + command.Description);
            }

            return 0;
        }

        var cmd = CliCommand.ByName(args[0] ?? string.Empty);

        if (cmd is null) {
            Console.Error.WriteLine("Unbekannter Befehl: " + args[0]);
            return 2;
        }

        Console.Out.WriteLine("Befehl: " + cmd.Command);
        Console.Out.WriteLine("Syntax: " + cmd.Syntax);
        Console.Out.WriteLine("Beschreibung: " + cmd.Description);

        if (cmd.Flags.Count > 0) {
            Console.Out.WriteLine("Schalter: " + string.Join(", ", cmd.Flags.Select(f => "--" + f)));
        }

        return 0;
    }

    #endregion
}
