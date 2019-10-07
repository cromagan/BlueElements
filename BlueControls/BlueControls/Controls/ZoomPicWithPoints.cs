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
using BlueControls.ItemCollection;
using static BlueBasics.Extensions;
using BlueBasics;

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
        private enSelectModus _SelectModus = enSelectModus.Ohne;
        public string Feedback = string.Empty;


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



            // Bild, Overlay und Hintergrund
            base.DrawControl(gr, state);


            /// Punkre
            foreach (var ThisPoint in points)
            {
                ThisPoint.Draw(gr, _Zoom, _MoveX, _MoveY, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard);
            }




            // Hilfslinien


            //if (_Helper.HasFlag(enHelpers.SymetricalHorizontal))
            //    {

            //}


        }




        public void LoadData(string PathOfPicture)
        {
            var x = LoadFromDisk(PathOfPicture);

            BMP = x.Item1;
            Tags = x.Item2;



            GeneratePointsFromTags();

            Invalidate();

        }

        private void GeneratePointsFromTags()
        {
            var Names = Tags.TagGet("AllPointNames").FromNonCritical().SplitByCRToList();

            points.Clear();

            foreach (var thisO in Names)
            {
                var s = Tags.TagGet(thisO);
                points.Add(new PointDF(null, s));
            }
        }

        public static BitmapListItem GenerateBitmapListItem(string PathOfPicture)
        {
            var x = LoadFromDisk(PathOfPicture);
            return GenerateBitmapListItem(x.Item1, x.Item2);
        }

        public static BitmapListItem GenerateBitmapListItem(Bitmap B, List<string> T)
        {
            var FilenamePNG = T.TagGet("Image-File");
            var i = new BitmapListItem(FilenamePNG, FilenamePNG.FileNameWithoutSuffix(), FilenamePNG, string.Empty);
            i.Padding = 10;
            i.Tags.AddRange(T);
            return i;
        }



        public BitmapListItem GenerateBitmapListItem()
        {
            WritePointsInTags();
            return ZoomPicWithPoints.GenerateBitmapListItem(BMP, Tags);
        }

        private void WritePointsInTags()
        {

            var Old = Tags.TagGet("AllPointNames").FromNonCritical().SplitByCRToList();

            foreach (var thisO in Old)
            {
                Tags.TagSet(thisO, string.Empty);
                Tags.TagSet(thisO + "-X", string.Empty);
                Tags.TagSet(thisO + "-Y", string.Empty);
            }



            var s = string.Empty;

            foreach (var ThisP in points)
            {
                s = s + ThisP.Name + "|";
                Tags.TagSet(ThisP.Name, ThisP.ToString());
                Tags.TagSet(ThisP.Name + "-X", ThisP.X.ToString());
                Tags.TagSet(ThisP.Name + "-Y", ThisP.Y.ToString());
            }


            Tags.TagSet("AllPointNames", s.ToNonCritical());

        }

        public PointDF GetPoint(string name)
        {
            foreach (var thisp in points)
            {
                if (thisp != null && thisp.Name.ToUpper() == name.ToUpper()) { return thisp; }
            }
            return null;

        }

        public void PointClear()
        {
            points.Clear();
            Invalidate();
        }

        public void PointSet(string name, int x, int y)
        {
            PointSet(name, (decimal)x, (decimal)y);
        }

        public void PointSet(string name, double x, double y)
        {
            PointSet(name, (decimal)x, (decimal)y);
        }

        public void PointSet(string name, decimal x, decimal y)
        {

            var p = GetPoint(name);

            if (p == null)
            {
                p = new PointDF(name, x, y);
                points.Add(p);
                Invalidate();
                return;
            }


            if (p.X != x || p.Y != y)
            {
                p.X = x;
                p.Y = y;
                Invalidate();
            }
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Invalidate(); // Mousedown bereits in _MouseDown gespeichert

        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);

        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);


        }



        public static Tuple<Bitmap, List<string>> LoadFromDisk(string PathOfPicture)
        {



            Bitmap B = null;
            var T = new List<string>();


            if (FileExists(PathOfPicture))
            {
                B = (Bitmap)Image_FromFile(PathOfPicture);
            }

            var FilenameTXT = PathOfPicture.TrimEnd(".PNG").TrimEnd(".JPG").TrimEnd(".JPG") + ".txt";


            if (FileExists(FilenameTXT))
            {
                T = modAllgemein.LoadFromDisk(FilenameTXT).SplitByCRToList();
            }


            T.TagSet("Image-File", PathOfPicture);

            return new Tuple<Bitmap, List<string>>(B, T);

        }
    }
}
