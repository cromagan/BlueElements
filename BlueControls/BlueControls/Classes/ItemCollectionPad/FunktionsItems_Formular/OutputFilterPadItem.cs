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
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using static BlueBasics.Converter;

#nullable enable

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Dieses Element kann einen Vorfilter empfangen und stellt dem Benutzer die Wahl, einen neuen Filter auszuwählen und gibt diesen weiter.
/// </summary>

public class OutputFilterPadItem : FakeControlPadItem, IItemToControl, IReadableText, IItemAcceptFilter, IItemSendFilter, IAutosizable {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;
    private readonly ItemSendFilter _itemSends;
    private string _columnName = string.Empty;
    private FlexiFilterDefaultFilter _filterart_Bei_Texteingabe = FlexiFilterDefaultFilter.Textteil;
    private FlexiFilterDefaultOutput _standard_Bei_Keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;

    //private string _anzeige = string.Empty;
    //private string _überschrift = string.Empty;
    private CaptionPosition _überschriftanordung = CaptionPosition.Über_dem_Feld;

    #endregion

    #region Constructors

    public OutputFilterPadItem(string keyName) : this(keyName, null, null) { }

    public OutputFilterPadItem(string keyName, Database? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        _itemAccepts = new();
        _itemSends = new();

        DatabaseOutput = db;
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-InputOutputElement";
    public AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;
    public bool AutoSizeableHeight => false;

    public CaptionPosition CaptionPosition {
        get => _überschriftanordung;
        set {
            if (IsDisposed) { return; }
            if (_überschriftanordung == value) { return; }
            _überschriftanordung = value;
            OnPropertyChanged();
        }
    }

    public ReadOnlyCollection<string> ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public ColumnItem? Column {
        get {
            var c = DatabaseOutput?.Column[_columnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    public string ColumnName {
        get => _columnName;
        set {
            if (IsDisposed) { return; }
            if (_columnName == value) { return; }
            _columnName = value;
            OnPropertyChanged();
            UpdateSideOptionMenu();
        }
    }

    public Database? DatabaseInput => _itemAccepts.DatabaseInputGet(this);
    public bool DatabaseInputMustMatchOutputDatabase => true;

    public Database? DatabaseOutput {
        get => _itemSends.DatabaseOutputGet(this);
        set => _itemSends.DatabaseOutputSet(value, this);
    }

    public override string Description => "Mit diesem Element wird dem Benutzer eine Filter-Möglichkeit angeboten.<br>Durch die empfangenen Filter können die auswählbaren Werte eingeschränkt werden.";

    public FlexiFilterDefaultFilter Filterart_bei_Texteingabe {
        get => _filterart_Bei_Texteingabe;
        set {
            if (IsDisposed) { return; }
            if (_filterart_Bei_Texteingabe == value) { return; }
            _filterart_Bei_Texteingabe = value;
            OnPropertyChanged();
        }
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => true;
    public override string MyClassId => ClassId;

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    public ReadOnlyCollection<string> Parents {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
        get => _standard_Bei_Keiner_Eingabe;
        set {
            if (IsDisposed) { return; }
            if (_standard_Bei_Keiner_Eingabe == value) { return; }
            _standard_Bei_Keiner_Eingabe = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public override void AddedToCollection() {
        base.AddedToCollection();
        _itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FlexiControlForFilter(Column, _überschriftanordung) {
            Standard_bei_keiner_Eingabe = _standard_Bei_Keiner_Eingabe,
            Filterart_bei_Texteingabe = _filterart_Bei_Texteingabe,
            SavesSettings = true
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemAccepts.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemSends.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        if (Column == null || Column.IsDisposed) {
            return "Spalte fehlt";
        }

        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var l = new List<GenericControl>();

        l.AddRange(_itemAccepts.GetProperties(this, widthOfControl));

        if (DatabaseOutput is Database db && !db.IsDisposed) {
            var lst = new List<AbstractListItem>();
            lst.AddRange(ItemsOf(db.Column, true));

            l.Add(new FlexiControlForProperty<string>(() => ColumnName, lst));
        }

        l.AddRange(_itemSends.GetProperties(this, widthOfControl));

        var u = new List<AbstractListItem>();
        u.AddRange(ItemsOf(typeof(CaptionPosition)));
        l.Add(new FlexiControlForProperty<CaptionPosition>(() => CaptionPosition, u));

        var u2 = new List<AbstractListItem>();
        u2.AddRange(ItemsOf(typeof(FlexiFilterDefaultOutput)));
        l.Add(new FlexiControlForProperty<FlexiFilterDefaultOutput>(() => Standard_bei_keiner_Eingabe, u2));

        var u3 = new List<AbstractListItem>();
        u3.AddRange(ItemsOf(typeof(FlexiFilterDefaultFilter)));
        l.Add(new FlexiControlForProperty<FlexiFilterDefaultFilter>(() => Filterart_bei_Texteingabe, u3));

        l.Add(new FlexiControl());
        l.AddRange(base.GetProperties(widthOfControl));

        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        _itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    public override bool ParseThis(string key, string value) {
        if (base.ParseThis(key, value)) { return true; }
        if (_itemSends.ParseThis(key, value)) { return true; }
        if (_itemAccepts.ParseThis(key, value)) { return true; }

        switch (key) {
            case "id":
                //Id = IntParse(value);
                return true;

            case "caption":
                _überschriftanordung = (CaptionPosition)IntParse(value);
                return true;

            case "columnname":
                _columnName = value;
                return true;

            case "defaultemptyfilter":
                _standard_Bei_Keiner_Eingabe = (FlexiFilterDefaultOutput)IntParse(value);
                return true;

            case "defaulttextfilter":
                _filterart_Bei_Texteingabe = (FlexiFilterDefaultFilter)IntParse(value);
                return true;

                //case "captiontext":
                //    _überschrift = value.FromNonCritical();
                //    return true;

                //case "showformat":
                //    _anzeige = value.FromNonCritical();
                //    return true;
        }
        return false;
    }

    public override string ReadableText() {
        const string txt = "Filter: ";

        if (this.IsOk() && DatabaseOutput != null) {
            return txt + DatabaseOutput.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptFilter remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));
            //return QuickImage.Get(ImageCode.Trichter, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags(), .. _itemSends.ParsableTags(this)];

        result.ParseableAdd("ColumnName", _columnName);
        //result.ParseableAdd("CaptionText", _überschrift);
        //result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("Caption", _überschriftanordung);
        result.ParseableAdd("DefaultEmptyFilter", _standard_Bei_Keiner_Eingabe);
        result.ParseableAdd("DefaultTextFilter", _filterart_Bei_Texteingabe);

        return result.Parseable(base.ToParseableString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, OutputColorId);
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    #endregion
}