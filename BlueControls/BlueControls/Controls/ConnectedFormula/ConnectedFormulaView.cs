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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueControls.ConnectedFormula.ConnectedFormula;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedFormulaView : GenericControlReciverSender {

    #region Fields

    private bool _generated;
    private GroupBoxStyle _groupBoxStyle = GroupBoxStyle.Normal;
    private ItemCollectionPadItem? _page;

    #endregion

    #region Constructors

    public ConnectedFormulaView() : this(string.Empty, null) { }

    public ConnectedFormulaView(string mode, ItemCollectionPadItem? page) : base(false, false, false) {
        InitializeComponent();
        SetNotFocusable();

        base.Mode = mode;
        Page = page;
    }

    #endregion

    #region Properties

    public new ControlCollection Controls {
        get {
            GenerateView();
            return base.Controls;
        }
    }

    //        InvalidateView();
    //        Invalidate_FilterOutput();
    //        Invalidate_RowsInput();
    //    }
    //}
    [DefaultValue(GroupBoxStyle.Normal)]
    public GroupBoxStyle GroupBoxStyle {
        get => _groupBoxStyle;
        set {
            if (_groupBoxStyle == value) { return; }
            _groupBoxStyle = value;
            Invalidate();
        }
    }

    public override string Mode {
        get => base.Mode;
        set {
            if (base.Mode == value) { return; }
            base.Mode = value;
            InvalidateView();
        }
    }

    public ItemCollectionPadItem? Page {
        get => _page;
        set {
            if (value == _page) { return; }

            if (_page != null) {
                //_page.Loaded -= _cf_Loaded;
                _page.PropertyChanged -= _page_PropertyChanged;
                _page.ItemAdded -= _page_ItemAdded;
                _page.ItemRemoved -= _page_ItemRemoved;

                if (_page.GetRowEntryItem()?.DatabaseOutput is { IsDisposed: false } db1) {
                    db1.DisposingEvent -= _database_Disposing;
                }
            }
            _page = value;

            if (_page != null) {
                //_page.Loaded += _cf_Loaded;
                _page.PropertyChanged += _page_PropertyChanged;
                _page.ItemAdded += _page_ItemAdded;
                _page.ItemRemoved += _page_ItemRemoved;
                if (_page.GetRowEntryItem()?.DatabaseOutput is { IsDisposed: false } db1) {
                    db1.DisposingEvent += _database_Disposing;
                }
            }

            //if (FilterOutput.Database != database) {
            //    if (FilterOutput.Database is { IsDisposed: false } db1) {
            //        db1.DisposingEvent -= _database_Disposing;
            //    }

            //    FilterOutput.Database = database;

            //    if (FilterOutput.Database is { IsDisposed: false } db2) {
            //        db2.DisposingEvent += _database_Disposing;
            //    }
            //}

            //InitFormula(null, null);

            updater.Enabled = true;

            InvalidateView();
            Invalidate_FilterOutput();
            Invalidate_RowsInput();
        }
    }

    //        _connectedFormula = value;
    public RowItem? ShowingRow {
        // Used: Only BZL
        get {
            HandleChangesNow();
            return RowSingleOrNull();
        }
    }

    #endregion

    #region Methods

    public void GenerateView() {
        if (IsDisposed) { return; }
        if (_generated) { return; }
        if (!Visible) { return; }
        if (_page == null || Width < 30 || Height < 10) {
            _generated = true;
            return;
        }

        if (_page.GetConnectedFormula()?.IsEditing() ?? true) { return; }

        #region Zuerst alle Controls als unused markieren

        var unused = new List<Control>();
        foreach (var thisco in base.Controls) {
            if (thisco is Control c) {
                unused.Add(c);
            }
        }

        _ = unused.Remove(btnScript);

        #endregion

        var x1 = 0;
        var x2 = 0;
        var y1 = 0;
        var y2 = 0;

        if (GroupBoxStyle != GroupBoxStyle.Nothing) {
            x1 = Skin.Padding;
            x2 = Skin.Padding;
            y1 = Skin.Padding * 3;
            y2 = Skin.Padding;
        }

        var l = ItemCollectionPadItem.ResizeControls(_page, Width - x1 - x2, Height - y1 - y2, Mode);
        var autoc = new List<FlexiControlForCell>();

        foreach (var thisit in _page) {
            if (thisit is IItemToControl thisitco) {
                var con = SearchOrGenerate(thisitco, false, Mode);

                if (con != null) {
                    _ = unused.Remove(con);

                    con.Visible = thisit is not ReciverControlPadItem cspi || cspi.IsVisibleForMe(Mode, true);

                    if (thisit is IAutosizable) {
                        foreach (var (item, newpos) in l) {
                            if (item == thisit) {
                                con.Left = (int)newpos.Left + x1;
                                con.Top = (int)newpos.Top + y1;
                                con.Width = (int)newpos.Width;
                                con.Height = (int)newpos.Height;
                            }
                        }
                    }

                    if (thisit is RowEntryPadItem rep) {
                        DoDefaultSettings(null, rep, Mode);
                    }

                    if (thisit is TabFormulaPadItem tabItem) {
                        tabItem.CreateTabs((TabControl)con, this, Mode);
                    }

                    if (con.Visible && con is FlexiControlForCell fo &&
                        thisit is EditFieldPadItem {
                            AutoX: true, CaptionPosition: CaptionPosition.Links_neben_dem_Feld or
                            CaptionPosition.Links_neben_dem_Feld_unsichtbar
                        }) { autoc.Add(fo); }
                }
            }
        }

        DoAutoX(autoc);

        _generated = true;

        foreach (var thisc in unused) {
            base.Controls.Remove(thisc);
            thisc?.Dispose();
        }

        if (_generated) {
            Invalidate_RowsInput();
        }
    }

    //public ConnectedFormula.ConnectedFormula? ConnectedFormula {
    //    get => _connectedFormula;
    //    private set {
    //        if (value == _connectedFormula) { return; }
    public void GetHeadPageFrom(Database? database) {
        if (database is { IsDisposed: false }) {
            var f = database.FormulaFileName();

            if (f != null) {
                var tmpFormula = GetByFilename(f);
                if (tmpFormula is { IsDisposed: false }) {
                    Page = tmpFormula.GetPage("Head");
                }
                return;
            }
        }

        Page = null;
    }

    public void InvalidateView() {
        if (IsDisposed) { return; }
        _generated = false;
        Invalidate(); // Sonst wird es nie neu gezeichnet
    }

    //    InvalidateView();
    //    ResumeLayout();
    //    Invalidate();
    //}
    public Control? SearchOrGenerate(IItemToControl? thisit, bool onlySerach, string mode) {
        if (thisit == null) { return null; }

        try {
            foreach (var thisC in base.Controls) {
                if (thisC is Control { Name: { } sx } cx && sx == thisit.DefaultItemToControlName(Page?.KeyName) && !cx.IsDisposed) { return cx; }
            }

            if (onlySerach) { return null; }

            var c = thisit.CreateControl(this, mode);
            if (c is not { Name: { } s } || s != thisit.DefaultItemToControlName(Page?.KeyName)) {
                Develop.DebugPrint("Name muss intern mit Internal-Version beschrieben werden!");
                return null;
            }

            if (c is GenericControlReciver gci && gci.GeneratedFrom != thisit) {
                Develop.DebugPrint("Item muss gesetzt werden!");
                return null;
            }

            base.Controls.Add(c);
            return c;
        } catch {
            // Beim Beenden- Fehler beim Fenster-Handle erstellen
            return null;
        }
    }

    internal ConnectedFormula.ConnectedFormula? GetConnectedFormula() => Page?.GetConnectedFormula();

    //        if (FilterOutput.Database is { IsDisposed: false } db2) {
    //            db2.DisposingEvent += _database_Disposing;
    //        }
    //    }
    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            Page = null;
            components?.Dispose();
            updater.Enabled = false;
            updater.Tick -= updater_Tick;
            updater.Dispose();
        }
        base.Dispose(disposing);
    }

    //        FilterOutput.Database = database;
    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        var s = States.Standard;

        if (!Enabled) { s = States.Standard_Disabled; }
        GroupBox.DrawGroupBox(this, gr, s, _groupBoxStyle, Text);
        GenerateView();

        if (!_generated) {
            updater.Enabled = true;
            return;
        }

        base.DrawControl(gr, state);
    }

    //    if (FilterOutput.Database != database) {
    //        if (FilterOutput.Database is { IsDisposed: false } db1) {
    //            db1.DisposingEvent -= _database_Disposing;
    //        }
    protected override void HandleChangesNow() {
        base.HandleChangesNow();

        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(FilterOutput.Database, false);
        DoRows();

        if (RowSingleOrNull() is { IsDisposed: false } r) {
            using var nfc = new FilterCollection(r, "ConnectedFormulaView");

            FilterOutput.ChangeTo(nfc);

            btnScript.Visible = r.Database is { IsDisposed: false } db && db.IsAdministrator() && !string.IsNullOrEmpty(db.CheckScriptError());

            if (btnScript.Visible) { btnScript.BringToFront(); }
        } else {
            FilterOutput.ChangeTo(new FilterItem(FilterOutput.Database, FilterType.AlwaysFalse, string.Empty));
        }
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        base.OnSizeChanged(e);

        if (_generated) {
            InvalidateView();
            updater.Enabled = false;
        }
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Page = null;

    private void _page_ItemAdded(object sender, System.EventArgs e) => InvalidateView();

    private void _page_ItemRemoved(object sender, System.EventArgs e) => InvalidateView();

    //    public void InitFormula(ItemCollectionPadItem? page, Database? database) {
    //    if (IsDisposed) { return; }

    //    if (page == _page && FilterOutput.Database == database) { return; }

    //    SuspendLayout();

    //    Invalidate_FilterOutput();
    //    Invalidate_RowsInput();
    private void _page_PropertyChanged(object sender, System.EventArgs e) => InvalidateView();

    private void btnSkript_Click(object sender, System.EventArgs e) {
        if (Generic.IsAdministrator()) {
            if (IsDisposed || RowSingleOrNull()?.Database is not { IsDisposed: false } db) { return; }

            _ = IUniqueWindowExtension.ShowOrCreate<DatabaseScriptEditor>(db);
        } else {
            MessageBox.Show("Die Skripte sind fehlerhaft.\r\nVerständigen sie einen Administrator", ImageCode.Kritisch, "Ok");
        }
        Invalidate_FilterInput();
    }

    private void DoAutoX(List<FlexiControlForCell> autoc) {
        if (autoc.Count == 0) { return; }

        var undone = new List<FlexiControlForCell>();
        var dohere = new List<FlexiControlForCell>();

        var left = autoc[0];

        foreach (var thisIt in autoc) {
            if (thisIt.Left < left.Left) { left = thisIt; }
        }

        var cx = -1;
        foreach (var thisIt in autoc) {
            if (thisIt.Left == left.Left && thisIt.Width == left.Width) {
                var s1 = Caption.RequiredTextSize(thisIt.Caption, SteuerelementVerhalten.Text_Abschneiden, Design.Caption, null, false, -1);
                cx = Math.Max(cx, s1.Width + 1);
                dohere.Add(thisIt);
            } else {
                undone.Add(thisIt);
            }
        }

        foreach (var thisIt in dohere) {
            thisIt.ControlX = cx;
        }

        if (undone.Count > 0) { DoAutoX(undone); }
    }

    private void updater_Tick(object sender, System.EventArgs e) {
        if (!_generated) {
            Invalidate();
            updater.Stop();
        }
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ConnectedFormulaView()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}