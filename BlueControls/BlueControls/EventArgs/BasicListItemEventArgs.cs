using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls.EventArgs
{
  public  class BasicListItemEventArgs : System.EventArgs
    {
        public BasicListItemEventArgs(BasicListItem Item)
        {
            this.Item = Item;


        }

        public BasicListItem Item { get; set; }


    }
}
