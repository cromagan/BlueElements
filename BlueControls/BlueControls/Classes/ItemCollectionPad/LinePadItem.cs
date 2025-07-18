﻿// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static BlueBasics.Converter;
using static BlueBasics.Geometry;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad;

public class LinePadItem : AbstractPadItem, IStyleableOne {

    #region Fields

    private readonly PointM _point1;

    private readonly PointM _point2;

    private string _calcTempPointsCode = string.Empty;

    private DateTime _lastRecalc = DateTime.UtcNow.AddHours(-1);

    private PadStyles _style = PadStyles.Standard;

    private List<PointF>? _tempPoints;

    #endregion

    #region Constructors

    public LinePadItem() : this(string.Empty, PadStyles.Standard, Point.Empty, Point.Empty) { }

    public LinePadItem(PadStyles format, Point point1, Point point2) : this(string.Empty, format, point1, point2) { }

    public LinePadItem(string keyName, PadStyles format, Point point1, Point point2) : base(keyName) {
        _point1 = new PointM(this, "Punkt 1", 0, 0);
        _point2 = new PointM(this, "Punkt 2", 0, 0);
        _point1.SetTo(point1, false);
        _point2.SetTo(point2, false);
        MovablePoint.Add(_point1);
        MovablePoint.Add(_point2);
        PointsForSuccesfullyMove.AddRange(MovablePoint);
        CalculateJointMiddle(_point1, _point2);
        _style = format;
        _tempPoints = [];
        Linien_Verhalten = ConectorStyle.Direct;
    }

    #endregion

    #region Properties

    public static string ClassId => "LINE";

    public override string Description => string.Empty;

    public BlueFont? Font { get; set; }

    public ConectorStyle Linien_Verhalten { get; set; }

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : string.Empty;

    public PadStyles Style {
        get => _style;
        set {
            if (_style == value) { return; }
            _style = value;
            this.InvalidateFont();
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override bool Contains(PointF value, float zoomfactor) {
        var ne = 5 / zoomfactor;
        if (_point1.X == 0d && _point2.X == 0d && _point1.Y == 0d && _point2.Y == 0d) { return false; }
        if (_tempPoints is { Count: 0 }) { CalcTempPoints(); }
        if (_tempPoints is { Count: 0 }) { return false; }

        if (_tempPoints != null) {
            for (var z = 0; z <= _tempPoints.Count - 2; z++) {
                if (value.DistanzZuStrecke(_tempPoints[z], _tempPoints[z + 1]) < ne) {
                    return true;
                }
            }
        }

        return false;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<AbstractListItem> verhalt =
        [
            ItemOf("Linie direkt zwischen zwei Punkten", ((int)ConectorStyle.Direct).ToString(),
                QuickImage.Get(ImageCode.Linie)),
            ItemOf("Linie soll Objekten ausweichen", ((int)ConectorStyle.Ausweichenx).ToString(),
                QuickImage.Get(ImageCode.Linie)),
            ItemOf("Linie soll Objekten ausweichen und rechtwinklig sein",
                ((int)ConectorStyle.AusweichenUndGerade).ToString(), QuickImage.Get(ImageCode.Linie))
        ];

        List<GenericControl> result =
        [
            new FlexiControlForProperty<ConectorStyle>(() => Linien_Verhalten, verhalt),
            new FlexiControlForProperty<PadStyles>(() => Style, Skin.GetRahmenArt(SheetStyle, true))
        ];

        result.AddRange(base.GetProperties(widthOfControl));
        return result;
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
    //        Develop.DebugPrint(ErrorType.Error, "Interner Name nicht vergeben.");
    //    }
    //}
    public override void InitialPosition(int x, int y, int width, int height) {
        _point1.SetTo(x, y + (height / 2), false);
        _point2.SetTo(x + width, y + (height / 2), false);
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Connection", Linien_Verhalten);
        result.ParseableAdd("Style", _style);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "connection":
                Linien_Verhalten = (ConectorStyle)IntParse(value);
                return true;

            case "style":
                _style = (PadStyles)IntParse(value);
                _style = Skin.RepairStyle(_style);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override void PointMoved(object sender, MoveEventArgs e) {
        if (sender is not PointM point) { return; }

        if (JointPoints.Contains(point)) {
            base.PointMoved(sender, e);
            return;
        }

        CalcTempPoints();
        CalculateJointMiddle(_point1, _point2);
        base.PointMoved(sender, e);
    }

    public override string ReadableText() => "Line";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Linie, 16);

    protected override RectangleF CalculateUsedArea() {
        if (_point1.X == 0d && _point2.X == 0d && _point1.Y == 0d && _point2.Y == 0d) { return RectangleF.Empty; }
        if (_tempPoints is not { Count: not 0 }) { CalcTempPoints(); }
        if (_tempPoints is not { Count: not 0 }) { return RectangleF.Empty; }
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

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        UnRegisterEvents();
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        if (_style != PadStyles.Undefiniert) {
            CalcTempPoints();
            if (_tempPoints is not { Count: not 0 } || Parent == null) { return; }

            for (var z = 0; z <= _tempPoints.Count - 2; z++) {
                gr.DrawLine(this.GetFont().Pen(scale), _tempPoints[z].ZoomAndMove(scale, shiftX, shiftY), _tempPoints[z + 1].ZoomAndMove(scale, shiftX, shiftY));
            }
        }
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        this.InvalidateFont();
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged += Icpi_StyleChanged;
        }
    }

    protected override void OnParentChanging() {
        base.OnParentChanging();
        UnRegisterEvents();
    }

    private static bool SchneidetDas(AbstractPadItem? thisBasicItem, PointM p1, PointM p2) {
        if (thisBasicItem == null) { return false; }
        if (thisBasicItem is not LinePadItem) {
            var a = thisBasicItem.UsedArea;
            if (a is { Width: > 0, Height: > 0 }) {
                a.Inflate(2, 2);

                PointF tP1 = p1;
                PointF tP2 = p2;
                var lo = a.PointOf(Alignment.Top_Left);
                var ro = a.PointOf(Alignment.Top_Right);
                var lu = a.PointOf(Alignment.Bottom_Left);
                var ru = a.PointOf(Alignment.Bottom_Right);
                if (!LinesIntersect(tP1, tP2, lo, ro, true).IsEmpty ||
                    !LinesIntersect(tP1, tP2, lu, ru, true).IsEmpty ||
                    !LinesIntersect(tP1, tP2, lo, lu, true).IsEmpty ||
                    !LinesIntersect(tP1, tP2, ro, ru, true).IsEmpty) { return true; }
            }
        }
        return false;
    }

    private bool Begradige(int p1) {
        if (_tempPoints == null || p1 >= _tempPoints.Count - 1) { return false; }
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
        var newCode = _point1 + _point2.ParseableItems().FinishParseable();
        if (_calcTempPointsCode != newCode) {
            _calcTempPointsCode = newCode;
            _tempPoints = null;
        }
        if (Linien_Verhalten != ConectorStyle.Direct && _tempPoints != null) {
            if (DateTime.UtcNow.Subtract(_lastRecalc).TotalSeconds > 5) {
                if (DateTime.UtcNow.Subtract(_lastRecalc).TotalSeconds > 5 + Constants.GlobalRnd.Next(10)) {
                    _tempPoints = null;
                }
                // r = Nothing
            }
        }
        if (_tempPoints is { Count: > 1 }) { return; }
        _lastRecalc = DateTime.UtcNow;
        _calcTempPointsCode = newCode;
        _tempPoints =
        [
            _point1,
            _point2
        ];
        if (Linien_Verhalten == ConectorStyle.Direct) { return; }
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
                if (Linien_Verhalten == ConectorStyle.AusweichenUndGerade && Begradige(z)) {
                    again = true;
                    break;
                }
                if (Linien_Verhalten is ConectorStyle.AusweichenUndGerade or ConectorStyle.Ausweichenx) {
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

    private void Icpi_StyleChanged(object sender, System.EventArgs e) => this.InvalidateFont();

    private bool IsVerdeckt(float x, float y) {
        if (Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return false; }

        foreach (var thisBasicItem in icpi) {
            if (thisBasicItem is { IsDisposed: false } and not LinePadItem) {
                var a = thisBasicItem.UsedArea;
                if (a is { Width: > 0, Height: > 0 }) {
                    a.Inflate(2, 2);
                    if (a.Contains(x, y)) { return true; }
                }
            }
        }
        return false;
    }

    private bool LöscheVerdeckte(int p1) {
        if (_tempPoints?[p1] is not { } p) { return false; }

        //if (Math.Abs(p.X - _point1.X) < DefaultTolerance && Math.Abs(p.Y - _point1.Y) < DefaultTolerance) { return false; }
        //if (Math.Abs(p.X - _point2.X) < DefaultTolerance && Math.Abs(p.Y - _point2.Y) < DefaultTolerance) { return false; }

        if (IsVerdeckt(p.X, p.Y)) {
            _tempPoints.RemoveAt(p1);
            return true;
        }
        return false;
    }

    private bool SchneidetWas(float x1, float y1, float x2, float y2) {
        if (Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return false; }
        PointM p1 = new(x1, y1);
        PointM p2 = new(x2, y2);
        return icpi.Any(thisItemBasic => SchneidetDas(thisItemBasic, p1, p2));
    }

    private void UnRegisterEvents() {
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged -= Icpi_StyleChanged;
        }
    }

    private bool Vereinfache(int p1) {
        if (_tempPoints == null) { return false; }

        if (Linien_Verhalten != ConectorStyle.AusweichenUndGerade) {
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
        if (_tempPoints == null) { return false; }
        if (Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return false; }

        if (_tempPoints.Count > 100) { return false; }
        if (p1 >= _tempPoints.Count - 1) { return false; }
        //   If _TempPoints.Count > 4 Then Return False
        foreach (var thisItemBasic in icpi) {
            if (thisItemBasic is { IsDisposed: false } and not LinePadItem)
            //    If ThisBasicItem IsNot Object1 AndAlso ThisBasicItem IsNot Object2 Then
            {
                var a = thisItemBasic.UsedArea;
                if (a is { Width: > 0, Height: > 0 }) {
                    a.Inflate(2, 2);
                    var lo = a.PointOf(Alignment.Top_Left);
                    var ro = a.PointOf(Alignment.Top_Right);
                    var lu = a.PointOf(Alignment.Bottom_Left);
                    var ru = a.PointOf(Alignment.Bottom_Right);
                    var tOben = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lo, ro, true);
                    var tUnten = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lu, ru, true);
                    var tLinks = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lo, lu, true);
                    var trechts = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], ro, ru, true);
                    //    If DirectCast(Object2, RowFormulaItem).Row.CellFirst().String.Contains("Lilo") AndAlso DirectCast(Object1, RowFormulaItem).Row.CellFirst().String.Contains("Karl") Then Stop
                    if (!tOben.IsEmpty || !tUnten.IsEmpty || !tLinks.IsEmpty || !trechts.IsEmpty) {
                        a.Inflate(-50, -50);
                        lo = a.PointOf(Alignment.Top_Left);
                        ro = a.PointOf(Alignment.Top_Right);
                        lu = a.PointOf(Alignment.Bottom_Left);
                        ru = a.PointOf(Alignment.Bottom_Right);
                        var oben = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lo, ro, true);
                        var unten = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lu, ru, true);
                        var links = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], lo, lu, true);
                        var rechts = LinesIntersect(_tempPoints[p1], _tempPoints[p1 + 1], ro, ru, true);
                        if (oben.IsEmpty && !tOben.IsEmpty) {
                            oben = tOben;
                        }
                        if (unten.IsEmpty && !tUnten.IsEmpty) {
                            unten = tUnten;
                        }
                        if (links.IsEmpty && !tLinks.IsEmpty) {
                            links = tLinks;
                        }
                        if (rechts.IsEmpty && !trechts.IsEmpty) {
                            rechts = trechts;
                        }
                        if (!oben.IsEmpty && !unten.IsEmpty) {
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
                        if (!links.IsEmpty && !rechts.IsEmpty) {
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
                        if (!unten.IsEmpty && !rechts.IsEmpty) {
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
                        if (!oben.IsEmpty && !rechts.IsEmpty) {
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
                        if (!unten.IsEmpty && !links.IsEmpty) {
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
                        if (!oben.IsEmpty && !links.IsEmpty) {
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
        return false;
    }

    #endregion
}