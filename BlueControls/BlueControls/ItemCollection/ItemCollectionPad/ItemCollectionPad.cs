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
using BlueBasics.Interfaces;
using BlueDatabase;
using BlueBasics.Enums;
using System.ComponentModel;

namespace BlueControls.ItemCollection
{
    public class ItemCollectionPad : BasicItemCollection<BasicPadItem>, IParseable
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


        public readonly List<PointDF> AllPoints = new List<PointDF>();
        public readonly List<clsPointRelation> AllRelations = new List<clsPointRelation>();


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

                _SheetStyle = value;
                OnChanged();
            }
        }

        public decimal SheetStyleScale
        {
            get { return _SheetStyleScale; }

            set
            {
                if (_SheetStyleScale == value) { return; }

                _SheetStyleScale = value;
                OnChanged();
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


        //internal void OnItemsZOrderChanged()
        //{
        //    ItemsZOrderChanged?.Invoke(this, System.EventArgs.Empty);
        //}

        public void Parse(string ToParse)
        {
            IsParsing = true;
            ThrowEvents = false;

            Clear();
            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "format": //_Format = DirectCast(Integer.Parse(pair.Value), enDataFormat)
                        break;

                    case "item":
                        Add(BasicPadItem.NewByParsing(pair.Value));
                        break;

                    case "dpi":
                        if (int.Parse(pair.Value) != DPI)
                        {
                            Develop.DebugPrint("DPI Unterschied: " + DPI + " <> " + pair.Value);
                        }

                        break;

                    case "sheetstyle":
                        if (Skin.StyleDB == null)
                        {
                            Skin.InitStyles();
                        }

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

            ThrowEvents = true;
            IsParsing = false;
        }





        public new string ToString()
        {
            var t = "{";
            t = t + "DPI=" + DPI + ", ";
            t = t + "SheetStyle=" + SheetStyle.CellFirstString().ToNonCritical() + ", ";
            t = t + "SheetStyleScale=" + _SheetStyleScale + ", ";

            foreach (var Thisitem in this)
            {
                if (Thisitem != null)
                {
                    t = t + "Item=" + Thisitem + ", ";
                }
            }
            return t.TrimEnd(", ") + "}";
        }


        protected override void OnItemAdded(BasicPadItem item)
        {
            base.OnItemAdded(item);
            AllPoints.AddIfNotExists(item.Points);
            AllRelations.AddIfNotExists(item.Relations);

            IsSaved = false;


            item.Changed += Item_Changed;
            item.PointOrRelationsChanged += Item_PointOrRelationsChanged;
 
            RecomputePointAndRelations();
        }

        private void Item_PointOrRelationsChanged(object sender, System.EventArgs e)
        {

            var ni = (BasicPadItem)sender;


            AllPoints.AddIfNotExists(ni.Points);
            AllRelations.AddIfNotExists(ni.Relations);

            dddd // Punkte Löschen 


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




        public void GenPoints()
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
                AllPoints.AddIfNotExists(P_rLU);

                P_rRU = new PointDF(this, "Druckbereich RU", 0, 0, true);
                AllPoints.AddIfNotExists(P_rRU);

                P_rLU = new PointDF(this, "Druckbereich LU", 0, 0, true);
                AllPoints.AddIfNotExists(P_rRO);
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
            OnDoInvalidate();
        }

        public void OnDoInvalidate()
        {
            DoInvalidate?.Invoke(this, System.EventArgs.Empty); // Invalidate-Befehl weitergeben an untergeordnete Steuerelemente
        }



    }
}