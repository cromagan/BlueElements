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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
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

    private Color _backColor_ColumnCell = Color.Transparent;

    private Color _backColor_ColumnHead = Color.Transparent;

    private int? _canvasContentWidth;

    private ColumnItem? _column;

    /// <summary>
    /// Koordinaten OHNE ShiftX, aber skaliert auf Controlebene
    /// </summary>
    private int? _controlColumnLeft;

    /// <summary>
    /// Control heißt, dass die Kooridanten sich auf die Controllebene beziehen und nicht auf den Canvas
    /// </summary>
    private int? _controlColumnWidth;

    private Color _fontColor_Caption = Color.Transparent;

    private bool _horizontal;

    private Renderer_Abstract? _renderer;

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
        Invalidate_CanvasContentWidth();
        Invalidate_ControlColumnLeft();
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

    public int CanvasContentWidth {
        get {
            if (_canvasContentWidth is { } v) { return v; }

            _canvasContentWidth = CalculateCanvasContentWith(_column, GetRenderer(SheetStyle));
            return (int)_canvasContentWidth;
        }
    }

    public string Caption => Column?.Caption ?? "[Spalte]";

    public string CaptionGroup1 => Column?.CaptionGroup1 ?? string.Empty;

    public string CaptionGroup2 => Column?.CaptionGroup2 ?? string.Empty;

    public string CaptionGroup3 => Column?.CaptionGroup3 ?? string.Empty;

    public ColumnItem? Column {
        get => _column;
        private set {
            if (_column == value) { return; }

            UnRegisterEvents();
            _column = value;
            Invalidate_CanvasContentWidth();
            RegisterEvents();
        }
    }

    public int? ControlColumnWidth { get; private set; }

    public Color FontColor_Caption {
        get => Column != null && _fontColor_Caption.IsMagentaOrTransparent() ? Column.ForeColor : _fontColor_Caption;
        set {
            if (_fontColor_Caption.ToArgb() == value.ToArgb()) { return; }
            _fontColor_Caption = value;
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
                Invalidate_ControlColumnWidth();
            }
        }
    }

    public string Renderer { get; set; }

    public string RendererSettings { get; set; }

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : Win11;

    public int? TmpIfFilterRemoved { get; set; }

    public ViewType ViewType {
        get;
        set {
            if (field != value) {
                field = value;
                Invalidate_ControlColumnWidth();
                OnPropertyChanged();
            }
        }
    } = ViewType.None;

    #endregion

    #region Methods

    public static int CalculateCanvasContentWith(ColumnItem? column, Renderer_Abstract renderer) {
        if (column is not { IsDisposed: false }) { return 16; }
        if (column.Table is not { IsDisposed: false } tb) { return 16; }
        if (column.FixedColumnWidth > 0) { return column.FixedColumnWidth; }

        var newContentWidth = 16; // Wert muss gesetzt werden, dass er am Ende auch gespeichert wird

        try {
            //  Parallel.ForEach führt ab und zu zu DeadLocks
            foreach (var thisRowItem in tb.Row) {
                var wx = renderer.ContentSize(thisRowItem.CellGetString(column), column.DoOpticalTranslation).Width;
                newContentWidth = Math.Max(newContentWidth, wx);
            }
        } catch {
            Develop.AbortAppIfStackOverflow();
            return CalculateCanvasContentWith(column, renderer);
        }

        return newContentWidth;
    }

    public int ControlColumnLeft(float shiftX) {
        if (Permanent) {
            return _controlColumnLeft ?? 0;
        }

        return (_controlColumnLeft ?? 0) + (int)shiftX;
    }

    public int ControlColumnRight(float shiftX) => ControlColumnLeft(shiftX) + ControlColumnWidth ?? 0;

    public void Dispose() =>
                // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
                Dispose(disposing: true);

    public void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                UnRegisterEvents();
                Parent = null;
                _column = null;
            }
            IsDisposed = true;
        }
    }

    //GC.SuppressFinalize(this);
    public Renderer_Abstract GetRenderer(string style) {
        if (_renderer != null) { return _renderer; }

        _renderer = TableView.RendererOf(this, style);
        return _renderer;
    }

    public void Invalidate_CanvasContentWidth() {
        _canvasContentWidth = null;
        Invalidate_ControlColumnWidth();
    }

    public void Invalidate_ControlColumnLeft() => _controlColumnLeft = null;

    public void Invalidate_ControlColumnWidth() {
        if (ControlColumnWidth is null) { return; }

        ControlColumnWidth = null;

        Parent?.Invalidate_XOfAllItems();
    }

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

    public void ParseFinished(string parsed) {
    }

    public bool ParseThis(string key, string value) {
        if (Parent?.Table is not { IsDisposed: false } tb) {
            Develop.DebugPrint(ErrorType.Error, "Tabelle unbekannt");
            return false;
        }

        switch (key) {
            case "column":
            case "columnname":// ColumnName wichtg, wegen CopyLayout
                Column = tb.Column[value];
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

    public QuickImage? SymbolForReadableText() => _column?.SymbolForReadableText();

    //    return new Rectangle(r.Right - size, r.Top, size, size);
    //}
    public override string ToString() => ParseableItems().FinishParseable();

    internal void ComputeLocation(int x, int tableviewWith, float zoom) {
        if (Column == null) { return; }

        _controlColumnLeft = x;

        ControlColumnWidth = ComputeControlColumnWidth(tableviewWith, zoom);
    }

    //    //if (!string.IsNullOrEmpty(CaptionGroup3)) {
    //    //    moveDown += pcch * 3;
    //    //} else if (!string.IsNullOrEmpty(CaptionGroup2)) {
    //    //    moveDown += pcch * 2;
    //    //} else if (!string.IsNullOrEmpty(CaptionGroup1)) {
    //    //    moveDown += pcch;
    //    //}
    private void _column_PropertyChanged(object sender, PropertyChangedEventArgs e) => Invalidate_CanvasContentWidth();

    //public Rectangle ReduceButtonLocation(float scale, float sliderx) {
    //    var r = RealHead(scale, sliderx);
    //    var size = (int)(18.CanvasToControl(scale));
    //    var pcch = (int)(ColumnCaptionSizeY.CanvasToControl(scale));
    private void _parent_StyleChanged(object? sender, System.EventArgs e) => Invalidate_CanvasContentWidth();

    private void Cell_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Column == _column) { Invalidate_CanvasContentWidth(); }
    }

    /// <summary>
    /// Control heißt, dass die Kooridanten sich auf die Controllebene beziehen und nicht auf den Canvas
    /// </summary>
    /// <param name="tableviewWith"></param>
    /// <param name="zoom"></param>
    /// <returns></returns>
    private int ComputeControlColumnWidth(int tableviewWith, float zoom) {
        // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!

        if (Parent == null) { return 16; }

        if (_controlColumnWidth is { } v) { return v; }

        if (_column is not { IsDisposed: false }) {
            _controlColumnWidth = 16;
            return (int)_controlColumnWidth;
        }

        if (Parent.Count == 1) {
            _controlColumnWidth = tableviewWith;
            return (int)_controlColumnWidth;
        }

        if (Reduced) {
            _controlColumnWidth = 16;
        } else {
            _controlColumnWidth = ViewType == ViewType.PermanentColumn
                ? Math.Min(CanvasContentWidth.CanvasToControl(zoom), (int)(tableviewWith * 0.3))
                : Math.Min(CanvasContentWidth.CanvasToControl(zoom), (int)(tableviewWith * 0.6));
        }

        _controlColumnWidth = Math.Max((int)_controlColumnWidth, FilterBarListItem.AutoFilterSize); // Mindestens so groß wie der Autofilter;
        return (int)_controlColumnWidth;
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
}