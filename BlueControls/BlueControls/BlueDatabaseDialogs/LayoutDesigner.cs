﻿// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using static BlueBasics.FileOperations;

namespace BlueControls.BlueDatabaseDialogs {

    internal partial class LayoutDesigner : PadEditor {

        #region Constructors

        //private string _LoadedLayout = string.Empty;
        public LayoutDesigner(Database database) : base() {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Database = database;
            scriptEditor.Database = database;
            Database.Disposing += Database_Disposing;
            befülleLayoutDropdown();
            CheckButtons();
        }

        #endregion

        #region Properties

        public Database Database { get; private set; }

        #endregion

        #region Methods

        public override void ItemChanged() {
            base.ItemChanged();
            CheckButtons();
        }

        internal void LoadLayout(string fileOrLayoutID) {
            SaveCurrentLayout();
            cbxLayout.Text = fileOrLayoutID;
            if (string.IsNullOrEmpty(fileOrLayoutID)) {
                DisablePad();
                return;
            }
            var ind = Database.Layouts.LayoutIDToIndex(fileOrLayoutID);
            if (ind < 0) {
                if (fileOrLayoutID.FileSuffix().ToUpper() == "BCR") {
                    LoadFile(fileOrLayoutID, fileOrLayoutID);
                } else {
                    DisablePad();
                    TextPadItem x = new(Pad.Item, "x", "Nicht editierbares Layout aus dem Dateisystem");
                    Pad.Item.Add(x);
                    x.Stil = Enums.PadStyles.Style_Überschrift_Haupt;
                    x.SetCoordinates(new RectangleM(0, 0, 1000, 400), true);
                    ItemChanged();
                }
            } else {
                LoadFromString(Database.Layouts[ind], fileOrLayoutID);
            }
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            base.OnFormClosing(e);
            SaveCurrentLayout();
        }

        private void befülleLayoutDropdown() {
            if (Database != null) {
                cbxLayout.Item.Clear();
                ExportDialog.AddLayoutsOff(cbxLayout.Item, Database, true);
            }
        }

        private void btnLayoutHinzu_Click(object sender, System.EventArgs e) {
            SaveCurrentLayout();
            var ex = InputBox.Show("Geben sie den Namen<br>des neuen Layouts ein:", "", enDataFormat.Text);
            if (string.IsNullOrEmpty(ex)) { return; }
            LoadLayout(string.Empty);
            CreativePad c = new();
            c.Item.Caption = ex;
            Database.Layouts.Add(c.Item.ToString());
            befülleLayoutDropdown();
            LoadLayout(c.Item.ID);
            CheckButtons();
        }

        private void btnLayoutLöschen_Click(object sender, System.EventArgs e) {
            SaveCurrentLayout();
            var ind = Database.Layouts.LayoutIDToIndex(Pad.Item.ID);
            if (ind < 0) {
                MessageBox.Show("Layout kann nur manuell gelöscht werden.");
                return;
            }
            if (MessageBox.Show("Layout <b>'" + Pad.Item.Caption + "'</b><br>wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            Database.Layouts.RemoveAt(ind);
            LoadLayout(string.Empty);
            befülleLayoutDropdown();
            CheckButtons();
        }

        private void btnLayoutOeffnen_Click(object sender, System.EventArgs e) => ExecuteFile(cbxLayout.Text, null, false);

        private void btnLayoutUmbenennen_Click(object sender, System.EventArgs e) {
            SaveCurrentLayout();
            var ind = Database.Layouts.LayoutIDToIndex(Pad.Item.ID);
            if (ind < 0) {
                MessageBox.Show("Layout kann nur manuell umbenannt werden.");
                return;
            }
            var ex = InputBox.Show("Namen des Layouts ändern:", Pad.Item.Caption, enDataFormat.Text);
            if (string.IsNullOrEmpty(ex)) { return; }
            Pad.Item.Caption = ex;
            SaveCurrentLayout();
            befülleLayoutDropdown();
            CheckButtons();
        }

        private void btnLayoutVerzeichnis_Click(object sender, System.EventArgs e) {
            if (Database == null) { return; }
            if (!string.IsNullOrEmpty(Database.AdditionaFilesPfadWhole())) {
                ExecuteFile(Database.AdditionaFilesPfadWhole());
            }
            ExecuteFile(Database.Filename.FilePath() + "Layouts\\");
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
            var ind = Database.Layouts.LayoutIDToIndex(Pad.Item.ID);
            if (ind < 0 && Pad.Item.ID.FileSuffix().ToUpper() != "BCR" && FileExists(Pad.Item.ID)) {
                btnTextEditor.Enabled = true;
                btnLayoutOeffnen.Enabled = true;
                tabPageControl.Enabled = false;
                grpDrucken.Enabled = false;
            } else {
                btnTextEditor.Enabled = false;
                btnLayoutOeffnen.Enabled = false;
            }
            //if (!string.IsNullOrEmpty(Pad.Item.ID)) {
            tabPageControl.Enabled = true;
            grpDateiSystem.Enabled = true;
            btnLayoutLöschen.Enabled = true;
            btnLayoutUmbenennen.Enabled = true;
            grpDrucken.Enabled = true;
            //} else {
            //    grpDrucken.Enabled = false;
            //     System.Windows.Forms.TabPageControl.Enabled = false;
            //    grpDateiSystem.Enabled = false;
            //    btnLayoutLöschen.Enabled = false;
            //    btnLayoutUmbenennen.Enabled = false;
            //}
        }

        private void Database_Disposing(object sender, System.EventArgs e) {
            Database.Disposing -= Database_Disposing;
            scriptEditor.Database = null;
            Database = null;
            Close();
        }

        private void Pad_ClickedItemChanged(object sender, System.EventArgs e) {
            tabElementEigenschaften.Controls.Clear();
            if (Pad.LastClickedItem == null) { return; }
            var Flexis = Pad.LastClickedItem.GetStyleOptions();
            if (Flexis.Count == 0) { return; }
            var top = Skin.Padding;
            foreach (var ThisFlexi in Flexis) {
                tabElementEigenschaften.Controls.Add(ThisFlexi);
                ThisFlexi.DisabledReason = string.Empty;
                ThisFlexi.Left = Skin.Padding;
                ThisFlexi.Top = top;
                ThisFlexi.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
                top = top + Skin.Padding + ThisFlexi.Height;
                ThisFlexi.Width = tabElementEigenschaften.Width - (Skin.Padding * 4);
                //ThisFlexi.ButtonClicked += FlexiButtonClick;
            }
        }

        private void SaveCurrentLayout() {
            scriptEditor.WriteScriptBack();
            if (Database == null) { return; }
            var newl = Pad.Item.ToString();
            var ind = Database.Layouts.LayoutIDToIndex(Pad.Item.ID);
            if (ind > -1) {
                if (Database.Layouts[ind] == newl) { return; }
                Database.Layouts[ind] = newl;
            } else if (Pad.Item.ID.FileSuffix().ToUpper() == "BCR") {
                WriteAllText(Pad.Item.ID, newl, System.Text.Encoding.UTF8, false);
            }
        }

        #endregion
    }
}