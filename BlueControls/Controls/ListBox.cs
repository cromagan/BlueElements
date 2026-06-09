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

    #region Constructors

    public ListBox() {
        InitializeComponent();
    }

    #endregion

    #region Delegates

    public delegate AbstractListItem? dAddMethod();

    #endregion

    #region Events

    /// <summary>
    /// Wir am Anfang des dirket nach dem Klicken ausgelöst. Damit können Items z.B. zurückgeschrieben werden.
    /// </summary>
    public event EventHandler? AddClicked;

    public event EventHandler<AbstractListItemEventArgs>? ItemAddedByClick;

    public event EventHandler? ItemCheckedChanged;

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    public event EventHandler<AbstractListItemEventArgs>? RemoveClicked;

    public event EventHandler? UpDownClicked;

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

    public void AddAndCheck(AbstractListItem? ali) => lstBox.AddAndCheck(ali);

    public Size CalculateColumnAndSize(Renderer_Abstract renderer) => lstBox.CalculateColumnAndSize(renderer);

    public void Check(IEnumerable<string> toCheck, bool uncheckOther) => lstBox.Check(toCheck, uncheckOther);

    public void Check(AbstractListItem ali) => lstBox.Check(ali);

    public void Check(string name) => lstBox.Check(name);

    public List<AbstractListItem>? GetContextMenuItems(object? hotItem) => lstBox.GetContextMenuItems(hotItem);

    public void ItemAdd(AbstractListItem? item) => lstBox.ItemAdd(item);

    public void ItemAddRange(List<AbstractListItem>? items) => lstBox.ItemAddRange(items);

    public void ItemAddRange(List<string>? list) => lstBox.ItemAddRange(list);

    public void ItemClear() => lstBox.ItemClear();

    public void Remove(string keyName) => lstBox.Remove(keyName);

    public void Remove(List<AbstractListItem> items) => lstBox.Remove(items);

    public void Remove(AbstractListItem? item) => lstBox.Remove(item);

    public void Swap(int index1, int index2) => lstBox.Swap(index1, index2);

    public void UnCheck(AbstractListItem ali) => lstBox.UnCheck(ali);

    public void UnCheck(string name) => lstBox.UnCheck(name);

    public void UncheckAll() => lstBox.UncheckAll();

    public void UpdateList(IEnumerable<IReadableTextWithKey> updateItems) => lstBox.UpdateList(updateItems);

    private void btnDown_Click(object sender, System.EventArgs e) {
        for (var z = lstBox.ItemCount - 2; z >= 0; z--) {
            if (lstBox[z] == lstBox.MouseOverItem) {
                lstBox.Swap(z, z + 1);
                OnUpDownClicked();
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

    private void btnPlus_Click(object sender, System.EventArgs e) {
        OnAddClicked();
        var (toAdd, mayBeNew) = AddAllowed switch {
            AddType.UserDef => (AddMethod?.Invoke(), true),
            AddType.Text => (Add_Text(), false),
            AddType.OnlySuggests => (Add_TextBySuggestion(), false),
            _ => (null, false)
        };

        if (toAdd is { } ali) {
            lstBox.AddAndCheck(ali);
            if (mayBeNew && ItemEditAllowed && ali is ReadableListItem { Item: IEditable ie }) { ie.Edit(); }
            OnItemAddedByClick(new AbstractListItemEventArgs(ali));
        }
        lstBox.DoMouseMovement(-1, -1);
    }

    private void btnUp_Click(object sender, System.EventArgs e) {
        for (var i = 1; i < lstBox.ItemCount; i++) {
            if (lstBox[i] == lstBox.MouseOverItem) {
                lstBox.Swap(i, i - 1);
                OnUpDownClicked();
                return;
            }
        }
    }

    private void HideAllButtons() => btnMinus.Visible = btnUp.Visible = btnDown.Visible = btnEdit.Visible = false;

    private void OnAddClicked() => AddClicked?.Invoke(this, System.EventArgs.Empty);

    private void OnCoreButtonUpdate(object? sender, ButtonUpdateEventArgs e) {
        UpdatePlusButton(e.IsInForm);
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

    private void OnUpDownClicked() => UpDownClicked?.Invoke(this, System.EventArgs.Empty);

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

        if (MoveAllowed && !lstBox.AutoSort && lstBox.ItemCount > 1) {
            UpdateButton(btnDown, cp.Top, ref right, p16, lstBox[lstBox.ItemCount - 1] != mh);
            UpdateButton(btnUp, cp.Top, ref right, p16, lstBox[0] != mh);
        } else { btnDown.Visible = btnUp.Visible = false; }

        var removeOk = RemoveAllowed && lstBox.CheckboxDesign() == Design.Undefined && mh.IsClickable() && !mh.RemoveLocked;
        if (removeOk) { UpdateButton(btnMinus, cp.Top, ref right, p16, true); } else { btnMinus.Visible = false; }

        var editOk = ItemEditAllowed && mh is ReadableListItem { Item: IEditable or ISimpleEditor };
        if (editOk) { UpdateButton(btnEdit, cp.Top, ref right, p16, true); } else { btnEdit.Visible = false; }
    }

    private void UpdatePlusButton(bool isInForm) {
        if (AddAllowed != AddType.None && isInForm && lstBox.IsMaxYOffset) {
            btnPlus.Left = 2;
            btnPlus.Top = Height - 2 - btnPlus.Height;
            btnPlus.Visible = btnPlus.Enabled = true;
            btnPlus.BringToFront();
        } else { btnPlus.Visible = false; }
    }

    #endregion
}