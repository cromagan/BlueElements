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
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Dieses Element kann eine Zeile empfangen, und einen Filter für eine andere Datenbank basteln und diesen abgeben.
/// Unsichtbares Element, wird nicht angezeigt
/// </summary>

public class InputRowOutputFilterPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptRow, IItemSendFilter {

    #region Fields

    private readonly ItemAcceptRow _itemAccepts;
    private readonly ItemSendFilter _itemSends;
    private string _eingangsWertSpalte = string.Empty;
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

    public ReadOnlyCollection<string> ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public override string Description => "Dieses Element kann eine Zeile empfangen und einen Filter für eine andere Datenbank erstellen und diesen abgeben.\r\nUnsichtbares Element, wird nicht angezeigt";

    [Description("Der Wert aus dieser Spalte wird zur Filterung verwendet.")]
    [DefaultValue("")]
    public string Eingangs_Wert_Spalte {
        get => _eingangsWertSpalte;
        set {
            if (IsDisposed) { return; }
            if (value == _eingangsWertSpalte) { return; }
            _eingangsWertSpalte = value;
            this.RaiseVersion();
            this.DoChilds();
            OnChanged();
        }
    }

    [Description("Dieser Filter-Typ wird angewendet.")]
    [DefaultValue(FilterTypeRowInputItem.Ist_GrossKleinEgal)]
    public FilterTypeRowInputItem Filter {
        get => _filtertype;
        set {
            if (IsDisposed) { return; }
            if (value == _filtertype) { return; }
            _filtertype = value;
            this.RaiseVersion();
            this.DoChilds();
            OnChanged();
        }
    }

    [Description("Auf diese Spalte wird der Filter angewendet.")]
    [DefaultValue("")]
    public string Filter_Spalte {
        get => _filterSpalte;
        set {
            if (IsDisposed) { return; }
            if (value == _filterSpalte) { return; }
            _filterSpalte = value;
            this.RaiseVersion();
            this.DoChilds();
            OnChanged();
        }
    }

    public IItemSendRow? GetRowFrom {
        get => _itemAccepts.GetRowFromGet(this);
        set => _itemAccepts.GetRowFromSet(value, this);
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public DatabaseAbstract? InputDatabase => _itemAccepts.InputDatabase(this);
    public DatabaseAbstract? InputDatabaseMustBe => OutputDatabase;
    public override bool MustBeInDrawingArea => false;

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    public DatabaseAbstract? OutputDatabase {
        get => _itemSends.OutputDatabaseGet();
        set => _itemSends.OutputDatabaseSet(value, this);
    }

    public bool WaitForDatabase => true;
    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var i = _itemAccepts.InputDatabase(this)?.Column.Exists(_eingangsWertSpalte);
        var o = OutputDatabase?.Column.Exists(_filterSpalte);
        var con = new InputRowOutputFilterControl(i, o, _filtertype);
        con.DoOutputSettings(parent, this);
        con.DoInputSettings(parent, this);

        return con;
    }

    public override string ErrorReason() {
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemAccepts.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemSends.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        if (InputDatabase?.Column.Exists(_eingangsWertSpalte) == null) {
            return "Die Spalte, aus der der Filterwert kommen soll, fehlt.";
        }

        if (OutputDatabase?.Column.Exists(_filterSpalte) == null) {
            return "Die Spalte, in der gefiltert werden soll, fehlt.";
        }

        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        var l = new List<GenericControl>();

        l.AddRange(_itemAccepts.GetStyleOptions(this, widthOfControl));

        var inr = _itemAccepts.GetRowFromGet(this);
        if (inr?.OutputDatabase is DatabaseAbstract dbin) {
            var ic = new ItemCollectionList.ItemCollectionList(true);
            ic.AddRange(dbin.Column, true);
            l.Add(new FlexiControlForProperty<string>(() => Eingangs_Wert_Spalte, ic));

            var ic2 = new ItemCollectionList.ItemCollectionList(true);
            ic2.AddRange(typeof(FilterTypeRowInputItem));
            l.Add(new FlexiControlForProperty<FilterTypeRowInputItem>(() => Filter, ic2));
        }

        l.Add(new FlexiControl());
        l.AddRange(_itemSends.GetStyleOptions(this, widthOfControl));

        if (_itemSends.OutputDatabaseGet() is DatabaseAbstract outdb) {
            var ic = new ItemCollectionList.ItemCollectionList(true);
            ic.AddRange(outdb.Column, true);
            l.Add(new FlexiControlForProperty<string>(() => Filter_Spalte, ic));
        }

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions(widthOfControl));

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
                _eingangsWertSpalte = value;
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

        if (this.IsOk() && OutputDatabase != null) {
            return txt + OutputDatabase.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptSomething remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        var result = new List<string>();
        result.AddRange(_itemAccepts.ParsableTags());
        result.AddRange(_itemSends.ParsableTags());

        result.ParseableAdd("InputColumn", _eingangsWertSpalte);
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
            DrawColorScheme(gr, positionModified, zoom, null, true, true, false);
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