// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

public sealed class ColumnViewItem : IParseable, IReadableText, IDisposableExtended, IStyleable {

    #region Fields

    public static readonly int AutoFilterSize = 22;
    private QuickImage? _captionBitmap;
    private ColumnItem? _column;

    private int? _drawWidth;

    private ColumnViewCollection? _parent;

    private bool _reduced;

    private Renderer_Abstract? _renderer;

    private SizeF _tmpCaptionTextSize = SizeF.Empty;

    private ViewType _viewType = ViewType.None;

    #endregion

    #region Constructors

    public ColumnViewItem(ColumnItem column, ViewType type, ColumnViewCollection parent) : this(parent) {
        Column = column;
        ViewType = type;
        Renderer = string.Empty;
    }

    public ColumnViewItem(ColumnViewCollection parent, string toParse) : this(parent) => this.Parse(toParse);

    private ColumnViewItem(ColumnViewCollection parent) : base() {
        Parent = parent;
        ViewType = ViewType.None;
        Column = null;
        X = null;
        //AutoFilterLocation = Rectangle.Empty;
        //ReduceLocation = Rectangle.Empty;
        Invalidate_DrawWidth();
        Reduced = false;
        Renderer = string.Empty;
        RendererSettings = string.Empty;
    }

    #endregion

    #region Properties

    public bool AutoFilterSymbolPossible {
        get {
            return Column?.AutoFilterSymbolPossible() ?? false;
        }
    }

    public Color BackColor {
        get {
            return Column?.BackColor ?? Color.White;
        }
    }

    public string Caption {
        get {
            return Column?.Caption ?? "[Spalte]";
        }
    }

    public QuickImage? CaptionBitmap {
        get {
            if (Column == null || string.IsNullOrEmpty(Column.CaptionBitmapCode)) { return null; }
            if (_captionBitmap != null) { return _captionBitmap; }

            _captionBitmap = QuickImage.Get(Column.CaptionBitmapCode + "|100");
            return _captionBitmap;
        }
    }

    public string CaptionGroup1 {
        get {
            return Column?.CaptionGroup1 ?? string.Empty;
        }
    }

    public string CaptionGroup2 {
        get {
            return Column?.CaptionGroup2 ?? string.Empty;
        }
    }

    public string CaptionGroup3 {
        get {
            return Column?.CaptionGroup3 ?? string.Empty;
        }
    }

    public ColumnItem? Column {
        get => _column;
        private set {
            if (_column == value) { return; }

            UnRegisterEvents();
            _column = value;
            Invalidate_All();
            RegisterEvents();
        }
    }

    public int? Contentwidth { get; private set; }
    public BlueFont Font_Head_Colored { get; private set; } = BlueFont.DefaultFont;
    public BlueFont Font_Head_Default { get; internal set; } = BlueFont.DefaultFont;
    public BlueFont Font_Numbers { get; private set; } = BlueFont.DefaultFont;
    public BlueFont Font_TextInFilter { get; private set; } = BlueFont.DefaultFont;
    public bool IsDisposed { get; private set; }

    public ColumnLineStyle LineLeft {
        get {
            return Column?.LineLeft ?? ColumnLineStyle.Dünn;
        }
    }

    public ColumnLineStyle LineRight {
        get {
            return Column?.LineRight ?? ColumnLineStyle.Ohne;
        }
    }

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

    //public Rectangle ReduceLocation { get; set; }
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
            }
        }
    }

    /// <summary>
    /// Koordinate der Spalte ohne Slider
    /// </summary>
    public int? X { get; set; }

    #endregion

    #region Methods

    public Rectangle AutoFilterLocation(float scale, float sliderx) {
        if (!AutoFilterSymbolPossible) { return Rectangle.Empty; }

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
                ? Math.Min(CalculateColumnContentWidth(), (int)(Parent.ClientWidth * 0.3))
                : Math.Min(CalculateColumnContentWidth(), (int)(Parent.ClientWidth * 0.6));
        }

        _drawWidth = Math.Max((int)_drawWidth, AutoFilterSize); // Mindestens so groß wie der Autofilter;
        return (int)_drawWidth;
    }

    public Renderer_Abstract GetRenderer(string style) {
        if (_renderer != null) { return _renderer; }

        _renderer = Table.RendererOf(this, style);
        return _renderer;
    }

    public void Invalidate_DrawWidth() {
        _drawWidth = null;

        if (_parent != null) {
            _parent.Invalidate_XOfAllItems();
        }
    }

    public void Invalidate_Head() {
        _tmpCaptionTextSize = SizeF.Empty;
        _captionBitmap = null;
    }

    public void Invalidate_X() {
        X = null;
    }

    public ColumnViewItem? NextVisible() => Parent?.NextVisible(this);

    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [];
        result.ParseableAdd("Type", ViewType);
        result.ParseableAdd("ColumnName", _column);

        if (_column is not { } c || c.DefaultRenderer != Renderer || c.RendererSettings != RendererSettings) {
            result.ParseableAdd("Renderer", Renderer);
            result.ParseableAdd("RendererSettings", RendererSettings);
        }

        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        if (Parent?.Database is not { IsDisposed: false } db) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank unbekannt");
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

    private void _column_PropertyChanged(object sender, System.EventArgs e) {
        Invalidate_All();
    }

    private void _parent_StyleChanged(object? sender, System.EventArgs e) {
        Invalidate_All();
    }

    private int CalculateColumnContentWidth() {
        if (_column is not { IsDisposed: false }) { return 16; }
        if (_column.Database is not { IsDisposed: false } db) { return 16; }
        if (_column.FixedColumnWidth > 0) { return _column.FixedColumnWidth; }
        if (Contentwidth is { } v) { return v; }

        _column.RefreshColumnsData();

        var newContentWidth = 16; // Wert muss gesetzt werden, dass er am Ende auch gespeichert wird

        var renderer = GetRenderer(SheetStyle);

        try {
            //  Parallel.ForEach führt ab und zu zu DeadLocks
            foreach (var thisRowItem in db.Row) {
                var wx = renderer.ContentSize(thisRowItem.CellGetString(_column), _column.DoOpticalTranslation).Width;
                newContentWidth = Math.Max(newContentWidth, wx);
            }
        } catch {
            Develop.CheckStackForOverflow();
            return CalculateColumnContentWidth();
        }

        Contentwidth = newContentWidth;
        return newContentWidth;
    }

    private void Cell_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Column == _column) { Invalidate_DrawWidth(); }
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                UnRegisterEvents();
                Parent = null;
                _column = null;
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void Invalidate_All() {
        Invalidate_Head();
        Invalidate_DrawWidth();
        Font_Head_Default = Skin.GetBlueFont(SheetStyle, PadStyles.Hervorgehoben);
        Font_Numbers = BlueFont.Get(Font_Head_Default.FontName, Font_Head_Default.Size, false, false, false, false, true, Color.Black, Color.White, false, false, false, Color.Transparent);

        Font_TextInFilter = BlueFont.Get(Font_Head_Default.FontName, Font_Head_Default.Size - 2, true, false, false, false, true, Color.White, Color.Red, false, false, false, Color.Transparent);

        if (Column != null) {
            Font_Head_Colored = BlueFont.Get(Font_Head_Default.FontName, Font_Head_Default.Size, Font_Head_Default.Bold, Font_Head_Default.Italic, Font_Head_Default.Underline, Font_Head_Default.StrikeOut, false, Column.ForeColor, Color.Transparent, Font_Head_Default.Kapitälchen, Font_Head_Default.OnlyLower, Font_Head_Default.OnlyLower, Color.Transparent);
        } else {
            Font_Head_Colored = Font_Head_Default;
        }
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
            if (_column.Database is { IsDisposed: false } db) {
                db.Cell.CellValueChanged += Cell_CellValueChanged;
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