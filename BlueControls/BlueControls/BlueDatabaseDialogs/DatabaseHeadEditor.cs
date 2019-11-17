#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.BlueDatabaseDialogs
{


    internal sealed partial class DatabaseHeadEditor
    {
        private readonly Database _Database;



        private bool frmHeadEditor_FormClosing_isin;

        private bool IgnoreAll;

        public DatabaseHeadEditor(Database cDatabase)
        {

            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();

            _Database = cDatabase;

        }





        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (IgnoreAll) { return; }
            if (_Database.ReadOnly) { return; }

            if (frmHeadEditor_FormClosing_isin) { return; }
            frmHeadEditor_FormClosing_isin = true;



            // Rules, die 1.
            var NewRules = new ListExt<RuleItem>();
            foreach (var ThisItem in lbxRuleSelector.Item)
            {
                var Rule = (RuleItem)((ObjectListItem)ThisItem).Obj;
                NewRules.Add((RuleItem)Rule.Clone());
            }


            if (!Database.AllRulesOK(NewRules))
            {
                if (MessageBox.Show("Es sind <b>fehlerhafte Regeln</b> vorhanden, diese werden <b>gelöscht</b>.<br><br>Wollen sie fortfahren?", enImageCode.Warnung, "Ja", "Nein") != 0)
                {
                    e.Cancel = true;
                    frmHeadEditor_FormClosing_isin = false;
                    return;
                }
            }


            _Database.GlobalShowPass = txbKennwort.Text;
            _Database.Caption = txbCaption.Text;



            if (tbxUndoAnzahl.Text.IsLong())
            {
                _Database.UndoCount = Math.Max(int.Parse(tbxUndoAnzahl.Text), 5);
            }
            else
            {
                _Database.UndoCount = 5;
            }



            if (tbxReloadVerzoegerung.Text.IsLong())
            {
                _Database.ReloadDelaySecond = Math.Max(int.Parse(tbxReloadVerzoegerung.Text), 5);
            }
            else
            {
                _Database.ReloadDelaySecond = 5;
            }

            if (txbGlobalScale.Text.IsDouble())
            {
                _Database.GlobalScale = Math.Min(double.Parse(txbGlobalScale.Text), 5);
                _Database.GlobalScale = Math.Max(0.5, _Database.GlobalScale);
            }
            else
            {
                _Database.ReloadDelaySecond = 1;
            }


            if (tbxTags.Text != _Database.Tags.JoinWithCr())
            {
                _Database.Tags.Clear();
                _Database.Tags.AddRange(tbxTags.Text.SplitByCR());
            }


            var l = lstBinary.Item.GetNamedBinaries();
            if (l.IsDifferentTo(_Database.Bins))
            {
                _Database.Bins.Clear();
                _Database.Bins.AddRange(l);
            }


            if (DatenbankAdmin.Item.ToListOfString().IsDifferentTo(_Database.DatenbankAdmin))
            {
                _Database.DatenbankAdmin.Clear();
                _Database.DatenbankAdmin.AddRange(DatenbankAdmin.Item.ToListOfString());
            }


            if (PermissionGroups_NewRow.Item.ToListOfString().IsDifferentTo(_Database.PermissionGroups_NewRow))
            {
                _Database.PermissionGroups_NewRow.Clear();
                _Database.PermissionGroups_NewRow.AddRange(PermissionGroups_NewRow.Item.ToListOfString());
                _Database.PermissionGroups_NewRow.Remove("#Administrator");
            }

            _Database.JoinTyp = (enJoinTyp)int.Parse(cbxJoinTyp.Text);
            _Database.VerwaisteDaten = (enVerwaisteDaten)int.Parse(cbxVerwaisteDaten.Text);
            _Database.Skin = int.Parse(cbxBevorzugtesSkin.Text);
            _Database.Ansicht = (enAnsicht)int.Parse(cbxAnsicht.Text);

            _Database.SortDefinition = new RowSortDefinition(_Database, lbxSortierSpalten.Item.ToListOfString().ToArray(), btnSortRichtung.Checked);

            // Regeln --------------
            RepairNewRules(NewRules);

            if (NewRules.IsDifferentTo(_Database.Rules))
            {
                _Database.Rules.Clear();
                _Database.Rules.AddRange(NewRules);
                RepairNewRules(_Database.Rules); // sollte umsonst sein
            }



            // Export ------------
            var NewExports = new List<ExportDefinition>();
            foreach (var ThisItem in ExportSets.Item)
            {
                NewExports.Add((ExportDefinition)((ObjectListItem)ThisItem).Obj);
            }

            if (NewExports.IsDifferentTo(_Database.Export))
            {
                _Database.Export.Clear();
                _Database.Export.AddRange(NewExports);
            }
        }


        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);


            cbxJoinTyp.Item.Clear();
            cbxJoinTyp.Item.AddRange(typeof(enJoinTyp));
            cbxJoinTyp.Text = ((int)_Database.JoinTyp).ToString();

            cbxVerwaisteDaten.Item.Clear();
            cbxVerwaisteDaten.Item.AddRange(typeof(enVerwaisteDaten));
            cbxVerwaisteDaten.Text = ((int)_Database.VerwaisteDaten).ToString();

            cbxBevorzugtesSkin.Item.Clear();
            cbxBevorzugtesSkin.Item.AddRange(typeof(enSkin));
            cbxBevorzugtesSkin.Text = _Database.Skin.ToString();

            cbxAnsicht.Item.Clear();
            cbxAnsicht.Item.AddRange(typeof(enAnsicht));
            cbxAnsicht.Text = ((int)_Database.Ansicht).ToString();


            PermissionGroups_NewRow.Item.Clear();
            PermissionGroups_NewRow.Item.AddRange(_Database.PermissionGroups_NewRow);

            DatenbankAdmin.Item.Clear();
            DatenbankAdmin.Item.AddRange(_Database.DatenbankAdmin);



            txbKennwort.Text = _Database.GlobalShowPass;



            lbxSortierSpalten.Item.Clear();
            if (_Database.SortDefinition != null)
            {
                btnSortRichtung.Checked = _Database.SortDefinition.Reverse;

                if (_Database.SortDefinition.Columns != null)
                {
                    foreach (var ThisColumn in _Database.SortDefinition.Columns)
                    {
                        if (ThisColumn != null) { lbxSortierSpalten.Item.Add(ThisColumn); }
                    }
                }
            }


            tbxTags.Text = _Database.Tags.JoinWithCr();
            // Rules ----------------------------------------
            lbxRuleSelector.Item.Clear();

            foreach (var ThisRule in _Database.Rules)
            {
                if (ThisRule != null)
                {
                    var obj = new ObjectListItem(ThisRule);
                    //obj.Enabled = string.IsNullOrEmpty(ThisRule.SystemKey);
                    lbxRuleSelector.Item.Add(obj);
                }
            }

            //  Enable_Rule_Controls()




            // Exports ----------------
            ExportSets.Item.Clear();

            foreach (var ThisSet in _Database.Export)
            {
                if (ThisSet != null)
                {
                    ExportSets.Item.Add(new ObjectListItem(ThisSet));
                }
            }
            ExportSets.Item.Sort();

            // -----------------------------

            txbCaption.Text = _Database.Caption;
            tbxReloadVerzoegerung.Text = _Database.ReloadDelaySecond.ToString();
            txbGlobalScale.Text = _Database.GlobalScale.ToString();

            tbxUndoAnzahl.Text = _Database.UndoCount.ToString();

            PermissionGroups_NewRow.Suggestions.Clear();
            PermissionGroups_NewRow.Suggestions.AddRange(_Database.Permission_AllUsed(true));


            DatenbankAdmin.Suggestions.Clear();
            DatenbankAdmin.Suggestions.AddRange(_Database.Permission_AllUsed(true));





            lbxSortierSpalten.Suggestions.Clear();
            foreach (var ThisColumnItem in _Database.Column)
            {
                if (ThisColumnItem != null) { lbxSortierSpalten.Suggestions.Add(ThisColumnItem); }
            }



            GenerateUndoTabelle();


            lstBinary.Item.Clear();
            lstBinary.Item.AddRange(_Database.Bins);

            CryptStatus();

            GenerateInfoText();
        }


        private void OkBut_Click(object sender, System.EventArgs e)
        {
            Close();
        }


        private void GenerateInfoText()
        {
            var t = "<b>Datei:</b><tab>" + _Database.Filename + "<br>";
            t = t + "<b>Zeilen:</b><tab>" + (_Database.Row.Count() - 1);
            capInfo.Text = t.TrimEnd("<br>");
        }




        #region  Export 

        private void ExportSets_AddClicked(object sender, System.EventArgs e)
        {
            var NewExportItem = new ObjectListItem(new ExportDefinition(_Database));
            ExportSets.Item.Add(NewExportItem);
            NewExportItem.Checked = true;
        }

        private void ExportSets_Item_CheckedChanged(object sender, System.EventArgs e)
        {
            if (ExportSets.Item.Checked().Count != 1)
            {
                ExportEditor.ObjectWithDialog = null;
                return;
            }

            if (_Database.ReadOnly)
            {
                ExportEditor.ObjectWithDialog = null;
                return;
            }
            var SelectedExport = (ExportDefinition)((ObjectListItem)ExportSets.Item.Checked()[0]).Obj;

            ExportEditor.ObjectWithDialog = SelectedExport;
        }





        #endregion


        #region  Regeln 


        private void lbxRuleSelector_AddClicked(object sender, System.EventArgs e)
        {
            var NewRuleItem = new ObjectListItem(new RuleItem(_Database));
            lbxRuleSelector.Item.Add(NewRuleItem);
            NewRuleItem.Checked = true;

        }

        private void lbxRuleSelector_ContextMenuInit(object sender, ContextMenuInitEventArgs e)
        {
            if (e.HotItem == null) { return; }
            e.UserMenu.Add(enContextMenuComands.Kopieren);
        }

        private void lbxRuleSelector_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e)
        {
            switch (e.ClickedComand)
            {
                case "Kopieren":
                    var ClickedRule = (RuleItem)((ObjectListItem)e.HotItem).Obj;
                    var NewRuleItem = new ObjectListItem((RuleItem)ClickedRule.Clone());
                    lbxRuleSelector.Item.Add(NewRuleItem);
                    NewRuleItem.Checked = true;
                    break;

                default:
                    Develop.DebugPrint(e);
                    break;
            }
        }


        private void lbxRuleSelector_ItemCheckedChanged(object sender, System.EventArgs e)
        {
            if (lbxRuleSelector.Item.Checked().Count != 1) { return; }
            var SelectedRule = (RuleItem)((ObjectListItem)lbxRuleSelector.Item.Checked()[0]).Obj;
            RuleItemEditor.ObjectWithDialog = SelectedRule;
        }



        private void RepairNewRules(ListExt<RuleItem> Rules)
        {
            Rules.Sort();
            foreach (var thisRule in Rules)
            {
                if (thisRule != null)
                {
                    if (!thisRule.IsOk() || thisRule.IsNullOrEmpty())
                    {
                        Rules.Remove(thisRule);
                        RepairNewRules(Rules);
                        return;
                    }
                    thisRule.Repair();
                }
            }

            Rules.Sort();
        }


        #endregion


        private void ExportSets_RemoveClicked(object sender, ListOfBasicListItemEventArgs e)
        {
            foreach (var thisitem in e.Items)
            {
                if (thisitem is ItemCollection.Basics.BasicItem ThisItemBasic)
                {
                    var tempVar = (ExportDefinition)((ObjectListItem)ThisItemBasic).Obj;
                    tempVar.DeleteAllBackups();
                }
            }
        }



        private void Bilder_ContextMenuInit(object sender, ContextMenuInitEventArgs e)
        {
            if (e.HotItem == null) { return; }
            if (!(e.HotItem is BitmapListItem)) { return; }
            e.UserMenu.Add(enContextMenuComands.Umbenennen);
        }


        private void Bilder_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e)
        {

            if (e.HotItem == null) { return; }

            if (!(e.HotItem is BitmapListItem)) { return; }

            var l = (BitmapListItem)e.HotItem;


            switch (e.ClickedComand)
            {
                case "Umbenennen":
                    var n = InputBox.Show("<b><u>Bild umbenennen:</u></b><br><br>Achtung! Dadruch können Bezüge<br> in Texten und Spalten verlorengehen!", l.Caption, enDataFormat.Text);
                    if (!string.IsNullOrEmpty(n)) { l.Caption = n; }
                    break;

                default:
                    Develop.DebugPrint(e);
                    break;
            }

        }

        private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e)
        {
            _Database.Column.GenerateOverView();
        }

        private void DateienSchlüssel_Click(object sender, System.EventArgs e)
        {
            btnDateiSchluessel.Enabled = false;
            btnDateiSchluessel.Text = "Dateien in Arbeit";

            var lLCase = _Database.AllConnectedFilesLCase();


            string NewKey;

            if (string.IsNullOrEmpty(_Database.FileEncryptionKey))

            {
                NewKey = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz äöü#_-<>ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10).Select(s => s[Constants.GlobalRND.Next(s.Length)]).ToArray());
                foreach (var ThisFile in lLCase)
                {
                    var b = modConverter.FileToByte(ThisFile);
                    b = modAllgemein.SimpleCrypt(b, NewKey, 1);
                    FileOperations.DeleteFile(ThisFile, true);
                    modConverter.ByteToFile(ThisFile, b);
                }
            }

            else
            {
                NewKey = string.Empty;
                foreach (var ThisFile in lLCase)
                {
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

        private void CryptStatus()
        {

            if (string.IsNullOrEmpty(_Database.FileEncryptionKey))
            {
                btnDateiSchluessel.Text = "Dateien verschlüsseln";
                btnDateiSchluessel.QuickInfo = "Dazugehörige Dateien der Datenbank sind aktuell im Originalformat auf dem Laufwerk für jedem zugänglich.";
            }
            else
            {
                btnDateiSchluessel.Text = "Dateien freigeben";
                btnDateiSchluessel.QuickInfo = "Dazugehörige Dateien der Datenbank sind aktuell verschlüsselt.";
            }


        }

        private void FremdImport_Click(object sender, System.EventArgs e)
        {
            if (_Database.ReadOnly) { return; }

            var en = new System.Windows.Forms.FormClosingEventArgs(System.Windows.Forms.CloseReason.None, false);

            OnFormClosing(en);
            if (en.Cancel) { return; }





            string GetFromFile;
            var openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.Filter = "Datenbanken|*.mdb";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                GetFromFile = openFileDialog1.FileName;
            }
            else
            {
                return;
            }

            var I = new ItemCollectionList();


            I.Add(new TextListItem(((int)enDatabaseDataType.ColumnArrangement).ToString(), "Anordnungen der Spaltenansichten"));
            I.Add(new TextListItem(((int)enDatabaseDataType.Views).ToString(), "Formulare"));
            I.Add(new TextListItem(((int)enDatabaseDataType.Rules).ToString(), "Regeln"));
            I.Add(new TextListItem(((int)enDatabaseDataType.UndoInOne).ToString(), "Undo-Speicher"));
            I.Add(new TextListItem(((int)enDatabaseDataType.AutoExport).ToString(), "Auto-Export"));
            I.Add(new TextListItem(((int)enDatabaseDataType.BinaryDataInOne).ToString(), "Binäre Daten im Kopf der Datenbank"));
            I.Add(new TextListItem(((int)enDatabaseDataType.Layouts).ToString(), "Eingebettete Layouts"));
            I.Add(new TextListItem(((int)enDatabaseDataType.Tags).ToString(), "Tags des Datenbankkopfes"));
            I.Add(new TextListItem(((int)enDatabaseDataType.SortDefinition).ToString(), "Standard-Sortierung"));

            I.Sort();

            var What = InputBoxComboStyle.Show("Welchen Code:", I, false);

            if (string.IsNullOrEmpty(What)) { return; }


            var _tmp = File.ReadAllBytes(GetFromFile);

            var B = new List<byte>();
            B.AddRange(_tmp);



            enDatabaseDataType Art = 0;
            var Pointer = 0;
            var ColKey = 0;
            var RowKey = 0;
            var X = 0;
            var Y = 0;
            var Inhalt = "";

            var Such = (enDatabaseDataType)int.Parse(What);

            do
            {
                if (Pointer >= B.Count) { break; }
                _Database.Parse(B, ref Pointer, ref Art, ref ColKey, ref RowKey, ref Inhalt, ref X, ref Y);

                if (Such == Art)
                {
                    _Database.InjectCommand(Art, Inhalt);
                    //_Database.AddPending(Art, -1, -1, "", Inhalt, true);

                    MessageBox.Show("<b>Importiert:</b><br>" + Inhalt, enImageCode.Information, "OK");

                }
            } while (Art != enDatabaseDataType.EOF);

            IgnoreAll = true;
            Close();


        }


        private void GenerateUndoTabelle()
        {

            var x = new Database(true);
            ColumnItem c;

            c = new ColumnItem(x, "Index", true);
            c.Caption = "Index";
            c.Format = enDataFormat.Ganzzahl;

            c = new ColumnItem(x, "ColumnKey", true);
            c.Caption = "Spalten-<br>Schlüssel";
            c.Format = enDataFormat.Ganzzahl;

            c = new ColumnItem(x, "ColumnName", true);
            c.Caption = "Spalten-<br>Name";
            c.Format = enDataFormat.Text;

            c = new ColumnItem(x, "ColumnCaption", true);
            c.Caption = "Spaten-<br>Beschriftung";
            c.Format = enDataFormat.Text;

            c = new ColumnItem(x, "RowKey", true);
            c.Caption = "Zeilen-<br>Schlüssel";
            c.Format = enDataFormat.Ganzzahl;

            c = new ColumnItem(x, "RowFirst", true);
            c.Caption = "Zeile, Wert der<br>1. Spalte";
            c.Format = enDataFormat.Text;

            c = new ColumnItem(x, "Aenderzeit", true);
            c.Caption = "Änder-<br>Zeit";
            c.Format = enDataFormat.Text;

            c = new ColumnItem(x, "Aenderer", true);
            c.Caption = "Änderer";
            c.Format = enDataFormat.Text;

            c = new ColumnItem(x, "Symbol", true);
            c.Caption = "Symbol";
            c.Format = enDataFormat.BildCode;

            c = new ColumnItem(x, "Aenderung", true);
            c.Caption = "Änderung";
            c.Format = enDataFormat.Text;

            c = new ColumnItem(x, "WertAlt", true);
            c.Caption = "Wert alt";
            c.Format = enDataFormat.Text;

            c = new ColumnItem(x, "WertNeu", true);
            c.Caption = "Wert neu";
            c.Format = enDataFormat.Text;



            foreach (var ThisColumn in x.Column)
            {
                if (string.IsNullOrEmpty(ThisColumn.Identifier))
                {
                    ThisColumn.MultiLine = true;
                    ThisColumn.TextBearbeitungErlaubt = false;
                    ThisColumn.DropdownBearbeitungErlaubt = false;
                }
            }

            x.RepairAfterParse();

            x.ColumnArrangements[1].HideSystemColumns();


            x.SortDefinition = new RowSortDefinition(x, "Index", true);



            for (var n = 0 ; n < _Database.Works.Count ; n++)
            {


                if (_Database.Works[n].HistorischRelevant)
                {

                    var cd = _Database.Works[n].CellKey.SplitBy("|");


                    _Database.Cell.DataOfCellKey(_Database.Works[n].CellKey, out var Col, out var Row);

                    var r = x.Row.Add(n.ToString());

                    r.CellSet("ColumnKey", cd[0]);
                    r.CellSet("RowKey", cd[1]);



                    if (Col != null)
                    {
                        r.CellSet("ColumnName", Col.Name);
                        r.CellSet("columnCaption", Col.Caption);
                    }


                    if (Col != null && Row != null)
                    {
                        r.CellSet("RowFirst", Row.CellFirstString());
                    }

                    r.CellSet("Aenderer", _Database.Works[n].User);
                    r.CellSet("AenderZeit", _Database.Works[n].CompareKey());



                    var Symb = enImageCode.Fragezeichen;
                    var alt = _Database.Works[n].PreviousValue;
                    var neu = _Database.Works[n].ChangedTo;
                    var aenderung = _Database.Works[n].Comand.ToString();

                    switch (_Database.Works[n].Comand)
                    {
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

                        case enDatabaseDataType.Rules:
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
        }

        private void btnSperreAufheben_Click(object sender, System.EventArgs e)
        {
            _Database.UnlockHard();
            MessageBox.Show("Erledigt.", enImageCode.Information, "OK");
        }


        private void lbxRuleSelector_ItemRemoving(object sender, ListEventArgs e)
        {

            if (RuleItemEditor.ObjectWithDialog == ((ObjectListItem)e.Item).Obj)
            {
                RuleItemEditor.ObjectWithDialog = null;
            }
        }
    }
}
