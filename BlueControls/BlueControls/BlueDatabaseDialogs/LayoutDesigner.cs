#region BlueElements - a collection of useful tools, database and controls
// Authors: 
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using System.Drawing;
using System.IO;
using static BlueBasics.FileOperations;

namespace BlueControls.BlueDatabaseDialogs {

    internal partial class LayoutDesigner : PadEditor {
        public Database Database { get; private set; }

        private string _LoadedLayout = string.Empty;
        private string _AdditionalLayoutPath = "";

        public LayoutDesigner(Database database, string additionalLayoutPath) : base() {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Database = database;
            scriptEditor.Database = database;
            Database.Disposing += Database_Disposing;
            _AdditionalLayoutPath = additionalLayoutPath;

            befülleLayoutDropdown();

            CheckButtons();
        }

        private void Database_Disposing(object sender, System.EventArgs e) {
            Database.Disposing -= Database_Disposing;
            scriptEditor.Database = null;
            Database = null;
            Close();
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            base.OnFormClosing(e);
            SaveCurrentLayout();
        }

        private void CheckButtons() {

            if (cbxLayout.Item.Count > 0) {
                cbxLayout.Enabled = true;
            } else {
                cbxLayout.Enabled = false;
                cbxLayout.Text = string.Empty;
                _LoadedLayout = string.Empty;
            }

            if (Database != null) {
                btnLayoutHinzu.Enabled = true;
                if (Database.Layouts.Count == 0) {
                    _LoadedLayout = string.Empty;
                }
            } else {
                btnLayoutHinzu.Enabled = true;
                _LoadedLayout = string.Empty;
            }

            if (cbxLayout.Text.FileSuffix().ToUpper() != "BCR" && FileExists(cbxLayout.Text)) {
                _LoadedLayout = string.Empty;
                btnTextEditor.Enabled = true;
                btnLayoutOeffnen.Enabled = true;
                tabPageControl.Enabled = false;
                Area_Drucken.Enabled = false;
            } else {
                btnTextEditor.Enabled = false;
                btnLayoutOeffnen.Enabled = false;
            }

            if (!string.IsNullOrEmpty(_LoadedLayout)) {
                tabPageControl.Enabled = true;

                grpDateiSystem.Enabled = true;

                btnLayoutLöschen.Enabled = true;
                btnLayoutUmbenennen.Enabled = true;
                Area_Drucken.Enabled = true;
            } else {
                Area_Drucken.Enabled = false;
                tabPageControl.Enabled = false;

                grpDateiSystem.Enabled = false;

                btnLayoutLöschen.Enabled = false;
                btnLayoutUmbenennen.Enabled = false;
            }
        }

        private void btnLayoutHinzu_Click(object sender, System.EventArgs e) {
            SaveCurrentLayout();

            var ex = InputBox.Show("Geben sie den Namen<br>des neuen Layouts ein:", "", enDataFormat.Text);
            if (string.IsNullOrEmpty(ex)) { return; }
            var c = new CreativePad();

            c.Item.Caption = ex;
            Database.Layouts.Add(c.Item.ToString());

            befülleLayoutDropdown();

            _LoadedLayout = string.Empty;

            LoadLayout(c.Item.ID);

            CheckButtons();
        }

        private void btnLayoutLöschen_Click(object sender, System.EventArgs e) {
            SaveCurrentLayout();

            if (string.IsNullOrEmpty(_LoadedLayout)) { return; }

            if (MessageBox.Show("Layout <b>'" + Pad.Item.Caption + "'</b><br>wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }

            Pad.Item.Clear();
            var ind = Database.Layouts.LayoutIDToIndex(_LoadedLayout);

            Database.Layouts.RemoveAt(ind);
            _LoadedLayout = string.Empty;

            befülleLayoutDropdown();

            cbxLayout.Text = string.Empty;

            CheckButtons();

        }

        private void btnLayoutUmbenennen_Click(object sender, System.EventArgs e) {
            SaveCurrentLayout();
            if (string.IsNullOrEmpty(_LoadedLayout)) { return; }

            var ex = InputBox.Show("Namen des Layouts ändern:", Pad.Item.Caption, enDataFormat.Text);
            if (string.IsNullOrEmpty(ex)) { return; }
            Pad.Item.Caption = ex;

            SaveCurrentLayout();
            befülleLayoutDropdown();
            CheckButtons();

        }

        private void cbxLayout_ItemClicked(object sender, BasicListItemEventArgs e) => LoadLayout(e.Item.Internal);

        internal void LoadLayout(string fileOrLayoutID) {
            SaveCurrentLayout();

            cbxLayout.Text = fileOrLayoutID;

            var ind = Database.Layouts.LayoutIDToIndex(fileOrLayoutID);

            if (ind < 0) {

                if (fileOrLayoutID.FileSuffix().ToUpper() == "BCR") {
                    Pad.Enabled = true;
                    _LoadedLayout = fileOrLayoutID;
                    var l = File.ReadAllText(fileOrLayoutID, Constants.Win1252);
                    Pad.Item = new ItemCollectionPad(l, string.Empty);
                    ItemChanged();

                } else {
                    _LoadedLayout = string.Empty;
                    Pad.Item.Clear();
                    Pad.Item.SheetSizeInMM = SizeF.Empty;
                    var x = new TextPadItem(Pad.Item, "x", "Nicht editierbares Layout aus dem Dateisystem");
                    Pad.Item.Add(x);
                    x.Stil = Enums.PadStyles.Style_Überschrift_Haupt;
                    x.SetCoordinates(new RectangleM(0, 0, 1000, 400), true);
                    ItemChanged();
                    Pad.Enabled = false;
                }
            } else {
                Pad.Enabled = true;
                _LoadedLayout = fileOrLayoutID;
                Pad.Item = new ItemCollectionPad(Database.Layouts[ind], string.Empty);
                ItemChanged();
            }
        }

        public override void ItemChanged() {
            base.ItemChanged();
            CheckButtons();
        }

        private void SaveCurrentLayout() {
            scriptEditor.WriteScriptBack();
            if (Database == null) { return; }
            if (string.IsNullOrEmpty(_LoadedLayout)) { return; }

            var newl = Pad.Item.ToString();
            var ind = Database.Layouts.LayoutIDToIndex(_LoadedLayout);

            if (ind > -1) {
                if (Database.Layouts[ind] == newl) { return; }
                Database.Layouts[ind] = newl;
            } else if (_LoadedLayout.FileSuffix().ToUpper() == "BCR") {
                SaveToDisk(_LoadedLayout, newl, false, System.Text.Encoding.UTF8);
            }

            Pad.ZoomFit();
        }

        private void btnTextEditor_Click(object sender, System.EventArgs e) => ExecuteFile("notepad.exe", cbxLayout.Text, false);

        private void btnLayoutOeffnen_Click(object sender, System.EventArgs e) => ExecuteFile(cbxLayout.Text, null, false);

        private void btnLayoutVerzeichnis_Click(object sender, System.EventArgs e) {

            if (string.IsNullOrEmpty(_AdditionalLayoutPath) && Database != null) { _AdditionalLayoutPath = Database.Filename.FilePath() + "Layouts\\"; }

            ExecuteFile(_AdditionalLayoutPath);
        }

        private void befülleLayoutDropdown() {
            if (Database != null) {
                cbxLayout.Item.Clear();
                ExportDialog.AddLayoutsOff(cbxLayout.Item, Database, true, _AdditionalLayoutPath);
            }
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
    }
}