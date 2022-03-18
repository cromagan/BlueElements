﻿// Authors:
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

    public class RowWithFilterPaditem : FixedRectangleBitmapPadItem, IReadableText {

        #region Fields

        public static BlueFont? CellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard);

        public static BlueFont? ChapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard);

        public static BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);

        /// <summary>
        /// Laufende Nummer, bestimmt die einfärbung
        /// </summary>
        public readonly int ID;

        public Database? Database;
        public FilterCollection? Filter;

        #endregion

        #region Constructors

        public RowWithFilterPaditem(Database? db, int id) : this(UniqueInternal(), db, id) { }

        public RowWithFilterPaditem(string intern, Database? db, int id) : base(intern) {
            Database = db;
            if (db != null) { Filter = new FilterCollection(db); }
            ID = id;
        }

        public RowWithFilterPaditem(string intern) : this(intern, null, 0) { }

        #endregion

        #region Methods

        /// <summary>
        /// Wird von Flexoptions aufgerufen
        /// </summary>
        public void Datenbankkopf() {
            if (Database == null) { return; }
            BlueControls.Forms.TableView.OpenDatabaseHeadEditor(Database);
        }

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new() { };
            if (Database == null) { return l; }

            l.Add(new FlexiControlForProperty(Database, "Caption"));
            l.Add(new FlexiControlForProperty(this, "Datenbankkopf", ImageCode.Datenbank));
            l.Add(new FlexiControl());
            //l.Add(new FlexiControlForProperty(Column, "Caption"));
            //l.Add(new FlexiControl());
            //l.Add(new FlexiControlForProperty(Column, "Ueberschrift1"));
            //l.Add(new FlexiControlForProperty(Column, "Ueberschrift2"));
            //l.Add(new FlexiControlForProperty(Column, "Ueberschrift3"));
            //l.Add(new FlexiControl());
            //l.Add(new FlexiControlForProperty(Database.t, "Quickinfo"));
            //l.Add(new FlexiControlForProperty(Column, "AdminInfo"));

            //if (AdditionalStyleOptions != null) {
            //    l.Add(new FlexiControl());
            //    l.AddRange(AdditionalStyleOptions);
            //}

            return l;
        }

        public string ReadableText() {
            if (Database != null) { return "Zeile aus " + Database.Caption; }

            return "[FEHLER]";
        }

        public QuickImage? SymbolForReadableText() {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IDColor(ID));
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            if (Database != null) {
                t = t + "Database=" + Database.Filename.ToNonCritical() + ", ";
            }
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "RowWithFilter";

        protected override Bitmap GeneratePic() {
            if (Database == null) {
                return QuickImage.Get(ImageCode.Warnung, 128)!;
            }

            var bmp = new Bitmap(300, 300);
            var gr = Graphics.FromImage(bmp);

            gr.Clear(Skin.IDColor(ID));
            Skin.Draw_FormatedText(gr, Database.Filename.FileNameWithoutSuffix(), QuickImage.Get(ImageCode.Datenbank, 32)!,
                Alignment.Horizontal_Vertical_Center, new Rectangle(5, 5, 290, 290), ColumnFont, false);
            ////Table.Draw_FormatedText(gr,)

            //for (var z = 0; z < 3; z++) {
            //    var n = Column.Ueberschrift(z);
            //    if (!string.IsNullOrEmpty(n)) {
            //        Skin.Draw_FormatedText(gr, n, null, enAlignment.Horizontal_Vertical_Center, new Rectangle(0, z * 16, bmp.Width, 61), null, false, ColumnFont, true);
            //    }
            //}

            //gr.TranslateTransform(bmp.Width / 2, 50);
            //gr.RotateTransform(-90);
            //Skin.Draw_FormatedText(gr, Column.Caption, null, enAlignment.VerticalCenter_Left, new Rectangle(-150, -150, 300, 300), null, false, ColumnFont, true);

            //gr.TranslateTransform(-bmp.Width / 2, -50);
            //gr.ResetTransform();

            //gr.DrawLine(Pens.Black, 0, 210, bmp.Width, 210);

            //var r = Column.Database.Row.First();
            //if (r != null) {
            //    Table.Draw_FormatedText(gr, r.CellGetString(Column), Column, new Rectangle(0, 210, bmp.Width, 90), Design.Table_Cell, States.Standard, BlueDatabase.Enums.ShortenStyle.Replaced, Column.BildTextVerhalten);
            //}

            return bmp;
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