// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class FloatingInputBoxListBoxStyle : FloatingForm {

    #region Constructors

    private FloatingInputBoxListBoxStyle(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, AddType addAllowed, BlueControls.Controls.ListBox.dAddMethod? addMethod, bool moveAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems, object? hotItem) : base(connectedControl, (Design)controlDesign) {
        InitializeComponent();
        xpos -= Skin.PaddingSmal;
        ypos -= Skin.PaddingSmal;
        Generate_ListBox1(items, checkBehavior, check, steuerWi, addAllowed, addMethod, moveAllowed, translate, controlDesign, itemDesign, autosort, removeAllowed, customContextMenuItems);

        lstbx.HotItemForClick = hotItem;
        lstbx.UpDownClicked += ListBox1_UpDownClicked;
        lstbx.ItemAddedByClick += ListBox1_ItemAddedByClick;

        if (ConnectedControl is IContextMenu cm) {
            lstbx.ContextMenuConnectedControl = cm;
            if (cm.CustomContextMenuItems is not null) {
                lstbx.CustomContextMenuItems = cm.CustomContextMenuItems;
            }
        }

        Position_SetWindowIntoScreen(Generic.PointOnScreenNr(new Point(xpos, ypos)), xpos, ypos);
        OutsideClicked += (_, _) => OnCancel();
        Show();
    }

    #endregion

    #region Events

    public event EventHandler? Cancel;

    public event EventHandler<AbstractListItemEventArgs>? ItemAddedByClick;

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    public event EventHandler<AbstractListItemEventArgs>? ItemRemoved;

    public event EventHandler<SwapEventArgs>? UpDownClicked;

    #endregion

    #region Properties

    /// <summary>
    /// Liefert die aktuell im Dropdown enthaltenen Items in ihrer sichtbaren Reihenfolge.
    /// </summary>
    public ReadOnlyCollection<AbstractListItem> Items => lstbx.Items;

    #endregion

    #region Methods

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
                translate, controlDesign, itemDesign, autosort, false, AddType.None, null, false, null, null);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort) => new(items, checkBehavior, check, xpos, ypos, steuerWi, connectedControl, translate, controlDesign, itemDesign, autosort, false, AddType.None, null, false, null, null);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
            translate, controlDesign, itemDesign, autosort, removeAllowed, AddType.None, null, false, null, null);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, object? hotItem) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
            translate, controlDesign, itemDesign, autosort, removeAllowed, AddType.None, null, false, null, hotItem);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, AddType addAllowed, BlueControls.Controls.ListBox.dAddMethod? addMethod, bool moveAllowed) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
            translate, controlDesign, itemDesign, autosort, removeAllowed, addAllowed, addMethod, moveAllowed, null, null);

    public static FloatingInputBoxListBoxStyle ShowComboBoxDropDown(List<AbstractListItem> items, string check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, bool autosort, bool removeAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) => new(items, CheckBehavior.SingleSelection, [check], xpos, ypos, steuerWi, connectedControl, translate, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, autosort, removeAllowed, AddType.None, null, false, customContextMenuItems, null);

    public static FloatingInputBoxListBoxStyle ShowComboBoxDropDown(List<AbstractListItem> items, string check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, bool autosort, bool removeAllowed, AddType addAllowed, BlueControls.Controls.ListBox.dAddMethod? addMethod, bool moveAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) => new(items, CheckBehavior.SingleSelection, [check], xpos, ypos, steuerWi, connectedControl, translate, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, autosort, removeAllowed, addAllowed, addMethod, moveAllowed, customContextMenuItems, null);

    public void Generate_ListBox1(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int minWidth, AddType addNewAllowed, BlueControls.Controls.ListBox.dAddMethod? addMethod, bool moveAllowed, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) {
        var (biggestItemX, _, heightAdded, _) = items.CanvasItemData(itemDesign);
        if (addNewAllowed != AddType.None) { heightAdded += 26; }
        lstbx.Appearance = controlDesign;
        lstbx.Translate = translate;
        lstbx.AutoSort = autosort;
        lstbx.RemoveAllowed = removeAllowed;
        lstbx.AddAllowed = addNewAllowed;
        lstbx.AddMethod = addMethod;
        lstbx.MoveAllowed = moveAllowed;
        lstbx.CustomContextMenuItems = customContextMenuItems;

        AdjustFormSize(biggestItemX, heightAdded, minWidth);
        lstbx.CheckBehavior = CheckBehavior.MultiSelection;
        lstbx.ItemAddRange(items);
        if (check is not null) { lstbx.Check(check, true); }
        lstbx.CheckBehavior = checkBehavior;
    }

    public override void Refresh() {
        Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
        base.Refresh();
        OnPaint(null);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) { components?.Dispose(); }
        base.Dispose(disposing);
    }

    private void AdjustFormSize(int biggestItemX, int heightAdded, int minWidth = 16) {
        heightAdded++; // Um ja den Slider zu vermeiden!
        heightAdded = Math.Max(heightAdded, 16);
        biggestItemX = Math.Max(biggestItemX, 16);
        biggestItemX = Math.Max(biggestItemX, minWidth);
        var maxWi = (int)(Screen.PrimaryScreen.Bounds.Size.Width * 0.7);
        var maxHe = (int)(Screen.PrimaryScreen.Bounds.Size.Height * 0.7);
        if (biggestItemX > maxWi) { biggestItemX = maxWi; }
        if (heightAdded > maxHe) {
            heightAdded = maxHe;
            biggestItemX += 20;
        }
        Size = new Size(biggestItemX + (lstbx.Left * 2), heightAdded + (lstbx.Top * 2));
    }

    /// <summary>
    /// Berechnet die Größe des Fensters anhand der aktuellen Items neu.
    /// Die aktuelle Breite wird als Minimum beibehalten, damit das Fenster
    /// beim Löschen eines Items nicht schrumpft und beim Hinzufügen wächst.
    /// </summary>
    private void RecalcFormSize() {
        var (biggestItemX, _, heightAdded, _) = lstbx.Items.CanvasItemData(lstbx.ItemDesign);
        if (lstbx.AddAllowed != AddType.None) { heightAdded += 26; }
        AdjustFormSize(biggestItemX, heightAdded, Width - (lstbx.Left * 2));
    }

    private void ListBox1_ItemAddedByClick(object? sender, AbstractListItemEventArgs e) {
        RecalcFormSize();
        ItemAddedByClick?.Invoke(this, e);
    }

    private void ListBox1_ItemClicked(object sender, AbstractListItemEventArgs e) {
        // Selectet Chanched bringt nix, da es ja drum geht, ob eine Node angeklickt wurde.
        // Nur Listboxen können überhaupt erst Checked werden!
        // Ob sie Checked wird, ist egal!

        // Einen Klick auf Überschriften einfach ignorieren, zB. kontextmenü
        if (!e.Item.IsClickable()) { return; }

        if (lstbx.Appearance is not ListBoxAppearance.Listbox and not ListBoxAppearance.Listbox_Boxes and not ListBoxAppearance.Gallery and not ListBoxAppearance.FileSystem and not ListBoxAppearance.ButtonList) {
            var handler = ItemClicked;
            Close();
            handler?.Invoke(this, e);
        }
    }

    private void ListBox1_ItemRemoved(object sender, AbstractListItemEventArgs e) {
        if (lstbx.Items.Count == 0) { Close(); OnItemRemoved(e); return; }
        RecalcFormSize();
        OnItemRemoved(e);
    }

    private void ListBox1_UpDownClicked(object? sender, SwapEventArgs e) => UpDownClicked?.Invoke(this, e);

    private void OnCancel() => Cancel?.Invoke(this, System.EventArgs.Empty);

    private void OnItemRemoved(AbstractListItemEventArgs e) => ItemRemoved?.Invoke(this, e);

    #endregion
}