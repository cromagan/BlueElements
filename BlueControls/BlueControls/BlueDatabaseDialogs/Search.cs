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
using BlueControls.Enums;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.EventArgs;

namespace BlueControls.BlueDatabaseDialogs {

    public sealed partial class Search : BlueControls.Forms.Form {

        private readonly Table _BlueTable;


        private RowItem _row = null;
        private ColumnItem _col = null;

        public Search(Table table) {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _BlueTable = table;
            _BlueTable.CursorPosChanged += CursorPosChanged;
            CursorPosChanged(_BlueTable, new CellEventArgs(_BlueTable.CursorPosColumn(), _BlueTable.CursorPosRow()));

        }




        private void CursorPosChanged(object sender, CellEventArgs e) {

            _row = e.Row;
            _col = e.Column;
        }





        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            base.OnFormClosing(e);
            _BlueTable.CursorPosChanged -= CursorPosChanged;
        }

        private void btnSuchSpalte_Click(object sender, System.EventArgs e) {

            if (_BlueTable.Design == enBlueTableAppearance.OnlyMainColumnWithoutHead) {
                MessageBox.Show("In dieser Ansicht nicht möglich", enImageCode.Information, "OK");
                return;
            }

            var searchT = SuchText();
            if (string.IsNullOrEmpty(searchT)) { return; }

            var found = _col;
            var ca = _BlueTable?.CurrentArrangement;
            if (found == null) { found = _BlueTable.Database.Column.SysLocked; }
            var columnStarted = _col;

            do {
                found = ca.NextVisible(found);
                if (found == null) { found = ca[0].Column; }

                var _Ist1 = found.ReadableText().ToLower();

                if (!string.IsNullOrEmpty(_Ist1)) {

                    // Allgemeine Prüfung
                    if (_Ist1.Contains(searchT.ToLower())) { break; }

                    if (btnAehnliches.Checked) {
                        var _Ist3 = _Ist1.StarkeVereinfachung(" ,");
                        var _searchTXT3 = searchT.StarkeVereinfachung(" ,");
                        if (!string.IsNullOrEmpty(_Ist3) && _Ist3.ToLower().Contains(_searchTXT3.ToLower())) {
                            break;
                        }
                    }
                }


                if (columnStarted == found) {
                    found = null;
                    break;
                }

            } while (true);

            if (found == null) {
                MessageBox.Show("Text in den Spalten nicht gefunden.", enImageCode.Information, "OK");
                return;
            }

            _BlueTable.CursorPos_Set(found, _row, true);

            txbSuchText.Focus();
        }

        private string SuchText() {
            var SuchtT = txbSuchText.Text.Trim();

            if (string.IsNullOrEmpty(SuchtT)) {
                MessageBox.Show("Bitte Text zum Suchen eingeben.", enImageCode.Information, "OK");
                return string.Empty;
            }


            return SuchtT.Replace(";cr;", "\r").Replace(";tab;", "\t").ToLower();
        }
        private void btnSuchInCell_Click(object sender, System.EventArgs e) {

            var SuchtT = SuchText();

            if (string.IsNullOrEmpty(SuchtT)) { return; }


            Table.SearchNextText(SuchtT, _BlueTable, _col, _row, out var found, out var GefRow, btnAehnliches.Checked);


            if (found == null) {
                MessageBox.Show("Text nicht gefunden", enImageCode.Information, "OK");
                return;
            }

            _BlueTable.CursorPos_Set(found, GefRow, true);

            txbSuchText.Focus();

        }

        private void Search_Load(object sender, System.EventArgs e) {
            txbSuchText.Focus();
        }

        private void txbSuchText_TextChanged(object sender, System.EventArgs e) {

        }

        private void txbSuchText_Enter(object sender, System.EventArgs e) {
            btnSuchInCell_Click(null, System.EventArgs.Empty);
        }
    }
}
