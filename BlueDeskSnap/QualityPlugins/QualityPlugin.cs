// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueDeskSnap.QualityPlugins;

/// <summary>
/// Basis eines Quality-Plugins: solange es in der Liste aktiviert ist, läuft es alle 3 Sekunden im UI-Thread.
/// Neue Plugins einfach von dieser Klasse ableiten - sie werden über alle geladenen Assemblies automatisch gefunden.
/// </summary>
public abstract class QualityPlugin : IHasKeyName {

    #region Fields

    public static readonly AssemblyAwareCache<QualityPlugin> AllPlugins = new();

    #endregion

    #region Constructors

    protected QualityPlugin() { }

    #endregion

    #region Properties

    /// <summary>
    /// Eindeutiger Name des Plugins, dient als Schlüssel in der Plugin-Liste.
    /// </summary>
    public abstract string KeyName { get; }

    /// <summary>
    /// Anzeigename in der Plugin-Liste.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Kurzbeschreibung als QuickInfo in der Plugin-Liste.
    /// </summary>
    public abstract string QuickInfo { get; }

    /// <summary>
    /// Symbol in der Plugin-Liste.
    /// </summary>
    public abstract ImageCode Symbol { get; }

    /// <summary>
    /// True, wenn das Plugin per Klick aktiviert wurde und mit dem Tick ausgeführt wird.
    /// </summary>
    public bool IsRunning { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Führt das Plugin aus, wenn es aktiviert ist. Wird alle 3 Sekunden aufgerufen.
    /// </summary>
    public void Tick() {
        if (!IsRunning) { return; }
        ExecuteTick();
    }

    /// <summary>
    /// Ein Durchlauf des Plugins - alle 3 Sekunden, solange aktiviert.
    /// </summary>
    protected abstract void ExecuteTick();

    #endregion
}
