#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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

namespace BlueControls
{
    public sealed class PointM : IParseable, IComparable
    {
        #region  Variablen-Deklarationen 

        private object _parent;
        private decimal _x;
        private decimal _y;
        private enXY _moveable;
        private bool _canUsedForAutoRelation;

        private int _order;

        private bool _primaryGridSnapPoint;

        private string _tag;


        private decimal _StoreX;
        private decimal _StoreY;


        public static Font SimpleArial = new Font("Arial", 8);

        #endregion




        #region  Event-Deklarationen + Delegaten 


        #endregion

        #region  Construktor + Initialize 


        // Polar - Construktor
        public PointM(object parent, string name, decimal startX, decimal startY, decimal laenge, decimal alpha)
        {
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
        public PointM(object parent, string codeToParse)
        {
            Parse(codeToParse);
            _parent = parent;
        }




        public PointM(object parent, string name, decimal x, decimal y, enXY moveable, bool canUsedForAutoRelation, bool primaryGridSnapPoint, string tag)
        {
            Initialize();
            _parent = parent;
            _x = x;
            _y = y;
            Name = name;
            _moveable = moveable;
            _canUsedForAutoRelation = canUsedForAutoRelation;
            _primaryGridSnapPoint = primaryGridSnapPoint;
            _tag = tag;
        }
        public PointM() : this(null, "Dummy Point ohne Angaben", 0m, 0m, enXY.XY, true, false, string.Empty) { }
        public PointM(string name, decimal x, decimal y) : this(null, name, x, y, enXY.XY, true, false, string.Empty) { }
        public PointM(object parent, string name, int x, int y, enXY moveable, bool canUsedForAutoRelation, bool primaryGridSnapPoint) : this(parent, name, (decimal)x, (decimal)y, moveable, canUsedForAutoRelation, primaryGridSnapPoint, string.Empty) { }
        public PointM(object parent, string name, decimal x, decimal y) : this(parent, name, x, y, enXY.XY, true, false, string.Empty) { }
        public PointM(PointF point) : this(null, "Dummy Point von PointF", (decimal)point.X, (decimal)point.Y, enXY.XY, true, false, string.Empty) { }
        public PointM(int x, int y) : this(null, "Dummy Point von IntX und IntY", (decimal)x, (decimal)y, enXY.XY, true, false, string.Empty) { }
        public PointM(double x, double y) : this(null, "Dummy Point von DoubleX und DoubleY", (decimal)x, (decimal)y, enXY.XY, true, false, string.Empty) { }
        public PointM(decimal x, decimal y) : this(null, "Dummy Point von DecimalX und DecimalY", x, y, enXY.XY, true, false, string.Empty) { }
        public PointM(PointM point) : this(null, "Dummy Point von PointM", point.X, point.Y, enXY.XY, true, false, string.Empty) { }
        public PointM(object parent, string name, int x, int y, enXY moveable) : this(parent, name, (decimal)x, (decimal)y, moveable, true, false, string.Empty) { }

        public PointM(object parent, string name, int x, int y, enXY moveable, bool canUsedForAutoRelation) : this(parent, name, (decimal)x, (decimal)y, moveable, canUsedForAutoRelation, false, string.Empty) { }

        public PointM(object parent, string name, decimal x, decimal y, enXY moveable, bool canUsedForAutoRelation) : this(parent, name, x, y, moveable, canUsedForAutoRelation, false, string.Empty) { }


        public PointM(object parent, PointM template) : this(parent, template.Name, template.X, template.Y, template.Moveable, template.CanUsedForAutoRelation, template.PrimaryGridSnapPoint, template.Tag) { }




        public void Initialize()
        {
            Name = string.Empty;
            _parent = null;
            _x = 0;
            _y = 0;
            _moveable = enXY.XY;
            _canUsedForAutoRelation = true;
            _primaryGridSnapPoint = false;
            _tag = string.Empty;
            _order = int.MaxValue; // Muß sein, daß nicht angezeigte Punkte immer verschoben werden können und nie fix sind.
        }

        #endregion

        #region  Properties 
        public bool IsParsing { get; private set; }

        public string Name { get; private set; }

        public object Parent
        {
            get
            {
                return _parent;
            }


            set
            {
                if (_parent == value) { return; }
                _parent = value;
                OnChanged();
            }

        }

        public decimal X
        {
            get
            {
                return _x;
            }

            set
            {
                if (_x == value) { return; }
                _x = value;
                OnChanged();
            }
        }

        public decimal Y
        {
            get
            {
                return _y;
            }

            set
            {
                if (_y == value) { return; }
                _y = value;
                OnChanged();
            }
        }

        public enXY Moveable
        {
            get
            {
                return _moveable;
            }

            set
            {
                if (_moveable == value) { return; }
                _moveable = value;
                OnChanged();
            }
        }

        public bool CanUsedForAutoRelation
        {
            get
            {
                return _canUsedForAutoRelation;
            }

            set
            {
                if (_canUsedForAutoRelation == value) { return; }
                _canUsedForAutoRelation = value;
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

        public bool PrimaryGridSnapPoint
        {
            get
            {
                return _primaryGridSnapPoint;
            }

            set
            {
                if (_primaryGridSnapPoint == value) { return; }
                _primaryGridSnapPoint = value;
                OnChanged();
            }
        }

        public string Tag
        {
            get
            {
                return _tag;
            }

            set
            {
                if (_tag == value) { return; }
                _tag = value;
                OnChanged();
            }
        }
        #endregion

        public event EventHandler Changed;


        public void Parse(string ToParse) => Parse(ToParse, null);


        public void Parse(string ToParse, object parent)
        {
            IsParsing = true;
            Initialize();

            _parent = parent;

            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
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

                        if (pair.Value.FromPlusMinus())
                        {
                            _moveable = enXY.none;
                        }
                        else
                        {
                            _moveable = enXY.XY;
                        }

                        break;
                    case "moveable":
                        _moveable = (enXY)int.Parse(pair.Value);
                        return;

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

        public static explicit operator PointF(PointM p) => new PointF((float)p.X, (float)p.Y);

        public static explicit operator Point(PointM p) => new Point((int)p.X, (int)p.Y);



        //public PointF ToPointF()
        //{
        //    return new PointF((float)_x, (float)_y);
        //}

        //public Point ToPoint()
        //{
        //    return new Point((int)_x, (int)_y);
        //}

        public static PointM Empty()
        {
            return new PointM(0m, 0m);
        }



        public override string ToString()
        {
            var t = "{";


            if (_parent != null)
            {
                switch (_parent)
                {
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

            if (!string.IsNullOrEmpty(_tag))
            {
                t = t + "Tag=" + _tag.ToNonCritical() + ", ";
            }
            //if (_moveable)
            //{
            //    t = t + "Fix=" + _moveable + ", ";
            //}

            t = t + "Moveable=" + ((int)_moveable).ToString() + ", ";

            if (!_canUsedForAutoRelation)
            {
                t = t + "GetSnapped=" + _canUsedForAutoRelation + ", ";
            }
            if (_primaryGridSnapPoint)
            {
                t = t + "PrimaryGridSnapPoint=" + _primaryGridSnapPoint + ", ";
            }


            return t.Trim(", ") + "}";
        }


        public bool IsOnScreen(decimal czoom, decimal MoveX, decimal MoveY, Rectangle DisplayRectangle)
        {

            var tx = _x * czoom - MoveX;
            var ty = _y * czoom - MoveY;

            if (tx < DisplayRectangle.Left || ty < DisplayRectangle.Top) { return false; }
            if (tx > DisplayRectangle.Right || ty > DisplayRectangle.Bottom) { return false; }
            return true;

        }


        public void Draw(Graphics GR, decimal cZoom, decimal MoveX, decimal MoveY, enDesign Type, enStates State, bool DrawName)
        {
            var tx = _x * cZoom - MoveX + cZoom / 2;
            var ty = _y * cZoom - MoveY + cZoom / 2;

            var r = new Rectangle((int)(tx - 4), (int)(ty - 4), 9, 9);

            Skin.Draw_Back(GR, Type, State, r, null, false);
            Skin.Draw_Border(GR, Type, State, r);


            if (CreativePad.Debug_ShowPointOrder)
            {
                if (_order >= 0)
                {
                    GR.DrawString(_order.ToString(), SimpleArial, Brushes.Magenta, r.PointOf(enAlignment.Top_Right).X + Constants.GlobalRND.Next(-20, 10), r.PointOf(enAlignment.Top_Right).Y + Constants.GlobalRND.Next(-20, 10));
                }
            }


            if (DrawName)
            {
                for (var x = -1; x < 2; x++)
                {
                    for (var y = -1; y < 2; y++)
                    {
                        GR.DrawString(Name, SimpleArial, Brushes.White, (float)tx + x, (float)ty + y - 16);
                    }

                }
                GR.DrawString(Name, SimpleArial, Brushes.Black, (float)tx, (float)ty - 16);
            }

        }

        public PointF ZoomAndMove(AdditionalDrawing e)
        {
            return ZoomAndMove(e.Zoom, e.MoveX, e.MoveY);
        }

        public PointF ZoomAndMove(decimal Zoom, decimal MoveX, decimal MoveY)
        {
            return new PointF((float)(_x * Zoom - MoveX + Zoom / 2), (float)(_y * Zoom - MoveY + Zoom / 2));
        }


        public void SetTo(decimal cx, decimal cy)
        {
            _x = cx;
            _y = cy;
        }

        public void SetTo(double cx, double cy)
        {
            _x = (decimal)cx;
            _y = (decimal)cy;
        }

        public void SetTo(PointM StartPoint, decimal Länge, decimal Alpha)
        {
            var tempVar = GeometryDF.PolarToCartesian(Länge, Convert.ToDouble(Alpha));
            _x = StartPoint.X + tempVar.X;
            _y = StartPoint.Y + tempVar.Y;
        }

        public void SetTo(PointM Point)
        {
            _x = Point.X;
            _y = Point.Y;
        }


        public void SetTo(Point point)
        {
            _x = point.X;
            _y = point.Y;
        }


        public void SetTo(int cx, int cy)
        {
            _x = cx;
            _y = cy;
        }


        private bool CanMove(enXY toCheck, List<clsPointRelation> Rel, List<clsPointRelation> Alredychecked)
        {
            if (_moveable == enXY.none) { return false; }


            foreach (var ThisRelation in Rel)
            {
                if (ThisRelation != null && !Alredychecked.Contains(ThisRelation) && ThisRelation.Points.Contains(this) && ThisRelation.Performs(false))
                {
                    Alredychecked.Add(ThisRelation);

                    if (ThisRelation.Connects(toCheck))
                    {
                        var Move = true;
                        foreach (var thispoint in ThisRelation.Points)
                        {
                            if (thispoint != this) { Move = thispoint.CanMove(toCheck, Rel, Alredychecked); }
                            if (!Move) { return false; }
                        }
                    }
                }
            }


            return true;
        }

        public bool CanMoveX(List<clsPointRelation> Rel)
        {
            var Alredychecked = new List<clsPointRelation>();

            return CanMove(enXY.X, Rel, Alredychecked);
        }

        public bool CanMoveY(List<clsPointRelation> Rel)
        {
            var Alredychecked = new List<clsPointRelation>();

            return CanMove(enXY.Y, Rel, Alredychecked);
        }

        public bool CanMove(List<clsPointRelation> Rel)
        {
            if (CanMoveX(Rel)) { return true; }
            if (CanMoveY(Rel)) { return true; }

            return false;
        }

        public void Store()
        {
            _StoreX = _x;
            _StoreY = _y;
        }


        public void ReStore()
        {
            _x = _StoreX;
            _y = _StoreY;
        }



        public int CompareTo(object obj)
        {
            if (obj is PointM tobj)
            {
                // hierist es egal, ob es ein DoAlways ist oder nicht. Es sollen nur Bedingugen VOR Aktionen kommen
                return CompareKey().CompareTo(tobj.CompareKey());
            }
            else
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!");
                return 0;
            }
        }


        internal string CompareKey()
        {
            if (_x > int.MaxValue / 10.0m || _y > int.MaxValue / 10.0m || _x < int.MinValue / 10.0m || _y < int.MinValue / 10.0m) { return _order.ToString(Constants.Format_Integer10) + "|ZZZ-ZZZ"; }
            return _order.ToString(Constants.Format_Integer10) + "|" + ((int)(_y * 10)).ToString(Constants.Format_Integer10) + "-" + ((int)(_x * 10)).ToString(Constants.Format_Integer10);
        }


        public decimal DistanzZuLinie(PointM P1, PointM P2)
        {
            return DistanzZuLinie(P1.X, P1.Y, P2.X, P2.Y);
        }

        public decimal DistanzZuLinie(decimal X1, decimal Y1, decimal X2, decimal Y2)
        {
            var pal = GeometryDF.PointOnLine(this, X1, Y1, X2, Y2);
            return GeometryDF.Länge(this, pal);
        }

        public void OnChanged()
        {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }


        public decimal Magnitude
        {
            get { return (decimal)Math.Sqrt((double)(_x * _x + _y * _y)); }
        }

        public void Normalize()
        {
            var magnitude = Magnitude;
            _x = _x / magnitude;
            _y = _y / magnitude;
        }



        public decimal DotProduct(PointM vector)
        {
            return this._x * vector._x + this._y * vector._y;
        }


        public static PointM operator +(PointM a, PointM b)
        {
            return new PointM(a._x + b._x, a._y + b._y);
        }

        public static PointM operator -(PointM a)
        {
            return new PointM(-a._x, -a._y);
        }

        public static PointM operator -(PointM a, PointM b)
        {
            return new PointM(a._x - b._x, a._y - b._y);
        }


        public static PointM operator *(PointM a, decimal b)
        {
            return new PointM(a._x * b, a._y * b);
        }

    }
}

