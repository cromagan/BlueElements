// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.ControlStrategies;
using BlueControls.EventArgs;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueControls.TableElements;

/// <summary>
/// Basisklasse aller Zeilen-Elemente einer <see cref="TableView"/>. Diese Klasse ist
/// bewusst KEINE Ableitung von <see cref="ListItem"/> mehr — sie kapselt die
/// gesamte für TableView nötige Item-Plumbing (Positionierung, Zeichnen, Sortierung,
/// Disposing) selbst. Alle konkreten Zeilen-Typen leiten von <see cref="TableElement"/> ab.
/// </summary>
public abstract class TableElement : IStyleable, IComparable, IHasKeyName, IHasQuickInfo, INotifyPropertyChanged, IDisposableExtended {

    #region Fields

    /// <summary>
    /// Canvas-Pixel pro Indent-Stufe. Jede Kapitel-Ebene rückt die Spalten
    /// um diesen Wert nach rechts ein.
    /// </summary>
    public const int IndentWidth = 20;

    public static readonly Brush TableHeadOverlayBrush = new SolidBrush(Color.FromArgb(80, 200, 200, 200));
    private volatile int _isDisposedFlag;
    private Size _untrimmedCanvasSize = Size.Empty;

    #endregion

    #region Constructors

    protected TableElement(string keyname, ColumnViewCollection? arrangement, string alignsToCaption) {
        KeyName = string.IsNullOrEmpty(keyname) ? GetUniqueKey() : keyname;
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

    public event EventHandler? Disposed;

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

    [DefaultValue(Win11)]
    public string SheetStyle {
        get;
        set {
            if (IsDisposed) { return; }

            if (field == value) { return; }

            field = value;
            OnPropertyChanged();
        }
    } = Win11;

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
        if (obj is TableElement tobj) {
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
        var p20 = IndentWidth.CanvasToControl(zoom) * Indent;
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

    /// <summary>
    /// Wird bei einem Doppelklick auf die Zeile aufgerufen. Die konkrete
    /// Zeilen-Implementierung übernimmt alle Prüfungen (Berechtigung,
    /// Spaltenzustand etc.) und fordert — wenn nötig — die <paramref name="tableView"/>
    /// auf, die eigentliche Editier-UI zu starten. Rückgabe <c>true</c>, wenn
    /// der Doppelklick verarbeitet wurde und die TableView keine weitere
    /// Standard-Aktion mehr ausführen soll.
    /// </summary>
    public virtual bool HandleDoubleClick(ColumnViewItem? mouseOverColumn, TableView tableView) => false;

    /// <summary>
    /// Wird bei einem Tastendruck aufgerufen, während diese Zeile unter dem
    /// Cursor liegt. Die konkrete Zeilen-Implementierung übernimmt die
    /// Zell-Aktionen (Ausschneiden, Kopieren, Einfügen, Editieren via F2,
    /// Löschen) — analog zu <see cref="HandleDoubleClick" />, das von der
    /// <see cref="TableView" /> bei einem Doppelklick dispatcht wird.
    /// Die <see cref="TableView" /> behält dagegen die reinen Navigations-
    /// Tasten (Pfeile, Bild-Auf/Ab, Strg+F) selbst.
    /// </summary>
    public virtual void HandleKeyDown(ColumnViewItem? cursorColumn, TableView tableView, KeyEventArgs e) { }

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

    /// <summary>
    /// Gemeinsame Logik für den Start einer Zell-Editierung direkt über
    /// <see cref="TableView.BeginEdit" />. Wird von <see cref="RowTableElement" />
    /// (mit echter Row) und <see cref="NewRowTableElement" /> (mit <c>null</c>)
    /// aus ihrem <see cref="HandleDoubleClick" /> aufgerufen.
    /// Übernimmt Editability-Prüfung, LinkedCell-Auflösung, Bestimmung des
    /// Edit-Typs, Positions-/Größen-Berechnung und startet das Edit über
    /// <see cref="TableView.BeginEdit" />. Die Auswahllisten-Ermittlung für
    /// Strategien mit NeedsSuggestions übernimmt die <see cref="TableView" />
    /// dort einheitlich.
    /// </summary>
    /// <param name="tableView"></param>
    /// <param name="viewItem"></param>
    /// <param name="row">Die Row, in der editiert wird — <c>null</c> für
    /// "neue Zeile".</param>
    /// <param name="chunkValue"></param>
    /// <param name="rowContainer">Das sichtbare Item (RowListItem oder
    /// NewRowTableElement), das für die Positionsberechnung herangezogen wird.</param>
    /// <returns><c>true</c>, wenn ein Edit gestartet wurde (oder die Zelle
    /// bewusst nicht editierbar ist, der Klick also verarbeitet wurde).</returns>
    protected internal bool BeginCellEdit(TableView tableView, ColumnViewItem? viewItem, TableElement rowContainer, RowItem? row, string? chunkValue) {
        var notEditableReason = tableView.IsCellEditableInView(viewItem, rowContainer as RowTableElement, chunkValue, true);
        if (notEditableReason is { Length: > 0 } f) {
            TableView.NotEditableInfo(f);
            return true;
        }

        if (viewItem?.Column is not { IsDisposed: false } originalColumn) {
            TableView.NotEditableInfo("Keine Spalte angeklickt.");
            return true;
        }

        // LinkedCell-Auflösung nur bei echter Row und Relations-Spalte.
        // Position, Style und Commit basieren weiterhin auf der Original-Spalte;
        // nur der ControlStrategy und das Dropdown werden über die Ziel-Spalte bestimmt.
        var contentHolderCellColumn = originalColumn;
        var contentHolderCellRow = row;
        if (contentHolderCellRow is { IsDisposed: false } cr && originalColumn.RelationType == RelationType.CellValues) {
            (contentHolderCellColumn, contentHolderCellRow, _, _) = cr.LinkedCellData(contentHolderCellColumn, true, true);
        }

        if (contentHolderCellColumn is not { IsDisposed: false }) {
            TableView.NotEditableInfo("Keine Spalte angeklickt.");
            return true;
        }

        var dia = contentHolderCellColumn.ControlStrategy;
        var strategy = ControlStrategy.Cached(dia);

        if (strategy.NotEditableReason is { Length: > 0 } strategyReason) {
            TableView.NotEditableInfo(strategyReason);
            return true;
        }

        if (strategy.SupportsSuggestions && !strategy.SupportsTextEdit) {
            // Dropdown benötigt ein echtes RowListItem als Container.
            if (rowContainer is not RowTableElement rli) { return true; }

            // Bei LinkedCell-Spalten (column != originalColumn) kann für
            // eine neue Zeile kein Dropdown erstellt werden.
            if (contentHolderCellColumn != originalColumn && contentHolderCellRow is null) {
                TableView.NotEditableInfo("Bei Zellverweisen kann keine neue Zeile erstellt werden.");
                return true;
            }

            contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);

            // Aktuell ausgewählte Werte der Zelle als Startwert für das
            // Dropdown. Die Auswahlliste ermittelt TableView über
            // NeedsSuggestions einheitlich in BeginEdit.
            var ddValue = contentHolderCellRow is { IsDisposed: false } cr2
                ? string.Join('\r', cr2.CellGetList(contentHolderCellColumn))
                : string.Empty;

            // Position / Größe der Zelle berechnen, damit die
            // FlexiListBox an der richtigen Stelle
            // erscheint. Die tatsächliche Größe (größer als die Zelle)
            // wird in TableView.BeginEdit berechnet.
            var ddZoom = tableView.Zoom;
            var ddOffsetX = tableView.OffsetX;
            var ddOffsetY = tableView.OffsetY;
            var ddControlPos = rowContainer.ControlPosition(ddZoom, ddOffsetX, ddOffsetY);
            var ddIndentOffset = IndentWidth.CanvasToControl(ddZoom) * rowContainer.Indent;
            var ddLocation = new Point(viewItem.ControlColumnLeft(ddOffsetX) + ddIndentOffset, ddControlPos.Y);
            var ddSize = new Size(viewItem.ControlColumnWidth(), ddControlPos.Height);

            tableView.BeginEdit(
                dia,
                new Rectangle(ddLocation, ddSize),
                ddValue,
                v => ApplyCellValue(tableView, viewItem, rli, v),
                originalColumn,
                contentHolderCellColumn,
                contentHolderCellRow,
                null,
                new CellExtEventArgs(viewItem, rli),
                Skin.GetBlueFont(SheetStyle, PadStyles.Standard));
            return true;
        }

        contentHolderCellColumn.AddSystemInfo("Edit in Table", UserName);

        // Position / Size — entspricht der früheren GetEditBounds + ConfigureAndActivateCellEdit-Logik.
        var zoom = tableView.Zoom;
        var offsetX = tableView.OffsetX;
        var offsetY = tableView.OffsetY;

        var controlPos = rowContainer.ControlPosition(zoom, offsetX, offsetY);
        var cellText = row?.CellGetString(originalColumn) ?? string.Empty;

        // Spalte erlaubt Mehrzeiler, wird aber einzeilig angezeigt ->
        // Edit-Feld vergrößern, damit mehrzeilig getippt werden kann.
        if (originalColumn.MultiLine && controlPos.Height <= 30) {
            var lineCount = Math.Clamp(cellText.CountChar('\r') + 1, 3, 6);
            controlPos.Height = controlPos.Height * lineCount;
        }

        var indentOffset = IndentWidth.CanvasToControl(zoom) * rowContainer.Indent;
        var location = new Point(viewItem.ControlColumnLeft(offsetX) + indentOffset, controlPos.Y);
        var size = new Size(viewItem.ControlColumnWidth(), controlPos.Height);

        // Für ComboBox / Suggestions die Items vorab besorgen. Fallbacks
        // (z. B. Auswahl ohne Items) macht TableView.BeginEdit zentral.
        List<ListItem>? items = null;
        var renderer = viewItem.GetRenderer(rowContainer.SheetStyle);

        if (strategy.SupportsSuggestions) {
            items = ItemsOf(originalColumn, contentHolderCellRow, 1000, renderer);
        }

        tableView.BeginEdit(
            dia,
            new Rectangle(location, size),
            cellText,
            v => ApplyCellValue(tableView, viewItem, rowContainer as RowTableElement, v),
            originalColumn,
            null,
            null,
            items,
            null,
            Skin.GetBlueFont(SheetStyle, PadStyles.Standard));
        return true;
    }

    protected abstract Size ComputeUntrimmedCanvasSize(Design itemdesign);

    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }
        OnDisposed();

        if (disposing) {
            Arrangement = null;
            PropertyChanged = null;
            CompareKeyChanged = null;
            LeftClickExecute = null;
            Disposed = null;
        }
    }

    protected virtual void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        if (Arrangement is null) { return; }

        // Indent auf die Spalten-Position anwenden — sonst würde der in Draw
        // berechnete controlIndented-Bereich für die Spalten ignoriert.
        var indentOffset = IndentWidth.CanvasToControl(zoom) * Indent;

        for (var du = 0; du < 2; du++) {
            foreach (var viewItem in Arrangement.RenderingItems) {
                if (DoSpezialOrder && (viewItem.Permanent && du == 0 || !viewItem.Permanent && du == 1)) { continue; }
                if (!viewItem.IsOk()) { continue; }

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
                if (this is RowTableElement rli && rli.Row is { IsDisposed: false } r && r.Table is { ChangesRowColor: true }) {
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

            if (!DoSpezialOrder) { break; }
        }

        // Bereich VOR dem Indent mit der Control-Backcolor ausfüllen — NACH
        // dem Zeichnen der Spalten, damit nicht-permanente Spalten, die beim
        // Scrollen in den Indent-Bereich rutschen, verdeckt werden.
        // Die Indent-Fläche ist an den permanenten Spalten ausgerichtet und
        // scrollt NICHT mit dem Inhalt. Daher muss der effektive offsetX
        // abgezogen werden, damit die Füllung immer am fixen linken Rand liegt.
        if (indentOffset > 0) {
            var effectiveOffsetX = IgnoreXOffset ? 0 : (int)offsetX;
            var fillX = positionControl.X - indentOffset - effectiveOffsetX;
            gr.FillRectangle(new SolidBrush(Skin.Color_Back(Design.Table_And_Pad, States.Standard)), new RectangleF(fillX, positionControl.Y, indentOffset, positionControl.Height));
        }

        // Bereich RECHTS neben den Spalten mit der Control-Backcolor füllen
        // — analog zum Indent-Fill links. HeadItems (Indent=0) erhalten in
        // CalculateAllViewItems_CalculateYPosition keinen Indent-Zuschlag
        // für die CanvasPosition-Breite und sind dadurch schmaler als
        // eingerückte Body-Zeilen. Der Spalt auf der rechten Seite wird
        // hier gefüllt.
        if (positionControl.Right < visibleAreaControl.Right) {
            gr.FillRectangle(new SolidBrush(Skin.Color_Back(Design.Table_And_Pad, States.Standard)), new RectangleF(positionControl.Right, positionControl.Y, visibleAreaControl.Right - positionControl.Right, positionControl.Height));
        }
    }

    protected virtual string GetCompareKey() => KeyName;

    protected void Invalidate_UntrimmedCanvasSize() => _untrimmedCanvasSize = Size.Empty;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Schreibt den übergebenen Wert als neuen Zellinhalt — inkl. Formatprüfung
    /// und Rückmeldung bei Fehler (über <see cref="TableView.UserEdited" />).
    /// Commit-Callback aus <see cref="BeginCellEdit" />, der an
    /// <see cref="TableView.BeginEdit" /> übergeben wird.
    /// </summary>
    private void ApplyCellValue(TableView tableView, ColumnViewItem? column, RowTableElement? row, string value) => TableView.NotEditableInfo(TableView.UserEdited(tableView, value, column, row, true));

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    #endregion
}