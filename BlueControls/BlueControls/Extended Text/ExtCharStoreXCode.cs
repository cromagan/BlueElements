// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System.Drawing;
using BlueControls.Enums;

namespace BlueControls.Extended_Text;

internal class ExtCharStoreXCode : ExtChar {

    #region Constructors

    internal ExtCharStoreXCode(Design design, States state, BlueFont? font, int stufe) : base(design, state, font, stufe) { }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point posModificator, float zoom) { }

    public override string HtmlText() => "<ZBX_STORE>";

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeperator() => false;

    public override string PlainText() => string.Empty;

    protected override SizeF CalculateSize() => SizeF.Empty;

    #endregion
}