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
    public sealed class PointDF : IParseable, IComparable
    {
        #region  Variablen-Deklarationen 

        private object _parent;
        private decimal _x;
        private decimal _y;
        private bool _positionFix;
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


        public PointDF()
        {
            Initialize();
            Name = "Dummy Point ohne Angaben";
        }



        public PointDF(object cParent, string cName, PointDF StartPoint, decimal Länge, decimal Alpha)
        {
            Initialize();
            _parent = cParent;
            Name = cName;
            var tempVar = GeometryDF.PolarToCartesian(Länge, Convert.ToDouble(Alpha));
            _x = StartPoint.X + tempVar.X;
            _y = StartPoint.Y + tempVar.Y;
        }


        public PointDF(PointDF StartPoint, decimal Länge, decimal Alpha)
        {
            Initialize();
            Name = "Dummy Point von PointDF mit Polarverschiebung";
            var tempVar = GeometryDF.PolarToCartesian(Länge, Convert.ToDouble(Alpha));
            _x = StartPoint.X + tempVar.X;
            _y = StartPoint.Y + tempVar.Y;
        }



        public PointDF(PointF StartPoint, decimal Länge, decimal Alpha)
        {
            Initialize();
            Name = "Dummy Point von PointF mit Polarverschiebung";
            var tempVar = GeometryDF.PolarToCartesian(Länge, Convert.ToDouble(Alpha));
            _x = (decimal)(StartPoint.X + (float)tempVar.X);
            _y = (decimal)(StartPoint.Y + (float)tempVar.Y);
        }

        public PointDF(object cParent, string Code)
        {
            Parse(Code);
            _parent = cParent;
        }


        public PointDF(object cParent, string cName, int cX, int cY)
        {
            Initialize();
            _parent = cParent;
            _x = cX;
            _y = cY;
            Name = cName;
        }

        public PointDF(string cName, decimal cX, decimal cY)
        {
            Initialize();
            _x = cX;
            _y = cY;
            Name = cName;
        }


        public PointDF(PointF PF)
        {
            Initialize();
            Name = "Dummy Point von PointF";
            _x = (decimal)PF.X;
            _y = (decimal)PF.Y;
        }

        public PointDF(int cx, int cy)
        {
            Initialize();
            Name = "Dummy Point von IntX und IntY";
            _x = (decimal)cx;
            _y = (decimal)cy;
        }

        public PointDF(double cx, double cY)
        {
            Initialize();
            Name = "Dummy Point von DoubleX und DoubleY";
            _x = (decimal)cx;
            _y = (decimal)cY;
        }

        public PointDF(decimal cx, decimal cY)
        {
            Initialize();
            Name = "Dummy Point von DecimalX und DecimalY";
            _x = cx;
            _y = cY;
        }


        public PointDF(PointDF PointDF)
        {
            Initialize();
            Name = "Dummy Point von PointDF";
            _x = PointDF.X;
            _y = PointDF.Y;
        }


        public PointDF(object cParent, string cName, int cX, int cY, bool IsFix)
        {
            Initialize();
            _parent = cParent;
            _x = cX;
            _y = cY;
            Name = cName;
            _positionFix = IsFix;
        }

        public PointDF(object cParent, string cName, int cX, int cY, bool IsFix, bool vCanUsedForAutoRelation)
        {
            Initialize();
            _parent = cParent;
            _x = cX;
            _y = cY;
            Name = cName;
            _positionFix = IsFix;
            _canUsedForAutoRelation = vCanUsedForAutoRelation;
        }

        public PointDF(object cParent, string cName, int cX, int cY, bool IsFix, bool vCanUsedForAutoRelation, bool vPrimaryGridSnapPoint)
        {
            Initialize();
            _parent = cParent;
            _x = cX;
            _y = cY;
            Name = cName;
            _positionFix = IsFix;
            _canUsedForAutoRelation = vCanUsedForAutoRelation;
            _primaryGridSnapPoint = vPrimaryGridSnapPoint;
        }


        public PointDF(object cParent, PointDF Vorlage)
        {
            Initialize();
            // Order und Parent weichen ab!

            _parent = cParent;

            Name = Vorlage.Name;
            _x = Vorlage.X;
            _y = Vorlage.Y;
            _StoreX = Vorlage._StoreX;
            _StoreY = Vorlage._StoreY;

            _canUsedForAutoRelation = Vorlage.CanUsedForAutoRelation;
            _positionFix = Vorlage.PositionFix;

            _primaryGridSnapPoint = Vorlage.PrimaryGridSnapPoint;
            _tag = Vorlage.Tag;
        }


        public PointDF(object cParent, string cName, decimal cX, decimal cY, bool IsFix, bool vCanUsedForAutoRelation)
        {
            Initialize();
            _parent = cParent;
            _x = cX;
            _y = cY;
            Name = cName;
            _positionFix = IsFix;
            _canUsedForAutoRelation = vCanUsedForAutoRelation;
        }

        public void Initialize()
        {
            Name = string.Empty;
            _parent = null;
            _x = 0;
            _y = 0;
            _positionFix = false;
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

        public bool PositionFix
        {
            get
            {
                return _positionFix;
            }

            set
            {
                if (_positionFix == value) { return; }
                _positionFix = value;
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

        public int Order
        {
            get
            {
                return _order;
            }

            set
            {
                if (_order == value) { return; }
                _order = value;
                OnChanged();
            }
        }

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

        public void Parse(string ToParse)
        {
            IsParsing = true;
            Initialize();
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
                        _positionFix = pair.Value.FromPlusMinus();
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

        public PointF ToPointF()
        {
            return new PointF((float)_x, (float)_y);
        }

        public Point ToPoint()
        {
            return new Point((int)_x, (int)_y);
        }

        public static PointDF Empty()
        {
            return new PointDF(0m, 0m);
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
                        t = t + "ParentType=Main, ";
                        break;
                    case ItemCollectionPad _:
                        t = t + "ParentType=Main, ";
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
            if (_positionFix)
            {
                t = t + "Fix=" + _positionFix + ", ";
            }
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

        public void SetTo(PointDF StartPoint, decimal Länge, decimal Alpha)
        {
            var tempVar = GeometryDF.PolarToCartesian(Länge, Convert.ToDouble(Alpha));
            _x = StartPoint.X + tempVar.X;
            _y = StartPoint.Y + tempVar.Y;
        }

        public void SetTo(PointDF Point)
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


        private bool CanMove(bool CheckX, List<clsPointRelation> Rel, List<clsPointRelation> Alredychecked)
        {
            if (_positionFix) { return false; }


            foreach (var ThisRelation in Rel)
            {
                if (ThisRelation != null && !Alredychecked.Contains(ThisRelation) && ThisRelation.Points.Contains(this) && ThisRelation.Performs(false))
                {
                    Alredychecked.Add(ThisRelation);

                    if (ThisRelation.Connects(CheckX))
                    {
                        var Move = true;
                        foreach (var thispoint in ThisRelation.Points)
                        {
                            if (thispoint != this) { Move = thispoint.CanMove(CheckX, Rel, Alredychecked); }
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

            return CanMove(true, Rel, Alredychecked);
        }

        public bool CanMoveY(List<clsPointRelation> Rel)
        {
            var Alredychecked = new List<clsPointRelation>();

            return CanMove(false, Rel, Alredychecked);
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
            if (obj is PointDF tobj)
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


        public decimal DistanzZuLinie(PointDF P1, PointDF P2)
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


    }
}

