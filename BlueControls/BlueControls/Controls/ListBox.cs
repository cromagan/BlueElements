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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

using BlueBasics;
using BlueBasics.Enums;

using BlueBasics.Interfaces;
using BlueControls.Controls;

using BlueControls.Enums;
using BlueControls.Forms;

using BlueDatabase;
using BlueDatabase.Enums;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

using MessageBox = BlueControls.Forms.MessageBox;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("ItemClicked")]
public partial class ListBox : GenericControl, IContextMenu, IBackgroundNone, ITranslateable {

    #region Fields

    private AddType _addAlloweds = AddType.Text;

    private ListBoxAppearance _appearance;

    private bool _autoSort = true;

    private CheckBehavior _checkBehavior = CheckBehavior.SingleSelection;

    private List<string> _checked = [];

    private Design _controlDesign;

    private string _filterText = string.Empty;

    private Design _itemDesign;

    private ReadOnlyCollection<AbstractListItem>? _itemOrder;

    private SizeF _lastCheckedMaxSize = Size.Empty;

    private Size _maxNeededItemSize;

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
        Item = List<AbstractListItem>();

        BindingOperations.EnableCollectionSynchronization(Item, new object());

        _maxNeededItemSize = Size.Empty;
        _appearance = ListBoxAppearance.Listbox;
        _itemDesign = Design.Undefiniert;
        _controlDesign = Design.Undefiniert;
        //_appearance = design;
        //_autoSort = autosort;
        GetDesigns();

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

    public ListBoxAppearance Appearance {
        get => _appearance;
        set {
            if (value == _appearance && _itemDesign != Design.Undefiniert) { return; }
            _appearance = value;
            GetDesigns();
            //DesignOrStyleChanged();
        }
    }

    [DefaultValue(true)]
    public bool AutoSort {
        get => _autoSort;
        set {
            if (value == _autoSort) { return; }
            _autoSort = value;
            _maxNeededItemSize = Size.Empty;
            _itemOrder = null;
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
            if (_controlDesign == Design.Undefiniert) { Develop.DebugPrint(FehlerArt.Fehler, "ControlDesign undefiniert!"); }
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

    //public string LastFilePath { get; set; }
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<AbstractListItem> Item { get; }

    /// <summary>
    /// Itemdesign wird durch Appearance gesetzt
    /// </summary>
    /// <returns></returns>
    public Design ItemDesign //Implements IDesignAble.Design
    {
        get {
            if (_itemDesign == Design.Undefiniert) { Develop.DebugPrint(FehlerArt.Fehler, "ItemDesign undefiniert!"); }
            return _itemDesign;
        }
    }

    public ReadOnlyCollection<AbstractListItem> ItemOrder {
        get {
            _itemOrder ??= CalculateItemOrder();
            return _itemOrder;
        }
    }

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
    public List<AbstractListItem> Suggestions { get; } = new();

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

    public Size CalculateColumnAndSize() {
        var (biggestItemX, _, heightAdded, orienation) = ItemData();
        if (orienation == Orientation.Waagerecht) { return ComputeAllItemPositions(new Size(300, 300), null, biggestItemX, heightAdded, orienation, 0); }
        BreakAfterItems = CalculateColumnCount(biggestItemX, heightAdded, orienation);
        return ComputeAllItemPositions(new Size(1, 30), null, biggestItemX, heightAdded, orienation, 0);
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

    public void GetContextMenuItems(MouseEventArgs? e, List<AbstractListItem> items, out object? hotItem) => hotItem = e == null ? null : MouseOverNode(e.X, e.Y);

    public virtual void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public virtual void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public void Swap(int index1, int index2) {
        if (index1 == index2) { return; }
        if (_autoSort) { return; }

        UnRegisterEvents();
        (Item[index1], Item[index2]) = (Item[index2], Item[index1]);
        RegisterEvents();

        Invalidate();
        DoMouseMovement();
        ValidateCheckStates(_itemOrder.ToListOfString(), string.Empty);
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

    internal Size ComputeAllItemPositions(Size controlDrawingArea, Slider? sliderY, int biggestItemX, int heightAdded, Orientation senkrechtAllowed, int addy) {
        try {
            if (Math.Abs(_lastCheckedMaxSize.Width - controlDrawingArea.Width) > 0.1 || Math.Abs(_lastCheckedMaxSize.Height - controlDrawingArea.Height) > 0.1) {
                _lastCheckedMaxSize = controlDrawingArea;
                _maxNeededItemSize = Size.Empty;
            }
            if (!_maxNeededItemSize.IsEmpty) { return _maxNeededItemSize; }
            if (Item.Count == 0) {
                _maxNeededItemSize = Size.Empty;
                return Size.Empty;
            }
            PreComputeSize();
            if (_itemDesign == Design.Undefiniert) { GetDesigns(); }
            if (BreakAfterItems < 1) { senkrechtAllowed = Orientation.Waagerecht; }
            var sliderWidth = 0;
            if (sliderY != null) {
                if (BreakAfterItems < 1 && heightAdded > controlDrawingArea.Height) {
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
                        var colCount = Item.Count / BreakAfterItems;
                        var r = Item.Count % colCount;
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
            foreach (var thisItem in ItemOrder) {
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
                            if (previtem.Pos.Right + colWidth > controlDrawingArea.Width || thisItem.IsCaption) {
                                cx = 0;
                                cy = previtem.Pos.Bottom;
                            } else {
                                cx = previtem.Pos.Right;
                                cy = previtem.Pos.Top;
                            }
                        } else {
                            if (itenc % BreakAfterItems == 0) {
                                cx = previtem.Pos.Right;
                                cy = 0;
                            } else {
                                cx = previtem.Pos.Left;
                                cy = previtem.Pos.Bottom;
                            }
                        }
                    }
                    thisItem.SetCoordinates(new Rectangle(cx, cy, wi, he));
                    maxX = Math.Max(thisItem.Pos.Right, maxX);
                    maxy = Math.Max(thisItem.Pos.Bottom, maxy);
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
            Develop.CheckStackForOverflow();
            return ComputeAllItemPositions(controlDrawingArea, sliderY, biggestItemX, heightAdded, senkrechtAllowed, addy);
        }
    }

    internal void Item_CompareKeyChangedChanged(object sender, System.EventArgs e) {
        _maxNeededItemSize = Size.Empty;
        _itemOrder = null;
    }

    /// <summary>
    ///  BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
    /// </summary>
    /// <returns></returns>
    internal (int BiggestItemX, int BiggestItemY, int HeightAdded, Orientation Orientation) ItemData() {
        try {
            var w = 16;
            var h = 0;
            var hall = 0;
            var sameh = -1;
            var or = Orientation.Senkrecht;
            PreComputeSize();

            foreach (var thisItem in ItemOrder) {
                if (thisItem != null) {
                    var s = thisItem.SizeUntouchedForListBox(ItemDesign);
                    w = Math.Max(w, s.Width);
                    h = Math.Max(h, s.Height);
                    hall += s.Height;
                    if (sameh < 0) {
                        sameh = thisItem.SizeUntouchedForListBox(ItemDesign).Height;
                    } else {
                        if (sameh != thisItem.SizeUntouchedForListBox(ItemDesign).Height) { or = Orientation.Waagerecht; }
                        sameh = thisItem.SizeUntouchedForListBox(ItemDesign).Height;
                    }
                    if (thisItem is not TextListItem) { or = Orientation.Waagerecht; }
                }
            }

            return (w, h, hall, or);
        } catch {
            Develop.CheckStackForOverflow();
            return ItemData();
        }
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

        var (biggestItemX, _, heightAdded, senkrechtAllowed) = ItemData();
        _ = ComputeAllItemPositions(new Size(DisplayRectangle.Width, DisplayRectangle.Height), SliderY, biggestItemX, heightAdded, senkrechtAllowed, addy);

        var tmpSliderWidth = SliderY.Visible ? SliderY.Width : 0;

        var borderCoords = new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width - tmpSliderWidth, DisplayRectangle.Height);
        var visArea = borderCoords with { Y = borderCoords.Y + (int)SliderY.Value };

        if (borderCoords.Height > 0) {
            Skin.Draw_Back(gr, tmpDesign, tmpState, borderCoords, this, true);
        }

        //_mouseOverItem = MouseOverNode(MousePos().X, MousePos().Y);
        object locker = new();

        Parallel.ForEach(_itemOrder, thisItem => {
            var currentItem = thisItem;
            if (currentItem.Pos.IntersectsWith(visArea)) {
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

    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
        _maxNeededItemSize = Size.Empty;
        _itemOrder = null;
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
        for (var z = _itemOrder.Count - 1; z >= 0; z--) {
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
        foreach (var thisItem in _itemOrder) {
            if (thisItem == _mouseOverItem) {
                if (ln == null) { return; }// Befehl verwerfen...
                Swap(Item.IndexOf(ln), Item.IndexOf(thisItem));
                return;
            }
            ln = thisItem;
        }
    }

    private int CalculateColumnCount(int biggestItemWidth, int allItemsHeight, Orientation orientation) {
        if (orientation != Orientation.Senkrecht) {
            Develop.DebugPrint(FehlerArt.Fehler, "Nur 'senkrecht' erlaubt mehrere Spalten");
        }
        if (Item.Count < 12) { return -1; }  // <10 ergibt dividieb by zere, weil es da 0 einträge währen bei 10 Spalten
        var dithemh = allItemsHeight / Item.Count;
        for (var testSp = 10; testSp >= 1; testSp--) {
            var colc = Item.Count / testSp;
            var rest = Item.Count % colc;
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

    private ReadOnlyCollection<AbstractListItem> CalculateItemOrder() {
        var l = new List<AbstractListItem>();
        l.AddRange(Item);

        if (_autoSort) { l.Sort(); }

        return l.AsReadOnly();
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

            if (_moveAllowed && !_autoSort && Item.Count > 1) {
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

            if (_moveAllowed && !_autoSort && Item.Count > 1) {
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

    private void GetDesigns() {
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
                Develop.DebugPrint(FehlerArt.Fehler, "Unbekanntes Design: " + _appearance);
                break;
        }
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

    private void PreComputeSize() {
        try {
            _ = Parallel.ForEach(Item, thisItem => {
                _ = thisItem?.SizeUntouchedForListBox(ItemDesign);
            });
        } catch {
            Develop.CheckStackForOverflow();
            PreComputeSize();
        }
    }

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