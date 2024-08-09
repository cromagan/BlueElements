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
using BlueDatabase;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class FilterConverterElementPadItem : ReciverSenderControlPadItem, IItemToControl, IReadableText, IAutosizable {

    #region Fields

    private string _fehler_text = string.Empty;
    private string _filterSpalte = string.Empty;
    private FilterTypeRowInputItem _filtertype = FilterTypeRowInputItem.Ist_schreibungsneutral;
    private string _filterwert = string.Empty;

    #endregion

    #region Constructors

    public FilterConverterElementPadItem(string keyName) : this(keyName, null, null) { }

    //private FlexiFilterDefaultOutput _standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;
    public FilterConverterElementPadItem(string keyName, Database? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        DatabaseOutput = db;
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-FilterConverterElement";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.More;
    public bool AutoSizeableHeight => false;

    public override bool DatabaseInputMustMatchOutputDatabase => false;

    public override string Description => "Erstellt einen Filter.\r\nEs kann eine Zeile empfangen. Dann können die Variablen der eingehenden Zeile benutzt werden, um den Filter-Wert zu berechnen.\r\n\r\nDas Element kann auch zur Anzeige benutzt werden und zeigt an, was gerade gefiltert wird.";

    [Description("Text, der angezeigt wird, wenn kein Filter generiert werden kann")]
    [DefaultValue("")]
    public string Fehler_Text {
        get => _fehler_text;
        set {
            if (IsDisposed) { return; }
            if (value == _fehler_text) { return; }
            _fehler_text = value;
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
    //        OnPropertyChanged();
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
            this.DoChilds();
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
            this.DoChilds();
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
            this.DoChilds();
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => false;
    public override string MyClassId => ClassId;

    //public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
    //    get => _standard_bei_keiner_Eingabe;
    //    set {
    //        if (IsDisposed) { return; }
    //        if (_standard_bei_keiner_Eingabe == value) { return; }
    //        _standard_bei_keiner_Eingabe = value;
    //        OnPropertyChanged();
    //    }
    //}

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var o = DatabaseOutput?.Column[_filterSpalte];
        var con = new InputRowOutputFilterControl(_filterwert, o, _filtertype);
        con.ErrorText = _fehler_text;
        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (DatabaseOutput?.Column[_filterSpalte] == null) {
            return "Die Spalte, in der gefiltert werden soll, fehlt.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var l = new List<GenericControl>();

        l.AddRange(base.GetProperties(widthOfControl));

        //if (DatabaseInput is Database dbin && !dbin.IsDisposed) {
        //    var u2 = new List<AbstractListItem>();
        //    u2.AddRange(ItemsOf(typeof(FlexiFilterDefaultOutput)));
        //    l.Add(new FlexiControlForProperty<FlexiFilterDefaultOutput>(() => Standard_bei_keiner_Eingabe, u2));
        //}

        //var inr = GetFilterFromGet();
        if (DatabaseOutput is Database dbout && !dbout.IsDisposed) {
            var ic = new List<AbstractListItem>();
            ic.AddRange(ItemsOf(dbout.Column, true));
            l.Add(new FlexiControlForProperty<string>(() => Filter_Spalte, ic));

            var ic2 = new List<AbstractListItem>();
            ic2.AddRange(ItemsOf(typeof(FilterTypeRowInputItem)));
            l.Add(new FlexiControlForProperty<FilterTypeRowInputItem>(() => Filter, ic2));

            l.Add(new FlexiControlForProperty<string>(() => Filter_Wert, 5));

            l.Add(new FlexiControlForProperty<string>(() => Fehler_Text));
        }

        l.AddRange(base.GetProperties(widthOfControl));

        return l;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
                //ColorId = IntParse(value);
                return true;

            case "errortext":
                _fehler_text = value.FromNonCritical();
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
        const string txt = "Filter: ";

        if (this.IsOk() && DatabaseOutput != null) {
            return txt + DatabaseOutput.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

            //return QuickImage.Get(ImageCode.Skript, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("Value", _filterwert);
        //result.ParseableAdd("InputColumn", _eingangsWertSpalte);
        result.ParseableAdd("OutputColumn", _filterSpalte);
        result.ParseableAdd("Filter", _filtertype);
        result.ParseableAdd("errortext", _fehler_text);

        //if (DatabaseInput is not Database dbin || dbin.IsDisposed) {
        //    _standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;
        //}

        //result.ParseableAdd("DefaultEmptyFilter", _standard_bei_keiner_Eingabe);

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