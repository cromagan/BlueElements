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

#endregion BlueElements - a collection of useful tools, database and controls

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.MultiUserFile;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlueControls.BlueDatabaseDialogs {

    internal sealed partial class DatabaseHeadEditor {
        private Database _Database;
        private bool frmHeadEditor_FormClosing_isin;

        public DatabaseHeadEditor(Database cDatabase) {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            _Database = cDatabase;
            _Database.Disposing += Database_Disposing;
        }

        private void Database_Disposing(object sender, System.EventArgs e) {
            RemoveDatabase();
            Close();
        }

        private void RemoveDatabase() {
            if (_Database == null) { return; }
            _Database.Disposing -= Database_Disposing;
            _Database = null;
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            if (frmHeadEditor_FormClosing_isin) { return; }
            frmHeadEditor_FormClosing_isin = true;
            base.OnFormClosing(e);
            if (_Database != null) {
                WriteInfosBack();
                RemoveDatabase();
            }
        }

        private void WriteInfosBack() {
            if (_Database == null) { return; } // Disposed
            if (_Database.ReadOnly) { return; }
            scriptEditor.WriteScriptBack();
            _Database.GlobalShowPass = txbKennwort.Text;
            _Database.Caption = txbCaption.Text;
            _Database.UndoCount = tbxUndoAnzahl.Text.IsLong() ? Math.Max(int.Parse(tbxUndoAnzahl.Text), 5) : 5;
            _Database.ReloadDelaySecond = tbxReloadVerzoegerung.Text.IsLong() ? Math.Max(int.Parse(tbxReloadVerzoegerung.Text), 5) : 5;
            if (txbGlobalScale.Text.IsDouble()) {
                _Database.GlobalScale = Math.Min(double.Parse(txbGlobalScale.Text), 5);
                _Database.GlobalScale = Math.Max(0.5, _Database.GlobalScale);
            } else {
                _Database.ReloadDelaySecond = 1;
            }
            _Database.FilterImagePfad = txbFilterImagePath.Text;
            _Database.AdditionaFilesPfad = txbAdditionalFiles.Text;
            _Database.ZeilenQuickInfo = txbZeilenQuickInfo.Text.Replace("\r", "<br>");
            if (tbxTags.Text != _Database.Tags.JoinWithCr()) {
                _Database.Tags.Clear();
                _Database.Tags.AddRange(tbxTags.Text.SplitByCR());
            }
            if (DatenbankAdmin.Item.ToListOfString().IsDifferentTo(_Database.DatenbankAdmin)) {
                _Database.DatenbankAdmin.Clear();
                _Database.DatenbankAdmin.AddRange(DatenbankAdmin.Item.ToListOfString());
            }
            if (PermissionGroups_NewRow.Item.ToListOfString().IsDifferentTo(_Database.PermissionGroups_NewRow)) {
                _Database.PermissionGroups_NewRow.Clear();
                _Database.PermissionGroups_NewRow.AddRange(PermissionGroups_NewRow.Item.ToListOfString());
                _Database.PermissionGroups_NewRow.Remove("#Administrator");
            }
            _Database.JoinTyp = (enJoinTyp)int.Parse(cbxJoinTyp.Text);
            _Database.VerwaisteDaten = (enVerwaisteDaten)int.Parse(cbxVerwaisteDaten.Text);
            _Database.Ansicht = (enAnsicht)int.Parse(cbxAnsicht.Text);
            _Database.SortDefinition = new RowSortDefinition(_Database, lbxSortierSpalten.Item.ToListOfString(), btnSortRichtung.Checked);
            // Export ------------
            List<ExportDefinition> NewExports = new();
            foreach (var ThisItem in lbxExportSets.Item) {
                NewExports.Add((ExportDefinition)((TextListItem)ThisItem).Tag);
            }
            if (NewExports.IsDifferentTo(_Database.Export)) {
                _Database.Export.Clear();
                _Database.Export.AddRange(NewExports);
            }
        }

        protected override void OnLoad(System.EventArgs e) {
            base.OnLoad(e);
            cbxJoinTyp.Item.Clear();
            cbxJoinTyp.Item.AddRange(typeof(enJoinTyp));
            cbxJoinTyp.Text = ((int)_Database.JoinTyp).ToString();
            cbxVerwaisteDaten.Item.Clear();
            cbxVerwaisteDaten.Item.AddRange(typeof(enVerwaisteDaten));
            cbxVerwaisteDaten.Text = ((int)_Database.VerwaisteDaten).ToString();
            cbxAnsicht.Item.Clear();
            cbxAnsicht.Item.AddRange(typeof(enAnsicht));
            cbxAnsicht.Text = ((int)_Database.Ansicht).ToString();
            PermissionGroups_NewRow.Item.Clear();
            PermissionGroups_NewRow.Item.AddRange(_Database.PermissionGroups_NewRow);
            DatenbankAdmin.Item.Clear();
            DatenbankAdmin.Item.AddRange(_Database.DatenbankAdmin);
            txbKennwort.Text = _Database.GlobalShowPass;
            lbxSortierSpalten.Item.Clear();
            if (_Database.SortDefinition != null) {
                btnSortRichtung.Checked = _Database.SortDefinition.Reverse;
                if (_Database.SortDefinition.Columns != null) {
                    foreach (var ThisColumn in _Database.SortDefinition.Columns) {
                        if (ThisColumn != null) { lbxSortierSpalten.Item.Add(ThisColumn, false); }
                    }
                }
            }
            tbxTags.Text = _Database.Tags.JoinWithCr();
            // Exports ----------------
            lbxExportSets.Item.Clear();
            foreach (var ThisSet in _Database.Export) {
                if (ThisSet != null) {
                    lbxExportSets.Item.Add(ThisSet);
                }
            }
            lbxExportSets.Item.Sort();
            // -----------------------------
            txbCaption.Text = _Database.Caption;
            tbxReloadVerzoegerung.Text = _Database.ReloadDelaySecond.ToString();
            txbGlobalScale.Text = _Database.GlobalScale.ToString();
            txbFilterImagePath.Text = _Database.FilterImagePfad;
            txbAdditionalFiles.Text = _Database.AdditionaFilesPfad;
            txbZeilenQuickInfo.Text = _Database.ZeilenQuickInfo.Replace("<br>", "\r");
            tbxUndoAnzahl.Text = _Database.UndoCount.ToString();
            PermissionGroups_NewRow.Suggestions.Clear();
            PermissionGroups_NewRow.Suggestions.AddRange(_Database.Permission_AllUsed(true));
            DatenbankAdmin.Suggestions.Clear();
            DatenbankAdmin.Suggestions.AddRange(_Database.Permission_AllUsed(true));
            lbxSortierSpalten.Suggestions.Clear();
            lbxSortierSpalten.Suggestions.AddRange(_Database.Column, false, false, false);
            //foreach (var ThisColumnItem in _Database.Column)
            //{
            //    if (ThisColumnItem != null) { lbxSortierSpalten.Suggestions.Add(ThisColumnItem); }
            //}
            GenerateUndoTabelle();
            CryptStatus();
            GenerateInfoText();
        }

        private void OkBut_Click(object sender, System.EventArgs e) => Close();

        private void GenerateInfoText() {
            var t = "<b>Datei:</b><tab>" + _Database.Filename + "<br>";
            t = t + "<b>Zeilen:</b><tab>" + (_Database.Row.Count() - 1);
            capInfo.Text = t.TrimEnd("<br>");
        }

        #region Export

        private void ExportSets_AddClicked(object sender, System.EventArgs e) {
            var NewExportItem = lbxExportSets.Item.Add(new ExportDefinition(_Database));
            NewExportItem.Checked = true;
        }

        private void lbxExportSets_ItemCheckedChanged(object sender, System.EventArgs e) {
            if (lbxExportSets.Item.Checked().Count != 1) {
                ExportEditor.Item = null;
                return;
            }
            if (_Database.ReadOnly) {
                ExportEditor.Item = null;
                return;
            }
            var SelectedExport = (ExportDefinition)((TextListItem)lbxExportSets.Item.Checked()[0]).Tag;
            ExportEditor.Item = SelectedExport;
        }

        #endregion Export

        private void lbxExportSets_RemoveClicked(object sender, ListOfBasicListItemEventArgs e) {
            foreach (var thisitem in e.Items) {
                if (thisitem is BasicListItem ThisItemBasic) {
                    var tempVar = (ExportDefinition)((TextListItem)ThisItemBasic).Tag;
                    tempVar.DeleteAllBackups();
                }
            }
        }

        private void Bilder_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
            if (e.HotItem is not BitmapListItem) { return; }
            e.UserMenu.Add(enContextMenuComands.Umbenennen);
        }

        private void Bilder_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            if (e.HotItem == null) { return; }
            if (e.HotItem is not BitmapListItem) { return; }
            var l = (BitmapListItem)e.HotItem;
            switch (e.ClickedComand) {
                case "Umbenennen":
                    var n = InputBox.Show("<b><u>Bild umbenennen:</u></b><br><br>Achtung! Dadruch können Bezüge<br> in Texten und Spalten verlorengehen!", l.Caption, enDataFormat.Text);
                    if (!string.IsNullOrEmpty(n)) { l.Caption = n; }
                    break;

                default:
                    Develop.DebugPrint(e);
                    break;
            }
        }

        private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => _Database.Column.GenerateOverView();

        private void DateienSchlüssel_Click(object sender, System.EventArgs e) {
            btnDateiSchluessel.Enabled = false;
            btnDateiSchluessel.Text = "Dateien in Arbeit";
            var lLCase = _Database.AllConnectedFilesLCase();
            string NewKey;
            if (string.IsNullOrEmpty(_Database.FileEncryptionKey)) {
                NewKey = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz äöü#_-<>ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10).Select(s => s[Constants.GlobalRND.Next(s.Length)]).ToArray());
                foreach (var ThisFile in lLCase) {
                    var b = modConverter.FileToByte(ThisFile);
                    b = modAllgemein.SimpleCrypt(b, NewKey, 1);
                    FileOperations.DeleteFile(ThisFile, true);
                    modConverter.ByteToFile(ThisFile, b);
                }
            } else {
                NewKey = string.Empty;
                foreach (var ThisFile in lLCase) {
                    var b = modConverter.FileToByte(ThisFile);
                    b = modAllgemein.SimpleCrypt(b, _Database.FileEncryptionKey, -1);
                    FileOperations.DeleteFile(ThisFile, true);
                    modConverter.ByteToFile(ThisFile, b);
                }
            }
            _Database.FileEncryptionKey = NewKey;
            btnDateiSchluessel.Enabled = true;
            CryptStatus();
        }

        private void CryptStatus() {
            if (string.IsNullOrEmpty(_Database.FileEncryptionKey)) {
                btnDateiSchluessel.Text = "Dateien verschlüsseln";
                btnDateiSchluessel.QuickInfo = "Dazugehörige Dateien der Datenbank sind aktuell im Originalformat auf dem Laufwerk für jedem zugänglich.";
            } else {
                btnDateiSchluessel.Text = "Dateien freigeben";
                btnDateiSchluessel.QuickInfo = "Dazugehörige Dateien der Datenbank sind aktuell verschlüsselt.";
            }
        }

        private void btnFremdImport_Click(object sender, System.EventArgs e) {
            if (_Database.ReadOnly) { return; }
            WriteInfosBack();
            string GetFromFile;
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new() {
                CheckFileExists = true,
                Filter = "Datenbanken|*.mdb"
            };
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                GetFromFile = openFileDialog1.FileName;
            } else {
                return;
            }
            ItemCollectionList I = new()
            {
                { "Anordnungen der Spaltenansichten", ((int)enDatabaseDataType.ColumnArrangement).ToString() },
                { "Formulare", ((int)enDatabaseDataType.Views).ToString() },
                //I.Add("Regeln", ((int)enDatabaseDataType.Rules_ALT).ToString());
                { "Undo-Speicher", ((int)enDatabaseDataType.UndoInOne).ToString() },
                { "Auto-Export", ((int)enDatabaseDataType.AutoExport).ToString() },
                //{ "Binäre Daten im Kopf der Datenbank", ((int)enDatabaseDataType.BinaryDataInOne).ToString() },
                { "Eingebettete Layouts", ((int)enDatabaseDataType.Layouts).ToString() },
                { "Tags des Datenbankkopfes", ((int)enDatabaseDataType.Tags).ToString() },
                { "Standard-Sortierung", ((int)enDatabaseDataType.SortDefinition).ToString() }
            };
            I.Sort();
            var What = InputBoxComboStyle.Show("Welchen Code:", I, false);
            if (string.IsNullOrEmpty(What)) { return; }
            var B = clsMultiUserFile.UnzipIt(File.ReadAllBytes(GetFromFile));
            enDatabaseDataType Art = 0;
            var Pointer = 0;
            var ColKey = 0;
            var RowKey = 0;
            var X = 0;
            var Y = 0;
            var Inhalt = "";
            var Such = (enDatabaseDataType)int.Parse(What);
            do {
                if (Pointer > B.Length) { break; }
                _Database.Parse(B, ref Pointer, ref Art, ref ColKey, ref RowKey, ref Inhalt, ref X, ref Y);
                if (Such == Art) {
                    _Database.InjectCommand(Art, Inhalt);
                    //_Database.AddPending(Art, -1, -1, "", Inhalt, true);
                    MessageBox.Show("<b>Importiert:</b><br>" + Inhalt, enImageCode.Information, "OK");
                }
            } while (Art != enDatabaseDataType.EOF);
            RemoveDatabase();
            Close();
        }

        private void GenerateUndoTabelle() {
            Database x = new(true);
            x.Column.Add("hidden", "hidden", enDataFormat.Text);
            x.Column.Add("Index", "Index", enDataFormat.Ganzzahl);
            x.Column.Add("db", "Herkunft", enDataFormat.Text);
            x.Column.Add("ColumnKey", "Spalten-<br>Schlüssel", enDataFormat.Ganzzahl);
            x.Column.Add("ColumnName", "Spalten-<br>Name", enDataFormat.Text);
            x.Column.Add("ColumnCaption", "Spalten-<br>Beschriftung", enDataFormat.Text);
            x.Column.Add("RowKey", "Zeilen-<br>Schlüssel", enDataFormat.Ganzzahl);
            x.Column.Add("RowFirst", "Zeile, Wert der<br>1. Spalte", enDataFormat.Text);
            x.Column.Add("Aenderzeit", "Änder-<br>Zeit", enDataFormat.Datum_und_Uhrzeit);
            x.Column.Add("Aenderer", "Änderer", enDataFormat.Text);
            x.Column.Add("Symbol", "Symbol", enDataFormat.BildCode);
            x.Column.Add("Aenderung", "Änderung", enDataFormat.Text);
            x.Column.Add("WertAlt", "Wert alt", enDataFormat.Text);
            x.Column.Add("WertNeu", "Wert neu", enDataFormat.Text);
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
            x.ColumnArrangements[1].Hide("hidden");
            x.ColumnArrangements[1].HideSystemColumns();
            x.SortDefinition = new RowSortDefinition(x, "Index", true);
            tblUndo.Database = x;
            tblUndo.Arrangement = 1;
            for (var n = 0; n < _Database.Works.Count; n++) {
                AddUndoToTable(_Database.Works[n], n, string.Empty);
            }
        }

        private void AddUndoToTable(WorkItem work, int index, string db) {
            if (work.HistorischRelevant) {
                var l = tblUndo.Database.Row[work.ToString()];
                if (l != null) { return; }
                var cd = work.CellKey.SplitBy("|");
                _Database.Cell.DataOfCellKey(work.CellKey, out var Col, out var Row);
                var r = tblUndo.Database.Row.Add(work.ToString());
                r.CellSet("ColumnKey", cd[0]);
                r.CellSet("RowKey", cd[1]);
                r.CellSet("index", index);
                r.CellSet("db", db);
                if (Col != null) {
                    r.CellSet("ColumnName", Col.Name);
                    r.CellSet("columnCaption", Col.Caption);
                }
                if (Row != null) {
                    r.CellSet("RowFirst", Row.CellFirstString());
                } else if (cd[1] != "-1") {
                    r.CellSet("RowFirst", "[gelöscht]");
                }
                r.CellSet("Aenderer", work.User);
                r.CellSet("AenderZeit", work.CompareKey());
                var Symb = enImageCode.Fragezeichen;
                var alt = work.PreviousValue;
                var neu = work.ChangedTo;
                var aenderung = work.Comand.ToString();
                switch (work.Comand) {
                    case enDatabaseDataType.ce_UTF8Value_withoutSizeData:
                    case enDatabaseDataType.ce_Value_withoutSizeData:
                        Symb = enImageCode.Textfeld;
                        aenderung = "Wert geändert";
                        break;

                    case enDatabaseDataType.AutoExport:
                        aenderung = "Export ausgeführt oder geändert";
                        alt = "";
                        neu = "";
                        Symb = enImageCode.Karton;
                        break;

                    case enDatabaseDataType.dummyComand_AddRow:
                        aenderung = "Neue Zeile";
                        Symb = enImageCode.PlusZeichen;
                        break;

                    case enDatabaseDataType.RulesScript:
                    case enDatabaseDataType.Rules_ALT:
                        aenderung = "Regeln verändert";
                        Symb = enImageCode.Formel;
                        alt = "";
                        neu = "";
                        break;

                    case enDatabaseDataType.ColumnArrangement:
                        aenderung = "Spalten-Anordnungen verändert";
                        Symb = enImageCode.Spalte;
                        alt = "";
                        neu = "";
                        break;

                    case enDatabaseDataType.dummyComand_RemoveRow:
                        aenderung = "Zeile gelöscht";
                        Symb = enImageCode.MinusZeichen;
                        break;
                }
                r.CellSet("Aenderung", aenderung);
                r.CellSet("symbol", Symb + "|24");
                r.CellSet("Wertalt", alt);
                r.CellSet("Wertneu", neu);
            }
        }

        private void btnSperreAufheben_Click(object sender, System.EventArgs e) {
            tblUndo.Database.Export_HTML(@"C:\01_Data\texxxxst.html", tblUndo.Database.ColumnArrangements[0], tblUndo.SortedRows(), false);
            _Database.UnlockHard();
            MessageBox.Show("Erledigt.", enImageCode.Information, "OK");
        }

        private void ExportEditor_Changed(object sender, System.EventArgs e) {
            foreach (var thisitem in lbxExportSets.Item) {
                if (thisitem is TextListItem tli) {
                    if (tli.Tag == ExportEditor.Item) {
                        tli.Text = ExportEditor.Item.ReadableText();
                        tli.Symbol = ExportEditor.Item.SymbolForReadableText();
                    }
                }
            }
        }

        private void GlobalTab_Selecting(object sender, System.Windows.Forms.TabControlCancelEventArgs e) {
            if (e.TabPageIndex == 1) {
                scriptEditor.Database = _Database;
            }
        }

        private void btnSave_Click(object sender, System.EventArgs e) {
            var ok = false;
            if (_Database != null) {
                WriteInfosBack();
                ok = _Database.Save(false);
            }
            if (!ok) {
                MessageBox.Show("Speichern fehlgeschlagen!");
            }
        }

        private void btnClipboard_Click(object sender, System.EventArgs e) => System.Windows.Forms.Clipboard.SetDataObject(tblUndo.Export_CSV(enFirstRow.ColumnCaption), true);

        private void btnAlleUndos_Click(object sender, System.EventArgs e) {
            btnAlleUndos.Enabled = false;
            var L = tabAdministration.Vorgängervsersionen(_Database);
            if (L.Count < 1) {
                MessageBox.Show("Keine Vorgänger gefunden.");
                return;
            }
            L.CheckBehavior = enCheckBehavior.MultiSelection;
            var alle = InputBoxListBoxStyle.Show("Datenbaken, die geladen werden sollen, wählen:", L, enAddType.None, true);
            if (alle.Count < 1) {
                MessageBox.Show("Abbruch.");
                btnAlleUndos.Enabled = true;
                return;
            }
            var nDB = 0;
            var x = Progressbar.Show("Lade Vorgänger Datenbanken", alle.Count);
            foreach (var thisf in alle) {
                nDB++;
                x.Update(nDB);
                var db = (Database)Database.GetByFilename(thisf, false);
                var disp = db == null;
                if (db == null) {
                    db = new Database(thisf, true, false);
                }
                if (db.Caption == _Database.Caption) {
                    for (var n = 0; n < db.Works.Count; n++) {
                        AddUndoToTable(db.Works[n], n, db.Filename.FileNameWithoutSuffix());
                    }
                }
                if (disp) { db.Dispose(); }
            }
            x.Close();
        }

        private void tblUndo_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
            var bt = (Table)sender;
            var CellKey = e.Tags.TagGet("Cellkey");
            if (string.IsNullOrEmpty(CellKey)) { return; }
            bt.Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);
            e.UserMenu.Add("Sortierung", true);
            e.UserMenu.Add(enContextMenuComands.SpaltenSortierungAZ, Column != null && Column.Format.CanBeChangedByRules());
            e.UserMenu.Add(enContextMenuComands.SpaltenSortierungZA, Column != null && Column.Format.CanBeChangedByRules());
        }

        private void tblUndo_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            var bt = (Table)sender;
            var CellKey = e.Tags.TagGet("CellKey");
            if (string.IsNullOrEmpty(CellKey)) { return; }
            bt.Database.Cell.DataOfCellKey(CellKey, out var Column, out var Row);
            switch (e.ClickedComand) {
                case "SpaltenSortierungAZ":
                    bt.SortDefinitionTemporary = new RowSortDefinition(bt.Database, Column.Name, false);
                    break;

                case "SpaltenSortierungZA":
                    bt.SortDefinitionTemporary = new RowSortDefinition(bt.Database, Column.Name, true);
                    break;
            }
        }
    }
}