// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

public interface IInputFormat {

    #region Properties

    AdditionalCheck AdditionalFormatCheck { get; set; }
    string AllowedChars { get; set; }

    int MaxTextLength { get; set; }

    bool MultiLine { get; set; }

    string RegexCheck { get; set; }

    bool SpellCheckingEnabled { get; set; }

    bool TextFormatingAllowed { get; set; }

    #endregion
}

public static class InputFormatExtensions {

    #region Methods

    /// <summary>
    /// Setzt: AllowedChars, RegexCheck, TextFormatingAllowed, AdditionalFormatCheck, SpellCheckingEnabled, MaxTextLength und Multiline
    /// </summary>
    /// <param name="t"></param>
    /// <param name="source"></param>
    public static void GetStyleFrom(this IInputFormat? t, IInputFormat? source) {
        if (source is null || t is null) { return; }

        t.AdditionalFormatCheck = source.AdditionalFormatCheck;
        t.AllowedChars = source.AllowedChars;
        t.RegexCheck = source.RegexCheck;
        t.MultiLine = source.MultiLine;
        t.MaxTextLength = source.MaxTextLength;
        t.SpellCheckingEnabled = source.SpellCheckingEnabled;
        t.TextFormatingAllowed = source.TextFormatingAllowed;
    }

    public static bool IsFormat(this List<string> list, IInputFormat formatToCheck, bool valueRequired) => list.Exists(thisstring => !thisstring.IsFormat(formatToCheck, valueRequired));

    public static bool IsFormat(this string txt, IInputFormat formatToCheck, bool valueRequired) {
        if (!valueRequired && string.IsNullOrEmpty(txt)) { return true; }
        return txt.IsFormat(formatToCheck);
    }

    /// <summary>
    /// Prüft den Text, ob er mit dem geforderten Format (z.B. FormatHolder_Filepath.Instance) übereinstimmt
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="formatToCheck">z.B. FormatHolder_Filepath.Instance</param>
    /// <returns></returns>
    public static bool IsFormat(this string txt, IInputFormat formatToCheck) {
        if (txt.Length > formatToCheck.MaxTextLength) { return false; }

        if (!string.IsNullOrEmpty(formatToCheck.AllowedChars) && !txt.ContainsOnlyChars(formatToCheck.AllowedChars)) {
            return false;
        }

        if (!string.IsNullOrEmpty(formatToCheck.RegexCheck) && !txt.RegexMatch(formatToCheck.RegexCheck)) {
            return false;
        }

        switch (formatToCheck.AdditionalFormatCheck) {
            case AdditionalCheck.None:
                break;

            case AdditionalCheck.Integer:
                if (!txt.IsLong()) { return false; }
                break;

            case AdditionalCheck.Float:
                if (!txt.IsDouble()) { return false; }
                break;

            case AdditionalCheck.DateTime:
                if (!txt.IsDateTime()) { return false; }
                break;

            default:
                Develop.DebugPrint(formatToCheck.AdditionalFormatCheck);
                break;
        }

        return true;
    }

    public static bool IsFormatIdentical(this IInputFormat t, IInputFormat source) => t.AdditionalFormatCheck == source.AdditionalFormatCheck &&
                t.AllowedChars == source.AllowedChars &&
            t.RegexCheck == source.RegexCheck &&
            t.MultiLine == source.MultiLine &&
            t.SpellCheckingEnabled == source.SpellCheckingEnabled &&
            t.TextFormatingAllowed == source.TextFormatingAllowed &&
            t.MaxTextLength == source.MaxTextLength;

    /// <summary>
    /// Ignoriert Multiline und wenn MaxTextLength 4000 ist
    /// </summary>
    /// <param name="t"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public static bool IsFormatIdenticalSoft(this IInputFormat t, IInputFormat source) {
        var x = t.AdditionalFormatCheck == source.AdditionalFormatCheck &&
                   t.AllowedChars == source.AllowedChars &&
                   t.RegexCheck == source.RegexCheck &&
                   t.SpellCheckingEnabled == source.SpellCheckingEnabled &&
                   t.TextFormatingAllowed == source.TextFormatingAllowed;
        if (!x) { return false; }

        if (t.MaxTextLength < 4000) {
            if (t.MaxTextLength != source.MaxTextLength) { return false; }
        }

        return t.MaxTextLength >= 1;
    }

    #endregion
}