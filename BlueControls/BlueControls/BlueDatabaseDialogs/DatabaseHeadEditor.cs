// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseHeadEditor : IHasDatabase {

    #region Fields

    private bool _frmHeadEditorFormClosingIsin;

    #endregion

    #region Constructors

    public DatabaseHeadEditor(DatabaseAbstract database) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        Database = database;
        Database.Disposing += Database_Disposing;
    }

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; private set; }

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

    protected override void OnFormClosing(FormClosingEventArgs e) {
        if (_frmHeadEditorFormClosingIsin) { return; }
        _frmHeadEditorFormClosingIsin = true;
        base.OnFormClosing(e);
        if (Database == null || Database.IsDisposed) {
            return;
        }

        WriteInfosBack();
        RemoveDatabase();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        PermissionGroups_NewRow.Item.Clear();
        PermissionGroups_NewRow.Item.AddRange(Database.PermissionGroupsNewRow);
        DatenbankAdmin.Item.Clear();
        DatenbankAdmin.Item.AddRange(Database.DatenbankAdmin);
        txbKennwort.Text = Database.GlobalShowPass;
        lbxSortierSpalten.Item.Clear();
        if (Database.SortDefinition != null) {
            btnSortRichtung.Checked = Database.SortDefinition.Reverse;
            if (Database.SortDefinition?.Columns != null) {
                foreach (var thisColumn in Database.SortDefinition.Columns) {
                    if (thisColumn != null && !thisColumn.IsDisposed) {
                        _ = lbxSortierSpalten.Item.Add(thisColumn);
                    }
                }
            }
        }
        txbTags.Text = Database.Tags.JoinWithCr();

        txbCaption.Text = Database.Caption;
        txbGlobalScale.Text = Database.GlobalScale.ToString(Constants.Format_Float1);
        txbAdditionalFiles.Text = Database.AdditionalFilesPfad;
        txbStandardFormulaFile.Text = Database.StandardFormulaFile;
        txbZeilenQuickInfo.Text = Database.ZeilenQuickInfo.Replace("<br>", "\r");
        tbxUndoAnzahl.Text = Database.UndoCount.ToString();

        PermissionGroups_NewRow.Suggestions.Clear();
        PermissionGroups_NewRow.Suggestions.AddRange(Database.Permission_AllUsed(false));

        DatenbankAdmin.Suggestions.Clear();
        DatenbankAdmin.Suggestions.AddRange(Database.Permission_AllUsed(false));

        lbxSortierSpalten.Suggestions.Clear();
        lbxSortierSpalten.Suggestions.AddRange(Database.Column, false);

        GenerateInfoText();
    }

    private void AddUndoToTable(WorkItem work, int index, string db, bool checkNeeded) {
        if (checkNeeded && tblUndo.Database.Row[work.ToString()] != null) { return; }

        var cd = work.CellKey.SplitAndCutBy("|");
        Database.Cell.DataOfCellKey(work.CellKey, out var col, out var row);
        var r = tblUndo.Database.Row.GenerateAndAdd(work.ToString(), "New Undo Item");
        r.CellSet("ColumnName", cd[0]);
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
            r.CellSet("RowFirst", "[gelöscht]");
        }
        r.CellSet("Aenderer", work.User);
        r.CellSet("AenderZeit", work.CompareKey());
        var symb = ImageCode.Fragezeichen;
        var alt = work.PreviousValue;
        var neu = work.ChangedTo;
        var aenderung = work.Comand.ToString();
        switch (work.Comand) {
            //case DatabaseDataType.UTF8Value_withoutSizeData:
            //case DatabaseDataType.Value_withoutSizeData:
            //    symb = ImageCode.Textfeld;
            //    aenderung = "Wert geändert";
            //    break;

            //case DatabaseDataType.AutoExport:
            //    aenderung = "Export ausgeführt oder geändert";
            //    alt = string.Empty;
            //    neu = string.Empty;
            //    symb = ImageCode.Karton;
            //    break;

            case DatabaseDataType.EventScript:
                aenderung = "Import Script geändert";
                alt = string.Empty;
                neu = string.Empty;
                symb = ImageCode.Pfeil_Links;
                break;

            case DatabaseDataType.Layouts:
                aenderung = "Layouts verändert";
                alt = string.Empty;
                neu = string.Empty;
                symb = ImageCode.Layout;
                break;

            case DatabaseDataType.Comand_AddRow:
                aenderung = "Neue Zeile";
                symb = ImageCode.PlusZeichen;
                break;

            //case DatabaseDataType.RulesScript:
            //    //case enDatabaseDataType.Rules_ALT:
            //    aenderung = "Regeln verändert";
            //    symb = ImageCode.Formel;
            //    alt = string.Empty;
            //    neu = string.Empty;
            //    break;

            case DatabaseDataType.ColumnArrangement:
                aenderung = "Spalten-Anordnungen verändert";
                symb = ImageCode.Spalte;
                alt = string.Empty;
                neu = string.Empty;
                break;

            case DatabaseDataType.Comand_RemoveRow:
                aenderung = "Zeile gelöscht";
                symb = ImageCode.MinusZeichen;
                break;
        }
        r.CellSet("Aenderung", aenderung);
        r.CellSet("symbol", symb + "|24");
        r.CellSet("Wertalt", alt);
        r.CellSet("Wertneu", neu);
    }

    private void btnClipboard_Click(object sender, System.EventArgs e) => Generic.CopytoClipboard(tblUndo.Export_CSV(FirstRow.ColumnCaption));

    private void btnOptimize_Click(object sender, System.EventArgs e) => Database?.Optimize();

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSave.Enabled = false;

        //scriptEditor.Message("Speichervorgang...");

        var ok = false;
        if (Database != null) {
            WriteInfosBack();
            ok = Database.Save();
        }
        if (ok) {
            MessageBox.Show("Speichern erfolgreich.", ImageCode.Häkchen, "Ok");
        } else {
            //scriptEditor.Message("Speichern fehlgeschlagen!");
            MessageBox.Show("Speichern fehlgeschlagen!", ImageCode.Kreuz, "Ok");
        }
        btnSave.Enabled = true;
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Database?.Column.GenerateOverView();

    private void Database_Disposing(object sender, System.EventArgs e) {
        RemoveDatabase();
        Close();
    }

    private void GenerateInfoText() {
        var t = "<b>Tabelle:</b> <tab>" + Database.ConnectionData.TableName + "<br>";
        t += "<b>ID:</b> <tab>" + Database.ConnectionData.UniqueID + "<br>";
        t += "<b>Zeilen:</b> <tab>" + (Database.Row.Count() - 1);
        capInfo.Text = t.TrimEnd("<br>");
    }

    private void GenerateUndoTabelle() {
        Database x = new(false, DatabaseAbstract.UniqueKeyValue(), string.Empty);
        _ = x.Column.GenerateAndAdd("hidden", "hidden", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("Index", "Index", ColumnFormatHolder.IntegerPositive);
        _ = x.Column.GenerateAndAdd("db", "Herkunft", ColumnFormatHolder.Text);
        //x.Column.GenerateAndAdd("ColumnKey", "Spalten-<br>Schlüssel", ColumnFormatHolder.IntegerPositive);
        _ = x.Column.GenerateAndAdd("ColumnName", "Spalten-<br>Name", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("ColumnCaption", "Spalten-<br>Beschriftung", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("RowKey", "Zeilen-<br>Schlüssel", ColumnFormatHolder.IntegerPositive);
        _ = x.Column.GenerateAndAdd("RowFirst", "Zeile, Wert der<br>1. Spalte", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("Aenderzeit", "Änder-<br>Zeit", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("Aenderer", "Änderer", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("Symbol", "Symbol", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("Aenderung", "Änderung", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("WertAlt", "Wert alt", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("WertNeu", "Wert neu", ColumnFormatHolder.Text);
        foreach (var thisColumn in x.Column) {
            if (!thisColumn.IsSystemColumn()) {
                thisColumn.MultiLine = true;
                thisColumn.TextBearbeitungErlaubt = false;
                thisColumn.DropdownBearbeitungErlaubt = false;
                thisColumn.BehaviorOfImageAndText = BildTextVerhalten.Bild_oder_Text;
            }
        }

        x.RepairAfterParse();

        var car = x.ColumnArrangements.CloneWithClones();
        ColumnViewCollection.ShowAllColumns(car[1]);
        ColumnViewCollection.Hide("hidden", car[1]);
        ColumnViewCollection.HideSystemColumns(car[1]);

        x.ColumnArrangements = new(car);

        x.SortDefinition = new RowSortDefinition(x, "Index", true);
        tblUndo.DatabaseSet(x, string.Empty);
        tblUndo.Arrangement = 1;

        if (Database is Database db) {
            for (var n = 0; n < db.Works.Count; n++) {
                AddUndoToTable(db.Works[n], n, string.Empty, false);
            }
        }
    }

    private void GlobalTab_Selecting(object sender, TabControlCancelEventArgs e) {
        if (e.TabPage == tabUndo) {
            if (tblUndo.Database == null) { GenerateUndoTabelle(); }
        }
    }

    private void OkBut_Click(object sender, System.EventArgs e) => Close();

    private void RemoveDatabase() {
        if (Database == null || Database.IsDisposed) { return; }
        Database.Disposing -= Database_Disposing;
        Database = null;
    }

    private void tblUndo_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        var bt = (Table)sender;
        var cellKey = e.Tags.TagGet("Cellkey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        bt.Database.Cell.DataOfCellKey(cellKey, out var column, out _);
        _ = e.UserMenu.Add("Sortierung", true);
        _ = e.UserMenu.Add(ContextMenuComands.SpaltenSortierungAZ, column != null && column.Format.CanBeChangedByRules());
        _ = e.UserMenu.Add(ContextMenuComands.SpaltenSortierungZA, column != null && column.Format.CanBeChangedByRules());
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
        if (Database == null || Database.IsDisposed || Database.ReadOnly) { return; } // Disposed

        //eventScriptEditor.WriteScriptBack();
        Database.GlobalShowPass = txbKennwort.Text;
        Database.Caption = txbCaption.Text;
        Database.UndoCount = tbxUndoAnzahl.Text.IsLong() ? Math.Max(IntParse(tbxUndoAnzahl.Text), 5) : 5;
        if (txbGlobalScale.Text.IsDouble()) {
            Database.GlobalScale = Math.Min(DoubleParse(txbGlobalScale.Text), 5);
            Database.GlobalScale = Math.Max(0.5, Database.GlobalScale);
        }
        Database.AdditionalFilesPfad = txbAdditionalFiles.Text;
        Database.StandardFormulaFile = txbStandardFormulaFile.Text;
        Database.ZeilenQuickInfo = txbZeilenQuickInfo.Text.Replace("\r", "<br>");

        Database.Tags = new(txbTags.Text.SplitAndCutByCrToList());

        Database.DatenbankAdmin = new(DatenbankAdmin.Item.ToListOfString());

        var tmp = PermissionGroups_NewRow.Item.ToListOfString();
        _ = tmp.Remove(DatabaseAbstract.Administrator);
        Database.PermissionGroupsNewRow = new(tmp);

        #region Sortierung

        var colnam = lbxSortierSpalten.Item.Select(thisk => ((ColumnItem)((ReadableListItem)thisk).Item).Name).ToList();
        Database.SortDefinition = new RowSortDefinition(Database, colnam, btnSortRichtung.Checked);

        #endregion
    }

    #endregion
}