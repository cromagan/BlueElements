// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.Classes;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.ClassesStatic.Constants;

namespace BlueControls.Extended_Text;

internal class ExtCharImageCode : ExtChar {

    #region Fields

    private QuickImage? _qi;

    #endregion

    #region Constructors

    internal ExtCharImageCode(ExtText parent, PadStyles style, List<string> overrideTags, QuickImage? qi) : base(parent, style, overrideTags) => _qi = qi;

    internal ExtCharImageCode(ExtText parent, int styleFromPos, QuickImage? qi) : base(parent, styleFromPos) => _qi = qi;

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) {
        // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)

        if (_qi == null) { return; }
        try {
            gr.DrawImage(Math.Abs(zoom - 1) < DefaultTolerance ? _qi : _qi.Scale(zoom), controlPos.X, controlPos.Y);
        } catch { }
    }

    public override string HtmlText() => _qi?.HTMLCode ?? string.Empty;

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => true;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => true;

    public override string PlainText() => string.Empty;

    protected override SizeF CalculateSizeCanvas() => _qi == null ? SizeF.Empty : new SizeF(_qi.Width + 1, _qi.Height + 1);

    #endregion
}