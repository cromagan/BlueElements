// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Diagnostics;
using System.Threading;

namespace TableStress;

/// <summary>
/// Watchdog als eigener Prozess derselben exe: beendet den Stresstest hart nach 11 Minuten und protokolliert das.
/// </summary>
internal static class Watchdog {

    #region Methods

    /// <summary>
    /// Watchdog-Schleife: endet sofort, wenn der Eltern-Prozess endet; nach 6 Minuten wird dieser getötet.
    /// </summary>
    public static int RunAsWatchdog(string parentPidText, string protocolFile) {
        if (!int.TryParse(parentPidText, out var pid)) { return 2; }

        Process? parent;
        try {
            parent = Process.GetProcessById(pid);
        } catch (ArgumentException) {
            // Der Eltern-Prozess existiert nicht mehr.
            return 0;
        }

        var startUtc = DateTime.UtcNow;

        // Laufzeit des Stresstests (10 min) + 1 Minute Spielraum: Der Watchdog schlägt nur zu, wenn das Programm hängt.
        while (DateTime.UtcNow.Subtract(startUtc).TotalMinutes < 11) {
            Thread.Sleep(1000);

            var exited = false;
            try {
                exited = parent.HasExited;
            } catch (InvalidOperationException) {
                return 0;
            }

            if (exited) { return 0; }
        }

        try {
            parent.Kill(true);
            StressProtocol.AppendWatchdog(protocolFile, $"Prozess {pid} nach 11 Minuten getötet.");
            return 0;
        } catch (Exception ex) {
            StressProtocol.AppendWatchdog(protocolFile, $"Prozess {pid} konnte nicht getötet werden: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Startet die eigene exe als Watchdog-Prozess für den gegebenen Eltern-Prozess.
    /// </summary>
    public static Process? Start(int parentPid, string protocolFile) {
        var exe = Environment.ProcessPath;
        if (exe is not { Length: > 0 }) { return null; }

        var pi = new ProcessStartInfo {
            FileName = exe,
            Arguments = $"--watchdog {parentPid} \"{protocolFile}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try {
            return Process.Start(pi);
        } catch (Exception ex) {
            Console.Error.WriteLine("Watchdog konnte nicht gestartet werden: " + ex.Message);
            return null;
        }
    }

    #endregion
}
