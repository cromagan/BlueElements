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
    /// Erfasst alle sichtbaren, nicht minimierten Fenster (das Snap-Fenster selbst ausgenommen) in einen neuen Snap und speichert die Ablage.
    /// </summary>
    public static WindowSnapshot Capture(string name) => CaptureInto(new WindowSnapshot { Name = name });

    /// <summary>
    /// Erfasst den aktuellen Desktop in den gegebenen Snap (neu anlegen oder aktualisieren) und speichert die Ablage.
    /// </summary>
    public static WindowSnapshot CaptureInto(WindowSnapshot snap) {
        if (snap.Id.Length == 0) { snap.Id = Generic.GetUniqueKey(); }
        snap.Created = DateTime.Now;
        Develop.Diagnose("Snap", "Capture gestartet: " + snap.Name);

        var windows = DesktopWindows();
        Develop.Diagnose("Snap", "Desktop-Fenster: " + windows.Count);

        var snapHandles = SnapFormHandles();

        snap.Windows.Clear();
        snap.ActivePlugins.Clear();
        snap.PluginSettings.Clear();

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
    /// Minimiert alle sichtbaren Fenster - die eigenen Snap-Fenster, Shell- und titellose Fenster ausgenommen.
    /// </summary>
    public static void MinimizeAll() {
        var snapHandles = SnapFormHandles();

        foreach (var window in DesktopWindows()) {
            if (snapHandles.Contains(window.MainWindowHandle)) { continue; }
            if (window.MainWindowTitle.Length == 0) { continue; }
            if (ShellClasses.Contains(window.Klasse)) { continue; }
            if (WindowsRemoteControl.WinState(window.MainWindowHandle) == FormWindowState.Minimized) { continue; }

            Develop.Diagnose("Snap", "Alle minimiert: " + window.Klasse + " | " + window.MainWindowTitle);
            WindowsRemoteControl.WinMinimize(window.MainWindowHandle);
        }
    }

    /// <summary>
    /// Stellt den Snapshot wieder her: alle dem Snap unbekannten Fenster werden minimiert,
    /// die gesicherten Fenster bekommen Zustand, Position und Z-Ordnung zurück.
    /// Fehlende Fenster werden über ihre Dokumente neu gestartet oder - ohne Dokumente -
    /// durch ein vorhandenes, freies Fenster desselben Typs ersetzt, das nur diesem Snap gehört (kein Prozessstart).
    /// </summary>
    public static void Restore(WindowSnapshot snapshot) {
        foreach (var plugin in QualityPlugin.AllPlugins.Instances) {
            plugin.IsRunning = snapshot.ActivePlugins.Contains(plugin.KeyName);

            if (snapshot.PluginSettings.FirstOrDefault(s => string.Equals(s.KeyName, plugin.KeyName, StringComparison.OrdinalIgnoreCase)) is { } state) {
                plugin.ApplySettings(state.Settings ?? new JsonObject());
            }
        }

        var snapHandles = SnapFormHandles();
        var claimedByOthers = ClaimedHandles(snapshot);

        // Das Snap-Fenster selbst nie zuordnen - Position und Zustand bleiben unangetastet.
        // Ersatzfenster anderer Snaps werden ebenfalls nicht angetastet.
        var pool = DesktopWindows();
        pool.RemoveAll(w => snapHandles.Contains(w.MainWindowHandle) || claimedByOthers.Contains(w.MainWindowHandle));
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
        var busyHandles = new HashSet<IntPtr>(assignedHandles);
        busyHandles.UnionWith(claimedByOthers);
        busyHandles.UnionWith(snapHandles);
        var changed = false;

        // Fehlende Fenster über die Dokumente neu starten - oder ohne Dokumente ein vorhandenes Fenster desselben Typs umwidmen
        foreach (var info in snapshot.Windows) {
            if (assignments.Exists(a => a.Info == info)) {
                if (info.ReplacementHandle != 0) { info.ReplacementHandle = 0; changed = true; } // Das Original ist wieder da
                continue;
            }

            IntPtr handle;

            if (info.Documents is { Count: > 0 }) {
                WindowPlugin.FindFor(info.ClassName, info.ProcessName).RestoreDocuments(info.Documents);
                handle = WaitForWindow(info, processNames, busyHandles);
            } else {
                Develop.Diagnose("Snap", "Restore: improvisiere Fenster: " + info.ProcessName + " | " + info.Title);
                handle = SimilarWindow(info, processNames, pool);
            }

            if (handle != IntPtr.Zero) {
                WindowsRemoteControl.WinPlace(handle, info.Bounds, info.State == FormWindowState.Maximized);
                _ = busyHandles.Add(handle);

                // Der neue Fensterplatz gehört nur diesem Snap
                if (info.ReplacementHandle != handle.ToInt64()) {
                    info.ReplacementHandle = handle.ToInt64();
                    changed = true;
                }
            } else {
                Develop.Diagnose("Snap", "Restore: Fenster nicht gestartet: " + info.ProcessName + " | " + info.Title);
            }
        }

        if (changed) { SnapshotStore.Save(snapshot); }
    }

    /// <summary>
    /// Handles aller Fenster, die als Ersatz für einen anderen Snap improvisiert wurden.
    /// Verfallene Ansprüche auf geschlossene Fenster werden dabei aus der Ablage entfernt.
    /// </summary>
    private static HashSet<IntPtr> ClaimedHandles(WindowSnapshot snapshot) {
        HashSet<IntPtr> result = [];
        var live = WindowsRemoteControl.Prozesse();
        var processNames = new Dictionary<int, string>();

        foreach (var other in SnapshotStore.LoadFile()) {
            if (string.Equals(other.Id, snapshot.Id, StringComparison.Ordinal)) { continue; }

            var changed = false;

            foreach (var info in other.Windows) {
                if (info.ReplacementHandle == 0) { continue; }

                var handle = new IntPtr(info.ReplacementHandle);
                var window = live.FirstOrDefault(w => w.MainWindowHandle == handle);

                if (window.MainWindowHandle == handle && SameWindow(window, info, processNames)) {
                    _ = result.Add(handle);
                } else {
                    // Der Anspruch verfällt, sobald das Ersatzfenster geschlossen wurde
                    info.ReplacementHandle = 0;
                    changed = true;
                }
            }

            if (changed) { SnapshotStore.Save(other); }
        }

        return result;
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

    /// <summary>
    /// True, wenn das gefundene Fenster zur gesicherten Beschreibung passt (Fensterklasse und Prozess).
    /// </summary>
    private static bool SameWindow(WindowsRemoteControl.StrProcess window, WindowInfo info, Dictionary<int, string> processNames) {
        if (!string.Equals(window.Klasse, info.ClassName, StringComparison.Ordinal)) { return false; }

        if (!processNames.TryGetValue(window.Prid, out var name)) {
            name = ProcessNameOf(window.Prid);
            processNames.Add(window.Prid, name);
        }

        return string.Equals(name, info.ProcessName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Liefert ein bereits vorhandenes, freies Fenster desselben Typs als Ersatz - ohne einen Prozess zu starten.
    /// </summary>
    private static IntPtr SimilarWindow(WindowInfo info, Dictionary<int, string> processNames, List<WindowsRemoteControl.StrProcess> pool) {
        if (info.ProcessName.Length == 0) { return IntPtr.Zero; }

        var window = FindWindow(pool, processNames, info, true) ?? FindWindow(pool, processNames, info, false);

        if (window is not { } match) { return IntPtr.Zero; }

        _ = pool.Remove(match);
        return match.MainWindowHandle;
    }

    private static IntPtr WaitForWindow(WindowInfo info, Dictionary<int, string> processNames, HashSet<IntPtr> busyHandles) {
        for (var i = 0; i < 16; i++) {
            Generic.Pause(0.25, false);

            // Frisch enumerieren, weil das Fenster gerade neu gestartet wurde
            var windows = DesktopWindows();
            windows.RemoveAll(w => busyHandles.Contains(w.MainWindowHandle));

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
