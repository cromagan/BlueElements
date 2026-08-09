// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using BlueScript.Classes;
using BlueScript.Variables;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeugt eine Liste mit Zeilen, die eine andere Tabelle befüllen können.
/// </summary>
public class RowAdderPadItem : ReciverSenderControlPadItem, IItemToControl, IAutosizable, ISimpleEditor {

    #region Fields

    private FlexiControlForDelegate? _button;

    /// <summary>
    /// Variablen zum Zeitpunkt des letzten Fehlers.
    /// </summary>
    private List<Variable>? _lastSavedVariables;

    #endregion

    #region Constructors

    public RowAdderPadItem() : this(string.Empty, null, null) { }

    public RowAdderPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public RowAdderPadItem(string keyName, Table? db, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowAdder";

    public ColumnItem? AdditionalInfoColumn {
        get {
            if (TableOutput is not { IsDisposed: false } tb) { return null; }

            var c = tb.Column[AdditionalInfoColumnKey];
            return c is not { IsDisposed: false } ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Tabelle.\r\nIn diese wird eine Zusatzinfo gespeichert.\r\nDiese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.")]
    public string AdditionalInfoColumnKey {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
            OnPropertyChangedExt("additionalInfoColumnKey", value);
        }
    } = string.Empty;

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    public override string Description => "Ein Steuerelement, das eine andere Tabelle befüllen kann.\r\n" +
                                          "<b>Aus der eingehenden Zeile (Referenz-Zeile)</b> wird eine ID generiert, diese wird zum dauerhaften Speichern in der Ausgangstabelle benutzt.\r\n" +
                                            "Diese ID wird auch als Ausgangsfilter weitergegeben.\r\n" +
                                            "<b>In die Ausgangs-Tabelle</b> werden durch Skripte gesteuert neue Zeilen angelegt.";

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezeptname, Personenname, Beleg-Nummer
    /// </summary>
    [Description("Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.\r\nDadurch können verschiedene Datensätze gespeichert werden.\r\nBeispiele: Rezepetname, Personenname, Beleg-Nummer")]
    public string EntityID {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
            OnPropertyChangedExt("entityId", value);
        }
    } = string.Empty;

    public override bool InputMustBeOneRow => true;

    /// <summary>
    /// Letzter Skript-Fehlertext, der beim Testen aufgetreten ist.
    /// Wird im Editor über "anzeigen" wieder sichtbar gemacht.
    /// </summary>
    public string LastFailedReason {
        get;
        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Variablen zum Zeitpunkt des letzten Fehlers.
    /// </summary>
    public List<Variable>? LastSavedVariables {
        get => _lastSavedVariables;
        set {
            if (IsDisposed) { return; }
            if (value == _lastSavedVariables) { return; }
            _lastSavedVariables = value;
            OnPropertyChanged();
        }
    }

    public override bool MustBeInDrawingArea => true;

    /// <summary>
    /// Eine Spalte in der Ziel-Tabelle.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    public ColumnItem? OriginIDColumn {
        get {
            if (TableOutput is not { IsDisposed: false } tb) { return null; }

            var c = tb.Column[OriginIDColumnKey];
            return c is not { IsDisposed: false } ? null : c;
        }
    }

    /// <summary>
    /// Eine Spalte in der Ziel-Tabelle.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    [Description("Eine Spalte in der Ziel-Tabelle.\r\nIn diese wird die generierte ID des klickbaren Elements gespeichert.\r\nDiese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.")]
    public string OriginIDColumnKey {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
            OnPropertyChangedExt("originIdColumnKey", value);
        }
    } = string.Empty;

    [Description("Skript, das die Auswahlliste (Menü) erzeugt, die dem User angezeigt wird. Aus der eingehenden Zeile und Variablen werden Einträge generiert, die bei Auswahl neue Zeilen in der Zieltabelle anlegen.")]
    public string Script {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
            OnPropertyChangedExt("script", value);
        }
    } = string.Empty;

    public override bool TableInputMustMatchOutputTable => false;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new RowAdder {
            EntityID = EntityID,
            OriginIDColumn = OriginIDColumn,
            AdditionalInfoColumn = AdditionalInfoColumn,
            Script = Script,
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (string.IsNullOrEmpty(EntityID)) { return "Id-Generierung fehlt"; }
        if (!EntityID.Contains('~')) { return "ID-Generierung muss mit Variablen definiert werden."; }

        if (OriginIDColumn is not { IsDisposed: false } oic) {
            return "Spalte, in der die Herkunft-ID geschrieben werden soll, fehlt";
        }

        if (oic is { IsKeyColumn: false, IsFirst: false }) {
            return $"Die Herkunft-ID-Spalte '{oic.Caption}' muss eine Schlüsselspalte oder die erste Spalte sein.";
        }

        if (AdditionalInfoColumn is not { IsDisposed: false } aci) {
            return "Spalte, in der die Zusatzinfo geschrieben werden soll, fehlt";
        }

        if (aci is { IsKeyColumn: false, IsFirst: false }) {
            return $"Die Zusatzinfo-Spalte '{aci.Caption}' muss eine Schlüsselspalte oder die erste Spalte sein.";
        }

        if (string.IsNullOrEmpty(Script)) {
            return "Kein Skript für die Menugenerierung definiert.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true)
        ];

        var inr = GetFilterFromGet();
        if (inr.Count > 0 && inr[0].TableOutput is { IsDisposed: false } inTable) {
            var entityFlex = new FlexiControlForProperty<string>(() => EntityID);
            entityFlex.EditType = EditTypeFormula.Textfeld_mit_Suggestions;
            entityFlex.ListItems = [.. inTable.Column.Where(c => !c.IsDisposed).Select(c => ItemOf($"~{c.KeyName}~"))];
            entityFlex.Height = 24;
            entityFlex.SuggestionPosition = SuggestionPosition.ContextMenuOnly;
            result.Add(entityFlex);
            _button = new FlexiControlForDelegate(OpenScriptEditor, "Skript Editor", ImageCode.Skript);
            result.Add(_button);
        }

        if (TableOutput is { IsDisposed: false } tb) {
            var lst = ItemsOf(tb.Column);
            result.Add(new FlexiControlForProperty<string>(() => OriginIDColumnKey, lst));
            result.Add(new FlexiControlForProperty<string>(() => AdditionalInfoColumnKey, lst));
        }

        return result;
    }

    /// <summary>
    /// Internes Skript
    /// </summary>
    public void OpenScriptEditor() {
        var f = _button?.ParentForm;

        f?.Opacity = 0f;

        try {
            var sd = new ScriptDescription(KeyName, Script);

            sd.ExecuteScript = ExecuteScriptTest;

            if (InputBoxEditor.Edit(sd)) {
                Script = sd.Script;
            }
        } finally {
            f?.Opacity = 1f;
        }
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("EntityID", EntityID);
        result.ParseableAdd("OriginIDColumnName", OriginIDColumnKey);
        result.ParseableAdd("AdditionalInfoColumnName", AdditionalInfoColumnKey);
        result.ParseableAdd("ScriptMenu", Script);
        result.ParseableAdd("LastFailedReason", LastFailedReason);
        result.ParseableAdd("LastSavedVariables", LastSavedVariables?.SortByKeyName().ToString(true) ?? string.Empty);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("entityid", EntityID);
        json.Set("originidcolumnkey", OriginIDColumnKey);
        json.Set("additionalinfocolumnkey", AdditionalInfoColumnKey);
        json.Set("scriptmenu", Script);
        json.Set("lastfailedreason", LastFailedReason);
        json.SetArrayIfNotEmpty("lastsavedvariables", LastSavedVariables?.SortByKeyName() ?? []);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            EntityID = json.GetString("entityid", EntityID);
            OriginIDColumnKey = json.GetString("originidcolumnkey", OriginIDColumnKey);
            AdditionalInfoColumnKey = json.GetString("additionalinfocolumnkey", AdditionalInfoColumnKey);
            Script = json.GetString("scriptmenu", Script);
            LastFailedReason = json.GetString("lastfailedreason", LastFailedReason);
            LastSavedVariables = json.GetList<Variable>("lastsavedvariables", true);

            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "entityid":
                EntityID = value.FromNonCritical();
                return true;

            case "originidcolum":
            case "originidcolumkey":
            case "originidcolumnname":
                OriginIDColumnKey = value.FromNonCritical();
                return true;

            case "additionalinfocolumn":
            case "additionalinfocolumnkey":
            case "additionalinfocolumnname":
                AdditionalInfoColumnKey = value.FromNonCritical();
                return true;

            case "script":
            case "scriptmenu":
                Script = value.FromNonCritical();
                return true;

            case "scriptbefore":
                return true;

            case "scriptafter":
                return true;

            case "lastfailedreason":
                LastFailedReason = value.FromNonCritical();
                return true;

            case "lastsavedvariables":
                LastSavedVariables = VariableCollection.ParseVariable(value.FromNonCritical(), true);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Zeilengenerator: ";

        return txt + TableOutput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        DrawArrowOutput(gr, positionControl, zoom, forPrinting, OutputColorId);

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, false);
        }

        //if (Column is null || Column .IsDisposed) {
        //    Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionControl.ToRect(), CaptionFnt.Scale(zoom), true);
        //} else {
        //DrawFakeControl(gr, positionControl, zoom, CaptionPosition, Column?.ReadableText() + ":", EditType);
        //}

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);

        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    /// <summary>
    /// Führt das Skript für den Testmodus im Editor aus. Für den Test wird eine
    /// Eingangs-Zeile benötigt, aus der die Entity-ID generiert werden kann.
    /// RowAdder verlangt zwingend genau eine Eingangs-Zeile (InputMustBeOneRow).
    /// </summary>
    private ScriptEndedFeedback ExecuteScriptTest(string script, bool testmode) {
        var row = TableInput?.Row.FirstOrDefault();
        if (row is not { IsDisposed: false }) {
            return new ScriptEndedFeedback("Keine Eingangs-Zeile zum Testen vorhanden.", false, false, "Allgemein");
        }

        return RowAdder.ExecuteScript(script, !testmode, "Testmodus", EntityID, row, true);
    }

    #endregion
}