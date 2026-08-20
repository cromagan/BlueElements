// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI;

/// <summary>
/// Hält die geparsten Kommandozeilen-Argumente eines Befehls.
/// Argumente ohne "--"-Präfix sind Positionsargumente, Argumente mit "--"-Präfix
/// sind Optionen (mit Wert) oder Schalter (ohne Wert).
/// </summary>
public class CliArgs {

    #region Fields

    private readonly Dictionary<string, string> _options;
    private readonly HashSet<string> _flags;
    private readonly List<string> _positional;

    #endregion

    #region Constructors

    /// <summary>
    /// Parst die Argumente. Schalter-Optionen (ohne Wert) müssen vom Befehl
    /// über <paramref name="flags" /> bekannt gegeben werden, alle übrigen
    /// "--"-Optionen erwarten einen nachfolgenden Wert.
    /// </summary>
    public CliArgs(IEnumerable<string> args, IEnumerable<string> flags) {
        var knownFlags = flags.ToHashSet(StringComparer.OrdinalIgnoreCase);

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

            if (i + 1 >= items.Count) {
                ParseError = $"Der Option '{token}' fehlt ein Wert.";
                return;
            }

            i++;
            _options[name] = items[i];
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
