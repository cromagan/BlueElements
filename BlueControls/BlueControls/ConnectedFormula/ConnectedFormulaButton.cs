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

using BlueBasics;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using BlueScript;
using BlueScript.EventArgs;
using BlueScript.Interfaces;
using BlueScript.Variables;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls;

internal class ConnectedFormulaButton : Button, IControlUsesRow {

    #region Fields

    private string _action = string.Empty;

    private string _arg1 = string.Empty;

    private string _arg2 = string.Empty;

    private string _arg3 = string.Empty;

    private string _arg4 = string.Empty;

    private ButtonArgs _enabledwhenrows;

    private FilterCollection? _filterInput;

    #endregion

    #region Constructors

    public ConnectedFormulaButton() : base() {
        //((IControlSendFilter)this).RegisterEvents();
        this.RegisterEvents();
    }

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Action {
        get => _action;
        set {
            if (IsDisposed) { return; }
            if (_action == value) { return; }
            _action = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg1 {
        get => _arg1;
        set {
            if (IsDisposed) { return; }
            if (_arg1 == value) { return; }
            _arg1 = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg2 {
        get => _arg2;
        set {
            if (IsDisposed) { return; }
            if (_arg2 == value) { return; }
            _arg2 = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg3 {
        get => _arg3;
        set {
            if (IsDisposed) { return; }
            if (_arg3 == value) { return; }
            _arg3 = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
    public FilterCollection? FilterInput {
        get => _filterInput;
        set {
            if (_filterInput == value) { return; }
            this.UnRegisterEventsAndDispose();
            _filterInput = value;
            this.RegisterEvents();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool FilterInputChangedHandled { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendFilter> Parents { get; } = [];

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<RowItem>? RowsInput { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputChangedHandled { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputManualSeted { get; set; } = false;

    #endregion

    #region Methods

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e) => this.FilterInput_DispodingEvent();

    public void FilterInput_RowsChanged(object sender, System.EventArgs e) => this.FilterInput_RowsChanged();

    public void HandleChangesNow() {
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        if (!FilterInputChangedHandled) {
            FilterInputChangedHandled = true;
            this.DoInputFilter(null, false);
        }

        RowsInputChangedHandled = true;
        this.DoRows();

        bool enabled;

        switch (_enabledwhenrows) {
            case ButtonArgs.Egal:
                enabled = true;
                break;

            case ButtonArgs.Keine_Zeile:
                enabled = RowsInput != null && RowsInput.Count == 0;
                break;

            case ButtonArgs.Genau_eine_Zeile:
                enabled = RowsInput != null && RowsInput.Count == 1;
                break;

            case ButtonArgs.Eine_oder_mehr_Zeilen:
                enabled = RowsInput != null && RowsInput.Count > 0;
                break;

            default:
                enabled = false;
                break;
        }

        if (string.IsNullOrEmpty(_action)) { enabled = false; }

        Enabled = enabled;
    }

    public void ParentFilterOutput_Changed() { }

    public void RowsInput_Changed() { }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.DoDispose();
        }
        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        HandleChangesNow();
        base.DrawControl(gr, state);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);

        if (e.Button != MouseButtons.Left) { return; }

        if (Script.Commands == null) {
            ButtonError("Befehle konnten nicht initialisiert werden.");
            return;
        }

        var m = Script.Commands.Get(_action);

        if (m is not IUseableForButton ufb) {
            ButtonError("Aktion '" + _action + "' nicht gefunden.");
            return;
        }

        if (!ButtonPadItem.PossibleFor(m, Drückbar_wenn)) {
            ButtonError("Aktion '" + _action + "' nicht möglich.");
            return;
        }

        HandleChangesNow();

        VariableCollection vars;
        object? ai = null;
        var row = this.RowSingleOrNull();

        if (row?.Database is Database db && !db.IsDisposed) {
            vars = db.CreateVariableCollection(row, true, false, false, false); // Kein Zugriff auf DBVariables, wegen Zeitmangel der Programmierung. Variablen müssten wieder zurückgeschrieben werden.
        } else {
            vars = new VariableCollection();
        }

        #region FilterVariablen erstellen und in fis speichern

        var fis = string.Empty;

        if (FilterInput is FilterCollection fi) {
            for (var fz = 0; fz < fi.Count; fz++) {
                if (fi[fz] is FilterItem thisf) {
                    var nam = "Filter" + fz;
                    vars.Add(new VariableFilterItem(nam, thisf, true, "FilterInput" + fz));
                    fis = fis + nam + ",";
                }
            }
            ai = fi.Database;
        }
        fis = fis.TrimEnd(",");

        #endregion

        #region RowVariable ertellen

        var rn = "thisrow";

        if (row != null) {
            vars.Add(new VariableRowItem("thisrow", row, true, "Eingangszeile"));
            ai = row;
        }

        #endregion

        var f = ufb.DoIt(vars, _arg1, _arg2, _arg3, _arg4, fis, rn, ai);

        if (!string.IsNullOrEmpty(f)) {
            Forms.MessageBox.Show("Dieser Knopfdruck wurde nicht komplett ausgeführt.\r\n\r\nGrund:\r\n" + f, BlueBasics.Enums.ImageCode.Kritisch, "Ok");
        }
    }

    private void ButtonError(string message) => Forms.MessageBox.Show("Dieser Knopfdruck konnte nicht ausgeführt werden.\r\n\r\nGrund:\r\n" + message, BlueBasics.Enums.ImageCode.Warnung, "Ok");

    #endregion
}