using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlueBasics.Enums;
using BlueControls.Enums;
using static BlueBasics.modAllgemein;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls
{
    public partial class ZoomPicWithPoints : ZoomPic
    {
        public ZoomPicWithPoints()
        {
            InitializeComponent();
        }


        List<PointDF> points = new List<PointDF>();


        public List<string> Tags = new List<string>();


        private enOrientation _MittelLinie = (enOrientation)(-1);



        public string Feedback = string.Empty;

        private enSelectModus _SelectModus = enSelectModus.Ohne;


        private enHelpers _Helper = enHelpers.Ohne;


        [DefaultValue((enOrientation)(-1))]
        public enOrientation Mittellinie
        {
            get
            {
                return _MittelLinie;
            }
            set
            {


                if (_MittelLinie == value) { return; }
                _MittelLinie = value;
                Invalidate();
            }

        }


        [DefaultValue(enHelpers.Ohne)]
        public enHelpers Helper
        {
            get
            {
                return _Helper;
            }
            set
            {


                if (_Helper == value) { return; }
                _Helper = value;
                Invalidate();
            }

        }

        [DefaultValue(enSelectModus.Ohne)]
        public enSelectModus SelectModus
        {
            get
            {
                return _SelectModus;
            }
            set
            {
                if (_SelectModus == value) { return; }
                _SelectModus = value;
                Invalidate();
            }

        }


        protected override RectangleDF MaxBounds()
        {

            var r = base.MaxBounds();
            foreach (var thisP in points)
            {
                r.X = Math.Min(r.X, thisP.X);
                r.Y = Math.Min(r.Y, thisP.Y);
                r.Width = Math.Max(r.Width, thisP.X - r.X);
                r.Height = Math.Max(r.Height, thisP.Y - r.Y);
            }

            return r;

        }

        protected override void DrawControl(Graphics gr, enStates state)
        {

            // Bild und Hintergrund
            base.DrawControl(gr, state);


            /// Punkre
            foreach (var ThisPoint in points)
            {
                ThisPoint.Draw(gr, _Zoom, _MoveX, _MoveY, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard);
            }


            // Mittellinie
            var PicturePos = base.MaxBounds();

            if (_MittelLinie.HasFlag(enOrientation.Waagerecht))
            {
                var p1 = PicturePos.PointOf(enAlignment.VerticalCenter_Left).ZoomAndMove(_Zoom, _MoveX, _MoveY);
                var p2 = PicturePos.PointOf(enAlignment.VerticalCenter_Right).ZoomAndMove(_Zoom, _MoveX, _MoveY);
                gr.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
                gr.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
            }
            if (_MittelLinie.HasFlag(enOrientation.Senkrecht))
            {
                var p1 = PicturePos.PointOf(enAlignment.Top_HorizontalCenter).ZoomAndMove(_Zoom, _MoveX, _MoveY);
                var p2 = PicturePos.PointOf(enAlignment.Bottom_HorizontalCenter).ZoomAndMove(_Zoom, _MoveX, _MoveY);
                gr.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
                gr.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
            }


            // Hilfslinien


            //if (_Helper.HasFlag(enHelpers.SymetricalHorizontal))
            //    {

            //}


        }




        public void LoadData(string PathOfPicture)
        {


            if (FileExists(PathOfPicture))
            {
                BMP = (Bitmap)Image_FromFile(PathOfPicture);
            }

        }

    }

}
