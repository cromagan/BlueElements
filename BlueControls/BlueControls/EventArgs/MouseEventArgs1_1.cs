using System.Windows.Forms;

namespace BlueControls.EventArgs
{
    public sealed class MouseEventArgs1_1 : MouseEventArgs
    {


        public MouseEventArgs1_1(MouseButtons button, int clicks, int x, int y, int delta, int trimmedX, int trimmedy, bool isinPic) : base(button, clicks, x, y, delta)
        {
            this.IsInPic = isinPic;

            this.TrimmedX = trimmedX;
            this.TrimmedY = trimmedy;

        }




        public int TrimmedX { get; }

        public int TrimmedY { get; }

        public bool IsInPic { get; }



    }
}