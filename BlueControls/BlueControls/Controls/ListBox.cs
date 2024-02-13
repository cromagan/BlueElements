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

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase.Interfaces;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("ItemClicked")]
public partial class ListBox : GenericControl, IContextMenu, IBackgroundNone, ITranslateable {

    #region Fields

    private AddType _addAlloweds = AddType.Text;

    private ListBoxAppearance _appearance;

    private bool _autosort = true;
    private bool _filterAllowed;

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
        Item.ItemCheckedChanged += _Item_ItemCheckedChanged;
        Item.CollectionChanged += Item_CollectionChanged;
        Item.Changed += Item_Changed;
        _appearance = ListBoxAppearance.Listbox;
    }

    #endregion

    #region Events

    public event EventHandler? AddClicked;

    public event EventHandler<NotifyCollectionChangedEventArgs>? CollectionChanged;

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
            CheckButtons();
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
            CheckButtons();
        }
    }

    [DefaultValue(CheckBehavior.SingleSelection)]
    public CheckBehavior CheckBehavior {
        get => Item.CheckBehavior;
        set => Item.CheckBehavior = value;
    }

    [DefaultValue(false)]
    public bool FilterAllowed {
        get => _filterAllowed;
        set {
            //  if (_MoveAllowed) { value = false; }
            if (_filterAllowed == value) { return; }
            _filterAllowed = value;
            CheckButtons();
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
            CheckButtons();
        }
    }

    [DefaultValue(false)]
    public bool RemoveAllowed {
        get => _removeAllowed;
        set {
            if (_removeAllowed == value) { return; }
            _removeAllowed = value;
            CheckButtons();
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
        i.Checked = true;
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
        Suggestions.CheckBehavior = CheckBehavior.SingleSelection;
        var rück = InputBoxListBoxStyle.Show("Bitte wählen sie einen Wert:", Suggestions, AddType.None, true);

        if (rück == null || rück.Count == 0) { return; }

        var sg = Suggestions[rück[0]];
        if (sg == null) { return; }

        Item.Add(sg.Clone() as AbstractListItem);
    }

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

    public new void Focus() {
        if (Focused()) { return; }
        _ = base.Focus();
    }

    public new bool Focused() => base.Focused || Plus.Focused || Minus.Focused || Up.Focused || Down.Focused || SliderY.Focused() || FilterCap.Focused || FilterTxt.Focused;

    public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList.ItemCollectionList items, out object? hotItem) => hotItem = e == null ? null : MouseOverNode(e.X, e.Y);

    public void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    protected override void DrawControl(Graphics gr, States state) {

        #region  tmpDesign

        var tmpDesign = Design.ListBox;
        if (_appearance is not ListBoxAppearance.Gallery
                       and not ListBoxAppearance.FileSystem
                       and not ListBoxAppearance.Listbox_Boxes) { tmpDesign = (Design)_appearance; }

        #endregion

        #region  checkboxDesign

        var checkboxDesign = Design.Undefiniert;
        if (_appearance == ListBoxAppearance.Listbox_Boxes && Item.CheckBehavior != CheckBehavior.NoSelection) {
            checkboxDesign = Design.CheckBox_TextStyle;
            if (Item.CheckBehavior is CheckBehavior.AlwaysSingleSelection or CheckBehavior.SingleSelection) {
                checkboxDesign = Design.OptionButton_TextStyle;
            }
        }

        #endregion

        #region tmpState

        var tmpState = state;
        if (tmpState.HasFlag(States.Standard_MouseOver)) { tmpState ^= States.Standard_MouseOver; }
        if (tmpState.HasFlag(States.Standard_MousePressed)) { tmpState ^= States.Standard_MousePressed; }
        if (tmpState.HasFlag(States.Standard_HasFocus)) { tmpState ^= States.Standard_HasFocus; }

        #endregion

        #region tmpbuttonleisteHeight

        var tmpbuttonleisteHeight = 0;
        if (ButtonsVisible()) { tmpbuttonleisteHeight = Plus.Height; }

        #endregion

        if (Item.Count == 0) {
            SliderY.Visible = false;
            SliderY.Value = 0;
        }

        var (biggestItemX, _, heightAdded, senkrechtAllowed) = Item.ItemData();
        _ = Item.ComputeAllItemPositions(new Size(DisplayRectangle.Width, DisplayRectangle.Height - tmpbuttonleisteHeight), SliderY, biggestItemX, heightAdded, senkrechtAllowed);

        var tmpsliderWidth = 0;
        if (SliderY.Visible) { tmpsliderWidth = SliderY.Width; }

        var borderCoords = new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top,
           DisplayRectangle.Width - tmpsliderWidth, DisplayRectangle.Height - tmpbuttonleisteHeight);
        var visArea = borderCoords with { Y = (int)(borderCoords.Y + SliderY.Value) };
        if (borderCoords.Height > 0) {
            Skin.Draw_Back(gr, tmpDesign, tmpState, borderCoords, this, true);
        }
        _mouseOverItem = MouseOverNode(MousePos().X, MousePos().Y);
        object locker = new();
        _ = Parallel.ForEach(Item.ItemOrder, thisItem => {
            // Kopie von thisItem erstellen
            var currentItem = thisItem;
            if (currentItem.Pos.IntersectsWith(visArea)) {
                var itemState = tmpState;
                if (_mouseOverItem == currentItem && Enabled) { itemState |= States.Standard_MouseOver; }
                if (!currentItem.Enabled) { itemState = States.Standard_Disabled; }
                if (currentItem.Checked) { itemState |= States.Checked; }
                lock (locker) {
                    currentItem.Draw(gr, 0, (int)SliderY.Value, Item.ControlDesign, Item.ItemDesign, itemState, true, FilterTxt.Text, false, checkboxDesign); // Items müssen beim Erstellen ersetzt werden!!!!
                }
            }
        });
        if (borderCoords.Height > 0) {
            // Kann sein, wenn PaintModY größer als die Höhe ist
            if (tmpDesign == Design.ListBox) { Skin.Draw_Border(gr, tmpDesign, tmpState, borderCoords); }
        }
        if (tmpbuttonleisteHeight > 0) { Skin.Draw_Back_Transparent(gr, new Rectangle(0, borderCoords.Bottom, Width, tmpbuttonleisteHeight), this); }
    }

    protected override void OnDoubleClick(System.EventArgs e) {
        if (!Enabled) { return; }
        var nd = MouseOverNode(MousePos().X, MousePos().Y);
        if (nd == null) { return; }
        OnItemDoubleClick(new AbstractListItemEventArgs(nd));
    }

    protected override void OnHandleCreated(System.EventArgs e) {
        base.OnHandleCreated(e);
        // Um den allerersten Check nicht zu verpassen
        CheckButtons();
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        if (_mouseOverItem != null) {
            _mouseOverItem = null;
            Invalidate();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        var nd = MouseOverNode(MousePos().X, MousePos().Y);
        if (nd != _mouseOverItem) {
            _mouseOverItem = nd;
            Invalidate();
            DoQuickInfo();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (!Enabled) { return; }
        var nd = MouseOverNode(e.X, e.Y);
        if (nd != null && !nd.Enabled) { return; }
        switch (e.Button) {
            case MouseButtons.Left:
                if (nd != null) {
                    if (Appearance is ListBoxAppearance.Listbox or ListBoxAppearance.Autofilter or ListBoxAppearance.Gallery or ListBoxAppearance.FileSystem) {
                        if (nd.IsClickable()) { nd.Checked = !nd.Checked; }
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
        Down.Invalidate();
        Up.Invalidate();
        Minus.Invalidate();
        Plus.Invalidate();
        SliderY.Invalidate();
        FilterCap.Invalidate();
        FilterTxt.Invalidate();
        CheckButtons();
        base.OnEnabledChanged(e);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        CheckButtons();
        base.OnVisibleChanged(e);
    }

    private void _Item_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        CheckButtons();
        Invalidate();
        OnItemCheckedChanged();
    }

    private bool ButtonsVisible() => Plus.Visible || Minus.Visible || Up.Visible || Down.Visible || FilterTxt.Visible;

    private void CheckButtons() {
        if (!Visible) { return; }
        if (Parent == null) { return; }
        var nr = Item.Checked();
        Down.Visible = _moveAllowed;
        Up.Visible = _moveAllowed;
        Plus.Visible = _addAlloweds != AddType.None;
        Minus.Visible = _removeAllowed;
        FilterTxt.Visible = _filterAllowed;
        FilterCap.Visible = _filterAllowed;
        FilterCap.Left = _moveAllowed && _filterAllowed ? Down.Right : 0;
        FilterTxt.Left = FilterCap.Right;
        FilterTxt.Width = Minus.Left - FilterTxt.Left;
        if (_removeAllowed) {
            Minus.Enabled = nr.Count != 0;
        }
        if (_moveAllowed) {
            if (nr.Count != 1) {
                Up.Enabled = false;
                Down.Enabled = false;
            } else {
                Up.Enabled = Item[0] != nr[0];
                Down.Enabled = Item[Item.Count - 1] != nr[0];
            }
        }
    }

    private void Down_Click(object sender, System.EventArgs e) {
        var ln = -1;
        for (var z = Item.ItemOrder.Count - 1; z >= 0; z--) {
            if (Item[z] != null) {
                if (Item[z].Checked) {
                    if (ln < 0) { return; }// Befehl verwerfen...
                    Item.Swap(ln, z);
                    CheckButtons();
                    return;
                }
                ln = z;
            }
        }
    }

    private void FilterTxt_TextChanged(object sender, System.EventArgs e) => Invalidate();

    private void Item_Changed(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
    }

    private void Item_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
        OnCollectionChanged(e);
    }

    private void Minus_Click(object sender, System.EventArgs e) {
        foreach (var thisItem in Item.Checked()) {
            Item.Remove(thisItem);
        }
        CheckButtons();
    }

    private AbstractListItem? MouseOverNode(int x, int y) => ButtonsVisible() && y >= Height - Plus.Height ? null : Item[x, (int)(y + SliderY.Value)];

    private void OnAddClicked() => AddClicked?.Invoke(this, System.EventArgs.Empty);

    private void OnItemCheckedChanged() => ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnItemClicked(AbstractListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    private void OnItemDoubleClick(AbstractListItemEventArgs e) => ItemDoubleClick?.Invoke(this, e);

    private void Plus_Click(object sender, System.EventArgs e) {
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
        CheckButtons();
    }

    private void SliderY_ValueChange(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
    }

    private void Up_Click(object sender, System.EventArgs e) {
        AbstractListItem? ln = null;
        foreach (var thisItem in Item.ItemOrder) {
            if (thisItem != null) {
                if (thisItem.Checked) {
                    if (ln == null) { return; }// Befehl verwerfen...
                    Item.Swap(Item.IndexOf(ln), Item.IndexOf(thisItem));
                    CheckButtons();
                    return;
                }
                ln = thisItem;
            }
        }
    }

    #endregion
}