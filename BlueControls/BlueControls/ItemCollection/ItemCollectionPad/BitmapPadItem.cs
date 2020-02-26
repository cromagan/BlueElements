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
using BlueControls.Forms;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
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


        public bool WhiteBack;
        public int Padding;
        public List<QuickImage> Overlays;
        public enSizeModes BildModus;

        private string _PlaceHolderString;
        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 


        public BitmapPadItem(ItemCollectionPad parent) : this(parent, string.Empty, null, Size.Empty) { }

        public BitmapPadItem(ItemCollectionPad parent, string internalname, string FileToLoad) : this(parent, internalname, (Bitmap)modAllgemein.Image_FromFile(FileToLoad), Size.Empty) { }

        public BitmapPadItem(ItemCollectionPad parent, string internalname, Bitmap bmp) : this(parent, internalname, bmp, Size.Empty) { }

        public BitmapPadItem(ItemCollectionPad parent, Bitmap bmp, Size size) : this(parent, string.Empty, bmp, size) { }

        public BitmapPadItem(ItemCollectionPad parent, Bitmap bmp) : this(parent, string.Empty, bmp, Size.Empty) { }

        public BitmapPadItem(ItemCollectionPad parent, string internalname, Bitmap bmp, Size size) : base(parent, internalname)
        {


            Bitmap = bmp;
            SetCoordinates(new RectangleDF(0, 0, size.Width, size.Height));

            Overlays = new List<QuickImage>();
            WhiteBack = true;
            Padding = 0;
            BildModus = enSizeModes.EmptySpace;
            Style = PadStyles.Undefiniert; // Kein Rahmen
        }


        #endregion


        #region  Properties 
        public Bitmap Bitmap { get; set; }

        #endregion

        public override void DesignOrStyleChanged()
        {
            // Keine Variablen zum Reseten, ein Invalidate reicht
        }


        protected override string ClassId()
        {
            return "IMAGE";
        }


        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {
            DCoordinates.Inflate(-Padding, -Padding);

            var r1 = new RectangleF(DCoordinates.Left + Padding, DCoordinates.Top + Padding, DCoordinates.Width - Padding * 2, DCoordinates.Height - Padding * 2);
            var r2 = new RectangleF();
            var r3 = new RectangleF();

            if (Bitmap != null)
            {
                r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);


                switch (BildModus)
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
            GR.RotateTransform(-Rotation);


            r1 = new RectangleF(r1.Left - trp.X, r1.Top - trp.Y, r1.Width, r1.Height);
            r2 = new RectangleF(r2.Left - trp.X, r2.Top - trp.Y, r2.Width, r2.Height);


            if (WhiteBack)
            {
                GR.FillRectangle(Brushes.White, r1);
            }


            try
            {
                if (Bitmap != null)
                {
                    GR.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    GR.DrawImage(Bitmap, r2, r3, GraphicsUnit.Pixel);
                }
            }
            catch
            {
                modAllgemein.CollectGarbage();
            }


            if (Style != PadStyles.Undefiniert)
            {
                if (Parent.SheetStyleScale > 0 && Parent.SheetStyle != null)
                {
                    GR.DrawRectangle(Skin.GetBlueFont(Style, Parent.SheetStyle).Pen(cZoom * Parent.SheetStyleScale), r1);
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
                GR.DrawRectangle(CreativePad.PenGray, DCoordinates);


                if (!string.IsNullOrEmpty(_PlaceHolderString))
                {
                    var f = new Font("Arial", 8);
                    GR.DrawString(_PlaceHolderString, f, Brushes.Black, DCoordinates.Left, DCoordinates.Top);
                }

            }
        }


        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                case "stretchallowed": // ALT
                    return true;
                case "modus":
                    BildModus = (enSizeModes)int.Parse(value);
                    return true;
                case "whiteback":
                    WhiteBack = value.FromPlusMinus();
                    return true;
                case "padding":
                    Padding = int.Parse(value);
                    return true;
                case "image":
                    Bitmap = modConverter.Base64ToBitmap(value);
                    return true;
                case "placeholder":
                    _PlaceHolderString = value.FromNonCritical();
                    return true;
            }

            return false;
        }




        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            t = t + "Modus=" + (int)BildModus + ", ";
            if (!string.IsNullOrEmpty(_PlaceHolderString))
            {
                t = t + "Placeholder=" + _PlaceHolderString.ToNonCritical() + ", ";
            }

            t = t + "WhiteBack=" + WhiteBack.ToPlusMinus() + ", ";

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




        //public object Clone()
        //{
        //    ClearInternalRelations(); // Damit nix geclont wird
        //    var i = (BitmapPadItem)MemberwiseClone();

        //    i.p_LO = new PointDF(i, p_LO);
        //    i.p_LU = new PointDF(i, p_LU);
        //    i.p_RU = new PointDF(i, p_RU);
        //    i.p_RO = new PointDF(i, p_RO);

        //    return i;
        //}






        public bool ParseVariable(string VariableName, enValueType ValueType, string Value)
        {

            if (string.IsNullOrEmpty(_PlaceHolderString))
            {
                return false;
            }

            var ot = Export.ParseVariable(_PlaceHolderString, VariableName, Value, ValueType, enValueType.BinaryImage);

            if (ot == _PlaceHolderString)
            {
                return false;
            }

            Bitmap = modConverter.StringToBitmap(ot);

            return true;
        }


        public bool ParseSpecialCodes()
        {
            return false;
        }


        public bool ResetVariables()
        {

            if (!string.IsNullOrEmpty(_PlaceHolderString) && Bitmap != null)
            {
                Bitmap.Dispose();
                Bitmap = null;
                return true;
            }

            return false;
        }



        public bool RenameColumn(string oldName, ColumnItem cColumnItem)
        {
            var ot = _PlaceHolderString;

            _PlaceHolderString = _PlaceHolderString.Replace("//TS/000" + oldName + "/", "//TS/000" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);
            _PlaceHolderString = _PlaceHolderString.Replace("//TS/001" + oldName + "/", "//TS/001" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);
            _PlaceHolderString = _PlaceHolderString.Replace("//TS/002" + oldName + "/", "//TS/002" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);
            _PlaceHolderString = _PlaceHolderString.Replace("//TS/003" + oldName + "/", "//TS/003" + cColumnItem.Name + "/", RegexOptions.IgnoreCase); // Spaltenname für Bedingungen
            _PlaceHolderString = _PlaceHolderString.Replace("//TS/302" + oldName + "/", "//TS/302" + cColumnItem.Name + "/", RegexOptions.IgnoreCase);
            return ot != _PlaceHolderString;
        }



        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {

            var l = new List<FlexiControl>();

            l.Add(new FlexiControl("Bildschirmbereich wählen", enImageCode.Bild));
            l.Add(new FlexiControl("Datei laden", enImageCode.Ordner));
            l.Add(new FlexiControl(true));
            l.Add(new FlexiControl("Platzhalter für Layout", _PlaceHolderString, enDataFormat.Text, 2));


            l.Add(new FlexiControl(true));

            var Comms = new ItemCollectionList();
            Comms.Add(new TextListItem(((int)enSizeModes.BildAbschneiden).ToString(), "Abschneiden", QuickImage.Get("BildmodusAbschneiden|32")));
            Comms.Add(new TextListItem(((int)enSizeModes.Verzerren).ToString(), "Verzerren", QuickImage.Get("BildmodusVerzerren|32")));
            Comms.Add(new TextListItem(((int)enSizeModes.EmptySpace).ToString(), "Einpassen", QuickImage.Get("BildmodusEinpassen|32")));

            l.Add(new FlexiControl("Bild-Modus", ((int)BildModus).ToString(), Comms));


            l.Add(new FlexiControl(true));


            l.Add(new FlexiControl("Umrandung", ((int)Style).ToString(), Skin.GetRahmenArt(Parent.SheetStyle, true)));


            l.Add(new FlexiControl("Hintergrund weiß füllen", WhiteBack));

            l.AddRange(base.GetStyleOptions(sender, e));

            return l;

        }




        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {

            base.DoStyleCommands(sender, Tags, ref CloseMenu);

            if (Tags.TagGet("Bildschirmbereich wählen").FromPlusMinus())
            {
                CloseMenu = false;
                if (Bitmap != null)
                {
                    if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                }

                Bitmap = ScreenShot.GrabArea(null, 2000, 2000).Pic;
                return;
            }

            if (Tags.TagGet("Datei laden").FromPlusMinus())
            {
                CloseMenu = false;
                if (Bitmap != null)
                {
                    if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                }

                var e = new System.Windows.Forms.OpenFileDialog();
                e.CheckFileExists = true;
                e.Multiselect = false;
                e.Title = "Bild wählen:";
                e.Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|BMP Windows Bitmap|*.bmp";

                e.ShowDialog();

                if (!FileExists(e.FileName))
                {
                    return;
                }

                Bitmap = (Bitmap)modAllgemein.Image_FromFile(e.FileName);
                return;
            }


            if (Tags.TagGet("Skalieren").FromPlusMinus())
            {
                CloseMenu = false;
                var t = InputBox.Show("Skalierfaktor oder Formel eingeben:", "1", enDataFormat.Text);

                var sc = modErgebnis.Ergebnis(t);
                if (sc == null || sc == 1)
                {
                    Notification.Show("Keine Änderung vorgenommen.");
                    return;
                }

                var x = p_RU.X - p_LO.X;
                var y = p_RU.Y - p_LO.Y;


                p_RU.X = (decimal)((double)p_LO.X + (double)x * sc);
                p_RU.Y = (decimal)((double)p_LO.Y + (double)y * sc);

                KeepInternalLogic();
                return;
            }



            WhiteBack = Tags.TagGet("Hintergrund weiß füllen").FromPlusMinus();
            BildModus = (enSizeModes)int.Parse(Tags.TagGet("Bild-Modus"));
            Style = (PadStyles)int.Parse(Tags.TagGet("Umrandung"));
            _PlaceHolderString = Tags.TagGet("Platzhalter für Layout").FromNonCritical();

        }
    }
}