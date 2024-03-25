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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollectionPad;
using BlueDatabase;
using BlueDatabase.Interfaces;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.IO;

namespace BlueControls.BlueDatabaseDialogs;

public partial class LayoutPadEditor : PadEditorWithFileAccess, IHasDatabase {

    #region Fields

    private Database? _database;

    #endregion

    #region Constructors

    public LayoutPadEditor(Database? database) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        Database = database;

        BefülleLayoutDropdown();
        CheckButtons();
    }

    #endregion

    #region Properties

    public Database? Database {
        get => _database;
        private set {
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

    #endregion

    #region Methods

    public override void ItemChanged() {
        base.ItemChanged();
        CheckButtons();
    }

    internal void LoadLayout(string fileOrLayoutId) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (Pad.Item == null) { return; }

        SaveCurrentLayout();
        cbxLayout.Text = fileOrLayoutId;

        if (string.IsNullOrEmpty(fileOrLayoutId)) {
            DisablePad();
            return;
        }

        if (fileOrLayoutId.FileSuffix().ToUpperInvariant() == "BCR") {
            LoadFile(fileOrLayoutId, fileOrLayoutId);
        } else {
            DisablePad();
            TextPadItem x = new("x", "Nicht editierbares Layout aus dem Dateisystem");
            Pad.Item.Add(x);
            x.Stil = PadStyles.Style_Überschrift_Haupt;
            x.SetCoordinates(new RectangleF(0, 0, 1000, 400), true);
            ItemChanged();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        SaveCurrentLayout();
        Database = null;
        base.OnFormClosing(e);
    }

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        Close();
    }

    private void BefülleLayoutDropdown() {
        if (Database != null && !Database.IsDisposed) {
            cbxLayout.ItemClear();
            ExportDialog.AddLayoutsOff(cbxLayout, Database);
        }
    }

    private void btnCopyID_Click(object sender, System.EventArgs e) {
        SaveCurrentLayout();
        if (Pad.Item?.KeyName != null) {
            _ = Generic.CopytoClipboard(Pad.Item.KeyName);
            Notification.Show("ID kopiert.", ImageCode.Clipboard);
        }
    }

    private void btnLayoutOeffnen_Click(object sender, System.EventArgs e) => ExecuteFile(cbxLayout.Text, string.Empty);

    private void btnLayoutVerzeichnis_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }
        if (!string.IsNullOrEmpty(Database.AdditionalFilesPfadWhole())) {
            _ = ExecuteFile(Database.AdditionalFilesPfadWhole());
        }
        _ = ExecuteFile(Database.DefaultLayoutPath());
    }

    private void btnTextEditor_Click(object sender, System.EventArgs e) => ExecuteFile("notepad.exe", cbxLayout.Text);

    private void cbxLayout_ItemClicked(object sender, AbstractListItemEventArgs e) => LoadLayout(e.Item.KeyName);

    private void CheckButtons() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) {
            DisablePad();
            cbxLayout.Enabled = false;
            return;
        }

        if (cbxLayout.ItemCount > 0) {
            cbxLayout.Enabled = true;
        } else {
            cbxLayout.Enabled = false;
            cbxLayout.Text = string.Empty;
            DisablePad();
        }

        if (cbxLayout.Text.FileSuffix().ToUpperInvariant() != "BCR" && FileExists(cbxLayout.Text)) {
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
        btnCopyID.Enabled = true;
        //} else {
        //    grpDrucken.Enabled = false;
        //     System.Windows.Forms.tabBearbeiten.Enabled = false;
        //    grpDateiSystem.Enabled = false;
        //    btnLayoutLöschen.Enabled = false;
        //    btnLayoutUmbenennen.Enabled = false;
        //}
    }

    private void SaveCurrentLayout() {
        if (IsDisposed || Database is not Database db || db.IsDisposed) { return; }

        if (Pad.Item == null) { return; }

        var newl = Pad.Item.ToString();
        if (Pad.Item.KeyName.FileSuffix().ToUpperInvariant() == "BCR") {
            WriteAllText(Pad.Item.KeyName, newl, Constants.Win1252, false);
        }
    }

    #endregion
}