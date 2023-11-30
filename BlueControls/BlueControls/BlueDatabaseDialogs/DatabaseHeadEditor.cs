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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Variables;
using static BlueBasics.Converter;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseHeadEditor : FormWithStatusBar, IHasDatabase {

    #region Fields

    private bool _frmHeadEditorFormClosingIsin;

    #endregion

    #region Constructors

    public DatabaseHeadEditor(DatabaseAbstract database) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        Database = database;
        Database.DisposingEvent += Database_DisposingEvent;
    }

    #endregion

    #region Properties

    public DatabaseAbstract? Database { get; private set; }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        if (_frmHeadEditorFormClosingIsin) { return; }
        _frmHeadEditorFormClosingIsin = true;
        base.OnFormClosing(e);
        if (Database is not DatabaseAbstract db || db.IsDisposed) {
            return;
        }

        WriteInfosBack();
        RemoveDatabase();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        PermissionGroups_NewRow.Item.Clear();
        PermissionGroups_NewRow.Item.AddRange(db.PermissionGroupsNewRow);

        DatenbankAdmin.Item.Clear();
        DatenbankAdmin.Item.AddRange(db.DatenbankAdmin);

        txbKennwort.Text = db.GlobalShowPass;
        lbxSortierSpalten.Item.Clear();

        if (db.SortDefinition != null) {
            btnSortRichtung.Checked = db.SortDefinition.Reverse;
            if (db.SortDefinition?.Columns != null) {
                foreach (var thisColumn in db.SortDefinition.Columns) {
                    if (thisColumn != null && !thisColumn.IsDisposed) {
                        _ = lbxSortierSpalten.Item.Add(thisColumn);
                    }
                }
            }
        }
        txbTags.Text = db.Tags.JoinWithCr();

        txbCaption.Text = db.Caption;
        txbGlobalScale.Text = db.GlobalScale.ToString(Constants.Format_Float1);
        txbAdditionalFiles.Text = db.AdditionalFilesPfad;
        txbStandardFormulaFile.Text = db.StandardFormulaFile;
        txbZeilenQuickInfo.Text = db.ZeilenQuickInfo.Replace("<br>", "\r");

        PermissionGroups_NewRow.Suggestions.Clear();
        PermissionGroups_NewRow.Suggestions.AddRange(db.Permission_AllUsed(false));

        DatenbankAdmin.Suggestions.Clear();
        DatenbankAdmin.Suggestions.AddRange(db.Permission_AllUsed(false));

        lbxSortierSpalten.Suggestions.Clear();
        lbxSortierSpalten.Suggestions.AddRange(db.Column, false);

        variableEditor.WriteVariablesToTable(Database?.Variables);

        GenerateUndoTabelle();

        GenerateInfoText();
    }

    private void AddUndoToTable(UndoItem work) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        var r = tblUndo?.Database?.Row.GenerateAndAdd(work.ToString(), "New Undo Item");
        if (r == null) { return; }

        r.CellSet("ColumnName", work.ColName);
        r.CellSet("RowKey", work.RowKey);
        if (db.Column.Exists(work.ColName) is ColumnItem col && !col.IsDisposed) {
            r.CellSet("columnCaption", col.Caption);
        }
        if (db.Row.SearchByKey(work.RowKey) is RowItem row && !row.IsDisposed) {
            r.CellSet("RowFirst", row.CellFirstString());
        } else if (!string.IsNullOrEmpty(work.RowKey)) {
            r.CellSet("RowFirst", "[gelöscht]");
        }
        r.CellSet("Aenderer", work.User);
        r.CellSet("AenderZeit", work.DateTimeUtc);
        r.CellSet("Kommentar", work.Comment);

        if (work.Container.IsFormat(FormatHolder.FilepathAndName)) {
            r.CellSet("Herkunft", work.Container.FileNameWithoutSuffix());
        }

        var symb = ImageCode.Fragezeichen;
        var alt = work.PreviousValue;
        var neu = work.ChangedTo;

        switch (work.Command) {
            case DatabaseDataType.Value_withoutSizeData:
                symb = ImageCode.Stift;
                break;

            case DatabaseDataType.DatabaseVariables:
                alt = "[Variablen alt]";
                neu = "[Variablen neu]";
                symb = ImageCode.Variable;
                break;

            case DatabaseDataType.EventScript:
                alt = "[Skript alt]";
                neu = "[Skript neu]";
                symb = ImageCode.Skript;
                break;

            case DatabaseDataType.Command_AddRow:
                symb = ImageCode.PlusZeichen;
                break;

            case DatabaseDataType.ColumnArrangement:
                symb = ImageCode.Spalte;
                alt = "[Spaltenanordnung alt]";
                neu = "[Spaltenanordnung neu]";
                break;

            case DatabaseDataType.Command_RemoveRow:
                symb = ImageCode.MinusZeichen;
                break;

            case DatabaseDataType.Command_NewStart:
                symb = ImageCode.Abspielen;
                break;

            case DatabaseDataType.TemporaryDatabaseMasterTimeUTC:
                symb = ImageCode.Uhr;
                break;

            case DatabaseDataType.TemporaryDatabaseMasterUser:
                symb = ImageCode.Person;
                break;
        }
        r.CellSet("Aenderung", work.Command.ToString());
        r.CellSet("symbol", symb + "|24");
        r.CellSet("Wertalt", alt);
        r.CellSet("Wertneu", neu);
    }

    private void btnClipboard_Click(object sender, System.EventArgs e) => Generic.CopytoClipboard(tblUndo.Export_CSV(FirstRow.ColumnCaption));

    private void btnOptimize_Click(object sender, System.EventArgs e) => Database?.Optimize();

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Database?.Column.GenerateOverView();

    private void butSystemspaltenErstellen_Click(object sender, System.EventArgs e) {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        db.Column.GenerateAndAddSystem();
    }

    private void Database_DisposingEvent(object sender, System.EventArgs e) {
        RemoveDatabase();
        Close();
    }

    private void GenerateInfoText() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) {
            capInfo.Text = "Datenbank-Fehler";
            return;
        }

        var t = "<b>Tabelle:</b> <tab>" + Database.TableName + "<br>";
        t += "<b>Zeilen:</b> <tab>" + (Database.Row.Count() - 1) + "<br>";
        t += "<b>Temporärer Master:</b>  <tab>" + Database.TemporaryDatabaseMasterTimeUtc + " " + Database.TemporaryDatabaseMasterUser + "<br>";
        t += "<b>Letzte Komplettierung:</b> <tab>" + Database.FileStateUTCDate.ToString(Constants.Format_Date7, CultureInfo.InvariantCulture) + "<br>";
        t += "<b>ID:</b> <tab>" + Database.ConnectionData.UniqueId + "<br>";
        capInfo.Text = t.TrimEnd("<br>");
    }

    private void GenerateUndoTabelle() {
        Database x = new(DatabaseAbstract.UniqueKeyValue());
        //_ = x.Column.GenerateAndAdd("hidden", "hidden", ColumnFormatHolder.Text);
        //_ = x.Column.GenerateAndAdd("Index", "Index", ColumnFormatHolder.IntegerPositive);
        _ = x.Column.GenerateAndAdd("ColumnName", "Spalten-<br>Name", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("ColumnCaption", "Spalten-<br>Beschriftung", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("RowKey", "Zeilen-<br>Schlüssel", ColumnFormatHolder.IntegerPositive);
        _ = x.Column.GenerateAndAdd("RowFirst", "Zeile, Wert der<br>1. Spalte", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("Aenderzeit", "Änder-<br>Zeit", ColumnFormatHolder.DateTime);
        _ = x.Column.GenerateAndAdd("Aenderer", "Änderer", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("Symbol", "Symbol", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("Aenderung", "Änderung", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("WertAlt", "Wert alt", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("WertNeu", "Wert neu", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("Kommentar", "Kommentar", ColumnFormatHolder.Text);
        _ = x.Column.GenerateAndAdd("Herkunft", "Herkunft", ColumnFormatHolder.Text);
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
        car[1].ShowColumns("ColumnName", "ColumnCaption", "RowKey", "RowFirst", "Aenderzeit", "Aenderer", "Symbol", "Aenderung", "WertAlt", "WertNeu", "Kommentar", "Herkunft");

        x.ColumnArrangements = new(car);

        x.SortDefinition = new RowSortDefinition(x, "Index", true);

        tblUndo.DatabaseSet(x, string.Empty);
        tblUndo.Arrangement = 1;

        if (Database is DatabaseAbstract db) {
            //if (!db.UndoLoaded) {
            //    UpdateStatusBar(FehlerArt.Info, "Lade Undo-Speicher", true);

            //    db.GetUndoCache();
            //}
            UpdateStatusBar(FehlerArt.Info, "Erstelle Tabellen Ansicht des Undo-Speichers", true);

            foreach (var thisUndo in db.Undo) {
                AddUndoToTable(thisUndo);
            }
        }

        tblUndo.SortDefinitionTemporary = new RowSortDefinition(x, "Aenderzeit", true);

        x.Freeze("Nur Ansicht");
    }

    //private void GlobalTab_Selecting(object sender, TabControlCancelEventArgs e) {
    //    if (e.TabPage == tabUndo) {
    //        if (tblUndo.Database is null) { GenerateUndoTabelle(); }
    //    }
    //}

    private void OkBut_Click(object sender, System.EventArgs e) => Close();

    private void RemoveDatabase() {
        if (Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        Database.DisposingEvent -= Database_DisposingEvent;
        Database = null;
    }

    private void tblUndo_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (sender is not Table tbl || tbl.Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        ColumnItem? column = null;
        if (e.HotItem is ColumnItem c) { column = c; }
        if (e.HotItem is string ck) { db.Cell.DataOfCellKey(ck, out column, out _); }

        _ = e.UserMenu.Add("Sortierung", true);
        _ = e.UserMenu.Add(ContextMenuCommands.SpaltenSortierungAZ, column != null && column.Format.CanBeChangedByRules());
        _ = e.UserMenu.Add(ContextMenuCommands.SpaltenSortierungZA, column != null && column.Format.CanBeChangedByRules());
    }

    private void tblUndo_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        if (sender is not Table tbl || tbl.Database is not DatabaseAbstract db || db.IsDisposed) { return; }
        //RowItem? row = null;
        ColumnItem? column = null;
        //if (e.HotItem is RowItem r) { row = r; }
        if (e.HotItem is ColumnItem c) { column = c; }
        if (e.HotItem is string ck) { db.Cell.DataOfCellKey(ck, out column, out _); }

        if (column == null) { return; }

        switch (e.ClickedCommand) {
            case "SpaltenSortierungAZ":
                tbl.SortDefinitionTemporary = new RowSortDefinition(tbl.Database, column.KeyName, false);
                break;

            case "SpaltenSortierungZA":
                tbl.SortDefinitionTemporary = new RowSortDefinition(tbl.Database, column.KeyName, true);
                break;
        }
    }

    private void WriteInfosBack() {
        if (TableView.ErrorMessage(Database, EditableErrorReasonType.EditAcut) || Database == null || Database.IsDisposed) { return; }

        //eventScriptEditor.WriteScriptBack();
        Database.GlobalShowPass = txbKennwort.Text;
        Database.Caption = txbCaption.Text;
        //Database.UndoCount = txbUndoAnzahl.Text.IsLong() ? Math.Max(IntParse(txbUndoAnzahl.Text), 5) : 5;
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
        _ = tmp.Remove(Constants.Administrator);
        Database.PermissionGroupsNewRow = new(tmp);

        #region Sortierung

        var colnam = lbxSortierSpalten.Item.Select(thisk => ((ColumnItem)((ReadableListItem)thisk).Item).KeyName).ToList();
        Database.SortDefinition = new RowSortDefinition(Database, colnam, btnSortRichtung.Checked);

        #endregion

        #region Variablen

        // Identisch in DatabaseHeadEditor und DatabaseScriptEditor
        var l = variableEditor.GetVariables();
        var l2 = new List<VariableString>();
        foreach (var thisv in l) {
            if (thisv is VariableString vs) {
                l2.Add(vs);
            }
        }
        Database.Variables = new VariableCollection(l2);

        #endregion
    }

    #endregion
}