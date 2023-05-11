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
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

/// <summary>
/// Dieses Element kann eine Zeile empfangen, und einen Filter für eine andere Datenbank basteln und diesen abgeben.
/// Unsichtbares Element, wird nicht angezeigt
/// </summary>

public class InputRowOutputFilterPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptRow, IItemSendFilter {

    #region Fields

    private readonly ItemAcceptRow _itemAccepts;
    private readonly ItemSendFilter _itemSends;
    private string _eigangsWertSpalte = string.Empty;
    private string _filterSpalte = string.Empty;
    private FilterTypeRowInputItem _filtertype = FilterTypeRowInputItem.Ist_GrossKleinEgal;

    #endregion

    #region Constructors

    public InputRowOutputFilterPadItem(string keyname, string toParse) : this(keyname, null as DatabaseAbstract) => Parse(toParse);

    public InputRowOutputFilterPadItem(DatabaseAbstract? db) : this(string.Empty, db) { }

    public InputRowOutputFilterPadItem(string intern, DatabaseAbstract? db) : base(intern) {
        _itemAccepts = new();
        _itemSends = new();
        Bei_Export_sichtbar = false;
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

    public override string Description => "Dieses Element kann eine Zeile empfangen und einen Filter\r\nfür eine andere Datenbank erstellen und diesen abgeben.\r\nUnsichtbares Element, wird nicht angezeigt";

    public string EigangsWertSpalte {
        get => _eigangsWertSpalte;
        set {
            if (value == _eigangsWertSpalte) { return; }
            _eigangsWertSpalte = value;
            this.RaiseVersion();
            this.DoChilds();
            this.OnChanged();
        }
    }

    public FilterTypeRowInputItem Filter {
        get => _filtertype;
        set {
            if (value == _filtertype) { return; }
            _filtertype = value;
            this.RaiseVersion();
            this.DoChilds();
            this.OnChanged();
        }
    }

    public string FilterSpalte {
        get => _filterSpalte;
        set {
            if (value == _filterSpalte) { return; }
            _filterSpalte = value;
            this.RaiseVersion();
            this.DoChilds();
            this.OnChanged();
        }
    }

    public IItemSendRow? GetRowFrom {
        get => _itemAccepts.GetRowFromGet(this);
        set => _itemAccepts.GetRowFromSet(value, this);
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);

    public DatabaseAbstract? InputDatabase => _itemAccepts.InputDatabase(this);

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

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var i = _itemAccepts.InputDatabase(this)?.Column.Exists(_eigangsWertSpalte);
        var o = OutputDatabase?.Column.Exists(_filterSpalte);
        var con = new InputRowOutputFilterControl(i, o, _filtertype);
        con.DoOutputSettings(parent, this);
        con.DoInputSettings(parent, this);

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
        var l = new List<GenericControl>();

        l.AddRange(_itemAccepts.GetStyleOptions(this));

        var inr = _itemAccepts.GetRowFromGet(this);
        if (inr?.OutputDatabase is Database dbin) {
            var ic = new ItemCollectionList.ItemCollectionList(true);
            ic.AddRange(dbin.Column, true);
            l.Add(new FlexiControlForProperty<string>(() => EigangsWertSpalte, ic));

            var ic2 = new ItemCollectionList.ItemCollectionList(true);
            ic2.AddRange(typeof(FilterTypeRowInputItem));
            l.Add(new FlexiControlForProperty<FilterTypeRowInputItem>(() => Filter, ic2));
        }

        l.Add(new FlexiControl());
        l.AddRange(_itemSends.GetStyleOptions(this));

        var outdb = _itemSends.OutputDatabaseGet();
        if (outdb is Database) {
            var ic = new ItemCollectionList.ItemCollectionList(true);
            ic.AddRange(outdb.Column, true);
            l.Add(new FlexiControlForProperty<string>(() => _filterSpalte, ic));
        }

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

            case "inputcolumn":
                _eigangsWertSpalte = value;
                return true;

            case "outputcolumn":
                _filterSpalte = value;
                return true;

            case "filter":
                _filtertype = (FilterTypeRowInputItem)IntParse(value);
                return true;
        }
        return false;

        //result.ParseableAdd("InputColumn", _eigangsWertSpalte);
        //result.ParseableAdd("OutputColumn", _filterSpalte);
        //result.ParseableAdd("Filter", _filtertype);
    }

    public override string ReadableText() {
        var txt = "Filter aus Zeile: ";

        if (IsOk() && OutputDatabase != null) {
            return txt + OutputDatabase.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptSomething remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage? SymbolForReadableText() {
        if (IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        var result = new List<string>();
        result.AddRange(_itemAccepts.ParsableTags());
        result.AddRange(_itemSends.ParsableTags());

        result.ParseableAdd("InputColumn", _eigangsWertSpalte);
        result.ParseableAdd("OutputColumn", _filterSpalte);
        result.ParseableAdd("Filter", _filtertype);

        return result.Parseable(base.ToString());
    }

    internal override void AddedToCollection() {
        base.AddedToCollection();
        _itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Trichter", OutputColorId);
            DrawColorScheme(gr, positionModified, zoom, null, true, true);
            base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
            DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", InputColorId);
        }
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        _itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    #endregion
}