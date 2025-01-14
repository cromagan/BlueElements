// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using System;
using System.Drawing;
using System.Drawing.Text;

namespace BlueBasics;

public static partial class Extensions {
    //public static Size FormatedText_NeededSize(this Font font, string text, QuickImage? qi, int minSize) {
    //    try {
    //        var pSize = SizeF.Empty;
    //        var tSize = SizeF.Empty;
    //        //if (font == null) { return new Size(3, 3); }

    //        if (qi != null) {
    //            lock (qi) {
    //                pSize = ((Bitmap)qi).Size;
    //            }
    //        }
    //        if (!string.IsNullOrEmpty(text)) { tSize = font.MeasureString(text); }

    //        if (!string.IsNullOrEmpty(text)) {
    //            if (qi == null) {
    //                return new Size((int)(tSize.Width + 1), Math.Max((int)tSize.Height, minSize));
    //            }

    //            return new Size((int)(tSize.Width + 2 + pSize.Width + 1),
    //                Math.Max((int)tSize.Height, (int)pSize.Height));
    //        }

    //        if (qi != null) {
    //            return new Size((int)pSize.Width, (int)pSize.Height);
    //        }

    //        return new Size(minSize, minSize);
    //    } catch {
    //        // tmpImageCode wird an anderer Stelle verwendet
    //        Develop.CheckStackForOverflow();
    //        return FormatedText_NeededSize(font, text, qi, minSize);
    //    }
    //}

    #region Methods

    public static SizeF MeasureString(this Font font, string text) => font.MeasureString(text, StringFormat.GenericDefault);

    public static SizeF MeasureString(this Font font, string text, StringFormat stringFormat) {
        if (string.IsNullOrEmpty(text))
            return SizeF.Empty;

        try {
            // Graphics-Objekt wiederverwendbar machen über static
            using var g = Graphics.FromHwnd(IntPtr.Zero);
            SetTextRenderingHint(g, font);

            // Prüfen auf Leerzeichen am Ende
            if (text.EndsWith(" ")) {
                // Wir messen den Text mit einem zusätzlichen x am Ende
                var withX = g.MeasureString(text + 'x', font, 9999, stringFormat);
                var x = g.MeasureString("x", font, 9999, stringFormat);

                return new SizeF(withX.Width - x.Width, withX.Height);
            }

            return g.MeasureString(text, font, 9999, stringFormat);
        } catch {
            return SizeF.Empty;
        }
    }

    public static void SetTextRenderingHint(Graphics gr, Font font) =>
                //http://csharphelper.com/blog/2014/09/understand-font-aliasing-issues-in-c/
                gr.TextRenderingHint = font.Size < 11 ? TextRenderingHint.ClearTypeGridFit : TextRenderingHint.AntiAlias;

    #endregion
}