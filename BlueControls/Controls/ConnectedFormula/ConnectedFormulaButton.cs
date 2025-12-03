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
using BlueControls.EventArgs;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueScript;
using BlueScript.Methods;
using BlueScript.Variables;
using BlueTable;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

internal partial class ConnectedFormulaButton : GenericControlReciver {

    #region Constructors

    public ConnectedFormulaButton() : base(false, false, false) => InitializeComponent();

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Action {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg1 {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg2 {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg3 {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg4 {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg5 {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg6 {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg7 {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Arg8 {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

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

        if (string.IsNullOrEmpty(Action)) { enabled = false; }

        Enabled = enabled;
    }

    private static void ButtonError(string message) => Forms.MessageBox.Show("Dieser Knopfdruck konnte nicht ausgeführt werden.\r\n\r\nGrund:\r\n" + message, BlueBasics.Enums.ImageCode.Warnung, "Ok");

    private void mainButton_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (TableInput is { IsDisposed: false } tb) {
            tb.Editor ??= typeof(TableHeadEditor);
            e.ContextMenu.Add(ItemOf(tb));
        }
    }

    private void mainButton_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button != MouseButtons.Left) { return; }

        var m = Method.AllMethods.GetByKey(Action);

        if (m is not IUseableForButton ufb) {
            ButtonError("Aktion '" + Action + "' nicht gefunden.");
            return;
        }

        if (!ButtonPadItem.PossibleFor(m, Drückbar_wenn)) {
            ButtonError("Aktion '" + Action + "' nicht möglich.");
            return;
        }

        mainButton.Enabled = false;
        mainButton.Refresh();

        HandleChangesNow();

        VariableCollection vars;
        object? ai = null;
        var row = RowSingleOrNull();

        if (row?.Table is { IsDisposed: false } tb) {
            vars = tb.CreateVariableCollection(row, true, false, false, true); // Kein Zugriff auf DBVariables, wegen Zeitmangel der Programmierung. Variablen müssten wieder zurückgeschrieben werden.
        } else {
            vars = [];
        }

        #region FilterVariablen erstellen und in fis speichern

        var fis = string.Empty;

        if (FilterInput is { IsDisposed: false } fi) {
            for (var fz = 0; fz < fi.Count; fz++) {
                if (fi[fz] is { } thisf) {
                    var nam = "Filter" + fz;
                    vars.Add(new VariableFilterItem(nam, thisf, true, "FilterInput" + fz));
                    fis = fis + nam + ",";
                }
            }
            ai = fi.Table;
        }
        fis = fis.TrimEnd(",");

        #endregion

        #region RowVariable ertellen

        var rn = "thisrow";

        if (row != null) {
            vars.Add(new VariableRowItem("thisrow", row, true, "Eingangszeile"));
            ai = row;
            row.DropMessage(ErrorType.Info, "Knopfdruck mit dieser Zeile");
        }

        #endregion

        List<string> args = [Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8];

        var f = ufb.DoIt(vars, args, fis, rn, ai);

        if (RowCollection.InvalidatedRowsManager.IsProcessing) {
            f = "Ein anderer Prozess ist noch aktiv. Bitte kurz warten und nochmal starten.";
        } else {
            RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(row, true, null);
        }

        if (!string.IsNullOrEmpty(f)) {
            Develop.Message?.Invoke(ErrorType.DevelopInfo, null, Develop.MonitorMessage, BlueBasics.Enums.ImageCode.Kritisch, "Fehler: " + f, 0);
            Forms.MessageBox.Show("Dieser Knopfdruck wurde nicht komplett ausgeführt.\r\n\r\nGrund:\r\n" + f, BlueBasics.Enums.ImageCode.Kritisch, "Ok");
        } else {
            Develop.Message?.Invoke(ErrorType.DevelopInfo, null, Develop.MonitorMessage, BlueBasics.Enums.ImageCode.Häkchen, "Knopfdruck ausgeführt", 0);
        }
        mainButton.Enabled = true;
        mainButton.Refresh();
    }

    #endregion
}