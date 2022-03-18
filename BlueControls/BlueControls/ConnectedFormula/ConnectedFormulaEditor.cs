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

namespace BlueControls.Forms {

    public partial class ConnectedFormulaEditor : BlueControls.Forms.PadEditor {

        #region Fields

        private ConnectedFormula.ConnectedFormula _cf;

        #endregion

        #region Constructors

        public ConnectedFormulaEditor(ConnectedFormula.ConnectedFormula cf) {
            InitializeComponent();
            _cf = cf;
        }

        #endregion

        #region Methods

        private void btnFeldHinzu_Click(object sender, System.EventArgs e) {
            var x = new EditFieldPadItem(_cf.NextID().ToString());

            Pad.Item.Add(x);
        }

        private void btnZeileHinzu_Click(object sender, System.EventArgs e) {
            var x = Directory.GetFiles(_cf.FilePath, "*.mdb").ToList();

            //x.RemoveRange(_cf.DatabaseFiles);

            if (x == null || x.Count == 0) {
                MessageBox.Show("Keine Datenbanken vorhanden.");
                return;
            }

            var rück = BlueControls.Forms.InputBoxListBoxStyle.Show("Datenbank wählen: ", x.ToList());

            if (rück == null) { return; }

            _cf.DatabaseFiles.Add(rück);

            var db = _cf.Databases[_cf.Databases.Count - 1];

            if (db != null) {
                var dbitem = new BlueControls.ItemCollection.RowWithFilterPaditem(db, _cf.Databases.Count - 1);
                Pad.Item.Add(dbitem);
            }
        }

        private void Pad_Changed(object sender, System.EventArgs e) {
            _cf.PadData = Pad.ToString();
        }

        #endregion
    }
}