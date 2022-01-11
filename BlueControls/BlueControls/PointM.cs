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
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System;
using System.Drawing;

namespace BlueControls {

    public sealed class PointM : IMoveable {

        #region Fields

        private double _x;
        private double _y;

        #endregion

        #region Constructors

        public PointM(object parent, string name, double startX, double startY, double laenge, double alpha) : this(parent) {
            Name = name;
            var tempVar = GeometryDF.PolarToCartesian(laenge, Convert.ToDouble(alpha));
            _x = startX + tempVar.X;
            _y = startY + tempVar.Y;
            Tag = string.Empty;
        }

        public PointM(PointM startPoint, double laenge, double alpha) : this(null, string.Empty, startPoint.X, startPoint.Y, laenge, alpha) { }

        public PointM(PointF startPoint, double laenge, double alpha) : this(null, string.Empty, startPoint.X, startPoint.Y, laenge, alpha) { }

        public PointM(object parent, string codeToParse) : this(parent) => Parse(codeToParse);

        public PointM(object parent, string name, double x, double y, string tag) {
            Parent = parent;
            _x = x;
            _y = y;
            Name = name;
            Tag = tag;
        }

        public PointM() : this(null, string.Empty, 0D, 0D, string.Empty) { }

        public PointM(object parent) : this(parent, string.Empty, 0D, 0D, string.Empty) { }

        public PointM(string name, double x, double y) : this(null, name, x, y, string.Empty) { }

        public PointM(object parent, string name, int x, int y) : this(parent, name, x, y, string.Empty) { }

        public PointM(object parent, string name, double x, double y) : this(parent, name, x, y, string.Empty) { }

        public PointM(PointF point) : this(null, string.Empty, point.X, point.Y, string.Empty) { }

        public PointM(int x, int y) : this(null, string.Empty, x, y, string.Empty) { }

        public PointM(double x, double y) : this(null, string.Empty, (double)x, (double)y, string.Empty) { }

        public PointM(PointM point) : this(null, string.Empty, point.X, point.Y, string.Empty) { }

        public PointM(object parent, PointM template) : this(parent, template.Name, template.X, template.Y, template.Tag) { }

        #endregion

        #region Events

        public event EventHandler<MoveEventArgs> Moved;

        public event EventHandler<MoveEventArgs> Moving;

        #endregion

        #region Properties

        public double Magnitude => (double)Math.Sqrt((double)((_x * _x) + (_y * _y)));
        public string Name { get; private set; }
        public object Parent { get; set; }

        public string Tag { get; set; }

        public double X {
            get => _x;
            set {
                if (_x == value) { return; }
                SetTo(value, _y);
            }
        }

        public double Y {
            get => _y;
            set {
                if (_y == value) { return; }
                SetTo(_x, value);
            }
        }

        #endregion

        #region Methods

        public static PointM Empty() => new(0d, 0d);

        public static explicit operator Point(PointM p) => new((int)p.X, (int)p.Y);

        public static explicit operator PointF(PointM p) => new((float)p.X, (float)p.Y);

        public static PointM operator -(PointM a) => new(-a._x, -a._y);

        public static PointM operator -(PointM a, PointM b) => new(a._x - b._x, a._y - b._y);

        public static PointM operator *(PointM a, double b) => new(a._x * b, a._y * b);

        public static PointM operator +(PointM a, PointM b) => new(a._x + b._x, a._y + b._y);

        public double DistanzZuLinie(PointM P1, PointM P2) => DistanzZuLinie(P1.X, P1.Y, P2.X, P2.Y);

        public double DistanzZuLinie(double X1, double Y1, double X2, double Y2) => GeometryDF.Länge(this, GeometryDF.PointOnLine(this, X1, Y1, X2, Y2));

        public double DotProduct(PointM vector) => (_x * vector._x) + (_y * vector._y);

        public void Draw(Graphics gr, double zoom, double shiftX, double shiftY, enDesign type, enStates state) {
            var tx = (_x * zoom) - shiftX + (zoom / 2);
            var ty = (_y * zoom) - shiftY + (zoom / 2);
            Rectangle r = new((int)(tx - 4), (int)(ty - 4), 9, 9);
            //if (!_UserSelectable) {
            //    type = enDesign.Button_EckpunktSchieber_Phantom;
            //    state = enStates.Standard;
            //}
            Skin.Draw_Back(gr, type, state, r, null, false);
            Skin.Draw_Border(gr, type, state, r);
            //if (!string.IsNullOrEmpty(textToDraw)) {
            //    for (var x = -1; x < 2; x++) {
            //        for (var y = -1; y < 2; y++) {
            //            BlueFont.DrawString(GR,textToDraw, SimpleArial, Brushes.White, (float)tx + x, (float)ty + y - 16);
            //        }
            //    }
            //    BlueFont.DrawString(GR,textToDraw, SimpleArial, Brushes.Black, (float)tx, (float)ty - 16);
            //}
        }

        public void Move(double x, double y) => SetTo(_x + x, _y + y);

        public void Normalize() {
            var magnitude = Magnitude;
            SetTo(_x / magnitude, _y / magnitude);
        }

        public void OnMoved(MoveEventArgs e) => Moved?.Invoke(this, e);

        public void OnMoving(MoveEventArgs e) => Moving?.Invoke(this, e);

        public void Parse(string codeToParse) {
            foreach (var pair in codeToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "parentname":
                        break;

                    case "name":
                        Name = pair.Value.FromNonCritical();
                        break;

                    case "tag":
                        Tag = pair.Value.FromNonCritical();
                        break;

                    case "x":
                        _x = double.Parse(pair.Value);
                        break;

                    case "y":
                        _y = double.Parse(pair.Value);
                        break;

                    case "fix": // TODO: Entfernt, 24.05.2021
                        break;

                    case "moveable": // TODO: Entfernt, 24.05.2021
                        break;

                    case "getsnapped": // TODO: Entfernt, 24.05.2021
                        break;

                    case "primarygridsnappoint": // TODO: Entfernt, 24.05.2021
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        public void SetTo(double x, double y) {
            var mx = x != _x;
            var my = y != _y;

            if (!mx && !my) { return; }
            OnMoving(new MoveEventArgs(mx, my));
            _x = x;
            _y = y;
            OnMoved(new MoveEventArgs(mx, my));
        }

        public void SetTo(PointM StartPoint, double Länge, double Alpha) {
            var tempVar = GeometryDF.PolarToCartesian(Länge, Convert.ToDouble(Alpha));
            SetTo(StartPoint.X + tempVar.X, Y = StartPoint.Y + tempVar.Y);
        }

        public void SetTo(PointM Point) => SetTo(Point.X, Point.Y);

        public void SetTo(Point point) => SetTo(point.X, (double)point.Y);

        public void SetTo(int x, int y) => SetTo(x, (double)y);

        public override string ToString() {
            var t = "{";
            if (Parent != null) {
                switch (Parent) {
                    case BasicPadItem _:
                        t = t + "ParentName=" + ((BasicPadItem)Parent).Internal.ToNonCritical() + ", ";
                        break;

                    case CreativePad _:
                        t += "ParentType=Main, ";
                        break;

                    case ItemCollectionPad _:
                        t += "ParentType=Main, ";
                        break;

                    default:
                        t = t + "ParentType=" + Parent.GetType().FullName + ", ";
                        break;
                }
            }
            t = t + "Name=" + Name.ToNonCritical() + ", ";
            t = t + "X=" + _x + ", ";
            t = t + "Y=" + _y + ", ";
            if (!string.IsNullOrEmpty(Tag)) { t = t + "Tag=" + Tag.ToNonCritical() + ", "; }
            return t.Trim(", ") + "}";
        }

        public PointF ZoomAndMove(AdditionalDrawing e) => ZoomAndMove(e.Zoom, e.ShiftX, e.ShiftY);

        public PointF ZoomAndMove(double zoom, double shiftX, double shiftY) => new((float)((_x * zoom) - shiftX + (zoom / 2)), (float)((_y * zoom) - shiftY + (zoom / 2)));

        #endregion
    }
}