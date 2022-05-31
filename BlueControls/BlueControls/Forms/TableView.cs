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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using static BlueBasics.Develop;
using static BlueBasics.FileOperations;
using static BlueBasics.Converter;
using BlueBasics.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using System.Threading.Tasks;
using System.Drawing;
using static BlueBasics.Generic;

namespace BlueControls.Forms;

public partial class TableView : Form {
    //protected readonly List<string> DBView = new();

    #region Fields

    protected Ansicht _ansicht = Ansicht.Nur_Tabelle;

    private bool _firstOne = true;

    //private readonly List<Database> DBStore = new();
    private Database? _originalDb;

    #endregion

    //private int PrevIndexNo = -1;

    #region Constructors

    public TableView() : this(null, true, true) { }

    public TableView(Database? database, bool loadTabVisible, bool adminTabVisible) {
        InitializeComponent();
        //var bmp = new System.Drawing.Bitmap(111,112);
        //var gr = System.Drawing.Graphics.FromImage(bmp);
        //gr.Clear(System.Drawing.Color.Gray);
        //gr.Dispose();
        //var stUTF8 = BlueBasics.modConverter.BitmapToStringUnicode(bmp, System.Drawing.Imaging.ImageFormat.Png);
        //var b = stUTF8.UTF8_ToByte();
        //var newstUTF8 = b.ToStringUTF8();
        //var tmpb = newstUTF8.UTF8_ToByte();
        //var eq = b.SequenceEqual(tmpb);
        //var newbmp = BlueBasics.modConverter.StringUnicodeToBitmap(newstUTF8);
        //Copyright.Text = "(c) 2010-" + DateTime.Now.Year + " Christian Peter";

        if (!adminTabVisible) {
            grpAdminAllgemein.Visible = false;
            //grpBearbeitung.Visible = false;
        }
        if (!loadTabVisible) {
            ribMain.Controls.Remove(tabFile);
        }

        if (btnDrucken != null) {
            btnDrucken.Item.Clear();
            btnDrucken.Item.Add("Drucken bzw. Export", "erweitert", QuickImage.Get(ImageCode.Drucker, 28));
            btnDrucken.Item.AddSeparator();
            btnDrucken.Item.Add("CSV-Format für Excel in die Zwischenablage", "csv", QuickImage.Get(ImageCode.Excel, 28));
            btnDrucken.Item.Add("HTML-Format für Internet-Seiten", "html", QuickImage.Get(ImageCode.Globus, 28));
            btnDrucken.Item.AddSeparator();
            btnDrucken.Item.Add("Layout-Editor öffnen", "editor", QuickImage.Get(ImageCode.Layout, 28));
        }
        Check_OrderButtons();

        SwitchTabToDatabase(database);
    }

    #endregion

    #region Methods

    public static void CheckDatabase(object? sender, LoadedEventArgs? e) {
        if (sender is Database database && !database.ReadOnly) {
            if (database.IsAdministrator()) {
                foreach (var thisColumnItem in database.Column) {
                    while (!thisColumnItem.IsOk()) {
                        DebugPrint(FehlerArt.Info, "Datenbank:" + database.Filename + "\r\nSpalte:" + thisColumnItem.Name + "\r\nSpaltenfehler: " + thisColumnItem.ErrorReason() + "\r\nUser: " + database.UserName + "\r\nGroup: " + database.UserGroup + "\r\nAdmins: " + database.DatenbankAdmin.JoinWith(";"));
                        MessageBox.Show("Die folgende Spalte enthält einen Fehler:<br>" + thisColumnItem.ErrorReason() + "<br><br>Bitte reparieren.", ImageCode.Information, "OK");
                        OpenColumnEditor(thisColumnItem, null);
                    }
                }
            }
        }
    }

    //        ChangeDatabase(TableView.Database);
    //        TableView.DatabaseChanged += TableView_DatabaseChanged;
    //        TableView.EnabledChanged += TableView_EnabledChanged;
    //    }
    //}
    public static void OpenColumnEditor(ColumnItem? column, RowItem? row, Table? tableview) {
        if (column == null) { return; }
        if (row == null) {
            OpenColumnEditor(column, tableview);
            return;
        }
        ColumnItem? columnLinked = null;
        var posError = false;
        switch (column.Format) {
            //case DataFormat.Columns_für_LinkedCellDropdown:
            //    var txt = row.CellGetString(column);
            //    if (IntTryParse(txt, out var colKey)) {
            //        columnLinked = column.LinkedDatabase().Column.SearchByKey(colKey);
            //    }
            //    break;

            //case DataFormat.Verknüpfung_zu_anderer_Datenbank_Skriptgesteuert:
            case DataFormat.Verknüpfung_zu_anderer_Datenbank:
            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                (columnLinked, _, _) = CellCollection.LinkedCellData(column, row, true, false);
                posError = true;
                break;
        }

        var bearbColumn = column;
        if (columnLinked != null) {
            columnLinked.Repair();
            if (MessageBox.Show("Welche Spalte bearbeiten?", ImageCode.Frage, "Spalte in dieser Datenbank", "Verlinkte Spalte") == 1) { bearbColumn = columnLinked; }
        } else {
            if (posError) {
                Notification.Show("Keine aktive Verlinkung.<br>Spalte in dieser Datenbank wird angezeigt.<br><br>Ist die Ziel-Zelle in der Ziel-Datenbank vorhanden?", ImageCode.Information);
            }
        }
        column.Repair();
        OpenColumnEditor(bearbColumn, tableview);
        bearbColumn.Repair();
    }

    //[DefaultValue((Table?)null)]
    //public Table? Table {
    //    get => TableView;
    //    set {
    //        if (TableView == value) { return; }
    //        if (TableView != null) {
    //            TableView.DatabaseChanged -= TableView_DatabaseChanged;
    //            TableView.EnabledChanged -= TableView_EnabledChanged;
    //            ChangeDatabase(null);
    //        }
    //        TableView = value;
    //        Check_OrderButtons();
    //        if (TableView == null) {
    //            return;
    //        }
    public static void OpenColumnEditor(ColumnItem? column, Table? tableview) {
        using ColumnEditor w = new(column, tableview);
        w.ShowDialog();
        column?.Invalidate_ColumAndContent();
    }

    /// <summary>
    /// Löst das DatabaseLoadedEvengt aus, weil es fast einem Neuladen gleichkommt.
    /// </summary>
    /// <param name="db"></param>
    public static void OpenDatabaseHeadEditor(Database db) {
        db.OnConnectedControlsStopAllWorking(null, new MultiUserFileStopWorkingEventArgs());
        if (!db.IsLoading) { db.Load_Reload(); } // Die Routine wird evtl. in der Laderoutine aufgerufen. z.B. bei Fehlerhaften Regeln
        using DatabaseHeadEditor w = new(db);
        w.ShowDialog();
        // DB.OnLoaded(new LoadedEventArgs(true));
    }

    public static void OpenLayoutEditor(Database db, string layoutToOpen) {
        var x = db.ErrorReason(ErrorReason.EditNormaly);
        if (!string.IsNullOrEmpty(x)) {
            MessageBox.Show(x);
            return;
        }
        db.CancelBackGroundWorker();
        LayoutPadEditor w = new(db);
        if (!string.IsNullOrEmpty(layoutToOpen)) { w.LoadLayout(layoutToOpen); }
        w.ShowDialog();
    }

    public static ItemCollectionList Vorgängerversionen(Database db) {
        List<string> zusatz = new();
        ItemCollectionList l = new();
        foreach (var thisExport in db.Export) {
            if (thisExport.Typ == ExportTyp.DatenbankOriginalFormat) {
                var lockMe = new object();
                Parallel.ForEach(thisExport.BereitsExportiert, (thisString, _) => {
                    var t = thisString.SplitAndCutBy("|");
                    if (FileExists(t[0])) {
                        var q1 = QuickImage.Get(ImageCode.Kugel, 16, Color.Red.MixColor(Color.Green, DateTime.Now.Subtract(DateTimeParse(t[1])).TotalDays / thisExport.AutoDelete), Color.Transparent);
                        lock (lockMe) {
                            l.Add(t[1], t[0], q1, true, t[1].CompareKey(SortierTyp.Datum_Uhrzeit));
                        }
                    }
                });

                //foreach (var ThisString in ThisExport.BereitsExportiert) {
                //    var t = ThisString.SplitAndCutBy("|");
                //    if (FileExists(t[0])) {
                //        var q1 = QuickImage.Get(ImageCode.Kugel, 16, Extensions.MixColor(Color.Red, Color.Green, DateTime.Now.Subtract(DateTimeParse(t[1])).TotalDays / ThisExport.AutoDelete).ToHTMLCode(), "");
                //        L.Add(t[1], t[0], q1, true, t[1].CompareKey(enSortierTyp.Datum_Uhrzeit));
                //    }
                //}
                zusatz.AddRange(Directory.GetFiles(thisExport.Verzeichnis, db.Filename.FileNameWithoutSuffix() + "_*.MDB"));
            }
        }
        foreach (var thisString in zusatz.Where(thisString => l[thisString] == null)) {
            l.Add(thisString.FileNameWithSuffix(), thisString, QuickImage.Get(ImageCode.Warnung), true, new FileInfo(thisString).CreationTime.ToString().CompareKey(SortierTyp.Datum_Uhrzeit));
        }
        l.Sort();
        return l;
    }

    public void ResetDatabaseSettings() {
        foreach (var thisT in tbcDatabaseSelector.TabPages) {
            if (thisT is System.Windows.Forms.TabPage tp && tp.Tag is List<string> s) {
                s[1] = string.Empty;
                tp.Tag = s;
            }
        }
    }

    protected void AddTabPage(string filename) {
        var NTabPage = new System.Windows.Forms.TabPage {
            Name = tbcDatabaseSelector.TabCount.ToString(),
            Text = filename.FileNameWithoutSuffix(),
            Tag = new List<string>() { filename, string.Empty }
        };
        tbcDatabaseSelector.Controls.Add(NTabPage);
        //return NTabPage;
    }

    protected virtual void btnCSVClipboard_Click(object sender, System.EventArgs e) {
        CopytoClipboard(Table.Export_CSV(FirstRow.ColumnCaption));
        Notification.Show("Die Daten sind nun<br>in der Zwischenablage.", ImageCode.Clipboard);
    }

    protected virtual void btnDrucken_ItemClicked(object sender, BasicListItemEventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
        switch (e.Item.Internal) {
            case "erweitert":
                Visible = false;
                List<RowItem?> selectedRows = new();
                if (Table.Design == BlueTableAppearance.OnlyMainColumnWithoutHead && Formula.ShowingRow != null) {
                    selectedRows.Add(Formula.ShowingRow);
                } else {
                    selectedRows = Table.VisibleUniqueRows();
                }
                using (ExportDialog l = new(Table.Database, selectedRows)) {
                    l.ShowDialog();
                }
                Visible = true;
                break;

            case "csv":
                CopytoClipboard(Table.Export_CSV(FirstRow.ColumnCaption));
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

    protected virtual void btnHTMLExport_Click(object sender, System.EventArgs e) {
        Table.Export_HTML();
    }

    protected void ChangeDatabaseInTab(string filename, System.Windows.Forms.TabPage xtab) {
        if (xtab == null) { return; }

        tbcDatabaseSelector.Enabled = false;
        Table.Enabled = false;
        Table.ShowWaitScreen = true;
        Table.Refresh();

        var s = (List<string>)(xtab.Tag);
        s[0] = filename;
        s[1] = string.Empty;
        xtab.Tag = s;
        tbcDatabaseSelector_Selected(null, new System.Windows.Forms.TabControlEventArgs(xtab, tbcDatabaseSelector.TabPages.IndexOf(xtab), System.Windows.Forms.TabControlAction.Selected));
    }

    protected virtual void CheckButtons() {
        var datenbankDa = Convert.ToBoolean(Table.Database != null);
        btnNeuDB.Enabled = true;
        btnOeffnen.Enabled = true;
        //btnNeu.Enabled = datenbankDa && Table.Database.PermissionCheck(Table.Database.PermissionGroupsNewRow, null);
        //btnLoeschen.Enabled = datenbankDa;
        btnDrucken.Enabled = datenbankDa;
        //Ansicht0.Enabled = datenbankDa;
        //Ansicht1.Enabled = datenbankDa;
        //Ansicht2.Enabled = datenbankDa;
        btnDatenbankenSpeicherort.Enabled = datenbankDa && !string.IsNullOrEmpty(Table.Database.Filename);
        //SuchenUndErsetzen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        //AngezeigteZeilenLöschen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        //Datenüberprüfung.Enabled = datenbankDa;
        btnZeileLöschen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        btnDatenüberprüfung.Enabled = datenbankDa;
        btnSaveAs.Enabled = datenbankDa;
        btnDrucken.Item["csv"].Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        btnDrucken.Item["html"].Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        //btnVorwärts.Enabled = datenbankDa;
        //btnZurück.Enabled = datenbankDa;
        //such.Enabled = datenbankDa;
        btnSuchenUndErsetzen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        FilterLeiste.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
    }

    protected void DatabaseSet(Database? database, string viewcode) {
        Table.Database = database;
        Formula.Database = database;
        SetFormulaBETA();

        FilterLeiste.Table = Table;

        if (!string.IsNullOrEmpty(viewcode)) {
            ParseView(viewcode);
        }

        Table.ShowWaitScreen = false;
        tbcDatabaseSelector.Enabled = true;
        Table.Enabled = true;

        //if (Table.Database == null) {
        //    SetDatabasetoNothing();
        //} else {
        //    //if (Table.Database.Ansicht != Ansicht.Unverändert) {
        //    //    _ansicht = Table.Database.Ansicht;
        //    //}
        //}
        //InitView();
        //CheckButtons();
        //CaptionAnzeige();
        //CheckButtons();
        if (Table.View_RowFirst() != null && database != null) {
            Table.CursorPos_Set(database.Column[0], Table.View_RowFirst(), false);
        }
    }

    protected virtual void FillFormula() {
        if (tbcSidebar.SelectedTab == tabFormula) {
            if (Formula is null || Formula.IsDisposed) { return; }
            if (!Formula.Visible) { return; }

            if (Formula.Width < 30 || Formula.Height < 10) {
                Formula.Database = null;
                return;
            }

            Formula.Database = Table.Database;
            //if (e.Column != null) { Formula.Database = e.Column.Database; }
            //if (e.RowData?.Row != null) { Formula.Database = e.RowData.Row.Database; }

            Formula.ShowingRowKey = Table.CursorPosRow is RowData r ? r.Row.Key : -1;
        }

        //protected override void FillFormula() {
        //    if (tbcSidebar.SelectedTab == tabFormulaBeta) {
        //        if (FormulaBETA is null || FormulaBETA.IsDisposed) { return; }
        //        if (!FormulaBETA.Visible) { return; }

        //        FormulaBETA.ConnectedFormula =

        //        FormulaBETA.InputRow = Table?.CursorPosRow?.Row;

        //        //if (Table?.CursorPosRow?.Row is RowItem r) {
        //        //    var k = Table.Database.Column.Exists("kommission");
        //        //    if (k != null) { FormulaBETA.Set("kommission", r.CellGetString(k)); }
        //        //}
        //        return;
        //    }

        //    base.FillFormula();
        //}

        if (tbcSidebar.SelectedTab == tabFormulaBeta) {
            if (FormulaBETA is null || FormulaBETA.IsDisposed) { return; }
            if (!FormulaBETA.Visible) { return; }

            SetFormulaBETA();
            FormulaBETA.InputRow = Table?.CursorPosRow?.Row;

            //FormulaBETA.Set("row", Table?.CursorPosRow?.Row);
            //FormulaBETA.Set("key", Table?.CursorPosRow?.Row?.CellFirstString());
        }
    }

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        DatabaseSet(null as Database, string.Empty);
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(true);
        base.OnFormClosing(e);
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        CheckButtons();
    }

    protected virtual void ParseView(string ToParse) {
        if (string.IsNullOrEmpty(ToParse)) { return; }
        foreach (var pair in ToParse.GetAllTags()) {
            switch (pair.Key) {
                case "tableview":
                    Table.ParseView(pair.Value.FromNonCritical());
                    break;
                //case "prio3":
                //    btnPrio3.Checked = pair.Value.FromPlusMinus();
                //    break;
                case "maintab":
                    ribMain.SelectedIndex = int.Parse(pair.Value);
                    break;
                //case "belegwahl":
                // cbxBelegWahl.Text = pair.Value.FromNonCritical();
                //return;
                default:
                    DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
        //        case "arrangementnr":
        //            pair.Value
        //            TableView.ParseView(i);
        //d
    }

    /// <summary>
    /// Sucht den Tab mit der angegebenen Datenbank.
    /// Ist kein Reiter vorhanden, wird ein neuer erzeugt.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    protected bool SwitchTabToDatabase(string filename) {
        if (string.IsNullOrEmpty(filename)) { return false; }
        if (!FileExists(filename)) { return false; }

        foreach (var thisT in tbcDatabaseSelector.TabPages) {
            if (thisT is System.Windows.Forms.TabPage tp && tp.Tag is List<string> s) {
                if (s[0].Equals(filename, StringComparison.InvariantCultureIgnoreCase)) {
                    tbcDatabaseSelector.SelectedTab = tp;

                    if (_firstOne) {
                        _firstOne = false;
                        tbcDatabaseSelector_Selected(null, new System.Windows.Forms.TabControlEventArgs(tp, tbcDatabaseSelector.TabPages.IndexOf(tp), System.Windows.Forms.TabControlAction.Selected));
                    }

                    return true;
                }
            }
        }

        AddTabPage(filename);
        return SwitchTabToDatabase(filename);
    }

    //protected bool ShowDatabase(string dbName) {
    //    var found = -1;
    //    var db = EnsureLoaded(dbName, true);
    //    if (db == null) {
    //        DebugPrint(FehlerArt.Warnung, "Datenbank '" + dbName + "' nicht gefunden");
    //        return false;
    //    }
    //    for (var count = 0; count < tbcDatabaseSelector.TabCount; count++) {
    //        if (tbcDatabaseSelector.TabPages[count].Text.ToUpper() == dbName.ToUpper()) {
    //            found = count;
    //        }
    //    }
    //    if (found >= 0) {
    //        tbcDatabaseSelector.SelectedIndex = found;
    //        return true;
    //    }
    //    AddTabPagex(db.Filename);
    //    tbcDatabaseSelector.SelectedIndex = tbcDatabaseSelector.TabCount - 1;
    //    return true;
    //}
    protected bool SwitchTabToDatabase(Database? database) {
        if (database == null) { return false; }
        return SwitchTabToDatabase(database.Filename);
    }

    protected virtual void TableView_ContextMenu_Init(object sender, ContextMenuInitEventArgs e) {
        var tbl = (Table)sender;
        var cellKey = e.Tags.TagGet("Cellkey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        tbl.Database.Cell.DataOfCellKey(cellKey, out var column, out var row);

        var editable = string.IsNullOrEmpty(CellCollection.ErrorReason(column, row, ErrorReason.EditNormaly));

        if (_ansicht != Ansicht.Überschriften_und_Formular) {
            e.UserMenu.Add("Info", true);
            if (tbl.PinnedRows.Contains(row)) {
                e.UserMenu.Add("Zeile nicht mehr pinnen", "pinlösen", QuickImage.Get(ImageCode.Pinnadel, 16), row != null);
            } else {
                e.UserMenu.Add("Zeile anpinnen", "anpinnen", QuickImage.Get(ImageCode.Pinnadel, 16), row != null);
            }

            e.UserMenu.Add("Sortierung", true);
            e.UserMenu.Add(ContextMenuComands.SpaltenSortierungAZ, column != null && column.Format.CanBeCheckedByRules());
            e.UserMenu.Add(ContextMenuComands.SpaltenSortierungZA, column != null && column.Format.CanBeCheckedByRules());
            e.UserMenu.AddSeparator();
            e.UserMenu.Add("Zelle", true);
            e.UserMenu.Add("Inhalt Kopieren", "ContentCopy", ImageCode.Kopieren, column != null && column.Format.CanBeChangedByRules());
            e.UserMenu.Add("Inhalt Einfügen", "ContentPaste", ImageCode.Clipboard, editable && column.Format.CanBeChangedByRules());
            e.UserMenu.Add("Inhalt löschen", "ContentDelete", ImageCode.Radiergummi, editable && column.Format.CanBeChangedByRules());
            e.UserMenu.Add(ContextMenuComands.VorherigenInhaltWiederherstellen, editable && column.Format.CanBeChangedByRules() && column.ShowUndo);
            e.UserMenu.Add(ContextMenuComands.SuchenUndErsetzen, column != null && tbl.Database.IsAdministrator());
            e.UserMenu.AddSeparator();
            e.UserMenu.Add("Spalte", true);
            e.UserMenu.Add(ContextMenuComands.SpaltenEigenschaftenBearbeiten, column != null && tbl.Database.IsAdministrator());
            e.UserMenu.Add("Statistik", "Statistik", QuickImage.Get(ImageCode.Balken, 16), column != null && tbl.Database.IsAdministrator());
            //e.UserMenu.Add("Inhalte aller angezeigten Zellen dieser Spalte löschen", "ColumnContentDelete", ImageCode.Radiergummi, column != null && column.Format.CanBeChangedByRules() && tbl.Database.IsAdministrator());
            e.UserMenu.Add("Summe", "Summe", ImageCode.Summe, column != null && tbl.Database.IsAdministrator());
            e.UserMenu.AddSeparator();
        }
        e.UserMenu.Add("Zeile", true);
        e.UserMenu.Add(ContextMenuComands.ZeileLöschen, row != null && tbl.Database.IsAdministrator());
        e.UserMenu.Add("Zeile prüfen", "Fehlersuche", ImageCode.Zeile, row != null);
    }

    protected virtual void TableView_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        var tbl = (Table)sender;
        var cellKey = e.Tags.TagGet("CellKey");
        if (string.IsNullOrEmpty(cellKey)) { return; }
        tbl.Database.Cell.DataOfCellKey(cellKey, out var column, out var row);

        var ValueCol0 = string.Empty;
        if (row != null) { ValueCol0 = row.CellFirstString(); }
        var editable = string.IsNullOrEmpty(CellCollection.ErrorReason(column, row, ErrorReason.EditAcut));

        switch (e.ClickedComand) {
            case "pinlösen":
                tbl.PinRemove(row);
                break;

            case "anpinnen":
                tbl.PinAdd(row);
                break;

            case "SpaltenSortierungAZ":
                tbl.SortDefinitionTemporary = new RowSortDefinition(tbl.Database, column.Name, false);
                break;

            case "SpaltenSortierungZA":
                tbl.SortDefinitionTemporary = new RowSortDefinition(tbl.Database, column.Name, true);
                break;

            case "Fehlersuche":
                MessageBox.Show(row.DoAutomatic(true, true, 10, "manual check").error);
                break;

            case "ZeileLöschen":
                if (tbl.Database.ReadOnly) { return; }
                if (!tbl.Database.IsAdministrator()) { return; }
                if (row == null) { return; }
                if (MessageBox.Show("Zeile wirklich löschen? (<b>" + ValueCol0 + "</b>)", ImageCode.Frage, "Ja", "Nein") == 0) {
                    tbl.Database.Row.Remove(row);
                }

                break;

            case "ContentDelete":
                if (!editable) { return; }
                tbl.Database.Cell.Delete(column, row.Key);
                break;

            case "SpaltenEigenschaftenBearbeiten":
                OpenColumnEditor(column, row, Table);
                CheckButtons();
                break;

            case "ContentCopy":
                Table.CopyToClipboard(column, row, true);
                break;

            case "SuchenUndErsetzen":
                if (!tbl.Database.IsAdministrator()) { return; }
                Table.OpenSearchAndReplace();
                break;

            case "Summe":
                if (!tbl.Database.IsAdministrator()) { return; }
                var summe = column.Summe(Table.Filter);
                if (!summe.HasValue) {
                    MessageBox.Show("Die Summe konnte nicht berechnet werden.", ImageCode.Summe, "OK");
                } else {
                    MessageBox.Show("Summe dieser Spalte, nur angezeigte Zeilen: <br><b>" + summe, ImageCode.Summe, "OK");
                }
                break;

            case "Statistik":
                if (!tbl.Database.IsAdministrator()) { return; }
                column.Statisik(Table.Filter, Table.PinnedRows);
                break;

            case "VorherigenInhaltWiederherstellen":
                if (!editable) { return; }
                Table.DoUndo(column, row);
                break;

                //case "ContentPaste":
                //    d
                //    row.CellSet(column, System.Windows.Forms.Clipboard.GetText());
                //    break;

                //case "ColumnContentDelete":
                //    if (column != null) {
                //        if (MessageBox.Show("Angezeite Inhalte dieser Spalte löschen?", ImageCode.Frage, "Ja", "Nein") == 0) {
                //            column.DeleteContents(Table.Filter, Table.PinnedRows);
                //        }
                //    }
                //    break;
        }
    }

    //    Table.Database = db;
    //    if (Table.Database != null) {
    //        tbcDatabaseSelector.TabPages[toIndex].Text = db.Filename.FileNameWithoutSuffix();
    //        ParseView(DBView[toIndex]);
    //    }
    //    tbcDatabaseSelector.Enabled = true;
    //    Table.Enabled = true;
    //}
    protected virtual string ViewToString() {
        var s = "{" +
                "MainTab=" + ribMain.SelectedIndex.ToString() + ", " +
                "TableView=" + Table.ViewToString().ToNonCritical() +
                "}";
        return s;
    }

    //    if (PrevIndexNo > -1 && DBStore[PrevIndexNo] != Table.Database) { PrevIndexNo = -1; }
    //    if (toIndex != PrevIndexNo && PrevIndexNo > -1 && DBStore[PrevIndexNo] != null) {
    //        DBView[PrevIndexNo] = ViewToString();
    //    }
    //    PrevIndexNo = toIndex;
    //    if (Table.Database == db) {
    //        Table.Database = null; // um den Filtern klar zu machen, das ändert sich was
    //    }
    private void _originalDB_Disposing(object sender, System.EventArgs e) => ChangeDatabase(null);

    //protected void TabSetDatabaseAndEnable(int toIndex, Database db) {
    //    tbcDatabaseSelector.Enabled = false;
    //    Table.Enabled = false;
    //    tbcDatabaseSelector.TabPages[toIndex].Enabled = true;
    //    while (DBStore.Count <= toIndex) {
    //        DBStore.Add(null);
    //        DBView.Add(string.Empty);
    //    }
    //    if (DBStore[toIndex] != db) { DBView[toIndex] = string.Empty; }
    //    DBStore[toIndex] = db;
    //    if (tbcDatabaseSelector.SelectedIndex != toIndex) {
    //        tbcDatabaseSelector.SelectedIndex = toIndex;
    //        return; // Selected Index changed soll diese Routine hier aufrufen
    //    }
    private void btnAlleErweitern_Click(object sender, System.EventArgs e) => Table.ExpandAll();

    private void btnAlleSchließen_Click(object sender, System.EventArgs e) => Table.CollapesAll();

    private void btnClipboardImport_Click(object sender, System.EventArgs e) {
        if (Table.Database == null || !Table.Database.IsAdministrator()) { return; }
        Table.ImportClipboard();
    }

    private void btnDatenbankenSpeicherort_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
        ExecuteFile(Table.Database.Filename.FilePath());
    }

    private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => OpenDatabaseHeadEditor(Table.Database);

    private void btnDatenüberprüfung_Click(object sender, System.EventArgs e) {
        if (Table.Database == null || !Table.Database.IsAdministrator()) { return; }
        var m = Table.Database.Row.DoAutomatic(Table.Filter, true, Table.PinnedRows, "manual check");

        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show(m);
        }
    }

    private void btnFormular_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (Table.Database == null) { return; }

        var x = new BlueControls.Forms.ConnectedFormulaEditor(Table?.Database.FormulaFileName(), null);
        x.ShowDialog();
        x.Dispose();
    }

    private void btnLayouts_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (Table.Database == null) { return; }
        OpenLayoutEditor(Table.Database, string.Empty);
    }

    private void btnLetzteDateien_ItemClicked(object sender, BasicListItemEventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
        SwitchTabToDatabase(e.Item.Internal);
    }

    private void btnNeuDB_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);

        SaveTab.ShowDialog();
        if (!PathExists(SaveTab.FileName.FilePath())) { return; }
        if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

        if (FileExists(SaveTab.FileName)) { DeleteFile(SaveTab.FileName, true); }

        var db = new Database(false);
        db.SaveAsAndChangeTo(SaveTab.FileName);
        SwitchTabToDatabase(SaveTab.FileName);
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
        LoadTab.ShowDialog();
    }

    private void btnPowerBearbeitung_Click(object sender, System.EventArgs e) {
        Notification.Show("20 Sekunden (fast) rechtefreies<br>Vearbeiten akiviert.", ImageCode.Stift);
        Table.PowerEdit = DateTime.Now.AddSeconds(20);
    }

    private void btnSaveAs_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);

        if (Table.Database == null) { return; }

        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);

        SaveTab.ShowDialog();
        if (!PathExists(SaveTab.FileName.FilePath())) { return; }
        if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

        if (FileExists(SaveTab.FileName)) { DeleteFile(SaveTab.FileName, true); }

        var db = Table.Database;

        db.SaveAsAndChangeTo(SaveTab.FileName);
        SwitchTabToDatabase(SaveTab.FileName);
    }

    private void btnSpaltenanordnung_Click(object sender, System.EventArgs e) {
        var x = new ColumnArrangementPadEditor(Table.Database);
        x.ShowDialog();
        Table.Database.ColumnArrangements[0].ShowAllColumns();
        Table.Invalidate_HeadSize();
        Table.Invalidate_AllColumnArrangements();
    }

    private void btnSpaltenUebersicht_Click(object sender, System.EventArgs e) => Table.Database.Column.GenerateOverView();

    private void btnSuchenUndErsetzen_Click(object sender, System.EventArgs e) => Table.OpenSearchAndReplace();

    private void btnTemporärenSpeicherortÖffnen_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
        ExecuteFile(Path.GetTempPath());
    }

    private void btnUnterschiede_CheckedChanged(object sender, System.EventArgs e) => Table.Unterschiede = btnUnterschiede.Checked ? Table.CursorPosRow.Row : null;

    private void btnVorherigeVersion_Click(object sender, System.EventArgs e) {
        btnVorherigeVersion.Enabled = false;
        if (_originalDb != null && Table.Database != _originalDb) {
            _originalDb.Disposing -= _originalDB_Disposing;
            Table.Database = _originalDb;
            _originalDb = null;
            btnVorherigeVersion.Text = "Vorherige Version";
            btnVorherigeVersion.Enabled = true;
            return;
        }
        var merker = Table.Database;
        var l = Vorgängerversionen(Table.Database);
        if (l.Count == 0) {
            MessageBox.Show("Kein Backup vorhanden.", ImageCode.Information, "OK");
            btnVorherigeVersion.Enabled = true;
            return;
        }
        var files = InputBoxListBoxStyle.Show("Stand wählen:", l, AddType.None, true);
        if (files == null || files.Count != 1) {
            btnVorherigeVersion.Enabled = true;
            return;
        }

        Table.Database = Database.GetByFilename(files[0], false, true);
        _originalDb = merker;
        _originalDb.Disposing += _originalDB_Disposing;
        btnVorherigeVersion.Text = "zurück";
        btnVorherigeVersion.Enabled = true;
    }

    private void btnZeileLöschen_Click(object sender, System.EventArgs e) {
        if (!Table.Database.IsAdministrator()) { return; }
        var m = MessageBox.Show("Angezeigte Zeilen löschen?", ImageCode.Warnung, "Ja", "Nein");
        if (m != 0) { return; }
        Table.Database.Row.Remove(Table.Filter, Table.PinnedRows);
    }

    private void cbxColumnArr_ItemClicked(object sender, BasicListItemEventArgs e) {
        if (string.IsNullOrEmpty(cbxColumnArr.Text)) { return; }
        Table.Arrangement = int.Parse(e.Item.Internal);
    }

    private void ChangeDatabase(Database? database) {
        if (_originalDb != null) {
            _originalDb.Disposing -= _originalDB_Disposing;
        }
        _originalDb = null;
        btnVorherigeVersion.Text = "Vorherige Version";
        CheckDatabase(database, null);
        Check_OrderButtons();
    }

    private void Check_OrderButtons() {
        if (InvokeRequired) {
            Invoke(new Action(Check_OrderButtons));
            return;
        }
        const bool enTabAllgemein = true;
        var enTabellenAnsicht = true;
        if (Table.Database == null || !Table.Database.IsAdministrator()) {
            tabAdmin.Enabled = false;
            return; // Weitere funktionen benötigen sicher eine Datenbank um keine Null Exception auszulösen
        }
        if (Table.Design != BlueTableAppearance.Standard || !Table.Enabled || Table.Database.ReadOnly) {
            enTabellenAnsicht = false;
        }
        grpAdminAllgemein.Enabled = enTabAllgemein;
        grpImport.Enabled = enTabellenAnsicht;
        tabAdmin.Enabled = true;
    }

    //private void DatabaseSetx(string filename) {
    //    DatabaseSet(null as Database);
    //    if (!FileExists(filename)) {
    //        CheckButtons();
    //        return;
    //    }
    //    btnLetzteDateien.AddFileName(filename, string.Empty);
    //    LoadTab.FileName = filename;
    //    var tmpDatabase = Database.GetByFilename(filename, false, false);
    //    if (tmpDatabase == null) { return; }
    //    DatabaseSet(tmpDatabase);
    //}
    private void Formula_SizeChanged(object sender, System.EventArgs e) => FillFormula();

    private void Formula_VisibleChanged(object sender, System.EventArgs e) => FillFormula();

    private void LoadTab_FileOk(object sender, CancelEventArgs e) => SwitchTabToDatabase(LoadTab.FileName);

    private void SetFormulaBETA() {
        if (Table?.Database != null) {
            var f = Table?.Database.FormulaFileName();

            if (f != null) {
                var tmpFormula = ConnectedFormula.ConnectedFormula.GetByFilename(f);
                if (tmpFormula != null) {
                    FormulaBETA.ConnectedFormula = tmpFormula;
                    return;
                }
            }
        }

        FormulaBETA.ConnectedFormula = null;
    }

    private void Table_CursorPosChanged(object sender, CellExtEventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => Table_CursorPosChanged(sender, e)));
            return;
        }

        btnUnterschiede_CheckedChanged(null, System.EventArgs.Empty);

        FillFormula();
    }

    private void Table_ViewChanged(object sender, System.EventArgs e) => Table.WriteColumnArrangementsInto(cbxColumnArr, Table.Database, Table.Arrangement);

    private void Table_VisibleRowsChanged(object sender, System.EventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => Table_VisibleRowsChanged(sender, e)));
            return;
        }

        capZeilen1.Text = "<IMAGECODE=Information|16> " + LanguageTool.DoTranslate("Einzigartige Zeilen:") + " " + Table.Database.Row.VisibleRowCount + " " + LanguageTool.DoTranslate("St.");
        capZeilen1.Refresh(); // Backgroundworker lassen wenig luft
        capZeilen2.Text = capZeilen1.Text;
        capZeilen2.Refresh();
    }

    private void TableView_DatabaseChanged(object sender, System.EventArgs e) {
        Table.WriteColumnArrangementsInto(cbxColumnArr, Table.Database, Table.Arrangement);
        ChangeDatabase(Table.Database);
        Check_OrderButtons();
        CheckButtons();
    }

    private void TableView_EnabledChanged(object sender, System.EventArgs e) => Check_OrderButtons();

    private void tbcDatabaseSelector_Deselecting(object sender, System.Windows.Forms.TabControlCancelEventArgs e) {
        var s = (List<string>)(e.TabPage.Tag);
        s[1] = ViewToString();
        e.TabPage.Tag = s;
    }

    private void tbcDatabaseSelector_Selected(object sender, System.Windows.Forms.TabControlEventArgs e) {
        Table.ShowWaitScreen = true;
        tbcDatabaseSelector.Enabled = false;
        Table.Enabled = false;
        Table.Refresh();

        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);

        var s = (List<string>)(e.TabPage.Tag);

        var DB = Database.GetByFilename(s[0], false, false);

        if (DB != null) {
            btnLetzteDateien.AddFileName(DB.Filename, string.Empty);
            LoadTab.FileName = DB.Filename;
            e.TabPage.Text = DB.Filename.FileNameWithoutSuffix();
        }

        DatabaseSet(DB, s[1]);
    }

    private void tbcSidebar_SelectedIndexChanged(object sender, System.EventArgs e) => FillFormula();

    #endregion
}