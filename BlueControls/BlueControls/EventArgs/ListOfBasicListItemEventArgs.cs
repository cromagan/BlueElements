using System.Collections.Generic;
using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls.EventArgs
{
    public class ListOfBasicListItemEventArgs : System.EventArgs
    {

        public ListOfBasicListItemEventArgs(List<BasicListItem> Items)
        {
            this.Items = Items;
        }

        public List<BasicListItem> Items { get; set; }


    }
}
