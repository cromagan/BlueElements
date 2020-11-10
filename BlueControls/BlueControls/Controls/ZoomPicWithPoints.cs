using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Extensions;
using static BlueBasics.FileOperations;

namespace BlueControls.Controls
{
    [Designer(typeof(BasicDesigner))]
    public partial class ZoomPicWithPoints : ZoomPic
    {

        #region Constructor
        public ZoomPicWithPoints() : base()
        {
            InitializeComponent();
        }
        #endregion


        private readonly List<PointM> points = new List<PointM>();



        public event EventHandler PointSetByUser;

        public List<string> Tags = new List<string>();
        public string Feedback = string.Empty;

        private bool _PointAdding = false;
        private static readonly Pen Pen_RotTransp = new Pen(Color.FromArgb(200, 255, 0, 0));
        private static readonly Brush Brush_RotTransp = new SolidBrush(Color.FromArgb(200, 255, 0, 0));

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



        protected override RectangleM MaxBounds()
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

        //protected override void DrawControl(Graphics gr, enStates state)
        //{
        //    PrepareOverlay();
        //    base.DrawControl(gr, state);
        //}


        protected override void OnDoAdditionalDrawing(AdditionalDrawing e)
        {
            base.OnDoAdditionalDrawing(e);

            DrawMittelLinien(e);


            /// Punkte
            foreach (var ThisPoint in points)
            {
                ThisPoint.Draw(e.G, e.Zoom, e.MoveX, e.MoveY, enDesign.Button_EckpunktSchieber, enStates.Standard, _Helper.HasFlag(enHelpers.PointNames));
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
                points.Add(new PointM(null, s));
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
            var i = new BitmapListItem(B, FilenamePNG, FilenamePNG, FilenamePNG.FileNameWithoutSuffix(), string.Empty)
            {
                Padding = 10,
                Tags = T,
            };
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
            }

            var s = string.Empty;

            foreach (var ThisP in points)
            {
                s = s + ThisP.Name + "|";
                Tags.TagSet(ThisP.Name, ThisP.ToString());
            }

            Tags.TagSet("AllPointNames", s.TrimEnd("|").ToNonCritical());
        }

        public PointM GetPoint(string name)
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
            WritePointsInTags();
            Invalidate();
        }

        public void PointSet(string name, int x, int y)
        {
            PointSet(name, x, (decimal)y);
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
                p = new PointM(name, x, y);
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

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);

            Invalidate();
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
                B = (Bitmap)BitmapExt.Image_FromFile(PathOfPicture);
            }

            var ftxt = FilenameTXT(PathOfPicture);


            if (FileExists(ftxt))
            {
                T = modAllgemein.LoadFromDisk(ftxt).SplitByCRToList();
            }


            T.TagSet("ImageFile", PathOfPicture);

            return new Tuple<Bitmap, List<string>>(B, T);

        }

        private void DrawMittelLinien(AdditionalDrawing eg)
        {

            if (BMP == null) { return; }


            var e = new PositionEventArgs(MousePos_1_1.X, MousePos_1_1.Y);
            OnOverwriteMouseImageData(e);


            ///// Punkte
            //foreach (var ThisPoint in points)
            //{
            //    ThisPoint.Draw(e.G, e.Zoom, e.MoveX, e.MoveY, enDesign.Button_EckpunktSchieber, enStates.Standard);
            //}
            //if (OverlayBMP == null || OverlayBMP.Width != BMP.Width || OverlayBMP.Height != BMP.Height)
            //{
            //    OverlayBMP = new Bitmap(BMP.Width, BMP.Height);
            //}


            //var TMPGR = Graphics.FromImage(BMP);

            //TMPGR.Clear(Color.Transparent);

            // Mittellinie
            var PicturePos = base.MaxBounds();

            if (_MittelLinie.HasFlag(enOrientation.Waagerecht))
            {
                var p1 = PicturePos.PointOf(enAlignment.VerticalCenter_Left).ZoomAndMove(eg);
                var p2 = PicturePos.PointOf(enAlignment.VerticalCenter_Right).ZoomAndMove(eg);
                eg.G.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
                eg.G.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
            }

            if (_MittelLinie.HasFlag(enOrientation.Senkrecht))
            {
                var p1 = PicturePos.PointOf(enAlignment.Top_HorizontalCenter).ZoomAndMove(eg);
                var p2 = PicturePos.PointOf(enAlignment.Bottom_HorizontalCenter).ZoomAndMove(eg);
                eg.G.DrawLine(new Pen(Color.FromArgb(10, 0, 0, 0), 3), p1, p2);
                eg.G.DrawLine(new Pen(Color.FromArgb(220, 100, 255, 100)), p1, p2);
            }


            if (MousePos_1_1.IsEmpty) { return; }


            if (_Helper.HasFlag(enHelpers.HorizontalLine))
            {
                var p1 = new PointM(0, e.Y).ZoomAndMove(eg);
                var p2 = new PointM(BMP.Width, e.Y).ZoomAndMove(eg);
                eg.G.DrawLine(Pen_RotTransp, p1, p2);
            }

            if (_Helper.HasFlag(enHelpers.VerticalLine))
            {
                var p1 = new PointM(e.X, 0).ZoomAndMove(eg);
                var p2 = new PointM(e.X, BMP.Height).ZoomAndMove(eg);
                eg.G.DrawLine(Pen_RotTransp, p1, p2);
            }



            if (_Helper.HasFlag(enHelpers.SymetricalHorizontal))
            {
                var h = BMP.Width / 2;
                var x = Math.Abs(h - e.X);

                var p1 = new PointM(h - x, e.Y).ZoomAndMove(eg);
                var p2 = new PointM(h + x, e.Y).ZoomAndMove(eg);
                eg.G.DrawLine(Pen_RotTransp, p1, p2);

            }

            if (_Helper.HasFlag(enHelpers.MouseDownPoint))
            {

                var m1 = new PointM(e.X, e.Y).ZoomAndMove(eg);

                eg.G.DrawEllipse(Pen_RotTransp, new RectangleF(m1.X - 3, m1.Y - 3, 6, 6));

                if (!MouseDownPos_1_1.IsEmpty)
                {

                    var md1 = new PointM(MouseDownPos_1_1).ZoomAndMove(eg);
                    var mc1 = new PointM(e.X, e.Y).ZoomAndMove(eg);

                    eg.G.DrawEllipse(Pen_RotTransp, new RectangleF(md1.X - 3, md1.Y - 3, 6, 6));
                    eg.G.DrawLine(Pen_RotTransp, mc1, md1);
                }

            }



            if (_Helper.HasFlag(enHelpers.FilledRectancle))
            {
                if (!MouseDownPos_1_1.IsEmpty)
                {
                    var md1 = new PointM(MouseDownPos_1_1).ZoomAndMove(eg);
                    var mc1 = new PointM(e.X, e.Y).ZoomAndMove(eg);
                    var r = new RectangleF(Math.Min(md1.X, e.X), Math.Min(md1.Y, e.Y), Math.Abs(md1.X - mc1.X) + 1, Math.Abs(md1.Y - mc1.Y) + 1);
                    eg.G.FillRectangle(Brush_RotTransp, r);
                }
            }


        }

        public void PointRemove(string name)
        {
            var p = GetPoint(name);
            if (p == null) { return; }
            points.Remove(p);
            WritePointsInTags();
            Invalidate();
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
                Tags.TagSet("Datum", DateTime.Now.ToString(Constants.Format_Date5));
                Tags.Save(pathtxt, false);
            }
        }


        public static Tuple<Bitmap, List<string>> ResizeData(Bitmap pic, List<string> tags, int width, int height)
        {

            var zoomx = (decimal)width / pic.Width;
            var zoomy = (decimal)height / pic.Height;


            var pic2 = BitmapExt.Resize(pic, width, height, enSizeModes.Verzerren, System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic, true);

            var tags2 = new List<string>(tags);


            var Names = tags2.TagGet("AllPointNames").FromNonCritical().SplitBy("|");



            foreach (var thisO in Names)
            {
                var s = tags2.TagGet(thisO);
                var ThisP = new PointM(null, s);

                ThisP.X *= zoomx;
                ThisP.Y *= zoomy;
                tags2.TagSet(ThisP.Name, ThisP.ToString());
            }


            return new Tuple<Bitmap, List<string>>(pic2, tags2);

        }
    }
}
