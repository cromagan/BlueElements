// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Drawing;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static Size CanvasToControl(this SizeF p, float zoom) => new(p.Width.CanvasToControl(zoom), p.Height.CanvasToControl(zoom));

    public static SizeF SizeFParse(this string toParse) {
        toParse = toParse.FromNonCritical().RemoveChars("{}Widtheg= ");
        var w = toParse.SplitBy(",H");
        return new SizeF(FloatParse(w[0]), FloatParse(w[1]));
    }

    public static Size SizeParse(this string toParse) {
        toParse = toParse.FromNonCritical().RemoveChars("{}Widtheg= ");
        var w = toParse.SplitBy(",H");
        return new Size(IntParse(w[0]), IntParse(w[1]));
    }

    #endregion
}