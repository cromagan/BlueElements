// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;
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

        if (args.Length is 2 or 3 && args[0].Equals("--verify", StringComparison.OrdinalIgnoreCase)) {
            return Verify.Run(args[1], args.Length == 3 ? args[2] : null);
        }

        if (args.Length != 2 || !int.TryParse(args[1], out var userNummer)) {
            Console.Error.WriteLine("Benutzung: TableStress <pfad-zur-tabelle> <usernummer>");
            return 2;
        }

        var tablePath = args[0];

        Develop.StartService();

        // Das von StartService in Temp gestartete Debug-Protokoll beenden.
        Develop.TraceLogging_End();

        var user = "B" + userNummer;
        Generic.UserName = user;
        RowCollection.AllowProcessingAborts = false;
        Develop.DiagFlag = true;

        // Debug-Protokoll neu starten, direkt im Verzeichnis des Stresstests.
        var traceName = $"Trace-{user}-{DateTime.Now:HH-mm-ss}";
        var traceFile = Path.Combine(Develop.AppPath(), traceName + ".html");
        if (File.Exists(traceFile)) { traceFile = Path.Combine(Develop.AppPath(), $"{traceName}-{DateTime.Now:fff}.html"); }
        Develop.TraceLogging_Start(traceFile);

        // Protokoll umschreiben und in den Ordner der Tabelle leiten: Benutzer-Uhrzeit.txt.
        using var prot = StressProtocol.Create(tablePath, user);
        prot.Write("DEBUGLOG", traceFile);
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

            if (tbl.Column.Count == 0) {
                prot.Write("FEHLER", $"Tabelle {tbl.KeyName} hat keine Spalten — Kopie unvollständig, Fragment-Unterordner fehlt vermutlich");
                tbl.Dispose();
                return 1;
            }

            // Chunk-Spalte sicherstellen: Ohne Spalte mit Chunk-Wert landen alle
            // Zeilen im Default-Chunk (_) und die Chunk-Verteilung
            // wird vom Test nicht geübt.
            if (tbl is TableFileNew { Saveable: SaveableFileChunk } && tbl.Column.ChunkValueColumn is not { IsDisposed: false } && tbl.Column["CHUNK"] is not { IsDisposed: false }) {
                var neueSpalte = tbl.Column.GenerateAndAdd("CHUNK", "Chunk", null, string.Empty);

                if (neueSpalte is { IsDisposed: false } chunkSpalte) {
                    chunkSpalte.Value_for_Chunk = ChunkType.ByName;
                    tbl.Column.GetSystems();
                    prot.Write("CHUNKSPALTE", "Spalte CHUNK angelegt (ChunkType.ByName)");
                } else {
                    prot.Write("CHUNKSPALTE", "Anlegen der Spalte CHUNK fehlgeschlagen");
                }
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
