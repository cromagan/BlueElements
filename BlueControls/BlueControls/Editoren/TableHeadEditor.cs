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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueTable;
using BlueTable.Enums;
using BlueTable.Interfaces;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

public sealed partial class TableHeadEditor : FormWithStatusBar, IHasTable, IIsEditor {

    #region Fields

    public bool UndoDone;
    private bool _frmHeadEditorFormClosingIsin;
    private Table? _table;

    #endregion Fields

    #region Constructors

    public TableHeadEditor() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        Table = null;
    }

    #endregion Constructors

    #region Properties

    public Table? Table {
        get => _table;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _table) { return; }

            if (_table != null) {
                _table.DisposingEvent -= _table_Disposing;
            }
            _table = value;

            if (_table != null) {
                _table.DisposingEvent += _table_Disposing;
            }
        }
    }

    public IEditable? ToEdit { set => Table = value is BlueTable.Table tb ? tb : null; }

    #endregion Properties

    #region Methods

    public static void AddUndosToTable(TableView tblUndo, Table? table, float maxAgeInDays) {
        if (table is { IsDisposed: false } db) {
            Develop.Message?.Invoke(ErrorType.Info, null, "?", ImageCode.Information, $"Erstelle Tabellen Ansicht des Undo-Speichers der Tabelle '{db.Caption}'", 0);

            List<UndoItem> un = [.. db.Undo]; // Kann und wird verändert!

            foreach (var thisUndo in un) {
                AddUndoToTable(tblUndo, thisUndo, db, maxAgeInDays);
            }
        }
    }

    public static void AddUndoToTable(TableView tblUndo, UndoItem work, Table db, float maxAgeInDays) {
        if (maxAgeInDays > 0 && DateTime.UtcNow.Subtract(work.DateTimeUtc).TotalDays > maxAgeInDays) { return; }
        var r = tblUndo.Table?.Row.GenerateAndAdd(work.ParseableItems().FinishParseable(), "New Undo Item");
        if (r == null) { return; }

        r.CellSet("ColumnName", work.ColName, string.Empty);
        r.CellSet("RowKey", work.RowKey, string.Empty);
        if (db.Column[work.ColName] is { IsDisposed: false } col) {
            r.CellSet("columnCaption", col.Caption, string.Empty);
        }
        if (db.Row.GetByKey(work.RowKey) is { IsDisposed: false } row) {
            r.CellSet("RowFirst", row.CellFirstString(), string.Empty);
        } else if (!string.IsNullOrEmpty(work.RowKey)) {
            r.CellSet("RowFirst", "[gelöscht]", string.Empty);
        }
        r.CellSet("Aenderer", work.User, string.Empty);
        r.CellSet("AenderZeit", work.DateTimeUtc, string.Empty);
        r.CellSet("Kommentar", work.Comment, string.Empty);

        r.CellSet("Table", db.Caption, string.Empty);

        if (work.Container.IsFormat(FormatHolder.FilepathAndName)) {
            r.CellSet("Herkunft", work.Container.FileNameWithoutSuffix(), string.Empty);
        }

        var symb = ImageCode.Fragezeichen;
        var alt = work.PreviousValue;
        var neu = work.ChangedTo;

        switch (work.Command) {
            case TableDataType.UTF8Value_withoutSizeData:
                symb = ImageCode.Stift;
                break;

            case TableDataType.TableVariables:
                alt = "[Variablen alt]";
                neu = "[Variablen neu]";
                symb = ImageCode.Variable;
                break;

            case TableDataType.EventScript:
                alt = "[Skript alt (" + alt.Length + " Zeichen)]";
                neu = "[Skript neu (" + neu.Length + " Zeichen)]";
                symb = ImageCode.Skript;
                break;

            //case TableDataType.EventScriptEdited:
            //    alt = "[Skript alt (" + alt.Length + " Zeichen)]";
            //    neu = "[Skript neu (" + neu.Length + " Zeichen)]";
            //    symb = ImageCode.Skript;
            //    break;

            case TableDataType.Command_AddRow:
                symb = ImageCode.PlusZeichen;
                break;

            case TableDataType.ColumnArrangement:
                symb = ImageCode.Spalte;
                alt = "[Spaltenanordnung alt]";
                neu = "[Spaltenanordnung neu]";
                break;

            case TableDataType.Command_RemoveRow:
                symb = ImageCode.MinusZeichen;
                break;

            case TableDataType.Command_NewStart:
                symb = ImageCode.Abspielen;
                break;

            case TableDataType.ColumnSystemInfo:
                symb = ImageCode.Information;
                break;

            case TableDataType.TemporaryTableMasterTimeUTC:
                symb = ImageCode.Uhr;
                break;

            case TableDataType.TemporaryTableMasterUser:
                symb = ImageCode.Person;
                break;

            case TableDataType.TemporaryTableMasterMachine:
                symb = ImageCode.Monitor;
                break;

            case TableDataType.TemporaryTableMasterApp:
                symb = ImageCode.Anwendung;
                break;

            case TableDataType.TemporaryTableMasterId:
                symb = ImageCode.Formel;

                break;
        }
        r.CellSet("Aenderung", work.Command.ToString(), string.Empty);
        r.CellSet("symbol", symb + "|24", string.Empty);
        r.CellSet("Wertalt", alt, string.Empty);
        r.CellSet("Wertneu", neu, string.Empty);
    }

    public static void GenerateUndoTabelle(TableView tblUndo) {
        var tb = new Table();
        //_ = x.Column.GenerateAndAdd("hidden", "hidden", ColumnFormatHolder.Text);
        var f = tb.Column.GenerateAndAdd("ID", "ID", ColumnFormatHolder.Text);
        f.IsFirst = true;
        _ = tb.Column.GenerateAndAdd("Table", "Tabelle", ColumnFormatHolder.Text);
        _ = tb.Column.GenerateAndAdd("ColumnName", "Spalten-<br>Name", ColumnFormatHolder.Text);
        _ = tb.Column.GenerateAndAdd("ColumnCaption", "Spalten-<br>Beschriftung", ColumnFormatHolder.Text);
        _ = tb.Column.GenerateAndAdd("RowKey", "Zeilen-<br>Schlüssel", ColumnFormatHolder.LongPositive);
        _ = tb.Column.GenerateAndAdd("RowFirst", "Zeile, Wert der<br>1. Spalte", ColumnFormatHolder.Text);
        var az = tb.Column.GenerateAndAdd("Aenderzeit", "Änder-<br>Zeit", ColumnFormatHolder.DateTime);
        _ = tb.Column.GenerateAndAdd("Aenderer", "Änderer", ColumnFormatHolder.Text);
        _ = tb.Column.GenerateAndAdd("Symbol", "Symbol", ColumnFormatHolder.BildCode);
        _ = tb.Column.GenerateAndAdd("Aenderung", "Änderung", ColumnFormatHolder.Text);
        _ = tb.Column.GenerateAndAdd("WertAlt", "Wert alt", ColumnFormatHolder.Text);
        _ = tb.Column.GenerateAndAdd("WertNeu", "Wert neu", ColumnFormatHolder.Text);
        _ = tb.Column.GenerateAndAdd("Kommentar", "Kommentar", ColumnFormatHolder.Text);
        _ = tb.Column.GenerateAndAdd("Herkunft", "Herkunft", ColumnFormatHolder.Text);
        foreach (var thisColumn in tb.Column) {
            if (!thisColumn.IsSystemColumn()) {
                thisColumn.MultiLine = true;
                thisColumn.EditableWithTextInput = false;
                thisColumn.EditableWithDropdown = false;
                thisColumn.DefaultRenderer = Renderer_TextOneLine.ClassId;
            }
        }

        if (az is { IsDisposed: false }) {
            var o = new Renderer_DateTime {
                UTCToLocal = true,
                ShowSymbol = true
            };
            az.DefaultRenderer = o.MyClassId;
            az.RendererSettings = o.ParseableItems().FinishParseable();
        }

        if (tb.Column["Symbol"] is { IsDisposed: false } c) {
            var o = new Renderer_ImageAndText {
                Text_anzeigen = false,
                Bild_anzeigen = true
            };
            c.DefaultRenderer = o.MyClassId;
            c.RendererSettings = o.ParseableItems().FinishParseable();
        }

        tb.RepairAfterParse();

        var tcvc = ColumnViewCollection.ParseAll(tb);
        tcvc[1].ShowColumns("Table", "ColumnName", "ColumnCaption", "RowKey", "RowFirst", "Aenderzeit", "Aenderer", "Symbol", "Aenderung", "WertAlt", "WertNeu", "Kommentar", "Herkunft");

        tb.ColumnArrangements = tcvc.ToString(false);

        //x.SortDefinition = new RowSortDefinition(db, "Index", true);

        tblUndo.TableSet(tb, string.Empty);
        tblUndo.Arrangement = string.Empty;
        tblUndo.SortDefinitionTemporary = new RowSortDefinition(tb, az, true);
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        if (_frmHeadEditorFormClosingIsin) { return; }
        _frmHeadEditorFormClosingIsin = true;
        base.OnFormClosing(e);

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        WriteInfosBack();
        Table = null;
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        if (IsDisposed || Table is not { IsDisposed: false } db) { return; }

        PermissionGroups_NewRow.ItemClear();
        PermissionGroups_NewRow.Check(db.PermissionGroupsNewRow);
        PermissionGroups_NewRow.Suggestions.Clear();
        PermissionGroups_NewRow.ItemAddRange(TableView.Permission_AllUsed(false));

        lbxTableAdmin.ItemClear();
        lbxTableAdmin.Check(db.TableAdmin);

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
        txbAdditionalFiles.Text = db.AdditionalFilesPath;
        txbStandardFormulaFile.Text = db.StandardFormulaFile;
        txbZeilenQuickInfo.Text = db.RowQuickInfo.Replace("<br>", "\r");

        lbxTableAdmin.Suggestions.Clear();
        lbxTableAdmin.ItemAddRange(TableView.Permission_AllUsed(false));

        lbxSortierSpalten.Suggestions.Clear();
        lbxSortierSpalten.Suggestions.AddRange(ItemsOf(db.Column, true));

        variableEditor.ToEdit = Table?.Variables;

        //GenerateUndoTabelle();

        GenerateInfoText();
    }

    private void _table_Disposing(object sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void btnClipboard_Click(object sender, System.EventArgs e) => Generic.CopytoClipboard(tblUndo.Export_CSV(FirstRow.ColumnCaption));

    private void btnLoadAll_Click(object sender, System.EventArgs e) {
        if (Table is not { IsDisposed: false }) { return; }
        Table.BeSureAllDataLoaded(-1);
    }

    private void btnMasterMe_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        tb.BeSureToBeUpToDate(false, true);
        tb.MasterMe();
        tb.BeSureToBeUpToDate(false, true);
        Close();
    }

    private void btnOptimize_Click(object sender, System.EventArgs e) => Table?.Optimize();

    private void btnSkripte_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return; }

        _ = IUniqueWindowExtension.ShowOrCreate<TableScriptEditor>(db);
        //var se = new_TableScriptEditor(db);
        //_ = se.ShowDialog();
    }

    private void btnSpaltenAnordnungen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return; }

        var tcvc = ColumnViewCollection.ParseAll(db);
        tcvc[1].Edit();
        _ = TableView.RepairColumnArrangements(db);
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Table?.Column.GenerateOverView();

    private void btnTabellenAnsicht_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return; }

        var c = new TableViewForm(db, false, true, true);
        _ = c.ShowDialog();
    }

    private void btnUnMaster_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        tb.BeSureToBeUpToDate(false, true);
        tb.UnMasterMe();
        tb.BeSureToBeUpToDate(false, false);
    }

    private void butSystemspaltenErstellen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return; }

        db.Column.GenerateAndAddSystem();
    }

    private void GenerateInfoText() {
        if (IsDisposed || Table is not { IsDisposed: false } tbl) {
            capInfo.Text = "Tabellen-Fehler";
            return;
        }

        var t = "<b>Tabelle:</b> <tab>" + tbl.KeyName + "<br>";
        t += "<b>Zeilen:</b> <tab>" + (tbl.Row.Count() - 1) + "<br>";
        t += $"<b>Temporärer Master:</b>  <tab>{tbl.TemporaryTableMasterTimeUtc} {Table.TemporaryTableMasterUser} {Table.TemporaryTableMasterMachine}<br>";

        t += "<b>Letzte Speicherung der Hauptdatei:</b> <tab>" + tbl.LastSaveMainFileUtcDate.ToString7() + " UTC<br>";

        capInfo.Text = t.TrimEnd("<br>");
    }

    //private void GlobalTab_Selecting(object sender, TabControlCancelEventArgs e) {
    //    if (e.TabPage == tabUndo) {
    //        if (tblUndo.Table is null) { GenerateUndoTabelle(); }
    //    }
    //}

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) {
        if (GlobalTab.SelectedTab == tabUndo && !UndoDone) {
            UndoDone = true;
            GenerateUndoTabelle(tblUndo);
            AddUndosToTable(tblUndo, Table, -1);
            tblUndo.Table?.Freeze("Nur Ansicht");
        }
    }

    private void OkBut_Click(object sender, System.EventArgs e) => Close();

    private void WriteInfosBack() {
        if (TableViewForm.EditabelErrorMessage(Table) || Table is not { IsDisposed: false }) { return; }

        //eventScriptEditor.WriteScriptBack();
        Table.GlobalShowPass = txbKennwort.Text;
        Table.Caption = txbCaption.Text;
        //Table.UndoCount = txbUndoAnzahl.Text.IsLong() ? Math.Max(IntParse(txbUndoAnzahl.Text), 5) : 5;
        //if (txbGlobalScale.Text.IsDouble()) {
        //    Table.GlobalScale = Math.Min(FloatParse(txbGlobalScale.Text), 5);
        //    Table.GlobalScale = Math.Max(0.5f, Table.GlobalScale);
        //}
        Table.AdditionalFilesPath = txbAdditionalFiles.Text;
        Table.StandardFormulaFile = txbStandardFormulaFile.Text;
        Table.RowQuickInfo = txbZeilenQuickInfo.Text.Replace("\r", "<br>");

        Table.Tags = new(txbTags.Text.SplitAndCutByCrToList());

        Table.TableAdmin = new(lbxTableAdmin.Checked);

        var tmp = PermissionGroups_NewRow.Checked.ToList();
        _ = tmp.Remove(Constants.Administrator);
        Table.PermissionGroupsNewRow = new(tmp);

        #region Sortierung

        var colnam = lbxSortierSpalten.Items.Select(thisk => (ColumnItem)((ReadableListItem)thisk).Item).ToList();
        Table.SortDefinition = new RowSortDefinition(Table, colnam, btnSortRichtung.Checked);

        #endregion Sortierung

        #region Variablen

        // Identisch in TableHeadEditor und TableScriptEditor
        var l = variableEditor.GetCloneOfCurrent();

        if (l is { } vl) {
            var l2 = new List<VariableString>();
            foreach (var thisv in vl) {
                if (thisv is VariableString vs) {
                    l2.Add(vs);
                }
            }
            Table.Variables = new VariableCollection(l2);
        }

        #endregion Variablen
    }

    #endregion Methods
}