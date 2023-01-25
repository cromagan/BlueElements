// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueControls.ConnectedFormula.ConnectedFormula;

#nullable enable

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedFormulaView : GenericControl, IBackgroundNone, IAcceptRowKey {

    #region Fields

    private ConnectedFormula.ConnectedFormula? _cf;
    private DatabaseAbstract? _database;
    private bool _generated;
    private string _pageToShow = "Head";
    private long _rowkey = -1;
    private RowItem? _tmpShowingRow;

    #endregion

    #region Constructors

    public ConnectedFormulaView() : this("Head") { }

    public ConnectedFormulaView(string page) {
        InitializeComponent();
        DoFormulaDatabaseAndRow(null, null, -1, page);
    }

    #endregion

    #region Properties

    public ConnectedFormula.ConnectedFormula? ConnectedFormula {
        get => _cf;
        set => DoFormulaDatabaseAndRow(value, Database, RowKey, Page);
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DatabaseAbstract? Database {
        get => _database;
        set => DoFormulaDatabaseAndRow(ConnectedFormula, value, RowKey, Page);
    }

    [DefaultValue("Head")]
    public string Page {
        get => _pageToShow;
        set => DoFormulaDatabaseAndRow(ConnectedFormula, Database, RowKey, value);
    }

    [DefaultValue(-1)]
    public long RowKey {
        get => _rowkey;
        set => DoFormulaDatabaseAndRow(ConnectedFormula, Database, value, Page);
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowItem? ShowingRow {
        get {
            Develop.DebugPrint_Disposed(IsDisposed);
            return _tmpShowingRow;
        }
    }

    #endregion

    #region Methods

    public void GetConnectedFormulaFromDatabase(DatabaseAbstract? database) {
        if (database != null) {
            var f = database.FormulaFileName();

            if (f != null) {
                var tmpFormula = GetByFilename(f);
                if (tmpFormula != null) {
                    ConnectedFormula = tmpFormula;
                    return;
                }
            }
        }

        ConnectedFormula = null;
    }

    public void InvalidateView() {
        _generated = false;
        Invalidate(); // Sonst wird es nie neu gezeichnet
    }

    public System.Windows.Forms.Control? SearchOrGenerate(IItemToControl? thisit) {
        if (thisit == null) { return null; }

        //if (thisit.Page != Page) {
        //    Develop.DebugPrint("Falscher Seitenaufruf!");
        //    return null;
        //}

        foreach (var thisC in Controls) {
            if (thisC is System.Windows.Forms.Control c) {
                if (c.Name is string s) {
                    if (s == thisit.Internal + "-" + thisit.Version) { return c; }
                }
            }
        }

        if (thisit is IItemToControl it) {
            var c = it.CreateControl(this);
            if (c != null && c.Name is string s && s == thisit.Internal + "-" + it.Version) {
                //alles ok
            } else {
                Develop.DebugPrint("Name muß intern mit Internal-Version beschrieben werden!");
                return null;
            }
            Controls.Add(c);
            return c;
        }

        Develop.DebugPrint("Typ nicht definiert.");
        return null;
    }

    protected override void DrawControl(Graphics gr, States state) {
        Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
        GenerateView();
        SetInputRow();
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        base.OnSizeChanged(e);
        if (!_generated) { return; }

        InvalidateView();
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);

        SetInputRow();
    }

    private void _cf_Changed(object sender, System.EventArgs e) {
        InvalidateView();
    }

    private void _cf_Loaded(object sender, System.EventArgs e) {
        InvalidateView();
    }

    private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;

    private void DoFormulaDatabaseAndRow(ConnectedFormula.ConnectedFormula? cf, DatabaseAbstract? database, long rowKey, string page) {
        if (_cf == cf &&
            _database == database &&
            _rowkey == rowKey &&
            _pageToShow.Equals(page, StringComparison.OrdinalIgnoreCase)) { return; }

        SuspendLayout();

        _rowkey = -1;
        _tmpShowingRow = null;
        if (_generated) { SetInputRow(); }

        _tmpShowingRow = null;

        if (_cf != cf) {
            if (_cf != null) {
                _cf.Loaded -= _cf_Loaded;
                _cf.Changed -= _cf_Changed;
            }

            _cf = cf;

            if (_cf != null) {
                _cf.Loaded += _cf_Loaded;
                _cf.Changed += _cf_Changed;
            }
        }

        if (_database != database) {
            if (_database != null) {
                _database.Disposing -= _Database_Disposing;
                _database.Row.RowRemoving -= Row_RowRemoving;
            }
            _database = database;

            if (_database != null) {
                _database.Disposing += _Database_Disposing;
                _database.Row.RowRemoving += Row_RowRemoving;
            }
        }

        _pageToShow = page;

        InvalidateView();

        //if (string.Equals(_pageToShow, value, System.StringComparison.OrdinalIgnoreCase)) { return; }
        //RowKey = -1;

        if (rowKey != -1 && _database != null && _cf != null) {
            _rowkey = rowKey;
            _tmpShowingRow = _database?.Row.SearchByKey(_rowkey);
            SetInputRow();
        }

        ResumeLayout();
        Invalidate();
    }

    private void GenerateView() {
        if (_generated) { return; }

        var unused = new List<System.Windows.Forms.Control>();
        foreach (var thisco in Controls) {
            if (thisco is System.Windows.Forms.Control c) {
                unused.Add(c);
            }
        }

        if (_cf != null && _cf.PadData != null) {
            if (Visible || Controls.Count > 0) {
                var addfactor = Size.Width / _cf.PadData.SheetSizeInPix.Width;

                foreach (var thisit in _cf.PadData) {
                    if (thisit.IsVisibleOnPage(_pageToShow) && thisit is IItemToControl thisitco) {
                        var o = SearchOrGenerate(thisitco);

                        if (o is System.Windows.Forms.Control c) {
                            unused.Remove(c);

                            if (thisit is CustomizableShowPadItem cspi) {
                                c.Visible = cspi.IsVisibleForMe(_database?.UserGroup, _database?.UserName);
                            } else {
                                c.Visible = true;
                            }

                            var ua = thisit.UsedArea;
                            c.Left = (int)(ua.Left * addfactor);
                            c.Top = (int)(ua.Top / Umrechnungsfaktor2);
                            c.Width = (int)(ua.Width * addfactor);
                            c.Height = (int)(ua.Height / Umrechnungsfaktor2);

                            if (thisit is TabFormulaPadItem c3) {
                                c3.CreateTabs((TabControl)c, _database?.UserGroup, _database?.UserName);
                            }
                        }
                    }
                }
                _generated = true;
            }
        }

        foreach (var thisc in unused) {
            Controls.Remove(thisc);
            thisc.Dispose();
        }
    }

    private void Row_RowRemoving(object sender, RowEventArgs e) {
        if (e.Row != null && _rowkey == e.Row.Key) {
            RowKey = -1;
        }
    }

    private void SetInputRow() {
        GenerateView();
        if (!_generated) { return; }

        if (_cf == null || _cf.PadData == null) { return; }

        var listf = new List<FlexiControlForCell>();

        foreach (var thisIt in _cf.PadData) {
            if (thisIt is RowInputPadItem ripi && ripi.IsVisibleOnPage(Page)) {
                var c = SearchOrGenerate(ripi);

                if (c is FlexiControlForCell fcfc) {
                    listf.Add(fcfc);
                    fcfc.PauseValueChanged = true;

                    ColumnItem? co;

                    if (ripi.Spaltenname.Equals("#first", StringComparison.OrdinalIgnoreCase)) {
                        co = Database?.Column.First;
                    } else {
                        co = Database?.Column.Exists(ripi.Spaltenname);
                    }
                    fcfc.Database = Database;

                    if (co != null) {
                        fcfc.ColumnName = co.Name;
                        fcfc.RowKey = RowKey;
                    } else {
                        fcfc.RowKey = -1;
                        fcfc.ColumnName = string.Empty;
                    }
                }
            }
        }

        foreach (var fcfc in listf) {
            fcfc.PauseValueChanged = false;
        }
    }

    #endregion
}