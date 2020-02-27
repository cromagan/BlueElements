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

using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using static BlueBasics.FileOperations;
using BlueDatabase;
using BlueBasics.Enums;
using System.ComponentModel;
using BlueControls.Interfaces;
using BlueDatabase.Enums;
using System.Drawing.Imaging;
using BlueControls.Forms;

namespace BlueControls.ItemCollection
{
    public class ItemCollectionPad : ListExt<BasicPadItem>
    {
        #region  Variablen-Deklarationen 

        public static readonly int DPI = 300;
        private RowItem _SheetStyle;
        private decimal _SheetStyleScale;
        private SizeF _SheetSizeInMM = SizeF.Empty;
        private System.Windows.Forms.Padding _RandinMM = System.Windows.Forms.Padding.Empty;


        public PointDF P_rLO;
        public PointDF P_rLU;
        public PointDF P_rRU;
        public PointDF P_rRO;



        private bool _OrdersValid;

        private bool ComputeOrders_isin;

        public string Caption = "";

        public string ID = string.Empty;



        /// <summary>
        /// Für automatische Generierungen, die zu schnell hintereinander kommen, ein Counter
        /// </summary>
        int IDCount = 0;


        public readonly ListExt<PointDF> AllPoints;
        public readonly ListExt<clsPointRelation> AllRelations;


        #endregion

        public bool IsParsing { get; private set; }

        [DefaultValue(true)]
        public bool IsSaved { get; set; }

        public RowItem SheetStyle
        {
            get { return _SheetStyle; }

            set
            {
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



        public SizeF SheetSizeInMM
        {
            get
            {
                return _SheetSizeInMM;
            }
            set
            {
                if (value == _SheetSizeInMM) { return; }
                _SheetSizeInMM = new SizeF(value.Width, value.Height);
                GenPoints();
            }
        }

        public System.Windows.Forms.Padding RandinMM
        {
            get
            {
                return _RandinMM;
            }
            set
            {
                _RandinMM = new System.Windows.Forms.Padding(Math.Max(0, value.Left), Math.Max(0, value.Top), Math.Max(0, value.Right), Math.Max(0, value.Bottom));
                GenPoints();
            }
        }



        #region  Construktor + Initialize 

        public ItemCollectionPad() : base()
        {

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

            AllRelations = new ListExt<clsPointRelation>();
            AllRelations.ListOrItemChanged += AllRelations_ListOrItemChanged;

            AllPoints = new ListExt<PointDF>();
            AllPoints.ListOrItemChanged += AllPoints_ListOrItemChanged;

        }


        public ItemCollectionPad(string LayoutID, RowItem Row) : this(Row.Database.Layouts[Row.Database.LayoutIDToIndex(LayoutID)], string.Empty)
        {

            ResetVariables();
            ParseVariableAndSpecialCodes(Row);



            var Count = 0;
            do
            {
                Count += 1;
                RepairAll(1, false);
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
        public ItemCollectionPad(string ToParse, string useThisID) : this()
        {

            if (string.IsNullOrEmpty(ToParse) || ToParse.Length < 3) { return; }
            if (ToParse.Substring(0, 1) != "{") { return; }// Alte Daten gehen eben verloren.


            ID = useThisID;

            foreach (var pair in ToParse.GetAllTags())
            {

                switch (pair.Key.ToLower())
                {
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
                        AllRelations.Add(new clsPointRelation(this, pair.Value));
                        break;

                    case "caption":
                        Caption = pair.Value.FromNonCritical();
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
                        if (int.Parse(pair.Value) != DPI)
                        {
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

            RepairAll(0, true);

            //if (needPrinterData) { RepairPrinterData(); }
        }

        private void ParseItems(string ToParse)
        {
            foreach (var pair in ToParse.GetAllTags())
            {

                switch (pair.Key.ToLower())
                {
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
                        if (int.Parse(pair.Value) != DPI)
                        {
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


        internal void InDenVordergrund(BasicPadItem ThisItem)
        {
            if (IndexOf(ThisItem) == Count - 1) { return; }

            Remove(ThisItem);
            Add(ThisItem);

            OnDoInvalidate();
        }

        internal void InDenHintergrund(BasicPadItem ThisItem)
        {
            if (IndexOf(ThisItem) == 0) { return; }

            Remove(ThisItem);
            Insert(0, ThisItem);

            OnDoInvalidate();
        }


        public void RecomputePointAndRelations()
        {
            foreach (var thisItem in this)
            {
                thisItem?.RecomputePointAndRelations();
            }
        }


        public void Swap(BasicPadItem Nr1, BasicPadItem Nr2)
        {
            Swap(IndexOf(Nr1), IndexOf(Nr2));
        }


        #region  Standard-Such-Properties 

        public BasicPadItem this[string Internal]
        {
            get
            {
                if (string.IsNullOrEmpty(Internal))
                {
                    return null;
                }

                foreach (var ThisItem in this)
                {
                    if (ThisItem != null)
                    {
                        if (Internal.ToUpper() == ThisItem.Internal.ToUpper())
                        {
                            return ThisItem;
                        }
                    }
                }

                return null;
            }
        }

        public List<BasicPadItem> this[int x, int Y]
        {
            get { return this[new Point(x, Y)]; }
        }

        public List<BasicPadItem> this[Point p]
        {
            get
            {
                var l = new List<BasicPadItem>();

                foreach (var ThisItem in this)
                {
                    if (ThisItem != null && ThisItem.Contains(p, 1))
                    {
                        l.Add(ThisItem);
                    }
                }

                return l;
            }
        }

        #endregion
        #region  Properties 

        [DefaultValue(1.0)]
        public decimal SheetStyleScale
        {
            get
            {
                return _SheetStyleScale;
            }
            set
            {

                if (value < 0.1m) { value = 0.1m; }

                if (_SheetStyleScale == value) { return; }

                _SheetStyleScale = value;

                //if (_isParsing) { return; }


                DesignOrStyleChanged();

                RepairAll(0, true);
                OnDoInvalidate();
            }
        }



        internal bool RenameColumn(string oldName, ColumnItem cColumnItem)
        {
            var did = false;

            foreach (var thisItem in this)
            {
                if (thisItem is ICanHaveColumnVariables variables)
                {
                    if (variables.RenameColumn(oldName, cColumnItem))
                    {
                        thisItem.RecomputePointAndRelations();
                        did = true;
                    }
                }
            }

            if (!did) { return false; }


            RepairAll(0, true);
            RepairAll(1, true);

            return true;
        }




        #endregion


        public void OnDoInvalidate()
        {
            DoInvalidate?.Invoke(this, System.EventArgs.Empty);
        }




        protected override void OnItemAdded(BasicPadItem item)
        {
            if (string.IsNullOrEmpty(item.Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Der Auflistung soll ein Item hinzugefügt werden, welches keinen Namen hat " + item.Internal);
            }

            base.OnItemAdded(item);
            AllPoints.AddIfNotExists(item.Points);
            AllRelations.AddIfNotExists(item.Relations);

            IsSaved = false;
            InvalidateOrder();

            item.Changed += Item_Changed;
            item.PointOrRelationsChanged += Item_PointOrRelationsChanged;

            RecomputePointAndRelations();

            if (item.Parent != this)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Parent ungleich!");

            }
            OnDoInvalidate();

        }

        private void Item_PointOrRelationsChanged(object sender, System.EventArgs e)
        {

            var ni = (BasicPadItem)sender;


            AllPoints.AddIfNotExists(ni.Points);
            AllRelations.AddIfNotExists(ni.Relations);

            RemoveInvalidPoints();
            RemoveInvalidRelations();
        }

        private void RemoveInvalidPoints()
        {

            /// Zuerst die Punkte
            foreach (var ThisPoint in AllPoints)
            {

                if (ThisPoint != null)
                {

                    if (ThisPoint.Parent is BasicPadItem Pad)
                    {
                        if (!Contains(Pad))
                        {
                            AllPoints.Remove(ThisPoint);
                            InvalidateOrder();
                            RemoveInvalidPoints(); //Rekursiv
                            return;
                        }
                    }
                    else if (ThisPoint.Parent is ItemCollectionPad ICP)
                    {
                        if (ICP != this)
                        {
                            AllPoints.Remove(ThisPoint);
                            InvalidateOrder();
                            RemoveInvalidPoints(); //Rekursiv
                            return;
                        }
                    }
                    else
                    {
                        AllPoints.Remove(ThisPoint);
                        InvalidateOrder();
                        RemoveInvalidPoints(); //Rekursiv
                        return;
                    }


                }
            }





        }

        public bool RemoveInvalidRelations()
        {
            var z = -1;
            var SomethingChanged = false;


            do
            {
                z += 1;
                if (z > AllRelations.Count - 1) { break; }

                if (!AllRelations[z].IsOk(false))
                {
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
        public int NotPerforming(bool Strongmode)
        {

            var f = 0;

            foreach (var ThisRelation in AllRelations)
            {
                if (!ThisRelation.Performs(Strongmode)) { f += 1; }
            }

            return f;
        }


        private void Item_Changed(object sender, System.EventArgs e)
        {
            IsSaved = false;
            OnDoInvalidate();

        }

        public void DesignOrStyleChanged()
        {
            foreach (var thisItem in this)
            {
                thisItem?.DesignOrStyleChanged();
            }
            OnDoInvalidate();
        }


        public void Remove(string Internal)
        {
            Remove(this[Internal]);
        }

        public new void Remove(BasicPadItem cItem)
        {
            if (cItem == null) { return; }

            base.Remove(cItem);

            foreach (var ThisToo in cItem.RemoveToo)
            {
                Remove(ThisToo);
            }
        }


        public RectangleDF MaximumBounds(List<BasicPadItem> ZoomItems)
        {
            var x1 = decimal.MaxValue;
            var y1 = decimal.MaxValue;
            var x2 = decimal.MinValue;
            var y2 = decimal.MinValue;

            var Done = false;


            foreach (var ThisItem in this)
            {
                if (ThisItem != null)
                {
                    if (ZoomItems == null || ZoomItems.Contains(ThisItem))
                    {

                        var UA = ThisItem.ZoomToArea();

                        x1 = Math.Min(x1, UA.Left);
                        y1 = Math.Min(y1, UA.Top);

                        x2 = Math.Max(x2, UA.Right);
                        y2 = Math.Max(y2, UA.Bottom);
                        Done = true;
                    }
                }
            }


            if (!Done) { return new RectangleDF(); }

            return new RectangleDF(x1, y1, x2 - x1, y2 - y1);
        }




        private void GenPoints()
        {

            if (Math.Abs(_SheetSizeInMM.Width) < 0.001 || Math.Abs(_SheetSizeInMM.Height) < 0.001)
            {
                if (P_rLO != null)
                {
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


            if (P_rLO == null)
            {

                P_rLO = new PointDF(this, "Druckbereich LO", 0, 0, true);
                AllPoints.AddIfNotExists(P_rLO);

                P_rRO = new PointDF(this, "Druckbereich RO", 0, 0, true);
                AllPoints.AddIfNotExists(P_rRO);

                P_rRU = new PointDF(this, "Druckbereich RU", 0, 0, true);
                AllPoints.AddIfNotExists(P_rRU);

                P_rLU = new PointDF(this, "Druckbereich LU", 0, 0, true);
                AllPoints.AddIfNotExists(P_rLU);
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
            OnDoInvalidate();
        }


        protected override void OnItemRemoving(BasicPadItem item)
        {
            item.Changed -= Item_Changed;
            item.PointOrRelationsChanged -= Item_PointOrRelationsChanged;


            base.OnItemRemoving(item);
            AllPoints.RemoveRange(item.Points);
            AllRelations.RemoveRange(item.Relations);
        }


        protected override void OnListOrItemChanged()
        {
            base.OnListOrItemChanged();
            IsSaved = false;
            InvalidateOrder();
            OnDoInvalidate();
        }


        public List<PointDF> ConnectsWith(PointDF Point, bool CheckX, bool IgnoreInternals)
        {

            var Points = new List<PointDF>();
            Points.Add(Point);

            var Ist = -1;

            // Nur, wenn eine Beziehung gut ist, kann man mit sicherheit sagen, daß das zusammenhängt. Deswegen auch ein Performs test

            do
            {
                Ist += 1;
                if (Ist >= Points.Count) { break; }


                foreach (var ThisRelation in AllRelations)
                {
                    if (ThisRelation != null && ThisRelation.Points.Contains(Points[Ist]) && ThisRelation.Performs(false) && ThisRelation.Connects(CheckX))
                    {


                        if (!IgnoreInternals || !ThisRelation.IsInternal())
                        {
                            Points.AddIfNotExists(ThisRelation.Points);
                        }
                    }
                }
            } while (true);



            return Points;
        }







        public new string ToString()
        {

            RepairAll(2, false);

            var t = "{";


            if (!string.IsNullOrEmpty(ID)) { t = t + "ID=" + ID.ToNonCritical() + ", "; }

            if (!string.IsNullOrEmpty(Caption)) { t = t + "Caption=" + Caption.ToNonCritical() + ", "; }

            if (SheetStyle != null) { t = t + "Style=" + SheetStyle.CellFirstString().ToNonCritical() + ", "; }

            if (SheetStyleScale < 0.1m) { SheetStyleScale = 1.0m; }

            if (Math.Abs(SheetStyleScale - 1) > 0.001m) { t = t + "FontScale=" + SheetStyleScale + ", "; }

            if (SheetSizeInMM.Width > 0 && SheetSizeInMM.Height > 0)
            {
                t = t + "SheetSize=" + SheetSizeInMM + ", ";
                t = t + "PrintArea=" + RandinMM + ", ";
            }

            t = t + "DPI=" + DPI + ", ";

            t = t + "Items={";


            foreach (var Thisitem in this)
            {
                if (Thisitem != null)
                {
                    t = t + "Item=" + Thisitem.ToString() + ", ";
                }
            }

            t = t.TrimEnd(", ") + "}, ";






            //t = t + "Grid=" + _Grid + ", ";
            //t = t + "GridShow=" + _GridShow + ", ";
            //t = t + "GridSnap=" + _Gridsnap + ", ";

            //Dim One As Boolean

            foreach (var ThisRelation in AllRelations)
            {
                if (ThisRelation != null)
                {
                    if (!ThisRelation.IsInternal() && ThisRelation.IsOk(false))
                    {
                        t = t + "Relation=" + ThisRelation + ", ";
                    }
                }
            }


            return t.TrimEnd(", ") + "}";

        }



        public Bitmap ToBitmap(decimal Scale)
        {
            var r = MaxBounds(null);
            if (r.Width == 0) { return null; }


            do
            {
                if ((int)(r.Width * Scale) > 15000)
                {
                    Scale = Scale * 0.8m;
                }
                else if ((int)(r.Height * Scale) > 15000)
                {
                    Scale = Scale * 0.8m;
                }
                else if ((int)(r.Height * Scale) * (int)(r.Height * Scale) > 90000000)
                {
                    Scale = Scale * 0.8m;
                }
                else
                {
                    break;
                }

                modAllgemein.CollectGarbage();
            } while (true);



            var I = new Bitmap((int)(r.Width * Scale), (int)(r.Height * Scale));


            using (var gr = Graphics.FromImage(I))
            {
                gr.Clear(Color.White);

                if (!Draw(gr, Scale, r.Left * Scale, r.Top * Scale, Size.Empty, true, null))
                {
                    return ToBitmap(Scale);
                }

            }
            return I;
        }

        public bool Draw(Graphics GR, decimal cZoom, decimal MoveX, decimal MoveY, Size SizeOfParentControl, bool ForPrinting, List<BasicPadItem> VisibleItems)
        {


            try
            {
                if (SheetStyle == null || SheetStyleScale < 0.1m) { return true; }


                foreach (var thisItem in this)
                {
                    if (thisItem != null)
                    {
                        if (VisibleItems == null || VisibleItems.Contains(thisItem))
                        {
                            thisItem.Draw(GR, cZoom, MoveX, MoveY, 0, SizeOfParentControl, ForPrinting);
                        }
                    }
                }
                return true;
            }
            catch
            {
                modAllgemein.CollectGarbage();
                return false;
            }


        }


        protected RectangleDF MaxBounds()
        {
            return MaxBounds(null);
        }

        internal RectangleDF MaxBounds(List<BasicPadItem> ZoomItems)
        {

            RectangleDF r;
            if (Count == 0)
            {
                r = new RectangleDF(0, 0, 0, 0);
            }
            else
            {
                r = MaximumBounds(ZoomItems);
            }

            if (SheetSizeInMM.Width > 0 && SheetSizeInMM.Height > 0)
            {

                var X1 = Math.Min(r.Left, 0);
                var y1 = Math.Min(r.Top, 0);


                var x2 = Math.Max(r.Right, modConverter.mmToPixel((decimal)SheetSizeInMM.Width, ItemCollectionPad.DPI));
                var y2 = Math.Max(r.Bottom, modConverter.mmToPixel((decimal)SheetSizeInMM.Height, ItemCollectionPad.DPI));

                return new RectangleDF(X1, y1, x2 - X1, y2 - y1);
            }

            return r;


        }


        public bool ParseVariable(string VariableName, enValueType ValueType, string Value)
        {

            var did = false;

            foreach (var thisItem in this)
            {
                if (thisItem is ICanHaveColumnVariables variables)
                {
                    if (variables.ParseVariable(VariableName, ValueType, Value))
                    {
                        thisItem.RecomputePointAndRelations();
                        did = true;
                    }
                }
            }

            if (did) { OnDoInvalidate(); }
            return did;
        }

        public void ParseVariableAndSpecialCodes(RowItem row)
        {

            foreach (var thiscolumnitem in row.Database.Column)
            {
                if (thiscolumnitem != null)
                {
                    ParseVariable(thiscolumnitem.Name, thiscolumnitem, row);
                }
            }

            ParseSpecialCodes();
        }

        private void ParseVariable(string VariableName, ColumnItem Column, RowItem Row)
        {


            switch (Column.Format)
            {
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

                    var f = Column.BestFile(Row.CellGetString(Column), false);

                    if (FileExists(f))
                    {
                        if (Column.MultiLine)
                        {
                            ParseVariable(VariableName, enValueType.Text, f);
                        }
                        else
                        {
                            var x = modConverter.FileToString(f);
                            ParseVariable(VariableName, enValueType.BinaryImage, x);
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




        public bool ParseSpecialCodes()
        {
            var did = false;

            foreach (var thisItem in this)
            {
                if (thisItem is ICanHaveColumnVariables variables)
                {
                    if (variables.ParseSpecialCodes())
                    {
                        thisItem.RecomputePointAndRelations();
                        did = true;
                    }
                }
            }

            if (did) { OnDoInvalidate(); }

            return did;
        }

        public bool ResetVariables()
        {
            var did = false;

            foreach (var thisItem in this)
            {
                if (thisItem is ICanHaveColumnVariables variables)
                {
                    if (variables.ResetVariables())
                    {
                        thisItem.RecomputePointAndRelations();
                        did = true;
                    }
                }
            }


            if (did) { OnDoInvalidate(); }
            return did;

        }




        internal Rectangle DruckbereichRect()
        {
            if (P_rLO == null) { return new Rectangle(0, 0, 0, 0); }
            return new Rectangle((int)P_rLO.X, (int)P_rLO.Y, (int)(P_rRU.X - P_rLO.X), (int)(P_rRU.Y - P_rLO.Y));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Level">Level 0 = Hart / Reparier alles mit Gewalt; 
        /// Level 1 = Normal / Reparier nur die neuen Sachen;
        ///  Level 2 = Leicht / Reparier nur die neuen Sachen mit schnelleren Abbruchbedingungen</param>
        /// <param name="AllowBigChanges"></param>
        /// <returns></returns>

        public bool RepairAll(int Level, bool AllowBigChanges)
        {
            InvalidateOrder();

            if (Level == 0)
            {
                //RepairAll_OldItemc = Itemc + 1; // Löst eine Kettenreaktion aus
                RecomputePointAndRelations();
            }

            return PerformAll(Level, AllowBigChanges);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ThisItemCol"></param>
        /// <param name="Level">             Level 0 gibt es nicht;
        /// Level 1 = Normal / Reparier nur die neuen Sachen ;
        /// Level 2 = Leicht / Reparier nur die neuen Sachen mit schnelleren Abbruchbedingungen</param>
        /// <param name="AllowBigChanges"></param>
        /// <returns></returns>
        public bool PerformAll(int Level, bool AllowBigChanges)
        {

            var L = new List<string>();
            var Methode = 0;

            InvalidateOrder();
            ComputeOrders(null);

            do
            {
                var tmp = "";

                foreach (var ThisRelation in AllRelations)
                {

                    if (ThisRelation.Performs(true))
                    {
                        ThisRelation.Computed = true;
                    }
                    else
                    {
                        ThisRelation.Computed = false;
                        tmp = tmp + ThisRelation.Order + ";";
                    }

                }

                if (string.IsNullOrEmpty(tmp)) { return true; }



                if (L.Contains(tmp))
                {
                    if (Level == 2) { return false; }
                    if (Methode == 2) { return false; }

                    Methode += 1;
                    Relations_Optimize();
                    RecomputePointAndRelations();
                    ComputeOrders(null);
                    L.Clear();
                }
                else
                {
                    L.Add(tmp);
                }

                foreach (var ThisRelation in AllRelations)
                {
                    if (!ThisRelation.Computed)
                    {
                        ThisRelation.MakePointKonsistent(LowestOrder(ThisRelation.Points), AllowBigChanges);
                    }
                }

            } while (true);

        }

        public void ComputeOrders(List<PointDF> Sel_P)
        {
            if (_OrdersValid) { return; }

            if (ComputeOrders_isin) { return; }
            ComputeOrders_isin = true;


            RemoveInvalidPoints();
            RemoveInvalidRelations();

            ComputePointOrder(Sel_P);
            ComputeRelationOrder();

            _OrdersValid = true;

            ComputeOrders_isin = false;
        }
        public void Relations_Optimize()
        {
            if (NotPerforming(true) > 0) { return; }


            var Cb = new List<PointDF>();
            var DobR = new List<clsPointRelation>();


            foreach (var thisPoint in AllPoints)
            {
                var CX = ConnectsWith(thisPoint, true, true);
                var CY = ConnectsWith(thisPoint, false, true);

                // Ermitteln, die auf X und Y miteinander verbunden sind
                Cb.Clear();
                foreach (var thisPoint2 in CX)
                {
                    if (CY.Contains(thisPoint2)) { Cb.Add(thisPoint2); }
                }


                if (Cb.Count > 1)
                {

                    DobR.Clear();
                    foreach (var ThisRelation in AllRelations)
                    {


                        // Wenn Punkte nicht direct verbunden sind, aber trotzdem Fix zueinander, die Beziehung optimieren
                        if (ThisRelation.RelationType == enRelationType.WaagerechtSenkrecht && !ThisRelation.IsInternal())
                        {
                            if (Cb.Contains(ThisRelation.Points[0]) && Cb.Contains(ThisRelation.Points[1]))
                            {
                                ThisRelation.RelationType = enRelationType.PositionZueinander;
                                ThisRelation.OverrideSavedRichtmaß(false, false);
                                InvalidateOrder();
                                Relations_Optimize();
                                return;
                            }
                        }


                        // Für nachher, die doppelten fixen Beziehungen merken
                        if (ThisRelation.RelationType == enRelationType.PositionZueinander)
                        {
                            if (Cb.Contains(ThisRelation.Points[0]) && Cb.Contains(ThisRelation.Points[1])) { DobR.Add(ThisRelation); }
                        }


                    }


                    // Und nun beziehungen löschen, die auf gleiche Objecte zugreifen
                    if (DobR.Count > 1)
                    {
                        foreach (var R1 in DobR)
                        {
                            // Mindestens eine muss external sein!!!
                            if (!R1.IsInternal())
                            {
                                foreach (var R2 in DobR)
                                {
                                    if (!R1.SinngemäßIdenitisch(R2))
                                    {

                                        if (R1.Points[0].Parent == R2.Points[0].Parent && R1.Points[1].Parent == R2.Points[1].Parent)
                                        {
                                            AllRelations.Remove(R1);
                                            InvalidateOrder();
                                            Relations_Optimize();
                                            return;
                                        }

                                        if (R1.Points[0].Parent == R2.Points[1].Parent && R1.Points[1].Parent == R2.Points[0].Parent)
                                        {
                                            AllRelations.Remove(R1);
                                            InvalidateOrder();
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
            foreach (var r1 in AllRelations)
            {
                if (!r1.IsInternal())
                {
                    foreach (var r2 in AllRelations)
                    {
                        if (!r1.SinngemäßIdenitisch(r2) && !r2.IsInternal())
                        {
                            if (r1.SinngemäßIdenitisch(r2))
                            {
                                AllRelations.Remove(r2);
                                Relations_Optimize();
                                return;

                            }

                            if (r1.UsesSamePoints(r2))
                            {
                                switch (r1.RelationType)
                                {
                                    case enRelationType.PositionZueinander:
                                        // Beziehungen mit gleichen punkten, aber einer mächtigen PositionZueinander -> andere löschen
                                        AllRelations.Remove(r2);
                                        Relations_Optimize();
                                        return;
                                    case enRelationType.WaagerechtSenkrecht when r2.RelationType == enRelationType.WaagerechtSenkrecht && r1.Richtmaß() != r2.Richtmaß():
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

        public void InvalidateOrder()
        {
            _OrdersValid = false;
        }

        private void AllPoints_ListOrItemChanged(object sender, System.EventArgs e)
        {
            InvalidateOrder();
        }

        private void AllRelations_ListOrItemChanged(object sender, System.EventArgs e)
        {
            InvalidateOrder();
        }

        public PointDF Getbetterpoint(double X, double Y, PointDF notPoint, bool MustUsableForAutoRelation)
        {

            foreach (var thispoint in AllPoints)
            {

                if (thispoint != null)
                {

                    if (!MustUsableForAutoRelation || thispoint.CanUsedForAutoRelation)
                    {

                        if (thispoint != notPoint)
                        {
                            if (Math.Abs((double)thispoint.X - X) < 0.01 && Math.Abs((double)thispoint.Y - Y) < 0.01) { return GetPointWithLowerIndex(notPoint, thispoint, true); }
                        }

                    }

                }
            }

            return null;
        }

        public PointDF GetPointWithLowerIndex(PointDF NotPoint, PointDF ErsatzFür, bool MustUsableForAutoRelation)
        {
            if (NotPoint != null && NotPoint.Parent == ErsatzFür.Parent) { return ErsatzFür; }

            var p = ErsatzFür;

            foreach (var thispoint in AllPoints)
            {
                if (thispoint != null)
                {
                    if (!MustUsableForAutoRelation || thispoint.CanUsedForAutoRelation)
                    {
                        if (thispoint != NotPoint && thispoint != ErsatzFür)
                        {
                            if (Math.Abs(thispoint.X - ErsatzFür.X) < 0.01m && Math.Abs(thispoint.Y - ErsatzFür.Y) < 0.01m)
                            {
                                if (thispoint.Order < p.Order) { p = thispoint; }
                            }
                        }
                    }
                }
            }

            return p;
        }


        private void ComputePointOrder(List<PointDF> Sel_P)
        {
            var Modus = 0;
            var Count = 1;


            var _Points = new List<PointDF>();
            _Points.AddRange(AllPoints);


            foreach (var Thispoint in _Points)
            {
                Thispoint.Order = int.MaxValue;
            }

            _Points.Sort();

            var RelationNone = new List<clsPointRelation>();
            var RelationY = new List<clsPointRelation>();
            var RelationX = new List<clsPointRelation>();
            var RelationXY = new List<clsPointRelation>();


            foreach (var thisRelation in AllRelations)
            {
                if (thisRelation.Connects(true))
                {
                    if (thisRelation.Connects(false))
                    {
                        RelationXY.Add(thisRelation);
                    }
                    else
                    {
                        RelationX.Add(thisRelation);
                    }
                }
                else
                {
                    if (thisRelation.Connects(false))
                    {
                        RelationY.Add(thisRelation);
                        //Stop
                    }
                    else
                    {
                        RelationNone.Add(thisRelation);
                    }
                }
            }


            do
            {
                PointDF DidPoint = null;
                clsPointRelation DidRel = null;


                foreach (var Thispoint in _Points)
                {
                    switch (Modus)
                    {
                        case 0: // Fixpunkte hinzufgen
                            if (Thispoint.PositionFix)
                            {
                                Thispoint.Order = Count;
                                DidPoint = Thispoint;
                            }
                            break;

                        case 1: // X und Y Fix hinzufügen

                            foreach (var thisRelation in RelationXY)
                            {
                                if (thisRelation.NeedCount(Thispoint))
                                {
                                    if (thisRelation.Connects(true) && thisRelation.Connects(false))
                                    {
                                        Thispoint.Order = Count;
                                        DidPoint = Thispoint;
                                        DidRel = thisRelation;
                                        break;
                                    }
                                }
                            }
                            break;

                        case 2: // Fixe Y-Punkte hinzufügen
                            foreach (var thisRelation in RelationY)
                            {
                                if (thisRelation.NeedCount(Thispoint))
                                {
                                    if (thisRelation.Connects(false))
                                    {
                                        Thispoint.Order = Count;
                                        DidPoint = Thispoint;
                                        DidRel = thisRelation;
                                        break;
                                    }
                                }
                            }

                            break;

                        case 3: // Fixe X-Punkte hinzufügen
                            foreach (var thisRelation in RelationX)
                            {
                                if (thisRelation.NeedCount(Thispoint))
                                {
                                    if (thisRelation.Connects(true))
                                    {
                                        Thispoint.Order = Count;
                                        DidPoint = Thispoint;
                                        DidRel = thisRelation;
                                        break;
                                    }
                                }
                            }
                            break;

                        case 4: // Punkte hinzufügen, die in einer Beziehung sind UND ein Punkt bereits einen Order hat

                            foreach (var thisRelation in RelationNone)
                            {
                                if (thisRelation.NeedCount(Thispoint))
                                {
                                    Thispoint.Order = Count;
                                    DidPoint = Thispoint;
                                    DidRel = thisRelation;
                                    break;
                                }
                            }
                            break;

                        case 5: // Selectierte Punkte bevorzugen
                            if (Sel_P.Contains(Thispoint))
                            {
                                Thispoint.Order = Count;
                                DidPoint = Thispoint;
                            }

                            break;

                        case 6: // Der gute Rest
                            Thispoint.Order = Count;
                            DidPoint = Thispoint;
                            break;

                        default:
                            return;
                    }

                    if (DidPoint != null) { break; }
                }



                if (DidPoint != null)
                {
                    _Points.Remove(DidPoint);
                    Count += 1;
                    if (Modus > 1) { Modus = 1; }
                }
                else
                {
                    Modus += 1;
                }

                if (Modus == 5)
                {
                    if (Sel_P == null || Sel_P.Count == 0) { Modus += 1; }
                }

                if (_Points.Count == 0) { return; }



                if (DidRel != null && DidRel.AllPointsHaveOrder())
                {
                    RelationNone.Remove(DidRel);
                    RelationY.Remove(DidRel);
                    RelationX.Remove(DidRel);
                    RelationXY.Remove(DidRel);
                }


            } while (true);

        }
        public void ComputeRelationOrder()
        {
            var Count = 0;

            // Zurücksetzen ---- 
            foreach (var ThisRelation in AllRelations)
            {
                ThisRelation.Order = -1;
            }


            for (var Durch = 0; Durch <= 1; Durch++)
            {

                do
                {
                    clsPointRelation NextRel = null;
                    var RelPO = int.MaxValue;

                    foreach (var ThisRelation in AllRelations)
                    {
                        if (ThisRelation.Order < 0)
                        {
                            if (Durch > 0 || ThisRelation.IsInternal())
                            {
                                if (LowestOrder(ThisRelation.Points) < RelPO)
                                {
                                    NextRel = ThisRelation;
                                    RelPO = LowestOrder(ThisRelation.Points);
                                }
                            }
                        }
                    }

                    if (NextRel == null) { break; }

                    Count += 1;
                    NextRel.Order = Count;
                } while (true);

            }

            AllRelations.Sort();
        }

        public int LowestOrder(ListExt<PointDF> ThisPoints)
        {
            var l = int.MaxValue;

            foreach (var Thispouint in ThisPoints)
            {
                l = Math.Min(l, Thispouint.Order);
            }

            return l;
        }

        public void SaveAsBitmap(string Filename)
        {

            var i = ToBitmap(1);

            if (i == null) { return; }


            switch (Filename.FileSuffix().ToUpper())
            {

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