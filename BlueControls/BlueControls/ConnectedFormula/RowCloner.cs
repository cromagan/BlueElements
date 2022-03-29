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

#nullable enable

using System.Collections.Generic;
using static BlueBasics.Develop;
using BlueDatabase;
using BlueBasics;
using BlueControls.ItemCollection;
using BlueControls.Controls;
using BlueDatabase.Enums;
using BlueControls.Interfaces;

namespace BlueControls.ConnectedFormula {

    internal class RowCloner : System.Windows.Forms.Control, IAcceptMultipleRows, IAcceptRowKey, ICalculateRowsControlLevel {

        #region Fields

        private readonly ListExt<RowItem> _rows = new();
        private readonly RowClonePadItem _rwf;
        private bool _disposing = false;

        #endregion

        #region Constructors

        public RowCloner(RowClonePadItem rwf) {
            _rwf = rwf;
            foreach (var thisConnector in Connector.AllConnectors) {
                if (thisConnector.VerbindungsId == rwf.VerbindungsID) {
                    thisConnector.Childs.Add(this);
                }
            }

            //_verbindungsID = verbindungsId;

            //if (!string.IsNullOrEmpty(verbindungsId)) {
            //    RemoveSameID(verbindungsId);
            //}

            //AllConnectors.Add(this);

            // den Rest initialisieren, bei OnParentChanged
            // weil der Parent gebraucht wird um Filter zu erstellen
        }

        #endregion

        #region Properties

        public ListExt<System.Windows.Forms.Control> Childs { get; } = new();
        public Database? Database { get; set; }

        public long RowKey {
            get {
                if (_rows.Count != 1) { return -1; }
                return _rows[0].Key;
            }
            set {
                if (_rows.Count == 1 && _rows[0].Key == value) { return; }
                _rows.Clear();
                var v = Database.Row.SearchByKey(value);
                if (v == null) { return; }
                _rows.Add(v);
            }
        }

        public ListExt<RowItem> Rows => _rows;

        #endregion

        #region Methods

        //private string _verbindungsID = string.Empty;
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                _disposing = true;

                //AllConnectors.Remove(this);

                Tag = null;
                _rwf.Changed -= _rwf_Changed;

                _disposing = true;
                Childs.Clear();
                //_parents.Clear();
                _rows.Clear();

                _rows.ItemAdded -= _rows_ItemAdded;
                _rows.ItemRemoved -= _rows_ItemRemoved;

                Childs.ItemAdded -= Childs_ItemAdded;
                //_parents.ItemAdded -= Parents_ItemAdded;
                //_parents.ItemRemoving -= Parents_ItemRemoving;
            }
        }

        protected override void OnParentChanged(System.EventArgs e) {
            base.OnParentChanged(e);

            _rwf.Changed += _rwf_Changed;
            Childs.ItemAdded += Childs_ItemAdded;
            _rows.ItemAdded += _rows_ItemAdded;
            _rows.ItemRemoved += _rows_ItemRemoved;
            //_parents.ItemAdded += Parents_ItemAdded;
            //_parents.ItemRemoving += Parents_ItemRemoving;

            //_rwf_Changed(null, System.EventArgs.Empty);
            Connector.DoChilds(_rows, Childs, _rwf);
        }

        private void _rows_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            Connector.DoChilds(_rows, Childs, _rwf);
        }

        private void _rows_ItemRemoved(object sender, System.EventArgs e) {
            Connector.DoChilds(_rows, Childs, _rwf);
        }

        private void _rwf_Changed(object? sender, System.EventArgs e) {
            Connector.DoChilds(_rows, Childs, _rwf);
        }

        //private void CalculateRows() {
        //    if (_disposing || IsDisposed) { return; }

        //    //#region Filter erstellen

        //    //var f = new FilterCollection(_rwf.Database);

        //    //foreach (var thisR in _rwf.FilterDefiniton.Row) {
        //    //    #region Column ermitteln

        //    //    var column = _rwf.Database.Column.SearchByKey(thisR.CellGetInteger("Spalte"));

        //    //    #endregion

        //    //    #region Type ermitteln

        //    //    FilterType ft;
        //    //    switch (thisR.CellGetString("Filterart").ToLower()) {
        //    //        case "=":
        //    //            ft = FilterType.Istgleich_GroßKleinEgal;
        //    //            break;

        //    //        case "x":
        //    //            // Filter löschen
        //    //            ft = FilterType.KeinFilter;
        //    //            break;

        //    //        default:
        //    //            ft = FilterType.Istgleich_GroßKleinEgal;
        //    //            DebugPrint("Filter " + thisR.CellGetInteger("Filterart") + " nicht definiert.");
        //    //            break;
        //    //    }

        //    //    #endregion

        //    //    #region Value ermitteln

        //    //    var value = string.Empty;
        //    //    if (ft != FilterType.KeinFilter) {
        //    //        var connected = _rwf.Parent[thisR.CellGetString("suchtxt")];

        //    //        switch (connected) {
        //    //            case ConstantTextPaditem ctpi:
        //    //                value = ctpi.Text;
        //    //                break;

        //    //            case EditFieldPadItem efpi:
        //    //                if (Parent is ConnectedFormulaView cfv) {
        //    //                    var se = cfv.SearchOrGenerate(efpi);

        //    //                    if (se is FlexiControlForCell fcfc) {
        //    //                        value = fcfc.Value;
        //    //                    } else if (se is FlexiControl fc) {
        //    //                        value = fc.Value;
        //    //                    } else {
        //    //                        DebugPrint("Unbekannt");
        //    //                    }
        //    //                } else {
        //    //                    value = "@@@";
        //    //                    //DebugPrint("Parent unbekannt!");
        //    //                }
        //    //                break;

        //    //            default:
        //    //                value = "@@@";
        //    //                ft = FilterType.KeinFilter; // Wurde dsa Parent eben gelöscht...
        //    //                //DebugPrint("Parent " + thisR.CellGetString("suchtxt") + " nicht gefunden.");
        //    //                break;
        //    //        }
        //    //    }

        //    //    #endregion

        //    //    if (column != null && ft != FilterType.KeinFilter) {
        //    //        f.Add(new FilterItem(column, ft, value));
        //    //    }
        //    //}

        //    //#endregion

        //    //#region Zeile(n) ermitteln

        //    //_rows = _rwf.Database.Row.CalculateFilteredRows(f);

        //    //#endregion

        //    Connector.DoChilds(_rows, Childs, _rwf);
        //}

        private void Childs_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            Connector.DoChilds(_rows, Childs, _rwf);
        }

        #endregion

        //private void GetParentsList() {
        //    if (_disposing || IsDisposed || Parent == null) { return; }

        //    foreach (var thisR in _rwf.FilterDefiniton.Row) {
        //        var item = _rwf.Parent[thisR.CellGetString("suchtxt")];
        //        if (item != null) {
        //            var c = ((ConnectedFormulaView)Parent).SearchOrGenerate(item);
        //            _parents.Add(c);
        //        }
        //    }
        //}

        //private void Parent_ValueChanged(object sender, System.EventArgs e) {
        //    CalculateRows();
        //}

        //private void Parents_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
        //    if (e.Item is FlexiControlForCell fcfc) {
        //        fcfc.ValueChanged += Parent_ValueChanged;
        //    } else if (e.Item is FlexiControl fc) {
        //        fc.ValueChanged += Parent_ValueChanged;
        //    } else {
        //        DebugPrint("unbekannt");
        //    }
        //    CalculateRows();
        //}

        //private void Parents_ItemRemoving(object sender, BlueBasics.EventArgs.ListEventArgs e) {
        //    if (e.Item is FlexiControlForCell fcfc) {
        //        fcfc.ValueChanged -= Parent_ValueChanged;
        //    }

        //    CalculateRows();
        //}

        //private void RemoveSameID(string verbindungsId) {
        //    foreach (var thisConnector in AllConnectors) {
        //        if (thisConnector._verbindungsID == verbindungsId) {
        //            AllConnectors.Remove(thisConnector);
        //            RemoveSameID(verbindungsId);
        //            return;
        //        }
        //    }
        //}
    }
}