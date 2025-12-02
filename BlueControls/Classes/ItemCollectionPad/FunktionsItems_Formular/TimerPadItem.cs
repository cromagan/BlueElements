// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.BlueTableDialogs;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.Abstract;
using BlueScript;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class TimerPadItem : RectanglePadItem, IItemToControl, IAutosizable, IOpenScriptEditor {

    #region Fields

    private FlexiDelegateControl? _button;
    private string _script = string.Empty;
    private int _sekunden = 1;

    #endregion

    #region Constructors

    public TimerPadItem() : this(string.Empty, null) { }

    public TimerPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-Timer";

    public bool AutoSizeableHeight => false;
    public override string Description => "Eine Schaltfläche, den der Benutzer drücken kann und eine Aktion startet.";

    public string Script {
        get => _script;

        set {
            if (value == _script) { return; }
            _script = value;
            this.RaiseVersion();
            OnPropertyChanged();
        }
    }

    public int Sekunden {
        get => _sekunden;
        set {
            if (IsDisposed) { return; }
            value = Math.Max(value, 1);
            value = Math.Min(value, 600);

            if (_sekunden == value) { return; }
            _sekunden = value;
            this.RaiseVersion();
            OnPropertyChanged();
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Version { get; set; }

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public static ScriptEndedFeedback ExecuteScript(string scripttext, string mode, string value0, string value1, string value2) {
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
            new VariableString("Value2", value2, false, "Diese Variable bleibt im Script erhalten uns steht beim nächsten Durchlauf wieder zur Verfügung.")
        ];

        //var m = Method.GetMethods(MethodType.);

        //using var gr = Graphics.FromImage(bmp);

        var scp = new ScriptProperties("Timer", Method.AllMethods, true, [], null, "Timer", "Timer in Formular");

        var sc = new Script(vars, scp) {
            ScriptText = scripttext
        };
        return sc.Parse(0, "Main", null, null);
    }

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FormulaTimer {
            Seconds = _sekunden,
            Script = _script,
            Name = this.DefaultItemToControlName(parent?.Page?.UniqueId),
            Mode = mode,
            ConnectedFormula = parent
        };

        //con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl)];

        _button = new FlexiDelegateControl(OpenScriptEditor, "Skript Editor", ImageCode.Skript);

        result.Add(new FlexiControl("Einstellungen:", widthOfControl, true));
        result.Add(_button);
        result.Add(new FlexiControlForProperty<string>(() => Script, 3));
        result.Add(new FlexiControlForProperty<int>(() => Sekunden));

        return result;
    }

    public bool IsVisibleForMe(string mode, bool nowDrawing) => true;

    /// <summary>
    /// Internes Skript
    /// </summary>
    public void OpenScriptEditor() {
        var f = GenericControl.ParentForm(_button);

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
        result.ParseableAdd("Script", _script);
        result.ParseableAdd("Seconds", _sekunden);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "version":
                Version = IntParse(value);
                return true;

            case "script":
                _script = value.FromNonCritical();
                return true;

            case "seconds":
                _sekunden = IntParse(value.FromNonCritical());
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Timer";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Uhr, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float scale, float offsetX, float offsetY) =>
        gr.DrawImage(SymbolForReadableText(), positionControl);

    #endregion

    //base.DrawExplicit(gr,visibleArea, positionControl, scale, offsetX, offsetY);//DrawArrorInput(gr, positionControl, scale, ForPrinting, InputColorId);
}