#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

namespace BlueControls.ItemCollection {
    public class ItemCollectionPad : ListExt<BasicPadItem> {
        #region  Variablen-Deklarationen 

        public static readonly int DPI = 300;
        private RowItem _SheetStyle;
        private decimal _SheetStyleScale;
        private SizeF _SheetSizeInMM = SizeF.Empty;
        private System.Windows.Forms.Padding _RandinMM = System.Windows.Forms.Padding.Empty;

        public PointM P_rLO;
        public PointM P_rLU;
        public PointM P_rRU;
        public PointM P_rRO;

        private enSnapMode _SnapMode = enSnapMode.SnapToGrid;
        private float _GridShow = 10;
        private float _Gridsnap = 1;

        public string Caption = string.Empty;

        public string ID = string.Empty;

        /// <summary>
        /// Für automatische Generierungen, die zu schnell hintereinander kommen, ein Counter für den Dateinamen
        /// </summary>
        private readonly int IDCount = 0;

        #endregion

        public bool IsParsing { get; private set; }

        [DefaultValue(true)]
        public bool IsSaved { get; set; }

        [DefaultValue(false)]
        public enSnapMode SnapMode {
            get => _SnapMode;
            set {
                if (_SnapMode == value) { return; }

                _SnapMode = value;
                CheckGrid();
            }
        }

        [DefaultValue(10.0)]
        public float GridShow {
            get => _GridShow;
            set {

                if (_GridShow == value) { return; }

                _GridShow = value;
                CheckGrid();
            }
        }

        [DefaultValue(10.0)]
        public float GridSnap {
            get => _Gridsnap;
            set {

                if (_Gridsnap == value) { return; }

                _Gridsnap = value;
                CheckGrid();
            }
        }

        public Color BackColor { get; set; } = Color.White;

        public RowItem SheetStyle {
            get => _SheetStyle;

            set {
                if (_SheetStyle == value) { return; }

                //        if (!_isParsing && value == SheetStyle) { return; }
                //if (Skin.StyleDB == null) { Skin.InitStyles(); }

                /// /       Item.SheetStyle = Skin.StyleDB.Row[value];
                //   if (Item.SheetStyle == null) { Item.SheetStyle = Skin.StyleDB.Row.First(); }// Einfach die Erste nehmen

                _SheetStyle = value;

                DesignOrStyleChanged();

                //RepairAll(0, false);

                OnDoInvalidate();
            }
        }

        public SizeF SheetSizeInMM {
            get => _SheetSizeInMM;
            set {
                if (value == _SheetSizeInMM) { return; }
                _SheetSizeInMM = new SizeF(value.Width, value.Height);
                GenPoints();
            }
        }

        public System.Windows.Forms.Padding RandinMM {
            get => _RandinMM;
            set {
                _RandinMM = new System.Windows.Forms.Padding(Math.Max(0, value.Left), Math.Max(0, value.Top), Math.Max(0, value.Right), Math.Max(0, value.Bottom));
                GenPoints();
            }
        }

        #region  Construktor + Initialize 

        public ItemCollectionPad() : base() {

            if (Skin.StyleDB == null) { Skin.InitStyles(); }

            SheetSizeInMM = Size.Empty;
            RandinMM = System.Windows.Forms.Padding.Empty;

            Caption = "";

            IDCount++;

            ID = "#" + DateTime.UtcNow.ToString(Constants.Format_Date) + IDCount; // # ist die erkennung, dass es kein Dateiname sondern ein Item ist

            if (Skin.StyleDB == null) { Skin.InitStyles(); }
            _SheetStyle = null;
            _SheetStyleScale = 1.0m;

            if (Skin.StyleDB != null) { _SheetStyle = Skin.StyleDB.Row.First(); }
        }

        public ItemCollectionPad(string layoutID, Database database, int rowkey) : this(database.Layouts[database.Layouts.LayoutIDToIndex(layoutID)], string.Empty) {

            // Wenn nur die Row ankommt und diese null ist, kann gar nix generiert werden
            ResetVariables();
            ParseVariable(database.Row.SearchByKey(rowkey));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="needPrinterData"></param>
        /// <param name="useThisID">Wenn das Blatt bereits eine Id hat, muss die Id verwendet werden. Wird das Feld leer gelassen, wird die beinhaltete Id benutzt.</param>
        public ItemCollectionPad(string ToParse, string useThisID) : this() {

            if (string.IsNullOrEmpty(ToParse) || ToParse.Length < 3) { return; }
            if (ToParse.Substring(0, 1) != "{") { return; }// Alte Daten gehen eben verloren.

            ID = useThisID;

            foreach (var pair in ToParse.GetAllTags()) {

                switch (pair.Key.ToLower()) {
                    case "sheetsize":
                        _SheetSizeInMM = Extensions.SizeFParse(pair.Value);
                        GenPoints();
                        break;

                    case "printarea":
                        _RandinMM = Extensions.PaddingParse(pair.Value);
                        GenPoints();
                        break;

                    //case "items":
                    //    Parse(pvalue);
                    //    break;

                    case "relation": // TODO: Entfernt, 24.05.2021
                        //AllRelations.Add(new clsPointRelation(this, null, pair.Value));
                        break;

                    case "caption":
                        Caption = pair.Value.FromNonCritical();
                        break;

                    case "backcolor":
                        BackColor = Color.FromArgb(int.Parse(pair.Value));
                        break;

                    case "id":
                        if (string.IsNullOrEmpty(ID)) { ID = pair.Value.FromNonCritical(); }
                        break;

                    case "style":
                        _SheetStyle = Skin.StyleDB.Row[pair.Value];
                        if (_SheetStyle == null) { _SheetStyle = Skin.StyleDB.Row.First(); }// Einfach die Erste nehmen
                        break;

                    case "fontscale":
                        _SheetStyleScale = decimal.Parse(pair.Value);
                        break;

                    case "snapmode":
                        _SnapMode = (enSnapMode)int.Parse(pair.Value);
                        break;

                    case "grid":
                        //_Grid = pair.Value.FromPlusMinus();
                        break;

                    case "gridshow":
                        _GridShow = float.Parse(pair.Value);
                        break;

                    case "gridsnap":
                        _Gridsnap = float.Parse(pair.Value);
                        break;

                    case "format": //_Format = DirectCast(Integer.Parse(pair.Value.Value), enDataFormat)
                        break;

                    case "items":
                        ParseItems(pair.Value);
                        break;

                    case "dpi":
                        if (int.Parse(pair.Value) != DPI) {
                            Develop.DebugPrint("DPI Unterschied: " + DPI + " <> " + pair.Value);
                        }

                        break;

                    case "sheetstyle":
                        if (Skin.StyleDB == null) { Skin.InitStyles(); }

                        _SheetStyle = Skin.StyleDB.Row[pair.Value];
                        break;

                    case "sheetstylescale":
                        _SheetStyleScale = decimal.Parse(pair.Value);
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        private void ParseItems(string ToParse) {
            foreach (var pair in ToParse.GetAllTags()) {

                switch (pair.Key.ToLower()) {
                    //case "sheetsize":
                    //    _SheetSizeInMM = Extensions.SizeFParse(pair.Value);
                    //    GenPoints();
                    //    break;

                    //case "printarea":
                    //    _RandinMM = Extensions.PaddingParse(pair.Value);
                    //    GenPoints();
                    //    break;

                    ////case "items":
                    ////    Parse(pvalue);
                    ////    break;

                    //case "relation":
                    //    AllRelations.Add(new clsPointRelation(this, pair.Value));
                    //    break;

                    //case "caption":
                    //    Caption = pair.Value.FromNonCritical();
                    //    break;

                    //case "id":
                    //    if (string.IsNullOrEmpty(ID)) { ID = pair.Value.FromNonCritical(); }
                    //    break;

                    //case "style":
                    //    _SheetStyle = Skin.StyleDB.Row[pair.Value];
                    //    if (_SheetStyle == null) { _SheetStyle = Skin.StyleDB.Row.First(); }// Einfach die Erste nehmen
                    //    break;

                    //case "fontscale":
                    //    _SheetStyleScale = decimal.Parse(pair.Value);
                    //    break;

                    //case "grid":
                    //    //_Grid = pair.Value.FromPlusMinus();
                    //    break;

                    //case "gridshow":
                    //    //_GridShow = float.Parse(pair.Value);
                    //    break;

                    //case "gridsnap":
                    //    //_Gridsnap = float.Parse(pair.Value);
                    //    break;
                    //case "format": //_Format = DirectCast(Integer.Parse(pair.Value.Value), enDataFormat)
                    //    break;

                    case "item":
                        var i = BasicPadItem.NewByParsing(this, pair.Value);
                        if (i != null) { Add(i); }
                        break;

                    case "dpi": // TODO: LÖschen 26.02.2020
                        if (int.Parse(pair.Value) != DPI) {
                            Develop.DebugPrint("DPI Unterschied: " + DPI + " <> " + pair.Value);
                        }

                        break;

                    case "sheetstyle": // TODO: LÖschen 26.02.2020
                        //if (Skin.StyleDB == null) { Skin.InitStyles(); }

                        //_SheetStyle = Skin.StyleDB.Row[pair.Value];
                        break;

                    case "sheetstylescale": // TODO: LÖschen 26.02.2020
                        //_SheetStyleScale = decimal.Parse(pair.Value);
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        #endregion

        #region  Event-Deklarationen + Delegaten 

        public event EventHandler DoInvalidate;
        #endregion

        internal void InDenVordergrund(BasicPadItem ThisItem) {
            if (IndexOf(ThisItem) == Count - 1) { return; }

            var g1 = ThisItem.Gruppenzugehörigkeit;
            ThisItem.Gruppenzugehörigkeit = string.Empty;

            Remove(ThisItem);
            Add(ThisItem);

            ThisItem.Gruppenzugehörigkeit = g1;
            OnDoInvalidate();
        }

        internal void InDenHintergrund(BasicPadItem ThisItem) {
            if (IndexOf(ThisItem) == 0) { return; }

            var g1 = ThisItem.Gruppenzugehörigkeit;
            ThisItem.Gruppenzugehörigkeit = string.Empty;

            Remove(ThisItem);
            Insert(0, ThisItem);

            ThisItem.Gruppenzugehörigkeit = g1;
            OnDoInvalidate();
        }

        //public void RecomputePointAndRelations()
        //{
        //    foreach (var thisItem in this)
        //    {
        //        thisItem?.GenerateInternalRelation();
        //    }
        //}

        public void Swap(BasicPadItem Nr1, BasicPadItem Nr2) {
            var g1 = Nr1.Gruppenzugehörigkeit;
            Nr1.Gruppenzugehörigkeit = string.Empty;

            var g2 = Nr2.Gruppenzugehörigkeit;
            Nr2.Gruppenzugehörigkeit = string.Empty;

            Swap(IndexOf(Nr1), IndexOf(Nr2));

            Nr1.Gruppenzugehörigkeit = g1;
            Nr2.Gruppenzugehörigkeit = g2;
            OnDoInvalidate();
        }

        #region  Standard-Such-Properties 

        public BasicPadItem this[string Internal] {
            get {
                if (string.IsNullOrEmpty(Internal)) {
                    return null;
                }

                foreach (var ThisItem in this) {
                    if (ThisItem != null) {
                        if (Internal.ToUpper() == ThisItem.Internal.ToUpper()) {
                            return ThisItem;
                        }
                    }
                }

                return null;
            }
        }

        public List<BasicPadItem> this[int x, int Y] => this[new Point(x, Y)];

        public List<BasicPadItem> this[Point p] {
            get {
                var l = new List<BasicPadItem>();

                foreach (var ThisItem in this) {
                    if (ThisItem != null && ThisItem.Contains(p, 1)) {
                        l.Add(ThisItem);
                    }
                }

                return l;
            }
        }

        #endregion
        #region  Properties 

        [DefaultValue(1.0)]
        public decimal SheetStyleScale {
            get => _SheetStyleScale;
            set {

                if (value < 0.1m) { value = 0.1m; }

                if (_SheetStyleScale == value) { return; }

                _SheetStyleScale = value;

                DesignOrStyleChanged();

                OnDoInvalidate();
            }
        }

        #endregion

        public void OnDoInvalidate() {
            DoInvalidate?.Invoke(this, System.EventArgs.Empty);
        }

        protected override void OnItemAdded(BasicPadItem item) {
            if (item == null) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Null Item soll hinzugefügt werden!");
            }

            if (string.IsNullOrEmpty(item.Internal)) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Der Auflistung soll ein Item hinzugefügt werden, welches keinen Namen hat " + item.Internal);
            }

            base.OnItemAdded(item);
            item.PointMoved(null);

            IsSaved = false;

            item.Changed += Item_Changed;

            if (item.Parent != this) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Parent ungleich!");

            }
            OnDoInvalidate();
        }

        private void Item_Changed(object sender, System.EventArgs e) {
            IsSaved = false;
            OnDoInvalidate();

        }

        public void DesignOrStyleChanged() {
            foreach (var thisItem in this) {
                thisItem?.DesignOrStyleChanged();
            }
            OnDoInvalidate();
        }

        public void Remove(string internalname) {
            Remove(this[internalname]);
        }

        public new void Remove(BasicPadItem item) {
            if (item == null || !Contains(item)) { return; }

            base.Remove(item);

            if (string.IsNullOrEmpty(item.Gruppenzugehörigkeit)) { return; }

            foreach (var ThisToo in this) {
                if (item.Gruppenzugehörigkeit.ToLower() == ThisToo.Gruppenzugehörigkeit?.ToLower()) {
                    Remove(ThisToo);
                    return; // Wird eh eine Kettenreaktion ausgelöst -  und der Iteraor hier wird beschädigt
                }
            }
        }

        public RectangleM MaximumBounds(List<BasicPadItem> ZoomItems) {
            var x1 = decimal.MaxValue;
            var y1 = decimal.MaxValue;
            var x2 = decimal.MinValue;
            var y2 = decimal.MinValue;

            var Done = false;

            foreach (var ThisItem in this) {
                if (ThisItem != null) {
                    if (ZoomItems == null || ZoomItems.Contains(ThisItem)) {

                        var UA = ThisItem.ZoomToArea();

                        x1 = Math.Min(x1, UA.Left);
                        y1 = Math.Min(y1, UA.Top);

                        x2 = Math.Max(x2, UA.Right);
                        y2 = Math.Max(y2, UA.Bottom);
                        Done = true;
                    }
                }
            }

            return !Done ? new RectangleM() : new RectangleM(x1, y1, x2 - x1, y2 - y1);
        }

        private void GenPoints() {

            if (Math.Abs(_SheetSizeInMM.Width) < 0.001 || Math.Abs(_SheetSizeInMM.Height) < 0.001) {
                if (P_rLO != null) {
                    P_rLO.Parent = null;
                    P_rLO = null;

                    P_rRO.Parent = null;
                    P_rRO = null;

                    P_rRU.Parent = null;
                    P_rRU = null;

                    P_rLU.Parent = null;
                    P_rLU = null;
                }

                return;
            }

            if (P_rLO == null) {
                P_rLO = new PointM(this, "Druckbereich LO", 0, 0);
                P_rRO = new PointM(this, "Druckbereich RO", 0, 0);
                P_rRU = new PointM(this, "Druckbereich RU", 0, 0);
                P_rLU = new PointM(this, "Druckbereich LU", 0, 0);
            }

            var SSW = Math.Round(modConverter.mmToPixel((decimal)_SheetSizeInMM.Width, DPI), 1);
            var SSH = Math.Round(modConverter.mmToPixel((decimal)_SheetSizeInMM.Height, DPI), 1);
            var rr = Math.Round(modConverter.mmToPixel(_RandinMM.Right, DPI), 1);
            var rl = Math.Round(modConverter.mmToPixel(_RandinMM.Left, DPI), 1);
            var ro = Math.Round(modConverter.mmToPixel(_RandinMM.Top, DPI), 1);
            var ru = Math.Round(modConverter.mmToPixel(_RandinMM.Bottom, DPI), 1);

            P_rLO.SetTo(rl, ro);
            P_rRO.SetTo(SSW - rr, ro);
            P_rRU.SetTo(SSW - rr, SSH - ru);
            P_rLU.SetTo(rl, SSH - ru);
            CheckGrid();
            OnDoInvalidate();
        }

        private void CheckGrid() {

            // Todo: bei erschiedenen SnapModes muss hier evtl. was gemacht werden.

        }

        protected override void OnItemRemoving(BasicPadItem item) {
            item.Changed -= Item_Changed;
            base.OnItemRemoving(item);
            OnDoInvalidate();
        }

        protected override void OnItemRemoved() {
            base.OnItemRemoved();
            OnDoInvalidate();
        }

        public override void OnChanged() {
            base.OnChanged();
            IsSaved = false;
            OnDoInvalidate();
        }

        public new string ToString() {

            var t = "{";

            if (!string.IsNullOrEmpty(ID)) { t = t + "ID=" + ID.ToNonCritical() + ", "; }

            if (!string.IsNullOrEmpty(Caption)) { t = t + "Caption=" + Caption.ToNonCritical() + ", "; }

            if (SheetStyle != null) { t = t + "Style=" + SheetStyle.CellFirstString().ToNonCritical() + ", "; }

            if (SheetStyleScale < 0.1m) { SheetStyleScale = 1.0m; }

            t = t + "BackColor=" + BackColor.ToArgb() + ", ";

            if (Math.Abs(SheetStyleScale - 1) > 0.001m) { t = t + "FontScale=" + SheetStyleScale + ", "; }

            if (SheetSizeInMM.Width > 0 && SheetSizeInMM.Height > 0) {
                t = t + "SheetSize=" + SheetSizeInMM + ", ";
                t = t + "PrintArea=" + RandinMM + ", ";
            }

            //t = t + "DPI=" + DPI + ", "; // TODO: Nach Update wieder aktivieren

            t += "Items={";

            foreach (var Thisitem in this) {
                if (Thisitem != null) {
                    t = t + "Item=" + Thisitem.ToString() + ", ";
                }
            }

            t = t.TrimEnd(", ") + "}, ";

            t = t + "SnapMode=" + ((int)_SnapMode).ToString() + ", ";
            t = t + "GridShow=" + _GridShow + ", ";
            t = t + "GridSnap=" + _Gridsnap + ", ";

            return t.TrimEnd(", ") + "}";

        }

        public Bitmap ToBitmap(decimal scale) {
            var r = MaxBounds(null);
            if (r.Width == 0) { return null; }

            modAllgemein.CollectGarbage();

            do {
                if ((int)(r.Width * scale) > 15000) {
                    scale *= 0.8m;
                } else if ((int)(r.Height * scale) > 15000) {
                    scale *= 0.8m;
                } else if ((int)(r.Height * scale) * (int)(r.Height * scale) > 90000000) {
                    scale *= 0.8m;
                } else {
                    break;
                }
            } while (true);

            var I = new Bitmap((int)(r.Width * scale), (int)(r.Height * scale));

            using (var gr = Graphics.FromImage(I)) {
                gr.Clear(BackColor);

                if (!Draw(gr, scale, r.Left * scale, r.Top * scale, Size.Empty, true, null)) {
                    return ToBitmap(scale);
                }
            }
            return I;
        }

        public bool Draw(Graphics gr, decimal zoom, decimal shiftX, decimal shiftY, Size sizeOfParentControl, bool forPrinting, List<BasicPadItem> visibleItems) {

            try {
                if (SheetStyle == null || SheetStyleScale < 0.1m) { return true; }

                foreach (var thisItem in this) {
                    if (thisItem != null) {
                        gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;

                        if (visibleItems == null || visibleItems.Contains(thisItem)) {
                            thisItem.Draw(gr, zoom, shiftX, shiftY, 0, sizeOfParentControl, forPrinting);
                        }
                    }
                }
                return true;
            } catch {
                modAllgemein.CollectGarbage();
                return false;
            }
        }

        protected RectangleM MaxBounds() {
            return MaxBounds(null);
        }

        internal RectangleM MaxBounds(List<BasicPadItem> ZoomItems) {

            var r = Count == 0 ? new RectangleM(0, 0, 0, 0) : MaximumBounds(ZoomItems);
            if (SheetSizeInMM.Width > 0 && SheetSizeInMM.Height > 0) {

                var X1 = Math.Min(r.Left, 0);
                var y1 = Math.Min(r.Top, 0);

                var x2 = Math.Max(r.Right, modConverter.mmToPixel((decimal)SheetSizeInMM.Width, ItemCollectionPad.DPI));
                var y2 = Math.Max(r.Bottom, modConverter.mmToPixel((decimal)SheetSizeInMM.Height, ItemCollectionPad.DPI));

                return new RectangleM(X1, y1, x2 - X1, y2 - y1);
            }

            return r;

        }

        public bool ParseVariable(BlueScript.Variable variable) {

            var did = false;

            foreach (var thisItem in this) {
                if (thisItem is ICanHaveColumnVariables variables) {
                    if (variables.ReplaceVariable(variable)) { did = true; }
                }
            }
            return did;
        }

        public void ParseVariable(RowItem row) {

            if (row != null) {

                (var didSuccesfullyCheck, var error, var script) = row.DoAutomatic(false, "export");
                foreach (var thisV in script.Variablen) {
                    ParseVariable(thisV);
                }
            }
        }

        //private void ParseVariable(string VariableName, ColumnItem Column, RowItem Row) {

        //    switch (Column.Format) {
        //        case enDataFormat.Text:
        //        case enDataFormat.Text_mit_Formatierung:
        //        case enDataFormat.Gleitkommazahl:
        //        case enDataFormat.Datum_und_Uhrzeit:
        //        case enDataFormat.Bit:
        //        case enDataFormat.Ganzzahl:
        //        case enDataFormat.RelationText:
        //            ParseVariable(VariableName, Row.CellGetString(Column));
        //            break;

        //        case enDataFormat.Link_To_Filesystem:

        //            if (!Column.MultiLine) {
        //                var f = Column.BestFile(Row.CellGetString(Column), false);

        //                if (FileExists(f)) {
        //                    if (Column.MultiLine) {
        //                        ParseVariable(VariableName, f);
        //                    } else {
        //                        var x = (Bitmap)BitmapExt.Image_FromFile(f);
        //                        ParseVariable(VariableName, x);
        //                    }
        //                }
        //            }
        //            break;

        //        //case enDataFormat.Relation:
        //        //    ParseVariable(VariableName, enValueType.Unknown, "Nicht implementiert");
        //        //    break;

        //        default:
        //            Develop.DebugPrint("Format unbekannt: " + Column.Format);
        //            break;

        //    }
        //}

        public bool ResetVariables() {
            var did = false;

            foreach (var thisItem in this) {
                if (thisItem is ICanHaveColumnVariables variables) {
                    if (variables.ResetVariables()) { did = true; }
                }
            }

            if (did) { OnDoInvalidate(); }
            return did;

        }

        internal Rectangle DruckbereichRect() {
            return P_rLO == null
                ? new Rectangle(0, 0, 0, 0)
                : new Rectangle((int)P_rLO.X, (int)P_rLO.Y, (int)(P_rRU.X - P_rLO.X), (int)(P_rRU.Y - P_rLO.Y));
        }

        public void SaveAsBitmap(string filename) {

            var i = ToBitmap(1);

            if (i == null) { return; }

            switch (filename.FileSuffix().ToUpper()) {

                case "JPG":
                case "JPEG":
                    i.Save(filename, ImageFormat.Jpeg);
                    break;

                case "PNG":
                    i.Save(filename, ImageFormat.Png);
                    break;

                case "BMP":
                    i.Save(filename, ImageFormat.Bmp);
                    break;

                default:
                    MessageBox.Show("Dateiformat unbekannt: " + filename.FileSuffix().ToUpper(), enImageCode.Warnung, "OK");
                    return;
            }
        }
    }
}