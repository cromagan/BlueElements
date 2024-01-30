// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using BlueScript;
using BlueScript.Methods;

namespace BlueControls.Controls;

internal class ConnectedFormulaButton : Button, IControlAcceptSomething {

    #region Fields

    private string _arg1 = string.Empty;
    private string _arg2 = string.Empty;
    private string _arg3 = string.Empty;
    private string _arg4 = string.Empty;
    private ButtonArgs _enabledwhenrows;

    private string _scriptname = string.Empty;

    #endregion

    #region Properties

    public string Arg1 {
        get => _arg1;
        set {
            if (IsDisposed) { return; }
            if (_arg1 == value) { return; }
            _arg1 = value;
            Invalidate();
        }
    }

    public string Arg2 {
        get => _arg2;
        set {
            if (IsDisposed) { return; }
            if (_arg2 == value) { return; }
            _arg2 = value;
            Invalidate();
        }
    }

    public string Arg3 {
        get => _arg3;
        set {
            if (IsDisposed) { return; }
            if (_arg3 == value) { return; }
            _arg3 = value;
            Invalidate();
        }
    }

    public string Arg4 {
        get => _arg4;
        set {
            if (IsDisposed) { return; }
            if (_arg4 == value) { return; }
            _arg4 = value;
            Invalidate();
        }
    }

    public ButtonArgs Drückbar_wenn {
        get => _enabledwhenrows;
        set {
            if (IsDisposed) { return; }
            if (_enabledwhenrows == value) { return; }
            _enabledwhenrows = value;
            Invalidate();
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public bool FilterManualSeted { get; set; } = false;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendSomething> Parents { get; } = [];

    public string SkriptName {
        get => _scriptname;
        set {
            if (IsDisposed) { return; }
            if (_scriptname == value) { return; }
            _scriptname = value;
            Invalidate();
        }
    }

    #endregion

    #region Methods

    public void FilterInput_Changed(object? sender, System.EventArgs e) {
        this.Invalidate_FilterInput(true);
        //if (FilterManualSeted) { DoInputFilterNow(); }
        Invalidate();
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) => Enabled = false;

    public void Parents_Added(bool hasFilter) {
        if (IsDisposed) { return; }
        if (!hasFilter) { return; }
        FilterInput_Changed(null, System.EventArgs.Empty);
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (disposing) {
            this.Invalidate_FilterInput(false);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (FilterInput == null) {
            this.DoInputFilter(null, false);
        }

        bool _enabled;

        switch (_enabledwhenrows) {
            case ButtonArgs.Egal:
                _enabled = true;
                break;

            case ButtonArgs.Keine_Zeile:
                _enabled = FilterInput != null && FilterInput.Rows.Count == 0;
                break;

            case ButtonArgs.Genau_eine_Zeile:
                _enabled = FilterInput != null && FilterInput.RowSingleOrNull != null;
                break;

            case ButtonArgs.Eine_oder_mehr_Zeilen:
                _enabled = FilterInput != null && FilterInput.Rows.Count > 0;
                break;

            default:
                Develop.DebugPrint(_enabledwhenrows);
                _enabled = false; break;
        }

        if (string.IsNullOrEmpty(_scriptname)) { _enabled = false; }

        Enabled = _enabled;

        base.DrawControl(gr, state);
    }

    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseUp(e);

        if (e.Button != System.Windows.Forms.MouseButtons.Left) { return; }

        if (Script.Commands == null) {
            _ = new Script(null, string.Empty, new BlueScript.Structures.ScriptProperties());
        }

        if (Script.Commands == null) {
            ButtonError("Befehle konnten nicht initialisiert werden."); return;
        }

        Method? m = null;
        foreach (var cmd in Script.Commands) {
            if (cmd.Syntax.Equals(_scriptname, System.StringComparison.OrdinalIgnoreCase)) {
                m = cmd;
                break;
            }
        }

        if (m == null) {
            ButtonError("Befehl '" + _scriptname + "' nicht gefunden."); return;
        }

        if (!ButtonPadItem.PossibleFor(m, Drückbar_wenn)) {
            ButtonError("Befehl '" + _scriptname + "' nicht möglich."); return;
        }
    }

    private void ButtonError(string message) {
        Forms.MessageBox.Show("Dieser Knopfdruck konnte nicht ausgeführt werden.\r\n\r\nGrund:\r\n" + message, BlueBasics.Enums.ImageCode.Warnung, "Ok");
    }

    #endregion
}