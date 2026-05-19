// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.Converter;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static System.Windows.Forms.Padding PaddingParse(this string? toParse) {
        if (toParse == null || string.IsNullOrEmpty(toParse)) { return System.Windows.Forms.Padding.Empty; }

        toParse = toParse.FromNonCritical().RemoveChars("{}LeftTopRightBm= ");
        var w = toParse.Split(',');

        if (w.Length != 4) { return System.Windows.Forms.Padding.Empty; }

        return new System.Windows.Forms.Padding(IntParse(w[0]), IntParse(w[1]), IntParse(w[2]), IntParse(w[3]));
    }

    #endregion
}