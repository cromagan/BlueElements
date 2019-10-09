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
using BlueControls.EventArgs;

namespace BlueControls.Controls
{
    public partial class ZoomPicWithPoints : ZoomPic
    {
        public ZoomPicWithPoints()
        {
            InitializeComponent();
        }


        List<PointDF> points = new List<PointDF>();



        public event EventHandler PointSetByUser;

        public List<string> Tags = new List<string>();
        // private enSelectModus _SelectModus = enSelectModus.Ohne;
        public string Feedback = string.Empty;

        private bool _PointAdding = false;



        static Pen Pen_RotTransp = new Pen(Color.FromArgb(200, 255, 0, 0));
        static Brush Brush_RotTransp = new SolidBrush(Color.FromArgb(200, 255, 0, 0));

        private enOrientation _MittelLinie = enOrientation.Ohne;

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
            PrepareOverlay();
            base.DrawControl(gr, state);
        }


        protected override void OnDoAdditionalDrawing(AdditionalDrawing e)
        {
            base.OnDoAdditionalDrawing(e);

            /// Punkte
            foreach (var ThisPoint in points)
            {
                ThisPoint.Draw(e.G, e.Zoom, e.MoveX, e.MoveY, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard);
            }




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
            var Names = Tags.TagGet("AllPointNames").FromNonCritical().SplitBy("|");

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
            var FilenamePNG = T.TagGet("ImageFile");
            var i = new BitmapListItem(FilenamePNG, FilenamePNG.FileNameWithoutSuffix(), FilenamePNG, string.Empty);
            i.Padding = 10;
            i.Tags.AddRange(T);
            i.Bitmap = B;
            return i;
        }



        public BitmapListItem GenerateBitmapListItem()
        {
            WritePointsInTags();
            return ZoomPicWithPoints.GenerateBitmapListItem(BMP, Tags);
        }

        private void WritePointsInTags()
        {

            var Old = Tags.TagGet("AllPointNames").FromNonCritical().SplitBy("|");

            foreach (var thisO in Old)
            {
                Tags.TagSet(thisO, string.Empty);
                //Tags.TagSet(thisO + "-X", string.Empty);
                //Tags.TagSet(thisO + "-Y", string.Empty);
            }



            var s = string.Empty;

            foreach (var ThisP in points)
            {
                s = s + ThisP.Name + "|";
                Tags.TagSet(ThisP.Name, ThisP.ToString());
                //Tags.TagSet(ThisP.Name + "-X", ThisP.X.ToString());
                //Tags.TagSet(ThisP.Name + "-Y", ThisP.Y.ToString());
            }


            Tags.TagSet("AllPointNames", s.TrimEnd("|").ToNonCritical());

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
                WritePointsInTags();
                Invalidate();
                return;
            }


            if (p.X != x || p.Y != y)
            {
                p.X = x;
                p.Y = y;
                Invalidate();
            }

            WritePointsInTags();

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


        public static string FilenameTXT(string PathOfPicture)
        {

            return PathOfPicture.FilePath() + PathOfPicture.FileNameWithoutSuffix() + ".txt";

//            return PathOfPicture.TrimEnd(".PNG").TrimEnd(".JPG").TrimEnd(".JPG") + ".txt";
        }


        public static Tuple<Bitmap, List<string>> LoadFromDisk(string PathOfPicture)
        {



            Bitmap B = null;
            var T = new List<string>();


            if (FileExists(PathOfPicture))
            {
                B = (Bitmap)Image_FromFile(PathOfPicture);
            }

            var ftxt = FilenameTXT(PathOfPicture);


            if (FileExists(ftxt))
            {
                T = modAllgemein.LoadFromDisk(ftxt).SplitByCRToList();
            }


            T.TagSet("ImageFile", PathOfPicture);

            return new Tuple<Bitmap, List<string>>(B, T);

        }

        private void PrepareOverlay()
        {
            //OverlayBMP = (BMP.Clone();

            if (BMP == null) { return; }


            if (OverlayBMP == null || OverlayBMP.Width != BMP.Width || OverlayBMP.Height != BMP.Height)
            {
                OverlayBMP = new Bitmap(BMP.Width, BMP.Height);
            }


            var TMPGR = Graphics.FromImage(OverlayBMP);

            TMPGR.Clear(Color.Transparent);

            // Mittellinie
            var PicturePos = base.MaxBounds();

            if (_MittelLinie.HasFlag(enOrientation.Waagerecht))
            {
                var p1 = PicturePos.PointOf(enAlignment.VerticalCenter_Left).ToPointF();
                var p2 = PicturePos.PointOf(enAlignment.VerticalCenter_Right).ToPointF();

                //var p1 = new Point(0, (int)(OverlayBMP.Height / 2));
                //var p2 = new Point(OverlayBMP.Width, (int)(OverlayBMP.Height / 2));

                TMPGR.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
                TMPGR.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
            }

            if (_MittelLinie.HasFlag(enOrientation.Senkrecht))
            {
                var p1 = PicturePos.PointOf(enAlignment.Top_HorizontalCenter).ToPointF();
                var p2 = PicturePos.PointOf(enAlignment.Bottom_HorizontalCenter).ToPointF();
                //var p1 = new Point((int)(OverlayBMP.Width / 2),0);
                //var p2 = new Point((int)(OverlayBMP.Width / 2), OverlayBMP.Height);
                TMPGR.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
                TMPGR.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
            }


            if (MousePos_1_1.IsEmpty) { return; }


            if (_Helper.HasFlag(enHelpers.HorizontalLine))
            {
                TMPGR.DrawLine(Pen_RotTransp, MousePos_1_1.X, 0, MousePos_1_1.X, OverlayBMP.Height);

            }
            if (_Helper.HasFlag(enHelpers.VerticalLine))
            {
                TMPGR.DrawLine(Pen_RotTransp, 0, MousePos_1_1.Y, OverlayBMP.Width, MousePos_1_1.Y);

            }


            if (_Helper.HasFlag(enHelpers.SymetricalHorizontal))
            {
                var h = (int)(BMP.Width / 2);
                var x = Math.Abs(h - MousePos_1_1.X);

                TMPGR.DrawLine(Pen_RotTransp, h - x, MousePos_1_1.Y, h + x, MousePos_1_1.Y);

            }



            if (_Helper.HasFlag(enHelpers.FilledRectancle))
            {
                if (!MouseDownPos_1_1.IsEmpty)
                {

                    var r = new Rectangle(Math.Min(MouseDownPos_1_1.X, MousePos_1_1.X), Math.Min(MouseDownPos_1_1.Y, MousePos_1_1.Y), Math.Abs(MouseDownPos_1_1.X - MousePos_1_1.X) + 1, Math.Abs(MouseDownPos_1_1.Y - MousePos_1_1.Y) + 1);
                    //var r = new Rectangle((int)Math.Min(MouseDownPos_1_1.X, MousePos_1_1.X), (int)Math.Min(MouseDownPos_1_1.Y, MousePos_1_1.Y), (int)Math.Abs(MouseDownPos_1_1.X - MousePos_1_1.X) + 1, (int)Math.Abs(MouseDownPos_1_1.Y - MousePos_1_1.Y) + 1);
                    TMPGR.FillRectangle(Brush_RotTransp, r);
                }
            }


        }

        public void LetUserAddAPoint(string pointName, enHelpers helper, enOrientation mittelline)
        {

            _MittelLinie = mittelline;
            _Helper = helper;
            Feedback = pointName;
            _PointAdding = true;
            Invalidate();

        }



        protected override void OnImageMouseUp(MouseEventArgs1_1 e)
        {


            if (_PointAdding && !string.IsNullOrEmpty(Feedback))
            {
                PointSet(Feedback, e.X, e.Y);
                _PointAdding = false;
                OnPointSetByUser();
            }

            base.OnImageMouseUp(e); // erst nachher, dass die MouseUpRoutine das Feedback nicht änddern kann

            //Feedback = string.Empty;

        }


        protected virtual void OnPointSetByUser()
        {
            PointSetByUser?.Invoke(this, System.EventArgs.Empty);
        }

        public void SaveData()
        {
            WritePointsInTags();
            var Path = Tags.TagGet("ImageFile");


            var pathtxt = FilenameTXT(Path);


            if (BMP != null)
            {
                BMP.Save(Path, System.Drawing.Imaging.ImageFormat.Png);

            }

            if (Tags != null)
            {
                Tags.TagSet("Erstellt", modAllgemein.UserName());
                Tags.TagSet("Datum", DateTime.Now.ToString());
                Tags.Save(pathtxt, false);
            }



        }
    }
}
