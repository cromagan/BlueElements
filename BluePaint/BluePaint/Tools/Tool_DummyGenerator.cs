using BlueBasics;
using BlueBasics.Enums;
using BlueControls.DialogBoxes;
using BluePaint.EventArgs;
using System;
using System.Drawing;

namespace BluePaint
{
    public partial class Tool_DummyGenerator
    {

        public Tool_DummyGenerator()
        {
            InitializeComponent();
        }
        private void Erstellen_Click(object sender, System.EventArgs e)
        {
            CreateDummy();
        }

        private void CreateDummy()
        {
            var W = modErgebnis.Ergebnis(X.Text);
            var H = modErgebnis.Ergebnis(Y.Text);

            if (W == null || W < 2)
            {
                Notification.Show("Bitte Breite eingeben.", enImageCode.Information);
                return;
            }


            if (H == null || H < 2)
            {
                Notification.Show("Bitte Höhe eingeben.", enImageCode.Information);
                return;
            }


            var _Pic = new Bitmap(Convert.ToInt32(W), Convert.ToInt32(H));


            var gr = Graphics.FromImage(_Pic);

            gr.Clear(Color.White);
            gr.DrawRectangle(new Pen(Color.Black, 2), 1, 1, _Pic.Width - 2, _Pic.Height - 2);

            if (!string.IsNullOrEmpty(TXT.Text))
            {

                var f = new Font("Arial", 50, FontStyle.Bold);

                var fs = gr.MeasureString(TXT.Text, f);

                gr.TranslateTransform((float)(_Pic.Width / 2.0), (float)(_Pic.Height / 2.0));

                gr.RotateTransform(-90);

                gr.DrawString(TXT.Text, f, new SolidBrush(Color.Black), new PointF((float)(-fs.Width / 2.0), (float)(-fs.Height / 2.0)));


            }

            OnOverridePic(new BitmapEventArgs(_Pic));
        }
    }
}