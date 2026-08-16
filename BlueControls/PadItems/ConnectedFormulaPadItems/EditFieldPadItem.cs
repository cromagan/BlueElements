// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.ControlStrategies;
using BlueControls.PadItems.FunktionsItems_Formular.Abstract;
using System.Windows.Forms;

namespace BlueControls.PadItems.FunktionsItems_Formular;

/// <summary>
/// Erzeut ein FlexiControllForCell
/// Standard-Bearbeitungs-Feld
/// </summary>
public class EditFieldPadItem : ReciverPadItem, IItemToControl, IAutosizable {

    #region Constructors

    public EditFieldPadItem() : this(string.Empty, null) { }

    public EditFieldPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-EditField";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;

    [DefaultValue(false)]
    [Description("Wenn aktiv, springt der Fokus automatisch zum nächsten Steuerelement, wenn am Ende des Textes die Rechts-Taste gedrückt wird.")]
    public bool AutoNext {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool AutoSizeableHeight {
        get {
            if (ControlStrategies.ControlStrategy.Cached(ControlStrategy).IsCaptionOnly) {
                return (int)CanvasUsedArea.Height > AutosizableExtension.MinHeigthCaption;
            }

            if (CaptionPosition is CaptionPosition.Links_neben_dem_Feld or CaptionPosition.ohne) {
                return (int)CanvasUsedArea.Height > AutosizableExtension.MinHeigthTextBox;
            }

            return (int)CanvasUsedArea.Height > AutosizableExtension.MinHeigthCapAndBox;
        }
    }

    [DefaultValue(true)]
    [Description("Richtet die Eingabefelder aller Steuerelemente auf gleicher horizontaler Ebene automatisch an der breitesten Beschriftung aus.")]
    public bool AutoX {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = true;

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
            var c = TableInput?.Column[ColumnKey];
            return c is not { IsDisposed: false } ? null : c;
        }
    }

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

    /// <summary>
    /// ClassId der ControlStrategie, mit der die Zelle im Formular bearbeitet wird.
    /// </summary>
    public string ControlStrategy {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = TextBoxControlStrategy.ClassId;

    public override string Description => "Standard Bearbeitungs-Steuerelement für Zellen.";
    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override bool TableInputMustMatchOutputTable => false;

    #endregion

    #region Methods

    /// <summary>
    /// Alle Strategien, die zur Spalten-Konfiguration (Text-Eingabe und/oder
    /// Auswahlliste) passen.
    /// </summary>
    public static List<ListItem> GetAllowedControlStrategys(bool textEditable, bool mayHaveDropdownItems) {
        var l = new List<ListItem>();

        foreach (var thisStrategy in ControlStrategies.ControlStrategy.AllStrategies.Instances) {
            if (thisStrategy.IsAllowed(textEditable, mayHaveDropdownItems)) {
                l.Add(new ReadableListItem(thisStrategy, true, string.Empty));
            }
        }
        return l;
    }

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        //var ff = parent.SearchOrGenerate(rfw2);

        var con = new FlexiControlForCell {
            ColumnKey = ColumnKey,
            ControlStrategy = ControlStrategy,
            CaptionPosition = CaptionPosition,
            AutoNext = AutoNext,
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (base.ErrorReason() is { Length: > 0 } f) { return f; }

        if (Column is not { IsDisposed: false }) { return "Spaltenangabe fehlt"; }

        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl)];

        if (TableInput is not { IsDisposed: false } tb) { return result; }

        result.Add(new FlexiControl("Einstellungen:", widthOfControl, true));

        var lst = new List<ListItem>();
        lst.AddRange(ItemsOf(tb.Column));

        result.Add(new FlexiControlForProperty<string>(() => ColumnKey, lst));

        if (Column is not { IsDisposed: false } col) { return result; }

        result.Add(new FlexiControlForProperty<CaptionPosition>(() => CaptionPosition, ItemsOf(typeof(CaptionPosition))));
        result.Add(new FlexiControlForProperty<bool>(() => AutoX));
        result.Add(new FlexiControlForProperty<bool>(() => AutoNext));
        result.Add(new FlexiControlForProperty<string>(() => ControlStrategy, GetAllowedControlStrategys(col.EditableWithTextInput, col.MayHaveDropDown())));

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("ColumnName", ColumnKey);
        result.ParseableAdd("EditType", ControlStrategy);
        result.ParseableAdd("Caption", CaptionPosition);
        result.ParseableAdd("AutoDistance", AutoX);
        result.ParseableAdd("AutoNext", AutoNext);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("columnkey", ColumnKey);
        json.Set("controlstrategy", ControlStrategy);
        json.Set("caption", (int)CaptionPosition);
        json.Set("autodistance", AutoX);
        json.Set("autonext", AutoNext);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            ColumnKey = json.GetString("columnkey", ColumnKey);
            ControlStrategy = json.GetString("controlstrategy", ControlStrategy);
            CaptionPosition = json.GetEnum("caption", CaptionPosition);
            AutoX = json.GetBool("autodistance", AutoX);
            AutoNext = json.GetBool("autonext", AutoNext);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "column":
            case "columnkey":
            case "columnname":
                ColumnKey = value;
                return true;

            case "fieldid":
                return true;

            case "edittype":
                ControlStrategy = ControlStrategies.ControlStrategy.ClassIdFromLegacyControlStrategy(value);
                return true;

            case "caption":
                CaptionPosition = (CaptionPosition)IntParse(value);
                return true;

            case "autodistance":
                AutoX = value.FromPlusMinus();
                return true;

            case "autonext":
                AutoNext = value.FromPlusMinus();
                return true;

            case "nosave":
                return true;

            case "style":
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Zelle: ";

        return txt + Column?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Stift, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, false);
        }

        //if (Column is null || Column .IsDisposed) {
        //    Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionControl.ToRect(), CaptionFnt.Scale(zoom), true);
        //} else {
        DrawFakeControl(gr, positionControl, zoom, CaptionPosition, Column?.ReadableText() + ":");
        //}

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);

        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    #endregion
}