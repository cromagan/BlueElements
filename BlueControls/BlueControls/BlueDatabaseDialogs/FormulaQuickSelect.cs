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
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.BlueDatabaseDialogs {

    public partial class FormulaQuickSelect {

        #region Fields

        private readonly RowItem Row;

        #endregion

        #region Constructors

        public FormulaQuickSelect(RowItem RowItem) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Row = RowItem;
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
            var x = e.Item.Internal.SplitBy("|");
            if (Row.Database.Column[x[0]].MultiLine) {
                var val = Row.CellGetList(Row.Database.Column[x[0]]);
                if (e.Item.Checked) {
                    val.AddIfNotExists(x[1]);
                } else {
                    val.Remove(x[1]);
                }
                Row.CellSet(Row.Database.Column[x[0]], val);
            } else {
                if (e.Item.Checked) {
                    Row.CellSet(Row.Database.Column[x[0]], x[1]);
                }
            }
            //       End If
            Such_TextChanged(null, System.EventArgs.Empty);
        }

        private void Init() {
            if (Row == null) {
                Close();
                return;
            }
            Für.Text = "<b>" + Row.CellFirstString();
        }

        private void Such_TextChanged(object sender, System.EventArgs e) {
            Auswahl.Item.Clear();
            var t = Such.Text;
            if (string.IsNullOrEmpty(t)) { return; }
            t = t.ToLower();
            foreach (var ThisColumn in Row.Database.Column) {
                if (ThisColumn != null) {
                    if (ThisColumn.EditType is enEditTypeFormula.SwapListBox or enEditTypeFormula.Listbox or enEditTypeFormula.Textfeld_mit_Auswahlknopf) {
                        if (ThisColumn.DropdownBearbeitungErlaubt) {
                            if (CellCollection.UserEditPossible(ThisColumn, Row, enErrorReason.OnlyRead)) {
                                var ThisView = Formula.SearchColumnView(ThisColumn);
                                if (ThisView != null) {
                                    if (Row.Database.PermissionCheck(ThisView.PermissionGroups_Show, null)) {
                                        ItemCollectionList dummy = new();
                                        ItemCollectionList.GetItemCollection(dummy, ThisColumn, Row, enShortenStyle.Replaced, 1000);
                                        if (dummy.Count > 0) {
                                            foreach (var thisItem in dummy) {
                                                if (thisItem.Internal.ToLower().Contains(t)) {
                                                    var ni = Auswahl.Item.Add(ThisColumn.ReadableText() + ": " + thisItem.Internal, ThisColumn.Name.ToUpper() + "|" + thisItem.Internal);
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
        }

        #endregion
    }
}