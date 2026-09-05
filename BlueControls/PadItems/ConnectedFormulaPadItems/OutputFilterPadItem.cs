// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.PadItems.FunktionsItems_Formular.Abstract;
using BlueScript.ScriptVariables;
using System.Windows.Forms;

namespace BlueControls.PadItems.FunktionsItems_Formular;

/// <summary>
/// Dieses Element kann einen Vorfilter empfangen und stellt dem Benutzer die Wahl, einen neuen Filter auszuwählen und gibt diesen weiter.
/// </summary>

public class OutputFilterPadItem : ReciverSenderPadItem, IItemToControl, IAutosizable, IHasFieldVariable {

    #region Constructors

    public OutputFilterPadItem() : this(string.Empty, null, null) { }

    public OutputFilterPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public OutputFilterPadItem(string keyName, Table? db, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-InputOutputElement";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;
    public bool AutoSizeableHeight => false;

    /// <summary>
    /// Position der Beschriftung: neben oder über dem Feld.
    /// </summary>
    public CaptionPosition CaptionPosition {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = CaptionPosition.Über_dem_Feld;

    public ColumnItem? Column {
        get {
            var c = TableOutput?.Column[ColumnKey];
            return c is not { IsDisposed: false } ? null : c;
        }
    }

    /// <summary>
    /// Die Spalte, die dieser Filter einschränkt.
    /// </summary>
    public string ColumnKey {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    } = string.Empty;

    public override string Description => "Mit diesem Element wird dem Benutzer eine Filter-Möglichkeit angeboten.<br>Durch die empfangenen Filter können die auswählbaren Werte eingeschränkt werden.\r\nWerte können mit 'Skript-Knöpfen' abgefragt und manipuluert werden.";

    /// <summary>
    /// Wenn gewählt, wird das Textfeld zu einem Knopf, sobald ein konkreter Wert gewählt ist.
    /// </summary>
    public bool Einschnappen {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public string FieldName {
        get {
            if (Column is not { IsDisposed: false } c || c.Table is not { IsDisposed: false } tb) { return string.Empty; }
            return $"FIELD_{tb.KeyName}_{c.KeyName}";
        }
    }

    /// <summary>
    /// Wie ein eingegebener Text mit den Werten verglichen wird.
    /// </summary>
    public FlexiFilterDefaultFilter Filterart_bei_Texteingabe {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = FlexiFilterDefaultFilter.Textteil;

    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => true;

    /// <summary>
    /// Legt fest, was angezeigt wird, solange keine Eingabe gemacht wurde.
    /// </summary>
    public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = FlexiFilterDefaultOutput.Alles_Anzeigen;

    public override bool TableInputMustMatchOutputTable => true;
    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FlexiControlForFilter(Column, CaptionPosition, Standard_bei_keiner_Eingabe, Filterart_bei_Texteingabe, Einschnappen, true) {
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

    public ScriptVariable? GetFieldVariable() {
        var fn = FieldName;
        if (!string.IsNullOrEmpty(fn) && Column is { IsDisposed: false } c) {
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

        if (TableOutput is { IsDisposed: false } tb) {
            result.Add(new FlexiControlForProperty<string>(() => ColumnKey, ItemsOf(tb.Column)));
        }

        result.Add(new FlexiControlForProperty<CaptionPosition>(() => CaptionPosition, ItemsOf(typeof(CaptionPosition))));
        result.Add(new FlexiControlForProperty<FlexiFilterDefaultOutput>(() => Standard_bei_keiner_Eingabe, ItemsOf(typeof(FlexiFilterDefaultOutput))));
        result.Add(new FlexiControlForProperty<FlexiFilterDefaultFilter>(() => Filterart_bei_Texteingabe, ItemsOf(typeof(FlexiFilterDefaultFilter))));
        result.Add(new FlexiControlForProperty<bool>(() => Einschnappen));

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("ColumnName", ColumnKey);
        //result.ParseableAdd("CaptionText", _überschrift);
        //result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("Caption", CaptionPosition);
        result.ParseableAdd("DefaultEmptyFilter", Standard_bei_keiner_Eingabe);
        result.ParseableAdd("DefaultTextFilter", Filterart_bei_Texteingabe);
        result.ParseableAdd("SnapFilter", Einschnappen);

        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("columnkey", ColumnKey);
        json.Set("caption", (int)CaptionPosition);
        json.Set("defaultemptyfilter", (int)Standard_bei_keiner_Eingabe);
        json.Set("defaulttextfilter", (int)Filterart_bei_Texteingabe);
        json.Set("snapfilter", Einschnappen);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            ColumnKey = json.GetString("columnkey", ColumnKey);
            CaptionPosition = json.GetEnum("caption", CaptionPosition);
            Standard_bei_keiner_Eingabe = json.GetEnum("defaultemptyfilter", Standard_bei_keiner_Eingabe);
            Filterart_bei_Texteingabe = json.GetEnum("defaulttextfilter", Filterart_bei_Texteingabe);
            Einschnappen = json.GetBool("snapfilter", Einschnappen);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
            case "style":
                return true;

            case "caption":
                CaptionPosition = (CaptionPosition)IntParse(value);
                return true;

            case "column":
            case "columnkey":
            case "columnname":
                ColumnKey = value;
                return true;

            case "defaultemptyfilter":
                Standard_bei_keiner_Eingabe = (FlexiFilterDefaultOutput)IntParse(value);
                return true;

            case "defaulttextfilter":
                Filterart_bei_Texteingabe = (FlexiFilterDefaultFilter)IntParse(value);
                return true;

            case "snapfilter":
                Einschnappen = value.FromPlusMinus();
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

    public void SetValueFromVariable(ScriptVariable v) { }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Trichter, 16, Skin.IdColor(OutputColorId), Color.Transparent); //  QuickImage.Get(ImageCode.Trichter, 16);

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