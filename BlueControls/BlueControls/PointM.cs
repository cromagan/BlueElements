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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls {
    public sealed class PointM : IParseable, IComparable {
        #region  Variablen-Deklarationen 

        private object _parent;
        private decimal _x;
        private decimal _y;
        private bool _canUsedForAutoRelation;

        //private int _order;

        private bool _primaryGridSnapPoint;

        private string _tag;


        private decimal _StoreX;
        private decimal _StoreY;


        public static readonly Font SimpleArial = new("Arial", 8);

        private bool _fix = false;

        private bool _UserSelectable = true;


        #endregion




        #region  Event-Deklarationen + Delegaten 


        #endregion

        #region  Construktor + Initialize 


        // Polar - Construktor
        public PointM(object parent, string name, decimal startX, decimal startY, decimal laenge, decimal alpha) {
            Initialize();
            _parent = parent;
            Name = name;
            var tempVar = GeometryDF.PolarToCartesian(laenge, Convert.ToDouble(alpha));
            _x = startX + tempVar.X;
            _y = startY + tempVar.Y;
        }
        public PointM(PointM startPoint, decimal laenge, decimal alpha) : this(null, "Dummy Point von PointM mit Polarverschiebung", startPoint.X, startPoint.Y, laenge, alpha) { }
        public PointM(PointF startPoint, decimal laenge, decimal alpha) : this(null, "Dummy Point von PointF mit Polarverschiebung", (decimal)startPoint.X, (decimal)startPoint.Y, laenge, alpha) { }

        // Parsen
        public PointM(object parent, string codeToParse) {
            Parse(codeToParse);
            _parent = parent;
        }




        public PointM(object parent, string name, decimal x, decimal y, bool fix, bool canUsedForAutoRelation, bool primaryGridSnapPoint, bool userselectable, string tag) {
            Initialize();
            _parent = parent;
            _x = x;
            _y = y;
            Name = name;
            Fix = fix;
            _canUsedForAutoRelation = canUsedForAutoRelation;
            _primaryGridSnapPoint = primaryGridSnapPoint;
            _tag = tag;
            _UserSelectable = userselectable;
        }
        public PointM() : this(null, "Dummy Point ohne Angaben", 0m, 0m, false, true, false, true, string.Empty) { }
        public PointM(string name, decimal x, decimal y) : this(null, name, x, y, false, true, false, true, string.Empty) { }
        public PointM(object parent, string name, int x, int y, bool fix, bool canUsedForAutoRelation, bool primaryGridSnapPoint) : this(parent, name, x, y, fix, canUsedForAutoRelation, primaryGridSnapPoint, true, string.Empty) { }
        public PointM(object parent, string name, decimal x, decimal y) : this(parent, name, x, y, false, true, false, true, string.Empty) { }
        public PointM(PointF point) : this(null, "Dummy Point von PointF", (decimal)point.X, (decimal)point.Y, false, true, false, true, string.Empty) { }
        public PointM(int x, int y) : this(null, "Dummy Point von IntX und IntY", x, y, false, true, false, true, string.Empty) { }
        public PointM(double x, double y) : this(null, "Dummy Point von DoubleX und DoubleY", (decimal)x, (decimal)y, false, true, false, true, string.Empty) { }
        public PointM(decimal x, decimal y) : this(null, "Dummy Point von DecimalX und DecimalY", x, y, false, true, false, true, string.Empty) { }
        public PointM(PointM point) : this(null, "Dummy Point von PointM", point.X, point.Y, false, true, false, true, string.Empty) { }
        public PointM(object parent, string name, int x, int y, bool fix) : this(parent, name, x, y, fix, true, false, true, string.Empty) { }

        public PointM(object parent, string name, int x, int y, bool fix, bool canUsedForAutoRelation) : this(parent, name, x, y, fix, canUsedForAutoRelation, false, true, string.Empty) { }

        public PointM(object parent, string name, decimal x, decimal y, bool fix, bool canUsedForAutoRelation) : this(parent, name, x, y, fix, canUsedForAutoRelation, false, true, string.Empty) { }


        public PointM(object parent, PointM template) : this(parent, template.Name, template.X, template.Y, template.Fix, template.CanUsedForAutoRelation, template.PrimaryGridSnapPoint, template.UserSelectable, template.Tag) { }




        public void Initialize() {
            Name = string.Empty;
            _parent = null;
            _x = 0;
            _y = 0;
            Fix = false;
            _canUsedForAutoRelation = true;
            _primaryGridSnapPoint = false;
            _tag = string.Empty;
        }

        #endregion

        #region  Properties 
        public bool IsParsing { get; private set; }

        public string Name { get; private set; }

        public object Parent {
            get => _parent;


            set {
                if (_parent == value) { return; }
                _parent = value;
                OnChanged();
            }

        }

        public decimal X {
            get => _x;

            set {
                if (_x == value) { return; }
                _x = value;
                OnChanged();
            }
        }

        public decimal Y {
            get => _y;

            set {
                if (_y == value) { return; }
                _y = value;
                OnChanged();
            }
        }

        //public enXY Moveable {
        //    get {
        //        return _moveable;
        //    }

        //    set {
        //        if (_moveable == value) { return; }
        //        _moveable = value;
        //        OnChanged();
        //    }
        //}

        public bool CanUsedForAutoRelation {
            get => _canUsedForAutoRelation;

            set {
                if (_canUsedForAutoRelation == value) { return; }
                _canUsedForAutoRelation = value;
                OnChanged();
            }
        }


        public bool Fix {
            get => _fix;

            set {
                if (_fix == value) { return; }
                _fix = value;
                OnChanged();
            }
        }


        public bool UserSelectable {
            get => _UserSelectable;

            set {
                if (_UserSelectable == value) { return; }
                _UserSelectable = value;
                OnChanged();
            }
        }

        //public int Order
        //{
        //    get
        //    {
        //        return _order;
        //    }

        //    set
        //    {
        //        if (_order == value) { return; }
        //        _order = value;
        //        OnChanged();
        //    }
        //}

        public bool PrimaryGridSnapPoint {
            get => _primaryGridSnapPoint;

            set {
                if (_primaryGridSnapPoint == value) { return; }
                _primaryGridSnapPoint = value;
                OnChanged();
            }
        }

        public string Tag {
            get => _tag;

            set {
                if (_tag == value) { return; }
                _tag = value;
                OnChanged();
            }
        }
        #endregion

        public event EventHandler Changed;


        public void Parse(string ToParse) {
            Parse(ToParse, null);
        }

        public void Parse(string ToParse, object parent) {
            IsParsing = true;
            Initialize();

            _parent = parent;

            foreach (var pair in ToParse.GetAllTags()) {
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
                    case "fix":
                        Fix = pair.Value.FromPlusMinus();
                        //if (pair.Value.FromPlusMinus()) {
                        //    _moveable = enXY.none;
                        //} else {
                        //    _moveable = enXY.XY;
                        //}

                        break;
                    case "moveable":
                        //   _moveable = (enXY)int.Parse(pair.Value);


                        Fix = (enXY)int.Parse(pair.Value) == enXY.none;
                        break;

                    case "getsnapped":
                        _canUsedForAutoRelation = pair.Value.FromPlusMinus();
                        break;
                    case "primarygridsnappoint":
                        _primaryGridSnapPoint = pair.Value.FromPlusMinus();
                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            IsParsing = false;
        }

        public static explicit operator PointF(PointM p) {
            return new((float)p.X, (float)p.Y);
        }

        public static explicit operator Point(PointM p) {
            return new((int)p.X, (int)p.Y);
        }



        //public PointF ToPointF()
        //{
        //    return new PointF((float)_x, (float)_y);
        //}

        //public Point ToPoint()
        //{
        //    return new Point((int)_x, (int)_y);
        //}

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

            t = t + "Fix=" + Fix.ToPlusMinus() + ", ";


            //t = t + "Moveable=" + ((int)_moveable).ToString() + ", ";

            if (!_canUsedForAutoRelation) {
                t = t + "GetSnapped=" + _canUsedForAutoRelation + ", ";
            }
            if (_primaryGridSnapPoint) {
                t = t + "PrimaryGridSnapPoint=" + _primaryGridSnapPoint + ", ";
            }


            return t.Trim(", ") + "}";
        }


        public bool IsOnScreen(decimal zoom, decimal shiftX, decimal shiftY, Rectangle displayRectangle) {

            var tx = _x * zoom - shiftX;
            var ty = _y * zoom - shiftY;

            if (tx < displayRectangle.Left || ty < displayRectangle.Top) { return false; }
            if (tx > displayRectangle.Right || ty > displayRectangle.Bottom) { return false; }
            return true;

        }


        public void Draw(Graphics gr, decimal zoom, decimal shiftX, decimal shiftY, enDesign type, enStates state, string textToDraw) {
            var tx = _x * zoom - shiftX + zoom / 2;
            var ty = _y * zoom - shiftY + zoom / 2;

            var r = new Rectangle((int)(tx - 4), (int)(ty - 4), 9, 9);


            if (!_UserSelectable) {
                type = enDesign.Button_EckpunktSchieber_Phantom;
                state = enStates.Standard;
            }


            Skin.Draw_Back(gr, type, state, r, null, false);
            Skin.Draw_Border(gr, type, state, r);



            if (!string.IsNullOrEmpty(textToDraw)) {
                for (var x = -1; x < 2; x++) {
                    for (var y = -1; y < 2; y++) {
                        gr.DrawString(textToDraw, SimpleArial, Brushes.White, (float)tx + x, (float)ty + y - 16);
                    }

                }
                gr.DrawString(textToDraw, SimpleArial, Brushes.Black, (float)tx, (float)ty - 16);
            }

        }

        public PointF ZoomAndMove(AdditionalDrawing e) {
            return ZoomAndMove(e.Zoom, e.ShiftX, e.ShiftY);
        }

        public PointF ZoomAndMove(decimal zoom, decimal shiftX, decimal shiftY) {
            return new PointF((float)(_x * zoom - shiftX + zoom / 2), (float)(_y * zoom - shiftY + zoom / 2));
        }


        public void SetTo(decimal cx, decimal cy) {
            _x = cx;
            _y = cy;
        }

        public void SetTo(double cx, double cy) {
            _x = (decimal)cx;
            _y = (decimal)cy;
        }

        public void SetTo(PointM StartPoint, decimal Länge, decimal Alpha) {
            var tempVar = GeometryDF.PolarToCartesian(Länge, Convert.ToDouble(Alpha));
            _x = StartPoint.X + tempVar.X;
            _y = StartPoint.Y + tempVar.Y;
        }

        public void SetTo(PointM Point) {
            _x = Point.X;
            _y = Point.Y;
        }


        public void SetTo(Point point) {
            _x = point.X;
            _y = point.Y;
        }


        public void SetTo(int cx, int cy) {
            _x = cx;
            _y = cy;
        }


        private bool CanMove(enXY toCheck, List<clsPointRelation> Rel, List<clsPointRelation> Alredychecked) {
            if (Fix) { return false; }


            foreach (var ThisRelation in Rel) {
                if (ThisRelation != null && !Alredychecked.Contains(ThisRelation) && ThisRelation.Points.Contains(this) && ThisRelation.Performs(false)) {
                    Alredychecked.Add(ThisRelation);

                    if (ThisRelation.Connects().HasFlag(toCheck)) {
                        var Move = true;
                        foreach (var thispoint in ThisRelation.Points) {
                            if (thispoint != this) { Move = thispoint.CanMove(toCheck, Rel, Alredychecked); }
                            if (!Move) { return false; }
                        }
                    }
                }
            }


            return true;
        }

        public bool CanMoveX(List<clsPointRelation> Rel) {
            var Alredychecked = new List<clsPointRelation>();

            return CanMove(enXY.X, Rel, Alredychecked);
        }

        public bool CanMoveY(List<clsPointRelation> Rel) {
            var Alredychecked = new List<clsPointRelation>();

            return CanMove(enXY.Y, Rel, Alredychecked);
        }

        public bool CanMove(List<clsPointRelation> Rel) {
            if (CanMoveX(Rel)) { return true; }
            if (CanMoveY(Rel)) { return true; }

            return false;
        }

        public void Store() {
            _StoreX = _x;
            _StoreY = _y;
        }


        public void ReStore() {
            _x = _StoreX;
            _y = _StoreY;
        }



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

        public void OnChanged() {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }


        public decimal Magnitude => (decimal)Math.Sqrt((double)(_x * _x + _y * _y));

        public void Normalize() {
            var magnitude = Magnitude;
            _x /= magnitude;
            _y /= magnitude;
        }



        public decimal DotProduct(PointM vector) {
            return _x * vector._x + _y * vector._y;
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

