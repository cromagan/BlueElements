// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.BlueTableDialogs;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using BlueControls.Extended_Text;
using BlueScript.Classes;
using BlueScript.Variables;
using BlueTable.AdditionalScriptVariables;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;
using Button = BlueControls.Controls.Button;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

public class ScriptButtonPadItem : ReciverControlPadItem, IItemToControl, IAutosizable, IErrorCheckable {

    #region Fields

    private string _beschriftung = string.Empty;

    private FlexiControlForDelegate? _button;

    private ButtonArgs _enabledwhenrows;

    private ExtText? _eTxt;

    private string _image = string.Empty;

    private string _quickinfo = string.Empty;

    private string _script = string.Empty;

    #endregion

    #region Constructors

    public ScriptButtonPadItem() : this(string.Empty, null) { }

    public ScriptButtonPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-ScriptButton";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;

    public bool AutoSizeableHeight => false;

    [Description("Die Beschriftung der Schaltfläche.")]
    public string Beschriftung {
        get => _beschriftung;
        set {
            if (IsDisposed) { return; }
            if (_beschriftung == value) { return; }
            _beschriftung = value;
            OnPropertyChanged();
        }
    }

    [Description("Ein Bild für die Schaltfläche. Beispiel: PlusZeichen|16")]
    public string Bild {
        get => _image;
        set {
            if (IsDisposed) { return; }
            if (_image == value) { return; }
            _image = value;
            OnPropertyChanged();
        }
    }

    [Description("Eine Information, die dem Benutzer angezeigt wird,\r\nwenn er den Mauszeiger über die Schaltfläche bewegt.")]
    public string ButtonQuickInfo {
        get => _quickinfo;
        set {
            if (IsDisposed) { return; }
            if (_quickinfo == value) { return; }
            _quickinfo = value;
            OnPropertyChanged();
        }
    }

    public override string Description => "Eine Schaltfläche, den der Benutzer drücken kann und dann ein Skript gestartet wird.";

    [Description("Schaltet den Knopf ein oder aus.<br>Dazu werden die Zeilen berechnet, die mit der Eingangsfilterung möglich sind.<br>Wobei ein Zahlenwert größer 1 als 'mehr als eine' gilt.")]
    public ButtonArgs Drückbar_wenn {
        get => _enabledwhenrows;
        set {
            if (IsDisposed) { return; }
            if (_enabledwhenrows == value) { return; }
            _enabledwhenrows = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public override bool InputMustBeOneRow => false;

    public override bool MustBeInDrawingArea => true;

    public string Script {
        get => _script;

        set {
            if (value == _script) { return; }
            _script = value;
            this.RaiseVersion();
            OnPropertyChanged();
        }
    }

    public override bool TableInputMustMatchOutputTable => false;

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public static ScriptEndedFeedback ExecuteScript(string scripttext, string mode, VariableCollection fields, RowItem? row, bool produktiv, List<string>? args) {
        //var generatedentityID = rowIn.ReplaceVariables(entitiId, true, null);

        VariableCollection vars =
        [
            new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."),
            new VariableString("User", Generic.UserName, true,
                "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."),

            new VariableString("Usergroup", Generic.UserGroup, true,
                "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."),
            new VariableString("Mode", mode, true, "In welchem Modus die Formulare angezeigt werden."),
            new VariableRowItem("RowEmpty", null, true, "Dummy Zeile ohne Inhalt")
        ];

        BlueScript.Classes.Script.AddAttributes(vars, args ?? []);
        vars.AddRange(fields);

        var scp = new ScriptProperties("ScriptButton", Method.AllMethods.Instances, produktiv, [], row, "ScriptButton", "ScriptButton in Formular");

        var sc = new Script(vars, scp) {
            ScriptText = scripttext
        };
        return sc.Parse(0, "Main", null);
    }

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new ConnectedFormulaScriptButton {
            Text = _beschriftung,
            ImageCode = _image + "|16",
            Drückbar_wenn = _enabledwhenrows,
            Script = _script,
            QuickInfo = ButtonQuickInfo
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (string.IsNullOrEmpty(_script)) {
            return "Kein Skript angegeben.";
        }
        if (string.IsNullOrEmpty(_quickinfo)) {
            return "Keine Quickinfo angegeben.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl)];

        //if (TableInput is not { IsDisposed: false }) { return result; }

        result.Add(new FlexiControl("Einstellungen:", widthOfControl, true));

        result.Add(new FlexiControlForProperty<string>(() => Beschriftung));

        var im = QuickImage.Images();

        var c = new List<AbstractListItem>();
        foreach (var thisIm in im) {
            c.Add(ItemOf(thisIm, thisIm, QuickImage.Get(thisIm, 16)));
        }

        result.Add(new FlexiControlForProperty<string>(() => Bild, c));

        List<AbstractListItem> za =
        [
            ItemOf("...keine Zeile gefunden wurde", ((int)ButtonArgs.Keine_Zeile).ToString1()),
            ItemOf("...genau eine Zeile gefunden wurde", ((int)ButtonArgs.Genau_eine_Zeile).ToString1()),
            ItemOf("...genau eine oder mehr Zeilen gefunden wurden",
                ((int)ButtonArgs.Eine_oder_mehr_Zeilen).ToString1()),
            ItemOf("...egal - immer", ((int)ButtonArgs.Egal).ToString1())
        ];

        result.Add(new FlexiControlForProperty<ButtonArgs>(() => Drückbar_wenn, za));

        _button = new FlexiControlForDelegate(OpenScriptEditor, "Skript Editor", ImageCode.Skript);
        result.Add(_button);
        result.Add(new FlexiControlForProperty<string>(() => Script, 3));

        result.Add(new FlexiControlForProperty<string>(() => ButtonQuickInfo, 3));

        return result;
    }

    /// <summary>
    /// Internes Skript
    /// </summary>
    public void OpenScriptEditor() {
        var f = _button?.ParentForm;

        f?.Opacity = 0f;

        var tse = new TimerScriptEditor {
            Object = this
        };
        tse.ShowDialog();

        //  var se = IUniqueWindowExtension.ShowOrCreate<TimerScriptEditor>(this);

        f?.Opacity = 1f;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Caption", _beschriftung);
        result.ParseableAdd("Image", _image);
        result.ParseableAdd("Script", _script);
        result.ParseableAdd("QuickInfo", _quickinfo);
        result.ParseableAdd("EnableWhenRows", _enabledwhenrows);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("caption", _beschriftung);
        json.Set("image", _image);
        json.Set("script", _script);
        json.Set("quickinfo", _quickinfo);
        json.Set("enablewhenrows", (int)_enabledwhenrows);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        _beschriftung = json.GetString("caption", _beschriftung);
        _image = json.GetString("image", _image);
        _script = json.GetString("script", _script);
        _quickinfo = json.GetString("quickinfo", _quickinfo);
        _enabledwhenrows = json.GetEnum("enablewhenrows", _enabledwhenrows);
        base.ParseJson(json);
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "caption":
                _beschriftung = value.FromNonCritical();
                return true;

            case "image":
                _image = value.FromNonCritical();
                return true;

            case "version":
                Version = IntParse(value);
                return true;

            case "script":
                _script = value.FromNonCritical();
                return true;

            case "quickinfo":
                _quickinfo = value.FromNonCritical();
                return true;

            case "enablewhenrows":
                _enabledwhenrows = (ButtonArgs)IntParse(value);
                return true;

            case "style":
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Knopf mit Skript: ";

        return txt + TableInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Stop, 16, Color.Transparent, Skin.IdColor(InputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        _eTxt ??= new ExtText(Design.Button, States.Standard);
        Button.DrawButton(null, gr, Design.Button, States.Standard, QuickImage.Get(_image), Alignment.Horizontal_Vertical_Center, false, _eTxt, _beschriftung, positionControl.ToRect(), false);

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, false, false, true);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);

        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    #endregion
}