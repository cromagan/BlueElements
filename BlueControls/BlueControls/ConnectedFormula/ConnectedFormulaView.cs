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
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using BlueControls.ItemCollection;
using static BlueBasics.Converter;
using BlueDatabase.Enums;
using static BlueControls.ConnectedFormula.ConnectedFormula;
using BlueControls.ConnectedFormula;

#nullable enable

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    public partial class ConnectedFormulaView : GenericControl, IBackgroundNone {

        #region Fields

        private ConnectedFormula.ConnectedFormula? _cf;

        #endregion

        #region Constructors

        public ConnectedFormulaView() {
            InitializeComponent();
            GenerateView();
        }

        #endregion

        #region Properties

        public ConnectedFormula.ConnectedFormula? ConnectedFormula {
            get => _cf;
            set {
                if (_cf == value) { return; }

                if (_cf != null) {
                    _cf.Changed -= _cf_Changed;
                }
                _cf = value;

                if (_cf != null) {
                    _cf.Changed += _cf_Changed;
                }

                _cf_Changed(null, null);
            }
        }

        #endregion

        #region Methods

        public System.Windows.Forms.Control? SearchOrGenerate(ItemCollection.BasicPadItem thisit) {
            foreach (var thisC in Controls) {
                if (thisC is System.Windows.Forms.Control c) {
                    if (c.Tag is string s) {
                        if (s == thisit.Internal) { return c; }
                    }
                }
            }

            switch (thisit) {
                case EditFieldPadItem ef:

                    if (ef.GetRowFrom is RowWithFilterPaditem rfw2) {
                        var ff = SearchOrGenerate(rfw2);

                        if (rfw2.Genau_eine_Zeile) {
                            var cx = new FlexiControlForCell();
                            cx.ColumnKey = ef.Column.Key;
                            cx.EditType = ef.EditType;
                            cx.CaptionPosition = ef.CaptionPosition;
                            cx.Tag = ef.Internal;
                            Controls.Add(cx);
                            if (ff is Connector cc) { cc.Childs.Add(cx); }
                            return cx;
                        } else {
                            var c = new FlexiControl();
                            c.Caption = ef.Column.ReadableText() + ":";
                            c.EditType = ef.EditType;
                            c.CaptionPosition = ef.CaptionPosition;
                            c.Tag = ef.Internal;
                            Controls.Add(c);
                            if (ff is Connector cc) { cc.Childs.Add(c); }
                            return c;
                        }
                    }

                    return null;

                case RowWithFilterPaditem rwf:
                    var c2 = new Connector(rwf);
                    c2.Tag = rwf.Internal;
                    Controls.Add(c2);
                    return c2;

                case ConstantTextPaditem ctpi:
                    var c3 = new FlexiControl();
                    c3.Tag = ctpi.Internal;
                    c3.CaptionPosition = ÜberschriftAnordnung.ohne;
                    c3.EditType = EditTypeFormula.Textfeld;
                    c3.DisabledReason = "Konstanter Wert";
                    c3.ValueSet(ctpi.Text, true, true);
                    Controls.Add(c3);
                    return c3;

                default:
                    Develop.DebugPrint("Typ nicht definiert.");
                    return null;
            }
        }

        protected override void DrawControl(Graphics gr, States state) => Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);

        protected override void OnSizeChanged(System.EventArgs e) {
            if (IsDisposed) { return; }
            base.OnSizeChanged(e);
            GenerateView();
            //if (_database != null && _inited) {
            //    Control_RepairSize_All();
            //}
            //grpEditor.Left = Width - grpEditor.Width;
            //grpEditor.Height = Height;
        }

        private void _cf_Changed(object sender, System.EventArgs e) { GenerateView(); }

        private void GenerateView() {
            var used = new List<object>();


            if (_cf != null) {


            //var v = Converter.MmToPixel(_cf.PadData.SheetSizeInMm.Width, 300);

            var addfactor =Size.Width / _cf.PadData.SheetSizeInPix.Width;

                foreach (var thisit in _cf.PadData) {
                    var o = SearchOrGenerate(thisit);
                    used.Add(o);

                    if (o is GenericControl c) {
                        c.Visible = true;
                        var ua = thisit.UsedArea;
                        c.Left = (int)(ua.Left * addfactor);
                        c.Top = (int)(ua.Top / Umrechnungsfaktor2);
                        c.Width = (int)(ua.Width  * addfactor);
                        c.Height = (int)(ua.Height / Umrechnungsfaktor2);
                    }
                }
            }

            foreach (var thisc in Controls) {
                if (thisc is System.Windows.Forms.Control c) {
                    if (!used.Contains(c)) {
                        Controls.Remove(c);
                        c.Dispose();
                    }
                }
            }

            //if(_storedPadData== null) {
            //    _storedPadData = new ItemCollection.ItemCollectionPad(_cf.PadData, string.Empty);
            //}
        }

        #endregion
    }
}