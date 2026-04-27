// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.BlueTableDialogs;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueScript.Classes;
using BlueScript.Methods;
using BlueScript.Variables;
using BlueTable.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

public class TimerPadItem : ReciverControlPadItem, IItemToControl, IAutosizable, IErrorCheckable {

    #region Fields

    private FlexiControlForDelegate? _button;
    private string _script = string.Empty;
    private int _sekunden = 1;

    #endregion

    #region Constructors

    public TimerPadItem() : this(string.Empty, null) { }

    public TimerPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-Timer";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;

    public bool AutoSizeableHeight => false;

    public override string Description => "Ein Timer, der in regelmäßigen Abständen ein Skript ausführt und auf Feld-Variablen zugreifen kann.";

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

    public int Sekunden {
        get => _sekunden;
        set {
            if (IsDisposed) { return; }
            value = Math.Clamp(value, 1, 600);

            if (_sekunden == value) { return; }
            _sekunden = value;
            this.RaiseVersion();
            OnPropertyChanged();
        }
    }

    public override bool TableInputMustMatchOutputTable => false;

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public static ScriptEndedFeedback ExecuteScript(string scripttext, string mode, VariableCollection fields, RowItem? row, bool produktiv) {
        VariableCollection vars =
        [
            new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."),
            new VariableString("User", Generic.UserName, true,
                "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."),

            new VariableString("Usergroup", Generic.UserGroup, true,
                "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."),
            new VariableString("Mode", mode, true, "In welchem Modus die Formulare angezeigt werden."),
            new VariableString("Feedback", "Skript ausgeführt.", false, "Der Text wird im Timer Element angezeigt"),
            new VariableString("Value0", string.Empty, false, "Diese Variable bleibt im Script erhalten uns steht beim nächsten Durchlauf wieder zur Verfügung."),
            new VariableString("Value1", string.Empty, false, "Diese Variable bleibt im Script erhalten uns steht beim nächsten Durchlauf wieder zur Verfügung."),
            new VariableString("Value2", string.Empty, false, "Diese Variable bleibt im Script erhalten uns steht beim nächsten Durchlauf wieder zur Verfügung.")
        ];

        vars.AddRange(fields);

        var scp = new ScriptProperties("Timer", Method.AllMethods, produktiv, [], row, "Timer", "Timer in Formular");

        var sc = new Script(vars, scp) {
            ScriptText = scripttext
        };
        return sc.Parse(0, "Main", null, null);
    }

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FormulaTimer {
            Seconds = _sekunden,
            Script = _script,
            Mode = mode
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (string.IsNullOrEmpty(_script)) {
            return "Kein Skript angegeben.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl)];

        _button = new FlexiControlForDelegate(OpenScriptEditor, "Skript Editor", ImageCode.Skript);

        result.Add(new FlexiControl("Einstellungen:", widthOfControl, true));
        result.Add(_button);
        result.Add(new FlexiControlForProperty<string>(() => Script, 3));
        result.Add(new FlexiControlForProperty<int>(() => Sekunden));

        return result;
    }

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

        f?.Opacity = 1f;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Script", _script);
        result.ParseableAdd("Seconds", _sekunden);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
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

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Uhr, 16, Color.Transparent, Skin.IdColor(InputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        gr.DrawImage(SymbolForReadableText(), positionControl);

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, false, false, true);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);

        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    #endregion
}
