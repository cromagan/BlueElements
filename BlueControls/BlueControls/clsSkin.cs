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
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueControls.Enums;
using BlueDatabase.Enums;

namespace BlueControls
{
    public sealed class clsSkin
    {
        private Database SkinDB; //(modAllgemein.UserName, "#Administrator")
        private readonly enImageCodeEffect[] ST = new enImageCodeEffect[1];
        internal Pen Pen_LinieDünn;
        internal Pen Pen_LinieKräftig;
        internal Pen Pen_LinieDick;

        private string _SkinString = string.Empty;


        public static float Scale = (float)Math.Round(System.Windows.Forms.SystemInformation.VirtualScreen.Width / System.Windows.SystemParameters.VirtualScreenWidth, 2);


        public static string ErrorFont = "<Name=Arial, Size=8, Color=FF0000>";
        public static string DummyStandardFont = "<Name=Arial, Size=10>";

        //Graphics g = Graphics.FromHwnd(this.Handle);
        //float dx = g.DpiX; float dy = g.DpiY;
        // Gibt bei 100% 96 DPI an, bei 125% 120 DPI, bei 150% DPI




        public static Database StyleDB;

        internal int PaddingSmal; //= 3 ' Der Abstand von z.B. in Textboxen: Text Linke Koordinate
        internal int Padding; //= 9

        private enSkin _Skin;

        private Bitmap DummyBMP;
        private Graphics DummyGR;



        public event EventHandler SkinChanged;




        private enStates SkinRow_LastState = enStates.Undefiniert;
        private enDesign SkinRow_LastType = enDesign.Undefiniert;
        private RowItem SkinRow_LastRow;

        public clsSkin()
        {
            _Skin = enSkin.Windows_10;
            LoadSkin();
        }


        public enSkin Skin
        {
            get
            {
                return _Skin;
            }
            set
            {
                if (value == _Skin) { return; }
                _Skin = value;
                LoadSkin();
            }
        }


        public string SkinString()
        {
            return _SkinString;
        }


        private void LoadSkin()
        {
            _SkinString = _Skin.ToString();
            _SkinString = _SkinString.Replace("_", "");
            _SkinString = _SkinString.Replace(" ", "");

            SkinDB = Database.LoadResource(Assembly.GetAssembly(typeof(clsSkin)), _SkinString + ".skn", "Skin", Convert.ToBoolean(_Skin == enSkin.Windows_10), Convert.ToBoolean(Develop.AppName() == "SkinDesigner"), Table.Database_NeedPassword, CreativePad.GenerateLayoutFromRow, CreativePad.RenameColumnInLayout);
            if (SkinDB == null)
            {
                Develop.DebugPrint("Skin '" + _SkinString + "' konnte nicht geladen werden!");
                _Skin = enSkin.Windows_10;
                LoadSkin();
                return;
            }

            GetEffects();

        }


        private void GetEffects()
        {
            PaddingSmal = 3;
            Padding = 9;

            ST[0] = (enImageCodeEffect)int.Parse(SkinDB.Tags[0]);
            Pen_LinieDünn = new Pen(Color_Border(enDesign.Table_Lines_thin, enStates.Standard));
            Pen_LinieKräftig = new Pen(Color_Border(enDesign.Table_Lines_thick, enStates.Standard));
            Pen_LinieDick = new Pen(Color_Border(enDesign.Table_Lines_thick, enStates.Standard), 3);

            OnSkinChanged();


        }

        private void OnSkinChanged()
        {
            SkinChanged?.Invoke(this, System.EventArgs.Empty);
        }

        public enImageCodeEffect AdditionalState(enStates vState)
        {
            if (Convert.ToBoolean(vState & enStates.Standard_Disabled)) { return ST[0]; }
            return enImageCodeEffect.Ohne;
        }

        public Color Color_Back(enDesign vDesign, enStates vState)
        {
            return Color.FromArgb(int.Parse(Value(SkinRow(vDesign, vState), "Color_Back_1", "0")));
        }

        internal Color Color_Border(enDesign vDesign, enStates vState)
        {
            return Color.FromArgb(int.Parse(Value(SkinRow(vDesign, vState), "Color_Border_1", "0")));
        }

        private string Value(RowItem Row, string vColumnName, string StandardValue)
        {
            if (Row == null || !IsReady()) { return StandardValue; }
            string w = null;
            if (Row != null)
            {
                w = Row.CellGetString(vColumnName);
            }
            else
            {
                w = string.Empty; // Kommt vor, wenn die Datenbank nicht savable ist, dann ist die Lastzeile bei einer newRow trotzdem nothing!
            }


            if (string.IsNullOrEmpty(w))
            {
                if (string.IsNullOrEmpty(StandardValue))
                {
                    Develop.DebugPrint("Skin-Fehler: " + Row.Database.Filename + " - " + vColumnName + " - " + Row.CellFirstString());
                }
                return StandardValue;
            }

            return w;
        }


        internal RowItem SkinRow(enDesign vDesign, enStates vState)
        {

            //Kann vorkommen, wenn die Database zweck Userwechsel neu geladen wird
            if (SkinDB == null) { return null; }


            if (SkinRow_LastType != vDesign || SkinRow_LastState != vState || SkinRow_LastRow == null)
            {
                SkinRow_LastRow = SkinDB.Row[new FilterItem(SkinDB.Column[0], enFilterType.Istgleich, Convert.ToInt32(vDesign).ToString()), new FilterItem(SkinDB.Column[1], enFilterType.Istgleich, Convert.ToInt32(vState).ToString())];
            }


            if (SkinRow_LastRow == null) { Develop.DebugPrint("Unbekanntes Skin: " + SkinDB.Filename.FileNameWithoutSuffix() + "/" + vDesign + "/" + vState); }


            SkinRow_LastState = vState;
            SkinRow_LastType = vDesign;


            return SkinRow_LastRow;
        }



        #region  Back 


        public void Draw_Back(Graphics GR, enDesign vDesign, enStates vState, Rectangle r, System.Windows.Forms.Control vControl, bool NeedTransparenz)
        {
            Draw_Back(GR, SkinRow(vDesign, vState), r, vControl, NeedTransparenz);
        }


        public void Draw_Back(Graphics GR, RowItem cRow, Rectangle r, System.Windows.Forms.Control cControl, bool NeedTransparenz)
        {
            try
            {
                var Need = true;
                var X1 = 0;
                var Y1 = 0;
                var X2 = 0;
                var Y2 = 0;
                var HA = enHintergrundArt.Ohne;

                if (!NeedTransparenz || cControl == null) { Need = false; }


                var Kon = (enKontur)int.Parse(Value(cRow, "Kontur", "-1"));
                if (Kon == enKontur.Unbekannt) { Kon = enKontur.Rechteck; }

                if (Kon != enKontur.Ohne)
                {

                    HA = (enHintergrundArt)int.Parse(Value(cRow, "Draw_Back", "-1"));

                    X1 = int.Parse(Value(cRow, "X1", "0"));
                    Y1 = int.Parse(Value(cRow, "Y1", "0"));
                    X2 = int.Parse(Value(cRow, "X2", "0"));
                    Y2 = int.Parse(Value(cRow, "Y2", "0"));


                    if (HA != enHintergrundArt.Ohne)
                    {
                        if (Kon == enKontur.Rechteck && X1 >= 0 && X2 >= 0 && Y1 >= 0 && Y2 >= 0) { Need = false; }
                        if (Kon == enKontur.Rechteck_R4 && X1 >= 1 && X2 >= 1 && Y1 >= 1 && Y2 >= 1) { Need = false; }
                    }

                }


                if (Need) { Draw_Back_Transparent(GR, r, cControl); }

                if (HA == enHintergrundArt.Ohne || Kon == enKontur.Ohne) { return; }


                r.X -= X1;
                r.Y -= Y1;
                r.Width += X1 + X2;
                r.Height += Y1 + Y2;

                if (r.Width < 1 || r.Height < 1) { return; }// Durchaus möglich, Creative-Pad, usereingabe

                GraphicsPath PathX = null;


                switch (HA)
                {
                    case enHintergrundArt.Ohne:
                        break;

                    case enHintergrundArt.Solide:
                        PathX = Kontur(Kon, r);
                        var BrushX = new SolidBrush(Color.FromArgb(int.Parse(Value(cRow, "Color_Back_1", "0"))));
                        GR.FillPath(BrushX, PathX);
                        break;

                    case enHintergrundArt.Verlauf_Vertical_2:
                        PathX = Kontur(Kon, r);
                        var c1 = Color.FromArgb(int.Parse(Value(cRow, "Color_Back_1", "0")));
                        var c2 = Color.FromArgb(int.Parse(Value(cRow, "Color_Back_2", "0")));
                        var lgb = new LinearGradientBrush(r, c1, c2, LinearGradientMode.Vertical);
                        GR.FillPath(lgb, PathX);
                        break;

                    case enHintergrundArt.Verlauf_Vertical_3:
                        Draw_Back_Verlauf_Vertical_3(GR, cRow, r);
                        break;

                    case enHintergrundArt.Verlauf_Horizontal_2:
                        Draw_Back_Verlauf_Horizontal_2(GR, cRow, r);
                        break;

                    case enHintergrundArt.Verlauf_Horizontal_3:
                        Draw_Back_Verlauf_Horizontal_3(GR, cRow, r);
                        break;

                    case enHintergrundArt.Verlauf_Diagonal_3:
                        PathX = Kontur(Kon, r);
                        var cx1 = Color.FromArgb(int.Parse(Value(cRow, "Color_Back_1", "0")));
                        var cx2 = Color.FromArgb(int.Parse(Value(cRow, "Color_Back_2", "0")));
                        var cx3 = Color.FromArgb(int.Parse(Value(cRow, "Color_Back_3", "0")));
                        var PR = float.Parse(Value(cRow, "Verlauf_Mitte", "0,7"));
                        var lgb2 = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Right, r.Bottom), cx1, cx3);
                        var cb = new ColorBlend();
                        cb.Colors = new[] { cx1, cx2, cx3 };
                        cb.Positions = new[] { 0.0F, PR, 1.0F };

                        lgb2.InterpolationColors = cb;
                        lgb2.GammaCorrection = true;
                        GR.FillPath(lgb2, PathX);
                        break;

                    case enHintergrundArt.Glossy:
                        Draw_Back_Glossy(GR, cRow, r);
                        break;

                    case enHintergrundArt.GlossyPressed:
                        Draw_Back_GlossyPressed(GR, cRow, r);
                        break;

                    case enHintergrundArt.Verlauf_Vertikal_Glanzpunkt:
                        Draw_Back_Verlauf_Vertical_Glanzpunkt(GR, cRow, r);
                        break;

                    case enHintergrundArt.Unbekannt:
                        break;

                    default:
                        Develop.DebugPrint(HA);
                        break;

                }
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
            }



        }


        public static void Draw_Back_Transparent(Graphics GR, Rectangle r, System.Windows.Forms.Control vControl)
        {
            if (vControl?.Parent == null) { return; }


            switch (vControl.Parent)
            {
                case IBackgroundBitmap TRB:
                    if (TRB.BitmapOfControl() == null) { return; }
                    GR.DrawImage(TRB.BitmapOfControl(), r, new Rectangle(vControl.Left + r.Left, vControl.Top + r.Top, r.Width, r.Height), GraphicsUnit.Pixel);
                    break;

                case IBackgroundNone _:
                    Draw_Back_Transparent(GR, r, vControl.Parent);
                    break;

                case System.Windows.Forms.Form frm:

                    if (frm.BackgroundImage != null)
                    {
                        // Wichtig, für Farbverläufe in MSGBoxen
                        GR.DrawImage(frm.BackgroundImage, r, new Rectangle(vControl.Left + r.Left, vControl.Top + r.Top, r.Width, r.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        GR.FillRectangle(new SolidBrush(frm.BackColor), r);
                    }
                    break;

                case System.Windows.Forms.SplitContainer _:
                    Draw_Back_Transparent(GR, r, vControl.Parent);
                    break;

                case System.Windows.Forms.SplitterPanel _:
                    Draw_Back_Transparent(GR, r, vControl.Parent);
                    break;

                case System.Windows.Forms.TableLayoutPanel _:
                    Draw_Back_Transparent(GR, r, vControl.Parent);
                    break;

                case System.Windows.Forms.Panel _:
                    Draw_Back_Transparent(GR, r, vControl.Parent);
                    break;

                default:
                    System.Windows.Forms.ButtonRenderer.DrawParentBackground(GR, r, vControl); // Ein Versuch ist es allemal wert..
                    Develop.DebugPrint("Unbekannter Typ: " + vControl.Parent.Name);
                    break;
            }

        }

        private void Draw_Back_Verlauf_Vertical_Glanzpunkt(Graphics GR, RowItem Row, Rectangle r)
        {

            LinearGradientBrush lgb = null;
            var cb = new ColorBlend();

            var c1 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_1", "0")));
            var c2 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_2", "0")));
            var PR = float.Parse(Value(Row, "Verlauf_Mitte", "0,05").Replace(".", ","));


            if (PR < 0.06F) { PR = 0.06F; }
            if (PR > 0.94F) { PR = 0.94F; }

            lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Bottom), c1, c1);


            cb.Colors = new[] { c1, c2, c1, c1, c1.Darken(0.3), c1 };
            cb.Positions = new[]
            {
                0.0F, (float) (PR - 0.05), (float) (PR + 0.05), (float) (1 - PR - 0.05), (float) (1 - PR + 0.05), 1.0F
            };


            lgb.InterpolationColors = cb;
            lgb.GammaCorrection = true;

            GR.FillRectangle(lgb, r);
        }

        private void Draw_Back_Verlauf_Horizontal_2(Graphics GR, RowItem Row, Rectangle r)
        {
            var c1 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_1", "0")));
            var c2 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_2", "0")));
            var lgb = new LinearGradientBrush(r, c1, c2, LinearGradientMode.Horizontal);
            GR.FillRectangle(lgb, r);

        }

        private void Draw_Back_Verlauf_Horizontal_3(Graphics GR, RowItem Row, Rectangle r)
        {

            LinearGradientBrush lgb = null;
            var cb = new ColorBlend();

            var c1 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_1", "0")));
            var c2 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_2", "0")));
            var c3 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_3", "0")));
            var PR = float.Parse(Value(Row, "Verlauf_Mitte", "0,5"));


            lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Right, r.Top), c1, c3);

            cb.Colors = new[] { c1, c2, c3 };
            cb.Positions = new[] { 0.0F, PR, 1.0F };

            lgb.InterpolationColors = cb;
            lgb.GammaCorrection = true;

            GR.FillRectangle(lgb, r);
            GR.DrawLine(new Pen(c3), r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);
        }

        private void Draw_Back_Glossy(Graphics GR, RowItem Row, Rectangle r)
        {

            var col1 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_1", "0")));


            var cb = new ColorBlend();

            var c1 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.Black, 1), 0.4);
            var c2 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.FromArgb(64, 64, 64), 1), 0.2);
            var c3 = Extensions.SoftLightMix(col1, Color.FromArgb(128, 128, 128), 1);
            var c4 = Extensions.SoftLightMix(col1, Color.FromArgb(192, 192, 192), 1);
            var c5 = Extensions.OverlayMix(Extensions.SoftLightMix(col1, Color.White, 1), Color.White, 0.75);


            cb.Colors = new[] { c1, c2, c3, c4, c5 };
            cb.Positions = new[] { 0.0F, 0.25F, 0.5F, 0.75F, 1 };
            var lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Top + r.Height + 1), c1, c5);
            lgb.InterpolationColors = cb;
            Draw_Back_Glossy_TMP(lgb, r, GR, 20);

            c2 = Color.White;
            cb.Colors = new[] { c2, c3, c4, c5 };
            cb.Positions = new[] { 0.0F, 0.5F, 0.75F, 1.0F };
            lgb = new LinearGradientBrush(new Point(r.Left + 1, r.Top), new Point(r.Left + 1, r.Top + r.Height - 1), c2, c5);
            lgb.InterpolationColors = cb;

            r.Inflate(-4, -4);
            GR.SmoothingMode = SmoothingMode.HighQuality;
            Draw_Back_Glossy_TMP(lgb, r, GR, 16);

        }

        private void Draw_Back_GlossyPressed(Graphics GR, RowItem Row, Rectangle r)
        {
            var col1 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_1", "0")));

            LinearGradientBrush lgb = null;
            var cb = new ColorBlend();

            var c5 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.Black, 1), 0.4);
            var c4 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.FromArgb(64, 64, 64), 1), 0.2);
            var c3 = Extensions.SoftLightMix(col1, Color.FromArgb(128, 128, 128), 1);
            var c2 = Extensions.SoftLightMix(col1, Color.FromArgb(192, 192, 192), 1);
            var c1 = Extensions.OverlayMix(Extensions.SoftLightMix(col1, Color.White, 1), Color.White, 0.75);


            cb.Colors = new[] { c1, c2, c3, c4, c5 };
            cb.Positions = new[] { 0.0F, 0.25F, 0.5F, 0.75F, 1 };
            lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Top + r.Height + 1), c1, c5);
            lgb.InterpolationColors = cb;
            Draw_Back_Glossy_TMP(lgb, r, GR, 20);

            c2 = Color.White;
            cb.Colors = new[] { c2, c3, c4, c5 };
            cb.Positions = new[] { 0.0F, 0.5F, 0.75F, 1.0F };
            lgb = new LinearGradientBrush(new Point(r.Left + 1, r.Top), new Point(r.Left + 1, r.Top + r.Height - 1), c2, c5);
            lgb.InterpolationColors = cb;


            r.Inflate(-4, -4);
            GR.SmoothingMode = SmoothingMode.HighQuality;
            Draw_Back_Glossy_TMP(lgb, r, GR, 16);
            //    GR.SmoothingModex = Drawing2D.SmoothingMode.None
        }

        private void Draw_Back_Glossy_TMP(Brush b, Rectangle rect, Graphics GR, int RMinus)
        {

            var r = Math.Min(RMinus, Math.Min(rect.Width, rect.Height) - 1);
            var r2 = Convert.ToInt32(Math.Truncate(r / 2.0));
            r = r2 * 2;

            GR.FillEllipse(b, new Rectangle(rect.Left, rect.Top - 1, r, r));
            GR.FillEllipse(b, new Rectangle(rect.Right - r - 1, rect.Top - 1, r, r));
            GR.FillEllipse(b, new Rectangle(rect.Left, rect.Bottom - r, r, r));
            GR.FillEllipse(b, new Rectangle(rect.Right - r - 1, rect.Bottom - r, r, r));
            GR.SmoothingMode = SmoothingMode.None;
            GR.FillRectangle(b, new Rectangle(rect.Left + r2, rect.Top, rect.Width - r, rect.Height));
            GR.FillRectangle(b, new Rectangle(rect.Left, rect.Top + r2, rect.Width, rect.Height - r));
        }


        #endregion


        private GraphicsPath Kontur(enKontur Kon, Rectangle r)
        {


            switch (Kon)
            {
                case enKontur.Rechteck:
                    // GR.SmoothingModex = Drawing2D.SmoothingMode.None
                    return modAllgemein.Poly_Rechteck(r);

                case enKontur.Rechteck_R4:
                    // GR.SmoothingModex = Drawing2D.SmoothingMode.HighQuality
                    return modAllgemein.Poly_RoundRec(r, 4);

                case enKontur.Rechteck_R11:
                    //  GR.SmoothingModex = Drawing2D.SmoothingMode.HighQuality
                    return modAllgemein.Poly_RoundRec(r, 11);

                case enKontur.Rechteck_R20:
                    //    GR.SmoothingModex = Drawing2D.SmoothingMode.HighQuality
                    return modAllgemein.Poly_RoundRec(r, 20);


                //break; case Is = enKontur.Rechteck_R4_NurOben
                //    r.Y2 += 4
                //    GR.SmoothingModex = Drawing2D.SmoothingMode.HighQuality
                //    Return Poly_RoundRec(r, 4)


                case enKontur.Ohne:
                    return null;

                default:
                    //  GR.SmoothingModex = Drawing2D.SmoothingMode.None
                    return modAllgemein.Poly_Rechteck(r);
            }
        }

        #region  Border 

        public void Draw_Border(Graphics GR, enDesign vDesign, enStates vState, Rectangle r)
        {
            Draw_Border(GR, SkinRow(vDesign, vState), r);
        }


        public void Draw_Border(Graphics GR, RowItem Row, Rectangle r)
        {

            if (Row == null) { return; }

            var Kon = (enKontur)int.Parse(Value(Row, "Kontur", "-1"));
            if (Kon == enKontur.Ohne) { return; }

            var Rahm = (enRahmenArt)int.Parse(Value(Row, "Border_Style", "-1"));
            if (Rahm == enRahmenArt.Ohne) { return; }


            if (Kon == enKontur.Unbekannt)
            {
                Kon = enKontur.Rechteck;
                r.Width -= 1;
                r.Height = -1;
            }
            else
            {
                r.X -= int.Parse(Value(Row, "X1", "0"));
                r.Y -= int.Parse(Value(Row, "Y1", "0"));
                r.Width += int.Parse(Value(Row, "X1", "0")) + int.Parse(Value(Row, "X2", "0")) - 1;
                r.Height += int.Parse(Value(Row, "y1", "0")) + int.Parse(Value(Row, "Y2", "0")) - 1;
            }

            if (r.Width < 1 || r.Height < 1) { return; }


            // PathX kann durch die ganzen Expand mal zu klein werden, dann wird nothing zurückgegeben
            GraphicsPath PathX = null;
            Pen PenX = null;
            try
            {


                switch (Rahm)
                {
                    case enRahmenArt.Solide_1px:
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(int.Parse(Value(Row, "Color_Border_1", "0"))));
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    case enRahmenArt.Solide_1px_FocusDotLine:
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(int.Parse(Value(Row, "Color_Border_1", "0"))));
                        GR.DrawPath(PenX, PathX);
                        r.Inflate(-3, -3);

                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(int.Parse(Value(Row, "Color_Border_3", "0"))));
                        PenX.DashStyle = DashStyle.Dot;
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    case enRahmenArt.FocusDotLine:
                        PenX = new Pen(Color.FromArgb(int.Parse(Value(Row, "Color_Border_3", "0"))));
                        PenX.DashStyle = DashStyle.Dot;
                        r.Inflate(-3, -3);
                        PathX = Kontur(Kon, r);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }

                        break;

                    case enRahmenArt.Solide_3px:
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(int.Parse(Value(Row, "Color_Border_1", "0"))), 3);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    case enRahmenArt.Solide_1px_DuoColor:
                        PathX = Kontur(Kon, r);
                        r.Inflate(-1, -1);
                        Draw_Border_DuoColor(GR, Row, r, false);
                        PenX = new Pen(Color.FromArgb(int.Parse(Value(Row, "Color_Border_1", "0"))));
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    case enRahmenArt.Solide_1px_DuoColor_NurOben:
                        PathX = Kontur(Kon, r);
                        r.Inflate(-1, -1);
                        Draw_Border_DuoColor(GR, Row, r, true);
                        PenX = new Pen(Color.FromArgb(int.Parse(Value(Row, "Color_Border_1", "0"))));
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    case enRahmenArt.ShadowBox:
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(int.Parse(Value(Row, "Color_Border_3", "0"))), 1);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }


                        r.Width -= 1;
                        r.Height -= 1;

                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(int.Parse(Value(Row, "Color_Border_2", "0"))), 1);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }


                        r.Width -= 1;
                        r.Height -= 1;
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(int.Parse(Value(Row, "Color_Border_1", "0"))), 1);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    default:
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.Red);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        Develop.DebugPrint(Rahm);
                        break;
                }

            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
            }
        }


        private void Draw_Border_DuoColor(Graphics GR, RowItem Row, Rectangle r, bool NurOben)
        {

            LinearGradientBrush lgb = null;
            // Dim cb As New ColorBlend
            var c1 = Color.FromArgb(int.Parse(Value(Row, "Color_Border_2", "0")));
            var c2 = Color.FromArgb(int.Parse(Value(Row, "Color_Border_3", "0")));

            lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Height), c1, c2);
            lgb.GammaCorrection = true;

            var x = GR.SmoothingMode;

            GR.SmoothingMode = SmoothingMode.Default; //Returns the smoothing mode to default for a crisp structure
            GR.FillRectangle(lgb, r.Left, r.Top, r.Width + 1, 2); // Oben

            if (!NurOben)
            {
                GR.FillRectangle(lgb, r.Left, r.Bottom - 1, r.Width + 1, 2); // unten
                GR.FillRectangle(lgb, r.Left, r.Top, 2, r.Height + 1); // links
                GR.FillRectangle(lgb, r.Right - 1, r.Top, 2, r.Height + 1); // rechts
            }

            GR.SmoothingMode = x;
        }


        #endregion

        /// <summary>
        /// Bild wird auf Disabled geändert
        /// </summary>
        /// <param name="column"></param>
        /// <param name="Txt"></param>
        /// <param name="QI"></param>
        /// <param name="GR"></param>
        /// <param name="FitInRect"></param>
        /// <param name="vAlign"></param>
        /// <param name="Child"></param>
        /// <param name="DeleteBack"></param>
        /// <param name="SkinRow"></param>
        public void Draw_FormatedText(ColumnItem column, string Txt, QuickImage QI, Graphics GR, Rectangle FitInRect, enAlignment vAlign, System.Windows.Forms.Control Child, bool DeleteBack, RowItem SkinRow, enShortenStyle Style)
        {
            if (string.IsNullOrEmpty(Txt) && QI == null) { return; }

            if (SkinRow == null) { return; }

            BlueFont f = null;
            if (!string.IsNullOrEmpty(Txt)) { f = GetBlueFont(SkinRow); }

            //var State = (enStates)Convert.ToInt32(Value(SkinRow, "Status", "-1"));
            Draw_FormatedText(GR, column, Txt, QI, vAlign, FitInRect, Child, DeleteBack, f, Style);
        }


        /// <summary>
        /// Stati (Disabled) werden nicht mehr geändert
        /// </summary>
        /// <param name="GR"></param>
        /// <param name="column"></param>
        /// <param name="Txt"></param>
        /// <param name="ImageCode"></param>
        /// <param name="vAlign"></param>
        /// <param name="FitInRect"></param>
        /// <param name="Child"></param>
        /// <param name="DeleteBack"></param>
        /// <param name="F"></param>
        public void Draw_FormatedText(Graphics GR, ColumnItem column, string Txt, QuickImage ImageCode, enAlignment vAlign, Rectangle FitInRect, System.Windows.Forms.Control Child, bool DeleteBack, BlueFont F, enShortenStyle Style)
        {
            var tmpImageCode = Draw_FormatedText_PicOf(Txt, ImageCode, column);
            var tmpText = CellItem.ValueReadable(Txt, column, Style);
            vAlign = Draw_FormatedText_Alignment(column, tmpText, tmpImageCode, vAlign);

            Draw_FormatedText(GR, tmpText, tmpImageCode, vAlign, FitInRect, Child, DeleteBack, F);
        }



        /// <summary>
        /// Bild wird in dieser Routine nicht mehr gändert, aber in der nachfolgenden
        /// </summary>
        /// <param name="GR"></param>
        /// <param name="txt"></param>
        /// <param name="vDesign"></param>
        /// <param name="vState"></param>
        /// <param name="ImageCode"></param>
        /// <param name="vAlign"></param>
        /// <param name="FitInRect"></param>
        /// <param name="Child"></param>
        /// <param name="DeleteBack"></param>
        public void Draw_FormatedText(Graphics GR, string txt, enDesign vDesign, enStates vState, QuickImage ImageCode, enAlignment vAlign, Rectangle FitInRect, System.Windows.Forms.Control Child, bool DeleteBack)
        {
            Draw_FormatedText(GR, txt, ImageCode, SkinRow(vDesign, vState), vAlign, FitInRect, Child, DeleteBack);
        }

        /// <summary>
        /// Status des Bildes (Disabled) wird geändert
        /// </summary>
        /// <param name="GR"></param>
        /// <param name="TXT"></param>
        /// <param name="QI"></param>
        /// <param name="SkinRow"></param>
        /// <param name="vAlign"></param>
        /// <param name="FitInRect"></param>
        /// <param name="Child"></param>
        /// <param name="DeleteBack"></param>
        public void Draw_FormatedText(Graphics GR, string TXT, QuickImage QI, RowItem SkinRow, enAlignment vAlign, Rectangle FitInRect, System.Windows.Forms.Control Child, bool DeleteBack)
        {
            if (string.IsNullOrEmpty(TXT) && QI == null) { return; }

            if (SkinRow == null) { return; }

            BlueFont f = null;
            if (!string.IsNullOrEmpty(TXT)) { f = GetBlueFont(SkinRow); }

            var State = (enStates)Convert.ToInt32(Value(SkinRow, "Status", "-1"));
            QuickImage tmpImage = null;
            if (QI != null) { tmpImage = QuickImage.Get(QI, AdditionalState(State)); }

            Draw_FormatedText(GR, TXT, tmpImage, vAlign, FitInRect, Child, DeleteBack, f);
        }


        /// <summary>
        /// Zeichnet den Text und das Bild ohne weitere Modifikation
        /// </summary>
        /// <param name="GR"></param>
        /// <param name="TXT"></param>
        /// <param name="QI"></param>
        /// <param name="vAlign"></param>
        /// <param name="FitInRect"></param>
        /// <param name="Child"></param>
        /// <param name="DeleteBack"></param>
        /// <param name="F"></param>
        public void Draw_FormatedText(Graphics GR, string TXT, QuickImage QI, enAlignment vAlign, Rectangle FitInRect, System.Windows.Forms.Control Child, bool DeleteBack, BlueFont F)
        {


            GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            //  GR.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit

            var pSize = SizeF.Empty;
            var tSize = SizeF.Empty;

            float XP = 0;
            float YP1 = 0;
            float YP2 = 0;

            if (QI != null) { pSize = QI.BMP.Size; }


            if (F != null)
            {
                if (FitInRect.Width > 0) { TXT = TXT.TrimByWidth(FitInRect.Width - pSize.Width, F); }
                tSize = GR.MeasureString(TXT, F.Font());
            }


            if (Convert.ToBoolean(vAlign & enAlignment.Right)) { XP = FitInRect.Width - pSize.Width - tSize.Width; }
            if (Convert.ToBoolean(vAlign & enAlignment.HorizontalCenter)) { XP = (float)((FitInRect.Width - pSize.Width - tSize.Width) / 2.0); }

            if (Convert.ToBoolean(vAlign & enAlignment.VerticalCenter))
            {
                YP1 = (float)((FitInRect.Height - pSize.Height) / 2.0);
                YP2 = (float)((FitInRect.Height - tSize.Height) / 2.0);
            }
            if (Convert.ToBoolean(vAlign & enAlignment.Bottom))
            {
                YP1 = FitInRect.Height - pSize.Height;
                YP2 = FitInRect.Height - tSize.Height;
            }


            if (DeleteBack)
            {
                if (!string.IsNullOrEmpty(TXT)) { Draw_Back_Transparent(GR, new Rectangle(Convert.ToInt32(FitInRect.X + pSize.Width + XP - 1), Convert.ToInt32(FitInRect.Y + YP2 - 1), (int)(tSize.Width + 2), (int)(tSize.Height + 2)), Child); }

                if (QI != null) { Draw_Back_Transparent(GR, new Rectangle(Convert.ToInt32(FitInRect.X + XP), Convert.ToInt32(FitInRect.Y + YP1), (int)pSize.Width, (int)pSize.Height), Child); }
            }

            try
            {
                if (QI != null) { GR.DrawImage(QI.BMP, Convert.ToInt32(FitInRect.X + XP), Convert.ToInt32(FitInRect.Y + YP1)); }
                if (!string.IsNullOrEmpty(TXT)) { GR.DrawString(TXT, F.Font(), F.Brush_Color_Main, FitInRect.X + pSize.Width + XP, FitInRect.Y + YP2); }
            }
            catch (Exception ex)
            {
                // es kommt selten vor, dass das Graphics-Objekt an anderer Stelle verwendet wird. Was immer das auch heißen mag...
                Develop.DebugPrint(ex);
            }
        }



        public Size FormatedText_NeededSize(ColumnItem Column, string txt, QuickImage ImageCode, BlueFont F, enShortenStyle Style)
        {
            var tmpImageCode = Draw_FormatedText_PicOf(txt, ImageCode, Column);
            var tmpText = CellItem.ValueReadable(txt, Column, Style);

            return FormatedText_NeededSize(tmpText, tmpImageCode, F);
        }

        public Size FormatedText_NeededSize(string tmpText, QuickImage tmpImageCode, BlueFont F)
        {
            var pSize = SizeF.Empty;
            var tSize = SizeF.Empty;

            if (F == null) { return new Size(3, 3); }

            if (tmpImageCode != null) { pSize = tmpImageCode.BMP.Size; }
            if (!string.IsNullOrEmpty(tmpText)) { tSize = DummyGraphics().MeasureString(tmpText, F.Font()); }

            if (!string.IsNullOrEmpty(tmpText))
            {
                if (tmpImageCode == null)
                {
                    return new Size((int)(tSize.Width + 1), Math.Max((int)tSize.Height, 16)); // 16 muss es sein, weil Multi-Line den Multiplikator von 16 nimmt
                }

                return new Size((int)(tSize.Width + 2 + pSize.Width + 1), Math.Max((int)tSize.Height, (int)pSize.Height));
            }

            if (tmpImageCode != null)
            {
                return new Size((int)pSize.Width, (int)pSize.Height);
            }

            return new Size(16, 16);
        }

        private Graphics DummyGraphics()
        {

            if (DummyGR == null)
            {
                DummyBMP = new Bitmap(1, 1);
                DummyGR = Graphics.FromImage(DummyBMP);
            }
            return DummyGR;

        }
        internal BlueFont GetBlueFont(int _Design, enStates vState, RowItem RowOfStyle, int Stufe)
        {
            if (_Design > 10000)
            {
                return GenericControl.Skin.GetBlueFont((PadStyles)_Design, RowOfStyle, Stufe);
            }

            return GenericControl.Skin.GetBlueFont((enDesign)_Design, vState, Stufe);
        }

        internal BlueFont GetBlueFont(PadStyles vDesign, RowItem RowOfStyle, int Stufe)
        {
            switch (Stufe)
            {
                case 4:
                    return GetBlueFont(vDesign, RowOfStyle);

                case 3:
                    switch (vDesign)
                    {
                        case PadStyles.Style_Standard:
                            return GetBlueFont(PadStyles.Style_Überschrift_Kapitel, RowOfStyle);
                        case PadStyles.Style_StandardFett:
                            return GetBlueFont(PadStyles.Style_Überschrift_Kapitel, RowOfStyle);
                            //    Case Else : Return BlueFont(vDesign, vState)
                    }
                    break;

                case 2:
                    switch (vDesign)
                    {
                        case PadStyles.Style_Standard:
                            return GetBlueFont(PadStyles.Style_Überschrift_Untertitel, RowOfStyle);
                        case PadStyles.Style_StandardFett:
                            return GetBlueFont(PadStyles.Style_Überschrift_Untertitel, RowOfStyle);
                            //    Case Else : Return BlueFont(vDesign, vState)
                    }
                    break;

                case 1:
                    switch (vDesign)
                    {
                        case PadStyles.Style_Standard:
                            return GetBlueFont(PadStyles.Style_Überschrift_Haupt, RowOfStyle);
                        case PadStyles.Style_StandardFett:
                            return GetBlueFont(PadStyles.Style_Überschrift_Haupt, RowOfStyle);
                            //  Case Else : Return BlueFont(vDesign, vState)
                    }
                    break;

                case 7:
                    switch (vDesign)
                    {
                        case PadStyles.Style_Standard:
                            return GetBlueFont(PadStyles.Style_StandardFett, RowOfStyle);
                        case PadStyles.Style_StandardFett:
                            return GetBlueFont(PadStyles.Style_Standard, RowOfStyle);
                            //default: : Return BlueFont(vDesign, vState)
                    }
                    break;

            }

            Develop.DebugPrint(enFehlerArt.Fehler, "Stufe " + Stufe + " nicht definiert.");
            return null;
        }


        internal BlueFont GetBlueFont(enDesign vDesign, enStates vState, int Stufe)
        {

            if (Stufe != 4 && vDesign != enDesign.TextBox)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, "Design unbekannt: " + (int)vDesign);
                return GetBlueFont(vDesign, vState);
            }


            switch (Stufe)
            {
                case 4:
                    return GetBlueFont(vDesign, vState);

                case 3:
                    return GetBlueFont(enDesign.TextBox_Stufe3, vState);

                case 2:
                    return GetBlueFont(enDesign.TextBox_Stufe3, vState);

                case 1:
                    return GetBlueFont(enDesign.TextBox_Stufe3, vState);
                case 7:
                    return GetBlueFont(enDesign.TextBox_Bold, vState);
            }

            Develop.DebugPrint(enFehlerArt.Fehler, "Stufe " + Stufe + " nicht definiert.");
            return GetBlueFont(vDesign, vState);
        }












        internal BlueFont GetBlueFont(PadStyles vFormat, RowItem RowOfStyle)
        {
            if (StyleDB == null) { InitStyles(); }
            if (StyleDB == null || RowOfStyle == null) { return BlueFont.Get(ErrorFont); }

            return GetBlueFont(StyleDB, Convert.ToInt32(vFormat).ToString(), RowOfStyle);
        }

        public BlueFont GetBlueFont(Database StyleDB, string ColumnName, RowItem Row)
        {
            return GetBlueFont(StyleDB, StyleDB.Column[ColumnName], Row);
        }

        public BlueFont GetBlueFont(Database StyleDB, ColumnItem Column, RowItem Row)
        {
            var _String = StyleDB.Cell.GetString(Column, Row);
            if (string.IsNullOrEmpty(_String))
            {
                Develop.DebugPrint("Schrift nicht definiert: " + StyleDB.Filename + " - " + Column.Name + " - " + Row.CellFirstString());
                return null;
            }
            return BlueFont.Get(_String);
        }



        public BlueFont GetBlueFont(enDesign vDesign, enStates vState)
        {
            return GetBlueFont(SkinRow(vDesign, vState));
        }


        public BlueFont GetBlueFont(RowItem Row)
        {
            return BlueFont.Get(Value(Row, "Font", ErrorFont));
        }


        private void Draw_Back_Verlauf_Vertical_3(Graphics GR, RowItem Row, Rectangle r)
        {

            LinearGradientBrush lgb = null;
            var cb = new ColorBlend();

            var c1 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_1", "0")));
            var c2 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_2", "0")));
            var c3 = Color.FromArgb(int.Parse(Value(Row, "Color_Back_3", "0")));
            var PR = float.Parse(Value(Row, "Verlauf_Mitte", "0,5"));
            lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Bottom), c1, c3);

            cb.Colors = new[] { c1, c2, c3 };
            cb.Positions = new[] { 0.0F, PR, 1.0F };

            lgb.InterpolationColors = cb;
            lgb.GammaCorrection = true;

            GR.FillRectangle(lgb, r);
        }


        public string PicCode(RowItem _Row)
        {
            return Value(_Row, "StandardPic", "Kreuz|16");
        }

        public bool IsReady()
        {
            if (SkinDB == null || SkinDB.Column.Count < 5) { return false; }
            return true;
        }


        #region  Styles 

        public static List<string> AllStyles()
        {
            if (StyleDB == null) { InitStyles(); }

            return StyleDB?.Column[0].Contents(null);
        }


        public static void InitStyles()
        {
            StyleDB = Database.LoadResource(Assembly.GetAssembly(typeof(clsSkin)), "Styles.MDB", "Styles", true, false, Table.Database_NeedPassword, CreativePad.GenerateLayoutFromRow, CreativePad.RenameColumnInLayout);
        }

        #endregion

        public static QuickImage Draw_FormatedText_PicOf(string Txt, QuickImage ImageCode, ColumnItem Column)
        {

            switch (Column.Format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Text_Ohne_Kritische_Zeichen:
                case enDataFormat.RelationText:
                    return ImageCode; // z.B. KontextMenu

                case enDataFormat.Bit:
                    if (Txt == true.ToPlusMinus())
                    {
                        return QuickImage.Get(enImageCode.Häkchen, 16);
                    }
                    else if (Txt == false.ToPlusMinus())
                    {
                        return QuickImage.Get(enImageCode.Kreuz, 16);
                    }
                    else if (Txt == "o" || Txt == "O")
                    {
                        return QuickImage.Get(enImageCode.Kreis2, 16);
                    }
                    else if (Txt == "?")
                    {
                        return QuickImage.Get(enImageCode.Fragezeichen, 16);
                    }
                    else
                    {
                        return QuickImage.Get(enImageCode.Kritisch, 16);
                    }


                case enDataFormat.BildCode:
                    if (ImageCode != null) { return ImageCode; }// z.B. Dropdownmenu-Textfeld mit bereits definierten Icon
                    if (Column.BildCode_ConstantHeight > 0) { Txt = Txt + "|" + Column.BildCode_ConstantHeight; }
                    ImageCode = QuickImage.Get(Column.Prefix + Txt + Column.Suffix);
                    if (ImageCode.IsError)
                    {
                        if (Column.BildCode_ImageNotFound != enImageNotFound.ShowErrorPic) { return null; }
                        Txt = "Fragezeichen||||||200|||80";
                        if (Column.BildCode_ConstantHeight > 0) { Txt = "Fragezeichen|" + Column.BildCode_ConstantHeight + "|||||200|||80"; }
                    }


                    return QuickImage.Get(Txt);


                case enDataFormat.Farbcode:

                    if (!string.IsNullOrEmpty(Txt) && Txt.IsFormat(enDataFormat.Farbcode))
                    {
                        var col = Color.FromArgb(int.Parse(Txt));
                        return QuickImage.Get(enImageCode.Kreis, 16, "", col.ToHTMLCode());
                    }
                    return null;


                //case enDataFormat.Relation:
                //    if (ImageCode != null) { return ImageCode; }
                //    if (!string.IsNullOrEmpty(Txt)) { return new clsRelation(Column, null, Txt).SymbolForReadableText(); }
                //    return null;



                case enDataFormat.Link_To_Filesystem:
                    if (ImageCode != null) { return ImageCode; }
                    if (Txt.FileType() == enFileFormat.Unknown) { return null; }
                    return QuickImage.Get(Txt.FileType(), 48);

                case enDataFormat.Schrift:
                    Develop.DebugPrint_NichtImplementiert();
                    //if (string.IsNullOrEmpty(Txt) || Txt.Substring(0, 1) != "{") { return ImageCode; }
                    //return BlueFont.Get(Txt).SymbolForReadableText();
                    return null;

                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    return null;

                default:
                    return null;

            }




        }



        public static enAlignment Draw_FormatedText_Alignment(ColumnItem Column, string Txt, QuickImage ImageCode, enAlignment Aling)
        {

            switch (Column.Format)
            {
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                    return enAlignment.Top_Right;

                case enDataFormat.Bit:
                    if (Column.CompactView) { return enAlignment.Top_HorizontalCenter; }
                    return enAlignment.Top_Left;

                default:
                    return Aling;
            }

        }







    }
}
