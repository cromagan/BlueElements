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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollection.Basics;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection
{
    public abstract class BasicPadItem : BasicItem, IParseable
    {
        // protected abstract bool ParseExplicit(KeyValuePair<string, string> pair);


        /// <summary>
        ///  Falls das Element über Kordinaten gesetzt werden kann, ist diese mit dieser Routine möglich.
        ///  Dabei werden Beziehungen komplett ignoriert!
        /// </summary>
        /// <remarks></remarks>
        public abstract void SetCoordinates(RectangleDF r);




        /// <summary>
        ///  Gibt die Punkte zurück, die ein Modifizieren des Objektes erlauben.
        /// </summary>
        /// <remarks></remarks>
        public abstract List<PointDF> PointList();



        /// <summary>
        /// Erstellt alle Internen Beziehungen
        /// </summary>
        /// <remarks></remarks>
        public abstract void GenerateInternalRelation(List<clsPointRelation> relations);


        protected abstract void KeepInternalLogic();


        /// <summary>
        /// Prüft, ob die angegebenen Koordinaten das Element berühren.
        /// Der Zoomfaktor wird nur benötigt, um Maßstabsunabhängige Punkt oder Linienberührungen zu berechnen.
        /// </summary>
        /// <remarks></remarks>
        public abstract bool Contains(PointF value, decimal zoomfactor);


        /// <summary>
        /// Gibt für das aktuelle Item das "Kontext-Menü" zurück.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public abstract List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e);

        public abstract void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu);

        protected abstract void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting);

        protected abstract string ClassId();


        /// <summary>
        /// Gibt den Bereich zurück, den das Element benötigt, um komplett dargestellt zu werden. Unabhängig von der aktuellen Ansicht.
        /// </summary>
        /// <remarks></remarks>
        public abstract RectangleDF UsedArea();


        /// <summary>
        /// Soll es gedruckt werden?
        /// </summary>
        /// <remarks></remarks>
        protected bool _PrintMe = true;

        private readonly List<clsPointRelation> _InternalRelations = new List<clsPointRelation>();

        private PadStyles _Style = PadStyles.Undefiniert;

        public List<string> RemoveToo = new List<string>();


        public ItemCollectionPad Parent
        {
            get
            {
                return (ItemCollectionPad)_parent;
            }

        }

        public bool PrintMe
        {
            get
            {
                return _PrintMe;
            }
            set
            {
                if (_PrintMe == value) { return; }
                _PrintMe = value;
                OnChanged();
            }
        }

        public PadStyles Style
        {
            get
            {
                return _Style;
            }
            set
            {
                if (_Style == value) { return; }
                _Style = value;
                OnChanged();
            }

        }


        protected BasicPadItem(string internalname) : base(internalname) { }


        public virtual bool ParseThis(string tag, string value)
        {
            switch (tag)
            {
                case "classid":
                case "type":
                case "enabled":
                case "checked":
                    return true;

                case "tag":
                    _Tags.Add(value.FromNonCritical());
                    return true;

                case "print":
                    _PrintMe = value.FromPlusMinus();
                    return true;

                case "point":
                    foreach (var ThisPoint in PointList())
                    {
                        if (value.Contains("Name=" + ThisPoint.Name + ","))
                        {
                            ThisPoint.Parse(value);
                            ThisPoint.Parent = this;
                        }
                    }
                    return true;

                // case "format": // = Textformat!!!
                case "design":
                case "style":
                    _Style = (PadStyles)int.Parse(value);
                    return true;

                case "removetoo":
                    RemoveToo.AddRange(value.FromNonCritical().SplitByCR());
                    return true;


                case "internalname":
                    if (value != Internal)
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Namen unterschiedlich: " + value + " / " + Internal);
                    }
                    return true;

                default:
                    return false;
            }

        }


        public bool IsParsing { get; private set; }


        public void Parse(List<KeyValuePair<string, string>> ToParse)
        {
            IsParsing = true;

            foreach (var pair in ToParse)
            {

                if (!ParseThis(pair.Key, pair.Value))
                {
                    Develop.DebugPrint(enFehlerArt.Warnung, "Kann nicht geparsed werden: " + pair.Key + "/" + pair.Value + "/" + ToParse);
                }


            }
            IsParsing = false;
        }


        public override string ToString()
        {

            var t = "{";

            t = t + "ClassID=" + ClassId() + ", ";
            t = t + "InternalName=" + Internal.ToNonCritical() + ", ";

            if (_Tags.Count > 0)
            {
                foreach (var ThisTag in _Tags)
                {
                    t = t + "Tag=" + ThisTag.ToNonCritical() + ", ";
                }
            }


            t = t + "Style=" + (int)_Style + ", ";
            t = t + "Print=" + _PrintMe.ToPlusMinus() + ", ";

            foreach (var ThisPoint in PointList())
            {
                t = t + "Point=" + ThisPoint + ", ";
            }


            if (RemoveToo.Count > 0) { t = t + "RemoveToo=" + RemoveToo.JoinWithCr().ToNonCritical() + ", "; }


            return t.Trim(", ") + "}";
        }


        public void InDenVordergrund()
        {
            ((ItemCollectionPad)Parent)?.InDenVordergrund(this);
        }

        public void InDenHintergrund()
        {
            ((ItemCollectionPad)Parent)?.InDenHintergrund(this);
        }

        public void EineEbeneNachVorne()
        {
            if (Parent == null) { return; }

            var i2 = Next();
            if (i2 != null)
            {
                var tempVar = this;
                ((ItemCollectionPad)Parent).Swap(tempVar, i2);
            }
        }

        public void EineEbeneNachHinten()
        {
            if (Parent == null) { return; }
            var i2 = Previous();
            if (i2 != null)
            {
                var tempVar = this;
                ((ItemCollectionPad)Parent).Swap(tempVar, i2);
            }
        }


        public List<clsPointRelation> RelationList()
        {
            if (_InternalRelations == null || _InternalRelations.Count == 0)
            {
                KeepInternalLogic();
                GenerateInternalRelation(_InternalRelations);
            }

            return _InternalRelations;
        }


        public void RecomputePointAndRelations()
        {
            KeepInternalLogic();

            if (_InternalRelations == null || _InternalRelations.Count == 0)
            {
                GenerateInternalRelation(_InternalRelations);
            }
            else
            {
                foreach (var ThisRelation in _InternalRelations)
                {
                    ThisRelation.InitRelationData(false);
                }
            }
        }


        public void ClearInternalRelations()
        {
            //foreach (clsPointRelation thisrelation in _InternalRelations)
            //{
            //    if (!thisrelation.IsDisposed()) { thisrelation.Dispose(); }
            //}
            _InternalRelations.Clear();
        }



        public void Draw(Graphics GR, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {
            if (Parent == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Parent nicht definiert"); }

            if (ForPrinting && !_PrintMe) { return; }

            var DCoordinates = UsedArea().ZoomAndMoveRect(cZoom, MoveX, MoveY);


            if (Parent == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Parent = null"); }

            if (!IsInDrawingArea(DCoordinates, SizeOfParentControl)) { return; }

            DrawExplicit(GR, DCoordinates, cZoom, MoveX, MoveY, vState, SizeOfParentControl, ForPrinting);


            if (!_PrintMe)
            {

                if (IsInDrawingArea(DCoordinates, SizeOfParentControl))
                {
                    GR.DrawImage(QuickImage.Get("Drucker|16||1").BMP, DCoordinates.PointOf(enAlignment.Top_Left));
                }
            }
        }


        internal BasicPadItem Previous()
        {
            var ItemCount = Parent.IndexOf(this);
            if (ItemCount < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }

            do
            {
                ItemCount -= 1;
                if (ItemCount < 0) { return null; }
                if (Parent[ItemCount] != null) { return Parent[ItemCount]; }
            } while (true);

        }

        internal BasicPadItem Next()
        {


            var ItemCount = Parent.IndexOf(this);
            if (ItemCount < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Item im SortDefinition nicht enthalten"); }

            do
            {
                ItemCount += 1;
                if (ItemCount >= Parent.Count) { return null; }
                if (Parent[ItemCount] != null) { return Parent[ItemCount]; }
            } while (true);

        }





        public void DrawOutline(Graphics GR, decimal cZoom, decimal MoveX, decimal MoveY, Color c)
        {
            GR.DrawRectangle(new Pen(c), UsedArea().ZoomAndMoveRect(cZoom, MoveX, MoveY));
        }

        protected bool IsInDrawingArea(Rectangle DrawingKoordinates, Size SizeOfParentControl)
        {
            if (SizeOfParentControl.IsEmpty || SizeOfParentControl.Width == 0 || SizeOfParentControl.Height == 0) { return true; }
            return DrawingKoordinates.IntersectsWith(new Rectangle(Point.Empty, SizeOfParentControl));
        }

        public void Parse(string ToParse)
        {
            Parse(ToParse.GetAllTags());
        }
    }
}