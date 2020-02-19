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

namespace BlueControls.ItemCollection
{
    public class ItemCollectionPad : BasicItemCollection<BasicPadItem>, IParseable
    {
        #region  Variablen-Deklarationen 

        public static readonly int DPI = 300;
        private RowItem _sheetStyle;
        private decimal _sheetStyleScale;

        #endregion

        public bool IsParsing { get; private set; }

        public RowItem SheetStyle
        {
            get { return _sheetStyle; }

            set
            {
                if (_sheetStyle == value) { return; }

                _sheetStyle = value;
                OnChanged();
            }
        }

        public decimal SheetStyleScale
        {
            get { return _sheetStyleScale; }

            set
            {
                if (_sheetStyleScale == value) { return; }

                _sheetStyleScale = value;
                OnChanged();
            }
        }


        #region  Construktor + Initialize 


        #endregion


        #region  Event-Deklarationen + Delegaten 

        public event EventHandler ItemsZOrderChanged;



        #endregion


        internal void InDenVordergrund(BasicPadItem ThisItem)
        {
            if (IndexOf(ThisItem) == Count - 1) { return; }

            Remove(ThisItem);
            Add(ThisItem);

            OnItemsZOrderChanged();
        }

        internal void InDenHintergrund(BasicPadItem ThisItem)
        {
            if (IndexOf(ThisItem) == 0) { return; }

            Remove(ThisItem);
            Insert(0, ThisItem);

            OnItemsZOrderChanged();
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


        internal void OnItemsZOrderChanged()
        {
            ItemsZOrderChanged?.Invoke(this, System.EventArgs.Empty);
        }

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

                        _sheetStyle = Skin.StyleDB.Row[pair.Value];
                        break;

                    case "sheetstylescale":
                        _sheetStyleScale = decimal.Parse(pair.Value);
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
            t = t + "SheetStyleScale=" + _sheetStyleScale + ", ";

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
            RecomputePointAndRelations();
        }



        public void DesignOrStyleChanged()
        {
            foreach (var thisItem in this)
            {
                thisItem?.DesignOrStyleChanged();
            }
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




    }
}