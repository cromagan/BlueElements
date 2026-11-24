// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Classes;

/// <summary>
/// Hält die geparsten Kommandozeilen-Argumente.
/// Argumente ohne "--"-Präfix sind Positionsargumente, Argumente mit "--"-Präfix
/// sind Optionen (mit Wert) oder Schalter (ohne Wert). Optionen dürfen wiederholt
/// angegeben werden; alle Werte einer Option liefert AllOptions.
/// Streng: Schalter und Optionen werden deklariert, Unbekanntes erzeugt einen ParseError.
/// Los: Jedes "--"-Token ist eine Option oder ein Schalter, unbekannte Namen sind erlaubt.
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
    /// Parst die Argumente im losen Modus: Ein "--"-Token mit folgendem Wert ist eine
    /// Option, ohne folgendes Token ein Schalter. Unbekannte Namen sind erlaubt.
    /// </summary>
    public CliArgs(IEnumerable<string> args) : this(args, [], [], false) { }

    /// <summary>
    /// Parst die Argumente im strengen Modus. Schalter (ohne Wert) müssen über
    /// <paramref name="flags" />, Optionen mit Wert über <paramref name="options" />
    /// bekannt gegeben werden. Unbekannte "--"-Optionen erzeugen einen ParseError,
    /// statt still als Wert zu gelten.
    /// </summary>
    public CliArgs(IEnumerable<string> args, IEnumerable<string> flags, IEnumerable<string> options) : this(args, flags, options, true) { }

    /// <summary>
    /// Parst die Argumente. Nur "--" kennzeichnet Optionen und Schalter,
    /// alle anderen Tokens sind Positionsargumente.
    /// </summary>
    private CliArgs(IEnumerable<string> args, IEnumerable<string> flags, IEnumerable<string> options, bool strict) {
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

            if (strict) {
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
            } else {
                // Lose: Ein Wert wird nur konsumiert, wenn er nicht selbst mit "--" beginnt.
                if (i + 1 < items.Count && !items[i + 1].StartsWith("--", StringComparison.Ordinal)) {
                    _options[name] = items[i + 1];
                    _allOptions.Add((name, items[i + 1]));
                    i++;
                } else {
                    _flags.Add(name);
                }
            }
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

    /// <summary>
    /// Liefert alle Optionen als Dictionary: letzter Wert gewinnt, Schalter als leerer String.
    /// </summary>
    public Dictionary<string, string> ToDictionary() {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, value) in _allOptions) {
            dict[name] = value;
        }
        foreach (var name in _flags) {
            dict[name] = string.Empty;
        }
        return dict;
    }

    #endregion
}
