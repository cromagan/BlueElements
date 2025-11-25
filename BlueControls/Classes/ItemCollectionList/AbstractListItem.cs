// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.EventArgs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace BlueControls.ItemCollectionList;

public abstract class AbstractListItem : IComparable, IHasKeyName, INotifyPropertyChanged {

    #region Fields

    public Rectangle Position { get; set; }

    /// <summary>
    /// Falls eine spezielle Information gespeichert und zurückgegeben werden soll
    /// </summary>
    /// <remarks></remarks>
    public object? Tag { get; set; }

    private Size _sizeUntouchedForListBox = Size.Empty;

    #endregion

    #region Constructors

    protected AbstractListItem(string keyName, bool enabled) {
        KeyName = string.IsNullOrEmpty(keyName) ? Generic.GetUniqueKey() : keyName;
        if (string.IsNullOrEmpty(KeyName)) { Develop.DebugPrint(ErrorType.Error, "Interner Name nicht vergeben."); }
        Enabled = enabled;
        Position = Rectangle.Empty;
        UserDefCompareKey = string.Empty;
    }

    #endregion

    #region Events

    public event EventHandler? CompareKeyChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public bool Enabled {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public int Indent {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool KeyIsCaseSensitive => false;

    public string KeyName {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public event EventHandler<ObjectEventArgs>? LeftClickExecute;

    public abstract string QuickInfo { get; }

    public string UserDefCompareKey {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnCompareKeyChanged();
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public string CompareKey() {
        if (!string.IsNullOrEmpty(UserDefCompareKey)) {
            if (UserDefCompareKey.Length > 0 && UserDefCompareKey[0] < 32) { Develop.DebugPrint("Sortierung inkorrekt: " + UserDefCompareKey); }

            return UserDefCompareKey;// + Constants.FirstSortChar + Parent?.IndexOf(this).ToString(Constants.Format_Integer6);
        }
        return GetCompareKey();
    }

    public int CompareTo(object obj) {
        if (obj is AbstractListItem tobj) {
            return string.Compare(CompareKey(), tobj.CompareKey(), StringComparison.OrdinalIgnoreCase);
        }

        Develop.DebugPrint(ErrorType.Error, "Falscher Objecttyp!");
        return 0;
    }

    public bool Contains(int x, int y) => Position.Contains(x, y);

    public void Draw(Graphics gr, int xModifier, int yModifier, Design controldesign, Design itemdesign, States state, bool drawBorderAndBack, string filterText, bool translate, Design checkboxDesign) {
        if (itemdesign == Design.Undefiniert) { return; }
        var positionModified = Position with { X = Position.X - xModifier + (Indent * 20), Y = Position.Y - yModifier, Width = Position.Width - (Indent * 20) };

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

    public virtual bool FilterMatch(string filterText) => KeyName.ToUpperInvariant().Contains(filterText.ToUpperInvariant());

    public abstract int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign);

    public virtual bool IsClickable() => true;

    public void OnCompareKeyChanged() => CompareKeyChanged?.Invoke(this, System.EventArgs.Empty);

    public void SetCoordinates(Rectangle r) {
        Position = r;
        OnPropertyChanged("Position");
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

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    internal void OnLeftClickExecute() {
        LeftClickExecute?.Invoke(this, new ObjectEventArgs(Tag));
    }

    #endregion
}