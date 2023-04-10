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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollection;

/// <summary>
/// Dieses Element kann eine Zeile empfangen, und einen Filter für eine andere Datenbank basteln und diesen abgeben.
/// Unsichtbares Element, wird nicht angezeigt
/// </summary>

public class InputRowOutputFilterPadItem : RectanglePadItemWithVersion, IReadableText, IItemToControl, IItemAcceptRow, IItemSendFilter {

    #region Fields

    private readonly ItemAcceptRow _itemAccepts;
    private readonly ItemSendFilter _itemSends;

    #endregion

    #region Constructors

    public InputRowOutputFilterPadItem(string keyname, string toParse) : this(keyname, null as DatabaseAbstract) => Parse(toParse);

    public InputRowOutputFilterPadItem(DatabaseAbstract? db) : this(string.Empty, db) { }

    public InputRowOutputFilterPadItem(string intern, DatabaseAbstract? db) : base(intern) {
        _itemAccepts = new();
        _itemSends = new();

        OutputDatabase = db;
    }

    public InputRowOutputFilterPadItem(string intern) : this(intern, null as DatabaseAbstract) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-UserSelectionFilter";

    public ReadOnlyCollection<string>? ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public IItemSendRow? GetRowFrom {
        get => _itemAccepts.GetRowFromGet(this);
        set => _itemAccepts.GetRowFromSet(value, this);
    }

    public int InputColorId {
        get => _itemAccepts.InputColorIdGet();
        set => _itemAccepts.InputColorIdSet(value, this);
    }

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

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

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
        var l = base.GetStyleOptions();

        var u = new ItemCollectionList.ItemCollectionList(false);

        l.AddRange(_itemAccepts.GetStyleOptions(this));
        l.AddRange(_itemSends.GetStyleOptions(this));

        l.Add(new FlexiControl());
        u.AddRange(typeof(ÜberschriftAnordnung));

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions());

        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemSends.ParseThis(tag, value)) { return true; }
        if (_itemAccepts.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "id":
                //ColorId = IntParse(value);
                return true;
        }
        return false;
    }

    public string ReadableText() {
        if (OutputDatabase != null && !OutputDatabase.IsDisposed) {
            return "eine Zeile aus: " + OutputDatabase.Caption;
        }

        return "Zeile einer Datenbank";
    }

    public void RemoveChild(IHasKeyName remove) => _itemSends.RemoveChild(remove, this);

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(InputColorId));

    public override string ToString() {
        var result = new List<string>();
        result.AddRange(_itemAccepts.ParsableTags());
        result.AddRange(_itemSends.ParsableTags());
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, _itemAccepts.InputColorIdGet());
            RowEntryPadItem.DrawInputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", InputColorId);

            if (OutputDatabase != null && !OutputDatabase.IsDisposed) {
                var txt = "Filtergenerator für " + OutputDatabase.Caption;

                Skin.Draw_FormatedText(gr, txt, QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            } else {
                Skin.Draw_FormatedText(gr, "Filtergenerator", QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnFont?.Scale(zoom), false);
            }
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        RowEntryPadItem.DrawOutputArrow(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Filter", OutputColorId);
    }

    protected override void OnParentChanged() {
        base.OnParentChanged();
        _itemSends.DoParentChanged(this);
        //RepairConnections();
    }

    #endregion
}