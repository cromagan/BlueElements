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
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using BlueDatabase.Interfaces;
using BlueScript.EventArgs;
using BlueScript.Structures;
using static BlueBasics.Constants;
using static BlueBasics.IO;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class RowAdderScriptEditor : ScriptEditorGeneric, IHasDatabase {

    #region Fields

    private Database? _database;

    private RowAdderPadItem? _item;

    #endregion

    #region Constructors

    public RowAdderScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();

        List<string> l =
        [
            "In die Variable MENU muss das Menu erstellt werden.",
            "Dies muss folgendes Format sein:",
            "Backen",
            "Backen\\Zutaten",
            "Backen\\Zutaten\\Mehl",
            " ",
            "Endet der Eintrag mit einem + - Zeichen, wird es als DropdownMenu dargestellt.",
            " ",
            "Parallel dazu kann die Variable Infos erstellt werden.",
            "Freie Texte."
        ];
        l.WriteAllText(TempFile(string.Empty, string.Empty, "txt"), Win1252, true);
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

    public object? Object {
        get {
            if (IsDisposed) { return null; }

            return _item;
        }
        set {
            if (value is not RowAdderPadItem) { value = null; }
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine Werte zurück zu schreiben während des Anzeigens

            if (value is RowAdderPadItem cpi) {
                eventScriptEditor.Enabled = true;
                eventScriptEditor.Script = cpi.Script;
                _item = cpi;
            } else {
                eventScriptEditor.Enabled = false;
                eventScriptEditor.Script = string.Empty;
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

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        base.OnFormClosing(e);

        Object = null; // erst das Item!
    }

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        Close();
    }

    private void btnAusführen_Click(object sender, System.EventArgs e) => eventScriptEditor.TesteScript("MAIN");

    private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Show(Database, typeof(DatabaseHeadEditor), false);

    private void eventScriptEditor_ExecuteScript(object sender, ScriptEventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) {
            e.Feedback = new ScriptEndedFeedback("Keine Datenbank geladen.", false, false, "Allgemein");
            return;
        }

        if (_item == null) {
            e.Feedback = new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
            return;
        }

        WriteInfosBack();

        if (!_item.IsOk()) {
            e.Feedback = new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false, "Allgemein");
            return;
        }

        if (Database.Row.Count == 0) {
            e.Feedback = new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false, "Allgemein");
            return;
        }
        if (string.IsNullOrEmpty(txbTestZeile.Text)) {
            txbTestZeile.Text = Database?.Row.First()?.CellFirstString() ?? string.Empty;
        }

        var r = Database?.Row[txbTestZeile.Text];
        if (r is not { IsDisposed: false }) {
            e.Feedback = new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
            return;
        }

        e.Feedback = RowAdder.ExecuteScript(_item.Script, "Testmodus", _item.EntityID, r);
    }

    private void WriteInfosBack() {
        //if (IsDisposed || TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly) || Database == null || Database.IsDisposed) { return; }

        if (_item != null) {
            _item.Script = eventScriptEditor.Script;
        }
    }

    #endregion
}