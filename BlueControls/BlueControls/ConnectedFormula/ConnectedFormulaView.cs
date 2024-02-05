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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using static BlueControls.ConnectedFormula.ConnectedFormula;

#nullable enable

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedFormulaView : GenericControl, IBackgroundNone, IControlAcceptFilter, IControlSendFilter {

    #region Fields

    private bool _generated;

    #endregion

    #region Constructors

    public ConnectedFormulaView() : this("Head") { }

    public ConnectedFormulaView(string page) {
        InitializeComponent();
        Page = page;
        InitFormula(null, null);
    }

    #endregion

    #region Properties

    public List<IControlAcceptFilter> Childs { get; } = [];

    public ConnectedFormula.ConnectedFormula? ConnectedFormula {
        get; private set;
        //set => SetData(value, Database, RowKey, Page);
    }

    public new ControlCollection Controls {
        get {
            GenerateView();
            return base.Controls;
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool FilterManualSeted { get; set; } = false;

    public FilterCollection FilterOutput { get; } = new("FilterOutput");

    public string Page { get; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendFilter> Parents { get; } = [];

    //set => SetData(ConnectedFormula, Database, value, Page);
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowItem? ShowingRow {
        get {
            if (IsDisposed) { return null; }
            FilterOutput.ChangeTo(FilterInput);
            return FilterOutput.RowSingleOrNull;
        }
    }

    #endregion

    #region Methods

    public void FilterInput_Changed(object? sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        this.Invalidate_FilterInput(true);
        Invalidate();
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    public void FilterInput_RowChanged(object? sender, System.EventArgs e) { }

    public void GenerateView() {
        if (IsDisposed) { return; }
        if (_generated) { return; }
        if (!Visible) { return; }
        if (ConnectedFormula == null || ConnectedFormula.IsEditing()) { return; }

        if (Width < 30 || Height < 10) { return; }

        #region Zuerst alle Controls als unused markieren

        var unused = new List<Control>();
        foreach (var thisco in base.Controls) {
            if (thisco is Control c) {
                unused.Add(c);
            }
        }

        #endregion

        if (ConnectedFormula?.PadData != null) {
            var l = ResizeControls(ConnectedFormula.PadData, Width, Height, Page);
            var autoc = new List<FlexiControlForCell>();

            foreach (var thisit in ConnectedFormula.PadData) {
                if (thisit is IItemToControl thisitco && thisit.IsVisibleOnPage(Page)) {
                    var o = SearchOrGenerate(thisitco);

                    if (o != null) {
                        _ = unused.Remove(o);

                        if (thisit is FakeControlPadItem cspi) {
                            o.Visible = cspi.IsVisibleForMe();
                        } else {
                            o.Visible = true;
                        }

                        if (thisit is IAutosizable) {
                            foreach (var (item, newpos) in l) {
                                if (item == thisit) {
                                    o.Left = (int)newpos.Left;
                                    o.Top = (int)newpos.Top;
                                    o.Width = (int)newpos.Width;
                                    o.Height = (int)newpos.Height;
                                }
                            }
                        }

                        if (thisit is TabFormulaPadItem c3) {
                            c3.CreateTabs((TabControl)o, this);
                        }

                        if (o.Visible && o is FlexiControlForCell fo &&
                           thisit is EditFieldPadItem efpi && efpi.AutoX &&
                           efpi.CaptionPosition is BlueDatabase.Enums.CaptionPosition.Links_neben_dem_Feld or
                                                   BlueDatabase.Enums.CaptionPosition.Links_neben_dem_Feld_unsichtbar) { autoc.Add(fo); }
                    }
                }
            }

            DoAutoX(autoc);

            _generated = true;
        }

        foreach (var thisc in unused) {
            if (thisc is IControlAcceptFilter child) {
                child.DisconnectChildParents(child.Parents);
            }

            base.Controls.Remove(thisc);
            thisc?.Dispose();
        }

        if (_generated) {
            FilterInput_Changed(this, System.EventArgs.Empty);
        }
    }

    public void GetConnectedFormulaFromDatabase(Database? database) {
        if (database != null && !database.IsDisposed) {
            var f = database.FormulaFileName();

            if (f != null) {
                var tmpFormula = GetByFilename(f);
                if (tmpFormula != ConnectedFormula) {
                    InitFormula(tmpFormula, database);
                }
                return;
            }
        }

        ConnectedFormula = null;
    }

    public void InitFormula(ConnectedFormula.ConnectedFormula? cf, Database? database) {
        if (IsDisposed) { return; }

        var oldf = ConnectedFormula; // Zwischenspeichern wegen möglichen NULL verweisen

        if (oldf == cf && FilterOutput.Database == database) { return; }

        SuspendLayout();

        FilterOutput.Clear();
        this.Invalidate_FilterInput(false);

        if (oldf != cf) {
            if (oldf != null) {
                oldf.Loaded -= _cf_Loaded;
                oldf.Changed -= _cf_Changed;
            }

            InvalidateView();
            ConnectedFormula = cf;

            if (cf != null) {
                cf.Loaded += _cf_Loaded;
                cf.Changed += _cf_Changed;
            }
        }

        if (FilterOutput.Database != database) {
            if (FilterOutput.Database is Database db1 && !db1.IsDisposed) {
                db1.DisposingEvent -= _database_Disposing;
            }
            InvalidateView();
            FilterOutput.Database = database;

            if (FilterOutput.Database is Database db2 && !db2.IsDisposed) {
                db2.DisposingEvent += _database_Disposing;
            }
        }

        ResumeLayout();
        Invalidate();
    }

    public void InvalidateView() {
        _generated = false;
        Invalidate(); // Sonst wird es nie neu gezeichnet
    }

    public void Parents_Added(bool hasFilter) {
        if (IsDisposed) { return; }
        if (!hasFilter) { return; }
        FilterInput_Changed(null, System.EventArgs.Empty);
    }

    public Control? SearchOrGenerate(IItemToControl? thisit) {
        if (thisit == null) { return null; }

        foreach (var thisC in base.Controls) {
            if (thisC is Control cx && cx.Name is string sx && sx == thisit.DefaultItemToControlName() && !cx.IsDisposed) { return cx; }
        }

        var c = thisit.CreateControl(this);
        if (c == null || c.Name is not string s || s != thisit.DefaultItemToControlName()) {
            Develop.DebugPrint("Name muss intern mit Internal-Version beschrieben werden!");
            return null;
        }

        base.Controls.Add(c);
        return c;
    }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            OnDisposingEvent();
            FilterOutput.Dispose();
            this.Invalidate_FilterInput(false);
            InitFormula(null, null);
        }

        if (disposing && (components != null)) {
            InitFormula(null, null);
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
        GenerateView();

        if (FilterInput == null) {
            this.DoInputFilter(FilterOutput.Database, false);
        }

        FilterOutput.ChangeTo(FilterInput);
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);

        if (e.Control is RowEntryControl rec) {
            rec.ConnectChildParents(this);
        }
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        base.OnSizeChanged(e);

        if (_generated) {
            InvalidateView();
        }
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);

        // Ist komisch, muss aber so sein:
        // Ist die View in einer Tab-Page, eine andere TabPage angezeigt und wird ConnectedFormula
        // Invalidiert, wird beim herschalten die ConnectedFormulaVuew NICHT neu gezeichet.
        // Dadurch wird der Filter bei DrawControll dann nicht neu berechnet
        Invalidate();
    }

    private void _cf_Changed(object sender, System.EventArgs e) => InvalidateView();

    private void _cf_Loaded(object sender, System.EventArgs e) => InvalidateView();

    private void _database_Disposing(object sender, System.EventArgs e) => InitFormula(null, null);

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

    #endregion
}