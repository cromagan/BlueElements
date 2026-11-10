// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueDeskSnap.Forms;
using System.Text.Json.Nodes;
using FormWindowState = System.Windows.Forms.FormWindowState;

namespace BlueDeskSnap.Snap;

/// <summary>
/// Erstellt Desktop-Snaps: sichtbare Fenster erfassen, Zustand sichern und wiederherstellen.
/// </summary>
public static class DesktopSnapshot {

    #region Methods

    /// <summary>
    /// Erfasst alle sichtbaren, nicht minimierten Fenster (das Snap-Fenster selbst ausgenommen) und speichert alles ab.
    /// </summary>
    public static WindowSnapshot Capture(string name) {
        Develop.Diagnose("Snap", "Capture gestartet: " + name);

        var folder = SnapshotStore.NewSnapshotFolder(name);
        var snap = new WindowSnapshot { Name = name, Created = DateTime.Now, Folder = folder };
        Develop.Diagnose("Snap", "Ordner erstellt: " + folder);

        var windows = DesktopWindows();
        Develop.Diagnose("Snap", "Desktop-Fenster: " + windows.Count);

        var snapHandles = SnapFormHandles();

        foreach (var window in windows) {
            if (snapHandles.Contains(window.MainWindowHandle)) { continue; }

            if (window.MainWindowTitle.Length == 0) { continue; }

            if (WindowsRemoteControl.WinState(window.MainWindowHandle) == FormWindowState.Minimized) { continue; }

            Develop.Diagnose("Snap", "Fenster wird erfasst: " + window.Klasse + " | " + window.MainWindowTitle);

            try {
                var processName = ProcessNameOf(window.Prid);
                var plugin = WindowPlugin.FindFor(window.Klasse, processName);

                var info = new WindowInfo {
                    Title = window.MainWindowTitle,
                    ClassName = window.Klasse,
                    ProcessName = processName,
                    Bounds = WindowsRemoteControl.WinBounds(window.MainWindowHandle),
                    State = WindowsRemoteControl.WinState(window.MainWindowHandle),
                    PluginKeyName = plugin.KeyName
                };

                info.Documents.AddRange(plugin.CaptureDocuments(window.MainWindowHandle));
                snap.Windows.Add(info);
                Develop.Diagnose("Snap", "Fenster erfasst: " + info.PluginKeyName + " | Dokumente: " + info.Documents.Count);
            } catch (Exception ex) {
                // Ein widerspenstiges Fenster darf den kompletten Snap nicht abbrechen
                Develop.Diagnose("Snap", "Fenster fehlgeschlagen: " + ex.GetType().Name + " - " + ex.Message);
                Develop.DebugPrint("Snap: Fenster übersprungen: " + window.MainWindowTitle, ex);
            }
        }

        foreach (var plugin in QualityPlugin.AllPlugins.Instances) {
            if (plugin.IsRunning) { snap.ActivePlugins.Add(plugin.KeyName); }
            snap.PluginSettings.Add(new QualityPluginState(plugin.KeyName, plugin.SettingsOf()));
        }

        SnapshotStore.Save(snap);
        Develop.Diagnose("Snap", "Snapshot gespeichert: " + snap.Windows.Count + " Fenster");

        return snap;
    }

    /// <summary>
    /// Stellt den Snapshot wieder her: alle dem Snap unbekannten Fenster werden minimiert,
    /// die gesicherten Fenster bekommen Zustand, Position und Z-Ordnung zurück.
    /// </summary>
    public static void Restore(WindowSnapshot snapshot) {
        foreach (var plugin in QualityPlugin.AllPlugins.Instances) {
            plugin.IsRunning = snapshot.ActivePlugins.Contains(plugin.KeyName);

            if (snapshot.PluginSettings.FirstOrDefault(s => string.Equals(s.KeyName, plugin.KeyName, StringComparison.OrdinalIgnoreCase)) is { } state) {
                plugin.ApplySettings(state.Settings ?? new JsonObject());
            }
        }

        var snapHandles = SnapFormHandles();

        // Das Snap-Fenster selbst nie zuordnen - Position und Zustand bleiben unangetastet
        var pool = DesktopWindows();
        pool.RemoveAll(w => snapHandles.Contains(w.MainWindowHandle));
        var processNames = new Dictionary<int, string>();
        var assignments = new List<(WindowInfo Info, IntPtr Handle)>();

        // Allen gesicherten Fenstern ein aktuelles Fenster zuordnen (vorne zuerst):
        // erst das Original mit gleicher Fensterklasse, dann generisch irgendein Fenster desselben Prozesses.
        foreach (var info in snapshot.Windows) {
            var window = FindWindow(pool, processNames, info, true) ?? FindWindow(pool, processNames, info, false);

            if (window is not { } match) { continue; }

            _ = pool.Remove(match);
            assignments.Add((info, match.MainWindowHandle));
        }

        // Alles, was dem Snap unbekannt ist, minimieren - das Snap-Fenster selbst, Shell- und titellose Fenster bleiben unberührt
        foreach (var window in DesktopWindows()) {
            if (assignments.Exists(a => a.Handle == window.MainWindowHandle)) { continue; }
            if (snapHandles.Contains(window.MainWindowHandle)) { continue; }
            if (window.MainWindowTitle.Length == 0) { continue; }
            if (ShellClasses.Contains(window.Klasse)) { continue; }
            if (WindowsRemoteControl.WinState(window.MainWindowHandle) == FormWindowState.Minimized) { continue; }

            Develop.Diagnose("Snap", "Unbekanntes Fenster minimiert: " + window.Klasse + " | " + window.MainWindowTitle);
            WindowsRemoteControl.WinMinimize(window.MainWindowHandle);
        }

        // Von hinten nach vorne wiederherstellen, damit die ursprüngliche Z-Ordnung erhalten bleibt
        for (var i = assignments.Count - 1; i >= 0; i--) {
            var (info, handle) = assignments[i];

            if (info.State == FormWindowState.Minimized) {
                WindowsRemoteControl.WinMinimize(handle);
                continue;
            }

            Develop.Diagnose("Snap", "Restore Fenster: " + info.ProcessName + " | " + info.Title);
            WindowsRemoteControl.WinPlace(handle, info.Bounds, info.State == FormWindowState.Maximized);
        }

        var assignedHandles = assignments.Select(a => a.Handle).ToHashSet();

        // Fehlende Fenster über die Dokumente neu starten
        foreach (var info in snapshot.Windows) {
            if (assignments.Exists(a => a.Info == info)) { continue; }

            if (info.Documents is not { Count: > 0 }) {
                Develop.Diagnose("Snap", "Restore: Fenster nicht gefunden: " + info.ProcessName + " | " + info.Title);
                continue;
            }

            WindowPlugin.FindFor(info.ClassName, info.ProcessName).RestoreDocuments(info.Documents);
            var handle = WaitForWindow(info, processNames, assignedHandles);

            if (handle != IntPtr.Zero) {
                WindowsRemoteControl.WinPlace(handle, info.Bounds, info.State == FormWindowState.Maximized);
            } else {
                Develop.Diagnose("Snap", "Restore: Fenster nicht gestartet: " + info.ProcessName + " | " + info.Title);
            }
        }
    }

    /// <summary>
    /// Sucht in der Fensterliste ein Fenster des Prozesses - mit gleicher Klasse (das Original) oder generisch.
    /// Bei mehreren Kandidaten entscheidet die geringste Entfernung der gesicherten Position.
    /// Die Prozessnamen werden pro Restore in processNames gecacht (Process.GetProcessById ist teuer).
    /// Der Titel wird bewusst nicht verglichen, da er Dateinamen und Ähnliches enthält.
    /// </summary>
    private static WindowsRemoteControl.StrProcess? FindWindow(List<WindowsRemoteControl.StrProcess> pool, Dictionary<int, string> processNames, WindowInfo info, bool byClass) {
        WindowsRemoteControl.StrProcess? best = null;
        var bestDistance = long.MaxValue;

        foreach (var window in pool) {
            if (!processNames.TryGetValue(window.Prid, out var name)) {
                name = ProcessNameOf(window.Prid);
                processNames.Add(window.Prid, name);
            }

            if (!string.Equals(name, info.ProcessName, StringComparison.OrdinalIgnoreCase)) { continue; }

            if (byClass && !string.Equals(window.Klasse, info.ClassName, StringComparison.Ordinal)) { continue; }

            // Minimierte Fenster melden über WinBounds nur Parkpositionen (-32000) - daher die Normal-Bounds
            var bounds = WindowsRemoteControl.WinState(window.MainWindowHandle) == FormWindowState.Minimized
                ? WindowsRemoteControl.WinNormalBounds(window.MainWindowHandle)
                : WindowsRemoteControl.WinBounds(window.MainWindowHandle);

            var distance = Math.Abs(bounds.Left - info.Bounds.Left) + Math.Abs(bounds.Top - info.Bounds.Top);

            if (distance >= bestDistance) { continue; }

            bestDistance = distance;
            best = window;
        }

        return best;
    }

    private static string ProcessNameOf(int prid) {
        try {
            using var process = Process.GetProcessById(prid);
            return process.ProcessName;
        } catch {
            return string.Empty;
        }
    }

    private static IntPtr WaitForWindow(WindowInfo info, Dictionary<int, string> processNames, HashSet<IntPtr> assignedHandles) {
        for (var i = 0; i < 16; i++) {
            Generic.Pause(0.25, false);

            // Frisch enumerieren, weil das Fenster gerade neu gestartet wurde
            var windows = DesktopWindows();
            windows.RemoveAll(w => assignedHandles.Contains(w.MainWindowHandle));

            var window = FindWindow(windows, processNames, info, true) ?? FindWindow(windows, processNames, info, false);
            if (window is { } match) { return match.MainWindowHandle; }
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Alle Top-Level-Fenster des Desktops: sichtbar oder minimiert, nicht "verhüllt", keine Werkzeugfenster.
    /// </summary>
    private static List<WindowsRemoteControl.StrProcess> DesktopWindows() {
        List<WindowsRemoteControl.StrProcess> result = [];

        foreach (var window in WindowsRemoteControl.Prozesse()) {
            var handle = window.MainWindowHandle;

            if (!WindowsRemoteControl.WinIsVisible(handle)) { continue; }
            if (WindowsRemoteControl.WinIsToolWindow(handle)) { continue; }

            result.Add(window);
        }

        return result;
    }

    /// <summary>
    /// Handles aller offenen DeskSnapForm-Fenster - eigene Fenster werden mitgesteuert, nur das Snap-Fenster selbst nicht.
    /// </summary>
    private static HashSet<IntPtr> SnapFormHandles() {
        HashSet<IntPtr> result = [];

        foreach (var form in System.Windows.Forms.Application.OpenForms) {
            if (form is DeskSnapForm { IsHandleCreated: true } snapForm) { _ = result.Add(snapForm.Handle); }
        }

        return result;
    }

    /// <summary>
    /// Fensterklassen, die zum Desktop gehören und nie angetastet werden dürfen.
    /// </summary>
    private static readonly List<string> ShellClasses = ["Progman", "WorkerW", "Shell_TrayWnd", "Shell_SecondaryTrayWnd"];

    #endregion
}
