// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics.Formats;
using BlueControls.Controls;
using BlueControls.PadItems.FunktionsItems_Formular.Abstract;
using BlueScript.ScriptVariables;
using System.Windows.Forms;

namespace BlueControls.PadItems.FunktionsItems_Formular;

/// <summary>
/// Ein Textfeld, das ohne Eingangsspalte und sogar ohne Tabelle auskommt.
/// Der Inhalt wird Skripten (z. B. Formular-Buttons) als Feld-Variable bereitgestellt
/// und von dort auch wieder zurückgeschrieben.
/// </summary>
public class EasyTextPadItem : ReciverPadItem, IItemToControl, IAutosizable, IHasFieldVariable {

    #region Constructors

    public EasyTextPadItem() : this(string.Empty, null) { }

    public EasyTextPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-EasyText";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None;
    public bool AutoSizeableHeight => true;

    /// <summary>
    /// Wenn gewählt, springt der Cursor zum nächsten Eingabefeld, wenn am Ende des Textes die Nach-rechts-Taste gedrückt wird.
    /// </summary>
    [DefaultValue(false)]
    public bool AutoNext {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Wenn gewählt, blendet das Feld eine Knopfleiste mit Kopieren und Einfügen ein.
    /// </summary>
    [DefaultValue(true)]
    public bool Buttonleiste {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = true;

    /// <summary>
    /// Der Name des Feldes. Im Skript wird das Feld mit dem Präfix field_ angesprochen, z. B. FIELD_MeinFeld.
    /// </summary>
    [DefaultValue("")]
    public string Feldname {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Der interne Variablen-Name des Feldes, z. B. FIELD_MeinFeld.
    /// </summary>
    public string FieldName => Feldname is { Length: > 0 } f ? "FIELD_" + f : string.Empty;

    /// <summary>
    /// Der Text, den das Feld beim Öffnen des Formulares enthält.
    /// </summary>
    [DefaultValue("")]
    public string DefaultValue {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Das Format des Inhaltes, z. B. Text oder Datum.
    /// </summary>
    [DefaultValue("")]
    public string FormatKey {
        get;

        set {
            if (IsDisposed) { return; }
            if (value.Equals(field, StringComparison.OrdinalIgnoreCase)) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => true;

    /// <summary>
    /// Wenn gewählt, kann der Benutzer den Text nicht ändern.
    /// </summary>
    [DefaultValue(false)]
    public bool NurLesen {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public override bool TableInputMustMatchOutputTable => false;
    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new EasyText {
            ReadOnly = NurLesen,
            Buttonleiste = Buttonleiste,
            FormatKey = FormatKey,
            AutoNext = AutoNext,
            DefaultValue = DefaultValue
        };

        con.DoDefaultSettings(parent, this, mode);
        return con;
    }

    /// <summary>
    /// Liefert die Feld-Variable für Skripte. Im Editor erscheint sie mit dem Standardwert des Feldes.
    /// </summary>
    public ScriptVariable? GetFieldVariable() {
        var fn = FieldName;

        if (!string.IsNullOrEmpty(fn)) {
            return new StringScriptVariable(fn, DefaultValue, false, "Wert des Formular-Feldes");
        }

        return null;
    }

    public void SetValueFromVariable(ScriptVariable v) => DefaultValue = v.ValueForCell;

    public override string ErrorReason() {
        if (base.ErrorReason() is { Length: > 0 } f) { return f; }

        if (Feldname is not { Length: > 0 }) { return "Feldname fehlt."; }

        if (FormatKey is not { Length: > 0 }) { return "Format fehlt."; }

        if (Format.AllFormats.Instances.FirstOrDefault(thisF => thisF.KeyName.Equals(FormatKey, StringComparison.OrdinalIgnoreCase)) is null) { return "Format unbekannt."; }

        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true),
            new FlexiControlForProperty<string>(() => Feldname),
            new FlexiControlForProperty<string>(() => DefaultValue),
            new FlexiControlForProperty<bool>(() => NurLesen),
            new FlexiControlForProperty<bool>(() => Buttonleiste),
            new FlexiControlForProperty<bool>(() => AutoNext),
            new FlexiControlForProperty<string>(() => FormatKey, FormatListe()),
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("FieldName", Feldname);
        result.ParseableAdd("DefaultValue", DefaultValue);
        result.ParseableAdd("FormatKey", FormatKey);
        result.ParseableAdd("ReadOnly", NurLesen);
        result.ParseableAdd("ButtonBar", Buttonleiste);
        result.ParseableAdd("AutoNext", AutoNext);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("feldname", Feldname);
        json.Set("defaultvalue", DefaultValue);
        json.Set("formatkey", FormatKey);
        json.Set("readonly", NurLesen);
        json.Set("buttonbar", Buttonleiste);
        json.Set("autonext", AutoNext);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            Feldname = json.GetString("feldname", Feldname);
            DefaultValue = json.GetString("defaultvalue", DefaultValue);
            FormatKey = json.GetString("formatkey", FormatKey);
            NurLesen = json.GetBool("readonly", NurLesen);
            Buttonleiste = json.GetBool("buttonbar", Buttonleiste);
            AutoNext = json.GetBool("autonext", AutoNext);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "fieldname":
            case "feldname":
                Feldname = value.FromNonCritical();
                return true;

            case "defaultvalue":
                DefaultValue = value.FromNonCritical();
                return true;

            case "formatkey":
                FormatKey = value.FromNonCritical();
                return true;

            case "readonly":
                NurLesen = value.FromPlusMinus();
                return true;

            case "buttonbar":
                Buttonleiste = value.FromPlusMinus();
                return true;

            case "autonext":
                AutoNext = value.FromPlusMinus();
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Textfeld: " + Feldname;

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, false);
        }

        DrawFakeControl(gr, positionControl, zoom, CaptionPosition.Über_dem_Feld, Feldname is { Length: > 0 } f ? f : "Textfeld");

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);
    }

    private static List<ListItem> FormatListe() {
        List<ListItem> l = [];

        foreach (var thisF in Format.AllFormats.Instances.OrderBy(thisF => thisF.KeyName, StringComparer.OrdinalIgnoreCase)) {
            l.Add(new ReadableListItem(thisF, true, string.Empty) { ShowError = false });
        }

        return l;
    }

    #endregion
}
