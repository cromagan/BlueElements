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
using BlueDatabase;
using BlueScript;
using static BlueBasics.IO;

namespace BlueControls;

public partial class ScriptEditorDatabase : ScriptEditor//System.Windows.Forms.UserControl //
{
    #region Fields

    private DatabaseAbstract? _database;

    #endregion

    #region Constructors

    public ScriptEditorDatabase() {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public DatabaseAbstract? Database {
        get => _database;
        set {
            if (_database != null) {
                _database.RulesScript = ScriptText;
                _database.Disposing -= _Database_Disposing;
            }
            _database = value;
            if (_database == null) {
                return;
            }

            ScriptText = _database.RulesScript;
            _database.Disposing += _Database_Disposing;
        }
    }

    #endregion

    #region Methods

    internal void WriteScriptBack() {
        if (_database == null) { return; }
        _database.RulesScript = ScriptText;
    }

    protected override Script? GenerateAndDoScript() {
        if (_database == null) {
            Message("Keine Datenbank geladen.");
            return null;
        }
        _database.RulesScript = ScriptText;

        if (_database.Row.Count == 0) {
            Message("Zum Test wird zumindest eine Zeile benötigt.");
            return null;
        }
        if (string.IsNullOrEmpty(txbTestZeile.Text)) {
            txbTestZeile.Text = _database.Row.First().CellFirstString();
        }

        var r = _database.Row[txbTestZeile.Text];
        if (r == null) {
            Message("Zeile nicht gefunden.");
            return null;
        }

        var (_, message, s) = r.DoAutomatic("script testing");

        if (!string.IsNullOrEmpty(message)) {
            Message("Allgemeiner Fehler: " + message);
            return null;
        }

        return s;
    }

    protected override void OpenAdditionalFileFolder() {
        if (_database == null) {
            Message("Keine Datenbank geladen.");
            return;
        }
        if (DirectoryExists(_database.AdditionalFilesPfadWhole())) { ExecuteFile(_database.AdditionalFilesPfadWhole()); }
    }

    private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;

    private void scriptEditor_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (e.HotItem is string txt) {
            var c = _database.Column.Exists(txt);
            if (c is null) { return; }
            e.UserMenu.Add(ContextMenuComands.SpaltenEigenschaftenBearbeiten);
        }
    }

    private void scriptEditor_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        ColumnItem? c = null;

        if (e.HotItem is string txt) { c = _database.Column.Exists(txt); }

        switch (e.ClickedComand.ToLower()) {
            case "spalteneigenschaftenbearbeiten":
                if (c != null) {
                    Forms.TableView.OpenColumnEditor(c, null, null);
                }

                break;
        }
    }

    #endregion
}