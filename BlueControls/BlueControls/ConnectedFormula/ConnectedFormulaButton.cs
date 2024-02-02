﻿// Authors:
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
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using BlueScript;
using BlueScript.EventArgs;
using BlueScript.Interfaces;
using BlueScript.Methods;
using BlueScript.Variables;

namespace BlueControls.Controls;

internal class ConnectedFormulaButton : Button, IControlAcceptSomething {

    #region Fields

    private string _action = string.Empty;
    private string _arg1 = string.Empty;
    private string _arg2 = string.Empty;
    private string _arg3 = string.Empty;
    private string _arg4 = string.Empty;
    private ButtonArgs _enabledwhenrows;

    #endregion

    #region Properties

    public string Action {
        get => _action;
        set {
            if (IsDisposed) { return; }
            if (_action == value) { return; }
            _action = value;
            Invalidate();
        }
    }

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

        bool enabled;

        switch (_enabledwhenrows) {
            case ButtonArgs.Egal:
                enabled = true;
                break;

            case ButtonArgs.Keine_Zeile:
                enabled = FilterInput != null && FilterInput.Rows.Count == 0;
                break;

            case ButtonArgs.Genau_eine_Zeile:
                enabled = FilterInput != null && FilterInput.RowSingleOrNull != null;
                break;

            case ButtonArgs.Eine_oder_mehr_Zeilen:
                enabled = FilterInput != null && FilterInput.Rows.Count > 0;
                break;

            default:
                Develop.DebugPrint(_enabledwhenrows);
                enabled = false;
                break;
        }

        if (string.IsNullOrEmpty(_action)) { enabled = false; }

        Enabled = enabled;

        base.DrawControl(gr, state);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);

        if (e.Button != MouseButtons.Left) { return; }

        if (Script.Commands == null) {
            ButtonError("Befehle konnten nicht initialisiert werden.");
            return;
        }

        Method? m = Script.Commands.Get(_action);

        if (m is not IUseableForButton ufb) {
            ButtonError("Aktion '" + _action + "' nicht gefunden.");
            return;
        }

        if (!ButtonPadItem.PossibleFor(m, Drückbar_wenn)) {
            ButtonError("Aktion '" + _action + "' nicht möglich.");
            return;
        }

        VariableCollection vars;
        object? ai = null;
        var row = FilterInput?.RowSingleOrNull;

        if (FilterInput?.Database is Database db && !db.IsDisposed) {
            vars = db.CreateVariableCollection(row, true, false);
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

    private void ButtonError(string message) {
        Forms.MessageBox.Show("Dieser Knopfdruck konnte nicht ausgeführt werden.\r\n\r\nGrund:\r\n" + message, BlueBasics.Enums.ImageCode.Warnung, "Ok");
    }

    #endregion
}