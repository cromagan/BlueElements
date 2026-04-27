// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Padding PaddingParse(this string? toParse) {
        if (toParse == null || string.IsNullOrEmpty(toParse)) { return Padding.Empty; }

        toParse = toParse.FromNonCritical().RemoveChars("{}LeftTopRightBm= ");
        var w = toParse.Split(',');

        if (w.Length != 4) { return Padding.Empty; }

        return new Padding(IntParse(w[0]), IntParse(w[1]), IntParse(w[2]), IntParse(w[3]));
    }

    #endregion
}