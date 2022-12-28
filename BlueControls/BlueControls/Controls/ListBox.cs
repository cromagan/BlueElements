// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("ItemClicked")]
public partial class ListBox : GenericControl, IContextMenu, IBackgroundNone, ITranslateable {

    #region Fields

    private AddType _addAlloweds = AddType.Text;

    private BlueListBoxAppearance _appearance;

    private bool _filterAllowed;

    //Muss was gesetzt werden, sonst hat der Designer nachher einen Fehler
    private BasicListItem? _mouseOverItem;

    private bool _moveAllowed;

    private bool _removeAllowed;

    #endregion

    #region Constructors

    public ListBox() : base(true, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Item = new ItemCollectionList();
        Item.Changed += _Item_ListOrItemChanged;
        Item.ItemCheckedChanged += _Item_ItemCheckedChanged;
        Item.ItemAdded += _Item_ItemAdded;
        Item.ItemRemoved += _Item_ItemRemoved;
        Item.ItemRemoving += _Item_ItemRemoving;
        _appearance = BlueListBoxAppearance.Listbox;
    }

    #endregion

    #region Events

    public event EventHandler AddClicked;

    public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

    public event EventHandler<ListEventArgs> ItemAdded;

    public event EventHandler ItemCheckedChanged;

    public event EventHandler<BasicListItemEventArgs> ItemClicked;

    public event EventHandler<BasicListItemEventArgs> ItemDoubleClick;

    /// <summary>
    /// Wird nach jedem entfernen eines Items ausgelöst. Auch beim Initialisiern oder bei einem Clear.
    /// Soll eine Benutzerinteraktion abgefragt werden, ist RemoveClicked besser.
    /// </summary>
    public event EventHandler ItemRemoved;

    /// <summary>
    /// Wird vor jedem entfernen eines Items ausgelöst. Auch beim Initialisiern oder bei einem Clear.
    /// Soll eine Benutzerinteraktion abgefragt werden, ist RemoveClicked besser.
    /// </summary>
    public event EventHandler<ListEventArgs> ItemRemoving;

    public event EventHandler ListOrItemChanged;

    /// <summary>
    /// Wird nur ausgelöst, wenn explicit der Button gedrückt wird.
    /// </summary>
    public event EventHandler<ListOfBasicListItemEventArgs> RemoveClicked;

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

    [DefaultValue(BlueListBoxAppearance.Listbox)]
    public BlueListBoxAppearance Appearance {
        get => _appearance;
        set {
            if (_appearance == value) { return; }
            _appearance = value;
            Item.Appearance = value;
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
    public ItemCollectionList Item { get; }

    [DefaultValue(false)]
    public bool MoveAllowed {
        get => _moveAllowed;
        set {
            if (_moveAllowed == value) { return; }
            _moveAllowed = value;
            ////if (_MoveAllowed) { _FilterAllowed = false; }
            CheckButtons();
        }
    }

    public override string QuickInfoText {
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
    public ItemCollectionList Suggestions { get; } = new();

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    #endregion

    //public BasicListItem? Add_FromFileSystem() {
    //    var f = IO.GetFilesWithFileSelector(string.Empty, false);
    //    if (f is null) { return null; }

    //    var picture = BitmapExt.Image_FromFile(f[0]);
    //    return picture != null ? Item.Add((Bitmap)picture, f[0]) : Item.Add(Converter.FileToByte(f[0]), f[0]);
    //}

    #region Methods

    public TextListItem? Add_Text(string? val) {
        if (val == null || string.IsNullOrEmpty(val)) { return null; }
        if (Item.Any(thisItem => thisItem != null && string.Equals(thisItem.Internal, val, StringComparison.OrdinalIgnoreCase))) {
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
        if (Suggestions == null || Suggestions.Count == 0) {
            MessageBox.Show("Keine (weiteren) Werte vorhanden.", ImageCode.Information, "OK");
            return;
        }
        Suggestions.CheckBehavior = CheckBehavior.SingleSelection;
        var rück = InputBoxListBoxStyle.Show("Bitte wählen sie einen Wert:", Suggestions, AddType.None, true);

        if (rück == null || rück.Count == 0) { return; }

        Item.Add((BasicListItem)Suggestions[rück[0]].Clone());
        //.CloneToNewCollection(Item);
    }

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

    public new void Focus() {
        if (Focused()) { return; }
        base.Focus();
    }

    public new bool Focused() => base.Focused || Plus.Focused || Minus.Focused || Up.Focused || Down.Focused || SliderY.Focused() || FilterCap.Focused || FilterTxt.Focused;

    public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs? e, ItemCollectionList? items, out object? hotItem, List<string> tags, ref bool cancel, ref bool translate) => hotItem = e == null ? null : MouseOverNode(e.X, e.Y);

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public void OnListOrItemChanged() => ListOrItemChanged?.Invoke(this, System.EventArgs.Empty);

    protected override void DrawControl(Graphics gr, States state) {
        if (Item != null) { Item.Appearance = _appearance; }
        var tmp = Design.ListBox;
        if (_appearance is not BlueListBoxAppearance.Gallery and not BlueListBoxAppearance.FileSystem) { tmp = (Design)_appearance; }
        var paintModXx = 0;
        var paintModYx = 0;
        var vStateBox = state;
        if (Convert.ToBoolean(vStateBox & States.Standard_MouseOver)) { vStateBox ^= States.Standard_MouseOver; }
        if (Convert.ToBoolean(vStateBox & States.Standard_MousePressed)) { vStateBox ^= States.Standard_MousePressed; }
        if (Convert.ToBoolean(vStateBox & States.Standard_HasFocus)) { vStateBox ^= States.Standard_HasFocus; }
        if (Item.Count == 0) {
            SliderY.Visible = false;
            SliderY.Value = 0;
        }
        if (ButtonsVisible()) { paintModYx = Plus.Height; }
        var (biggestItemX, _, heightAdded, senkrechtAllowed) = Item.ItemData();
        Item.ComputeAllItemPositions(new Size(DisplayRectangle.Width, DisplayRectangle.Height - paintModYx), SliderY, biggestItemX, heightAdded, senkrechtAllowed);
        if (SliderY.Visible) { paintModXx = SliderY.Width; }
        var borderCoords = new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top,
           DisplayRectangle.Width - paintModXx, DisplayRectangle.Height - paintModYx);
        var visArea = borderCoords with { Y = (int)(borderCoords.Y + SliderY.Value) };
        if (borderCoords.Height > 0) {
            //// Kann sein, wenn PaintModY größer als die Höhe ist
            //if (_Appearance == BlueListBoxAppearance.Listbox)
            //{
            Skin.Draw_Back(gr, tmp, vStateBox, borderCoords, this, true);
            //}
            //else
            //{
            //    clsSkin.Draw_Back_Transparent(GR, DisplayRectangle, this);
            //}
        }
        _mouseOverItem = MouseOverNode(MousePos().X, MousePos().Y);
        object locker = new();
        System.Threading.Tasks.Parallel.ForEach(Item, thisItem => {
            if (thisItem.Pos.IntersectsWith(visArea)) {
                var vStateItem = vStateBox;
                if (_mouseOverItem == thisItem && Enabled) { vStateItem |= States.Standard_MouseOver; }
                if (!thisItem.Enabled) { vStateItem = States.Standard_Disabled; }
                if (thisItem.Checked) { vStateItem |= States.Checked; }
                lock (locker) {
                    thisItem.Draw(gr, 0, (int)SliderY.Value, Item.ControlDesign, Item.ItemDesign, vStateItem, true, FilterTxt.Text, false); // Items müssen beim Erstellen ersetzt werden!!!!
                }
            }
        });
        if (borderCoords.Height > 0) {
            // Kann sein, wenn PaintModY größer als die Höhe ist
            if (tmp == Design.ListBox) { Skin.Draw_Border(gr, tmp, vStateBox, borderCoords); }
        }
        if (paintModYx > 0) { Skin.Draw_Back_Transparent(gr, new Rectangle(0, borderCoords.Bottom, Width, paintModYx), this); }
    }

    protected override void OnDoubleClick(System.EventArgs e) {
        if (!Enabled) { return; }
        var nd = MouseOverNode(MousePos().X, MousePos().Y);
        if (nd == null) { return; }
        OnItemDoubleClick(new BasicListItemEventArgs(nd));
    }

    protected override void OnHandleCreated(System.EventArgs e) {
        base.OnHandleCreated(e);
        // Um den allerersten Check nicht zu verpassen
        CheckButtons();
    }

    protected void OnItemAdded(ListEventArgs e) => ItemAdded?.Invoke(this, e);

    protected void OnItemRemoved(System.EventArgs e) => ItemRemoved?.Invoke(this, e);

    protected void OnItemRemoving(ListEventArgs e) => ItemRemoving?.Invoke(this, e);

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        if (_mouseOverItem != null) {
            _mouseOverItem = null;
            Invalidate();
        }
    }

    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseMove(e);
        var nd = MouseOverNode(MousePos().X, MousePos().Y);
        if (nd != _mouseOverItem) {
            _mouseOverItem = nd;
            Invalidate();
            DoQuickInfo();
        }
    }

    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseUp(e);
        if (!Enabled) { return; }
        var nd = MouseOverNode(e.X, e.Y);
        if (nd != null && !nd.Enabled) { return; }
        switch (e.Button) {
            case System.Windows.Forms.MouseButtons.Left:
                if (nd != null) {
                    if (Appearance is BlueListBoxAppearance.Listbox or BlueListBoxAppearance.Autofilter or BlueListBoxAppearance.Gallery or BlueListBoxAppearance.FileSystem) {
                        if (nd.IsClickable()) { nd.Checked = !nd.Checked; }
                    }
                    OnItemClicked(new BasicListItemEventArgs(nd));
                }
                break;

            case System.Windows.Forms.MouseButtons.Right:
                FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
                break;
        }
    }

    protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e) {
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

    private void _Item_ItemAdded(object sender, ListEventArgs e) {
        if (IsDisposed) { return; }
        //Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
        Invalidate();
        OnItemAdded(e);
    }

    private void _Item_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        CheckButtons();
        Invalidate();
        OnItemCheckedChanged();
    }

    private void _Item_ItemRemoved(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
        OnItemRemoved(e);
    }

    private void _Item_ItemRemoving(object sender, ListEventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
        OnItemRemoving(e);
    }

    private void _Item_ListOrItemChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        Invalidate();
        OnListOrItemChanged();
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
        for (var z = Item.Count - 1; z >= 0; z--) {
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

    private void Minus_Click(object sender, System.EventArgs e) {
        OnRemoveClicked(new ListOfBasicListItemEventArgs(Item.Checked()));
        foreach (var thisItem in Item.Checked()) {
            Item.Remove(thisItem);
        }
        CheckButtons();
    }

    private BasicListItem? MouseOverNode(int x, int y) => ButtonsVisible() && y >= Height - Plus.Height ? null : Item[x, (int)(y + SliderY.Value)];

    private void OnAddClicked() => AddClicked?.Invoke(this, System.EventArgs.Empty);

    private void OnItemCheckedChanged() => ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);

    private void OnItemClicked(BasicListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    private void OnItemDoubleClick(BasicListItemEventArgs e) => ItemDoubleClick?.Invoke(this, e);

    private void OnRemoveClicked(ListOfBasicListItemEventArgs e) => RemoveClicked?.Invoke(this, e);

    private void Plus_Click(object sender, System.EventArgs e) {
        OnAddClicked();
        switch (_addAlloweds) {
            case AddType.UserDef:
                break;

            case AddType.Text:
                Add_Text();
                break;

            case AddType.OnlySuggests:
                Add_TextBySuggestion();
                break;

            case AddType.None:
                break;

            //case AddType.BinarysFromFileSystem:
            //    Add_FromFileSystem();
            //    break;

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

    //private void SwapItems(BasicListItem Nr1, BasicListItem Nr2)
    //{
    //    Item.Swap(ref Nr1, ref Nr2);
    //    CheckButtons();
    //}
    private void Up_Click(object sender, System.EventArgs e) {
        BasicListItem ln = null;
        foreach (var thisItem in Item) {
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