// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics.ClassesStatic;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Zeigt alle Befehle oder die Details eines Befehls an: Syntax, Beschreibung, Schalter, Optionen und Beispiele.
/// </summary>
public class HelpCliCommand : CliCommand {

    #region Properties

    public override string Command => "help";
    public override List<string> Flags => ["dev"];
    public override string Syntax => "bcr help [befehl]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount == 0) {
            foreach (var command in All.Where(c => !c.Hidden || args.Flag("dev"))) {
                Console.Out.WriteLine(command.Command + ": " + Generic.Summary(command.GetType()));
            }

            return 0;
        }

        var cmd = ByName(args[0] ?? string.Empty);

        if (cmd is null || (cmd.Hidden && !args.Flag("dev"))) {
            Console.Error.WriteLine("Unbekannter Befehl: " + args[0]);
            return 2;
        }

        Console.Out.WriteLine("Befehl: " + cmd.Command);
        Console.Out.WriteLine("Syntax: " + cmd.Syntax);
        Console.Out.WriteLine("Beschreibung: " + Generic.Summary(cmd.GetType()));

        var flags = cmd.Flags.Where(f => !f.Equals("dev", StringComparison.OrdinalIgnoreCase)).ToList();

        if (flags.Count > 0) {
            Console.Out.WriteLine("Schalter: " + string.Join(", ", flags.Select(f => "--" + f)));
        }

        if (cmd.Options.Count > 0) {
            Console.Out.WriteLine("Optionen: " + string.Join(", ", cmd.Options.Select(o => "--" + o + " <wert>")));
        }

        if (cmd.HelpDetails is { Length: > 0 } details) {
            Console.Out.WriteLine();
            Console.Out.WriteLine(details);
        }

        return 0;
    }

    #endregion
}