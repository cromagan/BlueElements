// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BlueScript.Variables;
using static BlueBasics.FileOperations;
using MessageBox = BlueControls.Forms.MessageBox;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection {

    public sealed class BitmapPadItem : RectanglePadItem, ICanHaveColumnVariables, IDisposable {

        #region Fields

        public List<QuickImage> Overlays;
        public int Padding;
        private bool _disposedValue;

        #endregion

        #region Constructors

        public BitmapPadItem(string internalname) : this(internalname, null, Size.Empty) { }

        public BitmapPadItem(string internalname, string fileToLoad) : this(internalname, (Bitmap)BitmapExt.Image_FromFile(fileToLoad), Size.Empty) { }

        public BitmapPadItem(string internalname, Bitmap? bmp) : this(internalname, bmp, Size.Empty) { }

        public BitmapPadItem(Bitmap? bmp, Size size) : this(string.Empty, bmp, size) { }

        public BitmapPadItem(Bitmap? bmp) : this(string.Empty, bmp, Size.Empty) { }

        public BitmapPadItem(string internalname, Bitmap? bmp, Size size) : base(internalname) {
            Bitmap = bmp;
            SetCoordinates(new RectangleF(0, 0, size.Width, size.Height), true);
            Overlays = new List<QuickImage>();
            Hintergrund_Weiß_Füllen = true;
            Padding = 0;
            Bild_Modus = SizeModes.EmptySpace;
            Stil = PadStyles.Undefiniert; // Kein Rahmen
        }

        #endregion

        #region Properties

        public SizeModes Bild_Modus { get; set; }

        public string Bildschirmbereich_wählen {
            get => string.Empty;
            set {
                if (Bitmap != null) {
                    if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                }
                Bitmap = ScreenShot.GrabArea(null);
            }
        }

        public Bitmap? Bitmap { get; set; }

        public string Datei_laden {
            get => string.Empty;
            set {
                if (Bitmap != null) {
                    if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                }
                System.Windows.Forms.OpenFileDialog e = new() {
                    CheckFileExists = true,
                    Multiselect = false,
                    Title = "Bild wählen:",
                    Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|Bmp Windows Bitmap|*.bmp"
                };
                e.ShowDialog();

                if (!FileExists(e.FileName)) { return; }
                Bitmap = (Bitmap)BitmapExt.Image_FromFile(e.FileName);
            }
        }

        public bool Hintergrund_Weiß_Füllen { get; set; }

        [Description("Hier kann ein Variablenname als Platzhalter eingegeben werden. Beispiel: ~Bild~")]
        public string Platzhalter_Für_Layout { get; set; }

        protected override int SaveOrder => 999;

        #endregion

        #region Methods

        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(true);
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        public override List<GenericControl> GetStyleOptions() {
            List<GenericControl> l = new()
            {
                new FlexiControlForProperty<string>(() => Bildschirmbereich_wählen, ImageCode.Bild),
                new FlexiControlForProperty<string>(() => Datei_laden, ImageCode.Ordner),
                new FlexiControl(),
                new FlexiControlForProperty<string>(() => Platzhalter_Für_Layout, 2),
                new FlexiControl()
            };
            ItemCollectionList.ItemCollectionList comms = new()
            {
                { "Abschneiden", ((int)SizeModes.BildAbschneiden).ToString(), QuickImage.Get("BildmodusAbschneiden|32") },
                { "Verzerren", ((int)SizeModes.Verzerren).ToString(), QuickImage.Get("BildmodusVerzerren|32") },
                { "Einpassen", ((int)SizeModes.EmptySpace).ToString(), QuickImage.Get("BildmodusEinpassen|32") }
            };
            l.Add(new FlexiControlForProperty<SizeModes>(() => Bild_Modus, comms));
            l.Add(new FlexiControl());
            AddLineStyleOption(l);
            l.Add(new FlexiControlForProperty<bool>(() => Hintergrund_Weiß_Füllen));
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "stretchallowed": // ALT
                    return true;

                case "modus":
                    Bild_Modus = (SizeModes)IntParse(value);
                    return true;

                case "whiteback":
                    Hintergrund_Weiß_Füllen = value.FromPlusMinus();
                    return true;

                case "padding":
                    Padding = IntParse(value);
                    return true;

                case "image":
                    Bitmap = Base64ToBitmap(value);
                    return true;

                case "placeholder":
                    Platzhalter_Für_Layout = value.FromNonCritical();
                    return true;
            }
            return false;
        }

        public bool ReplaceVariable(Variable variable) {
            if (string.IsNullOrEmpty(Platzhalter_Für_Layout)) { return false; }
            if ("~" + variable.Name.ToLower() + "~" != Platzhalter_Für_Layout.ToLower()) { return false; }
            if (variable is not VariableBitmap vbmp) { return false; }
            var ot = vbmp.ValueBitmap;
            if (ot is Bitmap bmp) {
                Bitmap = bmp;
                OnChanged();
                return true;
            }

            return false;
        }

        public bool ResetVariables() {
            if (!string.IsNullOrEmpty(Platzhalter_Für_Layout) && Bitmap != null) {
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
            if (!string.IsNullOrEmpty(Platzhalter_Für_Layout)) {
                t = t + "Placeholder=" + Platzhalter_Für_Layout.ToNonCritical() + ", ";
            }
            t = t + "WhiteBack=" + Hintergrund_Weiß_Füllen.ToPlusMinus() + ", ";
            foreach (var thisQi in Overlays) {
                t = t + "Overlay=" + thisQi + ", ";
            }
            t = t + "Padding=" + Padding + ", ";
            if (Bitmap != null) {
                t = t + "Image=" + BitmapToBase64(Bitmap, ImageFormat.Png) + ", ";
            }
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "IMAGE";

        protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
            positionModified.Inflate(-Padding, -Padding);
            RectangleF r1 = new(positionModified.Left + Padding, positionModified.Top + Padding,
                positionModified.Width - (Padding * 2), positionModified.Height - (Padding * 2));
            RectangleF r2 = new();
            RectangleF r3 = new();
            if (Bitmap != null) {
                r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
                switch (Bild_Modus) {
                    case SizeModes.Verzerren: {
                            r2 = r1;
                            break;
                        }

                    case SizeModes.BildAbschneiden: {
                            var scale = Math.Max((positionModified.Width - (Padding * 2)) / Bitmap.Width, (positionModified.Height - (Padding * 2)) / Bitmap.Height);
                            var tmpw = (positionModified.Width - (Padding * 2)) / scale;
                            var tmph = (positionModified.Height - (Padding * 2)) / scale;
                            r3 = new RectangleF((Bitmap.Width - tmpw) / 2, (Bitmap.Height - tmph) / 2, tmpw, tmph);
                            r2 = r1;
                            break;
                        }
                    default: // Is = enSizeModes.WeißerRand
                        {
                            var scale = Math.Min((positionModified.Width - (Padding * 2)) / Bitmap.Width, (positionModified.Height - (Padding * 2)) / Bitmap.Height);
                            r2 = new RectangleF(((positionModified.Width - (Bitmap.Width * scale)) / 2) + positionModified.Left, ((positionModified.Height - (Bitmap.Height * scale)) / 2) + positionModified.Top, Bitmap.Width * scale, Bitmap.Height * scale);
                            break;
                        }
                }
            }
            var trp = positionModified.PointOf(Alignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);
            r1 = new RectangleF(r1.Left - trp.X, r1.Top - trp.Y, r1.Width, r1.Height);
            r2 = new RectangleF(r2.Left - trp.X, r2.Top - trp.Y, r2.Width, r2.Height);
            if (Hintergrund_Weiß_Füllen) {
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
            foreach (var thisQi in Overlays) {
                gr.DrawImage(thisQi, r2.Left + 8, r2.Top + 8);
            }
            gr.TranslateTransform(-trp.X, -trp.Y);
            gr.ResetTransform();
            if (!forPrinting) {
                if (!string.IsNullOrEmpty(Platzhalter_Für_Layout)) {
                    Font f = new("Arial", 8);
                    BlueFont.DrawString(gr, Platzhalter_Für_Layout, f, Brushes.Black, positionModified.Left, positionModified.Top);
                }
            }
            base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        }

        protected override BasicPadItem? TryCreate(string id, string name) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                return new BitmapPadItem(name);
            }
            return null;
        }

        private void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                if (Bitmap != null) {
                    Bitmap.Dispose();
                    Bitmap = null;
                }

                _disposedValue = true;
            }
        }

        #endregion

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~BitmapPadItem()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }
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
        //        e.Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|Bmp Windows Bitmap|*.bmp";
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
        //        p_RU.X = (float)((float)p_LO.X + (float)x * sc);
        //        p_RU.Y = (float)((float)p_LO.Y + (float)y * sc);
        //        KeepInternalLogic();
        //        return;
        //    }
        //    Hintergrund_weiß_füllen = Tags.TagGet("Hintergrund weiß füllen").FromPlusMinus();
        //    Bild_Modus = (enSizeModes)IntParse(Tags.TagGet("Bild-Modus"));
        //    Stil = (PadStyles)IntParse(Tags.TagGet("Umrandung"));
        //    Platzhalter_für_Layout = Tags.TagGet("Platzhalter für Layout").FromNonCritical();
        //}
    }
}