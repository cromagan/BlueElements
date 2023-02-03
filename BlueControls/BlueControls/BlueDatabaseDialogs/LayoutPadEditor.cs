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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using System;
using System.Drawing;
using static BlueBasics.IO;

namespace BlueControls.BlueDatabaseDialogs;

public partial class LayoutPadEditor : PadEditorWithFileAccess {

    #region Fields

    private DatabaseAbstract? _database;

    #endregion

    //private string _LoadedLayout = string.Empty;

    #region Constructors

    public LayoutPadEditor(DatabaseAbstract database) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        Database = database;
        scriptEditor.Database = database;

        BefülleLayoutDropdown();
        CheckButtons();
    }

    #endregion

    #region Properties

    public DatabaseAbstract? Database {
        get => _database;
        private set {
            if (value == _database) { return; }

            if (_database != null) {
                _database.Disposing -= Database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.Disposing += Database_Disposing;
            }
        }
    }

    #endregion

    #region Methods

    public override void ItemChanged() {
        base.ItemChanged();
        CheckButtons();
    }

    internal void LoadLayout(string fileOrLayoutId) {
        SaveCurrentLayout();
        cbxLayout.Text = fileOrLayoutId;

        if (string.IsNullOrEmpty(fileOrLayoutId)) {
            DisablePad();
            return;
        }

        var ind = Database.Layouts.LayoutIdToIndex(fileOrLayoutId);
        if (ind < 0) {
            if (fileOrLayoutId.FileSuffix().ToUpper() == "BCR") {
                LoadFile(fileOrLayoutId, fileOrLayoutId);
            } else {
                DisablePad();
                TextPadItem x = new("x", "Nicht editierbares Layout aus dem Dateisystem");
                Pad.Item.Add(x);
                x.Stil = Enums.PadStyles.Style_Überschrift_Haupt;
                x.SetCoordinates(new RectangleF(0, 0, 1000, 400), true);
                ItemChanged();
            }
        } else {
            LoadFromString(Database.Layouts[ind], fileOrLayoutId);
        }
    }

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        if (Database != null && !Database.IsDisposed) {
            Database.Disposing -= Database_Disposing;
        }
        SaveCurrentLayout();
        scriptEditor.Database = null;
        Database = null;
        base.OnFormClosing(e);
    }

    private void BefülleLayoutDropdown() {
        if (Database != null && !Database.IsDisposed) {
            cbxLayout.Item.Clear();
            ExportDialog.AddLayoutsOff(cbxLayout.Item, Database, true);
        }
    }

    private void btnCopyID_Click(object sender, System.EventArgs e) {
        SaveCurrentLayout();
        _ = Generic.CopytoClipboard(Pad.Item.Id);
        Notification.Show("ID kopiert.", ImageCode.Clipboard);
    }

    private void btnLayoutHinzu_Click(object sender, System.EventArgs e) {
        SaveCurrentLayout();
        var ex = InputBox.Show("Geben sie den Namen<br>des neuen Layouts ein:", string.Empty, FormatHolder.Text);
        if (string.IsNullOrEmpty(ex)) { return; }
        LoadLayout(string.Empty);

        ItemCollectionPad c = new() {
            Caption = ex
        };

        var lay = (LayoutCollection)Database.Layouts.Clone();
        lay.Add(c.ToString(false));
        Database.Layouts = lay;

        BefülleLayoutDropdown();
        LoadLayout(c.Id);
        CheckButtons();
    }

    private void btnLayoutLöschen_Click(object sender, System.EventArgs e) {
        SaveCurrentLayout();
        var ind = Database.Layouts.LayoutIdToIndex(Pad.Item.Id);
        if (ind < 0) {
            MessageBox.Show("Layout kann nur manuell gelöscht werden.");
            return;
        }
        if (MessageBox.Show("Layout <b>'" + Pad.Item.Caption + "'</b><br>wirklich löschen?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }

        var lay = (LayoutCollection)Database.Layouts.Clone();
        lay.RemoveAt(ind);
        Database.Layouts = lay;

        LoadLayout(string.Empty);
        BefülleLayoutDropdown();
        CheckButtons();
    }

    private void btnLayoutOeffnen_Click(object sender, System.EventArgs e) => ExecuteFile(cbxLayout.Text, string.Empty, false);

    private void btnLayoutUmbenennen_Click(object sender, System.EventArgs e) {
        SaveCurrentLayout();
        var ind = Database.Layouts.LayoutIdToIndex(Pad.Item.Id);
        if (ind < 0) {
            MessageBox.Show("Layout kann nur manuell umbenannt werden.");
            return;
        }
        var ex = InputBox.Show("Namen des Layouts ändern:", Pad.Item.Caption, FormatHolder.Text);
        if (string.IsNullOrEmpty(ex)) { return; }
        Pad.Item.Caption = ex;
        SaveCurrentLayout();
        BefülleLayoutDropdown();
        CheckButtons();
    }

    private void btnLayoutVerzeichnis_Click(object sender, System.EventArgs e) {
        if (Database == null) { return; }
        if (!string.IsNullOrEmpty(Database.AdditionalFilesPfadWhole())) {
            _ = ExecuteFile(Database.AdditionalFilesPfadWhole());
        }
        _ = ExecuteFile(Database.DefaultLayoutPath());
    }

    private void btnTextEditor_Click(object sender, System.EventArgs e) => ExecuteFile("notepad.exe", cbxLayout.Text, false);

    private void cbxLayout_ItemClicked(object sender, BasicListItemEventArgs e) => LoadLayout(e.Item.Internal);

    private void CheckButtons() {
        if (Database == null) {
            DisablePad();
            cbxLayout.Enabled = false;
            btnLayoutHinzu.Enabled = false;
            return;
        }
        btnLayoutHinzu.Enabled = true;
        if (cbxLayout.Item.Count > 0) {
            cbxLayout.Enabled = true;
        } else {
            cbxLayout.Enabled = false;
            cbxLayout.Text = string.Empty;
            DisablePad();
        }
        var ind = Database.Layouts.LayoutIdToIndex(cbxLayout.Text);
        if (ind < 0 && cbxLayout.Text.FileSuffix().ToUpper() != "BCR" && FileExists(cbxLayout.Text)) {
            btnTextEditor.Enabled = true;
            btnLayoutOeffnen.Enabled = true;
            tabStart.Enabled = false;
        } else {
            btnTextEditor.Enabled = false;
            btnLayoutOeffnen.Enabled = false;
        }
        //if (!string.IsNullOrEmpty(Pad.Item.ID)) {
        tabStart.Enabled = true;
        grpDateiSystem.Enabled = true;
        btnLayoutLöschen.Enabled = true;
        btnLayoutUmbenennen.Enabled = true;
        btnCopyID.Enabled = true;
        //} else {
        //    grpDrucken.Enabled = false;
        //     System.Windows.Forms.tabBearbeiten.Enabled = false;
        //    grpDateiSystem.Enabled = false;
        //    btnLayoutLöschen.Enabled = false;
        //    btnLayoutUmbenennen.Enabled = false;
        //}
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Close();

    private void SaveCurrentLayout() {
        //scriptEditor.WriteScriptBack();
        if (Database == null) { return; }
        var newl = Pad.Item.ToString(false);
        var ind = Database.Layouts.LayoutIdToIndex(Pad.Item.Id);
        if (ind > -1) {
            if (Database.Layouts[ind] == newl) { return; }

            var lay = (LayoutCollection)Database.Layouts.Clone();
            lay[ind] = newl;
            Database.Layouts = lay;

            if (!newl.StartsWith("{ID=#")) { Develop.DebugPrint("ID nicht gefunden: " + newl); }
            var ko = newl.IndexOf(", ", StringComparison.Ordinal);
            var id = newl.Substring(4, ko - 4);
            Database.InvalidateExports(id);
        } else if (Pad.Item.Id.FileSuffix().ToUpper() == "BCR") {
            WriteAllText(Pad.Item.Id, newl, System.Text.Encoding.UTF8, false);
        }
    }

    #endregion
}