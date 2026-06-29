// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.BlueTableDialogs;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Controls;
using BlueScript.Classes;
using BlueScript.Variables;
using BlueTable.AdditionalScriptVariables;
using System.Windows.Forms;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

public class TimerPadItem : RectanglePadItem, IItemToControl, IAutosizable {

    #region Fields

    private FlexiControlForDelegate? _button;

    #endregion

    #region Constructors

    public TimerPadItem() : this(string.Empty) { }

    public TimerPadItem(string keyName) : base(keyName) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-Timer";

    public bool AutoSizeableHeight => false;

    public bool Deaktivierbar {
        get;
        set {
            if (value == field) { return; }
            field = value;
            this.RaiseVersion();
            OnPropertyChanged();
        }
    }

    public override string Description => "Ein Timer, der in regelmäßigen Abständen ein Skript ausführt. Optional nur bei Benutzer-Inaktivität.";

    public int MindestInaktivitaet {
        get;
        set {
            if (IsDisposed) { return; }
            value = Math.Clamp(value, 0, 600);
            if (field == value) { return; }
            field = value;
            this.RaiseVersion();
            OnPropertyChanged();
        }
    }

    public string Script {
        get;
        set {
            if (value == field) { return; }
            field = value;
            this.RaiseVersion();
            OnPropertyChanged();
        }
    } = string.Empty;

    public int Sekunden {
        get;
        set {
            if (IsDisposed) { return; }
            value = Math.Clamp(value, 1, 600);
            if (field == value) { return; }
            field = value;
            this.RaiseVersion();
            OnPropertyChanged();
        }
    } = 5;

    public bool StandardAktiviert {
        get;
        set {
            if (value == field) { return; }
            field = value;
            this.RaiseVersion();
            OnPropertyChanged();
        }
    } = true;

    public string Text {
        get;
        set {
            if (value == field) { return; }
            field = value;
            this.RaiseVersion();
            OnPropertyChanged();
        }
    } = string.Empty;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Version { get; set; }

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public static ScriptEndedFeedback ExecuteScript(string scripttext, string mode, string value0, string value1, string value2, bool produktivPhase, bool syntaxCheck, List<string>? args) {
        //var generatedentityID = rowIn.ReplaceVariables(entitiId, true, null);

        VariableCollection vars =
        [
            new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."),
            new VariableString("User", Generic.UserName, true,
                "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."),

            new VariableString("Usergroup", Generic.UserGroup, true,
                "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."),
            new VariableString("Mode", mode, true, "In welchem Modus die Formulare angezeigt werden."),
            new VariableString("Feedback", "Skript ausgeführt.", false, "Der Text wird im Timer Element angezeigt"),
            new VariableString("Value0", value0, false, "Diese Variable bleibt im Script erhalten uns steht beim nächsten Durchlauf wieder zur Verfügung."),
            new VariableString("Value1", value1, false, "Diese Variable bleibt im Script erhalten uns steht beim nächsten Durchlauf wieder zur Verfügung."),
            new VariableString("Value2", value2, false, "Diese Variable bleibt im Script erhalten uns steht beim nächsten Durchlauf wieder zur Verfügung."),
            new VariableRowItem("RowEmpty", null, true, "Dummy Zeile ohne Inhalt")
        ];

        BlueScript.Classes.Script.AddAttributes(vars, args ?? []);

        var scp = new ScriptProperties("Timer", Method.AllMethods.Instances, produktivPhase, [], null, "Timer", "Timer in Formular", syntaxCheck);

        var sc = new Script(vars, scp) {
            ScriptText = scripttext
        };
        return sc.Parse(0, "Main", null);
    }

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FormulaTimer {
            Seconds = Sekunden,
            Script = Script,
            Name = this.DefaultItemToControlName(parent?.Page?.UniqueId),
            Mode = mode,
            ConnectedFormula = parent,
            Deaktivierbar = Deaktivierbar,
            ItemText = Text,
            IsActive = StandardAktiviert,
            MinIdleSekunden = MindestInaktivitaet
        };

        //con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl)];

        _button = new FlexiControlForDelegate(OpenScriptEditor, "Skript Editor", ImageCode.Skript);

        result.Add(new FlexiControl("Einstellungen:", widthOfControl, true));
        result.Add(_button);
        result.Add(new FlexiControlForProperty<string>(() => Script, 3));
        result.Add(new FlexiControlForProperty<int>(() => Sekunden));
        result.Add(new FlexiControlForProperty<int>(() => MindestInaktivitaet));
        result.Add(new FlexiControlForProperty<bool>(() => StandardAktiviert));
        result.Add(new FlexiControlForProperty<bool>(() => Deaktivierbar));
        result.Add(new FlexiControlForProperty<string>(() => Text));

        return result;
    }

    public bool IsVisibleForMe(string mode, bool nowDrawing) => true;

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
        result.ParseableAdd("Version", Version);
        result.ParseableAdd("Script", Script);
        result.ParseableAdd("Seconds", Sekunden);
        result.ParseableAdd("MinIdle", MindestInaktivitaet);
        result.ParseableAdd("Active", StandardAktiviert);
        result.ParseableAdd("Disableable", Deaktivierbar);
        result.ParseableAdd("Text", Text);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "version":
                Version = IntParse(value);
                return true;

            case "script":
                Script = value.FromNonCritical();
                return true;

            case "seconds":
                Sekunden = IntParse(value.FromNonCritical());
                return true;

            case "minidle":
                MindestInaktivitaet = IntParse(value.FromNonCritical());
                return true;

            case "active":
                StandardAktiviert = value == "+";
                return true;

            case "disableable":
                Deaktivierbar = value == "+";
                return true;

            case "text":
                Text = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Timer";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Uhr, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) =>
        gr.DrawImage(SymbolForReadableText(), positionControl);

    #endregion

    //base.DrawExplicit(gr,visibleArea, positionControl, zoom, offsetX, offsetY);//DrawArrorInput(gr, positionControl, zoom, ForPrinting, InputColorId);
}