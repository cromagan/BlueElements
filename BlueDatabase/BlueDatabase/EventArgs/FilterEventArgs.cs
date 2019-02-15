namespace BlueDatabase.EventArgs
{
    public class FilterEventArgs : System.EventArgs
    {

        public FilterEventArgs(FilterItem Filter)
        {
            this.Filter = Filter;
        }
        public FilterItem Filter { get; set; }
    }
}