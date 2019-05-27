#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection
{


    public class LinePadItem : BasicPadItem
    {

        private string CalcTempPoints_Code = string.Empty;


        public override void DesignOrStyleChanged()
        {
            // Keine Variablen zum Reseten, ein Invalidate reicht
        }


        #region  Variablen-Deklarationen 

        internal PointDF Point1;
        internal PointDF Point2;
        //   Dim _Design As enDesign

        internal enConectorStyle Art;
        private List<PointDF> _TempPoints;

        private DateTime _LastRecalc = DateTime.Now.AddHours(-1);


        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 

        public LinePadItem()
        { }
        public LinePadItem(PadStyles vFormat, Point cPoint1, Point cPoint2)
        {
            Point1.SetTo(cPoint1);
            Point2.SetTo(cPoint2);
            Format = vFormat;
        }


        public LinePadItem(string vInternal, PadStyles vFormat, enConectorStyle vArt, PointDF cPoint1, PointDF cPoint2)
        {
            _Internal = vInternal;
            Point1.SetTo(cPoint1);
            Point2.SetTo(cPoint2);
            Format = vFormat;
            Art = vArt;
            if (string.IsNullOrEmpty(_Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
            }
        }


        protected override void Initialize()
        {
            base.Initialize();
            Point1 = new PointDF(this, "Punkt 1", 0, 0);
            Point2 = new PointDF(this, "Punkt 2", 0, 0);

            _TempPoints = new List<PointDF>();
            Format = PadStyles.Style_Standard;
            Art = enConectorStyle.Direct;
        }


        #endregion


        #region  Properties 


        #endregion


        protected override string ClassId()
        {
            return "LINE";
        }

        public override bool Contains(PointF value, decimal zoomfactor)
        {
            var ne = 5 / zoomfactor;

            if (Point1.X == 0M && Point2.X == 0M && Point1.Y == 0M && Point2.Y == 0M) { return false; }


            if (_TempPoints.Count == 0) { CalcTempPoints(); }

            if (_TempPoints.Count == 0) { return false; }

            for (var z = 0; z <= _TempPoints.Count - 2; z++)
            {
                if (value.DistanzZuStrecke(_TempPoints[z], _TempPoints[z + 1]) < ne) { return true; }
            }

            return false;
        }


        public override List<PointDF> PointList()
        {
            var l = new List<PointDF>();
            l.Add(Point1);
            l.Add(Point2);
            return l;
        }


        public override RectangleDF UsedArea()
        {
            if (Point1.X == 0M && Point2.X == 0M && Point1.Y == 0M && Point2.Y == 0M) { return new RectangleDF(); }

            if (_TempPoints.Count == 0) { CalcTempPoints(); }

            if (_TempPoints.Count == 0) { return new RectangleDF(); }
            var x1 = decimal.MaxValue;
            var y1 = decimal.MaxValue;
            var x2 = decimal.MinValue;
            var y2 = decimal.MinValue;


            foreach (var ThisPoint in _TempPoints)
            {
                x1 = Math.Min(ThisPoint.X, x1);
                y1 = Math.Min(ThisPoint.Y, y1);

                x2 = Math.Max(ThisPoint.X, x2);
                y2 = Math.Max(ThisPoint.Y, y2);
            }

            return new RectangleDF(x1, y1, x2 - x1, y2 - y1);


            //Return New Rectangle(CInt(Math.Min(Point1.X, Point2.X)), CInt(Math.Min(Point1.Y, Point2.Y)), CInt(Math.Abs(Point2.X - Point1.X)), CInt(Math.Abs(Point2.Y - Point1.Y)))
        }


        protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {
            if (Format == PadStyles.Undefiniert)
            {
                return;
            }


            CalcTempPoints();
            if (_TempPoints.Count == 0)
            {
                return;
            }


            for (var z = 0; z <= _TempPoints.Count - 2; z++)
            {
                GR.DrawLine(GenericControl.Skin.GetBlueFont(Format, Parent.SheetStyle).Pen(cZoom * Parent.SheetStyleScale), _TempPoints[z].ZoomAndMove(cZoom, MoveX, MoveY), _TempPoints[z + 1].ZoomAndMove(cZoom, MoveX, MoveY));
            }

        }

        public override void SetCoordinates(RectangleDF r)
        {
            _LastRecalc = DateTime.Now.AddHours(-1);


            Point1.SetTo(r.PointOf(enAlignment.Top_Left));
            Point2.SetTo(r.PointOf(enAlignment.Bottom_Right));


            RecomputePointAndRelations();
        }


        protected override bool ParseExplicit(KeyValuePair<string, string> pair)
        {

            _LastRecalc = DateTime.Now.AddHours(-1);

            switch (pair.Key)
            {
                case "checked":
                    return true;
                case "connection":
                    Art = (enConectorStyle)int.Parse(pair.Value);
                    return true;

            }

            return false;


        }

        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            if (Art != enConectorStyle.Direct)
            {
                t = t + "Connection=" + (int)(Art) + ", ";
            }
            return t + "}";
        }


        public override void GenerateInternalRelation(List<clsPointRelation> relations)
        {
            // nix zu Tun
        }


        private void CalcTempPoints()
        {
            var NewCode = Point1 + Point2.ToString();

            if (CalcTempPoints_Code != NewCode)
            {
                CalcTempPoints_Code = NewCode;
                _TempPoints = null;
            }


            if (Art != enConectorStyle.Direct && _TempPoints != null)
            {
                if (DateTime.Now.Subtract(_LastRecalc).TotalSeconds > 5)
                {
                    var r = new Random();
                    if (DateTime.Now.Subtract(_LastRecalc).TotalSeconds > 5 + r.Next(10))
                    {
                        _TempPoints = null;
                    }
                    // r = Nothing
                }
            }


            if (_TempPoints != null && _TempPoints.Count > 1)
            {
                return;
            }


            _LastRecalc = DateTime.Now;
            CalcTempPoints_Code = NewCode;

            _TempPoints = new List<PointDF>();
            _TempPoints.Add(Point1);
            _TempPoints.Add(Point2);

            if (Art == enConectorStyle.Direct)
            {
                return;
            }

            var count = 0;

            do
            {
                count += 1;
                var again = false;
                if (_TempPoints.Count > 100)
                {
                    break;
                }

                for (var z = 0; z < _TempPoints.Count; z++)
                {
                    if (LöscheVerdeckte(z))
                    {
                        again = true;
                        break;
                    }

                    if (Art == enConectorStyle.AusweichenUndGerade && Begradige(z))
                    {
                        again = true;
                        break;
                    }


                    if (Art == enConectorStyle.AusweichenUndGerade || Art == enConectorStyle.Ausweichenx)
                    {
                        if (WeicheAus(z))
                        {
                            again = true;
                            break;
                        }
                    }

                    if (Vereinfache(z))
                    {
                        again = true;
                        break;
                    }
                }

                if (!again)
                {
                    break;
                }
                if (count > 50)
                {
                    break;
                }

            } while (true);
        }


        private bool IsVerdeckt(decimal X, decimal Y)
        {

            foreach (var ThisItemBasic in Parent)
            {
                if (ThisItemBasic != null)
                {


                    if (!(ThisItemBasic is LinePadItem))
                    {

                        var a = ThisItemBasic.UsedArea();

                        if (a.Width > 0 && a.Height > 0)
                        {
                            a.Inflate(-2, -2);
                            if (a.Contains(X, Y)) { return true; }
                        }
                    }
                }
            }


            return false;
        }

        private bool SchneidetWas(decimal X1, decimal Y1, decimal X2, decimal Y2)
        {

            var p1 = new PointDF(X1, Y1);
            var p2 = new PointDF(X2, Y2);

            foreach (var ThisItemBasic in Parent)
            {
                if (SchneidetDas(ThisItemBasic, p1, p2))
                {
                    return true;
                }
            }

            return false;
        }


        private bool SchneidetDas(BasicPadItem ThisBasicItem, PointDF P1, PointDF P2)
        {


            if (ThisBasicItem == null) { return false; }


            if (!(ThisBasicItem is LinePadItem))
            {

                var a = ThisBasicItem.UsedArea();

                if (a.Width > 0 && a.Height > 0)
                {
                    a.Inflate(-2, -2);

                    var lo = a.PointOf(enAlignment.Top_Left);
                    var ro = a.PointOf(enAlignment.Top_Right);
                    var lu = a.PointOf(enAlignment.Bottom_Left);
                    var ru = a.PointOf(enAlignment.Bottom_Right);
                    if (GeometryDF.LinesIntersect(P1, P2, lo, ro, true) != null || GeometryDF.LinesIntersect(P1, P2, lu, ru, true) != null || GeometryDF.LinesIntersect(P1, P2, lo, lu, true) != null || GeometryDF.LinesIntersect(P1, P2, ro, ru, true) != null)
                    {
                        return true;
                    }

                }
            }
            return false;
        }


        private bool Vereinfache(int P1)
        {

            if (Art != enConectorStyle.AusweichenUndGerade)
            {
                if (P1 > 0 && P1 < _TempPoints.Count - 1)
                {
                    if (!SchneidetWas(_TempPoints[P1 - 1].X, _TempPoints[P1 - 1].Y, _TempPoints[P1 + 1].X, _TempPoints[P1 + 1].Y))
                    {
                        _TempPoints.RemoveAt(P1);
                        return true;
                    }
                }

            }


            if (P1 < _TempPoints.Count - 3)
            {


                if ((int)(_TempPoints[P1].X) == (int)(_TempPoints[P1 + 1].X) && (int)(_TempPoints[P1].X) == (int)(_TempPoints[P1 + 2].X))
                {
                    _TempPoints.RemoveAt(P1 + 1);
                    return true;
                }

                if ((int)(_TempPoints[P1].Y) == (int)(_TempPoints[P1 + 1].Y) && (int)(_TempPoints[P1].Y) == (int)(_TempPoints[P1 + 2].Y))
                {
                    _TempPoints.RemoveAt(P1 + 1);
                    return true;
                }
            }

            if (P1 > 0 && P1 < _TempPoints.Count - 3)
            {
                if ((int)(_TempPoints[P1].X) == (int)(_TempPoints[P1 + 1].X) && (int)(_TempPoints[P1 + 1].Y) == (int)(_TempPoints[P1 + 2].Y))
                {
                    if (!IsVerdeckt(_TempPoints[P1 + 2].X, _TempPoints[P1].Y))
                    {

                        if (!SchneidetWas(_TempPoints[P1 - 1].X, _TempPoints[P1 - 1].Y, _TempPoints[P1 + 2].X, _TempPoints[P1].Y))
                        {
                            if (!SchneidetWas(_TempPoints[P1 + 3].X, _TempPoints[P1 + 3].Y, _TempPoints[P1 + 2].X, _TempPoints[P1].Y))
                            {
                                _TempPoints[P1].X = _TempPoints[P1 + 2].X;
                                _TempPoints.RemoveAt(P1 + 1);
                                _TempPoints.RemoveAt(P1 + 1);
                                return true;
                            }
                        }

                    }
                }

                if ((int)(_TempPoints[P1].Y) == (int)(_TempPoints[P1 + 1].Y) && (int)(_TempPoints[P1 + 1].X) == (int)(_TempPoints[P1 + 2].X))
                {
                    if (!IsVerdeckt(_TempPoints[P1].X, _TempPoints[P1 + 2].Y))
                    {

                        if (!SchneidetWas(_TempPoints[P1 - 1].X, _TempPoints[P1 - 1].Y, _TempPoints[P1].X, _TempPoints[P1 + 2].Y))
                        {
                            if (!SchneidetWas(_TempPoints[P1 + 3].X, _TempPoints[P1 + 3].Y, _TempPoints[P1].X, _TempPoints[P1 + 2].Y))
                            {
                                _TempPoints[P1].Y = _TempPoints[P1 + 2].Y;
                                _TempPoints.RemoveAt(P1 + 1);
                                _TempPoints.RemoveAt(P1 + 1);
                                return true;
                            }
                        }

                    }
                }

            }


            return false;
        }


        private bool WeicheAus(int P1)
        {

            if (_TempPoints.Count > 100)
            {
                return false;
            }
            if (P1 >= _TempPoints.Count - 1)
            {
                return false;
            }
            //   If _TempPoints.Count > 4 Then Return False

            foreach (var ThisItemBasic in Parent)
            {
                if (ThisItemBasic != null)
                {
                    //    If ThisBasicItem IsNot Object1 AndAlso ThisBasicItem IsNot Object2 Then

                    if (!(ThisItemBasic is LinePadItem))
                    {

                        var a = ThisItemBasic.UsedArea();

                        if (a.Width > 0 && a.Height > 0)
                        {
                            a.Inflate(-2, -2);

                            var lo = a.PointOf(enAlignment.Top_Left);
                            var ro = a.PointOf(enAlignment.Top_Right);
                            var lu = a.PointOf(enAlignment.Bottom_Left);
                            var ru = a.PointOf(enAlignment.Bottom_Right);
                            var tOben = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lo, ro, true);
                            var tUnten = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lu, ru, true);
                            var tLinks = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lo, lu, true);
                            var trechts = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], ro, ru, true);


                            //    If DirectCast(Object2, RowFormulaItem).Row.CellFirst().String.Contains("Lilo") AndAlso DirectCast(Object1, RowFormulaItem).Row.CellFirst().String.Contains("Karl") Then Stop


                            if (tOben != null || tUnten != null || tLinks != null || trechts != null)
                            {
                                a.Inflate(50, 50);
                                lo = a.PointOf(enAlignment.Top_Left);
                                ro = a.PointOf(enAlignment.Top_Right);
                                lu = a.PointOf(enAlignment.Bottom_Left);
                                ru = a.PointOf(enAlignment.Bottom_Right);
                                var Oben = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lo, ro, true);
                                var Unten = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lu, ru, true);
                                var Links = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lo, lu, true);
                                var rechts = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], ro, ru, true);


                                if (Oben == null && tOben != null)
                                {
                                    Oben = tOben;
                                }
                                if (Unten == null && tUnten != null)
                                {
                                    Unten = tUnten;
                                }
                                if (Links == null && tLinks != null)
                                {
                                    Links = tLinks;
                                }
                                if (rechts == null && trechts != null)
                                {
                                    rechts = trechts;
                                }


                                if (Oben != null && Unten != null)
                                {
                                    if (_TempPoints[P1].Y < _TempPoints[P1 + 1].Y)
                                    {
                                        // Schneidet durch, von oben nach unten
                                        _TempPoints.Insert(P1 + 1, Oben);

                                        if (Math.Abs(_TempPoints[P1].X - lo.X) > Math.Abs(_TempPoints[P1].X - ro.X))
                                        {
                                            _TempPoints.Insert(P1 + 2, ro);
                                            _TempPoints.Insert(P1 + 3, ru);
                                        }
                                        else
                                        {
                                            _TempPoints.Insert(P1 + 2, lo);
                                            _TempPoints.Insert(P1 + 3, lu);
                                        }

                                        _TempPoints.Insert(P1 + 4, Unten);
                                        return true;
                                    }

                                    // Schneidet durch, von unten nach oben
                                    _TempPoints.Insert(P1 + 1, Unten);

                                    if (Math.Abs(_TempPoints[P1].X - lo.X) > Math.Abs(_TempPoints[P1].X - ro.X))
                                    {
                                        _TempPoints.Insert(P1 + 2, ru);
                                        _TempPoints.Insert(P1 + 3, ro);
                                    }
                                    else
                                    {
                                        _TempPoints.Insert(P1 + 2, lu);
                                        _TempPoints.Insert(P1 + 3, lo);
                                    }


                                    _TempPoints.Insert(P1 + 4, Oben);
                                    return true;


                                }

                                if (Links != null && rechts != null)
                                {
                                    if (_TempPoints[P1].X < _TempPoints[P1 + 1].X)
                                    {
                                        // Schneidet durch, von links nach rechts
                                        _TempPoints.Insert(P1 + 1, Links);
                                        if (Math.Abs(_TempPoints[P1].Y - lo.Y) > Math.Abs(_TempPoints[P1].Y - lu.Y))
                                        {
                                            _TempPoints.Insert(P1 + 2, lu);
                                            _TempPoints.Insert(P1 + 3, ru);
                                        }
                                        else
                                        {
                                            _TempPoints.Insert(P1 + 2, lo);
                                            _TempPoints.Insert(P1 + 3, ro);
                                        }
                                        _TempPoints.Insert(P1 + 4, rechts);
                                        return true;
                                    }

                                    // Schneidet durch, von rechts nach links
                                    _TempPoints.Insert(P1 + 1, rechts);
                                    if (Math.Abs(_TempPoints[P1].Y - lo.Y) > Math.Abs(_TempPoints[P1].Y - lu.Y))
                                    {
                                        _TempPoints.Insert(P1 + 2, ru);
                                        _TempPoints.Insert(P1 + 3, lu);
                                    }
                                    else
                                    {
                                        _TempPoints.Insert(P1 + 2, ro);
                                        _TempPoints.Insert(P1 + 3, lo);
                                    }
                                    _TempPoints.Insert(P1 + 4, Links);

                                    return true;


                                }

                                if (Unten != null && rechts != null)
                                {
                                    if (_TempPoints[P1].X < _TempPoints[P1 + 1].X)
                                    {
                                        _TempPoints.Insert(P1 + 1, Unten);
                                        _TempPoints.Insert(P1 + 2, ru);
                                        _TempPoints.Insert(P1 + 3, rechts);
                                        return true;
                                    }

                                    _TempPoints.Insert(P1 + 1, rechts);
                                    _TempPoints.Insert(P1 + 2, ru);
                                    _TempPoints.Insert(P1 + 3, Unten);
                                    return true;

                                }

                                if (Oben != null && rechts != null)
                                {
                                    if (_TempPoints[P1].X < _TempPoints[P1 + 1].X)
                                    {
                                        _TempPoints.Insert(P1 + 1, Oben);
                                        _TempPoints.Insert(P1 + 2, ro);
                                        _TempPoints.Insert(P1 + 3, rechts);
                                        return true;
                                    }

                                    _TempPoints.Insert(P1 + 1, rechts);
                                    _TempPoints.Insert(P1 + 2, ro);
                                    _TempPoints.Insert(P1 + 3, Oben);
                                    return true;


                                }

                                if (Unten != null && Links != null)
                                {
                                    if (_TempPoints[P1].X < _TempPoints[P1 + 1].X)
                                    {
                                        _TempPoints.Insert(P1 + 1, Links);
                                        _TempPoints.Insert(P1 + 2, lu);
                                        _TempPoints.Insert(P1 + 3, Unten);
                                        return true;
                                    }

                                    _TempPoints.Insert(P1 + 1, Unten);
                                    _TempPoints.Insert(P1 + 2, lu);
                                    _TempPoints.Insert(P1 + 3, Links);
                                    return true;
                                }

                                if (Oben != null && Links != null)
                                {
                                    if (_TempPoints[P1].X < _TempPoints[P1 + 1].X)
                                    {
                                        _TempPoints.Insert(P1 + 1, Links);
                                        _TempPoints.Insert(P1 + 2, lo);
                                        _TempPoints.Insert(P1 + 3, Oben);
                                        return true;
                                    }

                                    _TempPoints.Insert(P1 + 1, Oben);
                                    _TempPoints.Insert(P1 + 2, lo);
                                    _TempPoints.Insert(P1 + 3, Links);
                                    return true;
                                }

                                return false;
                            }
                        }
                    }
                }
            }

            return false;
        }


        private bool Begradige(int P1)
        {

            if (P1 >= _TempPoints.Count - 1) { return false; }

            if ((int)(_TempPoints[P1].X) == (int)(_TempPoints[P1 + 1].X) || (int)(_TempPoints[P1].Y) == (int)(_TempPoints[P1 + 1].Y)) { return false; }

            PointDF NP1;
            PointDF NP2;

            if ((int)(_TempPoints[P1].X - _TempPoints[P1 + 1].X) > (int)(_TempPoints[P1].Y - _TempPoints[P1 + 1].Y))
            {


                NP1 = new PointDF(_TempPoints[P1].X, (_TempPoints[P1].Y + _TempPoints[P1 + 1].Y) / 2);
                NP2 = new PointDF(_TempPoints[P1 + 1].X, (_TempPoints[P1].Y + _TempPoints[P1 + 1].Y) / 2);
                _TempPoints.Insert(P1 + 1, NP1);
                _TempPoints.Insert(P1 + 2, NP2);


            }
            else
            {

                NP1 = new PointDF((_TempPoints[P1].X + _TempPoints[P1 + 1].X) / 2, _TempPoints[P1].Y);
                NP2 = new PointDF((_TempPoints[P1].X + _TempPoints[P1 + 1].X) / 2, _TempPoints[P1 + 1].Y);

                _TempPoints.Insert(P1 + 1, NP1);
                _TempPoints.Insert(P1 + 2, NP2);
            }


            return true;
        }


        private bool LöscheVerdeckte(int P1)
        {

            if (_TempPoints[P1] == Point1)
            {
                return false;
            }
            if (_TempPoints[P1] == Point2)
            {
                return false;
            }


            if (IsVerdeckt(_TempPoints[P1].X, _TempPoints[P1].Y))
            {
                _TempPoints.RemoveAt(P1);
                return true;
            }

            return false;
        }


        protected override void KeepInternalLogic()
        {
            CalcTempPoints();
        }
        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {
            var l = new List<FlexiControl>();


            var Verhalt = new ItemCollectionList();
            Verhalt.Add(new TextListItem(((int)enConectorStyle.Direct).ToString(), "Linie direkt zwischen zwei Punkten", QuickImage.Get(enImageCode.Linie)));
            Verhalt.Add(new TextListItem(((int)enConectorStyle.Ausweichenx).ToString(), "Linie soll Objekten ausweichen", QuickImage.Get(enImageCode.Linie)));
            Verhalt.Add(new TextListItem(((int)enConectorStyle.AusweichenUndGerade).ToString(), "Linie soll Objekten ausweichen und rechtwinklig sein", QuickImage.Get(enImageCode.Linie)));
            l.Add(new FlexiControl("Linien-Verhalten", ((int)Art).ToString(), Verhalt));

            var Rahms = new ItemCollectionList();
            //   Rahms.Add(New ItemCollection.TextListItem(CInt(PadStyles.Undefiniert).ToString, "Ohne Rahmen", enImageCode.Kreuz))
            Rahms.Add(new TextListItem(((int)PadStyles.Style_Überschrift_Haupt).ToString(), "Haupt-Überschrift", GenericControl.Skin.GetBlueFont(PadStyles.Style_Überschrift_Haupt, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_Überschrift_Untertitel).ToString(), "Untertitel für Haupt-Überschrift", GenericControl.Skin.GetBlueFont(PadStyles.Style_Überschrift_Untertitel, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_Überschrift_Kapitel).ToString(), "Überschrift für Kapitel", GenericControl.Skin.GetBlueFont(PadStyles.Style_Überschrift_Kapitel, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_Standard).ToString(), "Standard", GenericControl.Skin.GetBlueFont(PadStyles.Style_Standard, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_StandardFett).ToString(), "Standard Fett", GenericControl.Skin.GetBlueFont(PadStyles.Style_StandardFett, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_StandardAlternativ).ToString(), "Standard Alternativ-Design", GenericControl.Skin.GetBlueFont(PadStyles.Style_StandardAlternativ, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Add(new TextListItem(((int)PadStyles.Style_KleinerZusatz).ToString(), "Kleiner Zusatz", GenericControl.Skin.GetBlueFont(PadStyles.Style_KleinerZusatz, Parent.SheetStyle).SymbolOfLine()));
            Rahms.Sort();


            l.Add(new FlexiControl("Stil", ((int)Format).ToString(), Rahms));
            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {
            Format = (PadStyles)int.Parse(Tags.TagGet("Stil"));

            _TempPoints = null;
            Art = (enConectorStyle)int.Parse(Tags.TagGet("Linien-Verhalten"));
        }

    }
}