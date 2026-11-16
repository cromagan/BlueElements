// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text.Json.Nodes;

namespace CodeOptimizer;

/// <summary>
/// Basis eines Optimizer-Plugins: bearbeitet alle .cs-Dateien eines Ordners nach einer Regel.
/// Neue Plugins einfach von dieser Klasse ableiten - sie werden über alle geladenen Assemblies automatisch gefunden.
/// </summary>
public abstract class CodeOptimizerPlugin : IComparable<CodeOptimizerPlugin>, IReadableTextWithKey {

    #region Events

    /// <summary>
    /// Wird ausgelöst, wenn das Plugin die Optionsanzeige neu aufbauen lassen muss, z. B. nach Regel hinzugefügt oder entfernt.
    /// </summary>
    public event EventHandler? OptionsChanged;

    #endregion

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
    /// Liefert die aktuellen Einstellungen des Plugins als JSON.
    /// </summary>
    public virtual JsonObject SaveSettings() => new();

    /// <summary>
    /// Übernimmt die Einstellungen aus dem JSON; fehlende Werte führen zum Standardzustand.
    /// </summary>
    public virtual void LoadSettings(JsonObject settings) { }

    /// <summary>
    /// Führt das Plugin auf allen .cs-Dateien unter rootPath aus und liefert den Bericht.
    /// </summary>
    public abstract string Execute(string rootPath);

    /// <summary>
    /// Hängt übersprungene Dateien (kein gültiges UTF-8) an den Bericht an.
    /// </summary>
    protected static void AppendSkipped(StringBuilder report, List<string> skipped) {
        report.AppendLine(CultureInfo.InvariantCulture, $"Übersprungen (kein UTF-8): {skipped.Count}");

        foreach (var file in skipped) {
            report.AppendLine("  Übersprungen: " + file);
        }
    }

    /// <summary>
    /// Löst das OptionsChanged-Event aus.
    /// </summary>
    protected virtual void OnOptionsChanged() => OptionsChanged?.Invoke(this, EventArgs.Empty);

    #endregion
}
