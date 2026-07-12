// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.EventArgs;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BlueControls.Classes.TableItems;

/// <summary>
/// Basisklasse aller Zeilen-Elemente einer <see cref="TableView"/>. Diese Klasse ist
/// bewusst KEINE Ableitung von <see cref="AbstractListItem"/> mehr — sie kapselt die
/// gesamte für TableView nötige Item-Plumbing (Positionierung, Zeichnen, Sortierung,
/// Disposing) selbst. Alle konkreten Zeilen-Typen leiten von <see cref="RowBackground"/> ab.
/// </summary>
public abstract class RowBackground : IStyleable, IComparable, IHasKeyName, INotifyPropertyChanged, IDisposableExtended {

    #region Fields

    public static readonly Brush GrayBrush = new SolidBrush(Color.FromArgb(80, 200, 200, 200));
    public static readonly Brush GrayBrush2 = new SolidBrush(Color.FromArgb(150, 255, 255, 255));

    private volatile int _isDisposedFlag;
    private Size _untrimmedCanvasSize = Size.Empty;

    #endregion

    #region Constructors

    protected RowBackground(string keyname, ColumnViewCollection? arrangement, string alignsToCaption) {
        KeyName = string.IsNullOrEmpty(keyname) ? Generic.GetUniqueKey() : keyname;
        if (string.IsNullOrEmpty(KeyName)) { Develop.DebugError("Interner Name nicht vergeben."); }
        Enabled = true;
        CanvasPosition = Rectangle.Empty;
        UserDefCompareKey = string.Empty;
        Arrangement = arrangement;
        AlignsToChapter = alignsToCaption;
    }

    #endregion

    #region Events

    public event EventHandler? CompareKeyChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Großschreibung
    /// </summary>
    public string AlignsToChapter {
        get;
        private set {
            value = value.ToUpperInvariant();
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public ColumnViewCollection? Arrangement {
        get;
        set {
            if (field == value) { return; }

            field = value;
            Invalidate_UntrimmedCanvasSize();
            OnPropertyChanged();
        }
    }

    public Rectangle CanvasPosition {
        get;
        set {
            if (field.Equals(value)) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool Enabled {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IgnoreXOffset {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IgnoreYOffset {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public int Indent {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisposed => _isDisposedFlag == 1;

    public string KeyName {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public EventHandler<ContextMenuEventArgs>? LeftClickExecute { get; set; }

    // Es wird mit Zeilenschlüsseln gearbeitet
    public string QuickInfo { get; set; } = string.Empty;

    public bool RemoveLocked { get; set; }

    [DefaultValue(Constants.Win11)]
    public string SheetStyle {
        get;
        set {
            if (IsDisposed) { return; }

            if (field == value) { return; }

            field = value;
            OnPropertyChanged();
        }
    } = Constants.Win11;

    public string UserDefCompareKey {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnCompareKeyChanged();
            OnPropertyChanged();
        }
    }

    public bool Visible {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = true;

    /// <summary>
    /// True: Erst Non Permanent, dann Permanent
    /// False: Alle der Reihe Nach
    /// </summary>
    protected abstract bool DoSpezialOrder { get; }

    #endregion

    #region Methods

    public string CompareKey() {
        if (!string.IsNullOrEmpty(UserDefCompareKey)) {
            if (UserDefCompareKey.Length > 0 && UserDefCompareKey[0] < 32) { Develop.DebugPrint("Sortierung inkorrekt: " + UserDefCompareKey); }

            return UserDefCompareKey;// + Constants.FirstSortChar + Parent?.IndexOf(this).ToString(Constants.Format_Integer6);
        }
        return GetCompareKey();
    }

    public int CompareTo(object? obj) {
        if (obj is RowBackground tobj) {
            return string.Compare(CompareKey(), tobj.CompareKey(), StringComparison.OrdinalIgnoreCase);
        }

        Develop.DebugError("Falscher Objecttyp!");
        return 0;
    }

    /// <summary>
    /// Spezielle Berechnung, die die Ignore-Werte berücksichtigt
    /// </summary>
    public Rectangle ControlPosition(float zoom, float offsetX, float offsetY) {
        if (IgnoreYOffset) { offsetY = 0; }
        if (IgnoreXOffset) { offsetX = 0; }

        return CanvasPosition.CanvasToControl(zoom, offsetX, offsetY, true);
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void Draw(Graphics gr, Rectangle visibleArea, float offsetX, float offsetY, Design controldesign, Design itemdesign, States state, bool drawBorderAndBack, string filterText, bool translate, Design checkboxDesign, float zoom) {
        if (itemdesign == Design.Undefined) { return; }

        var controlPos = ControlPosition(zoom, offsetX, offsetY);
        var p20 = 20.CanvasToControl(zoom) * Indent;
        var controlIndented = new Rectangle(controlPos.X + p20, controlPos.Y, controlPos.Width - p20, controlPos.Height);

        if (checkboxDesign != Design.Undefined) {
            var design = IsClickable()
                ? Skin.DesignOf(checkboxDesign, state)
                : Skin.DesignOf(checkboxDesign, States.Standard_Disabled);
            gr.DrawImageUnscaled(QuickImage.Get(design.Image, 12.CanvasToControl(zoom)), controlIndented.X + 4.CanvasToControl(zoom), controlIndented.Y + 3.CanvasToControl(zoom));
            controlIndented.X += 20.CanvasToControl(zoom);
            controlIndented.Width -= 20.CanvasToControl(zoom);
            if (state.HasFlag(States.Checked)) { state ^= States.Checked; }
        }

        if (state.HasFlag(States.Standard_Disabled)) {
            state &= ~(States.Standard_MouseOver | States.Standard_MousePressed | States.Standard_HasFocus);
        }

        DrawExplicit(gr, visibleArea, controlIndented, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, zoom);
        if (drawBorderAndBack) {
            if (!string.IsNullOrEmpty(filterText) && !FilterMatch(filterText)) {
                var c1 = Skin.Color_Back(controldesign, States.Standard);// Standard als Notlösung, um nicht doppelt checken zu müssen
                c1 = c1.SetAlpha(160);
                var fb = BackgroundFill.GetBrush(c1);
                lock (fb) { gr.FillRectangle(fb, controlIndented); }
            }
        }
    }

    public virtual void Draw_Border(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float xPos, float top, float bottom) => DrawLine(gr, lin, xPos, xPos, top, bottom);

    //      viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, cellInThisTableRow, positionControl, cellInThisTableColumn.DoOpticalTranslation, (Alignment)cellInThisTableColumn.Alignx, _zoom);
    public virtual void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state, Brush? rowcolor) {
        if (viewItem.IsDummyColumn) {
            gr.FillRectangle(GrayBrush, positionControl);
            return;
        }
        var brush = BackgroundFill.GetBrush(viewItem.BackColor_ColumnCell);
        lock (brush) { gr.FillRectangle(brush, positionControl); }
    }

    public virtual void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) { }

    public virtual void Draw_ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) { }

    public virtual void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => DrawLine(gr, lin, left, right, bottom, bottom);

    public virtual void Draw_UpperLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => DrawLine(gr, lin, left, right, bottom, bottom);

    public void DrawLine(Graphics gr, ColumnLineStyle lin, float left, float right, float top, float bottom) {
        if (IsDisposed) { return; }

        try {
            switch (lin) {
                case ColumnLineStyle.Ohne:
                    break;

                case ColumnLineStyle.Dünn:
                    gr.DrawLine(Skin.PenLinieDünn, left, top, right, bottom);
                    break;

                case ColumnLineStyle.Kräftig:
                    gr.DrawLine(Skin.PenLinieKräftig, left, top, right, bottom);
                    break;

                case ColumnLineStyle.Dick:
                    gr.DrawLine(Skin.PenLinieDick, left, top, right, bottom);
                    break;

                case ColumnLineStyle.ShadowRight:
                    var c = Skin.Color_Border(Design.Table_Lines_Thick, States.Standard);
                    gr.DrawLine(Skin.PenLinieKräftig, left, top, right, bottom);
                    var sp1 = BorderDraw.GetPen(Color.FromArgb(80, c.R, c.G, c.B), 1);
                    var sp2 = BorderDraw.GetPen(Color.FromArgb(60, c.R, c.G, c.B), 1);
                    var sp3 = BorderDraw.GetPen(Color.FromArgb(40, c.R, c.G, c.B), 1);
                    var sp4 = BorderDraw.GetPen(Color.FromArgb(20, c.R, c.G, c.B), 1);
                    lock (sp1) { gr.DrawLine(sp1, left + 1, top, right + 1, bottom); }
                    lock (sp2) { gr.DrawLine(sp2, left + 2, top, right + 2, bottom); }
                    lock (sp3) { gr.DrawLine(sp3, left + 3, top, right + 3, bottom); }
                    lock (sp4) { gr.DrawLine(sp4, left + 4, top, right + 4, bottom); }
                    break;

                default:
                    Develop.DebugPrint(lin);
                    break;
            }
        } catch { }
    }

    public virtual bool FilterMatch(string filterText) => KeyName.Contains(filterText, StringComparison.OrdinalIgnoreCase);

    public virtual bool HandleClick(ColumnViewCollection ca, ColumnViewItem clickedColumn, int mouseXinColumn, int mouseYinColumn, float zoom, TableView tableView) => false;

    public abstract int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign);

    public virtual bool IsClickable() => true;

    public void OnCompareKeyChanged() => CompareKeyChanged?.Invoke(this, System.EventArgs.Empty);

    public abstract string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale);

    public Size UntrimmedCanvasSize(Design itemdesign) {
        if (_untrimmedCanvasSize.IsEmpty) {
            _untrimmedCanvasSize = ComputeUntrimmedCanvasSize(itemdesign);
        }
        return _untrimmedCanvasSize;
    }

    internal bool IsVisible(Rectangle controlArea, float zoom, float offsetX, float offsetY) => Visible && ControlPosition(zoom, offsetX, offsetY).IntersectsWith(controlArea);

    protected abstract Size ComputeUntrimmedCanvasSize(Design itemdesign);

    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            Arrangement = null;
            PropertyChanged = null;
            CompareKeyChanged = null;
            LeftClickExecute = null;
        }
    }

    protected virtual void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        if (Arrangement is null) { return; }

        // Indent auf die Spalten-Position anwenden — sonst würde der in Draw
        // berechnete controlIndented-Bereich für die Spalten ignoriert.
        var indentOffset = 20.CanvasToControl(zoom) * Indent;

        for (var du = 0; du < 2; du++) {
            foreach (var viewItem in Arrangement) {
                if (DoSpezialOrder && (viewItem.Permanent && du == 0 || !viewItem.Permanent && du == 1)) { continue; }
                if (viewItem.Column is null && !viewItem.IsDummyColumn) { continue; }

                var left = viewItem.ControlColumnLeft((int)offsetX) + indentOffset;

                if (left > visibleAreaControl.Width) { continue; }
                if (left + viewItem.ControlColumnWidth() < 0) { continue; }

                var area = new Rectangle(left, (int)positionControl.Top, viewItem.ControlColumnWidth(), (int)positionControl.Height);

                var t = viewItem.Column?.DoOpticalTranslation ?? TranslationType.Original_Anzeigen;
                if (!translate) { t = TranslationType.Original_Anzeigen; }

                if (!DoSpezialOrder) {
                    if (!viewItem.Permanent) {
                        area.X = Math.Max(area.X, Arrangement.ControlColumnsPermanentWidth() + indentOffset);
                    }
                }
                Brush? backcolor = null;
                if (this is RowListItem rli && rli.Row is { IsDisposed: false } r && r.Table is { ChangesRowColor: true } tb) {
                    backcolor = r.CheckRow().RowColor;
                }

                gr.SmoothingMode = SmoothingMode.None;
                Draw_ColumnBackGround(gr, viewItem, area, state, backcolor);
                Draw_Border(gr, viewItem, viewItem.LineLeft, area.Left, area.Top, area.Bottom);
                Draw_Border(gr, viewItem, viewItem.LineRight, area.Right, area.Top, area.Bottom);
                Draw_UpperLine(gr, ColumnLineStyle.Ohne, area.Right, area.Left, area.Top);
                Draw_LowerLine(gr, viewItem, ColumnLineStyle.Dünn, area.Right, area.Left, area.Bottom - 1);
                Draw_ColumnContent(gr, viewItem, area, zoom, t, offsetX, offsetY, state);
                Draw_ColumnOverlay(gr, viewItem, area, state);
            }

            if (!DoSpezialOrder) { return; }
        }
    }

    protected virtual string GetCompareKey() => KeyName;

    protected void Invalidate_UntrimmedCanvasSize() => _untrimmedCanvasSize = Size.Empty;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}