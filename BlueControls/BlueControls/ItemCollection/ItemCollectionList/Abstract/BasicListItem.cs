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
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlueControls.ItemCollection.ItemCollectionList;

public static class BasicListItemExtensions {

    #region Methods

    public static List<string> ToListOfString(this List<BasicListItem>? items) {
        List<string> w = new();
        if (items == null) { return w; }

        w.AddRange(from thisItem in items where thisItem != null where !string.IsNullOrEmpty(thisItem.Internal) select thisItem.Internal);
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
    /// Ist das Item markiert/selektiert?
    /// </summary>
    /// <remarks></remarks>
    private bool _checked;

    /// <summary>
    /// Ist das Item enabled?
    /// </summary>
    /// <remarks></remarks>
    private bool _enabled;

    private ItemCollectionList? _parent;
    private Size _sizeUntouchedForListBox = Size.Empty;

    #endregion

    #region Constructors

    protected BasicListItem(string internalname, bool enabled) {
        Internal = string.IsNullOrEmpty(internalname) ? BasicPadItem.UniqueInternal() : internalname;
        if (string.IsNullOrEmpty(Internal)) { Develop.DebugPrint(FehlerArt.Fehler, "Interner Name nicht vergeben."); }
        _checked = false;
        _enabled = enabled;
        Pos = Rectangle.Empty;
        UserDefCompareKey = string.Empty;
    }

    #endregion

    #region Properties

    public bool Checked {
        get => _checked;
        set {
            if (Parent == null) {
                _checked = value;
            } else {
                Parent?.SetNewCheckState(this, value, ref _checked);
            }
        }
    }

    public bool Enabled {
        get => _enabled;
        set {
            if (_enabled == value) { return; }
            _enabled = value;
            Parent?.OnChanged();
        }
    }

    public string Internal { get; set; }

    public bool IsCaption { get; protected set; }

    public ItemCollectionList? Parent {
        get => _parent;
        set {
            if (_parent == null || _parent == value) {
                _parent = value;
                return;
            }

            Develop.DebugPrint(FehlerArt.Fehler, "Parent Fehler!");
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
            return string.Compare(CompareKey(), tobj.CompareKey(), StringComparison.OrdinalIgnoreCase);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public bool Contains(int x, int y) => Pos.Contains(x, y);

    public void Draw(Graphics gr, int xModifier, int yModifier, Design controldesign, Design itemdesign, States vState, bool drawBorderAndBack, string filterText, bool translate) {
        if (Parent == null) { Develop.DebugPrint(FehlerArt.Fehler, "Parent nicht definiert"); }
        if (itemdesign == Design.Undefiniert) { return; }
        Rectangle positionModified = Pos with { X = Pos.X - xModifier, Y = Pos.Y - yModifier };
        DrawExplicit(gr, positionModified, itemdesign, vState, drawBorderAndBack, translate);
        if (drawBorderAndBack) {
            if (!string.IsNullOrEmpty(filterText) && !FilterMatch(filterText)) {
                var c1 = Skin.Color_Back(controldesign, States.Standard); // Standard als Notlösung, um nicht doppelt checken zu müssen
                c1 = c1.SetAlpha(160);
                gr.FillRectangle(new SolidBrush(c1), positionModified);
            }
        }
    }

    public virtual bool FilterMatch(string filterText) => Internal.ToUpper().Contains(filterText.ToUpper());

    public abstract int HeightForListBox(BlueListBoxAppearance style, int columnWidth);

    public virtual bool IsClickable() => !IsCaption;

    public void SetCoordinates(Rectangle r) {
        Pos = r;
        Parent?.OnChanged();
    }

    public Size SizeUntouchedForListBox() {
        if (_sizeUntouchedForListBox.IsEmpty) {
            _sizeUntouchedForListBox = ComputeSizeUntouchedForListBox();
        }
        return _sizeUntouchedForListBox;
    }

    protected abstract Size ComputeSizeUntouchedForListBox();

    protected abstract void DrawExplicit(Graphics gr, Rectangle positionModified, Design itemdesign, States state, bool drawBorderAndBack, bool translate);

    protected abstract string GetCompareKey();

    #endregion

    //return null;
}