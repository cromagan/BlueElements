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

    public abstract class ExtCharAbstract {

        #region Fields

        public PointF Pos = PointF.Empty;

        #endregion

        #region Constructors

        public ExtCharAbstract() : base() { }

        #endregion

        #region Properties

        public abstract enDesign Design { get; set; }

        public BlueFont Font { get; protected set; } = null;
        public enMarkState Marking { get; set; }
        public abstract SizeF Size { get; }

        public abstract enStates State { get; set; }

        public abstract int Stufe { get; set; }

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

        internal void GetStyleFrom(ExtCharAbstract c) {
            Font = c.Font;

            Design = c.Design;

            //Marking = c.Marking;
            //public abstract SizeF Size { get; }

            State = c.State;

            Stufe = c.Stufe;
        }

        #endregion
    }
}