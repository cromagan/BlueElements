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

    public abstract class ExtChar {

        #region Fields

        public PointF Pos = PointF.Empty;

        private enDesign _Design = enDesign.Undefiniert;

        private BlueFont _Font = null;

        private SizeF _Size = SizeF.Empty;
        private enStates _State = enStates.Undefiniert;

        private int _Stufe = 4;

        #endregion

        #region Constructors

        public ExtChar(enDesign design, enStates state, BlueFont font, int stufe) : base() {
            _Design = design;
            _State = state;
            _Stufe = stufe;
            _Font = font;
            _Size = SizeF.Empty;
        }

        #endregion

        #region Properties

        public enDesign Design {
            get => _Design;
            set {
                if (value == _Design) { return; }
                ChangeState(value, _State, _Stufe);
            }
        }

        public BlueFont Font {
            get {
                if (_Font == null) {
                    _Font = Design == enDesign.Undefiniert || State == enStates.Undefiniert ? null : Skin.GetBlueFont(Design, State, Stufe);
                    _Size = SizeF.Empty;
                }
                return _Font;
            }

            protected set => _Font = value;
        }

        public enMarkState Marking { get; set; }

        public SizeF Size {
            get {
                if (_Size.IsEmpty) { _Size = CalculateSize(); }
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

        #endregion

        #region Methods

        public abstract void Draw(Graphics gr, Point posModificator, float zoom);

        public abstract string HTMLText();

        public abstract bool isLineBreak();

        public abstract bool isPossibleLineBreak();

        public abstract bool isSpace();

        /// <summary>
        ///
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="drawingPos">Muss bereits Skaliert sein</param>
        /// <returns></returns>
        public bool IsVisible(float zoom, Point drawingPos, Rectangle drawingArea) {
            return (drawingArea.Width < 1 && drawingArea.Height < 1) ||
                ((drawingArea.Width <= 0 || (Pos.X * zoom) + drawingPos.X <= drawingArea.Right)
                && (drawingArea.Height <= 0 || (Pos.Y * zoom) + drawingPos.Y <= drawingArea.Bottom)
                && ((Pos.X + Size.Width) * zoom) + drawingPos.X >= drawingArea.Left
                && ((Pos.Y + Size.Height) * zoom) + drawingPos.Y >= drawingArea.Top);
        }

        public abstract bool isWordSeperator();

        public abstract string PlainText();

        internal void GetStyleFrom(ExtChar c) {
            ChangeState(c.Design, c.State, c.Stufe);
            Font = c.Font;
        }

        protected abstract SizeF CalculateSize();

        private void ChangeState(enDesign design, enStates state, int stufe) {
            if (state == _State && stufe == _Stufe && design == _Design) { return; }
            _Size = SizeF.Empty;
            _Design = design;
            _State = state;
            _Stufe = stufe;
            _Font = null;
        }

        #endregion
    }
}