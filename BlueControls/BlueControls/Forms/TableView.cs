// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueBasics.MultiUserFile;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueBasics.Develop;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class TableView : FormWithStatusBar, IHasSettings {

    #region Fields

    private bool _firstOne = true;

    #endregion

    #region Constructors

    public TableView() : this(null, true, true) { }

    public TableView(Database? database, bool loadTabVisible, bool adminTabVisible) : base() {
        InitializeComponent();

        if (!adminTabVisible) {
            grpAdminAllgemein.Visible = false;
        }

        if (!loadTabVisible) {
            ribMain.Controls.Remove(tabFile);
        }

        if (btnDrucken != null) {
            btnDrucken.ItemClear();
            btnDrucken.ItemAdd(ItemOf("Drucken bzw. Export", "erweitert", QuickImage.Get(ImageCode.Drucker, 28)));
            btnDrucken.ItemAdd(Separator());
            btnDrucken.ItemAdd(ItemOf("CSV-Format für Excel in die Zwischenablage", "csv", QuickImage.Get(ImageCode.Excel, 28)));
            btnDrucken.ItemAdd(ItemOf("HTML-Format für Internet-Seiten", "html", QuickImage.Get(ImageCode.Globus, 28)));
            btnDrucken.ItemAdd(Separator());
            btnDrucken.ItemAdd(ItemOf("Layout-Editor öffnen", "editor", QuickImage.Get(ImageCode.Layout, 28)));
        }

        Check_OrderButtons();

        this.LoadSettingsFromDisk(false);

        _ = SwitchTabToDatabase(database);
    }

    #endregion

    #region Properties

    public static bool SettingsLoadedStatic { get; set; }
    public static List<string> SettingsStatic { get; set; } = new();

    public string PreveredDatabaseID { get; set; } = Database.DatabaseId;

    public List<string> Settings { get => SettingsStatic; set => SettingsStatic = value; }

    public bool SettingsLoaded { get => SettingsLoadedStatic; set => SettingsLoadedStatic = value; }

    public string SettingsManualFilename { get => Application.StartupPath + "\\" + Name + "-Settings.ini"; set { } }

    #endregion

    #region Methods

    public static void ContextMenuInit(Table? tbl, ColumnItem? column, RowItem? row, ContextMenuInitEventArgs e) {
        var db = tbl?.Database ?? column?.Database ?? row?.Database;
        if (db == null) { return; }

        var editable = string.IsNullOrEmpty(CellCollection.EditableErrorReason(column, row, EditableErrorReasonType.EditNormaly, true, false, true, false));

        if (tbl != null && row != null) {
            e.ContextMenu.Add(ItemOf("Anheften", true));
            if (tbl.PinnedRows.Contains(row)) {
                e.ContextMenu.Add(ItemOf("Zeile nicht mehr pinnen", "pinlösen", QuickImage.Get(ImageCode.Pinnadel, 16), true));
            } else {
                e.ContextMenu.Add(ItemOf("Zeile anpinnen", "anpinnen", QuickImage.Get(ImageCode.Pinnadel, 16), true));
            }
        }

        if (column != null) {
            e.ContextMenu.Add(ItemOf("Sortierung", true));
            e.ContextMenu.Add(ItemOf(ContextMenuCommands.SpaltenSortierungDefault, column.Function.CanBeCheckedByRules()));

            e.ContextMenu.Add(ItemOf(ContextMenuCommands.SpaltenSortierungAZ, column.Function.CanBeCheckedByRules()));
            e.ContextMenu.Add(ItemOf(ContextMenuCommands.SpaltenSortierungZA, column.Function.CanBeCheckedByRules()));
            //_ = e.CurrentMenu.Add(AddSeparator());

            e.ContextMenu.Add(ItemOf("Zelle", true));
            e.ContextMenu.Add(ItemOf("Inhalt Kopieren", "ContentCopy", ImageCode.Kopieren, column.Function.CanBeChangedByRules()));
            e.ContextMenu.Add(ItemOf("Inhalt Einfügen", "ContentPaste", ImageCode.Clipboard, editable && column.Function.CanBeChangedByRules()));
            e.ContextMenu.Add(ItemOf("Inhalt löschen", "ContentDelete", ImageCode.Radiergummi, editable && column.Function.CanBeChangedByRules()));
            e.ContextMenu.Add(ItemOf(ContextMenuCommands.VorherigenInhaltWiederherstellen, editable && column.Function.CanBeChangedByRules() && column.ShowUndo));
            e.ContextMenu.Add(ItemOf(ContextMenuCommands.SuchenUndErsetzen, db.IsAdministrator()));
            //_ = e.CurrentMenu.Add(AddSeparator());
            e.ContextMenu.Add(ItemOf("Spalte", true));
            e.ContextMenu.Add(ItemOf(ContextMenuCommands.SpaltenEigenschaftenBearbeiten, db.IsAdministrator()));

            e.ContextMenu.Add(ItemOf("Gesamten Spalteninhalt kopieren", "CopyAll", ImageCode.Clipboard, db.IsAdministrator()));
            e.ContextMenu.Add(ItemOf("Gesamten Spalteninhalt kopieren + sortieren", "CopyAll2", ImageCode.Clipboard, db.IsAdministrator()));

            e.ContextMenu.Add(ItemOf("Statistik", "Statistik", QuickImage.Get(ImageCode.Balken, 16), db.IsAdministrator()));
            e.ContextMenu.Add(ItemOf("Summe", "Summe", ImageCode.Summe, db.IsAdministrator()));

            e.ContextMenu.Add(ItemOf("Voting", "Voting", ImageCode.Herz, db.IsAdministrator() && editable && column.Function.CanBeChangedByRules()));

            //_ = e.CurrentMenu.Add(AddSeparator());
        }

        if (row != null) {
            e.ContextMenu.Add(ItemOf("Zeile", true));
            e.ContextMenu.Add(ItemOf(ContextMenuCommands.ZeileLöschen, db.IsAdministrator()));
            e.ContextMenu.Add(ItemOf("Komplette Datenüberprüfung", "#datenüberprüfung", QuickImage.Get(ImageCode.HäkchenDoppelt, 16), true));

            var didmenu = false;

            foreach (var thiss in db.EventScript) {
                if (thiss != null && thiss.UserGroups.Count > 0 && db.PermissionCheck(thiss.UserGroups, null) && thiss.NeedRow && thiss.IsOk()) {
                    if (!didmenu) {
                        e.ContextMenu.Add(ItemOf("Skripte", true));
                        didmenu = true;
                    }
                    e.ContextMenu.Add(ItemOf("Skript: " + thiss.ReadableText(), "Skript|" + thiss.KeyName, thiss.SymbolForReadableText(), row != null && thiss.IsOk()));
                }
            }
        }
    }

    public static void ContextMenuItemClicked(Table? tbl, ColumnItem? column, RowItem? row, ContextMenuItemClickedEventArgs e) {
        var db = tbl?.Database ?? column?.Database ?? row?.Database;
        if (db == null) { return; }

        var valueCol0 = string.Empty;
        if (row != null && !row.IsDisposed) {
            valueCol0 = row.CellFirstString();
        }

        //var editable = string.IsNullOrEmpty(CellCollection.ErrorReason(column, row, ErrorReason.EditAcut));

        var ev = (e.Item.KeyName + "|").SplitAndCutBy("|");

        switch (ev[0]) {
            case "pinlösen":
                tbl?.PinRemove(row);
                break;

            case "anpinnen":
                tbl?.PinAdd(row);
                break;

            case "SpaltenSortierungDefault":
                if (tbl != null) { tbl.SortDefinitionTemporary = null; }
                break;

            case "SpaltenSortierungAZ":
                if (tbl != null) { tbl.SortDefinitionTemporary = new RowSortDefinition(db, column, false); }
                break;

            case "SpaltenSortierungZA":
                if (tbl != null) { tbl.SortDefinitionTemporary = new RowSortDefinition(db, column, true); }
                break;

            case "Skript":
                if (row != null && !row.IsDisposed) {
                    var t = row.ExecuteScript(null, ev[1], true, true, true, 10, null, true, false).Protocol.JoinWithCr();
                    if (string.IsNullOrEmpty(t)) {
                        MessageBox.Show("Skript fehlerfrei ausgeführt.", ImageCode.Häkchen, "Ok");
                    } else {
                        MessageBox.Show("Wähernd der Skript-Ausführung sind<br>Fehler aufgetreten:<br><br>" + t, ImageCode.Kreuz, "Ok");
                    }
                }

                break;

            case "ZeileLöschen":
                if (ErrorMessage(db, EditableErrorReasonType.EditCurrently)) { return; }
                if (!db.IsAdministrator()) { return; }
                if (row == null || row.IsDisposed) { return; }

                if (MessageBox.Show("Zeile wirklich löschen? (<b>" + valueCol0 + "</b>)", ImageCode.Frage, "Ja", "Nein") == 0) {
                    _ = db.Row.Remove(row, "Benutzer: löschen Befehl");
                }

                break;

            case "ContentDelete":
                if (ErrorMessage(db, EditableErrorReasonType.EditCurrently)) { return; }

                row?.CellSet(column, string.Empty, "Inhalt Löschen Kontextmenu");
                //tbl.Database.Cell.Delete(column, row?.KeyName);
                break;

            case "SpaltenEigenschaftenBearbeiten":
                OpenColumnEditor(column, row, tbl);
                //CheckButtons();
                break;

            case "ContentCopy":
                Table.CopyToClipboard(column, row, true);
                break;

            case "SuchenUndErsetzen":
                if (!db.IsAdministrator()) { return; }

                tbl?.OpenSearchAndReplaceInCells();
                break;

            case "Summe":
                if (!db.IsAdministrator() || tbl == null) { return; }

                var summe = column?.Summe(tbl.Filter);
                if (!summe.HasValue) {
                    MessageBox.Show("Die Summe konnte nicht berechnet werden.", ImageCode.Summe, "OK");
                } else {
                    MessageBox.Show("Summe dieser Spalte, nur angezeigte Zeilen: <br><b>" + summe, ImageCode.Summe, "OK");
                }

                break;

            case "Voting":
                if (!db.IsAdministrator() || tbl == null || column == null) { return; }

                var v = new Voting(column, [.. tbl.Filter.Rows]);
                v.ShowDialog();
                break;

            case "Statistik":
                if (!db.IsAdministrator() || column == null || tbl == null) { return; }

                var split = false;
                if (column.MultiLine) {
                    split = MessageBox.Show("Zeilen als Ganzes oder aufsplitten?", ImageCode.Frage, "Ganzes", "Splitten") != 0;
                }

                column.Statistik(tbl.RowsVisibleUnique(), !split);
                break;

            case "VorherigenInhaltWiederherstellen":
                if (ErrorMessage(db, EditableErrorReasonType.EditCurrently)) { return; }

                Table.DoUndo(column, row);
                break;

            case "#datenüberprüfung":
                if (row != null && !row.IsDisposed) {
                    row.InvalidateRowState("TableView, Kontextmenü, Datenüberprüfung");
                    row.UpdateRow(true, true, "TableView, Kontextmenü, Datenüberprüfung");
                    RowCollection.DoAllInvalidatedRows(row, true);
                    //row.CheckRowDataIfNeeded();
                    MessageBox.Show("Datenüberprüfung:\r\n" + row.LastCheckedMessage, ImageCode.HäkchenDoppelt, "Ok");
                }
                break;

            case "CopyAll": {
                    if (!db.IsAdministrator() || column == null || tbl == null) { return; }
                    var txt = tbl.Export_CSV(FirstRow.Without, column);
                    //txt = txt.TrimStart("Deutsch;\r\n");
                    //txt = txt.TrimStart("Englisch;\r\n");
                    txt = txt.Replace("|", "\r\n");
                    txt = txt.Replace(";", string.Empty);
                    _ = CopytoClipboard(txt);
                    Notification.Show("Die Daten sind nun<br>in der Zwischenablage.", ImageCode.Clipboard);
                    break;
                }
            case "CopyAll2": {
                    if (!db.IsAdministrator() || column == null || tbl == null) { return; }
                    var txt = tbl.Export_CSV(FirstRow.Without, column);
                    //txt = txt.TrimStart("Deutsch;\r\n");
                    //txt = txt.TrimStart("Englisch;\r\n");
                    txt = txt.Replace("|", "\r\n");
                    txt = txt.Replace(";", string.Empty);
                    var l = txt.SplitAndCutByCrToList().SortedDistinctList().JoinWithCr();
                    _ = CopytoClipboard(l);
                    Notification.Show("Die Daten sind nun<br>in der Zwischenablage.", ImageCode.Clipboard);
                    break;
                }
        }
    }

    /// <summary>
    /// Gibt TRUE zuück, wenn eine Fehlernachricht angezeigt wurde.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static bool ErrorMessage(Database? database, EditableErrorReasonType mode) {
        var m = Database.EditableErrorReason(database, mode);
        if (string.IsNullOrEmpty(m)) { return false; }

        MessageBox.Show("Aktion nicht möglich:<br>" + m);
        return true;
    }

    public static void OpenColumnEditor(ColumnItem? column, RowItem? row, Table? tableview) {
        if (column == null || column.IsDisposed) { return; }

        if (row == null || row.IsDisposed) {
            OpenColumnEditor(column, tableview);
            return;
        }

        ColumnItem? columnLinked = null;
        var posError = false;
        switch (column.Function) {
            case ColumnFunction.Verknüpfung_zu_anderer_Datenbank2:
            case ColumnFunction.Verknüpfung_zu_anderer_Datenbank:
            case ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems:
                (columnLinked, _, _, _) = CellCollection.LinkedCellData(column, row, true, false);
                posError = true;
                break;
        }

        var bearbColumn = column;
        if (columnLinked != null) {
            columnLinked.Repair();
            if (MessageBox.Show("Welche Spalte bearbeiten?", ImageCode.Frage, "Spalte in dieser Datenbank",
                    "Verlinkte Spalte") == 1) {
                bearbColumn = columnLinked;
            }
        } else {
            if (posError) {
                Notification.Show(
                    "Keine aktive Verlinkung.<br>Spalte in dieser Datenbank wird angezeigt.<br><br>Ist die Ziel-Zelle in der Ziel-Datenbank vorhanden?",
                    ImageCode.Information);
            }
        }

        column.Repair();
        OpenColumnEditor(bearbColumn, tableview);
        bearbColumn.Repair();
    }

    public static void OpenColumnEditor(ColumnItem? column, Table? tableview) {
        if (column == null || column.IsDisposed) { return; }
        column.Editor = typeof(ColumnEditor);

        using ColumnEditor w = new(column, tableview);
        _ = w.ShowDialog();
    }

    public static void OpenLayoutEditor(Database db, string layoutToOpen) {
        var x = db.EditableErrorReason(EditableErrorReasonType.EditNormaly);
        if (!string.IsNullOrEmpty(x)) {
            MessageBox.Show(x);
            return;
        }

        var w = new PadEditorWithFileAccess();

        if (!string.IsNullOrWhiteSpace(layoutToOpen)) {
            w.LoadFile(layoutToOpen);
        }

        _ = w.ShowDialog();
    }

    public static void OpenScriptEditor(Database? db) {
        if (db == null || db.IsDisposed) { return; }

        var se = new DatabaseScriptEditor(db);
        _ = se.ShowDialog();
    }

    /// <summary>
    /// Setzt von allen Reitern die Ansichts- und Filtereinstellungen zurück
    /// </summary>
    public void ResetDatabaseSettings() {
        foreach (var thisT in tbcDatabaseSelector.TabPages) {
            if (thisT is TabPage tp && tp.Tag is List<object> s) {
                s[1] = string.Empty;
                tp.Tag = s;
            }
        }
    }

    /// <summary>
    /// Erstellt einen Reiter mit den nötigen Tags um eine Datenbank laden zu können - lädt die Datenbank aber selbst nicht.
    /// HIer wird auch die Standard-Ansicht als Tag Injiziert
    /// </summary>
    protected void AddTabPage(ConnectionInfo? ci, string settings) {
        if (ci is null) { return; }

        var nTabPage = new TabPage {
            Name = tbcDatabaseSelector.TabCount.ToString(),
            Text = ci.TableName.ToTitleCase(),
            Tag = new List<object> { ci, settings }
        };
        tbcDatabaseSelector.Controls.Add(nTabPage);
    }

    protected virtual void btnCSVClipboard_Click(object sender, System.EventArgs e) {
        _ = CopytoClipboard(Table.Export_CSV(FirstRow.ColumnCaption));
        Notification.Show("Die Daten sind nun<br>in der Zwischenablage.", ImageCode.Clipboard);
    }

    protected virtual void btnDrucken_ItemClicked(object sender, AbstractListItemEventArgs e) {
        MultiUserFile.SaveAll(false);
        Database.ForceSaveAll();
        if (IsDisposed || Table.Database is not Database db || db.IsDisposed) { return; }

        switch (e.Item.KeyName) {
            case "erweitert":
                Visible = false;
                var selectedRows = Table.RowsVisibleUnique();

                using (ExportDialog l = new(db, selectedRows)) {
                    _ = l.ShowDialog();
                }

                Visible = true;
                break;

            case "csv":
                _ = CopytoClipboard(Table.Export_CSV(FirstRow.ColumnCaption));
                MessageBox.Show("Die gewünschten Daten<br>sind nun im Zwischenspeicher.", ImageCode.Clipboard, "Ok");
                break;

            case "html":
                Table.Export_HTML();
                break;

            default:
                DebugPrint(e.Item);
                break;
        }
    }

    protected virtual void btnHTMLExport_Click(object sender, System.EventArgs e) => Table.Export_HTML();

    protected virtual void btnSkripteBearbeiten_Click(object sender, System.EventArgs e) {
        OpenScriptEditor(Table.Database);
        UpdateScripts(Table.Database);
    }

    protected void ChangeDatabaseInTab(ConnectionInfo connectionId, TabPage? xtab) {
        if (xtab == null) {
            return;
        }

        tbcDatabaseSelector.Enabled = false;
        Table.Enabled = false;
        Table.ShowWaitScreen = true;
        Table.Refresh();

        var s = (List<object>)xtab.Tag;
        s[0] = connectionId;
        s[1] = string.Empty;
        xtab.Tag = s;
        tbcDatabaseSelector_Selected(null,
            new TabControlEventArgs(xtab, tbcDatabaseSelector.TabPages.IndexOf(xtab),
                TabControlAction.Selected));
    }

    protected void CheckButtons() {
        if (IsDisposed) { return; }
        var datenbankDa = Table.Database != null && !Table.Database.IsDisposed;
        btnNeuDB.Enabled = true;
        btnOeffnen.Enabled = true;
        btnDrucken.Enabled = datenbankDa;

        if (Table.Database is Database db && !db.IsDisposed) {
            btnDatenbankenSpeicherort.Enabled = !string.IsNullOrEmpty(db.Filename);
        } else {
            btnDatenbankenSpeicherort.Enabled = false;
        }

        btnZeileLöschen.Enabled = datenbankDa;
        lstAufgaben.Enabled = datenbankDa;
        btnSaveAs.Enabled = datenbankDa;

        if (btnDrucken["csv"] is AbstractListItem bli1) {
            bli1.Enabled = datenbankDa;
        }

        if (btnDrucken["html"] is AbstractListItem bli2) {
            bli2.Enabled = datenbankDa;
        }

        btnSuchenUndErsetzen.Enabled = datenbankDa;
        FilterLeiste.Enabled = datenbankDa;
    }

    protected virtual void DatabaseSet(Database? db, string toParse) {
        if (db != null && !db.IsDisposed) {
            DropMessages = db.IsAdministrator();
        }

        if (Table.Database != db) {
            CFO.InitFormula(null, null);
        }

        FilterLeiste.Table = Table;
        var did = false;

        if (!string.IsNullOrEmpty(toParse)) {
            foreach (var pair in toParse.GetAllTags()) {
                switch (pair.Key) {
                    case "tableview":
                        Table.DatabaseSet(db, pair.Value.FromNonCritical());
                        did = true;
                        break;

                    case "maintab":
                        ribMain.SelectedIndex = IntParse(pair.Value);
                        break;

                    case "splitterx":
                        SplitContainer1.SplitterDistance = IntParse(pair.Value);
                        break;

                    case "windowstate":
                        //WindowState = (FormWindowState)IntParse(pair.Value);
                        break;

                    default:
                        DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        if (!did) {
            Table.DatabaseSet(db, string.Empty);
            if (Table.View_RowFirst() != null && db != null) {
                Table.CursorPos_Set(Table.View_ColumnFirst(), Table.View_RowFirst(), false);
            }
        }

        Check_OrderButtons();

        Table.ShowWaitScreen = false;
        tbcDatabaseSelector.Enabled = true;
        Table.Enabled = true;
    }

    protected virtual void FillFormula(RowItem? r) {
        if (CFO is null) { return; }

        CFO.GetConnectedFormulaFromDatabase(r?.Database);
        CFO.InitFormula(CFO.ConnectedFormula, r?.Database);
        CFO.SetToRow(r);
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);

        if (Table.Database is Database db && !db.IsDisposed) {
            this.SetSetting("View_" + db.TableName, ViewToString());
        }

        DatabaseSet(null, string.Empty);
        MultiUserFile.SaveAll(true);
        Database.ForceSaveAll();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        CheckButtons();
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        InitView();
    }

    /// <summary>
    /// Sucht den Tab mit der angegebenen Datenbank.
    /// Ist kein Reiter vorhanden, wird ein Neuer erzeugt.
    /// </summary>
    /// <returns></returns>
    protected bool SwitchTabToDatabase(ConnectionInfo? connectionInfo) {
        if (connectionInfo is null) { return false; }

        connectionInfo.Editor = typeof(DatabaseHeadEditor);

        foreach (var thisT in tbcDatabaseSelector.TabPages) {
            if (thisT is TabPage tp && tp.Tag is List<object> s && s[0] is ConnectionInfo ci) {
                if (ci.UniqueId.Equals(connectionInfo.UniqueId, StringComparison.OrdinalIgnoreCase)) {
                    tbcDatabaseSelector.SelectedTab = tp; // tbcDatabaseSelector_Selected macht die eigentliche Arbeit

                    if (_firstOne) {
                        _firstOne = false;
                        tbcDatabaseSelector_Selected(null,
                            new TabControlEventArgs(tp, tbcDatabaseSelector.TabPages.IndexOf(tp),
                                TabControlAction.Selected));
                    }

                    return true;
                }
            }
        }

        var settings = this.GetSettings("View_" + connectionInfo.TableName);

        AddTabPage(connectionInfo, settings);
        return SwitchTabToDatabase(connectionInfo); // Rekursiver Aufruf, nun sollt der Tab ja gefunden werden.
    }

    /// <summary>
    /// Sucht den Tab mit der angegebenen Datenbank.
    /// Ist kein Reiter vorhanden, wird ein Neuer erzeugt.
    /// </summary>
    /// <returns></returns>
    protected bool SwitchTabToDatabase(Database? database) {
        if (database is null || database.IsDisposed) {
            return false;
        }

        return SwitchTabToDatabase(database.ConnectionData);
    }

    protected virtual void Table_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
        if (IsDisposed || sender is not Table tbl || tbl.Database is not Database db || db.IsDisposed) { return; }
        RowItem? row = null;
        ColumnItem? column = null;
        if (e.HotItem is RowItem r) { row = r; }
        if (e.HotItem is ColumnItem c) { column = c; }
        if (e.HotItem is string ck) { db.Cell.DataOfCellKey(ck, out column, out row); }
        ContextMenuInit(tbl, column, row, e);
    }

    protected virtual void Table_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        if (sender is not Table tbl || tbl.Database is not Database db || db.IsDisposed) { return; }
        RowItem? row = null;
        ColumnItem? column = null;
        if (e.HotItem is RowItem r) { row = r; }
        if (e.HotItem is ColumnItem c) { column = c; }
        if (e.HotItem is string ck) { db.Cell.DataOfCellKey(ck, out column, out row); }
        ContextMenuItemClicked(tbl, column, row, e);
    }

    protected virtual void Table_DatabaseChanged(object sender, System.EventArgs e) {
        Table.WriteColumnArrangementsInto(cbxColumnArr, Table.Database, Table.Arrangement);
        //ChangeDatabase(Table.Database);
        Check_OrderButtons();
        CheckButtons();
    }

    protected void Table_EnabledChanged(object sender, System.EventArgs e) => Check_OrderButtons();

    protected virtual void Table_SelectedCellChanged(object sender, CellExtEventArgs e) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => Table_SelectedCellChanged(sender, e)));
            return;
        }

        if (ckbZeilenclickInsClipboard.Checked) {
            Table.CopyToClipboard(e.Column, e.RowData?.Row, false);
            Table.Focus();
        }
    }

    protected virtual void Table_SelectedRowChanged(object sender, RowNullableEventArgs e) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => Table_SelectedRowChanged(sender, e)));
            return;
        }

        btnUnterschiede_CheckedChanged(this, System.EventArgs.Empty);

        FillFormula(e.Row);
    }

    protected void Table_ViewChanged(object sender, System.EventArgs e) =>
        Table.WriteColumnArrangementsInto(cbxColumnArr, Table.Database, Table.Arrangement);

    protected virtual void Table_VisibleRowsChanged(object sender, System.EventArgs e) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => Table_VisibleRowsChanged(sender, e)));
            return;
        }

        if (Table.Database != null) {
            capZeilen1.Text = "<IMAGECODE=Information|16> " + LanguageTool.DoTranslate("Einzigartige Zeilen:") + " " +
                              Table.VisibleRowCount + " " + LanguageTool.DoTranslate("St.");
        } else {
            {
                capZeilen1.Text = string.Empty;
            }
        }

        capZeilen1.Refresh(); // Backgroundworker lassen wenig luft
        capZeilen2.Text = capZeilen1.Text;
        capZeilen2.Refresh();
    }

    protected virtual string ViewToString() {
        //Reihenfolge wichtig, da die Ansicht vieles auf standard zurück setzt

        List<string> result = [];
        result.ParseableAdd("WindowState", WindowState);
        result.ParseableAdd("SplitterX", SplitContainer1.SplitterDistance);
        result.ParseableAdd("MainTab", ribMain.SelectedIndex);
        result.ParseableAdd("TableView", Table.ViewToString());

        return result.Parseable();
    }

    private void btnAlleErweitern_Click(object sender, System.EventArgs e) => Table.ExpandAll();

    //    Table.Database = db;
    //    if (Table.Database != null) {
    //        tbcDatabaseSelector.TabPages[toIndex].Text = db.Filename.FileNameWithoutSuffix();
    //        ParseView(DBView[toIndex]);
    //    }
    //    tbcDatabaseSelector.Enabled = true;
    //    Table.Enabled = true;
    //}
    //private void _originalDB_Disposing(object sender, System.EventArgs e) => ChangeDatabase(null);
    private void btnAlleSchließen_Click(object sender, System.EventArgs e) => Table.CollapesAll();

    private void btnAufräumen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Database is not Database db || db.IsDisposed || !db.IsAdministrator()) {
            return;
        }

        Table.RowCleanUp();
    }

    private void btnClipboardImport_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Database is not Database db || db.IsDisposed || !db.IsAdministrator()) {
            return;
        }

        Table.ImportClipboard();
    }

    private void btnDatenbankenSpeicherort_Click(object sender, System.EventArgs e) {
        Database.ForceSaveAll();
        MultiUserFile.SaveAll(false);

        if (Table.Database is Database db && !db.IsDisposed) {
            _ = ExecuteFile(db.Filename.FilePath());
        }
    }

    private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Show(Table.Database, typeof(DatabaseHeadEditor), false);

    private void btnFormular_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (IsDisposed || Table.Database is not Database db || db.IsDisposed) {
            return;
        }

        using var x = new ConnectedFormulaEditor(db.FormulaFileName(), null);
        x.ShowDialog();
    }

    private void btnLayouts_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (IsDisposed || Table.Database is not Database db || db.IsDisposed) { return; }

        OpenLayoutEditor(db, string.Empty);
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) {
        MultiUserFile.SaveAll(false);
        Database.ForceSaveAll();

        _ = SwitchTabToDatabase(new ConnectionInfo(e.Item.KeyName, PreveredDatabaseID, string.Empty));
    }

    private void btnMDBImport_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Database is not Database db || db.IsDisposed || !db.IsAdministrator()) {
            return;
        }

        Table.ImportBdb();
    }

    private void btnNeuDB_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        Database.ForceSaveAll();

        _ = SaveTab.ShowDialog();
        if (!DirectoryExists(SaveTab.FileName.FilePath())) {
            return;
        }

        if (string.IsNullOrEmpty(SaveTab.FileName)) {
            return;
        }

        if (FileExists(SaveTab.FileName)) {
            _ = DeleteFile(SaveTab.FileName, true);
        }

        var db = new Database(SaveTab.FileName.FileNameWithoutSuffix());
        db.SaveAsAndChangeTo(SaveTab.FileName);
        _ = SwitchTabToDatabase(new ConnectionInfo(SaveTab.FileName, PreveredDatabaseID, string.Empty));
    }

    private void btnNummerierung_CheckedChanged(object sender, System.EventArgs e) => Table.ShowNumber = btnNummerierung.Checked;

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        Database.ForceSaveAll();
        _ = LoadTab.ShowDialog();
    }

    private void btnPowerBearbeitung_Click(object sender, System.EventArgs e) {
        Notification.Show("5 Minuten (fast) rechtefreies<br>Bearbeiten aktiviert.", ImageCode.Stift);
        Table.PowerEdit = DateTime.UtcNow.AddSeconds(300);
    }

    private void btnSaveAs_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        Database.ForceSaveAll();

        if (Table.Database is Database db && !db.IsDisposed) {
            if (db.ReadOnly) { return; }
            _ = SaveTab.ShowDialog();
            if (!DirectoryExists(SaveTab.FileName.FilePath())) { return; }

            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

            if (FileExists(SaveTab.FileName)) {
                _ = DeleteFile(SaveTab.FileName, true);
            }

            db.SaveAsAndChangeTo(SaveTab.FileName);
            _ = SwitchTabToDatabase(new ConnectionInfo(SaveTab.FileName, PreveredDatabaseID, string.Empty));
        }
    }

    private void btnSaveLoad_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(true);
        Database.ForceSaveAll();
        Database.CheckSysUndoNow(Database.AllFiles, true);
    }

    private void btnSpaltenanordnung_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Database is not Database db || db.IsDisposed) { return; }

        var x = new ColumnArrangementPadEditor(db);
        _ = x.ShowDialog();

        if (db.ColumnArrangements.Count == 0) { return; }

        var car = db.ColumnArrangements.CloneWithClones();
        if (car[0] is ColumnViewCollection cvc) {
            cvc.ShowAllColumns();
            db.ColumnArrangements = new(car);
        }

        Table.Invalidate_AllColumnArrangements();
    }

    private void btnSpaltenUebersicht_Click(object sender, System.EventArgs e) => Table.Database?.Column.GenerateOverView();

    private void btnSuchenUndErsetzen_Click(object sender, System.EventArgs e) => Table.OpenSearchAndReplaceInCells();

    private void btnSuchFenster_Click(object sender, System.EventArgs e) {
        var x = new Search(Table);
        x.Show();
    }

    private void btnSuchInScript_Click(object sender, System.EventArgs e) => Table.OpenSearchAndReplaceInDBScripts();

    private void btnTemporärenSpeicherortÖffnen_Click(object sender, System.EventArgs e) {
        Database.ForceSaveAll();
        MultiUserFile.SaveAll(false);
        _ = ExecuteFile(Path.GetTempPath());
    }

    private void btnUnterschiede_CheckedChanged(object sender, System.EventArgs e) =>
        Table.Unterschiede = btnUnterschiede.Checked ? Table.CursorPosRow?.Row : null;

    private void btnUserInfo_Click(object sender, System.EventArgs e) {
        var t = new UserInfo();
        t.Show();
    }

    private void btnZeileLöschen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Database is not Database db || db.IsDisposed) { return; }
        if (!db.IsAdministrator()) { return; }

        var m = MessageBox.Show("Angezeigte Zeilen löschen?", ImageCode.Warnung, "Ja", "Nein");
        if (m != 0) {
            return;
        }

        _ = db.Row.Remove(Table.Filter, Table.PinnedRows, "Benutzer: Zeile löschen");
    }

    private void cbxColumnArr_ItemClicked(object sender, AbstractListItemEventArgs e) => Table.Arrangement = e.Item.KeyName;

    private void Check_OrderButtons() {
        if (IsDisposed) { return; }
        if (InvokeRequired) {
            _ = Invoke(new Action(Check_OrderButtons));
            return;
        }

        if (Table.Database is not Database db || db.IsDisposed || !db.IsAdministrator()) {
            tabAdmin.Enabled = false;
            return; // Weitere funktionen benötigen sicher eine Datenbank um keine Null Exception auszulösen
        }

        var m = Database.EditableErrorReason(db, EditableErrorReasonType.EditCurrently);

        grpAdminAllgemein.Enabled = string.IsNullOrEmpty(m);
        grpImport.Enabled = string.IsNullOrEmpty(m);
        tabAdmin.Enabled = string.IsNullOrEmpty(m);
    }

    private void InitView() {
        if (DesignMode) {
            tbcSidebar.Visible = true;
            grpHilfen.Visible = true;
            grpAnsicht.Visible = true;
            FilterLeiste.Visible = true;
            SplitContainer1.Panel2Collapsed = false;
            return;
        }

        Table.Filter.Clear();

        tbcSidebar.Visible = true;
        grpHilfen.Visible = true;
        grpAnsicht.Visible = true;
        FilterLeiste.Visible = true;
        SplitContainer1.IsSplitterFixed = false;
        SplitContainer1.Panel2Collapsed = false;
        SplitContainer1.SplitterDistance = Math.Max(SplitContainer1.SplitterDistance, SplitContainer1.Width / 2);

        if (Table.Visible) {
            if (Table.Database != null) {
                if (Table.CursorPosRow == null && Table.View_RowFirst() != null) {
                    Table.CursorPos_Set(Table.View_ColumnFirst(), Table.View_RowFirst(), false);
                }

                if (Table.CursorPosRow?.Row != null) {
                    FillFormula(Table.CursorPosRow?.Row);
                }
            }
        } else {
            FillFormula(null);
        }
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) {
        if (!FileExists(LoadTab.FileName)) { return; }

        _ = SwitchTabToDatabase(new ConnectionInfo(LoadTab.FileName, PreveredDatabaseID, string.Empty));
    }

    private void lstAufgaben_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (IsDisposed || Table.Database is not Database db || db.IsDisposed) { return; }

        lstAufgaben.Enabled = false;

        if (e.Item is ReadableListItem bli && bli.Item is DatabaseScriptDescription sc) {
            string m;

            if (sc.NeedRow) {
                if (MessageBox.Show("Skript bei <b>allen</b> aktuell<br>angezeigten Zeilen ausführen?", ImageCode.Skript, "Ja", "Nein") == 0) {
                    m = db.Row.ExecuteScript(null, e.Item.KeyName, Table.RowsVisibleUnique());
                } else {
                    m = "Durch Benutzer abgebrochen";
                }
            } else {
                //public Script? ExecuteScript(Events? eventname, string? scriptname, bool onlyTesting, RowItem? row) {
                var s = db.ExecuteScript(sc, sc.ChangeValues, null, null, true, false);
                m = s.Protocol.JoinWithCr();
            }

            lstAufgaben.Enabled = true;

            if (!string.IsNullOrEmpty(m)) {
                MessageBox.Show("Skript abgebrochen:\r\n" + m, ImageCode.Kreuz, "OK");
            } else {
                MessageBox.Show("Skript erfolgreich!", ImageCode.Häkchen, "OK");
            }
            return;
        }

        if (e.Item is TextListItem tli) {
            var com = (tli.KeyName + "|||").SplitBy("|");

            switch (com[0].ToLowerInvariant()) {
                case "#repairscript":
                case "#editscript":
                    OpenScriptEditor(db);
                    UpdateScripts(db);
                    return; // lstAufgaben.Enabled = true; wird von UpdateScript gemacht

                case "#enablerowscript":
                    db.EnableScript();
                    UpdateScripts(db);
                    return; // lstAufgaben.Enabled = true; wird von UpdateScript gemacht

                case "#repaircolumn":
                    var c = db.Column[com[1]];
                    OpenColumnEditor(c, null);
                    UpdateScripts(db);
                    return; // lstAufgaben.Enabled = true; wird von UpdateScript gemacht

                case "#datenüberprüfung":
                    var rows = Table.RowsVisibleUnique();
                    if (rows.Count == 0) {
                        MessageBox.Show("Keine Zeilen angezeigt.", ImageCode.Kreuz, "OK");
                        return;
                    }

                    foreach (var thisR in rows) {
                        thisR.InvalidateRowState("TableView, Kontextmenü, Datenüberprüfung");
                        thisR.UpdateRow(true, true, "TableView, Kontextmenü, Datenüberprüfung");
                    }

                    RowCollection.DoAllInvalidatedRows(null, true);

                    MessageBox.Show("Alle angezeigten Zeilen überprüft.", ImageCode.HäkchenDoppelt, "OK");
                    lstAufgaben.Enabled = true;
                    return;
            }
        }

        MessageBox.Show("Befehl unbekannt!", ImageCode.Kritisch, "OK");
        lstAufgaben.Enabled = true;
    }

    //private string NameRepair(string istName, RowItem? vRow) {
    //    var newName = istName;
    //    var istZ = 0;
    //    do {
    //        var changed = false;
    //        if (Table.Database != null && Table.Database.Row.Any(thisRow =>
    //                thisRow != null && thisRow != vRow && string.Equals(thisRow.CellFirstString(), newName,
    //                    StringComparison.OrdinalIgnoreCase))) {
    //            istZ++;
    //            newName = istName + " (" + istZ + ")";
    //            changed = true;
    //        }

    //        if (!changed) {
    //            return newName;
    //        }
    //    } while (true);
    //}

    //private void SuchEintragNoSave(Direction richtung, out ColumnItem? column, out RowData? row) {
    //    column = Table.View_ColumnFirst();
    //    row = null;

    //    if (Table.Database is not Database db || db.Row.Count < 1) { return; }

    //    // Temporär berechnen, um geflacker zu vermeiden (Endabled - > Disabled bei Nothing)
    //    if (richtung.HasFlag(Direction.Unten)) {
    //        row = Table.View_NextRow(Table.CursorPosRow) ?? Table.View_RowFirst();
    //    }

    //    if (richtung.HasFlag(Direction.Oben)) {
    //        row = Table.View_PreviousRow(Table.CursorPosRow) ?? Table.View_RowLast();
    //    }

    //    row ??= Table.View_RowFirst();
    //}

    private void tbcDatabaseSelector_Deselecting(object sender, TabControlCancelEventArgs e) {
        var s = (List<object>)e.TabPage.Tag;
        s[1] = ViewToString();

        e.TabPage.Tag = s;

        if (Table.Database is Database db && !db.IsDisposed) {
            this.SetSetting("View_" + db.TableName, ViewToString());
        }
    }

    /// <summary>
    /// Diese Routine lädt die Datenbank, falls nötig.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void tbcDatabaseSelector_Selected(object? sender, TabControlEventArgs e) {
        Table.ShowWaitScreen = true;
        tbcDatabaseSelector.Enabled = false;
        Table.Enabled = false;
        Table.Refresh();

        Database.ForceSaveAll();
        MultiUserFile.SaveAll(false);

        if (e.TabPage == null) { return; }

        var s = (List<object>)e.TabPage.Tag;

        var ci = (ConnectionInfo)s[0];
        if (ci == null) {
            e.TabPage.Text = "FEHLER";
            UpdateScripts(null);
            DatabaseSet(null, string.Empty);
            return;
        }

        #region Status-Meldung updaten?

        var maybeok = false;
        foreach (var thisdb in Database.AllFiles) {
            if (thisdb.TableName.Equals(ci.TableName, StringComparison.OrdinalIgnoreCase)) { maybeok = true; break; }
        }

        if (!maybeok) {
            UpdateStatusBar(FehlerArt.Info, "Lade Datenbank " + ci.TableName, true);
        }

        #endregion

        if (Database.GetById(ci, false, Table.Database_NeedPassword, true) is Database db && !db.IsDisposed) {
            if (!string.IsNullOrEmpty(db.Filename)) {
                btnLetzteDateien.AddFileName(db.Filename, db.TableName);
                LoadTab.FileName = db.Filename;
            } else {
                btnLetzteDateien.AddFileName(db.ConnectionData.UniqueId, db.TableName);
            }
            e.TabPage.Text = db.TableName.ToTitleCase();
            UpdateScripts(db);
            DatabaseSet(db, (string)s[1]);
        } else {
            e.TabPage.Text = "FEHLER";
            UpdateScripts(null);
            DatabaseSet(null, string.Empty);
        }
    }

    private void UpdateScripts(Database? db) {
        Table.Invalidate();
        lstAufgaben.ItemClear();

        if (db == null || db.IsDisposed || !string.IsNullOrEmpty(db.FreezedReason)) {
            lstAufgaben.Enabled = false;
            return;
        }

        var ok = true;
        foreach (var thisColumnItem in db.Column) {
            if (!thisColumnItem.IsOk()) {
                var d = ItemOf("Spalte ' " + thisColumnItem.KeyName + " ' reparieren", "#repaircolumn|" + thisColumnItem.KeyName, ImageCode.Kritisch);
                d.Enabled = db.IsAdministrator();
                lstAufgaben.ItemAdd(d);

                ok = false;
            }
            if (!ok) {
                lstAufgaben.Enabled = true;
                return;
            }
        }

        if (!string.IsNullOrEmpty(db.ScriptNeedFix) || !string.IsNullOrEmpty(db.CheckScriptError())) {
            var d = ItemOf("Skripte reparieren", "#repairscript", ImageCode.Kritisch);
            d.Enabled = db.IsAdministrator();
            lstAufgaben.ItemAdd(d);
            lstAufgaben.Enabled = true;
            return;
        }

        if (!db.IsRowScriptPossible(false)) {
            var d = ItemOf("Zeilen-Skripte erlauben", "#enablerowscript", ImageCode.Spalte);
            d.Enabled = db.IsAdministrator();
            lstAufgaben.ItemAdd(d);
            lstAufgaben.Enabled = true;
            return;
        }

        foreach (var thiss in db.EventScript) {
            if (thiss != null && thiss.UserGroups.Count > 0) {
                var d = ItemOf(thiss);
                lstAufgaben.ItemAdd(d);
                d.Enabled = db.PermissionCheck(thiss.UserGroups, null) && thiss.IsOk();

                if (d.Enabled && thiss.NeedRow && !db.IsRowScriptPossible(true)) {
                    d.Enabled = false;
                }

                //d.QuickInfo = thiss.QuickInfo;
            }
        }

        //if (db.CanDoPrepareFormulaCheckScript()) {
        lstAufgaben.ItemAdd(ItemOf("Komplette Datenüberprüfung", "#datenüberprüfung", ImageCode.HäkchenDoppelt));
        //}

        if (db.IsAdministrator()) {
            var d = ItemOf("Skripte bearbeiten", "#editscript", ImageCode.Skript);
            lstAufgaben.ItemAdd(d);
            d.Enabled = db.IsAdministrator();
        }

        lstAufgaben.Enabled = lstAufgaben.ItemCount > 0;
    }

    #endregion
}