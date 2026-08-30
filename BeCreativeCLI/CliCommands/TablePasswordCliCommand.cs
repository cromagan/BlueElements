// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class TablePasswordCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-password";
    public override string Description => "Tabellen: Lädt eine passwortgeschützte Tabelle in die Session und entsperrt sie mit dem Passwort. Nur innerhalb einer Shell-Session verfügbar.";
    public override string Syntax => "bcr table-password <tabelle> --password <kennwort>";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (!SessionActive) {
            Console.Error.WriteLine("Dieser Befehl ist nur innerhalb einer Shell-Session verfügbar: bcr shell");
            return 2;
        }

        if (args.PositionalCount != 1 || !args.HasOption("password")) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var tbl = LoadTableIgnoreLock(args);

        if (tbl is null) { return 1; }

        if (tbl.Unlocked) {
            Console.Error.WriteLine("Tabelle '" + tbl.KeyName + "' ist nicht passwortgeschützt.");
            Release(tbl);
            return 0;
        }

        var password = args.Option("password") ?? string.Empty;

        if (!string.Equals(password, tbl.GlobalShowPass, StringComparison.Ordinal)) {
            Console.Error.WriteLine("Falsches Passwort für Tabelle '" + tbl.KeyName + "'.");
            tbl.Dispose();
            return 1;
        }

        tbl.Unlocked = true;

        // Wie bei jedem Aufruf: Daten auf den aktuellen Stand bringen.
        if (!tbl.BeSureToBeUpToDate(false)) {
            Console.Error.WriteLine("Tabelle '" + tbl.KeyName + "' konnte nach dem Entsperren nicht aktualisiert werden.");
            tbl.Dispose();
            return 1;
        }

        Console.Out.WriteLine("Tabelle '" + tbl.KeyName + "' entsperrt und in der Session geladen.");
        return 0;
    }

    #endregion
}
