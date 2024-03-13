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
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("ItemClicked")]
public partial class ListBox : GenericControl, IContextMenu, IBackgroundNone, ITranslateable {

    #region Fields

    private AddType _addAlloweds = AddType.Text;
    private ListBoxAppearance _appearance;
    private bool _autosort = true;
    private CheckBehavior _checkBehavior = CheckBehavior.SingleSelection;
    private List<string> _checked = [];
    private string _filterText = string.Empty;

    //Muss was gesetzt werden, sonst hat der Designer nachher einen Fehler
    private AbstractListItem? _mouseOverItem;

    private bool _moveAllowed;

    private bool _removeAllowed;

    #endregion

    #region Constructors

    public ListBox() : base(true, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Item = new ItemCollectionList.ItemCollectionList(true);
        RegisterEvents();
        _appearance = ListBoxAppearance.Listbox;
    }

    #endregion

    #region Events

    public event EventHandler? AddClicked;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public event EventHandler? ItemCheckedChanged;

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    public event EventHandler<AbstractListItemEventArgs>? ItemDoubleClick;

    #endregion

    #region Properties

    [DefaultValue(true)]
    public AddType AddAllowed {
        get => _addAlloweds;
        set {
            if (_addAlloweds == value) { return; }
            _addAlloweds = value;
            DoMouseMovement();
        }
    }

    [DefaultValue(ListBoxAppearance.Listbox)]
    public ListBoxAppearance Appearance {
        get => _appearance;
        set {
            if (_appearance == value) { return; }
            _appearance = value;
            Item.Appearance = value;
        }
    }

    [DefaultValue(true)]
    public bool AutoSort {
        get => _autosort;
        set {
            if (_autosort == value) { return; }
            _autosort = value;
            Item.AutoSort = _autosort;
            Invalidate();
        }
    }

    [DefaultValue(CheckBehavior.SingleSelection)]
    public CheckBehavior CheckBehavior {
        get => _checkBehavior;
        set {
            if (value == _checkBehavior) { return; }
            _checkBehavior = value;
            ValidateCheckStates(_checked, string.Empty);
        }
    }

    public ReadOnlyCollection<string> Checked => _checked.AsReadOnly();

    [DefaultValue("")]
    public string FilterText {
        get => _filterText;
        set {
            if (_filterText == value) { return; }
            _filterText = value;
            Invalidate();
        }
    }

    //public string LastFilePath { get; set; }
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ItemCollectionList.ItemCollectionList Item { get; }

    [DefaultValue(false)]
    public bool MoveAllowed {
        get => _moveAllowed;
        set {
            if (_moveAllowed == value) { return; }
            _moveAllowed = value;
            DoMouseMovement();
        }
    }

    [DefaultValue(false)]
    public bool RemoveAllowed {
        get => _removeAllowed;
        set {
            if (_removeAllowed == value) { return; }
            _removeAllowed = value;
            DoMouseMovement();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ItemCollectionList.ItemCollectionList Suggestions { get; } = new(true);

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    protected override string QuickInfoText {
        get {
            var t1 = base.QuickInfoText;
            var t2 = string.Empty;
            if (_mouseOverItem != null) { t2 = _mouseOverItem.QuickInfo; }
            if (string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2)) {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2)) {
                return t1 + "<br><hr><br>" + t2;
            }

            return t1 + t2; // Eins davon ist leer
        }
    }

    #endregion

    #region Methods

    public TextListItem? Add_Text(string? val) {
        if (val == null || string.IsNullOrEmpty(val)) { return null; }
        if (Item.Any(thisItem => thisItem != null && string.Equals(thisItem.KeyName, val, StringComparison.OrdinalIgnoreCase))) {
            return null;
        }
        var i = Item.Add(val, val);
        Check(i);
        return i;
    }

    public TextListItem? Add_Text() {
        var val = InputBoxComboStyle.Show("Bitte geben sie einen Wert ein:", Suggestions, true);
        return Add_Text(val);
    }

    public void Add_TextBySuggestion() {
        if (Suggestions.Count == 0) {
            MessageBox.Show("Keine (weiteren) Werte vorhanden.", ImageCode.Information, "OK");
            return;
        }

        var rück = InputBoxListBoxStyle.Show("Bitte wählen sie einen Wert:", Suggestions, CheckBehavior.SingleSelection, null, AddType.None);

        if (rück == null || rück.Count == 0) { return; }

        var sg = Suggestions[rück[0]];
        if (sg == null) { return; }

        AddAndCheck(sg.Clone() as AbstractListItem);
    }

    public void Check(IEnumerable<string> ali) {
        foreach (var thiss in ali) {
            Check(thiss);
        }
    }

    public void Check(AbstractListItem ali) => Check(ali.KeyName);

    public void Check(string name) {
        if (IsChecked(name)) { return; }

        List<string> l = [.. _checked, name];

        ValidateCheckStates(l, name);
        Invalidate();
    }

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

    public new void Focus() {
        if (Focused()) { return; }
        _ = base.Focus();
    }

    public new bool Focused() => base.Focused || btnPlus.Focused || btnMinus.Focused || btnUp.Focused || btnDown.Focused || SliderY.Focused();

    public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList.ItemCollectionList items, out object? hotItem) => hotItem = e == null ? null : MouseOverNode(e.X, e.Y);

    public virtual void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public virtual void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public void Swap(int index1, int index2) {
        if (index1 == index2) { return; }
        if (Item.AutoSort) { return; }

        UnRegisterEvents();
        (Item[index1], Item[index2]) = (Item[index2], Item[index1]);
        RegisterEvents();

        Invalidate();
        DoMouseMovement();
        ValidateCheckStates(Item.ItemOrder.ToListOfString(), string.Empty);
    }

    public void UnCheck(IEnumerable<string> ali) {
        foreach (var thiss in ali) {
            UnCheck(thiss);
        }
    }

    public void UnCheck(AbstractListItem ali) => UnCheck(ali.KeyName);

    public void UnCheck(string name) {
        if (!IsChecked(name)) { return; }

        List<string> l = [.. _checked];
        l.Remove(name);

        ValidateCheckStates(l, string.Empty);
        Invalidate();
    }

    public void UncheckAll() => ValidateCheckStates([], string.Empty);

    internal void AddAndCheck(AbstractListItem? ali) {
        if (ali == null) { return; }

        if (Item[ali.KeyName] != null) { return; }

        var tmp = _checkBehavior;
        _checkBehavior = CheckBehavior.MultiSelection;

        //Item.Remove(ali.KeyName);

        Item.Add(ali);
        _checkBehavior = tmp;
        Check(ali);
    }

    internal void SetValuesTo(List<string> values) {
        var ist = Item.ToListOfString();
        var zuviel = ist.Except(values).ToList();
        var zuwenig = values.Except(ist).ToList();
        // Zu viele im Mains aus der Liste löschen
        foreach (var thisString in zuviel) {
            if (!values.Contains(thisString)) {
                Item.Remove(thisString);
            }
        }

        // und die Mains auffüllen
        foreach (var thisString in zuwenig) {
            if (IO.FileExists(thisString)) {
                if (thisString.FileType() == FileFormat.Image) {
                    _ = Item.Add(thisString, thisString, thisString.FileNameWithoutSuffix());
                } else {
                    _ = Item.Add(thisString.FileNameWithSuffix(), thisString, QuickImage.Get(thisString.FileType(), 48));
                }
            } else {
                _ = Item.Add(thisString);
            }
        }
    }

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
            }
            UnRegisterEvents();
        } finally {
            base.Dispose(disposing);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        var tmpDesign = _appearance is ListBoxAppearance.Gallery or ListBoxAppearance.FileSystem or ListBoxAppearance.Listbox_Boxes
            ? Design.ListBox
            : (Design)_appearance;

        var checkboxDesign = CheckboxDesign();

        var tmpState = state;
        tmpState &= ~(States.Standard_MouseOver | States.Standard_MousePressed | States.Standard_HasFocus);

        var addy = 0;

        if (_addAlloweds != AddType.None) { addy = 32; }

        if (Item.Count == 0) {
            SliderY.Visible = false;
            SliderY.Value = 0;
            addy = 0;
        }

        var (biggestItemX, _, heightAdded, senkrechtAllowed) = Item.ItemData();
        _ = Item.ComputeAllItemPositions(new Size(DisplayRectangle.Width, DisplayRectangle.Height), SliderY, biggestItemX, heightAdded, senkrechtAllowed, addy);

        var tmpSliderWidth = SliderY.Visible ? SliderY.Width : 0;

        var borderCoords = new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width - tmpSliderWidth, DisplayRectangle.Height);
        var visArea = borderCoords with { Y = borderCoords.Y + (int)SliderY.Value };

        if (borderCoords.Height > 0) {
            Skin.Draw_Back(gr, tmpDesign, tmpState, borderCoords, this, true);
        }

        //_mouseOverItem = MouseOverNode(MousePos().X, MousePos().Y);
        object locker = new();

        Parallel.ForEach(Item.ItemOrder, thisItem => {
            var currentItem = thisItem;
            if (currentItem.Pos.IntersectsWith(visArea)) {
                var itemState = tmpState;
                if (_mouseOverItem == currentItem && Enabled) { itemState |= States.Standard_MouseOver; }

                if (!currentItem.Enabled) { itemState = States.Standard_Disabled; }

                if (CheckBehavior != CheckBehavior.AllSelected && IsChecked(currentItem)) { itemState |= States.Checked; }

                lock (locker) {
                    currentItem.Draw(gr, 0, (int)SliderY.Value, Item.ControlDesign, Item.ItemDesign, itemState, true, _filterText, false, checkboxDesign);
                }
            }
        });

        if (borderCoords.Height > 0 && tmpDesign == Design.ListBox) {
            Skin.Draw_Border(gr, tmpDesign, tmpState, borderCoords);
        }

        //if (tmpButtonLeisteHeight > 0) {
        //    Skin.Draw_Back_Transparent(gr, new Rectangle(0, borderCoords.Bottom, Width, tmpButtonLeisteHeight), this);
        //}
    }

    protected override void OnDoubleClick(System.EventArgs e) {
        if (!Enabled) { return; }
        var nd = MouseOverNode(MousePos().X, MousePos().Y);
        if (nd == null) { return; }
        OnItemDoubleClick(new AbstractListItemEventArgs(nd));
    }

    protected virtual void OnItemClicked(AbstractListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);

        DoMouseMovement();
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        DoMouseMovement();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (!Enabled) { return; }
        var nd = MouseOverNode(e.X, e.Y);
        if (nd != null && !nd.Enabled) { return; }
        switch (e.Button) {
            case MouseButtons.Left:
                if (nd != null) {
                    if (Appearance is ListBoxAppearance.Listbox or
                                      ListBoxAppearance.Listbox_Boxes or
                                      ListBoxAppearance.Autofilter or
                                      ListBoxAppearance.Gallery or
                                      ListBoxAppearance.FileSystem) {
                        if (nd.IsClickable()) {
                            if (CheckBehavior != CheckBehavior.AllSelected) {
                                ChangeCheck(nd);
                            }
                        }
                    }
                    OnItemClicked(new AbstractListItemEventArgs(nd));
                }
                break;

            case MouseButtons.Right:
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                break;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);
        if (!SliderY.Visible) { return; }
        SliderY.DoMouseWheel(e);
    }

    protected override void OnParentEnabledChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        DoMouseMovement();
        base.OnEnabledChanged(e);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        DoMouseMovement();
        base.OnVisibleChanged(e);
    }

    private void btnDown_Click(object sender, System.EventArgs e) {
        var ln = -1;
        for (var z = Item.ItemOrder.Count - 1; z >= 0; z--) {
            if (Item[z] == _mouseOverItem) {
                if (ln < 0) { return; }// Befehl verwerfen...
                Swap(ln, z);
                return;
            }
            ln = z;
        }
    }

    private void btnMinus_Click(object sender, System.EventArgs e) {
        if (_mouseOverItem == null) { return; }

        //if (_checkBehaviorx == CheckBehavior.AlwaysSingleSelection && Item.Count < 2) { return; }
        if (CheckboxDesign() != Design.Undefiniert) { return; }

        UnCheck(_mouseOverItem);

        if (_checkBehavior != CheckBehavior.AllSelected) {
            Item.Remove(_mouseOverItem);
            //Check(string.Empty);
        }
    }

    private void btnPlus_Click(object sender, System.EventArgs e) {
        OnAddClicked();
        switch (_addAlloweds) {
            case AddType.UserDef:
                break;

            case AddType.Text:
                _ = Add_Text();
                break;

            case AddType.OnlySuggests:
                Add_TextBySuggestion();
                break;

            case AddType.None:
                break;

            default:
                Develop.DebugPrint(_addAlloweds);
                break;
        }
        DoMouseMovement();
    }

    private void btnUp_Click(object sender, System.EventArgs e) {
        AbstractListItem? ln = null;
        foreach (var thisItem in Item.ItemOrder) {
            if (thisItem == _mouseOverItem) {
                if (ln == null) { return; }// Befehl verwerfen...
                Swap(Item.IndexOf(ln), Item.IndexOf(thisItem));
                return;
            }
            ln = thisItem;
        }
    }

    private void ChangeCheck(AbstractListItem ne) {
        if (IsChecked(ne)) {
            UnCheck(ne);
        } else {
            Check(ne);
        }
    }

    private Design CheckboxDesign() {
        var checkboxDesign = Design.Undefiniert;
        if (_appearance == ListBoxAppearance.Listbox_Boxes && _checkBehavior != CheckBehavior.AllSelected) {
            checkboxDesign = _checkBehavior == CheckBehavior.SingleSelection
                ? Design.OptionButton_TextStyle
                : Design.CheckBox_TextStyle;
        }
        return checkboxDesign;
    }

    private void DoMouseMovement() {
        if (IsDisposed) { return; }

        var isInForm = true;

        if (MousePos().X < 0 || MousePos().Y < 0 || MousePos().X > Width || MousePos().Y > Height) { isInForm = false; }

        var nd = MouseOverNode(MousePos().X, MousePos().Y);
        if (!Enabled || Parent == null || !Parent.Enabled || !Visible) {
            nd = null;
            isInForm = false;
        }

        if (AddAllowed != AddType.None && isInForm) {
            btnPlus.Left = 2;
            btnPlus.Top = Height - 2 - btnPlus.Height;
            btnPlus.Visible = true;
            btnPlus.Enabled = true;
            btnPlus.BringToFront();
        } else {
            btnPlus.Visible = false;
        }

        if (nd == _mouseOverItem) { return; }
        _mouseOverItem = nd;

        if (_mouseOverItem != null) {
            var pos = _mouseOverItem.Pos.Right;

            #region down-Button

            //btnUp.Enabled = Item[0].KeyName != nr[0];
            //btnDown.Enabled = Item[Item.Count - 1].KeyName != nr[0];

            if (_moveAllowed && !Item.AutoSort && Item.Count > 1) {
                btnDown.Width = 16;
                btnDown.Height = 16;
                pos -= btnDown.Width;
                btnDown.Top = _mouseOverItem.Pos.Y - (int)SliderY.Value;
                btnDown.Left = pos;
                btnDown.Visible = true;
                btnDown.Enabled = Item[Item.Count - 1] != _mouseOverItem;
            } else {
                btnDown.Visible = false;
            }

            #endregion

            #region Up-Button

            if (_moveAllowed && !Item.AutoSort && Item.Count > 1) {
                btnUp.Width = 16;
                btnUp.Height = 16;
                pos -= btnUp.Width;
                btnUp.Top = _mouseOverItem.Pos.Y - (int)SliderY.Value;
                btnUp.Left = pos;
                btnUp.Visible = true;
                btnUp.Enabled = Item[0] != _mouseOverItem;
            } else {
                btnUp.Visible = false;
            }

            #endregion

            #region Löschen-Button

            var removeok = _removeAllowed;

            if (CheckboxDesign() != Design.Undefiniert) { removeok = false; }

            if (removeok) {
                btnMinus.Width = 16;
                btnMinus.Height = 16;
                pos -= btnMinus.Width;
                btnMinus.Top = _mouseOverItem.Pos.Y - (int)SliderY.Value;
                btnMinus.Left = pos;
                btnMinus.Visible = true;
                btnMinus.Enabled = true;
            } else {
                btnMinus.Visible = false;
            }

            #endregion
        } else {
            btnMinus.Visible = false;
            btnUp.Visible = false;
            btnDown.Visible = false;
        }

        Invalidate();
        DoQuickInfo();
        Invalidate();
    }

    private bool IsChecked(AbstractListItem thisItem) => IsChecked(thisItem.KeyName);

    private bool IsChecked(string name) => _checked.Contains(name);

    private void Item_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        var last = string.Empty;
        if (e.NewItems != null && e.NewItems.Count > 0) {
            if (e.NewItems[0] is AbstractListItem ali) {
                last = ali.KeyName;
            }
        }

        var l = new List<string>();
        foreach (var thisc in _checked) {
            if (Item[thisc] != null) { l.Add(thisc); }
        }

        ValidateCheckStates(l, last);

        Invalidate();
    }

    private void Item_PropertyChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
    }

    private AbstractListItem? MouseOverNode(int x, int y) => Item[x, (int)(y + SliderY.Value)];

    private void OnAddClicked() => AddClicked?.Invoke(this, System.EventArgs.Empty);

    private void OnItemCheckedChanged() => ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnItemDoubleClick(AbstractListItemEventArgs e) => ItemDoubleClick?.Invoke(this, e);

    private void RegisterEvents() {
        Item.PropertyChanged += Item_PropertyChanged;
        Item.CollectionChanged += Item_CollectionChanged;
    }

    private void SliderY_ValueChange(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        _mouseOverItem = null; // Damit die Buttons neu berechnet werden.
        DoMouseMovement();
        Invalidate();
    }

    private void UnRegisterEvents() {
        Item.PropertyChanged -= Item_PropertyChanged;
        Item.CollectionChanged -= Item_CollectionChanged;
    }

    private void ValidateCheckStates(List<string> newCheckedItems, string lastaddeditem) {
        newCheckedItems = newCheckedItems.SortedDistinctList();

        switch (_checkBehavior) {
            case CheckBehavior.AllSelected:
                SetValuesTo(newCheckedItems);
                newCheckedItems = Item.ToListOfString();
                break;

            case CheckBehavior.NoSelection:
                newCheckedItems.Clear();
                break;

            case CheckBehavior.MultiSelection:
                break;

            case CheckBehavior.SingleSelection:
                if (newCheckedItems.Count > 1) {
                    if (string.IsNullOrEmpty(lastaddeditem)) { lastaddeditem = newCheckedItems[0]; }
                    newCheckedItems.Clear();
                    newCheckedItems.Add(lastaddeditem);
                }
                break;

            //case CheckBehavior.AlwaysSingleSelection:
            //    if (newCheckedItems.Count > 1) {
            //        if (string.IsNullOrEmpty(lastaddeditem)) { lastaddeditem = newCheckedItems[0]; }
            //        newCheckedItems.Clear();
            //        newCheckedItems.Add(lastaddeditem);
            //    }

            //if (newCheckedItems.Count == 0) {
            //    var it = Item.FirstOrDefault(thisp => thisp != null && thisp.IsClickable());
            //    if (it != null) { newCheckedItems.Add(it.KeyName); }
            //}
            //break;

            default:
                Develop.DebugPrint(_checkBehavior);
                break;
        }

        List<string> newList = [];

        foreach (var thisit in newCheckedItems) {
            var it = Item[thisit] ?? Item.Add(thisit);

            if (it.IsClickable()) {
                newList.Add(thisit);
            }
        }

        if (newList.IsDifferentTo(_checked)) {
            _checked = newList;
            OnItemCheckedChanged();
        }

        Invalidate();
    }

    #endregion
}