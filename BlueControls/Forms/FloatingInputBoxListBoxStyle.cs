// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class FloatingInputBoxListBoxStyle : FloatingForm {

    #region Constructors

    private FloatingInputBoxListBoxStyle(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, AddType addAllowed, bool moveAllowed, bool itemEditAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems, object? hotItem) : base(connectedControl, (Design)controlDesign) {
        InitializeComponent();

        // Mini-Toolbar: weniger Innenabstand als andere Menüs, damit die
        // Icons bündig mit der Zelle abschließen und das Fenster weniger
        // "protzig" wirkt. Das Position-Offset (Skin.PaddingSmal) entfällt
        // ebenfalls, damit die Toolbar exakt an der übergebenen Position
        // (Spaltenrand) erscheint.
        // Anchor wird auf Top|Left gesetzt, weil die lstbx sonst durch den
        // Wechsel von Location=(8,8) auf (2,2) ohne gleichzeitige Size-
        // Anpassung eine viel zu kleine Fläche erhält (Anker hält den
        // ursprünglichen Randabstand von 14 px bei). Die Größe wird in
        // Generate_ListBox1 explizit zugewiesen.
        if (controlDesign == ListBoxAppearance.MiniToolbar) {
            lstbx.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lstbx.Location = new Point(Skin.PaddingSmal, Skin.PaddingSmal);
        } else {
            xpos -= Skin.PaddingSmal;
            ypos -= Skin.PaddingSmal;
        }

        Generate_ListBox1(items, checkBehavior, check, steuerWi, addAllowed, moveAllowed, itemEditAllowed, translate, controlDesign, itemDesign, autosort, removeAllowed, customContextMenuItems);

        lstbx.HotItemForClick = hotItem;
        lstbx.UpDownClicked += ListBox1_UpDownClicked;
        lstbx.ItemAddedByClick += ListBox1_ItemAddedByClick;
        lstbx.AddClicked += ListBox1_AddClicked;

        if (ConnectedControl is IContextMenu cm) {
            lstbx.ContextMenuConnectedControl = cm;
            if (cm.CustomContextMenuItems is not null) {
                lstbx.CustomContextMenuItems = cm.CustomContextMenuItems;
            }
        }

        // Bildschirm-Erkennung. Bei Menüs, die in Cursor-Nähe geöffnet werden
        // (Kontextmenü: Cursor-Position vs. Menü-Position differieren nur um
        // die Padding-Verschiebung), die Cursor-Position verwenden. Verhindert,
        // dass das Menü auf den falschen Bildschirm springt, wenn der Cursor
        // nah an einer Bildschirmkante steht. Bei Dropdowns ist der Cursor
        // weiter entfernt (mind. Button-Höhe), dann gilt die übergebene Position.
        var detectionPoint = new Point(xpos, ypos);
        if (Math.Abs(Cursor.Position.X - xpos) <= 15 && Math.Abs(Cursor.Position.Y - ypos) <= 15) {
            detectionPoint = Cursor.Position;
        }

        Position_SetWindowIntoScreen(Generic.PointOnScreenNr(detectionPoint), xpos, ypos);
        OutsideClicked += (_, _) => OnCancel();
        Show();
    }

    #endregion

    #region Events

    public event EventHandler<AddItemEventArgs>? AddClicked;

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

    /// <inheritdoc cref="Controls.ListBox.ItemPadding"/>
    public int ItemPadding {
        get => lstbx.ItemPadding;
        set {
            if (lstbx.ItemPadding == value) { return; }
            lstbx.ItemPadding = value;
            RecalcFormSize();
        }
    }

    #endregion

    #region Methods

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
                translate, controlDesign, itemDesign, autosort, false, AddType.None, false, false, null, null);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort) => new(items, checkBehavior, check, xpos, ypos, steuerWi, connectedControl, translate, controlDesign, itemDesign, autosort, false, AddType.None, false, false, null, null);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
            translate, controlDesign, itemDesign, autosort, removeAllowed, AddType.None, false, false, null, null);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, object? hotItem) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
            translate, controlDesign, itemDesign, autosort, removeAllowed, AddType.None, false, false, null, hotItem);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, AddType addAllowed, bool moveAllowed) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
            translate, controlDesign, itemDesign, autosort, removeAllowed, addAllowed, moveAllowed, false, null, null);

    /// <summary>
    /// Zeigt eine Mini-Toolbar an der übergebenen Bildschirm-Position.
    /// Wird von <see cref="Interfaces.IMiniToolbar.MiniToolbarShow"/> verwendet.
    /// </summary>
    public static FloatingInputBoxListBoxStyle ShowAtPosition(List<AbstractListItem> items, Point screenPosition, Control? connectedControl, object? hotItem) => new(items, CheckBehavior.NoSelection, null, screenPosition.X, screenPosition.Y, -1, connectedControl,
            false, ListBoxAppearance.MiniToolbar, Design.Item_MiniToolbar, false, false, AddType.None, false, false, null, hotItem);

    public static FloatingInputBoxListBoxStyle ShowComboBoxDropDown(List<AbstractListItem> items, string? check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, bool autosort, bool removeAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) => new(items, check is null ? CheckBehavior.NoSelection : CheckBehavior.SingleSelection, check is null ? null : [check], xpos, ypos, steuerWi, connectedControl, translate, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, autosort, removeAllowed, AddType.None, false, false, customContextMenuItems, null);

    public static FloatingInputBoxListBoxStyle ShowComboBoxDropDown(List<AbstractListItem> items, string? check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, bool autosort, bool removeAllowed, AddType addAllowed, bool moveAllowed, bool itemEditAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) => new(items, check is null ? CheckBehavior.NoSelection : CheckBehavior.SingleSelection, check is null ? null : [check], xpos, ypos, steuerWi, connectedControl, translate, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, autosort, removeAllowed, addAllowed, moveAllowed, itemEditAllowed, customContextMenuItems, null);

    public void Generate_ListBox1(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int minWidth, AddType addNewAllowed, bool moveAllowed, bool itemEditAllowed, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) {
        var (biggestItemX, _, heightAdded, _) = items.CanvasItemData(itemDesign);
        if (addNewAllowed != AddType.None) { heightAdded += 26; }

        // MiniToolbar: horizontales Layout — Breite = Summe aller Item-Breiten.
        // ItemPadding erzeugt den gewünschten Abstand zwischen den Icons
        // (Skin.PaddingSmal), ohne dass dafür zusätzliche Platzhalter-Items
        // oder eigene Zeichnungsroutinen nötig sind.
        var pad = 0;
        if (controlDesign == ListBoxAppearance.MiniToolbar) {
            pad = Skin.PaddingSmal;
            var itemSize = IMiniToolbar.IconSize;
            var visibleCount = items.Count(i => i.Visible);
            biggestItemX = Math.Max(biggestItemX, visibleCount * itemSize + Math.Max(0, visibleCount - 1) * pad);
            heightAdded = itemSize;
        }

        lstbx.Appearance = controlDesign;
        lstbx.Translate = translate;
        lstbx.AutoSort = autosort;
        lstbx.RemoveAllowed = removeAllowed;
        lstbx.AddAllowed = addNewAllowed;
        lstbx.MoveAllowed = moveAllowed;
        lstbx.ItemEditAllowed = itemEditAllowed;
        lstbx.CustomContextMenuItems = customContextMenuItems;
        lstbx.ItemPadding = pad;

        AdjustFormSize(biggestItemX, heightAdded, minWidth);

        // MiniToolbar: lstbx hat Anchor=Top|Left (siehe Konstruktor) und wird
        // daher nicht automatisch an die Form-Größe angepasst. Size explizit
        // setzen, damit die Icons den vollen Innenbereich ausfüllen.
        if (controlDesign == ListBoxAppearance.MiniToolbar) {
            lstbx.Size = new Size(Size.Width - (lstbx.Left * 2), Size.Height - (lstbx.Top * 2));
        }

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
        var primary = Screen.PrimaryScreen;
        var primaryWi = primary is null ? 1920 : primary.Bounds.Size.Width;
        var primaryHe = primary is null ? 1080 : primary.Bounds.Size.Height;
        var maxWi = (int)(primaryWi * 0.7);
        var maxHe = (int)(primaryHe * 0.7);
        if (biggestItemX > maxWi) { biggestItemX = maxWi; }
        if (heightAdded > maxHe) {
            heightAdded = maxHe;
            biggestItemX += 20;
        }
        Size = new Size(biggestItemX + (lstbx.Left * 2), heightAdded + (lstbx.Top * 2));
    }

    private void ListBox1_AddClicked(object? sender, AddItemEventArgs e) => AddClicked?.Invoke(this, e);

    private void ListBox1_ItemAddedByClick(object? sender, AbstractListItemEventArgs e) {
        RecalcFormSize();
        ItemAddedByClick?.Invoke(this, e);
    }

    private void ListBox1_ItemClicked(object sender, AbstractListItemEventArgs e) {
        // Selectet Chanched bringt nix, da es ja darum geht, ob eine Node angeklickt wurde.
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

    /// <summary>
    /// Berechnet die Größe des Fensters anhand der aktuellen Items neu.
    /// Die aktuelle Breite wird als Minimum beibehalten, damit das Fenster
    /// beim Löschen eines Items nicht schrumpft und beim Hinzufügen wächst.
    /// </summary>
    private void RecalcFormSize() {
        var (biggestItemX, _, heightAdded, _) = lstbx.Items.CanvasItemData(lstbx.ItemDesign);
        if (lstbx.AddAllowed != AddType.None) { heightAdded += 26; }
        if (lstbx.Appearance != ListBoxAppearance.MiniToolbar) {
            var visibleCount = lstbx.Items.Count(i => i.Visible);
            heightAdded += Math.Max(0, visibleCount - 1) * lstbx.ItemPadding;
        }
        AdjustFormSize(biggestItemX, heightAdded, Width - (lstbx.Left * 2));
        if (lstbx.Appearance == ListBoxAppearance.MiniToolbar) {
            lstbx.Size = new Size(Size.Width - (lstbx.Left * 2), Size.Height - (lstbx.Top * 2));
        }
    }

    #endregion
}