// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Variables;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseHeadEditor : FormWithStatusBar, IHasDatabase, IIsEditor {

    #region Fields

    public bool UndoDone;
    private Database? _database;
    private bool _frmHeadEditorFormClosingIsin;

    #endregion

    #region Constructors

    public DatabaseHeadEditor() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        Database = null;
    }

    #endregion

    #region Properties

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    public IEditable? ToEdit {
        set {
            Database? db = null;

            if (value is Database dbx) {
                db = dbx;
            } else if (value is ConnectionInfo ci) {
                db = Database.GetById(ci, false, null, true);
            }

            Database = db is { IsDisposed: false } ? db : null;
        }
    }

    #endregion

    #region Methods

    public static void AddUndosToTable(Table tblUndo, Database? database, float maxAgeInDays) {
        if (database is { IsDisposed: false } db) {
            UpdateStatusBar(FehlerArt.Info, $"Erstelle Tabellen Ansicht des Undo-Speichers der Datenbank '{db.Caption}'", true);

            List<UndoItem> un = [.. db.Undo]; // Kann und wird verändert!

            foreach (var thisUndo in un) {
                AddUndoToTable(tblUndo, thisUndo, db, maxAgeInDays);
            }
        }
    }

    public static void AddUndoToTable(Table tblUndo, UndoItem work, Database db, float maxAgeInDays) {
        if (maxAgeInDays > 0 && DateTime.UtcNow.Subtract(work.DateTimeUtc).TotalDays > maxAgeInDays) { return; }
        var r = tblUndo.Database?.Row.GenerateAndAdd(work.ParseableItems().FinishParseable(), null, "New Undo Item");
        if (r == null) { return; }

        r.CellSet("ColumnName", work.ColName, string.Empty);
        r.CellSet("RowKey", work.RowKey, string.Empty);
        if (db.Column[work.ColName] is { IsDisposed: false } col) {
            r.CellSet("columnCaption", col.Caption, string.Empty);
        }
        if (db.Row.SearchByKey(work.RowKey) is { IsDisposed: false } row) {
            r.CellSet("RowFirst", row.CellFirstString(), string.Empty);
        } else if (!string.IsNullOrEmpty(work.RowKey)) {
            r.CellSet("RowFirst", "[gelöscht]", string.Empty);
        }
        r.CellSet("Aenderer", work.User, string.Empty);
        r.CellSet("AenderZeit", work.DateTimeUtc, string.Empty);
        r.CellSet("Kommentar", work.Comment, string.Empty);

        r.CellSet("Database", db.Caption, string.Empty);

        if (work.Container.IsFormat(FormatHolder.FilepathAndName)) {
            r.CellSet("Herkunft", work.Container.FileNameWithoutSuffix(), string.Empty);
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
                alt = "[Skript alt (" + alt.Length + " Zeichen)]";
                neu = "[Skript neu (" + neu.Length + " Zeichen)]";
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
        r.CellSet("Aenderung", work.Command.ToString(), string.Empty);
        r.CellSet("symbol", symb + "|24", string.Empty);
        r.CellSet("Wertalt", alt, string.Empty);
        r.CellSet("Wertneu", neu, string.Empty);
    }

    public static void GenerateUndoTabelle(Table tblUndo) {
        Database db = new(Database.UniqueKeyValue());
        db.LogUndo = false;
        //_ = x.Column.GenerateAndAdd("hidden", "hidden", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("ID", "ID", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("Database", "Datenbank", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("ColumnName", "Spalten-<br>Name", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("ColumnCaption", "Spalten-<br>Beschriftung", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("RowKey", "Zeilen-<br>Schlüssel", ColumnFormatHolder.LongPositive);
        _ = db.Column.GenerateAndAdd("RowFirst", "Zeile, Wert der<br>1. Spalte", ColumnFormatHolder.Text);
        var az = db.Column.GenerateAndAdd("Aenderzeit", "Änder-<br>Zeit", ColumnFormatHolder.DateTime);
        _ = db.Column.GenerateAndAdd("Aenderer", "Änderer", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("Symbol", "Symbol", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("Aenderung", "Änderung", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("WertAlt", "Wert alt", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("WertNeu", "Wert neu", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("Kommentar", "Kommentar", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("Herkunft", "Herkunft", ColumnFormatHolder.Text);
        foreach (var thisColumn in db.Column) {
            if (!thisColumn.IsSystemColumn()) {
                thisColumn.MultiLine = true;
                thisColumn.TextBearbeitungErlaubt = false;
                thisColumn.DropdownBearbeitungErlaubt = false;
                thisColumn.DefaultRenderer = Renderer_TextOneLine.ClassId;
            }
        }

        if (db.Column["Symbol"] is { IsDisposed: false } c) {
            var o = new Renderer_ImageAndText {
                Text_anzeigen = false,
                Bild_anzeigen = true
            };

            c.RendererSettings = o.ParseableItems().FinishParseable();
        }

        db.RepairAfterParse();

        var tcvc = ColumnViewCollection.ParseAll(db);
        tcvc[1].ShowColumns("Database", "ColumnName", "ColumnCaption", "RowKey", "RowFirst", "Aenderzeit", "Aenderer", "Symbol", "Aenderung", "WertAlt", "WertNeu", "Kommentar", "Herkunft");

        db.ColumnArrangements = tcvc.ToString(false);

        //x.SortDefinition = new RowSortDefinition(db, "Index", true);

        tblUndo.DatabaseSet(db, string.Empty);
        tblUndo.Arrangement = string.Empty;
        tblUndo.SortDefinitionTemporary = new RowSortDefinition(db, az, true);
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        if (_frmHeadEditorFormClosingIsin) { return; }
        _frmHeadEditorFormClosingIsin = true;
        base.OnFormClosing(e);

        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        WriteInfosBack();
        Database = null;
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        PermissionGroups_NewRow.ItemClear();
        PermissionGroups_NewRow.Check(db.PermissionGroupsNewRow);
        PermissionGroups_NewRow.Suggestions.Clear();
        PermissionGroups_NewRow.ItemAddRange(Table.Permission_AllUsed(false));

        DatenbankAdmin.ItemClear();
        DatenbankAdmin.Check(db.DatenbankAdmin);

        txbKennwort.Text = db.GlobalShowPass;
        lbxSortierSpalten.ItemClear();

        if (db.SortDefinition != null) {
            btnSortRichtung.Checked = db.SortDefinition.Reverse;
            if (db.SortDefinition?.Columns != null) {
                foreach (var thisColumn in db.SortDefinition.Columns) {
                    if (thisColumn is { IsDisposed: false }) {
                        lbxSortierSpalten.AddAndCheck(ItemOf(thisColumn));
                    }
                }
            }
        }
        txbTags.Text = db.Tags.JoinWithCr();

        txbCaption.Text = db.Caption;
        txbAdditionalFiles.Text = db.AdditionalFilesPfad;
        txbStandardFormulaFile.Text = db.StandardFormulaFile;
        txbZeilenQuickInfo.Text = db.ZeilenQuickInfo.Replace("<br>", "\r");

        DatenbankAdmin.Suggestions.Clear();
        DatenbankAdmin.ItemAddRange(Table.Permission_AllUsed(false));

        lbxSortierSpalten.Suggestions.Clear();
        lbxSortierSpalten.Suggestions.AddRange(ItemsOf(db.Column, true));

        variableEditor.ToEdit = Database?.Variables;

        //GenerateUndoTabelle();

        GenerateInfoText();
    }

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        Close();
    }

    private void btnClipboard_Click(object sender, System.EventArgs e) => Generic.CopytoClipboard(tblUndo.Export_CSV(FirstRow.ColumnCaption));

    private void btnOptimize_Click(object sender, System.EventArgs e) => Database?.Optimize();

    private void btnSkripte_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        IUniqueWindowExtension.ShowOrCreate<DatabaseScriptEditor>(db);
        //var se = new_DatabaseScriptEditor(db);
        //_ = se.ShowDialog();
    }

    private void btnSpaltenAnordnungen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        var tcvc = ColumnViewCollection.ParseAll(db);
        tcvc[1].Edit();
        Table.RepairColumnArrangements(db);
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Database?.Column.GenerateOverView();

    private void btnTabellenAnsicht_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        var c = new TableView(db, false, true);
        c.ShowDialog();
    }

    private void butSystemspaltenErstellen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        db.Column.GenerateAndAddSystem();
    }

    private void GenerateInfoText() {
        if (IsDisposed || Database is not { IsDisposed: false }) {
            capInfo.Text = "Datenbank-Fehler";
            return;
        }

        var t = "<b>Tabelle:</b> <tab>" + Database.TableName + "<br>";
        t += "<b>Zeilen:</b> <tab>" + (Database.Row.Count() - 1) + "<br>";
        t += "<b>Temporärer Master:</b>  <tab>" + Database.TemporaryDatabaseMasterTimeUtc + " " + Database.TemporaryDatabaseMasterUser + "<br>";
        t += "<b>Letzte Komplettierung:</b> <tab>" + Database.FileStateUtcDate.ToString7() + "<br>";
        t += "<b>ID:</b> <tab>" + Database.ConnectionData.UniqueId + "<br>";
        capInfo.Text = t.TrimEnd("<br>");
    }

    //private void GlobalTab_Selecting(object sender, TabControlCancelEventArgs e) {
    //    if (e.TabPage == tabUndo) {
    //        if (tblUndo.Database is null) { GenerateUndoTabelle(); }
    //    }
    //}

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) {
        if (GlobalTab.SelectedTab == tabUndo && !UndoDone) {
            UndoDone = true;
            GenerateUndoTabelle(tblUndo);
            AddUndosToTable(tblUndo, Database, -1);
            tblUndo.Database?.Freeze("Nur Ansicht");
        }
    }

    private void OkBut_Click(object sender, System.EventArgs e) => Close();

    private void tblUndo_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (IsDisposed || sender is not Table { Database: { IsDisposed: false } db }) { return; }
        ColumnItem? column = null;
        if (e.HotItem is ColumnItem c) { column = c; }
        if (e.HotItem is string ck) { db.Cell.DataOfCellKey(ck, out column, out _); }

        e.ContextMenu.Add(ItemOf("Sortierung", true));
        e.ContextMenu.Add(ItemOf(ContextMenuCommands.SpaltenSortierungAZ, column != null && column.Function.CanBeChangedByRules()));
        e.ContextMenu.Add(ItemOf(ContextMenuCommands.SpaltenSortierungZA, column != null && column.Function.CanBeChangedByRules()));
    }

    private void tblUndo_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        if (IsDisposed || sender is not Table { Database: { IsDisposed: false } db } tbl) { return; }
        //RowItem? row = null;
        ColumnItem? column = null;
        //if (e.HotItem is RowItem r) { row = r; }
        if (e.HotItem is ColumnItem c) { column = c; }
        if (e.HotItem is string ck) { db.Cell.DataOfCellKey(ck, out column, out _); }

        if (column == null) { return; }

        switch (e.Item.KeyName) {
            case "SpaltenSortierungAZ":
                tbl.SortDefinitionTemporary = new RowSortDefinition(tbl.Database, column, false);
                break;

            case "SpaltenSortierungZA":
                tbl.SortDefinitionTemporary = new RowSortDefinition(tbl.Database, column, true);
                break;
        }
    }

    private void WriteInfosBack() {
        if (TableView.ErrorMessage(Database, EditableErrorReasonType.EditAcut) || Database is not { IsDisposed: false }) { return; }

        //eventScriptEditor.WriteScriptBack();
        Database.GlobalShowPass = txbKennwort.Text;
        Database.Caption = txbCaption.Text;
        //Database.UndoCount = txbUndoAnzahl.Text.IsLong() ? Math.Max(IntParse(txbUndoAnzahl.Text), 5) : 5;
        //if (txbGlobalScale.Text.IsDouble()) {
        //    Database.GlobalScale = Math.Min(FloatParse(txbGlobalScale.Text), 5);
        //    Database.GlobalScale = Math.Max(0.5f, Database.GlobalScale);
        //}
        Database.AdditionalFilesPfad = txbAdditionalFiles.Text;
        Database.StandardFormulaFile = txbStandardFormulaFile.Text;
        Database.ZeilenQuickInfo = txbZeilenQuickInfo.Text.Replace("\r", "<br>");

        Database.Tags = new(txbTags.Text.SplitAndCutByCrToList());

        Database.DatenbankAdmin = new(DatenbankAdmin.Checked);

        var tmp = PermissionGroups_NewRow.Checked.ToList();
        _ = tmp.Remove(Constants.Administrator);
        Database.PermissionGroupsNewRow = new(tmp);

        #region Sortierung

        var colnam = lbxSortierSpalten.Items.Select(thisk => (ColumnItem)((ReadableListItem)thisk).Item).ToList();
        Database.SortDefinition = new RowSortDefinition(Database, colnam, btnSortRichtung.Checked);

        #endregion

        #region Variablen

        // Identisch in DatabaseHeadEditor und DatabaseScriptEditor
        var l = variableEditor.GetCloneOfCurrent();

        if (l is { } vl) {
            var l2 = new List<VariableString>();
            foreach (var thisv in vl) {
                if (thisv is VariableString vs) {
                    l2.Add(vs);
                }
            }
            Database.Variables = new VariableCollection(l2);
        }

        #endregion
    }

    #endregion
}