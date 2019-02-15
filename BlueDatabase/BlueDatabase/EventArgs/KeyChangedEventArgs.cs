namespace BlueDatabase.EventArgs
{
    public class KeyChangedEventArgs : System.EventArgs
    {


        public KeyChangedEventArgs(int KeyOld, int KeyNew)
        {
            this.KeyOld = KeyOld;
            this.KeyNew = KeyNew;
        }

        public int KeyOld { get; set; }
        public int KeyNew { get; set; }

    }
}
