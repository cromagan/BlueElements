using BlueBasics.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls.EventArgs
{
    public class ItemSwapEventArgs : AllreadyHandledEventArgs
    {


        public ItemSwapEventArgs(BasicListItem ItemToSwap1, BasicListItem ItemToSwap2, bool AllreadyHandled) : base(AllreadyHandled)
        {
            this.ItemToSwap1 = ItemToSwap1;
            this.ItemToSwap2 = ItemToSwap2;
        }

        public BasicListItem ItemToSwap1 { get; set; }
        public BasicListItem ItemToSwap2 { get; set; }

    }
}
