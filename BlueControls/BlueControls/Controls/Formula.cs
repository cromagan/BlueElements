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
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using static BlueBasics.Converter;

#nullable enable

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    public partial class Formula : GenericControl, IBackgroundNone, IContextMenu {

        #region Fields

        private List<FlexiControlForCell>? _control;

        private Database? _database;

        private bool _inited;

        private long _savedRowKey = long.MinValue;

        private long _showingRowKey = -1;

        private long _tabGeneratorCount;
        private RowItem? _tmpShowingRow;

        #endregion

        #region Constructors

        public Formula() : base(false, false) => InitializeComponent();

        #endregion

        #region Events

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        public event EventHandler<RowEventArgs> ShowingRowChanged;

        #endregion

        #region Properties

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Database? Database {
            get => _database;
            set {
                if (_database == value) { return; }
                if (grpEditor.Visible) { grpEditor.Visible = false; }
                ShowingRowKey = -1; // Wichtig, dass ordenlich Showing-Row to Nothing gesetzt wird, weil dann alle Fokuse durch Enabled elemeniert werden und nachträglich nix mehr ausgelöst wird.
                Control_Remove_All();
                if (_database != null) {
                    _database.Loading -= _Database_StoreView;
                    _database.Loaded -= _DatabaseLoaded;
                    _database.Row.RowRemoving -= Row_RowRemoving;
                    _database.Column.ItemRemoved -= _Database_ColumnRemoved;
                    _database.Column.ItemInternalChanged -= _Database_ItemInternalChanged;
                    //_Database.RowKeyChanged -= _Database_RowKeyChanged;
                    _database.Disposing -= _Database_Disposing;
                    _database.Save(false); // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann
                }
                _database = value;
                if (_database != null) {
                    _database.Loading += _Database_StoreView;
                    _database.Loaded += _DatabaseLoaded;
                    _database.Row.RowRemoving += Row_RowRemoving;
                    _database.Column.ItemRemoved += _Database_ColumnRemoved;
                    _database.Column.ItemInternalChanged += _Database_ItemInternalChanged;
                    //_Database.RowKeyChanged += _Database_RowKeyChanged;
                    _database.Disposing += _Database_Disposing;
                }
                _inited = false;
            }
        }

        // <Obsolete("Database darf nicht im Designer gesetzt werden.", True)>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RowItem? ShowingRow {
            get {
                Develop.DebugPrint_Disposed(IsDisposed);
                return _tmpShowingRow;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long ShowingRowKey {
            get {
                Develop.DebugPrint_Disposed(IsDisposed);
                return _showingRowKey;
            }
            set {
                Develop.DebugPrint_Disposed(IsDisposed);
                if (value < 0) { value = -1; }
                if (grpEditor.Visible) { value = -1; }
                if (value == _showingRowKey) { return; }
                if (value > -1 && _database == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Database is nothing"); }
                if (!_inited) {
                    if (value < 0) { return; }
                    View_Init();
                }
                _showingRowKey = value;
                _tmpShowingRow = _database?.Row.SearchByKey(_showingRowKey);
                //Parallel.ForEach(_Control, thisFlex => {
                //    if (thisFlex != null && !thisFlex.IsDisposed) {
                //        thisFlex.RowKey = _ShowingRowKey;
                //        thisFlex.CheckEnabledState();
                //    }
                //});
                foreach (var thisFlex in _control) {
                    if (thisFlex != null && !thisFlex.IsDisposed) {
                        thisFlex.RowKey = _showingRowKey;
                        thisFlex.CheckEnabledState();
                    }
                }
                OnShowingRowChanged(new RowEventArgs(ShowingRow));
                ShowingRow?.DoAutomatic(false, false, "to be sure");
            }
        }

        #endregion

        #region Methods

        public static ColumnViewCollection? SearchColumnView(ColumnItem? column) {
            if (column == null) { return null; }
            foreach (var thisView in column.Database.Views) {
                if (thisView != null) {
                    if (thisView.Any(thisViewItem => thisViewItem?.Column != null && thisViewItem.Column == column)) {
                        return thisView;
                    }
                }
            }
            return null;
        }

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
            if (_database == null) { return true; }
            switch (e.ClickedComand.ToLower()) {
                case "#schnelleingabe":
                    if (ShowingRow == null) { return true; }
                    FormulaQuickSelect sh = new(ShowingRow);
                    sh.ShowDialog();
                    return true;

                case "#ansicht":
                    ShowViewEditor();
                    return true;
            }
            return false;
        }

        public void Control_Remove_All() {
            if (InvokeRequired) {
                Invoke(new Action(Control_Remove_All));
                return;
            }
            bool r;
            do {
                r = false;
                foreach (System.Windows.Forms.Control o in Controls) {
                    if (o is GroupBox g && g == grpEditor) {
                    } else if (o is TabControl t && t == Tabs) {
                        if (t.TabCount > 0) {
                            RemoveControl(o);
                            r = true;
                        }
                    } else {
                        RemoveControl(o);
                        r = true;
                    }
                }
            } while (r);
            _control?.Clear();
        }

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs? e, ItemCollectionList? items, out object? hotItem, List<string> tags, ref bool cancel, ref bool translate) {
            hotItem = null;
            if (_database == null) {
                cancel = true;
                return;
            }
            items.Add("Allgemeine Schnelleingabe öffnen", "#Schnelleingabe", QuickImage.Get(enImageCode.Lupe), _database != null && ShowingRow != null);
            if (_database.IsAdministrator()) {
                items.AddSeparator();
                items.Add("Formular bearbeiten", "#Ansicht", QuickImage.Get(enImageCode.Textfeld), _database != null);
            }
        }

        public void HideViewEditor() {
            if (!grpEditor.Visible) { return; }
            grpEditor.Visible = false;
            View_Init();
        }

        public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    _showingRowKey = -1;
                    _tmpShowingRow = null;
                    Database = null; // Wichtig,  (nicht _Database) um events zu lösen.
                    //components?.Dispose();
                }
            } finally {
                base.Dispose(disposing);
            }
        }

        protected override void DrawControl(Graphics gr, enStates state) => Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);

        protected override void OnSizeChanged(System.EventArgs e) {
            if (IsDisposed) { return; }
            base.OnSizeChanged(e);
            if (_database != null && _inited) {
                Control_RepairSize_All();
            }
            grpEditor.Left = Width - grpEditor.Width;
            grpEditor.Height = Height;
        }

        private static ColumnViewItem? SearchViewItem(ColumnItem? column) {
            var thisView = SearchColumnView(column);
            return thisView?[column];
        }

        /// <summary>
        /// Ermittelt, wie viele Spalten die Ansicht benutzt.
        /// </summary>
        /// <param name="viewCollection"></param>
        /// <returns></returns>
        private static int View_AnzahlSpalten(ColumnViewCollection viewCollection) {
            var maxS = 0;
            foreach (var thisViewItem in viewCollection) {
                if (thisViewItem != null) {
                    maxS = Math.Max(maxS, thisViewItem.Spalte_X1 + thisViewItem.Width - 1); // Spalte_X2
                }
            }
            return maxS;
        }

        private void _Database_ColumnRemoved(object sender, System.EventArgs e) {
            if (IsDisposed) { return; }
            View_Init();
        }

        private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;

        private void _Database_ItemInternalChanged(object sender, ListEventArgs e) {
            if (IsDisposed) { return; }
            var r = _showingRowKey;
            ShowingRowKey = -1;
            View_Init();
            ShowingRowKey = r;
        }

        //private void _Database_RowKeyChanged(object sender, KeyChangedEventArgs e) {
        //    // Ist aktuell nur möglich,wenn Pending Changes eine neue Zeile machen
        //    // Jedes FlexControl beachtet für sich die Änderung
        //    if (e.KeyOld == _savedRowKey) { _savedRowKey = e.KeyNew; }
        //}

        //private void _Database_RowChecked(object sender, RowCheckedEventArgs e) {
        //if (e.Row.Key != _ShowingRowKey) { return; }
        //var nr = int.MaxValue;
        //var ColNr = int.MaxValue;
        //for (var cc = 0; cc < e.ColumnsWithErrors.Count; cc++) {
        //    var p = e.ColumnsWithErrors[cc].Split('|');
        //    foreach (var ThisColumnItem in e.Row.Database.Column) {
        //        if (ThisColumnItem != null) {
        //            if (p[0].ToUpper() == ThisColumnItem.Name.ToUpper()) {
        //                //Bitte jeden Fehler anzeigen..... Es verursacht mehr Rätsel, wenn die Zeile einfach Fehlerhaft ist und überhaut kein Hinweis kommt
        //                var CD = SearchViewItem(ThisColumnItem);
        //                var View = SearchColumnView(ThisColumnItem);
        //                var tmp = CD == null
        //                    ? ThisColumnItem.Index() + 200000
        //                    : View == CurrentView() ? ThisColumnItem.Index() : ThisColumnItem.Index() + 100000;
        //                if (tmp < ColNr) {
        //                    ColNr = tmp;
        //                    nr = cc;
        //                }
        //            }
        //        }
        //    }
        //}
        //if (nr < int.MaxValue) {
        //    _ = e.ColumnsWithErrors[nr].Split('|');
        //}
        //}
        private void _Database_StoreView(object sender, LoadingEventArgs e) {
            if (e.OnlyReload) { return; }
            _savedRowKey = ShowingRowKey;
        }

        private void _DatabaseLoaded(object sender, LoadedEventArgs e) {
            if (InvokeRequired) {
                Invoke(new Action(() => _DatabaseLoaded(sender, e)));
                return;
            }
            if (IsDisposed) { return; }
            _inited = false;
            ShowingRowKey = _savedRowKey;
        }

        private void Arrangement_Swap(int ri) {
            var vn = _database.Views.IndexOf(CurrentView());
            if (vn < 1 || vn >= _database.Views.Count) { return; }
            if (ri is 0 or > 1 or < -1) { return; }
            if (vn < 2 && ri < 0) { return; }
            if (vn >= _database.Views.Count - 1 && ri > 0) { return; }

            var tmpx1 = _database.Views[vn];
            var tmpx2 = _database.Views[vn + ri];
            // Binding List Changes müsste reagieren
            _database.Views[vn] = tmpx2;
            _database.Views[vn + ri] = tmpx1;
            RedoView();
        }

        private void Btb_IAmInvalid(object sender, System.EventArgs e) {
            _inited = false;
            Control_Remove_All();
        }

        private void btnAnsichtHinzufuegen_Click(object sender, System.EventArgs e) {
            var ex = InputBox.Show("Geben sie den Namen<br>der neuen Ansicht ein:", "", enVarType.Text);
            if (string.IsNullOrEmpty(ex)) { return; }
            _database.Views.Add(new ColumnViewCollection(_database, "", ex));
            RedoView();
            SortColumnList();
        }

        private void btnAnsichtloeschen_Click(object sender, System.EventArgs e) {
            var currView = CurrentView();
            if (currView == null) { return; }
            var i = _database.Views.IndexOf(currView);
            if (i < 1) { return; } // 0 darf auch nicht gelöscht werden
            if (MessageBox.Show("Ansicht <b>'" + currView.Name + "'</b><br>wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _database.Views.RemoveAt(i);
            RedoView();
            SortColumnList();
        }

        private void btnExitEditor_Click(object sender, System.EventArgs e) {
            if (!grpEditor.Visible) { return; }
            grpEditor.Visible = false;
            View_Init();
        }

        private void btnGroesseLinks_Click(object sender, System.EventArgs e) {
            var viewItem = SearchViewItem(EditorSelectedColumn());
            viewItem?.KoordÄndern(0, -1, 0);
            RedoView();
            SortColumnList();
        }

        private void btnGroesseOben_Click(object sender, System.EventArgs e) {
            var viewItem = SearchViewItem(EditorSelectedColumn());
            viewItem?.KoordÄndern(0, 0, -1);
            RedoView();
            SortColumnList();
        }

        private void btnGroesseRechts_Click(object sender, System.EventArgs e) {
            var viewItem = SearchViewItem(EditorSelectedColumn());
            viewItem?.KoordÄndern(0, 1, 0);
            RedoView();
            SortColumnList();
        }

        private void btnGroesseUnten_Click(object sender, System.EventArgs e) {
            var viewItem = SearchViewItem(EditorSelectedColumn());
            viewItem?.KoordÄndern(0, 0, 1);
            RedoView();
            SortColumnList();
        }

        private void btnPositionLinks_Click(object sender, System.EventArgs e) {
            var viewItem = SearchViewItem(EditorSelectedColumn());
            viewItem?.KoordÄndern(-1, 0, 0);
            RedoView();
            SortColumnList();
        }

        private void btnPositionOben_Click(object sender, System.EventArgs e) {
            var column = EditorSelectedColumn();
            var view = SearchColumnView(column);
            var viewItem = SearchViewItem(column);
            if (viewItem != null) { view.Swap(viewItem, viewItem.PreviewsVisible(view)); }
            RedoView();
            SortColumnList();
        }

        private void btnPositionRechts_Click(object sender, System.EventArgs e) {
            var viewItem = SearchViewItem(EditorSelectedColumn());
            viewItem?.KoordÄndern(1, 0, 0);
            RedoView();
            SortColumnList();
        }

        private void btnRechteFuerAnsicht_Click(object sender, System.EventArgs e) {
            var currView = CurrentView();
            ItemCollectionList aa = new();
            aa.AddRange(_database.Permission_AllUsed(false));
            aa.CheckBehavior = enCheckBehavior.MultiSelection;
            aa.Check(currView.PermissionGroups_Show, true);
            if (_database.Views.Count > 1 && currView == _database.Views[1]) {
                aa["#Everybody"].Enabled = false;
                aa["#Everybody"].Checked = true;
            }
            aa.Sort();
            var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, enAddType.Text, true);
            if (b == null) { return; }
            currView.PermissionGroups_Show.Clear();
            currView.PermissionGroups_Show.AddRange(b.ToArray());
            currView.PermissionGroups_Show.RemoveString("#Administrator", false);
            if (currView == _database.Views[1]) { currView.PermissionGroups_Show.Add("#Everybody"); }
        }

        private void btnReiterNachLinks_Click(object sender, System.EventArgs e) => Arrangement_Swap(-1);

        private void btnReiterNachRechts_Click(object sender, System.EventArgs e) => Arrangement_Swap(1);

        private void btnRename_Click(object sender, System.EventArgs e) {
            var currView = CurrentView();
            if (currView == null || currView == _database.Views[0]) { return; }
            var n = InputBox.Show("Umbenennen:", currView.Name, enVarType.Text);
            if (!string.IsNullOrEmpty(n)) { currView.Name = n; }
            RedoView();
            SortColumnList();
        }

        private void cbxCaptionPosition_ItemClicked(object sender, BasicListItemEventArgs e) {
            var viewItem = SearchViewItem(EditorSelectedColumn());
            if (viewItem != null) { viewItem.ÜberschriftAnordnung = (enÜberschriftAnordnung)int.Parse(cbxCaptionPosition.Text); }
            RedoView();
            SortColumnList();
        }

        private void cbxControlType_ItemClicked(object sender, BasicListItemEventArgs e) {
            var viewItem = SearchViewItem(EditorSelectedColumn());
            if (viewItem != null) { viewItem.Column.EditType = (enEditTypeFormula)int.Parse(cbxControlType.Text); }
            RedoView();
            SortColumnList();
        }

        private void Control_Create(ColumnViewItem cd, System.Windows.Forms.Control vParent) {
            if (cd?.Column == null) { return; }
            Develop.Debugprint_BackgroundThread();
            FlexiControlForCell btb = new(cd.Column.Database, cd.Column.Key, cd.ÜberschriftAnordnung) {
                TabIndex = TabIndex + 10000,
                Tag = cd
            };
            btb.NeedRefresh += Btb_IAmInvalid;
            vParent.Controls.Add(btb);
            _control.Add(btb);
        }

        private void Control_Create_All() {
            var count = -1;
            _control = new List<FlexiControlForCell?>();
            foreach (var thisView in _database.Views) {
                if (thisView != null) {
                    var index = _database.Views.IndexOf(thisView);
                    count++;
                    foreach (var thisViewItem in thisView) {
                        if (thisViewItem?.Column != null) {
                            if (index == 0) {
                                Control_Create(thisViewItem, this);
                            } else {
                                Control_Create(thisViewItem, Tabs.TabPages[count - 1]);
                            }
                        }
                    }
                }
            }
        }

        private void Control_RepairSize_All() {
            var count = -1;
            foreach (var thisView in _database.Views) {
                count++;
                var viewSpalten = View_AnzahlSpalten(thisView);
                var belegterBereichTop = new int[viewSpalten + 1];
                var viewN = _database.Views.IndexOf(thisView);
                int widthInPixelOfParent;
                int heightOfParent;
                int moveIn;
                if (viewN == 0) {
                    widthInPixelOfParent = Width - 4;
                    heightOfParent = Height;
                    if (grpEditor.Visible) { widthInPixelOfParent = Width - grpEditor.Width - 8; }
                    moveIn = 0;
                } else {
                    heightOfParent = Tabs.Height - Tabs.TabPages[count - 1].Top;
                    widthInPixelOfParent = Tabs.Width - 10 - (Skin.PaddingSmal * 4);
                    moveIn = Skin.PaddingSmal * 2;
                }
                var widthInPixelOfColumn = (widthInPixelOfParent - (viewSpalten * Skin.PaddingSmal)) / (viewSpalten + 1);
                foreach (var thisViewItem in thisView) {
                    if (thisViewItem?.Column != null) {
                        Rectangle objPx = new() {
                            Width = (thisViewItem.Width * widthInPixelOfColumn) + ((thisViewItem.Width - 1) * Skin.PaddingSmal),
                            X = (thisViewItem.Spalte_X1 * widthInPixelOfColumn) + (thisViewItem.Spalte_X1 * Skin.PaddingSmal) + moveIn,
                            Y = moveIn
                        }; // Die Koordinaten in Pixel des Steuerelements
                        for (var z = thisViewItem.Spalte_X1; z < thisViewItem.Spalte_X1 + thisViewItem.Width; z++) // Spalte_X2
                        {
                            objPx.Y = Math.Max(belegterBereichTop[z], objPx.Y);
                        }
                        if (objPx.Y > 0) { objPx.Y += 8; }
                        objPx.Height = (Math.Max(thisViewItem.Height, 0) * 16) + 8;
                        if (thisViewItem.ÜberschriftAnordnung is enÜberschriftAnordnung.Ohne_mit_Abstand or enÜberschriftAnordnung.Über_dem_Feld) {
                            objPx.Height += Skin.PaddingSmal + 16;
                        }
                        if (thisViewItem.Height == 31) {
                            objPx.Height = heightOfParent - objPx.Y - (Skin.PaddingSmal * 3);
                            if (objPx.Height < 16) { objPx.Height = 16; }
                        }
                        for (var z = thisViewItem.Spalte_X1; z < thisViewItem.Spalte_X1 + thisViewItem.Width; z++) // Spalte_X2
                        {
                            belegterBereichTop[z] = Math.Max(belegterBereichTop[z], objPx.Bottom);
                        }
                        GenericControl? conVi = ControlOf(thisViewItem);
                        if (conVi != null) {
                            conVi.Location = new Point(objPx.X, objPx.Y);
                            conVi.Size = new Size(objPx.Width, objPx.Height);
                            conVi.Visible = true;
                            conVi.BringToFront();
                        }
                    }
                }
                if (viewN == 0) { SetButtonsToPosition(belegterBereichTop[0] + 3); }
            }
        }

        //private void Tabs_SelectedIndexChanged(object sender, System.EventArgs e)
        //{
        //    if (Editor.Visible) { ColumnsEinfärben(); }
        //}
        private FlexiControlForCell? ControlOf(ColumnViewItem thisViewItem) => _control.FirstOrDefault(thisControl => thisControl != null && !thisControl.IsDisposed && thisControl.Tag == thisViewItem);

        private void Controls_SetCorrectEnabledState_All() {
            Develop.DebugPrint_Disposed(IsDisposed);
            foreach (var thisControl in _control) {
                if (thisControl != null && !thisControl.IsDisposed) { thisControl.CheckEnabledState(); }
            }
        }

        private ColumnViewCollection? CurrentView() {
            if (_database.Views.Count == 0) { return null; }
            if (Tabs.SelectedIndex + 1 > _database.Views.Count - 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Index zu hoch"); }
            if (_database.Views[Tabs.SelectedIndex + 1] == null) { Develop.DebugPrint(enFehlerArt.Fehler, "View ist Nothing"); }
            return _database.Views[Tabs.SelectedIndex + 1];
        }

        private void Editor_CheckButtons(bool blinki) {
            if (!grpEditor.Visible) { return; }
            ColumnViewItem? viewItem = null;
            if (lbxColumns.Item.Checked().Count == 1) {
                viewItem = SearchViewItem(_database.Column.SearchByKey(LongParse(lbxColumns.Item.Checked()[0].Internal)));
            }
            grpPosition.Enabled = viewItem != null;
            grpGroesse.Enabled = viewItem != null;
            cbxCaptionPosition.Enabled = viewItem != null;
            cbxControlType.Enabled = viewItem != null;
            if (viewItem != null) {
                cbxCaptionPosition.Text = ((int)viewItem.ÜberschriftAnordnung).ToString();
                if (viewItem.Column != null) {
                    if (blinki) {
                        cbxControlType.Item.Clear();
                        for (var z = 0; z <= 999; z++) {
                            var w = (enEditTypeFormula)z;
                            if (w.ToString() != z.ToString()) {
                                if (viewItem.Column.UserEditDialogTypeInFormula(w)) {
                                    cbxControlType.Item.Add(w.ToString(), z.ToString());
                                }
                            }
                        }
                    }
                    cbxControlType.Text = ((int)viewItem.Column.EditType).ToString();
                } else {
                    cbxControlType.Enabled = false;
                }
            }
        }

        private ColumnItem? EditorSelectedColumn() => lbxColumns.Item.Checked().Count != 1 ? null : _database.Column.SearchByKey(LongParse(lbxColumns.Item.Checked()[0].Internal));

        private void Generate_Tabs() {
            if (_database.Views.Count < 1) { return; }
            foreach (var thisView in _database.Views) {
                if (thisView != null && thisView != _database.Views[0]) {
                    _tabGeneratorCount++;
                    System.Windows.Forms.TabPage tempPage = new() {
                        Text = "Seite #" + _tabGeneratorCount
                    };
                    tempPage.Text = thisView.Name;
                    tempPage.Enabled = _database.PermissionCheck(thisView.PermissionGroups_Show, null);
                    Tabs.TabPages.Add(tempPage);
                    tempPage.MouseUp += Tabs_MouseUp;
                }
            }
        }

        private void lbxColumns_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
            var item = (BasicListItem)e.HotItem;
            if (item == null) { return; }
            item.Checked = true;
            var cd = SearchViewItem(_database.Column.SearchByKey(LongParse(item.Internal)));
            if (cd == null) {
                e.UserMenu.Add("Zum Kopfbereich hinzufügen", "#AddColumnToHead", enImageCode.Sonne);
                e.UserMenu.Add("Zum Körperbereich hinzufügen", "#AddColumnToBody", enImageCode.Kreis, _database.Views.Count > 1);
            } else {
                e.UserMenu.Add("Dieses Feld ausblenden", "#RemoveColumnFromView", enImageCode.Kreuz, Convert.ToBoolean(cd != null));
            }
        }

        private void lbxColumns_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            var column = _database.Column.SearchByKey(LongParse(((BasicListItem)e.HotItem).Internal));

            var viewItem = SearchViewItem(column);
            var currView = CurrentView();
            switch (e.ClickedComand) {
                case "#RemoveColumnFromView":
                    if (viewItem != null) {
                        _database.Views[0]?.Remove(viewItem);
                        currView?.Remove(viewItem);
                    }
                    break;

                case "#AddColumnToHead":
                    if (_database.Views.Count == 0) {
                        _database.Views.Add(new ColumnViewCollection(_database, string.Empty, "##Head###"));
                    }
                    _database.Views[0].Add(column, false);
                    break;

                case "#AddColumnToBody":
                    currView?.Add(column, false);
                    break;
            }
            RedoView();
            SortColumnList();
        }

        private void lbxColumns_ItemCheckedChanged(object sender, System.EventArgs e) => Editor_CheckButtons(true);

        private void OnShowingRowChanged(RowEventArgs e) => ShowingRowChanged?.Invoke(this, e);

        private void RedoView() {
            var i = Tabs.SelectedIndex;
            _inited = false;
            View_Init();
            Editor_CheckButtons(true);
            if (i < 0) { i = 0; }
            if (i > Tabs.TabPages.Count - 1) { i = Tabs.TabPages.Count - 1; }
            if (i >= 0) { Tabs.SelectedIndex = i; }
        }

        private void RemoveControl(System.Windows.Forms.Control vObject) {
            if (vObject == null || vObject.IsDisposed) { return; }
            switch (vObject) {
                case AbstractTabControl:
                    foreach (System.Windows.Forms.Control o in vObject.Controls) {
                        RemoveControl(o);
                    }
                    return; // Raus hier

                case System.Windows.Forms.TabPage:
                    foreach (System.Windows.Forms.Control o in vObject.Controls) {
                        RemoveControl(o);
                    }
                    break;

                default:
                    var tempVar = (FlexiControlForCell)vObject;
                    tempVar.NeedRefresh -= Btb_IAmInvalid;
                    break;
            }
            vObject.Parent.Controls.Remove(vObject);
            vObject.Dispose();
        }

        private void Row_RowRemoving(object sender, RowEventArgs e) {
            if (_showingRowKey == e.Row.Key) {
                ShowingRowKey = -1;
            }
        }

        private void SetButtonsToPosition(int topPos) {
            var sph = 1;
            var spf = 1;
            var widthOfParent = Width - 4;
            if (grpEditor.Visible) {
                widthOfParent = Width - grpEditor.Width - 8;
            }
            foreach (var thisView in _database.Views) {
                if (_database.Views.IndexOf(thisView) == 0) {
                    sph = Math.Max(sph, View_AnzahlSpalten(thisView));
                } else {
                    spf = Math.Max(spf, View_AnzahlSpalten(thisView));
                }
            }
            Tabs.Top = topPos;
            Tabs.Left = 0;
            if (spf >= sph || spf == 0) {
                Tabs.Width = widthOfParent;
            } else {
                var widthOfColum = (widthOfParent - (sph * Skin.PaddingSmal)) / (sph + 1);
                Tabs.Width = (widthOfColum * (spf + 1)) + (spf * Skin.PaddingSmal);
            }
            Tabs.Height = Height - topPos;
        }

        private void ShowViewEditor() {
            if (_database == null || grpEditor.Visible || !_database.IsAdministrator()) { return; }

            _database.Load_Reload();
            _database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

            if (!_inited) { View_Init(); }

            ShowingRowKey = -1;

            lbxColumns.Item.Clear();
            lbxColumns.Item.AddRange(_database.Column, false);
            lbxColumns.Item.Sort();

            cbxCaptionPosition.Item.AddRange(typeof(enÜberschriftAnordnung));
            cbxControlType.Item.Clear();
            Editor_CheckButtons(false);
            grpEditor.Visible = true;

            View_Init();
            SortColumnList();
        }

        private void SortColumnList() {

            #region Zuerst die Überschriften (Namen der Views) erstellen oder löschen

            for (var viewNo = -1; viewNo < _database.Views.Count; viewNo++) {
                string? nam;
                switch (viewNo) {
                    case -1:
                        nam = "Unbenutzt";
                        break;

                    case 0:
                        nam = "Kopfbereich";
                        break;

                    default:
                        nam = _database.Views[viewNo] == null ? string.Empty : _database.Views[viewNo].Name + " "; // Leerzeichen wegen evtl. leeren namen
                        break;
                }

                var intName = "@Ansicht" + viewNo;
                if (string.IsNullOrEmpty(nam)) {
                    if (lbxColumns.Item[intName] != null) { lbxColumns.Item.Remove(intName); }
                } else {
                    if (lbxColumns.Item[intName] == null) { lbxColumns.Item.Add(nam, intName, true, (viewNo + 1).ToString(Constants.Format_Integer3) + "|A"); }
                }
            }

            #endregion

            #region Dann die Spalten den Überschriften zuordnen

            var columNo = -1;
            do {
                if (lbxColumns.Item.RemoveNull()) { columNo = -1; }
                columNo++;
                if (columNo > lbxColumns.Item.Count - 1) { break; }

                if (lbxColumns.Item[columNo] is TextListItem thisItem && thisItem.IsClickable()) {
                    var co = (ColumnItem)thisItem.Tag;
                    if (co == null) {
                        lbxColumns.Item.Remove(thisItem);
                    } else {
                        var cv = SearchColumnView(co);
                        var sort = cv == null ? 0 : _database.Views.IndexOf(cv) + 1;
                        thisItem.UserDefCompareKey = sort.ToString(Constants.Format_Integer3) + "|Z" + thisItem.CompareKey();
                    }
                }
            } while (true);

            #endregion

            lbxColumns.Item.Sort();
            lbxColumns.Invalidate();
        }

        private void SUnten_Click(object sender, System.EventArgs e) {
            var column = EditorSelectedColumn();
            var view = SearchColumnView(column);
            var viewItem = SearchViewItem(column);
            if (viewItem != null) { view.Swap(viewItem, viewItem.NextVisible(view)); }
            RedoView();
            SortColumnList();
        }

        private void Tabs_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) { FloatingInputBoxListBoxStyle.ContextMenuShow(this, e); }
        }

        private void View_Init() {
            if (_database == null) { return; }
            if (Parent == null) { return; } // Irgend ein Formular reagiert nioch?!?
            _inited = false;
            Control_Remove_All();
            foreach (var thisTabpage in Tabs.TabPages) {
                if (thisTabpage is System.Windows.Forms.TabPage tp) {
                    tp.MouseUp -= Tabs_MouseUp;
                    tp.Dispose();
                    Tabs.TabPages.Remove(tp);
                }
            }
            Tabs.TabPages.Clear();
            Generate_Tabs();
            Control_Create_All();
            Control_RepairSize_All();
            Controls_SetCorrectEnabledState_All();
            Tabs.Visible = _database.Views.Count > 0;
            _inited = true;
            if (Tabs.TabPages.Count >= 0) { Tabs.SelectedIndex = 0; }
        }

        #endregion
    }
}