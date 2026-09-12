// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI;

/// <summary>
/// Hält die geparsten Kommandozeilen-Argumente eines Befehls.
/// Argumente ohne "--"-Präfix sind Positionsargumente, Argumente mit "--"-Präfix
/// sind Optionen (mit Wert) oder Schalter (ohne Wert). Optionen dürfen wiederholt
/// angegeben werden; alle Werte einer Option liefert AllOptions.
/// </summary>
public class CliArgs {

    #region Fields

    private readonly Dictionary<string, string> _options;
    private readonly List<(string Name, string Value)> _allOptions = [];
    private readonly HashSet<string> _flags;
    private readonly List<string> _positional;

    #endregion

    #region Constructors

    /// <summary>
    /// Parst die Argumente. Schalter (ohne Wert) müssen über <paramref name="flags" />,
    /// Optionen mit Wert über <paramref name="options" /> bekannt gegeben werden.
    /// Unbekannte "--"-Optionen erzeugen einen ParseError, statt still als Wert zu gelten.
    /// </summary>
    public CliArgs(IEnumerable<string> args, IEnumerable<string> flags, IEnumerable<string> options) {
        var knownFlags = flags.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var knownOptions = options.ToHashSet(StringComparer.OrdinalIgnoreCase);

        _options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _flags = [];
        _positional = [];
        ParseError = string.Empty;

        var items = args.ToList();

        for (var i = 0; i < items.Count; i++) {
            var token = items[i];

            if (!token.StartsWith("--", StringComparison.Ordinal)) {
                _positional.Add(token);
                continue;
            }

            var name = token[2..];

            if (name is { Length: 0 }) {
                ParseError = "Leere Option '--' ist nicht erlaubt.";
                return;
            }

            if (knownFlags.Contains(name)) {
                _flags.Add(name);
                continue;
            }

            if (!knownOptions.Contains(name)) {
                ParseError = $"Unbekannte Option '{token}'.";
                return;
            }

            if (i + 1 >= items.Count) {
                ParseError = $"Der Option '{token}' fehlt ein Wert.";
                return;
            }

            i++;
            _options[name] = items[i];
            _allOptions.Add((name, items[i]));
        }
    }

    #endregion

    #region Properties

    public int PositionalCount => _positional.Count;

    /// <summary>
    /// Leer, wenn das Parsen erfolgreich war. Ansonsten die Fehlerbeschreibung.
    /// </summary>
    public string ParseError { get; }

    #endregion

    #region Indexers

    /// <summary>
    /// Liefert das Positionsargument am Index oder null, wenn der Index außerhalb liegt.
    /// </summary>
    public string? this[int index] => index >= 0 && index < _positional.Count ? _positional[index] : null;

    #endregion

    #region Methods

    /// <summary>
    /// Liefert alle Werte einer mehrfach angegebenen Option in Reihenfolge (z. B. --set A=1 --set B=2).
    /// </summary>
    public List<string> AllOptions(string name) => [.. _allOptions.Where(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Select(o => o.Value)];

    /// <summary>
    /// Prüft, ob ein Schalter (Wert-lose Option) gesetzt ist.
    /// </summary>
    public bool Flag(string name) => _flags.Contains(name);

    /// <summary>
    /// Prüft, ob eine Option mit Wert angegeben wurde (auch wenn der Wert leer ist).
    /// </summary>
    public bool HasOption(string name) => _options.ContainsKey(name);

    /// <summary>
    /// Liefert den Wert einer Option oder null, wenn sie nicht angegeben wurde.
    /// </summary>
    public string? Option(string name) => _options.TryGetValue(name, out var value) ? value : null;

    #endregion
}
