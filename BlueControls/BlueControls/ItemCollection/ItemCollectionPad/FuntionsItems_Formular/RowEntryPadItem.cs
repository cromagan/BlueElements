// Authors:
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.ConnectedFormula;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;
using static BlueControls.Interfaces.IItemSendSomethingExtensions;
using static BlueBasics.Polygons;

namespace BlueControls.ItemCollection;

/// <summary>
/// Diese Element ist in jedem Formular vorhanden und empfängt die Zeile aus einem anderen Element
/// Hat NICHT IAcceptRowItem, da es nur von einer einzigen internen Routine befüllt werden darf
/// Unsichtbares Element, wird nicht angezeigt
/// </summary>

public class RowEntryPadItem : AcceptSomethingPadItem, IReadableText, IItemToControl, IItemSendRow {

    #region Constructors

    public RowEntryPadItem(string keyname, string toParse) : this(keyname, null as DatabaseAbstract) => Parse(toParse);

    public RowEntryPadItem(DatabaseAbstract? db) : this(string.Empty, db) { }

    public RowEntryPadItem(string intern, DatabaseAbstract? db) : base(intern) {
        OutputDatabase = db;
    }

    public RowEntryPadItem(string intern) : this(intern, null as DatabaseAbstract) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowEntryElement";

    /// <summary>
    /// Laufende Nummer, bestimmt die Einfärbung
    /// </summary>
    public int ColorId { get; set; }

    public string Datenbank_wählen {
        get => string.Empty;
        set {
            var db = CommonDialogs.ChooseKnownDatabase();

            if (db == null) { return; }

            if (db == OutputDatabase) { return; }
            OutputDatabase = db;

            this.DoChilds();
            //RepairConnections();
        }
    }

    public string Datenbankkopf {
        get => string.Empty;
        set {
            if (OutputDatabase == null || OutputDatabase.IsDisposed) { return; }
            TableView.OpenDatabaseHeadEditor(OutputDatabase);
        }
    }

    public DatabaseAbstract? OutputDatabase { get; set; }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public static void DrawInputArrow(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting, string symbol, int colorId) {
        if (forPrinting) { return; }

        var p = positionModified.PointOf(Alignment.Top_HorizontalCenter);
        var s = (int)(zoom * 12);
        var s2 = (int)(zoom * 25);
        var pa = Poly_Arrow(new Rectangle(0, 0, s, s2));

        var c = Skin.IDColor(colorId);
        var c2 = c.Darken(0.4);

        gr.TranslateTransform(p.X + s2 / 2, p.Y - s * 0.35f);

        gr.RotateTransform(90);

        gr.FillPath(new SolidBrush(c), pa);
        gr.DrawPath(new Pen(c2, 1 * zoom), pa);

        gr.RotateTransform(-90);
        gr.TranslateTransform(-p.X - s2 / 2, -p.Y + s * 0.35f);

        if (!string.IsNullOrEmpty(symbol)) {
            var co = QuickImage.GenerateCode(symbol, (int)(5 * zoom), (int)(5 * zoom), ImageCodeEffect.Ohne, string.Empty, string.Empty, 120, 120, 0, 20, string.Empty);
            var sy = QuickImage.Get(co);
            gr.DrawImage(sy, p.X - sy.Width / 2, p.Y - s * 0.15f);
        }
    }

    public static void DrawOutputArrow(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting, string symbol, int colorId) {
        if (forPrinting) { return; }

        var p = positionModified.PointOf(Alignment.Bottom_HorizontalCenter);
        var s = (int)(zoom * 12);
        var s2 = (int)(zoom * 25);
        var pa = Poly_Arrow(new Rectangle(0, 0, s, s2));

        var c = Skin.IDColor(colorId);
        var c2 = c.Darken(0.4);

        gr.TranslateTransform(p.X + s2 / 2, p.Y - s * 0.45f);

        gr.RotateTransform(90);
        gr.FillPath(new SolidBrush(c), pa);
        gr.DrawPath(new Pen(c2, 1 * zoom), pa);

        gr.RotateTransform(-90);

        gr.TranslateTransform(-p.X - s2 / 2, -p.Y + s * 0.45f);

        if (!string.IsNullOrEmpty(symbol)) {
            var co = QuickImage.GenerateCode(symbol, (int)(5 * zoom), (int)(5 * zoom), ImageCodeEffect.Ohne, string.Empty, string.Empty, 120, 120, 0, 20, string.Empty);

            var sy = QuickImage.Get(co);
            gr.DrawImage(sy, p.X - sy.Width / 2, p.Y + s * 0.02f);
        }
    }

    public new Control CreateControl(ConnectedFormulaView parent) {
        //var con = new FlexiControlRowSelector(Database, FilterDefiniton, _überschrift, _anzeige) {
        //    EditType = _bearbeitung,
        //    CaptionPosition = CaptionPosition,
        //    Name = DefaultItemToControlName()
        //};
        //return con;
        Develop.DebugPrint_NichtImplementiert();
        return new Control();
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new() {
            new FlexiControlForProperty<string>(() => Datenbank_wählen, ImageCode.Datenbank),
            new FlexiControl()
        };
        if (OutputDatabase == null || OutputDatabase.IsDisposed) { return l; }

        l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "outputdatabase":

                var na = value.FromNonCritical();

                if (na.IsFormat(FormatHolder.FilepathAndName)) {
                    na = na.FilePath() + SqlBackAbstract.MakeValidTableName(na.FileNameWithoutSuffix()) + "." + na.FileSuffix();
                }

                OutputDatabase = DatabaseAbstract.GetById(new ConnectionInfo(na, null), null, string.Empty);
                return true;

            case "id": // TODO: 29.03.2023
                //Id = IntParse(value);
                return true;
        }
        return false;
    }

    public string ReadableText() {
        if (OutputDatabase != null && !OutputDatabase.IsDisposed) {
            return "Eingangs-Zeile der Datenbank: " + OutputDatabase.Caption;
        }

        return "Eingangs-Zeile einer Datenbank";
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(ColorId));

    public override string ToString() {
        var result = new List<string>();
        //result.ParseableAdd("ID", Id);
        result.ParseableAdd("OutputDatabase", OutputDatabase);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawOutputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", ColorId);
            DrawColorScheme(gr, positionModified, zoom, ColorId);

            if (OutputDatabase != null && !OutputDatabase.IsDisposed) {
                Skin.Draw_FormatedText(gr, "Eingangszeile: " + OutputDatabase.Caption, QuickImage.Get(ImageCode.Datenbank, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            } else {
                Skin.Draw_FormatedText(gr, "Eingangszeile: Bezug fehlt", QuickImage.Get(ImageCode.Datenbank, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            }
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawInputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", ColorId);
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        this.DoParentChanged();
        //RepairConnections();
    }

    #endregion
}