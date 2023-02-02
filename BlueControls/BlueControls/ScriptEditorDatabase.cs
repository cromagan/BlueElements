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
using System;
using static BlueBasics.IO;
using static BlueBasics.Extensions;
using System.Collections.ObjectModel;

namespace BlueControls;

public partial class ScriptEditorDatabase : ScriptEditor//System.Windows.Forms.UserControl //
{
    #region Fields

    private DatabaseAbstract? _database;

    private bool _isRowSkript = false;
    private string _skriptname = string.Empty;

    #endregion

    #region Constructors

    public ScriptEditorDatabase() => InitializeComponent();

    #endregion

    #region Properties

    public DatabaseAbstract? Database {
        get => _database;
        set {
            if (_database != null) {
                WriteScriptBack();
                _database.Disposing -= _Database_Disposing;
            }
            _database = value;
            if (_database == null || _database.IsDisposed) {
                return;
            }

            LoadScriptText();
            _database.Disposing += _Database_Disposing;
        }
    }

    public bool IsRowScript {
        get => _isRowSkript;
        set {
            if (_isRowSkript == value) { return; }
            WriteScriptBack();
            _isRowSkript = value;
            LoadScriptText();
        }
    }

    [Obsolete("Kann nur über Skriptname gesetzt werden!")]
    public new string ScriptText { get; set; }

    public string SkriptName {
        get => _skriptname;
        set {
            if (_skriptname == value) { return; }
            WriteScriptBack();
            _skriptname = value;
            LoadScriptText();
        }
    }

    #endregion

    #region Methods

    internal void WriteScriptBack() {
        if (_database == null || _database.IsDisposed) { return; }

        var sc = _database.EventScript.CloneWithClones();

        var ev = sc.Get(_skriptname);
        if (ev == null) { return; }

        ev.Script = base.ScriptText;

        _database.EventScript = new ReadOnlyCollection<EventScript?>(sc);
    }

    protected override Script? GenerateAndDoScript() {
        if (_database == null || _database.IsDisposed) {
            Message("Keine Datenbank geladen.");
            return null;
        }
        WriteScriptBack();
        string message = string.Empty;
        Script? sc = null;

        if (_isRowSkript) {
            if (_isRowSkript && _database.Row.Count == 0) {
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

            var (_, m, s) = r.DoAutomatic(string.Empty, true, _skriptname);
            message = m;
            sc = s;
        }

        if (!string.IsNullOrEmpty(message)) {
            Message("Allgemeiner Fehler: " + message);
            return null;
        }

        return sc;
    }

    protected override void OpenAdditionalFileFolder() {
        if (_database == null || _database.IsDisposed) {
            Message("Keine Datenbank geladen.");
            return;
        }
        if (DirectoryExists(_database.AdditionalFilesPfadWhole())) { _ = ExecuteFile(_database.AdditionalFilesPfadWhole()); }
    }

    private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;

    private void LoadScriptText() {
        if (_database == null || _database.IsDisposed) { return; }

        var sc = _database.EventScript.CloneWithClones();

        var ev = sc.Get(_skriptname);
        if (ev == null) {
            base.ScriptText = string.Empty;
            return;
        }

        base.ScriptText = ev.Script;
    }

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
                    Forms.TableView.OpenColumnEditor(c, null, null);
                }

                break;
        }
    }

    #endregion
}