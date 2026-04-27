// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ClassesStatic;

public static class Extensions {
    //public static readonly string ImageKennung = ((char)9001).ToString(false);
    //public static readonly string ObjectKennung = ((char)9002).ToString(false);

    #region Fields

    private static readonly string ReplacerForBackSlash = ((char)9003).ToString();
    private static readonly string ReplacerForN = ((char)9005).ToString();
    private static readonly string ReplacerForQuotes = ((char)9000).ToString();
    private static readonly string ReplacerForR = ((char)9004).ToString();

    #endregion

    #region Methods

    public static string RemoveCriticalVariableChars(this string txt) {
        if (string.IsNullOrEmpty(txt)) { return txt; }
        var sb = new System.Text.StringBuilder(txt.Length);
        foreach (var c in txt) {
            switch (c) {
                case '\\': sb.Append(ReplacerForBackSlash); break;
                case '"': sb.Append(ReplacerForQuotes); break;
                case '\r': sb.Append(ReplacerForR); break;
                case '\n': sb.Append(ReplacerForN); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }

    public static string RemoveEscape(this string txt) {
        if (string.IsNullOrEmpty(txt)) { return txt; }
        var sb = new System.Text.StringBuilder(txt.Length);
        var i = 0;
        while (i < txt.Length) {
            if (i + 1 < txt.Length && txt[i] == '\\') {
                var next = txt[i + 1];
                if (next == '\\') { sb.Append(ReplacerForBackSlash); i += 2; continue; }
                if (next == '"') { sb.Append(ReplacerForQuotes); i += 2; continue; }
                if (next == 'r') { sb.Append(ReplacerForR); i += 2; continue; }
                if (next == 'n') { sb.Append(ReplacerForN); i += 2; continue; }
            }
            sb.Append(txt[i]);
            i++;
        }
        return sb.ToString();
    }

    public static string RestoreCriticalVariableChars(this string txt) {
        if (string.IsNullOrEmpty(txt)) { return txt; }
        return txt.Replace(ReplacerForBackSlash, "\\")
                  .Replace(ReplacerForQuotes, "\"")
                  .Replace(ReplacerForR, "\r")
                  .Replace(ReplacerForN, "\n");
    }

    #endregion
}