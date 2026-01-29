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
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.Interfaces;
using BlueScript.Variables;
using BlueTable.Classes;
using BlueTable.Enums;
using System.ComponentModel;
using System.Windows.Forms;

namespace BlueControls.Controls;

internal partial class ConnectedFormulaScriptButton : GenericControlReciver {

    #region Constructors

    public ConnectedFormulaScriptButton() : base(false, false, false) => InitializeComponent();

    #endregion

    #region Properties

    public ButtonArgs Drückbar_wenn {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
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
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

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
        RowsInputChangedHandled = true;

        bool enabled;

        switch (Drückbar_wenn) {
            case ButtonArgs.Egal:
                enabled = true;
                break;

            case ButtonArgs.Keine_Zeile:
                enabled = FilterInput?.Rows is { Count: 0 };
                break;

            case ButtonArgs.Genau_eine_Zeile:
                enabled = FilterInput?.Rows is { Count: 1 };
                break;

            case ButtonArgs.Eine_oder_mehr_Zeilen:
                enabled = FilterInput?.Rows is { Count: > 0 };
                break;

            default:
                enabled = false;
                break;
        }

        if (string.IsNullOrEmpty(Script)) { enabled = false; }

        Enabled = enabled;
    }

    private void mainButton_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button != MouseButtons.Left) { return; }

        mainButton.Enabled = false;
        mainButton.Refresh();

        HandleChangesNow();

        #region Variablen erstellen

        VariableCollection vars;

        var row = RowSingleOrNull();
        Table? tb = null;

        if (row?.Table is { IsDisposed: false } row_tb) {
            tb = row_tb;
            vars = tb.CreateVariableCollection(row, false, false, true, true, FilterInput);
        } else if (FilterInput?.Table is { IsDisposed: false } fi_tb) {
            tb = fi_tb;
            vars = tb.CreateVariableCollection(null, false, false, true, true, FilterInput);
        } else {
            vars = [];
        }

        var rowstamp = row?.RowStamp();

        if (Parent is IHasFieldVariable hfvp && hfvp.GetFieldVariable() is { } v2) {
            vars.Add(v2);
        }

        foreach (var thisCon in Parent.Controls) {
            if (thisCon is IHasFieldVariable hfv && hfv.GetFieldVariable() is { } v) {
                vars.Add(v);
            }
        }

        #endregion

        var t = ScriptButtonPadItem.ExecuteScript(Script, Mode, vars, row, true);

        var errorreason = string.Empty;

        if (row?.RowStamp() != rowstamp) { errorreason = "Die Zeile wurde während des Ausführens verändert."; }

        if (t.Failed) { errorreason = t.ProtocolText; }

        if (string.IsNullOrEmpty(errorreason)) {

            #region Variablen zurückschreiben

            foreach (var thisCon in Parent.Controls) {
                if (thisCon is IHasFieldVariable hfv && vars.GetByKey(hfv.FieldName) is { ReadOnly: false } v) {
                    hfv.SetValueFromVariable(v);
                }
            }
            tb?.WriteBackVariables(row, vars, false, true, "Script-Button-Press", !t.Failed);

            #endregion
        } else {
            Develop.Message(ErrorType.DevelopInfo, null, Develop.MonitorMessage, BlueBasics.Enums.ImageCode.Kritisch, $"Fehler: {t.ProtocolText}", 0);
            Forms.MessageBox.Show($"Dieser Knopfdruck wurde nicht komplett ausgeführt.\r\n\r\nGrund:\r\n{errorreason}", BlueBasics.Enums.ImageCode.Kritisch, "Ok");
        }

        mainButton.Enabled = true;
        mainButton.Refresh();
    }

    #endregion
}