﻿// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using static BlueBasics.Polygons;

namespace BlueControls.ItemCollection;

/// <summary>
/// Standard für Objekte, die einen Datenbank/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt haben, werden als anzeigewürdig gewertet.
/// </summary>

public abstract class FakeControlPadItem : RectanglePadItemWithVersion, IItemToControl, IHasKeyName, IChangedFeedback, IHasVersion, IReadableText, IErrorCheckable {

    #region Fields

    public static BlueFont CaptionFnt = Skin.GetBlueFont(Design.Caption, States.Standard);

    public List<string> VisibleFor = new();

    #endregion

    #region Constructors

    protected FakeControlPadItem(string internalname) : base(internalname) => SetCoordinates(new RectangleF(0, 0, 50, 30), true);

    #endregion

    //public abstract int InputColorId { get; set; }

    //public abstract DatabaseAbstract? InputDatabase { get; }

    #region Properties

    protected override int SaveOrder => 3;

    #endregion

    #region Methods

    public void Breite_berechnen() {
        var li = new ItemCollectionList.ItemCollectionList(true);
        for (var br = 1; br <= 20; br++) {
            _ = li.Add(br + " Spalte(n)", br.ToString(), true, br.ToString(Constants.Format_Integer3) + Constants.FirstSortChar);

            for (var pos = 1; pos <= br; pos++) {
                _ = li.Add(br + " Spalte(n) - Position: " + pos, br + ";" + pos, false, br.ToString(Constants.Format_Integer3) + Constants.SecondSortChar + pos.ToString(Constants.Format_Integer3));
            }
        }

        var x2 = InputBoxListBoxStyle.Show("Bitte Breite und Position wählen:", li, AddType.None, true);

        if (x2 == null || x2.Count != 1) { return; }

        var doit = x2[0].SplitBy(";");

        var anzbr = IntParse(doit[0]);
        var npos = IntParse(doit[1]);
        SetXPosition(anzbr, npos, 1);
        this.RaiseVersion();
    }

    public abstract Control? CreateControl(ConnectedFormulaView parent);

    public abstract string ErrorReason();

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();

        l.Add(new FlexiControlForDelegate(Breite_berechnen, "Breite berechnen", ImageCode.Zeile));
        l.Add(new FlexiControlForDelegate(Standardhöhe_setzen, "Standardhöhe setzen", ImageCode.Zeile));

        l.Add(new FlexiControlForDelegate(Sichtbarkeit, "Sichtbarkeit", ImageCode.Schild));

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions());
        return l;
    }

    public bool IsOk() => string.IsNullOrEmpty(ErrorReason());

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "visiblefor":
                value = value.Replace("\r", "|");

                var tmp = value.FromNonCritical().SplitBy("|");
                VisibleFor.Clear();
                foreach (var thiss in tmp) {
                    VisibleFor.Add(thiss.FromNonCritical());
                }
                if (VisibleFor.Count == 0) { VisibleFor.Add(DatabaseAbstract.Everybody); }
                return true;
        }
        return false;
    }

    public abstract string ReadableText();

    public void SetXPosition(int anzahlSpaltenImFormular, int aufXPosition, int breiteInspalten) {
        if (Parent == null) { return; }

        var x = UsedArea;
        x.Width = (Parent.SheetSizeInPix.Width - (MmToPixel(0.5f, 300) * (anzahlSpaltenImFormular - 1))) / anzahlSpaltenImFormular;
        x.X = (x.Width * (aufXPosition - 1)) + (MmToPixel(0.5f, 300) * (aufXPosition - 1));
        SetCoordinates(x, true);
    }

    public void Sichtbarkeit() {
        ItemCollectionList.ItemCollectionList aa = new(true);
        aa.AddRange(Permission_AllUsed());

        if (aa[DatabaseAbstract.Administrator] == null) { _ = aa.Add(DatabaseAbstract.Administrator); }
        //aa.Sort();
        aa.CheckBehavior = CheckBehavior.MultiSelection;
        aa.Check(VisibleFor, true);
        var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:", aa, AddType.Text, true);
        if (b == null) { return; }
        VisibleFor.Clear();

        VisibleFor.AddRange(b.ToArray());

        if (VisibleFor.Count > 1 && VisibleFor.Contains(DatabaseAbstract.Everybody, false)) {
            VisibleFor.Clear();
            VisibleFor.Add(DatabaseAbstract.Everybody);
        }

        if (VisibleFor.Count == 0) { VisibleFor.Add(DatabaseAbstract.Administrator); }
        this.RaiseVersion();
        OnChanged();
    }

    public void Standardhöhe_setzen() {
        var x = UsedArea;

        var he = MmToPixel(ConnectedFormula.ConnectedFormula.StandardHöhe, 300);
        var he1 = MmToPixel(1, 300);
        x.Height = (int)(x.Height / he) * he;
        x.Height = (int)((x.Height / he1) + 0.99) * he1;

        if (x.Height < he) { x.Height = he; }
        this.RaiseVersion();
        SetCoordinates(x, true);
    }

    public abstract QuickImage? SymbolForReadableText();

    public override string ToString() {
        var result = new List<string>();

        if (VisibleFor.Count == 0) { VisibleFor.Add(DatabaseAbstract.Everybody); }

        result.ParseableAdd("VisibleFor", VisibleFor);

        return result.Parseable(base.ToString());
    }

    internal bool IsVisibleForMe(string? myGroup, string? myName) {
        if (myGroup == null || myName == null) { return false; }

        if (VisibleFor == null || VisibleFor.Count == 0 || VisibleFor.Contains(DatabaseAbstract.Everybody, false)) { return true; }

        if (myGroup.Equals(DatabaseAbstract.Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }

        if (VisibleFor.Contains(myGroup, false)) { return true; }

        if (VisibleFor.Contains("#USER: " + myName, false)) { return true; }
        if (VisibleFor.Contains("#USER:" + myName, false)) { return true; }
        return false;
    }

    protected static void DrawArrow(Graphics gr, RectangleF positionModified, float zoom, string symbol, int colorId, Alignment al, float valueArrow, float valueSymbol, float xmod) {
        var p = positionModified.PointOf(al);
        var width = (int)(zoom * 25);
        var height = (int)(zoom * 12);
        var pa = Poly_Arrow(new Rectangle(0, -(width / 2), height, width));

        var c = Skin.IdColor(colorId);
        var c2 = c.Darken(0.4);

        gr.TranslateTransform(p.X + xmod, p.Y - valueArrow);

        //Info: das ergibt zwei übernanderliegenede Ellipsen
        //gr.DrawEllipse(Pens.MediumSeaGreen, new Rectangle(-20,-10,40,20));
        //gr.RotateTransform(90);
        //gr.DrawEllipse(Pens.MediumSeaGreen, new Rectangle(-10, -20, 20, 40));

        gr.RotateTransform(90);

        gr.FillPath(new SolidBrush(c), pa);
        gr.DrawPath(new Pen(c2, 1 * zoom), pa);

        gr.RotateTransform(-90);
        gr.TranslateTransform(-p.X - xmod, -p.Y + valueArrow);

        //var x = QuickImage.GenerateCode("Pfeil_Unten", (int)(10 * zoom), (int)(10 * zoom), ImageCodeEffect.Ohne, string.Empty, string.Empty, 100, 100, 0, 0, symbol);

        //var sy2 = QuickImage.Get(x);
        //gr.DrawImage(sy2, p.X - (sy2.Width / 2) + xmod, p.Y - valueSymbol);

        if (!string.IsNullOrEmpty(symbol)) {
            var co = QuickImage.GenerateCode(symbol, (int)(5 * zoom), (int)(5 * zoom), ImageCodeEffect.Ohne, string.Empty, string.Empty, 120, 120, 0, 20, string.Empty);
            var sy = QuickImage.Get(co);
            gr.DrawImage(sy, p.X - (sy.Width / 2) + xmod, p.Y - valueSymbol);
        }
    }

    protected void DrawArrorInput(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting, string symbol, List<int>? colorId) {
        if (forPrinting) { return; }

        var arrowY = (int)(zoom * 12) * 0.35f;
        var symbolY = (int)(zoom * 12) * 0.35f;

        var width = (int)(zoom * 25);

        colorId ??= new List<int>();

        if (colorId.Count == 0) { colorId.Add(-1); }

        var start = -((colorId.Count - 1) * width / 2);

        for (var pos = 0; pos < colorId.Count; pos++) {
            DrawArrow(gr, positionModified, zoom, symbol, colorId[pos], Alignment.Top_HorizontalCenter, arrowY, symbolY, start);
            start += width;
        }
    }

    protected void DrawArrowOutput(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting, string symbol, int colorId) {
        if (forPrinting) { return; }
        var arrowY = (int)(zoom * 12) * 0.45f;
        var symbolY = (int)(zoom * 12) * 0.02f;
        DrawArrow(gr, positionModified, zoom, symbol, colorId, Alignment.Bottom_HorizontalCenter, arrowY, symbolY, 0);
    }

    protected void DrawColorScheme(Graphics gr, RectangleF drawingCoordinates, float zoom, List<int>? id, bool drawSymbol, bool drawText) {
        gr.FillRectangle(Brushes.White, drawingCoordinates);

        var w = zoom * 3;

        var tmp = drawingCoordinates;
        tmp.Inflate(-w, -w);

        gr.DrawRectangle(new Pen(Skin.IdColor(id).Brighten(0.9), w * 2), tmp);

        gr.DrawRectangle(new Pen(Skin.IdColor(id).Brighten(0.6), zoom), drawingCoordinates);

        if (drawSymbol && drawText) {
            var v2 = SymbolForReadableText().Scale(zoom);
            Skin.Draw_FormatedText(gr, ReadableText(), v2, Alignment.Horizontal_Vertical_Center, drawingCoordinates.ToRect(), ColumnFont?.Scale(zoom), false);
        } else if (drawSymbol) {
            var v2 = SymbolForReadableText().Scale(zoom);
            Skin.Draw_FormatedText(gr, string.Empty, v2, Alignment.Horizontal_Vertical_Center, drawingCoordinates.ToRect(), ColumnFont?.Scale(zoom), false);
        } else if (drawText) {
            Skin.Draw_FormatedText(gr, ReadableText(), null, Alignment.Horizontal_Vertical_Center, drawingCoordinates.ToRect(), ColumnFont?.Scale(zoom), false);
        }
    }

    protected void DrawFakeControl(Graphics gr, RectangleF positionModified, float zoom, ÜberschriftAnordnung captionPosition, string captiontxt) {
        Point cap;
        var uc = positionModified.ToRect();

        switch (captionPosition) {
            case ÜberschriftAnordnung.ohne:
                cap = new Point(-1, -1);
                break;

            case ÜberschriftAnordnung.Links_neben_Dem_Feld:
                cap = new Point(0, 0);
                uc.X += (int)(100 * zoom);
                uc.Width -= (int)(100 * zoom);
                break;

            case ÜberschriftAnordnung.Ohne_mit_Abstand:
                cap = new Point(-1, -1);
                uc.Y += (int)(19 * zoom);
                uc.Height -= (int)(19 * zoom);
                break;

            case ÜberschriftAnordnung.Über_dem_Feld:
            default:
                cap = new Point(0, 0);
                uc.Y += (int)(19 * zoom);
                uc.Height -= (int)(19 * zoom);
                break;
        }

        if (cap.X >= 0) {
            var e = new RectangleF(positionModified.Left + (cap.X * zoom), positionModified.Top + (cap.Y * zoom), positionModified.Width, 16 * zoom);
            Skin.Draw_FormatedText(gr, captiontxt, null, Alignment.Top_Left, e.ToRect(), CaptionFnt.Scale(zoom), true);
        }

        if (uc.Width > 0 && uc.Height > 0) {
            gr.DrawRectangle(new Pen(Color.Black, zoom), uc);
        }
    }

    private List<string> Permission_AllUsed() {
        var l = new List<string>();
        if (Parent != null) {
            foreach (var thisIt in Parent) {
                if (thisIt is FakeControlPadItem csi) {
                    l.AddRange(csi.VisibleFor);
                }
            }
        }
        l.Add(DatabaseAbstract.Everybody);
        l.Add("#User: " + Generic.UserName());

        return l.SortedDistinctList();
    }

    #endregion
}