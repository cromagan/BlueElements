// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer;

/// <summary>
/// Basis eines Optimizer-Plugins: bearbeitet alle .cs-Dateien eines Ordners nach einer Regel.
/// Neue Plugins einfach von dieser Klasse ableiten - sie werden über alle geladenen Assemblies automatisch gefunden.
/// </summary>
public abstract class CodeOptimizerPlugin : IComparable<CodeOptimizerPlugin>, IReadableTextWithKey {

    #region Fields

    public static readonly AssemblyAwareCache<CodeOptimizerPlugin> AllPlugins = new();

    #endregion

    #region Properties

    /// <summary>
    /// Eindeutiger Name des Plugins, wird in der Liste angezeigt.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Symbol des Plugins für die Anzeige in der Liste.
    /// </summary>
    public abstract ImageCode Symbol { get; }

    /// <summary>
    /// Kurzbeschreibung des Plugins, wird als QuickInfo angezeigt.
    /// </summary>
    public abstract string QuickInfo { get; }

    /// <summary>
    /// Reihenfolge der Ausführung. Je kleiner, desto früher.
    /// </summary>
    public abstract int Order { get; }

    public string KeyName => field ??= Name;

    #endregion

    #region Methods

    public int CompareTo(CodeOptimizerPlugin? other) {
        if (other is null) { return 1; }
        return Order.CompareTo(other.Order);
    }

    public string ReadableText() => Name;

    public QuickImage? SymbolForReadableText() => QuickImage.Get(Symbol);

    /// <summary>
    /// Liefert die Optionen des Plugins als Controls, die übereinander gestapelt werden.
    /// Standard: keine Optionen.
    /// </summary>
    public virtual List<GenericControl> GetOptions(int widthOfControl) => [];

    /// <summary>
    /// Führt das Plugin auf allen .cs-Dateien unter rootPath aus und liefert den Bericht.
    /// </summary>
    public abstract string Execute(string rootPath);

    #endregion
}
