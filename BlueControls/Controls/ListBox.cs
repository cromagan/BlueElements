// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;

namespace BlueControls.Controls;

/// <summary>
/// Eine modernisierte ListBox-Komponente zur Darstellung und Verwaltung von ListItems.
/// Hostet ein <see cref="ListBoxCore"/>-Control für die Kern-Logik und stellt die
/// zusätzlichen Steuerelemente (Hinzu, Löschen, Verschieben, Bearbeiten) bereit.
/// </summary>
[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(ItemClicked))]
public partial class ListBox : GenericControl, IContextMenu, ITranslateable, IBackgroundNone {

    #region Fields

    private bool _addAreaUpdateQueued;

    /// <summary>
    /// <c>true</c>, während OnGotFocus den Fokus absichtlich ans innere lstBox
    /// weiterreicht. Das dabei ausgelöste LostFocus der äußeren ListBox wird
    /// in <see cref="OnLostFocus" /> unterdrückt, damit das Control nicht
    /// vorzeitig geschlossen wird.
    /// </summary>
    private bool _focusingChild;

    #endregion

    #region Constructors

    public ListBox() {
        InitializeComponent();
        lstBox.AddAreaVisible = AddAllowed != AddType.None;
    }

    #endregion

    #region Events

    /// <summary>
    /// Wird beim Klick auf den Hinzufügen-Button ausgelöst, sobald der Text
    /// feststeht. Der Handler kann <see cref="AddItemEventArgs.Cancel"/> setzen,
    /// um die automatische Item-Erstellung zu unterbinden und selbst zu reagieren.
    /// Wird nicht gecancelt, wird das Item automatisch hinzugefügt.
    /// </summary>
    public event EventHandler<AddItemEventArgs>? AddClicked;

    public event EventHandler<ListItemEventArgs>? ItemAddedByClick;

    public event EventHandler? ItemCheckedChanged;

    public event EventHandler<ListItemEventArgs>? ItemClicked;

    public event EventHandler<ListItemEventArgs>? RemoveClicked;

    public event EventHandler<SwapEventArgs>? UpDownClicked;

    #endregion

    #region Properties

    [DefaultValue(AddType.Text)]
    public AddType AddAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            if (lstBox is { IsDisposed: false }) {
                lstBox.AddAreaVisible = value != AddType.None;
            }
            UpdateAddArea();
        }
    } = AddType.Text;

    [DefaultValue(ListBoxAppearance.Listbox)]
    public ListBoxAppearance Appearance {
        get => lstBox.Appearance;
        set => lstBox.Appearance = value;
    }

    [DefaultValue(true)]
    public bool AutoSort {
        get => lstBox.AutoSort;
        set => lstBox.AutoSort = value;
    }

    [DefaultValue(CheckBehavior.SingleSelection)]
    public CheckBehavior CheckBehavior {
        get => lstBox.CheckBehavior;
        set => lstBox.CheckBehavior = value;
    }

    public ReadOnlyCollection<string> Checked => lstBox.Checked;
    public ReadOnlyCollection<ListItem> CheckedItems => lstBox.CheckedItems;

    [DefaultValue(null)]
    public IContextMenu? ContextMenuConnectedControl {
        get => lstBox.ContextMenuConnectedControl;
        set => lstBox.ContextMenuConnectedControl = value;
    }

    [DefaultValue(true)]
    public bool ContextMenuDefault {
        get => lstBox.ContextMenuDefault;
        set => lstBox.ContextMenuDefault = value;
    }

    [DefaultValue(null)]
    public ReadOnlyCollection<ListItem>? CustomContextMenuItems {
        get => lstBox.CustomContextMenuItems;
        set => lstBox.CustomContextMenuItems = value;
    }

    [DefaultValue("")]
    public string FilterText {
        get => lstBox.FilterText;
        set => lstBox.FilterText = value;
    }

    /// <summary>
    /// Das HotItem, das an ContextMenuEventArgs.HotItem übergeben wird.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? HotItemForClick {
        get => lstBox.HotItemForClick;
        set => lstBox.HotItemForClick = value;
    }

    public int ItemCount => lstBox.ItemCount;
    public Design ItemDesign => lstBox.ItemDesign;

    [DefaultValue(false)]
    public bool ItemEditAllowed {
        get => lstBox.ItemEditAllowed;
        set => lstBox.ItemEditAllowed = value;
    }

    /// <inheritdoc cref="ListBoxCore.ItemPadding"/>
    [DefaultValue(0)]
    public int ItemPadding {
        get => lstBox.ItemPadding;
        set => lstBox.ItemPadding = value;
    }

    public ReadOnlyCollection<ListItem> Items => lstBox.Items;

    [DefaultValue(false)]
    public bool MoveAllowed {
        get => lstBox.MoveAllowed;
        set => lstBox.MoveAllowed = value;
    }

    [DefaultValue(false)]
    public bool RemoveAllowed {
        get => lstBox.RemoveAllowed;
        set => lstBox.RemoveAllowed = value;
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<ListItem> Suggestions => lstBox.Suggestions;

    [DefaultValue(true)]
    public bool Translate {
        get => lstBox.Translate;
        set => lstBox.Translate = value;
    }

    [DefaultValue(1f)]
    public float Zoom {
        get => lstBox.Zoom;
        set => lstBox.Zoom = value;
    }

    #endregion

    #region Indexers

    public ListItem? this[string @internal] => lstBox[@internal];
    public ListItem? this[int no] => lstBox[no];

    #endregion

    #region Methods

    public void AddAndCheck(ListItem? ali) => lstBox.AddAndCheck(ali);

    public Size CalculateColumnAndSize(Renderer.Renderer renderer) => lstBox.CalculateColumnAndSize(renderer);

    public void Check(IEnumerable<string> toCheck, bool uncheckOther) => lstBox.Check(toCheck, uncheckOther);

    public void Check(ListItem ali) => lstBox.Check(ali);

    public void Check(string name) => lstBox.Check(name);

    public List<ListItem>? GetContextMenuItems(object? hotItem) => lstBox.GetContextMenuItems(hotItem);

    public void ItemAdd(ListItem? item) {
        lstBox.ItemAdd(item);
        ScheduleAddAreaUpdate();
    }

    public void ItemAddRange(List<ListItem>? items) {
        lstBox.ItemAddRange(items);
        ScheduleAddAreaUpdate();
    }

    public void ItemAddRange(List<string>? list) {
        lstBox.ItemAddRange(list);
        ScheduleAddAreaUpdate();
    }

    public void ItemClear() {
        lstBox.ItemClear();
        ScheduleAddAreaUpdate();
    }

    public void Remove(string keyName) {
        lstBox.Remove(keyName);
        ScheduleAddAreaUpdate();
    }

    public void Remove(List<ListItem> items) {
        lstBox.Remove(items);
        ScheduleAddAreaUpdate();
    }

    public void Remove(ListItem? item) {
        lstBox.Remove(item);
        ScheduleAddAreaUpdate();
    }

    /// <summary>
    /// Setzt die Scroll-Position auf den Listenanfang zurück.
    /// </summary>
    public void ResetScroll() => lstBox.OffsetY = 0;

    public void Swap(int index1, int index2) {
        lstBox.Swap(index1, index2);
        ScheduleAddAreaUpdate();
    }

    public void UnCheck(ListItem ali) => lstBox.UnCheck(ali);

    public void UnCheck(string name) => lstBox.UnCheck(name);

    public void UncheckAll() => lstBox.UncheckAll();

    public void UpdateList(IEnumerable<IReadableTextWithKey> updateItems) => lstBox.UpdateList(updateItems);

    protected virtual void OnAddClicked(AddItemEventArgs e) => AddClicked?.Invoke(this, e);

    /// <summary>
    /// Verdrahtet das LostFocus jedes Kind-Controls (inkl. lstBox). Der Fokus
    /// liegt dabei immer auf einem Kind, nie auf der äußeren ListBox (siehe
    /// <see cref="OnGotFocus" />) — Item-Klicks verändern den Fokus nicht und
    /// lösen kein vorzeitiges LostFocus aus. Erst wenn ein Kind den Fokus an
    /// die Außenwelt abgibt, feuert <see cref="OnLostFocus" /> wirklich.
    /// </summary>
    protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is { } c) { c.LostFocus += Child_LostFocus; }
    }

    protected override void OnControlRemoved(System.Windows.Forms.ControlEventArgs e) {
        base.OnControlRemoved(e);
        if (e.Control is { } c) { c.LostFocus -= Child_LostFocus; }
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        UpdateAddArea();
    }

    /// <summary>
    /// Reicht den Fokus an das innere lstBox weiter. So liegt der Fokus immer
    /// auf dem Kind; Item-Klicks verändern den Fokus nicht und lösen kein
    /// vorzeitiges LostFocus der äußeren ListBox aus. Das beim Weiterreichen
    /// entstehende LostFocus wird über <see cref="_focusingChild" /> in
    /// <see cref="OnLostFocus" /> unterdrückt.
    /// </summary>
    protected override void OnGotFocus(System.EventArgs e) {
        base.OnGotFocus(e);
        if (IsDisposed || lstBox.Focused) { return; }

        _focusingChild = true;
        try { lstBox.Focus(); } finally { _focusingChild = false; }
    }

    protected virtual void OnItemAddedByClick(ListItemEventArgs e) => ItemAddedByClick?.Invoke(this, e);

    /// <summary>
    /// Löst das <see cref="ItemCheckedChanged"/>-Event aus. Abgeleitete Klassen
    /// können dies überschreiben, um auf Prüfzustandsänderungen zu reagieren.
    /// </summary>
    protected virtual void OnItemCheckedChanged(System.EventArgs e) => ItemCheckedChanged?.Invoke(this, e);

    protected virtual void OnItemClicked(ListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    /// <summary>
    /// Feuert das LostFocus-Event erst, wenn der Fokus das Control
    /// einschließlich aller Kind-Controls wirklich verlassen hat.
    /// Wandert der Fokus nur zwischen den Kindern, löst das KEIN
    /// LostFocus aus — wichtig für Inline-Dropdowns, die sonst beim
    /// ersten Item-Klick vorzeitig geschlossen würden.
    /// </summary>
    protected override void OnLostFocus(System.EventArgs e) {
        if (_focusingChild || ContainsFocus) { return; }
        base.OnLostFocus(e);
    }

    protected virtual void OnRemoveClicked(ListItemEventArgs e) => RemoveClicked?.Invoke(this, e);

    protected override void OnResize(System.EventArgs e) {
        base.OnResize(e);
        UpdateAddArea();
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        UpdateAddArea();
    }

    private void AddAndRaise(ListItem? ali) {
        if (ali is not { } item) { return; }
        lstBox.AddAndCheck(item);
        if (ItemEditAllowed && item is ReadableListItem { Item: IEditable ie }) { ie.Edit(); }
        OnItemAddedByClick(new ListItemEventArgs(item));
    }

    private void AddInput_EnterKey(object? sender, System.EventArgs e) {
        if (btnPlus.Enabled) { btnPlus_Click(sender, e); }
    }

    private void AddInput_TextChanged(object? sender, System.EventArgs e) => btnPlus.Enabled = IsAddTextValid(CurrentAddText());

    private List<ListItem> AvailableSuggestions() =>
        Suggestions.Where(s => lstBox[s.KeyName] is null).ToList();

    private void btnDown_Click(object sender, System.EventArgs e) {
        for (var z = lstBox.ItemCount - 2; z >= 0; z--) {
            if (lstBox[z] is not { } current) { continue; }
            if (current == lstBox.MouseOverItem) {
                if (lstBox[z + 1] is not { } next) { return; }
                if (current.MoveLocked || next.MoveLocked) { return; }
                lstBox.Swap(z, z + 1);
                OnUpDownClicked(z, z + 1);
                return;
            }
        }
    }

    private void btnEdit_Click(object sender, System.EventArgs e) {
        if (ItemEditAllowed && lstBox.MouseOverItem is ReadableListItem { Item: IEditable ie }) {
            ie.Edit();
        }
    }

    private void btnMinus_Click(object sender, System.EventArgs e) {
        var mhi = lstBox.MouseOverItem;
        if (mhi is null || mhi.RemoveLocked) { return; }
        var tmp = mhi;
        var p = PointToClient(System.Windows.Forms.Cursor.Position);
        lstBox.UnCheck(tmp);
        if (lstBox.CheckBehavior != CheckBehavior.AllSelected) {
            // Z.B. die Sktipt-Liste.
            // Items können gewählt werden, aber auch gelöscht
            lstBox.Remove(tmp);
        }
        OnRemoveClicked(new ListItemEventArgs(tmp));
        lstBox.DoMouseMovement(p.X, p.Y);
    }

    private void btnPlus_Click(object? sender, System.EventArgs e) {
        if (AddAllowed == AddType.Suggestions) {
            HandleSuggestionsClick();
            return;
        }

        var text = CurrentAddText();
        if (string.IsNullOrEmpty(text) || !IsAddTextValid(text)) { return; }

        var args = new AddItemEventArgs(text);
        OnAddClicked(args);
        if (args.Cancel) {
            ClearAddInput();
        } else {
            AddAndRaise(Suggestions.GetByKey(text) ?? ItemOf(text));
            ClearAddInput();
        }

        lstBox.DoMouseMovement(-1, -1);
        UpdateAddArea();
    }

    private void btnUp_Click(object sender, System.EventArgs e) {
        for (var i = 1; i < lstBox.ItemCount; i++) {
            if (lstBox[i] is not { } current) { continue; }
            if (current == lstBox.MouseOverItem) {
                if (lstBox[i - 1] is not { } prev) { return; }
                if (current.MoveLocked || prev.MoveLocked) { return; }
                lstBox.Swap(i, i - 1);
                OnUpDownClicked(i, i - 1);
                return;
            }
        }
    }

    private void CbxAdd_ItemAddedByClick(object? sender, ListItemEventArgs e) { }

    private void ClearAddInput() {
        txtAdd.Text = string.Empty;
        cbxAdd.Text = string.Empty;
    }

    private void Core_ButtonUpdate(object? sender, ButtonUpdateEventArgs e) {
        UpdateAddArea();
        if (!e.MouseOverChanged) { return; }
        if (lstBox.MouseOverItem is not null) {
            UpdateItemButtons();
        } else {
            HideAllButtons();
        }
    }

    private void Core_ItemCheckedChanged(object? sender, System.EventArgs e) => OnItemCheckedChanged(e);

    private void Core_ItemClicked(object? sender, ListItemEventArgs e) => OnItemClicked(e);

    private void Core_ItemLayoutChanged(object? sender, System.EventArgs e) {
        ScheduleAddAreaUpdate();
        // Bei Layout-Änderungen (insb. Zoom-Wechsel) müssen auch die Hover-Buttons
        // sofort nachpositioniert werden, da ihre Koordinaten direkt aus der
        // CanvasPosition des MouseOverItem abgeleitet sind.
        // ItemsCanvasBottom() erzwingt ggf. eine frische ComputeAllItemPositions-
        // Berechnung, damit UpdateItemButtons aktuelle Positionen vorfindet.
        lstBox.ItemsCanvasBottom();
        if (lstBox.MouseOverItem is not null) {
            UpdateItemButtons();
        } else {
            HideAllButtons();
        }
    }

    private string CurrentAddText() {
        if (cbxAdd.Visible) { return cbxAdd.Text; }
        if (txtAdd.Visible) { return txtAdd.Text; }
        return cbxAdd.Text.Length > 0 ? cbxAdd.Text : txtAdd.Text;
    }

    private void DropDownItemClicked(object? sender, ListItemEventArgs e) {
        var args = new AddItemEventArgs(e.Item.KeyName);
        OnAddClicked(args);
        if (!args.Cancel) {
            AddAndRaise(e.Item);
            ClearAddInput();
        }
        UpdateAddArea();
    }

    private void HandleSuggestionsClick() {
        var available = AvailableSuggestions();

        if (available.Count > 0) {
            var dropDown = FloatingInputBoxListBoxStyle.Show(
                available,
                CheckBehavior.NoSelection,
                null,
                this,
                Translate,
                ListBoxAppearance.DropdownSelectbox,
                Design.Item_ContextMenu,
                AutoSort);

            dropDown.ItemClicked += DropDownItemClicked;
            return;
        }

        // Keine Vorschläge vorhanden: AddClicked direkt feuern,
        // damit der Handler das Item selbst erstellen kann.
        var args = new AddItemEventArgs(string.Empty);
        OnAddClicked(args);
        lstBox.DoMouseMovement(-1, -1);
        UpdateAddArea();
    }

    private void HideAllButtons() => btnMinus.Visible = btnUp.Visible = btnDown.Visible = btnEdit.Visible = false;

    private bool IsAddTextValid(string text) {
        if (string.IsNullOrEmpty(text)) { return false; }
        if (lstBox[text] is not null) { return false; }
        if (AddAllowed.HasFlag(AddType.Suggestions) && !AddAllowed.HasFlag(AddType.Text)
            && Suggestions.GetByKey(text) is null && Suggestions.Count > 0) { return false; }
        return true;
    }

    private void Child_LostFocus(object? sender, System.EventArgs e) => OnLostFocus(e);

    private void OnUpDownClicked(int index1, int index2) => UpDownClicked?.Invoke(this, new SwapEventArgs(index1, index2));

    private void ScheduleAddAreaUpdate() {
        if (IsDisposed || _addAreaUpdateQueued) { return; }
        if (!IsHandleCreated) {
            UpdateAddArea();
            return;
        }
        _addAreaUpdateQueued = true;
        BeginInvoke(new Action(() => {
            _addAreaUpdateQueued = false;
            UpdateAddArea();
        }));
    }

    // lstBox liegt mit Dock=Fill hinter den Sibling-Buttons (btnPlus, txtAdd, ...).
    // Its OnMouseLeave hält die Maus für "noch innerhalb" und cleart den MouseOver
    // nicht. Beim Betreten eines Sibling-Buttons hier explizit zurücksetzen,
    // sonst bliebe das letzte Item optisch im MouseOver-Zustand.
    private void Sibling_MouseEnter(object? sender, System.EventArgs e) {
        if (lstBox is not { IsDisposed: false }) { return; }

        // Die Item-Aktions-Buttons (Minus, Hoch, Runter, Bearbeiten) liegen direkt
        // auf dem gehoverten Item. Würde hier MouseOverItem zurückgesetzt, blende
        // sich der Button sofort selbst aus (Maus auf Button -> Hover weg ->
        // Button weg -> Maus auf lstBox -> Hover an -> Button an -> ...) und
        // flackert. Sie gehören zur Hover-UI des Items und dürfen den Hover halten.
        if (ReferenceEquals(sender, btnMinus) || ReferenceEquals(sender, btnUp) ||
            ReferenceEquals(sender, btnDown) || ReferenceEquals(sender, btnEdit)) { return; }

        lstBox.DoMouseMovement(-1, -1);
    }

    private void SyncCbxAddSuggestions() {
        var available = AvailableSuggestions();
        if (cbxAdd.ItemCount == available.Count) {
            var same = true;
            var existing = cbxAdd.Items();
            for (var i = 0; i < available.Count; i++) {
                if (i >= existing.Count || !ReferenceEquals(existing[i], available[i])) { same = false; break; }
            }
            if (same) { return; }
        }

        var focus = cbxAdd.Focused;
        var oldText = cbxAdd.Text;
        cbxAdd.ItemClear();
        cbxAdd.ItemAddRange(available);
        cbxAdd.Text = oldText;
        if (focus) { cbxAdd.Focus(); }
    }

    private void UpdateAddArea() {
        if (IsDisposed || lstBox.IsDisposed) { return; }

        var show = AddAllowed != AddType.None && Visible && Enabled;
        if (!show) {
            btnPlus.Visible = false;
            btnPlus.Text = string.Empty;
            txtAdd.Visible = false;
            cbxAdd.Visible = false;
            return;
        }

        var itemsBottom = lstBox.ItemsCanvasBottom();
        var availableWidth = lstBox.AvailableControlPaintArea.Width;
        var p16 = 16.CanvasToControl(lstBox.Zoom);
        var btnSize = Math.Max(16, p16);

        var top = itemsBottom.CanvasToControl(lstBox.Zoom, lstBox.OffsetY);
        var padding = 2;
        var tbHeight = Math.Max(20, btnSize + 4);

        // Suggestions ohne TextEdit: vollflächige Schaltfläche ohne Texteingabe.
        if (AddAllowed == AddType.Suggestions) {
            txtAdd.Visible = false;
            cbxAdd.Visible = false;

            btnPlus.Text = "Hinzufügen";
            if (btnPlus.Height != tbHeight) { btnPlus.Height = tbHeight; }
            btnPlus.Top = top;
            btnPlus.Left = padding;
            btnPlus.Width = Math.Max(20, availableWidth - (padding * 2));
            btnPlus.Enabled = Suggestions.Count == 0 || AvailableSuggestions().Count > 0;
            btnPlus.Visible = true;
            btnPlus.BringToFront();
            return;
        }

        // TextEdit (mit oder ohne Suggestions): Text-/ComboBox + Button
        var useComboBox = Suggestions.Count > 0;
        if (useComboBox) { SyncCbxAddSuggestions(); }

        var input = useComboBox ? cbxAdd : txtAdd;
        var hidden = useComboBox ? txtAdd : cbxAdd;
        hidden.Visible = false;
        btnPlus.Text = string.Empty;

        input.Zoom = lstBox.Zoom;
        if (input.Height != tbHeight) { input.Height = tbHeight; }
        input.Top = top;
        input.Left = padding;
        input.Width = Math.Max(20, availableWidth - btnSize - (padding * 3));
        input.Visible = true;

        btnPlus.Width = btnSize;
        btnPlus.Height = btnSize;
        btnPlus.Top = top + ((tbHeight - btnSize) / 2);
        btnPlus.Left = availableWidth - btnSize - padding;
        btnPlus.Enabled = IsAddTextValid(CurrentAddText());
        btnPlus.Visible = true;
        btnPlus.BringToFront();
        input.BringToFront();
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
        var mhi = lstBox.MouseOverItem;
        if (mhi is not { } mh) { return; }
        var cp = mh.ControlPosition(lstBox.Zoom, lstBox.OffsetX, lstBox.OffsetY);
        var right = cp.Right;
        var p16 = 16.CanvasToControl(lstBox.Zoom);

        if (MoveAllowed && !lstBox.AutoSort && lstBox.ItemCount > 1 && mh.IsClickable() && !mh.MoveLocked) {
            var mouseIndex = -1;
            for (var i = 0; i < lstBox.ItemCount; i++) {
                if (ReferenceEquals(lstBox[i], mh)) { mouseIndex = i; break; }
            }
            var downEnabled = mouseIndex >= 0 && mouseIndex < lstBox.ItemCount - 1 && lstBox[mouseIndex + 1] is { } di && !di.MoveLocked;
            var upEnabled = mouseIndex > 0 && lstBox[mouseIndex - 1] is { } ui && !ui.MoveLocked;
            if (downEnabled) { UpdateButton(btnDown, cp.Top, ref right, p16, true); } else { btnDown.Visible = false; }
            if (upEnabled) { UpdateButton(btnUp, cp.Top, ref right, p16, true); } else { btnUp.Visible = false; }
        } else { btnDown.Visible = btnUp.Visible = false; }

        var removeOk = RemoveAllowed && lstBox.CheckboxDesign() == Design.Undefined && mh.IsClickable() && !mh.RemoveLocked;
        if (removeOk) { UpdateButton(btnMinus, cp.Top, ref right, p16, true); } else { btnMinus.Visible = false; }

        var editOk = ItemEditAllowed && mh is ReadableListItem { Item: IEditable or ISimpleEditor };
        if (editOk) { UpdateButton(btnEdit, cp.Top, ref right, p16, true); } else { btnEdit.Visible = false; }
    }

    #endregion
}