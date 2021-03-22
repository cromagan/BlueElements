#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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


using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;

namespace BlueControls {
    public static class Skin {
        public static Database SkinDB;
        public static Database StyleDB;

        private static readonly enImageCodeEffect[] ST = new enImageCodeEffect[1];
        internal static Pen Pen_LinieDünn;
        internal static Pen Pen_LinieKräftig;
        internal static Pen Pen_LinieDick;

        private static string _SkinString = string.Empty;


        public static readonly float Scale = (float)Math.Round(System.Windows.Forms.SystemInformation.VirtualScreen.Width / System.Windows.SystemParameters.VirtualScreenWidth, 2);


        public static string ErrorFont = "<Name=Arial, Size=8, Color=FF0000>";
        public static string DummyStandardFont = "<Name=Arial, Size=10>";


        public static readonly int PaddingSmal = 3; // Der Abstand von z.B. in Textboxen: Text Linke Koordinate
        public static readonly int Padding = 9;


        private static enStates SkinRow_LastState = enStates.Undefiniert;
        private static enDesign SkinRow_LastType = enDesign.Undefiniert;
        private static RowItem SkinRow_LastRow;



        private static ColumnItem ColX1 = null;
        private static ColumnItem ColX2 = null;
        private static ColumnItem ColY2 = null;
        private static ColumnItem ColY1 = null;

        private static ColumnItem col_Color_Back_1 = null;
        private static ColumnItem col_Color_Back_2 = null;
        private static ColumnItem col_Color_Back_3 = null;
        private static ColumnItem col_Color_Border_1 = null;
        private static ColumnItem col_Color_Border_2 = null;
        private static ColumnItem col_Color_Border_3 = null;
        private static ColumnItem col_Kontur = null;
        private static ColumnItem col_Draw_Back = null;
        private static ColumnItem col_Verlauf_Mitte = null;
        private static ColumnItem col_Border_Style = null;

        //private static ColumnItem col_Status = null;
        private static ColumnItem col_Font = null;
        private static ColumnItem col_StandardPic = null;




        public static void LoadSkin() {
            _SkinString = "Windows10";


            SkinDB = Database.LoadResource(Assembly.GetAssembly(typeof(Skin)), _SkinString + ".skn", "Skin", true, Convert.ToBoolean(Develop.AppName() == "SkinDesigner"));




            ColX1 = SkinDB.Column["X1"];
            ColX2 = SkinDB.Column["X2"];
            ColY1 = SkinDB.Column["Y1"];
            ColY2 = SkinDB.Column["Y2"];

            col_Color_Back_1 = SkinDB.Column["Color_Back_1"];
            col_Color_Back_2 = SkinDB.Column["Color_Back_2"];
            col_Color_Back_3 = SkinDB.Column["Color_Back_3"];
            col_Color_Border_1 = SkinDB.Column["Color_Border_1"];
            col_Color_Border_2 = SkinDB.Column["Color_Border_2"];
            col_Color_Border_3 = SkinDB.Column["Color_Border_3"];
            col_Kontur = SkinDB.Column["Kontur"];
            col_Draw_Back = SkinDB.Column["Draw_Back"];
            col_Verlauf_Mitte = SkinDB.Column["Verlauf_Mitte"];
            col_Border_Style = SkinDB.Column["Border_Style"];

            //col_Status = SkinDB.Column["Status"];
            col_Font = SkinDB.Column["Font"];
            col_StandardPic = SkinDB.Column["StandardPic"];


            ST[0] = (enImageCodeEffect)int.Parse(SkinDB?.Tags[0]);
            Pen_LinieDünn = new Pen(Color_Border(enDesign.Table_Lines_thin, enStates.Standard));
            Pen_LinieKräftig = new Pen(Color_Border(enDesign.Table_Lines_thick, enStates.Standard));
            Pen_LinieDick = new Pen(Color_Border(enDesign.Table_Lines_thick, enStates.Standard), 3);

        }






        public static enImageCodeEffect AdditionalState(enStates vState) {
            if (vState.HasFlag(enStates.Standard_Disabled)) { return ST[0]; }
            return enImageCodeEffect.Ohne;
        }

        public static Color Color_Back(enDesign vDesign, enStates vState) {
            return Color.FromArgb(Value(SkinRow(vDesign, vState), col_Color_Back_1, 0));
        }

        internal static Color Color_Border(enDesign vDesign, enStates vState) {
            return Color.FromArgb(Value(SkinRow(vDesign, vState), col_Color_Border_1, 0));
        }

        private static int Value(RowItem row, ColumnItem column, int defaultValue) {
            if (SkinDB == null || row == null) { return defaultValue; }


            //if (row.CellIsNullOrEmpty(column))
            //{
            //    Develop.DebugPrint("Skin-Fehler: " + row.Database.Filename + " - " + column.Caption + " - " + row.CellFirstString());
            //    return defaultValue;
            //}

            return row.CellGetInteger(column);
        }

        private static float Value(RowItem row, ColumnItem column, float defaultValue) {
            if (SkinDB == null || row == null) { return defaultValue; }


            //if (row.CellIsNullOrEmpty(column))
            //{
            //    Develop.DebugPrint("Skin-Fehler: " + row.Database.Filename + " - " + column.Caption + " - " + row.CellFirstString());
            //    return defaultValue;
            //}

            return (float)row.CellGetDouble(column);
        }



        private static string Value(RowItem row, ColumnItem column, string defaultValue) {
            if (SkinDB == null || row == null) { return defaultValue; }


            //if (row.CellIsNullOrEmpty(column))
            //{
            //    Develop.DebugPrint("Skin-Fehler: " + row.Database.Filename + " - " + column.Caption + " - " + row.CellFirstString());
            //    return defaultValue;
            //}

            return row.CellGetString(column);
        }


        internal static RowItem SkinRow(enDesign vDesign, enStates vState) {

            //Kann vorkommen, wenn die Database zweck Userwechsel neu geladen wird
            if (SkinDB == null) { return null; }


            if (SkinRow_LastType == vDesign && SkinRow_LastState == vState && SkinRow_LastRow == null) { return SkinRow_LastRow; }

            SkinRow_LastRow = SkinDB.Row[new FilterItem(SkinDB.Column[0], enFilterType.Istgleich, ((int)vDesign).ToString()),
                                         new FilterItem(SkinDB.Column[1], enFilterType.Istgleich, ((int)vState).ToString())];
            SkinRow_LastState = vState;
            SkinRow_LastType = vDesign;

            if (SkinRow_LastRow == null) { Develop.DebugPrint("Unbekanntes Skin: " + SkinDB.Filename.FileNameWithoutSuffix() + "/" + vDesign + "/" + vState); }

            return SkinRow_LastRow;
        }



        #region  Back 


        public static void Draw_Back(Graphics GR, enDesign vDesign, enStates vState, Rectangle r, System.Windows.Forms.Control vControl, bool NeedTransparenz) {
            Draw_Back(GR, SkinRow(vDesign, vState), r, vControl, NeedTransparenz);
        }


        public static void Draw_Back(Graphics GR, RowItem cRow, Rectangle r, System.Windows.Forms.Control cControl, bool NeedTransparenz) {
            try {
                bool Need = true;
                int X1 = 0;
                int Y1 = 0;
                int X2 = 0;
                int Y2 = 0;
                enHintergrundArt HA = enHintergrundArt.Ohne;

                if (!NeedTransparenz || cControl == null) { Need = false; }


                enKontur Kon = (enKontur)Value(cRow, col_Kontur, -1);
                if (Kon == enKontur.Unbekannt) { Kon = enKontur.Rechteck; }

                if (Kon != enKontur.Ohne) {

                    HA = (enHintergrundArt)Value(cRow, col_Draw_Back, -1);

                    X1 = Value(cRow, ColX1, 0);
                    Y1 = Value(cRow, ColY1, 0);
                    X2 = Value(cRow, ColX2, 0);
                    Y2 = Value(cRow, ColY2, 0);


                    if (HA != enHintergrundArt.Ohne) {
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


                switch (HA) {
                    case enHintergrundArt.Ohne:
                        break;

                    case enHintergrundArt.Solide:
                        PathX = Kontur(Kon, r);
                        SolidBrush BrushX = new SolidBrush(Color.FromArgb(Value(cRow, col_Color_Back_1, 0)));
                        GR.FillPath(BrushX, PathX);
                        break;

                    case enHintergrundArt.Verlauf_Vertical_2:
                        PathX = Kontur(Kon, r);
                        Color c1 = Color.FromArgb(Value(cRow, col_Color_Back_1, 0));
                        Color c2 = Color.FromArgb(Value(cRow, col_Color_Back_2, 0));
                        LinearGradientBrush lgb = new LinearGradientBrush(r, c1, c2, LinearGradientMode.Vertical);
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
                        Color cx1 = Color.FromArgb(Value(cRow, col_Color_Back_1, 0));
                        Color cx2 = Color.FromArgb(Value(cRow, col_Color_Back_2, 0));
                        Color cx3 = Color.FromArgb(Value(cRow, col_Color_Back_3, 0));
                        float PR = Value(cRow, col_Verlauf_Mitte, 0.7f);
                        LinearGradientBrush lgb2 = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Right, r.Bottom), cx1, cx3);
                        ColorBlend cb = new ColorBlend {
                            Colors = new[] { cx1, cx2, cx3 },
                            Positions = new[] { 0.0F, PR, 1.0F }
                        };

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
            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }



        }


        public static void Draw_Back_Transparent(Graphics GR, Rectangle r, System.Windows.Forms.Control vControl) {
            if (vControl?.Parent == null) { return; }


            switch (vControl.Parent) {
                case IUseMyBackColor _:
                    GR.FillRectangle(new SolidBrush(vControl.Parent.BackColor), r);
                    return;

                case IBackgroundNone _:
                    Draw_Back_Transparent(GR, r, vControl.Parent);
                    break;

                case GenericControl TRB:
                    if (TRB.BitmapOfControl() == null) { return; }
                    GR.DrawImage(TRB.BitmapOfControl(), r, new Rectangle(vControl.Left + r.Left, vControl.Top + r.Top, r.Width, r.Height), GraphicsUnit.Pixel);
                    break;



                case System.Windows.Forms.Form frm:

                    //if (frm.BackgroundImage != null)
                    //{
                    //    // Wichtig, für Farbverläufe in MSGBoxen
                    //    GR.DrawImage(frm.BackgroundImage, r, new Rectangle(vControl.Left + r.Left, vControl.Top + r.Top, r.Width, r.Height), GraphicsUnit.Pixel);
                    //}
                    //else
                    //{
                    GR.FillRectangle(new SolidBrush(frm.BackColor), r);
                    //}
                    break;

                //case TabPage tab:
                //    GR.FillRectangle(new SolidBrush(tab.BackColor), r);
                //    break;

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

        private static void Draw_Back_Verlauf_Vertical_Glanzpunkt(Graphics GR, RowItem Row, Rectangle r) {

            ColorBlend cb = new ColorBlend();

            Color c1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));
            Color c2 = Color.FromArgb(Value(Row, col_Color_Back_2, 0));
            float PR = Value(Row, col_Verlauf_Mitte, 0.05f);


            if (PR < 0.06F) { PR = 0.06F; }
            if (PR > 0.94F) { PR = 0.94F; }

            LinearGradientBrush lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Bottom), c1, c1);


            cb.Colors = new[] { c1, c2, c1, c1, c1.Darken(0.3), c1 };
            cb.Positions = new[]
            {
                0.0F, (float) (PR - 0.05), (float) (PR + 0.05), (float) (1 - PR - 0.05), (float) (1 - PR + 0.05), 1.0F
            };


            lgb.InterpolationColors = cb;
            lgb.GammaCorrection = true;

            GR.FillRectangle(lgb, r);
        }

        private static void Draw_Back_Verlauf_Horizontal_2(Graphics GR, RowItem Row, Rectangle r) {
            Color c1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));
            Color c2 = Color.FromArgb(Value(Row, col_Color_Back_2, 0));
            LinearGradientBrush lgb = new LinearGradientBrush(r, c1, c2, LinearGradientMode.Horizontal);
            GR.FillRectangle(lgb, r);

        }

        private static void Draw_Back_Verlauf_Horizontal_3(Graphics GR, RowItem Row, Rectangle r) {

            ColorBlend cb = new ColorBlend();

            Color c1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));
            Color c2 = Color.FromArgb(Value(Row, col_Color_Back_2, 0));
            Color c3 = Color.FromArgb(Value(Row, col_Color_Back_3, 0));
            float PR = Value(Row, col_Verlauf_Mitte, 0.5f);


            LinearGradientBrush lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Right, r.Top), c1, c3);

            cb.Colors = new[] { c1, c2, c3 };
            cb.Positions = new[] { 0.0F, PR, 1.0F };

            lgb.InterpolationColors = cb;
            lgb.GammaCorrection = true;

            GR.FillRectangle(lgb, r);
            GR.DrawLine(new Pen(c3), r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);
        }

        private static void Draw_Back_Glossy(Graphics GR, RowItem Row, Rectangle r) {
            Color col1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));


            ColorBlend cb = new ColorBlend();

            Color c1 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.Black, 1), 0.4);
            Color c2 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.FromArgb(64, 64, 64), 1), 0.2);
            Color c3 = Extensions.SoftLightMix(col1, Color.FromArgb(128, 128, 128), 1);
            Color c4 = Extensions.SoftLightMix(col1, Color.FromArgb(192, 192, 192), 1);
            Color c5 = Extensions.OverlayMix(Extensions.SoftLightMix(col1, Color.White, 1), Color.White, 0.75);


            cb.Colors = new[] { c1, c2, c3, c4, c5 };
            cb.Positions = new[] { 0.0F, 0.25F, 0.5F, 0.75F, 1 };
            LinearGradientBrush lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Top + r.Height + 1), c1, c5) {
                InterpolationColors = cb
            };
            Draw_Back_Glossy_TMP(lgb, r, GR, 20);

            c2 = Color.White;
            cb.Colors = new[] { c2, c3, c4, c5 };
            cb.Positions = new[] { 0.0F, 0.5F, 0.75F, 1.0F };
            lgb = new LinearGradientBrush(new Point(r.Left + 1, r.Top), new Point(r.Left + 1, r.Top + r.Height - 1), c2, c5) {
                InterpolationColors = cb
            };

            r.Inflate(-4, -4);
            GR.SmoothingMode = SmoothingMode.HighQuality;
            Draw_Back_Glossy_TMP(lgb, r, GR, 16);

        }

        private static void Draw_Back_GlossyPressed(Graphics GR, RowItem Row, Rectangle r) {

            Color col1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));


            ColorBlend cb = new ColorBlend();

            Color c5 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.Black, 1), 0.4);
            Color c4 = Extensions.MixColor(Color.White, Extensions.SoftLightMix(col1, Color.FromArgb(64, 64, 64), 1), 0.2);
            Color c3 = Extensions.SoftLightMix(col1, Color.FromArgb(128, 128, 128), 1);
            Color c2 = Extensions.SoftLightMix(col1, Color.FromArgb(192, 192, 192), 1);
            Color c1 = Extensions.OverlayMix(Extensions.SoftLightMix(col1, Color.White, 1), Color.White, 0.75);


            cb.Colors = new[] { c1, c2, c3, c4, c5 };
            cb.Positions = new[] { 0.0F, 0.25F, 0.5F, 0.75F, 1 };
            LinearGradientBrush lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Top + r.Height + 1), c1, c5) {
                InterpolationColors = cb
            };
            Draw_Back_Glossy_TMP(lgb, r, GR, 20);

            c2 = Color.White;
            cb.Colors = new[] { c2, c3, c4, c5 };
            cb.Positions = new[] { 0.0F, 0.5F, 0.75F, 1.0F };
            lgb = new LinearGradientBrush(new Point(r.Left + 1, r.Top), new Point(r.Left + 1, r.Top + r.Height - 1), c2, c5) {
                InterpolationColors = cb
            };


            r.Inflate(-4, -4);
            GR.SmoothingMode = SmoothingMode.HighQuality;
            Draw_Back_Glossy_TMP(lgb, r, GR, 16);
            //    GR.SmoothingModex = Drawing2D.SmoothingMode.None
        }

        private static void Draw_Back_Glossy_TMP(Brush b, Rectangle rect, Graphics GR, int RMinus) {

            int r = Math.Min(RMinus, Math.Min(rect.Width, rect.Height) - 1);
            int r2 = (int)Math.Truncate(r / 2.0);
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


        private static GraphicsPath Kontur(enKontur Kon, Rectangle r) {


            switch (Kon) {
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

        public static void Draw_Border(Graphics GR, enDesign vDesign, enStates vState, Rectangle r) {
            Draw_Border(GR, SkinRow(vDesign, vState), r);
        }



        //private static System.Windows.Forms.Padding Paddings(RowItem Row)
        //{

        //    return new System.Windows.Forms.Padding(Value(Row, ColX1, 0), Value(Row, ColY1, 0), Value(Row, ColX2, 0), Value(Row, ColY2, 0));


        //}

        public static void Draw_Border(Graphics GR, RowItem Row, Rectangle r) {

            if (Row == null) { return; }

            enKontur Kon = (enKontur)Value(Row, col_Kontur, -1);
            if (Kon == enKontur.Ohne) { return; }

            enRahmenArt Rahm = (enRahmenArt)Value(Row, col_Border_Style, -1);
            if (Rahm == enRahmenArt.Ohne) { return; }


            if (Kon == enKontur.Unbekannt) {
                Kon = enKontur.Rechteck;
                r.Width--;
                r.Height = -1;
            } else {
                r.X -= Value(Row, ColX1, 0);
                r.Y -= Value(Row, ColY1, 0);
                r.Width += Value(Row, ColX1, 0) + Value(Row, ColX2, 0) - 1;
                r.Height += Value(Row, ColY1, 0) + Value(Row, ColY2, 0) - 1;
            }

            if (r.Width < 1 || r.Height < 1) { return; }


            // PathX kann durch die ganzen Expand mal zu klein werden, dann wird nothing zurückgegeben
            GraphicsPath PathX;
            Pen PenX;
            try {


                switch (Rahm) {
                    case enRahmenArt.Solide_1px:
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(Value(Row, col_Color_Border_1, 0)));
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    case enRahmenArt.Solide_1px_FocusDotLine:
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(Value(Row, col_Color_Border_1, 0)));
                        GR.DrawPath(PenX, PathX);
                        r.Inflate(-3, -3);

                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(Value(Row, col_Color_Border_3, 0))) {
                            DashStyle = DashStyle.Dot
                        };
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    case enRahmenArt.FocusDotLine:
                        PenX = new Pen(Color.FromArgb(Value(Row, col_Color_Border_3, 0))) {
                            DashStyle = DashStyle.Dot
                        };
                        r.Inflate(-3, -3);
                        PathX = Kontur(Kon, r);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }

                        break;

                    case enRahmenArt.Solide_3px:
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(Value(Row, col_Color_Border_1, 0)), 3);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    case enRahmenArt.Solide_1px_DuoColor:
                        PathX = Kontur(Kon, r);
                        r.Inflate(-1, -1);
                        Draw_Border_DuoColor(GR, Row, r, false);
                        PenX = new Pen(Color.FromArgb(Value(Row, col_Color_Border_1, 0)));
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    case enRahmenArt.Solide_1px_DuoColor_NurOben:
                        PathX = Kontur(Kon, r);
                        r.Inflate(-1, -1);
                        Draw_Border_DuoColor(GR, Row, r, true);
                        PenX = new Pen(Color.FromArgb(Value(Row, col_Color_Border_1, 0)));
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    case enRahmenArt.ShadowBox:
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(Value(Row, col_Color_Border_3, 0)), 1);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }


                        r.Width--;
                        r.Height--;

                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(Value(Row, col_Color_Border_2, 0)), 1);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }


                        r.Width--;
                        r.Height--;
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.FromArgb(Value(Row, col_Color_Border_1, 0)), 1);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        break;

                    default:
                        PathX = Kontur(Kon, r);
                        PenX = new Pen(Color.Red);
                        if (PathX != null) { GR.DrawPath(PenX, PathX); }
                        Develop.DebugPrint(Rahm);
                        break;
                }

            } catch (Exception ex) {
                Develop.DebugPrint(ex);
            }
        }


        private static void Draw_Border_DuoColor(Graphics GR, RowItem Row, Rectangle r, bool NurOben) {


            Color c1 = Color.FromArgb(Value(Row, col_Color_Border_2, 0));
            Color c2 = Color.FromArgb(Value(Row, col_Color_Border_3, 0));

            LinearGradientBrush lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Height), c1, c2) {
                GammaCorrection = true
            };

            SmoothingMode x = GR.SmoothingMode;

            GR.SmoothingMode = SmoothingMode.Default; //Returns the smoothing mode to default for a crisp structure
            GR.FillRectangle(lgb, r.Left, r.Top, r.Width + 1, 2); // Oben

            if (!NurOben) {
                GR.FillRectangle(lgb, r.Left, r.Bottom - 1, r.Width + 1, 2); // unten
                GR.FillRectangle(lgb, r.Left, r.Top, 2, r.Height + 1); // links
                GR.FillRectangle(lgb, r.Right - 1, r.Top, 2, r.Height + 1); // rechts
            }

            GR.SmoothingMode = x;
        }


        #endregion




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
        public static void Draw_FormatedText(Graphics GR, string txt, enDesign vDesign, enStates vState, QuickImage ImageCode, enAlignment vAlign, Rectangle FitInRect, System.Windows.Forms.Control Child, bool DeleteBack, bool Translate) {
            Draw_FormatedText(GR, txt, ImageCode, SkinRow(vDesign, vState), vState, vAlign, FitInRect, Child, DeleteBack, Translate);
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
        public static void Draw_FormatedText(Graphics GR, string TXT, QuickImage QI, RowItem SkinRow, enStates State, enAlignment vAlign, Rectangle FitInRect, System.Windows.Forms.Control Child, bool DeleteBack, bool Translate) {
            if (string.IsNullOrEmpty(TXT) && QI == null) { return; }

            if (SkinRow == null) { return; }

            BlueFont f = null;
            if (!string.IsNullOrEmpty(TXT)) { f = GetBlueFont(SkinRow); }

            //  var State = (enStates)Value(SkinRow, col_Status, -1);
            QuickImage tmpImage = null;
            if (QI != null) { tmpImage = QuickImage.Get(QI, AdditionalState(State)); }

            Draw_FormatedText(GR, TXT, tmpImage, vAlign, FitInRect, Child, DeleteBack, f, Translate);
        }

        public static ItemCollectionList GetRahmenArt(RowItem SheetStyle, bool MitOhne) {

            ItemCollectionList Rahms = new ItemCollectionList();
            if (MitOhne) {
                Rahms.Add("Ohne Rahmen", ((int)PadStyles.Undefiniert).ToString(), enImageCode.Kreuz);
            }


            Rahms.Add("Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Haupt).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Haupt, SheetStyle).SymbolOfLine());
            Rahms.Add("Untertitel für Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Untertitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Untertitel, SheetStyle).SymbolOfLine());
            Rahms.Add("Überschrift für Kapitel", ((int)PadStyles.Style_Überschrift_Kapitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Kapitel, SheetStyle).SymbolOfLine());
            Rahms.Add("Standard", ((int)PadStyles.Style_Standard).ToString(), GetBlueFont(PadStyles.Style_Standard, SheetStyle).SymbolOfLine());
            Rahms.Add("Standard Fett", ((int)PadStyles.Style_StandardFett).ToString(), GetBlueFont(PadStyles.Style_StandardFett, SheetStyle).SymbolOfLine());
            Rahms.Add("Standard Alternativ-Design", ((int)PadStyles.Style_StandardAlternativ).ToString(), GetBlueFont(PadStyles.Style_StandardAlternativ, SheetStyle).SymbolOfLine());
            Rahms.Add("Kleiner Zusatz", ((int)PadStyles.Style_KleinerZusatz).ToString(), GetBlueFont(PadStyles.Style_KleinerZusatz, SheetStyle).SymbolOfLine());
            Rahms.Sort();

            return Rahms;

        }



        public static ItemCollectionList GetFonts(RowItem SheetStyle) {

            ItemCollectionList Rahms = new ItemCollectionList
            {
                //   Rahms.Add(New ItemCollection.TextListItem(CInt(PadStyles.Undefiniert).ToString, "Ohne Rahmen", enImageCode.Kreuz))
                { "Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Haupt).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Haupt, SheetStyle).SymbolForReadableText() },
                { "Untertitel für Haupt-Überschrift", ((int)PadStyles.Style_Überschrift_Untertitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Untertitel, SheetStyle).SymbolForReadableText() },
                { "Überschrift für Kapitel", ((int)PadStyles.Style_Überschrift_Kapitel).ToString(), GetBlueFont(PadStyles.Style_Überschrift_Kapitel, SheetStyle).SymbolForReadableText() },
                { "Standard", ((int)PadStyles.Style_Standard).ToString(), GetBlueFont(PadStyles.Style_Standard, SheetStyle).SymbolForReadableText() },
                { "Standard Fett", ((int)PadStyles.Style_StandardFett).ToString(), GetBlueFont(PadStyles.Style_StandardFett, SheetStyle).SymbolForReadableText() },
                { "Standard Alternativ-Design", ((int)PadStyles.Style_StandardAlternativ).ToString(), GetBlueFont(PadStyles.Style_StandardAlternativ, SheetStyle).SymbolForReadableText() },
                { "Kleiner Zusatz", ((int)PadStyles.Style_KleinerZusatz).ToString(), GetBlueFont(PadStyles.Style_KleinerZusatz, SheetStyle).SymbolForReadableText() }
            };
            Rahms.Sort();

            return Rahms;

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
        public static void Draw_FormatedText(Graphics GR, string TXT, QuickImage QI, enAlignment vAlign, Rectangle FitInRect, System.Windows.Forms.Control Child, bool DeleteBack, BlueFont F, bool Translate) {

            if (GR.TextRenderingHint != TextRenderingHint.ClearTypeGridFit) {
                GR.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            }


            //  GR.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit

            SizeF pSize = SizeF.Empty;
            SizeF tSize = SizeF.Empty;

            float XP = 0;
            float YP1 = 0;
            float YP2 = 0;

            if (QI != null) { pSize = QI.BMP.Size; }


            if (LanguageTool.Translation != null) { TXT = LanguageTool.DoTranslate(TXT, Translate); }



            if (F != null) {
                if (FitInRect.Width > 0) { TXT = TXT.TrimByWidth(FitInRect.Width - pSize.Width, F); }
                tSize = GR.MeasureString(TXT, F.Font());
            }


            if (vAlign.HasFlag(enAlignment.Right)) { XP = FitInRect.Width - pSize.Width - tSize.Width; }
            if (vAlign.HasFlag(enAlignment.HorizontalCenter)) { XP = (float)((FitInRect.Width - pSize.Width - tSize.Width) / 2.0); }

            if (vAlign.HasFlag(enAlignment.VerticalCenter)) {
                YP1 = (float)((FitInRect.Height - pSize.Height) / 2.0);
                YP2 = (float)((FitInRect.Height - tSize.Height) / 2.0);
            }
            if (vAlign.HasFlag(enAlignment.Bottom)) {
                YP1 = FitInRect.Height - pSize.Height;
                YP2 = FitInRect.Height - tSize.Height;
            }


            if (DeleteBack) {
                if (!string.IsNullOrEmpty(TXT)) { Draw_Back_Transparent(GR, new Rectangle((int)(FitInRect.X + pSize.Width + XP - 1), (int)(FitInRect.Y + YP2 - 1), (int)(tSize.Width + 2), (int)(tSize.Height + 2)), Child); }

                if (QI != null) { Draw_Back_Transparent(GR, new Rectangle((int)(FitInRect.X + XP), (int)(FitInRect.Y + YP1), (int)pSize.Width, (int)pSize.Height), Child); }
            }

            try {
                if (QI != null) { GR.DrawImage(QI.BMP, (int)(FitInRect.X + XP), (int)(FitInRect.Y + YP1)); }
                if (!string.IsNullOrEmpty(TXT)) { GR.DrawString(TXT, F.Font(), F.Brush_Color_Main, FitInRect.X + pSize.Width + XP, FitInRect.Y + YP2); }
            } catch (Exception) {
                // es kommt selten vor, dass das Graphics-Objekt an anderer Stelle verwendet wird. Was immer das auch heißen mag...
                //Develop.DebugPrint(ex);
            }
        }



        public static Size FormatedText_NeededSize(string tmpText, QuickImage tmpImageCode, BlueFont F, int MinSize) {
            SizeF pSize = SizeF.Empty;
            SizeF tSize = SizeF.Empty;

            if (F == null) { return new Size(3, 3); }

            if (tmpImageCode != null) { pSize = tmpImageCode.BMP.Size; }
            if (!string.IsNullOrEmpty(tmpText)) { tSize = BlueFont.MeasureString(tmpText, F.Font()); }

            if (!string.IsNullOrEmpty(tmpText)) {
                if (tmpImageCode == null) {
                    return new Size((int)(tSize.Width + 1), Math.Max((int)tSize.Height, MinSize));
                }

                return new Size((int)(tSize.Width + 2 + pSize.Width + 1), Math.Max((int)tSize.Height, (int)pSize.Height));
            }

            if (tmpImageCode != null) {
                return new Size((int)pSize.Width, (int)pSize.Height);
            }

            return new Size(MinSize, MinSize);
        }


        internal static BlueFont GetBlueFont(int _Design, enStates vState, RowItem RowOfStyle, int Stufe) {
            if (_Design > 10000) {
                return Skin.GetBlueFont((PadStyles)_Design, RowOfStyle, Stufe);
            }

            return Skin.GetBlueFont((enDesign)_Design, vState, Stufe);
        }

        internal static BlueFont GetBlueFont(PadStyles vDesign, RowItem RowOfStyle, int Stufe) {
            switch (Stufe) {
                case 4:
                    return GetBlueFont(vDesign, RowOfStyle);

                case 3:
                    switch (vDesign) {
                        case PadStyles.Style_Standard:
                            return GetBlueFont(PadStyles.Style_Überschrift_Kapitel, RowOfStyle);
                        case PadStyles.Style_StandardFett:
                            return GetBlueFont(PadStyles.Style_Überschrift_Kapitel, RowOfStyle);
                            //    Case Else : Return BlueFont(vDesign, vState)
                    }
                    break;

                case 2:
                    switch (vDesign) {
                        case PadStyles.Style_Standard:
                            return GetBlueFont(PadStyles.Style_Überschrift_Untertitel, RowOfStyle);
                        case PadStyles.Style_StandardFett:
                            return GetBlueFont(PadStyles.Style_Überschrift_Untertitel, RowOfStyle);
                            //    Case Else : Return BlueFont(vDesign, vState)
                    }
                    break;

                case 1:
                    switch (vDesign) {
                        case PadStyles.Style_Standard:
                            return GetBlueFont(PadStyles.Style_Überschrift_Haupt, RowOfStyle);
                        case PadStyles.Style_StandardFett:
                            return GetBlueFont(PadStyles.Style_Überschrift_Haupt, RowOfStyle);
                            //  Case Else : Return BlueFont(vDesign, vState)
                    }
                    break;

                case 7:
                    switch (vDesign) {
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


        internal static BlueFont GetBlueFont(enDesign vDesign, enStates vState, int Stufe) {

            if (Stufe != 4 && vDesign != enDesign.TextBox) {
                if (vDesign == enDesign.Form_QuickInfo) { return GetBlueFont(vDesign, vState); } // QuickInfo kann jeden Text enthatlten

                Develop.DebugPrint(enFehlerArt.Warnung, "Design unbekannt: " + (int)vDesign);
                return GetBlueFont(vDesign, vState);
            }


            switch (Stufe) {
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












        public static BlueFont GetBlueFont(PadStyles vFormat, RowItem RowOfStyle) {
            if (StyleDB == null) { InitStyles(); }
            if (StyleDB == null || RowOfStyle == null) { return BlueFont.Get(ErrorFont); }

            return GetBlueFont(StyleDB, ((int)vFormat).ToString(), RowOfStyle);
        }

        public static BlueFont GetBlueFont(Database StyleDB, string ColumnName, RowItem Row) {
            return GetBlueFont(StyleDB, StyleDB.Column[ColumnName], Row);
        }

        public static BlueFont GetBlueFont(Database StyleDB, ColumnItem Column, RowItem Row) {
            string _String = StyleDB.Cell.GetString(Column, Row);
            if (string.IsNullOrEmpty(_String)) {
                Develop.DebugPrint("Schrift nicht definiert: " + StyleDB.Filename + " - " + Column.Name + " - " + Row.CellFirstString());
                return null;
            }
            return BlueFont.Get(_String);
        }



        public static BlueFont GetBlueFont(enDesign vDesign, enStates vState) {
            return GetBlueFont(SkinRow(vDesign, vState));
        }


        public static BlueFont GetBlueFont(RowItem Row) {
            return BlueFont.Get(Value(Row, col_Font, ErrorFont));
        }


        private static void Draw_Back_Verlauf_Vertical_3(Graphics GR, RowItem Row, Rectangle r) {

            ColorBlend cb = new ColorBlend();

            Color c1 = Color.FromArgb(Value(Row, col_Color_Back_1, 0));
            Color c2 = Color.FromArgb(Value(Row, col_Color_Back_2, 0));
            Color c3 = Color.FromArgb(Value(Row, col_Color_Back_3, 0));
            float PR = Value(Row, col_Verlauf_Mitte, 0.5f);
            LinearGradientBrush lgb = new LinearGradientBrush(new Point(r.Left, r.Top), new Point(r.Left, r.Bottom), c1, c3);

            cb.Colors = new[] { c1, c2, c3 };
            cb.Positions = new[] { 0.0F, PR, 1.0F };

            lgb.InterpolationColors = cb;
            lgb.GammaCorrection = true;

            GR.FillRectangle(lgb, r);
        }


        public static string PicCode(RowItem _Row) {
            return Value(_Row, col_StandardPic, "Kreuz|16");
        }


        #region  Styles 

        public static List<string> AllStyles() {
            if (StyleDB == null) { InitStyles(); }

            return StyleDB?.Column[0].Contents(null);
        }


        public static void InitStyles() {
            StyleDB = Database.LoadResource(Assembly.GetAssembly(typeof(Skin)), "Styles.MDB", "Styles", true, false);
        }

        #endregion







    }
}
