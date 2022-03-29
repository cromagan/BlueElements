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
using System.Runtime.InteropServices;
using static BlueBasics.Develop;
using BlueDatabase;
using BlueBasics;
using BlueControls.ItemCollection;
using BlueControls.Controls;
using BlueDatabase.Enums;
using BlueControls.Interfaces;

namespace BlueControls.ConnectedFormula {

    internal class Connector : System.Windows.Forms.Control, ICalculateRowsControlLevel {

        #region Fields

        public static readonly List<Connector> AllConnectors = new();
        public readonly string VerbindungsId = string.Empty;
        private readonly ListExt<System.Windows.Forms.Control> _parents = new();
        private readonly RowWithFilterPaditem _rwf;
        private bool _disposing = false;
        private List<RowItem>? _rows;

        #endregion

        #region Constructors

        public Connector(RowWithFilterPaditem rwf, string verbindungsId) {
            _rwf = rwf;
            VerbindungsId = verbindungsId;

            if (!string.IsNullOrEmpty(verbindungsId)) {
                RemoveSameID(verbindungsId);
            }

            AllConnectors.Add(this);

            // den Rest initialisieren, bei OnParentChanged
            // weil der Parent gebraucht wird um Filter zu erstellen
        }

        #endregion

        #region Properties

        public ListExt<System.Windows.Forms.Control> Childs { get; } = new();

        #endregion

        #region Methods

        public static void DoChilds(List<RowItem>? rows, ListExt<System.Windows.Forms.Control> childs, IConnectionAttributes rwf) {
            //if (_disposing || IsDisposed) { return; }

            foreach (var thischild in childs) {
                var did = false;

                if (!did && thischild is IAcceptMultipleRows rc) {
                    did = DoChilds_MultipleRows(rc, rows, childs, rwf);
                }

                if (!did && thischild is IAcceptRowKey fcfc) {
                    did = DoChilds_OneRowKey(fcfc, rows, childs, rwf);
                }

                if (!did && thischild is IAcceptItemsForSelect fc) {
                    did = DoChilds_ListItems(fc, rows, childs, rwf);
                }

                if (thischild is IDisabledReason id) {
                    if (!did) {
                        id.DeleteValue();
                        id.DisabledReason = "Keine Befüllmethode bekannt.";
                    } else {
                        id.DisabledReason = string.Empty;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                _disposing = true;

                AllConnectors.Remove(this);

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

        private static bool DoChilds_ListItems(IAcceptItemsForSelect fc, List<RowItem>? rows, ListExt<System.Windows.Forms.Control> childs, IConnectionAttributes attributes) {
            // Dropdownmenü mehrerer Einträge
            if (attributes.Genau_eine_Zeile) {
                //fc.DisabledReason = "Vorgänger hat falschen Datentyp";
                //fc.StyleComboBox(null, System.Windows.Forms.ComboBoxStyle.DropDownList, true);
                //fc.ValueSet(string.Empty, true, true);
                return false;
            }

            // Wie lautet der eigene Ursprüngliche Name, von dem das FlexControl abstammt
            var id = (string)fc.Tag;

            // Sich selbst suchen - also, das Original Item. Das Patent hier ist die PadCollection
            var efpi = attributes.Parent[id];

            if (efpi is not EditFieldPadItem epfi2) {
                //fc.DisabledReason = "Interner Fehler: Ursprungsitem nicht vorhanden";
                //fc.StyleComboBox(null, System.Windows.Forms.ComboBoxStyle.DropDownList, true);
                //fc.ValueSet(string.Empty, true, true);
                return false;
            }

            var li = epfi2.Column.Contents(rows);
            var cbx = new ItemCollection.ItemCollectionList.ItemCollectionList();
            cbx.AddRange(li, epfi2.Column, ShortenStyle.Replaced, epfi2.Column.BildTextVerhalten);
            cbx.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird

            if (fc.EditType == EditTypeFormula.Textfeld_mit_Auswahlknopf) {
                fc.StyleComboBox(cbx, System.Windows.Forms.ComboBoxStyle.DropDownList, true);
            }

            return true;
        }

        private static bool DoChilds_MultipleRows(IAcceptMultipleRows fcfc, List<RowItem>? rows, ListExt<System.Windows.Forms.Control> childs, IConnectionAttributes attributes) {
            // Normales Zellenfeld
            if (attributes.Genau_eine_Zeile) {
                fcfc.Database = attributes.Database;
                fcfc.Rows.Clear();
                fcfc.Rows.AddRange(rows);
                return true;
            }

            return false;
        }

        private static bool DoChilds_OneRowKey(IAcceptRowKey fcfc, List<RowItem>? rows, ListExt<System.Windows.Forms.Control> childs, IConnectionAttributes attributes) {
            // Normales Zellenfeld
            if (attributes.Genau_eine_Zeile) {
                fcfc.Database = attributes.Database;

                if (rows != null && rows.Count == 1) {
                    fcfc.RowKey = rows[0].Key;
                } else {
                    fcfc.RowKey = -1;
                }
                return true;
            }
            return false;
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

                            if (Parent is ConnectedFormulaView cfvx) {
                                var se2 = cfvx.SearchOrGenerate(ctpi);

                                if (se2 is FlexiControl fcx) {
                                    value = fcx.Value;
                                } else {
                                    DebugPrint("Unbekannt");
                                }
                            } else {
                                value = "@@@";
                                //DebugPrint("Parent unbekannt!");
                            }
                            break;

                        case EditFieldPadItem efpi:
                            if (Parent is ConnectedFormulaView cfv) {
                                var se = cfv.SearchOrGenerate(efpi);

                                if (se is FlexiControlForCell fcfc) {
                                    value = fcfc.Value;
                                } else if (se is FlexiControl fc) {
                                    value = fc.Value;
                                } else {
                                    DebugPrint("Unbekannt");
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

            DoChilds(_rows, Childs, _rwf);
        }

        private void Childs_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            DoChilds(_rows, Childs, _rwf);
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
                DebugPrint("unbekannt");
            }
            CalculateRows();
        }

        private void Parents_ItemRemoving(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            if (e.Item is FlexiControlForCell fcfc) {
                fcfc.ValueChanged -= Parent_ValueChanged;
            }

            CalculateRows();
        }

        private void RemoveSameID(string verbindungsId) {
            foreach (var thisConnector in AllConnectors) {
                if (thisConnector.VerbindungsId == verbindungsId) {
                    AllConnectors.Remove(thisConnector);
                    RemoveSameID(verbindungsId);
                    return;
                }
            }
        }

        #endregion
    }
}