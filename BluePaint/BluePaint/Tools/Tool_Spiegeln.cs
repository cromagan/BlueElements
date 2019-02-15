using BluePaint.EventArgs;
using System.Drawing;
using System.Drawing.Drawing2D;
using static BlueBasics.modAllgemein;
using static BlueBasics.Extensions;

namespace BluePaint
{
    public partial class Tool_Spiegeln
    {

        public Tool_Spiegeln()
        {
            InitializeComponent();
        }

        private void SpiegelnH_Click(object sender, System.EventArgs e)
        {
            CollectGarbage();
            Bitmap ni = new Bitmap(_Pic.Width, _Pic.Height);
            Graphics gr = Graphics.FromImage(ni);
            gr.PixelOffsetMode = PixelOffsetMode.Half;
            gr.DrawImage(_Pic.Image_Clone(), 0, ni.Height, ni.Width, -ni.Height);
            gr.Dispose();

            OnOverridePic(new BitmapEventArgs(ni));
        }

        private void SpiegelnV_Click(object sender, System.EventArgs e)
        {
            CollectGarbage();
            Bitmap ni = new Bitmap(_Pic.Width, _Pic.Height);
            Graphics gr = Graphics.FromImage(ni);
            gr.PixelOffsetMode = PixelOffsetMode.Half;
            gr.DrawImage(_Pic.Image_Clone(), ni.Width, 0, -ni.Width, ni.Height);
            gr.Dispose();

            OnOverridePic(new BitmapEventArgs(ni));
        }
    }

}