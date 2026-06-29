// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueControls.Renderer;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

/// <summary>
/// Eine modernisierte ListBox-Komponente zur Darstellung und Verwaltung von AbstractListItems.
/// Hostet ein <see cref="ListBoxCore"/>-Control für die Kern-Logik und stellt die
/// zusätzlichen Steuerelemente (Hinzu, Löschen, Verschieben, Bearbeiten) bereit.
/// </summary>
[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(ItemClicked))]
public sealed partial class ListBox : GenericControl, IContextMenu, ITranslateable, IBackgroundNone {

    #region Fields

    private bool _addAreaUpdateQueued;

    #endregion

    #region Constructors

    public ListBox() {
        InitializeComponent();
    }

    #endregion

    #region Delegates

    public delegate AbstractListItem? dAddMethod(string text);

    #endregion

    #region Events

    /// <summary>
    /// Wird direkt nach dem Klicken ausgelöst, sobald die Eingaben validiert sind
    /// und bevor das eigentliche Item erzeugt wird. Damit können z.B. Items zurückgeschrieben werden.
    /// </summary>
    public event EventHandler? AddClicked;

    public event EventHandler<AbstractListItemEventArgs>? ItemAddedByClick;

    public event EventHandler? ItemCheckedChanged;

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    public event EventHandler<AbstractListItemEventArgs>? RemoveClicked;

    public event EventHandler<SwapEventArgs>? UpDownClicked;

    #endregion

    #region Properties

    [DefaultValue(AddType.Text)]
    public AddType AddAllowed {
        get => lstBox.AddAllowed;
        set => lstBox.AddAllowed = value;
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public dAddMethod? AddMethod {
        get => lstBox.AddMethod;
        set => lstBox.AddMethod = value;
    }

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
    public ReadOnlyCollection<AbstractListItem> CheckedItems => lstBox.CheckedItems;

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
    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems {
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

    public ReadOnlyCollection<AbstractListItem> Items => lstBox.Items;

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
    public List<AbstractListItem> Suggestions => lstBox.Suggestions;

    [DefaultValue(true)]
    public bool Translate {
        get => lstBox.Translate;
        set => lstBox.Translate = value;
    }

    #endregion

    #region Indexers

    public AbstractListItem? this[string @internal] => lstBox[@internal];
    public AbstractListItem? this[int no] => lstBox[no];

    #endregion

    #region Methods

    public void AddAndCheck(AbstractListItem? ali) => lstBox.AddAndCheck(ali);

    public Size CalculateColumnAndSize(Renderer_Abstract renderer) => lstBox.CalculateColumnAndSize(renderer);

    public void Check(IEnumerable<string> toCheck, bool uncheckOther) => lstBox.Check(toCheck, uncheckOther);

    public void Check(AbstractListItem ali) => lstBox.Check(ali);

    public void Check(string name) => lstBox.Check(name);

    public List<AbstractListItem>? GetContextMenuItems(object? hotItem) => lstBox.GetContextMenuItems(hotItem);

    public void ItemAdd(AbstractListItem? item) {
        lstBox.ItemAdd(item);
        ScheduleAddAreaUpdate();
    }

    public void ItemAddRange(List<AbstractListItem>? items) {
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

    public void Remove(List<AbstractListItem> items) {
        lstBox.Remove(items);
        ScheduleAddAreaUpdate();
    }

    public void Remove(AbstractListItem? item) {
        lstBox.Remove(item);
        ScheduleAddAreaUpdate();
    }

    public void Swap(int index1, int index2) {
        lstBox.Swap(index1, index2);
        ScheduleAddAreaUpdate();
    }

    public void UnCheck(AbstractListItem ali) => lstBox.UnCheck(ali);

    public void UnCheck(string name) => lstBox.UnCheck(name);

    public void UncheckAll() => lstBox.UncheckAll();

    public void UpdateList(IEnumerable<IReadableTextWithKey> updateItems) => lstBox.UpdateList(updateItems);

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        UpdateAddArea();
    }

    protected override void OnResize(System.EventArgs e) {
        base.OnResize(e);
        UpdateAddArea();
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        UpdateAddArea();
    }

    private void AddAndRaise(string text) => AddAndRaise(AddMethod?.Invoke(text), autoEdit: true);

    private void AddAndRaise(AbstractListItem? ali, bool autoEdit) {
        if (ali is not { } item) { return; }
        lstBox.AddAndCheck(item);
        if (autoEdit && ItemEditAllowed && item is ReadableListItem { Item: IEditable ie }) { ie.Edit(); }
        OnItemAddedByClick(new AbstractListItemEventArgs(item));
    }

    private void AddInput_EnterKey(object? sender, System.EventArgs e) {
        if (btnPlus.Enabled) { btnPlus_Click(sender, e); }
    }

    private void AddInput_TextChanged(object? sender, System.EventArgs e) {
        btnPlus.Enabled = IsAddTextValid(CurrentAddText());
    }

    private List<AbstractListItem> AvailableSuggestions() =>
        Suggestions.Where(s => lstBox[s.KeyName] is null).ToList();

    private void btnDown_Click(object sender, System.EventArgs e) {
        for (var z = lstBox.ItemCount - 2; z >= 0; z--) {
            if (lstBox[z] == lstBox.MouseOverItem) {
                if (lstBox[z].MoveLocked || lstBox[z + 1].MoveLocked) { return; }
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
        OnRemoveClicked(new AbstractListItemEventArgs(tmp));
        lstBox.DoMouseMovement(p.X, p.Y);
    }

    private void btnPlus_Click(object? sender, System.EventArgs e) {
        if (AddAllowed == AddType.UserDef_NoText) {
            HandleUserDefNoTextClick();
            return;
        }

        if (AddAllowed == AddType.OnlySuggests && AvailableSuggestions().Count == 0) {
            QuickNote.Show(NoteSymbols.Warning, "Keine Vorschläge mehr", btnPlus);
            return;
        }

        var text = CurrentAddText();
        if (string.IsNullOrEmpty(text) || !IsAddTextValid(text)) { return; }

        OnAddClicked();
        var (toAdd, autoEdit) = AddAllowed switch {
            AddType.UserDef => (AddMethod?.Invoke(text), true),
            AddType.Text => (ItemOf(text), false),
            AddType.OnlySuggests => (Suggestions.GetByKey(text), false),
            _ => (null, false)
        };

        AddAndRaise(toAdd, autoEdit);
        ClearAddInput();
        lstBox.DoMouseMovement(-1, -1);
        UpdateAddArea();
    }

    private void btnUp_Click(object sender, System.EventArgs e) {
        for (var i = 1; i < lstBox.ItemCount; i++) {
            if (lstBox[i] == lstBox.MouseOverItem) {
                if (lstBox[i].MoveLocked || lstBox[i - 1].MoveLocked) { return; }
                lstBox.Swap(i, i - 1);
                OnUpDownClicked(i, i - 1);
                return;
            }
        }
    }

    private void CbxAdd_ItemAddedByClick(object? sender, AbstractListItemEventArgs e) { }

    private void ClearAddInput() {
        txtAdd.Text = string.Empty;
        cbxAdd.Text = string.Empty;
    }

    private string CurrentAddText() {
        if (cbxAdd.Visible) { return cbxAdd.Text; }
        if (txtAdd.Visible) { return txtAdd.Text; }
        return cbxAdd.Text.Length > 0 ? cbxAdd.Text : txtAdd.Text;
    }

    private void DropDownItemClicked(object? sender, AbstractListItemEventArgs e) {
        AddAndRaise(e.Item.KeyName);
        UpdateAddArea();
    }

    private void HandleUserDefNoTextClick() {
        if (AddMethod is null) { return; }

        OnAddClicked();

        // Mit Suggestions: Auswahl in einem Floating-Menü, danach AddMethod
        // mit dem Schlüssel des gewählten Vorschlags aufrufen.
        if (Suggestions.Count > 0) {
            var available = AvailableSuggestions();
            if (available.Count == 0) {
                QuickNote.Show(NoteSymbols.Warning, "Keine Vorschläge mehr", btnPlus);
                return;
            }

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

        // Ohne Suggestions: AddMethod direkt (mit leerem Text) ausführen.
        AddAndRaise(string.Empty);
        UpdateAddArea();
    }

    private void HideAllButtons() => btnMinus.Visible = btnUp.Visible = btnDown.Visible = btnEdit.Visible = false;

    private bool IsAddTextValid(string text) {
        if (string.IsNullOrEmpty(text)) { return false; }
        if (lstBox[text] is not null) { return false; }
        if (AddAllowed == AddType.OnlySuggests && Suggestions.GetByKey(text) is null) { return false; }
        if (AddAllowed == AddType.UserDef && AddMethod is null) { return false; }
        return true;
    }

    private void OnAddClicked() => AddClicked?.Invoke(this, System.EventArgs.Empty);

    private void OnCoreButtonUpdate(object? sender, ButtonUpdateEventArgs e) {
        UpdateAddArea();
        if (!e.MouseOverChanged) { return; }
        if (lstBox.MouseOverItem is not null) {
            UpdateItemButtons();
        } else {
            HideAllButtons();
        }
    }

    private void OnCoreItemCheckedChanged(object? sender, System.EventArgs e) => ItemCheckedChanged?.Invoke(this, e);

    private void OnCoreItemClicked(object? sender, AbstractListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    private void OnItemAddedByClick(AbstractListItemEventArgs e) => ItemAddedByClick?.Invoke(this, e);

    private void OnRemoveClicked(AbstractListItemEventArgs e) => RemoveClicked?.Invoke(this, e);

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

        // Modus ohne Texteingabe: vollflächige Schaltfläche "Hinzufügen".
        if (AddAllowed == AddType.UserDef_NoText) {
            txtAdd.Visible = false;
            cbxAdd.Visible = false;

            btnPlus.Text = "Hinzufügen";
            if (btnPlus.Height != tbHeight) { btnPlus.Height = tbHeight; }
            btnPlus.Top = top;
            btnPlus.Left = padding;
            btnPlus.Width = Math.Max(20, availableWidth - (padding * 2));
            btnPlus.Enabled = AddMethod is not null;
            btnPlus.Visible = true;
            btnPlus.BringToFront();
            return;
        }

        var useComboBox = Suggestions.Count > 0;
        if (useComboBox) { SyncCbxAddSuggestions(); }

        var input = useComboBox ? cbxAdd : txtAdd;
        var hidden = useComboBox ? txtAdd : cbxAdd;
        hidden.Visible = false;
        btnPlus.Text = string.Empty;

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
            var downEnabled = mouseIndex >= 0 && mouseIndex < lstBox.ItemCount - 1 && !lstBox[mouseIndex + 1].MoveLocked;
            var upEnabled = mouseIndex > 0 && !lstBox[mouseIndex - 1].MoveLocked;
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