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
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection {

    public abstract class RectanglePadItem : BasicPadItem {

        #region Fields

        protected PointM? PL;
        protected PointM? PLo;
        protected PointM? PLu;
        protected PointM? PO;
        protected PointM? PR;
        protected PointM? PRo;
        protected PointM? PRu;
        protected PointM? PU;
        private int _drehwinkel;

        #endregion

        #region Constructors

        protected RectanglePadItem(string internalname) : base(internalname) {
            PLo = new PointM(this, "LO", 0, 0);
            PRo = new PointM(this, "RO", 0, 0);
            PRu = new PointM(this, "RU", 0, 0);
            PLu = new PointM(this, "LU", 0, 0);
            PL = new PointM(this, "L", 0, 0);
            PR = new PointM(this, "R", 0, 0);
            PO = new PointM(this, "O", 0, 0);
            PU = new PointM(this, "U", 0, 0);
            MovablePoint.Add(PLo);
            MovablePoint.Add(PRo);
            MovablePoint.Add(PLu);
            MovablePoint.Add(PRu);
            MovablePoint.Add(PL);
            MovablePoint.Add(PR);
            MovablePoint.Add(PU);
            MovablePoint.Add(PO);
            PointsForSuccesfullyMove.Add(PLo);
            PointsForSuccesfullyMove.Add(PRu);
            Drehwinkel = 0;
        }

        #endregion

        #region Properties

        public int Drehwinkel {
            get => _drehwinkel;
            set {
                if (_drehwinkel == value) { return; }
                _drehwinkel = value;
                OnChanged();
            }
        }

        #endregion

        #region Methods

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new()
            {
                new FlexiControl(),
                new FlexiControlForProperty(this, "Drehwinkel"),
                new FlexiControlForProperty(this, "Größe_fixiert")
            };
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "fixsize": // TODO: Entfernt am 24.05.2021
                    //_größe_fixiert = value.FromPlusMinus();
                    return true;

                case "rotation":
                    _drehwinkel = int.Parse(value);
                    return true;
            }
            return false;
        }

        public override void PointMoved(object sender, MoveEventArgs e) {
            base.PointMoved(sender, e);
            var x = 0f;
            var y = 0f;

            var point = (PointM)sender;

            if (point != null) {
                x = point.X;
                y = point.Y;
            }

            if (point == PLo) {
                if (e.Y) { PO.Y = y; }
                if (e.X) { PL.X = x; }
            }

            if (point == PRo) {
                if (e.Y) { PO.Y = y; }
                if (e.X) { PR.X = x; }
            }

            if (point == PLu) {
                if (e.X) { PL.X = x; }
                if (e.Y) { PU.Y = y; }
            }

            if (point == PRu) {
                if (e.X) { PR.X = x; }
                if (e.Y) { PU.Y = y; }
            }

            if (point == PO && e.Y) {
                PLo.Y = y;
                PRo.Y = y;
            }

            if (point == PU && e.Y) {
                PLu.Y = y;
                PRu.Y = y;
            }

            if (point == PL && e.X) {
                PLo.X = x;
                PLu.X = x;
            }

            if (point == PR && e.X) {
                PRo.X = x;
                PRu.X = x;
            }

            SizeChanged();
        }

        public void SetCoordinates(RectangleF r, bool overrideFixedSize) {
            if (!overrideFixedSize) {
                var vr = r.PointOf(enAlignment.Horizontal_Vertical_Center);
                var ur = UsedArea;
                PLo.SetTo(vr.X - (ur.Width / 2), vr.Y - (ur.Height / 2));
                PRu.SetTo(PLo.X + ur.Width, PLo.Y + ur.Height);
            } else {
                PLo.SetTo(r.PointOf(enAlignment.Top_Left));
                PRu.SetTo(r.PointOf(enAlignment.Bottom_Right));
            }
        }

        public virtual void SizeChanged() {
            // Punkte immer komplett setzen. Um eventuelle Parsing-Fehler auszugleichen
            PL.SetTo(PLo.X, PLo.Y + ((PLu.Y - PLo.Y) / 2));
            PR.SetTo(PRo.X, PLo.Y + ((PLu.Y - PLo.Y) / 2));
            PU.SetTo(PLo.X + ((PRo.X - PLo.X) / 2), PRu.Y);
            PO.SetTo(PLo.X + ((PRo.X - PLo.X) / 2), PRo.Y);
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (Drehwinkel != 0) { t = t + "Rotation=" + Drehwinkel + ", "; }
            return t.Trim(", ") + "}";
        }

        protected override RectangleF CalculateUsedArea() => PLo == null || PRu == null ? RectangleF.Empty
: new RectangleF(Math.Min(PLo.X, PRu.X), Math.Min(PLo.Y, PRu.Y), Math.Abs(PRu.X - PLo.X), Math.Abs(PRu.Y - PLo.Y));

        protected override void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, float zoom, float shiftX, float shiftY, bool forPrinting) {
            try {
                if (!forPrinting) {
                    if (zoom > 1) {
                        gr.DrawRectangle(new Pen(Color.Gray, zoom), drawingCoordinates);
                    } else {
                        gr.DrawRectangle(ZoomPad.PenGray, drawingCoordinates);
                    }
                    if (drawingCoordinates.Width < 1 || drawingCoordinates.Height < 1) {
                        gr.DrawEllipse(new Pen(Color.Gray, 3), drawingCoordinates.Left - 5, drawingCoordinates.Top + 5, 10, 10);
                        gr.DrawLine(ZoomPad.PenGray, drawingCoordinates.PointOf(enAlignment.Top_Left), drawingCoordinates.PointOf(enAlignment.Bottom_Right));
                    }
                }
            } catch { }
        }

        protected override void ParseFinished() => SizeChanged();

        #endregion
    }
}