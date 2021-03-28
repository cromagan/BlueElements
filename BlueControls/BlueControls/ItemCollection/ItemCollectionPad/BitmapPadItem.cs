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
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using static BlueBasics.FileOperations;

namespace BlueControls.ItemCollection
{
    public class BitmapPadItem : FormPadItemRectangle, ICanHaveColumnVariables
    {


        #region  Variablen-Deklarationen 


        public bool Hintergrund_weiß_füllen { get; set; }
        public int Padding;
        public List<QuickImage> Overlays;
        public enSizeModes Bild_Modus { get; set; }

        [Description("Hier kann ein Platzhalter, der mit dem Code-Generator erzeugt wurde, eingefügt werden.")]
        public string Platzhalter_für_Layout { get; set; }
        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 


        public BitmapPadItem(ItemCollectionPad parent) : this(parent, string.Empty, null, Size.Empty) { }

        public BitmapPadItem(ItemCollectionPad parent, string internalname, string FileToLoad) : this(parent, internalname, (Bitmap)BitmapExt.Image_FromFile(FileToLoad), Size.Empty) { }

        public BitmapPadItem(ItemCollectionPad parent, string internalname, Bitmap bmp) : this(parent, internalname, bmp, Size.Empty) { }

        public BitmapPadItem(ItemCollectionPad parent, Bitmap bmp, Size size) : this(parent, string.Empty, bmp, size) { }

        public BitmapPadItem(ItemCollectionPad parent, Bitmap bmp) : this(parent, string.Empty, bmp, Size.Empty) { }

        public BitmapPadItem(ItemCollectionPad parent, string internalname, Bitmap bmp, Size size) : base(parent, internalname, false)
        {


            Bitmap = bmp;
            SetCoordinates(new RectangleM(0, 0, size.Width, size.Height), true);

            Overlays = new List<QuickImage>();
            Hintergrund_weiß_füllen = true;
            Padding = 0;
            Bild_Modus = enSizeModes.EmptySpace;
            Stil = PadStyles.Undefiniert; // Kein Rahmen
        }


        #endregion


        #region  Properties 
        public Bitmap Bitmap { get; set; }



        public void Bildschirmbereich_wählen()
        {
            if (Bitmap != null)
            {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }

            Bitmap = ScreenShot.GrabArea(null, 2000, 2000).Pic;
        }

        public void Datei_laden()
        {
            if (Bitmap != null)
            {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }

            var e = new System.Windows.Forms.OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false,
                Title = "Bild wählen:",
                Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|BMP Windows Bitmap|*.bmp"
            };

            e.ShowDialog();

            if (!FileExists(e.FileName)) { return; }

            Bitmap = (Bitmap)BitmapExt.Image_FromFile(e.FileName);
        }

        #endregion


        protected override string ClassId()
        {
            return "IMAGE";
        }


        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal shiftX, decimal shiftY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {
            DCoordinates.Inflate(-Padding, -Padding);

            var r1 = new RectangleF(DCoordinates.Left + Padding, DCoordinates.Top + Padding, DCoordinates.Width - Padding * 2, DCoordinates.Height - Padding * 2);
            var r2 = new RectangleF();
            var r3 = new RectangleF();

            if (Bitmap != null)
            {
                r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);


                switch (Bild_Modus)
                {
                    case enSizeModes.Verzerren:
                        {
                            r2 = r1;

                            break;
                        }
                    case enSizeModes.BildAbschneiden:
                        {
                            var scale = (float)Math.Max((DCoordinates.Width - Padding * 2) / (double)Bitmap.Width, (DCoordinates.Height - Padding * 2) / (double)Bitmap.Height);
                            var tmpw = (DCoordinates.Width - Padding * 2) / scale;
                            var tmph = (DCoordinates.Height - Padding * 2) / scale;
                            r3 = new RectangleF((Bitmap.Width - tmpw) / 2, (Bitmap.Height - tmph) / 2, tmpw, tmph);
                            r2 = r1;


                            break;
                        }
                    default: // Is = enSizeModes.WeißerRand
                        {
                            var scale = (float)Math.Min((DCoordinates.Width - Padding * 2) / (double)Bitmap.Width, (DCoordinates.Height - Padding * 2) / (double)Bitmap.Height);
                            r2 = new RectangleF((DCoordinates.Width - Bitmap.Width * scale) / 2 + DCoordinates.Left, (DCoordinates.Height - Bitmap.Height * scale) / 2 + DCoordinates.Top, Bitmap.Width * scale, Bitmap.Height * scale);

                            break;
                        }
                }

            }


            var trp = DCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);

            GR.TranslateTransform(trp.X, trp.Y);
            GR.RotateTransform(-Drehwinkel);


            r1 = new RectangleF(r1.Left - trp.X, r1.Top - trp.Y, r1.Width, r1.Height);
            r2 = new RectangleF(r2.Left - trp.X, r2.Top - trp.Y, r2.Width, r2.Height);


            if (Hintergrund_weiß_füllen)
            {
                GR.FillRectangle(Brushes.White, r1);
            }


            try
            {
                if (Bitmap != null)
                {
                    if (ForPrinting)
                    {
                        GR.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        GR.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    }
                    else
                    {
                        GR.InterpolationMode = InterpolationMode.Low;
                        GR.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    }

                    GR.DrawImage(Bitmap, r2, r3, GraphicsUnit.Pixel);
                }
            }
            catch
            {
                modAllgemein.CollectGarbage();
            }


            if (Stil != PadStyles.Undefiniert)
            {
                if (Parent.SheetStyleScale > 0 && Parent.SheetStyle != null)
                {
                    GR.DrawRectangle(Skin.GetBlueFont(Stil, Parent.SheetStyle).Pen(cZoom * Parent.SheetStyleScale), r1);
                }
            }

            foreach (var thisQI in Overlays)
            {
                GR.DrawImage(thisQI.BMP, r2.Left + 8, r2.Top + 8);
            }


            GR.TranslateTransform(-trp.X, -trp.Y);
            GR.ResetTransform();
            if (!ForPrinting)
            {
                if (!string.IsNullOrEmpty(Platzhalter_für_Layout))
                {
                    var f = new Font("Arial", 8);
                    GR.DrawString(Platzhalter_für_Layout, f, Brushes.Black, DCoordinates.Left, DCoordinates.Top);
                }

            }

            base.DrawExplicit(GR, DCoordinates, cZoom, shiftX, shiftY, vState, SizeOfParentControl, ForPrinting);

        }


        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                case "stretchallowed": // ALT
                    return true;
                case "modus":
                    Bild_Modus = (enSizeModes)int.Parse(value);
                    return true;
                case "whiteback":
                    Hintergrund_weiß_füllen = value.FromPlusMinus();
                    return true;
                case "padding":
                    Padding = int.Parse(value);
                    return true;
                case "image":
                    Bitmap = modConverter.Base64ToBitmap(value);
                    return true;
                case "placeholder":
                    Platzhalter_für_Layout = value.FromNonCritical();
                    return true;
            }
            return false;
        }

        protected override void ParseFinished() { }

        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            t = t + "Modus=" + (int)Bild_Modus + ", ";
            if (!string.IsNullOrEmpty(Platzhalter_für_Layout))
            {
                t = t + "Placeholder=" + Platzhalter_für_Layout.ToNonCritical() + ", ";
            }

            t = t + "WhiteBack=" + Hintergrund_weiß_füllen.ToPlusMinus() + ", ";

            foreach (var thisQI in Overlays)
            {
                t = t + "Overlay=" + thisQI + ", ";
            }

            t = t + "Padding=" + Padding + ", ";

            if (Bitmap != null)
            {
                t = t + "Image=" + modConverter.BitmapToBase64(Bitmap, ImageFormat.Png) + ", ";
            }

            return t.Trim(", ") + "}";
        }



        public bool ReplaceVariable(string VariableName, object Value)
        {

            if (string.IsNullOrEmpty(Platzhalter_für_Layout)) { return false; }

            var ot = Export.ParseVariable(Platzhalter_für_Layout, VariableName, Value);

            if (ot == Platzhalter_für_Layout) { return false; }

            Bitmap = modConverter.StringUnicodeToBitmap(ot);

            OnChanged();

            return true;
        }


        public bool DoSpecialCodes()
        {
            return false;
        }


        public bool ResetVariables()
        {

            if (!string.IsNullOrEmpty(Platzhalter_für_Layout) && Bitmap != null)
            {
                Bitmap.Dispose();
                Bitmap = null;
                OnChanged();
                return true;
            }

            return false;
        }

        public bool RenameColumn(string oldName, ColumnItem cColumnItem)
        {
            var ot = Platzhalter_für_Layout;

            Platzhalter_für_Layout = Platzhalter_für_Layout.Replace("//TS/000" + oldName + "/", "//TS/000" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);
            Platzhalter_für_Layout = Platzhalter_für_Layout.Replace("//TS/001" + oldName + "/", "//TS/001" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);
            Platzhalter_für_Layout = Platzhalter_für_Layout.Replace("//TS/002" + oldName + "/", "//TS/002" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);
            Platzhalter_für_Layout = Platzhalter_für_Layout.Replace("//TS/003" + oldName + "/", "//TS/003" + cColumnItem.Name + "/", RegexOptions.IgnoreCase); // Spaltenname für Bedingungen
            Platzhalter_für_Layout = Platzhalter_für_Layout.Replace("//TS/302" + oldName + "/", "//TS/302" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);
            return ot != Platzhalter_für_Layout;
        }

        public override List<FlexiControl> GetStyleOptions()
        {

            var l = new List<FlexiControl>
            {
                new FlexiControlForProperty(this, "Bildschirmbereich_wählen", enImageCode.Bild),
                new FlexiControlForProperty(this, "Datei_laden", enImageCode.Ordner),
                new FlexiControl(),
                new FlexiControlForProperty(this, "Platzhalter_für_Layout", 2),


                new FlexiControl()
            };

            var Comms = new ItemCollectionList
            {
                { "Abschneiden", ((int)enSizeModes.BildAbschneiden).ToString(), QuickImage.Get("BildmodusAbschneiden|32") },
                { "Verzerren", ((int)enSizeModes.Verzerren).ToString(), QuickImage.Get("BildmodusVerzerren|32") },
                { "Einpassen", ((int)enSizeModes.EmptySpace).ToString(), QuickImage.Get("BildmodusEinpassen|32") }
            };

            l.Add(new FlexiControlForProperty(this, "Bild-Modus", Comms));


            l.Add(new FlexiControl());

            AddLineStyleOption(l);


            l.Add(new FlexiControlForProperty(this, "Hintergrund_weiß_füllen"));

            l.AddRange(base.GetStyleOptions());
            return l;
        }




        //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        //{

        //    base.DoStyleCommands(sender, Tags, ref CloseMenu);

        //    if (Tags.TagGet("Bildschirmbereich wählen").FromPlusMinus())
        //    {
        //        CloseMenu = false;
        //        if (Bitmap != null)
        //        {
        //            if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        //        }

        //        Bitmap = ScreenShot.GrabArea(null, 2000, 2000).Pic;
        //        return;
        //    }

        //    if (Tags.TagGet("Datei laden").FromPlusMinus())
        //    {
        //        CloseMenu = false;
        //        if (Bitmap != null)
        //        {
        //            if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        //        }

        //        var e = new System.Windows.Forms.OpenFileDialog();
        //        e.CheckFileExists = true;
        //        e.Multiselect = false;
        //        e.Title = "Bild wählen:";
        //        e.Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|BMP Windows Bitmap|*.bmp";

        //        e.ShowDialog();

        //        if (!FileExists(e.FileName))
        //        {
        //            return;
        //        }

        //        Bitmap = (Bitmap)modAllgemein.Image_FromFile(e.FileName);
        //        return;
        //    }


        //    if (Tags.TagGet("Skalieren").FromPlusMinus())
        //    {
        //        CloseMenu = false;
        //        var t = InputBox.Show("Skalierfaktor oder Formel eingeben:", "1", enDataFormat.Text);

        //        var sc = modErgebnis.Ergebnis(t);
        //        if (sc == null || sc == 1)
        //        {
        //            Notification.Show("Keine Änderung vorgenommen.");
        //            return;
        //        }

        //        var x = p_RU.X - p_LO.X;
        //        var y = p_RU.Y - p_LO.Y;


        //        p_RU.X = (decimal)((double)p_LO.X + (double)x * sc);
        //        p_RU.Y = (decimal)((double)p_LO.Y + (double)y * sc);

        //        KeepInternalLogic();
        //        return;
        //    }



        //    Hintergrund_weiß_füllen = Tags.TagGet("Hintergrund weiß füllen").FromPlusMinus();
        //    Bild_Modus = (enSizeModes)int.Parse(Tags.TagGet("Bild-Modus"));
        //    Stil = (PadStyles)int.Parse(Tags.TagGet("Umrandung"));
        //    Platzhalter_für_Layout = Tags.TagGet("Platzhalter für Layout").FromNonCritical();

        //}
    }
}