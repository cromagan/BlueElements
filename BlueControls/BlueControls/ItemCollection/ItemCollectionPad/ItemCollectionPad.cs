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
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace BlueControls.ItemCollection {

    public class ItemCollectionPad : ListExt<BasicPadItem> {

        #region Fields

        public static readonly int Dpi = 300;
        public string Caption;
        public string Id;
        public PointM? PRLo;
        public PointM? PRLu;
        public PointM? PRRo;
        public PointM? PRRu;

        /// <summary>
        /// Für automatische Generierungen, die zu schnell hintereinander kommen, ein Counter für den Dateinamen
        /// </summary>
        private readonly int _idCount;

        private float _gridShow = 10;
        private float _gridsnap = 1;
        private System.Windows.Forms.Padding _randinMm = System.Windows.Forms.Padding.Empty;
        private SizeF _sheetSizeInMm = SizeF.Empty;
        private RowItem? _sheetStyle;
        private float _sheetStyleScale;
        private enSnapMode _snapMode = enSnapMode.SnapToGrid;

        #endregion

        #region Constructors

        public ItemCollectionPad() : base() {
            if (Skin.StyleDb == null) { Skin.InitStyles(); }
            SheetSizeInMm = Size.Empty;
            RandinMm = System.Windows.Forms.Padding.Empty;
            Caption = "";
            _idCount++;
            Id = "#" + DateTime.UtcNow.ToString(Constants.Format_Date) + _idCount; // # ist die erkennung, dass es kein Dateiname sondern ein Item ist
            if (Skin.StyleDb == null) { Skin.InitStyles(); }
            _sheetStyle = null;
            _sheetStyleScale = 1f;
            if (Skin.StyleDb != null) { _sheetStyle = Skin.StyleDb.Row.First(); }
        }

        public ItemCollectionPad(string layoutId, Database? database, long rowkey) : this(database.Layouts[database.Layouts.LayoutIDToIndex(layoutId)], string.Empty) {
            // Wenn nur die Row ankommt und diese null ist, kann gar nix generiert werden
            ResetVariables();
            ParseVariable(database.Row.SearchByKey(rowkey));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="needPrinterData"></param>
        /// <param name="useThisId">Wenn das Blatt bereits eine Id hat, muss die Id verwendet werden. Wird das Feld leer gelassen, wird die beinhaltete Id benutzt.</param>
        public ItemCollectionPad(string toParse, string useThisId) : this() {
            if (string.IsNullOrEmpty(toParse) || toParse.Length < 3) { return; }
            if (toParse.Substring(0, 1) != "{") { return; }// Alte Daten gehen eben verloren.
            Id = useThisId;
            foreach (var pair in toParse.GetAllTags()) {
                switch (pair.Key.ToLower()) {
                    case "sheetsize":
                        _sheetSizeInMm = pair.Value.SizeFParse();
                        GenPoints();
                        break;

                    case "printarea":
                        _randinMm = pair.Value.PaddingParse();
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
                        if (string.IsNullOrEmpty(Id)) { Id = pair.Value.FromNonCritical(); }
                        break;

                    case "style":
                        _sheetStyle = Skin.StyleDb.Row[pair.Value];
                        if (_sheetStyle == null) { _sheetStyle = Skin.StyleDb.Row.First(); }// Einfach die Erste nehmen
                        break;

                    case "fontscale":
                        _sheetStyleScale = float.Parse(pair.Value);
                        break;

                    case "snapmode":
                        _snapMode = (enSnapMode)int.Parse(pair.Value);
                        break;

                    case "grid":
                        //_Grid = pair.Value.FromPlusMinus();
                        break;

                    case "gridshow":
                        _gridShow = float.Parse(pair.Value);
                        break;

                    case "gridsnap":
                        _gridsnap = float.Parse(pair.Value);
                        break;

                    case "format": //_Format = DirectCast(Integer.Parse(pair.Value.Value), enDataFormat)
                        break;

                    case "items":
                        ParseItems(pair.Value);
                        break;

                    case "dpi":
                        if (int.Parse(pair.Value) != Dpi) {
                            Develop.DebugPrint("Dpi Unterschied: " + Dpi + " <> " + pair.Value);
                        }
                        break;

                    case "sheetstyle":
                        if (Skin.StyleDb == null) { Skin.InitStyles(); }
                        _sheetStyle = Skin.StyleDb.Row[pair.Value];
                        break;

                    case "sheetstylescale":
                        _sheetStyleScale = float.Parse(pair.Value);
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
            get => _gridShow;
            set {
                if (_gridShow == value) { return; }
                _gridShow = value;
                OnDoInvalidate();
            }
        }

        [DefaultValue(10.0)]
        public float GridSnap {
            get => _gridsnap;
            set {
                if (_gridsnap == value) { return; }
                _gridsnap = value;
                OnDoInvalidate();
            }
        }

        [DefaultValue(true)]
        public bool IsSaved { get; set; }

        public System.Windows.Forms.Padding RandinMm {
            get => _randinMm;
            set {
                _randinMm = new System.Windows.Forms.Padding(Math.Max(0, value.Left), Math.Max(0, value.Top), Math.Max(0, value.Right), Math.Max(0, value.Bottom));
                GenPoints();
            }
        }

        public SizeF SheetSizeInMm {
            get => _sheetSizeInMm;
            set {
                if (value == _sheetSizeInMm) { return; }
                _sheetSizeInMm = new SizeF(value.Width, value.Height);
                GenPoints();
            }
        }

        public RowItem? SheetStyle {
            get => _sheetStyle;
            set {
                if (_sheetStyle == value) { return; }
                //        if (!_isParsing && value == SheetStyle) { return; }
                //if (Skin.StyleDB == null) { Skin.InitStyles(); }
                /// /       Item.SheetStyle = Skin.StyleDB.Row[value];
                //   if (Item.SheetStyle == null) { Item.SheetStyle = Skin.StyleDB.Row.First(); }// Einfach die Erste nehmen
                _sheetStyle = value;
                DesignOrStyleChanged();
                //RepairAll(0, false);
                OnDoInvalidate();
            }
        }

        [DefaultValue(1.0)]
        public float SheetStyleScale {
            get => _sheetStyleScale;
            set {
                if (value < 0.1d) { value = 0.1f; }
                if (_sheetStyleScale == value) { return; }
                _sheetStyleScale = value;
                DesignOrStyleChanged();
                OnDoInvalidate();
            }
        }

        [DefaultValue(false)]
        public enSnapMode SnapMode {
            get => _snapMode;
            set {
                if (_snapMode == value) { return; }
                _snapMode = value;
                OnDoInvalidate();
            }
        }

        #endregion

        #region Indexers

        public BasicPadItem? this[string @internal] {
            get {
                if (string.IsNullOrEmpty(@internal)) {
                    return null;
                }

                return this.FirstOrDefault(thisItem => thisItem != null && string.Equals(@internal, thisItem.Internal, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        public List<BasicPadItem> this[int x, int y] => this[new Point(x, y)];

        public List<BasicPadItem> this[Point p] => this.Where(thisItem => thisItem != null && thisItem.Contains(p, 1)).ToList();

        #endregion

        #region Methods

        public void DesignOrStyleChanged() {
            foreach (var thisItem in this) {
                thisItem?.DesignOrStyleChanged();
            }
            OnDoInvalidate();
        }

        public void DrawCreativePadToBitmap(Bitmap? bmp, enStates vState, float zoomf, float x, float y, List<BasicPadItem>? visibleItems) {
            var gr = Graphics.FromImage(bmp);
            DrawCreativePadTo(gr, bmp.Size, vState, zoomf, x, y, visibleItems, true);
            gr.Dispose();
        }

        public bool DrawItems(Graphics gr, float zoom, float shiftX, float shiftY, Size sizeOfParentControl, bool forPrinting, List<BasicPadItem>? visibleItems) {
            try {
                if (SheetStyle == null || SheetStyleScale < 0.1d) { return true; }
                foreach (var thisItem in this.Where(thisItem => thisItem != null)) {
                    gr.PixelOffsetMode = PixelOffsetMode.None;
                    if (visibleItems == null || visibleItems.Contains(thisItem)) {
                        thisItem.Draw(gr, zoom, shiftX, shiftY, sizeOfParentControl, forPrinting);
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

        public bool ParseVariable(BlueScript.Script? s, BlueScript.Variable variable) {
            var did = false;
            foreach (var thisItem in this) {
                if (thisItem is ICanHaveColumnVariables variables) {
                    if (variables.ReplaceVariable(s, variable)) { did = true; }
                }
            }
            return did;
        }

        public void ParseVariable(RowItem? row) {
            if (row != null) {
                var (_, _, script) = row.DoAutomatic("export");
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
            foreach (var thisToo in this.Where(thisToo => item.Gruppenzugehörigkeit.ToLower() == thisToo.Gruppenzugehörigkeit?.ToLower())) {
                Remove(thisToo);
                return; // Wird eh eine Kettenreaktion ausgelöst -  und der Iteraor hier wird beschädigt
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

                case "Bmp":
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

        public Bitmap? ToBitmap(float scale) {
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

            using var gr = Graphics.FromImage(I);
            gr.Clear(BackColor);
            if (!DrawCreativePadTo(gr, I.Size, enStates.Standard, scale, r.Left * scale, r.Top * scale, null, true)) {
                return ToBitmap(scale);
            }

            return I;
        }

        public new string ToString() {
            var t = "{";
            if (!string.IsNullOrEmpty(Id)) { t = t + "ID=" + Id.ToNonCritical() + ", "; }
            if (!string.IsNullOrEmpty(Caption)) { t = t + "Caption=" + Caption.ToNonCritical() + ", "; }
            if (SheetStyle != null) { t = t + "Style=" + SheetStyle.CellFirstString().ToNonCritical() + ", "; }
            if (SheetStyleScale < 0.1d) { SheetStyleScale = 1.0f; }
            t = t + "BackColor=" + BackColor.ToArgb() + ", ";
            if (Math.Abs(SheetStyleScale - 1) > 0.001d) { t = t + "FontScale=" + SheetStyleScale + ", "; }
            if (SheetSizeInMm.Width > 0 && SheetSizeInMm.Height > 0) {
                t = t + "SheetSize=" + SheetSizeInMm + ", ";
                t = t + "PrintArea=" + RandinMm + ", ";
            }
            //t = t + "Dpi=" + Dpi + ", "; // TODO: Nach Update wieder aktivieren
            t += "Items={";
            foreach (var thisitem in this) {
                if (thisitem != null) {
                    t = t + "Item=" + thisitem + ", ";
                }
            }
            t = t.TrimEnd(", ") + "}, ";
            t = t + "SnapMode=" + ((int)_snapMode) + ", ";
            t = t + "GridShow=" + _gridShow + ", ";
            t = t + "GridSnap=" + _gridsnap + ", ";
            return t.TrimEnd(", ") + "}";
        }

        internal bool DrawCreativePadTo(Graphics gr, Size sizeOfParentControl, enStates state, float zoom, float shiftX, float shiftY, List<BasicPadItem>? visibleItems, bool showinprintmode) {
            try {
                gr.PixelOffsetMode = PixelOffsetMode.None;

                #region Hintergrund und evtl. Zeichenbereich

                if (SheetSizeInMm.Width > 0 && SheetSizeInMm.Height > 0) {
                    //Skin.Draw_Back(gr, enDesign.Table_And_Pad, state, DisplayRectangle, this, true);
                    var ssw = (float)Math.Round(Converter.mmToPixel(SheetSizeInMm.Width, Dpi), 1);
                    var ssh = (float)Math.Round(Converter.mmToPixel(SheetSizeInMm.Height, Dpi), 1);
                    var lo = new PointM(0f, 0f).ZoomAndMove(zoom, shiftX, shiftY);
                    var ru = new PointM(ssw, ssh).ZoomAndMove(zoom, shiftX, shiftY);

                    if (BackColor.A > 0) {
                        Rectangle r = new((int)lo.X, (int)lo.Y, (int)(ru.X - lo.X), (int)(ru.Y - lo.Y));
                        gr.FillRectangle(new SolidBrush(BackColor), r);
                    }

                    if (!showinprintmode) {
                        var rLo = new PointM(PRLo.X, PRLo.Y).ZoomAndMove(zoom, shiftX, shiftY);
                        var rRu = new PointM(PRRu.X, PRRu.Y).ZoomAndMove(zoom, shiftX, shiftY);
                        Rectangle rr = new((int)rLo.X, (int)rLo.Y, (int)(rRu.X - rLo.X), (int)(rRu.Y - rLo.Y));
                        gr.DrawRectangle(ZoomPad.PenGray, rr);
                    }
                } else {
                    if (BackColor.A > 0) {
                        gr.Clear(BackColor);
                    }
                }

                #endregion

                #region Grid

                if (_gridShow > 0.1) {
                    var po = new PointM(0, 0).ZoomAndMove(zoom, shiftX, shiftY);
                    var mo = Converter.mmToPixel(_gridShow, Dpi) * zoom;

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

        internal Rectangle DruckbereichRect() => PRLo == null ? new Rectangle(0, 0, 0, 0) : new Rectangle((int)PRLo.X, (int)PRLo.Y, (int)(PRRu.X - PRLo.X), (int)(PRRu.Y - PRLo.Y));

        internal void InDenHintergrund(BasicPadItem thisItem) {
            if (IndexOf(thisItem) == 0) { return; }
            var g1 = thisItem.Gruppenzugehörigkeit;
            thisItem.Gruppenzugehörigkeit = string.Empty;
            Remove(thisItem);
            Insert(0, thisItem);
            thisItem.Gruppenzugehörigkeit = g1;
            OnDoInvalidate();
        }

        internal void InDenVordergrund(BasicPadItem thisItem) {
            if (IndexOf(thisItem) == Count - 1) { return; }
            var g1 = thisItem.Gruppenzugehörigkeit;
            thisItem.Gruppenzugehörigkeit = string.Empty;
            Remove(thisItem);
            Add(thisItem);
            thisItem.Gruppenzugehörigkeit = g1;
            OnDoInvalidate();
        }

        internal RectangleF MaxBounds(List<BasicPadItem>? zoomItems) {
            var r = Count == 0 ? new RectangleF(0, 0, 0, 0) : MaximumBounds(zoomItems);
            if (SheetSizeInMm.Width > 0 && SheetSizeInMm.Height > 0) {
                var x1 = Math.Min(r.Left, 0);
                var y1 = Math.Min(r.Top, 0);
                var x2 = Math.Max(r.Right, Converter.mmToPixel(SheetSizeInMm.Width, Dpi));
                var y2 = Math.Max(r.Bottom, Converter.mmToPixel(SheetSizeInMm.Height, Dpi));
                return new RectangleF(x1, y1, x2 - x1, y2 - y1);
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
            if (Math.Abs(_sheetSizeInMm.Width) < 0.001 || Math.Abs(_sheetSizeInMm.Height) < 0.001) {
                if (PRLo != null) {
                    PRLo.Parent = null;
                    PRLo = null;
                    PRRo.Parent = null;
                    PRRo = null;
                    PRRu.Parent = null;
                    PRRu = null;
                    PRLu.Parent = null;
                    PRLu = null;
                }
                return;
            }
            if (PRLo == null) {
                PRLo = new PointM(this, "Druckbereich LO", 0, 0);
                PRRo = new PointM(this, "Druckbereich RO", 0, 0);
                PRRu = new PointM(this, "Druckbereich RU", 0, 0);
                PRLu = new PointM(this, "Druckbereich LU", 0, 0);
            }
            var ssw = (float)Math.Round(Converter.mmToPixel(_sheetSizeInMm.Width, Dpi), 1);
            var ssh = (float)Math.Round(Converter.mmToPixel(_sheetSizeInMm.Height, Dpi), 1);
            var rr = (float)Math.Round(Converter.mmToPixel(_randinMm.Right, Dpi), 1);
            var rl = (float)Math.Round(Converter.mmToPixel(_randinMm.Left, Dpi), 1);
            var ro = (float)Math.Round(Converter.mmToPixel(_randinMm.Top, Dpi), 1);
            var ru = (float)Math.Round(Converter.mmToPixel(_randinMm.Bottom, Dpi), 1);
            PRLo.SetTo(rl, ro);
            PRRo.SetTo(ssw - rr, ro);
            PRRu.SetTo(ssw - rr, ssh - ru);
            PRLu.SetTo(rl, ssh - ru);
            OnDoInvalidate();
            OnDoInvalidate();
        }

        private RectangleF MaximumBounds(List<BasicPadItem>? zoomItems) {
            if (zoomItems is null) { return RectangleF.Empty; }
            var x1 = float.MaxValue;
            var y1 = float.MaxValue;
            var x2 = float.MinValue;
            var y2 = float.MinValue;
            var done = false;
            foreach (var thisItem in this) {
                if (thisItem != null) {
                    if (zoomItems == null || zoomItems.Contains(thisItem)) {
                        var ua = thisItem.ZoomToArea();
                        x1 = Math.Min(x1, ua.Left);
                        y1 = Math.Min(y1, ua.Top);
                        x2 = Math.Max(x2, ua.Right);
                        y2 = Math.Max(y2, ua.Bottom);
                        done = true;
                    }
                }
            }
            return !done ? RectangleF.Empty : new RectangleF(x1, y1, x2 - x1, y2 - y1);
        }

        private void ParseItems(string toParse) {
            //ToParse = ToParse.Replace("}, Item={ClassID=TEXT", ", Item={ClassID=TEXT");
            //ToParse = ToParse.Replace(", Item={ClassID=TEXT", "}, Item={ClassID=TEXT");
            //ToParse = ToParse.Replace(", }", "}");
            //var tmp = ToParse.IndexOf("Item=");

            //ToParse = "{" + ToParse.Substring(tmp);

            foreach (var pair in toParse.GetAllTags()) {
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
                        if (int.Parse(pair.Value) != Dpi) {
                            Develop.DebugPrint("Dpi Unterschied: " + Dpi + " <> " + pair.Value);
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