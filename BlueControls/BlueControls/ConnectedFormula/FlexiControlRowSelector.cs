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

using BlueBasics;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueScript;
using System.Collections.Generic;
using static BlueBasics.Converter;
using static BlueBasics.Develop;

namespace BlueControls.ConnectedFormula;

internal class FlexiControlRowSelector : FlexiControl, ICalculateRowsControlLevel {

    #region Fields

    public readonly DatabaseAbstract? FilterDefiniton;

    public ItemCollectionPad? ParentCol;

    private readonly ListExt<System.Windows.Forms.Control> _parents = new();

    private readonly string _showformat;

    private bool _disposing;

    private RowItem? _row;

    private List<RowItem>? _rows;

    #endregion

    #region Constructors

    public FlexiControlRowSelector(DatabaseAbstract? database, ItemCollectionPad parent, DatabaseAbstract? filterdef, string caption, string showFormat) : base() {
        CaptionPosition = ÜberschriftAnordnung.Über_dem_Feld;
        EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;

        Caption = string.IsNullOrEmpty(caption) ? "Wählen:" : caption;
        _showformat = showFormat;

        if (string.IsNullOrEmpty(_showformat) && database != null && database.Column.Count > 0 && database.Column.First is ColumnItem fc) {
            _showformat = "~" + fc.Name + "~";
        }

        ParentCol = parent;
        Database = database;
        FilterDefiniton = filterdef;

        // den Rest initialisieren, bei OnParentChanged
        // weil der Parent gebraucht wird um Filter zu erstellen
    }

    #endregion

    #region Properties

    public ListExt<System.Windows.Forms.Control> Childs { get; } = new();
    public DatabaseAbstract? Database { get; set; }

    public RowItem? Row {
        get => IsDisposed ? null : _row;
        private set {
            if (IsDisposed) { return; }
            if (value == _row) { return; }

            _row = value;
            Script = null;
            DoChilds(this, _row);
        }
    }

    public Script? Script { get; set; }

    #endregion

    #region Methods

    public static void DoChilds(ICalculateRowsControlLevel con, RowItem? row) {
        //if (_disposing || IsDisposed) { return; }

        foreach (var thischild in con.Childs) {
            var did = false;

            if (!did && thischild is IAcceptRowKey fcfc) {
                DoChilds_OneRowKey(fcfc, row, con.Database);
                did = true;
            }

            if (!did && thischild is IAcceptVariableList rv) {
                //if (row != null && rv.OriginalText.Contains("~") && con.Script == null) {
                //    (_, _, con.Script) = row.DoAutomatic("to be sure", false);
                //}

                DoChilds_VariableList(rv, con.Script);
                did = true;
            }

            if (thischild is IDisabledReason id) {
                if (!did) {
                    id.DeleteValue();
                    id.DisabledReason = "Keine Befüllmethode bekannt.";
                }
            }
        }
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            _disposing = true;

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

    protected override void OnValueChanged() {
        base.OnValueChanged();

        Row = string.IsNullOrEmpty(Value) ? null : Database?.Row.SearchByKey(LongParse(Value));
    }

    private static void DoChilds_OneRowKey(IAcceptRowKey fcfc, RowItem? row, DatabaseAbstract? database) {
        // Normales Zellenfeld
        if (fcfc.IsDisposed) { return; }

        fcfc.Database = database;

        if (row != null) {
            fcfc.RowKey = row.Key;
        } else {
            fcfc.RowKey = -1;
        }
    }

    private static void DoChilds_VariableList(IAcceptVariableList fcfc, Script? script) =>
        // Normales Zellenfeld
        fcfc.ParseVariables(script?.Variables);

    private void CalculateRows() {
        if (_disposing || IsDisposed) { return; }

        if (FilterDefiniton == null || FilterDefiniton.IsDisposed) { return; }

        if (FilterDefiniton.Row.Count > 0) {

            #region Filter erstellen

            var f = new FilterCollection(Database);

            foreach (var thisR in FilterDefiniton.Row) {

                #region Column ermitteln

                var column = Database?.Column.Exists(thisR.CellGetString("Spalte"));
                if (column == null) { return; }

                #endregion

                #region Type ermitteln

                var onlyifhasvalue = false;

                FilterType ft;
                switch (thisR.CellGetString("Filterart").ToLower()) {
                    case "=":
                        ft = FilterType.Istgleich_GroßKleinEgal;
                        break;

                    case "=!empty":
                        ft = FilterType.Istgleich_GroßKleinEgal;
                        onlyifhasvalue = true;
                        break;

                    //case "x":
                    //    // Filter löschen
                    //    ft = FilterType.KeinFilter;
                    //    break;

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
                        case ConstantTextPadItem ctpi:

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

                        case RowInputPadItem ripi:
                            if (Parent is ConnectedFormulaView cfvy) {
                                var se = cfvy.SearchOrGenerate(ripi);

                                if (se is FlexiControlForCell fcfc) {
                                    value = fcfc.Value;
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
                    if (!string.IsNullOrEmpty(value) || !onlyifhasvalue) {
                        f.Add(new FilterItem(column, ft, value));
                    }
                }
            }

            #endregion

            #region Zeile(n) ermitteln und Script löschen

            _rows = Database?.Row.CalculateFilteredRows(f);

            Script = null;

            #endregion
        }

        if (_disposing || IsDisposed) { return; } // Multitasking...

        UpdateMyCollection();
    }

    private void Childs_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) => DoChilds(this, _row);

    private void GetParentsList() {
        if (_disposing || IsDisposed || Parent == null) { return; }

        foreach (var thisR in FilterDefiniton.Row) {
            var item = ParentCol?[thisR.CellGetString("suchtxt")];
            if (item is IItemToControl itco) {
                var c = ((ConnectedFormulaView)Parent).SearchOrGenerate(itco);
                if (c != null) { _parents.Add(c); }
            }
        }
    }

    private void Parent_ValueChanged(object sender, System.EventArgs e) => CalculateRows();

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

    private void UpdateMyCollection() {
        if (IsDisposed) { return; }
        if (!Allinitialized) { _ = CreateSubControls(); }

        #region Combobox suchen

        ComboBox? cb = null;
        foreach (var thiscb in Controls) {
            if (thiscb is ComboBox cbx) { cb = cbx; break; }
        }

        #endregion

        if (cb == null) { return; }

        List<BasicListItem> ex = new();
        ex.AddRange(cb.Item);

        #region Zeilen erzeugen

        if (_rows != null) {
            foreach (var thisR in _rows) {
                if (cb?.Item?[thisR.Key.ToString()] == null) {
                    var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, true);
                    _ = (cb?.Item?.Add(tmpQuickInfo, thisR.Key.ToString()));
                    //cb.Item.Add(thisR, string.Empty);
                } else {
                    foreach (var thisIt in ex) {
                        if (thisIt.Internal == thisR.Key.ToString()) {
                            _ = ex.Remove(thisIt);
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Veraltete Zeilen entfernen

        foreach (var thisit in ex) {
            cb?.Item?.Remove(thisit);
        }

        #endregion

        #region Nur eine Zeile? auswählen!

        // nicht vorher auf null setzen, um Blinki zu vermeiden
        if (cb.Item.Count == 1) {
            ValueSet(cb.Item[0].Internal, true, true);
        }

        #endregion

        #region  Prüfen ob die aktuelle Auswahl passt

        // am Ende auf null setzen, um Blinki zu vermeiden

        if (cb.Item[Value] == null) {
            ValueSet(string.Empty, true, true);
        }

        #endregion
    }

    #endregion
}