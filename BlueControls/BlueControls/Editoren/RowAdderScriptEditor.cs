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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using BlueDatabase.Interfaces;
using BlueScript.Structures;
using System.Collections.Generic;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.IO;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class RowAdderScriptEditor : ScriptEditorGeneric, IHasDatabase {

    #region Fields

    private Database? _database;

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
        _ = l.WriteAllText(TempFile(string.Empty, string.Empty, "txt"), Win1252, true);
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
            if (value is not RowAdderPadItem) { value = null; }
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine Werte zurück zu schreiben während des Anzeigens

            if (value is RowAdderPadItem cpi) {
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

                _item = cpi;
            } else {
                tbcScriptEigenschaften.Enabled = false;
                Script = string.Empty;
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
            return new ScriptEndedFeedback("Keine Datenbank geladen.", false, false, "Allgemein");
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

        var r = Database?.Row[txbTestZeile.Text];
        if (r is not { IsDisposed: false }) {
            return new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
        }

        switch (scriptNo) {
            case 1:
                return RowAdder.ExecuteScript(_item.Script_Before, "Testmodus", _item.EntityID, r, true, "Before");

            case 3:
                return RowAdder.ExecuteScript(_item.Script_After, "Testmodus", _item.EntityID, r, true, "After");

            default:
                return RowAdder.ExecuteScript(_item.Script_MenuGeneration, "Testmodus", _item.EntityID, r, true, "Menu");
        }
    }

    public override void WriteInfosBack() {
        //if (IsDisposed || TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly) || Database == null || Database.IsDisposed) { return; }

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

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        Close();
    }

    private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Show(Database, typeof(DatabaseHeadEditor), false);

    #endregion
}