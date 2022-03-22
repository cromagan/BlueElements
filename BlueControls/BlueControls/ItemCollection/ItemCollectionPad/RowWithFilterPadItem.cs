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
using System.ComponentModel;

namespace BlueControls.ItemCollection {

    public class RowWithFilterPaditem : FixedRectanglePadItem, IReadableText {

        #region Fields

        public static BlueFont? CellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard);

        public static BlueFont? ChapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard);

        public static BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);

        public readonly Database? Database;

        /// <summary>
        /// Laufende Nummer, bestimmt die einfärbung
        /// </summary>
        public readonly int Id;

        public FilterCollection? Filter;

        private bool _genau_eine_Zeile = true;

        #endregion

        #region Constructors

        public RowWithFilterPaditem(Database? db, int id) : this(UniqueInternal(), db, id) { }

        public RowWithFilterPaditem(string intern, Database? db, int id) : base(intern) {
            Database = db;
            if (db != null) { Filter = new FilterCollection(db); }
            Id = id;
            Size = new Size(200, 50);
        }

        public RowWithFilterPaditem(string intern) : this(intern, null, 0) { }

        #endregion

        #region Properties

        public string Datenbankkopf {
            get => string.Empty;
            set {
                if (Database == null) { return; }
                Forms.TableView.OpenDatabaseHeadEditor(Database);
            }
        }

        [Description("Nur wenn das Filterergebis genau eine Zeile ergeben wird(und muss),\r\nkönnen die abhängige Zellen bearbeitet werden.\r\nAndernfalls werden abhängige Felder Auswahlfelder.")]
        public bool Genau_eine_Zeile {
            get => _genau_eine_Zeile;
            set {
                if (value == _genau_eine_Zeile) { return; }
                _genau_eine_Zeile = value;
                OnChanged();
            }
        }

        #endregion

        #region Methods

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new() { };
            if (Database == null) { return l; }
            l.Add(new FlexiControlForProperty<string>(() => Database.Caption));
            //l.Add(new FlexiControlForProperty(Database, "Caption"));
            l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));
            //l.Add(new FlexiControlForProperty(()=> this.Datenbankkopf"));
            l.Add(new FlexiControl());
            l.Add(new FlexiControlForProperty<bool>(() => Genau_eine_Zeile));

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
            if (Database != null) {
                if (Genau_eine_Zeile) {
                    return "(eine) Zeile aus: " + Database.Caption;
                } else {
                    return "Zeilen aus: " + Database.Caption;
                }
            }

            return "Zeile einer Datenbank";
        }

        public QuickImage? SymbolForReadableText() {
            if (Genau_eine_Zeile) {
                return QuickImage.Get(ImageCode.Kreis, 10, Color.Transparent, Skin.IDColor(Id));
            } else {
                return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IDColor(Id));
            }
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (Database != null) {
                t = t + "Database=" + Database.Filename.ToNonCritical() + ", ";
            }
            t = t + "OneRow=" + Genau_eine_Zeile.ToPlusMinus() + ", ";
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "RowWithFilter";

        protected override void DrawExplicit(Graphics gr, RectangleF modifiedPosition, float zoom, float shiftX, float shiftY, bool forPrinting) {
            DrawColorScheme(gr, modifiedPosition, zoom, Id);

            if (Database != null) {
                var txt = string.Empty;
                if (!Genau_eine_Zeile) { txt = "Mehrere Zeilen\r\n"; }

                Skin.Draw_FormatedText(gr, txt + Database.Caption, QuickImage.Get(ImageCode.Zeile, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, modifiedPosition.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
            }

            gr.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), modifiedPosition);
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
            base.DrawExplicit(gr, modifiedPosition, zoom, shiftX, shiftY, forPrinting);
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