using System.Drawing;

namespace BluePaint
{
    public partial class Tool_Paint
    {

        public Tool_Paint()
        {
            InitializeComponent();
        }


        public override void MouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            OnForceUndoSaving();
            ClearPreviewPic();
            MouseMove(e);
        }

        public override void MouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            SolidBrush Brush_RotTransp = new SolidBrush(Color.FromArgb(128, 255, 0, 0));

            if (e.Button == System.Windows.Forms. MouseButtons.Left)
            {

                if (IsInsidePic(e))
                {
                    Graphics gr = Graphics.FromImage(_Pic);
                    Rectangle r = new Rectangle(e.X - 1, e.Y - 1, 3, 3);
                    gr.FillEllipse(Brushes.Black, r);
                    OnPicChangedByTool();
                }

            }
            else
            {

                if (IsInsidePic(e))
                {
                    ClearPreviewPic();
                    Graphics gr = Graphics.FromImage(_PicPreview);
                    Rectangle r = new Rectangle(e.X - 1, e.Y - 1, 3, 3);
                    gr.FillEllipse(Brush_RotTransp, r);
                }

                OnPicChangedByTool();
            }

        }
    }
}