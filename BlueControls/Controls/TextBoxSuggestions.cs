// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using System.Collections.ObjectModel;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(TextChanged))]
public class TextBoxSuggestions : GenericControl, IBackgroundNone, IInputFormat, IContextMenu {

    #region Fields

    private const int AreaPadding = 4;
    private const int ChipSpacing = 6;
    private const int MinChipWidth = 30;
    private const int MinTbHeight = 24;
    private const int MinTbWidth = 50;
    private const int ScrollStep = 24;
    private readonly List<Rectangle> _chipContentRects = [];
    private readonly TextBox _textBox;
    private List<ExtCharListItem> _chipItems = [];
    private bool _chipsAbove;
    private int _hoveredIndex = -1;
    private bool _layoutDirty = true;
    private int _maxScroll;
    private int _scrollOffset;
    private Rectangle _suggestionArea;
    private int _totalContentHeight;
    private int _yAdjustment;

    #endregion

    #region Constructors

    public TextBoxSuggestions() : base(true, true, false) {
        SetStyle(System.Windows.Forms.ControlStyles.ContainerControl, true);

        _textBox = new TextBox();
        _textBox.TextChanged += TextBox_TextChanged;
        _textBox.EnterKey += (_, _) => OnEnterKey();
        _textBox.EscKey += (_, _) => OnEscKey();
        _textBox.TabKey += (_, _) => OnTabKey();
        _textBox.LostFocus += (_, e) => OnLostFocus(e);
        _textBox.NavigateToNext += (_, e) => OnNavigateToNext(e);
        Controls.Add(_textBox);
    }

    #endregion

    #region Events

    public event EventHandler? EnterKey;

    public event EventHandler? EscKey;

    public event EventHandler<NavigationDirectionEventArgs>? NavigateToNext;

    public event EventHandler? TabKey;

    #endregion

    #region Properties

    public AdditionalCheck AdditionalFormatCheck {
        get => _textBox.AdditionalFormatCheck;
        set => _textBox.AdditionalFormatCheck = value;
    }

    public string AllowedChars {
        get => _textBox.AllowedChars;
        set => _textBox.AllowedChars = value;
    }

    public bool ContextMenuDefault {
        get => _textBox.ContextMenuDefault;
        set => _textBox.ContextMenuDefault = value;
    }

    [DefaultValue(null)]
    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems {
        get => _textBox.CustomContextMenuItems;
        set => _textBox.CustomContextMenuItems = value;
    }

    public IReadOnlySet<string>? CustomVocabulary {
        get => _textBox.CustomVocabulary;
        set => _textBox.CustomVocabulary = value;
    }

    public int MaxTextLength {
        get => _textBox.MaxTextLength;
        set => _textBox.MaxTextLength = value;
    }

    public bool MultiLine {
        get => _textBox.MultiLine;
        set => _textBox.MultiLine = value;
    }

    [DefaultValue(SuggestionPosition.Bottom)]
    public SuggestionPosition Position {
        get;
        set {
            if (field == value) { return; }
            field = value;
            _scrollOffset = 0;
            _layoutDirty = true;
            Invalidate();
        }
    } = SuggestionPosition.Bottom;

    public override string QuickInfo {
        get => _textBox.QuickInfo;
        set => _textBox.QuickInfo = value;
    }

    public int RaiseChangeDelay {
        get => _textBox.RaiseChangeDelay;
        set => _textBox.RaiseChangeDelay = value;
    }

    public string RegexCheck {
        get => _textBox.RegexCheck;
        set => _textBox.RegexCheck = value;
    }

    public bool SpellCheckingEnabled {
        get => _textBox.SpellCheckingEnabled;
        set => _textBox.SpellCheckingEnabled = value;
    }

    public string Suffix {
        get => _textBox.Suffix;
        set => _textBox.Suffix = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ReadOnlyCollection<string> Suggestions {
        get => _chipItems.Select(c => c.ListItem.KeyName).ToList().AsReadOnly();
        set {
            _chipItems = [.. value.Select(s => new ExtCharListItem(new TextListItem(s, s, null, false, true, string.Empty, string.Empty)))];
            _scrollOffset = 0;
            _layoutDirty = true;
            Invalidate();
        }
    }

    /// <summary>
    /// Benötigt, dass der Designer das nicht erstellt
    /// </summary>
    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;
        set { _ = value; base.TabIndex = 0; }
    }

    /// <summary>
    /// Benötigt, dass der Designer das nicht erstellt
    /// </summary>
    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
        set { _ = value; base.TabStop = false; }
    }

    [DefaultValue("")]
    public override string Text {
        get => _textBox.Text;
        set => _textBox.Text = value;
    }

    [DefaultValue(typeof(Size), "0, 0")]
    public Size TextboxSize {
        get;
        set {
            if (field == value) { return; }
            if (_yAdjustment != 0) {
                Location = new Point(Location.X, Location.Y + _yAdjustment);
                _yAdjustment = 0;
                _chipsAbove = false;
            }
            field = value;
            _layoutDirty = true;
            Invalidate();
        }
    } = Size.Empty;

    public bool TextFormatingAllowed {
        get => _textBox.TextFormatingAllowed;
        set => _textBox.TextFormatingAllowed = value;
    }

    [DefaultValue(SteuerelementVerhalten.Scrollen_ohne_Textumbruch)]
    public SteuerelementVerhalten Verhalten {
        get => _textBox.Verhalten;
        set => _textBox.Verhalten = value;
    }

    #endregion

    #region Methods

    public new bool Focus() => _textBox.Focus();

    public List<AbstractListItem>? GetContextMenuItems(object? hotItem) => _textBox.GetContextMenuItems(hotItem);

    public int GetEstimatedHeight(int availableWidth, int textboxHeight, int maxChipRows = 3) {
        if (_chipItems.Count == 0 || textboxHeight < 1) { return textboxHeight; }

        var lineH = 0;
        var chipSizes = new List<Size>(_chipItems.Count);
        foreach (var item in _chipItems) {
            var itemSize = item.SizeCanvas;
            if ((int)itemSize.Height > lineH) { lineH = (int)itemSize.Height; }
            chipSizes.Add(new Size(Math.Max(MinChipWidth, (int)itemSize.Width), (int)itemSize.Height));
        }

        if (lineH < 1) { lineH = 20; }

        var areaW = availableWidth - 2 * AreaPadding;
        var x = 0;
        var rowCount = 1;
        for (var i = 0; i < chipSizes.Count; i++) {
            var chipW = chipSizes[i].Width;
            if (x + chipW > areaW && x > 0) {
                x = 0;
                rowCount++;
                if (rowCount > maxChipRows) { break; }
            }
            x += chipW + ChipSpacing;
        }

        var totalContentHeight = rowCount * (lineH + ChipSpacing) - ChipSpacing;
        return textboxHeight + totalContentHeight + 2 * AreaPadding;
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        _textBox.TextChanged -= TextBox_TextChanged;

        if (disposing) {
            EnterKey = null;
            EscKey = null;
            NavigateToNext = null;
            TabKey = null;
            _textBox.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);

        EnsureLayout();

        Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);

        if (_chipItems.Count == 0 || _suggestionArea.Width < 1 || _suggestionArea.Height < 1) { return; }

        Skin.Draw_Border(gr, Design.GroupBox_RoundRect, Enabled ? States.Standard : States.Standard_Disabled, _suggestionArea);

        for (var i = 0; i < _chipContentRects.Count; i++) {
            if (i >= _chipItems.Count) { break; }

            var r = _chipContentRects[i];
            var scrolled = new Rectangle(r.X, r.Y - _scrollOffset, r.Width, r.Height);

            if (!_suggestionArea.IntersectsWith(scrolled)) { continue; }

            _chipItems[i].ChipState = i == _hoveredIndex
                ? States.Standard_MouseOver
                : States.Standard;

            if (!Enabled) { _chipItems[i].ChipState = States.Standard_Disabled; }

            _chipItems[i].Draw(gr, scrolled.Location, scrolled.Size, 1f);
        }
    }

    protected virtual void OnEnterKey() => EnterKey?.Invoke(this, System.EventArgs.Empty);

    protected virtual void OnEscKey() => EscKey?.Invoke(this, System.EventArgs.Empty);

    protected override void OnGotFocus(System.EventArgs e) {
        base.OnGotFocus(e);
        if (!_textBox.Focused) {
            _textBox.Focus();
        }
    }

    protected override void OnLostFocus(System.EventArgs e) {
        if (ContainsFocus || _textBox.Focused) { return; }
        base.OnLostFocus(e);
    }

    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
        if (IsDisposed || !Enabled) { return; }

        EnsureLayout();
        var p = _textBox.CursorPosition;
        var hitIndex = HitTestChip(e.X, e.Y);
        if (hitIndex >= 0 && hitIndex < _chipItems.Count) {
            p = _textBox.Insert(p, _chipItems[hitIndex].ListItem.KeyName);
        }

        base.OnMouseDown(e);
        _textBox.Focus();
        _textBox.CursorPosition = p;
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        if (_hoveredIndex >= 0) {
            _hoveredIndex = -1;
            Invalidate();
        }
        base.OnMouseLeave(e);
    }

    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
        if (IsDisposed) { return; }
        base.OnMouseMove(e);

        EnsureLayout();

        var newHovered = HitTestChip(e.X, e.Y);
        if (newHovered != _hoveredIndex) {
            _hoveredIndex = newHovered;
            Invalidate();
        }
    }

    protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e) {
        if (IsDisposed) { return; }

        EnsureLayout();

        var mp = PointToClient(MousePosition);
        if (!_suggestionArea.Contains(mp)) {
            base.OnMouseWheel(e);
            return;
        }

        var delta = e.Delta > 0 ? -ScrollStep : ScrollStep;
        var newOffset = Math.Max(0, Math.Min(_maxScroll, _scrollOffset + delta));
        if (newOffset != _scrollOffset) {
            _scrollOffset = newOffset;
            Invalidate();
        }
    }

    protected virtual void OnNavigateToNext(NavigationDirectionEventArgs e) => NavigateToNext?.Invoke(this, e);

    protected override void OnSizeChanged(System.EventArgs e) {
        base.OnSizeChanged(e);
        _layoutDirty = true;
        Invalidate();
    }

    protected virtual void OnTabKey() => TabKey?.Invoke(this, System.EventArgs.Empty);

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        if (Visible) {
            AdjustPosition();
        } else if (_yAdjustment != 0) {
            Location = new Point(Location.X, Location.Y + _yAdjustment);
            _yAdjustment = 0;
            _chipsAbove = false;
        }
    }

    private void AdjustPosition() {
        if (IsDisposed || TextboxSize == Size.Empty || Parent is null) { return; }

        if (_yAdjustment != 0) {
            Location = new Point(Location.X, Location.Y + _yAdjustment);
            _yAdjustment = 0;
        }

        var chipAreaHeight = Height - TextboxSize.Height;
        if (chipAreaHeight <= 0) {
            _chipsAbove = false;
            _layoutDirty = true;
            Invalidate();
            return;
        }

        if (Bottom > Parent.ClientSize.Height && Location.Y - chipAreaHeight >= 0) {
            _chipsAbove = true;
            _yAdjustment = chipAreaHeight;
            Location = new Point(Location.X, Location.Y - _yAdjustment);
        } else {
            _chipsAbove = false;
        }

        _layoutDirty = true;
        Invalidate();
    }

    private void BuildChipRects(List<Size> chipSizes, int availableWidth, int lineH, bool flowHorizontal) {
        _chipContentRects.Clear();
        _totalContentHeight = 0;

        var x = 0;
        var y = 0;
        var rowCount = 1;

        for (var i = 0; i < chipSizes.Count; i++) {
            var chipW = chipSizes[i].Width;
            if (chipW < 0) { chipW = availableWidth; }

            if (flowHorizontal && x + chipW > availableWidth && x > 0) {
                x = 0;
                y += lineH + ChipSpacing;
                rowCount++;
            }

            _chipContentRects.Add(new Rectangle(x, y, chipW, lineH));
            x += chipW + ChipSpacing;
        }

        _totalContentHeight = rowCount * (lineH + ChipSpacing) - ChipSpacing;
    }

    private void CalculateLayout() {
        _chipContentRects.Clear();
        _totalContentHeight = 0;
        _maxScroll = 0;
        _suggestionArea = Rectangle.Empty;

        if (Width < 10 || Height < 10) {
            PositionTextBox(DisplayRectangle);
            return;
        }

        if (_chipItems.Count == 0) {
            PositionTextBox(DisplayRectangle);
            return;
        }

        var isHorizontal = Position is SuggestionPosition.Top or SuggestionPosition.Bottom || TextboxSize != Size.Empty;

        var lineH = 0;
        var chipSizes = new List<Size>(_chipItems.Count);
        foreach (var item in _chipItems) {
            var itemSize = item.SizeCanvas;
            if ((int)itemSize.Height > lineH) { lineH = (int)itemSize.Height; }
            var w = isHorizontal ? Math.Max(MinChipWidth, (int)itemSize.Width) : -1;
            chipSizes.Add(new Size(w, (int)itemSize.Height));
        }

        if (lineH < 1) { lineH = 20; }

        if (TextboxSize != Size.Empty) {
            var areaW = Width - 2 * AreaPadding;
            BuildChipRects(chipSizes, areaW, lineH, true);
            var chipAreaHeight = Math.Max(lineH + 2 * AreaPadding,
                Math.Min(_totalContentHeight + 2 * AreaPadding, Height - TextboxSize.Height));

            Rectangle tbRect;
            if (_chipsAbove) {
                _suggestionArea = new Rectangle(0, 0, Width, chipAreaHeight);
                OffsetChipRects(AreaPadding, AreaPadding);
                tbRect = new Rectangle(0, chipAreaHeight, TextboxSize.Width, TextboxSize.Height);
            } else {
                tbRect = new Rectangle(0, 0, TextboxSize.Width, TextboxSize.Height);
                _suggestionArea = new Rectangle(0, TextboxSize.Height, Width, chipAreaHeight);
                OffsetChipRects(AreaPadding, TextboxSize.Height + AreaPadding);
            }

            _maxScroll = Math.Max(0, _totalContentHeight + 2 * AreaPadding - _suggestionArea.Height);
            _scrollOffset = Math.Min(_scrollOffset, _maxScroll);
            PositionTextBox(tbRect);
            return;
        }

        Rectangle tbArea;
        switch (Position) {
            case SuggestionPosition.Top: {
                    var areaW = Width - 2 * AreaPadding;
                    BuildChipRects(chipSizes, areaW, lineH, true);
                    var areaH = Math.Max(lineH + 2 * AreaPadding,
                        Math.Min(_totalContentHeight + 2 * AreaPadding, Height - MinTbHeight));
                    _suggestionArea = new Rectangle(0, 0, Width, areaH);
                    OffsetChipRects(_suggestionArea.X + AreaPadding, _suggestionArea.Y + AreaPadding);
                    tbArea = new Rectangle(0, areaH, Width, Height - areaH);
                    break;
                }

            case SuggestionPosition.Bottom: {
                    var areaW = Width - 2 * AreaPadding;
                    BuildChipRects(chipSizes, areaW, lineH, true);
                    var areaH = Math.Max(lineH + 2 * AreaPadding,
                        Math.Min(_totalContentHeight + 2 * AreaPadding, Height - MinTbHeight));
                    _suggestionArea = new Rectangle(0, Height - areaH, Width, areaH);
                    OffsetChipRects(_suggestionArea.X + AreaPadding, _suggestionArea.Y + AreaPadding);
                    tbArea = new Rectangle(0, 0, Width, Height - areaH);
                    break;
                }

            case SuggestionPosition.Left: {
                    var maxChipW = GetMaxChipWidth();
                    var areaW = Math.Clamp(maxChipW + 2 * AreaPadding, MinChipWidth + 2 * AreaPadding, Width - MinTbWidth);
                    BuildChipRects(chipSizes, areaW - 2 * AreaPadding, lineH, false);
                    _suggestionArea = new Rectangle(0, 0, areaW, Height);
                    OffsetChipRects(_suggestionArea.X + AreaPadding, _suggestionArea.Y + AreaPadding);
                    tbArea = new Rectangle(areaW, 0, Width - areaW, Height);
                    break;
                }

            default: { // Right
                    var maxChipW = GetMaxChipWidth();
                    var areaW = Math.Clamp(maxChipW + 2 * AreaPadding, MinChipWidth + 2 * AreaPadding, Width - MinTbWidth);
                    BuildChipRects(chipSizes, areaW - 2 * AreaPadding, lineH, false);
                    _suggestionArea = new Rectangle(Width - areaW, 0, areaW, Height);
                    OffsetChipRects(_suggestionArea.X + AreaPadding, _suggestionArea.Y + AreaPadding);
                    tbArea = new Rectangle(0, 0, Width - areaW, Height);
                    break;
                }
        }

        _maxScroll = Math.Max(0, _totalContentHeight + 2 * AreaPadding - _suggestionArea.Height);
        _scrollOffset = Math.Min(_scrollOffset, _maxScroll);

        PositionTextBox(tbArea);
    }

    private void EnsureLayout() {
        if (!_layoutDirty) { return; }
        _layoutDirty = false;
        CalculateLayout();
    }

    private int GetMaxChipWidth() {
        var maxW = MinChipWidth;
        foreach (var item in _chipItems) {
            var w = (int)item.SizeCanvas.Width;
            if (w > maxW) { maxW = w; }
        }
        return maxW;
    }

    private int HitTestChip(int mx, int my) {
        if (_chipItems.Count == 0) { return -1; }
        if (!_suggestionArea.Contains(mx, my)) { return -1; }

        for (var i = 0; i < _chipContentRects.Count; i++) {
            var r = _chipContentRects[i];
            var scrolled = new Rectangle(r.X, r.Y - _scrollOffset, r.Width, r.Height);
            if (scrolled.Contains(mx, my)) { return i; }
        }
        return -1;
    }

    private void OffsetChipRects(int dx, int dy) {
        for (var i = 0; i < _chipContentRects.Count; i++) {
            _chipContentRects[i] = new Rectangle(
                _chipContentRects[i].X + dx,
                _chipContentRects[i].Y + dy,
                _chipContentRects[i].Width,
                _chipContentRects[i].Height);
        }
    }

    private void PositionTextBox(Rectangle area) {
        if (area.Width < 1 || area.Height < 1) {
            _textBox.Visible = false;
            return;
        }

        var newBounds = new Rectangle(area.X, area.Y, area.Width, area.Height);
        if (_textBox.Bounds == newBounds) { return; }

        _textBox.Bounds = newBounds;
        _textBox.Visible = true;
        _textBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
    }

    private void TextBox_TextChanged(object? sender, System.EventArgs e) => OnTextChanged(e);

    #endregion
}