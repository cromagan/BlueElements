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

    public class ExtChar : ExtCharAbstract {
        //public const char StoreX = (char)5;
        //public const char Top = (char)4;

        #region Fields

        private readonly char _Char;
        private enDesign _Design = enDesign.Undefiniert;
        private SizeF _Size = SizeF.Empty;
        private enStates _State = enStates.Undefiniert;
        private int _Stufe = 4;

        #endregion

        #region Constructors

        internal ExtChar(char charcode, enDesign design, enStates state, BlueFont font, int stufe, enMarkState markState) : base() {
            _Design = design;
            _Char = charcode;
            Font = font;
            _Stufe = stufe;
            _State = state;
            Marking = markState;
        }

        #endregion

        #region Properties

        public int Char => _Char;

        public override enDesign Design {
            get => _Design;
            set {
                if (value == _Design) { return; }
                ChangeState(value, _State, _Stufe);
            }
        }

        public override SizeF Size {
            get {
                if (!_Size.IsEmpty) { return _Size; }
                _Size = Font == null ? new SizeF(0, 16) : _Char < 0 ? Font.CharSize(0f) : Font.CharSize(_Char);
                return _Size;
            }
        }

        public override enStates State {
            get => _State;
            set {
                if (value == _State) { return; }
                ChangeState(_Design, value, _Stufe);
            }
        }

        public override int Stufe {
            get => _Stufe;
            set {
                if (_Stufe == value) { return; }
                ChangeState(_Design, _State, value);
            }
        }

        #endregion

        #region Methods

        public override void Draw(Graphics gr, Point posModificator, float zoom) {
            if (_Char < 20) { return; }
            var DrawX = (Pos.X * zoom) + posModificator.X;
            var DrawY = (Pos.Y * zoom) + posModificator.Y;

            try {
                Font?.DrawString(gr, _Char.ToString(), DrawX, DrawY, zoom, StringFormat.GenericTypographic);
            } catch { }

            //if (Math.Abs(zoom - 1) < 0.001) {
            //    var BNR = QuickImage.Get(_Char - (int)enASCIIKey.ImageStart);
            //    if (BNR == null) { return; }
            //    // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)
            //    gr.DrawImage(BNR, (int)DrawX, (int)DrawY);
            //} else {
            //    var l = QuickImage.Get(_Char - (int)enASCIIKey.ImageStart);
            //    if (l == null || l.Width == 0) { l = QuickImage.Get("Warnung|16"); }
            //    if (l.Width > 0) {
            //        gr.DrawImage(QuickImage.Get(l.Name, (int)(l.Width * zoom)), (int)DrawX, (int)DrawY);
            //    }
            //}
        }

        public override string HTMLText() => Convert.ToChar(_Char).ToString().CreateHtmlCodes(false);

        public override bool isLineBreak() => (int)_Char switch {
            11 or 13 => true,
            _ => false,
        };

        public override bool isPossibleLineBreak() => _Char.isPossibleLineBreak();

        public override bool isSpace() => (int)_Char switch {
            32 or 0 or 9 => true,
            _ => false,
        };

        public override bool isWordSeperator() => _Char.isWordSeperator();

        public override string PlainText() => Convert.ToChar(_Char).ToString();

        private void ChangeState(enDesign design, enStates state, int stufe) {
            if (state == _State && stufe == _Stufe && design == _Design) { return; }
            _Size = SizeF.Empty;
            _Design = design;
            _State = state;
            _Stufe = stufe;
            Font = design == enDesign.Undefiniert || state == enStates.Undefiniert ? null : Skin.GetBlueFont(design, state, stufe);
        }

        #endregion
    }
}