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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using MessageBox = BlueControls.Forms.MessageBox;
using static BlueBasics.Converter;
using BlueDatabase;
using BlueDatabase.Enums;

using BlueBasics.Interfaces;
using BlueControls.Interfaces;
using BlueControls.ConnectedFormula;

namespace BlueControls.ItemCollection {

    public class FileExplorerPadItem : VariableShowPadItem, IItemToControl {

        #region Fields

        private string _pfad = string.Empty;

        #endregion

        #region Constructors

        public FileExplorerPadItem(string internalname) : base(internalname) {
            SetCoordinates(new RectangleF(0, 0, 50, 30), true);
        }

        #endregion

        #region Properties

        [Description("Der Dateipfad, dessen Dateien angezeigt werden sollen.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
        public string Pfad {
            get => _pfad;

            set {
                if (value == _pfad) { return; }
                _pfad = value;
                OnChanged();
            }
        }

        protected override int SaveOrder => 4;

        #endregion

        #region Methods

        public override System.Windows.Forms.Control? CreateControl(ConnectedFormulaView parent) {
            if (GetRowFrom is ICalculateRowsItemLevel rfw2) {
                var ff = parent.SearchOrGenerate((BasicPadItem)rfw2);

                if (rfw2.Genau_eine_Zeile) {
                    var cx = new FileBrowser();
                    //cx.ColumnKey = Column.Key;
                    //cx.EditType = EditType;
                    //cx.CaptionPosition = CaptionPosition;
                    cx.Tag = Internal;
                    if (ff is ICalculateRowsControlLevel cc) { cc.Childs.Add(cx); }
                    return cx;
                }
            }

            return null;
        }

        //private string _quickinfo;
        //private string _adminInfo;
        //public EditFieldPadItem(string internalname, string fileToLoad) : this(internalname, (Bitmap)BitmapExt.Image_FromFile(fileToLoad), Size.Empty) { }

        //public EditFieldPadItem(string internalname, Bitmap? bmp) : this(internalname, bmp, Size.Empty) { }

        //public EditFieldPadItem(Bitmap? bmp, Size size) : this(string.Empty, bmp, size) { }

        //public EditFieldPadItem(Bitmap? bmp) : this(string.Empty, bmp, Size.Empty) { }
        public override List<GenericControl> GetStyleOptions() {
            List<GenericControl> l = new();
            l.AddRange(base.GetStyleOptions());
            l.Add(new FlexiControlForProperty<string>(() => Pfad));
            return l;
        }

        public bool IsRecursiveWith(IAcceptAndSends obj) {
            if (obj == this) { return true; }

            if (GetRowFrom is IAcceptAndSends i) { return i.IsRecursiveWith(obj); }
            return false;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "pfad":
                    _pfad = value.FromNonCritical();
                    return true;
            }
            return false;
        }

        //public string ReadableText() {
        //    if (Column != null) {
        //        return "Wert aus: " + Column.ReadableText();

        //        //if (Genau_eine_Zeile) {
        //        //    return "(eine) Zeile aus: " + Database.Caption;
        //        //} else {
        //        //    return "Zeilen aus: " + Database.Caption;
        //        //}
        //    }

        //    return "Wert einer Spalte";
        //}

        //public QuickImage? SymbolForReadableText() {
        //    if (GetRowFrom == null) { return null; }

        //    return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IDColor(GetRowFrom.Id));
        //}

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            t = t + "Pfad=" + _pfad.ToNonCritical() + ", ";

            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "FI-FileExplorer";

        //    return false;
        //}
        protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
            var id = -1; if (GetRowFrom != null) { id = GetRowFrom.Id; }

            if (!forPrinting) {
                DrawColorScheme(gr, positionModified, zoom, id);
            }

            //if (GetRowFrom == null) {
            //    Skin.Draw_FormatedText(gr, "Datenquelle fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFNT.Scale(zoom), true);
            //} else if (Column == null) {
            //    Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFNT.Scale(zoom), true);
            //} else {
            //    Point cap;
            //    var uc = positionModified.ToRect();

            //    switch (CaptionPosition) {
            //        case ÜberschriftAnordnung.ohne:
            //            cap = new Point(-1, -1);
            //            //uc = positionModified.ToRect();
            //            break;

            //        case ÜberschriftAnordnung.Links_neben_Dem_Feld:
            //            cap = new Point(0, 0);
            //            uc.X += (int)(100 * zoom);
            //            uc.Width -= (int)(100 * zoom);
            //            break;

            //        case ÜberschriftAnordnung.Ohne_mit_Abstand:
            //            cap = new Point(-1, -1);
            //            uc.Y += (int)(19 * zoom);
            //            uc.Height -= (int)(19 * zoom);
            //            break;

            //        case ÜberschriftAnordnung.Über_dem_Feld:
            //        default:
            //            cap = new Point(0, 0);
            //            uc.Y += (int)(19 * zoom);
            //            uc.Height -= (int)(19 * zoom);
            //            break;
            //    }

            //    if (cap.X >= 0) {
            //        var e = new RectangleF(positionModified.Left + cap.X * zoom, positionModified.Top + cap.Y * zoom, positionModified.Width, 16 * zoom);
            //        Skin.Draw_FormatedText(gr, Column.ReadableText() + ":", null, Alignment.Top_Left, e.ToRect(), CaptionFNT.Scale(zoom), true);
            //    }

            //    if (uc.Width > 0 && uc.Height > 0) {
            //        gr.DrawRectangle(new Pen(Color.Black, zoom), uc);
            //    }
            //}

            //drawingCoordinates.Inflate(-Padding, -Padding);
            //RectangleF r1 = new(drawingCoordinates.Left + Padding, drawingCoordinates.Top + Padding,
            //    drawingCoordinates.Width - (Padding * 2), drawingCoordinates.Height - (Padding * 2));
            //RectangleF r2 = new();
            //RectangleF r3 = new();
            //if (Bitmap != null) {
            //    r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
            //    switch (Bild_Modus) {
            //        case enSizeModes.Verzerren: {
            //                r2 = r1;
            //                break;
            //            }

            //        case enSizeModes.BildAbschneiden: {
            //                var scale = Math.Max((drawingCoordinates.Width - (Padding * 2)) / Bitmap.Width, (drawingCoordinates.Height - (Padding * 2)) / Bitmap.Height);
            //                var tmpw = (drawingCoordinates.Width - (Padding * 2)) / scale;
            //                var tmph = (drawingCoordinates.Height - (Padding * 2)) / scale;
            //                r3 = new RectangleF((Bitmap.Width - tmpw) / 2, (Bitmap.Height - tmph) / 2, tmpw, tmph);
            //                r2 = r1;
            //                break;
            //            }
            //        default: // Is = enSizeModes.WeißerRand
            //            {
            //                var scale = Math.Min((drawingCoordinates.Width - (Padding * 2)) / Bitmap.Width, (drawingCoordinates.Height - (Padding * 2)) / Bitmap.Height);
            //                r2 = new RectangleF(((drawingCoordinates.Width - (Bitmap.Width * scale)) / 2) + drawingCoordinates.Left, ((drawingCoordinates.Height - (Bitmap.Height * scale)) / 2) + drawingCoordinates.Top, Bitmap.Width * scale, Bitmap.Height * scale);
            //                break;
            //            }
            //    }
            //}
            //var trp = drawingCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
            //gr.TranslateTransform(trp.X, trp.Y);
            //gr.RotateTransform(-Drehwinkel);
            //r1 = new RectangleF(r1.Left - trp.X, r1.Top - trp.Y, r1.Width, r1.Height);
            //r2 = new RectangleF(r2.Left - trp.X, r2.Top - trp.Y, r2.Width, r2.Height);
            //if (Hintergrund_Weiß_Füllen) {
            //    gr.FillRectangle(Brushes.White, r1);
            //}
            //try {
            //    if (Bitmap != null) {
            //        if (forPrinting) {
            //            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //        } else {
            //            gr.InterpolationMode = InterpolationMode.Low;
            //            gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            //        }
            //        gr.DrawImage(Bitmap, r2, r3, GraphicsUnit.Pixel);
            //    }
            //} catch {
            //    Generic.CollectGarbage();
            //}
            //if (Stil != PadStyles.Undefiniert) {
            //    if (Parent.SheetStyleScale > 0 && Parent.SheetStyle != null) {
            //        gr.DrawRectangle(Skin.GetBlueFont(Stil, Parent.SheetStyle).Pen(zoom * Parent.SheetStyleScale), r1);
            //    }
            //}
            //foreach (var thisQi in Overlays) {
            //    gr.DrawImage(thisQi, r2.Left + 8, r2.Top + 8);
            //}
            //gr.TranslateTransform(-trp.X, -trp.Y);
            //gr.ResetTransform();
            //if (!forPrinting) {
            //    if (!string.IsNullOrEmpty(Platzhalter_Für_Layout)) {
            //        Font f = new("Arial", 8);
            //        BlueFont.DrawString(gr, Platzhalter_Für_Layout, f, Brushes.Black, drawingCoordinates.Left, drawingCoordinates.Top);
            //    }
            //}
            base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        }

        //public bool ReplaceVariable(Variable variable) {
        //    if (string.IsNullOrEmpty(Platzhalter_Für_Layout)) { return false; }
        //    if ("~" + variable.Name.ToLower() + "~" != Platzhalter_Für_Layout.ToLower()) { return false; }
        //    if (variable is not VariableBitmap vbmp) { return false; }
        //    var ot = vbmp.ValueBitmap;
        //    if (ot is Bitmap bmp) {
        //        Bitmap = bmp;
        //        OnChanged();
        //        return true;
        //    }
        protected override BasicPadItem? TryCreate(string id, string name) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                return new EditFieldPadItem(name);
            }
            return null;
        }

        #endregion
    }
}