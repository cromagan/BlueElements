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

using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.BlueTableDialogs;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueScript.Variables;
using BlueTable.AdditionalScriptVariables;
using BlueTable.Classes;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedFormulaView : GenericControlReciverSender, IHasFieldVariable {

    #region Fields

    [ThreadStatic]
    private static int _createControlDepth;

    private bool _generated;
    private bool _generating;
    private RowItem? _lastRow;
    private System.Threading.Timer? _updater;

    #endregion

    #region Constructors

    public ConnectedFormulaView() : this(string.Empty, null) {
    }

    public ConnectedFormulaView(string mode, ItemCollectionPadItem? page) : base(false, false, false) {
        InitializeComponent();
        SetNotFocusable();

        _updater = new System.Threading.Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(Updater_Tick)); }
        }, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

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

    public string FieldName => "Field_EntryRow";

    [DefaultValue(GroupBoxStyle.Normal)]
    public GroupBoxStyle GroupBoxStyle {
        get;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = GroupBoxStyle.Normal;

    public override string Mode {
        get => base.Mode;
        set {
            if (base.Mode == value) { return; }
            base.Mode = value;
            InvalidateView();
        }
    }

    public ItemCollectionPadItem? Page {
        get;
        set {
            if (value == field) { return; }

            if (field != null) {
                //_page.Loaded -= _cf_Loaded;
                field.PropertyChanged -= _page_PropertyChanged;
                field.ItemAdded -= _page_ItemAdded;
                field.ItemRemoved -= _page_ItemRemoved;

                if (field.GetRowEntryItem()?.TableOutput is { IsDisposed: false } tb) {
                    tb.DisposingEvent -= _table_Disposing;
                }
            }
            field = value;

            if (field != null) {
                //_page.Loaded += _cf_Loaded;
                field.PropertyChanged += _page_PropertyChanged;
                field.ItemAdded += _page_ItemAdded;
                field.ItemRemoved += _page_ItemRemoved;
                if (field.GetRowEntryItem()?.TableOutput is { IsDisposed: false } tb) {
                    tb.DisposingEvent += _table_Disposing;
                }
            }

            _updater?.Change(2000, 2000);

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
        if (Page == null || Width < 30 || Height < 10) {
            _generated = true;
            return;
        }

        if (Page.GetConnectedFormula()?.IsEditing() ?? true) { return; }

        if (_generating) { return; }
        _generating = true;

        SuspendLayout();

        try {

            #region Zuerst alle Controls als unused markieren

            var unused = new List<Control>();
            foreach (var thisco in base.Controls) {
                if (thisco is Control c) {
                    unused.Add(c);
                }
            }

            unused.Remove(btnScript);

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

            var l = ItemCollectionPadItem.ResizeControls(Page, Width - x1 - x2, Height - y1 - y2, Mode);

            var posLookup = new Dictionary<IAutosizable, RectangleF>(l.Count);
            foreach (var (item, pos) in l) { posLookup[item] = pos; }

            var autoc = new List<FlexiControlForCell>();

            foreach (var thisit in Page) {
                if (thisit is IItemToControl thisitco) {
                    var con = SearchOrGenerate(thisitco, Mode);

                    if (con != null) {
                        unused.Remove(con);

                        con.Visible = thisit is not ReciverControlPadItem cspi || cspi.IsVisibleForMe(Mode, true);

                        if (thisit is IAutosizable autoItem && posLookup.TryGetValue(autoItem, out var newpos)) {
                            con.Left = (int)newpos.Left + x1;
                            con.Top = (int)newpos.Top + y1;
                            con.Width = (int)newpos.Width;
                            con.Height = (int)newpos.Height;
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

            Invalidate_RowsInput();
        } finally {
            _generating = false;
            ResumeLayout(true);
        }
    }

    public Variable? GetFieldVariable() {
        var fn = FieldName;

        if (!string.IsNullOrEmpty(fn)) {
            return new VariableRowItem(fn, _lastRow, true, "Die Eingangszeile des Formulares");
        }

        return null;
    }

    public void GetHeadPageFrom(Table? table) {
        if (table is { IsDisposed: false }) {
            var filename = table.FormulaFileName();

            if (filename != null) {
                var tmpFormula = CachedFileSystem.Get<ConnectedFormula.ConnectedFormula>(filename);
                if (tmpFormula is { IsDisposed: false }) {
                    Page = tmpFormula.GetPage("Head");
                }
                return;
            }
        }

        Page = null;
    }

    public override void Invalidate_FilterInput() {
        base.Invalidate_FilterInput();
        HandleChangesNow();
    }

    public void InvalidateView() {
        if (IsDisposed) { return; }
        _generated = false;
        Invalidate(); // Sonst wird es nie neu gezeichnet
    }

    public Control? SearchOrGenerate(IItemToControl? thisit, string mode) {
        if (thisit == null) { return null; }

        try {
            foreach (var thisC in base.Controls) {
                if (thisC is Control { Name: { } sx } cx && sx == thisit.DefaultItemToControlName(Page?.UniqueId) && !cx.IsDisposed) { return cx; }
            }

            if (_createControlDepth > 10) { return null; }

            _createControlDepth++;
            Control? c;
            try {
                c = thisit.CreateControl(this, mode);
            } finally {
                _createControlDepth--;
            }
            if (c is not { Name: { } s } || s != thisit.DefaultItemToControlName(Page?.UniqueId)) {
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

    public void SetValueFromVariable(Variable v) {
    }

    internal ConnectedFormula.ConnectedFormula? GetConnectedFormula() => Page?.GetConnectedFormula();

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            Page = null;
            components?.Dispose();
            _updater?.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }

        var s = States.Standard;

        if (!Enabled) { s = States.Standard_Disabled; }
        GroupBox.DrawGroupBox(this, gr, s, GroupBoxStyle, Text);
        GenerateView();

        if (!_generated) {
            _updater?.Change(2000, 2000);
            return;
        }

        base.DrawControl(gr, state);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();

        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        SuspendLayout();
        BeginUpdate();

        try {
            DoInputFilter(FilterOutput.Table, false);
            RowsInputChangedHandled = true;

            _lastRow = null;

            if (RowSingleOrNull() is { IsDisposed: false } r) {
                if (Page?.GetRowEntryItem()?.TableOutput == r.Table) {
                    _lastRow = r;
                    using var nfc = new FilterCollection(r, "ConnectedFormulaView");

                    FilterOutput.ChangeTo(nfc);

                    btnScript.Visible = r.Table is { IsDisposed: false } tb && tb.IsAdministrator() && string.IsNullOrEmpty(tb.IsGenericEditable(false)) && !string.IsNullOrEmpty(tb.CheckScriptError());

                    if (btnScript.Visible) { btnScript.BringToFront(); }
                } else {
                    FilterOutput.ChangeTo(new FilterItem(FilterOutput.Table, FilterType.AlwaysFalse, string.Empty));
                }
            } else {
                FilterOutput.Clear();
            }
        } finally {
            EndUpdate();
            ResumeLayout(true);
        }

        Invalidate();
        InvalidateAllChilds();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        base.OnSizeChanged(e);

        if (_generated) {
            InvalidateView();
            _updater?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        if (Visible && !IsDisposed) {
            BeginUpdate();
            try {
                base.OnVisibleChanged(e);
            } finally {
                EndUpdate();
            }
        } else {
            base.OnVisibleChanged(e);
        }
    }

    private void _page_ItemAdded(object? sender, System.EventArgs e) => InvalidateView();

    private void _page_ItemRemoved(object? sender, System.EventArgs e) => InvalidateView();

    private void _page_PropertyChanged(object? sender, PropertyChangedEventArgs e) => InvalidateView();

    private void _table_Disposing(object? sender, System.EventArgs e) => Page = null;

    private void btnSkript_Click(object sender, System.EventArgs e) {
        if (Generic.IsAdministrator()) {
            if (IsDisposed || RowSingleOrNull()?.Table is not { IsDisposed: false } tb) { return; }

            if (TableViewForm.EditableErrorMessage(tb, null)) { return; }

            IUniqueWindowExtension.ShowOrCreate<TableScriptEditor>(tb);
        } else {
            Forms.MessageBox.Show("Die Skripte sind fehlerhaft.\r\nVerständigen sie einen Administrator", ImageCode.Kritisch, "Ok");
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
                var s1 = Caption.RequiredTextSize(thisIt.Caption, Design.Caption, false, -1);
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

    private void InvalidateAllChilds() {
        foreach (Control c in Controls) {
            if (c is GenericControl gc && !gc.IsDisposed && c != btnScript) {
                gc.Invalidate();
            }
        }
    }

    private void Updater_Tick() {
        _updater?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        if (!_generated) { Invalidate(); }
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ConnectedFormulaView()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}