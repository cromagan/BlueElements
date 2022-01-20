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
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueScript;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueControls {

    public partial class ScriptEditor : GroupBox, IContextMenu // System.Windows.Forms.UserControl//
    {
        #region Fields

        private string _LastVariableContent = string.Empty;
        private string _LastWord = string.Empty;
        private bool _MenuDone = false;
        private AutocompleteMenu _PopupMenu;

        #endregion

        #region Constructors

        public ScriptEditor() {
            InitializeComponent();
            GenerateVariableTable();
        }

        #endregion

        #region Events

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        #endregion

        #region Properties

        protected string ScriptText {
            get => txtSkript.Text;
            set { txtSkript.Text = value.TrimEnd(" ") + "    "; }
        }

        #endregion

        #region Methods

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
            switch (e.ClickedComand.ToLower()) {
                case "variableninhalt kopieren":
                    Generic.CopytoClipboard(_LastVariableContent);
                    return true;
            }

            return false;
        }

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollection.ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate) {
            if (!string.IsNullOrEmpty(_LastVariableContent)) {
                Items.Add("Variableninhalt kopieren");
            }

            HotItem = _LastWord;
        }

        public void Message(string txt) {
            txbSkriptInfo.Text = "[" + DateTime.Now.ToLongTimeString() + "] " + txt;
        }

        public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

        protected virtual Script GenerateAndDoScript() {
            var s = new Script(null);
            s.Parse();
            return s;
        }

        protected void WriteComandsToList(Script s) {
            lstComands.Item.Clear();
            if (s != null && Script.Comands != null) {
                foreach (var thisc in Script.Comands) {
                    lstComands.Item.Add(thisc, thisc.Syntax.ToLower());
                }
            }
            lstComands.Item.Sort();
            if (!_MenuDone) {
                _MenuDone = true;
                _PopupMenu = new AutocompleteMenu(txtSkript) {
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
                _PopupMenu.Items.SetAutocompleteItems(items);
            }
        }

        protected void WriteVariablesToTable(Script s) {
            if (s != null && s.Variablen != null) {
                foreach (var thisv in s.Variablen) {
                    var ro = tableVariablen.Database.Row.Add(thisv.Name);
                    ro.CellSet("typ", thisv.Type.ToString());
                    ro.CellSet("RO", thisv.Readonly);
                    ro.CellSet("System", thisv.SystemVariable);

                    var tmpi = thisv.ValueString;
                    if (tmpi.Length > 500) { tmpi = tmpi.Substring(0, 500) + "..."; }

                    ro.CellSet("Inhalt", tmpi);
                    ro.CellSet("Kommentar", thisv.Coment);
                }
            }
        }

        private void btnTest_Click(object sender, System.EventArgs e) {
            Message("Starte Skript");
            tableVariablen.Database.Row.Clear();

            var s = GenerateAndDoScript();

            WriteVariablesToTable(s);
            WriteComandsToList(s);

            if (string.IsNullOrEmpty(s.Error)) {
                Message("Erfolgreich, wenn auch IF-Routinen nicht geprüft wurden.");
            } else {
                Message("Fehler in Zeile: " + s.Line.ToString() + "\r\n" + s.Error + "\r\n >>> " + s.ErrorCode.RestoreEscape());
            }
        }

        private void GenerateVariableTable() {
            Database x = new(true);
            x.Column.Add("Name", "Name", enVarType.Text);
            x.Column.Add("Typ", "Typ", enVarType.Text);
            x.Column.Add("RO", "Schreibgeschützt", enVarType.Text);
            x.Column.Add("System", "Systemspalte", enVarType.Text);
            x.Column.Add("Inhalt", "Inhalt", enVarType.Text);
            x.Column.Add("Kommentar", "Kommentar", enVarType.Text);
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

        private void TxtSkript_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
            }
        }

        private void txtSkript_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) {
            try {
                _LastWord = string.Empty;
                _LastVariableContent = string.Empty;
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
                _LastWord = hoveredWordnew;

                foreach (var r in tableVariablen.Database.Row) {
                    if (r.CellFirstString().ToLower() == hoveredWordnew.ToLower()) {
                        var inh = r.CellGetString("Inhalt");
                        _LastVariableContent = inh;
                        inh = inh.Replace("\r", ";");
                        inh = inh.Replace("\n", ";");
                        if (inh.Length > 25) { inh = inh.Substring(0, 20) + "..."; }
                        var ro = string.Empty;
                        if (r.CellGetBoolean("RO")) { ro = "[ReadOnly] "; }

                        e.ToolTipTitle = ro + "(" + r.CellGetString("Typ") + ") " + hoveredWordnew + " = " + inh;
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