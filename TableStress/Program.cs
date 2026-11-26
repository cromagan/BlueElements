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
            return Verify.Run(args[1].Trim(), args.Length == 3 ? args[2].Trim() : null);
        }

        // Setup-Modus: Chunk-Spalte explizit anlegen und sofort speichern — vor einer
        // Stress-Runde, damit alle Benutzer die Spalte beim Start vorfinden.
        if (args.Length == 2 && args[0].Equals("--chunksetup", StringComparison.OrdinalIgnoreCase)) {
            return ChunkSetup(args[1].Trim());
        }

        // Chunk-Modus: --chunk stellt sicher, dass eine Chunk-Spalte existiert,
        // --nochunk sicher, dass keine existiert; ohne Schalter bleibt die
        // Tabelle, wie sie ist.
        var chunkModus = 0;
        List<string> restliche = [];

        foreach (var a in args) {
            if (a.Equals("--chunk", StringComparison.OrdinalIgnoreCase)) {
                chunkModus = 1;
            } else if (a.Equals("--nochunk", StringComparison.OrdinalIgnoreCase)) {
                chunkModus = -1;
            } else {
                restliche.Add(a);
            }
        }

        if (restliche.Count != 2 || !int.TryParse(restliche[1], out var userNummer)) {
            Console.Error.WriteLine("Benutzung: TableStress <pfad-zur-tabelle> <usernummer> [--chunk | --nochunk]");
            return 2;
        }

        var tablePath = restliche[0].Trim();
        if (!Path.IsPathRooted(tablePath)) { tablePath = Path.GetFullPath(tablePath); }

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

            // Chunk-Modus prüfen: Ohne Chunk-Spalte laufen alle Zeilen als
            // Kopf-Fragmente; --nochunk verlangt genau das, --chunk die
            // umgekehrte Phase mit Chunk-Spalte.
            var chunkSpalte = tbl.Column.ChunkValueColumn;
            if (chunkSpalte is not { IsDisposed: false }) { chunkSpalte = tbl.Column["CHUNK"]; }

            if (chunkModus < 0) {
                if (chunkSpalte is { IsDisposed: false } vorhanden) {
                    prot.Write("FEHLER", $"--nochunk verlangt eine Tabelle ohne Chunk-Spalte, gefunden: {vorhanden.KeyName}");
                    tbl.Dispose();
                    return 1;
                }

                prot.Write("CHUNKSPALTE", "Keine Chunk-Spalte vorhanden (--nochunk ok)");
            } else if (chunkModus > 0) {
                if (chunkSpalte is not { IsDisposed: false }) {
                    var neueSpalte = tbl.Column.GenerateAndAdd("CHUNK", "Chunk", null, string.Empty);

                    if (neueSpalte is { IsDisposed: false } chunkSpalteNeu) {
                        chunkSpalteNeu.Value_for_Chunk = ChunkType.ByName;
                        tbl.Column.GetSystems();
                        prot.Write("CHUNKSPALTE", "Spalte CHUNK angelegt (ChunkType.ByName)");
                    } else {
                        prot.Write("FEHLER", "--chunk: Anlegen der Spalte CHUNK fehlgeschlagen");
                        tbl.Dispose();
                        return 1;
                    }
                } else {
                    prot.Write("CHUNKSPALTE", $"Chunk-Spalte vorhanden ({chunkSpalte.KeyName}) (--chunk ok)");
                }
            }

            var runner = new StressRunner(tbl, prot, user);
            return runner.Run();
        } catch (Exception ex) {
            prot.Write("FEHLER", $"Fatal Typ={ex.GetType().Name} Meldung={ex.Message} Aufrufstack={Develop.DiagStack()}");
            return 1;
        }
    }

    /// <summary>
    /// Legt die Chunk-Spalte CHUNK (ChunkType.ByName) an und speichert sofort;
    /// existiert sie bereits, wird das gemeldet. Rückgabe ist der Exit-Code.
    /// </summary>
    private static int ChunkSetup(string tablePath) {
        if (!Path.IsPathRooted(tablePath)) { tablePath = Path.GetFullPath(tablePath); }

        Develop.StartService();
        Generic.UserName = "SETUP";
        RowCollection.AllowProcessingAborts = false;

        try {
            var tbl = Table.Get(tablePath);

            if (tbl is not { IsDisposed: false }) {
                Console.Error.WriteLine("Tabelle nicht gefunden oder unbrauchbar: " + tablePath);
                return 1;
            }

            if (!tbl.BeSureToBeUpToDate(false)) {
                Console.Error.WriteLine("Tabelle konnte nicht auf den aktuellen Stand gebracht werden: " + tablePath);
                tbl.Dispose();
                return 1;
            }

            _ = tbl.LoadTableRows(false, -1);

            var chunkSpalte = tbl.Column.ChunkValueColumn;

            if (chunkSpalte is not { IsDisposed: false }) { chunkSpalte = tbl.Column["CHUNK"]; }

            if (chunkSpalte is { IsDisposed: false } vorhanden) {
                Console.Out.WriteLine($"Chunk-Spalte vorhanden: {vorhanden.KeyName} (Value_for_Chunk={vorhanden.Value_for_Chunk})");
            } else {
                var neueSpalte = tbl.Column.GenerateAndAdd("CHUNK", "Chunk", null, string.Empty);

                if (neueSpalte is not { IsDisposed: false } cvc) {
                    Console.Error.WriteLine("Anlegen der Spalte CHUNK fehlgeschlagen");
                    tbl.Dispose();
                    return 1;
                }

                cvc.Value_for_Chunk = ChunkType.ByName;
                tbl.Column.GetSystems();
                Console.Out.WriteLine("Spalte CHUNK angelegt (ChunkType.ByName)");
            }

            RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);

            var opr = tbl is TableFileNew tfn ? tfn.Save() : (tbl as TableFile)?.Save() ?? OperationResult.Success;

            // Hintergrund-Speicher-Task abschließen und dessen Ergebnis ableiten:
            // Bleibt SaveRequired aktiv, konnten (Teil-)Daten nicht gesichert werden.
            switch (tbl) {
                case TableFileNew tfnw:
                    tfnw.Saveable.WaitDiskOperationFinished();

                    if (opr.IsSuccessful && tfnw.Saveable.SaveRequired) {
                        // Fremde Edit-Sperren (z. B. frische Chunk-Dateien der Vorrunde)
                        // halten das Chunk-Speichern bewusst offen — die Änderungen
                        // liegen sicher im Kopf-Journal und werden später nachgezogen.
                        var sperren = tfnw.Saveable.ForeignLockedChunkIds();

                        if (sperren.Count > 0) {
                            Console.Out.WriteLine($"SaveRequired bleibt aktiv (fremde Edit-Sperre: {string.Join(",", sperren)}) — ok, Daten im Kopf-Journal");
                        } else {
                            opr = OperationResult.Failed("Speichern unvollständig: Es bleiben Änderungen ausständig");
                        }
                    }

                    break;
                case TableFile tfw:
                    tfw.WaitDiskOperationFinished();
                    if (opr.IsSuccessful && tfw.SaveRequired) { opr = OperationResult.Failed("Speichern unvollständig: Es bleiben Änderungen ausständig"); }
                    break;
            }

            if (opr.IsFailed) {
                Console.Error.WriteLine("Speichern fehlgeschlagen: " + opr.FailedReason);
                tbl.Dispose();
                return 1;
            }

            Console.Out.WriteLine($"Gespeichert: Zeilen={tbl.Row.Count} Spalten={tbl.Column.Count}");
            tbl.Dispose();
            return 0;
        } catch (Exception ex) {
            Console.Error.WriteLine($"ChunkSetup fehlgeschlagen: {ex.GetType().Name} {ex.Message}");
            return 1;
        }
    }

    #endregion
}
