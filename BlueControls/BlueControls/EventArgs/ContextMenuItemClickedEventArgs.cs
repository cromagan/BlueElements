using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls.EventArgs
{
    public class ContextMenuItemClickedEventArgs : System.EventArgs
    {


        public ContextMenuItemClickedEventArgs(object Tag, BasicListItem ClickedComand)
        {
            this.Tag = Tag;
            this.ClickedComand = ClickedComand;
        }



        public BasicListItem ClickedComand { get; set; }
        public object Tag { get; set; }


    }
}
