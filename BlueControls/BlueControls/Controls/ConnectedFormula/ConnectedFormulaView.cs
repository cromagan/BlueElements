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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
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

#nullable enable

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedFormulaView : GenericControlReciver, IBackgroundNone, IControlSendFilter, IDisposable {

    #region Fields

    private bool _generated;

    private GroupBoxStyle _groupBoxStyle = GroupBoxStyle.Normal;
    private string _modus = string.Empty;

    private Timer _timer;

    #endregion

    #region Constructors

    public ConnectedFormulaView() : this("Head", string.Empty) { }

    public ConnectedFormulaView(string page, string mode) : base(false, false) {
        InitializeComponent();
        Page = page;
        Mode = mode;
        InitFormula(null, null);
        ((IControlSendFilter)this).RegisterEvents();
        base.RegisterEvents();

        _timer = new Timer();
        _timer.Enabled = true;
        _timer.Interval = 1000;
        _timer.Tick += Checker_Tick;
    }

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<GenericControlReciver> Childs { get; } = [];

    public ConnectedFormula.ConnectedFormula? ConnectedFormula { get; private set; }

    public new ControlCollection Controls {
        get {
            GenerateView();
            return base.Controls;
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput 03");

    [DefaultValue(GroupBoxStyle.Normal)]
    public GroupBoxStyle GroupBoxStyle {
        get => _groupBoxStyle;
        set {
            if (_groupBoxStyle == value) { return; }
            _groupBoxStyle = value;
            Invalidate();
        }
    }

    public string Mode {
        get => _modus;
        set {
            if (_modus == value) { return; }
            _modus = value;
            InvalidateView();
        }
    }

    public string Page { get; }

    public RowItem? ShowingRow => this.RowSingleOrNull();

    #endregion

    #region Methods

    #region Methods

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void FilterOutput_PropertyChanged(object sender, System.EventArgs e) => this.FilterOutput_PropertyChanged();

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

        if (ConnectedFormula?.PadData != null) {
            var l = ResizeControls(ConnectedFormula.PadData, Width - x1 - x2, Height - y1 - y2, Page, Mode);
            var autoc = new List<FlexiControlForCell>();

            foreach (var thisit in ConnectedFormula.PadData) {
                if (thisit is IItemToControl thisitco && thisit.IsVisibleOnPage(Page)) {
                    var o = SearchOrGenerate(thisitco);

                    if (o != null) {
                        _ = unused.Remove(o);

                        if (thisit is FakeControlPadItem cspi) {
                            o.Visible = cspi.IsVisibleForMe(Mode, true);
                        } else {
                            o.Visible = true;
                        }

                        if (thisit is IAutosizable) {
                            foreach (var (item, newpos) in l) {
                                if (item == thisit) {
                                    o.Left = (int)newpos.Left + x1;
                                    o.Top = (int)newpos.Top + y1;
                                    o.Width = (int)newpos.Width;
                                    o.Height = (int)newpos.Height;
                                }
                            }
                        }

                        if (thisit is TabFormulaPadItem c3) {
                            c3.CreateTabs((TabControl)o, this, Mode);
                        }

                        if (o.Visible && o is FlexiControlForCell fo &&
                           thisit is EditFieldPadItem efpi && efpi.AutoX &&
                           efpi.CaptionPosition is CaptionPosition.Links_neben_dem_Feld or
                                                   CaptionPosition.Links_neben_dem_Feld_unsichtbar) { autoc.Add(fo); }
                    }
                }
            }

            DoAutoX(autoc);

            _generated = true;
        }

        foreach (var thisc in unused) {
            if (thisc is GenericControlReciver child) {
                child.DisconnectChildParents(child.Parents);
            }

            base.Controls.Remove(thisc);
            thisc?.Dispose();
        }

        if (_generated) {
            RowsInput_Changed();
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

    public override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        if (!FilterInputChangedHandled) {
            FilterInputChangedHandled = true;
            this.DoInputFilter(FilterOutput.Database, false);
        }

        RowsInputChangedHandled = true;
        this.DoRows();

        if (this.RowSingleOrNull() is RowItem r) {
            FilterOutput.ChangeTo(new FilterItem(r));
        } else {
            FilterOutput.ChangeTo(new FilterItem(FilterOutput.Database, FilterType.AlwaysFalse, string.Empty));
        }
    }

    public void InitFormula(ConnectedFormula.ConnectedFormula? cf, Database? database) {
        if (IsDisposed) { return; }

        var oldf = ConnectedFormula; // Zwischenspeichern wegen möglichen NULL verweisen

        if (oldf == cf && FilterOutput.Database == database) { return; }

        SuspendLayout();

        this.Invalidate_FilterOutput();
        this.Invalidate_RowsInput();

        if (oldf != cf) {
            if (oldf != null) {
                oldf.Loaded -= _cf_Loaded;
                oldf.PropertyChanged -= _cf_PropertyChanged;
            }

            InvalidateView();
            ConnectedFormula = cf;

            if (cf != null) {
                cf.Loaded += _cf_Loaded;
                cf.PropertyChanged += _cf_PropertyChanged;
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
        if (IsDisposed) { return; }
        _generated = false;
        Invalidate(); // Sonst wird es nie neu gezeichnet
    }

    public Control? SearchOrGenerate(IItemToControl? thisit) {
        if (thisit == null) { return null; }

        try {
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
        } catch {
            // Beim Beenden- Fehler beim Fenster-Handle erstellen
            return null;
        }
    }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {

        if (disposing) {

            InitFormula(null, null);
            ((IControlSendFilter)this).DoDispose();
            components?.Dispose();
            _timer.Enabled = false;
            _timer.Tick -= Checker_Tick;
            _timer.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        GroupBox.DrawGroupBox(this, gr, state, _groupBoxStyle, Text);
        base.DrawControl(gr, state);
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);

        if (e.Control is RowEntryControl rec) {
            rec.ConnectChildParents(this);
        }
    }

    protected override void OnPaint(PaintEventArgs e) {
        if (IsDisposed) { return; }

        base.OnPaint(e);
        GenerateView();

        if (!_generated) {
            _timer.Enabled = true;
            return;
        }

        HandleChangesNow();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        base.OnSizeChanged(e);

        if (_generated) {
            InvalidateView();
            _timer.Enabled = false;
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

    private void _cf_Loaded(object sender, System.EventArgs e) => InvalidateView();

    private void _cf_PropertyChanged(object sender, System.EventArgs e) => InvalidateView();

    private void _database_Disposing(object sender, System.EventArgs e) => InitFormula(null, null);

    private void Checker_Tick(object sender, System.EventArgs e) {
        if (!_generated) {
            Invalidate();
            _timer.Stop();
        }
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

    #endregion

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ConnectedFormulaView()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}