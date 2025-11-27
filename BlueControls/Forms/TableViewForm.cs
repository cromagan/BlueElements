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
using BlueTable;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
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

    public TableViewForm() : this(null, true, true, true) {
    }

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

        this.LoadSettingsFromDisk(false);

        SwitchTabToTable(table);

        FormManager.FormAdded += FormManager_FormsChanged;
        FormManager.FormRemoved += FormManager_FormsChanged;

        // CheckButtons(); -> OnShown
    }

    #endregion

    #region Properties

    public static bool SettingsLoadedStatic { get; set; }

    public static List<string> SettingsStatic { get; set; } = [];

    public List<string> Settings { get => SettingsStatic; set => SettingsStatic = value; }

    public bool SettingsLoaded { get => SettingsLoadedStatic; set => SettingsLoadedStatic = value; }

    public string SettingsManualFilename { get => Application.StartupPath + "\\" + Name + "-Settings.ini"; set { } }

    public bool UsesSettings { get; }

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

        if (table.IsEditable(false)) { return false; } // keine Message angzeigt, alles OK!

        MessageBox.Show($"<b><u>Aktion nicht möglich:</u></b><br><br><u>Grund:</u><br>{table.IsNotEditableReason(false)}", ImageCode.Information, "OK");
        return true;
    }

    public static void OpenLayoutEditor(Table tb, string layoutToOpen) {
        if (EditabelErrorMessage(tb)) { return; }

        var w = new PadEditorWithFileAccess();

        if (!string.IsNullOrWhiteSpace(layoutToOpen)) {
            w.LoadFile(layoutToOpen);
        }

        w.ShowDialog();
    }

    public static TableScriptEditor? OpenScriptEditor(Table? tb) {
        if (EditabelErrorMessage(tb)) { return null; }

        return IUniqueWindowExtension.ShowOrCreate<TableScriptEditor>(tb);
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
        CopytoClipboard(Table.Export_CSV(FirstRow.ColumnCaption));
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

    protected virtual void btnHTMLExport_Click(object sender, System.EventArgs e) => Table.Export_HTML();

    protected virtual void ContextMenu_OpenScriptEditor(object sender, System.EventArgs e) => OpenScriptEditor(Table.Table);

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

    protected virtual void FillFormula(RowItem? r) {
        if (CFO is null) { return; }

        CFO.GetHeadPageFrom(r?.Table);
        CFO.SetToRow(r);
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);

        FormManager.FormAdded -= FormManager_FormsChanged;
        FormManager.FormRemoved -= FormManager_FormsChanged;

        if (Table.Table is { IsDisposed: false } tb) {
            this.SetSetting("View_" + tb.KeyName, ViewToString());
        }

        TableSet(null, string.Empty);
        MultiUserFile.SaveAll(true);
        BlueTable.Table.SaveAll(true);
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
            TableSet(null, string.Empty);
            return;
        }

        #region Status-Meldung updaten?

        var maybeok = false;
        foreach (var thisTb in BlueTable.Table.AllFiles) {
            if (thisTb.KeyName.Equals(tablename, StringComparison.OrdinalIgnoreCase)) { maybeok = true; break; }
        }

        if (!maybeok) {
            Develop.Message?.Invoke(ErrorType.Info, null, "Tabelle", ImageCode.Tabelle, "Lade Tabelle " + tablename, 0);
        }

        #endregion

        if (BlueTable.Table.Get(tablename, TableView.Table_NeedPassword, false) is { IsDisposed: false } tb) {
            if (btnLetzteDateien.Parent.Parent.Visible && tb is TableFile tbf) {
                if (!string.IsNullOrEmpty(tbf.Filename)) {
                    btnLetzteDateien.AddFileName(tbf.Filename, tb.KeyName);
                    LoadTab.FileName = tbf.Filename;
                } else {
                    btnLetzteDateien.AddFileName(tbf.Filename, tbf.KeyName);
                }
            }
            tabPage.Text = tb.KeyName.ToTitleCase();
            TableSet(tb, (string)s[1]);
        } else {
            tabPage.Text = "FEHLER";
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
            BlueTable.Table.Get(tablename, TableView.Table_NeedPassword, false);
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
    protected bool SwitchTabToTable(Table? table) => table?.IsDisposed == false && SwitchTabToTable(table.KeyName);

    protected virtual void Table_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
    }

    protected virtual void Table_SelectedCellChanged(object sender, CellExtEventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => Table_SelectedCellChanged(sender, e)));
            return;
        }

        if (ckbZeilenclickInsClipboard.Checked) {
            TableView.CopyToClipboard(e.ColumnView?.Column, e.RowData?.Row, false);
            Table.Focus();
        }
    }

    protected virtual void Table_SelectedRowChanged(object sender, RowNullableEventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => Table_SelectedRowChanged(sender, e)));
            return;
        }

        btnUnterschiede_CheckedChanged(this, System.EventArgs.Empty);

        FillFormula(e.Row);
    }

    protected virtual void Table_TableChanged(object sender, System.EventArgs e) {
        TableView.WriteColumnArrangementsInto(cbxColumnArr, Table.Table, Table.Arrangement);
        CheckButtons();
    }

    protected void Table_ViewChanged(object sender, System.EventArgs e) =>
        TableView.WriteColumnArrangementsInto(cbxColumnArr, Table.Table, Table.Arrangement);

    protected virtual void Table_VisibleRowsChanged(object sender, System.EventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => Table_VisibleRowsChanged(sender, e)));
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

    protected virtual void TableSet(Table? tb, string toParse) {
        if (Table.Table != tb) {
            CFO.Page = null;
        }

        var did = false;

        if (!string.IsNullOrEmpty(toParse) && toParse.GetAllTags() is { } x) {
            foreach (var pair in x) {
                switch (pair.Key) {
                    case "tableview":
                        Table.TableSet(tb, pair.Value.FromNonCritical());
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
            Table.TableSet(tb, string.Empty);
            if (Table.View_RowFirst() != null && tb != null) {
                Table.CursorPos_Set(Table.View_ColumnFirst(), Table.View_RowFirst(), false);
            }
        }

        if (tb != null) {
            tb.Loaded += Tb_Loaded;
            tb.InvalidateView += Tb_InvalidateView;
        }

        CheckButtons();
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

    private void btnAlleSchließen_Click(object sender, System.EventArgs e) => Table.CollapesAll();

    private void btnAufräumen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Table is not { IsDisposed: false } db || !db.IsAdministrator()) { return; }

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

        x.ShowDialog();
    }

    private void btnLayouts_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (IsDisposed || Table.Table is not { IsDisposed: false } db) { return; }

        OpenLayoutEditor(db, string.Empty);
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) => SwitchTabToTable(e.Item.KeyName);

    private void btnMDBImport_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Table is not { IsDisposed: false } db || !db.IsAdministrator()) {
            return;
        }

        Table.ImportBtb();
    }

    private void btnMonitoring_Click(object sender, System.EventArgs e) => GlobalMonitor.Start();

    private void btnNeuDB_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        BlueTable.Table.SaveAll(false);

        SaveTab.ShowDialog();
        if (!DirectoryExists(SaveTab.FileName.FilePath())) {
            return;
        }

        if (string.IsNullOrEmpty(SaveTab.FileName)) {
            return;
        }

        if (FileExists(SaveTab.FileName)) {
            DeleteFile(SaveTab.FileName, true);
        }

        var db = new TableFile(SaveTab.FileName.FileNameWithoutSuffix());
        db.SaveAsAndChangeTo(SaveTab.FileName);
        SwitchTabToTable(SaveTab.FileName);
    }

    private void btnNummerierung_CheckedChanged(object sender, System.EventArgs e) => Table.ShowNumber = btnNummerierung.Checked;

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        BlueTable.Table.SaveAll(false);
        LoadTab.ShowDialog();
    }

    private void btnPowerBearbeitung_Click(object sender, System.EventArgs e) {
        Notification.Show("5 Minuten (fast) rechtefreies<br>Bearbeiten aktiviert.", ImageCode.Stift);
        Table.PowerEdit = true;
    }

    private void btnSaveAs_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        BlueTable.Table.SaveAll(false);

        if (Table.Table is TableFile { IsDisposed: false } tbf) {
            if (!tbf.IsEditable(false)) { return; }

            SaveTab.ShowDialog();
            if (!DirectoryExists(SaveTab.FileName.FilePath())) { return; }

            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

            if (FileExists(SaveTab.FileName)) {
                DeleteFile(SaveTab.FileName, true);
            }

            tbf.SaveAsAndChangeTo(SaveTab.FileName);
            SwitchTabToTable(SaveTab.FileName);
        }
    }

    private void btnSaveLoad_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(true);
        BlueTable.Table.SaveAll(true);
        BlueTable.Table.BeSureToBeUpToDate(BlueTable.Table.AllFiles, false);
    }

    private void btnSpaltenanordnung_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table.Table is not { IsDisposed: false } db) { return; }

        var tcvc = ColumnViewCollection.ParseAll(db);
        tcvc.GetByKey(cbxColumnArr.Text)?.Edit();
        TableView.RepairColumnArrangements(db);
    }

    private void btnSpaltenUebersicht_Click(object sender, System.EventArgs e) => Table.Table?.Column.GenerateOverView();

    private void btnSuchenUndErsetzen_Click(object sender, System.EventArgs e) => Table.OpenSearchAndReplaceInCells();

    private void btnSuchFenster_Click(object sender, System.EventArgs e) {
        var x = new Search(Table);
        x.Show();
    }

    private void btnSuchInScript_Click(object sender, System.EventArgs e) => Table.OpenSearchAndReplaceInTbScripts();

    private void btnTabelleKopf_Click(object sender, System.EventArgs e) {
        if (EditabelErrorMessage(Table.Table)) { return; }
        InputBoxEditor.Show(Table.Table, typeof(TableHeadEditor), false);
    }

    private void btnTabellenSpeicherort_Click(object sender, System.EventArgs e) {
        BlueTable.Table.SaveAll(false);
        MultiUserFile.SaveAll(false);

        if (Table.Table is TableFile { IsDisposed: false } tbf) {
            ExecuteFile(tbf.Filename.FilePath());
        }
    }

    private void btnTemporärenSpeicherortÖffnen_Click(object sender, System.EventArgs e) {
        BlueTable.Table.SaveAll(false);
        MultiUserFile.SaveAll(false);
        ExecuteFile(System.IO.Path.GetTempPath());
    }

    private void btnUnterschiede_CheckedChanged(object sender, System.EventArgs e) =>
        Table.Unterschiede = btnUnterschiede.Checked ? Table.CursorPosRow?.Row : null;

    private void btnUserInfo_Click(object sender, System.EventArgs e) {
        var t = new UserInfo();
        t.Show();
    }

    private void btnZeileLöschen_Click(object sender, System.EventArgs e) => TableView.ContextMenu_DeleteRow(sender, new ObjectEventArgs(Table.RowsVisibleUnique()));

    private void btnZoomFit_Click(object sender, System.EventArgs e) => Table.Zoom = 1f;

    private void btnZoomIn_Click(object sender, System.EventArgs e) => Table.DoZoom(true);

    private void btnZoomOut_Click(object sender, System.EventArgs e) => Table.DoZoom(false);

    private void cbxColumnArr_ItemClicked(object sender, AbstractListItemEventArgs e) => Table.Arrangement = e.Item.KeyName;

    private void CheckButtons() {
        if (IsDisposed || IsClosed) { return; }

        if (InvokeRequired) {
            try {
                Invoke(new Action(CheckButtons));
            } catch { }
            return;
        }

        var tb = Table?.Table;
        var isEditable = false;
        var isAdmin = false;

        if (tb is { IsDisposed: false }) {
            isAdmin = tb.IsAdministrator();
            isEditable = tb.IsEditable(false);

            Table?.ShowWaitScreen = false;
            tbcTableSelector.Enabled = true;
            Table?.Enabled = true;
        } else {
            Table?.ShowWaitScreen = true;
            tbcTableSelector.Enabled = false;
            Table?.Enabled = false;
            tb = null;
        }

        if (isEditable) {
            List<System.Windows.Forms.Form> f = [.. FormManager.Forms];

            foreach (var thisf in f) {
                if (thisf is IHasTable iht && iht.Table == tb && thisf is IIsEditor) {
                    isEditable = false;
                    break;
                }
            }
        }

        var combi = isEditable && isAdmin;

        cbxColumnArr.ItemEditAllowed = combi;
        DropMessages = combi;
        grpAdminAllgemein.Enabled = combi;
        grpImport.Enabled = combi;
        tabAdmin.Enabled = combi;

        btnNeuDB.Enabled = true;
        btnOeffnen.Enabled = true;
        btnDrucken.Enabled = combi;

        btnTabellenSpeicherort.Enabled = combi && tb is TableFile { } tbf && !string.IsNullOrEmpty(tbf.Filename);

        btnZeileLöschen.Enabled = combi;
        lstAufgaben.Enabled = combi;
        btnSaveAs.Enabled = combi;

        btnDrucken["csv"]?.Enabled = combi;

        btnDrucken["html"]?.Enabled = combi;

        btnSuchenUndErsetzen.Enabled = combi;

        UpdateScripts(tb);
    }

    private void FormManager_FormsChanged(object sender, FormEventArgs e) {
        if (e.Form is IHasTable iht && iht.Table == Table?.Table) {
            CheckButtons();
        }
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

        CheckButtons();
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) {
        if (!FileExists(LoadTab.FileName)) { return; }

        SwitchTabToTable(LoadTab.FileName);
    }

    private void Tb_InvalidateView(object sender, System.EventArgs e) => Table.Invalidate();

    private void Tb_Loaded(object sender, FirstEventArgs e) => CheckButtons();

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

    private void UpdateScripts(Table? tb) {
        Table.Invalidate();
        lstAufgaben.ItemClear();

        if (tb is not { IsDisposed: false } || !tb.IsEditable(false)) {
            lstAufgaben.Enabled = false;
            return;
        }

        var l = new List<ColumnItem>();
        var ok = true;
        foreach (var thisColumnItem in tb.Column) {
            if (!thisColumnItem.IsOk()) {
                lstAufgaben.ItemAdd(ItemOf($"Spalte '{thisColumnItem.KeyName}' reparieren", QuickImage.Get(ImageCode.Kritisch, 16), TableView.ContextMenu_EditColumnProperties, thisColumnItem, tb.IsAdministrator()));
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
                lstAufgaben.ItemAdd(ItemOf($"Spalte '{thisColumnItem.KeyName}' ist die erste Spalte", QuickImage.Get(ImageCode.Kritisch, 16), TableView.ContextMenu_EditColumnProperties, thisColumnItem, tb.IsAdministrator()));
            }

            lstAufgaben.Enabled = true;
            return;
        }

        var addedit = true;
        if (!string.IsNullOrEmpty(tb.CheckScriptError())) {
            lstAufgaben.ItemAdd(ItemOf("Skripte reparieren", ImageCode.Kritisch, ContextMenu_OpenScriptEditor, null, tb.IsAdministrator()));
            addedit = false;
        }

        if (!tb.IsRowScriptPossible()) {
            lstAufgaben.ItemAdd(ItemOf("Zeilen-Skripte erlauben", ImageCode.Spalte, ContextMenu_EnableRowScript, null, tb.IsAdministrator()));
        }

        foreach (var script in tb.EventScript.Where(s => s.UserGroups.Count > 0)) {
            lstAufgaben.ItemAdd(ItemOf(script.ReadableText(), script.SymbolForReadableText(), TableView.ContextMenu_ExecuteScript, new { Script = script, Rows = (Func<List<RowItem>>)Table.RowsVisibleUnique }, tb.PermissionCheck(script.UserGroups, null) && script.IsOk() && (!script.NeedRow || tb.IsRowScriptPossible())));
        }

        lstAufgaben.ItemAdd(ItemOf("Komplette Datenüberprüfung", QuickImage.Get(ImageCode.HäkchenDoppelt, 16), TableView.ContextMenu_DataValidation, (Func<List<RowItem>>)Table.RowsVisibleUnique, tb.CanDoValueChangedScript(true)));

        if (addedit) {
            lstAufgaben.ItemAdd(ItemOf("Skripte bearbeiten", ImageCode.Skript, ContextMenu_OpenScriptEditor, null, tb.IsAdministrator()));
        }

        lstAufgaben.Enabled = lstAufgaben.ItemCount > 0;
    }

    internal void ContextMenu_EnableRowScript(object sender, System.EventArgs e) {
        Table.Table?.EnableScript();
        CheckButtons();
    }

    #endregion
}