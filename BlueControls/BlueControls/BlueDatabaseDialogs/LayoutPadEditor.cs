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

using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollectionPad;
using BlueDatabase;
using BlueDatabase.Interfaces;
using static BlueBasics.IO;

namespace BlueControls.BlueDatabaseDialogs;

public partial class LayoutPadEditor : PadEditorWithFileAccess, IHasDatabase {

    #region Fields

    private DatabaseAbstract? _database;

    #endregion

    #region Constructors

    public LayoutPadEditor(DatabaseAbstract? database) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        Database = database;

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
                _database.DisposingEvent -= Database_DisposingEvent;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += Database_DisposingEvent;
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (Pad.Item == null) { return; }

        SaveCurrentLayout();
        cbxLayout.Text = fileOrLayoutId;

        if (string.IsNullOrEmpty(fileOrLayoutId)) {
            DisablePad();
            return;
        }

        if (fileOrLayoutId.FileSuffix().ToUpper() == "BCR") {
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
        if (Database != null && !Database.IsDisposed) {
            Database.DisposingEvent -= Database_DisposingEvent;
        }
        SaveCurrentLayout();
        Database = null;
        base.OnFormClosing(e);
    }

    private void BefülleLayoutDropdown() {
        if (Database != null && !Database.IsDisposed) {
            cbxLayout.Item.Clear();
            ExportDialog.AddLayoutsOff(cbxLayout.Item, Database);
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
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        if (!string.IsNullOrEmpty(Database.AdditionalFilesPfadWhole())) {
            _ = ExecuteFile(Database.AdditionalFilesPfadWhole());
        }
        _ = ExecuteFile(Database.DefaultLayoutPath());
    }

    private void btnTextEditor_Click(object sender, System.EventArgs e) => ExecuteFile("notepad.exe", cbxLayout.Text);

    private void cbxLayout_ItemClicked(object sender, AbstractListItemEventArgs e) => LoadLayout(e.Item.KeyName);

    private void CheckButtons() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) {
            DisablePad();
            cbxLayout.Enabled = false;
            return;
        }

        if (cbxLayout.Item.Count > 0) {
            cbxLayout.Enabled = true;
        } else {
            cbxLayout.Enabled = false;
            cbxLayout.Text = string.Empty;
            DisablePad();
        }

        if (cbxLayout.Text.FileSuffix().ToUpper() != "BCR" && FileExists(cbxLayout.Text)) {
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

    private void Database_DisposingEvent(object sender, System.EventArgs e) => Close();

    private void SaveCurrentLayout() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        if (Pad.Item == null) { return; }

        var newl = Pad.Item.ToString();
        if (Pad.Item.KeyName.FileSuffix().ToUpper() == "BCR") {
            WriteAllText(Pad.Item.KeyName, newl, Constants.Win1252, false);
        }
    }

    #endregion
}