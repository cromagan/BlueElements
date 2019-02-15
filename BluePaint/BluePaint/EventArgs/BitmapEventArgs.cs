using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePaint.EventArgs
{
    public class BitmapEventArgs : System.EventArgs
    {


        public BitmapEventArgs(Bitmap BMP)
        {
            this.BMP = BMP;
        }

        public Bitmap BMP { get; set; }

    }
}
