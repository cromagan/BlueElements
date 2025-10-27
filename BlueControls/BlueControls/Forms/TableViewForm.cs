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
using BlueBasics.MultiUserFile;
using BlueControls.BlueTableDialogs;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueTable;
using BlueTable.Enums;
using BlueTable.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueBasics.Develop;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class TableViewForm : FormWithStatusBar, IHasSettings {

    #region Fields

    private bool _firstOne = true;

    #endregion

    #region Constructors

    public TableViewForm() : this(null, true, true, true) { }

    public TableViewForm(Table? table, bool loadTabVisible, bool adminTabVisible, bool usesSettings) : base() {
        InitializeComponent();

        UsesSettings = usesSettings;

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

        _ = SwitchTabToTable(table);
    }

    #endregion

    #region Properties

    public static bool SettingsLoadedStatic { get; set; }

    public static List<string> SettingsStatic { get; set; } = [];

    public List<string> Settings { get => SettingsStatic; set => SettingsStatic = value; }

    public bool SettingsLoaded { get => SettingsLoadedStatic; set => SettingsLoadedStatic = value; }

    public string SettingsManualFilename { get => Application.StartupPath + "\\" + Name + "-Settings.ini"; set { } }

    public bool UsesSettings { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Gibt TRUE zuück, wenn eine Fehlernachricht angezeigt wurde.
    /// </summary>
    /// <param name="table"></param>
    ///
    /// <returns></returns>
    public static bool EditabelErrorMessage(Table? table) {
        if (table == null) { return false; }

        var m = table.AreAllDataCorrect();
        if (string.IsNullOrEmpty(m)) { return false; }

        MessageBox.Show("Aktion nicht möglich:<br>" + m);
        return true;
    }

    public static void OpenLayoutEditor(Table db, string layoutToOpen) {
        var x = db.AreAllDataCorrect();
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

    public static void OpenScriptEditor(Table? db) {
        if (db is not { IsDisposed: false }) { return; }

        _ = IUniqueWindowExtension.ShowOrCreate<TableScriptEditor>(db);
    }

    [StandaloneInfo("Tabellen-Ansicht", ImageCode.Tabelle, "Allgemein", 800)]
    public static System.Windows.Forms.Form Start() => new TableViewForm();

    public void AddTabPage(string tablename) {
        var settings = this.GetSettings("View_" + tablename);

        AddTabPage(tablename, settings);
    }

    public string ReadableText() => "Tabellen Ansicht";

    /// <summary>
    /// Setzt von allen Reitern die Ansichts- und Filtereinstellungen zurück
    /// </summary>
    public void ResetTableSettings() {
        // Used: Only BZL
        foreach (var thisT in tbcTableSelector.TabPages) {
            if (thisT is TabPage { Tag: List<object> s } tp) {
                s[1] = string.Empty;
                tp.Tag = s;
            }
        }
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get("Tabelle|32");

    public TabPage? TabExists(string tablename) {
        tablename = tablename.FileNameWithoutSuffix();

        foreach (var thisT in tbcTableSelector.TabPages) {
            if (thisT is TabPage { Tag: List<object> s } tp && s[0] is string ci) {
                if (ci.Equals(tablename, StringComparison.OrdinalIgnoreCase)) {
                    return tp;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Erstellt einen Reiter mit den nötigen Tags um eine Tabelle laden zu können - lädt die Tabelle aber selbst nicht.
    /// HIer wird auch die Standard-Ansicht als Tag Injiziert
    /// </summary>
    protected void AddTabPage(string tablename, string settings) {
        if (tablename.IsFormat(FormatHolder.FilepathAndName)) {
            tablename = tablename.FileNameWithoutSuffix();
        }

        var nTabPage = new TabPage {
            Name = tbcTableSelector.TabCount.ToString(),
            Text = tablename.ToTitleCase(),
            Tag = (List<object>)[tablename, settings]
        };
        tbcTableSelector.Controls.Add(nTabPage);
    }

    protected virtual void btnCSVClipboard_Click(object sender, System.EventArgs e) {
        _ = CopytoClipboard(Table.Export_CSV(FirstRow.ColumnCaption));
        Notification.Show("Die Daten sind nun<br>in der Zwischenablage.", ImageCode.Clipboard);
    }

    protected virtual void btnDrucken_ItemClicked(object sender, AbstractListItemEventArgs e) {
        MultiUserFile.SaveAll(false);
        BlueTable.Table.SaveAll(false);
        if (IsDisposed || Table.Table is not { IsDisposed: false } db) { return; }

        switch (e.Item.KeyName) {
            case "erweitert":
                Visible = false;
                var selectedRows = Table.RowsVisibleUnique();

                using (var l = new ExportDialog(db, selectedRows)) {
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
        OpenScriptEditor(Table.Table);
        UpdateScripts(Table.Table);
    }

    protected void ChangeTableInTab(string tablename, TabPage? tabpage, string settings) {
        if (tabpage == null) { return; }

        if (tablename.IsFormat(FormatHolder.FilepathAndName)) {
            tablename = tablename.FileNameWithoutSuffix();
        }

        tabpage.Text = tablename.ToTitleCase();

        var s = (List<object>)tabpage.Tag;
        s[0] = tablename;
        s[1] = settings;
        tabpage.Tag = s;
    }

    protected void CheckButtons() {
        if (IsDisposed) { return; }
        var tableFound = Table.Table is { IsDisposed: false };
        btnNeuDB.Enabled = true;
        btnOeffnen.Enabled = true;
        btnDrucken.Enabled = tableFound;

        btnTabellenSpeicherort.Enabled = Table.Table is TableFile { IsFreezed: false } tbf && !string.IsNullOrEmpty(tbf.Filename);

        btnZeileLöschen.Enabled = tableFound;
        lstAufgaben.Enabled = tableFound;
        btnSaveAs.Enabled = tableFound;

        if (btnDrucken["csv"] is { } bli1) {
            bli1.Enabled = tableFound;
        }

        if (btnDrucken["html"] is { } bli2) {
            bli2.Enabled = tableFound;
        }

        btnSuchenUndErsetzen.Enabled = tableFound;
    }

    protected virtual void FillFormula(RowItem? r) {
        if (CFO is null) { return; }

        CFO.GetHeadPageFrom(r?.Table);
        CFO.SetToRow(r);
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);

        if (Table.Table is { IsDisposed: false } db) {
            this.SetSetting("View_" + db.KeyName, ViewToString());
        }

        TableSet(null, string.Empty);
        MultiUserFile.SaveAll(true);
        BlueTable.Table.SaveAll(true);
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        CheckButtons();
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        InitView();
    }

    protected void ShowTab(TabPage tabPage) {
        if (tabPage != tbcTableSelector.SelectedTab) {
            tbcTableSelector.SelectTab(tabPage);
            return;
        }

        Table.ShowWaitScreen = true;
        tbcTableSelector.Enabled = false;
        Table.Enabled = false;
        Table.Refresh();

        BlueTable.Table.SaveAll(false);
        MultiUserFile.SaveAll(false);

        var s = (List<object>)tabPage.Tag;

        if (s[0] is not string tablename) {
            tabPage.Text = "FEHLER";
            UpdateScripts(null);
            TableSet(null, string.Empty);
            return;
        }

        #region Status-Meldung updaten?

        var maybeok = false;
        foreach (var thisdb in BlueTable.Table.AllFiles) {
            if (thisdb.KeyName.Equals(tablename, StringComparison.OrdinalIgnoreCase)) { maybeok = true; break; }
        }

        if (!maybeok) {
            Develop.Message?.Invoke(ErrorType.Info, null, "Tabelle", ImageCode.Tabelle, "Lade Tabelle " + tablename, 0);
        }

        #endregion

        if (BlueTable.Table.Get(tablename, TableView.Table_NeedPassword) is { IsDisposed: false } tb) {
            if (btnLetzteDateien.Parent.Parent.Visible && tb is TableFile tbf) {
                if (!string.IsNullOrEmpty(tbf.Filename)) {
                    btnLetzteDateien.AddFileName(tbf.Filename, tb.KeyName);
                    LoadTab.FileName = tbf.Filename;
                } else {
                    btnLetzteDateien.AddFileName(tbf.Filename, tbf.KeyName);
                }
            }
            tabPage.Text = tb.KeyName.ToTitleCase();
            UpdateScripts(tb);
            TableSet(tb, (string)s[1]);
        } else {
            tabPage.Text = "FEHLER";
            UpdateScripts(null);
            TableSet(null, string.Empty);
        }
    }

    /// <summary>
    /// Sucht den Tab mit der angegebenen Tabelle.
    /// Ist kein Reiter vorhanden, wird ein Neuer erzeugt.
    /// </summary>
    /// <returns></returns>
    protected bool SwitchTabToTable(string tablename) {
        MultiUserFile.SaveAll(false);
        BlueTable.Table.SaveAll(false);

        if (tablename.IsFormat(FormatHolder.FilepathAndName)) {
            _ = BlueTable.Table.Get(tablename, TableView.Table_NeedPassword);
            tablename = tablename.FileNameWithoutSuffix();
        }

        if (TabExists(tablename) is { } tp) {
            tbcTableSelector.SelectedTab = tp; // tbcTableSelector_Selected macht die eigentliche Arbeit

            if (_firstOne) {
                _firstOne = false;
                tbcTableSelector_Selected(null,
                    new TabControlEventArgs(tp, tbcTableSelector.TabPages.IndexOf(tp),
                        TabControlAction.Selected));
            }

            return true;
        }

        AddTabPage(tablename);

        return SwitchTabToTable(tablename); // Rekursiver Aufruf, nun sollt der Tab ja gefunden werden.
    }

    /// <summary>
    /// Sucht den Tab mit der angegebenen Tabelle.
    /// Ist kein Reiter vorhanden, wird ein Neuer erzeugt.
    /// </summary>
    /// <returns></returns>
    protected bool SwitchTabToTable(Table? table) => table is not null && !table.IsDisposed && SwitchTabToTable(table.KeyName);

    protected virtual void Table_ContextMenuInit(object sender, ContextMenuInitEventArgs e) { }

    protected void Table_EnabledChanged(object sender, System.EventArgs e) => Check_OrderButtons();

    protected virtual void Table_SelectedCellChanged(object sender, CellExtEventArgs e) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => Table_SelectedCellChanged(sender, e)));
            return;
        }

        if (ckbZeilenclickInsClipboard.Checked) {
            TableView.CopyToClipboard(e.ColumnView?.Column, e.RowData?.Row, false);
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

    protected virtual void Table_TableChanged(object sender, System.EventArgs e) {
        TableView.WriteColumnArrangementsInto(cbxColumnArr, Table.Table, Table.Arrangement);
        Check_OrderButtons();
        CheckButtons();
    }

    protected void Table_ViewChanged(object sender, System.EventArgs e) =>
        TableView.WriteColumnArrangementsInto(cbxColumnArr, Table.Table, Table.Arrangement);

    protected virtual void Table_VisibleRowsChanged(object sender, System.EventArgs e) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => Table_VisibleRowsChanged(sender, e)));
            return;
        }

        if (Table.Table != null) {
            capZeilen1.Text = "<imagecode=Information|16> " + LanguageTool.DoTranslate("Einzigartige Zeilen:") + " " +
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

    protected virtual void TableSet(Table? db, string toParse) {
        if (db is { IsDisposed: false }) {
            DropMessages = db.IsAdministrator();
            cbxColumnArr.ItemEditAllowed = db.IsAdministrator();
        }

        if (Table.Table != db) {
            CFO.Page = null;
        }

        var did = false;

        if (!string.IsNullOrEmpty(toParse) && toParse.GetAllTags() is { } x) {
            foreach (var pair in x) {
                switch (pair.Key) {
                    case "tableview":
                        Table.TableSet(db, pair.Value.FromNonCritical());
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
                        DebugPrint(ErrorType.Warning, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        if (!did) {
            Table.TableSet(db, string.Empty);
            if (Table.View_RowFirst() != null && db != null) {
                Table.CursorPos_Set(Table.View_ColumnFirst(), Table.View_RowFirst(), false);
            }
        }

        Check_OrderButtons();

        Table.ShowWaitScreen = false;
        tbcTableSelector.Enabled = true;
        Table.Enabled = true;
    }

    protected virtual string ViewToString() {
        //Reihenfolge wichtig, da die Ansicht vieles auf standard zurück setzt

        List<string> result = [];
        result.ParseableAdd("WindowState", WindowState);
        result.ParseableAdd("SplitterX", SplitContainer1.SplitterDistance);
        result.ParseableAdd("MainTab", ribMain.SelectedIndex);
        result.ParseableAdd("TableView", Table.ViewToString());

        return result.FinishParseable();
    }

    private void btnAlleErweitern_Click(object sender, System.EventArgs e) => Table.ExpandAll();

    //    Table.Table = db;
    //    if (Table.Table != null) {
    //        tbcTableSelector.TabPages[toIndex].Text = db.Filename.FileNameWithoutSuffix();
    //        ParseView(DBView[toIndex]);
    //    }
    //    tbcTableSelector.Enabled = true;
    //    Table.Enabled = true;
    //}
    //private void _originalDB_Disposing(object sender, System.EventArgs e) => ChangeTable(null);
    private void btnAlleSchließen_Click(object sender, System.EventArgs e) => Table.CollapesAll();

    private void btnAufräumen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Table is not { IsDisposed: false } db || !db.IsAdministrator()) {
            return;
        }

        Table.RowCleanUp();
    }

    private void btnClipboardImport_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Table is not { IsDisposed: false } db || !db.IsAdministrator()) {
            return;
        }

        Table.ImportClipboard();
    }

    private void btnFormular_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (IsDisposed || Table.Table is not { IsDisposed: false } db) {
            return;
        }

        using var x = new ConnectedFormulaEditor(db.FormulaFileName(), null);

        if (x.IsClosed || x.IsDisposed) { return; }

        _ = x.ShowDialog();
    }

    private void btnLayouts_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (IsDisposed || Table.Table is not { IsDisposed: false } db) { return; }

        OpenLayoutEditor(db, string.Empty);
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) => _ = SwitchTabToTable(e.Item.KeyName);

    private void btnMDBImport_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Table is not { IsDisposed: false } db || !db.IsAdministrator()) {
            return;
        }

        Table.ImportBdb();
    }

    private void btnMonitoring_Click(object sender, System.EventArgs e) => GlobalMonitor.Start();

    private void btnNeuDB_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        BlueTable.Table.SaveAll(false);

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

        var db = new TableFile(SaveTab.FileName.FileNameWithoutSuffix());
        db.SaveAsAndChangeTo(SaveTab.FileName);
        _ = SwitchTabToTable(SaveTab.FileName);
    }

    private void btnNummerierung_CheckedChanged(object sender, System.EventArgs e) => Table.ShowNumber = btnNummerierung.Checked;

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        BlueTable.Table.SaveAll(false);
        _ = LoadTab.ShowDialog();
    }

    private void btnPowerBearbeitung_Click(object sender, System.EventArgs e) {
        Notification.Show("5 Minuten (fast) rechtefreies<br>Bearbeiten aktiviert.", ImageCode.Stift);
        Table.PowerEdit = true;
    }

    private void btnSaveAs_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        BlueTable.Table.SaveAll(false);

        if (Table.Table is TableFile { IsDisposed: false } tbf) {
            if (tbf.IsFreezed) { return; }

            _ = SaveTab.ShowDialog();
            if (!DirectoryExists(SaveTab.FileName.FilePath())) { return; }

            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

            if (FileExists(SaveTab.FileName)) {
                _ = DeleteFile(SaveTab.FileName, true);
            }

            tbf.SaveAsAndChangeTo(SaveTab.FileName);
            _ = SwitchTabToTable(SaveTab.FileName);
        }
    }

    private void btnSaveLoad_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(true);
        BlueTable.Table.SaveAll(true);
        BlueTable.Table.BeSureToBeUpToDate(BlueTable.Table.AllFiles);
    }

    private void btnSpaltenanordnung_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Table is not { IsDisposed: false } db) { return; }

        var tcvc = ColumnViewCollection.ParseAll(db);
        tcvc.GetByKey(cbxColumnArr.Text)?.Edit();
        _ = TableView.RepairColumnArrangements(db);
    }

    private void btnSpaltenUebersicht_Click(object sender, System.EventArgs e) => Table.Table?.Column.GenerateOverView();

    private void btnSuchenUndErsetzen_Click(object sender, System.EventArgs e) => Table.OpenSearchAndReplaceInCells();

    private void btnSuchFenster_Click(object sender, System.EventArgs e) {
        var x = new Search(Table);
        x.Show();
    }

    private void btnSuchInScript_Click(object sender, System.EventArgs e) => Table.OpenSearchAndReplaceInDBScripts();

    private void btnTabelleKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Show(Table.Table, typeof(TableHeadEditor), false);

    private void btnTabellenSpeicherort_Click(object sender, System.EventArgs e) {
        BlueTable.Table.SaveAll(false);
        MultiUserFile.SaveAll(false);

        if (Table.Table is TableFile { IsDisposed: false } tbf) {
            _ = ExecuteFile(tbf.Filename.FilePath());
        }
    }

    private void btnTemporärenSpeicherortÖffnen_Click(object sender, System.EventArgs e) {
        BlueTable.Table.SaveAll(false);
        MultiUserFile.SaveAll(false);
        _ = ExecuteFile(System.IO.Path.GetTempPath());
    }

    private void btnUnterschiede_CheckedChanged(object sender, System.EventArgs e) =>
        Table.Unterschiede = btnUnterschiede.Checked ? Table.CursorPosRow?.Row : null;

    private void btnUserInfo_Click(object sender, System.EventArgs e) {
        var t = new UserInfo();
        t.Show();
    }

    private void btnZeileLöschen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Table is not { IsDisposed: false } db) { return; }
        if (!db.IsAdministrator()) { return; }

        var m = MessageBox.Show("Angezeigte Zeilen löschen?", ImageCode.Warnung, "Ja", "Nein");
        if (m != 0) {
            return;
        }

        _ = RowCollection.Remove(Table.FilterCombined, Table.PinnedRows, "Benutzer: Zeile löschen");
    }

    private void btnZoomFit_Click(object sender, System.EventArgs e) => Table.Zoom = 1f;

    private void btnZoomIn_Click(object sender, System.EventArgs e) => Table.DoZoom(true);

    private void btnZoomOut_Click(object sender, System.EventArgs e) => Table.DoZoom(false);

    private void cbxColumnArr_ItemClicked(object sender, AbstractListItemEventArgs e) => Table.Arrangement = e.Item.KeyName;

    private void Check_OrderButtons() {
        if (IsDisposed) { return; }
        if (InvokeRequired) {
            _ = Invoke(new Action(Check_OrderButtons));
            return;
        }

        if (Table.Table is not { IsDisposed: false } db || !db.IsAdministrator()) {
            tabAdmin.Enabled = false;
            return; // Weitere funktionen benötigen sicher eine Tabelle um keine Null Exception auszulösen
        }

        var m = db.AreAllDataCorrect();

        grpAdminAllgemein.Enabled = string.IsNullOrEmpty(m);
        grpImport.Enabled = string.IsNullOrEmpty(m);
        tabAdmin.Enabled = string.IsNullOrEmpty(m);
    }

    private void InitView() {
        if (DesignMode) {
            tbcSidebar.Visible = true;
            grpHilfen.Visible = true;
            grpAnsicht.Visible = true;
            SplitContainer1.Panel2Collapsed = false;
            return;
        }

        //Table.Filter.Clear();

        tbcSidebar.Visible = true;
        grpHilfen.Visible = true;
        grpAnsicht.Visible = true;
        SplitContainer1.IsSplitterFixed = false;
        SplitContainer1.Panel2Collapsed = false;
        SplitContainer1.SplitterDistance = Math.Max(SplitContainer1.SplitterDistance, SplitContainer1.Width / 2);

        if (Table.Visible) {
            if (Table.Table != null) {
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

        _ = SwitchTabToTable(LoadTab.FileName);
    }

    private void lstAufgaben_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (IsDisposed || Table.Table is not { IsDisposed: false } db) { return; }

        lstAufgaben.Enabled = false;

        if (e.Item is ReadableListItem { Item: TableScriptDescription sc }) {
            string m;

            if (sc.NeedRow) {
                m = MessageBox.Show("Skript bei <b>allen</b> aktuell<br>angezeigten Zeilen ausführen?", ImageCode.Skript, "Ja", "Nein") == 0
                    ? db.Row.ExecuteScript(null, e.Item.KeyName, Table.RowsVisibleUnique())
                    : "Durch Benutzer abgebrochen";
            } else {
                //public Script? ExecuteScript(Events? eventname, string? scriptname, bool onlyTesting, RowItem? row) {
                var s = db.ExecuteScript(sc, sc.ChangeValuesAllowed, null, null, true, true, false);
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
                    Table.OpenColumnEditor(c, null);
                    UpdateScripts(db);
                    return; // lstAufgaben.Enabled = true; wird von UpdateScript gemacht

                case "#datenüberprüfung":
                    var rows = Table.RowsVisibleUnique();
                    if (rows.Count == 0) {
                        MessageBox.Show("Keine Zeilen angezeigt.", ImageCode.Kreuz, "OK");
                        return;
                    }

                    foreach (var thisR in rows) {
                        if (!db.CanDoValueChangedScript(true)) {
                            MessageBox.Show("Abbruch, Skriptfehler sind aufgetreten.", ImageCode.Warnung, "OK");
                            RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);

                            lstAufgaben.Enabled = true;
                            return;
                        }

                        thisR.InvalidateRowState("TableView, Kontextmenü, Datenüberprüfung");
                        _ = thisR.UpdateRow(true, "TableView, Kontextmenü, Datenüberprüfung");
                    }

                    RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);

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
    //        if (Table.Table != null && Table.Table.Row.Any(thisRow =>
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

    //    if (Table.Table is not Table db || db.Row.Count < 1) { return; }

    //    // Temporär berechnen, um geflacker zu vermeiden (Endabled - > Disabled bei Nothing)
    //    if (richtung.HasFlag(Direction.Unten)) {
    //        row = Table.View_NextRow(Table.CursorPosRow) ?? Table.View_RowFirst();
    //    }

    //    if (richtung.HasFlag(Direction.Oben)) {
    //        row = Table.View_PreviousRow(Table.CursorPosRow) ?? Table.View_RowLast();
    //    }

    //    row ??= Table.View_RowFirst();
    //}

    private void tbcTableSelector_Deselecting(object sender, TabControlCancelEventArgs e) {
        var s = (List<object>)e.TabPage.Tag;
        s[1] = ViewToString();

        e.TabPage.Tag = s;

        if (Table.Table is { IsDisposed: false } db) {
            this.SetSetting("View_" + db.KeyName, ViewToString());
        }
    }

    /// <summary>
    /// Diese Routine lädt die Tabelle, falls nötig.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void tbcTableSelector_Selected(object? sender, TabControlEventArgs e) => ShowTab(e.TabPage);

    private void UpdateScripts(Table? db) {
        Table.Invalidate();
        lstAufgaben.ItemClear();

        bool addedit = true;

        if (db is not { IsDisposed: false } || !string.IsNullOrEmpty(db.FreezedReason)) {
            lstAufgaben.Enabled = false;
            return;
        }

        var l = new List<ColumnItem>();
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

            if (thisColumnItem.IsFirst) { l.Add(thisColumnItem); }
        }

        if (l.Count > 1) {
            foreach (var thisColumnItem in l) {
                var d = ItemOf("Spalte ' " + thisColumnItem.KeyName + " ' ist die erste Spalte", "#repaircolumn|" + thisColumnItem.KeyName, ImageCode.Kritisch);
                d.Enabled = db.IsAdministrator();
                lstAufgaben.ItemAdd(d);
            }

            lstAufgaben.Enabled = true;
            return;
        }

        if (!string.IsNullOrEmpty(db.CheckScriptError())) {
            var d = ItemOf("Skripte reparieren", "#repairscript", ImageCode.Kritisch);
            d.Enabled = db.IsAdministrator();
            lstAufgaben.ItemAdd(d);
            lstAufgaben.Enabled = true;
            addedit = false;
            //return;
        }

        if (!db.IsRowScriptPossible()) {
            var d = ItemOf("Zeilen-Skripte erlauben", "#enablerowscript", ImageCode.Spalte);
            d.Enabled = db.IsAdministrator();
            lstAufgaben.ItemAdd(d);
            lstAufgaben.Enabled = true;
        }

        foreach (var script in db.EventScript.Where(s => s.UserGroups.Count > 0)) {
            var item = ItemOf(script);
            lstAufgaben.ItemAdd(item);
            item.Enabled = db.PermissionCheck(script.UserGroups, null)
                            && script.IsOk()
                            && (!script.NeedRow || db.IsRowScriptPossible());
        }

        lstAufgaben.ItemAdd(ItemOf("Komplette Datenüberprüfung", "#datenüberprüfung", ImageCode.HäkchenDoppelt, db.CanDoValueChangedScript(true)));

        if (addedit) {
            var d = ItemOf("Skripte bearbeiten", "#editscript", ImageCode.Skript);
            lstAufgaben.ItemAdd(d);
            d.Enabled = db.IsAdministrator();
        }

        lstAufgaben.Enabled = lstAufgaben.ItemCount > 0;
    }

    #endregion
}