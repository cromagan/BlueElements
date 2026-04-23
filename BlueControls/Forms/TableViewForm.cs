// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueBasics.Classes;
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.BlueTableDialogs;
using BlueControls.Classes;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueTable.Classes;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Develop;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class TableViewForm : FormWithStatusBar {

    #region Fields

    private bool _firstOne = true;

    #endregion

    #region Constructors

    public TableViewForm() : this(null, true, true) {
    }

    public TableViewForm(Table? table, bool loadTabVisible, bool adminTabVisible) : base() {
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

        var keyName = table?.KeyName;
        if (!string.IsNullOrEmpty(keyName)) {
            SwitchTabToTable(keyName);
        }
        TableView.ViewSaving += Table_ViewSaving;
        TableView.ViewLoading += Table_ViewLoading;

        FormManager.FormAdded += FormManager_FormsChanged;
        FormManager.FormRemoved += FormManager_FormsChanged;

        // CheckButtons(); -> OnShown
    }

    #endregion

    #region Properties

    public Table? Table {
        get => TableView.Table;

        set {
            if (TableView.Table == value) {
                return;
            }

            if (TableView.Table is { } tbold) {
                tbold.Loaded -= Tb_Loaded;
                tbold.InvalidateView -= Tb_InvalidateView;
            }

            CFO.Page = null;
            TableView.Table = value;

            if (value is { } tbnew) {
                tbnew.Loaded += Tb_Loaded;
                tbnew.InvalidateView += Tb_InvalidateView;
            }

            CheckButtons(true);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gibt TRUE zuück, wenn eine Fehlernachricht angezeigt wurde.
    /// </summary>
    /// <param name="table"></param>
    /// <param name="row"></param>
    ///
    /// <returns></returns>
    public static bool EditableErrorMessage(Table? table, RowItem? row) {
        var message = string.Empty;

        if (table == null) {
            message = "Interner Fehler!";
        } else {
            if (row == null) {
                var id = TableChunk.GetChunkId(table, TableDataType.Command_AddColumnByName, string.Empty);
                message = table.IsValueEditable(TableDataType.Command_AddColumnByName, id);
            } else {
                var id = TableChunk.GetChunkId(row);
                message = table.IsValueEditable(TableDataType.UTF8Value_withoutSizeData, id);
            }
        }

        if (string.IsNullOrEmpty(message)) { return false; }

        MessageBox.Show($"<b><u>Aktion nicht möglich:</u></b><br><br><u>Grund:</u><br>{message}", ImageCode.Information, "OK");
        return true;
    }

    public static void OpenLayoutEditor(Table tb, string layoutToOpen) {
        if (EditableErrorMessage(tb, null)) { return; }

        var w = new PadEditorWithFileAccess();

        if (!string.IsNullOrWhiteSpace(layoutToOpen)) {
            w.LoadFile(layoutToOpen);
        }

        w.ShowDialog();
    }

    public static TableScriptEditor? OpenScriptEditor(Table? tb) {
        if (EditableErrorMessage(tb, null)) { return null; }

        return IUniqueWindowExtension.ShowOrCreate<TableScriptEditor>(tb);
    }

    [StandaloneInfo("Tabellen-Ansicht", ImageCode.Tabelle, "Allgemein", "Allgemeine Tabellen-Ansicht", 800)]
    public static System.Windows.Forms.Form Start() => new TableViewForm();

    /// <summary>
    /// Erstellt einen Reiter mit den nötigen Tags um eine Tabelle laden zu können - lädt die Tabelle aber selbst nicht.
    /// HIer wird auch die Standard-Ansicht als Tag Injiziert
    /// </summary>
    public void AddTabPage(string tablename) {
        if (tablename.IsFormat(FormatHolder.FilepathAndName)) {
            tablename = tablename.FileNameWithoutSuffix();
        }

        var nTabPage = new TabPage {
            Name = tbcTableSelector.TabCount.ToString1(),
            Text = tablename.ToTitleCase(),
            Tag = new List<object?> { tablename, null }
        };
        tbcTableSelector.Controls.Add(nTabPage);
    }

    public void InitTabs(ICollection<string>? initialTabellen, int startindex) {

        #region Tabellen Initialisieren

        initialTabellen ??= [];
        if (initialTabellen.Count > 0) {
            foreach (var t in initialTabellen) {
                AddTabPage(t);
            }
            ShowTab(tbcTableSelector.TabPages[startindex]);
        }

        #endregion
    }

    /// <summary>
    /// Setzt von allen Reitern die Ansichts- und Filtereinstellungen zurück
    /// </summary>
    public void ResetTableSettings() {
        // Used: Only BZL
        foreach (var thisT in tbcTableSelector.TabPages) {
            if (thisT is TabPage { Tag: List<object> s } tp) {
                s[1] = null;
                tp.Tag = s;
            }
        }
    }

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

    public virtual void Table_ViewLoading(object? sender, BlueControls.EventArgs.ViewEventArgs e) {
        ribMain.SelectedIndex = e.ViewData.GetInt("MainTab");
        var splitterX = e.ViewData.GetInt("SplitterX");
        if (splitterX > 0 && splitterX < SplitContainer1.Width - SplitContainer1.SplitterWidth) {
            SplitContainer1.SplitterDistance = splitterX;
        }
        //var windowState = e.ViewData.GetInt("WindowState");
        //if (windowState >= 0 && windowState <= 2) {
        //    WindowState = (FormWindowState)windowState;
        //}
    }

    public virtual void Table_ViewSaving(object? sender, BlueControls.EventArgs.ViewEventArgs e) {
        e.ViewData.Add("WindowState", (int)WindowState);
        e.ViewData.Add("SplitterX", SplitContainer1.SplitterDistance);
        e.ViewData.Add("MainTab", ribMain.SelectedIndex);
    }

    internal void ContextMenu_EnableRowScript(object? sender, ContextMenuEventArgs e) {
        TableView.Table?.EnableScript();
        CheckButtons(true);
    }

    protected static void ChangeTableInTab(string tablename, TabPage? tabpage, JsonObject? settings) {
        if (tabpage == null) { return; }

        if (tablename.IsFormat(FormatHolder.FilepathAndName)) {
            tablename = tablename.FileNameWithoutSuffix();
        }

        tabpage.Text = tablename.ToTitleCase();

        if (tabpage.Tag is not List<object> s) { return; }

        s[0] = tablename;
        s[1] = settings;
        tabpage.Tag = s;
    }

    protected virtual void btnCSVClipboard_Click(object sender, System.EventArgs e) {
        CopytoClipboard(TableView.Export_CSV(FirstRow.ColumnCaption));
        Notification.Show("Die Daten sind nun<br>in der Zwischenablage.", ImageCode.Clipboard);
    }

    protected virtual void btnDrucken_ItemClicked(object sender, AbstractListItemEventArgs e) {
        CachedFileSystem.SaveAll(false);
        Table.SaveAll(false);
        if (IsDisposed || TableView.Table is not { IsDisposed: false } tb) { return; }

        switch (e.Item.KeyName) {
            case "erweitert":
                Visible = false;
                var selectedRows = TableView.RowsVisibleUnique();

                using (var l = new ExportDialog(tb, selectedRows)) {
                    l.ShowDialog();
                }

                Visible = true;
                break;

            case "csv":
                CopytoClipboard(TableView.Export_CSV(FirstRow.ColumnCaption));
                MessageBox.Show("Die gewünschten Daten<br>sind nun im Zwischenspeicher.", ImageCode.Clipboard, "Ok");
                break;

            case "html":
                TableView.Export_HTML();
                break;

            default:
                DebugPrint(e.Item);
                break;
        }
    }

    protected virtual void btnHTMLExport_Click(object sender, System.EventArgs e) => TableView.Export_HTML();

    protected virtual void ContextMenu_OpenScriptEditor(object? sender, System.EventArgs e) => OpenScriptEditor(TableView.Table);

    protected virtual void FillFormula(RowItem? r) {
        if (CFO is null) { return; }

        CFO.GetHeadPageFrom(r?.Table);
        CFO.SetToRow(r);
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        TableView.SaveCurrentView("Letzte Ansicht");

        base.OnFormClosing(e);

        if (e.Cancel) { return; }



        FormManager.FormAdded -= FormManager_FormsChanged;
        FormManager.FormRemoved -= FormManager_FormsChanged;

        TableView.ViewSaving -= Table_ViewSaving;
        TableView.ViewLoading -= Table_ViewLoading;

        Table = null;
        CachedFileSystem.SaveAll(true);
        Table.SaveAll(true);
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

        // Performance: UI-Updates bündeln
        TableView.ShowWaitScreen = true;
        tbcTableSelector.Enabled = false;
        TableView.Enabled = false;
        TableView.Refresh();

        // Nur speichern wenn nötig, um Hänger zu vermeiden
        Table.SaveAll(false);
        CachedFileSystem.SaveAll(false);

        var s = (List<object>)tabPage.Tag;

        if (s[0] is not string tablename) {
            tabPage.Text = "FEHLER";
            TableView.Table = null;
            return;
        }

        #region Status-Meldung updaten?

        var maybeok = false;
        foreach (var thisTb in Table.AllFiles) {
            if (thisTb.KeyName.Equals(tablename, StringComparison.OrdinalIgnoreCase)) { maybeok = true; break; }
        }

        if (!maybeok) {
            Message(ErrorType.Info, null, "Tabelle", ImageCode.Tabelle, $"Lade Tabelle {tablename}", 0);
        }

        #endregion

        var tb = Table.Get(tablename, BlueControls.Controls.TableView.Table_NeedPassword);
        if (tb is { IsDisposed: false }) {
            if (btnLetzteDateien.Parent.Parent.Visible && tb is TableFile tbf) {
                if (!string.IsNullOrEmpty(tbf.Filename)) {
                    btnLetzteDateien.AddFileName(tbf.Filename, tb.KeyName);
                    LoadTab.FileName = tbf.Filename;
                } else {
                    btnLetzteDateien.AddFileName(tbf.Filename, tbf.KeyName);
                }
            }
            tabPage.Text = tb.KeyName.ToTitleCase();
            Table = tb;

            if (s[1] is JsonObject root) {
                TableView.SetView(root);
            } else {
                TableView.CursorPos_Set(TableView.View_ColumnFirst(), TableView.View_RowFirst(), false);
            }
        } else {
            tabPage.Text = "FEHLER";
            Table = null;
        }
    }

    /// <summary>
    /// Sucht den Tab mit der angegebenen Tabelle.
    /// Ist kein Reiter vorhanden, wird ein Neuer erzeugt.
    /// </summary>
    /// <returns></returns>
    protected bool SwitchTabToTable(string tablename) {
        CachedFileSystem.SaveAll(false);
        Table.SaveAll(false);
        if (tablename.IsFormat(FormatHolder.FilepathAndName)) {
            Table.Get(tablename, BlueControls.Controls.TableView.Table_NeedPassword);
            tablename = tablename.FileNameWithoutSuffix();
        }

        var tp = TabExists(tablename);
        if (tp != null) {
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

    protected virtual void Table_SelectedCellChanged(object sender, CellExtEventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => Table_SelectedCellChanged(sender, e)));
            return;
        }

        if (ckbZeilenclickInsClipboard.Checked) {
            BlueControls.Controls.TableView.CopyToClipboard(e.ColumnView?.Column, e.RowData?.Row, false);

            TableView.Focus();
        }
    }

    protected virtual void Table_SelectedRowChanged(object sender, RowNullableEventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => Table_SelectedRowChanged(sender, e)));
            return;
        }

        FillFormula(e.Row);
    }

    protected virtual void Table_TableChanged(object sender, TableEventArgs e) {
        BlueControls.Controls.TableView.WriteColumnArrangementsInto(cbxColumnArr, TableView.Table, TableView.Arrangement);
        CheckButtons(true);
    }

    protected void Table_ViewChanged(object sender, System.EventArgs e) =>
        BlueControls.Controls.TableView.WriteColumnArrangementsInto(cbxColumnArr, TableView.Table, TableView.Arrangement);

    protected virtual void Table_VisibleRowsChanged(object sender, TableEventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => Table_VisibleRowsChanged(sender, e)));
            return;
        }

        if (TableView.Table != null) {
            capZeilen1.Text = $"<imagecode=Information|16> {LanguageTool.DoTranslate("Einzigartige Zeilen:")} {TableView.RowsVisibleUnique().Count} {LanguageTool.DoTranslate("St.")}";
        } else {
            capZeilen1.Text = string.Empty;
        }

        capZeilen1.Refresh(); // Backgroundworker lassen wenig luft
        capZeilen2.Text = capZeilen1.Text;
        capZeilen2.Refresh();
    }

    private void btnAlleErweitern_Click(object sender, System.EventArgs e) => TableView.ExpandAll();

    private void btnAlleSchließen_Click(object sender, System.EventArgs e) => TableView.CollapesAll();

    private void btnAufräumen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || TableView.Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return; }

        TableView.RowCleanUp();
    }

    private void btnClipboardImport_Click(object sender, System.EventArgs e) {
        if (IsDisposed || TableView.Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) {
            return;
        }

        TableView.ImportClipboard();
    }

    private void btnFormular_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (IsDisposed || TableView.Table is not { IsDisposed: false } tb) {
            return;
        }

        using var x = new ConnectedFormulaEditor(tb.FormulaFileName(), null);

        if (x.IsClosed || x.IsDisposed) { return; }

        x.ShowDialog();
    }

    private void btnLayouts_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (IsDisposed || TableView.Table is not { IsDisposed: false } tb) { return; }

        OpenLayoutEditor(tb, string.Empty);
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) => SwitchTabToTable(e.Item.KeyName);

    private void btnMDBImport_Click(object sender, System.EventArgs e) {
        if (IsDisposed || TableView.Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) {
            return;
        }

        TableView.ImportBtb();
    }

    private void btnMonitoring_Click(object sender, System.EventArgs e) => GlobalMonitor.Start();

    private void btnNeuDB_Click(object sender, System.EventArgs e) {
        CachedFileSystem.SaveAll(false);
        Table.SaveAll(false);

        DebugPrint_NichtImplementiert(false);
        //SaveTab.ShowDialog();
        //if (!DirectoryExists(SaveTab.FileName.FilePath())) {
        //    return;
        //}

        //if (string.IsNullOrEmpty(SaveTab.FileName)) {
        //    return;
        //}

        //if (FileExists(SaveTab.FileName)) {
        //    DeleteFile(SaveTab.FileName, true);
        //}

        //var tb = new TableFile(SaveTab.FileName.FileNameWithoutSuffix());
        //tb.SaveAsAndChangeTo(SaveTab.FileName);
        //SwitchTabToTable(SaveTab.FileName);
    }

    private void btnNummerierung_CheckedChanged(object sender, System.EventArgs e) => TableView.ShowNumber = btnNummerierung.Checked;

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        CachedFileSystem.SaveAll(false);
        Table.SaveAll(false);
        LoadTab.ShowDialog();
    }

    private void btnPowerBearbeitung_Click(object sender, System.EventArgs e) {
        Notification.Show("5 Minuten (fast) rechtefreies<br>Bearbeiten aktiviert.", ImageCode.Stift);
        TableView.PowerEdit = true;
    }

    private void btnSaveAs_Click(object sender, System.EventArgs e) {
        CachedFileSystem.SaveAll(false);
        Table.SaveAll(false);

        if (TableView.Table is TableFile { IsDisposed: false } tbf) {
            if (!string.IsNullOrEmpty(tbf.IsGenericEditable(false))) { return; }

            SaveTab.ShowDialog();
            if (!DirectoryExists(SaveTab.FileName.FilePath())) { return; }

            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

            if (FileExists(SaveTab.FileName)) {
                DeleteFile(SaveTab.FileName, true);
            }

            DebugPrint_NichtImplementiert(false);
            //tbf.SaveAsAndChangeTo(SaveTab.FileName);
            //SwitchTabToTable(SaveTab.FileName);
        }
    }

    private void btnSaveLoad_Click(object sender, System.EventArgs e) {
        CachedFileSystem.SaveAll(true);
        Table.SaveAll(true);
        Table.BeSureToBeUpToDate(Table.AllFiles);
    }

    private void btnSpaltenanordnung_Click(object sender, System.EventArgs e) {
        if (IsDisposed || TableView.Table is not { IsDisposed: false } tb) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);
        tcvc.GetByKey(cbxColumnArr.Text)?.Edit();
        BlueControls.Controls.TableView.RepairColumnArrangements(tb);
    }

    private void btnSpaltenUebersicht_Click(object sender, System.EventArgs e) => TableView.Table?.Column.GenerateOverView();

    private void btnSuchenUndErsetzen_Click(object sender, System.EventArgs e) => TableView.OpenSearchAndReplaceInCells();

    private void btnSuchFenster_Click(object sender, System.EventArgs e) => TableView.OpenSearchInCells();

    private void btnSuchInScript_Click(object sender, System.EventArgs e) => TableView.OpenSearchAndReplaceInTbScripts();

    private void btnTabelleKopf_Click(object sender, System.EventArgs e) {
        if (EditableErrorMessage(TableView.Table, null)) { return; }
        InputBoxEditor.Show(TableView.Table, typeof(TableHeadEditor), false);
    }

    private void btnTabellenSpeicherort_Click(object sender, System.EventArgs e) {
        Table.SaveAll(false);
        CachedFileSystem.SaveAll(false);

        if (TableView.Table is TableFile { IsDisposed: false } tbf) {
            ExecuteFile(tbf.Filename.FilePath());
        }
    }

    private void btnTemporärenSpeicherortÖffnen_Click(object sender, System.EventArgs e) {
        Table.SaveAll(false);
        CachedFileSystem.SaveAll(false);
        ExecuteFile(System.IO.Path.GetTempPath());
    }

    private void btnUserInfo_Click(object sender, System.EventArgs e) {
        var t = new UserInfo();
        t.Show();
    }

    private void btnZeileLöschen_Click(object sender, System.EventArgs e)
        => ((IContextMenu)TableView.TableView).ExecuteContextMenuComand(BlueControls.Controls.TableView.ContextMenu_DeleteRow, null, BlueControls.Controls.TableView.ContextMenuItemGenerate(TableView.TableView, null, null, TableView.RowsVisibleUnique()));

    private void btnZoomFit_Click(object sender, System.EventArgs e) => TableView.Zoom = 1f;

    private void btnZoomIn_Click(object sender, System.EventArgs e) => TableView.DoZoom(true);

    private void btnZoomOut_Click(object sender, System.EventArgs e) => TableView.DoZoom(false);

    private void cbxColumnArr_ItemClicked(object sender, AbstractListItemEventArgs e) => TableView.Arrangement = e.Item.KeyName;

    private void CheckButtons(bool affectingHead) {
        if (IsDisposed || IsClosed) { return; }

        if (InvokeRequired) {
            try {
                Invoke(new Action(() => CheckButtons(affectingHead)));
            } catch { }
            return;
        }

        var tb = TableView.Table;
        var isEditable = false;
        var isAdmin = false;

        if (tb is { IsDisposed: false }) {
            isAdmin = tb.IsAdministrator();
            isEditable = string.IsNullOrEmpty(tb.IsGenericEditable(false));

            TableView.ShowWaitScreen = false;
            tbcTableSelector.Enabled = true;
            TableView.Enabled = true;
        } else {
            TableView.ShowWaitScreen = true;
            tbcTableSelector.Enabled = false;
            TableView.Enabled = false;
            tb = null;
        }

        if (!affectingHead) { return; }

        if (isEditable) {
            var f = new List<System.Windows.Forms.Form>(FormManager.Forms);

            foreach (var thisf in f) {
                if (thisf is IHasTable iht && iht.Table == tb && thisf is IIsEditor) {
                    isEditable = false;
                    break;
                }
            }
        }

        var combi = isEditable && isAdmin;

        cbxColumnArr.ItemEditAllowed = combi;
        MessageBoxOnError = combi;
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

    private void FormManager_FormsChanged(object? sender, FormEventArgs e) {
        if (e.Form is IHasTable iht && iht.Table == TableView?.Table) {
            CheckButtons(true);
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

        tbcSidebar.Visible = true;
        grpHilfen.Visible = true;
        grpAnsicht.Visible = true;
        SplitContainer1.IsSplitterFixed = false;
        SplitContainer1.Panel2Collapsed = false;
        SplitContainer1.SplitterDistance = Math.Max(SplitContainer1.SplitterDistance, SplitContainer1.Width / 2);

        if (TableView.Visible) {
            if (TableView.Table != null) {
                if (TableView.CursorPosRow == null && TableView.View_RowFirst() != null) {
                    TableView.CursorPos_Set(TableView.View_ColumnFirst(), TableView.View_RowFirst(), false);
                }

                if (TableView.CursorPosRow?.Row != null) {
                    FillFormula(TableView.CursorPosRow?.Row);
                }
            }
        } else {
            FillFormula(null);
        }

        CheckButtons(true);
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) {
        if (!FileExists(LoadTab.FileName)) { return; }

        SwitchTabToTable(LoadTab.FileName);
    }

    private void Tb_InvalidateView(object? sender, System.EventArgs e) => TableView.Invalidate();

    private void Tb_Loaded(object? sender, FirstEventArgs e) => CheckButtons(e.AffectingHead);

    private void tbcTableSelector_Deselecting(object sender, TabControlCancelEventArgs e) {
        var s = (List<object>)e.TabPage.Tag;
        s[1] = TableView.ViewToJson();

        e.TabPage.Tag = s;

        if (TableView.Table is { IsDisposed: false }) {
            TableView.SaveCurrentView("Letzte Ansicht");
        }
    }

    /// <summary>
    /// Diese Routine lädt die Tabelle, falls nötig.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void tbcTableSelector_Selected(object sender, TabControlEventArgs e) => ShowTab(e.TabPage);

    private void UpdateScripts(Table? tb) {
        TableView.Invalidate();
        lstAufgaben.ItemClear();

        if (tb is not { IsDisposed: false } || !string.IsNullOrEmpty(tb.IsGenericEditable(false))) {
            lstAufgaben.Enabled = false;
            return;
        }

        var l = new List<ColumnItem>();
        var ok = true;
        foreach (var thisColumnItem in tb.Column) {
            if (!thisColumnItem.IsOk()) {
                void OnClickRepair(object? sender, ContextMenuEventArgs e) => ((IContextMenu)TableView.TableView).ExecuteContextMenuComand(BlueControls.Controls.TableView.ContextMenu_EditColumnProperties, thisColumnItem, BlueControls.Controls.TableView.ContextMenuItemGenerate(TableView.TableView, thisColumnItem, null, null));

                lstAufgaben.ItemAdd(ItemOf($"Spalte '{thisColumnItem.KeyName}' reparieren", thisColumnItem.KeyName, QuickImage.Get(ImageCode.Kritisch, 16), OnClickRepair, tb.IsAdministrator(), thisColumnItem.ErrorReason()));
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
                void OnClickFirst(object? sender, ContextMenuEventArgs e) => ((IContextMenu)TableView.TableView).ExecuteContextMenuComand(BlueControls.Controls.TableView.ContextMenu_EditColumnProperties, thisColumnItem, BlueControls.Controls.TableView.ContextMenuItemGenerate(TableView.TableView, thisColumnItem, null, null));
                lstAufgaben.ItemAdd(ItemOf($"Spalte '{thisColumnItem.KeyName}' ist die erste Spalte", thisColumnItem.KeyName, QuickImage.Get(ImageCode.Kritisch, 16), OnClickFirst, tb.IsAdministrator(), "Doppelt vorhanden!"));
            }

            lstAufgaben.Enabled = true;
            return;
        }

        var addedit = true;
        if (!string.IsNullOrEmpty(tb.CheckScriptError())) {
            lstAufgaben.ItemAdd(ItemOf("Skripte reparieren", ImageCode.Kritisch, ContextMenu_OpenScriptEditor, tb.IsAdministrator()));
            addedit = false;
        }

        if (!tb.IsRowScriptPossible()) {
            lstAufgaben.ItemAdd(ItemOf("Zeilen-Skripte erlauben", ImageCode.Spalte, ContextMenu_EnableRowScript, tb.IsAdministrator()));
        }

        void OnClickValidation(object? sender, ContextMenuEventArgs e) => ((IContextMenu)TableView.TableView).ExecuteContextMenuComand(BlueControls.Controls.TableView.ContextMenu_DataValidation, null, BlueControls.Controls.TableView.ContextMenuItemGenerate(TableView.TableView, null, null, TableView.TableView.RowsVisibleUnique()));

        lstAufgaben.ItemAdd(ItemOf("Komplette Datenüberprüfung", QuickImage.Get(ImageCode.HäkchenDoppelt, 16), OnClickValidation, tb.CanDoValueChangedScript(true), string.Empty));

        foreach (var script in tb.EventScript.Where(s => s.UserGroups.Count > 0)) {
            void OnScriptClick(object? sender, ContextMenuEventArgs e) => ((IContextMenu)TableView.TableView).ExecuteContextMenuComand(BlueControls.Controls.TableView.ContextMenu_ExecuteScript, script, BlueControls.Controls.TableView.ContextMenuItemGenerate(TableView.TableView, null, null, TableView.TableView.RowsVisibleUnique()));
            lstAufgaben.ItemAdd(ItemOf(script.ReadableText(), script.SymbolForReadableText(), OnScriptClick, tb.PermissionCheck(script.UserGroups, null) && script.IsOk() && (!script.NeedRow || tb.IsRowScriptPossible()), script.QuickInfo));
        }

        if (addedit) {
            lstAufgaben.ItemAdd(ItemOf("Skripte bearbeiten", ImageCode.Skript, ContextMenu_OpenScriptEditor, tb.IsAdministrator()));
        }

        lstAufgaben.Enabled = lstAufgaben.ItemCount > 0;
    }

    #endregion
}