#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
    public class ItemCollectionPad: ListExt<BasicPadItem>, IParseable
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
                if (_sheetStyle == value)
                {
                    return;
                }

                _sheetStyle = value;
                OnChanged();
            }
        }

        public decimal SheetStyleScale
        {
            get { return _sheetStyleScale; }

            set
            {
                if (_sheetStyleScale == value)
                {
                    return;
                }

                _sheetStyleScale = value;
                OnChanged();
            }
        }


        #region  Construktor + Initialize 

        public ItemCollectionPad()
        {
            Initialize();
        }

        private void Initialize()
        {
            Clear();
        }

        #endregion


        #region  Event-Deklarationen + Delegaten 

        public event EventHandler ItemsZOrderChanged;

        public event EventHandler Changed;

        #endregion


        internal void InDenVordergrund(BasicPadItem ThisItem)
        {
            if (IndexOf(ThisItem) == Count - 1)
            {
                return;
            }

            Remove(ThisItem);
            Add(ThisItem);

            OnItemsZOrderChanged();
        }

        internal void InDenHintergrund(BasicPadItem ThisItem)
        {
            if (IndexOf(ThisItem) == 0)
            {
                return;
            }

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
                        if (Internal.ToUpper() == ThisItem.Internal().ToUpper())
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

            Initialize();
            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "format": //_Format = DirectCast(Integer.Parse(pair.Value), enDataFormat)
                        break;

                    case "item":
                        AddByCode(pair.Value);
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


        private void AddByCode(string Code)
        {
            //TODO: Abspecken

            BasicPadItem i = null;

            if (Code.ToLower().StartsWith("{type=blueelements.clsitemtext,"))
            {
                i = new TextPadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.textitem,"))
            {
                i = new TextPadItem();
            }
            else if (Code.ToLower().StartsWith("{classid=text,"))
            {
                i = new TextPadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.clsitemdistanz,"))
            {
                i = new SpacerPadItem(this);
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.distanzitem,"))
            {
                i = new SpacerPadItem(this);
            }
            else if (Code.ToLower().StartsWith("{classid=spacer,"))
            {
                i = new SpacerPadItem(this);
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.clsitemimage,"))
            {
                i = new BitmapPadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.imageitem,"))
            {
                i = new BitmapPadItem();
            }
            else if (Code.ToLower().StartsWith("{classid=image,"))
            {
                i = new BitmapPadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.dimensionitem,"))
            {
                i = new DimensionPadItem();
            }
            else if (Code.ToLower().StartsWith("{classid=dimension,"))
            {
                i = new DimensionPadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.clsitemline,"))
            {
                i = new LinePadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.lineitem,"))
            {
                i = new LinePadItem();
            }
            else if (Code.ToLower().StartsWith("{classid=line,"))
            {
                i = new LinePadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.clsitempad,"))
            {
                i = new ChildPadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.childPaditem,"))
            {
                i = new ChildPadItem();
            }
            else if (Code.ToLower().StartsWith("{classid=childpad,"))
            {
                i = new ChildPadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.clsitemgrid,"))
            {
                i = new GridPadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.griditem,"))
            {
                i = new GridPadItem();
            }
            else if (Code.ToLower().StartsWith("{classid=grid,"))
            {
                i = new GridPadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.rowformulaitem,"))
            {
                i = new RowFormulaPadItem();
            }
            else if (Code.ToLower().StartsWith("{classid=row,"))
            {
                i = new RowFormulaPadItem();
            }
            else if (Code.ToLower().StartsWith("{type=blueelements.clsitemcomiccomp,"))
            {
                i = new ComicCompPadItem();
            }
            else if (Code.ToLower().StartsWith("{classid=comic,"))
            {
                i = new ComicCompPadItem();
            }
            else if (Code.ToLower().StartsWith("{classid=symbol,"))
            {
                i = new SymbolPadItem();
            }
            else
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Unbekanntes Item: " + Code);
            }

            if (i != null)
            {
                i.Parse(Code);
                Add(i);
            }
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

        public new void Add(BasicPadItem cItem)
        {
            if (string.IsNullOrEmpty(cItem.Internal()))
            {
                Develop.DebugPrint(enFehlerArt.Fehler,
                    "Der Auflistung soll ein Item hinzugefügt werden, welches keinen Namen hat " + cItem.Internal());
            }

            if (this[cItem.Internal()] != null)
            {
                Develop.DebugPrint(enFehlerArt.Fehler,
                    "Der Auflistung soll ein Item hinzugefügt werden, welches aber schon vorhanden ist: " +
                    cItem.Internal());
            }

            cItem.Parent = this;


            base.Add(cItem);
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
            if (cItem == null)
            {
                return;
            }

            base.Remove(cItem);

            foreach (var ThisToo in cItem.RemoveToo)
            {
                Remove(ThisToo);
            }
        }


        public RectangleDF MaximumBounds()
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
                    x1 = Math.Min(x1, ThisItem.UsedArea().Left);
                    y1 = Math.Min(y1, ThisItem.UsedArea().Top);

                    x2 = Math.Max(x2, ThisItem.UsedArea().Right);
                    y2 = Math.Max(y2, ThisItem.UsedArea().Bottom);
                    Done = true;
                }
            }


            if (!Done)
            {
                return new RectangleDF();
            }

            return new RectangleDF(x1, y1, x2 - x1, y2 - y1);
        }

        protected override void OnListOrItemChanged()
        {
            base.OnListOrItemChanged();
            OnChanged();
        }

        public void OnChanged()
        {
            if (IsParsing)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!");
                return;
            }

            Changed?.Invoke(this, System.EventArgs.Empty);
        }
    }
}