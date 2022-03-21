﻿// Authors:
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueScript;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Linq;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueScript.Variables;
using static BlueBasics.Extensions;

namespace BlueControls {

    public partial class ScriptEditor : GroupBox, IContextMenu, IDisposable // System.Windows.Forms.UserControl, IContextMenu//
    {
        #region Fields

        private Befehlsreferenz? _befehlsReferenz;
        private string _lastVariableContent = string.Empty;
        private string? _lastWord = string.Empty;
        private bool _menuDone;
        private AutocompleteMenu _popupMenu;

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
            get => txtSkript.Text.TrimEnd(" ");
            set {
                txtSkript.Text = value.TrimEnd(" ") + "    ";

                UpdateSubs(Script.ReduceText(value));
            }
        }

        #endregion

        #region Methods

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
            switch (e.ClickedComand.ToLower()) {
                case "variableninhalt kopieren":
                    Generic.CopytoClipboard(_lastVariableContent);
                    return true;
            }

            return false;
        }

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs? e, ItemCollectionList? items, out object? hotItem, List<string> tags, ref bool cancel, ref bool translate) {
            if (!string.IsNullOrEmpty(_lastVariableContent)) {
                items.Add("Variableninhalt kopieren");
            }

            hotItem = _lastWord;
        }

        public void Message(string txt) => txbSkriptInfo.Text = "[" + DateTime.Now.ToLongTimeString() + "] " + txt;

        public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                components?.Dispose();

                if (_befehlsReferenz != null && _befehlsReferenz.Visible) {
                    _befehlsReferenz.Close();
                    _befehlsReferenz.Dispose();
                    _befehlsReferenz = null;
                }
            }
            base.Dispose(disposing);
        }

        protected virtual Script? GenerateAndDoScript() {
            var s = new Script(null, string.Empty);
            s.Parse();
            return s;
        }

        protected virtual void OpenAdditionalFileFolder() { }

        protected void WriteComandsToList(Script s) {
            if (!_menuDone) {
                _menuDone = true;
                _popupMenu = new AutocompleteMenu(txtSkript) {
                    //popupMenu.Items.ImageList = imageList1;
                    SearchPattern = @"[\w\.:=!<>]",
                    AllowTabKey = true
                };
                List<AutocompleteItem> items = new();
                if (s != null && Script.Comands != null) {
                    foreach (var thisc in Script.Comands) {
                        items.Add(new SnippetAutocompleteItem(thisc.Syntax + " "));
                        if (thisc.Returns != BlueScript.Enums.VariableDataType.Null) {
                            items.Add(new SnippetAutocompleteItem("var " + thisc.Returns + " = " + thisc.Syntax + "; "));
                        }

                        items.AddRange(thisc.Comand(s).Select(thiscom => new AutocompleteItem(thiscom)));
                    }
                }
                //set as autocomplete source
                _popupMenu.Items.SetAutocompleteItems(items);
            }
        }

        protected void WriteVariablesToTable(Script s) {
            if (s?.Variables != null) {
                foreach (var thisv in s.Variables) {
                    var ro = tableVariablen.Database.Row.Add(thisv.Name);
                    ro.CellSet("typ", thisv.ShortName);
                    ro.CellSet("RO", thisv.Readonly);
                    ro.CellSet("System", thisv.SystemVariable);

                    var tmpi = thisv.ReadableText;
                    if (tmpi.Length > 500) { tmpi = tmpi.Substring(0, 500) + "..."; }

                    ro.CellSet("Inhalt", tmpi);
                    ro.CellSet("Kommentar", thisv.Coment);
                }
            }
        }

        private void btnBefehlsUebersicht_Click(object sender, System.EventArgs e) {
            if (_befehlsReferenz != null && _befehlsReferenz.Visible) {
                _befehlsReferenz.Close();
                _befehlsReferenz.Dispose();
                _befehlsReferenz = null;
            }

            _befehlsReferenz = new Befehlsreferenz();
            _befehlsReferenz.Show();
        }

        private void btnTest_Click(object sender, System.EventArgs e) {
            Message("Starte Skript");
            tableVariablen.Database?.Row.Clear();

            var s = GenerateAndDoScript();

            if (s == null) {
                //Message("Interner Fehler. Skript nicht ausgeführt.");
                return;
            }

            WriteVariablesToTable(s);
            WriteComandsToList(s);
            UpdateSubs(s?.ReducedScriptText);

            if (string.IsNullOrEmpty(s.Error)) {
                Message("Erfolgreich, wenn auch IF-Routinen nicht geprüft wurden.");
            } else {
                Message("Fehler in Zeile: " + s.Line + "\r\n" + s.Error + "\r\n >>> " + s.ErrorCode.RestoreEscape());
            }
        }

        private void btnZusatzDateien_Click(object sender, System.EventArgs e) {
            OpenAdditionalFileFolder();
        }

        private void GenerateVariableTable() {
            Database x = new(true);
            x.Column.Add("Name", "N", VarType.Text, "Variablenname");
            x.Column.Add("Typ", "T", VarType.Text, "Variablentyp");
            x.Column.Add("RO", "R", VarType.Bit, "Readonly, Schreibgeschützt");
            x.Column.Add("System", "S", VarType.Bit, "Systemspalte\r\nIm Script nicht verfügbar");
            x.Column.Add("Inhalt", "I", VarType.Text, "Inhalt (gekürzte Ansicht)");
            x.Column.Add("Kommentar", "K", VarType.Text, "Komentar");

            foreach (var thisColumn in x.Column.Where(thisColumn => string.IsNullOrEmpty(thisColumn.Identifier))) {
                thisColumn.MultiLine = true;
                thisColumn.TextBearbeitungErlaubt = false;
                thisColumn.DropdownBearbeitungErlaubt = false;
            }

            x.RepairAfterParse();
            x.ColumnArrangements[1].ShowAllColumns();
            x.ColumnArrangements[1].HideSystemColumns();
            x.SortDefinition = new RowSortDefinition(x, "Name", true);
            tableVariablen.Database = x;
            tableVariablen.Arrangement = 1;
            filterVariablen.Table = tableVariablen;
        }

        private void lstFunktionen_ItemClicked(object sender, BasicListItemEventArgs e) {
            if (e.Item.Internal == "[Main]") {
                txtSkript.Navigate(0);
                return;
            }

            var sc = Script.ReduceText(txtSkript.Text).ToLower();
            var x = new List<string> { "sub" + e.Item.Internal.ToLower() + "()" };

            var (pos, _) = NextText(sc, 0, x, true, false, KlammernStd);
            if (pos < 1) {
                MessageBox.Show("Routine " + e.Item.Internal + " nicht gefunden.<br>Skript starten, um diese<br>Liste zu aktualisieren.", ImageCode.Information, "OK");
                return;
            }

            txtSkript.Navigate(sc.Substring(0, pos).Count(c => c == '¶') + 1);
        }

        private void TxtSkript_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
            }
        }

        private void txtSkript_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) {
            if (Script.Comands == null) { return; }

            try {
                _lastWord = string.Empty;
                _lastVariableContent = string.Empty;
                foreach (var thisc in Script.Comands.Where(thisc => thisc.Comand(null).Contains(e.HoveredWord, false))) {
                    e.ToolTipTitle = thisc.Syntax;
                    e.ToolTipText = thisc.HintText();
                    return;
                }
                var hoveredWordnew = new Range(txtSkript, e.Place, e.Place).GetFragment("[A-Za-z0-9_]").Text;
                _lastWord = hoveredWordnew;

                foreach (var r in tableVariablen.Database.Row) {
                    if (string.Equals(r.CellFirstString(), hoveredWordnew, StringComparison.CurrentCultureIgnoreCase)) {
                        var inh = r.CellGetString("Inhalt");
                        _lastVariableContent = inh;
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

        private void UpdateSubs(string s) {
            lstFunktionen.Item.Clear();

            lstFunktionen.Item.Add("[Main]");

            if (s == null) { return; }

            var st = new List<string> { "sub" };
            var en = new List<string> { "()" };

            var pos = 0;
            do {
                var (stp, _) = NextText(s, pos, st, true, false, KlammernStd);

                if (stp > 0) {
                    var (endp, _) = NextText(s, stp, en, false, false, KlammernStd);

                    if (endp > stp) {
                        var n = s.Substring(stp + 3, endp - stp - 3);
                        if (!Variable.IsValidName(n)) { break; }
                        lstFunktionen.Item.Add(n);
                        pos = endp + 2;
                    } else {
                        break;
                    }
                } else {
                    break;
                }
            } while (true);
        }

        #endregion
    }
}