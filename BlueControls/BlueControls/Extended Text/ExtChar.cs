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

#nullable enable

using BlueControls.Enums;
using System.Drawing;

namespace BlueControls.Extended_Text {

    public abstract class ExtChar {

        #region Fields

        public PointF Pos = PointF.Empty;

        private enDesign _design;

        private BlueFont? _font;

        private SizeF _size;
        private enStates _state;

        private int _stufe;

        #endregion

        #region Constructors

        protected ExtChar(enDesign design, enStates state, BlueFont? font, int stufe) : base() {
            _design = design;
            _state = state;
            _stufe = stufe;
            _font = font;
            _size = SizeF.Empty;
        }

        #endregion

        #region Properties

        public enDesign Design {
            get => _design;
            set {
                if (value == _design) { return; }
                ChangeState(value, _state, _stufe);
            }
        }

        public BlueFont? Font {
            get {
                if (_font != null) {
                    return _font;
                }

                _font = Design == enDesign.Undefiniert || State == enStates.Undefiniert ? null : Skin.GetBlueFont(Design, State, Stufe);
                _size = SizeF.Empty;
                return _font;
            }

            protected set => _font = value;
        }

        public enMarkState Marking { get; set; }

        public SizeF Size {
            get {
                if (_size.IsEmpty) { _size = CalculateSize(); }
                return _size;
            }
        }

        public enStates State {
            get => _state;
            set {
                if (value == _state) { return; }
                ChangeState(_design, value, _stufe);
            }
        }

        public int Stufe {
            get => _stufe;
            set {
                if (_stufe == value) { return; }
                ChangeState(_design, _state, value);
            }
        }

        #endregion

        #region Methods

        public abstract void Draw(Graphics gr, Point posModificator, float zoom);

        public abstract string HtmlText();

        public abstract bool IsLineBreak();

        public abstract bool IsPossibleLineBreak();

        public abstract bool IsSpace();

        /// <summary>
        ///
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="drawingPos">Muss bereits Skaliert sein</param>
        /// <returns></returns>
        public bool IsVisible(float zoom, Point drawingPos, Rectangle drawingArea) => (drawingArea.Width < 1 && drawingArea.Height < 1) ||
                ((drawingArea.Width <= 0 || (Pos.X * zoom) + drawingPos.X <= drawingArea.Right)
                && (drawingArea.Height <= 0 || (Pos.Y * zoom) + drawingPos.Y <= drawingArea.Bottom)
                && ((Pos.X + Size.Width) * zoom) + drawingPos.X >= drawingArea.Left
                && ((Pos.Y + Size.Height) * zoom) + drawingPos.Y >= drawingArea.Top);

        public abstract bool IsWordSeperator();

        public abstract string PlainText();

        internal void GetStyleFrom(ExtChar c) {
            ChangeState(c.Design, c.State, c.Stufe);
            Font = c.Font;
        }

        protected abstract SizeF CalculateSize();

        private void ChangeState(enDesign design, enStates state, int stufe) {
            if (state == _state && stufe == _stufe && design == _design) { return; }
            _size = SizeF.Empty;
            _design = design;
            _state = state;
            _stufe = stufe;
            _font = null;
        }

        #endregion
    }
}