// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueControls.Enums;
using System.Drawing;

namespace BlueControls.Extended_Text {

    internal class ExtCharCRLFCode : ExtChar {

        #region Constructors

        internal ExtCharCRLFCode() : this(enDesign.TextBox, enStates.Standard, null, 4) { }

        internal ExtCharCRLFCode(enDesign design, enStates state, BlueFont? font, int stufe) : base(design, state, font, stufe) { }

        #endregion

        #region Methods

        public override void Draw(Graphics gr, Point posModificator, float zoom) { }

        public override string HtmlText() => "<BR>";

        public override bool IsLineBreak() => true;

        public override bool IsPossibleLineBreak() => true;

        public override bool IsSpace() => false;

        public override bool IsWordSeperator() => true;

        public override string PlainText() => "\r\n";

        protected override SizeF CalculateSize() => Font == null ? new SizeF(0, 16) : new SizeF(0, Font.CharSize(65).Height);

        #endregion
    }
}