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

    public class RowSelectFieldPadItem : CustomizableShowPadItem, IReadableText, IAcceptAndSends, IContentHolder, IItemToControl, ICalculateOneRowItemLevel {

        #region Fields

        private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld;
        private string _ID = string.Empty;
        private string _text = string.Empty;
        private ÜberschriftAnordnung _überschiftanordung = ÜberschriftAnordnung.Über_dem_Feld;

        #endregion

        #region Constructors

        public RowSelectFieldPadItem(Database? db, int id) : this(string.Empty, db, id) { }

        public RowSelectFieldPadItem(string intern, Database? db, int id) : base(intern) {
            Database = db;
            Id = id;
        }

        public RowSelectFieldPadItem(string internalname) : base(internalname) { }

        #endregion

        #region Properties

        public ÜberschriftAnordnung CaptionPosition {
            get => _überschiftanordung;
            set {
                if (_überschiftanordung == value) { return; }
                _überschiftanordung = value;
                OnChanged();
            }
        }

        public Database? Database { get; set; }

        public EditTypeFormula EditType {
            get => _bearbeitung;
            set {
                if (_bearbeitung == value) { return; }
                _bearbeitung = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Laufende Nummer, bestimmt die Einfärbung
        /// </summary>
        public int Id { get; set; }

        [Description("Wenn eine ID vergeben wird, ist es möglich, dieses Feld mit einer internen Programmierung anzusprechen.\r\bAls Nebeneffekt wird der Text im Editor mit angezeigt. ")]
        public string ID {
            get => _ID;
            set {
                if (_ID == value) { return; }
                _ID = value;
                OnChanged();
            }
        }

        public string Text {
            get => _text;
            set {
                if (_text == value) { return; }
                _text = value;
                OnChanged();
            }
        }

        #endregion

        #region Methods

        public override System.Windows.Forms.Control? CreateControl(ConnectedFormulaView parent) {
            if (GetRowFrom is ICalculateRowsItemLevel rfw2) {
                var ff = parent.SearchOrGenerate((BasicPadItem)rfw2);

                var c = new FlexiControl();
                c.Caption = _text + ":";
                c.EditType = EditType;
                c.CaptionPosition = CaptionPosition;
                c.Tag = Internal;
                if (ff is ICalculateRowsControlLevel cc) { cc.Childs.Add(c); }
                return c;
            }

            var cy = new FlexiControl();
            cy.Caption = CaptionPosition + ":";
            cy.EditType = EditType;
            cy.CaptionPosition = CaptionPosition;
            cy.DisabledReason = "Keine Verknüpfung vorhanden.";
            cy.Tag = Internal;
            return cy;
        }

        public override List<GenericControl> GetStyleOptions() {
            List<GenericControl> l = new();

            l.AddRange(base.GetStyleOptions());

            var u = new ItemCollection.ItemCollectionList.ItemCollectionList();
            u.AddRange(typeof(ÜberschriftAnordnung));
            l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
            var b = new ItemCollection.ItemCollectionList.ItemCollectionList();
            b.AddRange(typeof(EditTypeFormula));
            l.Add(new FlexiControlForProperty<EditTypeFormula>(() => EditType, b));

            l.Add(new FlexiControl());
            l.Add(new FlexiControlForProperty<string>(() => Text));
            l.Add(new FlexiControlForProperty<string>(() => ID));
            l.Add(new FlexiControl());

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
                case "text":
                    Text = value.FromNonCritical();
                    return true;

                case "id":
                    _ID = value.FromNonCritical();
                    return true;

                case "caption":
                    _überschiftanordung = (ÜberschriftAnordnung)IntParse(value);
                    return true;

                case "edittype":
                    _bearbeitung = (EditTypeFormula)IntParse(value);
                    return true;
            }
            return false;
        }

        public string ReadableText() {
            return "Gewählte Zeile: " + _text;
        }

        public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Zeile, 16);

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            t = t + "Text=" + _text.ToNonCritical() + ", ";
            t = t + "ID=" + _ID.ToNonCritical() + ", ";
            t = t + "EditType=" + ((int)_bearbeitung).ToString() + ", ";
            t = t + "Caption=" + ((int)_überschiftanordung).ToString() + ", ";

            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "FI-RowSelectField";

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
            //    base.DrawFakeControl(positionModified, zoom, CaptionPosition, gr, Column.ReadableText());
            //}

            base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        }

        protected override BasicPadItem? TryCreate(string id, string name) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                return new RowSelectFieldPadItem(name);
            }
            return null;
        }

        private void RepairConnections() {
            ConnectsTo.Clear();

            if (GetRowFrom != null) {
                ConnectsTo.Add(new ItemConnection(ConnectionType.Top, true, (BasicPadItem)GetRowFrom, ConnectionType.Bottom, false));
            }
        }

        #endregion
    }
}