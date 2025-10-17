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

    #region Fields

    public static readonly StringFormat DefaultWithTrailingSpaces = new StringFormat(StringFormat.GenericTypographic) {
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