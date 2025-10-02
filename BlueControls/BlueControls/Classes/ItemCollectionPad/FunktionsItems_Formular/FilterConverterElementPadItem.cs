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
using BlueTable;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class FilterConverterElementPadItem : ReciverSenderControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private string _fehlerText = string.Empty;
    private string _filterSpalte = string.Empty;
    private FilterTypeRowInputItem _filtertype = FilterTypeRowInputItem.Ist_schreibungsneutral;
    private string _filterwert = string.Empty;

    #endregion

    #region Constructors

    public FilterConverterElementPadItem() : this(string.Empty, null, null) { }

    public FilterConverterElementPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public FilterConverterElementPadItem(string keyName, Table? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-FilterConverterElement";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;
    public bool AutoSizeableHeight => false;

    public override string Description => "Erstellt einen Filter.\r\nEs kann eine Zeile empfangen. Dann können die Variablen der eingehenden Zeile benutzt werden, um den Filter-Wert zu berechnen.\r\n\r\nDas Element kann auch zur Anzeige benutzt werden und zeigt an, was gerade gefiltert wird.";

    [Description("Text, der angezeigt wird, wenn kein Filter generiert werden kann")]
    [DefaultValue("")]
    public string Fehler_Text {
        get => _fehlerText;
        set {
            if (IsDisposed) { return; }
            if (value == _fehlerText) { return; }
            _fehlerText = value;
            OnPropertyChanged();
        }
    }

    //[Description("Der Wert aus dieser Spalte wird zur Filterung verwendet.")]
    //[DefaultValue("")]
    //public string Eingangs_Wert_Spalte {
    //    get => _eingangsWertSpalte;
    //    set {
    //        if (IsDisposed) { return; }
    //        if (value == _eingangsWertSpalte) { return; }
    //        _eingangsWertSpalte = value;
    //        this.DoChilds();
    //        OnPropertyChanged(string propertyname);
    //    }
    //}
    [Description("Dieser Filter-Typ wird angewendet.")]
    [DefaultValue(FilterTypeRowInputItem.Ist_schreibungsneutral)]
    public FilterTypeRowInputItem Filter {
        get => _filtertype;
        set {
            if (IsDisposed) { return; }
            if (value == _filtertype) { return; }
            _filtertype = value;
            OnPropertyChanged();
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
            OnPropertyChanged();
        }
    }

    [Description("Nach diesem Wert wird gefiltert. Es können Variablen der eingehenden Zeile benutzt werden.")]
    [DefaultValue("")]
    public string Filter_Wert {
        get => _filterwert;
        set {
            if (IsDisposed) { return; }
            if (value == _filterwert) { return; }
            _filterwert = value;
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => false;
    public override bool TableInputMustMatchOutputTable => false;
    //public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
    //    get => _standard_bei_keiner_Eingabe;
    //    set {
    //        if (IsDisposed) { return; }
    //        if (_standard_bei_keiner_Eingabe == value) { return; }
    //        _standard_bei_keiner_Eingabe = value;
    //        OnPropertyChanged(string propertyname);
    //    }
    //}

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var o = TableOutput?.Column[_filterSpalte];
        var con = new InputRowOutputFilterControl(_filterwert, o, _filtertype) {
            ErrorText = _fehlerText
        };
        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        var f = base.ErrorReason();
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (TableOutput?.Column[_filterSpalte] == null) {
            return "Die Spalte, in der gefiltert werden soll, fehlt.";
        }

        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true)
        ];

        //if (TableInput is Table dbin && !dbin.IsDisposed) {
        //    var u2 = new List<AbstractListItem>();
        //    u2.AddRange(ItemsOf(typeof(FlexiFilterDefaultOutput)));
        //    l.Add(new FlexiControlForProperty<FlexiFilterDefaultOutput>(() => Standard_bei_keiner_Eingabe, u2));
        //}

        //var inr = GetFilterFromGet();
        if (TableOutput is { IsDisposed: false } dbout) {
            var ic = new List<AbstractListItem>();
            ic.AddRange(ItemsOf(dbout.Column, true));
            result.Add(new FlexiControlForProperty<string>(() => Filter_Spalte, ic));

            var ic2 = new List<AbstractListItem>();
            ic2.AddRange(ItemsOf(typeof(FilterTypeRowInputItem)));
            result.Add(new FlexiControlForProperty<FilterTypeRowInputItem>(() => Filter, ic2));

            result.Add(new FlexiControlForProperty<string>(() => Filter_Wert, 5));

            result.Add(new FlexiControlForProperty<string>(() => Fehler_Text));
        }

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Value", _filterwert);
        //result.ParseableAdd("InputColumn", _eingangsWertSpalte);
        result.ParseableAdd("OutputColumn", _filterSpalte);
        result.ParseableAdd("Filter", _filtertype);
        result.ParseableAdd("errortext", _fehlerText);

        //if (TableInput is not Table dbin || dbin.IsDisposed) {
        //    _standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;
        //}

        //result.ParseableAdd("DefaultEmptyFilter", _standard_bei_keiner_Eingabe);

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
                //ColorId = IntParse(value);
                return true;

            case "errortext":
                _fehlerText = value.FromNonCritical();
                return true;

            case "value":
                _filterwert = value.FromNonCritical();
                return true;

            case "inputcolumn":
                _filterwert = "~" + value.FromNonCritical() + "~";
                return true;

            case "outputcolumn":
                _filterSpalte = value;
                return true;

            case "filter":
                _filtertype = (FilterTypeRowInputItem)IntParse(value);
                return true;

            case "defaultemptyfilter":
                //_standard_bei_keiner_Eingabe = (FlexiFilterDefaultOutput)IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Filter-Generator: ";

        return txt + TableOutput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        if (!ForPrinting) {
            DrawArrowOutput(gr, positionModified, scale, ForPrinting, OutputColorId);
            DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, false);
        }
        base.DrawExplicit(gr, visibleArea, positionModified, scale, shiftX, shiftY);
        DrawArrorInput(gr, positionModified, scale, ForPrinting, InputColorId);
    }

    #endregion
}