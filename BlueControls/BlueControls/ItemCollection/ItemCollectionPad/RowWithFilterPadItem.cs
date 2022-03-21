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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics.Interfaces;

namespace BlueControls.ItemCollection {

    public class RowWithFilterPaditem : FixedRectanglePadItem, IReadableText {

        #region Fields

        public static BlueFont? CellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard);

        public static BlueFont? ChapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard);

        public static BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);

        /// <summary>
        /// Laufende Nummer, bestimmt die einfärbung
        /// </summary>
        public readonly int Id;

        public FilterCollection? Filter;
        private readonly Database? _database;

        #endregion

        #region Constructors

        public RowWithFilterPaditem(Database? db, int id) : this(UniqueInternal(), db, id) { }

        public RowWithFilterPaditem(string intern, Database? db, int id) : base(intern) {
            _database = db;
            if (db != null) { Filter = new FilterCollection(db); }
            Id = id;
            Size = new Size(300, 100);
        }

        public RowWithFilterPaditem(string intern) : this(intern, null, 0) { }

        #endregion

        #region Properties

        public string Datenbankkopf {
            get => string.Empty;
            set {
                if (_database == null) { return; }
                Forms.TableView.OpenDatabaseHeadEditor(_database);
            }
        }

        #endregion

        #region Methods

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new() { };
            if (_database == null) { return l; }
            l.Add(new FlexiControlForProperty<string>(() => _database.Caption));
            //l.Add(new FlexiControlForProperty(Database, "Caption"));
            l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));
            //l.Add(new FlexiControlForProperty(()=> this.Datenbankkopf"));
            l.Add(new FlexiControl());

            //l.Add(new FlexiControl());
            //l.Add(new FlexiControlForProperty<string>(() => Column.Ueberschrift1"));
            //l.Add(new FlexiControlForProperty<string>(() => Column.Ueberschrift2"));
            //l.Add(new FlexiControlForProperty<string>(() => Column.Ueberschrift3"));
            //l.Add(new FlexiControl());
            //l.Add(new FlexiControlForProperty(Database.t, "Quickinfo"));
            //l.Add(new FlexiControlForProperty<string>(() => Column.AdminInfo"));

            //if (AdditionalStyleOptions != null) {
            //    l.Add(new FlexiControl());
            //    l.AddRange(AdditionalStyleOptions);
            //}

            return l;
        }

        public string ReadableText() {
            if (_database != null) { return "Zeile aus: " + _database.Caption; }

            return "Zeile einer Datenbank";
        }

        public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IDColor(Id));

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (_database != null) {
                t = t + "Database=" + _database.Filename.ToNonCritical() + ", ";
            }
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "RowWithFilter";

        protected override void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, float zoom, float shiftX, float shiftY, bool forPrinting) {
            DrawColorScheme(gr, drawingCoordinates, zoom, Id);

            if (_database != null) {
                Skin.Draw_FormatedText(gr, _database.Caption, QuickImage.Get(ImageCode.Datenbank, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, drawingCoordinates.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
            }
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
            base.DrawExplicit(gr, drawingCoordinates, zoom, shiftX, shiftY, forPrinting);
        }

        protected override BasicPadItem? TryParse(string id, string name, List<KeyValuePair<string, string>> toParse) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                var x = new RowWithFilterPaditem(name);
                x.Parse(toParse);
                return x;
            }
            return null;
        }

        #endregion
    }
}