using BlueBasics.EventArgs;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls.Controls {

    public partial class SwapListBox : GenericControl, IBackgroundNone {

        #region Constructors

        public SwapListBox() => InitializeComponent();

        #endregion

        #region Events

        public event EventHandler? AddClicked;

        public event EventHandler<ListEventArgs>? ItemAdded;

        /// <summary>
        /// Wird nach jedem entfernen eines Items ausgelöst. Auch beim Initialisiern oder bei einem Clear.
        /// Soll eine Benutzerinteraktion abgefragt werden, ist RemoveClicked besser.
        /// </summary>
        public event EventHandler? ItemRemoved;

        #endregion

        #region Properties

        public enAddType AddAllowed {
            get => Main.AddAllowed;
            set => Main.AddAllowed = value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ItemCollectionList Item => Main.Item;

        #endregion

        #region Methods

        public void OnAddClicked() => AddClicked?.Invoke(this, System.EventArgs.Empty);

        internal void SuggestionsAdd(ItemCollectionList? item) {
            if (item == null) { return; }
            foreach (var thisi in item.Where(thisi => Main.Item[thisi.Internal] == null && Suggest.Item[thisi.Internal] == null)) {
                thisi.Checked = false;
                Suggest.Item.Add((BasicListItem)thisi.Clone());
            }
        }

        protected override void DrawControl(Graphics gr, enStates state) {
        }

        protected void MoveItemBetweenList(ListBox Source, ListBox Target, string Internal, bool doRemove) {
            var SourceItem = Source.Item[Internal];
            var TargetItem = Target.Item[Internal];
            if (SourceItem != null && TargetItem == null) {
                Target.Item.Add((BasicListItem)SourceItem.Clone());
            } else if (SourceItem == null && TargetItem == null) {
                TargetItem = new TextListItem(Internal, Internal, null, false, true, string.Empty);
                Target.Item.Add(TargetItem);
            }
            //var SourceItem = Source.Item[Internal];
            //var TargetItem = Target.Item[Internal];
            //if (SourceItem == null && TargetItem == null)
            //{
            //    TargetItem = new TextListItem(Internal);
            //    Target.Item.Add(TargetItem);
            //}
            Target.Item.Sort();
            if (SourceItem != null && doRemove) { Source.Item.Remove(SourceItem); }
        }

        //private void Main_ContextMenuItemClicked(object sender, EventArgs.ContextMenuItemClickedEventArgs e) {
        //    OnContextMenuItemClicked(e);
        //}
        //private void Main_ContextMenuInit(object sender, EventArgs.ContextMenuInitEventArgs e) {
        //    OnContextMenuInit(e);
        //}
        protected override void OnEnabledChanged(System.EventArgs e) {
            base.OnEnabledChanged(e);
            Main.Enabled = Enabled;
            Suggest.Enabled = Enabled;
        }

        protected void OnItemAdded(ListEventArgs e) {
            if (IsDisposed) { return; }
            ItemAdded?.Invoke(this, e);
        }

        protected void OnItemRemoved(System.EventArgs e) {
            if (IsDisposed) { return; }
            ItemRemoved?.Invoke(this, e);
        }

        private void Main_AddClicked(object sender, System.EventArgs e) => OnAddClicked();

        private void Main_ItemAdded(object sender, ListEventArgs e) {
            MoveItemBetweenList(Suggest, Main, ((BasicListItem)e.Item).Internal, true);
            OnItemAdded(e);
        }

        private void Main_ItemClicked(object sender, EventArgs.BasicListItemEventArgs e) => MoveItemBetweenList(Main, Suggest, e.Item.Internal, true);

        private void Main_ItemRemoved(object sender, System.EventArgs e) => OnItemRemoved(e);

        private void Main_ItemRemoving(object sender, ListEventArgs e) => MoveItemBetweenList(Main, Suggest, ((BasicListItem)e.Item).Internal, false);

        private void Suggest_ItemClicked(object sender, EventArgs.BasicListItemEventArgs e) => MoveItemBetweenList(Suggest, Main, e.Item.Internal, true);

        #endregion

        //public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) {
        //    ContextMenuItemClicked?.Invoke(this, e);
        //}
        //public void OnContextMenuInit(ContextMenuInitEventArgs e) {
        //    ContextMenuInit?.Invoke(this, e);
        //}
        //public void GetContextMenuItems(MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate) { }
        //public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) { }
    }
}