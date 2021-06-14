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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueScript;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueControls {

    public partial class ScriptEditor : GroupBox // System.Windows.Forms.UserControl
    {
        #region Fields

        private Database _Database = null;
        private bool menuDone = false;
        private AutocompleteMenu popupMenu;

        #endregion

        #region Constructors

        public ScriptEditor() {
            InitializeComponent();
            GenerateVariableTable();
        }

        #endregion

        #region Properties

        public Database Database {
            get => _Database;
            set {
                if (_Database != null) {
                    _Database.RulesScript = txtSkript.Text;
                    _Database.Disposing -= _Database_Disposing;
                }
                _Database = value;
                if (_Database != null) {
                    txtSkript.Text = _Database.RulesScript;
                    _Database.Disposing += _Database_Disposing;
                }
            }
        }

        #endregion

        #region Methods

        internal void WriteScriptBack() {
            if (_Database == null) { return; }
            _Database.RulesScript = txtSkript.Text;
        }

        private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;

        private void btnTest_Click(object sender, System.EventArgs e) {
            if (_Database == null) {
                MessageBox.Show("Keine Datenbank geladen.", enImageCode.Information, "OK");
                return;
            }
            _Database.RulesScript = txtSkript.Text;
            txbSkriptInfo.Text = string.Empty;
            tableVariablen.Database.Row.Clear();
            if (_Database.Row.Count == 0) {
                MessageBox.Show("Zum Test wird zumindest eine Zeile benötigt.", enImageCode.Information, "OK");
                return;
            }
            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = _Database.Row.First().CellFirstString();
            }
            var r = _Database.Row[txbTestZeile.Text];
            if (r == null) {
                MessageBox.Show("Zeile nicht gefunden.", enImageCode.Information, "OK");
                return;
            }
            (var _, var message, var s) = r.DoAutomatic(true, "script testing");
            if (s != null && s.Variablen != null) {
                foreach (var thisv in s.Variablen) {
                    var ro = tableVariablen.Database.Row.Add(thisv.Name);
                    ro.CellSet("typ", thisv.Type.ToString());
                    ro.CellSet("RO", thisv.Readonly);
                    ro.CellSet("System", thisv.SystemVariable);
                    ro.CellSet("Inhalt", thisv.ValueString);
                    ro.CellSet("Kommentar", thisv.Coment);
                }
            }
            lstComands.Item.Clear();
            if (s != null && Script.Comands != null) {
                foreach (var thisc in Script.Comands) {
                    lstComands.Item.Add(thisc, thisc.Syntax.ToLower());
                }
            }
            lstComands.Item.Sort();
            if (!menuDone) {
                menuDone = true;
                popupMenu = new AutocompleteMenu(txtSkript) {
                    //popupMenu.Items.ImageList = imageList1;
                    SearchPattern = @"[\w\.:=!<>]",
                    AllowTabKey = true
                };
                List<AutocompleteItem> items = new();
                if (s != null && Script.Comands != null) {
                    foreach (var thisc in Script.Comands) {
                        items.Add(new SnippetAutocompleteItem(thisc.Syntax + " "));
                        if (thisc.Returns != Skript.Enums.enVariableDataType.Null) {
                            items.Add(new SnippetAutocompleteItem("var " + thisc.Returns.ToString() + " = " + thisc.Syntax + "; "));
                        }
                        foreach (var thiscom in thisc.Comand(s)) {
                            items.Add(new AutocompleteItem(thiscom));
                        }
                    }
                }
                //set as autocomplete source
                popupMenu.Items.SetAutocompleteItems(items);
            }
            if (!string.IsNullOrEmpty(message)) {
                txbSkriptInfo.Text = "[" + DateTime.Now.ToLongTimeString() + "] Allgemeiner Fehler: " + message;
                return;
            }
            txbSkriptInfo.Text = string.IsNullOrEmpty(s.Error)
                ? "[" + DateTime.Now.ToLongTimeString() + "] Erfolgreich, wenn auch IF-Routinen nicht geprüft wurden."
                : "[" + DateTime.Now.ToLongTimeString() + "] Fehler in Zeile: " + s.Line.ToString() + "\r\n" + s.Error + "\r\n >>> " + s.ErrorCode;
        }

        private void GenerateVariableTable() {
            Database x = new(true);
            x.Column.Add("Name", "Name", enDataFormat.Text);
            x.Column.Add("Typ", "Typ", enDataFormat.Text);
            x.Column.Add("RO", "Schreibgeschützt", enDataFormat.Bit);
            x.Column.Add("System", "Systemspalte", enDataFormat.Bit);
            x.Column.Add("Inhalt", "Inhalt", enDataFormat.Text);
            x.Column.Add("Kommentar", "Kommentar", enDataFormat.Text);
            foreach (var ThisColumn in x.Column) {
                if (string.IsNullOrEmpty(ThisColumn.Identifier)) {
                    ThisColumn.MultiLine = true;
                    ThisColumn.TextBearbeitungErlaubt = false;
                    ThisColumn.DropdownBearbeitungErlaubt = false;
                    ThisColumn.BildTextVerhalten = enBildTextVerhalten.Bild_oder_Text;
                }
            }
            x.RepairAfterParse();
            x.ColumnArrangements[1].ShowAllColumns();
            x.ColumnArrangements[1].HideSystemColumns();
            x.SortDefinition = new RowSortDefinition(x, "Name", true);
            tableVariablen.Database = x;
            tableVariablen.Arrangement = 1;
            filterVariablen.Table = tableVariablen;
        }

        private void lstComands_ItemClicked(object sender, BasicListItemEventArgs e) {
            var co = string.Empty;
            if (e.Item.Tag is Method thisc) {
                co += thisc.HintText();
            }
            txbComms.Text = co;
        }

        private void txtSkript_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) {
            try {
                foreach (var thisc in lstComands.Item) {
                    if (thisc.Tag is Method m) {
                        if (m.Comand(null).Contains(e.HoveredWord, false)) {
                            e.ToolTipTitle = m.Syntax;
                            e.ToolTipText = m.HintText();
                            return;
                        }
                    }
                }
                //x.Column.Add("Name", "Name", enDataFormat.Text);
                //x.Column.Add("Typ", "Typ", enDataFormat.Text);
                //x.Column.Add("RO", "Schreibgeschützt", enDataFormat.Bit);
                //x.Column.Add("System", "Systemspalte", enDataFormat.Bit);
                //x.Column.Add("Inhalt", "Inhalt", enDataFormat.Text);
                //x.Column.Add("Kommentar", "Kommentar", enDataFormat.Text);
                var hoveredWordnew = new Range(txtSkript, e.Place, e.Place).GetFragment("[A-Za-z0-9_]").Text;
                foreach (var r in tableVariablen.Database.Row) {
                    if (r.CellFirstString().ToLower() == hoveredWordnew.ToLower()) {
                        var inh = r.CellGetString("Inhalt");
                        inh = inh.Replace("\r", ";");
                        inh = inh.Replace("\n", ";");
                        inh = inh.Replace("\"", string.Empty);
                        if (inh.Length > 25) { inh = inh.Substring(0, 20) + "..."; }
                        e.ToolTipTitle = "(" + r.CellGetString("Typ") + ") " + hoveredWordnew + " = " + inh;
                        e.ToolTipText = r.CellGetString("Kommentar") + " ";
                        return;
                    }
                }
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }

        #endregion
    }
}