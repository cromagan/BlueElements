// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueDeskSnap.Plugins;

/// <summary>
/// Basis eines Snapshot-Plugins: erkennt einen Anwendungstyp, liest dessen offene Dokumente und stellt sie wieder her.
/// Neue Plugins einfach von dieser Klasse ableiten - sie werden über alle geladenen Assemblies automatisch gefunden.
/// </summary>
public abstract class WindowPlugin : IComparable<WindowPlugin>, IHasKeyName {

    #region Fields

    public static readonly AssemblyAwareCache<WindowPlugin> AllPlugins = new();

    #endregion

    #region Constructors

    protected WindowPlugin() { }

    #endregion

    #region Properties

    /// <summary>
    /// Eindeutiger Name des Plugins, wird im Snapshot mitgespeichert.
    /// </summary>
    public abstract string KeyName { get; }

    /// <summary>
    /// Je kleiner, desto eher bekommt das Plugin ein Fenster zugeordnet. Das generische Plugin liegt bewusst ganz hinten.
    /// </summary>
    public abstract int Priority { get; }

    #endregion

    #region Methods

    public int CompareTo(WindowPlugin? other) {
        if (other is null) { return 1; }
        return Priority.CompareTo(other.Priority);
    }

    /// <summary>
    /// Liefert die offenen Dateien/URLs des Fensters. Leer, wenn nichts ermittelbar ist.
    /// </summary>
    public abstract List<string> CaptureDocuments(IntPtr handle);

    /// <summary>
    /// Liefert das Plugin mit der kleinsten Priority, das für den Fenstertyp zuständig ist.
    /// </summary>
    public static WindowPlugin FindFor(string className, string processName) {
        WindowPlugin? best = null;

        foreach (var plugin in AllPlugins.Instances) {
            if (!plugin.IsResponsibleFor(className, processName)) { continue; }

            if (best is null || plugin.Priority < best.Priority) { best = plugin; }
        }

        return best ?? new GenericWindowPlugin();
    }

    /// <summary>
    /// True, wenn dieses Plugin für das Fenstersignal (Klasse/Prozess) zuständig ist.
    /// </summary>
    public abstract bool IsResponsibleFor(string className, string processName);

    /// <summary>
    /// Öffnet die gesicherten Dokumente neu. Standard: per Shell-Zuordnung (Explorer öffnet Ordner, Browser öffnet URLs).
    /// </summary>
    public virtual void RestoreDocuments(List<string> documents) {
        foreach (var doc in documents) {
            _ = Process.Start(new ProcessStartInfo(doc) { UseShellExecute = true });
        }
    }

    #endregion
}
