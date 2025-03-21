// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using static BlueBasics.Constants;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class ColumnViewItem : IParseable, IReadableText, IDisposableExtended, IStyleable, IPropertyChangedFeedback {

    #region Fields

    public static readonly int AutoFilterSize = 22;
    private Color _backColor_ColumnCell = Color.Transparent;
    private Color _backColor_ColumnHead = Color.Transparent;
    private QuickImage? _captionBitmap;
    private ColumnItem? _column;
    private int? _contentWidth;
    private int? _drawWidth;
    private BlueFont? _font_Head_Colored;
    private BlueFont? _font_Head_Default;
    private BlueFont? _font_Numbers;
    private BlueFont? _font_TextInFilter;
    private Color _fontColor_Caption = Color.Transparent;
    private bool _horizontal;
    private ColumnViewCollection? _parent;

    private bool _reduced;

    private Renderer_Abstract? _renderer;

    private SizeF _tmpCaptionTextSize = SizeF.Empty;

    private ViewType _viewType = ViewType.None;

    #endregion

    #region Constructors

    public ColumnViewItem(ColumnItem column, ColumnViewCollection parent) : this(parent) {
        Column = column;
        ViewType = ViewType.Column;
        Renderer = string.Empty;
    }

    public ColumnViewItem(ColumnViewCollection parent, string toParse) : this(parent) => this.Parse(toParse);

    private ColumnViewItem(ColumnViewCollection parent) : base() {
        Parent = parent;
        ViewType = ViewType.None;
        Column = null;
        //AutoFilterLocation = Rectangle.Empty;
        //ReduceLocation = Rectangle.Empty;
        Invalidate_ContentWidth();
        Invalidate_X();
        Reduced = false;
        Renderer = string.Empty;
        RendererSettings = string.Empty;
    }

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public bool AutoFilterSymbolPossible => Column?.AutoFilterSymbolPossible() ?? false;

    public Color BackColor_ColumnCell {
        get {
            if (Column != null && _backColor_ColumnHead.IsMagentaOrTransparent()) {
                return Column.BackColor;
            }

            return _backColor_ColumnCell;
        }
        set {
            if (_backColor_ColumnCell.ToArgb() == value.ToArgb()) { return; }
            _backColor_ColumnCell = value;
            OnPropertyChanged("BackColor_ColumnCell");
        }
    }

    public Color BackColor_ColumnHead {
        get {
            if (Column != null && _backColor_ColumnHead.IsMagentaOrTransparent()) {
                return Column.BackColor.MixColor(Color.LightGray, 0.6);
            }

            return _backColor_ColumnHead;
        }

        set {
            if (_backColor_ColumnHead.ToArgb() == value.ToArgb()) { return; }
            _backColor_ColumnHead = value;
            OnPropertyChanged("BackColor_ColumnHead");
        }
    }

    public string Caption => Column?.Caption ?? "[Spalte]";

    public QuickImage? CaptionBitmap {
        get {
            if (Column == null || string.IsNullOrEmpty(Column.CaptionBitmapCode)) { return null; }
            if (_captionBitmap != null) { return _captionBitmap; }

            _captionBitmap = QuickImage.Get(Column.CaptionBitmapCode + "|100");
            return _captionBitmap;
        }
    }

    public string CaptionGroup1 => Column?.CaptionGroup1 ?? string.Empty;

    public string CaptionGroup2 => Column?.CaptionGroup2 ?? string.Empty;

    public string CaptionGroup3 => Column?.CaptionGroup3 ?? string.Empty;

    public ColumnItem? Column {
        get => _column;
        private set {
            if (_column == value) { return; }

            UnRegisterEvents();
            _column = value;
            Invalidate_Fonts();
            RegisterEvents();
        }
    }

    public int Contentwidth {
        get {
            if (_contentWidth is { } v) { return v; }

            _contentWidth = CalculateContentWith(_column, GetRenderer(SheetStyle));
            return (int)_contentWidth;
        }
    }

    public BlueFont Font_Head_Colored {
        get {
            if (IsDisposed) { return BlueFont.DefaultFont; }

            if (_font_Head_Colored == null) {
                if (_column != null) {
                    var baseFont = Font_Head_Default;
                    _font_Head_Colored = BlueFont.Get(baseFont.FontName, baseFont.Size, baseFont.Bold, baseFont.Italic, baseFont.Underline, baseFont.StrikeOut, false, FontColor_Caption, Color.Transparent, baseFont.Kapitälchen, baseFont.OnlyLower, baseFont.OnlyLower, Color.Transparent);
                } else {
                    _font_Head_Colored = Font_Head_Default;
                }
            }
            return _font_Head_Colored;
        }
    }

    public BlueFont Font_Head_Default {
        get {
            if (IsDisposed) { return BlueFont.DefaultFont; }

            return _font_Head_Default ??= Skin.GetBlueFont(SheetStyle, PadStyles.Hervorgehoben);
        }
    }

    public BlueFont Font_Numbers {
        get {
            if (IsDisposed) { return BlueFont.DefaultFont; }

            if (_font_Numbers == null) {
                var baseFont = Font_Head_Default;
                _font_Numbers = BlueFont.Get(baseFont.FontName, baseFont.Size, false, false, false, false, true, Color.Black, Color.White, false, false, false, Color.Transparent);
            }
            return _font_Numbers;
        }
    }

    public BlueFont Font_TextInFilter {
        get {
            if (IsDisposed) { return BlueFont.DefaultFont; }
            if (_font_TextInFilter == null) {
                var baseFont = Font_Head_Default;
                _font_TextInFilter = BlueFont.Get(baseFont.FontName, baseFont.Size - 2, true, false, false, false, true, Color.White, Color.Red, false, false, false, Color.Transparent);
            }
            return _font_TextInFilter;
        }
    }

    public Color FontColor_Caption {
        get {
            if (Column != null && _fontColor_Caption.IsMagentaOrTransparent()) {
                return Column.ForeColor;
            }

            return _fontColor_Caption;
        }
        set {
            if (_fontColor_Caption.ToArgb() == value.ToArgb()) { return; }
            _fontColor_Caption = value;
            Invalidate_Fonts();
            OnPropertyChanged("FontColor_Caption");
        }
    }

    public bool Horizontal {
        get {
            if (Column == null) {
                return false;
            }

            return _horizontal;
        }
        set {
            if (_horizontal = value) { return; }
            _horizontal = value;
            OnPropertyChanged("Horizontal");
        }
    }

    public bool IsDisposed { get; private set; }

    public ColumnLineStyle LineLeft => Column?.LineStyleLeft ?? ColumnLineStyle.Dünn;

    public ColumnLineStyle LineRight => Column?.LineStyleRight ?? ColumnLineStyle.Ohne;

    ///// <summary>
    ///// Koordinate der Spalte mit einbrechneten Slider
    ///// </summary>
    //public int? X_WithSlider { get; set; }
    public ColumnViewCollection? Parent {
        get => _parent;
        set {
            if (_parent == value) { return; }

            if (_parent != null) {
                _parent.StyleChanged -= _parent_StyleChanged;
            }

            _parent = value;
            _parent_StyleChanged(null, System.EventArgs.Empty);

            if (_parent != null) {
                _parent.StyleChanged += _parent_StyleChanged;
            }
        }
    }

    public bool Permanent {
        get => _viewType == ViewType.PermanentColumn;
        set => ViewType = value ? ViewType.PermanentColumn : ViewType.Column;
    }

    public bool Reduced {
        get => _reduced;
        set {
            if (_reduced != value) {
                _reduced = value;
                Invalidate_DrawWidth();
            }
        }
    }

    public string Renderer { get; set; }

    public string RendererSettings { get; set; }

    public string SheetStyle {
        get {
            if (_parent is IStyleable ist) { return ist.SheetStyle; }
            return Win11;
        }
    }

    public int? TmpIfFilterRemoved { get; set; } = null;

    public ViewType ViewType {
        get => _viewType;
        set {
            if (_viewType != value) {
                _viewType = value;
                Invalidate_DrawWidth();
                OnPropertyChanged("ViewType");
            }
        }
    }

    /// <summary>
    /// Koordinate der Spalte ohne Slider
    /// </summary>
    public int? X { get; set; }

    #endregion

    #region Methods

    public static int CalculateContentWith(ColumnItem? column, Renderer_Abstract renderer) {
        if (column is not { IsDisposed: false }) { return 16; }
        if (column.Database is not { IsDisposed: false } db) { return 16; }
        if (column.FixedColumnWidth > 0) { return column.FixedColumnWidth; }

        var newContentWidth = 16; // Wert muss gesetzt werden, dass er am Ende auch gespeichert wird

        try {
            //  Parallel.ForEach führt ab und zu zu DeadLocks
            foreach (var thisRowItem in db.Row) {
                var wx = renderer.ContentSize(thisRowItem.CellGetString(column), column.DoOpticalTranslation).Width;
                newContentWidth = Math.Max(newContentWidth, wx);
            }
        } catch {
            Develop.CheckStackForOverflow();
            return CalculateContentWith(column, renderer);
        }

        return newContentWidth;
    }

    public Rectangle AutoFilterLocation(float scale, float sliderx) {
        // Manchmal sind filter da, auch ohne Autofilter-Button
        //if (!AutoFilterSymbolPossible) { return Rectangle.Empty; }

        var r = RealHead(scale, sliderx);
        var size = (int)(AutoFilterSize * scale);

        return new Rectangle(r.Right - size, r.Bottom - size, size, size);
    }

    public SizeF ColumnCaptionText_Size() {
        if (Column is not { IsDisposed: false } c) { return new SizeF(16, 16); }

        if (!_tmpCaptionTextSize.IsEmpty) { return _tmpCaptionTextSize; }

        _tmpCaptionTextSize = Font_Head_Default.MeasureString(c.Caption.Replace("\r", "\r\n"));
        return _tmpCaptionTextSize;
    }

    public SizeF ColumnHead_Size() {
        if (Column is not { IsDisposed: false } c) { return new SizeF(16, 16); }

        var ccts = ColumnCaptionText_Size();

        var wi = ccts.Height + 4;
        var he = ccts.Width + 3;

        if (!IsDisposed) {
            if (!string.IsNullOrEmpty(c.CaptionGroup3)) {
                he += ColumnCaptionSizeY * 3;
            } else if (!string.IsNullOrEmpty(c.CaptionGroup2)) {
                he += ColumnCaptionSizeY * 2;
            } else if (!string.IsNullOrEmpty(c.CaptionGroup1)) {
                he += ColumnCaptionSizeY;
            }
        }

        return new SizeF(wi, he);
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        //GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                UnRegisterEvents();
                Parent = null;
                _column = null;
                Invalidate_Fonts();
            }
            IsDisposed = true;
        }
    }

    public int DrawWidth() {
        // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!

        if (Parent == null) { return 16; }

        if (_drawWidth is { } v) { return v; }

        if (_column is not { IsDisposed: false }) {
            _drawWidth = 16;
            return (int)_drawWidth;
        }

        if (Parent.Count == 1) {
            _drawWidth = Parent.ClientWidth;
            return (int)_drawWidth;
        }

        if (Reduced) {
            _drawWidth = 16;
        } else {
            _drawWidth = _viewType == ViewType.PermanentColumn
                ? Math.Min(Contentwidth, (int)(Parent.ClientWidth * 0.3))
                : Math.Min(Contentwidth, (int)(Parent.ClientWidth * 0.6));
        }

        _drawWidth = Math.Max((int)_drawWidth, AutoFilterSize); // Mindestens so groß wie der Autofilter;
        return (int)_drawWidth;
    }

    public Renderer_Abstract GetRenderer(string style) {
        if (_renderer != null) { return _renderer; }

        _renderer = Table.RendererOf(this, style);
        return _renderer;
    }

    public void Invalidate_ContentWidth() {
        _contentWidth = null;
        Invalidate_DrawWidth();
    }

    public void Invalidate_DrawWidth() {
        if (_drawWidth is null) { return; }

        _drawWidth = null;

        if (_parent != null) {
            _parent.Invalidate_XOfAllItems();
        }
    }

    public void Invalidate_Head() {
        _tmpCaptionTextSize = SizeF.Empty;
        _captionBitmap = null;
    }

    public void Invalidate_X() => X = null;

    public ColumnViewItem? NextVisible() => Parent?.NextVisible(this);

    public void OnPropertyChanged(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [];
        result.ParseableAdd("Type", ViewType);
        result.ParseableAdd("ColumnName", _column);

        if (_column is not { } c || c.DefaultRenderer != Renderer || c.RendererSettings != RendererSettings) {
            result.ParseableAdd("Renderer", Renderer);
            result.ParseableAdd("RendererSettings", RendererSettings);
        }

        if (_column is not { } c2 || c2.BackColor.ToArgb != _backColor_ColumnHead.ToArgb || !_backColor_ColumnCell.IsMagentaOrTransparent()) {
            result.ParseableAdd("BackColorColumnHead", _backColor_ColumnHead);
            result.ParseableAdd("BackColorColumnCell", _backColor_ColumnCell);
        }

        if (_column is not { } c3 || c3.ForeColor.ToArgb != _fontColor_Caption.ToArgb || !_fontColor_Caption.IsMagentaOrTransparent()) {
            result.ParseableAdd("FontColorCaption", _fontColor_Caption);
        }

        result.ParseableAdd("FontHorizontal", _horizontal);

        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        if (Parent?.Database is not { IsDisposed: false } db) {
            Develop.DebugPrint(ErrorType.Error, "Datenbank unbekannt");
            return false;
        }

        switch (key) {
            case "column":
            case "columnname":// ColumnName wichtg, wegen CopyLayout
                Column = db.Column[value];
                return true;

            case "columnkey":
                return true;

            case "type":
                ViewType = (ViewType)IntParse(value);
                if (_column != null && ViewType == ViewType.None) { ViewType = ViewType.Column; }
                return true;

            //case "edittype":
            //    //    _editType = (EditTypeFormula)IntParse(value);
            //    return true;

            case "renderer":
                Renderer = value;
                return true;

            case "renderersettings":
                RendererSettings = value.FromNonCritical();
                return true;

            case "backcolorcolumnhead":
                _backColor_ColumnHead = value.FromHtmlCode();
                return true;

            case "backcolorcolumncell":
                _backColor_ColumnCell = value.FromHtmlCode();
                return true;

            case "fontcolorcaption":
                _fontColor_Caption = value.FromHtmlCode();
                return true;

            case "fonthorizontal":
                _horizontal = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public ColumnViewItem? PreviewsVisible() => Parent?.PreviousVisible(this);

    public string ReadableText() => _column?.ReadableText() ?? "?";

    public Rectangle RealHead(float scale, float sliderx) {
        if (_parent is null) { return Rectangle.Empty; }

        if (!_parent.ShowHead) { return Rectangle.Empty; }

        if (X == null) { _parent.ComputeAllColumnPositions(); }

        if (X == null) { return Rectangle.Empty; }

        if (_viewType == ViewType.PermanentColumn) { sliderx = 0; }

        return new Rectangle((int)(((int)X * scale) - sliderx), 0, (int)(DrawWidth() * scale), (int)(_parent.HeadSize() * scale));
    }

    public Rectangle ReduceButtonLocation(float scale, float sliderx) {
        var r = RealHead(scale, sliderx);
        var size = (int)(18 * scale);

        return new Rectangle(r.Right - size, r.Top, size, size);
    }

    public QuickImage? SymbolForReadableText() => _column?.SymbolForReadableText();

    public override string ToString() => ParseableItems().FinishParseable();

    private void _column_PropertyChanged(object sender, System.EventArgs e) => Invalidate_Fonts();

    private void _parent_StyleChanged(object? sender, System.EventArgs e) => Invalidate_Fonts();

    private void Cell_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Column == _column) { Invalidate_ContentWidth(); }
    }

    private void Invalidate_Fonts() {
        _font_Head_Default = null;
        _font_Head_Colored = null;
        _font_Numbers = null;
        _font_TextInFilter = null;
        Invalidate_Head();
        Invalidate_ContentWidth();
    }

    private void RegisterEvents() {
        if (_column != null) {
            _column.PropertyChanged += _column_PropertyChanged;

            if (_column.Database is { IsDisposed: false } db) {
                db.Cell.CellValueChanged += Cell_CellValueChanged;
            }
        }
    }

    private void UnRegisterEvents() {
        if (_column != null) {
            _column.PropertyChanged -= _column_PropertyChanged;
            if (_column.Database is { } db) {
                db.Cell.CellValueChanged -= Cell_CellValueChanged;
            }
        }
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ColumnViewItem()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}