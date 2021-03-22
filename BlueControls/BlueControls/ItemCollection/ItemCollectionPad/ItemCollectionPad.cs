#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using static BlueBasics.FileOperations;

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


        private bool _OrdersValid;
        private bool ComputeOrders_isin;

        public string Caption = string.Empty;

        public string ID = string.Empty;

        /// <summary>
        /// Für automatische Generierungen, die zu schnell hintereinander kommen, ein Counter für den Dateinamen
        /// </summary>
        private readonly int IDCount = 0;


        /// <summary>
        /// Alle Punkte. Im Regelfall nach Wichtigkeit aufsteigend sortiert
        /// </summary>
        public readonly ListExt<PointM> AllPoints = new ListExt<PointM>();

        /// <summary>
        /// Alle Beziehungen. Im Regelfall nach Wichtigkeit aufsteigens sortiert
        /// </summary>
        public readonly ListExt<clsPointRelation> AllRelations = new ListExt<clsPointRelation>();


        #endregion

        public bool IsParsing { get; private set; }

        [DefaultValue(true)]
        public bool IsSaved { get; set; }



        public Color BackColor { get; set; } = Color.White;

        public RowItem SheetStyle {
            get { return _SheetStyle; }

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
            get {
                return _SheetSizeInMM;
            }
            set {
                if (value == _SheetSizeInMM) { return; }
                _SheetSizeInMM = new SizeF(value.Width, value.Height);
                GenPoints();
            }
        }

        public System.Windows.Forms.Padding RandinMM {
            get {
                return _RandinMM;
            }
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


        public ItemCollectionPad(string layoutID, Database database, int rowkey) : this(database.Layouts[database.LayoutIDToIndex(layoutID)], string.Empty) {


            // Wenn nur die Row ankommt und diese null ist, kann gar nix generiert werden
            ResetVariables();
            ParseVariableAndSpecialCodes(database.Row.SearchByKey(rowkey));



            int Count = 0;
            do {
                Count++;
                PerformAllRelations();
                if (NotPerforming(false) == 0) { break; }
                if (Count > 20) { break; }
            } while (true);


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

            foreach (KeyValuePair<string, string> pair in ToParse.GetAllTags()) {

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

                    case "relation":
                        AllRelations.Add(new clsPointRelation(this, null, pair.Value));
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

                    case "grid":
                        //_Grid = pair.Value.FromPlusMinus();
                        break;

                    case "gridshow":
                        //_GridShow = float.Parse(pair.Value);
                        break;

                    case "gridsnap":
                        //_Gridsnap = float.Parse(pair.Value);
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


            //   _AutoSort = False ' False beim Parsen was anderes Rauskommt

            //OnDoInvalidate();


            //CheckGrid();

            PerformAllRelations();

            //if (needPrinterData) { RepairPrinterData(); }
        }

        private void ParseItems(string ToParse) {
            foreach (KeyValuePair<string, string> pair in ToParse.GetAllTags()) {

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
                        Add(BasicPadItem.NewByParsing(this, pair.Value));
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

            string g1 = ThisItem.Gruppenzugehörigkeit;
            ThisItem.Gruppenzugehörigkeit = string.Empty;

            Remove(ThisItem);
            Add(ThisItem);

            ThisItem.Gruppenzugehörigkeit = g1;
            OnDoInvalidate();
        }

        internal void InDenHintergrund(BasicPadItem ThisItem) {
            if (IndexOf(ThisItem) == 0) { return; }

            string g1 = ThisItem.Gruppenzugehörigkeit;
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
            string g1 = Nr1.Gruppenzugehörigkeit;
            Nr1.Gruppenzugehörigkeit = string.Empty;

            string g2 = Nr2.Gruppenzugehörigkeit;
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

                foreach (BasicPadItem ThisItem in this) {
                    if (ThisItem != null) {
                        if (Internal.ToUpper() == ThisItem.Internal.ToUpper()) {
                            return ThisItem;
                        }
                    }
                }

                return null;
            }
        }

        public List<BasicPadItem> this[int x, int Y] {
            get { return this[new Point(x, Y)]; }
        }

        public List<BasicPadItem> this[Point p] {
            get {
                List<BasicPadItem> l = new List<BasicPadItem>();

                foreach (BasicPadItem ThisItem in this) {
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
            get {
                return _SheetStyleScale;
            }
            set {

                if (value < 0.1m) { value = 0.1m; }

                if (_SheetStyleScale == value) { return; }

                _SheetStyleScale = value;

                //if (_isParsing) { return; }


                DesignOrStyleChanged();

                PerformAllRelations();
                OnDoInvalidate();
            }
        }



        internal bool RenameColumn(string oldName, ColumnItem newName) {
            bool did = false;

            foreach (BasicPadItem thisItem in this) {
                if (thisItem is ICanHaveColumnVariables variables) {
                    if (variables.RenameColumn(oldName, newName)) { did = true; }
                }
            }

            if (!did) { return false; }


            PerformAllRelations();
            PerformAllRelations();

            return true;
        }




        #endregion


        public void OnDoInvalidate() {
            DoInvalidate?.Invoke(this, System.EventArgs.Empty);
        }




        protected override void OnItemAdded(BasicPadItem item) {
            if (string.IsNullOrEmpty(item.Internal)) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Der Auflistung soll ein Item hinzugefügt werden, welches keinen Namen hat " + item.Internal);
            }

            base.OnItemAdded(item);
            item.RecalculateAndOnChanged();

            AllPoints.AddIfNotExists(item.Points);
            AllRelations.AddIfNotExists(item.Relations); // Eigentlich überflüssig

            IsSaved = false;


            item.Changed += Item_Changed;
            item.PointOrRelationsChanged += Item_PointOrRelationsChanged;

            //RecomputePointAndRelations();

            if (item.Parent != this) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Parent ungleich!");

            }
            OnDoInvalidate();

        }

        private void Item_PointOrRelationsChanged(object sender, System.EventArgs e) {

            BasicPadItem ni = (BasicPadItem)sender;

            InvalidateOrder();

            AllPoints.AddIfNotExists(ni.Points);
            AllRelations.AddIfNotExists(ni.Relations);

            RemoveInvalidPoints();
            RemoveInvalidRelations();
        }

        public void RemoveInvalidPoints() {

            /// Zuerst die Punkte
            foreach (PointM ThisPoint in AllPoints) {

                if (ThisPoint != null) {

                    if (ThisPoint.Parent is BasicPadItem Pad) {
                        if (!Contains(Pad)) {
                            AllPoints.Remove(ThisPoint);
                            RemoveInvalidPoints(); //Rekursiv
                            return;
                        }
                    } else if (ThisPoint.Parent is ItemCollectionPad ICP) {
                        if (ICP != this) {
                            AllPoints.Remove(ThisPoint);
                            RemoveInvalidPoints(); //Rekursiv
                            return;
                        }
                    } else {
                        AllPoints.Remove(ThisPoint);
                        RemoveInvalidPoints(); //Rekursiv
                        return;
                    }


                }
            }

        }

        public bool RemoveInvalidRelations() {
            int z = -1;
            bool SomethingChanged = false;


            do {
                z++;
                if (z > AllRelations.Count - 1) { break; }

                if (!AllRelations[z].IsOk(false)) {
                    AllRelations.Remove(AllRelations[z]);
                    z = -1;
                    SomethingChanged = true;
                }
            } while (true);

            return SomethingChanged;
        }

        /// <summary>
        /// Ermittelt die anzahl der Beziehungen, die nicht korrekt sind.
        /// </summary>
        /// <param name="Strongmode"></param>
        /// <returns></returns>
        public int NotPerforming(bool Strongmode) {

            int f = 0;

            foreach (clsPointRelation ThisRelation in AllRelations) {
                if (!ThisRelation.Performs(Strongmode)) { f++; }
            }

            return f;
        }


        private void Item_Changed(object sender, System.EventArgs e) {
            IsSaved = false;
            OnDoInvalidate();

        }

        public void DesignOrStyleChanged() {
            foreach (BasicPadItem thisItem in this) {
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


            foreach (BasicPadItem ThisToo in this) {
                if (item.Gruppenzugehörigkeit.ToLower() == ThisToo.Gruppenzugehörigkeit?.ToLower()) {
                    Remove(ThisToo);
                    return; // Wird eh eine Kettenreaktion ausgelöst -  und der Iteraor hier wird beschädigt
                }
            }
        }


        public RectangleM MaximumBounds(List<BasicPadItem> ZoomItems) {
            decimal x1 = decimal.MaxValue;
            decimal y1 = decimal.MaxValue;
            decimal x2 = decimal.MinValue;
            decimal y2 = decimal.MinValue;

            bool Done = false;


            foreach (BasicPadItem ThisItem in this) {
                if (ThisItem != null) {
                    if (ZoomItems == null || ZoomItems.Contains(ThisItem)) {

                        RectangleM UA = ThisItem.ZoomToArea();

                        x1 = Math.Min(x1, UA.Left);
                        y1 = Math.Min(y1, UA.Top);

                        x2 = Math.Max(x2, UA.Right);
                        y2 = Math.Max(y2, UA.Bottom);
                        Done = true;
                    }
                }
            }


            if (!Done) { return new RectangleM(); }

            return new RectangleM(x1, y1, x2 - x1, y2 - y1);
        }




        private void GenPoints() {

            if (Math.Abs(_SheetSizeInMM.Width) < 0.001 || Math.Abs(_SheetSizeInMM.Height) < 0.001) {
                if (P_rLO != null) {
                    P_rLO.Parent = null;
                    AllPoints.Remove(P_rLO);
                    P_rLO = null;

                    P_rRO.Parent = null;
                    AllPoints.Remove(P_rRO);
                    P_rRO = null;

                    P_rRU.Parent = null;
                    AllPoints.Remove(P_rRU);
                    P_rRU = null;

                    P_rLU.Parent = null;
                    AllPoints.Remove(P_rLU);
                    P_rLU = null;
                }

                return;
            }


            if (P_rLO == null) {

                P_rLO = new PointM(this, "Druckbereich LO", 0, 0, true);
                AllPoints.AddIfNotExists(P_rLO);

                P_rRO = new PointM(this, "Druckbereich RO", 0, 0, true);
                AllPoints.AddIfNotExists(P_rRO);

                P_rRU = new PointM(this, "Druckbereich RU", 0, 0, true);
                AllPoints.AddIfNotExists(P_rRU);

                P_rLU = new PointM(this, "Druckbereich LU", 0, 0, true);
                AllPoints.AddIfNotExists(P_rLU);
            }

            decimal SSW = Math.Round(modConverter.mmToPixel((decimal)_SheetSizeInMM.Width, DPI), 1);
            decimal SSH = Math.Round(modConverter.mmToPixel((decimal)_SheetSizeInMM.Height, DPI), 1);
            decimal rr = Math.Round(modConverter.mmToPixel(_RandinMM.Right, DPI), 1);
            decimal rl = Math.Round(modConverter.mmToPixel(_RandinMM.Left, DPI), 1);
            decimal ro = Math.Round(modConverter.mmToPixel(_RandinMM.Top, DPI), 1);
            decimal ru = Math.Round(modConverter.mmToPixel(_RandinMM.Bottom, DPI), 1);

            P_rLO.SetTo(rl, ro);
            P_rRO.SetTo(SSW - rr, ro);
            P_rRU.SetTo(SSW - rr, SSH - ru);
            P_rLU.SetTo(rl, SSH - ru);
            OnDoInvalidate();
        }


        protected override void OnItemRemoving(BasicPadItem item) {
            item.Changed -= Item_Changed;
            item.PointOrRelationsChanged -= Item_PointOrRelationsChanged;


            base.OnItemRemoving(item);
            AllPoints.RemoveRange(item.Points);
            AllRelations.RemoveRange(item.Relations);
        }


        protected override void OnItemRemoved() {
            base.OnItemRemoved();
            RemoveInvalidPoints();
            RemoveInvalidRelations();
            InvalidateOrder();
        }

        public override void OnChanged() {
            base.OnChanged();
            IsSaved = false;
            OnDoInvalidate();
        }


        public List<PointM> ConnectsWith(PointM Point, enXY toCheck, bool IgnoreInternals) {

            List<PointM> Points = new List<PointM>
            {
                Point
            };

            int Ist = -1;

            // Nur, wenn eine Beziehung gut ist, kann man mit sicherheit sagen, daß das zusammenhängt. Deswegen auch ein Performs test

            do {
                Ist++;
                if (Ist >= Points.Count) { break; }


                foreach (clsPointRelation ThisRelation in AllRelations) {
                    if (ThisRelation != null && ThisRelation.Points.Contains(Points[Ist]) && ThisRelation.Performs(false) && ThisRelation.Connects().HasFlag(toCheck)) {


                        if (!IgnoreInternals || !ThisRelation.IsInternal()) {
                            Points.AddIfNotExists(ThisRelation.Points);
                        }
                    }
                }
            } while (true);



            return Points;
        }







        public new string ToString() {

            PerformAllRelations();

            string t = "{";


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


            foreach (BasicPadItem Thisitem in this) {
                if (Thisitem != null) {
                    t = t + "Item=" + Thisitem.ToString() + ", ";
                }
            }

            t = t.TrimEnd(", ") + "}, ";






            //t = t + "Grid=" + _Grid + ", ";
            //t = t + "GridShow=" + _GridShow + ", ";
            //t = t + "GridSnap=" + _Gridsnap + ", ";

            //Dim One As Boolean

            foreach (clsPointRelation ThisRelation in AllRelations) {
                if (ThisRelation != null) {
                    if (!ThisRelation.IsInternal() && ThisRelation.IsOk(false)) {
                        t = t + "Relation=" + ThisRelation + ", ";
                    }
                }
            }


            return t.TrimEnd(", ") + "}";

        }



        public Bitmap ToBitmap(decimal Scale) {
            RectangleM r = MaxBounds(null);
            if (r.Width == 0) { return null; }

            modAllgemein.CollectGarbage();

            do {
                if ((int)(r.Width * Scale) > 15000) {
                    Scale *= 0.8m;
                } else if ((int)(r.Height * Scale) > 15000) {
                    Scale *= 0.8m;
                } else if ((int)(r.Height * Scale) * (int)(r.Height * Scale) > 90000000) {
                    Scale *= 0.8m;
                } else {
                    break;
                }
            } while (true);



            Bitmap I = new Bitmap((int)(r.Width * Scale), (int)(r.Height * Scale));


            using (Graphics gr = Graphics.FromImage(I)) {
                gr.Clear(Color.White);

                if (!Draw(gr, Scale, r.Left * Scale, r.Top * Scale, Size.Empty, true, null)) {
                    return ToBitmap(Scale);
                }

            }
            return I;
        }

        public bool Draw(Graphics gr, decimal zoom, decimal shiftX, decimal shiftY, Size sizeOfParentControl, bool forPrinting, List<BasicPadItem> visibleItems) {


            try {
                if (SheetStyle == null || SheetStyleScale < 0.1m) { return true; }


                foreach (BasicPadItem thisItem in this) {
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

            RectangleM r;
            if (Count == 0) {
                r = new RectangleM(0, 0, 0, 0);
            } else {
                r = MaximumBounds(ZoomItems);
            }

            if (SheetSizeInMM.Width > 0 && SheetSizeInMM.Height > 0) {

                decimal X1 = Math.Min(r.Left, 0);
                decimal y1 = Math.Min(r.Top, 0);


                decimal x2 = Math.Max(r.Right, modConverter.mmToPixel((decimal)SheetSizeInMM.Width, ItemCollectionPad.DPI));
                decimal y2 = Math.Max(r.Bottom, modConverter.mmToPixel((decimal)SheetSizeInMM.Height, ItemCollectionPad.DPI));

                return new RectangleM(X1, y1, x2 - X1, y2 - y1);
            }

            return r;


        }


        public bool ParseVariable(string VariableName, enValueType ValueType, string Value) {

            bool did = false;

            foreach (BasicPadItem thisItem in this) {
                if (thisItem is ICanHaveColumnVariables variables) {
                    if (variables.ReplaceVariable(VariableName, ValueType, Value)) { did = true; }
                }
            }

            if (did) { PerformAllRelations(); }
            return did;
        }

        public void ParseVariableAndSpecialCodes(RowItem row) {


            if (row != null) {

                foreach (ColumnItem thiscolumnitem in row.Database.Column) {
                    if (thiscolumnitem != null) {
                        ParseVariable(thiscolumnitem.Name, thiscolumnitem, row);
                    }
                }
            }
            ParseSpecialCodes();
        }

        private void ParseVariable(string VariableName, ColumnItem Column, RowItem Row) {


            switch (Column.Format) {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Bit:
                case enDataFormat.Ganzzahl:
                case enDataFormat.RelationText:
                    ParseVariable(VariableName, enValueType.Text, Row.CellGetString(Column));
                    break;

                case enDataFormat.Link_To_Filesystem:

                    if (!Column.MultiLine) {
                        string f = Column.BestFile(Row.CellGetString(Column), false);

                        if (FileExists(f)) {
                            if (Column.MultiLine) {
                                ParseVariable(VariableName, enValueType.Text, f);
                            } else {
                                string x = modConverter.FileToString(f);
                                ParseVariable(VariableName, enValueType.BinaryImage, x);
                            }
                        }
                    }
                    break;


                //case enDataFormat.Relation:
                //    ParseVariable(VariableName, enValueType.Unknown, "Nicht implementiert");
                //    break;

                default:
                    Develop.DebugPrint("Format unbekannt: " + Column.Format);
                    break;

            }
        }




        public bool ParseSpecialCodes() {
            bool did = false;

            foreach (BasicPadItem thisItem in this) {
                if (thisItem is ICanHaveColumnVariables variables) {
                    if (variables.DoSpecialCodes()) { did = true; }
                }
            }

            if (did) { OnDoInvalidate(); }

            return did;
        }

        public bool ResetVariables() {
            bool did = false;

            foreach (BasicPadItem thisItem in this) {
                if (thisItem is ICanHaveColumnVariables variables) {
                    if (variables.ResetVariables()) { did = true; }
                }
            }


            if (did) { OnDoInvalidate(); }
            return did;

        }




        internal Rectangle DruckbereichRect() {
            if (P_rLO == null) { return new Rectangle(0, 0, 0, 0); }
            return new Rectangle((int)P_rLO.X, (int)P_rLO.Y, (int)(P_rRU.X - P_rLO.X), (int)(P_rRU.Y - P_rLO.Y));
        }


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="Level">Level 0 = Hart / Reparier alles mit Gewalt; 
        ///// Level 1 = Normal / Reparier nur die neuen Sachen;
        /////  Level 2 = Leicht / Reparier nur die neuen Sachen mit schnelleren Abbruchbedingungen</param>
        ///// <param name="AllowBigChanges"></param>
        ///// <returns></returns>

        //public bool RepairAll(int Level, bool AllowBigChanges)
        //{
        //    //InvalidateOrder();

        //    //if (Level == 0)
        //    //{
        //    //    //RepairAll_OldItemc = Itemc + 1; // Löst eine Kettenreaktion aus
        //    //    RecomputePointAndRelations();
        //    //}

        //    return PerformAll(Level, AllowBigChanges);
        //}


        public void PerformAllRelations() {
            ComputeOrders(null);
            foreach (clsPointRelation ThisRelation in AllRelations) {
                ThisRelation.Perform(AllPoints);
            }
        }


        public void ComputeOrders(List<PointM> selectedPoints) {
            if (_OrdersValid) { return; }

            if (ComputeOrders_isin) { return; }
            ComputeOrders_isin = true;


            RemoveInvalidPoints();
            RemoveInvalidRelations();

            ComputePointOrder(selectedPoints);

            _OrdersValid = true;
            ComputeOrders_isin = false;
        }
        public void Relations_Optimize() {
            if (NotPerforming(true) > 0) { return; }


            List<PointM> Cb = new List<PointM>();
            List<clsPointRelation> DobR = new List<clsPointRelation>();


            foreach (PointM thisPoint in AllPoints) {
                List<PointM> CX = ConnectsWith(thisPoint, enXY.X, true);
                List<PointM> CY = ConnectsWith(thisPoint, enXY.Y, true);

                // Ermitteln, die auf X und Y miteinander verbunden sind
                Cb.Clear();
                foreach (PointM thisPoint2 in CX) {
                    if (CY.Contains(thisPoint2)) { Cb.Add(thisPoint2); }
                }


                if (Cb.Count > 1) {

                    DobR.Clear();
                    foreach (clsPointRelation ThisRelation in AllRelations) {


                        // Wenn Punkte nicht direct verbunden sind, aber trotzdem Fix zueinander, die Beziehung optimieren
                        if (ThisRelation.RelationType == enRelationType.WaagerechtSenkrecht && !ThisRelation.IsInternal()) {
                            if (Cb.Contains(ThisRelation.Points[0]) && Cb.Contains(ThisRelation.Points[1])) {
                                ThisRelation.RelationType = enRelationType.PositionZueinander;
                                ThisRelation.OverrideSavedRichtmaß(false, false);
                                Relations_Optimize();
                                return;
                            }
                        }


                        // Für nachher, die doppelten fixen Beziehungen merken
                        if (ThisRelation.RelationType == enRelationType.PositionZueinander) {
                            if (Cb.Contains(ThisRelation.Points[0]) && Cb.Contains(ThisRelation.Points[1])) { DobR.Add(ThisRelation); }
                        }


                    }


                    // Und nun beziehungen löschen, die auf gleiche Objecte zugreifen
                    if (DobR.Count > 1) {
                        foreach (clsPointRelation R1 in DobR) {
                            // Mindestens eine muss external sein!!!
                            if (!R1.IsInternal()) {
                                foreach (clsPointRelation R2 in DobR) {
                                    if (!R1.SinngemäßIdenitisch(R2)) {

                                        if (R1.Points[0].Parent == R2.Points[0].Parent && R1.Points[1].Parent == R2.Points[1].Parent) {
                                            AllRelations.Remove(R1);
                                            Relations_Optimize();
                                            return;
                                        }

                                        if (R1.Points[0].Parent == R2.Points[1].Parent && R1.Points[1].Parent == R2.Points[0].Parent) {
                                            AllRelations.Remove(R1);
                                            Relations_Optimize();
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }



            // und nun direct nach doppelten suchen
            foreach (clsPointRelation r1 in AllRelations) {
                if (!r1.IsInternal()) {
                    foreach (clsPointRelation r2 in AllRelations) {
                        if (!r1.SinngemäßIdenitisch(r2) && !r2.IsInternal()) {
                            if (r1.SinngemäßIdenitisch(r2)) {
                                AllRelations.Remove(r2);
                                Relations_Optimize();
                                return;

                            }

                            if (r1.UsesSamePoints(r2)) {
                                switch (r1.RelationType) {
                                    case enRelationType.PositionZueinander:
                                        // Beziehungen mit gleichen punkten, aber einer mächtigen PositionZueinander -> andere löschen
                                        AllRelations.Remove(r2);
                                        Relations_Optimize();
                                        return;
                                    case enRelationType.WaagerechtSenkrecht when r2.RelationType == enRelationType.WaagerechtSenkrecht && r1._Richtmaß[0] != r2._Richtmaß[0]:
                                        // Beziehungen mit gleichen punkten, aber spearat mit X und Y -> PositionZueinander konvertieren 
                                        r1.RelationType = enRelationType.PositionZueinander;
                                        r1.OverrideSavedRichtmaß(false, false);
                                        AllRelations.Remove(r2);
                                        Relations_Optimize();
                                        return;
                                }
                            }
                        }
                    }
                }
            }

        }

        public void InvalidateOrder() {
            _OrdersValid = false;
        }
        public PointM Getbetterpoint(double X, double Y, PointM notPoint, bool MustUsableForAutoRelation) {

            foreach (PointM thispoint in AllPoints) {

                if (thispoint != null) {

                    if (!MustUsableForAutoRelation || thispoint.CanUsedForAutoRelation) {

                        if (thispoint != notPoint) {
                            if (Math.Abs((double)thispoint.X - X) < 0.01 && Math.Abs((double)thispoint.Y - Y) < 0.01) { return thispoint; }
                        }

                    }

                }
            }

            return null;
        }

        //public PointM GetPointWithLowerIndex(PointM NotPoint, PointM ErsatzFür, bool MustUsableForAutoRelation)
        //{
        //    if (NotPoint != null && NotPoint.Parent == ErsatzFür.Parent) { return ErsatzFür; }



        //    foreach (var thispoint in PointOrder)
        //    {
        //        if (thispoint != null)
        //        {
        //            if (!MustUsableForAutoRelation || thispoint.CanUsedForAutoRelation)
        //            {
        //                if (thispoint != NotPoint)
        //                {
        //                    if (Math.Abs(thispoint.X - ErsatzFür.X) < 0.01m && Math.Abs(thispoint.Y - ErsatzFür.Y) < 0.01m)
        //                    {
        //                        return thispoint; // der erste Punkt ist der niedrigste im Index - kann auch der "ErsatzFür" sein
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return ErsatzFür;
        //}


        private void ComputePointOrder(List<PointM> Sel_P) {
            int Modus = 0;
            bool done = false;

            #region Punkte vorbereiten
            List<PointM> _Points = new List<PointM>();
            _Points.AddRange(AllPoints);

            AllPoints.Clear();
            #endregion

            #region Beziehungen ermitteln, wie sie was verbinden

            List<clsPointRelation> RelationNone = new List<clsPointRelation>();
            List<clsPointRelation> RelationY = new List<clsPointRelation>();
            List<clsPointRelation> RelationX = new List<clsPointRelation>();
            List<clsPointRelation> RelationXY = new List<clsPointRelation>();

            foreach (clsPointRelation thisRelation in AllRelations) {
                switch (thisRelation.Connects()) {
                    case enXY.XY:
                        RelationXY.Add(thisRelation);
                        break;
                    case enXY.X:
                        RelationX.Add(thisRelation);
                        break;
                    case enXY.Y:
                        RelationY.Add(thisRelation);
                        break;
                    default:
                        RelationNone.Add(thisRelation);
                        break;
                }
            }


            AllRelations.Clear();
            #endregion

            do {

                int z = 0;
                while (z < _Points.Count) {

                    PointM Thispoint = _Points[z];

                    switch (Modus) {
                        case 0: // Unbewegliche Punkte hinzufügen
                            if (Thispoint.Fix) { AllPoints.Add(Thispoint); }
                            break;

                        case 1: // Verbundene Punkte, die durch verbindungen X und Y Fix sind
                            foreach (clsPointRelation thisRelation in RelationXY) {
                                if (thisRelation.NeedCount(Thispoint, AllPoints)) {
                                    AllPoints.Add(Thispoint);
                                    AllRelations.Add(thisRelation);
                                    RelationXY.Remove(thisRelation);
                                    break;

                                }
                            }
                            break;



                        //case 2: // Y-Unbewegliche Punkte hinzufügen
                        //    if (!Thispoint.Moveable.HasFlag(enXY.Y)) { AllPoints.Add(Thispoint); }
                        //    break;

                        case 2: // Fixe Y-Punkte hinzufügen
                            foreach (clsPointRelation thisRelation in RelationY) {
                                if (thisRelation.NeedCount(Thispoint, AllPoints)) {
                                    AllPoints.Add(Thispoint);
                                    AllRelations.Add(thisRelation);
                                    RelationY.Remove(thisRelation);
                                    break;
                                }
                            }

                            break;

                        //case 4: // X-Unbewegliche Punkte hinzufügen
                        //    if (!Thispoint.Moveable.HasFlag(enXY.X)) { AllPoints.Add(Thispoint); }
                        //    break;

                        case 3: // Fixe X-Punkte hinzufügen
                            foreach (clsPointRelation thisRelation in RelationX) {
                                if (thisRelation.NeedCount(Thispoint, AllPoints)) {
                                    AllPoints.Add(Thispoint);
                                    AllRelations.Add(thisRelation);
                                    RelationX.Remove(thisRelation);
                                    break;
                                }
                            }
                            break;

                        case 4: // Punkte hinzufügen, die in einer Beziehung sind UND ein Punkt bereits einen Order hat

                            foreach (clsPointRelation thisRelation in RelationNone) {
                                if (thisRelation.NeedCount(Thispoint, AllPoints)) {
                                    AllPoints.Add(Thispoint);
                                    AllRelations.Add(thisRelation);
                                    RelationNone.Remove(thisRelation);
                                    break;
                                }
                            }
                            break;

                        case 5: // Selectierte Punkte bevorzugen
                            if (Sel_P != null && Sel_P.Contains(Thispoint)) {
                                AllPoints.Add(Thispoint);
                            }

                            break;

                        case 6: // Der gute Rest
                            AllPoints.Add(Thispoint);
                            break;

                        default:
                            done = true;
                            break;
                    }

                    if (AllPoints.Contains(Thispoint)) {
                        Modus = 0;
                        z = 0;
                        _Points.Remove(Thispoint);
                    } else {
                        z++;

                        if (z >= _Points.Count) { Modus++; }
                    }


                }

                if (_Points.Count == 0) { done = true; }

            } while (!done);

            AllRelations.AddRange(RelationXY);
            AllRelations.AddRange(RelationX);
            AllRelations.AddRange(RelationY);
            AllRelations.AddRange(RelationNone);

            AllPoints.AddRange(_Points);


        }

        //public void ComputeRelationOrder()
        //{
        //    var Count = 0;

        //    // Zurücksetzen ---- 
        //    foreach (var ThisRelation in AllRelations)
        //    {
        //        ThisRelation.Order = -1;
        //    }


        //    for (var Durch = 0; Durch <= 1; Durch++)
        //    {

        //        do
        //        {
        //            clsPointRelation NextRel = null;
        //            var RelPO = int.MaxValue;

        //            foreach (var ThisRelation in AllRelations)
        //            {
        //                if (ThisRelation.Order < 0)
        //                {
        //                    if (Durch > 0 || ThisRelation.IsInternal())
        //                    {
        //                        if (LowestOrder(ThisRelation.Points) < RelPO)
        //                        {
        //                            NextRel = ThisRelation;
        //                            RelPO = LowestOrder(ThisRelation.Points);
        //                        }
        //                    }
        //                }
        //            }

        //            if (NextRel == null) { break; }

        //            Count++;
        //            NextRel.Order = Count;
        //        } while (true);

        //    }

        //    AllRelations.Sort();
        //}

        //public int LowestOrder(ListExt<PointM> ThisPoints)
        //{
        //    var l = int.MaxValue;

        //    foreach (var Thispouint in ThisPoints)
        //    {
        //        l = Math.Min(l, Thispouint.Order);
        //    }

        //    return l;
        //}

        public void SaveAsBitmap(string Filename) {

            Bitmap i = ToBitmap(1);

            if (i == null) { return; }


            switch (Filename.FileSuffix().ToUpper()) {

                case "JPG":
                case "JPEG":
                    i.Save(Filename, ImageFormat.Jpeg);
                    break;

                case "PNG":
                    i.Save(Filename, ImageFormat.Png);
                    break;

                case "BMP":
                    i.Save(Filename, ImageFormat.Bmp);
                    break;

                default:
                    MessageBox.Show("Dateiformat unbekannt: " + Filename.FileSuffix().ToUpper(), enImageCode.Warnung, "OK");
                    return;
            }
        }


    }
}