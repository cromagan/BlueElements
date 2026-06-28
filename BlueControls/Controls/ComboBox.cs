// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;
using System.Drawing.Design;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(TextChanged))]
public partial class ComboBox : TextBox, ITranslateable {

    #region Fields

    private readonly List<AbstractListItem> _items = [];

    private bool _btnDropDownIsIn;

    private System.Windows.Forms.ComboBoxStyle _dropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;

    private ExtText? _eTxt;

    /// <summary>
    /// Kümmert sich darum, wenn die ComboBox wie ein Button aussieht, dass immer
    /// der Button-Text stehen bleibt und nicht der Ausgewählte
    /// </summary>
    private string _initialtext = string.Empty;

    private string? _lastClickedText;

    #endregion

    #region Constructors

    public ComboBox() : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetStyle(System.Windows.Forms.ControlStyles.ContainerControl, true);
        btnDropDown.Left = Width - btnDropDown.Width;
        btnDropDown.Top = 0;
        btnDropDown.Height = Height;

        btnEdit.Left = Width - btnDropDown.Width - btnEdit.Width;
        btnEdit.Top = 0;
        btnEdit.Height = Height;
    }

    #endregion

    #region Events

    public event EventHandler? DropDownShowing;

    public event EventHandler<AbstractListItemEventArgs>? ItemAddedByClick;

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    public event EventHandler<AbstractListItemEventArgs>? ItemRemoved;

    public event EventHandler? UpDownClicked;

    #endregion

    #region Properties

    [DefaultValue(AddType.None)]
    public AddType AddAllowed { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ListBox.dAddMethod? AddMethod { get; set; }

    [DefaultValue(true)]
    public bool AutoSort {
        get;
        set {
            if (value == field) { return; }
            field = value;
            Invalidate();
        }
    } = true;

    [DefaultValue(ComboboxStyle.TextBox)]
    public ComboboxStyle DrawStyle {
        get;
        set {
            if (field != value) {
                field = value;
                Invalidate();
            }
            SetStyle();
        }
    } = ComboboxStyle.TextBox;

    [DefaultValue(System.Windows.Forms.ComboBoxStyle.DropDown)]
    public System.Windows.Forms.ComboBoxStyle DropDownStyle {
        get => _dropDownStyle;
        set {
            if (value != _dropDownStyle) {
                _dropDownStyle = value;
                Invalidate();
            }
            SetStyle();
        }
    }

    [DefaultValue("")]
    [Category("Darstellung")]
    [Editor(typeof(QuickPicSelector), typeof(UITypeEditor))]
    public string ImageCode {
        get;
        set {
            if (field != value) {
                field = value;
                Invalidate();
            }
            SetStyle();
        }
    } = string.Empty;

    public int ItemCount => _items.Count;

    [DefaultValue(false)]
    public bool ItemEditAllowed {
        get;
        set {
            if (field == value) { return; }
            field = value;
            btnEdit.Visible = false;
        }
    }

    [DefaultValue(false)]
    public bool MoveAllowed { get; set; }

    [DefaultValue(false)]
    public bool RemoveAllowed { get; set; }

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    protected override Design Design => GetParentType() is ParentType.RibbonGroupBox or ParentType.RibbonPage
                                                            ? Design.RibbonBar_ComboBox_TextBox
                                                            : Design.ComboBox_TextBox;

    #endregion

    #region Indexers

    public AbstractListItem? this[string @internal] {
        get {
            try {
                return _items.GetByKey(@internal);
            } catch {
                Develop.AbortAppIfStackOverflow();
                return this[@internal];
            }
        }
    }

    public AbstractListItem? this[int no] {
        get {
            try {
                return no < 0 || no >= _items.Count ? null : _items[no];
            } catch {
                Develop.AbortAppIfStackOverflow();
                return this[no];
            }
        }
    }

    #endregion

    #region Methods

    public void ItemAdd(AbstractListItem item) {
        _items.Add(item);
        Invalidate();
    }

    public void ItemClear() {
        if (_items.Count == 0) { return; }
        _items.Clear();
        Invalidate();
    }

    public void ShowMenu(object sender, System.Windows.Forms.MouseEventArgs e) {
        if (IsDisposed) { return; }
        btnEdit.Visible = false;

        if (_btnDropDownIsIn || !Enabled) { return; }
        _btnDropDownIsIn = true;
        OnDropDownShowing();
        if (_items.Count == 0) { _btnDropDownIsIn = false; return; }
        int x, y;
        if (sender is Button but) {
            x = System.Windows.Forms.Cursor.Position.X - but.MousePos().X - but.Location.X;
            y = System.Windows.Forms.Cursor.Position.Y - but.MousePos().Y + Height; //Identisch
        } else {
            x = System.Windows.Forms.Cursor.Position.X - MousePos().X;
            y = System.Windows.Forms.Cursor.Position.Y - MousePos().Y + Height; //Identisch
        }

        var dropDownMenu = FloatingInputBoxListBoxStyle.ShowComboBoxDropDown(_items, Text, x, y, Width, this, Translate, AutoSort, RemoveAllowed, AddAllowed, AddMethod, MoveAllowed, CustomContextMenuItems);
        dropDownMenu.Cancel += DropDownMenu_Cancel;
        dropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
        dropDownMenu.ItemRemoved += DropDownMenu_ItemRemoved;
        dropDownMenu.ItemAddedByClick += DropDownMenu_ItemAddedByClick;
        dropDownMenu.UpDownClicked += DropDownMenu_UpDownClicked;
        _btnDropDownIsIn = false;
    }

    internal void ItemAddRange(List<AbstractListItem>? items) {
        if (items is null) { return; }

        foreach (var thisIt in items) {
            _items.RemoveAll(x => x.KeyName == thisIt.KeyName);
            ItemAdd(thisIt);
        }
    }

    internal ReadOnlyCollection<AbstractListItem> Items() => _items.AsReadOnly();

    internal void Remove(AbstractListItem thisit) {
        if (!_items.Contains(thisit)) { return; }
        _items.Remove(thisit);
        Invalidate();
    }

    internal bool WasThisValueClicked() => _lastClickedText is not null && Text == _lastClickedText;

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) { }
            FloatingForm.Close(this);
            _dropDownStyle = 0;
            ImageCode = string.Empty;
            DrawStyle = 0;
        } finally {
            base.Dispose(disposing);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }

        if (_dropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDownList) {
            if (_items.Count == 0) {
                state = States.Standard_Disabled;
            }
        }

        if (DrawStyle != ComboboxStyle.TextBox) {
            btnEdit.Visible = false;
            if (string.IsNullOrEmpty(_initialtext) && !string.IsNullOrEmpty(Text)) { _initialtext = Text; }

            _eTxt ??= new ExtText((Design)DrawStyle, state);

            Button.DrawButton(this, gr, (Design)DrawStyle, state, QuickImage.Get(ImageCode), Alignment.Horizontal_Vertical_Center, true, _eTxt, _initialtext, DisplayRectangle, Translate);
            btnDropDown.Invalidate();
            return;
        }

        btnDropDown.Enabled = _items.Count > 0;
        var vType = Design.ComboBox_TextBox;
        if (GetParentType() is ParentType.RibbonGroupBox or ParentType.RibbonPage) {
            vType = Design.RibbonBar_ComboBox_TextBox;
        }

        var i = _items.GetByKey(Text);
        if (i is null) {
            base.DrawControl(gr, state);
            btnDropDown.Invalidate();
            btnEdit.Invalidate();
            return;
        }

        //i.Parent = Item; // Um den Stil zu wissen
        if (Focused && _dropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDown) {
            // Focused = Bearbeitung erwünscht, Cursor anzeigen und KEINE Items zeichnen
            base.DrawControl(gr, state);
            btnDropDown.Invalidate();
            btnEdit.Invalidate();
            return;
        }

        if (_dropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDown) {
            if (i is TextListItem { Symbol: null } tempVar2) {
                if (tempVar2.IsClickable()) {
                    base.DrawControl(gr, state);
                    btnDropDown.Invalidate();
                    btnEdit.Invalidate();
                    return;
                }
            }
        }

        Skin.Draw_Back(gr, vType, state, DisplayRectangle, this, true);

        if (!FloatingForm.IsShowing(this)) {
            // Nur wenn die Selectbox gerade Nicht angezeigt wird, um hin und her Konvertierungen zu vermeiden
            var r = i.UntrimmedCanvasSize(Design.Item_DropdownMenu);
            i.CanvasPosition = new Rectangle(0, 0, r.Width, r.Height);
            var ymod = (int)((DisplayRectangle.Height - r.Height) / 2.0);
            i.Draw(gr, DisplayRectangle, Skin.PaddingSmal, ymod, Design.ComboBox_TextBox, Design.ComboBox_TextBox, state, false, string.Empty, Translate, Design.Undefined, 1f);
        }
        Skin.Draw_Border(gr, vType, state, DisplayRectangle);
        btnDropDown.Invalidate();
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        FloatingForm.Close(this);
        btnDropDown.Enabled = Enabled;
        btnDropDown.Invalidate();

        btnEdit.Visible = false;
    }

    protected override void OnGotFocus(System.EventArgs e) {
        if (_dropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDownList) {
            btnDropDown.Focus();
        } else {
            base.OnGotFocus(e);
        }
    }

    protected virtual void OnItemAddedByClick(AbstractListItemEventArgs e) => ItemAddedByClick?.Invoke(this, e);

    protected virtual void OnItemClicked(AbstractListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    protected virtual void OnItemRemoved(AbstractListItemEventArgs e) => ItemRemoved?.Invoke(this, e);

    protected override void OnLostFocus(System.EventArgs e) {
        Invalidate();
        CheckLostFocus(e);
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);

        if (!ItemEditAllowed) { return; }

        var _mouseOverItem = _items.GetByKey(Text);

        var editok = false;

        if (_mouseOverItem is ReadableListItem rli) {
            if (rli.Item is IEditable) { editok = true; }
            if (rli.Item is ISimpleEditor) { editok = true; }
        }

        btnEdit.Visible = editok;
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);

        var clientPos = PointToClient(System.Windows.Forms.Cursor.Position);
        if (ClientRectangle.Contains(clientPos)) { return; }

        btnEdit.Visible = false;
    }

    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
        if (e.Button == System.Windows.Forms.MouseButtons.Right) {
            var selectedItem = _items.GetByKey(Text);
            if (selectedItem is null && !string.IsNullOrEmpty(Text)) {
                selectedItem = _items.FirstOrDefault(a => a.KeyName == Text);
            }
            if (selectedItem is null && !string.IsNullOrEmpty(_lastClickedText)) {
                selectedItem = _items.GetByKey(_lastClickedText);
            }
            ((IContextMenu)this).ContextMenuShow(selectedItem);
            base.OnMouseUp(e);
            return;
        }

        if (_dropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDownList) {
            ShowMenu(this, e);
        } else {
            base.OnMouseUp(e);
        }
    }

    protected override void OnTextChanged(System.EventArgs e) {
        base.OnTextChanged(e);
    }

    protected virtual void OnUpDownClicked() => UpDownClicked?.Invoke(this, System.EventArgs.Empty);

    private void btnDropDown_LostFocus(object sender, System.EventArgs e) => CheckLostFocus(e);

    private void btnDropDown_MouseEnter(object sender, System.EventArgs e) => btnEdit.Visible = false;

    private void btnEdit_Click(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        FloatingForm.Close(this);

        var _mouseOverItem = _items.GetByKey(Text);

        if (!ItemEditAllowed) { return; }
        if (_mouseOverItem is ReadableListItem rli && rli.Item is IEditable or ISimpleEditor) {
            rli.Item.Edit();
        }
    }

    private void btnEdit_MouseLeave(object sender, System.EventArgs e) => OnMouseLeave(e);

    private void CheckLostFocus(System.EventArgs e) {
        try {
            if (btnDropDown is null) { return; }
            if (!btnDropDown.Focused && !btnEdit.Focused && !Focused && !FloatingForm.IsShowing(this)) { base.OnLostFocus(e); }
        } catch { }
    }

    //private void _Item_ItemRemoved(object sender, System.EventArgs e) {
    private void DropDownMenu_Cancel(object? sender, object mouseOver) {
        FloatingForm.Close(this);
        Focus();
    }

    private void DropDownMenu_ItemAddedByClick(object? sender, AbstractListItemEventArgs e) {
        if (e.Item is { } ali) { _items.Add(ali); }
        OnItemAddedByClick(e);
    }

    private void DropDownMenu_ItemClicked(object? sender, AbstractListItemEventArgs e) {
        if (e.Item is { } bli) {
            _lastClickedText = bli.KeyName;
            Text = bli.KeyName;
            OnItemClicked(new AbstractListItemEventArgs(bli));
        }
        FloatingForm.Close(this);
        Focus();
    }

    private void DropDownMenu_ItemRemoved(object? sender, AbstractListItemEventArgs e) {
        if (e.Item is { } bli) {
            Remove(bli);
            OnItemRemoved(e);
        }
    }

    private void DropDownMenu_UpDownClicked(object? sender, SwapEventArgs e) {
        var (a, b) = (e.Index1, e.Index2);
        if ((uint)a < _items.Count && (uint)b < _items.Count) {
            (_items[a], _items[b]) = (_items[b], _items[a]);
        }
        Invalidate();
        OnUpDownClicked();
    }

    private void OnDropDownShowing() => DropDownShowing?.Invoke(this, System.EventArgs.Empty);

    private void SetStyle() {
        if (InvokeRequired) {
            try {
                Invoke(new Action(() => SetStyle()));
            } catch { }
            return;
        }

        if (DrawStyle != ComboboxStyle.TextBox) {
            Cursor = System.Windows.Forms.Cursors.Arrow;
            _dropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            btnDropDown.Visible = false;
            // ImageCode = string.Empty; - Egal, wird eh ignoriert wenn es nicht gebraucht wird
        }
    }

    #endregion
}