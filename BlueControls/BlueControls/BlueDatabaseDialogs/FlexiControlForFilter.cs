#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls {
    [Designer(typeof(BasicDesigner))]
    public partial class FlexiControlForFilter : FlexiControl, IContextMenu {


        /// <summary>
        /// ACHTUNG: Das Control wird niemals den Filter selbst ändern.
        /// Der Filter wird nur zur einfacheren Identifizierung der nachfolgenden Steuerelemente behalten.
        /// </summary>
        public readonly FilterItem Filter = null;

        public readonly Table TableView = null;

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public FlexiControlForFilter() : this(null, null, null) {
            // Dieser Aufruf ist für den Designer erforderlich.
            // InitializeComponent();
        }

        public FlexiControlForFilter(Table tableView, FilterItem filter, Filterleiste myParent) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Size = new Size(300, 300);
            TableView = tableView;
            Filter = filter;
            UpdateFilterData(myParent);

            InstantChangedEvent = true;

            Filter.Changed += Filter_Changed;
        }

        private void Filter_Changed(object sender, System.EventArgs e) {
            UpdateFilterData((Filterleiste)Parent);
        }

        private void UpdateFilterData(Filterleiste myParent) {


            if (Filter == null || Filter.Column == null) {
                DisabledReason = "Bezug zum Filter verloren.";
                Caption = string.Empty;
                EditType = enEditTypeFormula.None;
                QuickInfo = string.Empty;
                FileEncryptionKey = string.Empty;
                ValueSet(string.Empty, true, true);
            } else {
                if (string.IsNullOrEmpty(Filter.Herkunft)) {
                    DisabledReason = string.Empty;
                } else {
                    DisabledReason = "Dieser Filter ist automatisch<br>gesetzt worden.";
                }

                var qi = Filter.Column.QuickInfoText(string.Empty);

                if (string.IsNullOrEmpty(qi)) {
                    QuickInfo = "<b>Filter:</b><br>" + Filter.ReadableText();
                } else {
                    QuickInfo = "<b>Filter:</b><br>" + Filter.ReadableText() + "<br><br><b>Info:</b><br>" + qi;
                }



                if (!Filter.Column.AutoFilterSymbolPossible()) {
                    EditType = enEditTypeFormula.None;
                } else {

                    var ShowDelFilterButton = true;

                    if (Filter.FilterType == enFilterType.Instr_GroßKleinEgal && Filter.SearchValue != null && Filter.SearchValue.Count == 1) {

                        if (myParent == null || myParent.Orientation == enOrientation.Waagerecht || Filter.Column.DauerFilterPos.IsEmpty) {
                            CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
                        } else {
                            CaptionPosition = enÜberschriftAnordnung.Über_dem_Feld;

                        }
                        ShowDelFilterButton = false;
                        Caption = Filter.Column.ReadableText() + ":";
                        EditType = enEditTypeFormula.Textfeld_mit_Auswahlknopf;
                        ValueSet(Filter.SearchValue[0], true, true);
                    }


                    if (Filter.Column.FilterOptions == enFilterOptions.Enabled_OnlyAndAllowed || Filter.Column.FilterOptions == enFilterOptions.Enabled_OnlyOrAllowed) {
                        ShowDelFilterButton = false;
                        CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
                        Caption = Filter.Column.ReadableText() + ":";
                        EditType = enEditTypeFormula.Button;
                    }


                    if (ShowDelFilterButton) {
                        CaptionPosition = enÜberschriftAnordnung.ohne;
                        EditType = enEditTypeFormula.Button;
                    }
                }

            }
        }



        protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e) {

            base.OnControlAdded(e);

            e.Control.MouseUp += Control_MouseUp;

            if (e.Control is ComboBox cbx) {
                var Item2 = new ItemCollectionList
                {
                    { "Keine weiteren Einträge vorhanden", "|~" }
                };

                //var c = Filter.Column.Contents(null);

                //foreach (var thiss in c)
                //{
                //    Item2.Add("|" + thiss, thiss));
                //}

                StyleComboBox(cbx, Item2, System.Windows.Forms.ComboBoxStyle.DropDown);
                cbx.DropDownShowing += Cbx_DropDownShowing;
            }


            if (e.Control is Button btn) {

                if (CaptionPosition == enÜberschriftAnordnung.ohne) {
                    btn.ImageCode = "Trichter|16||1";
                    btn.Text = Filter.ReadableText();
                } else {
                    if (Filter != null && Filter.SearchValue != null && Filter.SearchValue.Count > 0 && !string.IsNullOrEmpty(Filter.SearchValue[0])) {
                        btn.ImageCode = "Trichter|16";
                        btn.Text = "wählen (" + Filter.SearchValue.Count.ToString() + ")";
                    } else {
                        btn.ImageCode = "Trichter|16";
                        btn.Text = "wählen";
                    }
                }


            }
        }

        private void Control_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {

            if (e.Button == System.Windows.Forms.MouseButtons.Right) {

                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
            }

        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {

                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
            }
        }

        private void Cbx_DropDownShowing(object sender, System.EventArgs e) {
            var cbx = (ComboBox)sender;

            cbx.Item.Clear();
            cbx.Item.CheckBehavior = enCheckBehavior.MultiSelection;


            if (TableView == null) {
                cbx.Item.Add("Anzeigefehler", "|~", enImageCode.Kreuz, false);
                return;
            }

            var List_FilterString = Filter.Column.Autofilter_ItemList(TableView.Filter, TableView.PinnedRows);

            if (List_FilterString.Count == 0) {

                cbx.Item.Add("Keine weiteren Einträge vorhanden", "|~", enImageCode.Kreuz, false);
            } else if (List_FilterString.Count < 400) {
                cbx.Item.AddRange(List_FilterString, Filter.Column, enShortenStyle.Replaced, Filter.Column.BildTextVerhalten);
                cbx.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
            } else {
                cbx.Item.Add("Zu viele Einträge", "|~", enImageCode.Kreuz, false);
            }

        }

        internal ComboBox GetComboBox() {

            foreach (var thisc in Controls) {
                if (thisc is ComboBox cbx) {

                    return cbx;
                }
            }
            return null;

        }

        internal bool WasThisValueClicked() {
            var cb = GetComboBox();
            if (cb == null) { return false; }
            return cb.WasThisValueClicked();
        }

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate) {
            HotItem = null;

            if (Filter == null) { return; }

            if (Filter.Column == null) { return; }

            if (!Filter.Column.Database.IsAdministrator()) { return; }


            HotItem = Filter.Column;




            Items.Add("Spalte bearbeiten", "#ColumnEdit", QuickImage.Get(enImageCode.Spalte));


            if (Parent is Filterleiste f) {

                if (f.pic.Visible) {
                    Items.Add("Filter verschieben", "#FilterVerschieben", QuickImage.Get(enImageCode.Trichter));

                    Items.Add("Bild-Pfad öffnen", "#BildPfad", QuickImage.Get(enImageCode.Ordner));
                }



            }





        }

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
            switch (e.ClickedComand.ToLower()) {
                case "#columnedit":
                    if (e.HotItem is ColumnItem col) {
                        tabAdministration.OpenColumnEditor(col, null);
                    }

                    return true;

                case "#filterverschieben":
                    if (e.HotItem is ColumnItem col2) {
                        var pc = (Filterleiste)Parent; // Parent geht verlren, wenn der Filter selbst disposed und neu erzeugt wird

                        while (true) {

                            var nx = InputBox.Show("X, von 0 bis 10000", col2.DauerFilterPos.X.ToString(), enDataFormat.Ganzzahl);
                            if (string.IsNullOrEmpty(nx)) { return true; }
                            var nxi = modConverter.IntParse(nx);
                            nxi = Math.Max(nxi, 0);
                            nxi = Math.Min(nxi, 10000);

                            var ny = InputBox.Show("Y, von 0 bis 10000", col2.DauerFilterPos.Y.ToString(), enDataFormat.Ganzzahl);
                            if (string.IsNullOrEmpty(ny)) { return true; }
                            var nyi = modConverter.IntParse(ny);
                            nyi = Math.Max(nyi, 0);
                            nyi = Math.Min(nyi, 10000);

                            col2.DauerFilterPos = new Point(nxi, nyi);
                            pc.FillFilters();
                        }
                    }
                    return true;

                case "#bildpfad":
                    var p = (string)((Filterleiste)Parent).pic.Tag;
                    ExecuteFile(p.FilePath());
                    MessageBox.Show("Aktuelle Datei:<br>" + p);
                    return true;

                default:

                    if (Parent is Formula f) {
                        return f.ContextMenuItemClickedInternalProcessig(sender, e);
                    }
                    break;
            }

            return false;
        }


        public void OnContextMenuInit(ContextMenuInitEventArgs e) {
            ContextMenuInit?.Invoke(this, e);
        }


        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) {
            ContextMenuItemClicked?.Invoke(this, e);
        }
    }
}