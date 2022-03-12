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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection {

    public class RowFormulaPadItem : FixedRectangleBitmapPadItem {

        #region Fields

        private Database? _database;
        private string _lastQuickInfo;
        private string _layoutId;
        private long _rowKey;
        private string _tmpQuickInfo;

        #endregion

        #region Constructors

        public RowFormulaPadItem() : this(string.Empty, null, -1, string.Empty) { }

        public RowFormulaPadItem(string internalname) : this(internalname, null, -1, string.Empty) { }

        public RowFormulaPadItem(Database database, long rowkey) : this(database, rowkey, string.Empty) { }

        public RowFormulaPadItem(Database database, long rowkey, string layoutId) : this(string.Empty, database, rowkey, layoutId) { }

        public RowFormulaPadItem(string internalname, Database? database, long rowkey, string layoutId) : base(internalname) {
            _database = database;
            if (_database != null) { _database.Disposing += _Database_Disposing; }
            _rowKey = rowkey;
            if (_database != null && string.IsNullOrEmpty(layoutId)) {
                ItemCollectionPad p = new(_database.Layouts[0], string.Empty);
                layoutId = p.Id;
            }
            _layoutId = layoutId;
        }

        #endregion

        #region Properties

        // Namen so lassen, wegen Kontextmenu
        public string Layout_Id {
            get => _layoutId;
            set {
                if (value == _layoutId) { return; }
                _layoutId = value;
                RemovePic();
            }
        }

        public override string QuickInfo {
            get {
                var r = Row;
                if (r == null) { return string.Empty; }
                if (_lastQuickInfo == r.QuickInfo) { return _tmpQuickInfo; }
                _lastQuickInfo = r.QuickInfo;
                _tmpQuickInfo = _lastQuickInfo.Replace(r.CellFirstString(), "<b>[<imagecode=Stern|16>" + Row.CellFirstString() + "]</b>");
                return _tmpQuickInfo;
            }
            set {
                // Werte zurücksetzen
                _lastQuickInfo = string.Empty;
                _tmpQuickInfo = string.Empty;
            }
        }

        public RowItem? Row => _database?.Row.SearchByKey(_rowKey);

        #endregion

        #region Methods

        public void Datensatz_bearbeiten() {
            _tmpQuickInfo = string.Empty; // eigentlich unnötig, da RowChanged anschlagen müsste
            EditBoxRow.Show("Datensatz bearbeiten:", Row, true);
        }

        public override void DesignOrStyleChanged() => RemovePic();

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new()
            {
                new FlexiControlForProperty(this, "Datensatz bearbeiten", enImageCode.Stift),
                new FlexiControl()
            };
            ItemCollectionList.ItemCollectionList layouts = new();
            foreach (var thisLayouts in Row.Database.Layouts) {
                ItemCollectionPad p = new(thisLayouts, string.Empty);
                layouts.Add(p.Caption, p.Id, enImageCode.Stern);
            }
            l.Add(new FlexiControlForProperty(this, "Layout-ID", layouts));
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "layoutid":
                    _layoutId = value.FromNonCritical();
                    return true;

                case "database":
                    _database = Database.GetByFilename(value, false, false);
                    _database.Disposing += _Database_Disposing;
                    return true;

                case "rowid": // TODO: alt
                case "rowkey":
                    _rowKey = LongParse(value);
                    //Row = ParseExplicit_TMPDatabase.Row.SearchByKey(LongParse(value));
                    //if (_Row != null) { ParseExplicit_TMPDatabase = null; }
                    return true;

                case "firstvalue":
                    var n = value.FromNonCritical();
                    if (Row != null) {
                        if (!string.Equals(Row.CellFirstString(), n, StringComparison.CurrentCultureIgnoreCase)) {
                            MessageBox.Show("<b><u>Eintrag hat sich geändert:</b></u><br><b>Von: </b> " + n + "<br><b>Nach: </b>" + Row.CellFirstString(), enImageCode.Information, "OK");
                        }
                        return true; // Alles beim Alten
                    }
                    var rowtmp = _database.Row[n];
                    if (rowtmp == null) {
                        MessageBox.Show("<b><u>Eintrag nicht hinzugefügt</b></u><br>" + n, enImageCode.Warnung, "OK");
                    } else {
                        _rowKey = rowtmp.Key;
                        MessageBox.Show("<b><u>Eintrag neu gefunden:</b></u><br>" + n, enImageCode.Warnung, "OK");
                    }
                    return true; // Alles beim Alten
            }
            return false;
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            t = t + "LayoutID=" + _layoutId.ToNonCritical() + ", ";
            if (_database != null) { t = t + "Database=" + _database.Filename.ToNonCritical() + ", "; }
            if (_rowKey != 0) { t = t + "RowKey=" + _rowKey + ", "; }
            if (Row is RowItem r) { t = t + "FirstValue=" + r.CellFirstString().ToNonCritical() + ", "; }
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "ROW";

        protected override Bitmap GeneratePic() {
            if (string.IsNullOrEmpty(_layoutId) || !_layoutId.StartsWith("#")) {
                return QuickImage.Get(enImageCode.Warnung, 128);
            }

            CreativePad pad = new(new ItemCollectionPad(_layoutId, _database, _rowKey));
            var re = pad.Item.MaxBounds(null);

            var generatedBitmap = new Bitmap((int)re.Width, (int)re.Height);

            var mb = pad.Item.MaxBounds(null);
            var zoomv = ItemCollectionPad.ZoomFitValue(mb, 0, 0, generatedBitmap.Size);
            var centerpos = ItemCollectionPad.CenterPos(mb, 0, 0, generatedBitmap.Size, zoomv);
            var slidervalues = ItemCollectionPad.SliderValues(mb, zoomv, centerpos);
            pad.ShowInPrintMode = true;
            pad.Unselect();
            if (Parent.SheetStyle != null) { pad.Item.SheetStyle = Parent.SheetStyle; }
            pad.Item.DrawCreativePadToBitmap(generatedBitmap, enStates.Standard, zoomv, slidervalues.X, slidervalues.Y, null);
            //if (sizeChangeAllowed) { p_RU.SetTo(p_LO.X + GeneratedBitmap.Width, p_LO.Y + GeneratedBitmap.Height); }
            //SizeChanged();
            return generatedBitmap;
        }

        private void _Database_Disposing(object sender, System.EventArgs e) {
            _database.Disposing -= _Database_Disposing;
            _database = null;
            RemovePic();
        }

        #endregion
    }
}