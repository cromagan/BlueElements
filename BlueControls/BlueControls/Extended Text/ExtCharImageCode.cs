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

using BlueBasics;
using BlueControls.Enums;
using System;
using System.Drawing;

namespace BlueControls {

    internal class ExtCharImageCode : ExtChar {

        #region Fields

        private QuickImage _QI;

        #endregion

        #region Constructors

        public ExtCharImageCode(QuickImage qi) : base() { _QI = qi; }

        public ExtCharImageCode(string imagecode) : base() { _QI = QuickImage.Get(imagecode); }

        #endregion

        #region Properties

        public override enDesign Design { get; set; }

        public override SizeF Size {
            get => _QI == null ? SizeF.Empty : new SizeF(_QI.Width + 1, _QI.Height + 1);
        }

        public override enStates State { get; set; }
        public override int Stufe { get; set; }

        #endregion

        #region Methods

        public override void Draw(Graphics gr, Point posModificator, float zoom) {
            // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)

            var DrawX = (int)((Pos.X * zoom) + posModificator.X);
            var DrawY = (int)((Pos.Y * zoom) + posModificator.Y);

            try {
                if (Math.Abs(zoom - 1) < 0.001) {
                    gr.DrawImage(_QI, DrawX, DrawY);
                } else {
                    gr.DrawImage(QuickImage.Get(_QI.Name, (int)(_QI.Width * zoom)), DrawX, DrawY);
                }
            } catch { }
        }

        public override string HTMLText() => "<IMAGECODE=" + _QI.Code + ">";

        public override bool isLineBreak() => false;

        public override bool isPossibleLineBreak() => true;

        public override bool isSpace() => false;

        public override bool isWordSeperator() => true;

        public override string PlainText() => string.Empty;

        #endregion
    }
}