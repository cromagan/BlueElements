#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
using BlueBasics.EventArgs;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls
{
    [Designer(typeof(BasicDesigner))]
    public partial class Formula : GenericControl, IBackgroundNone, IContextMenu
    {
        #region Constructor
        public Formula() : base(false, false)
        {
            InitializeComponent();
        }

        #endregion


        private Database _Database;


        private RowItem _tmpShowingRow = null;
        private int _ShowingRowKey = -1;
        private bool _Inited;


        public event EventHandler<RowEventArgs> ShowingRowChanged;
        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        private List<FlexiControlForCell> _Control;


        private int _savedRowKey = int.MinValue;

        private long TabGeneratorCount;

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    _ShowingRowKey = -1;
                    _tmpShowingRow = null;
                    Database = null; // Wichtig,  (nicht _Database) um events zu lösen.
                    //components?.Dispose();

                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }



        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Database Database
        {
            get
            {
                return _Database;
            }
            set
            {
                if (_Database == value) { return; }
                BeginnEdit();

                if (Editor.Visible) { Editor.Visible = false; }
                ShowingRowKey = -1; // Wichtig, dass ordenlich Showing-Row to Nothing gesetzt wird, weil dann alle Fokuse durch Enabled elemeniert werden und nachträglich nix mehr ausgelöst wird.
                Control_Remove_All();

                if (_Database != null)
                {
                    _Database.Loading -= _Database_StoreView;
                    _Database.Loaded -= _DatabaseLoaded;
                    _Database.Row.RowChecked -= _Database_RowChecked;
                    _Database.Column.ItemRemoved -= _Database_ColumnRemoved;
                    _Database.Column.ItemInternalChanged -= _Database_ColumnContentChanged;
                    //_Database.StoreView -= _Database_StoreView;
                    _Database.RowKeyChanged -= _Database_RowKeyChanged;
                    //_Database.RestoreView -= _Database_RestoreView;

                    _Database.Save(false); // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann

                }
                _Database = value;

                if (_Database != null)
                {
                    _Database.Loading += _Database_StoreView;
                    _Database.Loaded += _DatabaseLoaded;
                    _Database.Row.RowChecked += _Database_RowChecked;
                    _Database.Column.ItemRemoved += _Database_ColumnRemoved;
                    _Database.Column.ItemInternalChanged += _Database_ColumnContentChanged;
                    //_Database.StoreView += _Database_StoreView;
                    _Database.RowKeyChanged += _Database_RowKeyChanged;
                    //_Database.RestoreView += _Database_RestoreView;
                }

                _Inited = false;

                EndEdit();
            }
        }


        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ShowingRowKey
        {
            get
            {
                Develop.DebugPrint_Disposed(IsDisposed);
                return _ShowingRowKey;
            }
            set
            {
                Develop.DebugPrint_Disposed(IsDisposed);
                if (value < 0) { value = -1; }
                if (Editor.Visible) { value = -1; }
                if (value == _ShowingRowKey) { return; }

                if (value > -1 && _Database == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Database is nothing"); }


                if (!_Inited)
                {
                    if (value < 0) { return; }
                    View_Init();
                }


                BeginnEdit();


                _ShowingRowKey = value;
                _tmpShowingRow = _Database?.Row.SearchByKey(_ShowingRowKey);

                foreach (var thisFlex in _Control)
                {
                    if (thisFlex != null && !thisFlex.IsDisposed)
                    {
                        thisFlex.RowKey = _ShowingRowKey;
                        thisFlex.CheckEnabledState();
                    }
                }

                OnShowingRowChanged(new RowEventArgs(ShowingRow));

                ShowingRow?.DoAutomatic(false, false);

                EndEdit();
            }
        }

        private void OnShowingRowChanged(RowEventArgs e)
        {
            ShowingRowChanged?.Invoke(this, e);
        }






        // <Obsolete("Database darf nicht im Designer gesetzt werden.", True)>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RowItem ShowingRow
        {
            get
            {
                Develop.DebugPrint_Disposed(IsDisposed);
                return _tmpShowingRow;
            }
        }




        private void View_Init()
        {
            if (_Database == null) { return; }
            if (Parent == null) { return; } // Irgend ein Formular reagiert nioch?!?

            BeginnEdit();
            _Inited = false;

            Control_Remove_All();

            foreach (TabPage ThisTabpage in Tabs.TabPages)
            {
                ThisTabpage.MouseUp -= Tabs_MouseUp;
                ThisTabpage.Dispose();
                Tabs.TabPages.Remove(ThisTabpage);
            }

            Tabs.TabPages.Clear();

            Generate_Tabs();
            Control_Create_All();

            Control_RepairSize_All();
            Controls_SetCorrectEnabledState_All();

            Tabs.Visible = _Database.Views.Count > 0;

            _Inited = true;

            if (Tabs.TabPages.Count >= 0) { Tabs.SelectedIndex = 0; }

            EndEdit();

        }


        private void _DatabaseLoaded(object sender, LoadedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => _DatabaseLoaded(sender, e)));
                return;
            }
            if (IsDisposed) { return; }
            _Inited = false;
            _Database?.LoadPicsIntoImageChache();
            ShowingRowKey = _savedRowKey;
        }



        private void _Database_RowChecked(object sender, RowCheckedEventArgs e)
        {
            if (e.Row.Key != _ShowingRowKey) { return; }



            string[] p = null;
            var nr = int.MaxValue;
            var ColNr = int.MaxValue;



            for (var cc = 0; cc < e.ColumnsWithErrors.Count; cc++)
            {
                p = e.ColumnsWithErrors[cc].Split('|');



                foreach (var ThisColumnItem in e.Row.Database.Column)
                {
                    if (ThisColumnItem != null)
                    {


                        if (p[0].ToUpper() == ThisColumnItem.Name.ToUpper())
                        {

                            //Bitte jeden Fehler anzeigen..... Es verursacht mehr Rätsel, wenn die Zeile einfach Fehlerhaft ist und überhaut kein Hinweis kommt
                            var CD = SearchViewItem(ThisColumnItem);
                            var View = SearchColumnView(ThisColumnItem);

                            var tmp = 0;
                            if (CD == null)
                            {
                                tmp = ThisColumnItem.Index() + 200000;
                            }
                            else if (View == CurrentView())
                            {
                                tmp = ThisColumnItem.Index();
                            }
                            else
                            {
                                tmp = ThisColumnItem.Index() + 100000;
                            }

                            if (tmp < ColNr)
                            {
                                ColNr = tmp;
                                nr = cc;
                            }

                        }
                    }
                }
            }

            if (nr < int.MaxValue)
            {
                _ = e.ColumnsWithErrors[nr].Split('|');
            }

        }


        /// <summary>
        /// Ermittelt, wie viele Spalten die Ansicht benutzt.
        /// </summary>
        /// <param name="ViewCollection"></param>
        /// <returns></returns>
        private int View_AnzahlSpalten(ColumnViewCollection ViewCollection)
        {
            var MaxS = 0;

            foreach (var ThisViewItem in ViewCollection)
            {
                if (ThisViewItem != null)
                {
                    MaxS = Math.Max(MaxS, ThisViewItem.Spalte_X1 + ThisViewItem.Width - 1); // Spalte_X2
                }
            }

            return MaxS;
        }

        private void Control_RepairSize_All()
        {
            var Count = -1;
            BeginnEdit();

            foreach (var ThisView in _Database.Views)
            {
                Count++;

                var View_Spalten = View_AnzahlSpalten(ThisView);
                var _BelegterBereichTop = new int[View_Spalten + 1];

                var ViewN = _Database.Views.IndexOf(ThisView);


                var WidthInPixelOfParent = 0;
                var HeightOfParent = 0;
                var MoveIn = 0;
                if (ViewN == 0)
                {
                    WidthInPixelOfParent = Width - 4;
                    HeightOfParent = Height;
                    if (Editor.Visible) { WidthInPixelOfParent = Width - Editor.Width - 8; }
                    MoveIn = 0;
                }
                else
                {
                    HeightOfParent = Tabs.Height - Tabs.TabPages[Count - 1].Top;
                    WidthInPixelOfParent = Tabs.Width - 10 - Skin.PaddingSmal * 4;
                    MoveIn = Skin.PaddingSmal * 2;
                }

                var WidthInPixelOfColumn = (WidthInPixelOfParent - View_Spalten * Skin.PaddingSmal) / (View_Spalten + 1);


                foreach (var ThisViewItem in ThisView)
                {

                    if (ThisViewItem?.Column != null)
                    {

                        var ObjPX = new Rectangle
                        {
                            Width = ThisViewItem.Width * WidthInPixelOfColumn + (ThisViewItem.Width - 1) * Skin.PaddingSmal,
                            X = ThisViewItem.Spalte_X1 * WidthInPixelOfColumn + ThisViewItem.Spalte_X1 * Skin.PaddingSmal + MoveIn,

                            Y = MoveIn
                        }; // Die Koordinaten in Pixel des Steuerelements
                        for (var z = ThisViewItem.Spalte_X1; z < ThisViewItem.Spalte_X1 + ThisViewItem.Width; z++) // Spalte_X2
                        {
                            ObjPX.Y = Math.Max(_BelegterBereichTop[z], ObjPX.Y);
                        }



                        if (ObjPX.Y > 0) { ObjPX.Y += 8; }
                        ObjPX.Height = Math.Max(ThisViewItem.Height, 0) * 16 + 8;

                        if (ThisViewItem.ÜberschriftAnordnung == enÜberschriftAnordnung.Ohne_mit_Abstand || ThisViewItem.ÜberschriftAnordnung == enÜberschriftAnordnung.Über_dem_Feld)
                        {
                            ObjPX.Height += Skin.PaddingSmal + 16;

                        }



                        if (ThisViewItem.Height == 31)
                        {
                            ObjPX.Height = HeightOfParent - ObjPX.Y - Skin.PaddingSmal * 3;
                            if (ObjPX.Height < 16) { ObjPX.Height = 16; }
                        }


                        for (var z = ThisViewItem.Spalte_X1; z < ThisViewItem.Spalte_X1 + ThisViewItem.Width; z++) // Spalte_X2
                        {
                            _BelegterBereichTop[z] = Math.Max(_BelegterBereichTop[z], ObjPX.Bottom);
                        }


                        GenericControl ConVI = ControlOf(ThisViewItem);

                        if (ConVI != null)
                        {
                            ConVI.Location = new Point(ObjPX.X, ObjPX.Y);
                            ConVI.Size = new Size(ObjPX.Width, ObjPX.Height);
                            ConVI.Visible = true;
                            ConVI.BringToFront();
                        }

                    }
                }

                if (ViewN == 0) { SetButtonsToPosition(_BelegterBereichTop[0] + 3); }
            }

            EndEdit();
        }



        private void Control_Create(ColumnViewItem cd, System.Windows.Forms.Control vParent)
        {
            if (cd?.Column == null) { return; }

            Develop.Debugprint_BackgroundThread();
            var btb = new FlexiControlForCell(cd.Column.Database, cd.Column.Key, cd.ÜberschriftAnordnung)
            {
                TabIndex = TabIndex + 10000,
                Tag = cd
            };
            btb.NeedRefresh += Btb_IAmInvalid;

            vParent.Controls.Add(btb);
            _Control.Add(btb);
        }

        private void Btb_IAmInvalid(object sender, System.EventArgs e)
        {
            _Inited = false;
            Control_Remove_All();
        }

        private void SetButtonsToPosition(int TopPos)
        {
            var SPH = 1;
            var SPF = 1;
            var WidthOfParent = Width - 4;
            if (Editor.Visible)
            {
                WidthOfParent = Width - Editor.Width - 8;
            }

            foreach (var ThisView in _Database.Views)
            {
                if (_Database.Views.IndexOf(ThisView) == 0)
                {
                    SPH = Math.Max(SPH, View_AnzahlSpalten(ThisView));
                }
                else
                {
                    SPF = Math.Max(SPF, View_AnzahlSpalten(ThisView));
                }
            }

            Tabs.Top = TopPos;
            Tabs.Left = 0;

            if (SPF >= SPH || SPF == 0)
            {
                Tabs.Width = WidthOfParent;
            }
            else
            {
                var WidthOfColum = (WidthOfParent - SPH * Skin.PaddingSmal) / (SPH + 1);
                Tabs.Width = WidthOfColum * (SPF + 1) + SPF * Skin.PaddingSmal;
            }

            Tabs.Height = Height - TopPos;
        }


        private void Control_Create_All()
        {
            var Count = -1;

            BeginnEdit();

            _Control = new List<FlexiControlForCell>();

            foreach (var ThisView in _Database.Views)
            {
                if (ThisView != null)
                {
                    var Index = _Database.Views.IndexOf(ThisView);
                    Count++;

                    foreach (var ThisViewItem in ThisView)
                    {
                        if (ThisViewItem?.Column != null)
                        {
                            if (Index == 0)
                            {
                                Control_Create(ThisViewItem, this);
                            }
                            else
                            {
                                Control_Create(ThisViewItem, Tabs.TabPages[Count - 1]);
                            }
                        }
                    }
                }
            }

            EndEdit();
        }

        private void Controls_SetCorrectEnabledState_All()
        {
            Develop.DebugPrint_Disposed(IsDisposed);

            BeginnEdit();
            foreach (var ThisControl in _Control)
            {
                if (ThisControl != null && !ThisControl.IsDisposed) { ThisControl.CheckEnabledState(); }
            }
            EndEdit();
        }


        public void HideViewEditor()
        {
            if (!Editor.Visible) { return; }
            Editor.Visible = false;

            View_Init();
        }

        private void ShowViewEditor()
        {
            if (_Database == null) { return; }
            if (Editor.Visible) { return; }


            if (!_Database.IsAdministrator()) { return; }

            _Database.Load_Reload();


            _Database.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

            if (!_Inited) { View_Init(); }


            ShowingRowKey = -1;


            lbxColumns.Item.Clear();
            lbxColumns.Item.AddRange(_Database.Column, false, false, false);
            lbxColumns.Item.Sort();

            cbxCaptionPosition.Item.AddRange(typeof(enÜberschriftAnordnung));

            cbxControlType.Item.Clear();

            Editor_CheckButtons(false);

            Editor.Visible = true;



            View_Init();
            ColumnsEinfärben();
        }

        private void ColumnsEinfärben()
        {
            var ItC = 0;


            for (ItC = -1; ItC <= Tabs.TabCount + 100; ItC++)
            {
                string Nam = null;
                switch (ItC)
                {
                    case -1:
                        Nam = "Unbenutzt";
                        break;
                    case 0:
                        Nam = "Kopfbereich";
                        break;
                    default:
                        {
                            if (ItC >= 1 && ItC <= _Database.Views.Count - 1)
                            {
                                if (_Database.Views[ItC] == null)
                                {
                                    Nam = string.Empty;
                                }
                                else
                                {
                                    Nam = _Database.Views[ItC].Name + " "; // Leerzeichen wegen evtl. leeren namen
                                }
                            }
                            else
                            {
                                Nam = string.Empty;
                            }
                            break;
                        }
                }

                var intName = "@Ansicht" + ItC.ToString();

                if (string.IsNullOrEmpty(Nam))
                {
                    if (lbxColumns.Item[intName] != null) { lbxColumns.Item.Remove(intName); }
                }
                else
                {
                    if (lbxColumns.Item[intName] == null) { lbxColumns.Item.Add(Nam, intName, true, (ItC + 1).ToString(Constants.Format_Integer3) + "|A"); }
                }
            }




            ItC = -1;
            do
            {
                if (lbxColumns.Item.RemoveNull()) { ItC = -1; }

                ItC++;
                if (ItC > lbxColumns.Item.Count - 1) { break; }


                if (lbxColumns.Item[ItC] != null)
                {

                    if (lbxColumns.Item[ItC].IsClickable())
                    {
                        var ThisItem = (TextListItem)lbxColumns.Item[ItC];
                        var co = (ColumnItem)ThisItem.Tags; //  _Database.Column[ThisItem.Internal];

                        if (co == null)
                        {
                            lbxColumns.Item.Remove(ThisItem);
                        }
                        else
                        {

                            var cv = SearchColumnView(co);
                            var Sort = 0;
                            if (cv == null)
                            {
                                Sort = 0;
                            }
                            else
                            {
                                Sort = _Database.Views.IndexOf(cv) + 1;
                            }

                            ThisItem.UserDefCompareKey = Sort.ToString(Constants.Format_Integer3) + "|Z" + ThisItem.CompareKey(); // ObjectReadable.ReadableText
                        }

                    }

                }

            } while (true);

            lbxColumns.Item.Sort();
            lbxColumns.Invalidate();
        }


        private void Generate_Tabs()
        {
            if (_Database.Views.Count < 1) { return; }


            BeginnEdit();

            foreach (var ThisView in _Database.Views)
            {


                if (ThisView != null && ThisView != _Database.Views[0])
                {
                    TabGeneratorCount++;
                    var tempPage = new TabPage
                    {
                        Text = "Seite #" + TabGeneratorCount
                    };

                    tempPage.Text = ThisView.Name;
                    tempPage.Enabled = _Database.PermissionCheck(ThisView.PermissionGroups_Show, null);
                    Tabs.TabPages.Add(tempPage);

                    tempPage.MouseUp += Tabs_MouseUp;

                }
            }

            EndEdit();

        }


        protected override void DrawControl(Graphics gr, enStates state)
        {
            Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
        }


        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (IsDisposed) { return; }
            base.OnSizeChanged(e);

            if (_Database != null && _Inited)
            {
                Control_RepairSize_All();
            }

            Editor.Left = Width - Editor.Width;
            Editor.Height = Height;
        }

        private void SpaltBEnde_Click(object sender, System.EventArgs e)
        {
            if (!Editor.Visible) { return; }

            Editor.Visible = false;

            View_Init();
        }



        private void Editor_CheckButtons(bool Blinki)
        {
            if (!Editor.Visible) { return; }

            ColumnViewItem ViewItem = null;
            if (lbxColumns.Item.Checked().Count == 1)
            {
                ViewItem = SearchViewItem(_Database.Column[lbxColumns.Item.Checked()[0].Internal]);
            }

            x1.Enabled = ViewItem != null;
            x2.Enabled = ViewItem != null;
            cbxCaptionPosition.Enabled = ViewItem != null;
            cbxControlType.Enabled = ViewItem != null;

            if (ViewItem != null)
            {
                cbxCaptionPosition.Text = ((int)ViewItem.ÜberschriftAnordnung).ToString();

                if (ViewItem.Column != null)
                {

                    if (Blinki)
                    {
                        cbxControlType.Item.Clear();
                        for (var z = 0; z <= 999; z++)
                        {
                            var w = (enEditTypeFormula)z;
                            if (w.ToString() != z.ToString())
                            {
                                if (ViewItem.Column.UserEditDialogTypeInFormula(w))
                                {
                                    cbxControlType.Item.Add(w.ToString(), z.ToString());
                                }
                            }
                        }
                    }

                    cbxControlType.Text = ((int)ViewItem.Column.EditType).ToString();

                }
                else
                {
                    cbxControlType.Enabled = false;
                }
            }
        }

        private void lbxColumns_ContextMenuInit(object sender, ContextMenuInitEventArgs e)
        {
            var item = (BasicListItem)e.HotItem;
            if (item == null) { return; }

            item.Checked = true;

            var cd = SearchViewItem(_Database.Column[item.Internal]);

            if (cd == null)
            {
                e.UserMenu.Add("Zum Kopfbereich hinzufügen", "#AddColumnToHead", enImageCode.Sonne);
                e.UserMenu.Add("Zum Körperbereich hinzufügen", "#AddColumnToBody", enImageCode.Kreis, _Database.Views.Count > 1);
            }
            else
            {
                e.UserMenu.Add("Dieses Feld ausblenden", "#RemoveColumnFromView", enImageCode.Kreuz, Convert.ToBoolean(cd != null));
            }

        }


        private void lbxColumns_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e)
        {
            //if (_Database.Views.Count == 0) { _Database.Views.Add(new ColumnViewCollection(_Database, "")); }

            var Column = _Database.Column[((BasicListItem)e.HotItem).Internal];
            _ = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);

            var CurrView = CurrentView();


            switch (e.ClickedComand)
            {
                case "#RemoveColumnFromView":
                    if (ViewItem != null)
                    {
                        _Database.Views[0]?.Remove(ViewItem);
                        CurrView?.Remove(ViewItem);
                    }
                    break;

                case "#AddColumnToHead":

                    if (_Database.Views.Count ==0)
                    {
                        _Database.Views.Add(new ColumnViewCollection(_Database, string.Empty, "##Head###"));
                    }

                    _Database.Views[0].Add(Column, false);
                    break;

                case "#AddColumnToBody":
                    CurrView?.Add(Column, false);
                    break;

            }

            RedoView();
            ColumnsEinfärben();
        }


        private string EditorSelectedColumn()
        {
            if (lbxColumns.Item.Checked().Count != 1) { return string.Empty; }
            return lbxColumns.Item.Checked()[0].Internal;
        }


        private void lbxColumns_ItemCheckedChanged(object sender, System.EventArgs e)
        {
            Editor_CheckButtons(true);
        }



        private void RedoView()
        {
            var i = Tabs.SelectedIndex;


            BeginnEdit();
            _Inited = false;
            View_Init();

            Editor_CheckButtons(true);

            if (i < 0) { i = 0; }

            if (i > Tabs.TabPages.Count - 1) { i = Tabs.TabPages.Count - 1; }

            if (i >= 0) { Tabs.SelectedIndex = i; }

            EndEdit();
        }


        private void Arrangement_Swap(int Ri)
        {
            var vn = _Database.Views.IndexOf(CurrentView());

            if (vn < 1) { return; }
            if (vn >= _Database.Views.Count) { return; }
            if (Ri == 0 || Ri > 1 || Ri < -1) { return; }

            if (vn < 2 && Ri < 0) { return; }
            if (vn >= _Database.Views.Count - 1 && Ri > 0) { return; }


            var tmpx1 = _Database.Views[vn];
            var tmpx2 = _Database.Views[vn + Ri];

            // Binding List Changes müsste reagieren
            _Database.Views[vn] = tmpx2;
            _Database.Views[vn + Ri] = tmpx1;

            _Inited = false;
        }




        private void Rename_Click(object sender, System.EventArgs e)
        {
            var CurrView = CurrentView();
            if (CurrView == null || CurrView == _Database.Views[0]) { return; }
            var n = InputBox.Show("Umbenennen:", CurrView.Name, enDataFormat.Text);
            if (!string.IsNullOrEmpty(n)) { CurrView.Name = n; }


            RedoView();
            ColumnsEinfärben();

        }
        private void Rechte_Click(object sender, System.EventArgs e)
        {
            var CurrView = CurrentView();


            var aa = new ItemCollectionList();
            aa.AddRange(_Database.Permission_AllUsed(true));
            aa.CheckBehavior = enCheckBehavior.MultiSelection;
            aa.Check(CurrView.PermissionGroups_Show, true);

            if (_Database.Views.Count > 1 && CurrView == _Database.Views[1])
            {
                aa["#Everybody"].Enabled = false;
                aa["#Everybody"].Checked = true;
            }
            aa.Sort();

            var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, enAddType.Text, true);
            if (b == null) { return; }


            CurrView.PermissionGroups_Show.Clear();
            CurrView.PermissionGroups_Show.AddRange(b.ToArray());

            CurrView.PermissionGroups_Show.RemoveString("#Administrator", false);
            if (CurrView == _Database.Views[1]) { CurrView.PermissionGroups_Show.Add("#Everybody"); }
        }


        private void OrderDelete_Click(object sender, System.EventArgs e)
        {
            var CurrView = CurrentView();

            if (CurrView == null) { return; }
            var i = _Database.Views.IndexOf(CurrView);
            if (i < 1) { return; } // 0 darf auch nicht gelöscht werden
            if (MessageBox.Show("Ansicht <b>'" + CurrView.Name + "'</b><br>wirklich löschen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            _Database.Views.RemoveAt(i);


            RedoView();
            ColumnsEinfärben();


        }


        private void OrderAdd_Click(object sender, System.EventArgs e)
        {

            var ex = InputBox.Show("Geben sie den Namen<br>der neuen Ansicht ein:", "", enDataFormat.Text);
            if (string.IsNullOrEmpty(ex)) { return; }

            _Database.Views.Add(new ColumnViewCollection(_Database, "", ex));

            RedoView();
            ColumnsEinfärben();
        }

        private void cbxCaptionPosition_ItemClicked(object sender, BasicListItemEventArgs e)
        {

            var Column = _Database.Column[EditorSelectedColumn()];
            _ = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);


            if (ViewItem != null) { ViewItem.ÜberschriftAnordnung = (enÜberschriftAnordnung)int.Parse(cbxCaptionPosition.Text); }


            RedoView();
            ColumnsEinfärben();
        }

        private void cbxControlType_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            var Column = _Database.Column[EditorSelectedColumn()];
            _ = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);

            if (ViewItem != null) { ViewItem.Column.EditType = (enEditTypeFormula)int.Parse(cbxControlType.Text); }

            RedoView();
            ColumnsEinfärben();
        }


        private void SLinks_Click(object sender, System.EventArgs e)
        {
            var Column = _Database.Column[EditorSelectedColumn()];
            _ = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);

            ViewItem?.KoordÄndern(-1, 0, 0);

            RedoView();
            ColumnsEinfärben();
        }

        private void SRechts_Click(object sender, System.EventArgs e)
        {
            var Column = _Database.Column[EditorSelectedColumn()];
            _ = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);

            ViewItem?.KoordÄndern(1, 0, 0);

            RedoView();
            ColumnsEinfärben();
        }

        private void SOben_Click(object sender, System.EventArgs e)
        {
            var Column = _Database.Column[EditorSelectedColumn()];
            var View = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);


            if (ViewItem != null) { View.Swap(ViewItem, ViewItem.PreviewsVisible(View)); }

            RedoView();
            ColumnsEinfärben();
        }

        private void SUnten_Click(object sender, System.EventArgs e)
        {
            var Column = _Database.Column[EditorSelectedColumn()];
            var View = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);

            if (ViewItem != null) { View.Swap(ViewItem, ViewItem.NextVisible(View)); }

            RedoView();
            ColumnsEinfärben();


        }

        private void EOben_Click(object sender, System.EventArgs e)
        {
            var Column = _Database.Column[EditorSelectedColumn()];
            _ = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);

            ViewItem?.KoordÄndern(0, 0, -1);

            RedoView();
            ColumnsEinfärben();



        }

        private void EUnten_Click(object sender, System.EventArgs e)
        {

            var Column = _Database.Column[EditorSelectedColumn()];
            _ = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);

            ViewItem?.KoordÄndern(0, 0, 1);


            RedoView();
            ColumnsEinfärben();



        }

        private void ELinks_Click(object sender, System.EventArgs e)
        {
            var Column = _Database.Column[EditorSelectedColumn()];
            _ = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);

            ViewItem?.KoordÄndern(0, -1, 0);


            RedoView();
            ColumnsEinfärben();



        }

        private void ERechts_Click(object sender, System.EventArgs e)
        {
            var Column = _Database.Column[EditorSelectedColumn()];
            _ = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);

            ViewItem?.KoordÄndern(0, 1, 0);

            RedoView();
            ColumnsEinfärben();





        }





        public void Control_Remove_All()
        {

            if (InvokeRequired)
            {
                Invoke(new Action(() => Control_Remove_All()));
                return;
            }


            var R = false;
            BeginnEdit();

            do
            {
                R = false;

                foreach (System.Windows.Forms.Control o in Controls)
                {
                    switch (o.Name)
                    {
                        case "Editor":

                            break;
                        case "Tabs":
                            if (((AbstractTabControl)o).TabCount > 0)
                            {
                                RemoveControl(o);
                                R = true;
                            }
                            break;

                        default:
                            RemoveControl(o);
                            R = true;
                            break;
                    }
                }

            } while (R);


            _Control?.Clear();
            EndEdit();

        }


        private void RemoveControl(System.Windows.Forms.Control vObject)
        {
            if (vObject == null || vObject.IsDisposed) { return; }

            BeginnEdit();

            switch (vObject)
            {
                case AbstractTabControl _:
                    foreach (System.Windows.Forms.Control o in vObject.Controls)
                    {
                        RemoveControl(o);
                    }
                    EndEdit();
                    return; // Raus hier

                case TabPage _:
                    foreach (System.Windows.Forms.Control o in vObject.Controls)
                    {
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

            EndEdit();
        }

        private void Li_Click(object sender, System.EventArgs e)
        {
            Arrangement_Swap(-1);
        }

        private void Re_Click(object sender, System.EventArgs e)
        {
            Arrangement_Swap(1);
        }


        private void _Database_ColumnRemoved(object sender, System.EventArgs e)
        {
            if (IsDisposed) { return; }
            View_Init();
        }

        private void _Database_ColumnContentChanged(object sender, ListEventArgs e)
        {
            if (IsDisposed) { return; }
            var r = _ShowingRowKey;
            ShowingRowKey = -1;

            View_Init();
            ShowingRowKey = r;
        }

        private ColumnViewCollection CurrentView()
        {
            if (_Database.Views.Count == 0) { return null; }
            if (Tabs.SelectedIndex + 1 > _Database.Views.Count - 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Index zu hoch"); }
            if (_Database.Views[Tabs.SelectedIndex + 1] == null) { Develop.DebugPrint(enFehlerArt.Fehler, "View ist Nothing"); }

            return _Database.Views[Tabs.SelectedIndex + 1];
        }

        private ColumnViewItem SearchViewItem(ColumnItem Column)
        {
            var ThisView = SearchColumnView(Column);
            return ThisView?[Column];
        }


        public static ColumnViewCollection SearchColumnView(ColumnItem Column)
        {

            if (Column == null) { return null; }

            foreach (var ThisView in Column.Database.Views)
            {

                if (ThisView != null)
                {

                    foreach (var thisViewItem in ThisView)
                    {
                        if (thisViewItem?.Column != null)
                        {
                            if (thisViewItem.Column == Column) { return ThisView; }
                        }
                    }
                }

            }
            return null;
        }

        private void Tabs_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) { FloatingInputBoxListBoxStyle.ContextMenuShow(this, e); }
        }

        //private void Tabs_SelectedIndexChanged(object sender, System.EventArgs e)
        //{
        //    if (Editor.Visible) { ColumnsEinfärben(); }
        //}

        private FlexiControlForCell ControlOf(ColumnViewItem ThisViewItem)
        {
            foreach (var ThisControl in _Control)
            {
                if (ThisControl != null && !ThisControl.IsDisposed && ThisControl.Tag == ThisViewItem) { return ThisControl; }
            }

            return null;
        }

        #region  ContextMenu 

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate)
        {
            HotItem = null;

            if (_Database == null)
            {
                Cancel = true;
                return;
            }


            Items.Add("Allgemeine Schnelleingabe öffnen", "#Schnelleingabe", QuickImage.Get(enImageCode.Lupe), _Database != null && ShowingRow != null);


            if (_Database.IsAdministrator())
            {
                Items.AddSeparator();
                Items.Add("Formular bearbeiten", "#Ansicht", QuickImage.Get(enImageCode.Textfeld), _Database != null);

            }
        }

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e)
        {
            if (_Database == null) { return true; }

            switch (e.ClickedComand.ToLower())
            {

                case "#schnelleingabe":
                    if (ShowingRow == null) { return true; }
                    var sh = new FormulaQuickSelect(ShowingRow);
                    sh.ShowDialog();
                    return true;

                case "#ansicht":
                    ShowViewEditor();
                    return true;

            }

            return false;

        }

        public void OnContextMenuInit(ContextMenuInitEventArgs e)
        {
            ContextMenuInit?.Invoke(this, e);
        }

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }
        private void _Database_StoreView(object sender, LoadingEventArgs e)
        {
            if (e.OnlyReload) { return; }
            _savedRowKey = ShowingRowKey;
        }
        private void _Database_RowKeyChanged(object sender, KeyChangedEventArgs e)
        {
            // Ist aktuell nur möglich,wenn Pending Changes eine neue Zeile machen
            // Jedes FlexControl beachtet für sich die Änderung
            if (e.KeyOld == _savedRowKey) { _savedRowKey = e.KeyNew; }

        }

        #endregion
    }
}
