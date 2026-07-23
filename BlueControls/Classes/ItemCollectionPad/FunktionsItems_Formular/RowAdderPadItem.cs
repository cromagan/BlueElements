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

    private string _additionalInfoColumnKey = string.Empty;

    private FlexiControlForDelegate? _button;

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezeptname, Personenname, Beleg-Nummer
    /// </summary>
    private string _entityId = string.Empty;

    /// <summary>
    /// Letzter Skript-Fehlertext, der beim Testen aufgetreten ist.
    /// Wird im Editor über "anzeigen" wieder sichtbar gemacht.
    /// </summary>
    private string _lastFailedReason = string.Empty;

    /// <summary>
    /// Variablen zum Zeitpunkt des letzten Fehlers.
    /// </summary>
    private List<Variable>? _lastSavedVariables;

    /// <summary>
    /// Eine Spalte in der Ziel-Tabelle.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    private string _originIdColumnKey = string.Empty;

    private string _script = string.Empty;

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

            var c = tb.Column[_additionalInfoColumnKey];
            return c is not { IsDisposed: false } ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Tabelle.\r\nIn diese wird eine Zusatzinfo gespeichert.\r\nDiese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.")]
    public string AdditionalInfoColumnKey {
        get => _additionalInfoColumnKey;
        set {
            if (IsDisposed) { return; }
            if (_additionalInfoColumnKey == value) { return; }
            _additionalInfoColumnKey = value;
            OnPropertyChanged();
            OnPropertyChangedExt("additionalInfoColumnKey", value);
        }
    }

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
        get => _entityId;
        set {
            if (IsDisposed) { return; }
            if (_entityId == value) { return; }
            _entityId = value;
            OnPropertyChanged();
            OnPropertyChangedExt("entityId", value);
        }
    }

    public override bool InputMustBeOneRow => true;

    /// <summary>
    /// Letzter Skript-Fehlertext, der beim Testen aufgetreten ist.
    /// Wird im Editor über "anzeigen" wieder sichtbar gemacht.
    /// </summary>
    public string LastFailedReason {
        get => _lastFailedReason;
        set {
            if (IsDisposed) { return; }
            if (value == _lastFailedReason) { return; }
            _lastFailedReason = value;
            OnPropertyChanged();
        }
    }

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

            var c = tb.Column[_originIdColumnKey];
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
        get => _originIdColumnKey;
        set {
            if (IsDisposed) { return; }
            if (_originIdColumnKey == value) { return; }
            _originIdColumnKey = value;
            OnPropertyChanged();
            OnPropertyChangedExt("originIdColumnKey", value);
        }
    }

    [Description("Skript, das die Auswahlliste (Menü) erzeugt, die dem User angezeigt wird. Aus der eingehenden Zeile und Variablen werden Einträge generiert, die bei Auswahl neue Zeilen in der Zieltabelle anlegen.")]
    public string Script {
        get => _script;

        set {
            if (IsDisposed) { return; }
            if (value == _script) { return; }
            _script = value;
            OnPropertyChanged();
            OnPropertyChangedExt("script", value);
        }
    }

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
        if (string.IsNullOrEmpty(_entityId)) { return "Id-Generierung fehlt"; }
        if (!_entityId.Contains('~')) { return "ID-Generierung muss mit Variablen definiert werden."; }

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
            var sd = new ScriptDescription(KeyName, _script);
            if (InputBoxEditor.Edit(sd)) {
                _script = sd.Script;
            }
        } finally {
            f?.Opacity = 1f;
        }
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("EntityID", _entityId);
        result.ParseableAdd("OriginIDColumnName", _originIdColumnKey);
        result.ParseableAdd("AdditionalInfoColumnName", _additionalInfoColumnKey);
        result.ParseableAdd("ScriptMenu", _script);
        result.ParseableAdd("LastFailedReason", _lastFailedReason);
        result.ParseableAdd("LastSavedVariables", _lastSavedVariables?.SortByKeyName().ToString(true) ?? string.Empty);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("entityid", _entityId);
        json.Set("originidcolumnkey", _originIdColumnKey);
        json.Set("additionalinfocolumnkey", _additionalInfoColumnKey);
        json.Set("scriptmenu", _script);
        json.Set("lastfailedreason", _lastFailedReason);
        json.SetArrayIfNotEmpty("lastsavedvariables", _lastSavedVariables?.SortByKeyName() ?? []);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        _entityId = json.GetString("entityid", _entityId);
        _originIdColumnKey = json.GetString("originidcolumnkey", _originIdColumnKey);
        _additionalInfoColumnKey = json.GetString("additionalinfocolumnkey", _additionalInfoColumnKey);
        _script = json.GetString("scriptmenu", _script);
        _lastFailedReason = json.GetString("lastfailedreason", _lastFailedReason);
        _lastSavedVariables = json.GetList<Variable>("lastsavedvariables", true);

        base.ParseJson(json);
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "entityid":
                _entityId = value.FromNonCritical();
                return true;

            case "originidcolum":
            case "originidcolumkey":
            case "originidcolumnname":
                _originIdColumnKey = value.FromNonCritical();
                return true;

            case "additionalinfocolumn":
            case "additionalinfocolumnkey":
            case "additionalinfocolumnname":
                _additionalInfoColumnKey = value.FromNonCritical();
                return true;

            case "script":
            case "scriptmenu":
                _script = value.FromNonCritical();
                return true;

            case "scriptbefore":
                return true;

            case "scriptafter":
                return true;

            case "lastfailedreason":
                _lastFailedReason = value.FromNonCritical();
                return true;

            case "lastsavedvariables":
                _lastSavedVariables = VariableCollection.ParseVariable(value.FromNonCritical(), true);
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

    #endregion
}