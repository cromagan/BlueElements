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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static BlueBasics.Geometry;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection {

    public class LinePadItem : BasicPadItem {

        #region Fields

        internal PointM? Point1;

        internal PointM? Point2;

        private string _calcTempPointsCode = string.Empty;
        private DateTime _lastRecalc = DateTime.Now.AddHours(-1);

        private List<PointF>? _tempPoints;

        #endregion

        #region Constructors

        public LinePadItem(string internalname) : this(internalname, PadStyles.Style_Standard, Point.Empty, Point.Empty) { }

        public LinePadItem(PadStyles format, Point point1, Point point2) : this(string.Empty, format, point1, point2) { }

        public LinePadItem(string internalname, PadStyles format, Point point1, Point point2) : base(internalname) {
            Point1 = new PointM(this, "Punkt 1", 0, 0);
            Point2 = new PointM(this, "Punkt 2", 0, 0);
            Point1.SetTo(point1);
            Point2.SetTo(point2);
            MovablePoint.Add(Point1);
            MovablePoint.Add(Point2);
            PointsForSuccesfullyMove.AddRange(MovablePoint);
            Stil = format;
            _tempPoints = new List<PointF>();
            Linien_Verhalten = enConectorStyle.Direct;
        }

        #endregion

        #region Properties

        public enConectorStyle Linien_Verhalten { get; set; }

        #endregion

        #region Methods

        public override bool Contains(PointF value, float zoomfactor) {
            var ne = 5 / zoomfactor;
            if (Point1.X == 0d && Point2.X == 0d && Point1.Y == 0d && Point2.Y == 0d) { return false; }
            if (_tempPoints != null && _tempPoints.Count == 0) { CalcTempPoints(); }
            if (_tempPoints != null && _tempPoints.Count == 0) { return false; }
            for (var z = 0; z <= _tempPoints.Count - 2; z++) {
                if (value.DistanzZuStrecke(_tempPoints[z], _tempPoints[z + 1]) < ne) { return true; }
            }
            return false;
        }

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
        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new();
            ItemCollectionList.ItemCollectionList verhalt = new()
            {
                { "Linie direkt zwischen zwei Punkten", ((int)enConectorStyle.Direct).ToString(), QuickImage.Get(enImageCode.Linie) },
                { "Linie soll Objekten ausweichen", ((int)enConectorStyle.Ausweichenx).ToString(), QuickImage.Get(enImageCode.Linie) },
                { "Linie soll Objekten ausweichen und rechtwinklig sein", ((int)enConectorStyle.AusweichenUndGerade).ToString(), QuickImage.Get(enImageCode.Linie) }
            };
            l.Add(new FlexiControlForProperty(this, "Linien-Verhalten", verhalt));
            AddLineStyleOption(l);
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        //public void Move(float x, float y) {
        //    _LastRecalc = DateTime.Now.AddHours(-1);
        //    Point1.SetTo(Point1.X + x, Point1.Y + y);
        //    Point2.SetTo(Point2.X + x, Point2.Y + y);
        //}
        //public override void SetCoordinates(RectangleF r)
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
                    Linien_Verhalten = (enConectorStyle)IntParse(value);
                    return true;
            }
            return false;
        }

        public override void PointMoved(object sender, MoveEventArgs e) => CalcTempPoints();

        public void SetCoordinates(float px1, float py1, float px2, float py2) {
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

        protected override RectangleF CalculateUsedArea() {
            if (Point1.X == 0d && Point2.X == 0d && Point1.Y == 0d && Point2.Y == 0d) { return RectangleF.Empty; }
            if (_tempPoints.Count == 0) { CalcTempPoints(); }
            if (_tempPoints.Count == 0) { return RectangleF.Empty; }
            var x1 = float.MaxValue;
            var y1 = float.MaxValue;
            var x2 = float.MinValue;
            var y2 = float.MinValue;
            foreach (var thisPoint in _tempPoints) {
                x1 = Math.Min(thisPoint.X, x1);
                y1 = Math.Min(thisPoint.Y, y1);
                x2 = Math.Max(thisPoint.X, x2);
                y2 = Math.Max(thisPoint.Y, y2);
            }
            return new RectangleF(x1, y1, x2 - x1, y2 - y1);
            //Return New Rectangle(CInt(Math.Min(Point1.X, Point2.X)), CInt(Math.Min(Point1.Y, Point2.Y)), CInt(Math.Abs(Point2.X - Point1.X)), CInt(Math.Abs(Point2.Y - Point1.Y)))
        }

        protected override string ClassId() => "LINE";

        protected override void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, float zoom, float shiftX, float shiftY, bool forPrinting) {
            if (Stil == PadStyles.Undefiniert) { return; }
            CalcTempPoints();
            if (_tempPoints.Count == 0) { return; }
            for (var z = 0; z <= _tempPoints.Count - 2; z++) {
                gr.DrawLine(Skin.GetBlueFont(Stil, Parent.SheetStyle).Pen(zoom * Parent.SheetStyleScale), _tempPoints[z].ZoomAndMove(zoom, shiftX, shiftY), _tempPoints[z + 1].ZoomAndMove(zoom, shiftX, shiftY));
            }
        }

        protected override void ParseFinished() { }

        private static bool SchneidetDas(BasicPadItem thisBasicItem, PointM p1, PointM p2) {
            if (thisBasicItem == null) { return false; }
            if (thisBasicItem is not LinePadItem) {
                var a = thisBasicItem.UsedArea;
                if (a.Width > 0 && a.Height > 0) {
                    a.Inflate(2, 2);

                    PointF tP1 = p1;
                    PointF tP2 = p2;
                    var lo = a.PointOf(enAlignment.Top_Left);
                    var ro = a.PointOf(enAlignment.Top_Right);
                    var lu = a.PointOf(enAlignment.Bottom_Left);
                    var ru = a.PointOf(enAlignment.Bottom_Right);
                    if (!LinesIntersect(tP1, tP2, lo, ro, true).IsEmpty ||
                        !LinesIntersect(tP1, tP2, lu, ru, true).IsEmpty ||
                        !LinesIntersect(tP1, tP2, lo, lu, true).IsEmpty ||
                        !LinesIntersect(tP1, tP2, ro, ru, true).IsEmpty) { return true; }
                }
            }
            return false;
        }

        private bool Begradige(int p1) {
            if (p1 >= _tempPoints.Count - 1) { return false; }
            if ((int)_tempPoints[p1].X == (int)_tempPoints[p1 + 1].X || (int)_tempPoints[p1].Y == (int)_tempPoints[p1 + 1].Y) { return false; }
            PointF np1;
            PointF np2;
            if ((int)(_tempPoints[p1].X - _tempPoints[p1 + 1].X) > (int)(_tempPoints[p1].Y - _tempPoints[p1 + 1].Y)) {
                np1 = new PointF(_tempPoints[p1].X, (_tempPoints[p1].Y + _tempPoints[p1 + 1].Y) / 2);
                np2 = new PointF(_tempPoints[p1 + 1].X, (_tempPoints[p1].Y + _tempPoints[p1 + 1].Y) / 2);
                _tempPoints.Insert(p1 + 1, np1);
                _tempPoints.Insert(p1 + 2, np2);
            } else {
                np1 = new PointF((_tempPoints[p1].X + _tempPoints[p1 + 1].X) / 2, _tempPoints[p1].Y);
                np2 = new PointF((_tempPoints[p1].X + _tempPoints[p1 + 1].X) / 2, _tempPoints[p1 + 1].Y);
                _tempPoints.Insert(p1 + 1, np1);
                _tempPoints.Insert(p1 + 2, np2);
            }
            return true;
        }

        private void CalcTempPoints() {
            var newCode = Point1 + Point2.ToString();
            if (_calcTempPointsCode != newCode) {
                _calcTempPointsCode = newCode;
                _tempPoints = null;
            }
            if (Linien_Verhalten != enConectorStyle.Direct && _tempPoints != null) {
                if (DateTime.Now.Subtract(_lastRecalc).TotalSeconds > 5) {
                    if (DateTime.Now.Subtract(_lastRecalc).TotalSeconds > 5 + Constants.GlobalRND.Next(10)) {
                        _tempPoints = null;
                    }
                    // r = Nothing
                }
            }
            if (_tempPoints != null && _tempPoints.Count > 1) { return; }
            _lastRecalc = DateTime.Now;
            _calcTempPointsCode = newCode;
            _tempPoints = new List<PointF>
            {
                Point1,
                Point2
            };
            if (Linien_Verhalten == enConectorStyle.Direct) { return; }
            var count = 0;
            do {
                count++;
                var again = false;
                if (_tempPoints.Count > 100) {
                    break;
                }
                for (var z = 0; z < _tempPoints.Count; z++) {
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

        private bool IsVerdeckt(float x, float y) {
            foreach (var thisBasicItem in Parent) {
                if (thisBasicItem != null) {
                    if (thisBasicItem is not LinePadItem) {
                        var a = thisBasicItem.UsedArea;
                        if (a.Width > 0 && a.Height > 0) {
                            a.Inflate(2, 2);
                            if (a.Contains(x, y)) { return true; }
                        }
                    }
                }
            }
            return false;
        }

        private bool LöscheVerdeckte(int p1) {
            if (_tempPoints[p1].Equals(Point1)) { return false; }
            if (_tempPoints[p1].Equals(Point2)) { return false; }

            if (IsVerdeckt(_tempPoints[p1].X, _tempPoints[p1].Y)) {
                _tempPoints.RemoveAt(p1);
                return true;
            }
            return false;
        }

        private bool SchneidetWas(float x1, float y1, float x2, float y2) {
            PointM p1 = new(x1, y1);
            PointM p2 = new(x2, y2);
            return Parent.Any(thisItemBasic => SchneidetDas(thisItemBasic, p1, p2));
        }

        private bool Vereinfache(int p1) {
            if (Linien_Verhalten != enConectorStyle.AusweichenUndGerade) {
                if (p1 > 0 && p1 < _tempPoints.Count - 1) {
                    if (!SchneidetWas(_tempPoints[p1 - 1].X, _tempPoints[p1 - 1].Y, _tempPoints[p1 + 1].X, _tempPoints[p1 + 1].Y)) {
                        _tempPoints.RemoveAt(p1);
                        return true;
                    }
                }
            }
            if (p1 < _tempPoints.Count - 3) {
                if ((int)_tempPoints[p1].X == (int)_tempPoints[p1 + 1].X && (int)_tempPoints[p1].X == (int)_tempPoints[p1 + 2].X) {
                    _tempPoints.RemoveAt(p1 + 1);
                    return true;
                }
                if ((int)_tempPoints[p1].Y == (int)_tempPoints[p1 + 1].Y && (int)_tempPoints[p1].Y == (int)_tempPoints[p1 + 2].Y) {
                    _tempPoints.RemoveAt(p1 + 1);
                    return true;
                }
            }
            if (p1 > 0 && p1 < _tempPoints.Count - 3) {
                if ((int)_tempPoints[p1].X == (int)_tempPoints[p1 + 1].X && (int)_tempPoints[p1 + 1].Y == (int)_tempPoints[p1 + 2].Y) {
                    if (!IsVerdeckt(_tempPoints[p1 + 2].X, _tempPoints[p1].Y)) {
                        if (!SchneidetWas(_tempPoints[p1 - 1].X, _tempPoints[p1 - 1].Y, _tempPoints[p1 + 2].X, _tempPoints[p1].Y)) {
                            if (!SchneidetWas(_tempPoints[p1 + 3].X, _tempPoints[p1 + 3].Y, _tempPoints[p1 + 2].X, _tempPoints[p1].Y)) {
                                _tempPoints[p1] = new PointF(_tempPoints[p1 + 2].X, _tempPoints[p1].Y);
                                _tempPoints.RemoveAt(p1 + 1);
                                _tempPoints.RemoveAt(p1 + 1);
                                return true;
                            }
                        }
                    }
                }
                if ((int)_tempPoints[p1].Y == (int)_tempPoints[p1 + 1].Y && (int)_tempPoints[p1 + 1].X == (int)_tempPoints[p1 + 2].X) {
                    if (!IsVerdeckt(_tempPoints[p1].X, _tempPoints[p1 + 2].Y)) {
                        if (!SchneidetWas(_tempPoints[p1 - 1].X, _tempPoints[p1 - 1].Y, _tempPoints[p1].X, _tempPoints[p1 + 2].Y)) {
                            if (!SchneidetWas(_tempPoints[p1 + 3].X, _tempPoints[p1 + 3].Y, _tempPoints[p1].X, _tempPoints[p1 + 2].Y)) {
                                _tempPoints[p1] = new PointF(_tempPoints[p1].X, _tempPoints[p1 + 2].Y);
                                _tempPoints.RemoveAt(p1 + 1);
                                _tempPoints.RemoveAt(p1 + 1);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool WeicheAus(int p1) {
            if (_tempPoints.Count > 100) { return false; }
            if (p1 >= _tempPoints.Count - 1) { return false; }
            //   If _TempPoints.Count > 4 Then Return False
            foreach (var thisItemBasic in Parent) {
                if (thisItemBasic != null) {
                    //    If ThisBasicItem IsNot Object1 AndAlso ThisBasicItem IsNot Object2 Then
                    if (thisItemBasic is not LinePadItem) {
                        var a = thisItemBasic.UsedArea;
                        if (a.Width > 0 && a.Height > 0) {
                            a.Inflate(2, 2);
                            var lo = a.PointOf(enAlignment.Top_Left);
                            var ro = a.PointOf(enAlignment.Top_Right);
                            var lu = a.PointOf(enAlignment.Bottom_Left);
                            var ru = a.PointOf(enAlignment.Bottom_Right);
                            var tOben = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lo, ro, true);
                            var tUnten = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lu, ru, true);
                            var tLinks = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lo, lu, true);
                            var trechts = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], ro, ru, true);
                            //    If DirectCast(Object2, RowFormulaItem).Row.CellFirst().String.Contains("Lilo") AndAlso DirectCast(Object1, RowFormulaItem).Row.CellFirst().String.Contains("Karl") Then Stop
                            if (tOben != null || tUnten != null || tLinks != null || trechts != null) {
                                a.Inflate(-50, -50);
                                lo = a.PointOf(enAlignment.Top_Left);
                                ro = a.PointOf(enAlignment.Top_Right);
                                lu = a.PointOf(enAlignment.Bottom_Left);
                                ru = a.PointOf(enAlignment.Bottom_Right);
                                var oben = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lo, ro, true);
                                var unten = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lu, ru, true);
                                var links = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lo, lu, true);
                                var rechts = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], ro, ru, true);
                                if (oben == null && tOben != null) {
                                    oben = tOben;
                                }
                                if (unten == null && tUnten != null) {
                                    unten = tUnten;
                                }
                                if (links == null && tLinks != null) {
                                    links = tLinks;
                                }
                                if (rechts == null && trechts != null) {
                                    rechts = trechts;
                                }
                                if (oben != null && unten != null) {
                                    if (_tempPoints[p1].Y < _tempPoints[p1 + 1].Y) {
                                        // Schneidet durch, von oben nach unten
                                        _tempPoints.Insert(p1 + 1, oben);
                                        if (Math.Abs(_tempPoints[p1].X - lo.X) > Math.Abs(_tempPoints[p1].X - ro.X)) {
                                            _tempPoints.Insert(p1 + 2, ro);
                                            _tempPoints.Insert(p1 + 3, ru);
                                        } else {
                                            _tempPoints.Insert(p1 + 2, lo);
                                            _tempPoints.Insert(p1 + 3, lu);
                                        }
                                        _tempPoints.Insert(p1 + 4, unten);
                                        return true;
                                    }
                                    // Schneidet durch, von unten nach oben
                                    _tempPoints.Insert(p1 + 1, unten);
                                    if (Math.Abs(_tempPoints[p1].X - lo.X) > Math.Abs(_tempPoints[p1].X - ro.X)) {
                                        _tempPoints.Insert(p1 + 2, ru);
                                        _tempPoints.Insert(p1 + 3, ro);
                                    } else {
                                        _tempPoints.Insert(p1 + 2, lu);
                                        _tempPoints.Insert(p1 + 3, lo);
                                    }
                                    _tempPoints.Insert(p1 + 4, oben);
                                    return true;
                                }
                                if (links != null && rechts != null) {
                                    if (_tempPoints[p1].X < _tempPoints[p1 + 1].X) {
                                        // Schneidet durch, von links nach rechts
                                        _tempPoints.Insert(p1 + 1, links);
                                        if (Math.Abs(_tempPoints[p1].Y - lo.Y) > Math.Abs(_tempPoints[p1].Y - lu.Y)) {
                                            _tempPoints.Insert(p1 + 2, lu);
                                            _tempPoints.Insert(p1 + 3, ru);
                                        } else {
                                            _tempPoints.Insert(p1 + 2, lo);
                                            _tempPoints.Insert(p1 + 3, ro);
                                        }
                                        _tempPoints.Insert(p1 + 4, rechts);
                                        return true;
                                    }
                                    // Schneidet durch, von rechts nach links
                                    _tempPoints.Insert(p1 + 1, rechts);
                                    if (Math.Abs(_tempPoints[p1].Y - lo.Y) > Math.Abs(_tempPoints[p1].Y - lu.Y)) {
                                        _tempPoints.Insert(p1 + 2, ru);
                                        _tempPoints.Insert(p1 + 3, lu);
                                    } else {
                                        _tempPoints.Insert(p1 + 2, ro);
                                        _tempPoints.Insert(p1 + 3, lo);
                                    }
                                    _tempPoints.Insert(p1 + 4, links);
                                    return true;
                                }
                                if (unten != null && rechts != null) {
                                    if (_tempPoints[p1].X < _tempPoints[p1 + 1].X) {
                                        _tempPoints.Insert(p1 + 1, unten);
                                        _tempPoints.Insert(p1 + 2, ru);
                                        _tempPoints.Insert(p1 + 3, rechts);
                                        return true;
                                    }
                                    _tempPoints.Insert(p1 + 1, rechts);
                                    _tempPoints.Insert(p1 + 2, ru);
                                    _tempPoints.Insert(p1 + 3, unten);
                                    return true;
                                }
                                if (oben != null && rechts != null) {
                                    if (_tempPoints[p1].X < _tempPoints[p1 + 1].X) {
                                        _tempPoints.Insert(p1 + 1, oben);
                                        _tempPoints.Insert(p1 + 2, ro);
                                        _tempPoints.Insert(p1 + 3, rechts);
                                        return true;
                                    }
                                    _tempPoints.Insert(p1 + 1, rechts);
                                    _tempPoints.Insert(p1 + 2, ro);
                                    _tempPoints.Insert(p1 + 3, oben);
                                    return true;
                                }
                                if (unten != null && links != null) {
                                    if (_tempPoints[p1].X < _tempPoints[p1 + 1].X) {
                                        _tempPoints.Insert(p1 + 1, links);
                                        _tempPoints.Insert(p1 + 2, lu);
                                        _tempPoints.Insert(p1 + 3, unten);
                                        return true;
                                    }
                                    _tempPoints.Insert(p1 + 1, unten);
                                    _tempPoints.Insert(p1 + 2, lu);
                                    _tempPoints.Insert(p1 + 3, links);
                                    return true;
                                }
                                if (oben != null && links != null) {
                                    if (_tempPoints[p1].X < _tempPoints[p1 + 1].X) {
                                        _tempPoints.Insert(p1 + 1, links);
                                        _tempPoints.Insert(p1 + 2, lo);
                                        _tempPoints.Insert(p1 + 3, oben);
                                        return true;
                                    }
                                    _tempPoints.Insert(p1 + 1, oben);
                                    _tempPoints.Insert(p1 + 2, lo);
                                    _tempPoints.Insert(p1 + 3, links);
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