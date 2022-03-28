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

using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlueBasics.Extensions;
using static BlueBasics.Develop;
using BlueDatabase;
using BlueBasics;
using static BlueBasics.Converter;
using BlueBasics.Interfaces;
using BlueControls.ItemCollection;
using BlueControls.Controls;
using BlueDatabase.Enums;

using BlueControls.ItemCollection;

using BlueControls.Interfaces;

namespace BlueControls.ConnectedFormula {

    internal class Connector : System.Windows.Forms.Control {

        #region Fields

        public readonly ListExt<System.Windows.Forms.Control> Childs = new();
        private readonly ListExt<System.Windows.Forms.Control> _parents = new();
        private readonly RowWithFilterPaditem _rwf;
        private bool _disposing = false;
        private List<RowItem>? _rows;

        private string _verbindungsID = string.Empty;

        #endregion

        #region Constructors

        public Connector(RowWithFilterPaditem rwf, string verbindungsID) {
            _rwf = rwf;
            _verbindungsID = verbindungsID;

            // den Rest initialisieren, bei OnParentChanged
            // weil der Parent gebraucht wird um Filter zu erstellen
        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                _disposing = true;

                Tag = null;
                _rwf.Changed -= _rwf_Changed;

                _disposing = true;
                Childs.Clear();
                _parents.Clear();
                _rows = null;

                Childs.ItemAdded -= Childs_ItemAdded;
                _parents.ItemAdded -= Parents_ItemAdded;
                _parents.ItemRemoving -= Parents_ItemRemoving;
            }
        }

        protected override void OnParentChanged(System.EventArgs e) {
            base.OnParentChanged(e);

            _rwf.Changed += _rwf_Changed;
            Childs.ItemAdded += Childs_ItemAdded;
            _parents.ItemAdded += Parents_ItemAdded;
            _parents.ItemRemoving += Parents_ItemRemoving;

            _rwf_Changed(null, System.EventArgs.Empty);
        }

        private void _rwf_Changed(object? sender, System.EventArgs e) {
            GetParentsList();
            CalculateRows();
        }

        private void CalculateRows() {
            if (_disposing || IsDisposed) { return; }

            #region Filter erstellen

            var f = new FilterCollection(_rwf.Database);

            foreach (var thisR in _rwf.FilterDefiniton.Row) {

                #region Column ermitteln

                var column = _rwf.Database.Column.SearchByKey(thisR.CellGetInteger("Spalte"));

                #endregion

                #region Type ermitteln

                FilterType ft;
                switch (thisR.CellGetString("Filterart").ToLower()) {
                    case "=":
                        ft = FilterType.Istgleich_GroßKleinEgal;
                        break;

                    case "x":
                        // Filter löschen
                        ft = FilterType.KeinFilter;
                        break;

                    default:
                        ft = FilterType.Istgleich_GroßKleinEgal;
                        DebugPrint("Filter " + thisR.CellGetInteger("Filterart") + " nicht definiert.");
                        break;
                }

                #endregion

                #region Value ermitteln

                var value = string.Empty;
                if (ft != FilterType.KeinFilter) {
                    var connected = _rwf.Parent[thisR.CellGetString("suchtxt")];

                    switch (connected) {
                        case ConstantTextPaditem ctpi:
                            value = ctpi.Text;
                            break;

                        case EditFieldPadItem efpi:
                            if (Parent is ConnectedFormulaView cfv) {
                                var se = cfv.SearchOrGenerate(efpi);

                                if (se is FlexiControlForCell fcfc) {
                                    value = fcfc.Value;
                                } else if (se is FlexiControl fc) {
                                    value = fc.Value;
                                } else {
                                    Develop.DebugPrint("Unbekannt");
                                }
                            } else {
                                value = "@@@";
                                //DebugPrint("Parent unbekannt!");
                            }
                            break;

                        default:
                            value = "@@@";
                            ft = FilterType.KeinFilter; // Wurde dsa Parent eben gelöscht...
                            //DebugPrint("Parent " + thisR.CellGetString("suchtxt") + " nicht gefunden.");
                            break;
                    }
                }

                #endregion

                if (column != null && ft != FilterType.KeinFilter) {
                    f.Add(new FilterItem(column, ft, value));
                }
            }

            #endregion

            #region Zeile(n) ermitteln

            _rows = _rwf.Database.Row.CalculateFilteredRows(f);

            #endregion

            ChildsUndVerbundeneBefüllen();
        }

        private void Childs_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            ChildsUndVerbundeneBefüllen();
        }

        private void ChildsUndVerbundeneBefüllen() {
            if (_disposing || IsDisposed) { return; }

            foreach (var thischild in Childs) {
                switch (thischild) {
                    case IAcceptRowKey fcfc:
                        // Normales Zellenfeld
                        if (_rwf.Genau_eine_Zeile) {
                            fcfc.Database = _rwf.Database;

                            if (_rows != null && _rows.Count == 1) {
                                fcfc.RowKey = _rows[0].Key;
                            } else {
                                fcfc.RowKey = -1;
                            }
                        } else {
                            // Falscher Datentyp
                            fcfc.RowKey = -1;
                        }
                        break;

                    case FlexiControl fc:
                        // Dropdownmenü mehrerer Einträge
                        if (_rwf.Genau_eine_Zeile) {
                            fc.DisabledReason = "Vorgänger hat falschen Datentyp";
                            fc.ValueSet(string.Empty, true, true);
                        } else {
                            // Wie lautet der eigene Ursprüngliche Name, von dem das FlexControl abstammt
                            var id = (string)fc.Tag;

                            // Sich selbst suchen - also, das Original Item. Das Patent hier ist die PadCollection
                            var efpi = (EditFieldPadItem)_rwf.Parent[id];

                            if (efpi == null) {
                                fc.DisabledReason = "Interner Fehler: Ursprungsitem nicht vorhanden";
                                fc.ValueSet(string.Empty, true, true);
                            } else {
                                var li = efpi.Column.Contents(_rows);
                                var cbx = new ItemCollection.ItemCollectionList.ItemCollectionList();
                                cbx.AddRange(li, efpi.Column, ShortenStyle.Replaced, efpi.Column.BildTextVerhalten);
                                cbx.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird

                                if (fc.EditType == EditTypeFormula.Textfeld_mit_Auswahlknopf) {
                                    fc.StyleComboBox(cbx, System.Windows.Forms.ComboBoxStyle.DropDownList);
                                    if (!li.Contains(fc.Value)) { fc.ValueSet(string.Empty, true, true); }
                                }
                            }
                        }
                        break;

                    default:
                        DebugPrint("Child nicht definiert.");
                        break;
                }
            }
        }

        private void GetParentsList() {
            if (_disposing || IsDisposed || Parent == null) { return; }

            foreach (var thisR in _rwf.FilterDefiniton.Row) {
                var item = _rwf.Parent[thisR.CellGetString("suchtxt")];
                if (item != null) {
                    var c = ((ConnectedFormulaView)Parent).SearchOrGenerate(item);
                    _parents.Add(c);
                }
            }
        }

        private void Parent_ValueChanged(object sender, System.EventArgs e) {
            CalculateRows();
        }

        private void Parents_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            if (e.Item is FlexiControlForCell fcfc) {
                fcfc.ValueChanged += Parent_ValueChanged;
            } else if (e.Item is FlexiControl fc) {
                fc.ValueChanged += Parent_ValueChanged;
            } else {
                Develop.DebugPrint("unbekannt");
            }
            CalculateRows();
        }

        private void Parents_ItemRemoving(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            if (e.Item is FlexiControlForCell fcfc) {
                fcfc.ValueChanged -= Parent_ValueChanged;
            }

            CalculateRows();
        }

        #endregion
    }
}