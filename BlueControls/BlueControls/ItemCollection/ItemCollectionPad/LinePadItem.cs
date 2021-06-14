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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection {

    public class LinePadItem : BasicPadItem {

        #region Fields

        internal PointM Point1;
        internal PointM Point2;
        private DateTime _LastRecalc = DateTime.Now.AddHours(-1);
        private List<PointM> _TempPoints;
        private string CalcTempPoints_Code = string.Empty;

        #endregion

        #region Constructors

        public LinePadItem(ItemCollectionPad parent) : this(parent, PadStyles.Style_Standard, Point.Empty, Point.Empty) {
        }

        public LinePadItem(ItemCollectionPad parent, PadStyles format, Point point1, Point point2) : this(parent, string.Empty, PadStyles.Style_Standard, point1, point2) {
        }

        public LinePadItem(ItemCollectionPad parent, string internalname, PadStyles format, Point point1, Point point2) : base(parent, internalname) {
            Point1 = new PointM(this, "Punkt 1", 0, 0);
            Point2 = new PointM(this, "Punkt 2", 0, 0);
            Point1.SetTo(point1);
            Point2.SetTo(point2);
            MovablePoint.Add(Point1);
            MovablePoint.Add(Point2);
            PointsForSuccesfullyMove.AddRange(MovablePoint);
            Stil = format;
            _TempPoints = new List<PointM>();
            Linien_Verhalten = enConectorStyle.Direct;
        }

        #endregion

        #region Properties

        public enConectorStyle Linien_Verhalten { get; set; }

        #endregion

        //public LinePadItem(string vInternal, PadStyles vFormat, enConectorStyle vArt, PointM cPoint1, PointM cPoint2)
        //{
        //    _Internal = vInternal;
        //    Point1.SetTo(cPoint1);
        //    Point2.SetTo(cPoint2);
        //    Style = vFormat;
        //    Art = vArt;
        //    if (string.IsNullOrEmpty(_Internal))
        //    {
        //        Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
        //    }
        //}

        #region Methods

        public override bool Contains(PointF value, double zoomfactor) {
            var ne = 5 / zoomfactor;
            if (Point1.X == 0d && Point2.X == 0d && Point1.Y == 0d && Point2.Y == 0d) { return false; }
            if (_TempPoints.Count == 0) { CalcTempPoints(); }
            if (_TempPoints.Count == 0) { return false; }
            for (var z = 0; z <= _TempPoints.Count - 2; z++) {
                if (value.DistanzZuStrecke(_TempPoints[z], _TempPoints[z + 1]) < ne) { return true; }
            }
            return false;
        }

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new();
            ItemCollectionList Verhalt = new()
            {
                { "Linie direkt zwischen zwei Punkten", ((int)enConectorStyle.Direct).ToString(), QuickImage.Get(enImageCode.Linie) },
                { "Linie soll Objekten ausweichen", ((int)enConectorStyle.Ausweichenx).ToString(), QuickImage.Get(enImageCode.Linie) },
                { "Linie soll Objekten ausweichen und rechtwinklig sein", ((int)enConectorStyle.AusweichenUndGerade).ToString(), QuickImage.Get(enImageCode.Linie) }
            };
            l.Add(new FlexiControlForProperty(this, "Linien-Verhalten", Verhalt));
            AddLineStyleOption(l);
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        //public void Move(double x, double y) {
        //    _LastRecalc = DateTime.Now.AddHours(-1);
        //    Point1.SetTo(Point1.X + x, Point1.Y + y);
        //    Point2.SetTo(Point2.X + x, Point2.Y + y);
        //}
        //public override void SetCoordinates(RectangleM r)
        //{
        //    _LastRecalc = DateTime.Now.AddHours(-1);
        //    Point1.SetTo(r.PointOf(enAlignment.Top_Left));
        //    Point2.SetTo(r.PointOf(enAlignment.Bottom_Right));
        //    base.SetCoordinates(r);
        //}
        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "connection":
                    Linien_Verhalten = (enConectorStyle)int.Parse(value);
                    return true;
            }
            return false;
        }

        public override void PointMoved(PointM point) => CalcTempPoints();

        //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        //{
        //    Stil = (PadStyles)int.Parse(Tags.TagGet("Stil"));
        //    _TempPoints = null;
        //    Linien_Verhalten = (enConectorStyle)int.Parse(Tags.TagGet("Linien-Verhalten"));
        //}
        public void SetCoordinates(double px1, double py1, double px2, double py2) {
            Point1.SetTo(px1, py1);
            Point2.SetTo(px2, py2);
            //p_ML.SetTo(r.PointOf(enAlignment.VerticalCenter_Left));
            //p_MR.SetTo(r.PointOf(enAlignment.VerticalCenter_Right));
            //base.SetCoordinates(r);
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (Linien_Verhalten != enConectorStyle.Direct) { t = t + "Connection=" + (int)Linien_Verhalten + ", "; }
            return t.TrimEnd(", ") + "}";
        }

        protected override RectangleM CalculateUsedArea() {
            if (Point1.X == 0d && Point2.X == 0d && Point1.Y == 0d && Point2.Y == 0d) { return new RectangleM(); }
            if (_TempPoints.Count == 0) { CalcTempPoints(); }
            if (_TempPoints.Count == 0) { return new RectangleM(); }
            var x1 = double.MaxValue;
            var y1 = double.MaxValue;
            var x2 = double.MinValue;
            var y2 = double.MinValue;
            foreach (var ThisPoint in _TempPoints) {
                x1 = Math.Min(ThisPoint.X, x1);
                y1 = Math.Min(ThisPoint.Y, y1);
                x2 = Math.Max(ThisPoint.X, x2);
                y2 = Math.Max(ThisPoint.Y, y2);
            }
            return new RectangleM(x1, y1, x2 - x1, y2 - y1);
            //Return New Rectangle(CInt(Math.Min(Point1.X, Point2.X)), CInt(Math.Min(Point1.Y, Point2.Y)), CInt(Math.Abs(Point2.X - Point1.X)), CInt(Math.Abs(Point2.Y - Point1.Y)))
        }

        protected override string ClassId() => "LINE";

        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, double cZoom, double shiftX, double shiftY, enStates vState, Size SizeOfParentControl, bool ForPrinting) {
            if (Stil == PadStyles.Undefiniert) { return; }
            CalcTempPoints();
            if (_TempPoints.Count == 0) { return; }
            for (var z = 0; z <= _TempPoints.Count - 2; z++) {
                GR.DrawLine(Skin.GetBlueFont(Stil, Parent.SheetStyle).Pen(cZoom * Parent.SheetStyleScale), _TempPoints[z].ZoomAndMove(cZoom, shiftX, shiftY), _TempPoints[z + 1].ZoomAndMove(cZoom, shiftX, shiftY));
            }
        }

        protected override void ParseFinished() {
        }

        private bool Begradige(int P1) {
            if (P1 >= _TempPoints.Count - 1) { return false; }
            if ((int)_TempPoints[P1].X == (int)_TempPoints[P1 + 1].X || (int)_TempPoints[P1].Y == (int)_TempPoints[P1 + 1].Y) { return false; }
            PointM NP1;
            PointM NP2;
            if ((int)(_TempPoints[P1].X - _TempPoints[P1 + 1].X) > (int)(_TempPoints[P1].Y - _TempPoints[P1 + 1].Y)) {
                NP1 = new PointM(_TempPoints[P1].X, (_TempPoints[P1].Y + _TempPoints[P1 + 1].Y) / 2);
                NP2 = new PointM(_TempPoints[P1 + 1].X, (_TempPoints[P1].Y + _TempPoints[P1 + 1].Y) / 2);
                _TempPoints.Insert(P1 + 1, NP1);
                _TempPoints.Insert(P1 + 2, NP2);
            } else {
                NP1 = new PointM((_TempPoints[P1].X + _TempPoints[P1 + 1].X) / 2, _TempPoints[P1].Y);
                NP2 = new PointM((_TempPoints[P1].X + _TempPoints[P1 + 1].X) / 2, _TempPoints[P1 + 1].Y);
                _TempPoints.Insert(P1 + 1, NP1);
                _TempPoints.Insert(P1 + 2, NP2);
            }
            return true;
        }

        private void CalcTempPoints() {
            var NewCode = Point1 + Point2.ToString();
            if (CalcTempPoints_Code != NewCode) {
                CalcTempPoints_Code = NewCode;
                _TempPoints = null;
            }
            if (Linien_Verhalten != enConectorStyle.Direct && _TempPoints != null) {
                if (DateTime.Now.Subtract(_LastRecalc).TotalSeconds > 5) {
                    if (DateTime.Now.Subtract(_LastRecalc).TotalSeconds > 5 + Constants.GlobalRND.Next(10)) {
                        _TempPoints = null;
                    }
                    // r = Nothing
                }
            }
            if (_TempPoints != null && _TempPoints.Count > 1) { return; }
            _LastRecalc = DateTime.Now;
            CalcTempPoints_Code = NewCode;
            _TempPoints = new List<PointM>
            {
                Point1,
                Point2
            };
            if (Linien_Verhalten == enConectorStyle.Direct) { return; }
            var count = 0;
            do {
                count++;
                var again = false;
                if (_TempPoints.Count > 100) {
                    break;
                }
                for (var z = 0; z < _TempPoints.Count; z++) {
                    if (LöscheVerdeckte(z)) {
                        again = true;
                        break;
                    }
                    if (Linien_Verhalten == enConectorStyle.AusweichenUndGerade && Begradige(z)) {
                        again = true;
                        break;
                    }
                    if (Linien_Verhalten is enConectorStyle.AusweichenUndGerade or enConectorStyle.Ausweichenx) {
                        if (WeicheAus(z)) {
                            again = true;
                            break;
                        }
                    }
                    if (Vereinfache(z)) {
                        again = true;
                        break;
                    }
                }
                if (!again) { break; }
                if (count > 50) { break; }
            } while (true);
        }

        private bool IsVerdeckt(double X, double Y) {
            foreach (var ThisBasicItem in Parent) {
                if (ThisBasicItem != null) {
                    if (ThisBasicItem is not LinePadItem) {
                        var a = (RectangleM)ThisBasicItem.UsedArea().Clone();
                        if (a.Width > 0 && a.Height > 0) {
                            a.Inflate(2, 2);
                            if (a.Contains(X, Y)) { return true; }
                        }
                    }
                }
            }
            return false;
        }

        private bool LöscheVerdeckte(int P1) {
            if (_TempPoints[P1] == Point1) {
                return false;
            }
            if (_TempPoints[P1] == Point2) {
                return false;
            }
            if (IsVerdeckt(_TempPoints[P1].X, _TempPoints[P1].Y)) {
                _TempPoints.RemoveAt(P1);
                return true;
            }
            return false;
        }

        private bool SchneidetDas(BasicPadItem ThisBasicItem, PointM P1, PointM P2) {
            if (ThisBasicItem == null) { return false; }
            if (ThisBasicItem is not LinePadItem) {
                var a = (RectangleM)ThisBasicItem.UsedArea().Clone();
                if (a.Width > 0 && a.Height > 0) {
                    a.Inflate(2, 2);
                    var lo = a.PointOf(enAlignment.Top_Left);
                    var ro = a.PointOf(enAlignment.Top_Right);
                    var lu = a.PointOf(enAlignment.Bottom_Left);
                    var ru = a.PointOf(enAlignment.Bottom_Right);
                    if (GeometryDF.LinesIntersect(P1, P2, lo, ro, true) != null || GeometryDF.LinesIntersect(P1, P2, lu, ru, true) != null || GeometryDF.LinesIntersect(P1, P2, lo, lu, true) != null || GeometryDF.LinesIntersect(P1, P2, ro, ru, true) != null) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool SchneidetWas(double X1, double Y1, double X2, double Y2) {
            PointM p1 = new(X1, Y1);
            PointM p2 = new(X2, Y2);
            foreach (var ThisItemBasic in Parent) {
                if (SchneidetDas(ThisItemBasic, p1, p2)) { return true; }
            }
            return false;
        }

        private bool Vereinfache(int P1) {
            if (Linien_Verhalten != enConectorStyle.AusweichenUndGerade) {
                if (P1 > 0 && P1 < _TempPoints.Count - 1) {
                    if (!SchneidetWas(_TempPoints[P1 - 1].X, _TempPoints[P1 - 1].Y, _TempPoints[P1 + 1].X, _TempPoints[P1 + 1].Y)) {
                        _TempPoints.RemoveAt(P1);
                        return true;
                    }
                }
            }
            if (P1 < _TempPoints.Count - 3) {
                if ((int)_TempPoints[P1].X == (int)_TempPoints[P1 + 1].X && (int)_TempPoints[P1].X == (int)_TempPoints[P1 + 2].X) {
                    _TempPoints.RemoveAt(P1 + 1);
                    return true;
                }
                if ((int)_TempPoints[P1].Y == (int)_TempPoints[P1 + 1].Y && (int)_TempPoints[P1].Y == (int)_TempPoints[P1 + 2].Y) {
                    _TempPoints.RemoveAt(P1 + 1);
                    return true;
                }
            }
            if (P1 > 0 && P1 < _TempPoints.Count - 3) {
                if ((int)_TempPoints[P1].X == (int)_TempPoints[P1 + 1].X && (int)_TempPoints[P1 + 1].Y == (int)_TempPoints[P1 + 2].Y) {
                    if (!IsVerdeckt(_TempPoints[P1 + 2].X, _TempPoints[P1].Y)) {
                        if (!SchneidetWas(_TempPoints[P1 - 1].X, _TempPoints[P1 - 1].Y, _TempPoints[P1 + 2].X, _TempPoints[P1].Y)) {
                            if (!SchneidetWas(_TempPoints[P1 + 3].X, _TempPoints[P1 + 3].Y, _TempPoints[P1 + 2].X, _TempPoints[P1].Y)) {
                                _TempPoints[P1].X = _TempPoints[P1 + 2].X;
                                _TempPoints.RemoveAt(P1 + 1);
                                _TempPoints.RemoveAt(P1 + 1);
                                return true;
                            }
                        }
                    }
                }
                if ((int)_TempPoints[P1].Y == (int)_TempPoints[P1 + 1].Y && (int)_TempPoints[P1 + 1].X == (int)_TempPoints[P1 + 2].X) {
                    if (!IsVerdeckt(_TempPoints[P1].X, _TempPoints[P1 + 2].Y)) {
                        if (!SchneidetWas(_TempPoints[P1 - 1].X, _TempPoints[P1 - 1].Y, _TempPoints[P1].X, _TempPoints[P1 + 2].Y)) {
                            if (!SchneidetWas(_TempPoints[P1 + 3].X, _TempPoints[P1 + 3].Y, _TempPoints[P1].X, _TempPoints[P1 + 2].Y)) {
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

        private bool WeicheAus(int P1) {
            if (_TempPoints.Count > 100) { return false; }
            if (P1 >= _TempPoints.Count - 1) { return false; }
            //   If _TempPoints.Count > 4 Then Return False
            foreach (var ThisItemBasic in Parent) {
                if (ThisItemBasic != null) {
                    //    If ThisBasicItem IsNot Object1 AndAlso ThisBasicItem IsNot Object2 Then
                    if (ThisItemBasic is not LinePadItem) {
                        var a = (RectangleM)ThisItemBasic.UsedArea().Clone(); // Umwandeln, um den Bezu zu brechen
                        if (a.Width > 0 && a.Height > 0) {
                            a.Inflate(2, 2);
                            var lo = a.PointOf(enAlignment.Top_Left);
                            var ro = a.PointOf(enAlignment.Top_Right);
                            var lu = a.PointOf(enAlignment.Bottom_Left);
                            var ru = a.PointOf(enAlignment.Bottom_Right);
                            var tOben = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lo, ro, true);
                            var tUnten = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lu, ru, true);
                            var tLinks = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lo, lu, true);
                            var trechts = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], ro, ru, true);
                            //    If DirectCast(Object2, RowFormulaItem).Row.CellFirst().String.Contains("Lilo") AndAlso DirectCast(Object1, RowFormulaItem).Row.CellFirst().String.Contains("Karl") Then Stop
                            if (tOben != null || tUnten != null || tLinks != null || trechts != null) {
                                a.Inflate(-50, -50);
                                lo = a.PointOf(enAlignment.Top_Left);
                                ro = a.PointOf(enAlignment.Top_Right);
                                lu = a.PointOf(enAlignment.Bottom_Left);
                                ru = a.PointOf(enAlignment.Bottom_Right);
                                var Oben = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lo, ro, true);
                                var Unten = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lu, ru, true);
                                var Links = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], lo, lu, true);
                                var rechts = GeometryDF.LinesIntersect(_TempPoints[P1], _TempPoints[P1 + 1], ro, ru, true);
                                if (Oben == null && tOben != null) {
                                    Oben = tOben;
                                }
                                if (Unten == null && tUnten != null) {
                                    Unten = tUnten;
                                }
                                if (Links == null && tLinks != null) {
                                    Links = tLinks;
                                }
                                if (rechts == null && trechts != null) {
                                    rechts = trechts;
                                }
                                if (Oben != null && Unten != null) {
                                    if (_TempPoints[P1].Y < _TempPoints[P1 + 1].Y) {
                                        // Schneidet durch, von oben nach unten
                                        _TempPoints.Insert(P1 + 1, Oben);
                                        if (Math.Abs(_TempPoints[P1].X - lo.X) > Math.Abs(_TempPoints[P1].X - ro.X)) {
                                            _TempPoints.Insert(P1 + 2, ro);
                                            _TempPoints.Insert(P1 + 3, ru);
                                        } else {
                                            _TempPoints.Insert(P1 + 2, lo);
                                            _TempPoints.Insert(P1 + 3, lu);
                                        }
                                        _TempPoints.Insert(P1 + 4, Unten);
                                        return true;
                                    }
                                    // Schneidet durch, von unten nach oben
                                    _TempPoints.Insert(P1 + 1, Unten);
                                    if (Math.Abs(_TempPoints[P1].X - lo.X) > Math.Abs(_TempPoints[P1].X - ro.X)) {
                                        _TempPoints.Insert(P1 + 2, ru);
                                        _TempPoints.Insert(P1 + 3, ro);
                                    } else {
                                        _TempPoints.Insert(P1 + 2, lu);
                                        _TempPoints.Insert(P1 + 3, lo);
                                    }
                                    _TempPoints.Insert(P1 + 4, Oben);
                                    return true;
                                }
                                if (Links != null && rechts != null) {
                                    if (_TempPoints[P1].X < _TempPoints[P1 + 1].X) {
                                        // Schneidet durch, von links nach rechts
                                        _TempPoints.Insert(P1 + 1, Links);
                                        if (Math.Abs(_TempPoints[P1].Y - lo.Y) > Math.Abs(_TempPoints[P1].Y - lu.Y)) {
                                            _TempPoints.Insert(P1 + 2, lu);
                                            _TempPoints.Insert(P1 + 3, ru);
                                        } else {
                                            _TempPoints.Insert(P1 + 2, lo);
                                            _TempPoints.Insert(P1 + 3, ro);
                                        }
                                        _TempPoints.Insert(P1 + 4, rechts);
                                        return true;
                                    }
                                    // Schneidet durch, von rechts nach links
                                    _TempPoints.Insert(P1 + 1, rechts);
                                    if (Math.Abs(_TempPoints[P1].Y - lo.Y) > Math.Abs(_TempPoints[P1].Y - lu.Y)) {
                                        _TempPoints.Insert(P1 + 2, ru);
                                        _TempPoints.Insert(P1 + 3, lu);
                                    } else {
                                        _TempPoints.Insert(P1 + 2, ro);
                                        _TempPoints.Insert(P1 + 3, lo);
                                    }
                                    _TempPoints.Insert(P1 + 4, Links);
                                    return true;
                                }
                                if (Unten != null && rechts != null) {
                                    if (_TempPoints[P1].X < _TempPoints[P1 + 1].X) {
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
                                if (Oben != null && rechts != null) {
                                    if (_TempPoints[P1].X < _TempPoints[P1 + 1].X) {
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
                                if (Unten != null && Links != null) {
                                    if (_TempPoints[P1].X < _TempPoints[P1 + 1].X) {
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
                                if (Oben != null && Links != null) {
                                    if (_TempPoints[P1].X < _TempPoints[P1 + 1].X) {
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

        #endregion
    }
}