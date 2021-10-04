// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

    public class ExtChar {

        #region Fields

        public const char StoreX = (char)5;
        public const char Top = (char)4;
        public PointF Pos = PointF.Empty;

        private readonly char _Char;
        private enDesign _Design = enDesign.Undefiniert;
        private SizeF _Size = SizeF.Empty;
        private enStates _State = enStates.Undefiniert;
        private int _Stufe = 4;

        #endregion

        #region Constructors

        internal ExtChar(char charcode, enDesign design, enStates state, BlueFont font, int stufe, enMarkState markState) {
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

        public enDesign Design {
            get => _Design;
            set {
                if (value == _Design) { return; }
                ChangeState(value, _State, _Stufe);
            }
        }

        public enMarkState Marking { get; set; }

        public SizeF Size {
            get {
                if (!_Size.IsEmpty) { return _Size; }
                _Size = Font == null ? new SizeF(0, 16) : _Char < 0 ? Font.CharSize(0f) : Font.CharSize(_Char);
                return _Size;
            }
        }

        public enStates State {
            get => _State;
            set {
                if (value == _State) { return; }
                ChangeState(_Design, value, _Stufe);
            }
        }

        public int Stufe {
            get => _Stufe;
            set {
                if (_Stufe == value) { return; }
                ChangeState(_Design, _State, value);
            }
        }

        internal BlueFont Font { get; private set; } = null;

        #endregion

        #region Methods

        public void Draw(Graphics gr, Point posModificator, float zoom) {
            if (_Char < 20) { return; }
            var DrawX = (Pos.X * zoom) + posModificator.X;
            var DrawY = (Pos.Y * zoom) + posModificator.Y;

            if (_Char < (int)enASCIIKey.ImageStart) {
                try {
                    Font?.DrawString(gr, _Char.ToString(), DrawX, DrawY, zoom, StringFormat.GenericTypographic);
                } catch { }
                return;
            }

            if (Math.Abs(zoom - 1) < 0.001) {
                var BNR = QuickImage.Get(_Char - (int)enASCIIKey.ImageStart);
                if (BNR == null) { return; }
                // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)
                gr.DrawImage(BNR.BMP, (int)DrawX, (int)DrawY);
            } else {
                var l = QuickImage.Get(_Char - (int)enASCIIKey.ImageStart);
                if (l == null || l.Width == 0) { l = QuickImage.Get("Warnung|16"); }
                if (l.Width > 0) {
                    gr.DrawImage(QuickImage.Get(l.Name, (int)(l.Width * zoom)).BMP, (int)DrawX, (int)DrawY);
                }
            }
        }

        public bool isLineBreak() => (int)_Char switch {
            11 or 13 or Top => true,
            _ => false,
        };

        public bool isPossibleLineBreak() => _Char.isPossibleLineBreak();

        public bool isSpace() => (int)_Char switch {
            32 or 0 or 9 => true,
            _ => false,
        };

        /// <summary>
        ///
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="drawingPos">Muss bereits Skaliert sein</param>
        /// <returns></returns>
        public bool IsVisible(float zoom, Point drawingPos, Rectangle drawingArea) => (drawingArea.Width < 1 && drawingArea.Height < 1)
|| ((drawingArea.Width <= 0 || (Pos.X * zoom) + drawingPos.X <= drawingArea.Right)
&& (drawingArea.Height <= 0 || (Pos.Y * zoom) + drawingPos.Y <= drawingArea.Bottom)
&& ((Pos.X + Size.Width) * zoom) + drawingPos.X >= drawingArea.Left
&& ((Pos.Y + Size.Height) * zoom) + drawingPos.Y >= drawingArea.Top);

        public bool isWordSeperator() => _Char.isWordSeperator();

        public string ToHTML() => (int)_Char switch {
            13 => "<br>",
            //case enEtxtCodes.HorizontalLine:
            //    return "<hr>";
            11 => string.Empty,
            _ => Convert.ToChar(_Char).ToString().CreateHtmlCodes(true),
        };

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