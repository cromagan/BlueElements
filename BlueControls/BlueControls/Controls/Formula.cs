#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Forms;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.EventArgs;
using BlueDatabase.Enums;
using BlueControls.Enums;

namespace BlueControls.Controls
{
    public partial class Formula : GenericControl, IBackgroundNone, IContextMenu
    {

        public Formula()
        {
            InitializeComponent();
        }

        private Database _Database;

        private int _ShowingRowKey = -1;
        private bool _Inited;
        private int Suspendcounter;


        public event EventHandler<RowEventArgs> ShowingRowChanged;
        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        private List<FlexiControlForCell> _Control;


        private int SavedRowKey = int.MinValue;

        private long TabGeneratorCount;

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    _ShowingRowKey = 0;
                    Database = null; // Wichtig,  (nicht _Database) um events zu l�sen.
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

                if (Editor.Visible) { Editor.Visible = false; }
                ShowingRowKey = -1; // Wichtig, dass ordenlich Showing-Row to Nothing gesetzt wird, weil dann alle Fokuse durch Enabled elemeniert werden und nachtr�glich nix mehr ausgel�st wird.
                Control_Remove_All();

                if (_Database != null)
                {
                    _Database.Loaded -= _DatabaseLoaded;
                    _Database.Row.RowChecked -= _Database_RowChecked;
                    _Database.Column.ItemRemoved -= _Database_ColumnRemoved;
                    _Database.Column.ItemInternalChanged -= _Database_ColumnContentChanged;
                    _Database.StoreView -= _Database_StoreView;
                    _Database.RowKeyChanged -= _Database_RowKeyChanged;
                    _Database.RestoreView -= _Database_RestoreView;

                    _Database.Release(false, 180); // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann

                }
                _Database = value;

                if (_Database != null)
                {
                    _Database.Loaded += _DatabaseLoaded;
                    _Database.Row.RowChecked += _Database_RowChecked;
                    _Database.Column.ItemRemoved += _Database_ColumnRemoved;
                    _Database.Column.ItemInternalChanged += _Database_ColumnContentChanged;
                    _Database.StoreView += _Database_StoreView;
                    _Database.RowKeyChanged += _Database_RowKeyChanged;
                    _Database.RestoreView += _Database_RestoreView;
                }

                _Inited = false;
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


                SuspendAllLayout();


                _ShowingRowKey = value;

                foreach (var thisFlex in _Control)
                {
                    if (thisFlex != null && !thisFlex.IsDisposed) { thisFlex.RowKey = _ShowingRowKey; }
                }


                Controls_SetCorrectEnabledState_All();

                OnShowingRowChanged(new RowEventArgs(ShowingRow));

                ShowingRow?.DoAutomatic(false, false);
                ResumeAllLayout();
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
                var r = _Database?.Row.SearchByKey(_ShowingRowKey);
                return r;
            }
        }


        private void SuspendAllLayout()
        {
            Suspendcounter += 1;
            if (Suspendcounter > 1) { return; }

            SuspendLayout();
            Tabs.SuspendLayout();

            foreach (System.Windows.Forms.TabPage tp in Tabs.TabPages)
            {
                tp.SuspendLayout();
            }

        }

        private void ResumeAllLayout()
        {
            Suspendcounter -= 1;
            if (Suspendcounter > 0) { return; }
            foreach (System.Windows.Forms.TabPage tp in Tabs.TabPages)
            {
                tp.ResumeLayout();
            }
            Tabs.ResumeLayout();

            ResumeLayout();
        }


        private void View_Init()
        {
            if (_Database == null) { return; }
            if (Parent == null) { return; } // Irgend ein Formular reagiert nioch?!?

            SuspendAllLayout();
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

            ResumeAllLayout();

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

                            //Bitte jeden Fehler anzeigen..... Es verursacht mehr R�tsel, wenn die Zeile einfach Fehlerhaft ist und �berhaut kein Hinweis kommt
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
                p = e.ColumnsWithErrors[nr].Split('|');
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
            SuspendAllLayout();

            foreach (var ThisView in _Database.Views)
            {
                Count += 1;

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

                        var ObjPX = new Rectangle(); // Die Koordinaten in Pixel des Steuerelements

                        ObjPX.Width = ThisViewItem.Width * WidthInPixelOfColumn + (ThisViewItem.Width - 1) * Skin.PaddingSmal;
                        ObjPX.X = ThisViewItem.Spalte_X1 * WidthInPixelOfColumn + ThisViewItem.Spalte_X1 * Skin.PaddingSmal + MoveIn;

                        ObjPX.Y = MoveIn;
                        for (var z = ThisViewItem.Spalte_X1; z < ThisViewItem.Spalte_X1 + ThisViewItem.Width; z++) // Spalte_X2
                        {
                            ObjPX.Y = Math.Max(_BelegterBereichTop[z], ObjPX.Y);
                        }



                        if (ObjPX.Y > 0) { ObjPX.Y += 8; }
                        ObjPX.Height = Math.Max(ThisViewItem.Height, 0) * 16 + 8;

                        if (ThisViewItem.�berschriftAnordnung == en�berschriftAnordnung.Ohne_mit_Abstand || ThisViewItem.�berschriftAnordnung == en�berschriftAnordnung.�ber_dem_Feld)
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

            ResumeAllLayout();
        }




        private void Control_Create(ColumnViewItem cd, System.Windows.Forms.Control vParent)
        {
            if (cd?.Column == null) { return; }

            Develop.Debugprint_BackgroundThread();
            var btb = new FlexiControlForCell(cd.Column.Database, cd.Column.Key, cd.�berschriftAnordnung);
            btb.TabIndex = TabIndex + 10000;
            btb.Tag = cd;

            btb.ContextMenuInit += ContextMenuOfControls_Init;
            btb.ContextMenuItemClicked += ContextMenuOfControls_ItemClicked;
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

            SuspendAllLayout();

            _Control = new List<FlexiControlForCell>();

            foreach (var ThisView in _Database.Views)
            {
                if (ThisView != null)
                {
                    var Index = _Database.Views.IndexOf(ThisView);
                    Count += 1;

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

            ResumeAllLayout();
        }

        private void Controls_SetCorrectEnabledState_All()
        {
            Develop.DebugPrint_Disposed(IsDisposed);

            SuspendAllLayout();
            foreach (var ThisControl in _Control)
            {
                if (ThisControl != null && !ThisControl.IsDisposed) { ThisControl.CheckEnabledState(); }
            }
            ResumeAllLayout();
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

            _Database.Reload();


            _Database.OnConnectedControlsStopAllWorking(new DatabaseStoppedEventArgs());

            if (!_Inited) { View_Init(); }


            ShowingRowKey = -1;


            lbxColumns.Item.Clear();
            lbxColumns.Item.AddRange(_Database.Column, false, false);
            lbxColumns.Item.Sort();

            cbxCaptionPosition.Item.AddRange(typeof(en�berschriftAnordnung));

            cbxControlType.Item.Clear();

            Editor_CheckButtons(false);

            Editor.Visible = true;



            View_Init();
            ColumnsEinf�rben();
        }

        private void ColumnsEinf�rben()
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
                    if (lbxColumns.Item[intName] == null) { lbxColumns.Item.Add(new TextListItem(true, intName, Nam, (ItC + 1).ToString(Constants.Format_Integer3) + "|A")); }
                }
            }




            ItC = -1;
            do
            {
                if (lbxColumns.Item.RemoveNull()) { ItC = -1; }

                ItC += 1;
                if (ItC > lbxColumns.Item.Count - 1) { break; }


                if (lbxColumns.Item[ItC] != null)
                {

                    if (lbxColumns.Item[ItC].IsClickable())
                    {


                        var ThisItem = (ObjectListItem)lbxColumns.Item[ItC];


                        var co = _Database.Column[ThisItem.Internal()];

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
            lbxColumns.Refresh();
        }


        private void Generate_Tabs()
        {
            if (_Database.Views.Count < 1) { return; }
            SuspendAllLayout();



            foreach (var ThisView in _Database.Views)
            {


                if (ThisView != null && ThisView != _Database.Views[0])
                {
                    TabGeneratorCount += 1;
                    var tempPage = new TabPage();
                    tempPage.Text = "Seite #" + TabGeneratorCount;

                    tempPage.Text = ThisView.Name;
                    tempPage.Enabled = _Database.PermissionCheck(ThisView.PermissionGroups_Show, null);
                    Tabs.TabPages.Add(tempPage);

                    tempPage.MouseUp += Tabs_MouseUp;

                }
            }

            ResumeAllLayout();

        }


        protected override void InitializeSkin()
        {
            if (_Database == null) { return; }

            var r = _ShowingRowKey;
            ShowingRowKey = -1;

            _Database.LoadPicsIntoImageChache();
            RedoView();
            ShowingRowKey = r;
        }


        protected override void DrawControl(Graphics gr, enStates state)
        {
            Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
        }



        private void BlueFormula_SizeChanged(object sender, System.EventArgs e)
        {
            if (IsDisposed) { return; }


            if (_Database != null && _Inited)
            {
                SuspendAllLayout();
                Control_RepairSize_All();
                ResumeAllLayout();
            }

            PerformLayout();

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
                ViewItem = SearchViewItem(_Database.Column[lbxColumns.Item.Checked()[0].Internal()]);
            }

            x1.Enabled = ViewItem != null;
            x2.Enabled = ViewItem != null;
            cbxCaptionPosition.Enabled = ViewItem != null;
            cbxControlType.Enabled = ViewItem != null;

            if (ViewItem != null)
            {
                cbxCaptionPosition.Text = ((int)ViewItem.�berschriftAnordnung).ToString();

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
                                    cbxControlType.Item.Add(new TextListItem(z.ToString(), w.ToString()));
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
            var item = (BasicListItem)e.Tag;
            if (item == null) { return; }

            item.Checked = true;

            var cd = SearchViewItem(_Database.Column[item.Internal()]);

            if (cd == null)
            {
                e.UserMenu.Add(new TextListItem("#AddColumnToHead", "Zum Kopfbereich hinzuf�gen", enImageCode.Sonne));
                e.UserMenu.Add(new TextListItem("#AddColumnToBody", "Zum K�rperbereich hinzuf�gen", enImageCode.Kreis, _Database.Views.Count > 1));
            }
            else
            {
                e.UserMenu.Add(new TextListItem("#RemoveColumnFromView", "Dieses Feld ausblenden", enImageCode.Kreuz, Convert.ToBoolean(cd != null)));
            }

        }

        private void lbxColumns_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e)
        {
            DoColumnArrangementCommand(e.ClickedComand.Internal(), ((BasicListItem)e.Tag).Internal());
        }

        private string EditorSelectedColumn()
        {
            if (lbxColumns.Item.Checked().Count != 1) { return string.Empty; }
            return lbxColumns.Item.Checked()[0].Internal();
        }


        private void lbxColumns_ItemCheckedChanged(object sender, System.EventArgs e)
        {
            Editor_CheckButtons(true);
        }

        private void DoColumnArrangementCommand(string ClickedComand, string ColumnName)
        {
            if (_Database.Views.Count == 0) { _Database.Views.Add(new ColumnViewCollection(_Database, "")); }

            var Column = _Database.Column[ColumnName];

            var View = SearchColumnView(Column);
            var ViewItem = SearchViewItem(Column);

            var CurrView = CurrentView();


            switch (ClickedComand)
            {
                case "#RemoveColumnFromView":
                    if (ViewItem != null)
                    {
                        _Database.Views[0]?.Remove(ViewItem);
                        CurrView?.Remove(ViewItem);
                    }
                    ColumnsEinf�rben();
                    break;

                case "#AddColumnToHead":
                    _Database.Views[0].Add(Column, false);
                    ColumnsEinf�rben();
                    break;

                case "#AddColumnToBody":
                    CurrView?.Add(Column, false);
                    ColumnsEinf�rben();
                    break;

                case "SpaltenEigenschaftenBearbeiten":
                    using (var w = new ColumnEditor(_Database.Column[ColumnName]))
                    {
                        w.ShowDialog();
                        _Database.Column[ColumnName].Invalidate_ColumAndContent();
                    }
                    break;

                case "#Berechtigung":
                    if (CurrView == null || CurrView == _Database.Views[0]) { return; }
                    var aa = new ItemCollectionList();
                    aa.AddRange(_Database.Permission_AllUsed(true));
                    aa.CheckBehavior = enCheckBehavior.MultiSelection;
                    aa.Check(CurrView.PermissionGroups_Show, true);

                    if (CurrView == _Database.Views[1])
                    {
                        aa["#Everybody"].Enabled = false;
                        aa["#Everybody"].Checked = true;
                    }
                    aa.Sort();

                    var b = InputBoxListBoxStyle.Show("W�hlen sie, wer anzeigeberechtigt ist:<br><i>Info: Administratoren sehen alle Ansichten", aa, enAddType.Text, true);
                    if (b == null) { return; }


                    CurrView.PermissionGroups_Show.Clear();
                    CurrView.PermissionGroups_Show.AddRange(b.ToArray());

                    CurrView.PermissionGroups_Show.RemoveString("#Administrator", false);
                    if (CurrView == _Database.Views[1]) { CurrView.PermissionGroups_Show.Add("#Everybody"); }
                    break;

                case "#ViewRename":
                    if (CurrView == null || CurrView == _Database.Views[0]) { return; }
                    var n = InputBox.Show("Umbenennen:", CurrView.Name, enDataFormat.Text);
                    if (!string.IsNullOrEmpty(n)) { CurrView.Name = n; }
                    ColumnsEinf�rben();
                    break;

                case "#ViewAdd":
                    Arrangement_Add();
                    ColumnsEinf�rben();
                    break;

                case "#ChangeViewCaption":
                    if (ViewItem != null) { ViewItem.�berschriftAnordnung = (en�berschriftAnordnung)int.Parse(cbxCaptionPosition.Text); }
                    break;

                case "#ChangeControlType":
                    if (ViewItem != null) { ViewItem.Column.EditType = (enEditTypeFormula)int.Parse(cbxControlType.Text); }
                    break;

                case "#X1_O":
                    if (ViewItem != null) { View.Swap(ViewItem, ViewItem.PreviewsVisible(View)); }
                    break;

                case "#X1_U":
                    if (ViewItem != null) { View.Swap(ViewItem, ViewItem.NextVisible(View)); }
                    break;

                case "#X1_L":
                    ViewItem?.Koord�ndern(-1, 0, 0);
                    break;

                case "#X1_R":
                    ViewItem?.Koord�ndern(1, 0, 0);
                    break;

                case "#X2_O":
                    ViewItem?.Koord�ndern(0, 0, -1);
                    break;

                case "#X2_U":
                    ViewItem?.Koord�ndern(0, 0, 1);
                    break;

                case "#X2_L":
                    ViewItem?.Koord�ndern(0, -1, 0);
                    break;

                case "#X2_R":
                    ViewItem?.Koord�ndern(0, 1, 0);
                    break;

                case "#ViewMoveRight":
                    Arrangement_Swap(1);
                    break;

                case "#ViewMoveLeft":
                    Arrangement_Swap(-1);
                    break;

                case "#ViewDelete":
                    if (CurrView == null) { return; }
                    var i = _Database.Views.IndexOf(CurrView);
                    if (i < 1) { return; } // 0 darf auch nicht gel�scht werden
                    if (MessageBox.Show("Ansicht <b>'" + CurrView.Name + "'</b><br>wirklich l�schen?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                    _Database.Views.RemoveAt(i);
                    ColumnsEinf�rben();
                    break;

                default:
                    Develop.DebugPrint(ClickedComand);
                    break;
            }

            RedoView();
            ColumnsEinf�rben();
        }


        private void RedoView()
        {
            var i = Tabs.SelectedIndex;

            SuspendLayout();
            _Inited = false;
            View_Init();

            Editor_CheckButtons(true);

            if (i < 0) { i = 0; }

            if (i > Tabs.TabPages.Count - 1) { i = Tabs.TabPages.Count - 1; }

            if (i >= 0) { Tabs.SelectedIndex = i; }

            ResumeLayout();
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

            // Binding List Changes m�sste reagieren
            _Database.Views[vn] = tmpx2;
            _Database.Views[vn + Ri] = tmpx1;

            _Inited = false;
        }

        private void Arrangement_Add()
        {
            var e = InputBox.Show("Geben sie den Namen<br>der neuen Ansicht ein:", "", enDataFormat.Text);
            if (string.IsNullOrEmpty(e)) { return; }

            _Database.Views.Add(new ColumnViewCollection(_Database, "", e));
        }


        private void Rename_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#ViewRename", null);
        }
        private void Rechte_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#Berechtigung", null);
        }
        private void OrderDelete_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#ViewDelete", null);
        }


        private void OrderAdd_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#ViewAdd", null);
        }

        private void cbxCaptionPosition_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            DoColumnArrangementCommand("#ChangeViewCaption", EditorSelectedColumn());
        }

        private void cbxControlType_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            DoColumnArrangementCommand("#ChangeControlType", EditorSelectedColumn());
        }


        private void SLinks_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#X1_L", EditorSelectedColumn());
        }

        private void SRechts_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#X1_R", EditorSelectedColumn());
        }

        private void SOben_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#X1_O", EditorSelectedColumn());
        }

        private void SUnten_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#X1_U", EditorSelectedColumn());
        }

        private void EOben_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#X2_O", EditorSelectedColumn());
        }

        private void EUnten_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#X2_U", EditorSelectedColumn());
        }

        private void ELinks_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#X2_L", EditorSelectedColumn());
        }

        private void ERechts_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#X2_R", EditorSelectedColumn());
        }





        public void Control_Remove_All()
        {

            if (InvokeRequired)
            {
                Invoke(new Action(() => Control_Remove_All()));
                return;
            }


            var R = false;
            SuspendAllLayout();

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
                            if (((TabControl)o).TabCount > 0)
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
            ResumeAllLayout();

        }


        private void RemoveControl(System.Windows.Forms.Control vObject)
        {
            if (vObject == null || vObject.IsDisposed) { return; }

            switch (vObject)
            {
                case TabControl _:
                    vObject.SuspendLayout();
                    foreach (System.Windows.Forms.Control o in vObject.Controls)
                    {
                        RemoveControl(o);
                    }
                    vObject.ResumeLayout();
                    return;

                case TabPage _:
                    vObject.SuspendLayout();
                    foreach (System.Windows.Forms.Control o in vObject.Controls)
                    {
                        RemoveControl(o);
                    }
                    vObject.ResumeLayout();
                    break;

                default:
                    var tempVar = (FlexiControlForCell)vObject;
                    tempVar.ContextMenuInit -= ContextMenuOfControls_Init;
                    tempVar.ContextMenuItemClicked -= ContextMenuOfControls_ItemClicked;
                    tempVar.NeedRefresh -= Btb_IAmInvalid;
                    break;
            }

            vObject.Parent.Controls.Remove(vObject);
            vObject.Dispose();
        }

        private void Li_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#ViewMoveLeft", null);
        }

        private void Re_Click(object sender, System.EventArgs e)
        {
            DoColumnArrangementCommand("#ViewMoveRight", null);
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
            if (e.Button == System.Windows.Forms.MouseButtons.Right) { ContextMenu_Show(sender, e); }
        }

        private void Tabs_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (Editor.Visible) { ColumnsEinf�rben(); }
        }

        private FlexiControlForCell ControlOf(ColumnViewItem ThisViewItem)
        {
            foreach (var ThisControl in _Control)
            {
                if (ThisControl != null && !ThisControl.IsDisposed && ThisControl.Tag == ThisViewItem) { return ThisControl; }
            }

            return null;
        }

        #region  ContextMenu 

        public void ContextMenu_Show(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Diese Routine wird aufgerufen, wenn NICHT auf ein Element geklickz wird.
            // Um die Men�se immer gleich zu halten, wird aber zum kontextme�-Init der Elemente weitergeleitet

            var ThisContextMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);


            ContextMenuOfControls_Init(sender, new ContextMenuInitEventArgs(null, ThisContextMenu));

            if (ThisContextMenu.Count > 0)
            {
                ThisContextMenu.Add(new LineListItem());
                ThisContextMenu.Add(enContextMenuComands.Abbruch);
                var _ContextMenu = FloatingInputBoxListBoxStyle.Show(ThisContextMenu, null, this, Translate);
                _ContextMenu.ItemClicked += ContextMenuItemClickedInternalProcessig;
            }

        }
        private void ContextMenuOfControls_Init(object sender, ContextMenuInitEventArgs e)
        {
            // Eigentlich ist das die Routine, wo nur Bfehele hinzugef�gt werden der Elemente.
            // Es wird aber AUCH mi�braucht vom Kontextmenu des Blueformula. Dann ist der sender nothing, zum unterscheiden

            OnContextMenuInit(e);

            if (e.UserMenu.Count > 0) { e.UserMenu.Add(new LineListItem()); }


            e.UserMenu.Add(new TextListItem("#Schnelleingabe", "Allgemeine Schnelleingabe �ffnen", QuickImage.Get(enImageCode.Lupe), _Database != null && ShowingRow != null));


            if (_Database.IsAdministrator())
            {
                e.UserMenu.Add(new LineListItem());
                e.UserMenu.Add(new TextListItem("#Ansicht", "Formular bearbeiten", QuickImage.Get(enImageCode.Stift), _Database != null));
            }
        }

        private void ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e)
        {
            // Diese Routine wird aufgerufen, wenn NICHT auf ein Element geklickt wird.
            // Um die Men�se immer gleich zu halten, wird aber zum Clicked Ereigis der Elemente weitergeleitet
            ContextMenuOfControls_ItemClicked(null, e);
        }



        private void ContextMenuOfControls_ItemClicked(object sender, ContextMenuItemClickedEventArgs e)
        {
            FloatingInputBoxListBoxStyle.Close(this);

            if (_Database == null) { return; }

            switch (e.ClickedComand.Internal().ToLower())
            {
                case "abbruch":
                    return;

                case "#schnelleingabe":
                    if (ShowingRow == null) { return; }
                    var sh = new FormulaQuickSelect(ShowingRow);
                    sh.ShowDialog();
                    return;

                case "#ansicht":
                    ShowViewEditor();
                    return;
            }

            // So, wir gehen mal davon aus, dass wir nun die Item-Spezfischen Dingens abgefangen haben und geben nun weiter, als ob es vom BlueFormula kommt.
            OnContextMenuItemClicked(new ContextMenuItemClickedEventArgs(null, e.ClickedComand));

        }

        private void OnContextMenuInit(ContextMenuInitEventArgs e)
        {
            ContextMenuInit?.Invoke(this, e);
        }

        private void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }
        private void _Database_StoreView(object sender, System.EventArgs e)
        {
            SavedRowKey = ShowingRowKey;
        }


        private void _Database_RowKeyChanged(object sender, KeyChangedEventArgs e)
        {
            // Ist aktuell nur m�glich,wenn Pending Changes eine neue Zeile machen
            // Jedes FlexControl beachtet f�r sich die �nderung
            if (e.KeyOld == SavedRowKey) { SavedRowKey = e.KeyNew; }

        }


        private void _Database_RestoreView(object sender, System.EventArgs e)
        {
            ShowingRowKey = SavedRowKey;
        }

        #endregion
    }
}
