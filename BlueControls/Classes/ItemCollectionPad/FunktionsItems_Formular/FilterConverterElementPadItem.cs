// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using BlueControls.Controls.ConnectedFormula;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

public class FilterConverterElementPadItem : ReciverSenderControlPadItem, IItemToControl, IAutosizable {

    #region Constructors

    public FilterConverterElementPadItem() : this(string.Empty, null, null) { }

    public FilterConverterElementPadItem(string keyName, ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public FilterConverterElementPadItem(string keyName, Table? db, ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-FilterConverterElement";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;
    public bool AutoSizeableHeight => false;

    public override string Description => "Erstellt einen Filter.\r\nEs kann eine Zeile empfangen. Dann können die Variablen der eingehenden Zeile benutzt werden, um den Filter-Wert zu berechnen.\r\n\r\nDas Element kann auch zur Anzeige benutzt werden und zeigt an, was gerade gefiltert wird.";

    [Description("Text, der angezeigt wird, wenn kein Filter generiert werden kann")]
    [DefaultValue("")]
    public string Fehler_Text {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

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
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = FilterTypeRowInputItem.Ist_schreibungsneutral;

    [Description("Auf diese Spalte wird der Filter angewendet.")]
    [DefaultValue("")]
    public string Filter_Spalte {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    [Description("Nach diesem Wert wird gefiltert. Es können Variablen der eingehenden Zeile benutzt werden.")]
    [DefaultValue("")]
    public string Filter_Wert {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

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
        var o = TableOutput?.Column[Filter_Spalte];
        var con = new InputRowOutputFilterControl(Filter_Wert, o, Filter) {
            ErrorText = Fehler_Text
        };
        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (base.ErrorReason() is { Length: > 0 } f) { return f; }

        if (TableOutput?.Column[Filter_Spalte] is null) {
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

        if (TableOutput is { IsDisposed: false } tb) {
            result.Add(new FlexiControlForProperty<string>(() => Filter_Spalte, ItemsOf(tb.Column)));
            result.Add(new FlexiControlForProperty<FilterTypeRowInputItem>(() => Filter, ItemsOf(typeof(FilterTypeRowInputItem))));

            var filterWertFlex = new FlexiControlForProperty<string>(() => Filter_Wert);
            filterWertFlex.EditType = EditTypeFormula.Textfeld_mit_Suggestions;
            filterWertFlex.SuggestionPosition = SuggestionPosition.ContextMenuOnly;
            filterWertFlex.Height = 24;

            var inr = GetFilterFromGet();
            if (inr.Count > 0 && inr[0].TableOutput is { IsDisposed: false } inTable) {
                filterWertFlex.ListItems = [.. inTable.Column.Where(c => !c.IsDisposed).Select(c => ItemOf($"~{c.KeyName}~"))];
            }

            result.Add(filterWertFlex);
            result.Add(new FlexiControlForProperty<string>(() => Fehler_Text));
        }

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Value", Filter_Wert);
        //result.ParseableAdd("InputColumn", _eingangsWertSpalte);
        result.ParseableAdd("OutputColumn", Filter_Spalte);
        result.ParseableAdd("Filter", Filter);
        result.ParseableAdd("errortext", Fehler_Text);

        //if (TableInput is not Table dbin || dbin.IsDisposed) {
        //    _standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;
        //}

        //result.ParseableAdd("DefaultEmptyFilter", _standard_bei_keiner_Eingabe);

        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("value", Filter_Wert);
        json.Set("outputcolumn", Filter_Spalte);
        json.Set("filter", (int)Filter);
        json.Set("errortext", Fehler_Text);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        Filter_Wert = json.GetString("value", Filter_Wert);
        Filter_Spalte = json.GetString("outputcolumn", Filter_Spalte);
        Filter = json.GetEnum("filter", Filter);
        Fehler_Text = json.GetString("errortext", Fehler_Text);
        base.ParseJson(json);
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
                //ColorId = IntParse(value);
                return true;

            case "errortext":
                Fehler_Text = value.FromNonCritical();
                return true;

            case "value":
                Filter_Wert = value.FromNonCritical();
                return true;

            case "inputcolumn":
                Filter_Wert = "~" + value.FromNonCritical() + "~";
                return true;

            case "outputcolumn":
                Filter_Spalte = value;
                return true;

            case "filter":
                Filter = (FilterTypeRowInputItem)IntParse(value);
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

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (!forPrinting) {
            DrawArrowOutput(gr, positionControl, zoom, forPrinting, OutputColorId);
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, false);
        }
        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);
        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    #endregion
}