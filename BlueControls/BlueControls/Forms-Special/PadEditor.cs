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


using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;



namespace BlueControls.Forms
{
    public partial class PadEditor : Form
    {

        private readonly string _Title = string.Empty;


        public PadEditor() : base()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

            InitWindow(false, "", -1, "");
        }





        private void InitWindow(bool FitWindowToBest, string WindowCaption, int OpenOnScreen, string DesignName)
        {
 
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
                PadDesign.Text = PadDesign.Item[0].Internal;
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




        private void btnZoomFit_Click(object sender, System.EventArgs e)
        {
            Pad.ZoomFit();
        }



        private void Pad_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
            if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
        }




        private void btnAddLine_Click(object sender, System.EventArgs e)
        {
            var b = new LinePadItem(PadStyles.Style_Standard, new Point(300, 300), new Point(400, 300));
            Pad.Item.Add(b);
        }

        private void btnAddDistance_Click(object sender, System.EventArgs e)
        {
            var b = new SpacerPadItem();
            Pad.Item.Add(b);
            b.SetCoordinates(new RectangleDF(10, 10, 20, 20));
        }

        private void btnAddImage_Click(object sender, System.EventArgs e)
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


        private void btnAddDimension_Click(object sender, System.EventArgs e)
        {
            var b = new DimensionPadItem(new PointF(300, 300), new PointF(400, 300), 30);
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

        private void PadDesign_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            Pad.SheetStyle = e.Item.Internal;
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

        private void btnAddUnterStufe_Click(object sender, System.EventArgs e)
        {
            var b = new ChildPadItem();
            b.SetCoordinates(new RectangleDF(100, 100, 300, 300));
            Pad.Item.Add(b);
        }

        private void btnAddSymbol_Click(object sender, System.EventArgs e)
        {
            var b = new SymbolPadItem();
            b.SetCoordinates(new RectangleDF(100, 100, 300, 300));
            Pad.Item.Add(b);
        }
    }
}