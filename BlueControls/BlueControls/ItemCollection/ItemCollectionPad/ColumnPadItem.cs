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

namespace BlueControls.ItemCollection {

    public class ColumnPadItem : FixedRectangleBitmapPadItem {

        #region Fields

        public static BlueFont? CellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard);

        public static BlueFont? ChapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard);

        public static BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);

        public readonly ColumnItem? Column;

        #endregion

        #region Constructors

        public ColumnPadItem(string internalname) : base(internalname) { }

        public ColumnPadItem(ColumnItem c) : base(c.Name) => Column = c;

        #endregion

        #region Properties

        public string Datenbank {
            get {
                if (Column?.Database == null) { return "?"; }
                return Column.Database.Filename.FileNameWithSuffix();
            }
        }

        public string Interner_Name {
            get {
                if (Column == null) { return "?"; }
                return Column.Name;
            }
        }

        /// <summary>
        /// Wird von Flexoptions aufgerufen
        /// </summary>
        public string Spalte_bearbeiten {
            get => string.Empty;
            set {
                if (Column == null) { return; }
                Forms.TableView.OpenColumnEditor(Column, null, null);
            }
        }

        #endregion

        #region Methods

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new() { };
            if (Column == null) { return l; }

            l.Add(new FlexiControlForProperty<string>(() => Datenbank));
            l.Add(new FlexiControlForProperty<string>(() => Interner_Name));
            l.Add(new FlexiControlForProperty<string>(() => Spalte_bearbeiten, ImageCode.Spalte));
            l.Add(new FlexiControl());
            l.Add(new FlexiControlForProperty<string>(() => Column.Caption));
            l.Add(new FlexiControl());
            l.Add(new FlexiControlForProperty<string>(() => Column.Ueberschrift1));
            l.Add(new FlexiControlForProperty<string>(() => Column.Ueberschrift2));
            l.Add(new FlexiControlForProperty<string>(() => Column.Ueberschrift3));
            l.Add(new FlexiControl());
            l.Add(new FlexiControlForProperty<string>(() => Column.Quickinfo));
            l.Add(new FlexiControlForProperty<string>(() => Column.AdminInfo));

            if (AdditionalStyleOptions != null) {
                l.Add(new FlexiControl());
                l.AddRange(AdditionalStyleOptions);
            }

            //ItemCollectionList.ItemCollectionList layouts = new();
            //foreach (var thisLayouts in Row.Database.Layouts) {
            //    ItemCollectionPad p = new(thisLayouts, string.Empty);
            //    layouts.Add(p.Caption, p.Id, enImageCode.Stern);
            //}
            //l.Add(new FlexiControlForProperty(()=> this.Layout-ID", layouts));
            //l.AddRange(base.GetStyleOptions());
            return l;
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (Column != null) {
                t = t + "Database=" + Column.Database.Filename.ToNonCritical() + ", ";
                t = t + "ColumnKey=" + Column.Key + ", ";
                t = t + "ColumnName=" + Column.Name.ToNonCritical() + ", ";
            }
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "Column";

        protected override Bitmap GeneratePic() {
            if (Column == null) {
                return QuickImage.Get(ImageCode.Warnung, 128);
            }

            var wi = Table.TmpColumnContentWidth(null, Column, CellFont, 16);

            var bmp = new Bitmap(Math.Max((int)(wi * 0.7), 30), 300);
            var gr = Graphics.FromImage(bmp);

            gr.Clear(Column.BackColor);
            //gr.DrawString(Column.Caption, CellFont, )
            //Table.Draw_FormatedText(gr,)

            for (var z = 0; z < 3; z++) {
                var n = Column.Ueberschrift(z);
                if (!string.IsNullOrEmpty(n)) {
                    Skin.Draw_FormatedText(gr, n, null, Alignment.Horizontal_Vertical_Center, new Rectangle(0, z * 16, bmp.Width, 61), null, false, ColumnFont, true);
                }
            }

            gr.TranslateTransform(bmp.Width / 2, 50);
            gr.RotateTransform(-90);
            Skin.Draw_FormatedText(gr, Column.Caption, null, Alignment.VerticalCenter_Left, new Rectangle(-150, -150, 300, 300), null, false, ColumnFont, true);

            gr.TranslateTransform(-bmp.Width / 2, -50);
            gr.ResetTransform();

            gr.DrawLine(Pens.Black, 0, 210, bmp.Width, 210);

            var r = Column.Database.Row.First();
            if (r != null) {
                Table.Draw_FormatedText(gr, r.CellGetString(Column), Column, new Rectangle(0, 210, bmp.Width, 90), Design.Table_Cell, States.Standard, BlueDatabase.Enums.ShortenStyle.Replaced, Column.BildTextVerhalten);
            }

            return bmp;
        }

        protected override BasicPadItem? TryParse(string id, string name, List<KeyValuePair<string, string>> toParse) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                var x = new ColumnPadItem(name);
                x.Parse(toParse);
                return x;
            }
            return null;
        }

        #endregion
    }
}