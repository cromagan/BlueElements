// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueScript.Variables;
using BlueTable;
using BlueTable.Enums;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Dieses Element kann einen Vorfilter empfangen und stellt dem Benutzer die Wahl, einen neuen Filter auszuwählen und gibt diesen weiter.
/// </summary>

public class OutputFilterPadItem : ReciverSenderControlPadItem, IItemToControl, IAutosizable, IHasFieldVariable {

    #region Fields

    private CaptionPosition _captionPosition = CaptionPosition.Über_dem_Feld;
    private string _columnName = string.Empty;
    private bool _einschnappen = true;
    private FlexiFilterDefaultFilter _filterart_Bei_Texteingabe = FlexiFilterDefaultFilter.Textteil;
    private FlexiFilterDefaultOutput _standard_Bei_Keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;

    #endregion

    #region Constructors

    public OutputFilterPadItem() : this(string.Empty, null, null) { }

    public OutputFilterPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public OutputFilterPadItem(string keyName, Table? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-InputOutputElement";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;
    public bool AutoSizeableHeight => false;

    public CaptionPosition CaptionPosition {
        get => _captionPosition;
        set {
            if (IsDisposed) { return; }
            if (_captionPosition == value) { return; }
            _captionPosition = value;
            OnPropertyChanged();
        }
    }

    public ColumnItem? Column {
        get {
            var c = TableOutput?.Column[_columnName];
            return c is not { IsDisposed: false } ? null : c;
        }
    }

    public string ColumnName {
        get => _columnName;
        set {
            if (IsDisposed) { return; }
            if (_columnName == value) { return; }
            _columnName = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public override string Description => "Mit diesem Element wird dem Benutzer eine Filter-Möglichkeit angeboten.<br>Durch die empfangenen Filter können die auswählbaren Werte eingeschränkt werden.\r\nWerte können mit 'Skript-Knöpfen' abgefragt und manipuluert werden.";

    public bool Einschnappen {
        get => _einschnappen;
        set {
            if (IsDisposed) { return; }
            if (_einschnappen == value) { return; }
            _einschnappen = value;
            OnPropertyChanged();
        }
    }

    public string FieldName {
        get {
            if (Column is not { } c || c.Table is not { } tb) { return string.Empty; }
            return $"FIELD_{tb.KeyName}_{c.KeyName}";
        }
    }

    public FlexiFilterDefaultFilter Filterart_bei_Texteingabe {
        get => _filterart_Bei_Texteingabe;
        set {
            if (IsDisposed) { return; }
            if (_filterart_Bei_Texteingabe == value) { return; }
            _filterart_Bei_Texteingabe = value;
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => true;

    public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
        get => _standard_Bei_Keiner_Eingabe;
        set {
            if (IsDisposed) { return; }
            if (_standard_Bei_Keiner_Eingabe == value) { return; }
            _standard_Bei_Keiner_Eingabe = value;
            OnPropertyChanged();
        }
    }

    public override bool TableInputMustMatchOutputTable => true;
    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FlexiFilterControl(Column, _captionPosition, _standard_Bei_Keiner_Eingabe, _filterart_Bei_Texteingabe, _einschnappen, true) {
            SavesSettings = true
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (Column is not { IsDisposed: false }) {
            return "Spalte fehlt";
        }

        return base.ErrorReason();
    }

    public Variable? GetFieldVariable() {
        var fn = FieldName;
        if (!string.IsNullOrEmpty(fn) && Column is { } c) {
            return RowItem.CellToVariable(fn, c.ScriptType, c.MostUsedValue, false, "Feld im Formular");
        }
        return null;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true)
        ];

        if (TableOutput is { IsDisposed: false } db) {
            var lst = new List<AbstractListItem>();
            lst.AddRange(ItemsOf(db.Column, true));

            result.Add(new FlexiControlForProperty<string>(() => ColumnName, lst));
        }

        var u = new List<AbstractListItem>();
        u.AddRange(ItemsOf(typeof(CaptionPosition)));
        result.Add(new FlexiControlForProperty<CaptionPosition>(() => CaptionPosition, u));

        var u2 = new List<AbstractListItem>();
        u2.AddRange(ItemsOf(typeof(FlexiFilterDefaultOutput)));
        result.Add(new FlexiControlForProperty<FlexiFilterDefaultOutput>(() => Standard_bei_keiner_Eingabe, u2));

        var u3 = new List<AbstractListItem>();
        u3.AddRange(ItemsOf(typeof(FlexiFilterDefaultFilter)));
        result.Add(new FlexiControlForProperty<FlexiFilterDefaultFilter>(() => Filterart_bei_Texteingabe, u3));

        result.Add(new FlexiControlForProperty<bool>(() => Einschnappen));

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("ColumnName", _columnName);
        //result.ParseableAdd("CaptionText", _überschrift);
        //result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("Caption", _captionPosition);
        result.ParseableAdd("DefaultEmptyFilter", _standard_Bei_Keiner_Eingabe);
        result.ParseableAdd("DefaultTextFilter", _filterart_Bei_Texteingabe);
        result.ParseableAdd("SnapFilter", _einschnappen);

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
            case "style":
                return true;

            case "caption":
                _captionPosition = (CaptionPosition)IntParse(value);
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

            case "snapfilter":
                _einschnappen = value.FromPlusMinus();
                return true;

                //case "captiontext":
                //    _überschrift = value.FromNonCritical();
                //    return true;

                //case "showformat":
                //    _anzeige = value.FromNonCritical();
                //    return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Filter-Auswahl: ";

        return txt + TableOutput?.Caption;
    }

    public void SetValueFromVariable(Variable v) { }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Trichter, 16, Skin.IdColor(OutputColorId), Color.Transparent); //  QuickImage.Get(ImageCode.Trichter, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionInControl, float scale, float offsetX, float offsetY) {
        if (!ForPrinting) {
            DrawArrowOutput(gr, positionInControl, scale, ForPrinting, OutputColorId);
            DrawColorScheme(gr, positionInControl, scale, InputColorId, true, true, false);
        }

        base.DrawExplicit(gr, visibleArea, positionInControl, scale, offsetX, offsetY);
        DrawArrorInput(gr, positionInControl, scale, ForPrinting, InputColorId);
    }

    #endregion
}