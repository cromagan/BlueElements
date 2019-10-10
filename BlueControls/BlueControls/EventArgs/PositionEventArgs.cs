using BlueDatabase;
using BlueDatabase.EventArgs;

namespace BlueControls.EventArgs
{
    public class PositionEventArgs : System.EventArgs
    {


        public PositionEventArgs(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

    }
}
