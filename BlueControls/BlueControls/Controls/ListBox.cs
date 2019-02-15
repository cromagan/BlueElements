using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueControls.DialogBoxes;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.EventArgs;
using BlueDatabase.Enums;
using BlueControls.Enums;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls
{
    public partial class ListBox : IContextMenu, IBackgroundNone, IQuickInfo, IChangedFeedback
    {


        private string Plus_Click_LastPfad = "";

        public ListBox()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Item = new ItemCollectionList();
            Item.ListOrItemChanged += _Item_ListOrItemChanged;
            Item.ItemCheckedChanged += _Item_ItemCheckedChanged;
            Item.ItemAdded += _Item_Item_Added;
            Item.ItemRemoved += _Item_ItemRemoved;
            _Appearance = enBlueListBoxAppearance.Listbox;
            SetDoubleBuffering();
        }

        private enBlueListBoxAppearance _Appearance; //Muss was gesetzt werden, sonst hat der Designer nachher einen Fehler
        private BasicListItem _MouseOverItem;
        private bool _MoveAllowed;
        private bool _RemoveAllowed;
        private bool _FilterAllowed;
        private enAddType _AddAlloweds = enAddType.Text;




        #region  Events 
        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;
        public event EventHandler ItemCheckedChanged;
        public event EventHandler<ListEventArgs> ItemAdded;
        public event EventHandler ItemRemoved;
        public event EventHandler<BasicListItemEventArgs> ItemDoubleClick;
        public event EventHandler<BasicListItemEventArgs> ItemClick;
        public event EventHandler<AllreadyHandledEventArgs> AddClicked;
        public event EventHandler<ListOfBasicListItemEventArgs> RemoveClicked;
        public event EventHandler<ItemSwapEventArgs> UpOrDownClicked;
        public event EventHandler<ColumnEventArgs> NeedColumn;
        public event EventHandler Changed;

        #endregion

        #region  Properties 

        [DefaultValue(enCheckBehavior.SingleSelection)]
        public enCheckBehavior CheckBehavior
        {
            get
            {
                return Item.CheckBehavior;
            }
            set
            {
                Item.CheckBehavior = value;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionList Item { get; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionList Suggestions { get; } = new ItemCollectionList();

        [DefaultValue(enBlueListBoxAppearance.Listbox)]
        public enBlueListBoxAppearance Appearance
        {
            get
            {
                return _Appearance;
            }
            set
            {
                _Appearance = value;
            }
        }


        [DefaultValue(false)]
        public bool RemoveAllowed
        {
            get
            {
                return _RemoveAllowed;
            }
            set
            {
                if (_RemoveAllowed == value)
                {
                    return;
                }
                _RemoveAllowed = value;
                CheckButtons();
            }
        }

        [DefaultValue(true)]
        public enAddType AddAllowed
        {
            get
            {
                return _AddAlloweds;
            }
            set
            {
                if (_AddAlloweds == value)
                {
                    return;
                }
                _AddAlloweds = value;
                CheckButtons();
            }
        }

        [DefaultValue(false)]
        public bool MoveAllowed
        {
            get
            {
                return _MoveAllowed;
            }
            set
            {
                if (_MoveAllowed == value) { return; }
                _MoveAllowed = value;
                if (_MoveAllowed) { _FilterAllowed = false; }

                CheckButtons();
            }
        }


        [DefaultValue(false)]
        public bool FilterAllowed
        {
            get
            {
                return _FilterAllowed;
            }
            set
            {
                if (_FilterAllowed == value) { return; }
                _FilterAllowed = value;
                CheckButtons();
            }
        }





        #endregion

        #region  QuickInfo 
        // Dieser Codeblock ist im Interface IQuickInfo herauskopiert und muss überall Identisch sein.
        private string _QuickInfo = "";
        [Category("Darstellung")]
        [Description("QuickInfo des Steuerelementes - im extTXT-Format")]
        public string QuickInfo
        {
            get
            {
                return _QuickInfo;
            }
            set
            {
                if (_QuickInfo != value)
                {
                    DialogBoxes.QuickInfo.Close();
                    _QuickInfo = value;
                }
            }
        }
        #endregion


        protected override void OnVisibleChanged(System.EventArgs e)
        {
            CheckButtons();
            base.OnVisibleChanged(e);
        }

        private void CheckButtons()
        {
            if (!Visible) { return; }
            if (Parent == null) { return; }

            var nr = Item.Checked();

            var tmpAddAllowed = _AddAlloweds;
            if (GetEditType(false) == enEditTypeTable.None) { tmpAddAllowed = enAddType.None; }
            // Bei GetEditType KEINEN Error abwerfen - es kann ja auch keine Zeile somit Zelle zugeordnet sein

            Down.Visible = _MoveAllowed;
            Up.Visible = _MoveAllowed;
            Plus.Visible = tmpAddAllowed != enAddType.None;
            Minus.Visible = _RemoveAllowed;
            FilterTxt.Visible = _FilterAllowed;
            FilterCap.Visible = _FilterAllowed;

            if (_RemoveAllowed)
            {
                if (nr.Count == 0)
                {
                    Minus.Enabled = false;
                }
                else
                {
                    Minus.Enabled = true;
                }
            }


            if (_MoveAllowed)
            {
                if (nr.Count != 1)
                {
                    Up.Enabled = false;
                    Down.Enabled = false;
                }
                else
                {
                    Up.Enabled = Item[0] != nr[0];
                    Down.Enabled = Item[Item.Count - 1] != nr[0];
                }
            }
        }


        protected override void OnParentEnabledChanged(System.EventArgs e)
        {
            if (IsDisposed) { return; }

            Down.Invalidate();
            Up.Invalidate();
            Minus.Invalidate();
            Plus.Invalidate();
            SliderY.Invalidate();
            FilterCap.Invalidate();
            FilterTxt.Invalidate();

            CheckButtons();
            base.OnEnabledChanged(e);
        }





        protected override void OnHandleCreated(System.EventArgs e)
        {
            base.OnHandleCreated(e);
            // Um den allerersten Check nicht zu verpassen
            CheckButtons();
        }


        private void ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e)
        {
            FloatingInputBoxListBoxStyle.Close(this);
            if (e.ClickedComand.Internal().ToLower() == "abbruch") { return; }
            OnContextMenuItemClicked(e);
        }

        private void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        private BasicListItem MouseOverNode(int X, int Y)
        {
            if (ButtonsVisible() && Y >= Height - Plus.Height) { return null; }
            return Item[X, Convert.ToInt32(Y + SliderY.Value)];
        }


        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!Enabled) { return; }

            var ND = MouseOverNode(e.X, e.Y);

            if (ND != null && !ND.Enabled) { return; }

            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    if (ND != null)
                    {
                        if (Appearance == enBlueListBoxAppearance.Listbox || Appearance == enBlueListBoxAppearance.Autofilter || Appearance == enBlueListBoxAppearance.Gallery || Appearance == enBlueListBoxAppearance.FileSystem)
                        {
                            if (ND.IsClickable()) { ND.Checked = !ND.Checked; }
                        }
                        OnItemClick(new BasicListItemEventArgs(ND));

                    }
                    break;

                case System.Windows.Forms.MouseButtons.Right:
                    ContextMenu_Show(this, e);
                    break;
            }

        }

        private void OnItemClick(BasicListItemEventArgs e)
        {
            ItemClick?.Invoke(this, e);
        }

        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!SliderY.Visible) { return; }
            SliderY.DoMouseWheel(e);
        }




        protected override void OnDoubleClick(System.EventArgs e)
        {
            if (!Enabled) { return; }

            var ND = MouseOverNode(MousePos().X, MousePos().Y);

            if (ND == null) { return; }

            OnItemDoubleClick(new BasicListItemEventArgs(ND));
        }


        private void OnItemDoubleClick(BasicListItemEventArgs e)
        {
            ItemDoubleClick?.Invoke(this, e);
        }

        private bool ButtonsVisible()
        {
            if (Plus.Visible) { return true; }
            if (Minus.Visible) { return true; }
            if (Up.Visible) { return true; }
            if (Down.Visible) { return true; }
            if (FilterTxt.Visible) { return true; }
            return false;
        }



        protected override void InitializeSkin()
        {

        }



        protected override void DrawControl(Graphics GR, enStates vState)
        {
            if (Item != null) { Item.Appearance = _Appearance; }
            var tmp = enDesign.ListBox;
            if (_Appearance != enBlueListBoxAppearance.Gallery && _Appearance != enBlueListBoxAppearance.FileSystem) { tmp = (enDesign)_Appearance; }

            var PaintModXx = 0;
            var PaintModYx = 0;
            var vStateBox = vState;

            if (Convert.ToBoolean(vStateBox & enStates.Standard_MouseOver)) { vStateBox = vStateBox ^ enStates.Standard_MouseOver; }
            if (Convert.ToBoolean(vStateBox & enStates.Standard_MousePressed)) { vStateBox = vStateBox ^ enStates.Standard_MousePressed; }
            if (Convert.ToBoolean(vStateBox & enStates.Standard_HasFocus)) { vStateBox = vStateBox ^ enStates.Standard_HasFocus; }

            if (Item.Count == 0)
            {
                SliderY.Visible = false;
                SliderY.Value = 0;
            }


            if (ButtonsVisible()) { PaintModYx = Plus.Height; }

            Item.ComputeAllItemPositions(new Size(DisplayRectangle.Width, DisplayRectangle.Height - PaintModYx), false, _MoveAllowed, _Appearance, this, SliderY);


            if (SliderY.Visible) { PaintModXx = SliderY.Width; }

            var BorderCoords = new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width - PaintModXx, DisplayRectangle.Height - PaintModYx);
            var VisArea = new Rectangle(BorderCoords.X, Convert.ToInt32(BorderCoords.Y + SliderY.Value), BorderCoords.Width, BorderCoords.Height);


            if (BorderCoords.Height > 0)
            {
                //// Kann sein, wenn PaintModY größer als die Höhe ist
                //if (_Appearance == enBlueListBoxAppearance.Listbox)
                //{
                Skin.Draw_Back(GR, tmp, vStateBox, BorderCoords, this, true);
                //}
                //else
                //{
                //    clsSkin.Draw_Back_Transparent(GR, DisplayRectangle, this);
                //}
            }

            _MouseOverItem = MouseOverNode(MousePos().X, MousePos().Y);

            foreach (var ThisItem in Item)
            {

                if (ThisItem.Pos.IntersectsWith(VisArea))
                {
                    var vStateItem = vStateBox;
                    if (_MouseOverItem == ThisItem && Enabled) { vStateItem |= enStates.Standard_MouseOver; }
                    if (!ThisItem.Enabled) { vStateItem = enStates.Standard_Disabled; }
                    if (ThisItem.Checked) { vStateItem |= enStates.Checked; }

                    ThisItem.Draw(GR, 0, Convert.ToInt32(SliderY.Value), vStateItem, true, FilterTxt.Text);
                }

            }

            if (BorderCoords.Height > 0)
            {
                // Kann sein, wenn PaintModY größer als die Höhe ist
                if (tmp == enDesign.ListBox) { Skin.Draw_Border(GR, tmp, vStateBox, BorderCoords); }
            }

            if (PaintModYx > 0) { clsSkin.Draw_Back_Transparent(GR, new Rectangle(0, BorderCoords.Bottom, Width, PaintModYx), this); }
        }

        private void SliderY_ValueChange(object sender, System.EventArgs e)
        {
            if (IsDisposed) { return; }
            Invalidate();
        }

        private void _Item_ListOrItemChanged(object sender, System.EventArgs e)
        {
            if (IsDisposed) { return; }
            Invalidate();
            OnChanged();
        }



        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var ND = MouseOverNode(MousePos().X, MousePos().Y);


            if (ND != _MouseOverItem)
            {
                _MouseOverItem = ND;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_MouseOverItem != null)
            {
                _MouseOverItem = null;
                Invalidate();
            }
        }







        private void _Item_ItemCheckedChanged(object sender, System.EventArgs e)
        {
            if (IsDisposed) { return; }

            CheckButtons();
            Invalidate();
            OnItemCheckedChanged();

        }

        private void OnItemCheckedChanged()
        {
            ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void SwapItems(BasicListItem Nr1, BasicListItem Nr2)
        {
            var ed = new ItemSwapEventArgs(Nr1, Nr2, false);

            OnUpOrDownClicked(ed);
            if (ed.AlreadyHandled) { return; }

            Item.Swap(ref Nr1, ref Nr2);
            CheckButtons();
        }

        private void OnUpOrDownClicked(ItemSwapEventArgs e)
        {
            UpOrDownClicked?.Invoke(this, e);
        }

        private void Up_Click(object sender, System.EventArgs e)
        {
            BasicListItem LN = null;

            foreach (var thisItem in Item)
            {
                if (thisItem != null)
                {

                    if (thisItem.Checked)
                    {
                        if (LN == null) { return; }// Befehl verwerfen...
                        SwapItems(LN, thisItem);
                        return;
                    }

                    LN = thisItem;
                }
            }



        }

        private void Down_Click(object sender, System.EventArgs e)
        {

            var LN = -1;

            for (var z = Item.Count - 1 ; z >= 0 ; z--)
            {
                if (Item[z] != null)
                {

                    if (Item[z].Checked)
                    {
                        if (LN < 0) { return; }// Befehl verwerfen...
                        SwapItems(Item[LN], Item[z]);
                        return;
                    }

                    LN = z;
                }
            }

        }



        private void Minus_Click(object sender, System.EventArgs e)
        {

            var Dia = GetEditType(true);

            OnRemoveClicked(new ListOfBasicListItemEventArgs(Item.Checked()));



            switch (Dia)
            {
                //case enEditTypeTable.RelationEditor_InTable:
                case enEditTypeTable.Textfeld:
                case enEditTypeTable.Textfeld_mit_Auswahlknopf:
                case enEditTypeTable.Dropdown_Single:
                case enEditTypeTable.Image_Auswahl_Dialog:
                case enEditTypeTable.None:
                    foreach (var ThisItem in Item.Checked())
                    {
                        Item.Remove(ThisItem);
                    }
                    break;

                case enEditTypeTable.FileHandling_InDateiSystem:


                    if (MessageBox.Show("Datei(en) unwiderruflich <b>löschen</b>?", enImageCode.Frage, "Ja", "Nein") != 0) { return; }
                    // Das ist eine Fake-Meldung.
                    // In Wahrheit werden die Dateien erst beim Speichern der Datenbank gelöscht. Um Ja Sicherzusetellen, dass keine weiteren Bezüge vorhanden sind.

                    foreach (var ThisItem in Item.Checked())
                    {
                        Item.Remove(ThisItem);
                    }

                    break;
                default:

                    Develop.DebugPrint(Dia);
                    break;
            }

            CheckButtons();

        }

        private void OnRemoveClicked(ListOfBasicListItemEventArgs e)
        {
            RemoveClicked?.Invoke(this, e);
        }

        private enEditTypeTable GetEditType(bool ThrowCellError)
        {

            var ed = new ColumnEventArgs(null);

            OnNeedColumn(ed);



            switch (_AddAlloweds)
            {
                case enAddType.CellDecide:
                    if (ThrowCellError && ed.Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "CellDecide fehlgeschlagen, keine Spalte vorhanden"); }
                    if (ed.Column == null) { return enEditTypeTable.None; }
                    return ColumnItem.UserEditDialogTypeInTable(ed.Column, false);

                case enAddType.Text:
                    if (ThrowCellError && ed.Column != null) { Develop.DebugPrint(enFehlerArt.Fehler, "Texteingabe fehlgeschlagen, Spalte vorhanden"); }
                    return enEditTypeTable.Textfeld;

                case enAddType.OnlySuggests:
                    if (ThrowCellError && ed.Column != null) { Develop.DebugPrint(enFehlerArt.Fehler, "OnlySuggests fehlgeschlagen, Spalte vorhanden"); }
                    return enEditTypeTable.Listbox;

                case enAddType.Images:
                    if (ThrowCellError && ed.Column != null) { Develop.DebugPrint(enFehlerArt.Fehler, "ImageAdd fehlgeschlagen, Spalte vorhanden"); }
                    return enEditTypeTable.Image_Auswahl_Dialog;

                case enAddType.BinaryAndImages:
                    if (ThrowCellError && ed.Column != null) { Develop.DebugPrint(enFehlerArt.Fehler, "BinaryAndImages fehlgeschlagen, Spalte vorhanden"); }
                    return enEditTypeTable.FileHandling_InDateiSystem;

                case enAddType.None:
                    if (ThrowCellError && ed.Column != null) { Develop.DebugPrint(enFehlerArt.Fehler, "None fehlgeschlagen, Spalte vorhanden"); }
                    return enEditTypeTable.None;

                case enAddType.OnlyInternalCoded:
                    if (ThrowCellError && ed.Column != null) { Develop.DebugPrint(enFehlerArt.Fehler, "OnlyInternalCoded fehlgeschlagen, Spalte vorhanden"); }
                    return enEditTypeTable.Textfeld; // Dummy

                default:
                    Develop.DebugPrint(_AddAlloweds);
                    return enEditTypeTable.None;
            }

        }

        private void OnNeedColumn(ColumnEventArgs d)
        {
            NeedColumn?.Invoke(this, d);
        }

        private void Plus_Click(object sender, System.EventArgs e)
        {

            var Dia = GetEditType(true);

            var Val = "";
            var ed = new AllreadyHandledEventArgs(false);

            OnAddClicked(ed);


            if (ed.AlreadyHandled) { return; }

            if (Convert.ToBoolean(AddAllowed & enAddType.OnlyInternalCoded))
            {
                Develop.DebugPrint("Add Ignoriert!");
                return;
            }


            Suggestions.CheckBehavior = enCheckBehavior.SingleSelection;

            var ce = new ColumnEventArgs(null);
            //var cr = new RowEventArgs(null);

            switch (Dia)
            {
                //case enEditTypeTable.RelationEditor_InTable:
                //    Develop.DebugPrint_NichtImplementiert();
                //    //OnNeedColumn(ce);
                //    //OnNeedRow(cr);

                //    //var REl = new clsRelation(ce.Column, cr.Row, "");
                //    //var nob = DialogBox.eEditClass(REl, false);
                //    //Val = nob == REl ? string.Empty : nob.ToString();
                //    break;


                case enEditTypeTable.Image_Auswahl_Dialog:

                    string[] d1 = null;
                    using (var f = new System.Windows.Forms.OpenFileDialog())
                    {
                        f.CheckFileExists = true;
                        f.CheckPathExists = true;
                        f.Multiselect = false;
                        f.InitialDirectory = Plus_Click_LastPfad;
                        f.Title = "Datei hinzufügen:";
                        f.ShowDialog();
                        d1 = f.FileNames;
                    }

                    if (d1 == null || d1.Length != 1) { return; }
                    if (FileExists(d1[0]))
                    {
                        if (d1[0].FileType() == enFileFormat.Image)
                        {
                            Item.Add(new BitmapListItem((Bitmap)modAllgemein.Image_FromFile(d1[0])));
                            Plus_Click_LastPfad = d1[0].FilePath();
                        }
                        else
                        {
                            MessageBox.Show("Unbekanntes Bild-Format.", enImageCode.Warnung, "OK");
                            return;
                        }
                    }


                    break;

                case enEditTypeTable.FileHandling_InDateiSystem:
                    OnNeedColumn(ce);
                    // korrektheit der Zelle bereits geprüft
                    if (ce.Column != null && string.IsNullOrEmpty(Plus_Click_LastPfad)) { Plus_Click_LastPfad = ce.Column.Database.Filename.FilePath(); }

                    string[] d2 = null;
                    using (var f = new System.Windows.Forms.OpenFileDialog())
                    {
                        f.CheckFileExists = true;
                        f.CheckPathExists = true;
                        f.Multiselect = true;
                        f.InitialDirectory = Plus_Click_LastPfad;
                        f.Title = "Datei(en) hinzufügen:";
                        f.ShowDialog();
                        d2 = f.FileNames;
                    }


                    if (d2 == null || d2.Length == 0) { return; }

                    var DelList = new List<string>();
                    for (var z = 0 ; z <= d2.GetUpperBound(0) ; z++)
                    {
                        if (FileExists(d2[z]))
                        {


                            if (ce.Column != null && ce.Column.Format == enDataFormat.Link_To_Filesystem)
                            {

                                var b = modConverter.FileToByte(d2[z]);

                                if (!string.IsNullOrEmpty(ce.Column.Database.FileEncryptionKey)) { b = modAllgemein.SimpleCrypt(b, ce.Column.Database.FileEncryptionKey, 1); }

                                var neu = d2[z].FileNameWithSuffix();
                                neu = ce.Column.FreeFileName(neu.FileNameWithoutSuffix(), neu.FileSuffix());
                                Plus_Click_LastPfad = d2[z].FilePath();

                                modConverter.ByteToFile(neu, b);


                                Item.Add(neu.FileNameWithSuffix(), ce.Column, enShortenStyle.Replaced);

                            }
                            else
                            {
                                var x = new clsNamedBinary();
                                x.LoadFromFile(d2[z]);
                                Item.Add(x);
                            }

                            DelList.Add(d2[z]);
                        }

                    }

                    FileOperations.DeleteFile(DelList);
                    return;

                case enEditTypeTable.Textfeld:
                    Val = InputBoxComboStyle.Show("Bitte geben sie einen Wert ein:", Suggestions, true);
                    if (string.IsNullOrEmpty(Val)) { return; }
                    break;

                case enEditTypeTable.Listbox:
                    if (Suggestions == null || Suggestions.Count == 0)
                    {
                        MessageBox.Show("Keine (weiteren) Werte vorhanden.", enImageCode.Information, "OK");
                        return;
                    }

                    var rück = InputBoxListBoxStyle.Show("Bitte wählen sie einen Wert:", Suggestions, enAddType.None, true);
                    if (rück == null || rück.Count == 0) { return; }
                    Val = rück[0];
                    break;

                case enEditTypeTable.None:
                    return;

                default:
                    Develop.DebugPrint(Dia);
                    return;
            }


            if (string.IsNullOrEmpty(Val)) { return; }

            foreach (var thisItem in Item)
            {
                if (thisItem != null && thisItem.Internal().ToUpper() == Val.ToUpper()) { return; }
            }

            var i = new TextListItem(Val, Val);
            Item.Add(i);
            i.Checked = true;


            CheckButtons();
        }

        //private void OnNeedRow(RowEventArgs e)
        //{
        //    NeedRow?.Invoke(this, e);
        //}

        private void OnAddClicked(AllreadyHandledEventArgs e)
        {
            AddClicked?.Invoke(this, e);
        }


        public void ContextMenu_Show(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var UserMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);



            var ND = MouseOverNode(e.X, e.Y);

            OnContextMenuInit(new ContextMenuInitEventArgs(ND, UserMenu));



            if (UserMenu.Count > 0)
            {
                UserMenu.Add(new LineListItem());
                UserMenu.Add(enContextMenuComands.Abbruch);

                var _ContextMenu = FloatingInputBoxListBoxStyle.Show(UserMenu, ND, this);
                _ContextMenu.ItemClicked += ContextMenuItemClickedInternalProcessig;
            }

        }
        private void OnContextMenuInit(ContextMenuInitEventArgs e)
        {
            ContextMenuInit?.Invoke(this, e);
        }

        public new void Focus()
        {
            if (Focused()) { return; }
            base.Focus();
        }

        public new bool Focused()
        {
            if (base.Focused) { return true; }
            if (Plus.Focused) { return true; }
            if (Minus.Focused) { return true; }
            if (Up.Focused) { return true; }
            if (Down.Focused) { return true; }
            if (SliderY.Focused()) { return true; }
            if (FilterCap.Focused) { return true; }
            if (FilterTxt.Focused()) { return true; }

            return false;
        }

        private void _Item_Item_Added(object sender, ListEventArgs e)
        {
            if (IsDisposed) { return; }

            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            Invalidate();

            OnItemAdded(e);

        }


        protected void OnItemAdded(ListEventArgs e)
        {
            ItemAdded?.Invoke(this, e);
        }


        protected void OnItemRemoved(System.EventArgs e)
        {
            ItemRemoved?.Invoke(this, e);
        }


        private void _Item_ItemRemoved(object sender, System.EventArgs e)
        {
            if (IsDisposed) { return; }
            Invalidate();
            OnItemRemoved(e);
        }

        private void FilterTxt_TextChanged(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        public void OnChanged()
        {
            Changed?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
