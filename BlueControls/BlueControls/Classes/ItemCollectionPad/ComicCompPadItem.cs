﻿// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueControls.ItemCollectionPad.Abstract;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Geometry;
using BlueControls.EventArgs;

namespace BlueControls.ItemCollectionPad;

public class ComicCompPadItem : AbstractPadItem {

    #region Fields

    /// <summary>
    /// Haupt Gelenkpunkt 1
    /// </summary>
    public readonly PointM P1;

    /// <summary>
    /// Haupt Gelenkpunkt 2
    /// </summary>
    public readonly PointM P2;

    /// <summary>
    /// Diese Punkte bestimmen die gedrehten Eckpunkte des Bildes und werden von den Gelenkpunkten aus berechnet. Unskaliert und auch ohne Berücksichtigung der 'Move' Koordinaten
    /// </summary>
    private readonly PointM _ber_Lo = new();

    private readonly PointM _ber_Lu = new();
    private readonly PointM _ber_Ro = new();
    private readonly PointM _ber_Ru = new();

    private Bitmap? _bitmap;
    private int _width;

    #endregion

    public override List<GenericControl> GetProperties(int widthOfControl) {
        //List<GenericControl> result =
        //[new FlexiControlForDelegate(Skript_Bearbeiten, "Skript bearbeiten", ImageCode.Skript),
        //.. base.GetProperties(widthOfControl),
        //];
        //return result;

        List<GenericControl> result =
[

            //new FlexiControl("Einstellungen:", widthOfControl, true),
            new FlexiControlForProperty<int>(() => Width),
                .. base.GetProperties(widthOfControl),

        ];
        return result;

    }

    #region Constructors

    public ComicCompPadItem() : this(string.Empty, null) { }

    public ComicCompPadItem(string keyName, Bitmap? bitmap) : base(keyName) {
        _bitmap = bitmap;
        _width = 100;
        P1 = new PointM(this, "Punkt1", 0, 0);
        P2 = new PointM(this, "Punkt2", 0, 0);
        PointsForSuccesfullyMove.Add(P1);
        PointsForSuccesfullyMove.Add(P2);
        MovablePoint.Add(P1);
        MovablePoint.Add(P2);
        _bitmap = null;
        CalculateJointMiddle(P1, P2);
        ImageChanged();
    }

    #endregion

    #region Properties

    public static string ClassId => "COMIC";

    public Bitmap? Bitmap {
        get => _bitmap;
        set {
            _bitmap = value;
            ImageChanged();
        }
    }

    public override string Description => string.Empty;
    public override string MyClassId => ClassId;
    protected override int SaveOrder => 999;

    #endregion

    #region Methods



    public override bool Contains(PointF value, float zoomfactor) {
        var ne = 5 / zoomfactor;
        if (value.DistanzZuStrecke(P1, P2) < ne) { return true; }
        foreach (var thispoint in PointsForSuccesfullyMove) {
            if (GetLenght(value, (PointF)thispoint) < ne) { return true; }
        }
        return false;
    }

    public override void PointMoved(object sender, MoveEventArgs e) {

        if (sender == P1 || sender == P2) {
           CalculateJointMiddle(P1,P2);
        }

        base.PointMoved(sender, e);
    }



    public Bitmap GetTransformedBitmap() {
        var r = UsedArea;
        var i = new Bitmap((int)r.Width, (int)r.Height);
        var gr = Graphics.FromImage(i);
        gr.Clear(Color.White);
        var p = new PointF[4];
        p[0] = (PointF)_ber_Lo;
        p[1] = (PointF)_ber_Ro;
        p[2] = (PointF)_ber_Lu;
        p[3] = (PointF)_ber_Ru;
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        for (var z = 0; z <= 3; z++) {
            minX = Math.Min(p[z].X, minX);
            minY = Math.Min(p[z].Y, minY);
        }
        for (var z = 0; z <= 3; z++) {
            p[z].X -= minX;
            p[z].Y -= minY;
        }
        PointF[] destPara2 = { p[0], p[1], p[2] }; //LO,RO,RU
        if (_bitmap != null) {
            gr.DrawImage(_bitmap, destPara2, new RectangleF(0, 0, _bitmap.Width, _bitmap.Height), GraphicsUnit.Pixel);
        }
        gr.Dispose();
        return i;
    }

    public override void InitialPosition(int x, int y, int width, int height) {
        P1.SetTo(x + (width / 2), y, false);
        P2.SetTo(x + (width / 2), y + height, false);
    }





    public override bool ParseThis(string tag, string value) {
        Develop.DebugPrint_NichtImplementiert(true);
        return true;
    }

    public override void ProcessStyleChange() {
        // Keine Variablen zum Reseten, ein Invalidate reicht
    }


    public int Width {
        get => _width;
        set {
            if (_width == value) { return; }
            _width = value;
            OnPropertyChanged();
        }
    }

    public void SetCoordinates(Rectangle r) {
        _width = r.Width;
        P1.SetTo(r.PointOf(Alignment.Top_HorizontalCenter), false);
        P2.SetTo(r.PointOf(Alignment.Bottom_HorizontalCenter), false);
    }

    public override string ToParseableString() => string.Empty;

    protected override RectangleF CalculateUsedArea() {
        //var wp12 = AngleOfMiddleLine();
        var angleOfMiddleLine = GetAngle(P1, JointMiddle);

        _ber_Lo.SetTo(P1, _width / 2, angleOfMiddleLine - 90, false);
        _ber_Ro.SetTo(P1, _width / 2, angleOfMiddleLine + 90, false);
        _ber_Lu.SetTo(P2, _width / 2, angleOfMiddleLine - 90, false);
        _ber_Ru.SetTo(P2, _width / 2, angleOfMiddleLine + 90, false);
        var x = new List<PointM>
        {
            P1,
            P2,
            _ber_Lo,
            _ber_Ro,
            _ber_Lu,
            _ber_Ru
        };
        var x1 = int.MaxValue;
        var y1 = int.MaxValue;
        var x2 = int.MinValue;
        var y2 = int.MinValue;
        foreach (var thisPoint in x) {
            x1 = (int)Math.Min(thisPoint.X, x1);
            y1 = (int)Math.Min(thisPoint.Y, y1);
            x2 = (int)Math.Max(thisPoint.X, x2);
            y2 = (int)Math.Max(thisPoint.Y, y2);
        }
        return new RectangleF(x1, y1, x2 - x1, y2 - y1);
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        var lOt = _ber_Lo.ZoomAndMove(zoom, shiftX, shiftY);
        var rOt = _ber_Ro.ZoomAndMove(zoom, shiftX, shiftY);
        var rUt = _ber_Ru.ZoomAndMove(zoom, shiftX, shiftY);
        var lUt = _ber_Lu.ZoomAndMove(zoom, shiftX, shiftY);
        PointF[] destPara2 = { lOt, rOt, lUt };
        if (_bitmap != null) {
            gr.DrawImage(_bitmap, destPara2, new RectangleF(0, 0, _bitmap.Width, _bitmap.Height), GraphicsUnit.Pixel);
        }
        if (_bitmap == null || !forPrinting) {
            gr.DrawLine(ZoomPad.PenGray, lOt, rOt);
            gr.DrawLine(ZoomPad.PenGray, rOt, rUt);
            gr.DrawLine(ZoomPad.PenGray, rUt, lUt);
            gr.DrawLine(ZoomPad.PenGray, lUt, lOt);
            gr.DrawLine(ZoomPad.PenGray, P1.ZoomAndMove(zoom, shiftX, shiftY), P2.ZoomAndMove(zoom, shiftX, shiftY));
        }
        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }




    private void ImageChanged() {
        P1.X = 0f;
        P1.Y = 0f;
        if (_bitmap == null) {
            P2.X = 100f;
            P2.Y = 100f;
        } else {
            P2.X = _bitmap.Width;
            P2.Y = _bitmap.Height;
        }
        OnPropertyChanged();
    }

    public override string ReadableText() => "Bewegliches Element";
    public override QuickImage? SymbolForReadableText() => null;

    #endregion

}