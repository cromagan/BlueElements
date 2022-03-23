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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BlueBasics.FileOperations;
using BlueControls.ConnectedFormula;
using static BlueBasics.Extensions;
using BlueControls.ItemCollection;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;

namespace BlueControls.Forms {

    public partial class ConnectedFormulaEditor : PadEditor {

        #region Fields

        private ConnectedFormula.ConnectedFormula _cf;
        private bool _creating = false;

        #endregion

        #region Constructors

        public ConnectedFormulaEditor(ConnectedFormula.ConnectedFormula cf) {
            InitializeComponent();
            _creating = true;
            _cf = cf;
            Pad.Item = new ItemCollectionPad(cf.PadData, string.Empty);
            _creating = false;
        }

        #endregion

        #region Methods

        private void btnFeldHinzu_Click(object sender, System.EventArgs e) {
            var l = Pad.LastClickedItem;

            var x = new EditFieldPadItem(_cf.NextID().ToString());

            if (l is RowWithFilterPaditem ri) {
                x.GetValueFrom = ri;
            }
            if (l is EditFieldPadItem efi && efi.GetValueFrom != null) {
                x.GetValueFrom = efi.GetValueFrom;
            }

            Pad.Item.Add(x);

            if (x.GetValueFrom != null && x.GetValueFrom.Database != null) {
                x.Spalte_wählen = string.Empty; // Dummy setzen
            }
        }

        private void btnKonstante_Click(object sender, System.EventArgs e) {
            var x = new ConstantTextPaditem();
            x.Bei_Export_Sichtbar = false;
            Pad.Item.Add(x);
        }

        private void btnZeileHinzu_Click(object sender, System.EventArgs e) {
            var x = Directory.GetFiles(_cf.FilePath, "*.mdb").ToList();

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
                Pad.Item.Add(dbitem);
            }
        }

        private void Pad_Changed(object sender, System.EventArgs e) {
            if (_creating) { return; }

            _creating = true;
            Pad.Item.Sort();
            _cf.PadData = Pad.Item.ToString();
            _creating = false;
        }

        #endregion
    }
}