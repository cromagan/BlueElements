namespace BlueBasics.EventArgs
{
    public class ListEventArgs : System.EventArgs
    {
        public object Item { get; }

        public ListEventArgs(object Item)
        {
            this.Item = Item;
        }
    }
}
