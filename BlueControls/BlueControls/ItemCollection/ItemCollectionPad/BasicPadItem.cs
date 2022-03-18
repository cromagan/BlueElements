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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection {

    public abstract class BasicPadItem : IParseable, ICloneable, IChangedFeedback, IMoveable {

        #region Fields

        public readonly ListExt<ItemConnection> ConnectsTo = new();
        public readonly ListExt<PointM> MovablePoint = new();

        public readonly List<PointM> PointsForSuccesfullyMove = new();

        public List<FlexiControl>? AdditionalStyleOptions = null;
        private static int _uniqueInternalCount;

        private static string _uniqueInternalLastTime = "InitialDummy";

        /// <summary>
        /// Soll es gedruckt werden?
        /// </summary>
        /// <remarks></remarks>
        private bool _beiExportSichtbar = true;

        private ItemCollectionPad _parent;

        private PadStyles _style = PadStyles.Style_Standard;

        private RectangleF _usedArea;
        private int _zoomPadding;

        #endregion

        #region Constructors

        protected BasicPadItem(string internalname) {
            Internal = string.IsNullOrEmpty(internalname) ? UniqueInternal() : internalname;
            if (string.IsNullOrEmpty(Internal)) { Develop.DebugPrint(FehlerArt.Fehler, "Interner Name nicht vergeben."); }
            MovablePoint.ItemAdded += Points_ItemAdded;
            MovablePoint.ItemRemoving += Points_ItemRemoving;
        }

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        [Description("Wird bei einem Export (wie z. B. Drucken) nur angezeigt, wenn das Häkchen gesetzt ist.")]
        public bool Bei_Export_Sichtbar {
            get => _beiExportSichtbar;
            set {
                if (_beiExportSichtbar == value) { return; }
                _beiExportSichtbar = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Wird ein Element gelöscht, das diese Feld befüllt hat, werden automatisch alle andern Elemente mit der selben Gruppe gelöscht.
        /// </summary>
        [Description("Alle Elemente, die der selben Gruppe angehören, werden beim Löschen eines Elements ebenfalls gelöscht.")]
        public string Gruppenzugehörigkeit { get; set; } = string.Empty;

        public string Internal { get; }

        public bool IsParsing { get; private set; }

        public ItemCollectionPad Parent {
            get => _parent;
            set {
                if (_parent == null || _parent == value) {
                    _parent = value;
                    return;
                }

                Develop.DebugPrint(FehlerArt.Fehler, "Parent Fehler!");
            }
        }

        public virtual string QuickInfo { get; set; } = string.Empty;

        public PadStyles Stil {
            get => _style;
            set {
                if (_style == value) { return; }
                _style = value;
                DesignOrStyleChanged();
            }
        }

        /// <summary>
        /// Falls eine Spezielle Information gespeichert und zurückgegeben werden soll
        /// </summary>
        /// <remarks></remarks>
        public List<string> Tags { get; } = new();

        /// <summary>
        /// Gibt den Bereich zurück, den das Element benötigt, um komplett dargestellt zu werden. Unabhängig von der aktuellen Ansicht.
        /// nicht berücksichtigt werden z.b. Verbindungslinien zu anderen Objekten
        /// </summary>
        /// <remarks></remarks>
        public RectangleF UsedArea {
            get {
                if (_usedArea.IsEmpty) { _usedArea = CalculateUsedArea(); }
                return _usedArea;
            }
        }

        public int ZoomPadding {
            get => _zoomPadding;
            set {
                if (_zoomPadding == value) { return; }
                _zoomPadding = value;
                OnChanged();
            }
        }

        #endregion

        #region Methods

        public static BasicPadItem? NewByParsing(string code) {
            BasicPadItem? i = null;
            var x = code.GetAllTags();
            var ding = string.Empty;
            var name = string.Empty;
            foreach (var thisIt in x) {
                switch (thisIt.Key) {
                    case "type":

                    case "classid":
                        ding = thisIt.Value;
                        break;

                    case "internalname":
                        name = thisIt.Value;
                        break;
                }
            }
            if (string.IsNullOrEmpty(ding)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Itemtyp unbekannt: " + code);
                return null;
            }
            if (string.IsNullOrEmpty(name)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Itemname unbekannt: " + code);
                return null;
            }

            if (ItemCollectionPad.PadItemTypes != null) {
                foreach (var thisType in ItemCollectionPad.PadItemTypes) {
                    i = thisType.TryParse(ding, name, x);
                    if (i != null) { return i; }
                }
            }

            return null;
        }

        public static string UniqueInternal() {
            var neueZeit = DateTime.UtcNow.ToString(Constants.Format_Date7).ReduceToChars(Constants.Char_Numerals);
            if (neueZeit == _uniqueInternalLastTime) {
                _uniqueInternalCount++;
            } else {
                _uniqueInternalCount = 0;
                _uniqueInternalLastTime = neueZeit;
            }
            return "Auto " + neueZeit + " IDX" + _uniqueInternalCount;
        }

        public object? Clone() => NewByParsing(ToString());

        /// <summary>
        /// Prüft, ob die angegebenen Koordinaten das Element berühren.
        /// Der Zoomfaktor wird nur benötigt, um Maßstabsunabhängige Punkt oder Linienberührungen zu berechnen.
        /// </summary>
        /// <remarks></remarks>
        public virtual bool Contains(PointF value, float zoomfactor) {
            var tmp = UsedArea; // Umwandlung, um den Bezug zur Klasse zu zerstören
            var ne = -5 / zoomfactor + 1;
            tmp.Inflate(ne, ne);
            return tmp.Contains(value);
        }

        public virtual void DesignOrStyleChanged() { }

        public void Draw(Graphics gr, float zoom, float shiftX, float shiftY, Size sizeOfParentControl, bool forPrinting) {
            if (_parent == null) { Develop.DebugPrint(FehlerArt.Fehler, "Parent nicht definiert"); }

            if (forPrinting && !_beiExportSichtbar) { return; }

            var drawingCoordinates = UsedArea.ZoomAndMoveRect(zoom, shiftX, shiftY, false);

            if (IsInDrawingArea(drawingCoordinates, sizeOfParentControl)) {
                DrawExplicit(gr, drawingCoordinates, zoom, shiftX, shiftY, forPrinting);

                if (!_beiExportSichtbar) {
                    gr.DrawImage(QuickImage.Get("Drucker|16||1"), drawingCoordinates.X, drawingCoordinates.Y);
                }
            }

            #region Verknüpfte Pfeile Zeichnen

            var line = 1f;
            if (zoom > 1) { line = zoom; }
            foreach (var thisV in ConnectsTo) {
                if (Parent.Contains(thisV.OtherItem) && thisV != null && thisV.OtherItem != this) {
                    var t1 = ItemConnection.GetConnectionPoint(this, thisV.MyItemType, thisV.OtherItem).ZoomAndMove(zoom, shiftX, shiftY);
                    var t2 = ItemConnection.GetConnectionPoint(thisV.OtherItem, thisV.OtherItemType, this).ZoomAndMove(zoom, shiftX, shiftY);

                    if (Geometry.GetLenght(t1, t2) > 1) {
                        gr.DrawLine(new Pen(Color.Gray, line), t1, t2);
                        var wi = Geometry.Winkel(t1, t2);
                        if (thisV.ArrowOnMyItem) { DimensionPadItem.DrawArrow(gr, t1, wi, Color.Gray, zoom * 20); }
                        if (thisV.ArrowOnOtherItem) { DimensionPadItem.DrawArrow(gr, t2, wi + 180, Color.Gray, zoom * 20); }
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Zeichnet die UsedArea. mehr für Debugging gedacht.
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="zoom"></param>
        /// <param name="shiftX"></param>
        /// <param name="shiftY"></param>
        /// <param name="c"></param>
        public void DrawOutline(Graphics gr, float zoom, float shiftX, float shiftY, Color c) => gr.DrawRectangle(new Pen(c), UsedArea.ZoomAndMoveRect(zoom, shiftX, shiftY, false));

        public void EineEbeneNachHinten() {
            if (_parent == null) { return; }
            var i2 = Previous();
            if (i2 != null) {
                var tempVar = this;
                _parent.Swap(tempVar, i2);
            }
        }

        public void EineEbeneNachVorne() {
            if (_parent == null) { return; }
            var i2 = Next();
            if (i2 != null) {
                var tempVar = this;
                _parent.Swap(tempVar, i2);
            }
        }

        /// <summary>
        /// Gibt für das aktuelle Item das "Kontext-Menü" zurück.
        /// </summary>
        /// <returns></returns>
        public virtual List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new()
            {
                new FlexiControl(),
                new FlexiControlForProperty(this, "Gruppenzugehörigkeit"),
                new FlexiControlForProperty(this, "Bei_Export_sichtbar")
            };
            if (AdditionalStyleOptions != null) {
                l.Add(new FlexiControl());
                l.AddRange(AdditionalStyleOptions);
            }

            return l;
        }

        public void InDenHintergrund() => _parent?.InDenHintergrund(this);

        public void InDenVordergrund() => _parent?.InDenVordergrund(this);

        public void Move(float x, float y) {
            if (x == 0 && y == 0) { return; }
            foreach (var t in PointsForSuccesfullyMove) {
                t.Move(x, y);
            }
            OnChanged();
        }

        /// <summary>
        /// Invalidiert UsedArea und löst das Ereignis Changed aus
        /// </summary>
        public void OnChanged() {
            //if (this is IParseable O && O.IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            _usedArea = default;
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        public void Parse(List<KeyValuePair<string, string>> toParse) {
            IsParsing = true;
            foreach (var pair in toParse.Where(pair => !ParseThis(pair.Key, pair.Value))) {
                Develop.DebugPrint(FehlerArt.Warnung, "Kann nicht geparsed werden: " + pair.Key + "/" + pair.Value + "/" + toParse);
            }

            ParseFinished();
            IsParsing = false;
        }

        public void Parse(string toParse) => Parse(toParse.GetAllTags());

        public virtual bool ParseThis(string tag, string value) {
            switch (tag.ToLower()) {
                case "classid":

                case "type":

                case "enabled":

                case "checked":
                    return true;

                case "tag":
                    Tags.Add(value.FromNonCritical());
                    return true;

                case "print":
                    _beiExportSichtbar = value.FromPlusMinus();
                    return true;

                case "point":

                    foreach (var thisPoint in MovablePoint.Where(thisPoint => value.Contains("Name=" + thisPoint.Name + ","))) {
                        thisPoint.Parse(value);
                    }
                    return true;

                case "format": // = Textformat!!!

                case "design":

                case "style":
                    _style = (PadStyles)IntParse(value);
                    return true;

                case "removetoo": // TODO: Alt, löschen, 02.03.2020
                    //RemoveToo.AddRange(value.FromNonCritical().SplitAndCutByCr());
                    return true;

                case "removetoogroup":
                    Gruppenzugehörigkeit = value.FromNonCritical();
                    return true;

                case "internalname":
                    if (value != Internal) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Namen unterschiedlich: " + value + " / " + Internal);
                    }
                    return true;

                case "zoompadding":
                    _zoomPadding = IntParse(value);
                    return true;

                case "quickinfo":
                    QuickInfo = value.FromNonCritical();
                    return true;

                default:
                    return false;
            }
        }

        public virtual void PointMoved(object sender, MoveEventArgs e) => OnChanged();

        public virtual void PointMoving(object sender, MoveEventArgs e) { }

        public override string ToString() {
            var t = "{";
            t = t + "ClassID=" + ClassId() + ", ";
            t = t + "InternalName=" + Internal.ToNonCritical() + ", ";
            if (Tags.Count > 0) {
                foreach (var thisTag in Tags) {
                    t = t + "Tag=" + thisTag.ToNonCritical() + ", ";
                }
            }
            t = t + "Style=" + (int)_style + ", ";
            t = t + "Print=" + _beiExportSichtbar.ToPlusMinus() + ", ";
            t = t + "QuickInfo=" + QuickInfo.ToNonCritical() + ", ";
            if (_zoomPadding != 0) {
                t = t + "ZoomPadding=" + _zoomPadding + ", ";
            }
            foreach (var thisPoint in MovablePoint) {
                t = t + "Point=" + thisPoint + ", ";
            }
            if (!string.IsNullOrEmpty(Gruppenzugehörigkeit)) {
                t = t + "RemoveTooGroup=" + Gruppenzugehörigkeit.ToNonCritical() + ", ";
            }
            return t.Trim(", ") + "}";
        }

        /// <summary>
        /// Gibt den Bereich zurück, den das Element benötigt, um komplett dargestellt zu werden. Unabhängig von der aktuellen Ansicht. Zusätzlich mit dem Wert aus Padding.
        /// </summary>
        /// <remarks></remarks>
        public RectangleF ZoomToArea() {
            var x = UsedArea;
            if (_zoomPadding == 0) { return x; }
            x.Inflate(-ZoomPadding, -ZoomPadding);
            return x;
        }

        internal void AddLineStyleOption(List<FlexiControl> l) => l.Add(new FlexiControlForProperty(this, "Stil", Skin.GetRahmenArt(_parent.SheetStyle, true)));

        internal void AddStyleOption(List<FlexiControl> l) => l.Add(new FlexiControlForProperty(this, "Stil", Skin.GetFonts(_parent.SheetStyle)));

        internal BasicPadItem? Next() {
            var itemCount = _parent.IndexOf(this);
            if (itemCount < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }
            do {
                itemCount++;
                if (itemCount >= _parent.Count) { return null; }
                if (_parent[itemCount] != null) { return _parent[itemCount]; }
            } while (true);
        }

        internal BasicPadItem? Previous() {
            var itemCount = _parent.IndexOf(this);
            if (itemCount < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }
            do {
                itemCount--;
                if (itemCount < 0) { return null; }
                if (_parent[itemCount] != null) { return _parent[itemCount]; }
            } while (true);
        }

        protected abstract RectangleF CalculateUsedArea();

        protected abstract string ClassId();

        protected abstract void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, float zoom, float shiftX, float shiftY, bool forPrinting);

        protected bool IsInDrawingArea(RectangleF drawingKoordinates, Size sizeOfParentControl) => sizeOfParentControl.IsEmpty || sizeOfParentControl.Width == 0 || sizeOfParentControl.Height == 0
|| drawingKoordinates.IntersectsWith(new Rectangle(Point.Empty, sizeOfParentControl));

        protected abstract void ParseFinished();

        protected abstract BasicPadItem? TryParse(string id, string name, List<KeyValuePair<string, string>> toParse);

        private void Points_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            if (e.Item is PointM p) {
                p.Moving += PointMoving;
                p.Moved += PointMoved;
            }
        }

        private void Points_ItemRemoving(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            if (e.Item is PointM p) {
                p.Moving -= PointMoving;
                p.Moved -= PointMoved;
            }
        }

        #endregion
    }
}