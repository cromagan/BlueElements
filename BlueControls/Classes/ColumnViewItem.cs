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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueTable.Enums;
using BlueTable.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using static BlueBasics.Constants;
using static BlueBasics.Converter;

namespace BlueTable;

public sealed class ColumnViewItem : IParseable, IReadableText, IDisposableExtended, IStyleable, INotifyPropertyChanged {

    #region Fields

    public static readonly int AutoFilterSize = 22;
    private Color _backColor_ColumnCell = Color.Transparent;
    private Color _backColor_ColumnHead = Color.Transparent;
    private ColumnItem? _column;
    private int? _contentWidth;
    private int? _drawWidth;
    private Color _fontColor_Caption = Color.Transparent;
    private bool _horizontal;
    private Renderer_Abstract? _renderer;

    private SizeF _tmpCaptionTextSize = SizeF.Empty;

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
        get => Column != null && _backColor_ColumnHead.IsMagentaOrTransparent() ? Column.BackColor : _backColor_ColumnCell;
        set {
            if (_backColor_ColumnCell.ToArgb() == value.ToArgb()) { return; }
            _backColor_ColumnCell = value;
            OnPropertyChanged();
        }
    }

    public Color BackColor_ColumnHead {
        get => Column != null && _backColor_ColumnHead.IsMagentaOrTransparent()
                ? Column.BackColor.MixColor(Color.LightGray, 0.6)
                : _backColor_ColumnHead;

        set {
            if (_backColor_ColumnHead.ToArgb() == value.ToArgb()) { return; }
            _backColor_ColumnHead = value;
            OnPropertyChanged();
        }
    }

    public string Caption => Column?.Caption ?? "[Spalte]";

    public QuickImage? CaptionBitmap {
        get {
            if (Column == null || string.IsNullOrEmpty(Column.CaptionBitmapCode)) { return null; }
            if (field != null) { return field; }

            field = QuickImage.Get(Column.CaptionBitmapCode + "|100");
            return field;
        }

        private set;
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

            if (field == null) {
                if (_column != null) {
                    var baseFont = Font_Head_Default;
                    field = BlueFont.Get(baseFont.FontName, baseFont.Size, baseFont.Bold, baseFont.Italic, baseFont.Underline, baseFont.StrikeOut, FontColor_Caption, Color.Transparent, Color.Transparent);
                } else {
                    field = Font_Head_Default;
                }
            }
            return field;
        }

        private set;
    }

    public BlueFont Font_Head_Default { get => IsDisposed ? BlueFont.DefaultFont : (field ??= Skin.GetBlueFont(SheetStyle, PadStyles.Hervorgehoben)); private set; }

    public BlueFont Font_Numbers {
        get {
            if (IsDisposed) { return BlueFont.DefaultFont; }

            if (field == null) {
                var baseFont = Font_Head_Default;
                field = BlueFont.Get(baseFont.FontName, baseFont.Size, false, false, false, false, Color.Black, Color.White, Color.Transparent);
            }
            return field;
        }

        private set;
    }

    public BlueFont Font_TextInFilter {
        get {
            if (IsDisposed) { return BlueFont.DefaultFont; }
            if (field == null) {
                var baseFont = Font_Head_Default;
                field = BlueFont.Get(baseFont.FontName, baseFont.Size - 2, true, false, false, false, Color.White, Color.Red, Color.Transparent);
            }
            return field;
        }

        private set;
    }

    public Color FontColor_Caption {
        get => Column != null && _fontColor_Caption.IsMagentaOrTransparent() ? Column.ForeColor : _fontColor_Caption;
        set {
            if (_fontColor_Caption.ToArgb() == value.ToArgb()) { return; }
            _fontColor_Caption = value;
            Invalidate_Fonts();
            OnPropertyChanged();
        }
    }

    public bool Horizontal {
        get => Column != null && _horizontal;
        set {
            if (_horizontal = value) { return; }
            _horizontal = value;
            OnPropertyChanged();
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
        get;
        set {
            if (field == value) { return; }

            if (field != null) {
                field.StyleChanged -= _parent_StyleChanged;
            }

            field = value;
            _parent_StyleChanged(null, System.EventArgs.Empty);

            if (field != null) {
                field.StyleChanged += _parent_StyleChanged;
            }
        }
    }

    public bool Permanent {
        get => ViewType == ViewType.PermanentColumn;
        set => ViewType = value ? ViewType.PermanentColumn : ViewType.Column;
    }

    public bool Reduced {
        get;
        set {
            if (field != value) {
                field = value;
                Invalidate_DrawWidth();
            }
        }
    }

    public string Renderer { get; set; }

    public string RendererSettings { get; set; }

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : Win11;

    public int? TmpIfFilterRemoved { get; set; } = null;

    public ViewType ViewType {
        get;
        set {
            if (field != value) {
                field = value;
                Invalidate_DrawWidth();
                OnPropertyChanged();
            }
        }
    } = ViewType.None;

    /// <summary>
    /// Koordinate der Spalte ohne Slider
    /// </summary>
    public int? X { get; set; }

    #endregion

    #region Methods

    public static int CalculateContentWith(ColumnItem? column, Renderer_Abstract renderer) {
        if (column is not { IsDisposed: false }) { return 16; }
        if (column.Table is not { IsDisposed: false } db) { return 16; }
        if (column.FixedColumnWidth > 0) { return column.FixedColumnWidth; }

        var newContentWidth = 16; // Wert muss gesetzt werden, dass er am Ende auch gespeichert wird

        try {
            //  Parallel.ForEach führt ab und zu zu DeadLocks
            foreach (var thisRowItem in db.Row) {
                var wx = renderer.ContentSize(thisRowItem.CellGetString(column), column.DoOpticalTranslation).Width;
                newContentWidth = Math.Max(newContentWidth, wx);
            }
        } catch {
            Develop.AbortAppIfStackOverflow();
            return CalculateContentWith(column, renderer);
        }

        return newContentWidth;
    }

    public Rectangle AutoFilterLocation(float scale, float sliderx, int add) {
        // Manchmal sind filter da, auch ohne Autofilter-Button
        //if (!AutoFilterSymbolPossible) { return Rectangle.Empty; }

        var realHead = RealHead(scale, sliderx);
        var size = (int)(AutoFilterSize * scale);

        return new Rectangle(realHead.Right - size, realHead.Bottom - size + add, size, size);
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

    public void Dispose() =>
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);//GC.SuppressFinalize(this);

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
            _drawWidth = ViewType == ViewType.PermanentColumn
                ? Math.Min(Contentwidth, (int)(Parent.ClientWidth * 0.3))
                : Math.Min(Contentwidth, (int)(Parent.ClientWidth * 0.6));
        }

        _drawWidth = Math.Max((int)_drawWidth, AutoFilterSize); // Mindestens so groß wie der Autofilter;
        return (int)_drawWidth;
    }

    public Renderer_Abstract GetRenderer(string style) {
        if (_renderer != null) { return _renderer; }

        _renderer = TableView.RendererOf(this, style);
        return _renderer;
    }

    public void Invalidate_ContentWidth() {
        _contentWidth = null;
        Invalidate_DrawWidth();
    }

    public void Invalidate_DrawWidth() {
        if (_drawWidth is null) { return; }

        _drawWidth = null;

        Parent?.Invalidate_XOfAllItems();
    }

    public void Invalidate_Head() {
        _tmpCaptionTextSize = SizeF.Empty;
        CaptionBitmap = null;
    }

    public void Invalidate_X() => X = null;

    public ColumnViewItem? NextVisible() => Parent?.NextVisible(this);

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [];
        result.ParseableAdd("Type", ViewType);
        result.ParseableAdd("ColumnName", _column);

        if (_column is not { IsDisposed: false } c || c.DefaultRenderer != Renderer || c.RendererSettings != RendererSettings) {
            result.ParseableAdd("Renderer", Renderer);
            result.ParseableAdd("RendererSettings", RendererSettings);
        }

        if (_column is not { IsDisposed: false } c2 || c2.BackColor.ToArgb != _backColor_ColumnHead.ToArgb || !_backColor_ColumnCell.IsMagentaOrTransparent()) {
            result.ParseableAdd("BackColorColumnHead", _backColor_ColumnHead);
            result.ParseableAdd("BackColorColumnCell", _backColor_ColumnCell);
        }

        if (_column is not { IsDisposed: false } c3 || c3.ForeColor.ToArgb != _fontColor_Caption.ToArgb || !_fontColor_Caption.IsMagentaOrTransparent()) {
            result.ParseableAdd("FontColorCaption", _fontColor_Caption);
        }

        result.ParseableAdd("FontHorizontal", _horizontal);

        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        if (Parent?.Table is not { IsDisposed: false } db) {
            Develop.DebugPrint(ErrorType.Error, "Tabelle unbekannt");
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
                _backColor_ColumnHead = ColorParse(value);
                return true;

            case "backcolorcolumncell":
                _backColor_ColumnCell = ColorParse(value);
                return true;

            case "fontcolorcaption":
                _fontColor_Caption = ColorParse(value);
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
        if (Parent is null) { return Rectangle.Empty; }

        if (!Parent.ShowHead) { return Rectangle.Empty; }

        if (X == null) { Parent.ComputeAllColumnPositions(); }

        if (X == null) { return Rectangle.Empty; }

        if (ViewType == ViewType.PermanentColumn) { sliderx = 0; }

        return new Rectangle((int)(((int)X * scale) - sliderx), 0, (int)(DrawWidth() * scale), (int)(Parent.HeadSize() * scale));
    }

    public Rectangle ReduceButtonLocation(float scale, float sliderx, int moveDown) {
        var r = RealHead(scale, sliderx);
        var size = (int)(18 * scale);
        var pcch = (int)(ColumnCaptionSizeY * scale);

        if (!string.IsNullOrEmpty(CaptionGroup3)) {
            moveDown += pcch * 3;
        } else if (!string.IsNullOrEmpty(CaptionGroup2)) {
            moveDown += pcch * 2;
        } else if (!string.IsNullOrEmpty(CaptionGroup1)) {
            moveDown += pcch;
        }

        return new Rectangle(r.Right - size, r.Top + moveDown, size, size);
    }

    public QuickImage? SymbolForReadableText() => _column?.SymbolForReadableText();

    public override string ToString() => ParseableItems().FinishParseable();

    private void _column_PropertyChanged(object sender, PropertyChangedEventArgs e) => Invalidate_Fonts();

    private void _parent_StyleChanged(object? sender, System.EventArgs e) => Invalidate_Fonts();

    private void Cell_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Column == _column) { Invalidate_ContentWidth(); }
    }

    private void Invalidate_Fonts() {
        Font_Head_Default = null;
        Font_Head_Colored = null;
        Font_Numbers = null;
        Font_TextInFilter = null;
        Invalidate_Head();
        Invalidate_ContentWidth();
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void RegisterEvents() {
        if (_column != null) {
            _column.PropertyChanged += _column_PropertyChanged;

            if (_column.Table is { IsDisposed: false } db) {
                db.Cell.CellValueChanged += Cell_CellValueChanged;
            }
        }
    }

    private void UnRegisterEvents() {
        if (_column != null) {
            _column.PropertyChanged -= _column_PropertyChanged;
            if (_column.Table is { IsDisposed: false } db) {
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