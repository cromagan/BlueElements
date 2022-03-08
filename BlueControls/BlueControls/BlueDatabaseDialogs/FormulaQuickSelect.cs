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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.BlueDatabaseDialogs {

    public partial class FormulaQuickSelect {

        #region Fields

        private readonly RowItem? _row;

        #endregion

        #region Constructors

        public FormulaQuickSelect(RowItem? rowItem) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _row = rowItem;
        }

        #endregion

        #region Methods

        //Private Sub Auswahl_Item_CheckedChanged(sender As Object) Handles Auswahl.Item_CheckedChanged
        //End Sub
        protected override void OnLoad(System.EventArgs e) {
            base.OnLoad(e);
            Init();
        }

        private void Auswahl_ItemClicked(object sender, BasicListItemEventArgs e) {
            var x = e.Item.Internal.SplitAndCutBy("|");
            if (_row.Database.Column[x[0]].MultiLine) {
                var val = _row.CellGetList(_row.Database.Column[x[0]]);
                if (e.Item.Checked) {
                    val.AddIfNotExists(x[1]);
                } else {
                    val.Remove(x[1]);
                }
                _row.CellSet(_row.Database.Column[x[0]], val);
            } else {
                if (e.Item.Checked) {
                    _row.CellSet(_row.Database.Column[x[0]], x[1]);
                }
            }
            //       End If
            Such_TextChanged(null, System.EventArgs.Empty);
        }

        private void Init() {
            if (_row == null) {
                Close();
                return;
            }
            Für.Text = "<b>" + _row.CellFirstString();
        }

        private void Such_TextChanged(object? sender, System.EventArgs e) {
            Auswahl.Item.Clear();
            var t = Such.Text;
            if (string.IsNullOrEmpty(t)) { return; }
            t = t.ToLower();
            foreach (var thisColumn in _row.Database.Column) {
                if (thisColumn?.EditType is enEditTypeFormula.SwapListBox or enEditTypeFormula.Listbox or enEditTypeFormula.Textfeld_mit_Auswahlknopf) {
                    if (thisColumn.DropdownBearbeitungErlaubt) {
                        if (CellCollection.UserEditPossible(thisColumn, _row, enErrorReason.OnlyRead)) {
                            var thisView = Formula.SearchColumnView(thisColumn);
                            if (thisView != null) {
                                if (_row.Database.PermissionCheck(thisView.PermissionGroups_Show, null)) {
                                    ItemCollectionList dummy = new();
                                    ItemCollectionList.GetItemCollection(dummy, thisColumn, _row, enShortenStyle.Replaced, 1000);
                                    if (dummy.Count > 0) {
                                        foreach (var thisItem in dummy) {
                                            if (thisItem.Internal.ToLower().Contains(t)) {
                                                var ni = Auswahl.Item.Add(thisColumn.ReadableText() + ": " + thisItem.Internal, thisColumn.Name.ToUpper() + "|" + thisItem.Internal);
                                                ni.Checked = thisItem.Checked;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}