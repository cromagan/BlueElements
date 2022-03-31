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
using BlueScript;

namespace BlueControls.ConnectedFormula {

    internal class Connector : System.Windows.Forms.Control, ICalculateRowsControlLevel {

        #region Fields

        public static readonly List<Connector> AllConnectors = new();
        public readonly string VerbindungsId = string.Empty;
        public ItemCollectionPad? ParentCol;
        private readonly ListExt<System.Windows.Forms.Control> _parents = new();

        //private readonly RowWithFilterPaditem _rwf;
        private bool _disposing = false;

        private Database _FilterDefiniton;
        private List<RowItem>? _rows;

        #endregion

        #region Constructors

        public Connector(string verbindungsId, Database? database, bool genaueinezeile, ItemCollectionPad parent, Database filterdef) {
            //_rwf = rwf;
            ParentCol = parent;
            Database = database;
            _FilterDefiniton = filterdef;
            Genau_eine_Zeile = genaueinezeile;
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
        public Database? Database { get; }
        public bool Genau_eine_Zeile { get; set; }
        public Script? Script { get; set; }

        #endregion

        #region Methods

        public static void DoChilds(ICalculateRowsControlLevel con, List<RowItem>? rows, ItemCollectionPad? parent) {
            //if (_disposing || IsDisposed) { return; }

            foreach (var thischild in con.Childs) {
                var did = false;

                if (!did && thischild is IAcceptVariableList rv) {
                    if (con.Genau_eine_Zeile && rv.OriginalText.Contains("~") && con.Script == null && rows.Count == 1) {
                        (_, _, con.Script) = rows[0].DoAutomatic("to be sure");
                    }

                    did = DoChilds_VariableList(rv, con.Script, rows, con.Database, con.Genau_eine_Zeile);
                }

                if (!did && thischild is IAcceptMultipleRows rc) {
                    did = DoChilds_MultipleRows(rc, rows, con.Database, con.Genau_eine_Zeile);
                }

                if (!did && thischild is IAcceptRowKey fcfc) {
                    did = DoChilds_OneRowKey(fcfc, rows, con.Database, con.Genau_eine_Zeile);
                }

                if (!did && thischild is IAcceptItemsForSelect fc) {
                    did = DoChilds_ListItems(fc, rows, con.Genau_eine_Zeile, parent);
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

                _disposing = true;
                Childs.Clear();
                _parents.Clear();
                _rows = null;
                ParentCol = null;

                Childs.ItemAdded -= Childs_ItemAdded;
                _parents.ItemAdded -= Parents_ItemAdded;
                _parents.ItemRemoving -= Parents_ItemRemoving;
            }
        }

        protected override void OnParentChanged(System.EventArgs e) {
            base.OnParentChanged(e);

            Childs.ItemAdded += Childs_ItemAdded;
            _parents.ItemAdded += Parents_ItemAdded;
            _parents.ItemRemoving += Parents_ItemRemoving;

            GetParentsList();
            CalculateRows();
        }

        private static bool DoChilds_ListItems(IAcceptItemsForSelect fc, List<RowItem>? rows, bool genaueinezeile, ItemCollectionPad? parent) {
            // Dropdownmenü mehrerer Einträge
            if (genaueinezeile) { return false; }

            // Wie lautet der eigene Ursprüngliche Name, von dem das FlexControl abstammt
            var id = (string)fc.Tag;

            // Sich selbst suchen - also, das Original Item. Das Parent hier ist die PadCollection
            var efpi = parent[id];

            if (efpi is not EditFieldPadItem epfi2) { return false; }

            var li = epfi2.Column.Contents(rows);
            var cbx = new ItemCollection.ItemCollectionList.ItemCollectionList();
            cbx.AddRange(li, epfi2.Column, ShortenStyle.Replaced, epfi2.Column.BildTextVerhalten);
            cbx.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird

            if (fc.EditType == EditTypeFormula.Textfeld_mit_Auswahlknopf) {
                fc.StyleComboBox(cbx, System.Windows.Forms.ComboBoxStyle.DropDownList, true);
            }

            return true;
        }

        private static bool DoChilds_MultipleRows(IAcceptMultipleRows fcfc, List<RowItem>? rows, Database? database, bool genaueinezeile) {
            // Normales Zellenfeld
            if (!genaueinezeile) {
                fcfc.Database = database;
                fcfc.Rows.Clear();
                fcfc.Rows.AddRange(rows);
                return true;
            }

            return false;
        }

        private static bool DoChilds_OneRowKey(IAcceptRowKey fcfc, List<RowItem>? rows, Database? database, bool genaueinezeile) {
            // Normales Zellenfeld
            if (genaueinezeile) {
                fcfc.Database = database;

                if (rows != null && rows.Count == 1) {
                    fcfc.RowKey = rows[0].Key;
                } else {
                    fcfc.RowKey = -1;
                }
                return true;
            }
            return false;
        }

        private static bool DoChilds_VariableList(IAcceptVariableList fcfc, Script? script, List<RowItem>? rows, Database? database, bool genaueinezeile) {
            // Normales Zellenfeld
            if (genaueinezeile && script != null && script.Variables != null && script.Variables.Count > 0) {
                fcfc.ParseVariables(script.Variables);
                //fcfc.Database = database;
                //fcfc.Rows.Clear();
                //fcfc.Rows.AddRange(rows);
                return true;
            }

            return false;
        }

        //private void _rwf_Changed(object? sender, System.EventArgs e) {
        //    GetParentsList();
        //    CalculateRows();
        //}

        private void CalculateRows() {
            if (_disposing || IsDisposed) { return; }

            #region Filter erstellen

            var f = new FilterCollection(Database);

            foreach (var thisR in _FilterDefiniton.Row) {

                #region Column ermitteln

                var column = Database.Column.SearchByKey(thisR.CellGetInteger("Spalte"));

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
                    var connected = ParentCol[thisR.CellGetString("suchtxt")];

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

            #region Zeile(n) ermitteln und Script löschen

            _rows = Database.Row.CalculateFilteredRows(f);
            Script = null;

            #endregion

            DoChilds(this, _rows, ParentCol);
        }

        private void Childs_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            DoChilds(this, _rows, ParentCol);
        }

        private void GetParentsList() {
            if (_disposing || IsDisposed || Parent == null) { return; }

            foreach (var thisR in _FilterDefiniton.Row) {
                var item = ParentCol[thisR.CellGetString("suchtxt")];
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