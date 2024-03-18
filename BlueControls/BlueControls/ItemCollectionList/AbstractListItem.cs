﻿// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueBasics.Interfaces;
using BlueControls.Enums;
using System;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

public abstract class AbstractListItem : IComparable, IHasKeyName, IPropertyChangedFeedback {

    #region Fields

    public Rectangle Pos;

    /// <summary>
    /// Falls eine Spezielle Information gespeichert und zurückgegeben werden soll
    /// </summary>
    /// <remarks></remarks>
    public object? Tag;

    /// <summary>
    /// Ist das Item enabled?
    /// </summary>
    /// <remarks></remarks>
    private bool _enabled;

    private bool _isCaption;
    private string _keyName = string.Empty;
    private Size _sizeUntouchedForListBox = Size.Empty;

    private string _userDefCompareKey;

    #endregion

    #region Constructors

    protected AbstractListItem(string keyName, bool enabled) {
        KeyName = string.IsNullOrEmpty(keyName) ? Generic.UniqueInternal() : keyName;
        if (string.IsNullOrEmpty(KeyName)) { Develop.DebugPrint(FehlerArt.Fehler, "Interner Name nicht vergeben."); }
        _enabled = enabled;
        Pos = Rectangle.Empty;
        _userDefCompareKey = string.Empty;
    }

    #endregion

    #region Events

    public event EventHandler? CompareKeyChanged;

    public event EventHandler? PropertyChanged;

    #endregion

    #region Properties

    public bool Enabled {
        get => _enabled;
        set {
            if (_enabled == value) { return; }
            _enabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsCaption {
        get => _isCaption;
        protected set {
            if (_isCaption == value) { return; }
            _isCaption = value;
            OnPropertyChanged();
        }
    }

    public string KeyName {
        get => _keyName;
        set {
            if (_keyName == value) { return; }
            _keyName = value;
            OnPropertyChanged();
        }
    }

    public abstract string QuickInfo { get; }

    //        Develop.DebugPrint(FehlerArt.Fehler, "Parent Fehler!");
    //    }
    //}
    public string UserDefCompareKey {
        get => _userDefCompareKey;
        set {
            if (_userDefCompareKey == value) { return; }
            _userDefCompareKey = value;
            OnCompareKeyChanged();
            OnPropertyChanged();
        }
    }

    #endregion

    //public void CloneBasicStatesFrom(AbstractListItem sourceItem) {
    //    Enabled = sourceItem.Enabled;
    //    Tag = sourceItem.Tag;
    //    UserDefCompareKey = sourceItem.UserDefCompareKey;
    //    KeyName = sourceItem.KeyName;
    //    IsCaption = sourceItem.IsCaption;
    //}

    #region Methods

    ///// <summary>
    ///// Klont das aktuelle Objekt (es wird ein neues Objekt des gleichen Typs erstellt) und fügt es in die angegebene ItemCollection hinzu
    ///// </summary>
    ///// <param name="newParent"></param>
    //public virtual void CloneToNewCollection(ItemCollectionList newParent) => Develop.DebugPrint_RoutineMussUeberschriebenWerden();
    public string CompareKey() {
        if (!string.IsNullOrEmpty(UserDefCompareKey)) {
            if (Convert.ToChar(UserDefCompareKey.Substring(0, 1)) < 32) { Develop.DebugPrint("Sortierung inkorrekt: " + UserDefCompareKey); }

            return UserDefCompareKey;// + Constants.FirstSortChar + Parent?.IndexOf(this).ToString(Constants.Format_Integer6);
        }
        return GetCompareKey();
    }

    public int CompareTo(object obj) {
        if (obj is AbstractListItem tobj) {
            return string.Compare(CompareKey(), tobj.CompareKey(), StringComparison.OrdinalIgnoreCase);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public bool Contains(int x, int y) => Pos.Contains(x, y);

    /// <summary>
    /// Vereinfacung für Null Conditional Operator.
    /// </summary>
    public void Disable() => Enabled = false;

    public void Draw(Graphics gr, int xModifier, int yModifier, Design controldesign, Design itemdesign, States state, bool drawBorderAndBack, string filterText, bool translate, Design checkboxDesign) {
        if (itemdesign == Design.Undefiniert) { return; }
        var positionModified = Pos with { X = Pos.X - xModifier, Y = Pos.Y - yModifier };

        if (checkboxDesign != Design.Undefiniert) {
            var design = Skin.DesignOf(checkboxDesign, state);
            gr.DrawImage(QuickImage.Get(design.Image, 12), positionModified.X + 4, positionModified.Y + 3);
            positionModified.X += 20;
            positionModified.Width -= 20;
            if (state.HasFlag(States.Checked)) { state ^= States.Checked; }
        }

        DrawExplicit(gr, positionModified, itemdesign, state, drawBorderAndBack, translate);
        if (drawBorderAndBack) {
            if (!string.IsNullOrEmpty(filterText) && !FilterMatch(filterText)) {
                var c1 = Skin.Color_Back(controldesign, States.Standard); // Standard als Notlösung, um nicht doppelt checken zu müssen
                c1 = c1.SetAlpha(160);
                gr.FillRectangle(new SolidBrush(c1), positionModified);
            }
        }
    }

    public virtual bool FilterMatch(string filterText) => KeyName.ToUpper().Contains(filterText.ToUpper());

    public abstract int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign);

    public virtual bool IsClickable() => !IsCaption;

    public void OnCompareKeyChanged() => CompareKeyChanged?.Invoke(this, System.EventArgs.Empty);

    public void OnPropertyChanged() => PropertyChanged?.Invoke(this, System.EventArgs.Empty);

    public void SetCoordinates(Rectangle r) {
        Pos = r;
        OnPropertyChanged();
    }

    public Size SizeUntouchedForListBox(Design itemdesign) {
        if (_sizeUntouchedForListBox.IsEmpty) {
            _sizeUntouchedForListBox = ComputeSizeUntouchedForListBox(itemdesign);
        }
        return _sizeUntouchedForListBox;
    }

    protected abstract Size ComputeSizeUntouchedForListBox(Design itemdesign);

    protected abstract void DrawExplicit(Graphics gr, Rectangle positionModified, Design itemdesign, States state, bool drawBorderAndBack, bool translate);

    protected abstract string GetCompareKey();

    #endregion
}