namespace BlueDatabase.EventArgs
{
    public class DatabaseChangedEventArgs : System.EventArgs
    {


        public DatabaseChangedEventArgs(bool OnlyReloaded)
        {
            this.OnlyReloaded = OnlyReloaded;
        }

        public bool OnlyReloaded { get; set; }

    }
}
