// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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

    private FlexiControlForDelegate? _button;

    private ExtText? _eTxt;

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
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    [Description("Ein Bild für die Schaltfläche. Beispiel: PlusZeichen|16")]
    public string Bild {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    [Description("Eine Information, die dem Benutzer angezeigt wird,\r\nwenn er den Mauszeiger über die Schaltfläche bewegt.")]
    public string ButtonQuickInfo {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override string Description => "Eine Schaltfläche, den der Benutzer drücken kann und dann ein Skript gestartet wird.";

    [Description("Schaltet den Knopf ein oder aus.<br>Dazu werden die Zeilen berechnet, die mit der Eingangsfilterung möglich sind.<br>Wobei ein Zahlenwert größer 1 als 'mehr als eine' gilt.")]
    public ButtonArgs Drückbar_wenn {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public override bool InputMustBeOneRow => false;

    public override bool MustBeInDrawingArea => true;

    public string Script {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override bool TableInputMustMatchOutputTable => false;

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    /// <summary>
    /// Führt das übergebene Skript aus und erzeugt dabei alle benötigten Variablen
    /// (Basis-, Tabellen-, Zeilen-, Filter- und Feld-Variablen) selbst.
    /// <para>
    /// Im Produktivmodus (<paramref name="produktiv"/> = true) werden virtuelle
    /// Spalten (z.B. RowColor) erzeugt und die echten Filter verwendet. Im Testmodus
    /// (<paramref name="produktiv"/> = false) entfallen virtuelle Spalten und es
    /// werden die übergebenen (ggf. Dummy-) <paramref name="filterItems"/> genutzt.
    /// </para>
    /// Die erzeugte VariableCollection wird im zurückgegebenen
    /// <see cref="ScriptEndedFeedback"/>.Variables bereitgestellt.
    /// </summary>
    public static ScriptEndedFeedback ExecuteScript(string scripttext, string mode, bool produktiv, List<string>? args,
                                                    RowItem? row, Table? table, IEnumerable<FilterItem>? filterItems,
                                                    IEnumerable<IHasFieldVariable>? fieldSources) {
        VariableCollection generatedVars =
        [
            new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."),
            new VariableString("User", Generic.UserName, true,
                "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."),
            new VariableString("Usergroup", Generic.UserGroup, true,
                "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."),
            new VariableString("Mode", mode, true, "In welchem Modus die Formulare angezeigt werden."),
            new VariableRowItem("RowEmpty", null, true, "Dummy Zeile ohne Inhalt")
        ];

        BlueScript.Classes.Script.AddAttributes(generatedVars, args ?? []);

        if (row?.Table is { IsDisposed: false } rowTb) {
            generatedVars.AddRange(rowTb.CreateVariableCollection(row, false, false, produktiv, true, filterItems));
        } else if (table is { IsDisposed: false }) {
            generatedVars.AddRange(table.CreateVariableCollection(null, false, false, produktiv, true, filterItems));
        }

        if (fieldSources is not null) {
            foreach (var hfv in fieldSources) {
                if (hfv.GetFieldVariable() is { } v) { generatedVars.Add(v); }
            }
        }

        var scp = new ScriptProperties("ScriptButton", Method.AllMethods.Instances, produktiv, [], row, "ScriptButton", "ScriptButton in Formular");

        var sc = new Script(generatedVars, scp) {
            ScriptText = scripttext
        };
        return sc.Parse(0, "Main", null);
    }

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new ConnectedFormulaScriptButton {
            Text = Beschriftung,
            ImageCode = Bild + "|16",
            Drückbar_wenn = Drückbar_wenn,
            Script = Script,
            QuickInfo = ButtonQuickInfo
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (string.IsNullOrEmpty(Script)) {
            return "Kein Skript angegeben.";
        }
        if (string.IsNullOrEmpty(ButtonQuickInfo)) {
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

        result.ParseableAdd("Caption", Beschriftung);
        result.ParseableAdd("Image", Bild);
        result.ParseableAdd("Script", Script);
        result.ParseableAdd("QuickInfo", ButtonQuickInfo);
        result.ParseableAdd("EnableWhenRows", Drückbar_wenn);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("caption", Beschriftung);
        json.Set("image", Bild);
        json.Set("script", Script);
        json.Set("buttonquickinfo", ButtonQuickInfo);
        json.Set("enablewhenrows", (int)Drückbar_wenn);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            Beschriftung = json.GetString("caption", Beschriftung);
            Bild = json.GetString("image", Bild);
            Script = json.GetString("script", Script);
            ButtonQuickInfo = json.GetString("buttonquickinfo", ButtonQuickInfo);
            Drückbar_wenn = json.GetEnum("enablewhenrows", Drückbar_wenn);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "caption":
                Beschriftung = value.FromNonCritical();
                return true;

            case "image":
                Bild = value.FromNonCritical();
                return true;

            case "version":
                Version = IntParse(value);
                return true;

            case "script":
                Script = value.FromNonCritical();
                return true;

            case "quickinfo":
                ButtonQuickInfo = value.FromNonCritical();
                return true;

            case "enablewhenrows":
                Drückbar_wenn = (ButtonArgs)IntParse(value);
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
        Button.DrawButton(null, gr, Design.Button, States.Standard, QuickImage.Get(Bild), Alignment.Horizontal_Vertical_Center, false, _eTxt, Beschriftung, positionControl.ToRect(), false);

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, false, false, true);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);

        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    /// <summary>
    /// Führt das Skript für den Testmodus im Editor aus. Stellt die Roh-Zutaten
    /// bereit: Eingehende Zeile, Dummy-Filter (damit Filter-abhängige Variablen
    /// erzeugt werden können) und die Field-Variablen-Quellen des Formulars.
    /// </summary>
    private ScriptEndedFeedback ExecuteScriptTest(string script, bool testmode) {
        var row = TableInput?.Row?.First();

        List<FilterItem>? fi = null;
        if (Parents.Count > 0 && TableInput is { IsDisposed: false } tbf && tbf.Column.First is { } c) {
            fi = [];
            for (var co = 0; co < Parents.Count; co++) {
                fi.Add(new FilterItem(c, FilterType.Istgleich_GroßKleinEgal, "DUMMY!"));
            }
        }

        List<IHasFieldVariable>? fieldSources = null;
        if (Parent is ItemCollectionPadItem { IsDisposed: false } icpi) {
            fieldSources = [];
            foreach (var thisCon in icpi) {
                if (thisCon is IHasFieldVariable hfv) { fieldSources.Add(hfv); }
            }
        }

        return ExecuteScript(script, "Testmodus", !testmode, null, row, TableInput, fi, fieldSources);
    }

    #endregion
}