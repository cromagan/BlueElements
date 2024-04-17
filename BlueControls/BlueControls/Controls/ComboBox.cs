// Authors:
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
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueControls.ItemCollectionList;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;

namespace BlueControls.Controls;

#nullable enable

[Designer(typeof(BasicDesigner))]
[DefaultEvent("TextChanged")]
public partial class ComboBox : TextBox, ITranslateable {

    #region Fields

    private bool _autoSort = true;
    private bool _btnDropDownIsIn;
    private ComboboxStyle _drawStyle = ComboboxStyle.TextBox;
    private ComboBoxStyle _dropDownStyle = ComboBoxStyle.DropDown;
    private ExtText? _eTxt;
    private string _imageCode = string.Empty;

    /// <summary>
    /// Kümmert sich darum, wenn die ComboBox wie ein Button aussieht, dass immer
    /// der Button-Text stehen bleibt und nicht der Ausgewählte
    /// </summary>
    private string _initialtext = string.Empty;

    private List<AbstractListItem> _item = new();
    private Design _itemDesign;
    private string? _lastClickedText;

    #endregion

    #region Constructors

    public ComboBox() : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        MouseHighlight = true;
        SetStyle(ControlStyles.ContainerControl, true);
        btnDropDown.Left = Width - btnDropDown.Width;
        btnDropDown.Top = 0;
        btnDropDown.Height = Height;
    }

    #endregion

    #region Events

    public event EventHandler? DropDownShowing;

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    #endregion

    #region Properties

    [DefaultValue(true)]
    public bool AutoSort {
        get => _autoSort;
        set {
            if (value == _autoSort) { return; }
            _autoSort = value;
            Invalidate();
        }
    }

    [DefaultValue(ComboboxStyle.TextBox)]
    public ComboboxStyle DrawStyle {
        get => _drawStyle;
        set {
            if (_drawStyle != value) {
                _drawStyle = value;
                Invalidate();
            }
            SetStyle();
        }
    }

    [DefaultValue(ComboBoxStyle.DropDown)]
    public ComboBoxStyle DropDownStyle {
        get => _dropDownStyle;
        set {
            if (value != _dropDownStyle) {
                _dropDownStyle = value;
                Invalidate();
            }
            SetStyle();
        }
    }

    [DefaultValue("")]
    [Category("Darstellung")]
    [Editor(typeof(QuickPicSelector), typeof(UITypeEditor))]
    public string ImageCode {
        get => _imageCode;
        set {
            if (_imageCode != value) {
                _imageCode = value;
                Invalidate();
            }
            SetStyle();
        }
    }

    public int ItemCount => _item.Count;

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    #endregion

    #region Indexers

    public AbstractListItem? this[string @internal] {
        get {
            try {
                return _item.Get(@internal);
            } catch {
                Develop.CheckStackForOverflow();
                return this[@internal];
            }
        }
    }

    public AbstractListItem? this[int no] {
        get {
            try {
                if (no < 0 || no > _item.Count) { return null; }

                return _item[no];
            } catch {
                Develop.CheckStackForOverflow();
                return this[no];
            }
        }
    }

    #endregion

    #region Methods

    public void ItemAdd(AbstractListItem item) {
        _item.Add(item);
        Invalidate();
    }

    public void ItemClear() {
        if (_item.Count == 0) { return; }
        _item.Clear();
        Invalidate();
    }

    public void ShowMenu(object? sender, MouseEventArgs? e) {
        if (_btnDropDownIsIn || IsDisposed || !Enabled) { return; }
        _btnDropDownIsIn = true;
        OnDropDownShowing();
        if (_item.Count == 0) { _btnDropDownIsIn = false; return; }
        int x, y;
        if (sender is Button but) {
            x = Cursor.Position.X - but.MousePos().X - but.Location.X;
            y = Cursor.Position.Y - but.MousePos().Y + Height; //Identisch
        } else {
            x = Cursor.Position.X - MousePos().X;
            y = Cursor.Position.Y - MousePos().Y + Height; //Identisch
        }

        List<string> itc = [];
        if (_drawStyle != ComboboxStyle.RibbonBar) { itc.Add(Text); }
        var dropDownMenu = FloatingInputBoxListBoxStyle.Show(_item, CheckBehavior.SingleSelection, itc, x, y, Width, null, this, Translate, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, _autoSort);
        dropDownMenu.Cancel += DropDownMenu_Cancel;
        dropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
        _btnDropDownIsIn = false;
    }

    internal void ItemAddRange(IEnumerable<AbstractListItem>? items) {
        if (items == null || !items.Any()) { return; }

        foreach (var thisIt in items) {
            _item.Remove(thisIt.KeyName);
            ItemAdd(thisIt);
        }
    }

    internal ReadOnlyCollection<AbstractListItem> Items() => _item.AsReadOnly();

    internal void Remove(AbstractListItem thisit) {
        if (!_item.Contains(thisit)) { return; }
        _item.Remove(thisit);
        Invalidate();
    }

    internal bool WasThisValueClicked() => _lastClickedText != null && Text == _lastClickedText;

    protected override void DrawControl(Graphics gr, States state) {
        if (_dropDownStyle == ComboBoxStyle.DropDownList) {
            if (_item.Count == 0) {
                state = States.Standard_Disabled;
            }
        }

        if (_drawStyle != ComboboxStyle.TextBox) {
            if (string.IsNullOrEmpty(_initialtext) && !string.IsNullOrEmpty(Text)) { _initialtext = Text; }

            _eTxt ??= new ExtText((Design)_drawStyle, state);

            Button.DrawButton(this, gr, (Design)_drawStyle, state, QuickImage.Get(_imageCode), Alignment.Horizontal_Vertical_Center, true, _eTxt, _initialtext, base.DisplayRectangle, Translate);
            btnDropDown.Invalidate();
            return;
        }

        btnDropDown.Enabled = _item.Count > 0;
        var vType = Design.ComboBox_Textbox;
        if (GetParentType() is ParentType.RibbonGroupBox or ParentType.RibbonPage) {
            vType = Design.Ribbon_ComboBox_Textbox;
        }

        var i = _item.Get(Text);
        if (i == null) {
            base.DrawControl(gr, state);
            btnDropDown.Invalidate();
            return;
        }

        //i.Parent = Item; // Um den Stil zu wissen
        if (Focused && _dropDownStyle == ComboBoxStyle.DropDown) {
            // Focused = Bearbeitung erwünscht, Cursor anzeigen und KEINE Items zeichnen
            base.DrawControl(gr, state);
            btnDropDown.Invalidate();
            return;
        }

        if (_dropDownStyle == ComboBoxStyle.DropDown) {
            if (i is TextListItem tempVar2) {
                if (tempVar2.Symbol == null && tempVar2.IsClickable()) {
                    base.DrawControl(gr, state);
                    btnDropDown.Invalidate();
                    return;
                }
            }
        }

        Skin.Draw_Back(gr, vType, state, DisplayRectangle, this, true);

        if (!FloatingForm.IsShowing(this)) {
            // Nur wenn die Selectbox gerade Nicht angezeigt wird, um hin und her Konvertierungen zu vermeiden
            var r = i.Pos;
            var ymod = -(int)((DisplayRectangle.Height - i.SizeUntouchedForListBox(Design.Item_DropdownMenu).Height) / 2.0);
            i.SetCoordinates(new Rectangle(Skin.PaddingSmal, -ymod, Width - 30, i.SizeUntouchedForListBox(_itemDesign).Height));
            i.Draw(gr, 0, 0, Design.ComboBox_Textbox, Design.ComboBox_Textbox, state, false, string.Empty, Translate, Design.Undefiniert);
            i.SetCoordinates(r);
        }
        Skin.Draw_Border(gr, vType, state, DisplayRectangle);
        btnDropDown.Invalidate();
    }

    protected override Design GetDesign() => GetParentType() is ParentType.RibbonGroupBox or ParentType.RibbonPage
        ? Design.Ribbon_ComboBox_Textbox
        : Design.ComboBox_Textbox;

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        FloatingForm.Close(this);
        btnDropDown.Enabled = Enabled;
        btnDropDown.Invalidate();
    }

    protected override void OnGotFocus(System.EventArgs e) {
        if (_dropDownStyle == ComboBoxStyle.DropDownList) {
            _ = btnDropDown.Focus();
        } else {
            base.OnGotFocus(e);
        }
        FloatingForm.Close(this);
    }

    protected virtual void OnItemClicked(AbstractListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    protected override void OnLostFocus(System.EventArgs e) {
        Invalidate();
        CheckLostFocus(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        if (e.Button != MouseButtons.Right) {
            // nicht bei rechts, ansonsten gibt's evtl. Kontextmenü (von der Textbox aus gesteuert) UND den Auswahldialog
            if (_dropDownStyle == ComboBoxStyle.DropDownList) {
                ShowMenu(this, e);
            } else {
                base.OnMouseUp(e);
            }
        } else {
            base.OnMouseUp(e);
        }
    }

    //protected override void OnMouseEnter(System.EventArgs e)
    //{
    //    base.OnMouseEnter(e);
    //    if (_DrawStyle == enComboboxStyle.TextBox || !Enabled) { return; }
    //    Invalidate();
    //}
    //protected override void OnMouseLeave(System.EventArgs e)
    //{
    //    base.OnMouseLeave(e);
    //    if (_DrawStyle == enComboboxStyle.TextBox || !Enabled) { return; }
    //    Invalidate();
    //}
    protected override void OnTextChanged(System.EventArgs e) {
        base.OnTextChanged(e);
        FloatingForm.Close(this);
    }

    private void btnDropDown_LostFocus(object sender, System.EventArgs e) => CheckLostFocus(e);

    //}
    private void CheckLostFocus(System.EventArgs e) {
        try {
            if (btnDropDown == null) { return; }
            if (!btnDropDown.Focused && !Focused && !FloatingForm.IsShowing(this)) { base.OnLostFocus(e); }
        } catch { }
    }

    //private void _Item_ItemRemoved(object sender, System.EventArgs e) {
    private void DropDownMenu_Cancel(object sender, object mouseOver) {
        FloatingForm.Close(this);
        _ = Focus();
    }

    private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        FloatingForm.Close(this);
        if (!string.IsNullOrEmpty(e.ClickedCommand) && _item.Get(e.ClickedCommand) is AbstractListItem bli) {
            _lastClickedText = e.ClickedCommand;
            Text = e.ClickedCommand;
            OnItemClicked(new AbstractListItemEventArgs(bli));
        }
        _ = Focus();
    }

    private void Item_PropertyChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        if (_btnDropDownIsIn) { return; }
        FloatingForm.Close(this);
        //Invalidate();
    }

    private void OnDropDownShowing() => DropDownShowing?.Invoke(this, System.EventArgs.Empty);

    private void SetStyle() {
        if (DrawStyle != ComboboxStyle.TextBox) {
            Cursor = Cursors.Arrow;
            _dropDownStyle = ComboBoxStyle.DropDownList;
            btnDropDown.Visible = false;
            // ImageCode = string.Empty; - Egal, wird eh ignoriert wenn es nicht gebraucht wird
        }
    }

    #endregion
}