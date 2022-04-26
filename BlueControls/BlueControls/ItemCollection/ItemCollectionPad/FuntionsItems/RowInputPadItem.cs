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
using System.Drawing;
using BlueBasics.Interfaces;
using BlueControls.Interfaces;
using System.Windows.Forms;
using BlueDatabase.Enums;
using System.ComponentModel;

namespace BlueControls.ItemCollection {

    public class RowInputPadItem : RectanglePadItem, IReadableText, IContentHolder, IItemToControl {

        #region Fields

        public static BlueFont? CellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard);
        public static BlueFont? ChapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard);
        public static BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);

        //private string _ID = string.Empty;
        private string _spaltenname = string.Empty;

        #endregion

        #region Constructors

        public RowInputPadItem() : this(UniqueInternal(), string.Empty) { }

        public RowInputPadItem(string intern, string spaltenname) : base(intern) {
            _spaltenname = spaltenname;
            //Size = new Size(200, 40);
        }

        public RowInputPadItem(string intern) : this(intern, string.Empty) { }

        #endregion

        #region Properties

        [Description("Aus welcher Spalte der Eingangs-Zeile kommen soll.\r\nEs muss der interne Spaltenname der ankommenden Zeile verwendet werden.\r\nAlternativ kann auch #first benutzt werden, wenn die erste Spalte benutzt werden soll.")]
        public string Spaltenname {
            get => _spaltenname;
            set {
                if (_spaltenname == value) { return; }
                _spaltenname = value;
                OnChanged();
            }
        }

        protected override int SaveOrder => 999;

        #endregion

        #region Methods

        public Control? CreateControl(ConnectedFormulaView parent) {
            var c3 = new FlexiControlForCell();
            c3.Width = 200;
            c3.Height = 32;

            //if (string.IsNullOrEmpty(_ID)) {
            c3.CaptionPosition = ÜberschriftAnordnung.Über_dem_Feld;
            //} else {
            //    c3.CaptionPosition = ÜberschriftAnordnung.Über_dem_Feld;
            //    c3.Caption = _ID;
            //}

            c3.EditType = EditTypeFormula.nur_als_Text_anzeigen;
            //c3.DisabledReason = "Dieser Wert kommt wird von einer anderen Datenbank gelesen.;

            //c3.ValueSet(string.Empty, true, true);
            c3.Tag = Internal;
            return c3;
        }

        public override List<GenericControl> GetStyleOptions() {
            List<GenericControl> l = new() { };
            l.Add(new FlexiControlForProperty<string>(() => Spaltenname));
            //l.Add(new FlexiControlForProperty<string>(() => ID));
            return l;
        }

        //public bool IsRecursiveWith(IAcceptAndSends obj) {
        //    if (obj == this) { return true; }

        //    return false;
        //}

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "column":
                    Spaltenname = value.FromNonCritical();
                    return true;

                    //case "id":
                    //    _ID = value.FromNonCritical();
                    //    return true;
            }
            return false;
        }

        public string ReadableText() {
            //if (!string.IsNullOrEmpty(_ID)) { return "Konstanter Wert: " + _text + "(" + _ID + ")"; }

            return "Einganszeilen-Spalte: " + _spaltenname;
        }

        public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Zeile, 10, Color.Transparent, Color.Green);

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            t = t + "Column=" + _spaltenname.ToNonCritical() + ", ";
            //t = t + "ID=" + _ID.ToNonCritical() + ", ";
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "FI-InputRow";

        protected override void DrawExplicit(Graphics gr, RectangleF modifiedPosition, float zoom, float shiftX, float shiftY, bool forPrinting) {
            gr.DrawRectangle(new Pen(Color.Black, zoom), modifiedPosition);

            var t = "Eingangs-Zeilen-Spalte\r\n" + _spaltenname;

            Skin.Draw_FormatedText(gr, t, SymbolForReadableText(), Alignment.Horizontal_Vertical_Center, modifiedPosition.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);

            //gr.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), modifiedPosition);

            base.DrawExplicit(gr, modifiedPosition, zoom, shiftX, shiftY, forPrinting);
        }

        protected override BasicPadItem? TryCreate(string id, string name) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                return new RowInputPadItem(name);
            }
            return null;
        }

        #endregion
    }
}