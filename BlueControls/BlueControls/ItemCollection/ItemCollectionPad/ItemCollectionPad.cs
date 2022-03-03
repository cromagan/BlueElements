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
using BlueControls.Interfaces;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BlueControls.ItemCollection {

    public class ItemCollectionPad : ListExt<BasicPadItem> {

        #region Fields

        public static readonly int DPI = 300;
        public string Caption = string.Empty;
        public string ID = string.Empty;
        public PointM P_rLO;
        public PointM P_rLU;
        public PointM P_rRO;
        public PointM P_rRU;

        /// <summary>
        /// Für automatische Generierungen, die zu schnell hintereinander kommen, ein Counter für den Dateinamen
        /// </summary>
        private readonly int IDCount = 0;

        private float _GridShow = 10;
        private float _Gridsnap = 1;
        private System.Windows.Forms.Padding _RandinMM = System.Windows.Forms.Padding.Empty;
        private SizeF _SheetSizeInMM = SizeF.Empty;
        private RowItem _SheetStyle;
        private float _SheetStyleScale;
        private enSnapMode _SnapMode = enSnapMode.SnapToGrid;

        #endregion

        #region Constructors

        public ItemCollectionPad() : base() {
            if (Skin.StyleDB == null) { Skin.InitStyles(); }
            SheetSizeInMM = Size.Empty;
            RandinMM = System.Windows.Forms.Padding.Empty;
            Caption = "";
            IDCount++;
            ID = "#" + DateTime.UtcNow.ToString(Constants.Format_Date) + IDCount; // # ist die erkennung, dass es kein Dateiname sondern ein Item ist
            if (Skin.StyleDB == null) { Skin.InitStyles(); }
            _SheetStyle = null;
            _SheetStyleScale = 1f;
            if (Skin.StyleDB != null) { _SheetStyle = Skin.StyleDB.Row.First(); }
        }

        public ItemCollectionPad(string layoutID, Database database, long rowkey) : this(database.Layouts[database.Layouts.LayoutIDToIndex(layoutID)], string.Empty) {
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
                        _SheetStyleScale = float.Parse(pair.Value);
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
                        _SheetStyleScale = float.Parse(pair.Value);
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler DoInvalidate;

        #endregion

        #region Properties

        public Color BackColor { get; set; } = Color.White;

        [DefaultValue(10.0)]
        public float GridShow {
            get => _GridShow;
            set {
                if (_GridShow == value) { return; }
                _GridShow = value;
                OnDoInvalidate();
            }
        }

        [DefaultValue(10.0)]
        public float GridSnap {
            get => _Gridsnap;
            set {
                if (_Gridsnap == value) { return; }
                _Gridsnap = value;
                OnDoInvalidate();
            }
        }

        public bool IsParsing { get; private set; }

        [DefaultValue(true)]
        public bool IsSaved { get; set; }

        public System.Windows.Forms.Padding RandinMM {
            get => _RandinMM;
            set {
                _RandinMM = new System.Windows.Forms.Padding(Math.Max(0, value.Left), Math.Max(0, value.Top), Math.Max(0, value.Right), Math.Max(0, value.Bottom));
                GenPoints();
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

        [DefaultValue(1.0)]
        public float SheetStyleScale {
            get => _SheetStyleScale;
            set {
                if (value < 0.1d) { value = 0.1f; }
                if (_SheetStyleScale == value) { return; }
                _SheetStyleScale = value;
                DesignOrStyleChanged();
                OnDoInvalidate();
            }
        }

        [DefaultValue(false)]
        public enSnapMode SnapMode {
            get => _SnapMode;
            set {
                if (_SnapMode == value) { return; }
                _SnapMode = value;
                OnDoInvalidate();
            }
        }

        #endregion

        #region Indexers

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
                List<BasicPadItem> l = new();
                foreach (var ThisItem in this) {
                    if (ThisItem != null && ThisItem.Contains(p, 1)) {
                        l.Add(ThisItem);
                    }
                }
                return l;
            }
        }

        #endregion

        #region Methods

        public void DesignOrStyleChanged() {
            foreach (var thisItem in this) {
                thisItem?.DesignOrStyleChanged();
            }
            OnDoInvalidate();
        }

        public void DrawCreativePadToBitmap(Bitmap BMP, enStates vState, float zoomf, float X, float Y, List<BasicPadItem> visibleItems) {
            var gr = Graphics.FromImage(BMP);
            DrawCreativePadTo(gr, BMP.Size, vState, zoomf, X, Y, visibleItems, true);
            gr.Dispose();
        }

        public bool DrawItems(Graphics gr, float zoom, float shiftX, float shiftY, Size sizeOfParentControl, bool forPrinting, List<BasicPadItem> visibleItems) {
            try {
                if (SheetStyle == null || SheetStyleScale < 0.1d) { return true; }
                foreach (var thisItem in this) {
                    if (thisItem != null) {
                        gr.PixelOffsetMode = PixelOffsetMode.None;
                        if (visibleItems == null || visibleItems.Contains(thisItem)) {
                            thisItem.Draw(gr, zoom, shiftX, shiftY, sizeOfParentControl, forPrinting);
                        }
                    }
                }
                return true;
            } catch {
                Generic.CollectGarbage();
                return false;
            }
        }

        public override void OnChanged() {
            base.OnChanged();
            IsSaved = false;
            OnDoInvalidate();
        }

        public void OnDoInvalidate() => DoInvalidate?.Invoke(this, System.EventArgs.Empty);

        public bool ParseVariable(string name, string wert) => ParseVariable(null, new BlueScript.Variable(name, wert, Skript.Enums.enVariableDataType.String));

        public bool ParseVariable(BlueScript.Script s, BlueScript.Variable variable) {
            var did = false;
            foreach (var thisItem in this) {
                if (thisItem is ICanHaveColumnVariables variables) {
                    if (variables.ReplaceVariable(s, variable)) { did = true; }
                }
            }
            return did;
        }

        public void ParseVariable(RowItem row) {
            if (row != null) {
                (_, _, var script) = row.DoAutomatic("export");
                if (script == null) { return; }
                foreach (var thisV in script.Variablen) {
                    ParseVariable(script, thisV);
                }
            }
        }

        public void Remove(string internalname) => Remove(this[internalname]);

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

        public new void Swap(BasicPadItem item1, BasicPadItem item2) {
            var g1 = item1.Gruppenzugehörigkeit;
            item1.Gruppenzugehörigkeit = string.Empty;
            var g2 = item2.Gruppenzugehörigkeit;
            item2.Gruppenzugehörigkeit = string.Empty;

            base.Swap(item1, item2);

            item1.Gruppenzugehörigkeit = g1;
            item2.Gruppenzugehörigkeit = g2;
            OnDoInvalidate();
        }

        public Bitmap ToBitmap(float scale) {
            var r = MaxBounds(null);
            if (r.Width == 0) { return null; }

            Generic.CollectGarbage();

            do {
                if ((int)(r.Width * scale) > 15000) {
                    scale *= 0.8f;
                } else if ((int)(r.Height * scale) > 15000) {
                    scale *= 0.8f;
                } else if ((int)(r.Height * scale) * (int)(r.Height * scale) > 90000000) {
                    scale *= 0.8f;
                } else {
                    break;
                }
            } while (true);

            Bitmap I = new((int)(r.Width * scale), (int)(r.Height * scale));

            using (var gr = Graphics.FromImage(I)) {
                gr.Clear(BackColor);
                if (!DrawCreativePadTo(gr, I.Size, enStates.Standard, scale, r.Left * scale, r.Top * scale, null, true)) {
                    return ToBitmap(scale);
                }
            }
            return I;
        }

        public new string ToString() {
            var t = "{";
            if (!string.IsNullOrEmpty(ID)) { t = t + "ID=" + ID.ToNonCritical() + ", "; }
            if (!string.IsNullOrEmpty(Caption)) { t = t + "Caption=" + Caption.ToNonCritical() + ", "; }
            if (SheetStyle != null) { t = t + "Style=" + SheetStyle.CellFirstString().ToNonCritical() + ", "; }
            if (SheetStyleScale < 0.1d) { SheetStyleScale = 1.0f; }
            t = t + "BackColor=" + BackColor.ToArgb() + ", ";
            if (Math.Abs(SheetStyleScale - 1) > 0.001d) { t = t + "FontScale=" + SheetStyleScale + ", "; }
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

        internal bool DrawCreativePadTo(Graphics gr, Size sizeOfParentControl, enStates state, float zoom, float shiftX, float shiftY, List<BasicPadItem> visibleItems, bool showinprintmode) {
            try {
                gr.PixelOffsetMode = PixelOffsetMode.None;

                #region Hintergrund und evtl. Zeichenbereich

                if (SheetSizeInMM.Width > 0 && SheetSizeInMM.Height > 0) {
                    //Skin.Draw_Back(gr, enDesign.Table_And_Pad, state, DisplayRectangle, this, true);
                    var SSW = (float)Math.Round(Converter.mmToPixel(SheetSizeInMM.Width, DPI), 1);
                    var SSH = (float)Math.Round(Converter.mmToPixel(SheetSizeInMM.Height, DPI), 1);
                    var LO = new PointM(0f, 0f).ZoomAndMove(zoom, shiftX, shiftY);
                    var RU = new PointM(SSW, SSH).ZoomAndMove(zoom, shiftX, shiftY);

                    if (BackColor.A > 0) {
                        Rectangle R = new((int)LO.X, (int)LO.Y, (int)(RU.X - LO.X), (int)(RU.Y - LO.Y));
                        gr.FillRectangle(new SolidBrush(BackColor), R);
                    }

                    if (!showinprintmode) {
                        var rLO = new PointM(P_rLO.X, P_rLO.Y).ZoomAndMove(zoom, shiftX, shiftY);
                        var rRU = new PointM(P_rRU.X, P_rRU.Y).ZoomAndMove(zoom, shiftX, shiftY);
                        Rectangle Rr = new((int)rLO.X, (int)rLO.Y, (int)(rRU.X - rLO.X), (int)(rRU.Y - rLO.Y));
                        gr.DrawRectangle(ZoomPad.PenGray, Rr);
                    }
                } else {
                    if (BackColor.A > 0) {
                        gr.Clear(BackColor);
                    }
                }

                #endregion

                #region Grid

                if (_GridShow > 0.1) {
                    var po = new PointM(0, 0).ZoomAndMove(zoom, shiftX, shiftY);
                    var mo = Converter.mmToPixel(_GridShow, DPI) * zoom;

                    var p = new Pen(Color.FromArgb(10, 0, 0, 0));
                    float ex = 0;

                    for (var z = 0; z < 20; z++) {
                        if (mo < 5) { mo *= 2; }
                    }

                    if (mo >= 5) {
                        do {
                            gr.DrawLine(p, po.X + (int)ex, 0, po.X + (int)ex, sizeOfParentControl.Height);
                            gr.DrawLine(p, 0, po.Y + (int)ex, sizeOfParentControl.Width, po.Y + (int)ex);

                            if (ex > 0) {
                                // erste Linie nicht doppelt zeichnen
                                gr.DrawLine(p, po.X - (int)ex, 0, po.X - (int)ex, sizeOfParentControl.Height);
                                gr.DrawLine(p, 0, po.Y - (int)ex, sizeOfParentControl.Width, po.Y - (int)ex);
                            }
                            ex += mo;
                            if (po.X - ex < 0 && po.Y - ex < 0 && po.X + ex > sizeOfParentControl.Width && po.Y + ex > sizeOfParentControl.Height) {
                                break;
                            }
                        } while (true);
                    }
                }

                #endregion

                #region Items selbst

                if (!DrawItems(gr, zoom, shiftX, shiftY, sizeOfParentControl, showinprintmode, visibleItems)) {
                    return DrawCreativePadTo(gr, sizeOfParentControl, state, zoom, shiftX, shiftY, visibleItems, showinprintmode);
                }

                #endregion
            } catch {
                return DrawCreativePadTo(gr, sizeOfParentControl, state, zoom, shiftX, shiftY, visibleItems, showinprintmode);
            }
            return true;
        }

        internal Rectangle DruckbereichRect() => P_rLO == null ? new Rectangle(0, 0, 0, 0) : new Rectangle((int)P_rLO.X, (int)P_rLO.Y, (int)(P_rRU.X - P_rLO.X), (int)(P_rRU.Y - P_rLO.Y));

        internal void InDenHintergrund(BasicPadItem ThisItem) {
            if (IndexOf(ThisItem) == 0) { return; }
            var g1 = ThisItem.Gruppenzugehörigkeit;
            ThisItem.Gruppenzugehörigkeit = string.Empty;
            Remove(ThisItem);
            Insert(0, ThisItem);
            ThisItem.Gruppenzugehörigkeit = g1;
            OnDoInvalidate();
        }

        internal void InDenVordergrund(BasicPadItem ThisItem) {
            if (IndexOf(ThisItem) == Count - 1) { return; }
            var g1 = ThisItem.Gruppenzugehörigkeit;
            ThisItem.Gruppenzugehörigkeit = string.Empty;
            Remove(ThisItem);
            Add(ThisItem);
            ThisItem.Gruppenzugehörigkeit = g1;
            OnDoInvalidate();
        }

        internal RectangleF MaxBounds(List<BasicPadItem>? ZoomItems) {
            var r = Count == 0 ? new RectangleF(0, 0, 0, 0) : MaximumBounds(ZoomItems);
            if (SheetSizeInMM.Width > 0 && SheetSizeInMM.Height > 0) {
                var X1 = Math.Min(r.Left, 0);
                var y1 = Math.Min(r.Top, 0);
                var x2 = Math.Max(r.Right, Converter.mmToPixel(SheetSizeInMM.Width, DPI));
                var y2 = Math.Max(r.Bottom, Converter.mmToPixel(SheetSizeInMM.Height, DPI));
                return new RectangleF(X1, y1, x2 - X1, y2 - y1);
            }
            return r;
        }

        protected RectangleF MaxBounds() => MaxBounds(null);

        protected override void OnItemAdded(BasicPadItem item) {
            if (item == null) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Null Item soll hinzugefügt werden!");
            }
            if (string.IsNullOrEmpty(item.Internal)) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Der Auflistung soll ein Item hinzugefügt werden, welches keinen Namen hat " + item.Internal);
            }

            item.Parent = this;
            base.OnItemAdded(item);

            IsSaved = false;

            OnDoInvalidate();
        }

        protected override void OnItemRemoved() {
            base.OnItemRemoved();
            OnDoInvalidate();
        }

        protected override void OnItemRemoving(BasicPadItem item) {
            base.OnItemRemoving(item);
            OnDoInvalidate();
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
            var SSW = (float)Math.Round(Converter.mmToPixel(_SheetSizeInMM.Width, DPI), 1);
            var SSH = (float)Math.Round(Converter.mmToPixel(_SheetSizeInMM.Height, DPI), 1);
            var rr = (float)Math.Round(Converter.mmToPixel(_RandinMM.Right, DPI), 1);
            var rl = (float)Math.Round(Converter.mmToPixel(_RandinMM.Left, DPI), 1);
            var ro = (float)Math.Round(Converter.mmToPixel(_RandinMM.Top, DPI), 1);
            var ru = (float)Math.Round(Converter.mmToPixel(_RandinMM.Bottom, DPI), 1);
            P_rLO.SetTo(rl, ro);
            P_rRO.SetTo(SSW - rr, ro);
            P_rRU.SetTo(SSW - rr, SSH - ru);
            P_rLU.SetTo(rl, SSH - ru);
            OnDoInvalidate();
            OnDoInvalidate();
        }

        private RectangleF MaximumBounds(List<BasicPadItem>? zoomItems) {
            if (zoomItems is null) { return RectangleF.Empty; }
            var x1 = float.MaxValue;
            var y1 = float.MaxValue;
            var x2 = float.MinValue;
            var y2 = float.MinValue;
            var Done = false;
            foreach (var ThisItem in this) {
                if (ThisItem != null) {
                    if (zoomItems == null || zoomItems.Contains(ThisItem)) {
                        var UA = ThisItem.ZoomToArea();
                        x1 = Math.Min(x1, UA.Left);
                        y1 = Math.Min(y1, UA.Top);
                        x2 = Math.Max(x2, UA.Right);
                        y2 = Math.Max(y2, UA.Bottom);
                        Done = true;
                    }
                }
            }
            return !Done ? RectangleF.Empty : new RectangleF(x1, y1, x2 - x1, y2 - y1);
        }

        private void ParseItems(string ToParse) {
            //ToParse = ToParse.Replace("}, Item={ClassID=TEXT", ", Item={ClassID=TEXT");
            //ToParse = ToParse.Replace(", Item={ClassID=TEXT", "}, Item={ClassID=TEXT");
            //ToParse = ToParse.Replace(", }", "}");
            //var tmp = ToParse.IndexOf("Item=");

            //ToParse = "{" + ToParse.Substring(tmp);

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
                    //    _SheetStyleScale = float.Parse(pair.Value);
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
                        var i = BasicPadItem.NewByParsing(pair.Value);
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
                        //_SheetStyleScale = float.Parse(pair.Value);
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        #endregion
    }
}