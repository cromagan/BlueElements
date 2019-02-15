namespace BlueControls.EventArgs
{
   public class AllreadyHandledEventArgs : System.EventArgs
    {
        public AllreadyHandledEventArgs(bool AllreadyHandled)
        {
            AlreadyHandled = AllreadyHandled;
        }

        public bool AlreadyHandled { get; set; }
    }
}
