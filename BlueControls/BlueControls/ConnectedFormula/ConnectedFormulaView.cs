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
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using static BlueControls.ConnectedFormula.ConnectedFormula;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedFormulaView : GenericControl, IBackgroundNone, IHasDatabase {

    #region Fields

    private bool _generated;
    private RowItem? _tmpShowingRow;

    private string _userGroup = string.Empty;

    private string _userName = string.Empty;

    #endregion

    #region Constructors

    public ConnectedFormulaView() : this("Head", string.Empty, DatabaseAbstract.Administrator) { }

    public ConnectedFormulaView(string page, string username, string usergroup) {
        InitializeComponent();
        Page = page;
        _userName = username;
        _userGroup = usergroup;
        SetData(null, null, -1);
    }

    #endregion

    #region Properties

    public ConnectedFormula.ConnectedFormula? ConnectedFormula {
        get; private set;
        //set => SetData(value, Database, RowKey, Page);
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DatabaseAbstract? Database { get; private set; }

    //public IControlSendRow? GetRowFrom {
    //    get => _getRowFrom;
    //    set {
    //        if (_getRowFrom == value) { return; }
    //        if (_getRowFrom != null) {
    //            Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "Änderung nicht erlaubt");
    //        }

    //        _getRowFrom = value;
    //        if (_getRowFrom != null) { _getRowFrom.ChildAdd(this); }
    //    }
    //}

    public string Page { get; } = "Head";

    [DefaultValue(-1)]
    public long RowKey { get; private set; } = -1;

    //set => SetData(ConnectedFormula, Database, value, Page);
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowItem? ShowingRow {
        get {
            if (IsDisposed) { return null; }
            return _tmpShowingRow;
        }
    }

    public string UserGroup {
        get => _userGroup;
        set {
            if (_userGroup.Equals(value, StringComparison.OrdinalIgnoreCase)) { return; }
            _userGroup = value;
            InvalidateView();
        }
    }

    public string UserName {
        get => _userName;
        set {
            if (_userName.Equals(value, StringComparison.OrdinalIgnoreCase)) { return; }
            _userName = value;
            InvalidateView();
        }
    }

    #endregion

    #region Methods

    public void GenerateView() {
        if (_generated) { return; }

        var unused = new List<Control>();
        foreach (var thisco in Controls) {
            if (thisco is Control c) {
                unused.Add(c);
            }
        }

        if (ConnectedFormula != null && ConnectedFormula.PadData != null) {
            //if (Visible || Controls.Count > 0) {
            var addfactor = Size.Width / ConnectedFormula.PadData.SheetSizeInPix.Width;

            foreach (var thisit in ConnectedFormula.PadData) {
                if (thisit.IsVisibleOnPage(Page) && thisit is IItemToControl thisitco) {
                    var o = SearchOrGenerate(thisitco);

                    if (o is Control c) {
                        _ = unused.Remove(c);

                        if (thisit is FakeControlPadItem cspi) {
                            c.Visible = cspi.IsVisibleForMe(UserName, UserGroup);
                        } else {
                            c.Visible = true;
                        }

                        var ua = thisit.UsedArea;
                        c.Left = (int)(ua.Left * addfactor);
                        c.Top = (int)(ua.Top / Umrechnungsfaktor2);
                        c.Width = (int)(ua.Width * addfactor);
                        c.Height = (int)(ua.Height / Umrechnungsfaktor2);

                        if (thisit is TabFormulaPadItem c3) {
                            c3.CreateTabs((TabControl)c, UserName, UserGroup);
                        }
                    }
                }
            }
            _generated = true;
            //}
        }

        foreach (var thisc in unused) {
            Controls.Remove(thisc);
            thisc?.Dispose();
        }
    }

    public void GetConnectedFormulaFromDatabase(DatabaseAbstract? database) {
        if (database != null && !database.IsDisposed) {
            var f = database.FormulaFileName();

            UserName = database.UserName;
            UserGroup = database.UserGroup;

            if (f != null) {
                var tmpFormula = GetByFilename(f);
                if (tmpFormula != null) {
                    SetData(tmpFormula, database, -1);
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

    public Control? SearchOrGenerate(IItemToControl? thisit) {
        if (thisit == null) { return null; }

        foreach (var thisC in Controls) {
            if (thisC is Control cx && cx.Name is string sx && sx == thisit.DefaultItemToControlName()) { return cx; }
        }

        var c = thisit.CreateControl(this);
        if (c == null || c.Name is not string s || s != thisit.DefaultItemToControlName()) {
            Develop.DebugPrint("Name muss intern mit Internal-Version beschrieben werden!");
            return null;
        }

        Controls.Add(c);
        return c;
    }

    public void SetData(DatabaseAbstract? database, long? rowKey) => SetData(ConnectedFormula, database, rowKey ?? -1);

    public void SetData(ConnectedFormula.ConnectedFormula? cf, DatabaseAbstract? database, long rowKey) {
        if (IsDisposed) { return; }

        var oldf = ConnectedFormula; // Zwischenspeichern wegen möglichen NULL verweisen

        if (oldf == cf &&
            Database == database &&
            RowKey == rowKey) { return; }

        SuspendLayout();

        if (oldf != cf) {
            if (oldf != null) {
                RemoveRow();
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

        if (Database != database) {
            if (Database != null) {
                RemoveRow();
                Database.Disposing -= _Database_Disposing;
                Database.Row.RowRemoving -= Row_RowRemoving;
            }
            InvalidateView();
            Database = database;

            if (Database != null) {
                Database.Disposing += _Database_Disposing;
                Database.Row.RowRemoving += Row_RowRemoving;
            }
        }

        if (rowKey != -1 && Database != null && cf != null) {
            RowKey = rowKey;
            _tmpShowingRow = Database?.Row.SearchByKey(RowKey);
            SetInputRow();
        } else {
            RemoveRow();
        }

        ResumeLayout();
        Invalidate();
    }

    //public void SetData(DatabaseAbstract? database, long? rowkey) {
    //    if (database != Database && rowkey == RowKey) { return; }
    //    //_database = database;
    //    //_rowkey = rowkey ?? -1;
    //    SetData(ConnectedFormula, database, rowkey ?? -1, Page);
    //}

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

    private void _cf_Changed(object sender, System.EventArgs e) => InvalidateView();

    private void _cf_Loaded(object sender, System.EventArgs e) => InvalidateView();

    private void _Database_Disposing(object sender, System.EventArgs e) => SetData(null, -1);

    private void RemoveRow() {
        RowKey = -1;
        _tmpShowingRow = null;
        if (_generated) { SetInputRow(); }
    }

    private void Row_RowRemoving(object sender, RowEventArgs e) {
        if (e.Row != null && RowKey == e.Row.Key) {
            SetData(ConnectedFormula, Database, -1);
        }
    }

    private void SetInputRow() {
        if (IsDisposed) { return; }

        GenerateView();
        if (!_generated) { return; }

        if (ConnectedFormula == null || ConnectedFormula.PadData == null) { return; }

        foreach (var thisIt in ConnectedFormula.PadData) {
            if (thisIt is IItemRowInput ripi && ripi.IsVisibleOnPage(Page)) {
                var c = SearchOrGenerate(ripi);
                if (c is IControlRowInput fcfc) {
                    fcfc.SetData(Database, RowKey);
                }
            }
        }

        if (Database?.Row.SearchByKey(RowKey) is RowItem r) {
            r.CheckRowDataIfNeeded();
        }
    }

    #endregion
}