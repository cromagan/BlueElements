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

namespace BlueControls.ItemCollection {

    public abstract class BasicPadItem : IParseable, ICloneable, IChangedFeedback, IMoveable {

        #region Fields

        public readonly ListExt<PointM> MovablePoint = new();
        public readonly ItemCollectionPad Parent = null;
        public readonly List<PointM> PointsForSuccesfullyMove = new();
        private static int UniqueInternal_Count;
        private static string UniqueInternal_LastTime = "InitialDummy";

        /// <summary>
        /// Falls eine Spezielle Information gespeichert und zurückgegeben werden soll
        /// </summary>
        /// <remarks></remarks>
        private readonly List<string> _Tags = new();

        /// <summary>
        /// Soll es gedruckt werden?
        /// </summary>
        /// <remarks></remarks>
        private bool _Bei_Export_sichtbar = true;

        private PadStyles _Style = PadStyles.Undefiniert;
        private int _ZoomPadding = 0;
        private RectangleM tmpUsedArea = null;

        #endregion

        #region Constructors

        protected BasicPadItem(ItemCollectionPad parent, string internalname) {
            Parent = parent;
            Internal = string.IsNullOrEmpty(internalname) ? UniqueInternal() : internalname;
            if (string.IsNullOrEmpty(Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
            MovablePoint.ItemAdded += Points_ItemAdded;
            MovablePoint.ItemRemoving += Points_ItemRemoving;
        }

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        [Description("Wird bei einem Export (wie z. B. Drucken) nur angezeigt, wenn das Häkchen gesetzt ist.")]
        public bool Bei_Export_sichtbar {
            get => _Bei_Export_sichtbar;
            set {
                if (_Bei_Export_sichtbar == value) { return; }
                _Bei_Export_sichtbar = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Wird ein Element gelöscht, das diese Feld befüllt hat, werden automatisch alle andern Elemente mit der selben Gruppe gelöscht.
        /// </summary>
        [Description("Alle Elemente, die der selben Gruppe angehören, werden beim Löschen eines Elements ebenfalls gelöscht.")]
        public string Gruppenzugehörigkeit { get; set; } = string.Empty;

        public string Internal { get; private set; }
        public bool IsParsing { get; private set; }
        public virtual string QuickInfo { get; set; } = string.Empty;

        public PadStyles Stil {
            get => _Style;
            set {
                if (_Style == value) { return; }
                _Style = value;
                DesignOrStyleChanged();
            }
        }

        public List<string> Tags => _Tags;

        public int ZoomPadding {
            get => _ZoomPadding;
            set {
                if (_ZoomPadding == value) { return; }
                _ZoomPadding = value;
                OnChanged();
            }
        }

        #endregion

        #region Methods

        public static BasicPadItem NewByParsing(ItemCollectionPad parent, string code) {
            BasicPadItem i = null;
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
                Develop.DebugPrint(enFehlerArt.Fehler, "Itemtyp unbekannt: " + code);
                return null;
            }
            if (string.IsNullOrEmpty(name)) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Itemname unbekannt: " + code);
                return null;
            }
            switch (ding.ToLower()) {
                case "blueelements.clsitemtext":

                case "blueelements.textitem":

                case "text":
                    i = new TextPadItem(parent, name, string.Empty);
                    break;

                case "blueelements.clsitemdistanz": // Todo: Entfernt am 24.05.2021

                case "blueelements.distanzitem": // Todo: Entfernt am 24.05.2021

                case "spacer": // Todo: Entfernt am 24.05.2021
                    i = null;
                    break;

                case "blueelements.clsitemimage":

                case "blueelements.imageitem":

                case "image":
                    i = new BitmapPadItem(parent, name, string.Empty);
                    break;

                case "blueelements.clsdimensionitem":

                case "blueelements.dimensionitem":

                case "dimension":
                    i = new DimensionPadItem(parent, name, null, null, 0);
                    break;

                case "blueelements.clsitemline":

                case "blueelements.itemline":

                case "line":
                    i = new LinePadItem(parent, name, PadStyles.Style_Standard, Point.Empty, Point.Empty);
                    break;

                case "blueelements.clsitempad":

                case "blueelements.itempad":

                case "childpad":
                    i = new ChildPadItem(parent, name);
                    break;

                case "blueelements.clsitemgrid": // Todo: Entfernt am 24.05.2021

                case "blueelements.itemgrid": // Todo: Entfernt am 24.05.2021

                case "grid": // Todo: Entfernt am 24.05.2021
                    i = null;// new GridPadItem(parent, name);
                    break;

                case "blueelements.rowformulaitem":

                case "row":
                    i = new RowFormulaPadItem(parent, name);
                    break;

                case "symbol":
                    i = new SymbolPadItem(parent, name);
                    break;

                default:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Unbekanntes Item: " + code);
                    break;
            }
            if (i != null) { i.Parse(x); }
            return i;
        }

        public static string UniqueInternal() {
            var NeueZeit = DateTime.Now + " " + DateTime.Now.Millisecond;
            if (NeueZeit == UniqueInternal_LastTime) {
                UniqueInternal_Count++;
            } else {
                UniqueInternal_Count = 0;
                UniqueInternal_LastTime = NeueZeit;
            }
            return "Auto " + NeueZeit + " IDX" + UniqueInternal_Count;
        }

        public object Clone() {
            var t = ToString();
            return NewByParsing(Parent, t);
        }

        /// <summary>
        /// Prüft, ob die angegebenen Koordinaten das Element berühren.
        /// Der Zoomfaktor wird nur benötigt, um Maßstabsunabhängige Punkt oder Linienberührungen zu berechnen.
        /// </summary>
        /// <remarks></remarks>
        public virtual bool Contains(PointF value, double zoomfactor) {
            var tmp = (RectangleF)UsedArea(); // Umwandlung, um den Bezug zur Klasse zu zerstören
            var ne = (float)(-5 / zoomfactor) + 1;
            tmp.Inflate(ne, ne);
            return tmp.Contains(value);
        }

        public virtual void DesignOrStyleChanged() { }

        public void Draw(Graphics gr, double zoom, double shiftX, double shiftY, enStates state, Size sizeOfParentControl, bool forPrinting) {
            if (Parent == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Parent nicht definiert"); }
            if (forPrinting && !_Bei_Export_sichtbar) { return; }
            var drawingCoordinates = UsedArea().ZoomAndMoveRect(zoom, shiftX, shiftY, false);
            if (Parent == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Parent = null"); }
            if (!IsInDrawingArea(drawingCoordinates, sizeOfParentControl)) { return; }
            DrawExplicit(gr, drawingCoordinates, zoom, shiftX, shiftY, state, sizeOfParentControl, forPrinting);
            if (!_Bei_Export_sichtbar) {
                if (IsInDrawingArea(drawingCoordinates, sizeOfParentControl)) {
                    gr.DrawImage(QuickImage.Get("Drucker|16||1").BMP, drawingCoordinates.X, drawingCoordinates.Y);
                }
            }
        }

        /// <summary>
        /// Zeichnet die UsedArea. mehr für Debugging gedacht.
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="zoom"></param>
        /// <param name="shiftX"></param>
        /// <param name="shiftY"></param>
        /// <param name="c"></param>
        public void DrawOutline(Graphics gr, double zoom, double shiftX, double shiftY, Color c) => gr.DrawRectangle(new Pen(c), UsedArea().ZoomAndMoveRect(zoom, shiftX, shiftY, false));

        public void EineEbeneNachHinten() {
            if (Parent == null) { return; }
            var i2 = Previous();
            if (i2 != null) {
                var tempVar = this;
                Parent.Swap(tempVar, i2);
            }
        }

        public void EineEbeneNachVorne() {
            if (Parent == null) { return; }
            var i2 = Next();
            if (i2 != null) {
                var tempVar = this;
                Parent.Swap(tempVar, i2);
            }
        }

        /// <summary>
        /// Gibt für das aktuelle Item das "Kontext-Menü" zurück.
        /// </summary>
        /// <returns></returns>
        public virtual List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new()
            {
                new FlexiControlForProperty(this, "Gruppenzugehörigkeit"),
                new FlexiControlForProperty(this, "Bei_Export_sichtbar")
            };
            return l;
        }

        public void InDenHintergrund() => Parent?.InDenHintergrund(this);

        public void InDenVordergrund() => Parent?.InDenVordergrund(this);

        public void Move(double x, double y) {
            if (x == 0 && y == 0) { return; }
            for (var i = 0; i < PointsForSuccesfullyMove.Count; i++) {
                PointsForSuccesfullyMove[i].Move(x, y);
            }
            OnChanged();
        }

        ///// <summary>
        ///// OnChanged wird nicht im Parsing gemacht
        ///// </summary>
        //[Obsolete]
        //public void RecalculateAndOnChanged() {
        //    CaluclatePointsWORelations();
        //    if (!IsParsing) { OnChanged(); }
        //}
        public void OnChanged() {
            //if (this is IParseable O && O.IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            tmpUsedArea = null;
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        public void Parse(List<KeyValuePair<string, string>> ToParse) {
            IsParsing = true;
            foreach (var pair in ToParse) {
                if (!ParseThis(pair.Key, pair.Value)) {
                    Develop.DebugPrint(enFehlerArt.Warnung, "Kann nicht geparsed werden: " + pair.Key + "/" + pair.Value + "/" + ToParse);
                }
            }

            ParseFinished();
            IsParsing = false;
        }

        public void Parse(string ToParse) => Parse(ToParse.GetAllTags());

        public virtual bool ParseThis(string tag, string value) {
            switch (tag.ToLower()) {
                case "classid":

                case "type":

                case "enabled":

                case "checked":
                    return true;

                case "tag":
                    _Tags.Add(value.FromNonCritical());
                    return true;

                case "print":
                    _Bei_Export_sichtbar = value.FromPlusMinus();
                    return true;

                case "point":
                    foreach (var ThisPoint in MovablePoint) {
                        if (value.Contains("Name=" + ThisPoint.Name + ",")) {
                            ThisPoint.Parse(value);
                        }
                    }
                    return true;

                case "format": // = Textformat!!!

                case "design":

                case "style":
                    _Style = (PadStyles)int.Parse(value);
                    return true;

                case "removetoo": // TODO: Alt, löschen, 02.03.2020
                    //RemoveToo.AddRange(value.FromNonCritical().SplitAndCutByCR());
                    return true;

                case "removetoogroup":
                    Gruppenzugehörigkeit = value.FromNonCritical();
                    return true;

                case "internalname":
                    if (value != Internal) {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Namen unterschiedlich: " + value + " / " + Internal);
                    }
                    return true;

                case "zoompadding":
                    _ZoomPadding = int.Parse(value);
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
            if (_Tags.Count > 0) {
                foreach (var ThisTag in _Tags) {
                    t = t + "Tag=" + ThisTag.ToNonCritical() + ", ";
                }
            }
            t = t + "Style=" + (int)_Style + ", ";
            t = t + "Print=" + _Bei_Export_sichtbar.ToPlusMinus() + ", ";
            t = t + "QuickInfo=" + QuickInfo.ToNonCritical() + ", ";
            if (_ZoomPadding != 0) {
                t = t + "ZoomPadding=" + _ZoomPadding + ", ";
            }
            foreach (var ThisPoint in MovablePoint) {
                t = t + "Point=" + ThisPoint + ", ";
            }
            if (!string.IsNullOrEmpty(Gruppenzugehörigkeit)) {
                t = t + "RemoveTooGroup=" + Gruppenzugehörigkeit.ToNonCritical() + ", ";
            }
            return t.Trim(", ") + "}";
        }

        /// <summary>
        /// Gibt den Bereich zurück, den das Element benötigt, um komplett dargestellt zu werden. Unabhängig von der aktuellen Ansicht.
        /// </summary>
        /// <remarks></remarks>
        public RectangleM UsedArea() {
            if (tmpUsedArea == null) { tmpUsedArea = CalculateUsedArea(); }
            return tmpUsedArea;
        }

        /// <summary>
        /// Gibt den Bereich zurück, den das Element benötigt, um komplett dargestellt zu werden. Unabhängig von der aktuellen Ansicht. Zusätzlich mit dem Wert aus Padding.
        /// </summary>
        /// <remarks></remarks>
        public RectangleM ZoomToArea() {
            var x = UsedArea();
            if (_ZoomPadding == 0) { return x; }
            x.Inflate(-ZoomPadding, -ZoomPadding);
            return x;
        }

        internal void AddLineStyleOption(List<FlexiControl> l) => l.Add(new FlexiControlForProperty(this, "Stil", Skin.GetRahmenArt(Parent.SheetStyle, true)));

        internal void AddStyleOption(List<FlexiControl> l) => l.Add(new FlexiControlForProperty(this, "Stil", Skin.GetFonts(Parent.SheetStyle)));

        internal BasicPadItem Next() {
            var ItemCount = Parent.IndexOf(this);
            if (ItemCount < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }
            do {
                ItemCount++;
                if (ItemCount >= Parent.Count) { return null; }
                if (Parent[ItemCount] != null) { return Parent[ItemCount]; }
            } while (true);
        }

        internal BasicPadItem Previous() {
            var ItemCount = Parent.IndexOf(this);
            if (ItemCount < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }
            do {
                ItemCount--;
                if (ItemCount < 0) { return null; }
                if (Parent[ItemCount] != null) { return Parent[ItemCount]; }
            } while (true);
        }

        protected abstract RectangleM CalculateUsedArea();

        protected abstract string ClassId();

        protected abstract void DrawExplicit(Graphics gr, RectangleF drawingCoordinates, double zoom, double shiftX, double shiftY, enStates state, Size sizeOfParentControl, bool forPrinting);

        protected bool IsInDrawingArea(RectangleF DrawingKoordinates, Size SizeOfParentControl) => SizeOfParentControl.IsEmpty || SizeOfParentControl.Width == 0 || SizeOfParentControl.Height == 0
|| DrawingKoordinates.IntersectsWith(new Rectangle(Point.Empty, SizeOfParentControl));

        protected abstract void ParseFinished();

        private void Points_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            if (e.Item is PointM P) {
                P.Moving += PointMoving;
                P.Moved += PointMoved;
            }
        }

        private void Points_ItemRemoving(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            if (e.Item is PointM P) {
                P.Moving -= PointMoving;
                P.Moved -= PointMoved;
            }
        }

        #endregion
    }
}