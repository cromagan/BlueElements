using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls.EventArgs
{
    public class ContextMenuInitEventArgs : System.EventArgs
    {


        public ContextMenuInitEventArgs(object Tag, ItemCollectionList UserMenu)
        {
            this.UserMenu = UserMenu;
            this.Tag = Tag;

        }


        public ItemCollectionList UserMenu { get; set; }

        public object Tag { get; set; }

    }
}
