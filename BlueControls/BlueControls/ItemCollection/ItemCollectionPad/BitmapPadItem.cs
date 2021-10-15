// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueScript;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using static BlueBasics.FileOperations;

namespace BlueControls.ItemCollection {

    public class BitmapPadItem : FormPadItemRectangle, ICanHaveColumnVariables {

        #region Fields

        public List<QuickImage> Overlays;
        public int Padding;

        #endregion

        #region Constructors

        public BitmapPadItem(ItemCollectionPad parent) : this(parent, string.Empty, null, Size.Empty) {
        }

        public BitmapPadItem(ItemCollectionPad parent, string internalname, string FileToLoad) : this(parent, internalname, (Bitmap)BitmapExt.Image_FromFile(FileToLoad), Size.Empty) {
        }

        public BitmapPadItem(ItemCollectionPad parent, string internalname, Bitmap bmp) : this(parent, internalname, bmp, Size.Empty) {
        }

        public BitmapPadItem(ItemCollectionPad parent, Bitmap bmp, Size size) : this(parent, string.Empty, bmp, size) {
        }

        public BitmapPadItem(ItemCollectionPad parent, Bitmap bmp) : this(parent, string.Empty, bmp, Size.Empty) {
        }

        public BitmapPadItem(ItemCollectionPad parent, string internalname, Bitmap bmp, Size size) : base(parent, internalname) {
            Bitmap = bmp;
            SetCoordinates(new RectangleM(0, 0, size.Width, size.Height), true);
            Overlays = new List<QuickImage>();
            Hintergrund_weiß_füllen = true;
            Padding = 0;
            Bild_Modus = enSizeModes.EmptySpace;
            Stil = PadStyles.Undefiniert; // Kein Rahmen
        }

        #endregion

        #region Properties

        public enSizeModes Bild_Modus { get; set; }
        public Bitmap Bitmap { get; set; }
        public bool Hintergrund_weiß_füllen { get; set; }

        [Description("Hier kann ein Platzhalter, der mit dem Code-Generator erzeugt wurde, eingefügt werden.")]
        public string Platzhalter_für_Layout { get; set; }

        #endregion

        #region Methods

        public void Bildschirmbereich_wählen() {
            if (Bitmap != null) {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }
            Bitmap = ScreenShot.GrabArea(null, 2000, 2000).Pic;
        }

        public void Datei_laden() {
            if (Bitmap != null) {
                if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
            }
            System.Windows.Forms.OpenFileDialog e = new() {
                CheckFileExists = true,
                Multiselect = false,
                Title = "Bild wählen:",
                Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|BMP Windows Bitmap|*.bmp"
            };
            e.ShowDialog();
            if (!FileExists(e.FileName)) { return; }
            Bitmap = (Bitmap)BitmapExt.Image_FromFile(e.FileName);
        }

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new()
            {
                new FlexiControlForProperty(this, "Bildschirmbereich_wählen", enImageCode.Bild),
                new FlexiControlForProperty(this, "Datei_laden", enImageCode.Ordner),
                new FlexiControl(),
                new FlexiControlForProperty(this, "Platzhalter_für_Layout", 2),
                new FlexiControl()
            };
            ItemCollectionList Comms = new()
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

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
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
                    Bitmap = Converter.Base64ToBitmap(value);
                    return true;

                case "placeholder":
                    Platzhalter_für_Layout = value.FromNonCritical();
                    return true;
            }
            return false;
        }

        public bool ReplaceVariable(Script s, Variable variable) {
            if (string.IsNullOrEmpty(Platzhalter_für_Layout)) { return false; }
            if ("~" + variable.Name.ToLower() + "~" != Platzhalter_für_Layout.ToLower()) { return false; }
            if (variable.Type != Skript.Enums.enVariableDataType.Bitmap) { return false; }
            var ot = variable.GetValueBitmap(s);
            if (ot is Bitmap bmp) {
                Bitmap = bmp;
                OnChanged();
                return true;
            } else {
                return false;
            }
        }

        public bool ResetVariables() {
            if (!string.IsNullOrEmpty(Platzhalter_für_Layout) && Bitmap != null) {
                Bitmap.Dispose();
                Bitmap = null;
                OnChanged();
                return true;
            }
            return false;
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            t = t + "Modus=" + (int)Bild_Modus + ", ";
            if (!string.IsNullOrEmpty(Platzhalter_für_Layout)) {
                t = t + "Placeholder=" + Platzhalter_für_Layout.ToNonCritical() + ", ";
            }
            t = t + "WhiteBack=" + Hintergrund_weiß_füllen.ToPlusMinus() + ", ";
            foreach (var thisQI in Overlays) {
                t = t + "Overlay=" + thisQI + ", ";
            }
            t = t + "Padding=" + Padding + ", ";
            if (Bitmap != null) {
                t = t + "Image=" + Converter.BitmapToBase64(Bitmap, ImageFormat.Png) + ", ";
            }
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "IMAGE";

        protected override void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, double zoom, double shiftX, double shiftY, enStates state, Size sizeOfParentControl, bool forPrinting) {
            drawingCoordinates.Inflate(-Padding, -Padding);
            RectangleF r1 = new(drawingCoordinates.Left + Padding, drawingCoordinates.Top + Padding, drawingCoordinates.Width - (Padding * 2), drawingCoordinates.Height - (Padding * 2));
            RectangleF r2 = new();
            RectangleF r3 = new();
            if (Bitmap != null) {
                r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
                switch (Bild_Modus) {
                    case enSizeModes.Verzerren: {
                            r2 = r1;
                            break;
                        }

                    case enSizeModes.BildAbschneiden: {
                            var scale = (float)Math.Max((drawingCoordinates.Width - (Padding * 2)) / (double)Bitmap.Width, (drawingCoordinates.Height - (Padding * 2)) / (double)Bitmap.Height);
                            var tmpw = (drawingCoordinates.Width - (Padding * 2)) / scale;
                            var tmph = (drawingCoordinates.Height - (Padding * 2)) / scale;
                            r3 = new RectangleF((Bitmap.Width - tmpw) / 2, (Bitmap.Height - tmph) / 2, tmpw, tmph);
                            r2 = r1;
                            break;
                        }
                    default: // Is = enSizeModes.WeißerRand
                        {
                            var scale = (float)Math.Min((drawingCoordinates.Width - (Padding * 2)) / (double)Bitmap.Width, (drawingCoordinates.Height - (Padding * 2)) / (double)Bitmap.Height);
                            r2 = new RectangleF(((drawingCoordinates.Width - (Bitmap.Width * scale)) / 2) + drawingCoordinates.Left, ((drawingCoordinates.Height - (Bitmap.Height * scale)) / 2) + drawingCoordinates.Top, Bitmap.Width * scale, Bitmap.Height * scale);
                            break;
                        }
                }
            }
            var trp = drawingCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);
            r1 = new RectangleF(r1.Left - trp.X, r1.Top - trp.Y, r1.Width, r1.Height);
            r2 = new RectangleF(r2.Left - trp.X, r2.Top - trp.Y, r2.Width, r2.Height);
            if (Hintergrund_weiß_füllen) {
                gr.FillRectangle(Brushes.White, r1);
            }
            try {
                if (Bitmap != null) {
                    if (forPrinting) {
                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    } else {
                        gr.InterpolationMode = InterpolationMode.Low;
                        gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    }
                    gr.DrawImage(Bitmap, r2, r3, GraphicsUnit.Pixel);
                }
            } catch {
                Generic.CollectGarbage();
            }
            if (Stil != PadStyles.Undefiniert) {
                if (Parent.SheetStyleScale > 0 && Parent.SheetStyle != null) {
                    gr.DrawRectangle(Skin.GetBlueFont(Stil, Parent.SheetStyle).Pen(zoom * Parent.SheetStyleScale), r1);
                }
            }
            foreach (var thisQI in Overlays) {
                gr.DrawImage(thisQI.BMP, r2.Left + 8, r2.Top + 8);
            }
            gr.TranslateTransform(-trp.X, -trp.Y);
            gr.ResetTransform();
            if (!forPrinting) {
                if (!string.IsNullOrEmpty(Platzhalter_für_Layout)) {
                    Font f = new("Arial", 8);
                    BlueFont.DrawString(gr, Platzhalter_für_Layout, f, Brushes.Black, drawingCoordinates.Left, drawingCoordinates.Top);
                }
            }
            base.DrawExplicit(gr, drawingCoordinates, zoom, shiftX, shiftY, state, sizeOfParentControl, forPrinting);
        }

        protected override void ParseFinished() {
        }

        #endregion

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
        //        p_RU.X = (double)((double)p_LO.X + (double)x * sc);
        //        p_RU.Y = (double)((double)p_LO.Y + (double)y * sc);
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