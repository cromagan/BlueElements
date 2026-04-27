// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;
using System.Drawing;
using System.Drawing.Text;

namespace BlueBasics;

public static partial class Extensions {

    #region Fields

    public static readonly StringFormat DefaultWithTrailingSpaces = new(StringFormat.GenericTypographic) {
        FormatFlags = StringFormat.GenericDefault.FormatFlags | StringFormatFlags.MeasureTrailingSpaces,
        Alignment = StringAlignment.Near,
        LineAlignment = StringAlignment.Near,
        Trimming = StringTrimming.None
    };

    #endregion

    #region Methods

    public static SizeF MeasureString(this Font font, string text) {
        if (string.IsNullOrEmpty(text)) { return SizeF.Empty; }

        try {
            using var gr = Graphics.FromHwnd(IntPtr.Zero);
            SetTextRenderingHint(gr, font);
            return gr.MeasureString(text, font, int.MaxValue, DefaultWithTrailingSpaces);
        } catch {
            return SizeF.Empty;
        }
    }

    public static void SetTextRenderingHint(Graphics gr, Font font) =>
                //http://csharphelper.com/blog/2014/09/understand-font-aliasing-issues-in-c/
                gr.TextRenderingHint = font.Size < 11 ? TextRenderingHint.ClearTypeGridFit : TextRenderingHint.AntiAlias;

    #endregion
}