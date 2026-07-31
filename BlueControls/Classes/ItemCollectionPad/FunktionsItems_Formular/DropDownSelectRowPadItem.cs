// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

public class DropDownSelectRowPadItem : ReciverSenderControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld_mit_Auswahlknopf;

    #endregion

    #region Constructors

    public DropDownSelectRowPadItem() : this(string.Empty, null, null) { }

    public DropDownSelectRowPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public DropDownSelectRowPadItem(string keyName, Table? db, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-SelectRowWithDropDownMenu";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.More;

    [Description("Nach welchem Format die Zeilen angezeigt werden sollen. Es können Variablen im Format ~Variable~ benutzt werden. Achtung, KEINE Skript-Variaben, nur Spaltennamen.")]
    public string Anzeige {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public bool AutoSizeableHeight => false;

    public string Caption {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public CaptionPosition CaptionPosition {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = CaptionPosition.Über_dem_Feld;

    public override string Description => "Ein Auswahlmenü, aus dem der Benutzer eine Zeile wählen kann, die durch die Vor-Filter bestimmt wurden.";
    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => true;
    public override bool TableInputMustMatchOutputTable => true;
    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FlexiControlForRowSelector(TableOutput, Caption, Anzeige) {
            EditType = _bearbeitung,
            CaptionPosition = CaptionPosition
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", -1, true),
            new FlexiControlForProperty<string>(() => Caption),
            new FlexiControlForProperty<string>(() => Anzeige),
        ];

        result.Add(new FlexiControlForProperty<CaptionPosition>(() => CaptionPosition, ItemsOf(typeof(CaptionPosition))));

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("CaptionText", Caption);
        result.ParseableAdd("ShowFormat", Anzeige);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", CaptionPosition);
        //result.ParseableAdd("ID", ColorId);

        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("captiontext", Caption);
        json.Set("showformat", Anzeige);
        json.Set("edittype", (int)_bearbeitung);
        json.Set("caption", (int)CaptionPosition);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        Caption = json.GetString("captiontext", Caption);
        Anzeige = json.GetString("showformat", Anzeige);
        _bearbeitung = json.GetEnum("edittype", _bearbeitung);
        CaptionPosition = json.GetEnum("caption", CaptionPosition);
        base.ParseJson(json);
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
            case "style":
                return true;

            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                CaptionPosition = (CaptionPosition)IntParse(value);
                return true;

            case "captiontext":
                Caption = value.FromNonCritical();
                return true;

            case "showformat":
                Anzeige = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Zeilenauswahl: ";

        return txt + TableOutput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (!forPrinting) {
            DrawArrowOutput(gr, positionControl, zoom, forPrinting, OutputColorId);
            DrawFakeControl(gr, positionControl, zoom, CaptionPosition, Caption, EditTypeFormula.Textfeld_mit_Auswahlknopf);
            DrawColorScheme(gr, positionControl, zoom, null, true, true, true);
        } else {
            DrawFakeControl(gr, positionControl, zoom, CaptionPosition, Caption, EditTypeFormula.Textfeld_mit_Auswahlknopf);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);
        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    #endregion
}