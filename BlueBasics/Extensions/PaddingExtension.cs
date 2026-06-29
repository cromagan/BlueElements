// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static System.Windows.Forms.Padding PaddingParse(this string? toParse) {
        if (toParse is not { Length: > 0 }) { return System.Windows.Forms.Padding.Empty; }

        var clean = toParse.FromNonCritical().RemoveChars("{}LeftTopRightBm= ");
        var w = clean.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (w.Length == 1) { return new System.Windows.Forms.Padding(IntParse(w[0])); }
        if (w.Length == 2) { return new System.Windows.Forms.Padding(IntParse(w[0]), IntParse(w[1]), IntParse(w[0]), IntParse(w[1])); }
        if (w.Length == 4) { return new System.Windows.Forms.Padding(IntParse(w[0]), IntParse(w[1]), IntParse(w[2]), IntParse(w[3])); }
        return System.Windows.Forms.Padding.Empty;
    }

    /// <summary>
    /// Erzeugt eine kompakte, parsbare Darstellung der Padding-Werte.
    /// Symmetrische Paddings werden verkürzt ({X} bzw. {X, Y}), sonst voll {L, T, R, B}.
    /// </summary>
    public static string ToParseable(this System.Windows.Forms.Padding p) {
        if (p.Left == p.Right && p.Top == p.Bottom) {
            if (p.Left == p.Top) { return $"{{{p.Left}}}"; }
            return $"{{{p.Left}, {p.Top}}}";
        }
        return $"{{{p.Left}, {p.Top}, {p.Right}, {p.Bottom}}}";
    }

    #endregion
}