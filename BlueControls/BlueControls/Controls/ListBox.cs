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
using BlueControls.CellRenderer;
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
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using MessageBox = BlueControls.Forms.MessageBox;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("ItemClicked")]
public sealed partial class ListBox : GenericControl, IContextMenu, IBackgroundNone, ITranslateable {

    #region Fields

    private readonly List<AbstractListItem> _item = [];
    private AddType _addAlloweds = AddType.Text;

    private ListBoxAppearance _appearance;

    private bool _autoSort = true;

    private CheckBehavior _checkBehavior = CheckBehavior.SingleSelection;

    private List<string> _checked = [];

    private Design _controlDesign;

    private string _filterText = string.Empty;
    private Design _itemDesign;

    private bool _itemEditAllowed;
    private SizeF _lastCheckedMaxSize = Size.Empty;
    private Size _maxNeededItemSize;

    private bool _mousemoved;

    //Muss was gesetzt werden, sonst hat der Designer nachher einen Fehler
    private AbstractListItem? _mouseOverItem;

    /// <summary>
    /// Einfaches Flag, dass die Buttons nur einblendet, wenn eine Mausbewegung stattgefunden hat.
    /// Bei einem MouseWheel wird nichts eingeblendet
    /// </summary>
    private Point _mousepos = Point.Empty;

    private bool _moveAllowed;
    private bool _removeAllowed;
    private bool _sorted;

    #endregion

    #region Constructors

    public ListBox() : base(true, false, true) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        BindingOperations.EnableCollectionSynchronization(_item, new object());

        _maxNeededItemSize = Size.Empty;
        _appearance = ListBoxAppearance.Listbox;
        _itemDesign = Design.Undefiniert;
        _controlDesign = Design.Undefiniert;
        InvalidateItemOrder();
        GetDesigns();
    }

    #endregion

    #region Delegates

    public delegate AbstractListItem? dAddMethod();

    #endregion

    #region Events

    /// <summary>
    /// Wir am Anfang des dirket nach dem Klicken ausgelöst. Damit können Items z.B. zurückgeschreiben werden.
    /// </summary>
    public event EventHandler? AddClicked;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public event EventHandler<AbstractListItemEventArgs>? ItemAddedByClick;

    public event EventHandler? ItemCheckedChanged;

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    public event EventHandler<AbstractListItemEventArgs>? RemoveClicked;

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

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public dAddMethod? AddMethod { get; set; }

    [DefaultValue(ListBoxAppearance.Listbox)]
    public ListBoxAppearance Appearance {
        get => _appearance;
        set {
            if (value == _appearance && _itemDesign != Design.Undefiniert) { return; }
            _appearance = value;
            GetDesigns();
            Invalidate();
        }
    }

    [DefaultValue(true)]
    public bool AutoSort {
        get => _autoSort;
        set {
            if (value == _autoSort) { return; }
            _autoSort = value;
            InvalidateItemOrder();
        }
    }

    public int BreakAfterItems { get; private set; }

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

    /// <summary>
    /// ControlDesign wird durch Appearance gesetzt
    /// </summary>
    /// <returns></returns>
    public Design ControlDesign //Implements IDesignAble.Design
    {
        get {
            if (_controlDesign == Design.Undefiniert) { Develop.DebugPrint(ErrorType.Error, "ControlDesign undefiniert!"); }
            return _controlDesign;
        }
    }

    [DefaultValue("")]
    public string FilterText {
        get => _filterText;
        set {
            if (_filterText == value) { return; }
            _filterText = value;
            Invalidate();
        }
    }

    public int ItemCount => _item.Count;

    /// <summary>
    /// Itemdesign wird durch Appearance gesetzt
    /// </summary>
    /// <returns></returns>
    public Design ItemDesign //Implements IDesignAble.Design
    {
        get {
            if (_itemDesign == Design.Undefiniert) { Develop.DebugPrint(ErrorType.Error, "ItemDesign undefiniert!"); }
            return _itemDesign;
        }
    }

    [DefaultValue(false)]
    public bool ItemEditAllowed {
        get => _itemEditAllowed;
        set {
            if (_itemEditAllowed == value) { return; }
            _itemEditAllowed = value;
            DoMouseMovement();
        }
    }

    public ReadOnlyCollection<AbstractListItem> Items => _item.AsReadOnly();

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

    public Renderer_Abstract Renderer => Renderer_Abstract.Default;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<AbstractListItem> Suggestions { get; } = [];

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    #endregion

    #region Indexers

    public AbstractListItem? this[string @internal] {
        get {
            try {
                return _item.Get(@internal);
            } catch {
                Develop.CheckStackOverflow();
                return this[@internal];
            }
        }
    }

    public AbstractListItem? this[int no] {
        get {
            try {
                return no < 0 || no > _item.Count ? null : _item[no];
            } catch {
                Develop.CheckStackOverflow();
                return this[no];
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    ///  BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
    /// </summary>
    /// <returns></returns>
    public static (int BiggestItemX, int BiggestItemY, int HeightAdded, Orientation Orientation) ItemData(
        List<AbstractListItem> item, Design itemDesign) {
        try {
            var w = 16;
            var h = 0;
            var hall = 0;
            var sameh = -1;
            var or = Orientation.Senkrecht;
            PreComputeSize(item, itemDesign);

            foreach (var thisItem in item) {
                if (thisItem != null) {
                    var s = thisItem.SizeUntouchedForListBox(itemDesign);
                    w = Math.Max(w, s.Width);
                    h = Math.Max(h, s.Height);
                    hall += s.Height;
                    if (sameh < 0) {
                        sameh = thisItem.SizeUntouchedForListBox(itemDesign).Height;
                    } else {
                        if (sameh != thisItem.SizeUntouchedForListBox(itemDesign).Height) { or = Orientation.Waagerecht; }
                        sameh = thisItem.SizeUntouchedForListBox(itemDesign).Height;
                    }
                    if (thisItem is not TextListItem) { or = Orientation.Waagerecht; }
                }
            }

            return (w, h, hall, or);
        } catch {
            Develop.CheckStackOverflow();
            return ItemData(item, itemDesign);
        }
    }

    public static void PreComputeSize(List<AbstractListItem> item, Design itemDesign) {
        try {
            _ = Parallel.ForEach(item, thisItem => {
                _ = thisItem?.SizeUntouchedForListBox(itemDesign);
            });
        } catch {
            Develop.CheckStackOverflow();
            PreComputeSize(item, itemDesign);
        }
    }

    public AbstractListItem? Add_Text() {
        var val = InputBoxComboStyle.Show("Bitte geben sie einen Wert ein:", Suggestions, true);

        return string.IsNullOrEmpty(val) ? null : (AbstractListItem)ItemOf(val);
    }

    public AbstractListItem? Add_TextBySuggestion() {
        if (Suggestions.Count == 0) {
            MessageBox.Show("Keine (weiteren) Werte vorhanden.", ImageCode.Information, "OK");
            return null;
        }

        var rück = InputBoxListBoxStyle.Show("Bitte wählen sie einen Wert:", Suggestions, CheckBehavior.SingleSelection, null, AddType.None);

        return rück is not { Count: not 0 } ? null : Suggestions.Get(rück[0]);
    }

    public Size CalculateColumnAndSize(Renderer_Abstract renderer) {
        var (biggestItemX, _, heightAdded, orienation) = ItemData(_item, _itemDesign);
        if (orienation == Orientation.Waagerecht) { return ComputeAllItemPositions(new Size(300, 300), null, biggestItemX, heightAdded, orienation, 0, renderer); }
        BreakAfterItems = CalculateColumnCount(biggestItemX, heightAdded, orienation);
        return ComputeAllItemPositions(new Size(1, 30), null, biggestItemX, heightAdded, orienation, 0, renderer);
    }

    public void Check(IEnumerable<string> ali) {
        if (ali == null) { return; }

        // Sammle nur die Items, die noch nicht in der checked-Liste sind
        var itemsToAdd = ali.Where(name => !IsChecked(name)).ToList();

        if (itemsToAdd.Count == 0) { return; } // Nichts zu tun

        // Erstelle eine neue Liste mit kombiniertem Inhalt
        ValidateCheckStates([.. _checked, .. itemsToAdd], itemsToAdd.FirstOrDefault());
    }

    public void Check(AbstractListItem ali) => Check(ali.KeyName);

    public void Check(string name) {
        if (IsChecked(name)) { return; }

        List<string> l = [.. _checked, name];

        ValidateCheckStates(l, name);
    }

    public ReadOnlyCollection<AbstractListItem> CheckedItems() {
        var l = new List<AbstractListItem>();

        foreach (var thisItem in _item) {
            if (thisItem != null && IsChecked(thisItem)) {
                l.Add(thisItem);
            }
        }
        return l.AsReadOnly();
    }

    public void DoContextMenuItemClick(ContextMenuItemClickedEventArgs e) => OnContextMenuItemClicked(e);

    public new void Focus() {
        if (Focused()) { return; }
        _ = base.Focus();
    }

    public new bool Focused() => base.Focused || btnPlus.Focused || btnMinus.Focused || btnUp.Focused || btnDown.Focused || btnEdit.Focused || SliderY.Focused();

    public void GetContextMenuItems(ContextMenuInitEventArgs e) => OnContextMenuInit(e);

    public void GetDesigns() {
        _controlDesign = (Design)_appearance;
        switch (_appearance) {
            case ListBoxAppearance.Autofilter:
                _itemDesign = Design.Item_Autofilter;
                break;

            case ListBoxAppearance.DropdownSelectbox:
                _itemDesign = Design.Item_DropdownMenu;
                break;

            case ListBoxAppearance.Gallery:
                _itemDesign = Design.Item_Listbox;
                _controlDesign = Design.ListBox;
                break;

            case ListBoxAppearance.FileSystem:
                _itemDesign = Design.Item_Listbox;
                _controlDesign = Design.ListBox;
                break;

            case ListBoxAppearance.Listbox_Boxes:
            case ListBoxAppearance.Listbox:
                _itemDesign = Design.Item_Listbox;
                _controlDesign = Design.ListBox;
                break;

            case ListBoxAppearance.KontextMenu:
                _itemDesign = Design.Item_KontextMenu;
                break;

            case ListBoxAppearance.ComboBox_Textbox:
                _itemDesign = Design.ComboBox_Textbox;
                break;

            default:
                Develop.DebugPrint(ErrorType.Error, "Unbekanntes Design: " + _appearance);
                break;
        }
    }

    public void ItemAdd(AbstractListItem? item) {
        if (item == null) { Develop.DebugPrint(ErrorType.Error, "Item ist null"); return; }
        if (_item.Contains(item)) { Develop.DebugPrint(ErrorType.Error, "Bereits vorhanden!"); return; }
        if (this[item.KeyName] != null) { Develop.DebugPrint(ErrorType.Warning, "Name bereits vorhanden: " + item.KeyName); return; }

        if (string.IsNullOrEmpty(item.KeyName)) { Develop.DebugPrint(ErrorType.Error, "Item ohne Namen!"); return; }
        AddAndRegister(item);
        InvalidateItemOrder();
        ValidateCheckStates(_checked, item.KeyName);
    }

    public void ItemAddRange(List<AbstractListItem>? items) {
        if (items is not { Count: not 0 }) { return; }

        foreach (var thisIt in items) {
            AddAndRegister(thisIt);
        }

        InvalidateItemOrder();
        ValidateCheckStates(_checked, items[0].KeyName);
    }

    public void ItemAddRange(List<string>? list) {
        if (list is not { Count: not 0 }) { return; }

        foreach (var thisstring in list) {
            if (!string.IsNullOrEmpty(thisstring) && this[thisstring] == null) {
                var it = ItemOf(thisstring, thisstring);
                AddAndRegister(it);
            }
        }

        InvalidateItemOrder();
        ValidateCheckStates(_checked, list[0]);
    }

    public void ItemClear() {
        if (_item.Count == 0) { return; }

        foreach (var thisIt in _item) {
            thisIt.CompareKeyChanged -= Item_CompareKeyChangedChanged;
        }

        _checked.Clear();

        _item.Clear();
        InvalidateItemOrder();
        ValidateCheckStates(null, string.Empty);
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public void Remove(string keyName) {
        if (string.IsNullOrEmpty(keyName)) { return; }

        var nl = new List<AbstractListItem>();
        nl.AddRange(_item);

        foreach (var thisItem in nl) {
            if (thisItem.KeyName.Equals(keyName, StringComparison.OrdinalIgnoreCase)) {
                RemoveAndUnRegister(thisItem);
            }
        }

        ValidateCheckStates(_checked, string.Empty);
    }

    public void Remove(AbstractListItem? item) {
        if (item == null) { return; }
        if (!_item.Contains(item)) { return; }

        RemoveAndUnRegister(item);

        ValidateCheckStates(_checked, string.Empty);
    }

    public void Swap(int index1, int index2) {
        if (index1 == index2) { return; }
        if (_autoSort) { return; }

        (_item[index1], _item[index2]) = (_item[index2], _item[index1]);

        InvalidateItemOrder();
        DoItemOrder();
        ValidateCheckStates(_item.ToListOfString(), string.Empty);
    }

    public void UnCheck(AbstractListItem ali) => UnCheck(ali.KeyName);

    public void UnCheck(IEnumerable<string> ali) {
        if (ali == null) { return; }

        // Filtere die zu entfernenden Items aus der vorhandenen Liste
        var newCheckedList = _checked.Except(ali).ToList();

        // Wenn sich nichts geändert hat, früh zurückkehren
        if (newCheckedList.Count == _checked.Count) { return; }

        // Validiere alle auf einmal
        ValidateCheckStates(newCheckedList, string.Empty);
    }

    public void UnCheck(string name) {
        if (!IsChecked(name)) { return; }

        List<string> l = [.. _checked];
        _ = l.Remove(name);

        ValidateCheckStates(l, string.Empty);
    }

    public void UncheckAll() => ValidateCheckStates([], string.Empty);

    internal void AddAndCheck(AbstractListItem? ali) {
        if (ali == null) { return; }

        if (_item.Get(ali.KeyName) != null) { return; }

        var tmp = _checkBehavior;
        _checkBehavior = CheckBehavior.MultiSelection;

        ItemAdd(ali);
        //_itemx.Remove(ali.KeyName);

        //_itemx.Add(ali);
        _checkBehavior = tmp;
        Check(ali);
    }

    internal Size ComputeAllItemPositions(Size controlDrawingArea, Slider? sliderY, int biggestItemX, int heightAdded, Orientation senkrechtAllowed, int addy, Renderer_Abstract renderer) {
        try {
            if (Math.Abs(_lastCheckedMaxSize.Width - controlDrawingArea.Width) > 0.1 || Math.Abs(_lastCheckedMaxSize.Height - controlDrawingArea.Height) > 0.1) {
                _lastCheckedMaxSize = controlDrawingArea;
                _maxNeededItemSize = Size.Empty;
            }
            if (!_maxNeededItemSize.IsEmpty) { return _maxNeededItemSize; }
            if (_item.Count == 0) {
                _maxNeededItemSize = Size.Empty;
                return Size.Empty;
            }

            if (_itemDesign == Design.Undefiniert) { GetDesigns(); }
            PreComputeSize(_item, _itemDesign);

            if (BreakAfterItems < 1) { senkrechtAllowed = Orientation.Waagerecht; }
            var sliderWidth = 0;
            if (sliderY != null) {
                if (BreakAfterItems < 1 && heightAdded + addy > controlDrawingArea.Height) {
                    sliderWidth = sliderY.Width;
                }
            }

            #region colWidth

            int colWidth;
            switch (_appearance) {
                case ListBoxAppearance.Gallery:
                    colWidth = 200;
                    break;

                case ListBoxAppearance.FileSystem:
                    colWidth = 110;
                    break;

                default:
                    // u.a. Autofilter
                    if (BreakAfterItems < 1) {
                        colWidth = controlDrawingArea.Width - sliderWidth;
                    } else {
                        var colCount = _item.Count / BreakAfterItems;
                        var r = _item.Count % colCount;
                        if (r != 0) { colCount++; }
                        colWidth = controlDrawingArea.Width < 5 ? biggestItemX : (controlDrawingArea.Width - sliderWidth) / colCount;
                    }
                    break;
            }

            #endregion

            var maxX = int.MinValue;
            var maxy = int.MinValue;
            var itenc = -1;
            AbstractListItem? previtem = null;
            DoItemOrder();
            foreach (var thisItem in _item) {
                // PaintmodX kann immer abgezogen werden, da es eh nur bei einspaltigen Listboxen verändert wird!
                if (thisItem != null) {
                    var cx = 0;
                    var cy = 0;
                    var wi = colWidth;
                    int he;
                    itenc++;
                    if (senkrechtAllowed == Orientation.Waagerecht) {
                        if (thisItem.IsCaption) { wi = controlDrawingArea.Width - sliderWidth; }
                        he = thisItem.HeightForListBox(_appearance, wi, ItemDesign);
                    } else {
                        he = thisItem.HeightForListBox(_appearance, wi, ItemDesign);
                    }
                    if (previtem != null) {
                        if (senkrechtAllowed == Orientation.Waagerecht) {
                            if (previtem.Position.Right + colWidth > controlDrawingArea.Width || thisItem.IsCaption) {
                                cx = 0;
                                cy = previtem.Position.Bottom;
                            } else {
                                cx = previtem.Position.Right;
                                cy = previtem.Position.Top;
                            }
                        } else {
                            if (itenc % BreakAfterItems == 0) {
                                cx = previtem.Position.Right;
                                cy = 0;
                            } else {
                                cx = previtem.Position.Left;
                                cy = previtem.Position.Bottom;
                            }
                        }
                    }
                    thisItem.SetCoordinates(new Rectangle(cx, cy, wi, he));
                    maxX = Math.Max(thisItem.Position.Right, maxX);
                    maxy = Math.Max(thisItem.Position.Bottom, maxy);
                    previtem = thisItem;
                }
            }

            maxy += addy;

            #region  sliderY

            if (sliderY != null) {
                bool setTo0;

                if (sliderWidth > 0) {
                    if (maxy - controlDrawingArea.Height <= 0) {
                        sliderY.Enabled = false;
                        setTo0 = true;
                    } else {
                        sliderY.Left = Width - sliderY.Width;
                        sliderY.Enabled = true;
                        sliderY.Minimum = 0;
                        sliderY.SmallChange = 16;
                        sliderY.LargeChange = controlDrawingArea.Height;
                        sliderY.Maximum = maxy - controlDrawingArea.Height;
                        setTo0 = false;
                    }
                    sliderY.Height = controlDrawingArea.Height;
                    sliderY.Visible = true;
                } else {
                    setTo0 = true;
                    sliderY.Visible = false;
                }

                if (setTo0) {
                    sliderY.Minimum = 0;
                    sliderY.Maximum = 0;
                    sliderY.Value = 0;
                }
            }

            #endregion

            _maxNeededItemSize = new Size(maxX, maxy);
            return _maxNeededItemSize;
        } catch {
            Develop.CheckStackOverflow();
            return ComputeAllItemPositions(controlDrawingArea, sliderY, biggestItemX, heightAdded, senkrechtAllowed, addy, renderer);
        }
    }

    internal void Item_CompareKeyChangedChanged(object sender, System.EventArgs e) => InvalidateItemOrder();

    internal void SetValuesTo(List<string> values) {
        var ist = _item.ToListOfString();
        var zuviel = ist.Except(values).ToList();
        var zuwenig = values.Except(ist).ToList();
        // Zu viele im Mains aus der Liste löschen
        foreach (var thisString in zuviel) {
            if (!values.Contains(thisString)) {
                Remove(thisString);
            }
        }

        // und die Mains auffüllen
        foreach (var thisString in zuwenig) {
            var it = IO.FileExists(thisString)
                ? thisString.FileType() == FileFormat.Image
                    ? ItemOf(thisString, thisString, thisString.FileNameWithoutSuffix())
                    : ItemOf(thisString.FileNameWithSuffix(), thisString, QuickImage.Get(thisString.FileType(), 48))
                : (AbstractListItem)ItemOf(thisString);
            AddAndRegister(it);
        }

        InvalidateItemOrder();
    }

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                AddMethod = null;
            }
            _item.Clear();
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

        if (_addAlloweds != AddType.None) { addy = 33; }

        if (_item.Count == 0) {
            SliderY.Visible = false;
            SliderY.Value = 0;
            addy = 0;
        }

        var (biggestItemX, _, heightAdded, senkrechtAllowed) = ItemData(_item, _itemDesign);
        _ = ComputeAllItemPositions(new Size(DisplayRectangle.Width, DisplayRectangle.Height), SliderY, biggestItemX, heightAdded, senkrechtAllowed, addy, Renderer);

        var tmpSliderWidth = SliderY.Visible ? SliderY.Width : 0;

        var borderCoords = new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width - tmpSliderWidth, DisplayRectangle.Height);
        var visArea = borderCoords with { Y = borderCoords.Y + (int)SliderY.Value };

        if (borderCoords.Height > 0) {
            gr.ScaleTransform(1, 1);
            Skin.Draw_Back(gr, tmpDesign, tmpState, borderCoords, this, true);
        }

        //_mouseOverItem = MouseOverNode(MousePos().X, MousePos().Y);
        object locker = new();
        DoItemOrder();

        _ = Parallel.ForEach(_item, thisItem => {
            var currentItem = thisItem;
            if (currentItem.Position.IntersectsWith(visArea)) {
                var itemState = tmpState;
                if (_mouseOverItem == currentItem && Enabled) { itemState |= States.Standard_MouseOver; }

                if (!currentItem.Enabled) { itemState = States.Standard_Disabled; }

                if (CheckBehavior != CheckBehavior.AllSelected && IsChecked(currentItem)) { itemState |= States.Checked; }

                lock (locker) {
                    currentItem.Draw(gr, 0, (int)SliderY.Value, _controlDesign, _itemDesign, itemState, true, _filterText, false, checkboxDesign);
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

    //protected override void OnDoubleClick(System.EventArgs e) {
    //    if (!Enabled) { return; }
    //    var nd = MouseOverNode(MousePos().X, MousePos().Y);
    //    if (nd == null) { return; }
    //    OnItemDoubleClick(new AbstractListItemEventArgs(nd));
    //}
    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);

        DoMouseMovement();
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        if (e.X != _mousepos.X || e.Y != _mousepos.Y) { _mousemoved = true; }
        _mousepos = new Point(e.X, e.Y);
        DoMouseMovement();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (!Enabled) { return; }
        var nd = MouseOverNode(e.X, e.Y);
        if (nd is { Enabled: false }) { return; }
        switch (e.Button) {
            case MouseButtons.Left:
                if (nd != null) {
                    if (_appearance is ListBoxAppearance.Listbox or
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
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, MouseOverNode(e.X, e.Y), e);
                break;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);
        if (!SliderY.Visible) { return; }
        _mousemoved = false;
        SliderY.DoMouseWheel(e);
    }

    protected override void OnParentEnabledChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        DoMouseMovement();
        base.OnParentEnabledChanged(e);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        DoMouseMovement();
        base.OnVisibleChanged(e);
    }

    private void AddAndRegister(AbstractListItem item) {
        _item.Add(item);
        item.CompareKeyChanged += Item_CompareKeyChangedChanged;
        item.PropertyChanged += Item_PropertyChanged;
    }

    private void btnDown_Click(object sender, System.EventArgs e) {
        var ln = -1;
        DoItemOrder();
        for (var z = _item.Count - 1; z >= 0; z--) {
            if (_item[z] == _mouseOverItem) {
                if (ln < 0) { return; }// Befehl verwerfen...
                Swap(ln, z);
                return;
            }
            ln = z;
        }
    }

    private void btnEdit_Click(object sender, System.EventArgs e) {
        if (_itemEditAllowed && _mouseOverItem is ReadableListItem { Item: IEditable ie }) {
            ie.Edit();
        }
    }

    private void btnMinus_Click(object sender, System.EventArgs e) {
        if (_mouseOverItem == null) { return; }

        //if (_checkBehaviorx == CheckBehavior.AlwaysSingleSelection && Item.Count < 2) { return; }
        if (CheckboxDesign() != Design.Undefiniert) { return; }

        OnRemoveClicked(new AbstractListItemEventArgs(_mouseOverItem));

        UnCheck(_mouseOverItem);

        if (_checkBehavior != CheckBehavior.AllSelected) {
            // Z.B. die Sktipt-Liste.
            // Items können gewählt werden, aber auch gelöscht
            Remove(_mouseOverItem);
        }
    }

    private void btnPlus_Click(object sender, System.EventArgs e) {
        OnAddClicked();

        AbstractListItem? toAdd = null;

        switch (_addAlloweds) {
            case AddType.UserDef:
                toAdd = AddMethod?.Invoke();
                break;

            case AddType.Text:
                toAdd = Add_Text();
                break;

            case AddType.OnlySuggests:
                toAdd = Add_TextBySuggestion();
                break;

            case AddType.None:
                break;

            default:
                Develop.DebugPrint(_addAlloweds);
                break;
        }

        if (toAdd is { } ali) {
            AddAndCheck(ali);

            if (_itemEditAllowed && ali is ReadableListItem { Item: IEditable ie }) {
                ie.Edit();
            }

            OnItemAddedByClick(new AbstractListItemEventArgs(ali));
        }

        DoMouseMovement();
    }

    private void btnUp_Click(object sender, System.EventArgs e) {
        AbstractListItem? ln = null;
        DoItemOrder();
        foreach (var thisItem in _item) {
            if (thisItem == _mouseOverItem) {
                if (ln == null) { return; }// Befehl verwerfen...
                Swap(_item.IndexOf(ln), _item.IndexOf(thisItem));
                return;
            }
            ln = thisItem;
        }
    }

    private int CalculateColumnCount(int biggestItemWidth, int allItemsHeight, Orientation orientation) {
        if (orientation != Orientation.Senkrecht) {
            Develop.DebugPrint(ErrorType.Error, "Nur 'senkrecht' erlaubt mehrere Spalten");
        }
        if (_item.Count < 12) { return -1; }  // <10 ergibt dividieb by zere, weil es da 0 einträge währen bei 10 Spalten
        var dithemh = allItemsHeight / _item.Count;
        for (var testSp = 10; testSp >= 1; testSp--) {
            var colc = _item.Count / testSp;
            var rest = _item.Count % colc;
            var ok = !(rest > 0 && rest < colc / 2);
            if (colc < 5) { ok = false; }
            if (colc > 20) { ok = false; }
            if (colc * dithemh > 600) { ok = false; }
            if (colc * dithemh < 150) { ok = false; }
            if (testSp * biggestItemWidth > 600) { ok = false; }
            if (colc * (float)dithemh / (testSp * (float)biggestItemWidth) < 0.5) { ok = false; }
            if (ok) {
                return colc;
            }
        }
        return -1;
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

    private void DoItemOrder() {
        if (!_autoSort || _sorted) { return; }
        _item.Sort();
        _sorted = true;
    }

    private void DoMouseMovement() {
        if (IsDisposed) { return; }

        var isInForm = !(MousePos().X < 0 || MousePos().Y < 0 || MousePos().X > Width || MousePos().Y > Height);

        var nd = MouseOverNode(MousePos().X, MousePos().Y);
        if (!Enabled || Parent is not { Enabled: true } || !Visible) {
            nd = null;
            isInForm = false;
        }

        QuickInfo = nd?.QuickInfo ?? string.Empty;

        var showAdd = AddAllowed != AddType.None && isInForm;

        showAdd = showAdd && (Math.Abs(SliderY.Value - SliderY.Maximum) < Constants.IntTolerance || _mousemoved);

        if (showAdd) {
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

        if (_mouseOverItem != null && _mousemoved) {
            var pos = _mouseOverItem.Position.Right;

            #region down-Button

            //btnUp.Enabled = Item[0].KeyName != nr[0];
            //btnDown.Enabled = Item[Item.Count - 1].KeyName != nr[0];

            if (_moveAllowed && !_autoSort && _item.Count > 1) {
                btnDown.Width = 16;
                btnDown.Height = 16;
                pos -= btnDown.Width;
                btnDown.Top = _mouseOverItem.Position.Y - (int)SliderY.Value;
                btnDown.Left = pos;
                btnDown.Visible = true;
                btnDown.Enabled = _item[_item.Count - 1] != _mouseOverItem;
            } else {
                btnDown.Visible = false;
            }

            #endregion

            #region Up-Button

            if (_moveAllowed && !_autoSort && _item.Count > 1) {
                btnUp.Width = 16;
                btnUp.Height = 16;
                pos -= btnUp.Width;
                btnUp.Top = _mouseOverItem.Position.Y - (int)SliderY.Value;
                btnUp.Left = pos;
                btnUp.Visible = true;
                btnUp.Enabled = _item[0] != _mouseOverItem;
            } else {
                btnUp.Visible = false;
            }

            #endregion

            #region Löschen-Button

            var removeok = _removeAllowed;

            if (CheckboxDesign() != Design.Undefiniert) { removeok = false; }
            if (CheckBehavior == CheckBehavior.MultiSelection) { removeok = false; }

            if (removeok) {
                btnMinus.Width = 16;
                btnMinus.Height = 16;
                pos -= btnMinus.Width;
                btnMinus.Top = _mouseOverItem.Position.Y - (int)SliderY.Value;
                btnMinus.Left = pos;
                btnMinus.Visible = true;
                btnMinus.Enabled = true;
            } else {
                btnMinus.Visible = false;
            }

            #endregion

            #region Edit-Button

            var editok = false;

            if (_itemEditAllowed && _mouseOverItem is ReadableListItem rli) {
                if (rli.Item is IEditable { Editor: not null }) { editok = true; }
                if (rli.Item is ISimpleEditor) { editok = true; }
            }

            if (editok) {
                btnEdit.Width = 16;
                btnEdit.Height = 16;
                pos -= btnEdit.Width;
                btnEdit.Top = _mouseOverItem.Position.Y - (int)SliderY.Value;
                btnEdit.Left = pos;
                btnEdit.Visible = true;
                btnEdit.Enabled = true;
            } else {
                btnEdit.Visible = false;
            }

            #endregion
        } else {
            btnMinus.Visible = false;
            btnUp.Visible = false;
            btnDown.Visible = false;
            btnEdit.Visible = false;
        }

        Invalidate();
        DoQuickInfo();
        Invalidate();
    }

    private void InvalidateItemOrder() {
        _maxNeededItemSize = Size.Empty;
        _sorted = false;
    }

    private bool IsChecked(AbstractListItem thisItem) => IsChecked(thisItem.KeyName);

    private bool IsChecked(string name) => _checked.Contains(name);

    private void Item_PropertyChanged(object sender, System.EventArgs e) => Invalidate();

    private AbstractListItem? MouseOverNode(int x, int y) => _item.FirstOrDefault(thisItem => thisItem != null && thisItem.Contains(x, y + (int)SliderY.Value));

    private void OnAddClicked() => AddClicked?.Invoke(this, System.EventArgs.Empty);

    private void OnItemAddedByClick(AbstractListItemEventArgs e) => ItemAddedByClick?.Invoke(this, e);

    private void OnItemCheckedChanged() => ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnItemClicked(AbstractListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    private void OnRemoveClicked(AbstractListItemEventArgs e) => RemoveClicked?.Invoke(this, e);

    private void RemoveAndUnRegister(AbstractListItem item) {
        item.CompareKeyChanged -= Item_CompareKeyChangedChanged;
        item.PropertyChanged -= Item_PropertyChanged;
        _ = _item.Remove(item);
        _ = _checked.Remove(item.KeyName);
        InvalidateItemOrder();
    }

    //private void OnItemDoubleClick(AbstractListItemEventArgs e) => ItemDoubleClick?.Invoke(this, e);

    private void SliderY_ValueChange(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        _mouseOverItem = null; // Damit die Buttons neu berechnet werden.
        _mousemoved = false;
        DoMouseMovement();
        Invalidate();
    }

    private void ValidateCheckStates(List<string>? newCheckedItems, string lastaddeditem) {
        newCheckedItems ??= [];

        switch (_checkBehavior) {
            case CheckBehavior.AllSelected:
                //SetValuesTo(newCheckedItems);
                //newCheckedItems = _item.ToListOfString();
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

            default:
                Develop.DebugPrint(_checkBehavior);
                break;
        }

        List<string> newList = [];

        foreach (var thisit in newCheckedItems) {
            var it = _item.Get(thisit) ?? ItemOf(thisit);

            if (it.IsClickable()) {
                newList.Add(thisit);
            }
        }

        if (newList.IsDifferentTo(_checked)) {
            if (_checkBehavior == CheckBehavior.AllSelected) {
                SetValuesTo(newCheckedItems);
            }

            _checked = newList;
            OnItemCheckedChanged();
        }

        Invalidate();
    }

    #endregion
}