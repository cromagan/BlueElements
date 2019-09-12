#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
// https://github.com/cromagan/BlueElements
// 
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER  
// DEALINGS IN THE SOFTWARE. 
#endregion


using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;



namespace BlueControls.Forms
{
    public partial class PictureView
    {
        protected List<string> _FileList;
        private int _NR = -1;
        private readonly string _Title = string.Empty;


        public PictureView()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

            InitWindow(false, "", -1, "");
        }


        public PictureView(List<string> FileList, bool MitScreenResize, string WindowCaption)
        {

            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();

            _FileList = FileList;
            LoadPic(0);

            InitWindow(MitScreenResize, WindowCaption, -1, "");


            Area_KomponenteHinzufügen.Visible = false;
            Area_Assistent.Visible = false;
            Area_Design.Visible = false;

            ZoomIn.Checked = true;
            Auswahl.Enabled = false;
            Pad.MouseEditEnabled = false;
            Pad.KeyboardEditEnabled = false;
        }

        public PictureView(Bitmap BMP)
        {

            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();

            _FileList = new List<string>();

            Pad.Item.Clear();
            Pad.Item.Add(new BitmapPadItem((Bitmap)BMP.Clone(), BMP.Size));


            InitWindow(false, "", -1, "");


            Area_KomponenteHinzufügen.Visible = false;
            Area_Assistent.Visible = false;
            Area_Design.Visible = false;

            ZoomIn.Checked = true;
            Auswahl.Enabled = false;
            Pad.MouseEditEnabled = false;
            Pad.KeyboardEditEnabled = false;
        }


        public PictureView(List<string> FileList, bool MitScreenResize, string WindowCaption, int OpenOnScreen)
        {

            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();

            _FileList = FileList;
            LoadPic(0);

            InitWindow(MitScreenResize, WindowCaption, OpenOnScreen, "");

            Area_KomponenteHinzufügen.Visible = false;
            Area_Assistent.Visible = false;
            Area_Design.Visible = false;
            Area_Seiten.Visible = FileList != null && FileList.Count > 1;

            ZoomIn.Checked = true;
            Auswahl.Enabled = false;
            Pad.MouseEditEnabled = false;
            Pad.KeyboardEditEnabled = false;
        }

        //public PictureView(string CodeToParse, string Title)
        //{

        //    // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        //    InitializeComponent();

        //    _FileList = null;
        //    _Title = Title;
        //    Pad.Item.Clear();

        //    Pad.ParseData(CodeToParse, true, true);


        //    var Count = 0;


        //    do
        //    {
        //        Count += 1;
        //        if (Pad.RepairAll(0, true)) { break; }
        //        if (Count > 10) { break; }
        //    } while (true);

        //    Pad.RepairAll(1, true);


        //    InitWindow(false, Title, Title, -1, Pad.SheetStyle);
        //}


        protected void LoadPic(int Nr)
        {
            _NR = Nr;
            Pad.Item.Clear();

            if (_FileList != null && Nr < _FileList.Count)
            {
                try
                {
                    var BMP = (Bitmap)modAllgemein.Image_FromFile(_FileList[Nr]);
                    if (BMP != null)
                    {
                        Pad.Item.Add(new BitmapPadItem(BMP, BMP.Size));
                    }
                } 
                catch (Exception ex)
                {
                    Develop.DebugPrint(ex);
                }


            }
            Ribbon.SelectedIndex = 1;


            Area_Seiten.Visible = _FileList != null && _FileList.Count > 1;

            if (_FileList == null || _FileList.Count == 0)
            {
                Links.Enabled = false;
                Rechts.Enabled = false;
            }
            else
            {
                Area_Seiten.Enabled = true;
                Links.Enabled = Convert.ToBoolean(_NR > 0);
                Rechts.Enabled = Convert.ToBoolean(_NR < _FileList.Count - 1);

            }



            Pad.ZoomFit();

        }


        private void InitWindow(bool FitWindowToBest, string WindowCaption, int OpenOnScreen, string DesignName)
        {
            //    Me.ShowInTaskbar = False

            if (_FileList == null || _FileList.Count < 2) { Area_Seiten.Enabled = false; }


            if (FitWindowToBest)
            {
                if (System.Windows.Forms.Screen.AllScreens.Length == 1 || OpenOnScreen < 0)
                {
                    var OpScNr = modAllgemein.PointOnScreenNr(System.Windows.Forms.Cursor.Position);

                    Width = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Width / 1.5);
                    Height = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Height / 1.5);
                    Left = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Left + (System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Width - Width) / 2.0);
                    Top = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Top + (System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Height - Height) / 2.0);

                }
                else
                {
                    Width = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Width;
                    Height = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Height;
                    Left = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Left;
                    Top = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Top;
                }
            }


            if (!string.IsNullOrEmpty(WindowCaption))
            {
                Text = WindowCaption;
            }




            PadDesign.Item.Clear();
            PadDesign.Item.AddRange(Skin.AllStyles());

            if (string.IsNullOrEmpty(DesignName))
            {
                PadDesign.Text = PadDesign.Item[0].Internal();
            }
            else
            {
                PadDesign.Text = DesignName;
            }

            Pad.SheetStyle = PadDesign.Text;

            SchriftGröße.Item.Add(new TextListItem("050", "50%"));
            SchriftGröße.Item.Add(new TextListItem("060", "60%"));
            SchriftGröße.Item.Add(new TextListItem("070", "70%"));
            SchriftGröße.Item.Add(new TextListItem("080", "80%"));
            SchriftGröße.Item.Add(new TextListItem("090", "90%"));
            SchriftGröße.Item.Add(new TextListItem("100", "100%"));
            SchriftGröße.Item.Add(new TextListItem("110", "110%"));
            SchriftGröße.Item.Add(new TextListItem("120", "120%"));
            SchriftGröße.Item.Add(new TextListItem("130", "130%"));
            SchriftGröße.Item.Add(new TextListItem("140", "140%"));
            SchriftGröße.Item.Add(new TextListItem("150", "150%"));

            SchriftGröße.Item.Sort();

            SchriftGröße.Text = "100";

            if (Develop.IsHostRunning()) { TopMost = false; }
        }


        private void Links_Click(object sender, System.EventArgs e)
        {

            _NR -= 1;
            if (_NR <= 0) { _NR = 0; }

            LoadPic(_NR);
        }

        private void Rechts_Click(object sender, System.EventArgs e)
        {
            _NR += 1;
            if (_NR >= _FileList.Count - 1) { _NR = _FileList.Count - 1; }

            LoadPic(_NR);
        }


        private void ZoomFitBut_Click(object sender, System.EventArgs e)
        {
            Pad.ZoomFit();
        }



        private void Pad_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (ZoomIn.Checked) { Pad.ZoomIn(e); }
            if (ZoomOut.Checked) { Pad.ZoomOut(e); }
        }




        private void AddLine_Click(object sender, System.EventArgs e)
        {
            var b = new LinePadItem(PadStyles.Style_Standard, new Point(300, 300), new Point(400, 300));
            Pad.Item.Add(b);
        }

        private void AddDistance_Click(object sender, System.EventArgs e)
        {
            var b = new SpacerPadItem(Pad.Item);
            Pad.Item.Add(b);
            b.SetCoordinates(new RectangleDF(10, 10, 20, 20));
        }

        private void AddImage_Click(object sender, System.EventArgs e)
        {
            var b = new BitmapPadItem(QuickImage.Get(enImageCode.Fragezeichen).BMP, new Size(1000, 1000));
            Pad.Item.Add(b);
        }


        private void AddText_Click(object sender, System.EventArgs e)
        {
            var b = new TextPadItem();
            b.Text = "";
            b.Style = PadStyles.Style_Standard;
            Pad.Item.Add(b);
            b.SetCoordinates(new RectangleDF(10, 10, 200, 200));
        }


        private void AddDimension_Click(object sender, System.EventArgs e)
        {
            var b = new DimensionPadItem(new PointF(300, 300), new PointF(400, 300), 30, Pad.DPI);
            Pad.Item.Add(b);
        }




        //protected override OnItem




        private void Bild_Click(object sender, System.EventArgs e)
        {
            Pad.SaveAsBitmap(_Title, string.Empty);
        }


        private void ButtonPageSetup_Click(object sender, System.EventArgs e)
        {
            Pad.ShowPrinterPageSetup();
        }

        private void Pad_Parsed(object sender, System.EventArgs e)
        {


            PadDesign.Text = Pad.SheetStyle;

            SchriftGröße.Text = ((int)(Pad.SheetStyleScale * 100)).ToString(Constants.Format_Integer3);


            //     Nur70.Checked = CBool(Pad.SheetStyleScale = 0.7F)
        }

        private void Drucken_Click(object sender, System.EventArgs e)
        {
            Pad.Print();
        }

        private void Vorschau_Click(object sender, System.EventArgs e)
        {
            Pad.ShowPrintPreview();
        }

        private void PadDesign_Item_Click(object sender, BasicListItemEventArgs e)
        {
            Pad.SheetStyle = e.Item.Internal();
        }


        private void Raster_CheckedChanged(object sender, System.EventArgs e)
        {
            Pad.Grid = Raster.Checked;
        }

        private void RasterAnzeige_TextChanged(object sender, System.EventArgs e)
        {

            if (!RasterAnzeige.Text.IsNumeral()) { return; }

            Pad.GridShow = float.Parse(RasterAnzeige.Text);
        }

        private void RasterFangen_TextChanged(object sender, System.EventArgs e)
        {
            if (!RasterFangen.Text.IsNumeral()) { return; }

            Pad.GridSnap = float.Parse(RasterFangen.Text);
        }


        private void BezMode_CheckedChanged(object sender, System.EventArgs e)
        {
            CheckBezMode();
        }


        private void CheckBezMode()
        {
            if (Bez_None.Checked)
            {
                Pad.AutoRelation = enAutoRelationMode.None;
            }
            else if (Bez_Direkt.Checked)
            {
                Pad.AutoRelation = enAutoRelationMode.DirektVerbindungen_Erhalten;
            }
            else if (Bez_All.Checked)
            {
                Pad.AutoRelation = enAutoRelationMode.Alle_Erhalten;

            }
        }


        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            CheckBezMode();
        }

        private void SchriftGröße_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            Pad.SheetStyleScale = decimal.Parse(SchriftGröße.Text) / 100m;
        }

        private void ArbeitsbreichSetup_Click(object sender, System.EventArgs e)
        {
            Pad.ShowWorkingAreaSetup();
        }

    }
}