// Authors:
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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.Abstract;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.ItemCollectionList;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.Polygons;
using BlueControls.ItemCollectionList;

#nullable enable

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;

/// <summary>
/// Standard für Objekte, die einen Datenbank/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt haben, werden als anzeigewürdig gewertet.
/// </summary>

public abstract class FakeControlPadItem : RectanglePadItemWithVersion, IItemToControl, IHasKeyName, IPropertyChangedFeedback, IHasVersion, IReadableText, IErrorCheckable {

    #region Fields

    public static readonly BlueFont CaptionFnt = Skin.GetBlueFont(Design.Caption, States.Standard);

    #endregion

    #region Constructors

    protected FakeControlPadItem(string internalname) : base(internalname) => SetCoordinates(new RectangleF(0, 0, 50, 30), true);

    #endregion

    #region Properties

    public abstract bool MustBeInDrawingArea { get; }
    public ReadOnlyCollection<string> VisibleFor { get; set; } = new([]);
    protected override int SaveOrder => 3;

    #endregion

    #region Methods

    //public abstract Database? DatabaseInput { get; }
    public void Breite_berechnen() {
        var li = new List<AbstractListItem>();
        for (var br = 1; br <= 20; br++) {
            li.Add(Add(br + " Spalte(n)", br.ToString(), true, br.ToString(Constants.Format_Integer3) + Constants.FirstSortChar));

            for (var pos = 1; pos <= br; pos++) {
                li.Add(Add(br + " Spalte(n) - Position: " + pos, br + ";" + pos, false, br.ToString(Constants.Format_Integer3) + Constants.SecondSortChar + pos.ToString(Constants.Format_Integer3)));
            }
        }

        var x2 = InputBoxListBoxStyle.Show("Bitte Breite und Position wählen:", li, CheckBehavior.SingleSelection, null, AddType.None);

        if (x2 == null || x2.Count != 1) { return; }

        var doit = x2[0].SplitBy(";");

        var anzbr = IntParse(doit[0]);
        var npos = IntParse(doit[1]);
        SetXPosition(anzbr, npos);
        OnPropertyChanged();
    }

    //public abstract int InputColorId { get; set; }
    public abstract Control? CreateControl(ConnectedFormulaView parent);

    public virtual string ErrorReason() {
        if (Parent == null) {
            return "Keiner Ansicht zugeordnet.";
        }

        if (MustBeInDrawingArea && !IsInDrawingArea(UsedArea, Parent.SheetSizeInPix.ToSize())) {
            return "Element ist nicht im Zeichenbereich."; // Invalidate löste die Berechnungen aus, muss sein, weil mehrere Filter die Berechnungen triggern
        }

        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        List<GenericControl> l = [];

        if (Bei_Export_sichtbar) {
            l.Add(new FlexiControl("Sichtbarkeit:", widthOfControl, true));
            l.Add(new FlexiControlForDelegate(Breite_berechnen, "Breite berechnen", ImageCode.Zeile));
            l.Add(new FlexiControlForDelegate(Standardhöhe_setzen, "Standardhöhe setzen", ImageCode.Zeile));

            var vf = new List<AbstractListItem>();
            vf.AddRange(BlueControls.ConnectedFormula.ConnectedFormula.VisibleFor_AllUsed());
            l.Add(new FlexiControlForProperty<ReadOnlyCollection<string>>(() => VisibleFor, "In diesen Ansichten sichtbar:", 5, vf, CheckBehavior.MultiSelection, AddType.Text));
        }

        //l.Add(new FlexiControl());
        //l.AddRange(base.GetStyleOptions(widthOfControl));
        return l;
    }

    public bool IsVisibleForMe(string mode) {
        //if(!Bei_Export_sichtbar ) { return false; }
        if (VisibleFor.Count == 0) { return true; }
        if (string.IsNullOrEmpty(mode)) { return true; }

        if (VisibleFor.Contains(Constants.Everybody, false)) { return true; }
        if (VisibleFor.Contains(mode, false)) { return true; }
        if (string.Equals(UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase) &&
    VisibleFor.Contains(Constants.Administrator, false)) { return true; }

        if (VisibleFor.Contains("#USER: " + UserName, false)) { return true; }
        if (VisibleFor.Contains("#USER:" + UserName, false)) { return true; }

        //if (!string.IsNullOrEmpty(modus) && Modes.Count > 0) {
        //}

        //if (string.IsNullOrEmpty(UserGroup) || string.IsNullOrEmpty(UserName)) { return true; }

        //if (UserGroup.Equals(Constants.Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }

        //if (VisibleFor.Contains(UserGroup, false)) { return true; }

        //   return modes.Any(mode => VisibleFor.Any(visible => string.Equals(mode, visible, StringComparison.OrdinalIgnoreCase)));

        return false;
    }

    public override void OnPropertyChanged() {
        if (IsDisposed) { return; }
        base.OnPropertyChanged();
        this.RaiseVersion();
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "visiblefor":
                value = value.Replace("\r", "|");
                var tmp = value.FromNonCritical().SplitBy("|").ToList();
                if (tmp.Count == 0) { tmp.Add(Constants.Everybody); }
                tmp = tmp.SortedDistinctList();
                VisibleFor = tmp.AsReadOnly();
                return true;
        }
        return false;
    }

    public abstract string ReadableText();

    public void SetXPosition(int anzahlSpaltenImFormular, int aufXPosition) {
        if (Parent == null) { return; }

        var x = UsedArea;
        x.Width = (Parent.SheetSizeInPix.Width - (AutosizableExtension.GridSize * (anzahlSpaltenImFormular - 1))) / anzahlSpaltenImFormular;
        x.X = (x.Width * (aufXPosition - 1)) + (AutosizableExtension.GridSize * (aufXPosition - 1));
        SetCoordinates(x, true);
    }

    public void Standardhöhe_setzen() {
        var x = UsedArea;

        var he = MmToPixel(ConnectedFormula.ConnectedFormula.StandardHöhe, ItemCollectionPad.Dpi);
        var he1 = MmToPixel(1, ItemCollectionPad.Dpi);
        x.Height = (int)(x.Height / he) * he;
        x.Height = (int)((x.Height / he1) + 0.99) * he1;

        if (x.Height < he) { x.Height = he; }
        SetCoordinates(x, true);
        OnPropertyChanged();
    }

    public abstract QuickImage? SymbolForReadableText();

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        //if (VisibleFor.Count == 0) { VisibleFor.Add(Constants.Everybody); }

        result.ParseableAdd("VisibleFor", VisibleFor);

        return result.Parseable(base.ToString());
    }

    protected static void DrawArrow(Graphics gr, RectangleF positionModified, float zoom, int colorId, Alignment al, float valueArrow, float xmod) {
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

        //if (!string.IsNullOrEmpty(symbol)) {
        //    var co = QuickImage.GenerateCode(symbol, (int)(5 * zoom), (int)(5 * zoom), ImageCodeEffect.Ohne, string.Empty, string.Empty, 120, 120, 0, 20, string.Empty);
        //    var sy = QuickImage.Get(co);
        //    gr.DrawImage(sy, p.X - (sy.Width / 2) + xmod, p.Y - valueSymbol);
        //}
    }

    protected void DrawArrorInput(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting, List<int>? colorId) {
        if (forPrinting) { return; }

        var arrowY = (int)(zoom * 12) * 0.35f;

        var width = (int)(zoom * 25);

        colorId ??= [];

        if (colorId.Count == 0) { colorId.Add(-1); }

        var start = -((colorId.Count - 1) * width / 2);

        foreach (var thisColorId in colorId) {
            DrawArrow(gr, positionModified, zoom, thisColorId, Alignment.Top_HorizontalCenter, arrowY, start);
            start += width;
        }
    }

    protected void DrawArrowOutput(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting, int colorId) {
        if (forPrinting) { return; }
        var arrowY = (int)(zoom * 12) * 0.45f;
        DrawArrow(gr, positionModified, zoom, colorId, Alignment.Bottom_HorizontalCenter, arrowY, 0);
    }

    protected void DrawColorScheme(Graphics gr, RectangleF drawingCoordinates, float zoom, List<int>? id, bool drawSymbol, bool drawText, bool transparent) {
        if (!transparent) {
            gr.FillRectangle(Brushes.White, drawingCoordinates);
        }

        var w = zoom * 3;

        var tmp = drawingCoordinates;
        tmp.Inflate(-w, -w);

        var c = Skin.IdColor(id);
        var c2 = c;

        if (c.IsNearWhite(0.99)) {
            c2 = Color.Gray;
            c = Color.Transparent;
        }

        if (transparent) {
            if (c.A > 128) {
                gr.DrawRectangle(new Pen(c.Brighten(0.9).MakeTransparent(128), w * 2), tmp);
            }

            gr.DrawRectangle(new Pen(c2.Brighten(0.6).MakeTransparent(128), zoom), drawingCoordinates);
        } else {
            gr.DrawRectangle(new Pen(c.Brighten(0.9), w * 2), tmp);
            gr.DrawRectangle(new Pen(c2.Brighten(0.6), zoom), drawingCoordinates);
        }

        if (drawSymbol && drawText) {
            Skin.Draw_FormatedText(gr, ReadableText(), SymbolForReadableText()?.Scale(zoom), Alignment.Horizontal_Vertical_Center, drawingCoordinates.ToRect(), null, true, ColumnFont?.Scale(zoom), false);
        } else if (drawSymbol) {
            Skin.Draw_FormatedText(gr, string.Empty, SymbolForReadableText()?.Scale(zoom), Alignment.Horizontal_Vertical_Center, drawingCoordinates.ToRect(), null, true, ColumnFont?.Scale(zoom), false);
        } else if (drawText) {
            Skin.Draw_FormatedText(gr, ReadableText(), null, Alignment.Horizontal_Vertical_Center, drawingCoordinates.ToRect(), null, true, ColumnFont?.Scale(zoom), false);
        }
    }

    protected void DrawFakeControl(Graphics gr, RectangleF positionModified, float zoom, CaptionPosition captionPosition, string captiontxt, EditTypeFormula edittype) {
        Point cap;
        var uc = positionModified.ToRect();

        switch (captionPosition) {
            case CaptionPosition.ohne:
                cap = new Point(-1, -1);
                break;

            case CaptionPosition.Links_neben_dem_Feld_unsichtbar:
            case CaptionPosition.Links_neben_dem_Feld:
                cap = new Point(0, 0);
                uc.X += (int)(100 * zoom);
                uc.Width -= (int)(100 * zoom);
                break;

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
            Skin.Draw_Back(gr, Design.TextBox, States.Standard, uc, null, false);
            Skin.Draw_Border(gr, Design.TextBox, States.Standard, uc);

            //gr.FillRectangle(Brushes.LightGray, uc);
            //gr.DrawRectangle(new Pen(Color.Black, zoom), uc);
        }
    }

    #endregion
}