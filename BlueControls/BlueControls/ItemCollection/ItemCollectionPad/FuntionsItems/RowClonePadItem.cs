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
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics.Interfaces;
using System.ComponentModel;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using BlueControls.Interfaces;
using System.Windows.Forms;
using System.Xml;
using BlueControls.ConnectedFormula;

namespace BlueControls.ItemCollection {

    public class RowClonePadItem : FixedRectanglePadItem, IReadableText, IAcceptAndSends, ICalculateRowsItemLevel, IItemToControl {

        #region Fields

        public static BlueFont? CellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard);
        public static BlueFont? ChapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard);
        public static BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);

        public ItemCollectionPad? Parent;
        private bool _genau_eine_Zeile = true;

        private string _VerbindungsID = string.Empty;

        #endregion

        #region Constructors

        public RowClonePadItem(Database? db, int id) : this(string.Empty, db, id) { }

        public RowClonePadItem(string intern, Database? db, int id) : base(intern) {
            Database = db;
            //if (db != null) { Filter = new FilterCollection(db); }
            Id = id;
            Size = new Size(200, 50);
        }

        public RowClonePadItem(string intern) : this(intern, null, 0) { }

        #endregion

        #region Properties

        public Database? Database { get; set; }

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

        /// <summary>
        /// Laufende Nummer, bestimmt die Einfärbung
        /// </summary>
        public int Id { get; set; }

        [Description("Mit dieser Verbindungs-ID können formularübergeifend Filter an\r\nan andere Filterelemnte übergeben werden bzw.\r\nempfangen werden.\r\nZiegt KEIN Pfeil auf dieses Element, übernimmt es den Wert.\r\nAndernfalls empfängt es den Wert.")]
        public string VerbindungsID {
            get => _VerbindungsID;
            set {
                if (_VerbindungsID == value) { return; }
                _VerbindungsID = value;
                OnChanged();
            }
        }

        protected override int SaveOrder => 2;

        #endregion

        #region Methods

        public Control? CreateControl(ConnectedFormulaView parent) {
            var c = new RowCloner(Database, Genau_eine_Zeile, VerbindungsID);
            c.Tag = Internal;
            return c;
        }

        public override List<GenericControl> GetStyleOptions() {
            List<GenericControl> l = new() { };
            if (Database == null) { return l; }
            l.Add(new FlexiControlForProperty<string>(() => Database.Caption));
            //l.Add(new FlexiControlForProperty(Database, "Caption"));
            l.Add(new FlexiControlForProperty<string>(() => Datenbankkopf, ImageCode.Datenbank));
            //l.Add(new FlexiControlForProperty(()=> this.Datenbankkopf"));
            l.Add(new FlexiControl());
            l.Add(new FlexiControlForProperty<bool>(() => Genau_eine_Zeile));
            l.Add(new FlexiControlForProperty<string>(() => VerbindungsID));

            return l;
        }

        public bool IsRecursiveWith(IAcceptAndSends obj) {
            return false;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "database":
                    Database = Database.GetByFilename(value.FromNonCritical(), false, false);
                    return true;

                case "onerow":
                    Genau_eine_Zeile = value.FromPlusMinus();
                    return true;

                case "connectionid":
                    VerbindungsID = value.FromNonCritical();
                    return true;

                case "id":
                    Id = IntParse(value);
                    return true;
            }
            return false;
        }

        public string ReadableText() {
            if (Database != null) {
                if (Genau_eine_Zeile) {
                    return "Geklonte Zeile aus: " + Database.Caption;
                } else {
                    return "Geklonte Zeilen aus: " + Database.Caption;
                }
            }

            return "Zeile-Klone einer Datenbank";
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

            t = t + "ID=" + Id.ToString() + ", ";

            if (Database != null) {
                t = t + "Database=" + Database.Filename.ToNonCritical() + ", ";
            }
            t = t + "OneRow=" + Genau_eine_Zeile.ToPlusMinus() + ", ";

            t = t + "ConnectionID=" + VerbindungsID.ToNonCritical() + ", ";

            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "FI-CloneRowsFilter";

        protected override void DrawExplicit(Graphics gr, RectangleF modifiedPosition, float zoom, float shiftX, float shiftY, bool forPrinting) {
            DrawColorScheme(gr, modifiedPosition, zoom, Id);

            if (Database != null) {
                var txt = string.Empty;
                if (!Genau_eine_Zeile) {
                    txt = "gleiche Zeilen wie\r\n";
                } else {
                    txt = "gleiche Zeile wie\r\n";
                }
                if (!string.IsNullOrEmpty(_VerbindungsID)) {
                    txt = txt + "Klon-ID: " + _VerbindungsID;
                }

                txt = txt + "\r\n(=" + Database.Caption + ")";

                Skin.Draw_FormatedText(gr, txt, QuickImage.Get(ImageCode.Blitz, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, modifiedPosition.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
            }

            gr.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), modifiedPosition);

            base.DrawExplicit(gr, modifiedPosition, zoom, shiftX, shiftY, forPrinting);
        }

        protected override BasicPadItem? TryCreate(string id, string name) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                return new RowClonePadItem(name);
            }
            return null;
        }

        #endregion
    }
}