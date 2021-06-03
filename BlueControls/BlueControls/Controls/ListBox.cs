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
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("ItemClicked")]
    public partial class ListBox : GenericControl, IContextMenu, IBackgroundNone {

        #region Constructor
        public ListBox() : base(true, false) {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Item = new ItemCollectionList();
            Item.Changed += _Item_ListOrItemChanged;
            Item.ItemCheckedChanged += _Item_ItemCheckedChanged;
            Item.ItemAdded += _Item_ItemAdded;
            Item.ItemRemoved += _Item_ItemRemoved;
            Item.ItemRemoving += _Item_ItemRemoving;
            _Appearance = enBlueListBoxAppearance.Listbox;
        }

        #endregion

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

        /// <summary>
        /// Wird vor jedem entfernen eines Items ausgelöst. Auch beim Initialisiern oder bei einem Clear.
        /// Soll eine Benutzerinteraktion abgefragt werden, ist RemoveClicked besser. 
        /// </summary>
        public event EventHandler<ListEventArgs> ItemRemoving;
        /// <summary>
        /// Wird nach jedem entfernen eines Items ausgelöst. Auch beim Initialisiern oder bei einem Clear.
        /// Soll eine Benutzerinteraktion abgefragt werden, ist RemoveClicked besser. 
        /// </summary>
        public event EventHandler ItemRemoved;
        public event EventHandler<BasicListItemEventArgs> ItemDoubleClick;
        public event EventHandler<BasicListItemEventArgs> ItemClicked;
        public event EventHandler AddClicked;
        /// <summary>
        /// Wird nur ausgelöst, wenn explicit der Button gedrückt wird.
        /// </summary>
        public event EventHandler<ListOfBasicListItemEventArgs> RemoveClicked;
        public event EventHandler ListOrItemChanged;

        #endregion

        #region  Properties 

        [DefaultValue(enCheckBehavior.SingleSelection)]
        public enCheckBehavior CheckBehavior {
            get => Item.CheckBehavior;
            set => Item.CheckBehavior = value;
        }

        public string LastFilePath { get; set; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionList Item { get; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionList Suggestions { get; } = new ItemCollectionList();

        [DefaultValue(enBlueListBoxAppearance.Listbox)]
        public enBlueListBoxAppearance Appearance {
            get => _Appearance;
            set {
                if (_Appearance == value) { return; }
                _Appearance = value;
                Item.Appearance = value;
            }
        }

        [DefaultValue(false)]
        public bool RemoveAllowed {
            get => _RemoveAllowed;
            set {
                if (_RemoveAllowed == value) { return; }
                _RemoveAllowed = value;
                CheckButtons();
            }
        }

        [DefaultValue(true)]
        public enAddType AddAllowed {
            get => _AddAlloweds;
            set {
                if (_AddAlloweds == value) { return; }
                _AddAlloweds = value;
                CheckButtons();
            }
        }

        [DefaultValue(false)]
        public bool MoveAllowed {
            get => _MoveAllowed;
            set {
                if (_MoveAllowed == value) { return; }
                _MoveAllowed = value;
                ////if (_MoveAllowed) { _FilterAllowed = false; }
                CheckButtons();
            }
        }

        [DefaultValue(false)]
        public bool FilterAllowed {
            get => _FilterAllowed;
            set {
                //  if (_MoveAllowed) { value = false; }
                if (_FilterAllowed == value) { return; }
                _FilterAllowed = value;
                CheckButtons();
            }
        }

        #endregion

        protected override void OnVisibleChanged(System.EventArgs e) {
            CheckButtons();
            base.OnVisibleChanged(e);
        }

        private void CheckButtons() {
            if (!Visible) { return; }
            if (Parent == null) { return; }

            var nr = Item.Checked();

            Down.Visible = _MoveAllowed;
            Up.Visible = _MoveAllowed;
            Plus.Visible = _AddAlloweds != enAddType.None;
            Minus.Visible = _RemoveAllowed;
            FilterTxt.Visible = _FilterAllowed;
            FilterCap.Visible = _FilterAllowed;

            FilterCap.Left = _MoveAllowed && _FilterAllowed ? Down.Right : 0;

            FilterTxt.Left = FilterCap.Right;
            FilterTxt.Width = Minus.Left - FilterTxt.Left;

            if (_RemoveAllowed) {
                Minus.Enabled = nr.Count != 0;
            }

            if (_MoveAllowed) {
                if (nr.Count != 1) {
                    Up.Enabled = false;
                    Down.Enabled = false;
                } else {
                    Up.Enabled = Item[0] != nr[0];
                    Down.Enabled = Item[Item.Count - 1] != nr[0];
                }
            }
        }

        protected override void OnParentEnabledChanged(System.EventArgs e) {
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

        protected override void OnHandleCreated(System.EventArgs e) {
            base.OnHandleCreated(e);
            // Um den allerersten Check nicht zu verpassen
            CheckButtons();
        }

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
            return false;
        }

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        private BasicListItem MouseOverNode(int X, int Y) {
            return ButtonsVisible() && Y >= Height - Plus.Height ? null : Item[X, (int)(Y + SliderY.Value)];
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);
            if (!Enabled) { return; }

            var ND = MouseOverNode(e.X, e.Y);

            if (ND != null && !ND.Enabled) { return; }

            switch (e.Button) {
                case System.Windows.Forms.MouseButtons.Left:
                    if (ND != null) {
                        if (Appearance == enBlueListBoxAppearance.Listbox || Appearance == enBlueListBoxAppearance.Autofilter || Appearance == enBlueListBoxAppearance.Gallery || Appearance == enBlueListBoxAppearance.FileSystem) {
                            if (ND.IsClickable()) { ND.Checked = !ND.Checked; }
                        }
                        OnItemClicked(new BasicListItemEventArgs(ND));

                    }
                    break;

                case System.Windows.Forms.MouseButtons.Right:
                    FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                    break;
            }
        }

        private void OnItemClicked(BasicListItemEventArgs e) {
            ItemClicked?.Invoke(this, e);
        }

        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseWheel(e);
            if (!SliderY.Visible) { return; }
            SliderY.DoMouseWheel(e);
        }

        protected override void OnDoubleClick(System.EventArgs e) {
            if (!Enabled) { return; }

            var ND = MouseOverNode(MousePos().X, MousePos().Y);

            if (ND == null) { return; }

            OnItemDoubleClick(new BasicListItemEventArgs(ND));
        }

        private void OnItemDoubleClick(BasicListItemEventArgs e) {
            ItemDoubleClick?.Invoke(this, e);
        }

        private bool ButtonsVisible() {
            return Plus.Visible || Minus.Visible || Up.Visible || Down.Visible || FilterTxt.Visible;
        }

        protected override void DrawControl(Graphics gr, enStates state) {
            if (Item != null) { Item.Appearance = _Appearance; }
            var tmp = enDesign.ListBox;
            if (_Appearance != enBlueListBoxAppearance.Gallery && _Appearance != enBlueListBoxAppearance.FileSystem) { tmp = (enDesign)_Appearance; }

            var PaintModXx = 0;
            var PaintModYx = 0;
            var vStateBox = state;

            if (Convert.ToBoolean(vStateBox & enStates.Standard_MouseOver)) { vStateBox ^= enStates.Standard_MouseOver; }
            if (Convert.ToBoolean(vStateBox & enStates.Standard_MousePressed)) { vStateBox ^= enStates.Standard_MousePressed; }
            if (Convert.ToBoolean(vStateBox & enStates.Standard_HasFocus)) { vStateBox ^= enStates.Standard_HasFocus; }

            if (Item.Count == 0) {
                SliderY.Visible = false;
                SliderY.Value = 0;
            }

            if (ButtonsVisible()) { PaintModYx = Plus.Height; }

            (var BiggestItemX, var _, var HeightAdded, var SenkrechtAllowed) = Item.ItemData();

            Item.ComputeAllItemPositions(new Size(DisplayRectangle.Width, DisplayRectangle.Height - PaintModYx), SliderY, BiggestItemX, HeightAdded, SenkrechtAllowed);

            if (SliderY.Visible) { PaintModXx = SliderY.Width; }

            var BorderCoords = new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width - PaintModXx, DisplayRectangle.Height - PaintModYx);
            var VisArea = new Rectangle(BorderCoords.X, (int)(BorderCoords.Y + SliderY.Value), BorderCoords.Width, BorderCoords.Height);

            if (BorderCoords.Height > 0) {
                //// Kann sein, wenn PaintModY größer als die Höhe ist
                //if (_Appearance == enBlueListBoxAppearance.Listbox)
                //{
                Skin.Draw_Back(gr, tmp, vStateBox, BorderCoords, this, true);
                //}
                //else
                //{
                //    clsSkin.Draw_Back_Transparent(GR, DisplayRectangle, this);
                //}
            }

            _MouseOverItem = MouseOverNode(MousePos().X, MousePos().Y);

            var _locker = new object();

            System.Threading.Tasks.Parallel.ForEach(Item, ThisItem => {

                if (ThisItem.Pos.IntersectsWith(VisArea)) {
                    var vStateItem = vStateBox;
                    if (_MouseOverItem == ThisItem && Enabled) { vStateItem |= enStates.Standard_MouseOver; }
                    if (!ThisItem.Enabled) { vStateItem = enStates.Standard_Disabled; }
                    if (ThisItem.Checked) { vStateItem |= enStates.Checked; }

                    lock (_locker) {
                        ThisItem.Draw(gr, 0, (int)SliderY.Value, Item.ControlDesign, Item.ItemDesign, vStateItem, true, FilterTxt.Text, false); // Items müssen beim Erstellen ersetzt werden!!!!
                    }
                }
            });

            if (BorderCoords.Height > 0) {
                // Kann sein, wenn PaintModY größer als die Höhe ist
                if (tmp == enDesign.ListBox) { Skin.Draw_Border(gr, tmp, vStateBox, BorderCoords); }
            }

            if (PaintModYx > 0) { Skin.Draw_Back_Transparent(gr, new Rectangle(0, BorderCoords.Bottom, Width, PaintModYx), this); }
        }

        private void SliderY_ValueChange(object sender, System.EventArgs e) {
            if (IsDisposed) { return; }
            Invalidate();
        }

        private void _Item_ListOrItemChanged(object sender, System.EventArgs e) {
            if (IsDisposed) { return; }
            Invalidate();
            OnListOrItemChanged();
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);

            var ND = MouseOverNode(MousePos().X, MousePos().Y);

            if (ND != _MouseOverItem) {
                _MouseOverItem = ND;
                Invalidate();
                DoQuickInfo();
            }
        }

        protected override void OnMouseLeave(System.EventArgs e) {
            base.OnMouseLeave(e);
            if (_MouseOverItem != null) {
                _MouseOverItem = null;
                Invalidate();
            }
        }

        public override string QuickInfoText {
            get {
                var t1 = base.QuickInfoText;
                var t2 = string.Empty;

                if (_MouseOverItem != null) { t2 = _MouseOverItem.QuickInfo; }

                if (string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2)) {
                    return string.Empty;
                } else if (string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2)) {
                    return t1 + "<br><hr><br>" + t2;
                } else {
                    return t1 + t2; // Eins davon ist leer
                }
            }
        }

        private void _Item_ItemCheckedChanged(object sender, System.EventArgs e) {
            if (IsDisposed) { return; }

            CheckButtons();
            Invalidate();
            OnItemCheckedChanged();

        }

        private void OnItemCheckedChanged() {
            ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);
        }

        //private void SwapItems(BasicListItem Nr1, BasicListItem Nr2)
        //{
        //    Item.Swap(ref Nr1, ref Nr2);
        //    CheckButtons();
        //}

        private void Up_Click(object sender, System.EventArgs e) {
            BasicListItem LN = null;

            foreach (var thisItem in Item) {
                if (thisItem != null) {

                    if (thisItem.Checked) {
                        if (LN == null) { return; }// Befehl verwerfen...
                        Item.Swap(Item.IndexOf(LN), Item.IndexOf(thisItem));
                        CheckButtons();
                        return;
                    }

                    LN = thisItem;
                }
            }
        }

        private void Down_Click(object sender, System.EventArgs e) {

            var LN = -1;

            for (var z = Item.Count - 1; z >= 0; z--) {
                if (Item[z] != null) {

                    if (Item[z].Checked) {
                        if (LN < 0) { return; }// Befehl verwerfen...
                        Item.Swap(LN, z);
                        CheckButtons();
                        return;
                    }

                    LN = z;
                }
            }
        }

        private void Minus_Click(object sender, System.EventArgs e) {

            OnRemoveClicked(new ListOfBasicListItemEventArgs(Item.Checked()));

            foreach (var ThisItem in Item.Checked()) {
                Item.Remove(ThisItem);
            }

            CheckButtons();
        }

        private void OnRemoveClicked(ListOfBasicListItemEventArgs e) {
            RemoveClicked?.Invoke(this, e);
        }

        public BasicListItem Add_FromFileSystem() {

            using var f = new System.Windows.Forms.OpenFileDialog();
            f.CheckFileExists = true;
            f.CheckPathExists = true;
            f.Multiselect = false;
            f.InitialDirectory = LastFilePath;
            f.Title = "Datei hinzufügen:";
            f.ShowDialog();

            if (f.FileNames == null || f.FileNames.Length != 1) { return null; }

            //var x = new clsNamedBinary();
            //x.LoadFromFile(f.FileNames[0]);
            //Item.Add(x);
            var Picture = BitmapExt.Image_FromFile(f.FileNames[0]);

            return Picture != null
                ? Item.Add((Bitmap)Picture, f.FileNames[0])
                : Item.Add(modConverter.FileToByte(f.FileNames[0]), f.FileNames[0]);
        }

        public BasicListItem Add_TextBySuggestion() {
            if (Suggestions == null || Suggestions.Count == 0) {
                MessageBox.Show("Keine (weiteren) Werte vorhanden.", enImageCode.Information, "OK");
                return null;
            }
            Suggestions.CheckBehavior = enCheckBehavior.SingleSelection;

            var rück = InputBoxListBoxStyle.Show("Bitte wählen sie einen Wert:", Suggestions, enAddType.None, true);
            return rück == null || rück.Count == 0 ? null : Add_Text(rück[0]);
        }

        public TextListItem Add_Text(string Val) {
            if (string.IsNullOrEmpty(Val)) { return null; }

            foreach (var thisItem in Item) {
                if (thisItem != null && thisItem.Internal.ToUpper() == Val.ToUpper()) { return null; }
            }

            var i = Item.Add(Val, Val);
            i.Checked = true;

            return i;
        }

        public TextListItem Add_Text() {
            var Val = InputBoxComboStyle.Show("Bitte geben sie einen Wert ein:", Suggestions, true);
            return Add_Text(Val);
        }

        private void Plus_Click(object sender, System.EventArgs e) {
            OnAddClicked();

            switch (_AddAlloweds) {
                case enAddType.UserDef:
                    break;

                case enAddType.Text:
                    Add_Text();
                    break;

                case enAddType.OnlySuggests:
                    Add_TextBySuggestion();
                    break;

                case enAddType.None:
                    break;

                case enAddType.BinarysFromFileSystem:
                    Add_FromFileSystem();
                    break;

                default:
                    Develop.DebugPrint(_AddAlloweds);
                    break;

            }

            CheckButtons();
        }

        private void OnAddClicked() {
            AddClicked?.Invoke(this, System.EventArgs.Empty);
        }

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate) {
            HotItem = MouseOverNode(e.X, e.Y);
        }
        public void OnContextMenuInit(ContextMenuInitEventArgs e) {
            ContextMenuInit?.Invoke(this, e);
        }

        public new void Focus() {
            if (Focused()) { return; }
            base.Focus();
        }

        public new bool Focused() {
            return base.Focused || Plus.Focused || Minus.Focused || Up.Focused || Down.Focused || SliderY.Focused() || FilterCap.Focused || FilterTxt.Focused();
        }

        private void _Item_ItemAdded(object sender, ListEventArgs e) {
            if (IsDisposed) { return; }

            //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

            Invalidate();

            OnItemAdded(e);

        }

        protected void OnItemAdded(ListEventArgs e) {
            ItemAdded?.Invoke(this, e);
        }

        protected void OnItemRemoved(System.EventArgs e) {
            ItemRemoved?.Invoke(this, e);
        }
        protected void OnItemRemoving(ListEventArgs e) {
            ItemRemoving?.Invoke(this, e);
        }

        private void _Item_ItemRemoving(object sender, ListEventArgs e) {
            if (IsDisposed) { return; }
            Invalidate();
            OnItemRemoving(e);
        }

        private void _Item_ItemRemoved(object sender, System.EventArgs e) {
            if (IsDisposed) { return; }
            Invalidate();
            OnItemRemoved(e);
        }

        private void FilterTxt_TextChanged(object sender, System.EventArgs e) {
            Invalidate();
        }

        public void OnListOrItemChanged() {
            ListOrItemChanged?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
