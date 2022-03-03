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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlueControls.ItemCollection {

    public static class BasicListItemExtensions {

        #region Methods

        public static List<string> ToListOfString(this List<BasicListItem> Items) {
            List<string> w = new();
            if (Items == null) { return w; }

            w.AddRange(from ThisItem in Items where ThisItem != null where !string.IsNullOrEmpty(ThisItem.Internal) select ThisItem.Internal);
            return w;
        }

        #endregion
    }

    public abstract class BasicListItem : IComparable, ICloneable {

        #region Fields

        public Rectangle Pos;

        /// <summary>
        /// Falls eine Spezielle Information gespeichert und zurückgegeben werden soll
        /// </summary>
        /// <remarks></remarks>
        public object Tag;

        /// <summary>
        /// Ist das Item enabled?
        /// </summary>
        /// <remarks></remarks>
        protected bool _Enabled = true;

        /// <summary>
        /// Ist das Item markiert/selektiert?
        /// </summary>
        /// <remarks></remarks>
        private bool _Checked;

        private ItemCollectionList? _Parent;
        private Size _SizeUntouchedForListBox = Size.Empty;

        #endregion

        #region Constructors

        protected BasicListItem(string internalname) {
            Internal = string.IsNullOrEmpty(internalname) ? BasicPadItem.UniqueInternal() : internalname;
            if (string.IsNullOrEmpty(Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
            _Checked = false;
            Pos = Rectangle.Empty;
            UserDefCompareKey = string.Empty;
        }

        #endregion

        #region Properties

        public bool Checked {
            get => _Checked;
            set {
                if (Parent == null) {
                    _Checked = value;
                } else {
                    Parent?.SetNewCheckState(this, value, ref _Checked);
                }
            }
        }

        public bool Enabled {
            get => _Enabled;
            set {
                if (_Enabled == value) { return; }
                _Enabled = value;
                Parent?.OnDoInvalidate();
            }
        }

        public string Internal { get; set; }

        public bool IsCaption { get; protected set; }

        public ItemCollectionList? Parent {
            get => _Parent;
            set {
                if (_Parent == null || _Parent == value) {
                    _Parent = value;
                    return;
                }

                Develop.DebugPrint(enFehlerArt.Fehler, "Parent Fehler!");
            }
        }

        public abstract string QuickInfo { get; }

        public string UserDefCompareKey { get; set; }

        #endregion

        #region Methods

        public abstract object Clone();

        ///// <summary>
        ///// Klont das aktuelle Objekt (es wird ein neues Objekt des gleichen Typs erstellt) und fügt es in die angegebene ItemCollection hinzu
        ///// </summary>
        ///// <param name="newParent"></param>
        //public virtual void CloneToNewCollection(ItemCollectionList newParent) => Develop.DebugPrint_RoutineMussUeberschriebenWerden();

        public void CloneBasicStatesFrom(BasicListItem sourceItem) {
            Checked = sourceItem.Checked;
            Enabled = sourceItem.Enabled;
            Tag = sourceItem.Tag;
            UserDefCompareKey = sourceItem.UserDefCompareKey;
            Internal = sourceItem.Internal;
            IsCaption = sourceItem.IsCaption;
        }

        public string CompareKey() {
            if (!string.IsNullOrEmpty(UserDefCompareKey)) {
                if (Convert.ToChar(UserDefCompareKey.Substring(0, 1)) < 32) { Develop.DebugPrint("Sortierung inkorrekt: " + UserDefCompareKey); }
                return UserDefCompareKey + Constants.FirstSortChar + Parent?.IndexOf(this).ToString(Constants.Format_Integer6);
            }
            return GetCompareKey();
        }

        public int CompareTo(object obj) {
            if (obj is BasicListItem tobj) {
                return CompareKey().CompareTo(tobj.CompareKey());
            }

            Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!");
            return 0;
        }

        public bool Contains(int x, int y) => Pos.Contains(x, y);

        public void Draw(Graphics GR, int xModifier, int YModifier, enDesign controldesign, enDesign itemdesign, enStates vState, bool DrawBorderAndBack, string FilterText, bool Translate) {
            if (Parent == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Parent nicht definiert"); }
            if (itemdesign == enDesign.Undefiniert) { return; }
            Rectangle PositionModified = new(Pos.X - xModifier, Pos.Y - YModifier, Pos.Width, Pos.Height);
            DrawExplicit(GR, PositionModified, itemdesign, vState, DrawBorderAndBack, Translate);
            if (DrawBorderAndBack) {
                if (!string.IsNullOrEmpty(FilterText) && !FilterMatch(FilterText)) {
                    var c1 = Skin.Color_Back(controldesign, enStates.Standard); // Standard als Notlösung, um nicht doppelt checken zu müssen
                    c1 = c1.SetAlpha(160);
                    GR.FillRectangle(new SolidBrush(c1), PositionModified);
                }
            }
        }

        public virtual bool FilterMatch(string FilterText) => Internal.ToUpper().Contains(FilterText.ToUpper());

        public abstract int HeightForListBox(enBlueListBoxAppearance style, int columnWidth);

        public virtual bool IsClickable() => !IsCaption;

        public void SetCoordinates(Rectangle r) {
            Pos = r;
            Parent?.OnDoInvalidate();
        }

        public Size SizeUntouchedForListBox() {
            if (_SizeUntouchedForListBox.IsEmpty) {
                _SizeUntouchedForListBox = ComputeSizeUntouchedForListBox();
            }
            return _SizeUntouchedForListBox;
        }

        protected abstract Size ComputeSizeUntouchedForListBox();

        protected abstract void DrawExplicit(Graphics gr, Rectangle positionModified, enDesign itemdesign, enStates state, bool drawBorderAndBack, bool translate);

        protected abstract string GetCompareKey();

        #endregion

        //return null;
    }
}