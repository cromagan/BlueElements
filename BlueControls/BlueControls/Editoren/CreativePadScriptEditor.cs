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
using BlueControls.Forms;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using BlueDatabase.Interfaces;
using BlueScript.Structures;
using System.Windows.Forms;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class CreativePadScriptEditor : ScriptEditorGeneric, IHasDatabase {

    #region Fields

    private Database? _database;

    private CreativePadItem? _item;

    #endregion

    #region Constructors

    public CreativePadScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
    }

    #endregion

    #region Properties

    public Database? Database {
        get => _database;
        set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    public override object? Object {
        get => IsDisposed ? null : (object?)_item;
        set {
            if (value is not CreativePadItem) { value = null; }
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine Werte zurück zu schreiben während des Anzeigens

            if (value is CreativePadItem cpi) {
                tbcScriptEigenschaften.Enabled = true;
                Script = cpi.Script;
                LastFailedReason = string.Empty;
                _item = cpi;
            } else {
                tbcScriptEigenschaften.Enabled = false;
                Script = string.Empty;
                LastFailedReason = string.Empty;
            }
        }
    }

    /// <summary>
    /// Nur zum setzen der Zeile zum Testen.
    /// </summary>
    public RowItem? Row {
        set {
            txbTestZeile.Text = value?.CellFirstString() ?? string.Empty;
            Database = value?.Database;
        }
    }

    #endregion

    #region Methods

    public override ScriptEndedFeedback ExecuteScript(bool testmode) {
        if (IsDisposed || Database is not { IsDisposed: false }) {
            return new ScriptEndedFeedback("Keine Tabelle geladen.", false, false, "Allgemein");
        }

        if (_item == null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        WriteInfosBack();

        if (!_item.IsOk()) {
            return new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false, "Allgemein");
        }

        if (Database.Row.Count == 0) {
            return new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false, "Allgemein");
        }
        if (string.IsNullOrEmpty(txbTestZeile.Text)) {
            txbTestZeile.Text = Database?.Row.First()?.CellFirstString() ?? string.Empty;
        }

        var r = Database?.Row[txbTestZeile.Text] ?? Database?.Row.SearchByKey(txbTestZeile.Text); 
        if (r is not { IsDisposed: false }) {
            return new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
        }

        var p = new ItemCollectionPadItem {
            Endless = true
        };
        var f = p.ExecuteScript(_item.Script, "Testmodus", r);

        cpad.Items = p;
        cpad.ZoomFit();
        return f;
    }

    public override void WriteInfosBack() {
        //if (IsDisposed || TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly) || Database == null || Database.IsDisposed) { return; }

        if (_item != null) {
            _item.Script = Script;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        base.OnFormClosing(e);

        Object = null; // erst das Item!
    }

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        Close();
    }

    private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Show(Database, typeof(DatabaseHeadEditor), false);

    #endregion
}