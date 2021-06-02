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
using BlueBasics.MultiUserFile;
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
            _Database.Disposing += _Database_Disposing;

        }

        private void _Database_Disposing(object sender, System.EventArgs e) {
            RemoveDatabase();
            Close();

        }

        private void RemoveDatabase() {
            if (_Database == null) { return; }
            _Database.Disposing -= _Database_Disposing;
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



            if (tbxUndoAnzahl.Text.IsLong()) {
                _Database.UndoCount = Math.Max(int.Parse(tbxUndoAnzahl.Text), 5);
            } else {
                _Database.UndoCount = 5;
            }



            if (tbxReloadVerzoegerung.Text.IsLong()) {
                _Database.ReloadDelaySecond = Math.Max(int.Parse(tbxReloadVerzoegerung.Text), 5);
            } else {
                _Database.ReloadDelaySecond = 5;
            }

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
            var NewExports = new List<ExportDefinition>();
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


        private void OkBut_Click(object sender, System.EventArgs e) {
            Close();
        }


        private void GenerateInfoText() {
            var t = "<b>Datei:</b><tab>" + _Database.Filename + "<br>";
            t = t + "<b>Zeilen:</b><tab>" + (_Database.Row.Count() - 1);
            capInfo.Text = t.TrimEnd("<br>");
        }

        #region  Export 

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





        #endregion


        private void lbxExportSets_RemoveClicked(object sender, ListOfBasicListItemEventArgs e) {
            foreach (var thisitem in e.Items) {
                if (thisitem is BasicListItem ThisItemBasic) {
                    var tempVar = (ExportDefinition)((TextListItem)ThisItemBasic).Tag;
                    tempVar.DeleteAllBackups();
                }
            }
        }



        private void Bilder_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
            if (e.HotItem == null) { return; }
            if (!(e.HotItem is BitmapListItem)) { return; }
            e.UserMenu.Add(enContextMenuComands.Umbenennen);
        }


        private void Bilder_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {

            if (e.HotItem == null) { return; }

            if (!(e.HotItem is BitmapListItem)) { return; }

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

        private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) {
            _Database.Column.GenerateOverView();
        }

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
            var openFileDialog1 = new System.Windows.Forms.OpenFileDialog {
                CheckFileExists = true,
                Filter = "Datenbanken|*.mdb"
            };
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                GetFromFile = openFileDialog1.FileName;
            } else {
                return;
            }

            var I = new ItemCollectionList
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

            var x = new Database(true);
            x.Column.Add("Index", "Index", enDataFormat.Ganzzahl);
            x.Column.Add("ColumnKey", "Spalten-<br>Schlüssel", enDataFormat.Ganzzahl);
            x.Column.Add("ColumnName", "Spalten-<br>Name", enDataFormat.Text);
            x.Column.Add("ColumnCaption", "Spalten-<br>Beschriftung", enDataFormat.Text);
            x.Column.Add("RowKey", "Zeilen-<br>Schlüssel", enDataFormat.Ganzzahl);
            x.Column.Add("RowFirst", "Zeile, Wert der<br>1. Spalte", enDataFormat.Text);
            x.Column.Add("Aenderzeit", "Änder-<br>Zeit", enDataFormat.Text);
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
            x.ColumnArrangements[1].HideSystemColumns();


            x.SortDefinition = new RowSortDefinition(x, "Index", true);



            for (var n = 0; n < Math.Min(_Database.Works.Count, 3000); n++) {


                if (_Database.Works[n].HistorischRelevant) {

                    var cd = _Database.Works[n].CellKey.SplitBy("|");


                    _Database.Cell.DataOfCellKey(_Database.Works[n].CellKey, out var Col, out var Row);

                    var r = x.Row.Add(n.ToString());

                    r.CellSet("ColumnKey", cd[0]);
                    r.CellSet("RowKey", cd[1]);



                    if (Col != null) {
                        r.CellSet("ColumnName", Col.Name);
                        r.CellSet("columnCaption", Col.Caption);
                    }


                    if (Row != null) {
                        r.CellSet("RowFirst", Row.CellFirstString());
                    } else if (cd[1] != "-1") {
                        r.CellSet("RowFirst", "[gelöscht]");
                    }



                    r.CellSet("Aenderer", _Database.Works[n].User);
                    r.CellSet("AenderZeit", _Database.Works[n].CompareKey());



                    var Symb = enImageCode.Fragezeichen;
                    var alt = _Database.Works[n].PreviousValue;
                    var neu = _Database.Works[n].ChangedTo;
                    var aenderung = _Database.Works[n].Comand.ToString();

                    switch (_Database.Works[n].Comand) {
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

            tblUndo.Database = x;
            tblUndo.Arrangement = 1;
        }

        private void btnSperreAufheben_Click(object sender, System.EventArgs e) {
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
    }
}
