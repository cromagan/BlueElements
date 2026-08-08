// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel;

namespace BlueBasics.Interfaces;

public static class ParseableExtension {

    #region Methods

    public static bool Parse(this IParseable parsable, string toParse) => parsable.Parse(toParse, '{', '}', ',');

    public static bool Parse(this IParseable parsable, string toParse, char bracketOpen, char bracketClose, char separator) {
        if (toParse.GetAllTags(bracketOpen, bracketClose, separator) is not { } x) { return false; }
        return parsable.Parse(x, toParse);
    }

    public static bool Parse(this IParseable parsable, List<KeyValuePair<string, string>> allTags, string originalParse) {
        // ParseableItem: Suppress-Modus whrend des gesamten Parsens, damit
        // Property-Setter der Subklassen keine Change-Events feuern (siehe
        // ParseableItem.ISupportInitialize). Andere IParseable-Implementierungen
        // profitieren nicht davon, werden aber auch nicht beeintrchtigt.
        if (parsable is ISupportInitialize pi) { pi.BeginInit(); }
        try {
            foreach (var pair in allTags) {
                var i = parsable.ParseThis(pair.Key.ToLowerInvariant(), pair.Value);

                if (!i) {
                    Develop.DebugPrint("Kann nicht geparsed werden: " + pair.Key + " = " + pair.Value + Develop.ContextInfo(parsable));
                }
            }
            parsable.ParseFinished(originalParse);
        } finally {
            if (parsable is ISupportInitialize pi2) { pi2.EndInit(); }
        }
        return true;
    }

    #endregion
}

public interface IParseable : IStringable {

    #region Methods

    void ParseFinished(string parsed);

    /// <summary>
    ///
    /// </summary>
    /// <param name="key">Der Key in Kleinschreibung konvertiert</param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool ParseThis(string key, string value);

    #endregion
}