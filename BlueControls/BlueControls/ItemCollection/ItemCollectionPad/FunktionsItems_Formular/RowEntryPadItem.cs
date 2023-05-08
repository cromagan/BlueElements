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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase;
using static BlueBasics.Polygons;

namespace BlueControls.ItemCollection;

public class RowEntryPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemSendRow, IItemRowInput {

    #region Fields

    private readonly ItemSendRow _itemSends;

    #endregion

    #region Constructors

    public RowEntryPadItem(string keyname, string toParse) : this(keyname, null as DatabaseAbstract) => Parse(toParse);

    public RowEntryPadItem(DatabaseAbstract? db) : this(string.Empty, db) { }

    public RowEntryPadItem(string intern, DatabaseAbstract? db) : base(intern) {
        _itemSends = new();

        OutputDatabase = db;
    }

    public RowEntryPadItem(string intern) : this(intern, null as DatabaseAbstract) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowEntryElement";

    public ReadOnlyCollection<string>? ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public override string Description => " Diese Element ist in jedem Formular vorhanden und empfängt die Zeile aus einem anderen Element.\r\nHat NICHT IAcceptRowItem, da es nur von einer einzigen internen Routine befüllt werden darf.\r\n Unsichtbares Element, wird nicht angezeigt.";
    public override int InputColorId { get; set; }
    public override DatabaseAbstract? InputDatabase => OutputDatabase;

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    public DatabaseAbstract? OutputDatabase {
        get => _itemSends.OutputDatabaseGet();
        set => _itemSends.OutputDatabaseSet(value, this);
    }

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

        gr.TranslateTransform(p.X + (s2 / 2), p.Y - (s * 0.35f));

        gr.RotateTransform(90);

        gr.FillPath(new SolidBrush(c), pa);
        gr.DrawPath(new Pen(c2, 1 * zoom), pa);

        gr.RotateTransform(-90);
        gr.TranslateTransform(-p.X - (s2 / 2), -p.Y + (s * 0.35f));

        if (!string.IsNullOrEmpty(symbol)) {
            var co = QuickImage.GenerateCode(symbol, (int)(5 * zoom), (int)(5 * zoom), ImageCodeEffect.Ohne, string.Empty, string.Empty, 120, 120, 0, 20, string.Empty);
            var sy = QuickImage.Get(co);
            gr.DrawImage(sy, p.X - (sy.Width / 2), p.Y - (s * 0.15f));
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

        gr.TranslateTransform(p.X + (s2 / 2), p.Y - (s * 0.45f));

        gr.RotateTransform(90);
        gr.FillPath(new SolidBrush(c), pa);
        gr.DrawPath(new Pen(c2, 1 * zoom), pa);

        gr.RotateTransform(-90);

        gr.TranslateTransform(-p.X - (s2 / 2), -p.Y + (s * 0.45f));

        if (!string.IsNullOrEmpty(symbol)) {
            var co = QuickImage.GenerateCode(symbol, (int)(5 * zoom), (int)(5 * zoom), ImageCodeEffect.Ohne, string.Empty, string.Empty, 120, 120, 0, 20, string.Empty);

            var sy = QuickImage.Get(co);
            gr.DrawImage(sy, p.X - (sy.Width / 2), p.Y + (s * 0.02f));
        }
    }

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new RowEntryControl(OutputDatabase);
        //con.DoInputSettings(parent, this);
        con.DoOutputSettings(parent, this);
        return con;
    }

    public override string ErrorReason() {
        if (InputDatabase == null || InputDatabase.IsDisposed) {
            return "Quelle fehlt";
        }
        if (OutputDatabase == null || OutputDatabase.IsDisposed) {
            return "Ziel fehlt";
        }
        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();

        l.AddRange(_itemSends.GetStyleOptions(this));

        //l.Add(new FlexiControl());
        //l.AddRange(base.GetStyleOptions());

        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemSends.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "id": // TODO: 29.03.2023
                //Id = IntParse(value);
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        var txt = "Eingangs-Zeile: ";

        if (IsOk() && InputDatabase != null) {
            return txt + InputDatabase.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptSomething remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage? SymbolForReadableText() {
        if (IsOk()) {
            return QuickImage.Get(ImageCode.Zeile, 16, Color.Transparent, Skin.IDColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        var result = new List<string>();
        result.AddRange(_itemSends.ParsableTags());
        return result.Parseable(base.ToString());
    }

    internal override void AddedToCollection() {
        base.AddedToCollection();
        _itemSends.DoCreativePadAddedToCollection(this);
        //_itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawOutputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", InputColorId);
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawInputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", OutputColorId);
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        _itemSends.ParseFinished(this);
        //_itemAccepts.ParseFinished(this);
    }

    #endregion

    // Dummy, wird nicht benötigt
}