namespace BlueDatabase.EventArgs
{
    public class LoadedEventArgs : System.EventArgs
    {


        public LoadedEventArgs(bool OnlyReloaded)
        {
            this.OnlyReloaded = OnlyReloaded;
        }

        public bool OnlyReloaded { get; set; }

    }
}
