// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using static BlueBasics.FileOperations;
using BlueControls.ItemCollection;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using static BlueBasics.Converter;

namespace BlueControls.Forms {

    public partial class ConnectedFormulaEditor : PadEditor {

        #region Fields

        private ConnectedFormula.ConnectedFormula? _cf;

        #endregion

        #region Constructors

        public ConnectedFormulaEditor(string filename, List<string>? notAllowedchilds) {
            InitializeComponent();

            FormulaSet(filename, notAllowedchilds);
        }

        public ConnectedFormulaEditor() : this(string.Empty, null) { }

        #endregion

        #region Methods

        private void btnFeldHinzu_Click(object sender, System.EventArgs e) {
            var l = Pad.LastClickedItem;

            var x = new EditFieldPadItem(string.Empty);

            if (l is RowWithFilterPaditem ri) {
                x.GetRowFrom = ri;
            }
            if (l is EditFieldPadItem efi && efi.GetRowFrom != null) {
                x.GetRowFrom = efi.GetRowFrom;
            }

            Pad.AddCentered(x);

            if (x.GetRowFrom != null && x.GetRowFrom.Database != null) {
                x.Spalte_wählen = string.Empty; // Dummy setzen
            }
        }

        private void btnKonstante_Click(object sender, System.EventArgs e) {
            var x = new ConstantTextPaditem();
            x.Bei_Export_Sichtbar = false;
            Pad.AddCentered(x);
        }

        private void btnLetzteDateien_ItemClicked(object sender, EventArgs.BasicListItemEventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);

            if (e?.Item == null) { return; }
            FormulaSet(e.Item.Internal, null);
        }

        private void btnNeuDB_SaveAs_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);

            if (sender == btnSaveAs) {
                if (_cf == null) { return; }
            }

            if (sender == btnNeuDB) {
                if (_cf != null) { FormulaSet(null as ConnectedFormula.ConnectedFormula, null); }
            }

            SaveTab.ShowDialog();
            if (!PathExists(SaveTab.FileName.FilePath())) { return; }
            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

            if (sender == btnNeuDB) {
                FormulaSet(new ConnectedFormula.ConnectedFormula(), null); // Ab jetzt in der Variable _Database zu finden
            }
            if (FileExists(SaveTab.FileName)) { DeleteFile(SaveTab.FileName, true); }

            _cf.SaveAsAndChangeTo(SaveTab.FileName);

            FormulaSet(SaveTab.FileName, null);
        }

        private void btnOeffnen_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            LoadTab.ShowDialog();
        }

        private void btnPfeileAusblenden_CheckedChanged(object sender, System.EventArgs e) => btnVorschauModus.Checked = btnPfeileAusblenden.Checked;

        private void btnTabControlAdd_Click(object sender, System.EventArgs e) {
            if (_cf == null) { return; }

            var x = new ChildFormulaPaditem(string.Empty, _cf.Filename, _cf.NotAllowedChilds);
            x.Bei_Export_Sichtbar = true;
            Pad.AddCentered(x);
        }

        private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => btnPfeileAusblenden.Checked = btnVorschauModus.Checked;

        private void btnVorschauÖffnen_Click(object sender, System.EventArgs e) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            EditBoxRow_NEW.Show("Achtung:\r\nVoll funktionsfähige Test-Ansicht", _cf, true);
        }

        private void btnZeileHinzu_Click(object sender, System.EventArgs e) {
            if (_cf == null) { return; }

            var x = Directory.GetFiles(_cf.Filename.FilePath(), "*.mdb").ToList();

            if (x == null || x.Count == 0) {
                MessageBox.Show("Keine Datenbanken vorhanden.");
                return;
            }

            var fi = new ItemCollectionList();
            foreach (var thisf in x) {
                fi.Add(thisf.FileNameWithoutSuffix(), thisf);
            }

            var rück = InputBoxListBoxStyle.Show("Datenbank wählen: ", fi, Enums.AddType.None, true);

            if (rück == null || rück.Count != 1) { return; }

            _cf.DatabaseFiles.AddIfNotExists(rück[0]);

            var db = Database.GetByFilename(rück[0], false, false);

            if (db != null) {
                var dbitem = new RowWithFilterPaditem(db, _cf.NextID());
                dbitem.Bei_Export_Sichtbar = false;
                Pad.AddCentered(dbitem);
            }
        }

        private void CheckButtons() {
        }

        private void FormulaSet(string filename, List<string>? notAllowedchilds) {
            FormulaSet(null as ConnectedFormula.ConnectedFormula, notAllowedchilds);

            if (!FileExists(filename)) {
                CheckButtons();
                return;
            }

            btnLetzteFormulare.AddFileName(filename, string.Empty);
            LoadTab.FileName = filename;
            var tmpDatabase = ConnectedFormula.ConnectedFormula.GetByFilename(filename);
            if (tmpDatabase == null) { return; }
            FormulaSet(tmpDatabase, notAllowedchilds);
        }

        private void FormulaSet(ConnectedFormula.ConnectedFormula? formular, List<string>? notAllowedchilds) {
            _cf = formular;

            if (notAllowedchilds != null && _cf != null) {
                _cf.NotAllowedChilds.AddRange(notAllowedchilds);
            }

            if (_cf == null) {
                Pad.Item = null;
            } else {
                Pad.Item = _cf.PadData;
                Pad.Item.SheetSizeInMm = new SizeF(PixelToMm(500, 300), PixelToMm(850, 300));
                Pad.Item.GridShow = 0.5f;
                Pad.Item.GridSnap = 0.5f;
            }
        }

        private void LoadTab_FileOk(object sender, CancelEventArgs e) => FormulaSet(LoadTab.FileName, null);

        #endregion
    }
}