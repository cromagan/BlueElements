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
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.ItemCollection.Basics;
using System;
using System.Drawing;

namespace BlueControls.ItemCollection
{
    public abstract class BasicListItem : BasicItem, ICompareKey, IComparable
    {
        public abstract Size SizeUntouchedForListBox();



        public abstract int HeightForListBox(enBlueListBoxAppearance style, int columnWidth);


        protected abstract string GetCompareKey();

        protected abstract void DrawExplicit(Graphics gr, Rectangle positionModified, enDesign itemdesign, enStates state, bool drawBorderAndBack, bool translate);

        /// <summary>
        /// Falls eine Spezielle Information gespeichert und zurückgegeben werden soll
        /// </summary>
        /// <remarks></remarks>
        public object Tags;


        /// <summary>
        /// Ist das Item markiert/selektiert?
        /// </summary>
        /// <remarks></remarks>
        private bool _Checked;

        public Rectangle Pos;
        private string _UserDefCompareKey = "";

        public bool IsCaption { get; protected set; }

        /// <summary>
        /// Ist das Item enabled?
        /// </summary>
        /// <remarks></remarks>
        protected bool _Enabled = true;


        protected BasicListItem(string internalname) : base(internalname)
        {
            _Checked = false;
            Pos = new Rectangle(0, 0, 0, 0);
            _UserDefCompareKey = string.Empty;
        }


        public virtual bool IsClickable()
        {
            return !IsCaption;
        }



        public bool Contains(int x, int y)
        {
            return Pos.Contains(x, y);
        }





        public void SetCoordinates(Rectangle r)
        {
            Pos = r;
            Parent?.OnNeedRefresh();
        }


        public bool Enabled
        {
            get
            {
                return _Enabled;
            }
            set
            {
                if (_Enabled == value) { return; }

                _Enabled = value;
                Parent?.OnNeedRefresh();
            }
        }


        public string CompareKey()
        {

            if (!string.IsNullOrEmpty(_UserDefCompareKey))
            {
                if (Convert.ToChar(_UserDefCompareKey.Substring(0, 1)) < 32) { Develop.DebugPrint("Sortierung inkorrekt: " + _UserDefCompareKey); }
                return _UserDefCompareKey + Constants.FirstSortChar + Parent.IndexOf(this).ToString(Constants.Format_Integer6);
            }

            return GetCompareKey();
        }


        public string UserDefCompareKey
        {
            get
            {
                return _UserDefCompareKey;
            }
            set
            {
                if (value == _UserDefCompareKey) { return; }
                _UserDefCompareKey = value;

                OnChanged();
            }
        }


        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                if (Parent == null) { Develop.DebugPrint(enFehlerArt.Warnung, "Parent == null!"); }

                Parent?.SetNewCheckState(this, value, ref _Checked);
                OnChanged();
            }
        }

        public ItemCollectionList Parent
        {
            get
            { return (ItemCollectionList)_parent; }

        }

        public void Draw(Graphics GR, int xModifier, int YModifier, enDesign controldesign, enDesign itemdesign, enStates vState, bool DrawBorderAndBack, string FilterText, bool Translate)
        {


            if (Parent == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Parent nicht definiert"); }
            if (itemdesign == enDesign.Undefiniert) { return; }


            var PositionModified = new Rectangle(Pos.X - xModifier, Pos.Y - YModifier, Pos.Width, Pos.Height);




            DrawExplicit(GR, PositionModified, itemdesign, vState, DrawBorderAndBack, Translate);


            if (DrawBorderAndBack)
            {
                if (!string.IsNullOrEmpty(FilterText) && !FilterMatch(FilterText))
                {
                    var c1 = Skin.Color_Back(controldesign, enStates.Standard); // Standard als Notlösung, um nicht doppelt checken zu müssen
                    c1 = c1.SetAlpha(160);
                    GR.FillRectangle(new SolidBrush(c1), PositionModified);
                }
            }

        }

        public int CompareTo(object obj)
        {
            if (obj is BasicListItem tobj)
            {
                // hierist es egal, ob es ein DoAlways ist oder nicht. Es sollen nur Bedingugen VOR Aktionen kommen
                return CompareKey().CompareTo(tobj.CompareKey());
            }
            else
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!");
                return 0;
            }
        }

        public virtual BasicListItem CloneToNewCollection(ItemCollectionList newParent)
        {
            Develop.DebugPrint_RoutineMussUeberschriebenWerden();
            return null;
        }

        public BasicListItem CloneToNewCollection(ItemCollectionList newParent, BasicListItem newItem)
        {
            if (newItem.Internal != Internal)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Clone fehlgeschlagen, Internal unterschiedlich");
            }

            newParent.Add(newItem);

            newItem.Checked = Checked; // Parent muss gesetz sein!
            newItem.Enabled = Enabled;
            newItem.Tags = Tags;
            newItem.UserDefCompareKey = UserDefCompareKey;

            return newItem;
        }

        public virtual bool FilterMatch(string FilterText)
        {
            if (Internal.ToUpper().Contains(FilterText.ToUpper())) { return true; }
            return false;
        }

    }
}
