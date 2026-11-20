// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text;

namespace TableStress;

internal static class Program {

    #region Methods

    [STAThread]
    private static int Main(string[] args) {
        try {
            Console.OutputEncoding = new UTF8Encoding(false);
        } catch {
            // Ohne nutzbare Konsole (z. B. geerbte Handles) die Standardausgabe belassen.
        }

        // Watchdog-Modus: gleiche exe, eigener Prozess.
        if (args.Length == 3 && args[0].Equals("--watchdog", StringComparison.OrdinalIgnoreCase)) {
            return Watchdog.RunAsWatchdog(args[1], args[2]);
        }

        if (args.Length != 2 || !int.TryParse(args[1], out var userNummer)) {
            Console.Error.WriteLine("Benutzung: TableStress <pfad-zur-tabelle> <usernummer>");
            return 2;
        }

        var tablePath = args[0];

        Develop.StartService();

        var user = "B" + userNummer;
        Generic.UserName = user;
        RowCollection.AllowProcessingAborts = false;

        // Protokoll umschreiben und in den Ordner der Tabelle leiten: Benutzer-Uhrzeit.txt.
        using var prot = StressProtocol.Create(tablePath, user);
        prot.Write("START", $"Benutzer={user} Tabelle={tablePath} PID={Environment.ProcessId} EXE={Environment.ProcessPath}");

        var watchdog = Watchdog.Start(Environment.ProcessId, prot.FullPath);
        prot.Write("START", watchdog is null ? "Watchdog konnte nicht gestartet werden" : $"Watchdog gestartet PID={watchdog.Id}");

        try {
            var tbl = Table.Get(tablePath);

            if (tbl is not { IsDisposed: false }) {
                prot.Write("FEHLER", $"Tabelle nicht gefunden oder unbrauchbar: {tablePath}");
                return 1;
            }

            if (!tbl.BeSureToBeUpToDate(false)) {
                prot.Write("FEHLER", $"Tabelle {tbl.KeyName} konnte nicht auf den aktuellen Stand gebracht werden");
                tbl.Dispose();
                return 1;
            }

            var runner = new StressRunner(tbl, prot, user);
            return runner.Run();
        } catch (Exception ex) {
            prot.Write("FEHLER", $"Fatal Typ={ex.GetType().Name} Meldung={ex.Message} Aufrufstack={Develop.DiagStack()}");
            return 1;
        }
    }

    #endregion
}
