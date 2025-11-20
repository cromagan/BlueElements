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

using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueScript.Structures;
using BlueTable;
using BlueTable.Interfaces;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

public sealed partial class RowAdderScriptEditor : ScriptEditorGeneric, IHasTable {

    #region Fields

    private RowAdderPadItem? _item;

    /// <summary>
    /// 1 = Before
    /// 2 = Menu
    /// 3 = After
    /// </summary>
    private int scriptNo = 2;

    #endregion

    #region Constructors

    public RowAdderScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
    }

    #endregion

    #region Properties

    public override object? Object {
        get => IsDisposed ? null : (object?)_item;
        set {
            if (value is not RowAdderPadItem) { value = null; }
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null;

            if (value is RowAdderPadItem cpi) { _item = cpi; }

            ShowScript();
        }
    }

    /// <summary>
    /// Nur zum setzen der Zeile zum Testen.
    /// </summary>
    public RowItem? Row {
        set {
            txbTestZeile.Text = value?.CellFirstString() ?? string.Empty;
            Table = value?.Table;
        }
    }

    public Table? Table {
        get;
        set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            if (field != null) {
                field.DisposingEvent -= _table_Disposing;
            }
            field = value;

            if (field != null) {
                field.DisposingEvent += _table_Disposing;
            }
        }
    }

    #endregion

    #region Methods

    public override ScriptEndedFeedback ExecuteScript(bool testmode) {
        if (IsDisposed || Table is not { IsDisposed: false }) {
            return new ScriptEndedFeedback("Keine Tabelle geladen.", false, false, "Allgemein");
        }

        if (_item == null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        WriteInfosBack();

        if (!_item.IsOk()) {
            return new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false, "Allgemein");
        }

        if (Table.Row.Count == 0) {
            return new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false, "Allgemein");
        }
        if (string.IsNullOrEmpty(txbTestZeile.Text)) {
            txbTestZeile.Text = Table?.Row.First()?.CellFirstString() ?? string.Empty;
        }

        var r = Table?.Row[txbTestZeile.Text] ?? Table?.Row.GetByKey(txbTestZeile.Text);
        if (r is not { IsDisposed: false }) {
            return new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
        }

        switch (scriptNo) {
            case 1:
                return RowAdder.ExecuteScript(_item.Script_Before, "Testmodus", _item.EntityID, r, false, "Before");

            case 3:
                return RowAdder.ExecuteScript(_item.Script_After, "Testmodus", _item.EntityID, r, false, "After");

            default:
                return RowAdder.ExecuteScript(_item.Script_MenuGeneration, "Testmodus", _item.EntityID, r, true, "Menu");
        }
    }

    public override void WriteInfosBack() {
        //if (IsDisposed || TableView.ErrorMessage(Table, EditableErrorReasonType.EditNormaly) || Table == null || Table.IsDisposed) { return; }

        if (_item != null) {
            switch (scriptNo) {
                case 1:
                    _item.Script_Before = Script;
                    break;

                case 2:
                    _item.Script_MenuGeneration = Script;
                    break;

                case 3:
                    _item.Script_After = Script;
                    break;
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        base.OnFormClosing(e);

        Object = null; // erst das Item!
    }

    private void _table_Disposing(object sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void btnScriptAfter_Click(object sender, System.EventArgs e) {
        WriteInfosBack();
        scriptNo = 3;
        ShowScript();
    }

    private void btnScriptBefore_Click(object sender, System.EventArgs e) {
        WriteInfosBack();
        scriptNo = 1;
        ShowScript();
    }

    private void btnScriptMenu_Click(object sender, System.EventArgs e) {
        WriteInfosBack();
        scriptNo = 2;
        ShowScript();
    }

    private void btnTabelleKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Show(Table, typeof(TableHeadEditor), false);

    private void ShowScript() {
        if (_item is RowAdderPadItem cpi) {
            tbcScriptEigenschaften.Enabled = true;

            switch (scriptNo) {
                case 1:
                    Script = cpi.Script_Before;
                    break;

                case 2:
                    Script = cpi.Script_MenuGeneration;
                    break;

                case 3:
                    Script = cpi.Script_After;
                    break;

                default:
                    tbcScriptEigenschaften.Enabled = false;
                    Script = string.Empty;
                    break;
            }
        } else {
            tbcScriptEigenschaften.Enabled = false;
            Script = string.Empty;
        }
    }

    #endregion
}