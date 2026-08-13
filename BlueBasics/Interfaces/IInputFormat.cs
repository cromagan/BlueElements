// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

public interface IInputFormat {

    #region Properties

    AdditionalCheck AdditionalFormatCheck { get; set; }
    string AllowedChars { get; set; }

    /// <summary>
    /// Zeichen, die im Text nicht erlaubt sind. Steuerzeichen werden direkt
    /// als tatsächliche Zeichen gespeichert (CR/LF also als echte Steuerzeichen,
    /// nicht als Escape-Sequenz). Die Escape-Sequenz-Darstellung wird nur zur
    /// Anzeige in Textfeldern (z.B. ColumnEditor) verwendet.
    /// Leer bedeutet: keine verbotenen Zeichen.
    /// </summary>
    string ForbiddenChars { get; set; }

    int MaxTextLength { get; set; }

    /// <summary>
    /// Mindestlänge des Textes. 0 bedeutet: auch leer erlaubt.
    /// </summary>
    int MinTextLength { get; set; }

    bool MultiLine { get; set; }

    string RegexCheck { get; set; }

    bool SpellCheckingEnabled { get; set; }

    bool TextFormatingAllowed { get; set; }

    #endregion
}

public static class InputFormatExtensions {

    #region Methods

    /// <summary>
    /// Setzt: AllowedChars, ForbiddenChars, RegexCheck, TextFormatingAllowed,
    /// AdditionalFormatCheck, SpellCheckingEnabled, MaxTextLength, MinTextLength und Multiline
    /// </summary>
    public static void GetStyleFrom(this IInputFormat? t, IInputFormat? source) {
        if (source is null || t is null) { return; }

        t.AdditionalFormatCheck = source.AdditionalFormatCheck;
        t.AllowedChars = source.AllowedChars.DistinctCharsSorted();
        t.ForbiddenChars = source.ForbiddenChars.DistinctCharsSorted();
        t.RegexCheck = source.RegexCheck;
        t.MultiLine = source.MultiLine;
        t.MaxTextLength = source.MaxTextLength;
        t.MinTextLength = source.MinTextLength;
        t.SpellCheckingEnabled = source.SpellCheckingEnabled;
        t.TextFormatingAllowed = source.TextFormatingAllowed;
    }

    public static string IsFormat(this List<string> list, IInputFormat formatToCheck) {
        foreach (var thisstring in list) {
            var reason = thisstring.IsFormat(formatToCheck, false);
            if (reason is { Length: > 0 }) { return reason; }
        }
        return string.Empty;
    }

    /// <summary>
    /// Prüft den Text, ob er mit dem geforderten Format übereinstimmt.
    /// Gibt string.Empty zurück, wenn das Format stimmt, sonst eine
    /// lesbare Begründung.
    /// </summary>
    public static string IsFormat(this string txt, IInputFormat formatToCheck, bool splitallowed) {
        if (txt is { Length: > 0 } && splitallowed && formatToCheck.MultiLine && txt.Contains('\r')) {
            return txt.SplitByCr().ToList().IsFormat(formatToCheck);
        }

        if ((txt?.Length ?? 0) < formatToCheck.MinTextLength) {
            return "Der Text muss mindestens " + formatToCheck.MinTextLength + " Zeichen lang sein.";
        }

        if (txt is null) { return string.Empty; }

        if (txt.Length > formatToCheck.MaxTextLength) {
            return "Der Text darf maximal " + formatToCheck.MaxTextLength + " Zeichen lang sein.";
        }

        if (formatToCheck.ForbiddenChars is { Length: > 0 } forbidden) {
            for (var i = 0; i < forbidden.Length; i++) {
                if (txt.Contains(forbidden[i])) {
                    return "Der Text enthält ein verbotenes Zeichen.";
                }
            }
        }

        if (formatToCheck.AllowedChars is { Length: > 0 } allowed && !txt.ContainsOnlyChars(allowed)) {
            return "Der Text enthält nicht erlaubte Zeichen.";
        }

        if (formatToCheck.RegexCheck is { Length: > 0 } regex && !txt.RegexMatch(regex)) {
            return "Der Text entspricht nicht dem erwarteten Muster.";
        }

        switch (formatToCheck.AdditionalFormatCheck) {
            case AdditionalCheck.None:
                break;

            case AdditionalCheck.Integer:
                if (!txt.IsLong()) { return "Der Text ist keine gültige Ganzzahl."; }
                break;

            case AdditionalCheck.Float:
                if (!txt.IsDouble()) { return "Der Text ist keine gültige Zahl."; }
                break;

            case AdditionalCheck.DateTime:
                if (!txt.IsDateTime()) { return "Der Text ist kein gültiges Datum."; }
                break;

            default:
                Develop.DebugPrint(formatToCheck.AdditionalFormatCheck);
                break;
        }

        return string.Empty;
    }

    public static bool IsFormatIdentical(this IInputFormat t, IInputFormat source) => t.AdditionalFormatCheck == source.AdditionalFormatCheck &&
            t.AllowedChars == source.AllowedChars &&
            t.ForbiddenChars == source.ForbiddenChars &&
            t.RegexCheck == source.RegexCheck &&
            t.MultiLine == source.MultiLine &&
            t.SpellCheckingEnabled == source.SpellCheckingEnabled &&
            t.TextFormatingAllowed == source.TextFormatingAllowed &&
            t.MinTextLength == source.MinTextLength &&
            t.MaxTextLength == source.MaxTextLength;

    /// <summary>
    /// Ignoriert Multiline und wenn MaxTextLength 4000 ist
    /// </summary>
    public static bool IsFormatIdenticalSoft(this IInputFormat t, IInputFormat source) {
        var x = t.AdditionalFormatCheck == source.AdditionalFormatCheck &&
                   t.AllowedChars == source.AllowedChars &&
                   t.ForbiddenChars == source.ForbiddenChars &&
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