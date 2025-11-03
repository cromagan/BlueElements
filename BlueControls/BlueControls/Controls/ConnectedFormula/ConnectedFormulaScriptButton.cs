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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueScript.Variables;
using BlueTable.Enums;
using System.ComponentModel;
using System.Windows.Forms;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls;

internal partial class ConnectedFormulaScriptButton : GenericControlReciver {

    #region Fields

    private ButtonArgs _enabledwhenrows;
    private string _script = string.Empty;

    #endregion

    #region Constructors

    public ConnectedFormulaScriptButton() : base(false, false, false) => InitializeComponent();

    #endregion

    #region Properties

    public ButtonArgs Drückbar_wenn {
        get => _enabledwhenrows;
        set {
            if (IsDisposed) { return; }
            if (_enabledwhenrows == value) { return; }
            _enabledwhenrows = value;
            Invalidate();
        }
    }

    public string ImageCode {
        get => mainButton.ImageCode;
        set => mainButton.ImageCode = value;
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Script {
        get => _script;
        set {
            if (IsDisposed) { return; }
            if (_script == value) { return; }
            _script = value;
            Invalidate();
        }
    }

    public new string Text {
        get => mainButton.Text;
        set => mainButton.Text = value;
    }

    #endregion

    #region Methods

    protected override void HandleChangesNow() {
        base.HandleChangesNow();

        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);

        DoRows();

        bool enabled;

        switch (_enabledwhenrows) {
            case ButtonArgs.Egal:
                enabled = true;
                break;

            case ButtonArgs.Keine_Zeile:
                enabled = RowsInput is { Count: 0 };
                break;

            case ButtonArgs.Genau_eine_Zeile:
                enabled = RowsInput is { Count: 1 };
                break;

            case ButtonArgs.Eine_oder_mehr_Zeilen:
                enabled = RowsInput is { Count: > 0 };
                break;

            default:
                enabled = false;
                break;
        }

        if (string.IsNullOrEmpty(_script)) { enabled = false; }

        Enabled = enabled;
    }

    private void mainButton_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button != MouseButtons.Left) { return; }

        mainButton.Enabled = false;
        mainButton.Refresh();

        HandleChangesNow();

        VariableCollection vars;

        var row = RowSingleOrNull();

        if (row?.Table is { IsDisposed: false } tb) {
            vars = tb.CreateVariableCollection(row, true, false, false, true); // Kein Zugriff auf DBVariables, wegen Zeitmangel der Programmierung. Variablen müssten wieder zurückgeschrieben werden.
        } else {
            vars = [];
        }

        //#region FilterVariablen erstellen und in fis speichern

        //var fis = string.Empty;

        //if (FilterInput is { IsDisposed: false } fi) {
        //    for (var fz = 0; fz < fi.Count; fz++) {
        //        if (fi[fz] is { } thisf) {
        //            var nam = "Filter" + fz;
        //            _ = vars.Add(new VariableFilterItem(nam, thisf, true, "FilterInput" + fz));
        //            fis = fis + nam + ",";
        //        }
        //    }
        //    ai = fi.Table;
        //}
        //fis = fis.TrimEnd(",");

        //#endregion

        var t = ScriptButtonPadItem.ExecuteScript(Script, Mode);

        if (t.Failed) {
            Develop.Message?.Invoke(ErrorType.DevelopInfo, null, Develop.MonitorMessage, BlueBasics.Enums.ImageCode.Kritisch, "Fehler: " + t.Protocol, 0);
            MessageBox.Show("Dieser Knopfdruck wurde nicht komplett ausgeführt.\r\n\r\nGrund:\r\n" + t.Protocol, BlueBasics.Enums.ImageCode.Kritisch, "Ok");
        }
        mainButton.Enabled = true;
        mainButton.Refresh();
    }

    #endregion
}