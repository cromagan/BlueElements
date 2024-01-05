﻿// Authors:
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
public partial class ConnectedFormulaView : GenericControl, IBackgroundNone, IControlAcceptSomething, IControlSendSomething {

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

    public List<IControlAcceptSomething> Childs { get; } = [];

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
    public List<IControlSendSomething> Parents { get; } = [];

    //set => SetData(ConnectedFormula, Database, value, Page);
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowItem? ShowingRow {
        get {
            if (IsDisposed) { return null; }
            return FilterOutput.RowSingleOrNull;
        }
    }

    #endregion

    #region Methods

    public void FilterInput_Changed(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }

        this.DoInputFilter();
        Invalidate();

        if (FilterInput == null || FilterOutput.Database != FilterInput.Database) {
            FilterOutput.Clear();
            return;
        }

        FilterOutput.ChangeTo(FilterInput);
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    public void GenerateView() {
        if (IsDisposed) { return; }
        if (_generated) { return; }
        if (!Visible) { return; }
        if (ConnectedFormula == null || ConnectedFormula.IsEditing()) { return; }

        if (Width < 30 || Height < 10) { return; }

        var unused = new List<Control>();
        foreach (var thisco in base.Controls) {
            if (thisco is Control c) {
                unused.Add(c);
            }
        }

        if (ConnectedFormula?.PadData != null) {
            var l = ResizeControls(ConnectedFormula.PadData, Width, Height, Page);

            //var addfactor = Size.Width / ConnectedFormula.PadData.SheetSizeInPix.Width;

            foreach (var thisit in ConnectedFormula.PadData) {
                if (thisit.IsVisibleOnPage(Page) && thisit is IItemToControl thisitco) {
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
                    }
                }
            }
            _generated = true;
        }

        foreach (var thisc in unused) {
            if (thisc is IControlAcceptSomething child) {
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
            if (FilterOutput.Database is Database db1) {
                db1.DisposingEvent -= _Database_DisposingEvent;
            }
            InvalidateView();
            FilterOutput.Database = database;

            if (FilterOutput.Database is Database db2) {
                db2.DisposingEvent += _Database_DisposingEvent;
            }
        }

        ResumeLayout();
        Invalidate();
    }

    public void InvalidateView() {
        _generated = false;
        Invalidate(); // Sonst wird es nie neu gezeichnet
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
            FilterInput?.Dispose();
            FilterOutput.Dispose();
            FilterInput = null;
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
        GenerateView();
    }

    private void _cf_Changed(object sender, System.EventArgs e) => InvalidateView();

    private void _cf_Loaded(object sender, System.EventArgs e) => InvalidateView();

    private void _Database_DisposingEvent(object sender, System.EventArgs e) => InitFormula(null, null);

    #endregion
}