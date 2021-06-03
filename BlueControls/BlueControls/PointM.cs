#region BlueElements - a collection of useful tools, database and controls
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
#endregion

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
    public sealed class PointM : IComparable, IMoveable {
        #region  Variablen-Deklarationen 

        private object _parent;
        private decimal _x;
        private decimal _y;

        private string _tag;

        #endregion

        #region  Event-Deklarationen + Delegaten 

        public event System.EventHandler Moved;

        #endregion

        #region  Construktor + Initialize 

        public PointM(object parent, string name, decimal startX, decimal startY, decimal laenge, decimal alpha) : this(parent) {

            Name = name;
            var tempVar = GeometryDF.PolarToCartesian(laenge, Convert.ToDouble(alpha));
            _x = startX + tempVar.X;
            _y = startY + tempVar.Y;
            _tag = string.Empty;
        }
        public PointM(PointM startPoint, decimal laenge, decimal alpha) : this(null, string.Empty, startPoint.X, startPoint.Y, laenge, alpha) { }
        public PointM(PointF startPoint, decimal laenge, decimal alpha) : this(null, string.Empty, (decimal)startPoint.X, (decimal)startPoint.Y, laenge, alpha) { }

        public PointM(object parent, string codeToParse) : this(parent) {

            Parse(codeToParse);

        }

        public PointM(object parent, string name, decimal x, decimal y, string tag) {
            _parent = parent;
            _x = x;
            _y = y;
            Name = name;
            _tag = tag;
        }
        public PointM() : this(null, string.Empty, 0m, 0m, string.Empty) { }
        public PointM(object parent) : this(parent, string.Empty, 0m, 0m, string.Empty) { }
        public PointM(string name, decimal x, decimal y) : this(null, name, x, y, string.Empty) { }
        public PointM(object parent, string name, int x, int y) : this(parent, name, x, y, string.Empty) { }
        public PointM(object parent, string name, decimal x, decimal y) : this(parent, name, x, y, string.Empty) { }
        public PointM(PointF point) : this(null, string.Empty, (decimal)point.X, (decimal)point.Y, string.Empty) { }
        public PointM(int x, int y) : this(null, string.Empty, x, y, string.Empty) { }
        public PointM(double x, double y) : this(null, string.Empty, (decimal)x, (decimal)y, string.Empty) { }
        public PointM(decimal x, decimal y) : this(null, string.Empty, x, y, string.Empty) { }
        public PointM(PointM point) : this(null, string.Empty, point.X, point.Y, string.Empty) { }
        public PointM(object parent, PointM template) : this(parent, template.Name, template.X, template.Y, template.Tag) { }

        #endregion

        #region  Properties 

        public string Name { get; private set; }

        public object Parent {
            get => _parent;

            set {
                if (_parent == value) { return; }
                _parent = value;
            }
        }

        public decimal X {
            get => _x;

            set {
                if (_x == value) { return; }
                _x = value;
                OnMoved();
            }
        }

        public decimal Y {
            get => _y;

            set {
                if (_y == value) { return; }
                _y = value;
                OnMoved();
            }
        }

        public string Tag {
            get => _tag;

            set {
                if (_tag == value) { return; }
                _tag = value;
            }
        }
        #endregion

        public void Parse(string codeToParse) {

            foreach (var pair in codeToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "parentname":
                        break;

                    case "name":
                        Name = pair.Value.FromNonCritical();
                        break;

                    case "tag":
                        _tag = pair.Value.FromNonCritical();
                        break;

                    case "x":
                        _x = decimal.Parse(pair.Value);
                        break;

                    case "y":
                        _y = decimal.Parse(pair.Value);
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

        public static explicit operator PointF(PointM p) {
            return new((float)p.X, (float)p.Y);
        }

        public static explicit operator Point(PointM p) {
            return new((int)p.X, (int)p.Y);
        }

        public static PointM Empty() {
            return new PointM(0m, 0m);
        }

        public override string ToString() {
            var t = "{";

            if (_parent != null) {
                switch (_parent) {
                    case BasicPadItem _:
                        t = t + "ParentName=" + ((BasicPadItem)_parent).Internal.ToNonCritical() + ", ";
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

            if (!string.IsNullOrEmpty(_tag)) {
                t = t + "Tag=" + _tag.ToNonCritical() + ", ";
            }

            //t = t + "Fix=" + Fix.ToPlusMinus() + ", ";

            //if (!_canUsedForAutoRelation) {
            //    t = t + "GetSnapped=" + _canUsedForAutoRelation + ", ";
            //}
            //if (_primaryGridSnapPoint) {
            //    t = t + "PrimaryGridSnapPoint=" + _primaryGridSnapPoint + ", ";
            //}

            return t.Trim(", ") + "}";
        }

        public bool IsOnScreen(decimal zoom, decimal shiftX, decimal shiftY, Rectangle displayRectangle) {

            var tx = (_x * zoom) - shiftX;
            var ty = (_y * zoom) - shiftY;

            return tx >= displayRectangle.Left && ty >= displayRectangle.Top && tx <= displayRectangle.Right && ty <= displayRectangle.Bottom;
        }

        public void Draw(Graphics gr, decimal zoom, decimal shiftX, decimal shiftY, enDesign type, enStates state) {
            var tx = (_x * zoom) - shiftX + (zoom / 2);
            var ty = (_y * zoom) - shiftY + (zoom / 2);

            var r = new Rectangle((int)(tx - 4), (int)(ty - 4), 9, 9);

            //if (!_UserSelectable) {
            //    type = enDesign.Button_EckpunktSchieber_Phantom;
            //    state = enStates.Standard;
            //}

            Skin.Draw_Back(gr, type, state, r, null, false);
            Skin.Draw_Border(gr, type, state, r);

            //if (!string.IsNullOrEmpty(textToDraw)) {
            //    for (var x = -1; x < 2; x++) {
            //        for (var y = -1; y < 2; y++) {
            //            gr.DrawString(textToDraw, SimpleArial, Brushes.White, (float)tx + x, (float)ty + y - 16);
            //        }

            //    }
            //    gr.DrawString(textToDraw, SimpleArial, Brushes.Black, (float)tx, (float)ty - 16);
            //}

        }

        public PointF ZoomAndMove(AdditionalDrawing e) {
            return ZoomAndMove(e.Zoom, e.ShiftX, e.ShiftY);
        }

        public PointF ZoomAndMove(decimal zoom, decimal shiftX, decimal shiftY) {
            return new PointF((float)((_x * zoom) - shiftX + (zoom / 2)), (float)((_y * zoom) - shiftY + (zoom / 2)));
        }

        public void SetTo(decimal x, decimal y) {
            if (x == _x && y == _y) { return; }
            _x = x;
            _y = y;
            OnMoved();
        }

        public void SetTo(double x, double y) => SetTo((decimal)x, (decimal)y);

        public void SetTo(PointM StartPoint, decimal Länge, decimal Alpha) {
            var tempVar = GeometryDF.PolarToCartesian(Länge, Convert.ToDouble(Alpha));
            SetTo(StartPoint.X + tempVar.X, Y = StartPoint.Y + tempVar.Y);
        }

        public void SetTo(PointM Point) => SetTo(Point.X, Point.Y);

        public void SetTo(Point point) => SetTo(point.X, (decimal)point.Y);

        public void SetTo(int x, int y) => SetTo(x, (decimal)y);

        public int CompareTo(object obj) {
            if (obj is PointM tobj) {
                // hierist es egal, ob es ein DoAlways ist oder nicht. Es sollen nur Bedingugen VOR Aktionen kommen
                return CompareKey().CompareTo(tobj.CompareKey());
            } else {
                Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!");
                return 0;
            }
        }

        internal string CompareKey() {
            //if (_x > int.MaxValue / 10.0m || _y > int.MaxValue / 10.0m || _x < int.MinValue / 10.0m || _y < int.MinValue / 10.0m) { return "ZZZ-ZZZ"; }
            return _y.ToString(Constants.Format_Float5_1) + "-" + _x.ToString(Constants.Format_Float5_1);
        }

        public decimal DistanzZuLinie(PointM P1, PointM P2) {
            return DistanzZuLinie(P1.X, P1.Y, P2.X, P2.Y);
        }

        public decimal DistanzZuLinie(decimal X1, decimal Y1, decimal X2, decimal Y2) {
            var pal = GeometryDF.PointOnLine(this, X1, Y1, X2, Y2);
            return GeometryDF.Länge(this, pal);
        }

        public void OnMoved() {
            Moved?.Invoke(this, System.EventArgs.Empty);
        }

        public decimal Magnitude => (decimal)Math.Sqrt((double)((_x * _x) + (_y * _y)));

        public void Normalize() {
            var magnitude = Magnitude;
            _x /= magnitude;
            _y /= magnitude;
        }

        public decimal DotProduct(PointM vector) {
            return (_x * vector._x) + (_y * vector._y);
        }

        public void Move(decimal x, decimal y) {

            if (x == 0 && y == 0) { return; }
            _x += x;
            _y += y;
            OnMoved();
        }

        public static PointM operator +(PointM a, PointM b) {
            return new PointM(a._x + b._x, a._y + b._y);
        }

        public static PointM operator -(PointM a) {
            return new PointM(-a._x, -a._y);
        }

        public static PointM operator -(PointM a, PointM b) {
            return new PointM(a._x - b._x, a._y - b._y);
        }

        public static PointM operator *(PointM a, decimal b) {
            return new PointM(a._x * b, a._y * b);
        }
    }
}

