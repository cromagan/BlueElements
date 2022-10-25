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
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Linq;
using static BlueBasics.Converter;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseHeadEditor {

    #region Fields

    private DatabaseAbstract? _database;
    private bool _frmHeadEditorFormClosingIsin;

    #endregion

    #region Constructors

    public DatabaseHeadEditor(DatabaseAbstract database) {
        // Dieser Aufruf ist f�r den Windows Form-Designer erforderlich.
        InitializeComponent();
        _database = database;
        _database.Disposing += Database_Disposing;
        _database.ShouldICancelSaveOperations += Database_ShouldICancelSaveOperations;
    }

    #endregion

    //public static void FormularWandeln(Database _database, string fn) {
    //    var x = new ConnectedFormula.ConnectedFormula();
    //    var tmp = new Formula();
    //    tmp.Size = x.PadData.SheetSizeInPix.ToSize();
    //    tmp.Database = _database;
    //    tmp.GenerateTabsToNewFormula(x);
    //    x.SaveAsAndChangeTo(fn);
    //}

    #region Methods

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        if (_frmHeadEditorFormClosingIsin) { return; }
        _frmHeadEditorFormClosingIsin = true;
        base.OnFormClosing(e);
        if (_database == null) {
            return;
        }

        WriteInfosBack();
        RemoveDatabase();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        cbxAnsicht.Item.Clear();
        cbxAnsicht.Item.AddRange(typeof(Ansicht));
        PermissionGroups_NewRow.Item.Clear();
        PermissionGroups_NewRow.Item.AddRange(_database.PermissionGroupsNewRow);
        DatenbankAdmin.Item.Clear();
        DatenbankAdmin.Item.AddRange(_database.DatenbankAdmin);
        txbKennwort.Text = _database.GlobalShowPass;
        lbxSortierSpalten.Item.Clear();
        if (_database.SortDefinition != null) {
            btnSortRichtung.Checked = _database.SortDefinition.Reverse;
            if (_database.SortDefinition.Columns != null) {
                foreach (var thisColumn in _database.SortDefinition.Columns.Where(thisColumn => thisColumn != null)) {
                    lbxSortierSpalten.Item.Add(thisColumn);
                }
            }
        }
        txbTags.Text = _database.Tags.JoinWithCr();
        // Exports ----------------
        lbxExportSets.Item.Clear();
        foreach (var thisSet in _database.Export.Where(thisSet => thisSet != null)) {
            lbxExportSets.Item.Add((ExportDefinition)(thisSet.Clone()), string.Empty, string.Empty);
        }
        lbxExportSets.Item.Sort();
        // -----------------------------
        txbCaption.Text = _database.Caption;
        tbxReloadVerzoegerung.Text = _database.ReloadDelaySecond.ToString();
        txbGlobalScale.Text = _database.GlobalScale.ToString(Constants.Format_Float1);
        txbAdditionalFiles.Text = _database.AdditionaFilesPfad;
        txbStandardFormulaFile.Text = _database.StandardFormulaFile;
        txbZeilenQuickInfo.Text = _database.ZeilenQuickInfo.Replace("<br>", "\r");
        tbxUndoAnzahl.Text = _database.UndoCount.ToString();

        PermissionGroups_NewRow.Suggestions.Clear();
        PermissionGroups_NewRow.Suggestions.AddRange(_database.Permission_AllUsed(false));

        DatenbankAdmin.Suggestions.Clear();
        DatenbankAdmin.Suggestions.AddRange(_database.Permission_AllUsed(false));

        lbxSortierSpalten.Suggestions.Clear();
        lbxSortierSpalten.Suggestions.AddRange(_database.Column, false);

        GenerateInfoText();
    }

    private static void Database_ShouldICancelSaveOperations(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

    private void AddUndoToTable(WorkItem work, int index, string db, bool checkNeeded) {
        if (work.HistorischRelevant) {
            if (checkNeeded && tblUndo.Database.Row[work.ToString()] != null) { return; }

            var cd = work.CellKey.SplitAndCutBy("|");
            _database.Cell.DataOfCellKey(work.CellKey, out var col, out var row);
            var r = tblUndo.Database.Row.Add(work.ToString());
            r.CellSet("ColumnKey", cd[0]);
            r.CellSet("RowKey", cd[1]);
            r.CellSet("index", index);
            r.CellSet("db", db);
            if (col != null) {
                r.CellSet("ColumnName", col.Name);
                r.CellSet("columnCaption", col.Caption);
            }
            if (row != null) {
                r.CellSet("RowFirst", row.CellFirstString());
            } else if (cd[1] != "-1") {
                r.CellSet("RowFirst", "[gel�scht]");
            }
            r.CellSet("Aenderer", work.User);
            r.CellSet("AenderZeit", work.CompareKey());
            var symb = ImageCode.Fragezeichen;
            var alt = work.PreviousValue;
            var neu = work.ChangedTo;
            var aenderung = work.Comand.ToString();
            switch (work.Comand) {
                case DatabaseDataType.ce_UTF8Value_withoutSizeData:
                case DatabaseDataType.ce_Value_withoutSizeData:
                    symb = ImageCode.Textfeld;
                    aenderung = "Wert ge�ndert";
                    break;

                case DatabaseDataType.AutoExport:
                    aenderung = "Export ausgef�hrt oder ge�ndert";
                    alt = "";
                    neu = "";
                    symb = ImageCode.Karton;
                    break;

                case DatabaseDataType.Layouts:
                    aenderung = "Layouts ver�ndert";
                    alt = "";
                    neu = "";
                    symb = ImageCode.Layout;
                    break;

                case DatabaseDataType.dummyComand_AddRow:
                    aenderung = "Neue Zeile";
                    symb = ImageCode.PlusZeichen;
                    break;

                case DatabaseDataType.RulesScript:
                    //case enDatabaseDataType.Rules_ALT:
                    aenderung = "Regeln ver�ndert";
                    symb = ImageCode.Formel;
                    alt = "";
                    neu = "";
                    break;

                case DatabaseDataType.ColumnArrangement:
                    aenderung = "Spalten-Anordnungen ver�ndert";
                    symb = ImageCode.Spalte;
                    alt = "";
                    neu = "";
                    break;

                case DatabaseDataType.dummyComand_RemoveRow:
                    aenderung = "Zeile gel�scht";
                    symb = ImageCode.MinusZeichen;
                    break;
            }
            r.CellSet("Aenderung", aenderung);
            r.CellSet("symbol", symb + "|24");
            r.CellSet("Wertalt", alt);
            r.CellSet("Wertneu", neu);
        }
    }

    private void btnClipboard_Click(object sender, System.EventArgs e) => Generic.CopytoClipboard(tblUndo.Export_CSV(FirstRow.ColumnCaption));

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSave.Enabled = false;
        scriptEditor.Message("Speichervorgang...");

        var ok = false;
        if (_database != null) {
            WriteInfosBack();
            ok = _database.Save(false);
        }
        if (ok) {
            scriptEditor.Message("Speichern erfolgreich.");
        } else {
            scriptEditor.Message("Speichern fehlgeschlagen!");
            MessageBox.Show("Speichern fehlgeschlagen!");
        }
        btnSave.Enabled = true;
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => _database.Column.GenerateOverView();

    private void btnSperreAufheben_Click(object sender, System.EventArgs e) {
        _database.UnlockHard();
        MessageBox.Show("Erledigt.", ImageCode.Information, "OK");
    }

    private void Database_Disposing(object sender, System.EventArgs e) {
        RemoveDatabase();
        Close();
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

    private void ExportSets_AddClicked(object sender, System.EventArgs e) {
        var newExportItem = lbxExportSets.Item.Add(new ExportDefinition(_database), string.Empty, string.Empty);
        newExportItem.Checked = true;
    }

    private void GenerateInfoText() {
        var t = "<b>Datei:</b><tab>" + _database.ConnectionID + "<br>";
        t = t + "<b>Zeilen:</b><tab>" + (_database.Row.Count() - 1);
        capInfo.Text = t.TrimEnd("<br>");
    }

    private void GenerateUndoTabelle() {
        Database x = new(true, "Undo " + _database.ConnectionID);
        x.Column.Add("hidden", "hidden", VarType.Text);
        x.Column.Add("Index", "Index", VarType.Integer);
        x.Column.Add("db", "Herkunft", VarType.Text);
        x.Column.Add("ColumnKey", "Spalten-<br>Schl�ssel", VarType.Integer);
        x.Column.Add("ColumnName", "Spalten-<br>Name", VarType.Text);
        x.Column.Add("ColumnCaption", "Spalten-<br>Beschriftung", VarType.Text);
        x.Column.Add("RowKey", "Zeilen-<br>Schl�ssel", VarType.Integer);
        x.Column.Add("RowFirst", "Zeile, Wert der<br>1. Spalte", VarType.Text);
        x.Column.Add("Aenderzeit", "�nder-<br>Zeit", VarType.Text);
        x.Column.Add("Aenderer", "�nderer", VarType.Text);
        x.Column.Add("Symbol", "Symbol", VarType.Text);
        x.Column.Add("Aenderung", "�nderung", VarType.Text);
        x.Column.Add("WertAlt", "Wert alt", VarType.Text);
        x.Column.Add("WertNeu", "Wert neu", VarType.Text);
        foreach (var thisColumn in x.Column.Where(thisColumn => string.IsNullOrEmpty(thisColumn.Identifier))) {
            thisColumn.MultiLine = true;
            thisColumn.TextBearbeitungErlaubt = false;
            thisColumn.DropdownBearbeitungErlaubt = false;
            thisColumn.BehaviorOfImageAndText = BildTextVerhalten.Bild_oder_Text;
        }

        x.RepairAfterParse();

        var car = x.ColumnArrangements.CloneWithClones();
        car[1].ShowAllColumns();
        car[1].Hide("hidden");
        car[1].HideSystemColumns();

        x.ColumnArrangements = car;

        x.SortDefinition = new RowSortDefinition(x, "Index", true);
        tblUndo.DatabaseSet(x, string.Empty);
        tblUndo.Arrangement = 1;

        if (_database is Database db) {
            for (var n = 0; n < db.Works.Count; n++) {
                AddUndoToTable(db.Works[n], n, string.Empty, false);
            }
        }
    }

    private void GlobalTab_Selecting(object sender, System.Windows.Forms.TabControlCancelEventArgs e) {
        if (e.TabPage == Tab_Regeln) {
            scriptEditor.Database = _database;
        }
        if (e.TabPage == Tab_Undo) {
            if (tblUndo.Database == null) { GenerateUndoTabelle(); }
        }
    }

    private void lbxExportSets_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (lbxExportSets.Item.Checked().Count != 1) {
            ExportEditor.Item = null;
            return;
        }
        if (_database.ReadOnly) {
            ExportEditor.Item = null;
            return;
        }
        var selectedExport = (ExportDefinition)((TextListItem)lbxExportSets.Item.Checked()[0]).Tag;
        ExportEditor.Item = selectedExport;
    }

    private void lbxExportSets_RemoveClicked(object sender, ListOfBasicListItemEventArgs e) {
        foreach (var thisitem in e.Items) {
            if (thisitem is BasicListItem thisItemBasic) {
                var tempVar = (ExportDefinition)((TextListItem)thisItemBasic).Tag;
                tempVar.DeleteAllBackups();
            }
        }
    }

    private void OkBut_Click(object sender, System.EventArgs e) => Close();

    private void RemoveDatabase() {
        if (_database == null) { return; }
        _database.Disposing -= Database_Disposing;
        _database.ShouldICancelSaveOperations -= Database_ShouldICancelSaveOperations;
        _database = null;
    }

    private void tblUndo_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        var bt = (Table)sender;
        var cellKey = e.Tags.TagGet("Cellkey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        bt.Database.Cell.DataOfCellKey(cellKey, out var column, out _);
        e.UserMenu.Add("Sortierung", true);
        e.UserMenu.Add(ContextMenuComands.SpaltenSortierungAZ, column != null && column.Format.CanBeChangedByRules());
        e.UserMenu.Add(ContextMenuComands.SpaltenSortierungZA, column != null && column.Format.CanBeChangedByRules());
    }

    private void tblUndo_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        var bt = (Table)sender;
        var cellKey = e.Tags.TagGet("CellKey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        bt.Database.Cell.DataOfCellKey(cellKey, out var column, out _);
        switch (e.ClickedComand) {
            case "SpaltenSortierungAZ":
                bt.SortDefinitionTemporary = new RowSortDefinition(bt.Database, column.Name, false);
                break;

            case "SpaltenSortierungZA":
                bt.SortDefinitionTemporary = new RowSortDefinition(bt.Database, column.Name, true);
                break;
        }
    }

    private void WriteInfosBack() {
        if (_database == null) { return; } // Disposed
        if (_database.ReadOnly) { return; }
        scriptEditor.WriteScriptBack();
        _database.GlobalShowPass = txbKennwort.Text;
        _database.Caption = txbCaption.Text;
        _database.UndoCount = tbxUndoAnzahl.Text.IsLong() ? Math.Max(IntParse(tbxUndoAnzahl.Text), 5) : 5;
        _database.ReloadDelaySecond = tbxReloadVerzoegerung.Text.IsLong() ? Math.Max(IntParse(tbxReloadVerzoegerung.Text), 5) : 5;
        if (txbGlobalScale.Text.IsDouble()) {
            _database.GlobalScale = Math.Min(DoubleParse(txbGlobalScale.Text), 5);
            _database.GlobalScale = Math.Max(0.5, _database.GlobalScale);
        } else {
            _database.ReloadDelaySecond = 1;
        }
        _database.AdditionaFilesPfad = txbAdditionalFiles.Text;
        _database.StandardFormulaFile = txbStandardFormulaFile.Text;
        _database.ZeilenQuickInfo = txbZeilenQuickInfo.Text.Replace("\r", "<br>");

        _database.Tags = txbTags.Text.SplitAndCutByCrToList();

        _database.DatenbankAdmin = DatenbankAdmin.Item.ToListOfString();

        var tmp = PermissionGroups_NewRow.Item.ToListOfString();
        tmp.Remove("#Administrator");
        _database.PermissionGroupsNewRow = tmp;

        #region Sortierung

        var colnam = lbxSortierSpalten.Item.Select(thisk => ((ColumnItem)thisk.Tag).Name).ToList();
        _database.SortDefinition = new RowSortDefinition(_database, colnam, btnSortRichtung.Checked);

        #endregion

        // Export ------------
        var t = new ListExt<ExportDefinition>();
        t.AddRange(lbxExportSets.Item.Select(thisItem => (ExportDefinition)((TextListItem)thisItem).Tag));
        _database.Export = t;
    }

    #endregion
}