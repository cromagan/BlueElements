// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueScript.Structures;
using static BlueBasics.IO;

namespace BlueControls;

public partial class ScriptEditorDatabase : ScriptEditor//System.Windows.Forms.UserControl //
{
    #region Fields

    private DatabaseAbstract? _database;

    private EventTypes _eventTypes = 0;
    private bool _isRowSkript;

    #endregion

    #region Constructors

    public ScriptEditorDatabase() => InitializeComponent();

    #endregion

    #region Properties

    public DatabaseAbstract? Database {
        get => _database;
        set {
            if (_database != null) {
                _database.Disposing -= _Database_Disposing;
            }

            _database = value;

            if (_database != null && !_database.IsDisposed) {
                _database.Disposing += _Database_Disposing;
            }
        }
    }

    public EventTypes EventTypes {
        get => _eventTypes;
        set {
            if (_eventTypes == value) { return; }
            _eventTypes = value;
            OnChanged();
        }
    }

    public bool IsRowScript {
        get => _isRowSkript;
        set {
            if (_isRowSkript == value) { return; }
            _isRowSkript = value;
            txbTestZeile.Enabled = value;
            OnChanged();
        }
    }

    #endregion

    #region Methods

    protected override ScriptEndedFeedback GenerateAndExecuteScript() {
        if (_database == null || _database.IsDisposed) {
            return new ScriptEndedFeedback("Keine Datenbank geladen.");
        }

        RowItem? r = null;

        if (_isRowSkript) {
            if (_isRowSkript && _database.Row.Count == 0) {
                return new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.");
            }
            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = _database.Row.First().CellFirstString();
            }

            r = _database.Row[txbTestZeile.Text];
            if (r == null) {
                return new ScriptEndedFeedback("Zeile nicht gefunden.");
            }
        }
        var s = _database.ExecuteScript(ScriptText, false, r, _eventTypes.HasFlag(EventTypes.error_check));
        return s;
    }

    protected override void OpenAdditionalFileFolder() {
        if (_database == null || _database.IsDisposed) {
            Message("Keine Datenbank geladen.");
            return;
        }
        if (DirectoryExists(_database.AdditionalFilesPfadWhole())) { _ = ExecuteFile(_database.AdditionalFilesPfadWhole()); }
    }

    private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;

    private void scriptEditor_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (e.HotItem is string txt) {
            var c = _database.Column.Exists(txt);
            if (c is null) { return; }
            _ = e.UserMenu.Add(ContextMenuComands.SpaltenEigenschaftenBearbeiten);
        }
    }

    private void scriptEditor_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        ColumnItem? c = null;

        if (e.HotItem is string txt) { c = _database.Column.Exists(txt); }

        switch (e.ClickedComand.ToLower()) {
            case "spalteneigenschaftenbearbeiten":
                if (c != null) {
                    TableView.OpenColumnEditor(c, null, null);
                }

                break;
        }
    }

    #endregion
}