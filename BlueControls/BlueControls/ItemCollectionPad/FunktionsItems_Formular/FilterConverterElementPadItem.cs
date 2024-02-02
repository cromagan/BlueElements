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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using static BlueBasics.Converter;

#nullable enable

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class FilterConverterElementPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptSomething, IItemSendSomething {

    #region Fields

    private readonly ItemAcceptSomething _itemAccepts;
    private readonly ItemSendSomething _itemSends;
    private string _eingangsWertSpalte = string.Empty;
    private string _filterSpalte = string.Empty;
    private FilterTypeRowInputItem _filtertype = FilterTypeRowInputItem.Ist_GrossKleinEgal;

    private FlexiFilterDefaultOutput _standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;

    #endregion

    #region Constructors

    public FilterConverterElementPadItem(string keyname, string toParse) : this(keyname, null as Database) => this.Parse(toParse);

    public FilterConverterElementPadItem(Database? db) : this(string.Empty, db) { }

    public FilterConverterElementPadItem(string intern, Database? db) : base(intern) {
        _itemAccepts = new();
        _itemSends = new();

        DatabaseOutput = db;
    }

    public FilterConverterElementPadItem(string intern) : this(intern, null as Database) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-FilterConverterElement";

    public ReadOnlyCollection<string> ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public Database? DatabaseInput => _itemAccepts.DatabaseInput(this);
    public bool DatabaseInputMustMatchOutputDatabase => false;

    public Database? DatabaseOutput {
        get => _itemSends.DatabaseOutputGet();
        set => _itemSends.DatabaseOutputSet(value, this);
    }

    public override string Description => "Kann aus dem empfangenen Filtern einen komplett anderen Filter erstellen und ausgeben.";

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

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public override bool MustBeInDrawingArea => false;

    public bool MustBeOneRow => false;

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    public ReadOnlyCollection<string> Parents {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
        get => _standard_bei_keiner_Eingabe;
        set {
            if (IsDisposed) { return; }
            if (_standard_bei_keiner_Eingabe == value) { return; }
            _standard_bei_keiner_Eingabe = value;
            OnChanged();
        }
    }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent) {
        var i = _itemAccepts.DatabaseInput(this)?.Column.Exists(_eingangsWertSpalte);
        var o = DatabaseOutput?.Column.Exists(_filterSpalte);
        var con = new InputRowOutputFilterControl(i, o, _filtertype) {
            Standard_bei_keiner_Eingabe = _standard_bei_keiner_Eingabe
        };
        con.DoOutputSettings(this);
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

        if (DatabaseInput?.Column.Exists(_eingangsWertSpalte) == null) {
            return "Die Spalte, aus der der Filterwert kommen soll, fehlt.";
        }

        if (DatabaseOutput?.Column.Exists(_filterSpalte) == null) {
            return "Die Spalte, in der gefiltert werden soll, fehlt.";
        }

        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        var l = new List<GenericControl>();

        l.AddRange(_itemAccepts.GetStyleOptions(this, widthOfControl));

        var inr = _itemAccepts.GetFilterFromGet(this);
        if (inr.Count > 0 && inr[0].DatabaseOutput is Database dbin && !dbin.IsDisposed) {
            var ic = new ItemCollectionList.ItemCollectionList(true);
            ic.AddRange(dbin.Column, true);
            l.Add(new FlexiControlForProperty<string>(() => Eingangs_Wert_Spalte, ic));

            var ic2 = new ItemCollectionList.ItemCollectionList(true);
            ic2.AddRange(typeof(FilterTypeRowInputItem));
            l.Add(new FlexiControlForProperty<FilterTypeRowInputItem>(() => Filter, ic2));
        }

        var u2 = new ItemCollectionList.ItemCollectionList(false);
        u2.AddRange(typeof(FlexiFilterDefaultOutput));
        l.Add(new FlexiControlForProperty<FlexiFilterDefaultOutput>(() => Standard_bei_keiner_Eingabe, u2));

        l.Add(new FlexiControl());
        l.AddRange(_itemSends.GetStyleOptions(this, widthOfControl));

        if (_itemSends.DatabaseOutputGet() is Database dbout && !dbout.IsDisposed) {
            var ic = new ItemCollectionList.ItemCollectionList(true);
            ic.AddRange(dbout.Column, true);
            l.Add(new FlexiControlForProperty<string>(() => Filter_Spalte, ic));
        }

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions(widthOfControl));

        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        _itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
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

            case "defaultemptyfilter":
                _standard_bei_keiner_Eingabe = (FlexiFilterDefaultOutput)IntParse(value);
                return true;
        }
        return false;

        //result.ParseableAdd("InputColumn", _eigangsWertSpalte);
        //result.ParseableAdd("OutputColumn", _filterSpalte);
        //result.ParseableAdd("Filter", _filtertype);
    }

    public override string ReadableText() {
        const string txt = "Filter aus Zeile: ";

        if (this.IsOk() && DatabaseOutput != null) {
            return txt + DatabaseOutput.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptSomething remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Skript, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags(), .. _itemSends.ParsableTags()];

        result.ParseableAdd("InputColumn", _eingangsWertSpalte);
        result.ParseableAdd("OutputColumn", _filterSpalte);
        result.ParseableAdd("Filter", _filtertype);
        result.ParseableAdd("DefaultEmptyFilter", _standard_bei_keiner_Eingabe);

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
            DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, OutputColorId);
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }
        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, InputColorId);
    }

    #endregion
}