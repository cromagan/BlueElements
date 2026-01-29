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

using BlueControls.Classes;
using BlueControls.Enums;
using System.Drawing;

namespace BlueControls.Extended_Text;

internal class ExtCharStoreXCode : ExtChar {

    #region Constructors

    public ExtCharStoreXCode(ExtText parent, PadStyles style, BlueFont font) : base(parent, style, font) { }

    public ExtCharStoreXCode(ExtText parent, int styleFromPos) : base(parent, styleFromPos) { }

    #endregion

    #region Properties

    public static string ClassId => "ExtCharStoreXCode";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) { }

    public override string HtmlText() => "<zbx_store>";

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => false;

    public override string PlainText() => string.Empty;

    protected override SizeF CalculateSizeCanvas() => SizeF.Empty;

    #endregion
}