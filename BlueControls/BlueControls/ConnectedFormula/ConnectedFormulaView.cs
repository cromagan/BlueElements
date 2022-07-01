// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueControls.ConnectedFormula.ConnectedFormula;

#nullable enable

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedFormulaView : GenericControl, IBackgroundNone, IAcceptRowKey {

    #region Fields

    public bool Generated = false;

    private ConnectedFormula.ConnectedFormula? _cf;
    private Database? _database = null;

    //private RowItem? _inputrow = null;
    private string _pageToShow = "Head";

    private long _rowkey = -1;

    #endregion

    #region Constructors

    public ConnectedFormulaView() : this("Head") { }

    public ConnectedFormulaView(string page) {
        InitializeComponent();
        _pageToShow = page;
        GenerateView();
    }

    #endregion

    #region Properties

    public ConnectedFormula.ConnectedFormula? ConnectedFormula {
        get => _cf;
        set {
            if (_cf == value) { return; }
            RowKey = -1;
            Database = null;

            if (_cf != null) {
                _cf.Changed -= _cf_Changed;
            }
            _cf = value;

            if (_cf != null) {
                _cf.Changed += _cf_Changed;
            }

            InvalidateView();
        }
    }

    public Database? Database {
        get => _database;
        set {
            if (_database != value) {
                RowKey = -1;
                _database = value;
                InvalidateView();
            }
        }
    }

    public string Page {
        get => _pageToShow;
        set {
            if (string.Equals(_pageToShow, value, System.StringComparison.InvariantCultureIgnoreCase)) { return; }
            RowKey = -1;

            _pageToShow = value;

            InvalidateView();
        }
    }

    public long RowKey {
        get => _rowkey;
        set {
            if (_rowkey != value) {
                _rowkey = value;
                InvalidateView();
            }
        }
    }

    #endregion

    #region Methods

    public void InvalidateView() {
        Generated = false;
        Invalidate(); // Sonst wird es nie neu gezeichnet
    }

    //public RowItem? InputRow {
    //    get => _inputrow;
    //    set {
    //        if (value == _inputrow) { return; }
    //        _inputrow = value;
    //        SetInputRow();
    //    }
    //}
    public System.Windows.Forms.Control? SearchOrGenerate(ItemCollection.BasicPadItem thisit) {
        if (thisit == null) { return null; }

        if (thisit.Page != Page) {
            Develop.DebugPrint("Falscher Seitenaufruf!");
            return null;
        }

        foreach (var thisC in Controls) {
            if (thisC is System.Windows.Forms.Control c) {
                if (c.Tag is string s) {
                    if (s == thisit.Internal) { return c; }
                }
            }
        }

        if (thisit is IItemToControl it) {
            var c = it.CreateControl(this);
            if (c.Tag is string s && s == thisit.Internal) {
                //alles ok
            } else {
                Develop.DebugPrint("Tag muß intern mit Internal beschrieben werden!");
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
        Generated = false;
        GenerateView();
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);

        SetInputRow();
    }

    //            case FlexiControl fc:
    //                if (value is string values) {
    //                    if (fc.Caption.Equals(id, System.StringComparison.InvariantCultureIgnoreCase)) {
    //                        fc.ValueSet(values, true, true);
    //                    }
    //                }
    //                break;
    private void _cf_Changed(object sender, System.EventArgs e) {
        InvalidateView();
    }

    private void GenerateView() {
        if (Generated) { return; }
        Generated = false;

        var unused = new List<System.Windows.Forms.Control>();
        foreach (var thisco in Controls) {
            if (thisco is System.Windows.Forms.Control c) {
                unused.Add(c);
            }
        }

        if (_cf != null && Visible) {
            var addfactor = Size.Width / _cf.PadData.SheetSizeInPix.Width;

            foreach (var thisit in _cf.PadData) {
                if (thisit.IsVisibleOnPage(_pageToShow)) {
                    var o = SearchOrGenerate(thisit);

                    if (o is System.Windows.Forms.Control c) {
                        unused.Remove(c);
                        c.Visible = true;
                        var ua = thisit.UsedArea;
                        c.Left = (int)(ua.Left * addfactor);
                        c.Top = (int)(ua.Top / Umrechnungsfaktor2);
                        c.Width = (int)(ua.Width * addfactor);
                        c.Height = (int)(ua.Height / Umrechnungsfaktor2);
                    }
                }
            }
            Generated = true;
        }

        foreach (var thisc in unused) {
            Controls.Remove(thisc);
            thisc.Dispose();
        }
    }

    private void SetInputRow() {
        GenerateView();
        if (!Generated) { return; }

        foreach (var thisIt in _cf.PadData) {
            if (thisIt is RowInputPadItem ripi && ripi.IsVisibleOnPage(Page)) {
                var c = SearchOrGenerate(ripi);

                if (c is FlexiControlForCell fcfc) {
                    ColumnItem? co;

                    if (ripi.Spaltenname.Equals("#first", System.StringComparison.InvariantCultureIgnoreCase)) {
                        co = Database?.Column.First();
                    } else {
                        co = Database?.Column.Exists(ripi.Spaltenname);
                    }
                    fcfc.Database = Database;

                    if (co != null) {
                        fcfc.ColumnKey = co.Key;
                        fcfc.RowKey = RowKey;
                    } else {
                        fcfc.RowKey = -1;
                        fcfc.ColumnKey = -1;
                    }
                }
            }
        }
    }

    #endregion
}