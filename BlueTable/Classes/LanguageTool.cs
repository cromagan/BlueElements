// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace BlueTable.Classes;

/// <summary>
/// Stellt Werkzeuge zur Übersetzung und Textaufbereitung bereit.
/// </summary>
public static class LanguageTool {

    #region Fields

    private static readonly ConcurrentCache<string, string> _translationCache = new(5000);
    private static readonly object?[] EmptyArgs = [];

    #endregion

    #region Properties

    /// <summary>
    /// Die aktuelle Übersetzungstabelle.
    /// </summary>
    public static Table? Translation { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Übersetzt einen Text ohne zusätzliche Argumente.
    /// </summary>
    public static string DoTranslate(string txt) => DoTranslate(txt, true, EmptyArgs);

    /// <summary>
    /// Übersetzt den angegebenen Text unter Berücksichtigung von Formatierungsargumenten.
    /// </summary>
    /// <param name="txt">Der zu übersetzende Quelltext.</param>
    /// <param name="mustTranslate">Gibt an, ob bei fehlender Übersetzung ein Eintrag in der Tabelle erstellt werden soll.</param>
    /// <param name="args">Optionale Formatierungsargumente.</param>
    /// <returns>Der übersetzte und formatierte Text.</returns>
    public static string DoTranslate(string txt, bool mustTranslate, params object?[] args) {
        try {
            if (string.IsNullOrEmpty(txt)) { return string.Empty; }
            if (Translation is null) { return FormatResult(txt, args); }

            var result = _translationCache.GetOrAdd(txt, _ => DoTranslateCore(txt, mustTranslate));
            return FormatResult(result, args);
        } catch {
            return txt;
        }
    }

    /// <summary>
    /// Parst Ersetzungsregeln im Format "Alt|Neu", eine Regel pro Zeile.
    /// Ungültige Regeln und Regeln ohne Suchtext werden verworfen.
    /// </summary>
    public static List<(string Search, string Replace)> ParseOpticalReplace(string[] rules) {
        List<(string Search, string Replace)> result = [];
        foreach (var thisRule in rules) {
            var parts = thisRule.SplitBy("|");
            if (parts.Length != 2 || parts[0].Length == 0) { continue; }
            result.Add((parts[0], parts[1]));
        }
        return result;
    }

    /// <summary>
    /// Fügt Präfix und Suffix hinzu und ersetzt den Text nach dem gewünschten Stil.
    /// </summary>
    /// <param name="txt">Der Basistext.</param>
    /// <param name="style">Der Stil der Kürzung/Ersetzung.</param>
    /// <param name="prefix">Ein optionaler Präfix.</param>
    /// <param name="suffix">Ein optionaler Suffix.</param>
    /// <param name="doOpticalTranslation">Bestimmt, ob eine optische Übersetzung stattfinden soll.</param>
    /// <param name="opticalRules">Bereits geparste Ersetzungsregeln.</param>
    public static string PrepaireText(string txt, ShortenStyle style, string prefix, string suffix, TranslationType doOpticalTranslation, ReadOnlyCollection<(string Search, string Replace)>? opticalRules) {
        if (!string.IsNullOrEmpty(txt)) {
            if (Translation is not null && doOpticalTranslation is TranslationType.Übersetzen) {
                txt = DoTranslate(txt, true);
                if (!string.IsNullOrEmpty(prefix)) { prefix = DoTranslate(prefix, true); }
                if (!string.IsNullOrEmpty(suffix)) { suffix = DoTranslate(suffix, true); }
            }
            if (!string.IsNullOrEmpty(prefix)) { txt = $"{prefix} {txt}"; }
            if (!string.IsNullOrEmpty(suffix)) { txt = $"{txt} {suffix}"; }
        }

        if (opticalRules is not { Count: > 0 } || style is ShortenStyle.Unreplaced) { return txt; }

        var originalText = txt;
        var sb = new StringBuilder(txt);

        foreach (var (oldVal, newVal) in opticalRules) {
            sb.Replace(oldVal, newVal);
        }

        txt = sb.ToString();
        return style is ShortenStyle.Replaced or ShortenStyle.HTML || originalText.Equals(txt, StringComparison.Ordinal)
            ? txt
            : $"{originalText} ({txt})";
    }

    /// <summary>
    /// Kernlogik für die Übersetzung eines einzelnen Strings.
    /// </summary>
    private static string DoTranslateCore(string txt, bool mustTranslate) {
        var addend = txt.EndsWith(':') ? ":" : string.Empty;
        if (addend is ":") { txt = txt.TrimEnd(':'); }

        txt = txt.Replace("\r\n", "\r");

        var r = Translation?.Row[txt];
        if (r is not { IsDisposed: false }) {
            if (!string.IsNullOrEmpty(Translation?.IsGenericEditable(false)) || !mustTranslate) { return txt + addend; }

            r = Translation?.Row.GenerateAndAdd(txt, "Missing translation");
            if (r is not { IsDisposed: false }) { return txt + addend; }
        }

        var t = r.CellGetString("Translation");
        return string.IsNullOrEmpty(t) ? txt + addend : t + addend;
    }

    /// <summary>
    /// Hilfsmethode zur String-Formatierung, um doppelten Code zu vermeiden.
    /// </summary>
    private static string FormatResult(string pattern, object?[] args) => args.Length == 0 ? pattern : string.Format(CultureInfo.InvariantCulture, pattern, args);

    #endregion
}