// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueBasics.Interfaces;
using BlueControls.CellRenderer;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(ItemClicked))]
public sealed partial class ListBox : ZoomPad, IContextMenu, ITranslateable {

    #region Fields

    private readonly List<AbstractListItem> _item = [];
    private readonly object _itemLock = new();
    private ListBoxAppearance _appearance;
    private CheckBehavior _checkBehavior = CheckBehavior.SingleSelection;
    private List<string> _checked = [];
    private Design _controlDesign;
    private Design _itemDesign;
    private SizeF _lastCheckedMaxSize = Size.Empty;
    private Size _maxNeededItemSize;

    //Muss was gesetzt werden, sonst hat der Designer nachher einen Fehler
    private AbstractListItem? _mouseOverItem;

    private bool _sorted;

    #endregion

    #region Constructors

    public ListBox() : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

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

    public event EventHandler<AbstractListItemEventArgs>? ItemAddedByClick;

    public event EventHandler? ItemCheckedChanged;

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    public event EventHandler<AbstractListItemEventArgs>? RemoveClicked;

    public event EventHandler? UpDownClicked;

    #endregion

    #region Properties

    [DefaultValue(true)]
    public AddType AddAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            DoMouseMovement(-1, -1);
        }
    } = AddType.Text;

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
        get;
        set {
            if (value == field) { return; }
            field = value;
            InvalidateItemOrder();
        }
    } = true;

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
    public override bool ControlMustPressedForZoomWithWheel => true;

    [DefaultValue("")]
    public string FilterText {
        get;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    public new bool Focused => base.Focused || btnPlus.Focused || btnMinus.Focused || btnUp.Focused || btnDown.Focused || btnEdit.Focused;
    public int ItemCount => _item.Count;

    [DefaultValue(false)]
    public bool ItemEditAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            DoMouseMovement(-1, -1);
        }
    }

    public ReadOnlyCollection<AbstractListItem> Items {
        get {
            DoItemOrder();
            return _item.AsReadOnly();
        }
    }

    [DefaultValue(false)]
    public bool MoveAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            DoMouseMovement(-1, -1);
        }
    }

    [DefaultValue(false)]
    public bool RemoveAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            DoMouseMovement(-1, -1);
        }
    }

    public Renderer_Abstract Renderer => Renderer_Abstract.Default;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<AbstractListItem> Suggestions { get; } = [];

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    protected override bool ShowSliderX => false;
    protected override int SmallChangeY => 10;

    #endregion

    #region Indexers

    public AbstractListItem? this[string @internal] {
        get {
            try {
                return _item.GetByKey(@internal);
            } catch {
                Develop.AbortAppIfStackOverflow();
                return this[@internal];
            }
        }
    }

    public AbstractListItem? this[int no] {
        get {
            try {
                return no < 0 || no > _item.Count ? null : _item[no];
            } catch {
                Develop.AbortAppIfStackOverflow();
                return this[no];
            }
        }
    }

    #endregion

    #region Methods

    public AbstractListItem? Add_Text() {
        var val = InputBoxComboStyle.Show("Bitte geben sie einen Wert ein:", Suggestions, true);

        return string.IsNullOrEmpty(val) ? null : (AbstractListItem)ItemOf(val);
    }

    public AbstractListItem? Add_TextBySuggestion() {
        if (Suggestions.Count == 0) {
            Forms.MessageBox.Show("Keine (weiteren) Werte vorhanden.", ImageCode.Information, "OK");
            return null;
        }

        var rück = InputBoxListBoxStyle.Show("Bitte wählen sie einen Wert:", Suggestions, CheckBehavior.SingleSelection, null, AddType.None);

        return rück is not { Count: not 0 } ? null : Suggestions.GetByKey(rück[0]);
    }

    public Size CalculateColumnAndSize(Renderer_Abstract renderer) {
        var (biggestItemX, _, heightAdded, orienation) = _item.CanvasItemData(_itemDesign);
        if (orienation == Orientation.Waagerecht) { return ComputeAllItemPositions(new Size(300, 300), biggestItemX, heightAdded, orienation, renderer); }
        BreakAfterItems = CalculateColumnCount(biggestItemX, heightAdded, orienation);
        return ComputeAllItemPositions(new Size(1, 30), biggestItemX, heightAdded, orienation, renderer);
    }

    public void Check(IEnumerable<string> toCheck, bool uncheckOther) {
        if (uncheckOther) {
            if (!_checked.IsDifferentTo(toCheck)) { return; }

            ValidateCheckStates([.. toCheck], toCheck.FirstOrDefault());
        } else {
            // Sammle nur die Items, die noch nicht in der checked-Liste sind
            var newItemsToCheck = toCheck.Where(name => !IsChecked(name)).ToList();
            if (newItemsToCheck.Count == 0) { return; } // Nichts zu tun
            // Erstelle eine neue Liste mit kombiniertem Inhalt
            ValidateCheckStates([.. _checked, .. newItemsToCheck], newItemsToCheck.FirstOrDefault());
        }
    }

    public void Check(AbstractListItem ali) => Check(ali.KeyName);

    public void Check(string name) {
        if (IsChecked(name)) { return; }

        List<string> l = [.. _checked, name];

        ValidateCheckStates(l, name);
    }

    public new void Focus() {
        if (Focused) { return; }
        base.Focus();
    }

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

            case ListBoxAppearance.ButtonList:
                _itemDesign = Design.Button;
                _controlDesign = Design.GroupBox;
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
        Invalidate_MaxBounds();
    }

    public void ItemAddRange(List<AbstractListItem>? items) {
        if (items is not { Count: not 0 }) { return; }

        foreach (var thisIt in items) {
            if (_item.GetByKey(thisIt.KeyName) is AbstractListItem it) {
                Remove(it);
            }

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
        lock (_itemLock) {
            _item.Clear();
        }
        InvalidateItemOrder();
        ValidateCheckStates(null, string.Empty);
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

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

    public void Remove(List<AbstractListItem> items) {
        foreach (var thisItem in items) {
            if (!_item.Contains(thisItem)) { return; }

            RemoveAndUnRegister(thisItem);
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
        if (AutoSort) { return; }

        lock (_itemLock) {
            (_item[index1], _item[index2]) = (_item[index2], _item[index1]);
        }

        InvalidateItemOrder();
        DoItemOrder();
        ValidateCheckStates(_item.ToListOfString(), string.Empty);
    }

    public void UnCheck(AbstractListItem ali) => UnCheck(ali.KeyName);

    public void UnCheck(string name) {
        if (!IsChecked(name)) { return; }

        List<string> l = [.. _checked];
        l.Remove(name);

        ValidateCheckStates(l, string.Empty);
    }

    public void UncheckAll() => ValidateCheckStates([], string.Empty);

    internal void AddAndCheck(AbstractListItem? ali) {
        if (ali == null) { return; }

        if (_item.GetByKey(ali.KeyName) != null) { return; }

        var tmp = _checkBehavior;
        _checkBehavior = CheckBehavior.MultiSelection;

        ItemAdd(ali);
        //_itemx.Remove(ali.KeyName);

        //_itemx.Add(ali);
        _checkBehavior = tmp;
        Check(ali);
    }

    internal Size ComputeAllItemPositions(Size drawAreaControl, int biggestItemX, int heightAdded, Orientation senkrechtAllowed, Renderer_Abstract renderer) {
        try {
            if (Math.Abs(_lastCheckedMaxSize.Width - drawAreaControl.Width) > 0.1 || Math.Abs(_lastCheckedMaxSize.Height - drawAreaControl.Height) > 0.1) {
                _lastCheckedMaxSize = drawAreaControl;
                _maxNeededItemSize = Size.Empty;
            }

            if (!_maxNeededItemSize.IsEmpty) { return _maxNeededItemSize; }

            if (_item.Count == 0) {
                _maxNeededItemSize = Size.Empty;
                return Size.Empty;
            }

            if (_itemDesign == Design.Undefiniert) { GetDesigns(); }
            _item.PreComputeSize(_itemDesign);

            if (BreakAfterItems < 1) { senkrechtAllowed = Orientation.Waagerecht; }

            #region colWidth

            int colWidth;
            int colHeight;
            switch (_appearance) {
                case ListBoxAppearance.Gallery:
                    colWidth = 200;
                    colHeight = 200;
                    break;

                case ListBoxAppearance.FileSystem:
                    colWidth = 110;
                    colHeight = 110;
                    break;

                case ListBoxAppearance.ButtonList:
                    colWidth = 64;
                    colHeight = 80;
                    break;

                default:
                    // u.a. Autofilter
                    if (BreakAfterItems < 1) {
                        colWidth = drawAreaControl.Width;
                    } else {
                        var colCount = _item.Count / BreakAfterItems;
                        var r = _item.Count % colCount;
                        if (r != 0) { colCount++; }
                        colWidth = drawAreaControl.Width < 5 ? biggestItemX : drawAreaControl.Width / colCount;
                    }
                    colHeight = colWidth;
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
                if (thisItem != null && thisItem.Visible) {
                    var cx = 0;
                    var cy = 0;
                    var wi = colWidth;
                    itenc++;

                    var isCaption = thisItem is TextListItem { IsCaption: true };

                    if (senkrechtAllowed == Orientation.Waagerecht) {
                        if (isCaption) { wi = drawAreaControl.Width; }
                    }
                    var he = thisItem.HeightInControl(_appearance, colHeight, _itemDesign);

                    if (previtem != null) {
                        if (senkrechtAllowed == Orientation.Waagerecht) {
                            if (previtem.CanvasPosition.Right + colWidth > drawAreaControl.Width || isCaption) {
                                cx = 0;
                                cy = previtem.CanvasPosition.Bottom;
                            } else {
                                cx = previtem.CanvasPosition.Right;
                                cy = previtem.CanvasPosition.Top;
                            }
                        } else {
                            if (itenc % BreakAfterItems == 0) {
                                cx = previtem.CanvasPosition.Right;
                                cy = 0;
                            } else {
                                cx = previtem.CanvasPosition.Left;
                                cy = previtem.CanvasPosition.Bottom;
                            }
                        }
                    }
                    thisItem.CanvasPosition = new Rectangle(cx, cy, wi, he);
                    maxX = Math.Max(thisItem.CanvasPosition.Right, maxX);
                    maxy = Math.Max(thisItem.CanvasPosition.Bottom, maxy);
                    previtem = thisItem;
                }
            }

            _maxNeededItemSize = new Size(maxX, maxy);
            return _maxNeededItemSize;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ComputeAllItemPositions(drawAreaControl, biggestItemX, heightAdded, senkrechtAllowed, renderer);
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
            var it = Suggestions.GetByKey(thisString);
            if (it == null && IO.FileExists(thisString)) {
                if (thisString.FileType() == FileFormat.Image) {
                    it = ItemOf(thisString, thisString, thisString.FileNameWithoutSuffix(), string.Empty);
                } else {
                    it = ItemOf(thisString.FileNameWithSuffix(), thisString, QuickImage.Get(thisString.FileType(), 48));
                }
            }
            it ??= ItemOf(thisString);

            AddAndRegister(it);
        }

        InvalidateItemOrder();
    }

    protected override RectangleF CalculateCanvasMaxBounds() {
        var areaControl = AvailableControlPaintArea;
        var (biggestItemX, _, heightAdded, senkrechtAllowed) = _item.CanvasItemData(_itemDesign);
        var s = ComputeAllItemPositions(new Size(areaControl.Width, areaControl.Height), biggestItemX, heightAdded, senkrechtAllowed, Renderer);

        if (AddAllowed != AddType.None) { return new RectangleF(0, 0, s.Width, s.Height + 33); }
        return new RectangleF(0, 0, s.Width, s.Height);
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
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);
        var checkboxDesign = CheckboxDesign();

        var controlState = state;
        controlState &= ~(States.Standard_MouseOver | States.Standard_MousePressed | States.Standard_HasFocus);

        var visControPaintArea = AvailableControlPaintArea;
        var (biggestItemX, _, heightAdded, senkrechtAllowed) = _item.CanvasItemData(_itemDesign);
        ComputeAllItemPositions(new Size(visControPaintArea.Width, visControPaintArea.Height), biggestItemX, heightAdded, senkrechtAllowed, Renderer);

        gr.ScaleTransform(1, 1);
        Skin.Draw_Back(gr, _controlDesign, controlState, visControPaintArea, this, true);

        DoItemOrder();

        if (CheckBehavior == CheckBehavior.AllSelected) {
            _item.DrawItems(gr, visControPaintArea, _mouseOverItem, OffsetX, OffsetY, FilterText, controlState, _controlDesign, _itemDesign, checkboxDesign, null, Zoom);
        } else {
            _item.DrawItems(gr, visControPaintArea, _mouseOverItem, OffsetX, OffsetY, FilterText, controlState, _controlDesign, _itemDesign, checkboxDesign, _checked, Zoom);
        }

        if (_controlDesign == Design.ListBox) {
            Skin.Draw_Border(gr, _controlDesign, controlState, visControPaintArea);
        }
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        var clientPos = PointToClient(Cursor.Position);
        if (!ClientRectangle.Contains(clientPos)) {
            DoMouseMovement(-1, -1);
        }
    }

    protected override void OnMouseMove(CanvasMouseEventArgs e) {
        base.OnMouseMove(e);
        DoMouseMovement(e.ControlX, e.ControlY);
    }

    protected override void OnMouseUp(CanvasMouseEventArgs e) {
        base.OnMouseUp(e);
        if (!Enabled) { return; }
        var nd = _item.ElementAtPosition(e.ControlX, e.ControlY, Zoom, OffsetX, OffsetY);
        if (nd is { Enabled: false }) { return; }
        switch (e.Button) {
            case MouseButtons.Left:
                if (nd != null) {
                    if (_appearance is ListBoxAppearance.Listbox or
                                      ListBoxAppearance.Listbox_Boxes or
                                      ListBoxAppearance.Autofilter or
                                      ListBoxAppearance.Gallery or
                                      ListBoxAppearance.FileSystem or
                                      ListBoxAppearance.ButtonList) {
                        if (nd.IsClickable()) {
                            if (CheckBehavior != CheckBehavior.AllSelected) {
                                ChangeCheck(nd);
                            }
                        }
                    }

                    // Erst Item Clicked. Zb. geht dann das Kontextmenu zu.
                    OnItemClicked(new AbstractListItemEventArgs(nd));
                    nd.OnLeftClickExecute();
                }
                break;

            case MouseButtons.Right:
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, _item.ElementAtPosition(e.ControlX, e.ControlY, Zoom, OffsetX, OffsetY), e.ToMouseEventArgs());
                break;
        }
    }

    protected override void OnOffsetYChanged() {
        base.OnOffsetYChanged();

        if (IsDisposed) { return; }
        _mouseOverItem = null; // Damit die Buttons neu berechnet werden.

        var p = PointToClient(Cursor.Position);

        DoMouseMovement(p.X, p.Y);
        Invalidate();
    }

    protected override void OnParentEnabledChanged(System.EventArgs e) {
        if (IsDisposed) { return; }
        DoMouseMovement(-1, -1);
        base.OnParentEnabledChanged(e);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        DoMouseMovement(-1, -1);
        base.OnVisibleChanged(e);
    }

    private void AddAndRegister(AbstractListItem item) {
        lock (_itemLock) { _item.Add(item); }

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
                OnUpDownClicked();
                return;
            }
            ln = z;
        }
    }

    private void btnEdit_Click(object sender, System.EventArgs e) {
        if (ItemEditAllowed && _mouseOverItem is ReadableListItem { Item: IEditable ie }) {
            ie.Edit();
        }
    }

    private void btnMinus_Click(object sender, System.EventArgs e) {
        if (_mouseOverItem == null) { return; }

        //if (_checkBehaviorx == CheckBehavior.AlwaysSingleSelection && Item.Count < 2) { return; }
        if (CheckboxDesign() != Design.Undefiniert) { return; }

        var tmp = _mouseOverItem;

        var p = PointToClient(Cursor.Position);

        UnCheck(tmp);

        if (_checkBehavior != CheckBehavior.AllSelected) {
            // Z.B. die Sktipt-Liste.
            // Items können gewählt werden, aber auch gelöscht
            Remove(tmp);
        }

        OnRemoveClicked(new AbstractListItemEventArgs(tmp));

        DoMouseMovement(p.X, p.Y);
    }

    private void btnPlus_Click(object sender, System.EventArgs e) {
        OnAddClicked();

        AbstractListItem? toAdd = null;
        var mayBeNew = false;

        switch (AddAllowed) {
            case AddType.UserDef:
                toAdd = AddMethod?.Invoke();
                mayBeNew = true;
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
                Develop.DebugPrint(AddAllowed);
                break;
        }

        if (toAdd is { } ali) {
            AddAndCheck(ali);

            if (mayBeNew && ItemEditAllowed && ali is ReadableListItem { Item: IEditable ie }) {
                ie.Edit();
            }

            OnItemAddedByClick(new AbstractListItemEventArgs(ali));
        }

        DoMouseMovement(-1, -1);
    }

    private void btnUp_Click(object sender, System.EventArgs e) {
        AbstractListItem? ln = null;
        DoItemOrder();
        foreach (var thisItem in _item) {
            if (thisItem == _mouseOverItem) {
                if (ln == null) { return; }// Befehl verwerfen...
                Swap(_item.IndexOf(ln), _item.IndexOf(thisItem));
                OnUpDownClicked();
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
        if (!AutoSort || _sorted) { return; }
        lock (_itemLock) {
            _item.Sort();
        }
        _sorted = true;
    }

    private void DoMouseMovement(int controlX, int controlY) {
        if (IsDisposed || Parent == null) { return; }

        var isInForm = true;
        var nd = _item.ElementAtPosition(controlX, controlY, Zoom, OffsetX, OffsetY);
        if (!Enabled || Parent is not { Enabled: true } || !Visible) {
            nd = null;
            isInForm = false;
        }

        QuickInfo = nd?.QuickInfo ?? string.Empty;

        if (AddAllowed != AddType.None && isInForm && IsMaxYOffset) {
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
            var cp = _mouseOverItem.ControlPosition(Zoom, OffsetX, OffsetY);
            var right = cp.Right;
            var p16 = 16.CanvasToControl(Zoom);

            #region down-Button

            //btnUp.Enabled = Item[0].KeyName != nr[0];
            //btnDown.Enabled = Item[Item.Count - 1].KeyName != nr[0];

            if (MoveAllowed && !AutoSort && _item.Count > 1) {
                btnDown.Width = p16;
                btnDown.Height = p16;
                right -= p16;
                btnDown.Top = cp.Top;
                btnDown.Left = right;
                btnDown.Visible = true;
                btnDown.Enabled = _item[_item.Count - 1] != _mouseOverItem;
                btnDown.BringToFront();
            } else {
                btnDown.Visible = false;
            }

            #endregion

            #region Up-Button

            if (MoveAllowed && !AutoSort && _item.Count > 1) {
                btnUp.Width = p16;
                btnUp.Height = p16;
                right -= p16;
                btnUp.Top = cp.Top;
                btnUp.Left = right;
                btnUp.Visible = true;
                btnUp.Enabled = _item[0] != _mouseOverItem;
                btnUp.BringToFront();
            } else {
                btnUp.Visible = false;
            }

            #endregion

            #region Löschen-Button

            var removeok = RemoveAllowed;

            if (CheckboxDesign() != Design.Undefiniert) { removeok = false; }
            if (CheckBehavior == CheckBehavior.MultiSelection) { removeok = false; }
            if (!_mouseOverItem.IsClickable()) { removeok = false; }

            if (removeok) {
                btnMinus.Width = p16;
                btnMinus.Height = p16;
                right -= p16;
                btnMinus.Top = cp.Top;
                btnMinus.Left = right;
                btnMinus.Visible = true;
                btnMinus.Enabled = true;
                btnMinus.BringToFront();
            } else {
                btnMinus.Visible = false;
            }

            #endregion

            #region Edit-Button

            var editok = false;

            if (ItemEditAllowed && _mouseOverItem is ReadableListItem rli) {
                if (rli.Item is IEditable) { editok = true; }
                if (rli.Item is ISimpleEditor) { editok = true; }
            }

            if (editok) {
                btnEdit.Width = p16;
                btnEdit.Height = p16;
                right -= p16;
                btnEdit.Top = cp.Top;
                btnEdit.Left = right;
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

        DoQuickInfo();
        Refresh();
    }

    private void InvalidateItemOrder() {
        _maxNeededItemSize = Size.Empty;
        _sorted = false;
    }

    private bool IsChecked(AbstractListItem thisItem) => IsChecked(thisItem.KeyName);

    private bool IsChecked(string name) => _checked.Contains(name);

    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) => Invalidate();

    private void OnAddClicked() => AddClicked?.Invoke(this, System.EventArgs.Empty);

    private void OnItemAddedByClick(AbstractListItemEventArgs e) => ItemAddedByClick?.Invoke(this, e);

    private void OnItemCheckedChanged() => ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnItemClicked(AbstractListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    private void OnRemoveClicked(AbstractListItemEventArgs e) => RemoveClicked?.Invoke(this, e);

    private void OnUpDownClicked() => UpDownClicked?.Invoke(this, System.EventArgs.Empty);

    private void RemoveAndUnRegister(AbstractListItem item) {
        item.CompareKeyChanged -= Item_CompareKeyChangedChanged;
        item.PropertyChanged -= Item_PropertyChanged;
        lock (_itemLock) {
            _item.Remove(item);
        }
        _checked.Remove(item.KeyName);
        InvalidateItemOrder();
    }

    //private void OnItemDoubleClick(AbstractListItemEventArgs e) => ItemDoubleClick?.Invoke(this, e);

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
            var it = _item.GetByKey(thisit) ?? Suggestions.GetByKey(thisit) ?? ItemOf(thisit);

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