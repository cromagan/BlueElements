// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueControls.Renderer;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

/// <summary>
/// Eine modernisierte ListBox-Komponente zur Darstellung und Verwaltung von AbstractListItems.
/// </summary>
[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(ItemClicked))]
public sealed partial class ListBox : ZoomPad, IContextMenu, ITranslateable {

    #region Fields

    private readonly List<AbstractListItem> _item = [];
    private readonly object _itemLock = new();
    private ListBoxAppearance _appearance = ListBoxAppearance.Listbox;
    private CheckBehavior _checkBehavior = CheckBehavior.SingleSelection;
    private List<AbstractListItem> _checked = [];
    private Design _controlDesign = Design.Undefined;
    private SizeF _lastCheckedMaxSize = Size.Empty;
    private Size _maxNeededItemSize = Size.Empty;
    private AbstractListItem? _mouseOverItem;
    private bool _sorted;

    #endregion

    #region Constructors

    public ListBox() : base() {
        InitializeComponent();
        ItemDesign = Design.Undefined;
        HotItemForClick = null;
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

    public event EventHandler<AbstractListItemEventArgs>? ItemAddedByClick;

    public event EventHandler? ItemCheckedChanged;

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    public event EventHandler<AbstractListItemEventArgs>? RemoveClicked;

    public event EventHandler? UpDownClicked;

    #endregion

    #region Properties

    public static Renderer_Abstract Renderer => Renderer_Abstract.Default;

    [DefaultValue(AddType.Text)]
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
            if (value == _appearance && ItemDesign != Design.Undefined) { return; }
            _appearance = value;
            GetDesigns();
            InvalidateItemOrder();
            Invalidate();
        }
    }

    [DefaultValue(true)]
    public bool AutoSort {
        get;
        set {
            if (field == value) { return; }
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
            ValidateCheckStates(_checked.ToListOfString(), string.Empty);
        }
    }

    public ReadOnlyCollection<string> Checked => _checked.ToListOfString().AsReadOnly();
    public ReadOnlyCollection<AbstractListItem> CheckedItems => _checked.AsReadOnly();

    /// <summary>
    /// Machnmal ist die Listbox teil eines anderen Controls.
    /// Z.B. Combobox. Dann soll jedes ListItem die gleichen Einträge
    /// wie die Combobox haben. Hier kann das Control bestimmt werden,
    /// von dem es erben soll.
    /// </summary>
    [DefaultValue(null)]
    public IContextMenu? ContextMenuConnectedControl { get; set; }

    [DefaultValue(true)]
    public bool ContextMenuDefault { get; set; } = true;

    public override bool ControlMustPressedForZoomWithWheel => true;

    [DefaultValue(null)]
    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems { get; set; }

    [DefaultValue("")]
    public string FilterText {
        get;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    public int ItemCount => _item.Count;
    public Design ItemDesign { get; private set; }

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

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<AbstractListItem> Suggestions { get; } = [];

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    /// <summary>
    /// Das HotItem, das an ContextMenuEventArgs.HotItem übergeben wird,
    /// wenn ein ListItem per Linksklick ausgelöst wird.
    /// Wird z.B. von FloatingInputBoxListBoxStyle gesetzt, um Kontextinformationen
    /// an die LeftClickExecute-Delegate der Items weiterzugeben.
    /// </summary>
    internal object? HotItemForClick { get; set; }

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
                return (no < 0 || no >= _item.Count) ? null : _item[no];
            } catch {
                Develop.AbortAppIfStackOverflow();
                return this[no];
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Fügt ein neues Text-Item über einen Dialog hinzu.
    /// </summary>
    public AbstractListItem? Add_Text() {
        var val = InputBoxComboStyle.Show("Bitte geben sie einen Wert ein:", FormatHolder_Text.Instance, Suggestions, true);
        return string.IsNullOrEmpty(val) ? null : (AbstractListItem)ItemOf(val);
    }

    /// <summary>
    /// Fügt ein Item basierend auf Vorschlägen hinzu.
    /// </summary>
    public AbstractListItem? Add_TextBySuggestion() {
        if (Suggestions.Count == 0) {
            QuickNote.Show(NoteSymbols.Warning, "Keine Werte vorhanden");
            return null;
        }
        var rück = InputBoxListBoxStyle.Show("Bitte wählen sie einen Wert:", Suggestions, CheckBehavior.SingleSelection, null, AddType.None);
        return rück is { Count: > 0 } ? rück[0] : null;
    }

    public Size CalculateColumnAndSize(Renderer_Abstract renderer) {
        var (biggestItemX, _, heightAdded, orientation) = _item.CanvasItemData(ItemDesign);
        if (orientation == Orientation.Waagerecht) { return ComputeAllItemPositions(new Size(300, 300), biggestItemX, heightAdded, orientation, renderer); }
        BreakAfterItems = CalculateColumnCount(biggestItemX, heightAdded, orientation);
        return ComputeAllItemPositions(new Size(1, 30), biggestItemX, heightAdded, orientation, renderer);
    }

    /// <summary>
    /// Markiert die angegebenen Schlüssel als ausgewählt.
    /// </summary>
    public void Check(IEnumerable<string> toCheck, bool uncheckOther) {
        if (uncheckOther) {
            if (!_checked.ToListOfString().IsDifferentTo(toCheck)) { return; }
            ValidateCheckStates([.. toCheck], toCheck.FirstOrDefault() ?? string.Empty);
        } else {
            // Sammle nur die Items, die noch nicht in der checked-Liste sind
            var newItems = toCheck.Where(name => !IsChecked(name)).ToList();
            if (newItems.Count == 0) { return; }
            // Erstelle eine neue Liste mit kombiniertem Inhalt
            ValidateCheckStates([.. _checked.ToListOfString(), .. newItems], newItems.FirstOrDefault() ?? string.Empty);
        }
    }

    public void Check(AbstractListItem ali) => Check(ali.KeyName);

    public void Check(string name) {
        if (IsChecked(name)) { return; }
        ValidateCheckStates([.. _checked.ToListOfString(), name], name);
    }

    public List<AbstractListItem>? GetContextMenuItems(object? hotItem) => null;

    /// <summary>
    /// Aktualisiert die Design-Zuweisung basierend auf der Appearance.
    /// </summary>
    public void GetDesigns() {
        _controlDesign = (Design)_appearance;
        switch (_appearance) {
            case ListBoxAppearance.Autofilter:
                ItemDesign = Design.Item_AutoFilter;
                break;

            case ListBoxAppearance.DropdownSelectbox:
                ItemDesign = Design.Item_DropdownMenu;
                break;

            case ListBoxAppearance.Gallery:
            case ListBoxAppearance.FileSystem:
            case ListBoxAppearance.Listbox_Boxes:
            case ListBoxAppearance.Listbox:
                ItemDesign = Design.Item_ListBox;
                _controlDesign = Design.ListBox;
                break;

            case ListBoxAppearance.KontextMenu:
                ItemDesign = Design.Item_ContextMenu;
                break;

            case ListBoxAppearance.ComboBox_Textbox:
                ItemDesign = Design.ComboBox_TextBox;
                break;

            case ListBoxAppearance.ButtonList:
                ItemDesign = Design.Button;
                _controlDesign = Design.GroupBox;
                break;

            default:
                Develop.DebugError("Unbekanntes Design: " + _appearance);
                break;
        }
    }

    /// <summary>
    /// Fügt ein einzelnes Item hinzu.
    /// </summary>
    public void ItemAdd(AbstractListItem? item) {
        if (item is null) { Develop.DebugError("Item ist null"); return; }
        if (_item.Contains(item)) { Develop.DebugError("Bereits vorhanden!"); return; }
        if (this[item.KeyName] is not null) { Develop.DebugPrint($"Name bereits vorhanden: {item.KeyName}"); return; }
        if (string.IsNullOrEmpty(item.KeyName)) { throw Develop.DebugError("Item ohne Namen!"); }

        AddAndRegister(item);
        InvalidateItemOrder();
        ValidateCheckStates(_checked.ToListOfString(), item.KeyName);
    }

    /// <summary>
    /// Fügt eine Liste von Items hinzu und ersetzt bestehende mit gleichem Key.
    /// </summary>
    public void ItemAddRange(List<AbstractListItem>? items) {
        if (items is not { Count: > 0 }) { return; }
        foreach (var thisItem in items) {
            if (_item.GetByKey(thisItem.KeyName) is AbstractListItem existing) {
                Remove(existing);
            }
            AddAndRegister(thisItem);
        }
        InvalidateItemOrder();
        ValidateCheckStates(_checked.ToListOfString(), items[0].KeyName);
    }

    public void ItemAddRange(List<string>? list) {
        if (list is not { Count: > 0 }) { return; }
        foreach (var s in list.Where(s => !string.IsNullOrEmpty(s) && this[s] is null)) {
            AddAndRegister(ItemOf(s, s));
        }
        InvalidateItemOrder();
        ValidateCheckStates(_checked.ToListOfString(), list[0]);
    }

    public void ItemClear() {
        if (_item.Count == 0) { return; }
        _item.ForEach(it => it.CompareKeyChanged -= Item_CompareKeyChanged);
        _checked.Clear();
        lock (_itemLock) { _item.Clear(); }
        InvalidateItemOrder();
        ValidateCheckStates(null, string.Empty);
    }

    public void Remove(string keyName) {
        if (string.IsNullOrEmpty(keyName)) { return; }
        _item.Where(i => i.KeyName.Equals(keyName, StringComparison.OrdinalIgnoreCase))
             .ToList().ForEach(RemoveAndUnRegister);
        ValidateCheckStates(_checked.ToListOfString(), string.Empty);
    }

    public void Remove(List<AbstractListItem> items) {
        items.Where(i => _item.Contains(i)).ToList().ForEach(RemoveAndUnRegister);
        ValidateCheckStates(_checked.ToListOfString(), string.Empty);
    }

    public void Remove(AbstractListItem? item) {
        if (item is null || !_item.Contains(item)) { return; }
        RemoveAndUnRegister(item);
        ValidateCheckStates(_checked.ToListOfString(), string.Empty);
    }

    public void Swap(int index1, int index2) {
        if (index1 == index2 || AutoSort) { return; }
        lock (_itemLock) { (_item[index1], _item[index2]) = (_item[index2], _item[index1]); }
        InvalidateItemOrder();
        DoItemOrder();
        ValidateCheckStates(_item.ToListOfString(), string.Empty);
    }

    public void UnCheck(AbstractListItem ali) => UnCheck(ali.KeyName);

    public void UnCheck(string name) {
        if (!IsChecked(name)) { return; }
        var l = _checked.ToListOfString();
        l.Remove(name);
        ValidateCheckStates(l, string.Empty);
    }

    public void UncheckAll() => ValidateCheckStates([], string.Empty);

    /// <summary>
    /// Aktualisiert die Liste: Entfernt veraltete Einträge, fügt neue hinzu und behält bestehende mit identischer Referenz bei.
    /// </summary>
    public void UpdateList(IEnumerable<IReadableTextWithKey> updateItems) {
        if (IsDisposed) {
            ItemClear();
            return;
        }

        var updateDict = updateItems.DistinctBy(u => u.KeyName, StringComparer.Ordinal).ToDictionary(u => u.KeyName, u => u, StringComparer.Ordinal);

        // 1. Ermitteln & Löschen: Alles, was nicht mehr da ist oder ersetzt werden muss
        var toRemove = _item.OfType<ReadableListItem>()
                            .Where(rli => !updateDict.ContainsKey(rli.KeyName) ||
                                          !ReferenceEquals(rli.Item, updateDict[rli.KeyName]))
                            .ToList();

        // 3. Ausführen
        var wasChecked = Checked;

        foreach (var item in toRemove) {
            Remove(item);
        }

        // 2. Ermitteln: Was muss hinzugefügt werden? (Nur das, was jetzt nicht mehr im _item existiert)
        // Da wir toRemove bereits gelöscht haben, reicht ein einfacher Check gegen den aktuellen Zustand
        var toAdd = updateItems.Where(u => this[u.KeyName] == null).ToList();

        foreach (var updateItem in toAdd) {
            ItemAdd(ItemOf(updateItem));
        }

        Check(wasChecked, true);
    }

    internal void AddAndCheck(AbstractListItem? ali) {
        if (ali is null || _item.GetByKey(ali.KeyName) is not null) { return; }
        var tmp = _checkBehavior;
        _checkBehavior = CheckBehavior.MultiSelection;
        ItemAdd(ali);
        _checkBehavior = tmp;
        Check(ali);
    }

    internal Size ComputeAllItemPositions(Size drawAreaControl, int biggestItemX, int heightAdded, Orientation orientation, Renderer_Abstract renderer) {
        try {
            if (Math.Abs(_lastCheckedMaxSize.Width - drawAreaControl.Width) > 0.1 || Math.Abs(_lastCheckedMaxSize.Height - drawAreaControl.Height) > 0.1) {
                _lastCheckedMaxSize = drawAreaControl;
                _maxNeededItemSize = Size.Empty;
            }

            if (!_maxNeededItemSize.IsEmpty) { return _maxNeededItemSize; }
            if (_item.Count == 0) { return _maxNeededItemSize = Size.Empty; }
            if (ItemDesign == Design.Undefined) { GetDesigns(); }

            _item.PreComputeSize(ItemDesign);
            var layoutOrientation = BreakAfterItems < 1 ? Orientation.Waagerecht : orientation;

            var (colWidth, colHeight) = GetColumnDimensions(drawAreaControl, biggestItemX);

            var maxX = int.MinValue;
            var maxY = int.MinValue;
            AbstractListItem? prev = null;
            var index = -1;
            DoItemOrder();

            foreach (var item in _item.Where(i => i is { Visible: true })) {
                index++;
                var isCaption = item is TextListItem { IsCaption: true };
                var wi = (layoutOrientation == Orientation.Waagerecht && isCaption) ? drawAreaControl.Width : colWidth;
                var he = item.HeightInControl(_appearance, colHeight, ItemDesign);

                var (cx, cy) = CalculateItemPosition(item, prev, index, layoutOrientation, drawAreaControl, colWidth);

                item.CanvasPosition = new Rectangle(cx, cy, wi, he);
                maxX = Math.Max(item.CanvasPosition.Right, maxX);
                maxY = Math.Max(item.CanvasPosition.Bottom, maxY);
                prev = item;
            }

            return _maxNeededItemSize = new Size(maxX, maxY);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ComputeAllItemPositions(drawAreaControl, biggestItemX, heightAdded, orientation, renderer);
        }
    }

    internal void Item_CompareKeyChanged(object? sender, System.EventArgs e) => InvalidateItemOrder();

    internal void SetValuesTo(List<string> values) {
        var ist = _item.ToListOfString();
        ist.Except(values).ToList().ForEach(Remove);

        foreach (var s in values.Except(ist)) {
            var it = Suggestions.GetByKey(s) ?? CreateFileItem(s) ?? ItemOf(s);
            AddAndRegister(it);
        }
        InvalidateItemOrder();
    }

    protected override RectangleF CalculateCanvasMaxBounds() {
        var areaControl = AvailableControlPaintArea;
        var (biggestX, _, heightAdded, orientation) = _item.CanvasItemData(ItemDesign);
        var s = ComputeAllItemPositions(new Size(areaControl.Width, areaControl.Height), biggestX, heightAdded, orientation, Renderer);
        return new RectangleF(0, 0, s.Width, s.Height + (AddAllowed != AddType.None ? 33 : 0));
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
        var controlState = state & ~(States.Standard_MouseOver | States.Standard_MousePressed | States.Standard_HasFocus);
        var controPaintArea = AvailableControlPaintArea;

        if (_maxNeededItemSize.IsEmpty) {
            var (biggestX, _, heightAdded, orientation) = _item.CanvasItemData(ItemDesign);
            ComputeAllItemPositions(new Size(controPaintArea.Width, controPaintArea.Height), biggestX, heightAdded, orientation, Renderer);
        }

        Skin.Draw_Back(gr, _controlDesign, controlState, controPaintArea, this, true);
        DoItemOrder();

        var checkedKeys = CheckBehavior == CheckBehavior.AllSelected ? null : _checked.ToListOfString();
        _item.DrawItems(gr, controPaintArea, _mouseOverItem, OffsetX, OffsetY, FilterText, controlState, _controlDesign, ItemDesign, checkboxDesign, checkedKeys, Zoom);

        if (_controlDesign == Design.ListBox) { Skin.Draw_Border(gr, _controlDesign, controlState, controPaintArea); }
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        var clientPos = PointToClient(System.Windows.Forms.Cursor.Position);
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

        if (e.Button == System.Windows.Forms.MouseButtons.Left && nd is not null) {
            if (IsAppearanceClickable() && nd.IsClickable() && CheckBehavior != CheckBehavior.AllSelected) { ChangeCheck(nd); }
            OnItemClicked(new AbstractListItemEventArgs(nd));
            nd.LeftClickExecute?.Invoke(this, new ContextMenuEventArgs(nd, HotItemForClick));
        } else if (e.Button == System.Windows.Forms.MouseButtons.Right) {
            ((IContextMenu)this).ContextMenuShow(nd);
        }
    }

    protected override void OnOffsetYChanged() {
        base.OnOffsetYChanged();
        if (IsDisposed) { return; }
        _mouseOverItem = null;
        var p = PointToClient(System.Windows.Forms.Cursor.Position);
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
        item.CompareKeyChanged += Item_CompareKeyChanged;
        item.PropertyChanged += Item_PropertyChanged;
    }

    private void btnDown_Click(object sender, System.EventArgs e) {
        DoItemOrder();
        for (var z = _item.Count - 2; z >= 0; z--) {
            if (_item[z] == _mouseOverItem) {
                Swap(z, z + 1);
                OnUpDownClicked();
                return;
            }
        }
    }

    private void btnEdit_Click(object sender, System.EventArgs e) {
        if (ItemEditAllowed && _mouseOverItem is ReadableListItem { Item: IEditable ie }) {
            ie.Edit();
        }
    }

    private void btnMinus_Click(object sender, System.EventArgs e) {
        if (_mouseOverItem is null || CheckboxDesign() != Design.Undefined || _mouseOverItem.RemoveLocked) { return; }
        var tmp = _mouseOverItem;
        var p = PointToClient(System.Windows.Forms.Cursor.Position);
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
        var (toAdd, mayBeNew) = AddAllowed switch {
            AddType.UserDef => (AddMethod?.Invoke(), true),
            AddType.Text => (Add_Text(), false),
            AddType.OnlySuggests => (Add_TextBySuggestion(), false),
            _ => (null, false)
        };

        if (toAdd is { } ali) {
            AddAndCheck(ali);
            if (mayBeNew && ItemEditAllowed && ali is ReadableListItem { Item: IEditable ie }) { ie.Edit(); }
            OnItemAddedByClick(new AbstractListItemEventArgs(ali));
        }
        DoMouseMovement(-1, -1);
    }

    private void btnUp_Click(object sender, System.EventArgs e) {
        DoItemOrder();
        for (var i = 1; i < _item.Count; i++) {
            if (_item[i] == _mouseOverItem) {
                Swap(i, i - 1);
                OnUpDownClicked();
                return;
            }
        }
    }

    private int CalculateColumnCount(int biggestItemWidth, int allItemsHeight, Orientation orientation) {
        if (orientation != Orientation.Senkrecht) { Develop.DebugError("Nur 'senkrecht' erlaubt mehrere Spalten"); }
        if (_item.Count < 12) { return -1; }
        var avgHeight = allItemsHeight / _item.Count;
        for (var testSp = 10; testSp >= 1; testSp--) {
            var colc = _item.Count / testSp;
            var rest = _item.Count % colc;
            var ok = !(rest > 0 && rest < colc / 2) &&
                colc is >= 5 and <= 20 &&
                colc * avgHeight is >= 150 and <= 600 && testSp * biggestItemWidth <= 600 &&
                (colc * (float)avgHeight / (testSp * (float)biggestItemWidth)) >= 0.5;
            if (ok) { return colc; }
        }
        return -1;
    }

    private (int Width, int Height) CalculateDefaultDimensions(Size area, int biggestX) {
        if (BreakAfterItems < 1) { return (area.Width, area.Width); }
        var colCount = Math.Max(1, (int)Math.Ceiling((double)_item.Count / BreakAfterItems));
        var w = area.Width < 5 ? biggestX : area.Width / colCount;
        return (w, w);
    }

    private (int X, int Y) CalculateItemPosition(AbstractListItem current, AbstractListItem? prev, int index, Orientation layout, Size area, int colWidth) {
        if (prev is null) { return (0, 0); }
        var isCaption = current is TextListItem { IsCaption: true };

        if (layout == Orientation.Waagerecht) {
            if (prev.CanvasPosition.Right + colWidth > area.Width || isCaption) { return (0, prev.CanvasPosition.Bottom); }
            return (prev.CanvasPosition.Right, prev.CanvasPosition.Top);
        }

        if (index % BreakAfterItems == 0) { return (prev.CanvasPosition.Right, 0); }
        return (prev.CanvasPosition.Left, prev.CanvasPosition.Bottom);
    }

    private void ChangeCheck(AbstractListItem ne) { if (IsChecked(ne)) { UnCheck(ne); } else { Check(ne); } }

    private Design CheckboxDesign() => (_appearance == ListBoxAppearance.Listbox_Boxes && _checkBehavior != CheckBehavior.AllSelected)
        ? (_checkBehavior == CheckBehavior.SingleSelection ? Design.OptionButton_TextStyle : Design.CheckBox_TextStyle)
        : Design.Undefined;

    private AbstractListItem? CreateFileItem(string path) {
        if (!IO.FileExists(path)) { return null; }
        return path.FileType() == FileFormat.Image
            ? ItemOf(path, path, path.FileNameWithoutSuffix(), string.Empty)
            : ItemOf(path.FileNameWithSuffix(), path, QuickImage.Get(path.FileType(), 48));
    }

    private void DoItemOrder() {
        if (!AutoSort || _sorted) { return; }
        lock (_itemLock) { _item.Sort(); }
        _sorted = true;
    }

    private void DoMouseMovement(int controlX, int controlY) {
        if (IsDisposed || Parent is null) { return; }
        var isInForm = controlX >= 0 && controlY >= 0;
        var nd = _item.ElementAtPosition(controlX, controlY, Zoom, OffsetX, OffsetY);
        if (!Enabled || !Parent.Enabled || !Visible) { nd = null; isInForm = false; }

        QuickInfo = nd?.QuickInfo ?? string.Empty;
        UpdatePlusButton(isInForm);

        if (nd == _mouseOverItem) { return; }
        _mouseOverItem = nd;

        if (_mouseOverItem is not null) {
            UpdateItemButtons();
        } else {
            HideAllButtons();
        }

        DoQuickInfo();
        Refresh();
    }

    private (int Width, int Height) GetColumnDimensions(Size area, int biggestX) => _appearance switch {
        ListBoxAppearance.Gallery => (200, 200),
        ListBoxAppearance.FileSystem => (110, 110),
        ListBoxAppearance.ButtonList => (64, 80),
        _ => CalculateDefaultDimensions(area, biggestX)
    };

    private void HideAllButtons() => btnMinus.Visible = btnUp.Visible = btnDown.Visible = btnEdit.Visible = false;

    private void InvalidateItemOrder() { _maxNeededItemSize = Size.Empty; _sorted = false; Invalidate_MaxBounds(); }

    private bool IsAppearanceClickable() => _appearance is ListBoxAppearance.Listbox or ListBoxAppearance.Listbox_Boxes or ListBoxAppearance.Autofilter or ListBoxAppearance.Gallery or ListBoxAppearance.FileSystem or ListBoxAppearance.ButtonList;

    private bool IsChecked(AbstractListItem item) => IsChecked(item.KeyName);

    private bool IsChecked(string name) => _checked.Any(x => x.KeyName == name);

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) => Invalidate();

    private void OnAddClicked() => AddClicked?.Invoke(this, System.EventArgs.Empty);

    private void OnItemAddedByClick(AbstractListItemEventArgs e) => ItemAddedByClick?.Invoke(this, e);

    private void OnItemCheckedChanged() => ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnItemClicked(AbstractListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    private void OnRemoveClicked(AbstractListItemEventArgs e) => RemoveClicked?.Invoke(this, e);

    private void OnUpDownClicked() => UpDownClicked?.Invoke(this, System.EventArgs.Empty);

    private void RemoveAndUnRegister(AbstractListItem item) {
        item.CompareKeyChanged -= Item_CompareKeyChanged;
        item.PropertyChanged -= Item_PropertyChanged;
        lock (_itemLock) { _item.Remove(item); }
        _checked.RemoveAll(x => x.KeyName == item.KeyName);
        InvalidateItemOrder();
    }

    private void UpdateButton(System.Windows.Forms.Control btn, int top, ref int right, int size, bool enabled) {
        btn.Width = btn.Height = size;
        right -= size;
        btn.Top = top;
        btn.Left = right;
        btn.Visible = true;
        btn.Enabled = enabled;
        btn.BringToFront();
    }

    private void UpdateItemButtons() {
        if (_mouseOverItem is not { } mhi) { return; }
        var cp = mhi.ControlPosition(Zoom, OffsetX, OffsetY);
        var right = cp.Right;
        var p16 = 16.CanvasToControl(Zoom);

        if (MoveAllowed && !AutoSort && _item.Count > 1) {
            UpdateButton(btnDown, cp.Top, ref right, p16, _item[^1] != mhi);
            UpdateButton(btnUp, cp.Top, ref right, p16, _item[0] != mhi);
        } else { btnDown.Visible = btnUp.Visible = false; }

        var removeOk = RemoveAllowed && CheckboxDesign() == Design.Undefined && mhi.IsClickable() && !mhi.RemoveLocked;
        if (removeOk) { UpdateButton(btnMinus, cp.Top, ref right, p16, true); } else { btnMinus.Visible = false; }

        var editOk = ItemEditAllowed && mhi is ReadableListItem { Item: IEditable or ISimpleEditor };
        if (editOk) { UpdateButton(btnEdit, cp.Top, ref right, p16, true); } else { btnEdit.Visible = false; }
    }

    private void UpdatePlusButton(bool isInForm) {
        if (AddAllowed != AddType.None && isInForm && IsMaxYOffset) {
            btnPlus.Left = 2;
            btnPlus.Top = Height - 2 - btnPlus.Height;
            btnPlus.Visible = btnPlus.Enabled = true;
            btnPlus.BringToFront();
        } else { btnPlus.Visible = false; }
    }

    private void ValidateCheckStates(List<string>? newChecked, string lastAdded) {
        newChecked ??= [];
        switch (_checkBehavior) {
            case CheckBehavior.NoSelection:
                newChecked.Clear();
                break;

            case CheckBehavior.SingleSelection when newChecked.Count > 1:
                var keep = string.IsNullOrEmpty(lastAdded) ? newChecked[0] : lastAdded;
                newChecked.Clear();
                newChecked.Add(keep);
                break;
        }

        var newList = newChecked.Select(s => _item.GetByKey(s) ?? Suggestions.GetByKey(s) ?? ItemOf(s))
                                .Where(it => it.IsClickable()).ToList();

        if (newList.ToListOfString().IsDifferentTo(_checked.ToListOfString())) {
            if (_checkBehavior == CheckBehavior.AllSelected) { SetValuesTo(newChecked); }
            _checked = newList;
            OnItemCheckedChanged();
        }
        Invalidate();
    }

    #endregion
}