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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static BlueBasics.Converter;
using static BlueBasics.Develop;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueControls.Forms;

public partial class TableView : Form {

    #region Fields



    private Ansicht _ansicht = Ansicht.Tabelle_und_Formular_nebeneinander;

    private bool _firstOne = true;

    private DatabaseAbstract? _originalDb;

    #endregion

    #region Constructors

    public TableView() : this(null, true, true) { }

    public TableView(DatabaseAbstract? database, bool loadTabVisible, bool adminTabVisible) {
        InitializeComponent();

        if (!adminTabVisible) {
            grpAdminAllgemein.Visible = false;
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

    #region Properties

    public SQLBackAbstract? CopyToSQL { get; set; } = null;

    #endregion

    #region Methods

    public static void CheckDatabase(object? sender, LoadedEventArgs? e) {
        if (sender is DatabaseAbstract database && !database.ReadOnly) {
            if (database.IsAdministrator()) {
                foreach (var thisColumnItem in database.Column) {
                    while (!thisColumnItem.IsOk()) {
                        DebugPrint(FehlerArt.Info, "Datenbank:" + database.TableName + "\r\nSpalte:" + thisColumnItem.Name + "\r\nSpaltenfehler: " + thisColumnItem.ErrorReason() + "\r\nUser: " + database.UserName + "\r\nGroup: " + database.UserGroup + "\r\nAdmins: " + database.DatenbankAdmin.JoinWith(";"));
                        MessageBox.Show("Die folgende Spalte enthält einen Fehler:<br>" + thisColumnItem.ErrorReason() + "<br><br>Bitte reparieren.", ImageCode.Information, "OK");
                        OpenColumnEditor(thisColumnItem, null);
                    }
                }
            }
        }
    }

    public static void OpenColumnEditor(ColumnItem? column, RowItem? row, Table? tableview) {
        if (column == null) { return; }
        if (row == null) {
            OpenColumnEditor(column, tableview);
            return;
        }
        ColumnItem? columnLinked = null;
        var posError = false;
        switch (column.Format) {
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

    public static void OpenColumnEditor(ColumnItem? column, Table? tableview) {
        using ColumnEditor w = new(column, tableview);
        w.ShowDialog();
        //column?.Invalidate_ColumAndContentx();
    }

    /// <summary>
    /// Löst das DatabaseLoadedEvengt aus, weil es fast einem Neuladen gleichkommt.
    /// </summary>
    /// <param name="db"></param>
    public static void OpenDatabaseHeadEditor(DatabaseAbstract db) {
        db.OnConnectedControlsStopAllWorking(null, new MultiUserFileStopWorkingEventArgs());
        if (!db.IsLoading) { db.Load_Reload(); } // Die Routine wird evtl. in der Laderoutine aufgerufen. z.B. bei Fehlerhaften Regeln
        using DatabaseHeadEditor w = new(db);
        w.ShowDialog();
        // DB.OnLoaded(new LoadedEventArgs(true));
    }

    public static void OpenLayoutEditor(DatabaseAbstract db, string layoutToOpen) {
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

    public static ItemCollectionList Vorgängerversionen(DatabaseAbstract db) {
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
            if (thisT is System.Windows.Forms.TabPage tp && tp.Tag is List<object> s) {
                s[1] = string.Empty;
                tp.Tag = s;
            }
        }
    }

    /// <summary>
    /// Erstellt einen Reiter mit den nötigen Tags um eine Datenbank laden zu können - lädt die Datenbank aber selbst nicht.
    /// </summary>
    /// <param name="uniqueID"></param>
    protected void AddTabPage(ConnectionInfo ci) {
        if (ci is null) { return; }

        var NTabPage = new System.Windows.Forms.TabPage {
            Name = tbcDatabaseSelector.TabCount.ToString(),
            Text = ci.TableName.ToTitleCase(),
            Tag = new List<object>() { ci, string.Empty }
        };
        tbcDatabaseSelector.Controls.Add(NTabPage);
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

    protected void ChangeDatabaseInTab(ConnectionInfo connectionID, System.Windows.Forms.TabPage? xtab) {
        if (xtab == null) { return; }

        tbcDatabaseSelector.Enabled = false;
        Table.Enabled = false;
        Table.ShowWaitScreen = true;
        Table.Refresh();

        var s = (List<object>)(xtab.Tag);
        s[0] = connectionID;
        s[1] = string.Empty;
        xtab.Tag = s;
        tbcDatabaseSelector_Selected(null, new System.Windows.Forms.TabControlEventArgs(xtab, tbcDatabaseSelector.TabPages.IndexOf(xtab), System.Windows.Forms.TabControlAction.Selected));
    }

    protected virtual void CheckButtons() {
        var datenbankDa = Convert.ToBoolean(Table.Database != null);
        btnNeuDB.Enabled = true;
        btnOeffnen.Enabled = true;
        btnNeu.Enabled = datenbankDa && Table.Database.PermissionCheck(Table.Database.PermissionGroupsNewRow, null);
        btnLoeschen.Enabled = datenbankDa;
        btnDrucken.Enabled = datenbankDa;
        chkAnsichtNurTabelle.Enabled = datenbankDa;
        chkAnsichtFormular.Enabled = datenbankDa;
        chkAnsichtTableFormular.Enabled = datenbankDa;
        btnDatenbankenSpeicherort.Enabled = datenbankDa && !string.IsNullOrEmpty(Table.Database.Filename);
        //SuchenUndErsetzen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        //AngezeigteZeilenLöschen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        //Datenüberprüfung.Enabled = datenbankDa;
        btnZeileLöschen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        btnDatenüberprüfung.Enabled = datenbankDa;
        btnSaveAs.Enabled = datenbankDa;
        btnDrucken.Item["csv"].Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        btnDrucken.Item["html"].Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        btnVorwärts.Enabled = datenbankDa;
        btnZurück.Enabled = datenbankDa;
        txbTextSuche.Enabled = datenbankDa;
        btnSuchenUndErsetzen.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
        FilterLeiste.Enabled = datenbankDa && Table.Design != BlueTableAppearance.OnlyMainColumnWithoutHead;
    }

    protected virtual void DatabaseSet(DatabaseAbstract? database, string toParse) {
        if (Table.Database != database) {
            Formula.ConnectedFormula = null;
        }

        FilterLeiste.Table = Table;
        var did = false;

        if (!string.IsNullOrEmpty(toParse)) {
            foreach (var pair in toParse.GetAllTags()) {
                switch (pair.Key) {
                    case "tableview":
                        Table.DatabaseSet(database, pair.Value.FromNonCritical());
                        did = true;
                        break;

                    case "maintab":
                        ribMain.SelectedIndex = int.Parse(pair.Value);
                        break;

                    case "ansicht":
                        _ansicht = (Ansicht)int.Parse(pair.Value);
                        InitView();
                        break;

                    default:
                        DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        if (!did) {
            Table.DatabaseSet(database, string.Empty);
            if (Table.View_RowFirst() != null && database != null) {
                Table.CursorPos_Set(database.Column[0], Table.View_RowFirst(), false);
            }
        }

        Table.ShowWaitScreen = false;
        tbcDatabaseSelector.Enabled = true;
        Table.Enabled = true;
    }

    protected virtual void FillFormula(RowItem? r) {
        if (tbcSidebar.SelectedTab == tabFormula) {
            if (Formula is null || Formula.IsDisposed) { return; }
            if (!Formula.Visible) { return; }
            if (Formula.Width < 30 || Formula.Height < 10) { return; }

            if (r?.Database != null) {
                SetFormula(r?.Database);
            }
            Formula.Database = r?.Database;
            if (r != null) {
                Formula.RowKey = r.Key;
            } else {
                Formula.RowKey = -1;
            }
        }
    }

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        DatabaseSet(null, string.Empty);
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(true);
        base.OnFormClosing(e);
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
    /// Ist kein Reiter vorhanden, wird ein neuer erzeugt.
    /// </summary>
    /// <param name="connectionInfo"></param>
    /// <returns></returns>
    protected bool SwitchTabToDatabase(ConnectionInfo connectionInfo) {
        if (connectionInfo is null) { return false; }

        foreach (var thisT in tbcDatabaseSelector.TabPages) {
            if (thisT is System.Windows.Forms.TabPage tp && tp.Tag is List<object> s && s[0] is ConnectionInfo ci) {
                if (ci.UniqueID.Equals(connectionInfo.UniqueID, StringComparison.OrdinalIgnoreCase)) {
                    tbcDatabaseSelector.SelectedTab = tp;

                    if (_firstOne) {
                        _firstOne = false;
                        tbcDatabaseSelector_Selected(null, new System.Windows.Forms.TabControlEventArgs(tp, tbcDatabaseSelector.TabPages.IndexOf(tp), System.Windows.Forms.TabControlAction.Selected));
                    }

                    return true;
                }
            }
        }

        AddTabPage(connectionInfo);
        return SwitchTabToDatabase(connectionInfo); // Rekursiver Aufruf, nun sollt der Tab ja gefunden werden.
    }

    protected bool SwitchTabToDatabase(DatabaseAbstract? database) {
        if (database == null) { return false; }
        return SwitchTabToDatabase(database.ConnectionData);
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
            //e.UserMenu.GenerateAndAdd("Inhalte aller angezeigten Zellen dieser Spalte löschen", "ColumnContentDelete", ImageCode.Radiergummi, column != null && column.Format.CanBeChangedByRules() && tbl.Database.IsAdministrator());
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
                column?.Statisik(Table.Filter, Table.PinnedRows);
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
        //Reihenfolge wichtig, da die Ansicht vieles auf standard zurück setzt
        var s = "{" +
                "Ansicht=" + ((int)_ansicht).ToString() + ", " +
                "MainTab=" + ribMain.SelectedIndex.ToString() + ", " +
                "TableView=" + Table.ViewToString().ToNonCritical() +
                "}";
        return s;
    }

    private void _originalDB_Disposing(object sender, System.EventArgs e) => ChangeDatabase(null);

    private void Ansicht_Click(object sender, System.EventArgs e) {
        if (chkAnsichtFormular.Checked) { _ansicht = Ansicht.Überschriften_und_Formular; } else if (chkAnsichtNurTabelle.Checked) { _ansicht = Ansicht.Nur_Tabelle; } else { _ansicht = Ansicht.Tabelle_und_Formular_nebeneinander; }

        InitView();
        CheckButtons();
    }

    private void btnAlleErweitern_Click(object sender, System.EventArgs e) => Table.ExpandAll();

    private void btnAlleSchließen_Click(object sender, System.EventArgs e) => Table.CollapesAll();

    private void btnClipboardImport_Click(object sender, System.EventArgs e) {
        if (Table.Database == null || !Table.Database.IsAdministrator()) { return; }
        Table.ImportClipboard();
    }

    private void btnDatenbankenSpeicherort_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.ForceLoadSaveAll();
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

        var x = new ConnectedFormulaEditor(Table?.Database.FormulaFileName(), null);
        x.Show();
        //x.Dispose();
    }

    private void btnLayouts_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (Table.Database == null) { return; }
        OpenLayoutEditor(Table.Database, string.Empty);
    }

    private void btnLetzteDateien_ItemClicked(object sender, BasicListItemEventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
        SwitchTabToDatabase(new ConnectionInfo(e.Item.Internal));
    }

    private void btnLoeschen_Click(object sender, System.EventArgs e) {
        if (chkAnsichtFormular.Checked) {
            if (Formula.ShowingRow == null) {
                MessageBox.Show("Kein Eintrag gewählt.", ImageCode.Information, "OK");
                return;
            }
            var tmpr = Formula.ShowingRow;
            if (MessageBox.Show("Soll der Eintrag<br><b>" + tmpr.CellFirstString() + "</b><br>wirklich <b>gelöscht</b> werden?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            SuchEintragNoSave(Direction.Unten, out var column, out var row);
            Table.CursorPos_Set(column, row, false);
            Table.Database.Row.Remove(tmpr);
        } else {
            Table.Database.Row.Remove(Table.Filter, Table.PinnedRows);
        }
    }

    private void btnNeu_Click(object sender, System.EventArgs e) {
        var r = Table.Database.Column.First().SortType == SortierTyp.Datum_Uhrzeit
    ? Table.Database.Row.GenerateAndAdd(NameRepair(DateTime.Now.ToString(Constants.Format_Date5), null))
    : Table.Database.Row.GenerateAndAdd(NameRepair("Neuer Eintrag", null));
        Table.CursorPos_Set(Table.Database.Column.First(), Table.SortedRows().Get(r), true);
    }

    private void btnNeuDB_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);

        SaveTab.ShowDialog();
        if (!DirectoryExists(SaveTab.FileName.FilePath())) { return; }
        if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

        if (FileExists(SaveTab.FileName)) { DeleteFile(SaveTab.FileName, true); }

        var db = new Database(false, string.Empty);
        db.SaveAsAndChangeTo(SaveTab.FileName);
        SwitchTabToDatabase(new ConnectionInfo(SaveTab.FileName));
    }

    private void btnNummerierung_CheckedChanged(object sender, System.EventArgs e) => Table.ShowNumber = btnNummerierung.Checked;

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
        LoadTab.ShowDialog();
    }

    private void btnPowerBearbeitung_Click(object sender, System.EventArgs e) {
        Notification.Show("20 Sekunden (fast) rechtefreies<br>bearbeiten akiviert.", ImageCode.Stift);
        Table.PowerEdit = DateTime.Now.AddSeconds(20);
    }

    private void btnSaveAs_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);

        if (Table.Database is Database db) {
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);

            SaveTab.ShowDialog();
            if (!DirectoryExists(SaveTab.FileName.FilePath())) { return; }
            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

            if (FileExists(SaveTab.FileName)) { DeleteFile(SaveTab.FileName, true); }

            db.SaveAsAndChangeTo(SaveTab.FileName);
            SwitchTabToDatabase(new ConnectionInfo(SaveTab.FileName));
        }
    }

    private void btnSaveLoad_Click(object sender, System.EventArgs e) {
        Table?.Database?.Load_Reload();
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(true);
    }

    private void btnSpaltenanordnung_Click(object sender, System.EventArgs e) {
        var x = new ColumnArrangementPadEditor(Table.Database);
        x.ShowDialog();

        var car = Table.Database.ColumnArrangements.CloneWithClones();
        car[0].ShowAllColumns();
        Table.Database.ColumnArrangements = car;

        Table.Invalidate_HeadSize();
        Table.Invalidate_AllColumnArrangements();
    }

    private void btnSpaltenUebersicht_Click(object sender, System.EventArgs e) => Table.Database.Column.GenerateOverView();

    private void btnSuchenUndErsetzen_Click(object sender, System.EventArgs e) => Table.OpenSearchAndReplace();

    private void btnSuchFenster_Click(object sender, System.EventArgs e) {
        var x = new Search(Table);
        x.Show();
    }

    private void btnTemporärenSpeicherortÖffnen_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.ForceLoadSaveAll();
        ExecuteFile(Path.GetTempPath());
    }

    private void btnTextSuche_Click(object sender, System.EventArgs e) {
        var suchtT = txbTextSuche.Text.Trim();
        if (string.IsNullOrEmpty(suchtT)) {
            MessageBox.Show("Bitte Text zum Suchen eingeben.", ImageCode.Information, "OK");
            return;
        }
        Table.SearchNextText(suchtT, Table, null, Table.CursorPosRow, out _, out var gefRow, true);
        //var CheckRow = BlueFormulax.ShowingRow;
        //RowItem GefRow = null;
        //if (CheckRow == null) { CheckRow = TableView.View_RowFirst(); }
        //var Count = 0;
        //do
        //{
        //    if (Count > TableView.Database.Row.Count() + 1) { break; }
        //    if (GefRow != null && GefRow != BlueFormulax.ShowingRow) { break; }
        //    Count++;
        //    CheckRow = TableView.View_NextRow(CheckRow);
        //    if (CheckRow == null) { CheckRow = TableView.View_RowFirst(); }
        //    foreach (var ThisColumnItem in TableView.Database.Column)
        //    {
        //        if (ThisColumnItem != null)
        //        {
        //            if (!ThisColumnItem.IgnoreAtRowFilter)
        //            {
        //                var IsT = CheckRow.CellGetString(ThisColumnItem);
        //                if (!string.IsNullOrEmpty(IsT))
        //                {
        //                    if (ThisColumnItem.Format == DataFormat.Text_mit_Formatierung)
        //                    {
        //                        var l = new ExtText(enDesign.TextBox, enStates.Standard);
        //                        l.HtmlText = IsT;
        //                        IsT = l.PlainText;
        //                    }
        //                    // Allgemeine Prüfung
        //                    if (IsT.ToLower().Contains(SuchtT.ToLower()))
        //                    {
        //                        GefRow = CheckRow;
        //                    }
        //                    // Spezielle Format-Prüfung
        //                    var SuchT2 = DataFormat.CleanFormat(SuchtT, ThisColumnItem.Format);
        //                    IsT = DataFormat.CleanFormat(IsT, ThisColumnItem.Format);
        //                    if (!string.IsNullOrEmpty(SuchT2) && !string.IsNullOrEmpty(IsT))
        //                    {
        //                        if (IsT.ToLower().Contains(SuchT2.ToLower()))
        //                        {
        //                            GefRow = CheckRow;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //} while (true);
        if (gefRow == null) {
            MessageBox.Show("Kein Eintrag gefunden!", ImageCode.Information, "OK");
        } else {
            if (gefRow?.Row == Formula.ShowingRow) {
                MessageBox.Show("Text nur im <b>aktuellen Eintrag</b> gefunden,<br>aber sonst keine weiteren Einträge!", ImageCode.Information, "OK");
            } else {
                Table.CursorPos_Set(Table.Database.Column[0], gefRow, true);
            }
        }
    }

    private void btnUnterschiede_CheckedChanged(object sender, System.EventArgs e) => Table.Unterschiede = btnUnterschiede.Checked ? Table.CursorPosRow.Row : null;

    private void btnVorwärts_Click(object sender, System.EventArgs e) {
        SuchEintragNoSave(Direction.Unten, out var column, out var row);
        Table.CursorPos_Set(column, row, false);
    }

    private void btnZeileLöschen_Click(object sender, System.EventArgs e) {
        if (!Table.Database.IsAdministrator()) { return; }
        var m = MessageBox.Show("Angezeigte Zeilen löschen?", ImageCode.Warnung, "Ja", "Nein");
        if (m != 0) { return; }
        Table.Database.Row.Remove(Table.Filter, Table.PinnedRows);
    }

    private void btnZurück_Click(object sender, System.EventArgs e) {
        SuchEintragNoSave(Direction.Oben, out var column, out var row);
        Table.CursorPos_Set(column, row, false);
    }

    private void cbxColumnArr_ItemClicked(object sender, BasicListItemEventArgs e) {
        if (string.IsNullOrEmpty(cbxColumnArr.Text)) { return; }
        Table.Arrangement = int.Parse(e.Item.Internal);
    }

    private void ChangeDatabase(DatabaseAbstract? database) {
        if (_originalDb != null) {
            _originalDb.Disposing -= _originalDB_Disposing;
        }
        _originalDb = null;
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

    private void Check_SuchButton() => btnTextSuche.Enabled = Table.Database != null && Table.Database.Row.Count >= 1 && !string.IsNullOrEmpty(txbTextSuche.Text) && !string.IsNullOrEmpty(txbTextSuche.Text.RemoveChars(" "));

    private void InitView() {
        if (DesignMode) {
            grpFormularSteuerung.Visible = true;
            tbcSidebar.Visible = true;
            grpHilfen.Visible = true;
            grpAnsicht.Visible = true;
            FilterLeiste.Visible = true;
            SplitContainer1.Panel2Collapsed = false;
            return;
        }

        chkAnsichtNurTabelle.Checked = _ansicht == Ansicht.Nur_Tabelle;
        chkAnsichtFormular.Checked = _ansicht == Ansicht.Überschriften_und_Formular;
        chkAnsichtTableFormular.Checked = _ansicht == Ansicht.Tabelle_und_Formular_nebeneinander;

        Table?.Filter?.Clear();

        switch (_ansicht) {
            case Ansicht.Nur_Tabelle:
                grpFormularSteuerung.Visible = false;
                Table.Design = BlueTableAppearance.Standard;
                tbcSidebar.Visible = false;
                grpHilfen.Visible = true;
                grpAnsicht.Visible = true;
                FilterLeiste.Visible = true;
                SplitContainer1.IsSplitterFixed = false;
                SplitContainer1.Panel2Collapsed = true;
                break;

            case Ansicht.Überschriften_und_Formular:
                grpFormularSteuerung.Visible = true;
                Table.Design = BlueTableAppearance.OnlyMainColumnWithoutHead;
                tbcSidebar.Visible = true;
                grpHilfen.Visible = false;
                grpAnsicht.Visible = false;
                FilterLeiste.Visible = false;
                SplitContainer1.SplitterDistance = 250;
                SplitContainer1.IsSplitterFixed = true;
                SplitContainer1.Panel2Collapsed = false;
                break;

            case Ansicht.Tabelle_und_Formular_nebeneinander:
                grpFormularSteuerung.Visible = false;
                Table.Design = BlueTableAppearance.Standard;
                tbcSidebar.Visible = true;
                grpHilfen.Visible = true;
                grpAnsicht.Visible = true;
                FilterLeiste.Visible = true;
                SplitContainer1.IsSplitterFixed = false;
                SplitContainer1.Panel2Collapsed = false;
                SplitContainer1.SplitterDistance = Math.Max(SplitContainer1.SplitterDistance, SplitContainer1.Width / 2);
                break;

            default:
                DebugPrint(_ansicht);
                break;
        }

        if (Table.Visible) {
            if (Table.Database != null) {
                if (Table.CursorPosRow == null && Table.View_RowFirst() != null) {
                    Table.CursorPos_Set(Table.Database.Column[0], Table.View_RowFirst(), false);
                }
                if (Table.CursorPosRow != null) {
                    FillFormula(Table.CursorPosRow.Row);
                }
            }
        } else {
            FillFormula(null);
        }
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) => SwitchTabToDatabase(new ConnectionInfo(LoadTab.FileName));

    private string NameRepair(string istName, RowItem? vRow) {
        var newName = istName;
        var istZ = 0;
        do {
            var changed = false;
            if (Table.Database.Row.Any(thisRow => thisRow != null && thisRow != vRow && string.Equals(thisRow.CellFirstString(), newName, StringComparison.OrdinalIgnoreCase))) {
                istZ++;
                newName = istName + " (" + istZ + ")";
                changed = true;
            }
            if (!changed) { return newName; }
        } while (true);
    }

    private void SetFormula(DatabaseAbstract? database) {
        if (database != null) {
            Formula.GetConnectedFormulaFromDatabase(database);
            Formula.Database = database;
            return;
        }

        Formula.Database = null;
        Formula.ConnectedFormula = null;
    }

    private void SuchEintragNoSave(Direction richtung, out ColumnItem? column, out RowData? row) {
        column = Table.Database.Column[0];
        row = null;
        if (Table.Database.Row.Count < 1) { return; }
        // Temporär berechnen, um geflacker zu vermeiden (Endabled - > Disabled bei Nothing)
        if (Convert.ToBoolean(richtung & Direction.Unten)) {
            row = Table.View_NextRow(Table.CursorPosRow);
            if (row == null) { row = Table.View_RowFirst(); }
        }
        if (Convert.ToBoolean(richtung & Direction.Oben)) {
            row = Table.View_PreviousRow(Table.CursorPosRow);
            if (row == null) { row = Table.View_RowLast(); }
        }
        if (row == null) { row = Table.View_RowFirst(); }
    }

    private void Table_EditBeforeBeginEdit(object sender, CellCancelEventArgs e) {
        if (Table.Design == BlueTableAppearance.OnlyMainColumnWithoutHead) {
            e.CancelReason = "In dieser Ansicht kann der Eintrag nicht bearbeitet werden.";
        }
    }

    private void Table_SelectedCellChanged(object sender, CellExtEventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => Table_SelectedCellChanged(sender, e)));
            return;
        }

        if (ckbZeilenclickInsClipboard.Checked) {
            Table.CopyToClipboard(e.Column, e.RowData.Row, false);
            Table.Focus();
        }
    }

    private void Table_SelectedRowChanged(object sender, RowEventArgs? e) {
        if (InvokeRequired) {
            Invoke(new Action(() => Table_SelectedRowChanged(sender, e)));
            return;
        }

        btnUnterschiede_CheckedChanged(this, System.EventArgs.Empty);

        FillFormula(e?.Row);
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
        var s = (List<object>)(e.TabPage.Tag);
        s[1] = ViewToString();
        e.TabPage.Tag = s;
    }

    /// <summary>
    /// Diese Routine lädt die Datenbank, falls nötig.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void tbcDatabaseSelector_Selected(object? sender, System.Windows.Forms.TabControlEventArgs e) {
        Table.ShowWaitScreen = true;
        tbcDatabaseSelector.Enabled = false;
        Table.Enabled = false;
        Table.Refresh();

        BlueBasics.MultiUserFile.MultiUserFile.ForceLoadSaveAll();

        if (e.TabPage == null) { return; }

        var s = (List<object>)(e.TabPage.Tag);

        var DB = Database.GetByID((ConnectionInfo)s[0]);

        if (DB != null) {
            if (!string.IsNullOrEmpty(DB.Filename)) {
                btnLetzteDateien.AddFileName(DB.Filename, DB.TableName);
                LoadTab.FileName = DB.Filename;
            } else {
                btnLetzteDateien.AddFileName(DB.ConnectionData.UniqueID, DB.TableName);
            }

            e.TabPage.Text = DB.Caption;
        }
        DatabaseSet(DB, (string)s[1]);
    }

    private void tbcSidebar_SelectedIndexChanged(object sender, System.EventArgs e) => FillFormula(Table?.CursorPosRow?.Row);

    private void txbTextSuche_Enter(object sender, System.EventArgs e) {
        if (btnTextSuche.Enabled) { btnTextSuche_Click(btnTextSuche, System.EventArgs.Empty); }
    }

    private void txbTextSuche_TextChanged(object sender, System.EventArgs e) => Check_SuchButton();

    #endregion
}