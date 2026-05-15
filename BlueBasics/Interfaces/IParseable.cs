// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

public static class ParseableExtension {

    #region Methods

    public static bool Parse(this IParseable parsable, string toParse) => parsable.Parse(toParse, '{', '}', ',');

    public static bool Parse(this IParseable parsable, string toParse, char bracketOpen, char bracketClose, char separator) {
        if (toParse.GetAllTags(bracketOpen, bracketClose, separator) is not { } x) { return false; }

        foreach (var pair in x) {
            var i = parsable.ParseThis(pair.Key.ToLowerInvariant(), pair.Value);

            if (!i) {
                Develop.DebugPrint("Kann nicht geparsed werden: " + pair.Key + "/" + pair.Value + "/" + toParse);
            }
        }
        parsable.ParseFinished(toParse);
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